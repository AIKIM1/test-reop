/*******************************************************************************************************************
 Created Date : 2020.12.30
      Creator : Kang Dong Hee
   Decription : PQC 검사의뢰
--------------------------------------------------------------------------------------------------------------------
 [Change History]
  2020.12.30  NAME : Initial Created
  2021.03.31  KDH  : 이벤트 및 컬럼명 수정 대응
  2021.04.08  KDH  : 검사대상 더블클릭 시 .Tag 값이 Null인 경우 Return 처리
  2021.04.09  KDH  : 검사의뢰 공정 데이터 중문 출력 처리 및 링크 기능을 Head -> Cell 로 전환
  2021.04.13  KDH : 화면간 이동 시 초기화 현상 제거
  2021.04.20  KDH : Lot ID 생성 로직 수정
  2021.05.03  KDH : CELL 저장 Tab의 CELL ID 정보 조회 오류 수정
  2021.01.13  PSM : PQC 의뢰 - 리튬석출검사 조건 조회 로직 추가
  2022.06.20  KDH : CSR NO : C20220603-000198
                    기준정보 변경 (공통코드 -> 동별 공통코드), code_type = 'PQC_REQ_STEP_CODE'
                    오류수정 (REQTYPE1 값 변경 : 1 -> 01 형식으로)
  2022.12.31  HJW : 화면 오픈 시, 쿼리 오류나는 부분 수정 
                    E등급 샘플출고 요청 Tab에서 버튼 클릭시 NULL 에러 수정
  2023.01.12  LHR : Sample 조회 Tab 추가
  2023.03.09 임근영: PQC 의뢰 취소 조회 TAB 추가 
  2023.08.22 이지은: D/E 등급 Sorting Lot 현황 재출고 기능 수정
  2023.10.04 이정미 :D/E등급 Sorting Tab 수정
                     (Lot별 등급 수량 팝업 닫을 때 발생하는 오류 수정, Lot별 등급 수량 파라미터 인수 추가, 
                     모델 콤보박스 선택 시 자동 조회되는 기능 삭제)
  2023.11.06 이지은: PQC 검사 생산라인 명칭 변경건 - 활성화 라인을 조립 라인으로 표시하도록 변경
                     CSR NO : E20230706-001465
  2023.11.23 이지은: 검사대상 조회tab 조립 라인호기로 검색되도록 변경
                     CSR NO : E20230706-001465
  2023.12.08 이지은: 해제이력tab 조립 라인호기로 검색되도록 변경
                     CSR NO : E20230706-001465
  2024.05.30 이지은: 검사 조회 화면에서 의뢰 요청중일경우 QMS 검사 취소 I/F 송신할 수 있도록 수정
                     CSR NO : E20240222-001627
  2024.12.19 최도훈: 조회기간 기본값 수정
  2025.04.09 이현승: Catch-Up [CSR NO : E20240911-001061] 등급변경 적용 UI 추적성 향상을 위한 MENUID 추가
********************************************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using Microsoft.Win32;
using System.Configuration;
using C1.WPF.Excel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_083 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        //20220620_C20220603-000198 START
        //private readonly string REQTYPE1 = "1"; //검사완료
        //private readonly string REQTYPE2 = "2"; //의뢰완료
        //private readonly string REQTYPE3 = "3"; //의뢰대기
        //private readonly string REQTYPE4 = "4"; //의뢰가능
        //private readonly string REQTYPE5 = "5"; //생성대기
        //private readonly string REQTYPE6 = "6"; //생산없음
        private readonly string REQTYPE1 = "01"; //검사완료
        private readonly string REQTYPE2 = "02"; //의뢰완료
        private readonly string REQTYPE3 = "03"; //의뢰대기
        private readonly string REQTYPE4 = "04"; //의뢰가능
        private readonly string REQTYPE5 = "05"; //생성대기
        private readonly string REQTYPE6 = "06"; //생산없음
        //20220620_C20220603-000198 END

        public bool lotMixReqTab1 = false;
        public int lotMixReqDayTab1 = 0;

        // 제품저장에서 Lot, Lot 별 Cell 수량 정보
        private List<String> lotList_tpSaveCell = null;
        private List<int> cellCnt_tpSaveCell = null;

        private int iOpStepCount = 0;
        private int tpReqInsW_day_startColumn = 5;
        private int tpReqInsW_day_endColumn = 21;

        //2024.05.30 PQC_RSLT_CODE
        private readonly String PQC_REQ_REFUSE = "R";    //거부
        private readonly String PQC_REQ_COMPLETE = "C";  //완료
        private readonly String PQC_REQ_TRANSFER = "T";  //QMS 의뢰 전송
        private readonly String PQC_REQ_CANCEL = "M";    //QMS 의뢰 취소 요청

        DataTable dtTemp = new DataTable();

        public FCS001_083()
        {

            InitializeComponent();

            //Control Setting
            InitControl();

            //this.Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion


        #region [Initialize]

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Combo Setting
            InitCombo();
            //Tab_Inspection Object(검사대상) Setting
            Init_tpSaveCell();

            this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거
        }
        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            #region [검사대상 Tabpage]
            C1ComboBox[] cboLineTargetChild = { cboModelTarget };
            //2023.11.10 E20230706-001465 
            //ComCombo.SetCombo(cboLineTarget, CommonCombo_Form.ComboStatus.NONE, sCase: "LINE", cbChild: cboLineTargetChild);
            ComCombo.SetCombo(cboLineTarget, CommonCombo_Form.ComboStatus.NONE, sCase: "PQCASSYLINE", cbChild: cboLineTargetChild);

            C1ComboBox[] cboModelTargetParent = { cboLineTarget };
            ComCombo.SetCombo(cboModelTarget, CommonCombo_Form.ComboStatus.NONE, sCase: "PQCLINEMODEL", cbParent: cboModelTargetParent);
            #endregion

            #region [Cell 저장 Tabpage]
            C1ComboBox[] cboCellGroupChild = { cboCellUser };
            ComCombo.SetCombo(cboCellGroup, CommonCombo_Form.ComboStatus.ALL, sCase: "PQCDEPT", cbChild: cboCellGroupChild);

            C1ComboBox[] cboCellUserParent = { cboCellGroup };
            ComCombo.SetCombo(cboCellUser, CommonCombo_Form.ComboStatus.NONE, sCase: "PQCUSER", cbParent: cboCellUserParent);

            ComCombo.SetCombo(cboCellStep, CommonCombo_Form.ComboStatus.SELECT, sCase: "QAREQSTEP");
            #endregion

            #region [검사의뢰 Tabpage]
            C1ComboBox[] cboLineChild = { cboModel };
            //2023.11.10 E20230706-001465 
            //ComCombo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);
            ComCombo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "PQCASSYLINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelParent = { cboLine };
            ComCombo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "PQCLINEMODEL", cbParent: cboModelParent);

            string[] sFilter = { null };
            ComCombo.SetCombo(cboUser, CommonCombo_Form.ComboStatus.ALL, sCase: "PQCUSER", sFilter: sFilter);
            #endregion

            #region [검사조회 Tabpage]
            C1ComboBox[] cboLineSearchChild = { cboModelSearch };
            //2023.11.10 E20230706-001465 
            //ComCombo.SetCombo(cboLineSearch, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineSearchChild);
            ComCombo.SetCombo(cboLineSearch, CommonCombo_Form.ComboStatus.ALL, sCase: "PQCASSYLINE", cbChild: cboLineSearchChild);

            C1ComboBox[] cboModelSearchParent = { cboLineSearch };
            ComCombo.SetCombo(cboModelSearch, CommonCombo_Form.ComboStatus.ALL, sCase: "PQCLINEMODEL", cbParent: cboModelSearchParent); /////////

            ComCombo.SetCombo(cboReqUser, CommonCombo_Form.ComboStatus.ALL, sCase: "PQCUSER", sFilter: sFilter);

            string[] sFilter2 = { "PQC_RSLT_CODE" };
            ComCombo.SetCombo(cboStatus, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilter2, sCase: "CMN");  //검사 의뢰 결과

            string[] sFilter1 = { "LAST_JUDG_VALUE" };
            ComCombo.SetCombo(cboResult, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilter1, sCase: "CMN");  //최종 판정 결과
            #endregion

            #region [검사기준 Tabpage]

            #endregion

            #region [제품검사 해제 Tabpage]

            #endregion

            #region [W등급 제품검사 의뢰 Tabpage]
            C1ComboBox[] cboLineWChild = { cboModelW };
            ComCombo.SetCombo(cboLineW, CommonCombo_Form.ComboStatus.NONE, sCase: "LINE", cbChild: cboLineWChild);

            C1ComboBox[] cboModelWParent = { cboLineW };
            ComCombo.SetCombo(cboModelW, CommonCombo_Form.ComboStatus.NONE, sCase: "LINEMODEL", cbParent: cboModelWParent);
            #endregion

            #region [E등급 샘플출고 요청 Tabpage]
            C1ComboBox[] cboLineEChild = { cboModelE };
            ComCombo.SetCombo(cboLineE, CommonCombo_Form.ComboStatus.NONE, sCase: "LINE", cbChild: cboLineEChild);

            C1ComboBox[] cboModelEParent = { cboLineE };
            ComCombo.SetCombo(cboModelE, CommonCombo_Form.ComboStatus.NONE, sCase: "LINEMODEL", cbParent: cboModelEParent);
            #endregion

            #region [D/E등급 Sorting Lot 현황 Tabpage]
            C1ComboBox[] cboLineESSTLChild = { cboModelDESoStus };
            ComCombo.SetCombo(cboLineDESoStus, CommonCombo_Form.ComboStatus.NONE, sCase: "LINE", cbChild: cboLineESSTLChild);

            C1ComboBox[] cboModelESSTLParent = { cboLineDESoStus };
            ComCombo.SetCombo(cboModelDESoStus, CommonCombo_Form.ComboStatus.NONE, sCase: "LINEMODEL", cbParent: cboModelESSTLParent);
            #endregion

            #region [조회 Tabpage]

            string[] sSampleFilter = { "FORM_SMPL_TYPE_CODE", "Y", null, null, null, null };
            ComCombo.SetCombo(cboSearchSampleType, CommonCombo_Form.ComboStatus.ALL, sFilter: sSampleFilter, sCase: "CMN_WITH_OPTION");

            //20220329_조회조건추가-생산라인,모델,LotType START
            ComCombo.SetCombo(cboLineSample, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE");

            C1ComboBox[] cboModelParentSample = { cboLineSample };
            ComCombo.SetCombo(cboModelSample, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParentSample);

            // Lot 유형
            ComCombo.SetCombo(cboLotType, CommonCombo_Form.ComboStatus.ALL, sCase: "LOTTYPE"); // 2021.08.19 Lot 유형 검색조건 추가
                                                                                               //20220329_조회조건추가-생산라인,모델,LotType END
            #endregion

            #region [담당자 관리 Tabpage]

            #endregion

            #region [취소이력 조회 Tabpage] 
            //2023.12.08 E20230706-001465 
            C1ComboBox[] cboLineCancelChild = { cboModelSelect };
            ComCombo.SetCombo(cboLineSelect, CommonCombo_Form.ComboStatus.ALL, sCase: "PQCASSYLINE", cbChild: cboLineCancelChild); //생산라인 

            C1ComboBox[] cboModelCancelParent = { cboLineSelect };
            ComCombo.SetCombo(cboModelSelect, CommonCombo_Form.ComboStatus.ALL, sCase: "PQCLINEMODEL", cbParent: cboModelCancelParent); //모델명

            //ComCombo.SetCombo(cboReqUser, CommonCombo_Form.ComboStatus.ALL, sCase: "PQCUSER", sFilter: sFilter); //담당자 

        }

        private void InitControl()
        {
            dtpTarget.SelectedDateTime = DateTime.Now;

            dtpFromDate.SelectedDateTime = DateTime.Now;
            dtpToDate.SelectedDateTime = DateTime.Now.AddDays(1);

            dtpFromDateSearch.SelectedDateTime = DateTime.Now;
            dtpToDateSearch.SelectedDateTime = DateTime.Now.AddDays(1);

            dtpWMonth.SelectedDateTime = DateTime.Now;

            dtpEGrade.SelectedDateTime = DateTime.Now;

            dtpFromDESoStus.SelectedDateTime = DateTime.Now;
            dtpToDESoStus.SelectedDateTime = DateTime.Now;

            dtpFromDateSample.SelectedDateTime = DateTime.Now;
            dtpToDateSample.SelectedDateTime = DateTime.Now.AddDays(1);

            dtpDateFromSearch.SelectedDateTime = DateTime.Now; 
            dtpDateToSearch.SelectedDateTime = DateTime.Now.AddDays(1);

        }

        private void Init_tpSaveCell()
        {
            txtCellCnt.Text = string.Empty;
            txtLotId.Text = string.Empty;
            txtReqDesc.Text = string.Empty;
            txtCellId.Text = string.Empty;

            cboCellStep.SelectedIndex = 0;

            DataTable dt = ((DataView)cboCellStep.ItemsSource).Table;
            for (int i = 1; i < dt.Rows.Count; i++)
            {
                // LOT 혼합 검사
                if (cboCellStep.SelectedValue.ToString() == dt.Rows[i]["CBO_CODE"].ToString()
                    && int.Parse(dt.Rows[i]["PERIOD_REQ_DAY"].ToString()) > 1)
                {
                    lotMixReqTab1 = true;
                    lotMixReqDayTab1 = int.Parse(dt.Rows[i]["PERIOD_REQ_DAY"].ToString());
                }
            }

            cboReqUser.SelectedIndex = 0;

            lotList_tpSaveCell = new List<String>();
            cellCnt_tpSaveCell = new List<int>();

            Util.gridClear(dgCellList);
        }

        private void Init_tpInsObj()
        {
            DataSet dsInData = new DataSet();
            DataSet dsOutData = new DataSet();

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string)); //2021.04.09 검사의뢰 공정 데이터 중문 출력 처리.

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID; //2021.04.09 검사의뢰 공정 데이터 중문 출력 처리.
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_REQ_OP", "RQSTDT", "RSLTDT", dtRqst);

                iOpStepCount = dtRslt.Rows.Count;

                int iRowCnt = dtRslt.Rows.Count * 2 + 2;
                int iColCnt = tpReqInsW_day_endColumn;
                double dColumnWidth = 0;

                //상단 Datatable, 하단 Datatable 정의
                DataTable Udt = new DataTable();
                DataTable Ddt = new DataTable();

                DataRow UrowHeader = Udt.NewRow();
                DataRow DrowHeader = Ddt.NewRow();

                for (int i = 0; i < iColCnt; i++)
                {
                    switch (i.ToString())
                    {
                        case "0":
                            dColumnWidth = 155;
                            break;
                        case "1":
                            dColumnWidth = 70;
                            break;
                        case "2":
                            dColumnWidth = 130;
                            break;
                        case "3":
                            dColumnWidth = 160;
                            break;
                        default:
                            dColumnWidth = 55;
                            break;
                    }
                    //GRID Column Create
                    SetGridHeaderSingle((i + 1).ToString(), dgTarget, dColumnWidth);
                    Udt.Columns.Add((i + 1).ToString(), typeof(string));
                    Ddt.Columns.Add((i + 1).ToString(), typeof(string));
                }

                //Row 생성
                for (int i = 0; i < dtRslt.Rows.Count + 1; i++)
                {
                    DataRow Urow = Udt.NewRow();
                    Udt.Rows.Add(Urow);

                    DataRow Drow = Ddt.NewRow();
                    Ddt.Rows.Add(Drow);
                }

                DateTime dt = dtpTarget.SelectedDateTime;

                //2021.04.20 Lot ID 생성 로직 수정 START
                string year = Convert.ToChar((dt.Year - 2001) % 20 + 65).ToString();  //2021.01.13  jinmingfei
                if (dt.Year < 2010) return;
                string month = Convert.ToChar(dt.Month + 64).ToString();
                //2021.04.20 Lot ID 생성 로직 수정 END

                //마지막 일자 구하기
                DateTime dt2 = dt.AddMonths(1);
                dt2 = dt2.AddDays(-dt2.Day);

                for (int k = 0; k < dtRslt.Rows.Count; k++)
                {
                    if (k == 0)
                    {
                        Udt.Rows[k][0] = ObjectDic.Instance.GetObjectName("LINE_ID");
                        Udt.Rows[k][2] = ObjectDic.Instance.GetObjectName("MODEL");
                        Udt.Rows[k][3] = ObjectDic.Instance.GetObjectName("REQ_OP_NAME");
                        for (int iDay = 1; iDay <= 16; iDay++)
                        {
                            Udt.Rows[k][iDay + tpReqInsW_day_startColumn - 1] = Util.NVC(year + month + string.Format("{0:D2}", iDay));
                        }

                        Ddt.Rows[k][0] = ObjectDic.Instance.GetObjectName("LINE_ID");
                        Ddt.Rows[k][2] = ObjectDic.Instance.GetObjectName("MODEL");
                        Ddt.Rows[k][3] = ObjectDic.Instance.GetObjectName("REQ_OP_NAME");

                        int lastCol = 0;
                        for (int iDay = 17; iDay <= dt2.Day; iDay++)
                        {
                            Ddt.Rows[k][iDay + tpReqInsW_day_startColumn - 1 - 16] = Util.NVC(year + month + string.Format("{0:D2}", iDay));
                            lastCol = iDay + tpReqInsW_day_startColumn - 1 - 16;
                        }

                        //마지막 부분 지우기
                        for (int iDay = lastCol + 1; iDay < 19; iDay++)
                        {
                            Ddt.Rows[k][iDay] = string.Empty;
                        }
                    }

                    Udt.Rows[k + 1][0] = Util.NVC(cboLineTarget.Text);
                    Udt.Rows[k + 1][1] = Util.NVC(cboLineTarget.SelectedValue);
                    Udt.Rows[k + 1][2] = Util.NVC(cboModelTarget.SelectedValue);
                    Udt.Rows[k + 1][3] = Util.NVC(dtRslt.Rows[k]["CMN_CD_NAME"]);
                    Udt.Rows[k + 1][4] = Util.NVC(dtRslt.Rows[k]["CMN_CD"]);

                    Ddt.Rows[k + 1][0] = Util.NVC(cboLineTarget.Text);
                    Ddt.Rows[k + 1][1] = Util.NVC(cboLineTarget.SelectedValue);
                    Ddt.Rows[k + 1][2] = Util.NVC(cboModelTarget.SelectedValue);
                    Ddt.Rows[k + 1][3] = Util.NVC(dtRslt.Rows[k]["CMN_CD_NAME"]);
                    Ddt.Rows[k + 1][4] = Util.NVC(dtRslt.Rows[k]["CMN_CD"]);
                }

                //상,하 Merge
                Udt.Merge(Ddt, false, MissingSchemaAction.Add);

                //dgTarget.ItemsSource = DataTableConverter.Convert(Udt);
                Util.GridSetData(dgTarget, Udt, FrameOperation, false);

                dgTarget.Columns[1].Visibility = Visibility.Collapsed;
                dgTarget.Columns[4].Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool SetGridCboItem_CommonCode(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = false, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE_LIST", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sClassId;
                if (!string.IsNullOrEmpty(sCmnCd))
                {
                    dr["CMCODE_LIST"] = sCmnCd;
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_CMN_FORM", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


        #region [조회 Method]
        #region [검사대상 Tabpage]
        /// <summary>
        /// 검사대상 조회
        /// </summary>
        private void GetListTarget()
        {
            try
            {
                dtTemp = null;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("MODEL_ID", typeof(string));
                dtRqst.Columns.Add("LINE_ID", typeof(string));
                dtRqst.Columns.Add("LOT_ID", typeof(string));

                DateTime dt = dtpTarget.SelectedDateTime;

                //2021.04.20 Lot ID 생성 로직 수정 START
                string year = Convert.ToChar((dt.Year - 2001) % 20 + 65).ToString();  //2021.01.13  jinmingfei
                if (dt.Year < 2010) return;
                string month = Convert.ToChar(dt.Month + 64).ToString();
                //2021.04.20 Lot ID 생성 로직 수정 END

                //마지막 일자 구하기
                DateTime dt2 = dt.AddMonths(1);
                dt2 = dt2.AddDays(-dt2.Day);

                for (int iDay = 1; iDay <= dt2.Day; iDay++)
                {
                    DataRow dr = dtRqst.NewRow();

                    dr["MODEL_ID"] = Util.GetCondition(cboModelTarget);
                    dr["LINE_ID"] = Util.GetCondition(cboLineTarget);
                    dr["LOT_ID"] = ((year + month + string.Format("{0:D2}", iDay).Trim()));
                    dtRqst.Rows.Add(dr);
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_REQ_TARGET_BY_MONTH", "RQSTDT", "RSLTDT", dtRqst);

                dtTemp = dtRslt.Copy();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 검사조회
        /// </summary>
        /// <param name="sPqcReqId"></param>
        private void GetReqList(string sPqcReqId)
        {
            try
            {
                Util.gridClear(dgReqList);
                Util.gridClear(dgReqCellInfo);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PQC_REQ_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PQC_REQ_ID"] = sPqcReqId;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_PQC_LIST", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgReqList, dtRslt, FrameOperation, true);
                Util.gridClear(dgReqCellInfo);

                txtReject.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Cell 저장 Tabpage]
        #endregion

        #region [검사의뢰 Tabpage]
        /// <summary>
        /// 검사의뢰 조회
        /// </summary>
        private void GetList()
        {
            try
            {
                Util _Util = new Util();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("FROM_TIME", typeof(string));
                dtRqst.Columns.Add("TO_TIME", typeof(string));
                dtRqst.Columns.Add("PQC_REQ_ID", typeof(string));
                dtRqst.Columns.Add("PQC_REQ_GR_ID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PQC_REQ_USERID", typeof(string));
                dtRqst.Columns.Add("PQC_RSLT_CODE", typeof(string));
                dtRqst.Columns.Add("LAST_JUDGE_VALUE", typeof(string));
                dtRqst.Columns.Add("GROUP_IS_NULL", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["FROM_TIME"] = Util.GetCondition(dtpFromDate);
                dr["TO_TIME"] = Util.GetCondition(dtpToDate);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["PQC_REQ_USERID"] = Util.GetCondition(cboUser, bAllNull: true);
                dr["GROUP_IS_NULL"] = "Y";

                dtRqst.Rows.Add(dr);

                DataTable dtRsltAll = new ClientProxy().ExecuteServiceSync("DA_SEL_PQC_LIST", "RQSTDT", "RSLTDT", dtRqst);

                dtRsltAll = _Util.gridCheckColumnAdd(dtRsltAll, "CHK");

                DataTable dtRslt = dtRsltAll.Copy(); // 스프레드 SetDataSource 에서 변형이 생김으로 여러 스프레드 사용시 복사해서 사용해야함 scpark 


                Util.GridSetData(dgReqAll, dtRsltAll, FrameOperation, true);

                /*Util.GridSetData(dgReq, dtRslt, FrameOperation, true);
                for (int i = 0; i < dgReq.Rows.Count; i++)
                {
                    dgReq.Rows[i].Presenter.Visibility = Visibility.Collapsed;
                }*/

                dr["PQC_REQ_GR_ID"] = null;

                DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_SEL_PQC_GROUP", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgReqGroup, dtRslt1, FrameOperation, true);
                Util.gridClear(dgSummary);
                Util.gridClear(dgReq);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 검사의뢰 대상 Summary
        /// </summary>
        private void QaReqSummary()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("PQC_REQ_ID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;

            for (int i = 0; i < dgReq.Rows.Count; i++)
            {
                // if (dgReq.Rows[i].Presenter.Visibility == Visibility.Visible)
                // {
                dr["PQC_REQ_ID"] += Util.NVC(DataTableConverter.GetValue(dgReq.Rows[i].DataItem, "PQC_REQ_ID")) + ",";
                // }
            }
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_PQC_SUMMARY_LOT", "RQSTDT", "RSLTDT", dtRqst);
            Util.GridSetData(dgSummary, dtRslt, FrameOperation, true);
        }

        private void GetReqList()
        {
            try
            {
                SetGridCboItem_CommonCode(dgReqList.Columns["REQ_OP"], "PQC_REQ_STEP_CODE");
                SetGridCboItem_CommonCode(dgReqList.Columns["PQC_RSLT_CODE"], "PQC_RSLT_CODE");
                SetGridCboItem_CommonCode(dgReqList.Columns["LAST_JUDGE_VALUE"], "LAST_JUDG_VALUE");

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("FROM_TIME", typeof(string));
                dtRqst.Columns.Add("TO_TIME", typeof(string));
                dtRqst.Columns.Add("PQC_REQ_ID", typeof(string));
                dtRqst.Columns.Add("PQC_REQ_GR_ID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PQC_REQ_USERID", typeof(string));
                dtRqst.Columns.Add("PQC_RSLT_CODE", typeof(string));
                dtRqst.Columns.Add("LAST_JUDGE_VALUE", typeof(string));
                dtRqst.Columns.Add("GROUP_IS_NOT_NULL", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                if (!string.IsNullOrEmpty(txtSaveID.Text))
                {
                    dr["PQC_REQ_ID"] = Util.NVC(txtSaveID.Text);
                }
                else if (!string.IsNullOrEmpty(txtReqID.Text))
                {
                    dr["PQC_REQ_GR_ID"] = Util.NVC(txtReqID.Text);
                }
                else
                {
                    dr["FROM_TIME"] = Util.GetCondition(dtpFromDateSearch);
                    dr["TO_TIME"] = Util.GetCondition(dtpToDateSearch);
                    dr["MDLLOT_ID"] = Util.GetCondition(cboModelSearch, bAllNull: true);
                    dr["EQSGID"] = Util.GetCondition(cboLineSearch, bAllNull: true);
                    dr["PQC_REQ_USERID"] = Util.GetCondition(cboReqUser, bAllNull: true);
                    dr["LAST_JUDGE_VALUE"] = Util.GetCondition(cboResult, bAllNull: true);
                    dr["PQC_RSLT_CODE"] = Util.GetCondition(cboStatus, bAllNull: true);
                    dr["GROUP_IS_NOT_NULL"] = "Y";
                }
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_PQC_LIST", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgReqList, dtRslt, FrameOperation, true);

                Util.gridClear(dgReqCellInfo);
                txtReject.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void OpenReqList(string sGroup, string sReq)
        {
            txtReqID.Text = sGroup;
            txtSaveID.Text = sReq;

            //tcReqIns.SelectedTab = tpSearchIns;
            tcReqIns.SelectedItem = tpSearchIns;

            GetReqList();
        }
        #endregion

        #region [검사조회 Tabpage]
        #endregion

        #region [검사기준 Tabpage]
        /// <summary>
        /// 검사기준 조회
        /// </summary>
        private void GetListSpec()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TM_PQC_CELL_SUMMARY", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgSpec, dtRslt, FrameOperation, true);

                if (dtRslt.Rows.Count > 0)
                {
                    Util _Util = new Util();
                    string[] sColumnName = new string[] { "MODEL_NO", "MDLLOT_ID" };
                    _Util.SetDataGridMergeExtensionCol(dgSpec, sColumnName, DataGridMergeMode.VERTICAL);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [제품검사 해제 Tabpage]
        /// <summary>
        /// 제품검사 해제 대상 Cell ID 조회
        /// </summary>
        /// <param name="cell"></param>
        private void ScanClearId(string cell)
        {
            Util _util = new Util();
            if (string.IsNullOrEmpty(cell)) return;

            if (cell.Length < 10) return;

            //스프레드에 있는지 확인
            int iRow = -1;

            iRow = _util.GetDataGridRowIndex(dgCancel, dgCancel.Columns["SUBLOTID"].Name, cell);
            if (iRow > -1)
            {
                Util.MessageValidation("FM_ME_0193");  //이미 스캔한 ID 입니다.
                return;
            }

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["SUBLOTID"] = cell;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_PQC_CHK_SAVE_CELL", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    DataTable preTable = DataTableConverter.Convert(dgCancel.ItemsSource);
                    DataTable dtTemp = new DataTable();

                    if (preTable.Columns.Count == 0)
                    {
                        preTable = new DataTable();
                        foreach (C1.WPF.DataGrid.DataGridColumn col in dgCancel.Columns)
                        {
                            preTable.Columns.Add(Convert.ToString(col.Name));
                        }

                        dtTemp = preTable.Copy();
                    }
                    else
                    {
                        dtTemp = new DataTable();
                        foreach (C1.WPF.DataGrid.DataGridColumn col in dgCancel.Columns)
                        {
                            dtTemp.Columns.Add(Convert.ToString(col.Name));
                        }
                    }

                    for (int i = 0; i < dtRslt.Rows.Count; i++)
                    {
                        DataRow row = dtTemp.NewRow();
                        row["LOTID"] = Util.NVC(dtRslt.Rows[i]["LOTID"]);
                        row["SUBLOTID"] = Util.NVC(dtRslt.Rows[i]["SUBLOTID"]);
                        row["CREATE_TIME"] = Util.NVC(dtRslt.Rows[i]["CREATE_TIME"]);
                        row["EQSGNAME"] = Util.NVC(dtRslt.Rows[i]["EQSGNAME_ASSY"]);
                        row["MDLLOT_ID"] = Util.NVC(dtRslt.Rows[i]["MDLLOT_ID"]);
                        row["PQC_REQ_USER"] = Util.NVC(dtRslt.Rows[i]["PQC_REQ_USERID"]);

                        //row["LOTID"] = "ABDS000041AATP02";
                        //row["SUBLOTID"] = "USLEA100027";
                        //row["CREATE_TIME"] = "2021-03-10 19:15:17.540";
                        //row["EQSGNAME"] = "CNB 汽车 活性化 10";
                        //row["MDLLOT_ID"] = "FA3";
                        //row["PQC_REQ_USER"] = "acudium";

                        dtTemp.Rows.Add(row);
                    }
                    dtTemp.Merge(preTable);

                    Util.GridSetData(dgCancel, dtTemp, FrameOperation, true);
                }
                else
                {
                    Util.MessageValidation("FM_ME_0232");  //조회된 값이 없습니다.
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [W등급 제품검사 의뢰 Tabpage]
        /// <summary>
        /// W등급 제품검사 의뢰 조회
        /// </summary>
        private void GetWList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MONTH_LOT", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["MDLLOT_ID"] = Util.GetCondition(cboModelW);
                dr["EQSGID"] = Util.GetCondition(cboLineW);

                DateTime dt = dtpWMonth.SelectedDateTime;
                //2021.04.20 Lot ID 생성 로직 수정 START
                string year = Convert.ToChar((dt.Year - 2001) % 20 + 65).ToString();  //2021.01.13  jinmingfei
                if (dt.Year < 2010) return;
                string month = Convert.ToChar(dt.Month + 64).ToString();
                //2021.04.20 Lot ID 생성 로직 수정 END

                dr["MONTH_LOT"] = year + month;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_WCODE_SAMPLE_TRAY", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgW, dtRslt, FrameOperation, true);

                if (dtRslt.Rows.Count > 0)
                {
                    Util _Util = new Util();
                    string[] sColumnName = new string[] { "LOT_ID", "WCODE_COUNT_BY_LOT", "CSTID" };
                    _Util.SetDataGridMergeExtensionCol(dgW, sColumnName, DataGridMergeMode.VERTICAL);

                    if (FrameOperation.AUTHORITY.Equals("W"))
                    {
                        dgW.Columns["PQC_REQ_USER"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgW.Columns["PQC_REQ_USER"].Visibility = Visibility.Collapsed;
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [E등급 샘플출고 요청 Tabpage]
        /// <summary>
        /// E등급 샘플출고 요청 조회
        /// </summary>
        private void GetEList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MONTH_LOT", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["MDLLOT_ID"] = Util.GetCondition(cboModelE);
                dr["EQSGID"] = Util.GetCondition(cboLineE);

                DateTime dt = dtpEGrade.SelectedDateTime;

                //2021.04.20 Lot ID 생성 로직 수정 START
                string year = Convert.ToChar((dt.Year - 2001) % 20 + 65).ToString();  //2021.01.13  jinmingfei
                if (dt.Year < 2010) return;
                string month = Convert.ToChar(dt.Month + 64).ToString();
                //2021.04.20 Lot ID 생성 로직 수정 END

                dr["MONTH_LOT"] = year + month;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ECODE_SAMPLE_TRAY", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgE, dtRslt, FrameOperation, true);

                if (dtRslt.Rows.Count > 0)
                {
                    Util _Util = new Util();
                    string[] sColumnName = new string[] { "LOT_ID", "ECODE_COUNT_BY_LOT", "CSTID" };
                    _Util.SetDataGridMergeExtensionCol(dgE, sColumnName, DataGridMergeMode.VERTICAL);

                    if (FrameOperation.AUTHORITY.Equals("W"))
                    {
                        dgE.Columns["PQC_REQ_USER"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgE.Columns["PQC_REQ_USER"].Visibility = Visibility.Collapsed;
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [D/E등급 Sorting Lot 현황 Tabpage]
        private void GetESoStusList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = Util.GetCondition(cboLineDESoStus);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModelDESoStus);
                dr["FROM_DATE"] = dtpFromDESoStus.SelectedDateTime.ToString("yyyyMMdd000000");
                dr["TO_DATE"] = dtpToDESoStus.SelectedDateTime.ToString("yyyyMMdd235959");
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOW_VOLT_SORTING_LOT_CHECK", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgDESoStus, dtRslt, FrameOperation, true);

                if (dtRslt.Rows.Count > 0)
                {
                    Util _Util = new Util();
                    string[] sColumnName = new string[] { "JUDG_OP", "SORTING_OP" };
                    _Util.SetDataGridMergeExtensionCol(dgDESoStus, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

                    if (FrameOperation.AUTHORITY.Equals("W"))
                    {
                        dgDESoStus.Columns["RE_SHIP"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgDESoStus.Columns["RE_SHIP"].Visibility = Visibility.Collapsed;
                    }

                    if (LoginInfo.CFG_AREA_ID.Equals("A6"))
                    {
                        dgDESoStus.Columns["WRK_TYPE_DESC"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgDESoStus.Columns["WRK_TYPE_DESC"].Visibility = Visibility.Collapsed;
                    }

                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [조회 Tabpage]
        private void GetSampleList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("SMPL_TYPE_CODE", typeof(string));
                //20220329_조회조건추가-생산라인,모델,LotType START
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));
                //20220329_조회조건추가-생산라인,모델,LotType END

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = dtpFromDateSample.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpToDateSample.SelectedDateTime.ToString("yyyyMMdd");
                if (!string.IsNullOrEmpty(txtSearchCellId.Text))
                {
                    dr["SUBLOTID"] = Util.GetCondition(txtSearchCellId, bAllNull: true);
                }

                dr["SMPL_TYPE_CODE"] = Util.GetCondition(cboSearchSampleType, bAllNull: true);
                //20220329_조회조건추가-생산라인,모델,LotType START
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = Util.GetCondition(cboLineSample, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModelSample, bAllNull: true);
                dr["LOTTYPE"] = Util.GetCondition(cboLotType, bAllNull: true);
                //20220329_조회조건추가-생산라인,모델,LotType END
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SAMPLE_CELL_PQC", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgSearch, dtRslt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
        #endregion


        #region [기타 Method]

        #region [Cell 저장 Tabpage]
        private void SetCellInfo(string cell)
        {
            try
            {
                #region [Lot 구성 여부 확인과 제품검사 결과 확인]

                //1. 제품검사 구성이력 확인
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["SUBLOTID"] = cell;
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_SEL_CELL_PQC_INFO", "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (bizResult.Rows.Count == 0)
                    {
                        //포장대기 Cell 확인
                        bizResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_PQC_INFO_WAIT_BOX", "RQSTDT", "RSLTDT", dtRqst);

                        if (bizResult.Rows.Count == 0)
                        {
                            Util.MessageValidation("FM_ME_0021");  //Cell 정보가 존재하지않습니다.
                            return;
                        }
                    }

                    /* 2014.10.30 정종덕D // [CSR ID:2609471] FCS 제품검사 의뢰 기준 변경 요청
                        * 안전성검사 lot 혼합의뢰이면 Lot을 추가하지 않는다.
                        * */
                    #region [안전성 Lot 혼합시 Lot Validation]

                    if (lotMixReqTab1 && lotList_tpSaveCell.Count == 0)
                    {
                        if (
                            //16일 전 LOT 조건과, 16일 후 LOT 조건에 없을때 
                            !(
                                (int.Parse(bizResult.Rows[0]["DAY_GR_LOTID"].ToString().Substring(5, 2)) <= lotMixReqDayTab1)
                            ||
                                (int.Parse(bizResult.Rows[0]["DAY_GR_LOTID"].ToString().Substring(5, 2)) >= 16
                                    && int.Parse(bizResult.Rows[0]["DAY_GR_LOTID"].ToString().Substring(5, 2)) < 16 + lotMixReqDayTab1)
                            )
                            )
                        {
                            Util.MessageValidation("FM_ME_0192");  //의뢰범위 Lot이 아닙니다.
                            return;
                        }
                    }
                    else if (lotMixReqTab1 && lotList_tpSaveCell.Count != 0)
                    {
                        // 1일~16일 조건인데 입력셀이 범위에 아닐때
                        if (int.Parse(lotList_tpSaveCell[0].Substring(5, 2)) < 16)
                        {
                            if (int.Parse(bizResult.Rows[0]["DAY_GR_LOTID"].ToString().Substring(5, 2)) >= 1 + lotMixReqDayTab1)
                            {
                                Util.MessageValidation("FM_ME_0190", new string[] { (lotMixReqDayTab1).ToString() });  //의뢰 Lot 범위: 01 ~ {0}
                                return;
                            }
                        }
                        //16일~31일 조건인데 입력셀이 범위에 아닐때
                        else
                        {
                            if (
                                !(int.Parse(bizResult.Rows[0]["DAY_GR_LOTID"].ToString().Substring(5, 2)) >= 16
                                    && int.Parse(bizResult.Rows[0]["DAY_GR_LOTID"].ToString().Substring(5, 2)) < 16 + lotMixReqDayTab1)
                                )
                            {
                                Util.MessageValidation("FM_ME_0191", new string[] { (16 + lotMixReqDayTab1 - 1).ToString() });  //의뢰 Lot 범위: 16 ~ {0}
                                return;
                            }
                        }
                    }
                    #endregion

                    //Lot_ID[8자리] 있는지 확인
                    //LOT_ID 추가여부 변수
                    bool chk = false;

                    //첫 셀이면
                    if (lotList_tpSaveCell.Count == 0)
                    {
                        lotList_tpSaveCell.Add(bizResult.Rows[0]["DAY_GR_LOTID"].ToString());
                        cellCnt_tpSaveCell.Add(1);
                    }
                    else
                    {
                        // 다른 모델 입력 불가
                        if (!lotList_tpSaveCell[0].ToString().Substring(0, 3).Equals(
                            bizResult.Rows[0]["DAY_GR_LOTID"].ToString().Substring(0, 3))
                            )
                        {
                            Util.MessageValidation("FM_ME_0130");  //모델이 다른 Cell을 추가할 수 없습니다.
                            return;
                        }

                        //LOT_ID 추가시
                        for (int i = 0; i < lotList_tpSaveCell.Count; i++)
                        {
                            if (lotList_tpSaveCell[i].Equals(bizResult.Rows[0]["DAY_GR_LOTID"].ToString()))
                            {
                                chk = true;
                                // 셀 수량 추가
                                cellCnt_tpSaveCell[i] = cellCnt_tpSaveCell[i] + 1;
                            }
                        }

                        if (!chk && !lotMixReqTab1)
                        {
                            lotList_tpSaveCell.Add(bizResult.Rows[0]["DAY_GR_LOTID"].ToString());
                            cellCnt_tpSaveCell.Add(1);
                        }
                        else if (!chk && lotMixReqTab1)  //LOT 혼합일때 빠른Lot이 대표 LOT이 된다.
                        {
                            // 셀 수량 추가
                            cellCnt_tpSaveCell[0] = cellCnt_tpSaveCell[0] + 1;

                            if (lotList_tpSaveCell[0].CompareTo(bizResult.Rows[0]["DAY_GR_LOTID"].ToString()) > 0)
                            {
                                lotList_tpSaveCell[0] = bizResult.Rows[0]["DAY_GR_LOTID"].ToString();
                            }
                        }
                    }

                    //추가 행 찾기
                    int insertRow = 0;
                    if (!chk || lotMixReqTab1)
                    {
                        insertRow = dgCellList.Rows.Count; //마지막 행
                    }
                    else
                    {
                        for (int i = 0; i < dgCellList.Rows.Count; i++)
                        {
                            if (bizResult.Rows[0]["DAY_GR_LOTID"].ToString().Equals(Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "DAY_GR_LOTID"))))
                            {
                                insertRow = i + cellCnt_tpSaveCell[lotList_tpSaveCell.IndexOf(bizResult.Rows[0]["DAY_GR_LOTID"].ToString())] - 1;    // 인덱스라 -1, cellCnt_tab1 에 현재 셀추가 되서 -1                  
                                break;
                            }
                        }
                    }

                    /* 2014.10.30 정종덕D // [CSR ID:2609471] FCS 제품검사 의뢰 기준 변경 요청
                        * 안전성검사 lot 혼합의뢰 이면 LOT별 수량을 체크하지 않는다.
                        * */
                    DataTable dt = DataTableConverter.Convert(dgCellList.ItemsSource);
                    DataTable dtTemp = new DataTable();

                    if (dt.Columns.Count == 0)
                    {
                        dt = new DataTable();
                        foreach (C1.WPF.DataGrid.DataGridColumn col in dgCellList.Columns)
                        {
                            dt.Columns.Add(Convert.ToString(col.Name));
                        }

                        dtTemp = dt.Copy();
                    }
                    else
                    {
                        dtTemp = new DataTable();
                        foreach (C1.WPF.DataGrid.DataGridColumn col in dgCellList.Columns)
                        {
                            dtTemp.Columns.Add(Convert.ToString(col.Name));
                        }
                    }

                    //2023.11.14 [E20230706-001465] 2021.11.21 배포 예정
                    //입력된 셀과 같은 라인이고 모델이면 갯수 체크
                    //int SameModelRow = 0;

                    //for (int i = 0; i < dgCellList.Rows.Count; i++)
                    //{
                    //    if (bizResult.Rows[0]["EQSGID"].ToString().Equals(Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "EQSGID")))
                    //    && (bizResult.Rows[0]["EQSGID"].ToString().Equals(Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "EQSGID"))))
                    //    {
                    //        insertRow = i + cellCnt_tpSaveCell[lotList_tpSaveCell.IndexOf(bizResult.Rows[0]["DAY_GR_LOTID"].ToString())] - 1;    // 인덱스라 -1, cellCnt_tab1 에 현재 셀추가 되서 -1                  
                    //        break;
                    //    }
                    //}


                    DataRow drRow = dtTemp.NewRow();

                    if (lotMixReqTab1)
                    {
                        for (int i = 0; i < dgCancel.Rows.Count; i++)
                        {
                            drRow["PQC_REQ_GROUP"] = Util.NVC((i + 1).ToString());
                        }
                    }
                    else
                    {
                        drRow["PQC_REQ_GROUP"] = Util.NVC(cellCnt_tpSaveCell[lotList_tpSaveCell.IndexOf(bizResult.Rows[0]["DAY_GR_LOTID"].ToString())].ToString());
                    }
                    drRow["SUBLOTID"] = Util.NVC(bizResult.Rows[0]["SUBLOTID"]);
                    drRow["CSTID"] = Util.NVC(bizResult.Rows[0]["CSTID"]);

                    //drRow["LINE_NAME"] = Util.NVC(bizResult.Rows[0]["LINE_NAME"]); //2023.11.14 E20230706-001465
                    drRow["LINE_NAME"] = Util.NVC(bizResult.Rows[0]["EQSG_NAME_ASSY"]); 
                    drRow["MODEL"] = Util.NVC(bizResult.Rows[0]["MODEL"]);
                    drRow["DAY_GR_LOTID"] = Util.NVC(bizResult.Rows[0]["DAY_GR_LOTID"]);
                    drRow["REQ_STEP"] = Util.NVC(cboCellStep.Text);

                    drRow["LAST_OCV_VAL"] = Util.NVC(bizResult.Rows[0]["LAST_OCV_VAL"]);
                    drRow["ROUTID"] = Util.NVC(bizResult.Rows[0]["ROUTID"]);
                    drRow["FINL_JUDG_CODE"] = Util.NVC(bizResult.Rows[0]["FINL_JUDG_CODE"]);
                    drRow["DEFECT_NAME"] = Util.NVC(bizResult.Rows[0]["DEFECT_NAME"]);

                    dtTemp.Rows.Add(drRow);
                    dtTemp.Merge(dt);

                    dgCellList.ItemsSource = DataTableConverter.Convert(dtTemp);

                    txtCellCnt.Text = dgCellList.Rows.Count.ToString();
                });
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ScanCellId(string cell)
        {
            try
            {
                Util _util = new Util();
                if (string.IsNullOrEmpty(cell)) return;

                if (cell.Length < 10) return;

                //스프레드에 있는지 확인
                int iRow = -1;
                iRow = _util.GetDataGridRowIndex(dgCellList, dgCellList.Columns["SUBLOTID"].Name, cell);

                if (iRow > -1)
                {
                    Util.MessageValidation("FM_ME_0193");  //이미 스캔한 ID 입니다.
                    return;
                }

                #region [Lot 구성 여부 확인과 제품검사 결과 확인]
                //1. 제품검사 구성이력 확인
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["SUBLOTID"] = cell;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CHK_LOTID_BY_CELLID", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    Util.MessageConfirm("FM_ME_0262", (result) =>  //해당 구성 Lot은 기존에 제품검사 구성이력이 존재합니다. 의뢰 하시겠습니까?
                    {
                        if (result != MessageBoxResult.OK)
                        {
                            return;
                        }
                        else
                        {
                            if (dtRslt.Rows.Count > 0 && dtRslt.Rows[0]["LOT_STATUS"].ToString().Equals("1"))
                            {
                                Util.MessageConfirm("FM_ME_0259", (result1) =>  //해당 Lot에 제품검사 결과 이력이 존재합니다. 의뢰 하시겠습니까?
                                {
                                    if (result1 != MessageBoxResult.OK)
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        //1. 제품검사 구성이력 확인
                                        DataTable dtRslt1 = TcReqDetailByCellID(cell);

                                        if (dtRslt1.Rows.Count > 0)
                                        {
                                            Util.MessageConfirm("FM_ME_0261", (result2) =>  //해당 구성 Cell ID는 기존에 제품검사 구성이력이 존재합니다. 의뢰 하시겠습니까?
                                            {
                                                if (result2 != MessageBoxResult.OK)
                                                {
                                                    return;
                                                }
                                                else
                                                {
                                                    //2. QMS I/F 이력확인
                                                    DataTable dtRslt2 = IfPQCDetailByCellID(cell);

                                                    if (dtRslt2.Rows.Count > 0 && dtRslt2.Rows[0]["PQC_RSLT_CODE"].ToString().Equals("T"))
                                                    {
                                                        Util.MessageConfirm("FM_ME_0060", (result3) =>  //QMS로 전송된 이력이 존재합니다.\r\n의뢰 하시겠습니까?
                                                        {
                                                            if (result3 != MessageBoxResult.OK)
                                                            {
                                                                return;
                                                            }
                                                            else
                                                            {
                                                                SetCellInfo(cell);
                                                            }
                                                        });
                                                    }
                                                    //2021.05.03 CELL 저장 Tab의 CELL ID 정보 조회 오류 수정 START
                                                    else
                                                    {
                                                        SetCellInfo(cell);
                                                    }
                                                    //2021.05.03 CELL 저장 Tab의 CELL ID 정보 조회 오류 수정 END
                                                }
                                            });
                                        }
                                        else
                                        {
                                            //2. QMS I/F 이력확인
                                            DataTable dtRslt2 = IfPQCDetailByCellID(cell);

                                            if (dtRslt2.Rows.Count > 0 && dtRslt2.Rows[0]["PQC_RSLT_CODE"].ToString().Equals("T"))
                                            {
                                                Util.MessageConfirm("FM_ME_0060", (result3) =>  //QMS로 전송된 이력이 존재합니다.\r\n의뢰 하시겠습니까?
                                                {
                                                    if (result3 != MessageBoxResult.OK)
                                                    {
                                                        return;
                                                    }
                                                    else
                                                    {
                                                        SetCellInfo(cell);
                                                    }
                                                });
                                            }
                                            //2021.05.03 CELL 저장 Tab의 CELL ID 정보 조회 오류 수정 START
                                            else
                                            {
                                                SetCellInfo(cell);
                                            }
                                            //2021.05.03 CELL 저장 Tab의 CELL ID 정보 조회 오류 수정 END
                                        }
                                    }
                                });
                            }
                            else
                            {
                                //1. 제품검사 구성이력 확인
                                DataTable dtRslt1 = TcReqDetailByCellID(cell);

                                if (dtRslt1.Rows.Count > 0)
                                {
                                    Util.MessageConfirm("FM_ME_0261", (result2) =>  //해당 구성 Cell ID는 기존에 제품검사 구성이력이 존재합니다. 의뢰 하시겠습니까?
                                    {
                                        if (result2 != MessageBoxResult.OK)
                                        {
                                            return;
                                        }
                                        else
                                        {
                                            //2. QMS I/F 이력확인
                                            DataTable dtRslt2 = IfPQCDetailByCellID(cell);

                                            if (dtRslt2.Rows.Count > 0 && dtRslt2.Rows[0]["PQC_RSLT_CODE"].ToString().Equals("T"))
                                            {
                                                Util.MessageConfirm("FM_ME_0060", (result3) =>  //QMS로 전송된 이력이 존재합니다.\r\n의뢰 하시겠습니까?
                                                {
                                                    if (result3 != MessageBoxResult.OK)
                                                    {
                                                        return;
                                                    }
                                                    else
                                                    {
                                                        SetCellInfo(cell);
                                                    }
                                                });
                                            }
                                            //2021.05.03 CELL 저장 Tab의 CELL ID 정보 조회 오류 수정 START
                                            else
                                            {
                                                SetCellInfo(cell);
                                            }
                                            //2021.05.03 CELL 저장 Tab의 CELL ID 정보 조회 오류 수정 END
                                        }
                                    });
                                }
                                else
                                {
                                    //2. QMS I/F 이력확인
                                    DataTable dtRslt2 = IfPQCDetailByCellID(cell);

                                    if (dtRslt2.Rows.Count > 0 && dtRslt2.Rows[0]["PQC_RSLT_CODE"].ToString().Equals("T"))
                                    {
                                        Util.MessageConfirm("FM_ME_0060", (result3) =>  //QMS로 전송된 이력이 존재합니다.\r\n의뢰 하시겠습니까?
                                        {
                                            if (result3 != MessageBoxResult.OK)
                                            {
                                                return;
                                            }
                                            else
                                            {
                                                SetCellInfo(cell);
                                            }
                                        });
                                    }
                                    //2021.05.03 CELL 저장 Tab의 CELL ID 정보 조회 오류 수정 START
                                    else
                                    {
                                        SetCellInfo(cell);
                                    }
                                    //2021.05.03 CELL 저장 Tab의 CELL ID 정보 조회 오류 수정 END
                                }
                            }
                        }
                    });
                }
                else
                {
                    //1. 제품검사 구성이력 확인
                    DataTable dtRslt1 = TcReqDetailByCellID(cell);

                    if (dtRslt1.Rows.Count > 0)
                    {
                        Util.MessageConfirm("FM_ME_0261", (result2) =>  //해당 구성 Cell ID는 기존에 제품검사 구성이력이 존재합니다. 의뢰 하시겠습니까?
                        {
                            if (result2 != MessageBoxResult.OK)
                            {
                                return;
                            }
                            else
                            {
                                //2. QMS I/F 이력확인
                                DataTable dtRslt2 = IfPQCDetailByCellID(cell);

                                if (dtRslt2.Rows.Count > 0 && dtRslt2.Rows[0]["PQC_RSLT_CODE"].ToString().Equals("T"))
                                {
                                    Util.MessageConfirm("FM_ME_0060", (result3) =>  //QMS로 전송된 이력이 존재합니다.\r\n의뢰 하시겠습니까?
                                    {
                                        if (result3 != MessageBoxResult.OK)
                                        {
                                            return;
                                        }
                                        else
                                        {
                                            SetCellInfo(cell);
                                        }
                                    });
                                }
                                //2021.05.03 CELL 저장 Tab의 CELL ID 정보 조회 오류 수정 START
                                else
                                {
                                    SetCellInfo(cell);
                                }
                                //2021.05.03 CELL 저장 Tab의 CELL ID 정보 조회 오류 수정 END
                            }
                        });
                    }
                    else
                    {
                        //2. QMS I/F 이력확인
                        DataTable dtRslt2 = IfPQCDetailByCellID(cell);

                        if (dtRslt2.Rows.Count > 0 && dtRslt2.Rows[0]["PQC_RSLT_CODE"].ToString().Equals("T"))
                        {
                            Util.MessageConfirm("FM_ME_0060", (result3) =>  //QMS로 전송된 이력이 존재합니다.\r\n의뢰 하시겠습니까?
                            {
                                if (result3 != MessageBoxResult.OK)
                                {
                                    return;
                                }
                                else
                                {
                                    SetCellInfo(cell);
                                }
                            });
                        }
                        //2021.05.03 CELL 저장 Tab의 CELL ID 정보 조회 오류 수정 START
                        else
                        {
                            SetCellInfo(cell);
                        }
                        //2021.05.03 CELL 저장 Tab의 CELL ID 정보 조회 오류 수정 END
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable TcReqDetailByCellID(string CellID)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("SUBLOTID", typeof(string));

            try
            {
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["SUBLOTID"] = CellID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_TC_REQ_DETAIL_BY_CELLID", "RQSTDT", "RSLTDT", RQSTDT);
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return RQSTDT;
            }
        }

        private DataTable IfPQCDetailByCellID(string CellID)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("SUBLOTID", typeof(string));

            try
            {
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["SUBLOTID"] = CellID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_IF_PQC_DETAIL_BY_CELLID", "RQSTDT", "RSLTDT", RQSTDT);
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return RQSTDT;
            }
        }

        #endregion

        #region [검사대상 Tabpage]
        private void SetGridHeaderSingle(string sColName, C1DataGrid dg, double dWidth)
        {
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = sColName,
                Binding = new Binding() { Path = new PropertyPath(sColName), Mode = BindingMode.OneWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
                IsReadOnly = true,
                Width = new C1.WPF.DataGrid.DataGridLength(dWidth, DataGridUnitType.Pixel)
            });
        }

        private void GetReqCellList(string sPqcReqId)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("PQC_REQ_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PQC_REQ_ID"] = sPqcReqId;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_PQC_DETAIL", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgReqCellInfo, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string CHECK_ALL_MODEL_PQCABLE()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = Util.GetCondition(cboLineTarget);
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOAD_MODEL_INFO_ALL_LINE", "RQSTDT", "RSLTDT", dtRqst);

                string ModelList = string.Empty;
                DateTime dt = dtpTarget.SelectedDateTime;

                //2021.04.20 Lot ID 생성 로직 수정 START
                string year = Convert.ToChar((dt.Year - 2001) % 20 + 65).ToString();  //2021.01.13  jinmingfei
                if (dt.Year < 2010) return ModelList;
                string month = Convert.ToChar(dt.Month + 64).ToString();
                //2021.04.20 Lot ID 생성 로직 수정 END

                string Line_id = null;
                string Model_id = null;
                string Model_name = null;
                string Lot_id = null;

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    Line_id = dtRslt.Rows[i][0].ToString();
                    Model_id = dtRslt.Rows[i][1].ToString();
                    Model_name = dtRslt.Rows[i][2].ToString();
                    Lot_id = Model_id + year + month;

                    for (int nDay = 1; nDay <= 31; nDay++)
                    {
                        string CurLot_id = Lot_id + string.Format("{0:D2}", nDay).Trim();
                        if (Check_E_Judge_Lot(Line_id, CurLot_id))
                        {
                            //PQCable_LotList.Add(CurLot_id);
                            Model_name/*Model_id*/ = Model_name/*Model_id*/ + "(" + Line_id + ")";
                            if (string.IsNullOrEmpty(ModelList))
                                ModelList = Model_name/*Model_id*/;
                            else
                                ModelList = ModelList + ", " + Model_name/*Model_id*/;

                            break;
                        }
                    }
                }
                return ModelList;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }

        private bool Check_E_Judge_Lot(string line_id, string lot_id)
        {
            //의뢰가능(=REQ_TYPE 30) 기준 : 치수검사일 때, 용량Route, D/E등급 판정 10%이상인 Lot

            //치수검사를 의뢰한 적 있는 Lot은 제외.(검사조건 변경은 BizRule 수정으로)
            if (Check_PQC_LOT(line_id, lot_id))
                return false;

            //ECell이 있는 경우에만 AllCell을 찾도록 BizRule을 분리. 모든Lot의 AllCell을 조회하면 너무 많기 때문
            int nECell = Get_ECell_Count(line_id, lot_id);
            // 리튬석출검사 조회 가능 Lot 검색 추가 2022-01-11
            int nOCell = Get_OCell_Count(line_id, lot_id);

            if (nECell > 0 || nOCell > 0)
            {
                int nAllCell = Get_AllCell_Count(line_id, lot_id);

                if (nECell * 10 >= nAllCell || nOCell * 10 >= nAllCell)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        private int Get_AllCell_Count(string line_id, string lot_id)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = line_id;
                dr["PROD_LOTID"] = lot_id;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_QA_ALL_CELL_COUNT", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0) return 0;

                if (dtRslt.Rows.Count > 0) return int.Parse(dtRslt.Rows[0]["ALL_COUNT"].ToString());

                return 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return 0;
            }
        }

        private int Get_ECell_Count(string line_id, string lot_id)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = line_id;
                dr["PROD_LOTID"] = lot_id;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ECELL_COUNT", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0) return 0;

                if (dtRslt.Rows.Count > 0) return int.Parse(dtRslt.Rows[0]["E_COUNT"].ToString());

                return 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return 0;
            }
        }
        private int Get_OCell_Count(string line_id, string lot_id)
        {
            //리튬석출검사 의뢰가능 Lot 조회
            //용량/재작업/HIST, 전용OCV#2 10%이상인 Lot 대상
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = line_id;
                dr["PROD_LOTID"] = lot_id;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_OCELL_COUNT", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0) return 0;

                if (dtRslt.Rows.Count > 0) return int.Parse(dtRslt.Rows[0]["O_COUNT"].ToString());

                return 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return 0;
            }
        }

        private bool Check_PQC_LOT(string line_id, string lot_id)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("DAY_GR_LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = line_id;
                dr["DAY_GR_LOTID"] = lot_id;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CHECK_PQC", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0) return false;

                if (dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Rows[0]["RLT"].ToString().Equals("0"))
                        return true;
                    else
                        return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void PageLock(bool bLock)
        {
            tcReqIns.IsEnabled = !bLock;
        }

        #endregion

        #endregion


        #region [Event]

        #region [검사대상 Tabpage]
        private void dtpTarget_ValueChanged(object sender, RoutedEventArgs e)
        {
            dgTarget.ItemsSource = null;
            dgTarget.Columns.Clear();
            dgTarget.Refresh();

            Init_tpInsObj();
        }

        private void btnchekAllLot_Click(object sender, RoutedEventArgs e)
        {
            string message = string.Empty;
            message = MessageDic.Instance.GetMessage("FM_ME_0299").Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            txtPQCable.Text = message;  //PQC 의뢰가능한 모델 조회중입니다. 잠시만 기다려주십시오...

            PageLock(true);
            string ModelList = CHECK_ALL_MODEL_PQCABLE();

            if (string.IsNullOrEmpty(ModelList))
            {
                message = MessageDic.Instance.GetMessage("FM_ME_0300").Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
                txtPQCable.Text = message;  //의뢰가능한 모델이 없습니다.
            }
            else
            {
                txtPQCable.Text = ModelList;
            }

            PageLock(false);
        }

        private void btnSearchTarget_Click(object sender, RoutedEventArgs e)
        {
            if (cboModelTarget.SelectedValue == null)
                return;

            dtpTarget_ValueChanged(sender, e);
            GetListTarget();
        }

        private void dgTarget_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgTarget.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Index < tpReqInsW_day_startColumn)
                    {
                        return;
                    }

                    //2021.04.08 검사대상 더블클릭 시 .Tag 값이 Null인 경우 Return 처리 START
                    if (cell.Presenter.Tag == null)
                    {
                        return;
                    }
                    //2021.04.08 검사대상 더블클릭 시 .Tag 값이 Null인 경우 Return 처리 END

                    //화면 ID 확인 후 수정
                    loadingIndicator.Visibility = Visibility.Visible;

                    string sPqcID = cell.Presenter.Tag.ToString().Split('_')[0];
                    string sReqType = cell.Presenter.Tag.ToString().Split('_')[1];

                    //검사완료
                    if (sReqType.Equals(REQTYPE1))
                    {
                        if (!cell.Presenter.Tag.ToString().Equals(string.Empty))
                        {
                            tcReqIns.SelectedItem = tpSearchIns;
                            GetReqList(sPqcID);
                            GetReqCellList(sPqcID);
                        }
                    }
                    //의뢰완료
                    else if (sReqType.Equals(REQTYPE2))
                    {
                        tcReqIns.SelectedItem = tpSearchIns;
                        GetReqList(sPqcID);
                        GetReqCellList(sPqcID);
                    }
                    //의뢰대기
                    else if (sReqType.Equals(REQTYPE3))
                    {
                        tcReqIns.SelectedItem = tpSearchIns;
                        GetReqList(sPqcID);
                        GetReqCellList(sPqcID);
                    }
                    //의뢰가능
                    else if (sReqType.Equals(REQTYPE4))
                    {
                        return;
                    }
                    //생성대기
                    else if (sReqType.Equals(REQTYPE5))
                    {
                        tcReqIns.SelectedItem = tpSaveCell;
                        cboCellStep.SelectedValue = Util.NVC(dgTarget.GetCell(cell.Row.Index, 4).Text);
                    }

                    loadingIndicator.Visibility = Visibility.Collapsed;
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

        private void dgTarget_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    //////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);

                    //2023.11.09 추가
                    e.Cell.Presenter.Padding = new Thickness(0);
                    e.Cell.Presenter.Margin = new Thickness(0);
                    e.Cell.Presenter.BorderBrush = Brushes.LightGray;
                    e.Cell.Presenter.BorderThickness = new Thickness(0, 1, 1, 0);
                    /////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Row.Index.Equals(0) || e.Cell.Row.Index.Equals(dgTarget.Rows.Count / 2))
                    {
                        e.Cell.Presenter.Height = 50;
                        e.Cell.Presenter.IsEnabled = false;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.WhiteSmoke);
                    }
                    else
                    {
                        if (e.Cell.Column.Index >= tpReqInsW_day_startColumn && e.Cell.Column.Index < tpReqInsW_day_endColumn)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }

                    #region [데이터 뿌려주기]
                    int idgTargetRowCnt = dgTarget.Rows.Count;
                    int iFstRow = 0;
                    int iSecRow = idgTargetRowCnt / 2;
                    string sColName = string.Empty;
                    string expression;
                    DataRow[] foundRows;

                    if (e.Cell.Row.Index == iFstRow || e.Cell.Row.Index == iSecRow) return;
                    if (e.Cell.Column.Index < 5) return;

                    // 헤드 행일 경우 컬럼명 Get
                    if (e.Cell.Row.Index > iSecRow)
                    {
                        sColName = Util.NVC(dgTarget[iSecRow, e.Cell.Column.Index].Text);
                    }
                    else
                    {
                        sColName = Util.NVC(dgTarget[iFstRow, e.Cell.Column.Index].Text);
                    }

                    if (string.IsNullOrEmpty(sColName)) return;

                    expression = " REQ_OP = '" + Util.NVC(dgTarget[e.Cell.Row.Index, 4].Text) + "'  AND LOT_DAY = '" + sColName + "' ";

                    foundRows = dtTemp.Select(expression);

                    if (foundRows.Length != 0)
                    {
                        //의뢰 수량
                        DataTableConverter.SetValue(dgTarget.Rows[e.Cell.Row.Index].DataItem, dgTarget.Columns[e.Cell.Column.Index].Name, foundRows[0]["REQ_CNT"].ToString());

                        int reqType = int.Parse(foundRows[0]["REQ_TYPE"].ToString());
                        string sPqcID = Util.NVC(foundRows[0]["PQC_ID"].ToString());

                        //검사완료
                        if (reqType / 10000 == 1)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Teal);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            e.Cell.Presenter.Tag = sPqcID + "_" + REQTYPE1;
                        }
                        //의뢰완료
                        else if (reqType / 1000 == 1)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.LimeGreen);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            e.Cell.Presenter.Tag = sPqcID + "_" + REQTYPE2;
                        }
                        //의뢰대기
                        else if (reqType / 100 == 1)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            e.Cell.Presenter.Tag = sPqcID + "_" + REQTYPE3;
                        }
                        //의뢰가능
                        else if (reqType / 20 == 1 || reqType / 30 == 1)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            e.Cell.Presenter.Tag = sPqcID + "_" + REQTYPE4;
                        }
                        //생성대기
                        else if (reqType / 10 == 1)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Violet);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Tag = sPqcID + "_" + REQTYPE5;
                        }
                        //생산없음
                        else if (reqType / 1 == 1)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.DimGray);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            e.Cell.Presenter.Tag = sPqcID + "_" + REQTYPE6;
                        }

                        //var _mergeList = new List<DataGridCellsRange>();
                        ////string sPeriodReqDay = Util.NVC(foundRows[0]["PERIOD_REQ_DAY"]);
                        //string sPeriodReqDay = "15";

                        //if (int.Parse(sPeriodReqDay) > 1 && foundRows[0]["REQ_TIME"].ToString().Equals("P")) // 2014년 10월 이후 안전성 혼합
                        //{
                        //    // 열병합 수량이 넘어가는 경우
                        //    if (int.Parse(sPeriodReqDay) + e.Cell.Column.Index > dgTarget.Columns.Count)
                        //    {
                        //        //수정필요
                        //        //fpsTarget.ActiveSheet.Cells[iRow, tpReqInsW_day_startColumn].ColumnSpan = dgTarget.Columns.Count - tpReqInsW_day_startColumn; //마지막 열까지 병합

                        //        //_mergeList.Add(new DataGridCellsRange(dgTarget.GetCell(e.Cell.Row.Index, tpReqInsW_day_startColumn), dgTarget.GetCell(e.Cell.Row.Index, (tpReqInsW_day_endColumn - 1))));


                        //        //나머지 병합은 16일 이후 병합은 다음행(동일 의뢰)에서 열 병합 
                        //        //for (int jRow = dgTarget.Rows.Count / 2 + 1; jRow < dgTarget.Rows.Count; jRow++)
                        //        //{
                        //        //    if (dgTarget[e.Cell.Row.Index, 2].Presenter.Tag.ToString() == dgTarget[jRow, 2].Presenter.Tag.ToString())
                        //        //    {
                        //        //        //수정필요
                        //        //        //fpsTarget.ActiveSheet.Cells[jRow, tpReqInsW_day_startColumn].ColumnSpan = int.Parse(foundRows[0]["PERIOD_REQ_DAY"].ToString())
                        //        //        //                                                                            - (dgTarget.Columns.Count - tpReqInsW_day_startColumn);

                        //        //        DataTableConverter.SetValue(dgTarget.Rows[jRow].DataItem, dgTarget.Columns[tpReqInsW_day_startColumn].Name, foundRows[0]["REQ_CNT"].ToString());

                        //        //        ////의뢰표시
                        //        //        ////검사완료
                        //        //        //if (reqType / 10000 == 1)
                        //        //        //{
                        //        //        //    dgTarget[jRow, tpReqInsW_day_startColumn].Presenter.Background = new SolidColorBrush(Colors.Teal);
                        //        //        //    dgTarget[jRow, tpReqInsW_day_startColumn].Presenter.Foreground = new SolidColorBrush(Colors.White);
                        //        //        //    dgTarget[jRow, tpReqInsW_day_startColumn].Presenter.FontWeight = FontWeights.Bold;
                        //        //        //    dgTarget[jRow, tpReqInsW_day_startColumn].Presenter.Tag = foundRows[0]["PQC_ID"].ToString();
                        //        //        //}
                        //        //        ////의뢰완료
                        //        //        //else if (reqType / 1000 == 1)
                        //        //        //{
                        //        //        //    dgTarget[jRow, tpReqInsW_day_startColumn].Presenter.Background = new SolidColorBrush(Colors.LimeGreen);
                        //        //        //    dgTarget[jRow, tpReqInsW_day_startColumn].Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //        //        //    dgTarget[jRow, tpReqInsW_day_startColumn].Presenter.FontWeight = FontWeights.Bold;
                        //        //        //    dgTarget[jRow, tpReqInsW_day_startColumn].Presenter.Tag = foundRows[0]["PQC_ID"].ToString();
                        //        //        //}
                        //        //        ////의뢰대기
                        //        //        //else if (reqType / 100 == 1)
                        //        //        //{
                        //        //        //    dgTarget[jRow, tpReqInsW_day_startColumn].Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        //        //        //    dgTarget[jRow, tpReqInsW_day_startColumn].Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //        //        //    dgTarget[jRow, tpReqInsW_day_startColumn].Presenter.FontWeight = FontWeights.Bold;
                        //        //        //    dgTarget[jRow, tpReqInsW_day_startColumn].Presenter.Tag = foundRows[0]["PQC_ID"].ToString();
                        //        //        //}
                        //        //        ////의뢰가능
                        //        //        //else if (reqType / 20 == 1)
                        //        //        //{
                        //        //        //    dgTarget[jRow, tpReqInsW_day_startColumn].Presenter.Background = new SolidColorBrush(Colors.Orange);
                        //        //        //    dgTarget[jRow, tpReqInsW_day_startColumn].Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //        //        //    dgTarget[jRow, tpReqInsW_day_startColumn].Presenter.FontWeight = FontWeights.Bold;
                        //        //        //}
                        //        //        ////생성대기
                        //        //        //else if (reqType / 10 == 1)
                        //        //        //{
                        //        //        //    dgTarget[jRow, tpReqInsW_day_startColumn].Presenter.Background = new SolidColorBrush(Colors.Violet);
                        //        //        //    dgTarget[jRow, tpReqInsW_day_startColumn].Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        //        //        //}
                        //        //        ////생산없음
                        //        //        //else if (reqType / 1 == 1)
                        //        //        //{
                        //        //        //    dgTarget[jRow, tpReqInsW_day_startColumn].Presenter.Background = new SolidColorBrush(Colors.DimGray);
                        //        //        //    dgTarget[jRow, tpReqInsW_day_startColumn].Presenter.Foreground = new SolidColorBrush(Colors.White);
                        //        //        //    dgTarget[jRow, tpReqInsW_day_startColumn].Presenter.FontWeight = FontWeights.Bold;
                        //        //        //}
                        //        //    }
                        //        //}
                        //    }
                        //    //수정필요
                        //    //fpsTarget.ActiveSheet.Cells[iRow, iCol].ColumnSpan = int.Parse(foundRows[0]["PERIOD_REQ_DAY"].ToString());
                        //}
                    }
                    #endregion
                }
            }));
        }

        #endregion

        #region [Cell 저장 Tabpage]
        private void btnCellSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtCellId.Text))
            {
                Util.MessageValidation("FM_ME_0019");  //Cell ID를 입력해주세요.
                return;
            }

            ScanCellId(txtCellId.Text);
            txtCellId.Text = string.Empty;
            txtCellId.SelectAll();
        }

        private void txtCellId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(txtCellId.Text))
                    return;
                try
                {
                    ScanCellId(txtCellId.Text);
                    txtCellId.Text = string.Empty;
                    txtCellId.SelectAll();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void btnCellClear_Click(object sender, RoutedEventArgs e)
        {
            Init_tpSaveCell();
        }

        private void btnCellSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgCellList.Rows.Count == 0)
                    return;

                Util.MessageConfirm("FM_ME_0059", (result) =>  //PQC ID를 발번하시겠습니까?
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }
                    else
                    {
                        if (cboCellStep.SelectedIndex == 0)
                        {
                            Util.MessageValidation("FM_ME_0251", new string[] { ObjectDic.Instance.GetObjectName("REQ_STEP") });  //필수항목누락 : {0}
                            return;
                        }
                        if (string.IsNullOrEmpty(txtReqDesc.Text))
                        {
                            Util.MessageValidation("FM_ME_0251", new string[] { ObjectDic.Instance.GetObjectName("REQ_DESC") });  //필수항목누락 : {0}
                            return;
                        }

                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "INDATA";
                        dtRqst.Columns.Add("SUBLOTID", typeof(string));
                        dtRqst.Columns.Add("REQ_USER", typeof(string));
                        dtRqst.Columns.Add("REQ_OP", typeof(string));
                        dtRqst.Columns.Add("EXTRA_INFORM_SND", typeof(string));
                        dtRqst.Columns.Add("UPDUSERID", typeof(string));
                        dtRqst.Columns.Add("AREAID", typeof(string));
                        dtRqst.Columns.Add("LANGID", typeof(string));
                        dtRqst.Columns.Add("MENUID", typeof(string));  // 2025.04.09 이현승 :등급변경 적용 UI 추적성 향상을 위한 MENUID 추가
                        txtReqDesc.Text = cboCellStep.Text + "\r\n" + txtReqDesc.Text;
                        for (int i = 0; i < dgCellList.Rows.Count; i++)
                        {
                            DataRow dr = dtRqst.NewRow();
                            dr["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "SUBLOTID"));
                            if (i == 0)
                            {
                                dr["REQ_USER"] = Util.GetCondition(cboCellUser);
                                dr["REQ_OP"] = Util.GetCondition(cboCellStep);
                                dr["EXTRA_INFORM_SND"] = Util.GetCondition(txtReqDesc);
                            }
                            dr["UPDUSERID"] = LoginInfo.USERID;
                            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                            dr["LANGID"] = LoginInfo.LANGID;
                            dr["MENUID"] = LoginInfo.CFG_MENUID;
                            dtRqst.Rows.Add(dr);
                        }

                        DataTable dtRqst1 = new DataTable();
                        dtRqst1.TableName = "INLOT";
                        dtRqst1.Columns.Add("LOT_ID", typeof(string));
                        dtRqst1.Columns.Add("CELL_CNT", typeof(Int32));
                        dtRqst1.Columns.Add("LOT_MIX_YN", typeof(string));

                        for (int i = 0; i < lotList_tpSaveCell.Count; i++)
                        {
                            //행데이터 생성
                            DataRow drNewRow = dtRqst1.NewRow();
                            drNewRow["LOT_ID"] = lotList_tpSaveCell[i];
                            drNewRow["CELL_CNT"] = cellCnt_tpSaveCell[i];
                            if (lotMixReqTab1)
                            {
                                drNewRow["LOT_MIX_YN"] = "Y";
                            }
                            else
                            {
                                drNewRow["LOT_MIX_YN"] = "N";
                            }
                            dtRqst1.Rows.Add(drNewRow);
                        }

                        DataSet dsRqst = new DataSet();
                        dsRqst.Tables.Add(dtRqst);
                        dsRqst.Tables.Add(dtRqst1);

                        new ClientProxy().ExecuteService_Multi("BR_SET_REQ_CELL_SAVE", "INDATA,INLOT", "OUTDATA", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                                {
                                    Util.MessageInfo("FM_ME_0058");  //PQC ID 발행을 완료하였습니다.
                                    Init_tpSaveCell();
                                }
                                else
                                {
                                    Util.MessageInfo("FM_ME_0057");  //PQC ID 발행에 실패하였습니다.
                                }
                            }
                            catch (Exception ex)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            }
                        }, dsRqst);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboCellStep_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            if (lotMixReqTab1)
            {
                Util.gridClear(dgCellList);
                lotList_tpSaveCell = new List<String>();
                cellCnt_tpSaveCell = new List<int>();
            }

            for (int row = 0; row < dgCellList.Rows.Count; row++)
            {
                DataTableConverter.SetValue(row, "REQ_STEP", cboCellStep.Text);
            }

            lotMixReqTab1 = false;
            //안전성 LOT 혼용검사시 periodReq_tab1 표시

            DataTable dt = ((DataView)cboCellStep.ItemsSource).Table;
            for (int i = 1; i < dt.Rows.Count; i++)
            {
                // LOT 혼합 검사
                if (cboCellStep.SelectedValue.ToString() == dt.Rows[i]["CBO_CODE"].ToString()
                    && int.Parse(dt.Rows[i]["PERIOD_REQ_DAY"].ToString()) > 1)
                {
                    lotMixReqTab1 = true;
                    lotMixReqDayTab1 = int.Parse(dt.Rows[i]["PERIOD_REQ_DAY"].ToString());
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                buttonAccess(sender);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void buttonAccess(object sender)
        {
            try
            {

                Button btnDelCell = sender as Button;
                DataTable dt = DataTableConverter.Convert(dgCellList.ItemsSource);
                if (btnDelCell != null)
                {
                    DataRowView dataRow = ((FrameworkElement)sender).DataContext as DataRowView;

                    if (string.Equals(btnDelCell.Name, "DELETE"))
                    {
                        DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

                        if (presenter == null)
                            return;

                        if (!FrameOperation.AUTHORITY.Equals("W"))
                        {
                            return;
                        }

                        if (lotMixReqTab1)
                        {
                            cellCnt_tpSaveCell[0] = cellCnt_tpSaveCell[0] - 1;
                        }
                        else
                        {
                            cellCnt_tpSaveCell[lotList_tpSaveCell.IndexOf(Util.NVC(DataTableConverter.GetValue(dgCellList.CurrentRow.DataItem, "DAY_GR_LOTID")))] =
                                cellCnt_tpSaveCell[lotList_tpSaveCell.IndexOf(Util.NVC(DataTableConverter.GetValue(dgCellList.CurrentRow.DataItem, "DAY_GR_LOTID")))] - 1;
                        }

                        if (lotMixReqTab1)
                        {
                            if (cellCnt_tpSaveCell[0] == 0)
                            {
                                cellCnt_tpSaveCell.RemoveAt(0);
                                lotList_tpSaveCell.RemoveAt(0);
                            }
                        }
                        else
                        {
                            if (cellCnt_tpSaveCell[lotList_tpSaveCell.IndexOf(Util.NVC(DataTableConverter.GetValue(dgCellList.CurrentRow.DataItem, "DAY_GR_LOTID")))] == 0)
                            {
                                cellCnt_tpSaveCell.RemoveAt(lotList_tpSaveCell.IndexOf(Util.NVC(DataTableConverter.GetValue(dgCellList.CurrentRow.DataItem, "DAY_GR_LOTID"))));
                                lotList_tpSaveCell.RemoveAt(lotList_tpSaveCell.IndexOf(Util.NVC(DataTableConverter.GetValue(dgCellList.CurrentRow.DataItem, "DAY_GR_LOTID"))));
                            }
                        }

                        int clickedIndex = presenter.Row.Index;
                        dt.Rows.RemoveAt(clickedIndex);
                        Util.GridSetData(presenter.DataGrid, dt, this.FrameOperation);

                        if (dgCellList.Rows.Count == 0)
                            return;

                        //Lot 별 Cell 순번 재정의
                        int icount = 0;
                        string lotId = Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[0].DataItem, "DAY_GR_LOTID"));
                        for (int iRow = 0; iRow < dgCellList.Rows.Count; iRow++)
                        {
                            /* 2014.10.30 정종덕D // [CSR ID:2609471] FCS 제품검사 의뢰 기준 변경 요청
                             * 안전성검사 lot 혼합의뢰 이면 LOT별 수량을 체크하지 않는다.
                             * */
                            if (lotMixReqTab1)
                            {
                                icount++;
                                DataTableConverter.SetValue(dgCellList.Rows[iRow].DataItem, "PQC_REQ_GROUP", icount.ToString());
                                continue;
                            }

                            icount++;

                            DataTableConverter.SetValue(dgCellList.Rows[iRow].DataItem, "PQC_REQ_GROUP", icount.ToString());
                            if (lotId.Equals(Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[iRow].DataItem, "DAY_GR_LOTID"))))
                            {
                                continue;
                            }

                            icount = 1;
                            lotId = Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[iRow].DataItem, "DAY_GR_LOTID"));
                            DataTableConverter.SetValue(dgCellList.Rows[iRow].DataItem, "PQC_REQ_GROUP", icount.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCellList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("DELETE"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                        e.Column.HeaderPresenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }
        #endregion

        #region [검사의뢰 Tabpage]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnReq_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("FM_ME_0222", (result) =>  //제품검사 의뢰 하시겠습니까?
            {
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
                else
                {
                    if (dgReq.Rows.Count <= 0)
                    {
                        Util.MessageValidation("FM_ME_0240");  //처리할 데이터가 없습니다.
                        return;
                    }

                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("AREAID", typeof(string));
                    dtRqst.Columns.Add("PQC_REQ_ID", typeof(string));
                    dtRqst.Columns.Add("PQC_REQ_GROUP_USER", typeof(string));

                    for (int i = 0; i < dgReq.Rows.Count; i++)
                    {
                        //if (dgReq.Rows[i].Presenter.Visibility == Visibility.Visible)
                        //{
                        //}
                        DataRow dr = dtRqst.NewRow();

                        dr["PQC_REQ_ID"] = Util.NVC(DataTableConverter.GetValue(dgReq.Rows[i].DataItem, "PQC_REQ_ID"));
                        dr["PQC_REQ_GROUP_USER"] = LoginInfo.USERNAME;
                        dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                        dtRqst.Rows.Add(dr);
                    }

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_REQ_LOT_COMPARE", "RQSTDT", "RSLTDT", dtRqst);

                    if (dtRslt.Rows.Count > 0)
                    {
                        if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("2"))
                        {
                            Util.MessageConfirm("FM_ME_0004", (result1) =>  //[Lot ID : {0}]은 의뢰되지 않은 Lot 입니다. 계속하시겠습니까?
                            {
                                if (result1 != MessageBoxResult.OK)
                                {
                                    return;
                                }
                                SetReqPQC();
                            }, new string[] { dtRslt.Rows[0]["LOT_ID"].ToString() });
                        }
                        else
                        {
                            SetReqPQC();
                        }
                    }
                    else
                    {
                        SetReqPQC();
                    }

                }
            });
        }

        private void SetReqPQC()
        {
            bool bReturn = false;

            for (int i = 0; i < dgReq.Rows.Count; i++)
            {
                DataTable dtRqst2 = new DataTable();
                dtRqst2.TableName = "RQSTDT";
                dtRqst2.Columns.Add("AREAID", typeof(string));
                dtRqst2.Columns.Add("PQC_REQ_ID", typeof(string));
                dtRqst2.Columns.Add("PQC_REQ_GROUP_USER", typeof(string));

                //if (dgReq.Rows[i].Presenter.Visibility == Visibility.Visible)
                //{
                //}
                DataRow dr2 = dtRqst2.NewRow();
                dr2["PQC_REQ_ID"] = Util.NVC(DataTableConverter.GetValue(dgReq.Rows[i].DataItem, "PQC_REQ_ID"));
                dr2["PQC_REQ_GROUP_USER"] = LoginInfo.USERNAME;
                dr2["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst2.Rows.Add(dr2);
                DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("BR_SET_REQ_PQC", "RQSTDT", "RSLTDT", dtRqst2);
                if (dtRslt1.Rows.Count > 0)
                {
                    if (dtRslt1.Rows[0]["RETVAL"].ToString().Equals("0"))
                    {
                        bReturn = true; //제품검사의뢰 완료
                    }
                }
            }
            if (bReturn) Util.MessageValidation("FM_ME_0223");  //제품검사 의뢰를 완료하였습니다.
            else Util.MessageValidation("FM_ME_0224");  //제품검사 의뢰에 실패하였습니다.
            Init_tpSaveCell();
            GetList();
        }

        private void dgReqGroup_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgReqGroup.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (!cell.Column.Name.Equals("PQC_REQ_GR_ID"))
                    {
                        return;
                    }

                    OpenReqList(Util.NVC(DataTableConverter.GetValue(dgReqGroup.CurrentRow.DataItem, "PQC_REQ_GR_ID")), string.Empty);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgReqAll_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);

            C1.WPF.DataGrid.DataGridCell cell = dgReqAll.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (!cell.Column.Name.Equals("CHK"))
                {
                    return;
                }

                // DataTableConverter.SetValue(dgReqAll.Rows[cell.Row.Index].DataItem, "CHK", false);
                //  dgReqAll.Rows[cell.Row.Index].Presenter.Visibility = Visibility.Collapsed;
                //  dgReq.Rows[cell.Row.Index].Presenter.Visibility = Visibility.Visible;

                DataTable dtTemp = new DataTable();
                if (dgReq.Rows.Count == 0)
                {

                    dtTemp.Columns.Add("CHK", typeof(bool));
                    dtTemp.Columns.Add("PQC_REQ_ID", typeof(string));
                    dtTemp.Columns.Add("DAY_GR_LOTID", typeof(string));
                    dtTemp.Columns.Add("SAMPLE_ISSUE_DAY", typeof(DateTime));
                    dtTemp.Columns.Add("PQC_REQ_QTY", typeof(Decimal));
                    dtTemp.Columns.Add("PQC_REQ_USER", typeof(string));
                    dtTemp.Columns.Add("PQC_REQ_GR_ID", typeof(string));
                }
                else
                {
                    dtTemp = DataTableConverter.Convert(dgReq.ItemsSource);
                }

                //Row Add
                DataRow dr = dtTemp.NewRow();

                dr["CHK"] = false;
                dr["PQC_REQ_ID"] = Util.NVC(DataTableConverter.GetValue(dgReqAll.Rows[cell.Row.Index].DataItem, "PQC_REQ_ID"));
                dr["DAY_GR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReqAll.Rows[cell.Row.Index].DataItem, "DAY_GR_LOTID"));
                dr["SAMPLE_ISSUE_DAY"] = Util.NVC(DataTableConverter.GetValue(dgReqAll.Rows[cell.Row.Index].DataItem, "SAMPLE_ISSUE_DAY"));
                dr["PQC_REQ_QTY"] = Util.NVC(DataTableConverter.GetValue(dgReqAll.Rows[cell.Row.Index].DataItem, "PQC_REQ_QTY"));
                dr["PQC_REQ_USER"] = Util.NVC(DataTableConverter.GetValue(dgReqAll.Rows[cell.Row.Index].DataItem, "PQC_REQ_USER"));
                dr["PQC_REQ_GR_ID"] = Util.NVC(DataTableConverter.GetValue(dgReqAll.Rows[cell.Row.Index].DataItem, "PQC_REQ_GR_ID"));
                dtTemp.Rows.Add(dr);

                DataView dv = new DataView(dtTemp);
                dv.Sort = "PQC_REQ_ID";
                DataTable dtSort = dv.ToTable();

                Util.GridSetData(dgReq, dtSort, this.FrameOperation);

                //Row Remove
                DataTable dt = DataTableConverter.Convert(dgReqAll.ItemsSource);
                dt.Rows.RemoveAt(cell.Row.Index);
                Util.GridSetData(dgReqAll, dt, this.FrameOperation);

                QaReqSummary();
            }
        }

        private void dgReqAll_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgReqAll.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (!cell.Column.Name.Equals("PQC_REQ_ID"))
                    {
                        return;
                    }

                    OpenReqList("", Util.NVC(DataTableConverter.GetValue(dgReqAll.CurrentRow.DataItem, "PQC_REQ_ID")));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgReq_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgReq.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (!cell.Column.Name.Equals("CHK"))
                {
                    return;
                }

                /*
                  DataTableConverter.SetValue(dgReq.Rows[cell.Row.Index].DataItem, "CHK", false);
                 dgReq.Rows[cell.Row.Index].Presenter.Visibility = Visibility.Collapsed;
                 dgReqAll.Rows[cell.Row.Index].Presenter.Visibility = Visibility.Visible;
                  */
                DataTable dtTemp = new DataTable();
                if (dgReqAll.Rows.Count == 0)
                {
                    dtTemp.Columns.Add("CHK", typeof(bool));
                    dtTemp.Columns.Add("PQC_REQ_ID", typeof(string));
                    dtTemp.Columns.Add("DAY_GR_LOTID", typeof(string));
                    dtTemp.Columns.Add("SAMPLE_ISSUE_DAY", typeof(DateTime));
                    dtTemp.Columns.Add("PQC_REQ_QTY", typeof(Decimal));
                    dtTemp.Columns.Add("PQC_REQ_USER", typeof(string));
                    dtTemp.Columns.Add("PQC_REQ_GR_ID", typeof(string));
                }
                else
                {
                    dtTemp = DataTableConverter.Convert(dgReqAll.ItemsSource);
                }

                //Row Add
                DataRow dr = dtTemp.NewRow();

                dr["CHK"] = false;
                dr["PQC_REQ_ID"] = Util.NVC(DataTableConverter.GetValue(dgReq.Rows[cell.Row.Index].DataItem, "PQC_REQ_ID"));
                dr["DAY_GR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReq.Rows[cell.Row.Index].DataItem, "DAY_GR_LOTID"));
                dr["SAMPLE_ISSUE_DAY"] = Util.NVC(DataTableConverter.GetValue(dgReq.Rows[cell.Row.Index].DataItem, "SAMPLE_ISSUE_DAY"));
                dr["PQC_REQ_QTY"] = Util.NVC(DataTableConverter.GetValue(dgReq.Rows[cell.Row.Index].DataItem, "PQC_REQ_QTY"));
                dr["PQC_REQ_USER"] = Util.NVC(DataTableConverter.GetValue(dgReq.Rows[cell.Row.Index].DataItem, "PQC_REQ_USER"));
                dr["PQC_REQ_GR_ID"] = Util.NVC(DataTableConverter.GetValue(dgReq.Rows[cell.Row.Index].DataItem, "PQC_REQ_GR_ID"));
                dtTemp.Rows.Add(dr);

                DataView dv = new DataView(dtTemp);
                dv.Sort = "PQC_REQ_ID";
                DataTable dtSort = dv.ToTable();

                Util.GridSetData(dgReqAll, dtSort, this.FrameOperation);

                //Row Remove
                DataTable dt = DataTableConverter.Convert(dgReq.ItemsSource);
                dt.Rows.RemoveAt(cell.Row.Index);
                Util.GridSetData(dgReq, dt, this.FrameOperation);
                QaReqSummary();
            }
        }

        private void dgReq_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgReq.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (!cell.Column.Name.Equals("PQC_REQ_ID"))
                    {
                        return;
                    }

                    OpenReqList("", Util.NVC(DataTableConverter.GetValue(dgReqList.CurrentRow.DataItem, "PQC_REQ_ID")));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void chkHeaderAllReqAll_Checked(object sender, RoutedEventArgs e)
        {
            /*for (int i = 0; i < dgReqAll.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgReqAll.Rows[i].DataItem, "CHK", false);
                dgReqAll.Rows[i].Presenter.Visibility = Visibility.Collapsed;
                dgReq.Rows[i].Presenter.Visibility = Visibility.Visible;
            }*/
            DataTable dtTempAll = new DataTable();
            if (dgReqAll.Rows.Count == 0)
            {
                chkHeaderAllReqAll.IsChecked = false;
                return;
            }

            dtTempAll = DataTableConverter.Convert(dgReqAll.ItemsSource);
            dtTempAll = dtTempAll.DefaultView.ToTable(false, new string[] { "CHK", "PQC_REQ_ID", "DAY_GR_LOTID", "SAMPLE_ISSUE_DAY", "PQC_REQ_QTY", "PQC_REQ_USER", "PQC_REQ_GR_ID" });
            DataTable dtTemp = new DataTable();
            if (dgReq.Rows.Count == 0)
            {
                dtTemp.Columns.Add("CHK", typeof(bool));
                dtTemp.Columns.Add("PQC_REQ_ID", typeof(string));
                dtTemp.Columns.Add("DAY_GR_LOTID", typeof(string));
                dtTemp.Columns.Add("SAMPLE_ISSUE_DAY", typeof(DateTime));
                dtTemp.Columns.Add("PQC_REQ_QTY", typeof(Decimal));
                dtTemp.Columns.Add("PQC_REQ_USER", typeof(string));
                dtTemp.Columns.Add("PQC_REQ_GR_ID", typeof(string));
            }
            else
            {
                dtTemp = DataTableConverter.Convert(dgReq.ItemsSource);
            }

            dtTemp.Merge(dtTempAll);

            DataView dv = new DataView(dtTemp);
            dv.Sort = "PQC_REQ_ID";
            DataTable dtSort = dv.ToTable();

            Util.GridSetData(dgReq, dtSort, this.FrameOperation);
            Util.gridClear(dgReqAll);
            chkHeaderAllReqAll.IsChecked = false;
            QaReqSummary();

        }

        private void chkHeaderAllReqAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgReqAll);
        }

        private void chkHeaderAllReq_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dtTemp = new DataTable();
            if (dgReq.Rows.Count == 0)
            {
                chkHeaderAllReq.IsChecked = false;
                return;
            }

            dtTemp = DataTableConverter.Convert(dgReq.ItemsSource);

            DataTable dtTempAll = new DataTable();
            if (dgReqAll.Rows.Count == 0)
            {
                dtTempAll.Columns.Add("CHK", typeof(bool));
                dtTempAll.Columns.Add("PQC_REQ_ID", typeof(string));
                dtTempAll.Columns.Add("DAY_GR_LOTID", typeof(string));
                dtTempAll.Columns.Add("SAMPLE_ISSUE_DAY", typeof(DateTime));
                dtTempAll.Columns.Add("PQC_REQ_QTY", typeof(Decimal));
                dtTempAll.Columns.Add("PQC_REQ_USER", typeof(string));
                dtTempAll.Columns.Add("PQC_REQ_GR_ID", typeof(string));
            }
            else
            {
                dtTempAll = DataTableConverter.Convert(dgReqAll.ItemsSource);
            }
            dtTempAll = dtTempAll.DefaultView.ToTable(false, new string[] { "CHK", "PQC_REQ_ID", "DAY_GR_LOTID", "SAMPLE_ISSUE_DAY", "PQC_REQ_QTY", "PQC_REQ_USER", "PQC_REQ_GR_ID" });

            dtTempAll.Merge(dtTemp);

            DataView dv = new DataView(dtTempAll);
            dv.Sort = "PQC_REQ_ID";
            DataTable dtSort = dv.ToTable();

            Util.GridSetData(dgReqAll, dtSort, this.FrameOperation);
            Util.gridClear(dgReq);
            chkHeaderAllReq.IsChecked = false;
            QaReqSummary();
        }

        private void chkHeaderAllReq_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgReq);
        }

        private void dgReqGroup_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("PQC_REQ_GR_ID"))
                    {
                        //2021.04.09 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 START
                        //e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                        //e.Column.HeaderPresenter.Cursor = Cursors.Hand;
                        //2021.04.09 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 END
                    }
                }
            }));
        }

        private void dgReqAll_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("PQC_REQ_ID"))
                    {
                        //2021.04.09 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 START
                        //    e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //    e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                        //    e.Column.HeaderPresenter.Cursor = Cursors.Hand;
                        //2021.04.09 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 END
                    }
                }
            }));
        }

        private void dgReq_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("PQC_REQ_ID"))
                    {
                        //2021.04.09 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 START
                        //e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                        //e.Column.HeaderPresenter.Cursor = Cursors.Hand;
                        //2021.04.09 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 END
                    }
                }
            }));
        }

        //2021.04.09 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 START
        private void dgReqGroup_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////
                    if (e.Cell.Column.Name.Equals("PQC_REQ_GR_ID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }
        //2021.04.09 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 END

        //2021.04.09 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 START
        private void dgReqAll_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////
                    if (e.Cell.Column.Name.Equals("PQC_REQ_ID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }
        //2021.04.09 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 END

        //2021.04.09 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 START
        private void dgReq_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////
                    if (e.Cell.Column.Name.Equals("PQC_REQ_ID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }
        //2021.04.09 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 END
        #endregion

        #region [검사조회 Tabpage]
        private void btnReqSearch_Click(object sender, RoutedEventArgs e)
        {
            // 더블클릭하여 검사조회 tab으로 이동되었을 경우 고려하여 GetReqList() 에서 호출하도록 변경
            // SetGridCboItem_CommonCode(dgReqList.Columns["REQ_OP"], "PQC_REQ_STEP_CODE");
            // SetGridCboItem_CommonCode(dgReqList.Columns["PQC_RSLT_CODE"], "PQC_RSLT_CODE");
            // SetGridCboItem_CommonCode(dgReqList.Columns["LAST_JUDGE_VALUE"], "LAST_JUDG_VALUE");

            GetReqList();
        }

        private void dgReqList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("PQC_REQ_ID"))
                    {
                        //2021.04.09 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 START
                        //e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                        //e.Column.HeaderPresenter.Cursor = Cursors.Hand;
                        //2021.04.09 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 END
                    }
                }
            }));
        }

        //2021.04.09 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 START
        private void dgReqList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);

                    e.Cell.Presenter.BorderBrush = Brushes.LightGray;
                    e.Cell.Presenter.BorderThickness = new Thickness(0.5, 0, 0, 0.5);
                    ///////////////////////////////////////////////////////////////////////////////////
                    if (e.Cell.Column.Name.Equals("PQC_REQ_ID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                    //기존 콤보박스 내용이 잘리는 현상이 있어 Padding, Margin 최소화
                    if (e.Cell.Column.Name.Equals("PQC_RSLT_CODE"))
                    {
                        e.Cell.Presenter.Padding = new Thickness(0,0,0,0);
                        e.Cell.Presenter.Margin = new Thickness(0,0,0,0);
                    }

                    //요청취소 버튼 제어
                    if (e.Cell.Column.Name.Equals("PQC_REQ_CANCEL"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PQC_RSLT_CODE")).ToString().Equals(PQC_REQ_TRANSFER) && string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LAST_JUDGE_VALUE"))))
                        {
                            e.Cell.Presenter.IsEnabled = true;
                        }
                        else
                        {
                            e.Cell.Presenter.IsEnabled = false;
                        }
                    }
                }
            }));
        }


        //2021.04.09 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 END

        private void dgReqList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgReqList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (!cell.Column.Name.Equals("PQC_REQ_ID"))
                    {
                        return;
                    }

                    if (cell.Column.Name.Equals("PQC_REQ_ID"))
                    {
                        GetReqCellList(Util.NVC(DataTableConverter.GetValue(dgReqList.CurrentRow.DataItem, "PQC_REQ_ID")));

                        if (dgReqList.CurrentCell != null && dgReqList.CurrentRow.Index > -1)
                        {
                            if (chkResult.IsChecked == true)
                            {
                                txtReject.Text = Util.NVC(DataTableConverter.GetValue(dgReqList.CurrentRow.DataItem, "EXTRA_INFORM_RCV"));
                            }
                            else
                            {
                                txtReject.Text = Util.NVC(DataTableConverter.GetValue(dgReqList.CurrentRow.DataItem, "EXTRA_INFORM_SND"));
                            }
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

        private void chkResult_Checked(object sender, RoutedEventArgs e)
        {
            dgReqList.Columns[dgReqList.Columns["SAMPLE_ISSUE_DAY"].Index].Visibility = Visibility.Collapsed;
            dgReqList.Columns[dgReqList.Columns["REQ_OP"].Index].Visibility = Visibility.Collapsed;
            dgReqList.Columns[dgReqList.Columns["PQC_REQ_QTY"].Index].Visibility = Visibility.Collapsed;
            dgReqList.Columns[dgReqList.Columns["DEFECT_CNT"].Index].Visibility = Visibility.Collapsed;
            dgReqList.Columns[dgReqList.Columns["PQC_REQ_USER"].Index].Visibility = Visibility.Collapsed;
            dgReqList.Columns[dgReqList.Columns["EXTRA_INFORM_SND"].Index].Visibility = Visibility.Collapsed;
            dgReqList.Columns[dgReqList.Columns["PQC_CYCL_INSP_FLAG"].Index].Visibility = Visibility.Visible;
            dgReqList.Columns[dgReqList.Columns["PQC_RSLT_CODE"].Index].Visibility = Visibility.Visible;
            dgReqList.Columns[dgReqList.Columns["LAST_JUDGE_VALUE"].Index].Visibility = Visibility.Visible;
            dgReqList.Columns[dgReqList.Columns["LAST_JUDG_DTTM"].Index].Visibility = Visibility.Visible;
            dgReqList.Columns[dgReqList.Columns["LAST_JUDG_USERID"].Index].Visibility = Visibility.Visible;
            dgReqList.Columns[dgReqList.Columns["EXTRA_INFORM_RCV"].Index].Visibility = Visibility.Visible;

            lblDesc.Text = ObjectDic.Instance.GetObjectName("REMARK_REQ_REJ");  //비고(요청거부사항)

            if (dgReqList.CurrentCell != null && dgReqList.CurrentRow.Index > -1)
            {
                txtReject.Text = Util.NVC(DataTableConverter.GetValue(dgReqList.CurrentRow.DataItem, "EXTRA_INFORM_RCV"));
            }
        }

        private void chkResult_Unchecked(object sender, RoutedEventArgs e)
        {
            dgReqList.Columns[dgReqList.Columns["SAMPLE_ISSUE_DAY"].Index].Visibility = Visibility.Visible;
            dgReqList.Columns[dgReqList.Columns["REQ_OP"].Index].Visibility = Visibility.Visible;
            dgReqList.Columns[dgReqList.Columns["PQC_REQ_QTY"].Index].Visibility = Visibility.Visible;
            dgReqList.Columns[dgReqList.Columns["DEFECT_CNT"].Index].Visibility = Visibility.Visible;
            dgReqList.Columns[dgReqList.Columns["PQC_REQ_USER"].Index].Visibility = Visibility.Visible;
            dgReqList.Columns[dgReqList.Columns["EXTRA_INFORM_SND"].Index].Visibility = Visibility.Visible;
            dgReqList.Columns[dgReqList.Columns["PQC_CYCL_INSP_FLAG"].Index].Visibility = Visibility.Collapsed;
            dgReqList.Columns[dgReqList.Columns["PQC_RSLT_CODE"].Index].Visibility = Visibility.Collapsed;
            dgReqList.Columns[dgReqList.Columns["LAST_JUDGE_VALUE"].Index].Visibility = Visibility.Collapsed;
            dgReqList.Columns[dgReqList.Columns["LAST_JUDG_DTTM"].Index].Visibility = Visibility.Collapsed;
            dgReqList.Columns[dgReqList.Columns["LAST_JUDG_USERID"].Index].Visibility = Visibility.Collapsed;
            dgReqList.Columns[dgReqList.Columns["EXTRA_INFORM_RCV"].Index].Visibility = Visibility.Collapsed;

            lblDesc.Text = ObjectDic.Instance.GetObjectName("REQ_DESC");  //비고(요청거부사항)

            if (dgReqList.CurrentCell != null && dgReqList.CurrentRow.Index > -1)
            {
                txtReject.Text = Util.NVC(DataTableConverter.GetValue(dgReqList.CurrentRow.DataItem, "EXTRA_INFORM_SND"));
            }
        }

        // 2024.05.30 QMS 요청 중일때 MES에서 CANCEL_YN(취소 요청) I/F 하도록 변경 
        private void btnReqCancel_Click(object sender, RoutedEventArgs e)
        {

            DataGridCellPresenter a = (DataGridCellPresenter)((Button)sender).Parent;
            C1.WPF.DataGrid.C1DataGrid datagrid = a.DataGrid;
            int idx = a.Row.Index;

            try
            {
                //PQC 검사의뢰 취소 요청 하시겠습니까?
                Util.MessageConfirm("FM_ME_0610", result =>
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }

                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "INDATA";
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("PQC_REQ_ID", typeof(string));
                    dtRqst.Columns.Add("USERID", typeof(string));
                    dtRqst.Columns.Add("AREAID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["PQC_REQ_ID"] = Util.NVC(DataTableConverter.GetValue(dgReqList.Rows[idx].DataItem, "PQC_REQ_ID")).ToString();
                    dr["USERID"] = LoginInfo.USERID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                    dtRqst.Rows.Add(dr);

                    new ClientProxy().ExecuteService("BR_SET_REQ_PQC_CANCEL_REQ", "INDATA", null, dtRqst, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }
                            Util.MessageInfo("FM_ME_0611");  //취소 요청했습니다.

                            GetReqList();
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    });

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region [검사기준 Tabpage]
        private void btnSearchSpec_Click(object sender, RoutedEventArgs e)
        {
            GetListSpec();
        }

        private void dgSpec_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    string sD_CNT = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "D_CNT"));
                    string sP_CNT = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "P_CNT"));
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Column.Name.ToString() == "D_CNT" && string.IsNullOrEmpty(sD_CNT))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gray);
                    }

                    if (e.Cell.Column.Name.ToString() == "P_CNT" && string.IsNullOrEmpty(sP_CNT))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gray);
                    }
                }
            }));
        }

        #endregion

        #region [제품검사 해제 Tabpage]
        private void txtClearId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(txtClearId.Text))
                    return;

                ScanClearId(txtClearId.Text);
            }
        }

        private void btnClearCellClear_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgCancel);
        }

        private void btnCellClearSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("FM_ME_0166", (result) =>  //선택한 Cell의 제품검사의뢰를 해제하시겠습니까?
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }
                    else
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "RQSTDT";
                        dtRqst.Columns.Add("TRAY_NO", typeof(string));
                        dtRqst.Columns.Add("CELL_ID", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));
                        dtRqst.Columns.Add("GLOT_FLAG", typeof(string));
                        dtRqst.Columns.Add("MENUID", typeof(string)); // 2025.04.09 이현승 : 등급변경 적용 UI 추적성 향상을 위한 MENUID 추가

                        //기존 Tray로 복구하겠습니까? (버튼을 OK / NO로 생성함)
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0482"), null, "Information", MessageBoxButton.YesNo, MessageBoxIcon.None, (result_restore) =>
                        {
                            if (result_restore == MessageBoxResult.OK) // 기존TRAY로 복구
                            {

                                for (int i = 0; i < dgCancel.Rows.Count; i++)
                                {
                                    DataRow dr = dtRqst.NewRow();

                                    dr["TRAY_NO"] = DataTableConverter.GetValue(dgCancel.Rows[i].DataItem, "LOTID").ToString();
                                    dr["CELL_ID"] = DataTableConverter.GetValue(dgCancel.Rows[i].DataItem, "SUBLOTID").ToString();
                                    dr["USERID"] = LoginInfo.USERID;
                                    dr["GLOT_FLAG"] = "N";
                                    dr["MENUID"] = LoginInfo.CFG_MENUID;
                                    dtRqst.Rows.Add(dr);
                                }

                                new ClientProxy().ExecuteService("BR_SET_REQ_PQC_CANCEL", "INDATA", null, dtRqst, (bizResult, bizException) =>
                                {
                                    try
                                    {
                                        if (bizException != null)
                                        {
                                            Util.MessageException(bizException);
                                            return;
                                        }
                                        Util.MessageInfo("FM_ME_0225");  //제품검사 해제를 완료하였습니다.

                                        Util.gridClear(dgCancel);
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.MessageException(ex);
                                    }
                                });
                            }

                            if (result_restore == MessageBoxResult.No) // 가상 G LOT 발번하여 복구
                            {

                                for (int i = 0; i < dgCancel.Rows.Count; i++)
                                {
                                    DataRow dr = dtRqst.NewRow();

                                    dr["TRAY_NO"] = DataTableConverter.GetValue(dgCancel.Rows[i].DataItem, "LOTID").ToString();
                                    dr["CELL_ID"] = DataTableConverter.GetValue(dgCancel.Rows[i].DataItem, "SUBLOTID").ToString();
                                    dr["USERID"] = LoginInfo.USERID;
                                    dr["GLOT_FLAG"] = "Y";
                                    dr["MENUID"] = LoginInfo.CFG_MENUID;
                                    dtRqst.Rows.Add(dr);
                                }

                                new ClientProxy().ExecuteService("BR_SET_REQ_PQC_CANCEL", "INDATA", null, dtRqst, (bizResult, bizException) =>
                                {
                                    try
                                    {
                                        if (bizException != null)
                                        {
                                            Util.MessageException(bizException);
                                            return;
                                        }
                                        Util.MessageInfo("FM_ME_0225");  //제품검사 해제를 완료하였습니다.

                                        Util.gridClear(dgCancel);
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.MessageException(ex);
                                    }
                                });
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [W등급 제품검사 의뢰 Tabpage]
        private void btnWGrade_Click(object sender, RoutedEventArgs e)
        {
            GetWList();
        }

        private void WButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                wbuttonAccess(sender);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void wbuttonAccess(object sender)
        {
            try
            {
                if (!FrameOperation.AUTHORITY.Equals("W"))
                {
                    return;
                }

                Util.MessageConfirm("FM_ME_0011", (vResult) =>         //[Tray ID : {0}]를 제품검사 의뢰하시겠습니까?
                {
                    if (vResult != MessageBoxResult.OK)
                    {
                        return;
                    }
                    else
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "RQSTDT";
                        dtRqst.Columns.Add("LOTID", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgW.CurrentRow.DataItem, "LOTID"));
                        dtRqst.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("SET_TRAY_SAMPLE_OUT_PQC", "RQSTDT", "RSLTDT", dtRqst);

                        if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                        {
                            Util.MessageValidation("FM_ME_0223");  //제품검사 의뢰를 완료하였습니다.
                        }
                        else
                        {
                            Util.MessageValidation("FM_ME_0224");  //제품검사 의뢰에 실패하였습니다.
                        }
                    }
                }, new object[] { DataTableConverter.GetValue(dgW.CurrentRow.DataItem, "CSTID") });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [E등급 샘플출고 요청 Tabpage]
        private void btnSearchE_Click(object sender, RoutedEventArgs e)
        {
            GetEList();
        }

        private void EButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ebuttonAccess(sender);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void ebuttonAccess(object sender)
        {
            try
            {
                if (!FrameOperation.AUTHORITY.Equals("W"))
                {
                    return;
                }

                Util.MessageConfirm("FM_ME_0008", (vResult) =>         //[Tray ID : {0}]를 Sample 출고 하시겠습니까?
                {
                    if (vResult != MessageBoxResult.OK)
                    {
                        return;
                    }
                    else
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "RQSTDT";
                        dtRqst.Columns.Add("LOTID", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgE.CurrentRow.DataItem, "LOTID"));
                        dtRqst.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("SET_TRAY_SAMPLE_OUT_PQC", "RQSTDT", "RSLTDT", dtRqst);

                        if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                        {
                            Util.MessageValidation("FM_ME_0065");  //Sample 출고 지시를 완료하였습니다.
                        }
                        else
                        {
                            Util.MessageValidation("FM_ME_0066");  //Sample 출고 지시에 실패하였습니다.
                        }
                    }
                }, new object[] { DataTableConverter.GetValue(dgE.CurrentRow.DataItem, "CSTID") });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region [D/E등급 Sorting Lot 현황 Tabpage]
        private void btnSearchDESoStus_Click(object sender, RoutedEventArgs e) //20210331 이벤트 변경
        {
            GetESoStusList();
        }

        private void DESoStusButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DESoStusButtonAccess(sender);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void DESoStusButtonAccess(object sender)
        {
            try
            {
                Util.MessageConfirm("FM_ME_0333", (vResult) =>         //재출고 요청을 완료하였습니다.
                {
                    if (vResult != MessageBoxResult.OK)
                    {
                        return;
                    }
                    else
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "RQSTDT";
                        dtRqst.Columns.Add("DAY_GR_LOTID", typeof(string));
                        dtRqst.Columns.Add("WRK_TYPE", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["DAY_GR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgDESoStus.CurrentRow.DataItem, "DAY_GR_LOTID")); //20210331 컬럼명 변경
                        dr["WRK_TYPE"] = Util.NVC(DataTableConverter.GetValue(dgDESoStus.CurrentRow.DataItem, "WRK_TYPE"));
                        dr["USERID"] = LoginInfo.USERID;
                        dtRqst.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_FORCE_OUT_BY_ASSYLOT", "INDATA", "OUTDATA", dtRqst);

                        Util.MessageValidation("FM_ME_0334");  //재출고 요청을 완료하였습니다.
                        GetESoStusList();
                    }
                }, new object[] { });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDESoStus_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgDESoStus.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (!cell.Column.Name.Equals("DAY_GR_LOTID")) //20210331 컬럼명 변경
                    {
                        return;
                    }

                    FCS001_083_CELL_LIST wndPopup = new FCS001_083_CELL_LIST();
                    wndPopup.FrameOperation = FrameOperation;

                    if (wndPopup != null)
                    {
                        object[] parameters = new object[5];
                        parameters[0] = Util.NVC(DataTableConverter.GetValue(dgDESoStus.Rows[cell.Row.Index].DataItem, "DAY_GR_LOTID")); //20210331 컬럼명 변경
                        parameters[1] = DBNull.Value;
                        parameters[2] = DBNull.Value;
                        parameters[3] = Util.NVC(DataTableConverter.GetValue(dgDESoStus.Rows[cell.Row.Index].DataItem, "WRK_TYPE_DESC")).ToString().Equals("HPCD Sorting") ? "HPCD_GR_LOT_LOW_VLTG_SORT_GRD_CODE" : "GR_LOT_LOW_VLTG_SORT_GRD_CODE";
                        parameters[4] = Util.NVC(DataTableConverter.GetValue(dgDESoStus.Rows[cell.Row.Index].DataItem, "WRK_TYPE_DESC")).ToString().Equals("HPCD Sorting") ? "1" : "0";
                        C1WindowExtension.SetParameters(wndPopup, parameters);
                        wndPopup.Closed += new EventHandler(wndPopup_Closed);

                        // 팝업 화면 숨겨지는 문제 수정.
                        this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDESoStus_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////
                    if (e.Cell.Column.Name.Equals("DAY_GR_LOTID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }

                    if (e.Cell.Column.Name.Equals("RE_SHIP"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }

                }
            }));
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            FCS001_038_CELL_LIST window = sender as FCS001_038_CELL_LIST;
        }
        #endregion

        #region 조회 Tabpage
        private void btnSearchSample_Click(object sender, RoutedEventArgs e)
        {
            GetSampleList();
        }
        private void dgSearch_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.Name.Equals("CSTID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }

                if (e.Cell.Column.Name.Equals("SMPL_STAT"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SMPL_STAT")).Equals(ObjectDic.Instance.GetObjectName("RESTORE")))
                    {
                        C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Cell.Row.Index, 0);
                        cell.Presenter.IsEnabled = false;
                    }
                }

                if (e.Cell.Column.Name.Equals("CHK"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DFCT_SLOC_SMPL_FLAG")).Equals("Y")) //불량창고의 셀일 경우
                    {
                        C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Cell.Row.Index, 0);
                        cell.Presenter.IsEnabled = false;
                    }
                }
            }));
        }

        private void dgSearch_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgSearch_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell == null || datagrid.CurrentRow == null)
                {
                    return;
                }

                if (datagrid.CurrentColumn.Name.Equals("CSTID"))
                {
                    FCS001_021 wndRunStart = new FCS001_021();
                    wndRunStart.FrameOperation = FrameOperation;

                    if (wndRunStart != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = cell.Text;

                        this.FrameOperation.OpenMenu("SFU010710010", true, Parameters);
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #endregion


        private void GetTestData(ref DataTable dt)
        {
            dt.TableName = "RQSTDT";
            //dt.Columns.Add("TRAY_OP_STATUS_CD", typeof(string));

            #region ROW ADD
            DataRow row1 = dt.NewRow();
            row1["REQ_OP"] = "6";
            row1["LOT_DAY"] = "UC01";
            row1["GPQC_ID"] = "";
            row1["PQC_ID"] = "";
            row1["REQ_TYPE"] = "0";
            row1["REQ_CNT"] = "5";
            row1["PERIOD_REQ_DAY"] = "1";
            row1["REQ_TIME"] = "D";
            dt.Rows.Add(row1);
            DataRow row2 = dt.NewRow();
            row2["REQ_OP"] = "6";
            row2["LOT_DAY"] = "UC02";
            row2["GPQC_ID"] = "";
            row2["PQC_ID"] = "";
            row2["REQ_TYPE"] = "10000";
            row2["REQ_CNT"] = "5";
            row2["PERIOD_REQ_DAY"] = "1";
            row2["REQ_TIME"] = "P";
            dt.Rows.Add(row2);
            DataRow row3 = dt.NewRow();
            row3["REQ_OP"] = "6";
            row3["LOT_DAY"] = "UC03";
            row3["GPQC_ID"] = "";
            row3["PQC_ID"] = "";
            row3["REQ_TYPE"] = "1000";
            row3["REQ_CNT"] = "5";
            row3["PERIOD_REQ_DAY"] = "1";
            row3["REQ_TIME"] = "D";
            dt.Rows.Add(row3);
            DataRow row4 = dt.NewRow();
            row4["REQ_OP"] = "6";
            row4["LOT_DAY"] = "UC04";
            row4["GPQC_ID"] = "";
            row4["PQC_ID"] = "";
            row4["REQ_TYPE"] = "0";
            row4["REQ_CNT"] = "5";
            row4["PERIOD_REQ_DAY"] = "1";
            row4["REQ_TIME"] = "D";
            dt.Rows.Add(row4);
            DataRow row5 = dt.NewRow();
            row5["REQ_OP"] = "6";
            row5["LOT_DAY"] = "UC05";
            row5["GPQC_ID"] = "";
            row5["PQC_ID"] = "";
            row5["REQ_TYPE"] = "100";
            row5["REQ_CNT"] = "5";
            row5["PERIOD_REQ_DAY"] = "2";
            row5["REQ_TIME"] = "P";
            dt.Rows.Add(row5);
            DataRow row6 = dt.NewRow();
            row6["REQ_OP"] = "6";
            row6["LOT_DAY"] = "UC06";
            row6["GPQC_ID"] = "";
            row6["PQC_ID"] = "";
            row6["REQ_TYPE"] = "0";
            row6["REQ_CNT"] = "5";
            row6["PERIOD_REQ_DAY"] = "1";
            row6["REQ_TIME"] = "D";
            dt.Rows.Add(row6);
            DataRow row7 = dt.NewRow();
            row7["REQ_OP"] = "6";
            row7["LOT_DAY"] = "UC07";
            row7["GPQC_ID"] = "";
            row7["PQC_ID"] = "";
            row7["REQ_TYPE"] = "10";
            row7["REQ_CNT"] = "5";
            row7["PERIOD_REQ_DAY"] = "2";
            row7["REQ_TIME"] = "P";
            dt.Rows.Add(row7);
            DataRow row8 = dt.NewRow();
            row8["REQ_OP"] = "6";
            row8["LOT_DAY"] = "UC08";
            row8["GPQC_ID"] = "";
            row8["PQC_ID"] = "";
            row8["REQ_TYPE"] = "0";
            row8["REQ_CNT"] = "5";
            row8["PERIOD_REQ_DAY"] = "1";
            row8["REQ_TIME"] = "D";
            dt.Rows.Add(row8);
            DataRow row9 = dt.NewRow();
            row9["REQ_OP"] = "6";
            row9["LOT_DAY"] = "UC09";
            row9["GPQC_ID"] = "";
            row9["PQC_ID"] = "";
            row9["REQ_TYPE"] = "1";
            row9["REQ_CNT"] = "5";
            row9["PERIOD_REQ_DAY"] = "1";
            row9["REQ_TIME"] = "D";
            dt.Rows.Add(row9);
            DataRow row10 = dt.NewRow();
            row10["REQ_OP"] = "6";
            row10["LOT_DAY"] = "UC10";
            row10["GPQC_ID"] = "";
            row10["PQC_ID"] = "";
            row10["REQ_TYPE"] = "0";
            row10["REQ_CNT"] = "5";
            row10["PERIOD_REQ_DAY"] = "1";
            row10["REQ_TIME"] = "D";
            dt.Rows.Add(row10);
            DataRow row11 = dt.NewRow();
            row11["REQ_OP"] = "6";
            row11["LOT_DAY"] = "UC11";
            row11["GPQC_ID"] = "";
            row11["PQC_ID"] = "";
            row11["REQ_TYPE"] = "0";
            row11["REQ_CNT"] = "5";
            row11["PERIOD_REQ_DAY"] = "1";
            row11["REQ_TIME"] = "D";
            dt.Rows.Add(row11);
            DataRow row12 = dt.NewRow();
            row12["REQ_OP"] = "6";
            row12["LOT_DAY"] = "UC12";
            row12["GPQC_ID"] = "";
            row12["PQC_ID"] = "";
            row12["REQ_TYPE"] = "0";
            row12["REQ_CNT"] = "5";
            row12["PERIOD_REQ_DAY"] = "1";
            row12["REQ_TIME"] = "D";
            dt.Rows.Add(row12);
            DataRow row13 = dt.NewRow();
            row13["REQ_OP"] = "6";
            row13["LOT_DAY"] = "UC13";
            row13["GPQC_ID"] = "";
            row13["PQC_ID"] = "";
            row13["REQ_TYPE"] = "0";
            row13["REQ_CNT"] = "5";
            row13["PERIOD_REQ_DAY"] = "1";
            row13["REQ_TIME"] = "D";
            dt.Rows.Add(row13);
            DataRow row14 = dt.NewRow();
            row14["REQ_OP"] = "6";
            row14["LOT_DAY"] = "UC14";
            row14["GPQC_ID"] = "";
            row14["PQC_ID"] = "";
            row14["REQ_TYPE"] = "0";
            row14["REQ_CNT"] = "5";
            row14["PERIOD_REQ_DAY"] = "1";
            row14["REQ_TIME"] = "D";
            dt.Rows.Add(row14);
            DataRow row15 = dt.NewRow();
            row15["REQ_OP"] = "6";
            row15["LOT_DAY"] = "UC15";
            row15["GPQC_ID"] = "";
            row15["PQC_ID"] = "";
            row15["REQ_TYPE"] = "0";
            row15["REQ_CNT"] = "5";
            row15["PERIOD_REQ_DAY"] = "1";
            row15["REQ_TIME"] = "D";
            dt.Rows.Add(row15);
            DataRow row16 = dt.NewRow();
            row16["REQ_OP"] = "6";
            row16["LOT_DAY"] = "UC16";
            row16["GPQC_ID"] = "";
            row16["PQC_ID"] = "";
            row16["REQ_TYPE"] = "0";
            row16["REQ_CNT"] = "5";
            row16["PERIOD_REQ_DAY"] = "1";
            row16["REQ_TIME"] = "D";
            dt.Rows.Add(row16);
            DataRow row17 = dt.NewRow();
            row17["REQ_OP"] = "6";
            row17["LOT_DAY"] = "UC17";
            row17["GPQC_ID"] = "";
            row17["PQC_ID"] = "";
            row17["REQ_TYPE"] = "0";
            row17["REQ_CNT"] = "5";
            row17["PERIOD_REQ_DAY"] = "1";
            row17["REQ_TIME"] = "D";
            dt.Rows.Add(row17);
            DataRow row18 = dt.NewRow();
            row18["REQ_OP"] = "6";
            row18["LOT_DAY"] = "UC18";
            row18["GPQC_ID"] = "";
            row18["PQC_ID"] = "";
            row18["REQ_TYPE"] = "10000";
            row18["REQ_CNT"] = "5";
            row18["PERIOD_REQ_DAY"] = "25";
            row18["REQ_TIME"] = "P";
            dt.Rows.Add(row18);
            DataRow row19 = dt.NewRow();
            row19["REQ_OP"] = "6";
            row19["LOT_DAY"] = "UC19";
            row19["GPQC_ID"] = "";
            row19["PQC_ID"] = "";
            row19["REQ_TYPE"] = "0";
            row19["REQ_CNT"] = "5";
            row19["PERIOD_REQ_DAY"] = "1";
            row19["REQ_TIME"] = "D";
            dt.Rows.Add(row19);
            DataRow row20 = dt.NewRow();
            row20["REQ_OP"] = "6";
            row20["LOT_DAY"] = "UC20";
            row20["GPQC_ID"] = "";
            row20["PQC_ID"] = "";
            row20["REQ_TYPE"] = "0";
            row20["REQ_CNT"] = "5";
            row20["PERIOD_REQ_DAY"] = "1";
            row20["REQ_TIME"] = "D";
            dt.Rows.Add(row20);
            DataRow row21 = dt.NewRow();
            row21["REQ_OP"] = "6";
            row21["LOT_DAY"] = "UC21";
            row21["GPQC_ID"] = "";
            row21["PQC_ID"] = "";
            row21["REQ_TYPE"] = "0";
            row21["REQ_CNT"] = "5";
            row21["PERIOD_REQ_DAY"] = "1";
            row21["REQ_TIME"] = "D";
            dt.Rows.Add(row21);
            DataRow row22 = dt.NewRow();
            row22["REQ_OP"] = "6";
            row22["LOT_DAY"] = "UC22";
            row22["GPQC_ID"] = "";
            row22["PQC_ID"] = "";
            row22["REQ_TYPE"] = "0";
            row22["REQ_CNT"] = "5";
            row22["PERIOD_REQ_DAY"] = "1";
            row22["REQ_TIME"] = "D";
            dt.Rows.Add(row22);
            DataRow row23 = dt.NewRow();
            row23["REQ_OP"] = "6";
            row23["LOT_DAY"] = "UC23";
            row23["GPQC_ID"] = "";
            row23["PQC_ID"] = "";
            row23["REQ_TYPE"] = "0";
            row23["REQ_CNT"] = "5";
            row23["PERIOD_REQ_DAY"] = "1";
            row23["REQ_TIME"] = "D";
            dt.Rows.Add(row23);
            DataRow row24 = dt.NewRow();
            row24["REQ_OP"] = "6";
            row24["LOT_DAY"] = "UC24";
            row24["GPQC_ID"] = "";
            row24["PQC_ID"] = "";
            row24["REQ_TYPE"] = "0";
            row24["REQ_CNT"] = "5";
            row24["PERIOD_REQ_DAY"] = "1";
            row24["REQ_TIME"] = "D";
            dt.Rows.Add(row24);
            DataRow row25 = dt.NewRow();
            row25["REQ_OP"] = "6";
            row25["LOT_DAY"] = "UC25";
            row25["GPQC_ID"] = "";
            row25["PQC_ID"] = "";
            row25["REQ_TYPE"] = "0";
            row25["REQ_CNT"] = "5";
            row25["PERIOD_REQ_DAY"] = "1";
            row25["REQ_TIME"] = "D";
            dt.Rows.Add(row25);
            DataRow row26 = dt.NewRow();
            row26["REQ_OP"] = "6";
            row26["LOT_DAY"] = "UC26";
            row26["GPQC_ID"] = "";
            row26["PQC_ID"] = "";
            row26["REQ_TYPE"] = "0";
            row26["REQ_CNT"] = "5";
            row26["PERIOD_REQ_DAY"] = "1";
            row26["REQ_TIME"] = "D";
            dt.Rows.Add(row26);
            DataRow row27 = dt.NewRow();
            row27["REQ_OP"] = "6";
            row27["LOT_DAY"] = "UC27";
            row27["GPQC_ID"] = "";
            row27["PQC_ID"] = "";
            row27["REQ_TYPE"] = "0";
            row27["REQ_CNT"] = "5";
            row27["PERIOD_REQ_DAY"] = "1";
            row27["REQ_TIME"] = "D";
            dt.Rows.Add(row27);
            DataRow row28 = dt.NewRow();
            row28["REQ_OP"] = "6";
            row28["LOT_DAY"] = "UC28";
            row28["GPQC_ID"] = "";
            row28["PQC_ID"] = "";
            row28["REQ_TYPE"] = "0";
            row28["REQ_CNT"] = "5";
            row28["PERIOD_REQ_DAY"] = "1";
            row28["REQ_TIME"] = "D";
            dt.Rows.Add(row28);
            DataRow row29 = dt.NewRow();
            row29["REQ_OP"] = "6";
            row29["LOT_DAY"] = "UC29";
            row29["GPQC_ID"] = "";
            row29["PQC_ID"] = "";
            row29["REQ_TYPE"] = "0";
            row29["REQ_CNT"] = "5";
            row29["PERIOD_REQ_DAY"] = "1";
            row29["REQ_TIME"] = "D";
            dt.Rows.Add(row29);
            DataRow row30 = dt.NewRow();
            row30["REQ_OP"] = "6";
            row30["LOT_DAY"] = "UC30";
            row30["GPQC_ID"] = "";
            row30["PQC_ID"] = "";
            row30["REQ_TYPE"] = "0";
            row30["REQ_CNT"] = "5";
            row30["PERIOD_REQ_DAY"] = "1";
            row30["REQ_TIME"] = "D";
            dt.Rows.Add(row30);
            DataRow row31 = dt.NewRow();
            row31["REQ_OP"] = "6";
            row31["LOT_DAY"] = "UC31";
            row31["GPQC_ID"] = "";
            row31["PQC_ID"] = "";
            row31["REQ_TYPE"] = "0";
            row31["REQ_CNT"] = "5";
            row31["PERIOD_REQ_DAY"] = "1";
            row31["REQ_TIME"] = "D";
            dt.Rows.Add(row31);


            #endregion

        }

        private void GetTestData_1(ref DataTable dt)
        {
            dt.TableName = "RQSTDT";
            //dt.Columns.Add("TRAY_OP_STATUS_CD", typeof(string));

            #region ROW ADD
            DataRow row1 = dt.NewRow();
            row1["PQC_REQ_GR_ID"] = "0000005085";
            row1["REQ_DAY"] = "03/16/2021 01:58:14";
            row1["TOTL_REQ_QTY"] = "10";
            row1["PQC_REQ_GR_USERID"] = "陈晗铃";
            row1["DEL_FLAG"] = "A";
            dt.Rows.Add(row1);
            DataRow row2 = dt.NewRow();
            row2["PQC_REQ_GR_ID"] = "0000005086";
            row2["REQ_DAY"] = "03/16/2021 05:05:38";
            row2["TOTL_REQ_QTY"] = "1";
            row2["PQC_REQ_GR_USERID"] = "赵慧超";
            row2["DEL_FLAG"] = "A";
            dt.Rows.Add(row2);
            DataRow row3 = dt.NewRow();
            row3["PQC_REQ_GR_ID"] = "0000005087";
            row3["REQ_DAY"] = "03/16/2021 05:15:17";
            row3["TOTL_REQ_QTY"] = "10";
            row3["PQC_REQ_GR_USERID"] = "冯凌霏";
            row3["DEL_FLAG"] = "A";
            dt.Rows.Add(row3);
            DataRow row4 = dt.NewRow();
            row4["PQC_REQ_GR_ID"] = "0000005088";
            row4["REQ_DAY"] = "03/16/2021 05:18:09";
            row4["TOTL_REQ_QTY"] = "20";
            row4["PQC_REQ_GR_USERID"] = "冯凌霏";
            row4["DEL_FLAG"] = "A";
            dt.Rows.Add(row4);
            DataRow row5 = dt.NewRow();
            row5["PQC_REQ_GR_ID"] = "0000005089";
            row5["REQ_DAY"] = "03/16/2021 05:33:05";
            row5["TOTL_REQ_QTY"] = "1";
            row5["PQC_REQ_GR_USERID"] = "赵慧超";
            row5["DEL_FLAG"] = "A";
            dt.Rows.Add(row5);
            DataRow row6 = dt.NewRow();
            row6["PQC_REQ_GR_ID"] = "0000005090";
            row6["REQ_DAY"] = "03/16/2021 09:24:05";
            row6["TOTL_REQ_QTY"] = "10";
            row6["PQC_REQ_GR_USERID"] = "刘安";
            row6["DEL_FLAG"] = "A";
            dt.Rows.Add(row6);
            DataRow row7 = dt.NewRow();
            row7["PQC_REQ_GR_ID"] = "0000005091";
            row7["REQ_DAY"] = "03/16/2021 09:28:39";
            row7["TOTL_REQ_QTY"] = "5";
            row7["PQC_REQ_GR_USERID"] = "陈晗铃";
            row7["DEL_FLAG"] = "A";
            dt.Rows.Add(row7);
            DataRow row8 = dt.NewRow();
            row8["PQC_REQ_GR_ID"] = "0000005092";
            row8["REQ_DAY"] = "03/16/2021 09:37:01";
            row8["TOTL_REQ_QTY"] = "5";
            row8["PQC_REQ_GR_USERID"] = "陈晗铃";
            row8["DEL_FLAG"] = "A";
            dt.Rows.Add(row8);
            DataRow row9 = dt.NewRow();
            row9["PQC_REQ_GR_ID"] = "0000005093";
            row9["REQ_DAY"] = "03/16/2021 10:10:40";
            row9["TOTL_REQ_QTY"] = "5";
            row9["PQC_REQ_GR_USERID"] = "陈晗铃";
            row9["DEL_FLAG"] = "A";
            dt.Rows.Add(row9);
            DataRow row10 = dt.NewRow();
            row10["PQC_REQ_GR_ID"] = "0000005094";
            row10["REQ_DAY"] = "03/16/2021 10:23:01";
            row10["TOTL_REQ_QTY"] = "5";
            row10["PQC_REQ_GR_USERID"] = "陈晗铃";
            row10["DEL_FLAG"] = "A";
            dt.Rows.Add(row10);
            DataRow row11 = dt.NewRow();
            row11["PQC_REQ_GR_ID"] = "0000005095";
            row11["REQ_DAY"] = "03/16/2021 13:48:48";
            row11["TOTL_REQ_QTY"] = "5";
            row11["PQC_REQ_GR_USERID"] = "章磊";
            row11["DEL_FLAG"] = "A";
            dt.Rows.Add(row11);
            DataRow row12 = dt.NewRow();
            row12["PQC_REQ_GR_ID"] = "0000005096";
            row12["REQ_DAY"] = "03/16/2021 14:21:41";
            row12["TOTL_REQ_QTY"] = "5";
            row12["PQC_REQ_GR_USERID"] = "章磊";
            row12["DEL_FLAG"] = "A";
            dt.Rows.Add(row12);
            DataRow row13 = dt.NewRow();
            row13["PQC_REQ_GR_ID"] = "0000005097";
            row13["REQ_DAY"] = "03/16/2021 14:42:24";
            row13["TOTL_REQ_QTY"] = "5";
            row13["PQC_REQ_GR_USERID"] = "章磊";
            row13["DEL_FLAG"] = "A";
            dt.Rows.Add(row13);
            DataRow row14 = dt.NewRow();
            row14["PQC_REQ_GR_ID"] = "0000005098";
            row14["REQ_DAY"] = "03/16/2021 17:07:32";
            row14["TOTL_REQ_QTY"] = "5";
            row14["PQC_REQ_GR_USERID"] = "冯凌霏";
            row14["DEL_FLAG"] = "A";
            dt.Rows.Add(row14);
            DataRow row15 = dt.NewRow();
            row15["PQC_REQ_GR_ID"] = "0000005099";
            row15["REQ_DAY"] = "03/16/2021 17:07:38";
            row15["TOTL_REQ_QTY"] = "5";
            row15["PQC_REQ_GR_USERID"] = "冯凌霏";
            row15["DEL_FLAG"] = "A";
            dt.Rows.Add(row15);
            DataRow row16 = dt.NewRow();
            row16["PQC_REQ_GR_ID"] = "0000005100";
            row16["REQ_DAY"] = "03/16/2021 18:58:17";
            row16["TOTL_REQ_QTY"] = "5";
            row16["PQC_REQ_GR_USERID"] = "冯凌霏";
            row16["DEL_FLAG"] = "A";
            dt.Rows.Add(row16);
            DataRow row17 = dt.NewRow();
            row17["PQC_REQ_GR_ID"] = "0000005101";
            row17["REQ_DAY"] = "03/16/2021 19:15:19";
            row17["TOTL_REQ_QTY"] = "5";
            row17["PQC_REQ_GR_USERID"] = "冯凌霏";
            row17["DEL_FLAG"] = "A";
            dt.Rows.Add(row17);
            DataRow row18 = dt.NewRow();
            row18["PQC_REQ_GR_ID"] = "0000005102";
            row18["REQ_DAY"] = "03/16/2021 20:58:34";
            row18["TOTL_REQ_QTY"] = "5";
            row18["PQC_REQ_GR_USERID"] = "冯凌霏";
            row18["DEL_FLAG"] = "A";
            dt.Rows.Add(row18);
            DataRow row19 = dt.NewRow();
            row19["PQC_REQ_GR_ID"] = "0000005103";
            row19["REQ_DAY"] = "03/16/2021 21:22:33";
            row19["TOTL_REQ_QTY"] = "5";
            row19["PQC_REQ_GR_USERID"] = "王会";
            row19["DEL_FLAG"] = "A";
            dt.Rows.Add(row19);
            DataRow row20 = dt.NewRow();
            row20["PQC_REQ_GR_ID"] = "0000005104";
            row20["REQ_DAY"] = "03/16/2021 23:56:08";
            row20["TOTL_REQ_QTY"] = "5";
            row20["PQC_REQ_GR_USERID"] = "赵慧超";
            row20["DEL_FLAG"] = "A";
            dt.Rows.Add(row20);
            DataRow row21 = dt.NewRow();
            row21["PQC_REQ_GR_ID"] = "0000005105";
            row21["REQ_DAY"] = "03/17/2021 00:18:08";
            row21["TOTL_REQ_QTY"] = "5";
            row21["PQC_REQ_GR_USERID"] = "刘刚";
            row21["DEL_FLAG"] = "A";
            dt.Rows.Add(row21);
            DataRow row22 = dt.NewRow();
            row22["PQC_REQ_GR_ID"] = "0000005106";
            row22["REQ_DAY"] = "03/17/2021 02:49:06";
            row22["TOTL_REQ_QTY"] = "5";
            row22["PQC_REQ_GR_USERID"] = "冯凌霏";
            row22["DEL_FLAG"] = "A";
            dt.Rows.Add(row22);
            DataRow row23 = dt.NewRow();
            row23["PQC_REQ_GR_ID"] = "0000005107";
            row23["REQ_DAY"] = "03/17/2021 05:06:13";
            row23["TOTL_REQ_QTY"] = "5";
            row23["PQC_REQ_GR_USERID"] = "王会";
            row23["DEL_FLAG"] = "A";
            dt.Rows.Add(row23);
            DataRow row24 = dt.NewRow();
            row24["PQC_REQ_GR_ID"] = "0000005108";
            row24["REQ_DAY"] = "03/17/2021 09:04:46";
            row24["TOTL_REQ_QTY"] = "5";
            row24["PQC_REQ_GR_USERID"] = "章磊";
            row24["DEL_FLAG"] = "A";
            dt.Rows.Add(row24);
            DataRow row25 = dt.NewRow();
            row25["PQC_REQ_GR_ID"] = "0000005109";
            row25["REQ_DAY"] = "03/17/2021 09:29:58";
            row25["TOTL_REQ_QTY"] = "5";
            row25["PQC_REQ_GR_USERID"] = "刘刚";
            row25["DEL_FLAG"] = "A";
            dt.Rows.Add(row25);
            DataRow row26 = dt.NewRow();
            row26["PQC_REQ_GR_ID"] = "0000005110";
            row26["REQ_DAY"] = "03/17/2021 13:10:57";
            row26["TOTL_REQ_QTY"] = "5";
            row26["PQC_REQ_GR_USERID"] = "陈晗铃";
            row26["DEL_FLAG"] = "A";
            dt.Rows.Add(row26);
            DataRow row27 = dt.NewRow();
            row27["PQC_REQ_GR_ID"] = "0000005111";
            row27["REQ_DAY"] = "03/17/2021 13:14:15";
            row27["TOTL_REQ_QTY"] = "5";
            row27["PQC_REQ_GR_USERID"] = "陈晗铃";
            row27["DEL_FLAG"] = "A";
            dt.Rows.Add(row27);
            DataRow row28 = dt.NewRow();
            row28["PQC_REQ_GR_ID"] = "0000005112";
            row28["REQ_DAY"] = "03/17/2021 13:16:17";
            row28["TOTL_REQ_QTY"] = "5";
            row28["PQC_REQ_GR_USERID"] = "陈晗铃";
            row28["DEL_FLAG"] = "A";
            dt.Rows.Add(row28);
            DataRow row29 = dt.NewRow();
            row29["PQC_REQ_GR_ID"] = "0000005113";
            row29["REQ_DAY"] = "03/17/2021 13:18:24";
            row29["TOTL_REQ_QTY"] = "5";
            row29["PQC_REQ_GR_USERID"] = "陈晗铃";
            row29["DEL_FLAG"] = "A";
            dt.Rows.Add(row29);
            DataRow row30 = dt.NewRow();
            row30["PQC_REQ_GR_ID"] = "0000005114";
            row30["REQ_DAY"] = "03/17/2021 13:22:38";
            row30["TOTL_REQ_QTY"] = "5";
            row30["PQC_REQ_GR_USERID"] = "陈晗铃";
            row30["DEL_FLAG"] = "A";
            dt.Rows.Add(row30);
            DataRow row31 = dt.NewRow();
            row31["PQC_REQ_GR_ID"] = "0000005115";
            row31["REQ_DAY"] = "03/17/2021 13:23:08";
            row31["TOTL_REQ_QTY"] = "5";
            row31["PQC_REQ_GR_USERID"] = "陈晗铃";
            row31["DEL_FLAG"] = "A";
            dt.Rows.Add(row31);
            DataRow row32 = dt.NewRow();
            row32["PQC_REQ_GR_ID"] = "0000005116";
            row32["REQ_DAY"] = "03/17/2021 13:23:44";
            row32["TOTL_REQ_QTY"] = "5";
            row32["PQC_REQ_GR_USERID"] = "陈晗铃";
            row32["DEL_FLAG"] = "A";
            dt.Rows.Add(row32);
            DataRow row33 = dt.NewRow();
            row33["PQC_REQ_GR_ID"] = "0000005117";
            row33["REQ_DAY"] = "03/17/2021 21:16:39";
            row33["TOTL_REQ_QTY"] = "5";
            row33["PQC_REQ_GR_USERID"] = "冯凌霏";
            row33["DEL_FLAG"] = "A";
            dt.Rows.Add(row33);
            DataRow row34 = dt.NewRow();
            row34["PQC_REQ_GR_ID"] = "0000005118";
            row34["REQ_DAY"] = "03/17/2021 22:38:49";
            row34["TOTL_REQ_QTY"] = "5";
            row34["PQC_REQ_GR_USERID"] = "王会";
            row34["DEL_FLAG"] = "A";
            dt.Rows.Add(row34);
            DataRow row35 = dt.NewRow();
            row35["PQC_REQ_GR_ID"] = "0000005119";
            row35["REQ_DAY"] = "03/17/2021 23:55:05";
            row35["TOTL_REQ_QTY"] = "5";
            row35["PQC_REQ_GR_USERID"] = "刘安";
            row35["DEL_FLAG"] = "A";
            dt.Rows.Add(row35);

            #endregion

        }

        private void GetTestData_2(ref DataTable dt)
        {
            dt.TableName = "RQSTDT";
            //dt.Columns.Add("TRAY_OP_STATUS_CD", typeof(string));

            #region ROW ADD
            DataRow row1 = dt.NewRow();
            row1["PQC_REQ_ID"] = "20210310BORM01_01";
            row1["PQC_REQ_GR_ID"] = "GPQC1000000001";
            row1["SAMPLE_ISSUE_DAY"] = "2021-03-10  19:15:18";
            row1["REQ_OP"] = "NULL";
            row1["PQC_REQ_QTY"] = "5";
            row1["DEFECT_CNT"] = "5";
            row1["DAY_GR_LOTID"] = "FA3TD253";
            row1["EQSGID"] = "AAJ10";
            row1["EQSG_NAME"] = "CNB 汽车 活性化 10";
            row1["PRODID"] = "ACEN1063I-D1";
            row1["MDLLOT_ID"] = "FA3";
            row1["MODEL_NAME"] = "FA3(E63F)";
            row1["PQC_REQ_USER"] = "이제섭";
            row1["EXTRA_INFORM_SND"] = "TEST";
            row1["PQC_CYCL_INSP_FLAG"] = "N";
            row1["PQC_RSLT_CODE"] = "NULL";
            row1["LAST_JUDGE_VALUE"] = "NULL";
            //row1["LAST_JUDG_DTTM"] = "";
            row1["LAST_JUDG_USERID"] = "NULL";
            row1["EXTRA_INFORM_RCV"] = "NULL";
            dt.Rows.Add(row1);
            DataRow row2 = dt.NewRow();
            row2["PQC_REQ_ID"] = "20210315BORM01_01";
            row2["PQC_REQ_GR_ID"] = "GPQC1000000001";
            row2["SAMPLE_ISSUE_DAY"] = "2021-03-10  19:15:18";
            row2["REQ_OP"] = "NULL";
            row2["PQC_REQ_QTY"] = "5";
            row2["DEFECT_CNT"] = "5";
            row2["DAY_GR_LOTID"] = "FA3TD253";
            row2["EQSGID"] = "AAJ10";
            row2["EQSG_NAME"] = "CNB 汽车 活性化 10";
            row2["PRODID"] = "ACEN1063I-D1";
            row2["MDLLOT_ID"] = "FA3";
            row2["MODEL_NAME"] = "FA3(E63F)";
            row2["PQC_REQ_USER"] = "이제섭";
            row2["EXTRA_INFORM_SND"] = "TEST";
            row2["PQC_CYCL_INSP_FLAG"] = "N";
            row2["PQC_RSLT_CODE"] = "R";
            row2["LAST_JUDGE_VALUE"] = "NULL";
            //row2["LAST_JUDG_DTTM"] = "NULL";
            row2["LAST_JUDG_USERID"] = "NULL";
            row2["EXTRA_INFORM_RCV"] = "반려";
            dt.Rows.Add(row2);
            DataRow row3 = dt.NewRow();
            row3["PQC_REQ_ID"] = "20210317BORM01_01";
            row3["PQC_REQ_GR_ID"] = "GPQC1000000002";
            row3["SAMPLE_ISSUE_DAY"] = "2021-03-17  10:25:35";
            row3["REQ_OP"] = "NULL";
            row3["PQC_REQ_QTY"] = "3";
            row3["DEFECT_CNT"] = "1";
            row3["DAY_GR_LOTID"] = "FA3TL141";
            row3["EQSGID"] = "AAJ10";
            row3["EQSG_NAME"] = "CNB 汽车 活性化 10";
            row3["PRODID"] = "ACEN1063I-D1";
            row3["MDLLOT_ID"] = "FA3";
            row3["MODEL_NAME"] = "FA3(E63F)";
            row3["PQC_REQ_USER"] = "이제섭";
            row3["EXTRA_INFORM_SND"] = "안전성";
            row3["PQC_CYCL_INSP_FLAG"] = "N";
            row3["PQC_RSLT_CODE"] = "NULL";
            row3["LAST_JUDGE_VALUE"] = "NULL";
            //row3["LAST_JUDG_DTTM"] = "NULL";
            row3["LAST_JUDG_USERID"] = "NULL";
            row3["EXTRA_INFORM_RCV"] = "NULL";
            dt.Rows.Add(row3);
            DataRow row4 = dt.NewRow();
            row4["PQC_REQ_ID"] = "20210317BORM01_02";
            row4["PQC_REQ_GR_ID"] = "GPQC1000000003";
            row4["SAMPLE_ISSUE_DAY"] = "2021-03-17  10:25:50";
            row4["REQ_OP"] = "NULL";
            row4["PQC_REQ_QTY"] = "3";
            row4["DEFECT_CNT"] = "1";
            row4["DAY_GR_LOTID"] = "FA3TL141";
            row4["EQSGID"] = "AAJ10";
            row4["EQSG_NAME"] = "CNB 汽车 活性化 10";
            row4["PRODID"] = "ACEN1063I-D1";
            row4["MDLLOT_ID"] = "FA3";
            row4["MODEL_NAME"] = "FA3(E63F)";
            row4["PQC_REQ_USER"] = "이제섭";
            row4["EXTRA_INFORM_SND"] = "신뢰성";
            row4["PQC_CYCL_INSP_FLAG"] = "N";
            row4["PQC_RSLT_CODE"] = "NULL";
            row4["LAST_JUDGE_VALUE"] = "NULL";
            //row4["LAST_JUDG_DTTM"] = "NULL";
            row4["LAST_JUDG_USERID"] = "NULL";
            row4["EXTRA_INFORM_RCV"] = "NULL";
            dt.Rows.Add(row4);
            DataRow row5 = dt.NewRow();
            row5["PQC_REQ_ID"] = "20210317BORM01_03";
            row5["PQC_REQ_GR_ID"] = "GPQC1000000004";
            row5["SAMPLE_ISSUE_DAY"] = "2021-03-17  10:26:01";
            row5["REQ_OP"] = "NULL";
            row5["PQC_REQ_QTY"] = "3";
            row5["DEFECT_CNT"] = "3";
            row5["DAY_GR_LOTID"] = "FA3TL141";
            row5["EQSGID"] = "AAJ10";
            row5["EQSG_NAME"] = "CNB 汽车 活性化 10";
            row5["PRODID"] = "ACEN1063I-D1";
            row5["MDLLOT_ID"] = "FA3";
            row5["MODEL_NAME"] = "FA3(E63F)";
            row5["PQC_REQ_USER"] = "이제섭";
            row5["EXTRA_INFORM_SND"] = "장기재고";
            row5["PQC_CYCL_INSP_FLAG"] = "N";
            row5["PQC_RSLT_CODE"] = "NULL";
            row5["LAST_JUDGE_VALUE"] = "NULL";
            //row5["LAST_JUDG_DTTM"] = "NULL";
            row5["LAST_JUDG_USERID"] = "NULL";
            row5["EXTRA_INFORM_RCV"] = "NULL";
            dt.Rows.Add(row5);
            DataRow row6 = dt.NewRow();
            row6["PQC_REQ_ID"] = "20210317BORM01_04";
            row6["PQC_REQ_GR_ID"] = "GPQC1000000005";
            row6["SAMPLE_ISSUE_DAY"] = "2021-03-17  10:26:13";
            row6["REQ_OP"] = "NULL";
            row6["PQC_REQ_QTY"] = "3";
            row6["DEFECT_CNT"] = "3";
            row6["DAY_GR_LOTID"] = "FA3TL141";
            row6["EQSGID"] = "AAJ10";
            row6["EQSG_NAME"] = "CNB 汽车 活性化 10";
            row6["PRODID"] = "ACEN1063I-D1";
            row6["MDLLOT_ID"] = "FA3";
            row6["MODEL_NAME"] = "FA3(E63F)";
            row6["PQC_REQ_USER"] = "이제섭";
            row6["EXTRA_INFORM_SND"] = "장기Cycle";
            row6["PQC_CYCL_INSP_FLAG"] = "N";
            row6["PQC_RSLT_CODE"] = "NULL";
            row6["LAST_JUDGE_VALUE"] = "NULL";
            //row6["LAST_JUDG_DTTM"] = "NULL";
            row6["LAST_JUDG_USERID"] = "NULL";
            row6["EXTRA_INFORM_RCV"] = "NULL";
            dt.Rows.Add(row6);
            DataRow row7 = dt.NewRow();
            row7["PQC_REQ_ID"] = "20210317BORM01_05";
            row7["PQC_REQ_GR_ID"] = "GPQC1000000006";
            row7["SAMPLE_ISSUE_DAY"] = "2021-03-17  10:26:25";
            row7["REQ_OP"] = "NULL";
            row7["PQC_REQ_QTY"] = "3";
            row7["DEFECT_CNT"] = "3";
            row7["DAY_GR_LOTID"] = "FA3TL141";
            row7["EQSGID"] = "AAJ10";
            row7["EQSG_NAME"] = "CNB 汽车 活性化 10";
            row7["PRODID"] = "ACEN1063I-D1";
            row7["MDLLOT_ID"] = "FA3";
            row7["MODEL_NAME"] = "FA3(E63F)";
            row7["PQC_REQ_USER"] = "이제섭";
            row7["EXTRA_INFORM_SND"] = "치수";
            row7["PQC_CYCL_INSP_FLAG"] = "N";
            row7["PQC_RSLT_CODE"] = "NULL";
            row7["LAST_JUDGE_VALUE"] = "NULL";
            //row7["LAST_JUDG_DTTM"] = "NULL";
            row7["LAST_JUDG_USERID"] = "NULL";
            row7["EXTRA_INFORM_RCV"] = "NULL";
            dt.Rows.Add(row7);
            #endregion

        }

        private void dgSearch_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgSearch.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void btnCancelSearch_Click(object sender, RoutedEventArgs e)
        {
            GetCancelList();
        }

        private void dgCancelSearch_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
  
                    if (e.Cell.Column.Name.Equals("PQC_REQ_ID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }

        private void GetCancelList()
        {
            try
            {

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["FROM_DATE"] = dtpDateFromSearch.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpDateToSearch.SelectedDateTime.ToString("yyyyMMdd");
                dr["EQSGID"] = Util.GetCondition(cboLineSelect, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModelSelect, bAllNull: true);

                dtRqst.Rows.Add(dr);

                DataTable dtRsltAll = new ClientProxy().ExecuteServiceSync("DA_SEL_PQC_CANCEL", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgCancelSearch, dtRsltAll, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCancelSearch_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell != null && cell.Column.Name.Equals("PQC_REQ_ID"))
            {
                string sPQCID = cell.Text;

                try
                {

                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("AREAID", typeof(string));
                    dtRqst.Columns.Add("FROM_DATE", typeof(string));
                    dtRqst.Columns.Add("TO_DATE", typeof(string));
                    dtRqst.Columns.Add("PQC_REQ_ID", typeof(string));//의뢰ID

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["FROM_DATE"] = dtpDateFromSearch.SelectedDateTime.ToString("yyyyMMdd");
                    dr["TO_DATE"] = dtpDateToSearch.SelectedDateTime.ToString("yyyyMMdd");
                    dr["PQC_REQ_ID"] = sPQCID;


                    dtRqst.Rows.Add(dr);

                    DataTable dtRsltAll = new ClientProxy().ExecuteServiceSync("DA_SEL_PQC_CANCEL_DETL", "RQSTDT", "RSLTDT", dtRqst);


                    Util.GridSetData(dgReqDetl, dtRsltAll, FrameOperation, true);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }


        }
    }
}


#endregion
