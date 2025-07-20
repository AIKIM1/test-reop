/*************************************************************************************
 Created Date : 2021.03.16
      Creator : 
   Decription : Degas 실적관리
--------------------------------------------------------------------------------------
 [Change History]
  2021.01. DEVELOPER : Initial Created.
  2021.08.18  강동희 : 특이사항 입/출력 기능 추가
  2021.10.21  박수미 : 설비조건점검 진공-진공도, Pre Sealing - 시간(초), CHAMBER 분리(1동과 설비 구조가 다름)
                       (동별 관리 필요함.)
  2022.05.20  이정미 : 설비조건점검 INDATA 컬럼명 변경
  2022.05.26  조영대 : Chamber 4단으로 변경.
  2022.07.02  조영대 : 다시 Chamber 8단으로 변경.
  2022.12.30  이정미 : DEGAS 실적 Tab 조회 OUTDATA 추가로 수정
  2023.02.10  임근영 : 특이사항 저장시 라인정보도 함께 저장.
  2023.03.27  조영대 : 불량명 컬럼헤더 다국어 처리 오류 수정, 필터 적용시 합계 수정
  2023.04.17  하유승 : MultiSelectionBox 추가
  2023.04.24  조영대 : Summary Table 변경
  2023.07.14  이정미 : Parameters 변수 배열 크기 수정 
  2023.10.18  조영대 : 작업조 콤보박스 현재 작업조로 자동 설정
  2023.10.23  조영대 : LOT(8자리, 10자리) 구분 적용
  2023.11.01  조영대 : 작업조에 해당하는 날짜 설정
  2023.11.26  손동혁 : ESNJ 요청 설비별 검색 필터링 추가
  2023.12.01  김용식 : ESNJ 설비 MultiSelectionBox 조회 기능 추가
  2024.02.19  권순범 : [WA3동] 설비조건점검TAB - 진공시간 칼람 추가
  2024.03.08  권순범 : [WA3동] 설비조건점검TAB - 조회/저장 시 폴란드 수치 표기 '.' 에서 ','로 수정
  2024.06.13  이준영 : E20240613-001446 [ESNB PI] 생산 실적 관련 화면 사용 불가 - NB1 , ESS 코드 중복 대응을 위한 AREAID INDATA 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Linq;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_054_SUMMARY : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        private DataTable dtHeader = new DataTable();
        private string[] col = { "IPT_01", "IPT_02", "ST2_01", "ST2_02", "ST2_03", "WFT_01", "WFT_02", "WFT_03", "WFT_04" };
        //CAD_01, SS1_01, NA 제거 
        private string[] col4 = { "NA1", "SP2_01", "SS2_01" };
        private string[] colH = { "HPT_01", "HPT_02", "HPP_01", "HPS_01" };
        private string LOT_KEY = "DIGIT8_LOTID";

        bool bUseFlag = false; //설비별 검색 필터링 추가
        bool bMultiMachineOptionUseYN = false; // 2023.12.01 Multi Selection 설비 기능 추가

        bool bUseFlag2 = false; //설비별 검색 필터링 추가

        public FCS001_054_SUMMARY()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
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
            InitCombo();
            InitControl();

            SetGridHeader(dgDir);    //직행 작업일지 Header Setting
            SetGridHeader(dgRework); //재작업 작업일지 Header Setting

            bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_055_SUMMARY_MACHINE"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.

            // 2023.12.01 동별코드사용 (콤보동일 위치, 동일 조건으로 사용되기에 동시 사용 불가능)
            bMultiMachineOptionUseYN = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_055_SUMMARY_MULTI_EQP_OPT_USE_YN");
            if (bMultiMachineOptionUseYN)
            {
                txtEQPTID.Visibility = Visibility.Visible;
                mcboEQPTID.Visibility = Visibility.Visible;
            }
            else if (bUseFlag)
            {
                txtEQPTID.Visibility = Visibility.Visible;
                cboEQPTID.Visibility = Visibility.Visible;
            }
            else
            {
                txtEQPTID.Visibility = Visibility.Collapsed;
                cboEQPTID.Visibility = Visibility.Collapsed;
            }

            bUseFlag2 = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_055_SUMMARY"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
            if (bUseFlag2)
            {
                Tab1.Visibility = Visibility.Visible;
            }
            else
            {
                Tab1.Visibility = Visibility.Hidden;
            }

            this.Loaded -= UserControl_Loaded;
        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            C1ComboBox[] cboChild = { cboEqp, cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.NONE, sCase: "LINE", cbChild: cboChild);

            object[] objParent = { "D", cboLine };
            _combo.SetComboObjParent(cboEqp, CommonCombo_Form.ComboStatus.NONE, sCase: "EQPDEGASEOL_WC", objParent: objParent);
            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.NONE, sCase: "LINEMODEL", cbParent: cboModelParent);
            _combo.SetCombo(cboShift, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_SHIFT");
            _combo.SetCombo(cboShift2, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_SHIFT");
            _combo.SetCombo(cboEQSGID, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE"); // Degas 실적 탭의 LINE
            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            string[] sFilter = { "D", null, "M" };
            _combo.SetCombo(cboEQPTID, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPID", sFilter: sFilter);
            SetCurrentShift();
        }

        private void InitControl()
        {
            dtpWorkDate.SelectedDateTime = DateTime.Now;
            EqpConditionReset(dgEqp);

            dgDir.UserConfigExceptColumns.Add("DIGIT8_LOTID");
            dgRework.UserConfigExceptColumns.Add("DIGIT8_LOTID");

            dgDir.UserConfigExceptColumns.Add("DIGIT10_LOTID");
            dgRework.UserConfigExceptColumns.Add("DIGIT10_LOTID");

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                dgDir.SetColumnVisible("DIGIT10_LOTID", Visibility.Collapsed);
                dgRework.SetColumnVisible("DIGIT10_LOTID", Visibility.Collapsed);
            }));
        }
        #endregion

        #region Method
                
        private void SetCurrentShift()
        {
            // 현재 작업조의 날짜
            DataTable dtRqst = new DataTable("RQSTDT");
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("WIPDTTM_ED", typeof(DateTime));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["WIPDTTM_ED"] = DateTime.Now;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE_WIPDTTM_ED", "RQSTDT", "RSLTDT", dtRqst);
            if (dtRslt != null && dtRslt.Rows.Count > 0)
            {
                if (!string.IsNullOrEmpty(Util.NVC(dtRslt.Rows[0]["CALDATE"])))
                {
                    dtpWorkDate.SelectedDateTime = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"]);
                    dtpWorkDate2.SelectedDateTime = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"]);
                }

            }

            dtRqst = new DataTable("RQSTDT");
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("SYSTIME", typeof(DateTime));

            dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["SYSTIME"] = DateTime.Now;
            dtRqst.Rows.Add(dr);

            dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORM_SHFT_ID_BY_SYSTIME", "RQSTDT", "RSLTDT", dtRqst);
            if (dtRslt != null && dtRslt.Rows.Count > 0)
            {
                if (!string.IsNullOrEmpty(Util.NVC(dtRslt.Rows[0]["SHFT_ID"])))
                {
                    cboShift.SelectedValue = Util.NVC(dtRslt.Rows[0]["SHFT_ID"]);
                    cboShift2.SelectedValue = Util.NVC(dtRslt.Rows[0]["SHFT_ID"]);
                }

            }
        }

        #region [설비조건점검]
        private void GetEqpConditon(DataTable dt, string colName, int colIdx, int rowInterval, bool flagH)
        {
            int Rowidx = 3;

            for (int i = 1; i <= dgEqp.GetRowCount() / rowInterval; i = i + 1)
            {
                string value = Util.NVC(DataTableConverter.GetValue(dgEqp.Rows[Rowidx].DataItem, colName)).ToString().Replace(',', '.');
                if (string.IsNullOrEmpty(value))
                { Rowidx = Rowidx + rowInterval; continue; }

                DataRow dr = dt.NewRow();
                dr["WRKLOG_TYPE_CODE"] = "D";
                dr["EQPT_ITEM_TYPE_CODE"] = "T";
                dr["EQPT_ITEM_LEVEL1_CODE"] = colName.Split('_')[0];
                dr["EQPT_ITEM_LEVEL2_CODE"] = colName.Split('_')[1];
                if (flagH && i >= 9) dr["EQPT_ITEM_LEVEL3_CODE"] = i - 2;
                else dr["EQPT_ITEM_LEVEL3_CODE"] = i;
                dr["EQPT_ITEM_VALUE1"] = value;
                dr["EQPTID"] = Util.GetCondition(cboEqp, bAllNull: true);
                dt.Rows.Add(dr);
                Rowidx = Rowidx + rowInterval;
            }
        }

        private void GetList()
        {
            try
            {
                //설비조건
                SearchEqpCondition(dgEqp);
                for (int i = 0; i < dgEqp.Columns.Count; i++)
                {
                    if (!dgEqp.Columns[i].Name.Equals("ITEM") && !dgEqp.Columns[i].Name.Contains("NA"))
                    {
                        dgEqp.Columns[i].IsReadOnly = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void SearchEqpCondition(C1DataGrid dg)
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("WORK_DATE", typeof(string));
            dtRqst.Columns.Add("EQSGID", typeof(string));
            dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
            dtRqst.Columns.Add("SHFT_ID", typeof(string));
            dtRqst.Columns.Add("WRKLOG_TYPE_CODE", typeof(string));
            dtRqst.Columns.Add("EQPTID", typeof(string));
            dtRqst.Columns.Add("EQPT_ITEM_TYPE_CODE", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["WORK_DATE"] = dtpWorkDate.SelectedDateTime.ToString("yyyyMMdd");
            dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
            dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
            dr["SHFT_ID"] = Util.GetCondition(cboShift, bAllNull: true);
            dr["WRKLOG_TYPE_CODE"] = "D";
            dr["EQPTID"] = Util.GetCondition(cboEqp, bAllNull: true); //DEGAS LOADER
            dr["EQPT_ITEM_TYPE_CODE"] = "T";
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_WORKSHEET_BY_EQP", "RQSTDT", "RSLTDT", dtRqst);

            dg.ItemsSource = null;
            EqpConditionReset(dgEqp);

            //dgEqp의 Column Name = (EQPT_ITEM_LEVEL1_CODE)_(EQPT_ITEM_LEVEL2_CODE)
            for (int i = 0; i < dtRslt.Rows.Count; i++)
            {
                bool flagCol2 = false;
                bool flagCol4 = false;
                bool flagColH = false;

                string colName = Util.NVC(dtRslt.Rows[i]["EQPT_ITEM_LEVEL1_CODE"]) + "_" + Util.NVC(dtRslt.Rows[i]["EQPT_ITEM_LEVEL2_CODE"]);

                string sValue;
                if (LoginInfo.LANGID == "pl-PL")
                {
                    sValue = Convert.ToString(dtRslt.Rows[i]["EQPT_ITEM_VALUE1"]).ToString().Replace('.', ',');
                }
                else
                {
                    sValue = Convert.ToString(dtRslt.Rows[i]["EQPT_ITEM_VALUE1"]);
                }

                for (int j = 0; j < col.Length; j++)
                {
                    if (colName.Equals(col[j]))
                    {
                        int rowNo = (Util.NVC_Int(dtRslt.Rows[i]["EQPT_ITEM_LEVEL3_CODE"])) * 2 + 1;
                        //DataTableConverter.SetValue(dgEqp.Rows[rowNo].DataItem, colName, dtRslt.Rows[i]["EQPT_ITEM_VALUE1"]);
                        DataTableConverter.SetValue(dgEqp.Rows[rowNo].DataItem, colName, sValue);
                        flagCol2 = true;
                        break;
                    }
                }

                if (!flagCol2)
                {
                    for (int j = 0; j < col4.Length; j++)
                    {
                        if (colName.Equals(col4[j]))
                        {
                            int rowNo = (Util.NVC_Int(dtRslt.Rows[i]["EQPT_ITEM_LEVEL3_CODE"])) * 4 - 1;
                            //DataTableConverter.SetValue(dgEqp.Rows[rowNo].DataItem, colName, dtRslt.Rows[i]["EQPT_ITEM_VALUE1"]);
                            DataTableConverter.SetValue(dgEqp.Rows[rowNo].DataItem, colName, sValue);
                            flagCol4 = true;
                            break;
                        }
                    }
                }
                //HotPress
                if (!flagCol2 && !flagCol4)
                {
                    for (int j = 0; j < colH.Length; j++)
                    {
                        if (colName.Equals(colH[j]))
                        {
                            int rowNo = (Util.NVC_Int(dtRslt.Rows[i]["EQPT_ITEM_LEVEL3_CODE"]));
                            //if (rowNo <= 6) DataTableConverter.SetValue(dgEqp.Rows[rowNo + 2].DataItem, colName, dtRslt.Rows[i]["EQPT_ITEM_VALUE1"]);
                            //else DataTableConverter.SetValue(dgEqp.Rows[rowNo + 4].DataItem, colName, dtRslt.Rows[i]["EQPT_ITEM_VALUE1"]);
                            if (rowNo <= 6) DataTableConverter.SetValue(dgEqp.Rows[rowNo + 2].DataItem, colName, sValue);
                            else DataTableConverter.SetValue(dgEqp.Rows[rowNo + 4].DataItem, colName, sValue);
                            flagColH = true;
                            break;
                        }
                    }
                }

                if (!flagCol2 && !flagCol4 && !flagColH)
                {
                    int rowNo = (Util.NVC_Int(dtRslt.Rows[i]["EQPT_ITEM_LEVEL3_CODE"]) + 2);
                    //DataTableConverter.SetValue(dgEqp.Rows[rowNo].DataItem, colName, dtRslt.Rows[i]["EQPT_ITEM_VALUE1"]);
                    DataTableConverter.SetValue(dgEqp.Rows[rowNo].DataItem, colName, sValue);
                }

                if (dtRslt.Rows.Count > 0)
                {
                    txtWorkUser.Text = Util.NVC(dtRslt.Rows[0]["USERNAME"]);
                }
            }
        }

        private void EqpConditionReset(C1DataGrid dg)
        {
            try
            {
                DataGridRowAdd(dg, 16);

                //항목 - 실측값
                DataTableConverter.SetValue(dg.Rows[3].DataItem, "ITEM", ObjectDic.Instance.GetObjectName("REAL_VALUE"));

                //CHAMBER 
                for (int i = 1; i <= 2; i++)
                {
                    for (int j = 1; j <= 8; j++)
                    {
                        // Chamber 8단
                        DataTableConverter.SetValue(dg.Rows[(i - 1) * 8 + (j + 1) + 1].DataItem, "NA", (i + "-" + j + "#"));
                        //// Chamber 4단
                        //DataTableConverter.SetValue(dg.Rows[(i - 1) * 8 + (j + 1) + 1].DataItem, "NA", (i + "-" + ((j - 1) / 2 + 1) + "#"));
                    }
                }
                //Sealing No
                for (int i = 1; i <= 4; i++)
                {
                    DataTableConverter.SetValue(dg.Rows[i * 4 - 1].DataItem, "NA1", i + "#");
                }
                for (int i = 1; i <= 6; i++)
                {
                    DataTableConverter.SetValue(dg.Rows[i + 2].DataItem, "NA2", i + "#");
                    DataTableConverter.SetValue(dg.Rows[10 + i].DataItem, "NA2", (6 + i) + "#");
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                DataTable dt = new DataTable();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);
                        DataRow dr2 = dt.NewRow();
                        dt.Rows.Add(dr2);
                        //  dg.BeginEdit();
                        //  dg.EndEdit();
                    }
                    Util.GridSetData(dg, dt, this.FrameOperation, true);
                }
                else
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dt.Rows.Add(dr);
                        //  dg.BeginEdit();
                        // dg.EndEdit();
                    }
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [DEGAS 실적]

        private void SetGridHeader(C1DataGrid dg)
        {
            Util.gridClear(dg); //Grid clear
            DataSet dsDirectInfo = new DataSet();
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("USE_FLAG", typeof(string));
            dtRqst.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["USE_FLAG"] = "Y";
            dr["DFCT_GR_TYPE_CODE"] = "D";
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TM_CELL_DEFECT", "RQSTDT", "RSLTDT", dtRqst);

            DataGridAggregate.SetAggregateFunctions(dg.Columns["LOT_ATTR"], new DataGridAggregatesCollection { new CMM001.Controls.UcBaseDataGrid.DataGridAggregateText("합계") { ResultTemplate = grdMain.Resources["ResultTemplateSum"] as DataTemplate } });
            DataGridAggregate.SetAggregateFunctions(dg.Columns["INPUT_SUBLOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultTemplate = grdMain.Resources["ResultTemplate"] as DataTemplate } });
            DataGridAggregate.SetAggregateFunctions(dg.Columns["GOOD_SUBLOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultTemplate = grdMain.Resources["ResultTemplate"] as DataTemplate } });
            DataGridAggregate.SetAggregateFunctions(dg.Columns["NG_SUBLOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultTemplate = grdMain.Resources["ResultTemplate"] as DataTemplate } });

            for (int i = 0; i < dtRslt.Rows.Count; i++)
            {
                string sDefCode = dtRslt.Rows[i]["DFCT_CODE"].ToString();
                string sGrpName = "[*]" + dtRslt.Rows[i]["GROUP_NAME"].ToString();
                string SDefName = dtRslt.Rows[i]["DFCT_NAME"].ToString();
                string sTag = dtRslt.Rows[i]["DFCT_TYPE_CODE"].ToString();

                dg.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                {
                    Name = sDefCode,
                    //[*] 문자 선행일때 다국어 처리 스킵
                    Header = new string[] { sGrpName, "[*]" + SDefName }.ToList<string>(),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath(sDefCode),
                        Mode = BindingMode.TwoWay
                    },
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true,
                    Format = "#,##0",
                    Tag = sTag
                });

                DataGridAggregate.SetAggregateFunctions(dg.Columns[sDefCode], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultTemplate = grdMain.Resources["ResultTemplate"] as DataTemplate } });
            }
            dtHeader = dtRslt.DefaultView.ToTable(false, new string[] { "DFCT_CODE" });
        }

        private void GetListWorksheet()
        {
            // txtWorkUser2.Clear();
            Util.gridClear(dgDir);
            Util.gridClear(dgRework);
            GetSummaryData();
            GetRemarkData(); //2021.08.18 특이사항 입/출력 기능 추가
        }

        private void GetSummaryData()
        {
            try
            {
                DataSet InDataSet = new DataSet();
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("WORK_DATE", typeof(string));
                dtRqst.Columns.Add("SHFT_ID", typeof(string));
                dtRqst.Columns.Add("WRKLOG_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));
                dtRqst.Columns.Add("DIGIT_TYPE", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["WORK_DATE"] = dtpWorkDate2.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["SHFT_ID"] = Util.GetCondition(cboShift2, bAllNull: true);
                dr["WRKLOG_TYPE_CODE"] = "D";
                if (!string.IsNullOrEmpty(Util.GetCondition(cboEQSGID, bAllNull: true)))
                {
                    dr["EQSGID"] = Util.GetCondition(cboEQSGID, bAllNull: true);
                }
                dr["LOTTYPE"] = cboLotType.GetBindValue();
                dr["DIGIT_TYPE"] = chkDigitLot.IsChecked.Equals(true) ? "8" : "10";

                dtRqst.Rows.Add(dr);
                InDataSet.Tables.Add(dtRqst);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService_Multi("BR_SEL_PROD_PERF_MANAGEMENT_SUMMARY", "INDATA", "OUT_DIR,OUT_DIR_DETAIL,OUT_RWK,OUT_RWK_DETAIL,OUT_GOOD_RWK,OUT_GOOD_RWK_DETAIL", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        DataTable dtTempDir = bizResult.Tables["OUT_DIR"];
                        DataTable dtDetailDir = bizResult.Tables["OUT_DIR_DETAIL"];
                        DataTable dtTempRwk = bizResult.Tables["OUT_RWK"];
                        DataTable dtDetailRwk = bizResult.Tables["OUT_RWK_DETAIL"];

                        if (bUseFlag && Util.GetCondition(cboEQPTID, bAllNull: true) != null)
                        {
                            DataRow[] drrowCheck = dtTempDir.Select("EQPTID LIKE '" + Util.GetCondition(cboEQPTID, bAllNull: true) + "%" + "'").ToArray();
                            if (drrowCheck.Length > 0)
                            {
                                dtTempDir = dtTempDir.Select("EQPTID LIKE '" + Util.GetCondition(cboEQPTID, bAllNull: true) + "%" + "'").CopyToDataTable();

                            }
                            else
                            {
                                dtTempDir.Clear();
                            }

                            drrowCheck = dtDetailDir.Select("EQPTID LIKE '" + Util.GetCondition(cboEQPTID, bAllNull: true) + "%" + "'").ToArray();
                            if (drrowCheck.Length > 0)
                            {
                                dtDetailDir = dtDetailDir.Select("EQPTID LIKE '" + Util.GetCondition(cboEQPTID, bAllNull: true) + "%" + "'").CopyToDataTable();
                            }
                            else
                            {
                                dtDetailDir.Clear();
                            }

                            drrowCheck = dtTempRwk.Select("EQPTID LIKE '" + Util.GetCondition(cboEQPTID, bAllNull: true) + "%" + "'").ToArray();
                            if (drrowCheck.Length > 0)
                            {
                                dtTempRwk = dtTempRwk.Select("EQPTID LIKE '" + Util.GetCondition(cboEQPTID, bAllNull: true) + "%" + "'").CopyToDataTable();
                            }
                            else
                            {
                                dtTempRwk.Clear();
                            }

                            drrowCheck = dtDetailRwk.Select("EQPTID LIKE '" + Util.GetCondition(cboEQPTID, bAllNull: true) + "%" + "'").ToArray();
                            if (drrowCheck.Length > 0)
                            {
                                dtDetailRwk = dtDetailRwk.Select("EQPTID LIKE '" + Util.GetCondition(cboEQPTID, bAllNull: true) + "%" + "'").CopyToDataTable();
                            }
                            else
                            {
                                dtDetailRwk.Clear();
                            }
                        }

                        // 2023.12.01 Multi 조건 기능 추가 (SUBSTRING은 -001 이 붙은 이상데이터? 들이 있는 것 같아 추가, 추후 데이터 이상으로 결론이 나오면 SUBSTRING 제거 필요)
                        string sTempEqpList = Util.ConvertEmptyToNull(mcboEQPTID.SelectedItemsToString);
                        if (bMultiMachineOptionUseYN && sTempEqpList.Length > 0)
                        {
                            sTempEqpList = "SUBSTRING(EQPTID,1,11) IN ('" + sTempEqpList.Replace(",", "','") + "')";
                            DataRow[] drrowCheck = dtTempDir.Select(sTempEqpList).ToArray();
                            if (drrowCheck.Length > 0)
                            {
                                dtTempDir = dtTempDir.Select(sTempEqpList).CopyToDataTable();
                            }
                            else
                            {
                                dtTempDir.Clear();
                            }

                            drrowCheck = dtDetailDir.Select(sTempEqpList).ToArray();
                            if (drrowCheck.Length > 0)
                            {
                                dtDetailDir = dtDetailDir.Select(sTempEqpList).CopyToDataTable();
                            }
                            else
                            {
                                dtDetailDir.Clear();
                            }

                            drrowCheck = dtTempRwk.Select(sTempEqpList).ToArray();
                            if (drrowCheck.Length > 0)
                            {
                                dtTempRwk = dtTempRwk.Select(sTempEqpList).CopyToDataTable();
                            }
                            else
                            {
                                dtTempRwk.Clear();
                            }

                            drrowCheck = dtDetailRwk.Select(sTempEqpList).ToArray();
                            if (drrowCheck.Length > 0)
                            {
                                dtDetailRwk = dtDetailRwk.Select(sTempEqpList).CopyToDataTable();
                            }
                            else
                            {
                                dtDetailRwk.Clear();
                            }
                        }

                        for (int i = 0; i < dtHeader.Rows.Count; i++) //불량코드 컬럼 추가
                        {
                            dtTempDir.Columns.Add(Util.NVC(dtHeader.Rows[i]["DFCT_CODE"]), typeof(string));
                            dtTempRwk.Columns.Add(Util.NVC(dtHeader.Rows[i]["DFCT_CODE"]), typeof(string));
                        }
                        SetGridPerf(dgDir, dtDetailDir, dtTempDir); // 직행 실적
                        SetGridPerf(dgRework, dtDetailRwk, dtTempRwk); //재작업 실적

                        #region 설비명 콤보화
                        if (dtTempDir.Rows.Count > 0)
                        {
                            List<string> eqptNameDir = dtTempDir.AsEnumerable().Select(s => s.Field<string>("EQPTNAME")).Distinct().ToList();
                            dgDir.SetDataGridComboBoxColumn("EQPTNAME", eqptNameDir);
                        }
                        if (dtTempRwk.Rows.Count > 0)
                        {
                            List<string> eqptNameRework = dtTempRwk.AsEnumerable().Select(s => s.Field<string>("EQPTNAME")).Distinct().ToList();
                            dgRework.SetDataGridComboBoxColumn("EQPTNAME", eqptNameRework);
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }

                }, InDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetGridPerf(C1DataGrid dg, DataTable dtDetail, DataTable dtTemp)
        {
            try
            {
                dg.ClearFilter();

                // 2023.12.01 Merge 컬럼 변경
                string EQPTID = "EQPTID";
                if (bMultiMachineOptionUseYN)
                {
                    // 2023.12.07 불량코드 데이터는 장비값이 없어 장비명 값이 null이 되어 불량 세부 카운트가 표시되지 않는 이슈로 인해 장비ID로 재수정
                    EQPTID = "EQPTNAME";
                }

                foreach (DataRow drDetail in dtDetail.Rows)
                {
                    int iRow = -1;
                    int iCol = -1;

                    //LOT TYPE 추가 시 수정
                    string sCurrRow = "";
                    sCurrRow = (Util.NVC(drDetail["LOT_COMENT"]) + "_" + Util.NVC(drDetail["EQSGID"]) + "_" + Util.NVC(drDetail["MDLLOT_ID"])
                        + "_" + Util.NVC(drDetail["EQPTID"]) + "_" + Util.NVC(drDetail[LOT_KEY])) + "_" + Util.NVC(drDetail["LOTTYPE"]);
                    for (int i = 0; i < dtTemp.Rows.Count; i++)
                    {
                        //LOT TYPE 추가 시 수정
                        string sRow = "";
                        sRow = (Util.NVC(dtTemp.Rows[i]["LOT_COMENT"]) + "_" + Util.NVC(dtTemp.Rows[i]["EQSGID"]) + "_" + Util.NVC(dtTemp.Rows[i]["MDLLOT_ID"])
                        + "_" + Util.NVC(dtTemp.Rows[i]["EQPTID"]) + "_" + Util.NVC(dtTemp.Rows[i][LOT_KEY]) + "_" + Util.NVC(dtTemp.Rows[i]["LOTTYPE"]));

                        if (sCurrRow.Equals(sRow))
                        {
                            iRow = i;
                            iCol = dtTemp.Columns.IndexOf(Util.NVC(drDetail["DFCT_CODE"]));
                            if (iRow != -1 && iCol != -1)
                            {
                                dtTemp.Rows[iRow][iCol] = Util.NVC(drDetail["BAD_SUBLOT_COUNT"]);
                                dtTemp.Rows[iRow]["LOT_COMENT"] = Util.NVC(drDetail["LOT_COMENT"]);
                                break;
                            }
                        }
                    }
                }

                if (dtTemp.Rows.Count > 0)
                {
                    Util.GridSetData(dg, dtTemp, this.FrameOperation, true);
                }

                string[] sColumnName = new string[] { "MDLLOT_ID", "EQSGID", "EQSGNAME", EQPTID, LOT_KEY };
                _Util.SetDataGridMergeExtensionCol(dg, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

                // 구분 컬럼 콤보 설정
                if (dtTemp.Columns.Contains("LOT_ATTR"))
                {
                    DataTable dtFlag = new DataTable("FLAG");
                    dtFlag.Columns.Add("CODE", typeof(string));
                    dtFlag.Columns.Add("NAME", typeof(string));

                    List<string> flagList = dtTemp.AsEnumerable().Select(s => s.Field<string>("LOT_ATTR")).Distinct().ToList();
                    foreach (string flag in flagList)
                    {
                        DataRow drNew = dtFlag.NewRow();
                        drNew["CODE"] = flag;
                        drNew["NAME"] = flag;
                        dtFlag.Rows.Add(drNew);
                    }
                    dtFlag.AcceptChanges();

                    if (dtFlag.Rows.Count > 0) dg.SetGridColumnCombo("LOT_ATTR", dtFlag, "", false, false);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2021.08.18 특이사항 입/출력 기능 추가 START
        private void GetRemarkData()
        {
            try
            {
                txtRemark.Text = string.Empty;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("WRK_DATE", typeof(string));
                dtRqst.Columns.Add("SHFT_ID", typeof(string));
                dtRqst.Columns.Add("WRKLOG_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));//

                DataRow dr = dtRqst.NewRow();
                dr["WRK_DATE"] = dtpWorkDate2.SelectedDateTime.ToString("yyyyMMdd");//dtpWorkDate2
                dr["SHFT_ID"] = Util.GetCondition(cboShift2, bAllNull: true);
                dr["WRKLOG_TYPE_CODE"] = "D";

                if (string.IsNullOrEmpty(Util.GetCondition(cboEQSGID, bAllNull: true))) //콤보박스 all선택시
                {
                    dr["EQSGID"] = "ALL";
                }
                else
                {
                    dr["EQSGID"] = Util.GetCondition(cboEQSGID, bAllNull: true);
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_RSLT_REMARK", "RQSTDT", "RSLTDT", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    txtRemark.Text = Util.NVC(dtRslt.Rows[0]["REMARKS_CNTT"].ToString());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //2021.08.18 특이사항 입/출력 기능 추가 END

        #endregion

        #region [Common]

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            string userName;

            userName = "";

            parameters[0] = userName;
            C1WindowExtension.SetParameters(wndPerson, parameters);

            wndPerson.Closed += new EventHandler(wndUser_Closed);
            //grdMain.Children.Add(wndPerson); _grid     
            this.Dispatcher.BeginInvoke(new Action(() => wndPerson.ShowModal()));
            wndPerson.BringToFront();
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtWorkUser.Text = wndPerson.USERNAME;
                txtWorkUser.Tag = wndPerson.USERID;
            }
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            FCS001_054_DFCT_CELL_LIST window = sender as FCS001_054_DFCT_CELL_LIST;
        }
        #endregion

        #region LotType MultSelection
        //2023.02.07 LotType MultSelection 
        private void SetLotTypeCombo(MultiSelectionBox mcb)
        {
            try
            {
                DataTable dtRqstA = new DataTable();
                dtRqstA.TableName = "RQSTDT";
                dtRqstA.Columns.Add("LANGID", typeof(string));

                DataRow drA = dtRqstA.NewRow();
                drA["LANGID"] = LoginInfo.LANGID;
                dtRqstA.Rows.Add(drA);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTTYPE_CBO", "RQSTDT", "RSLTDT", dtRqstA);

                if (dtResult.Rows.Count != 0)
                {
                    mcb.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.Check(-1);
                    }
                    else
                    {
                        mcb.isAllUsed = true;
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.CheckAll();
                    }
                }
                else
                {
                    mcb.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
        #region EQPTID MultSelection
        // 2023.12.01 Multi EQP Compo Set
        private void SetEQPTIDCombo(MultiSelectionBox mcb)
        {
            try
            {
                DataTable dtRqstA = new DataTable();
                dtRqstA.TableName = "RQSTDT";
                dtRqstA.Columns.Add("LANGID", typeof(string));
                dtRqstA.Columns.Add("S70", typeof(string));
                dtRqstA.Columns.Add("EQPTLEVEL", typeof(string));

                DataRow drA = dtRqstA.NewRow();
                drA["LANGID"] = LoginInfo.LANGID;
                drA["S70"] = "D";
                drA["EQPTLEVEL"] = "M";
                dtRqstA.Rows.Add(drA);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_EQP", "RQSTDT", "RSLTDT", dtRqstA);

                if (dtResult.Rows.Count != 0)
                {
                    mcb.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.Check(-1);
                    }
                    else
                    {
                        mcb.isAllUsed = true;
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.CheckAll();
                    }
                }
                else
                {
                    mcb.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
        #endregion

        #region Event

        #region [Common]
        private void btnName_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        #endregion

        #region [설비조건점검]

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            FCS001_054_HISTORY history = new FCS001_054_HISTORY();
            history.FrameOperation = FrameOperation;

            object[] parameters = new object[6];
            parameters[0] = dtpWorkDate.SelectedDateTime.ToString("yyyyMMdd");
            parameters[1] = Util.GetCondition(cboLine);
            parameters[2] = Util.GetCondition(cboShift);
            parameters[3] = Util.GetCondition(cboEqp);
            parameters[4] = "D";
            parameters[5] = Util.GetCondition(cboModel);

            C1WindowExtension.SetParameters(history, parameters);
            history.Closed += new EventHandler(history_Closed);

            this.Dispatcher.BeginInvoke(new Action(() => history.ShowModal()));
            return;
        }

        private void history_Closed(object sender, EventArgs e)
        {
            FCS001_054_HISTORY window = sender as FCS001_054_HISTORY;
            this.grdMain.Children.Remove(window);
        }

        private void dgEqp_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid grid = sender as C1DataGrid;
            if (grid != null && grid.ItemsSource != null && grid.Rows.Count > 0)
            {
                var _mergeList = new List<DataGridCellsRange>();
                _mergeList.Add(new DataGridCellsRange(grid.GetCell(3, 0), grid.GetCell(18, 0)));

                for (int i = 0; i < col.Length; i++)
                {
                    int colIndex = grid.Columns[col[i]].Index;
                    for (int j = 0; j < grid.GetRowCount(); j += 2)
                    {
                        _mergeList.Add(new DataGridCellsRange(grid.GetCell(j + 3, colIndex), grid.GetCell(j + 4, colIndex)));
                    }
                }

                for (int i = 0; i < col4.Length; i++)
                {
                    int colIndex = grid.Columns[col4[i]].Index;
                    for (int j = 0; j < grid.GetRowCount(); j += 4)
                    {
                        _mergeList.Add(new DataGridCellsRange(grid.GetCell(j + 3, colIndex), grid.GetCell(j + 6, colIndex)));
                    }
                }

                foreach (var range in _mergeList)
                {
                    e.Merge(range);
                }

                if(LoginInfo.LANGID == "pl-PL")
                {
                    if(grid.Name == "dgEqp")
                    {
                        grid.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);
                    }
                }
            }
        }

        private void dgEqp_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

            if (sender == null)
                return;

            if (e.Cell.Presenter == null)
            {
                return;
            }

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Column.Name.Equals("NA2") || e.Cell.Column.Name.Equals("HPT_01") || e.Cell.Column.Name.Equals("HPT_02")
                || e.Cell.Column.Name.Equals("HPP_01") || e.Cell.Column.Name.Equals("HPS_01"))
                {
                    if (e.Cell.Row.Index == 9 || e.Cell.Row.Index == 10 || e.Cell.Row.Index == 17 || e.Cell.Row.Index == 18)
                    {
                        if (e.Cell.Presenter != null) e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gray);
                    }
                }

            }));
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("FM_ME_0214", (result) =>
            {
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
                else
                {
                    if (string.IsNullOrEmpty(txtWorkUser.Text))
                    {
                        Util.Alert("FM_ME_0200"); //작업자를 입력해주세요.
                        return;
                    }

                    DataSet dsRqst = new DataSet();
                    //공통
                    DataTable dt = new DataTable();
                    dt.TableName = "COMMON";
                    dt.Columns.Add("WRK_DATE", typeof(string));
                    dt.Columns.Add("EQSGID", typeof(string));
                    dt.Columns.Add("MDLLOT_ID", typeof(string));
                    dt.Columns.Add("SHFT_ID", typeof(string));
                    dt.Columns.Add("EQPTID", typeof(string));
                    dt.Columns.Add("USERID", typeof(string));

                    DataRow dr = dt.NewRow();
                    dr["WRK_DATE"] = dtpWorkDate.SelectedDateTime.ToString("yyyyMMdd");
                    dr["EQSGID"] = Util.GetCondition(cboLine);
                    dr["MDLLOT_ID"] = Util.GetCondition(cboModel);
                    dr["SHFT_ID"] = Util.GetCondition(cboShift);
                    dr["EQPTID"] = Util.GetCondition(cboEqp);
                    dr["USERID"] = txtWorkUser.Text;

                    dt.Rows.Add(dr);

                    dsRqst.Tables.Add(dt);

                    //설비 조건 점검 저장
                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "EQPINSPECT";
                    dtRqst.Columns.Add("WRKLOG_TYPE_CODE", typeof(string));
                    dtRqst.Columns.Add("EQPT_ITEM_TYPE_CODE", typeof(string));
                    dtRqst.Columns.Add("EQPT_ITEM_LEVEL1_CODE", typeof(string));
                    dtRqst.Columns.Add("EQPT_ITEM_LEVEL2_CODE", typeof(string));
                    dtRqst.Columns.Add("EQPT_ITEM_LEVEL3_CODE", typeof(string));
                    dtRqst.Columns.Add("EQPT_ITEM_VALUE1", typeof(string));
                    dtRqst.Columns.Add("EQPTID", typeof(string));

                    for (int i = 0; i < dgEqp.Columns.Count; i++)
                    {
                        bool flagCol = false;
                        bool flagCol4 = false;
                        bool flagColH = false;
                        string colName = dgEqp.Columns[i].Name;
                        if (colName.Equals("ITEM") || colName.Contains("NA")) continue;

                        for (int j = 0; j < col.Length; j++)
                        {
                            //컬럼별로 다르게 적용
                            if (colName.Equals(col[j]))
                            {
                                int rowInterval = 2;
                                GetEqpConditon(dtRqst, colName, i, rowInterval, false);
                                flagCol = true;
                                break;
                            }
                        }
                        if (!flagCol)
                        {
                            for (int j = 0; j < col4.Length; j++)
                            {
                                if (colName.Equals(col4[j]))
                                {
                                    int rowinterval = 4;
                                    GetEqpConditon(dtRqst, colName, i, rowinterval, false);
                                    flagCol4 = true;
                                }
                            }
                        }
                        if (!flagCol && !flagCol4)
                        {
                            for (int j = 0; j < colH.Length; j++)
                            {
                                if (colName.Equals(colH[j]))
                                {
                                    GetEqpConditon(dtRqst, colName, i, 1, true);
                                    flagColH = true;
                                }
                            }
                        }
                        if (!flagCol && !flagCol4 && !flagColH)
                        {
                            GetEqpConditon(dtRqst, colName, i, 1, false);
                        }
                    }
                    dsRqst.Tables.Add(dtRqst);
                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_WORKSHEET_D_UPDATE", "COMMON,EQPINSPECT", "OUTDATA", dsRqst);
                    if (Util.NVC_Int(dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"]) == 0) Util.MessageInfo("FM_ME_0215"); //저장하였습니다.
                    else Util.MessageInfo("FM_ME_0213"); //저장실패하였습니다.
                    GetList();
                }
            });
        }

        private void dgEqp_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name.Equals("NA2") || e.Column.Name.Equals("HPT_01") || e.Column.Name.Equals("HPT_02")
                || e.Column.Name.Equals("HPP_01") || e.Column.Name.Equals("HPS_01"))
            {
                if (e.Row.Index == 9 || e.Row.Index == 10 || e.Row.Index == 17 || e.Row.Index == 18)
                {
                    e.Cancel = true;
                }
            }
        }

        private void dgEqp_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            if(LoginInfo.LANGID == "pl-PL")
            {
                //
            }
        }

        #endregion

        #region [Degas 실적]
        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            GetListWorksheet();
        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Cell.Presenter == null)
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

                string tag = Util.NVC(e.Cell.Column.Tag);
                if (!string.IsNullOrEmpty(tag) && !string.IsNullOrEmpty(Util.NVC(e.Cell.Value)))
                {
                    e.Cell.Presenter.Foreground = Brushes.Blue;
                }

                if (!string.IsNullOrEmpty(tag) && e.Cell.Row.Type == DataGridRowType.Bottom)
                {
                    if (dataGrid.FilteredColumns.Length == 0)
                    {
                        e.Cell.Presenter.Foreground = Brushes.Blue;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = Brushes.Black;
                    }
                }
            }));
        }

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item || e.Cell.Row.Type == DataGridRowType.Bottom)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        private void dgList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell == null)
            {
                if (datagrid.CurrentCell == null || datagrid.CurrentRow == null || datagrid.CurrentColumn == null) return;
                if (string.IsNullOrEmpty(Util.NVC(datagrid.CurrentColumn.Tag))) return;
                if (datagrid.CurrentRow.Type != DataGridRowType.Bottom) return;

                if (datagrid.CurrentRow.Type == DataGridRowType.Bottom && datagrid.FilteredColumns.Length > 0)
                {
                    //필터가 적용된 경우 합계 목록을 조회 할 수 없습니다. 필터를 초기화 하시겠습니까?
                    Util.MessageConfirm("FM_ME_0479", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            datagrid.ClearFilter();
                        }
                    });
                    return;
                }

                cell = datagrid.CurrentCell;
            }
            else
            {
                if (cell.Row.Type != DataGridRowType.Item) return;
                if (string.IsNullOrEmpty(Util.NVC(cell.Value)) || Util.NVC(cell.Value).Equals("0")) return;
                if (cell.Column.Index <= datagrid.Columns["NG_SUBLOT_QTY"].Index) return;
            }

            if (string.IsNullOrEmpty(Util.NVC(cell.Text)) || Util.NVC(cell.Text).Equals("0")) return;

            FCS001_054_DFCT_CELL_LIST wndPopup = new FCS001_054_DFCT_CELL_LIST();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                int row = cell.Row.Index;
                object[] Parameters = new object[16];
                Parameters[0] = dtpWorkDate2.SelectedDateTime.ToString("yyyy-MM-dd"); //WORK_DATE
                Parameters[1] = Util.GetCondition(cboShift2, bAllNull: true); //SHFT_ID

                if (datagrid.CurrentRow.Type == DataGridRowType.Bottom)
                {
                    for (int i = 2; i <= 6; i++)
                    {
                        Parameters[i] = "";
                    }
                }
                else
                {
                    Parameters[2] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "MDLLOT_ID")); //MDLLOT_ID
                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "EQSGID")); //EQSGID
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "EQPTID")); //EQPTID                    
                    Parameters[6] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "LOTTYPE")); // LOTTYPE
                }

                Parameters[7] = cell.Column.Name; // DFCT_CODE
                Parameters[8] = "D";
                Parameters[10] = ""; // ROUT_TYPE_CODE


                if (datagrid.Name.Equals("dgDir"))
                {
                    Parameters[9] = "FRST"; // WRK_TYPE
                }
                else
                {
                    Parameters[9] = "RWK"; // WRK_TYPE
                    Parameters[11] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "PRE_DFCT_CODE")); //LOTCOMENT, 이전불량명
                }

                Parameters[14] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "DIGIT8_LOTID")); // 8자리
                Parameters[15] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "DIGIT10_LOTID")); //10자리

                C1WindowExtension.SetParameters(wndPopup, Parameters);
                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }

        }

        //2021.08.18 특이사항 입/출력 기능 추가 START
        private void btnRemarkSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("FM_ME_0214", (result) =>
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }
                    else
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "INDATA";
                        dtRqst.Columns.Add("WRK_DATE", typeof(string));
                        dtRqst.Columns.Add("SHFT_ID", typeof(string));
                        dtRqst.Columns.Add("WRKLOG_TYPE_CODE", typeof(string));
                        dtRqst.Columns.Add("REMARKS_CNTT", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));
                        dtRqst.Columns.Add("EQSGID", typeof(string));//

                        DataRow dr = dtRqst.NewRow();
                        dr["WRK_DATE"] = dtpWorkDate2.SelectedDateTime.ToString("yyyyMMdd");//dtpWorkDate2
                        dr["SHFT_ID"] = Util.GetCondition(cboShift2);
                        dr["WRKLOG_TYPE_CODE"] = "D";
                        dr["REMARKS_CNTT"] = Util.GetCondition(txtRemark);
                        dr["USERID"] = LoginInfo.USERID;
                        if (string.IsNullOrEmpty(Util.GetCondition(cboEQSGID, bAllNull: true))) //all 선택시
                        {
                            dr["EQSGID"] = "ALL";
                        }
                        else
                        {
                            dr["EQSGID"] = Util.GetCondition(cboEQSGID, bAllNull: true);
                        }

                        dtRqst.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_RSLT_REMARKS", "INDATA", "OUTDATA", dtRqst);

                        if (Util.NVC_Int(dtRslt.Rows[0]["RETVAL"]) == 0)
                        {
                            Util.MessageInfo("FM_ME_0215"); //저장하였습니다.
                        }
                        else
                        {
                            Util.MessageInfo("FM_ME_0213"); //저장실패하였습니다.
                        }
                        GetRemarkData();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboLotType_Loaded(object sender, RoutedEventArgs e)
        {
            if (_tabControl.SelectedIndex != 0) return;
            if (cboLotType.ItemsSource == null) SetLotTypeCombo(cboLotType);
        }

        private void cboLotType_SelectionChanged(object sender, EventArgs e)
        {
            if (cboLotType.SelectedItems.Count == 0)
            {
                cboLotType.CheckAll();
            }
        }
        //2021.08.18 특이사항 입/출력 기능 추가 END

        // 2023.12.01 Mutli Combo EQP Add START
        private void mcboEQPTID_Loaded(object sender, RoutedEventArgs e)
        {
            if (bMultiMachineOptionUseYN == false || _tabControl.SelectedIndex != 0) return;
            if (mcboEQPTID.ItemsSource == null) SetEQPTIDCombo(mcboEQPTID);
        }

        private void mcboEQPTID_SelectionChanged(object sender, EventArgs e)
        {
            if (mcboEQPTID.SelectedItems.Count == 0)
            {
                mcboEQPTID.CheckAll();
            }
        }
        // 2023.12.01 Mutli Combo EQP Add END
        #endregion

        #endregion

        private void chkDigitLot_CheckedChanged(object sender, bool isChecked, RoutedEventArgs e)
        {
            if (isChecked)
            {
                dgDir.SetColumnVisible("DIGIT8_LOTID", Visibility.Visible);
                dgRework.SetColumnVisible("DIGIT8_LOTID", Visibility.Visible);

                LOT_KEY = "DIGIT8_LOTID";

                GetListWorksheet();
            }
            else
            {
                dgDir.SetColumnVisible("DIGIT8_LOTID", Visibility.Collapsed);
                dgRework.SetColumnVisible("DIGIT8_LOTID", Visibility.Collapsed);
            }
        }

        private void chkDigitPkgLot_CheckedChanged(object sender, bool isChecked, RoutedEventArgs e)
        {
            if (isChecked)
            {
                dgDir.SetColumnVisible("DIGIT10_LOTID", Visibility.Visible);
                dgRework.SetColumnVisible("DIGIT10_LOTID", Visibility.Visible);

                LOT_KEY = "DIGIT10_LOTID";

                GetListWorksheet();
            }
            else
            {
                dgDir.SetColumnVisible("DIGIT10_LOTID", Visibility.Collapsed);
                dgRework.SetColumnVisible("DIGIT10_LOTID", Visibility.Collapsed);
            }
        }
    }
}
