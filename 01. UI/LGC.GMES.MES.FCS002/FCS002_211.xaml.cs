/*************************************************************************************
 Created Date : 2023.01.10
      Creator : 김태균
   Decription : 생산 실적 레포트(소형)
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.10  DEVELOPER : Initial Created.
  2023.12.07  SORTER 추가, AREA별 BizRule 분기, 오류 수정
  2024.01.18  dgProdResult 및 dgProdResultSummary의 Column의 Visiable 관련 수정
  2024.02.16  전체양품율 추가, GDW 수집 여부 추가
  2024.03.07  합계 구현 방법 변경
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Linq;
using System.Threading;
using C1.WPF.DataGrid.Summaries;
using static LGC.GMES.MES.CMM001.Controls.UcBaseDataGrid;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_211 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;
        private string sCellType = string.Empty;

        // 2023.12.28  미사용
        //private bool b2LowVoltFlag = false;
        //private DataTable _dtHeader;
        //private DataTable _dtHeaderSummary; 

        private DataTable dtTemp; 

        public class ResultElement
        {
            public CheckBox chkBox = null;
            public string Title = string.Empty;
            public Control Control;
            public bool Visibility = true;
            public int SpaceInCharge = 1;
        }
        
        System.ComponentModel.BackgroundWorker bgWorker = null;

        #endregion

        #region [Initialize]
        public FCS002_211()
        {
            InitializeComponent();

            bgWorker = new System.ComponentModel.BackgroundWorker();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.ProgressChanged += BgWorker_ProgressChanged;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Combo Setting            
            InitCombo();
            //Control Setting
            InitControl();

            // Cell Type 가져오기 NFF 구분용
            SetCellType();

            Set_CheckBox();

            InitSpread();
            InitSpreadSummary();

            chkSummary.Checked += chkSummary_Checked;
            chkSummary.Unchecked += chkSummary_UnChecked;

            chkAll.Checked += chkAll_Checked;
            chkAll.Unchecked += chkAll_Unchecked;

            this.Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (bgWorker.IsBusy)
            {
                bgWorker.CancelAsync();
            }
        }

        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent);

            // Lot 유형
            _combo.SetCombo(cboLotType, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LOTTYPE"); // 2021.08.19 Lot 유형 검색조건 추가

            // 2024.02.16 GDW 수집 여부 추가
            string[] sFLAG_YN = { "FLAG_YN" };
            _combo.SetCombo(cboSumFlagGDW, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFLAG_YN);
        }

        private void InitControl()
        {
            // Util 에 해당 함수 추가 필요.
            dtpFromDate.SelectedDateTime = DateTime.Now;
            dtpToDate.SelectedDateTime = DateTime.Now.AddDays(1);

            // 2023.12.28  미사용
            //_dtHeader = new DataTable();
            //_dtHeaderSummary = new DataTable();

            dtTemp = new DataTable();
    }

        // 2025.05.28 CELL TYPE 체크, 현재는 NFF만 체크 
        // 동일 FACTORY 내 여러 CELL TYPE 등록 시 라인조건 추가 필요
        private void SetCellType()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "INDATA";
            dtRqst.Columns.Add("FACILITY_CODE", typeof(string));
            dtRqst.Columns.Add("CELL_TYPE_CODE", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
            dr["CELL_TYPE_CODE"] = "NFF_CYLINDRICAL";
            dtRqst.Rows.Add(dr);
           
            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_TYPE_MB", "RQSTDT", "RSLTDT", dtRqst);
            
            if (dtRslt.Rows.Count > 0)
                sCellType = "NFF_CYLINDRICAL";

        }


        private void InitSpread()
        {
            Util.gridClear(dgProdResult); //Grid clear

            int Header_Row_count = 3;

            //칼럼 헤더 행 추가
            for (int i = 0; i < Header_Row_count; i++)
            {
                DataGridColumnHeaderRow HR = new DataGridColumnHeaderRow();
                dgProdResult.TopRows.Add(HR);
            }

            // 2024.03.07 수정  sSumType 추가 

            //FIX
            FixedMultiHeader("WORK_DATE|WORK_DATE|WORK_DATE", "CALDATE", false, false, 100, oHorizonAlign: HorizontalAlignment.Center, sSumType: null);
            FixedMultiHeader("LINE_ID|LINE_ID|LINE_ID", "EQSGNAME", false, false, 150, oHorizonAlign: HorizontalAlignment.Center, sSumType: null);
            FixedMultiHeader("MODEL_ID|MODEL_ID|MODEL_ID", "MDLLOT_ID", false, false, oHorizonAlign: HorizontalAlignment.Center, sSumType: null);
            FixedMultiHeader("MODEL_NAME|MODEL_NAME|MODEL_NAME", "MODEL_NAME", false, false, oHorizonAlign: HorizontalAlignment.Center, sSumType: "합계");
            FixedMultiHeader("PRE_AGING|PRE_AGING|AGING_INPUT", "PRE_AGING_IN", false, false, oHorizonAlign: HorizontalAlignment.Center, sSumType: null);

            // 2023.12.07 AREA별 PRE_AGING_IN Hide
            // 2025.05.28 AREAID -> CellType 변경.
            switch (sCellType)
            {
                case "NFF_CYLINDRICAL":      // OC2 Mobile Assy Bldg#2
                    dgProdResult.Columns["PRE_AGING_IN"].Visibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }

            var procList = dtTemp.AsEnumerable().OrderBy(o => o.Field<string>("ATTR1")).ToList();
            int idx = 1;
            foreach (var Item in procList)
            {
                DataRow[] drcominfo = dtTemp.AsEnumerable().Where(f => f.Field<string>("ATTR1").Equals(idx.ToString())).ToArray();

                idx++;

                foreach (DataRow dr in drcominfo)
                {
                    switch(Util.NVC(dr["COM_CODE"]))
                    {
                        case "LCISELECTOR":     // LCI SELECTOR
                            FixedMultiHeader("LCI_SELECTOR|PERF|INPUT", "LCI_SELECTOR_IN", false, false);
                            FixedMultiHeader("LCI_SELECTOR|PERF|GOOD_PRD", "LCI_SELECTOR_OUT", false, false);
                            FixedMultiHeader("LCI_SELECTOR|PERF|GOOD_RATE", "LCI_SELECTOR_YEILD", true, false, sSumType: null);

                            // 2024.03.07 추가
                            DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["LCI_SELECTOR_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResult.Columns["LCI_SELECTOR_IN"], dgProdResult.Columns["LCI_SELECTOR_OUT"]) });

                            AddDefectHeader(dgProdResult, "L", "LCI_SELECTOR", "LCI_SELECTOR_", iWidth:75, bVisible: false);
                            FixedMultiHeader("LCI_SELECTOR|DEFECT|DEFECT", "LCI_SELECTOR_LOSS", false, false);
                            break;

                        case "LCISELECTORRE":   // LCI SELECTOR 재작업
                            FixedMultiHeader("LCI_SELECTOR_REWORK|PERF|INPUT", "LCI_SELECTOR_REWORK_IN", false, false);
                            FixedMultiHeader("LCI_SELECTOR_REWORK|PERF|GOOD_PRD", "LCI_SELECTOR_REWORK_OUT", false, false);
                            FixedMultiHeader("LCI_SELECTOR_REWORK|PERF|GOOD_RATE", "LCI_SELECTOR_REWORK_YEILD", true, false, sSumType: null);

                            // 2024.03.07 추가
                            DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["LCI_SELECTOR_REWORK_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResult.Columns["LCI_SELECTOR_REWORK_IN"], dgProdResult.Columns["LCI_SELECTOR_REWORK_OUT"]) });

                            AddDefectHeader(dgProdResult, "L", "LCI_SELECTOR_REWORK", "LCI_SELECTOR_REWORK_", iWidth:75, bVisible: false);
                            FixedMultiHeader("LCI_SELECTOR_REWORK|DEFECT|DEFECT", "LCI_SELECTOR_REWORK_LOSS", false, false);
                            break;

                        case "SELECTOR":        // SELECTOR
                            FixedMultiHeader("SELECTOR|PERF|INPUT", "SELECTOR_IN", false, false);
                            FixedMultiHeader("SELECTOR|PERF|GOOD_PRD", "SELECTOR_OUT", false, false);
                            FixedMultiHeader("SELECTOR|PERF|GOOD_RATE", "SELECTOR_YEILD", true, false, sSumType: null);

                            // 2024.03.07 추가
                            DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["SELECTOR_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResult.Columns["SELECTOR_IN"], dgProdResult.Columns["SELECTOR_OUT"]) });

                            AddDefectHeader(dgProdResult, "S", "SELECTOR", "SELECTOR_", iWidth:75, bVisible: false);
                            FixedMultiHeader("SELECTOR|DEFECT|DEFECT", "SELECTOR_LOSS", false, false);
                            break;

                        case "SELECTORRE":      // SELECTOR 재작업
                            FixedMultiHeader("SELECTOR_REWORK|PERF|INPUT", "SELECTOR_REWORK_IN", false, false);
                            FixedMultiHeader("SELECTOR_REWORK|PERF|GOOD_PRD", "SELECTOR_REWORK_OUT", false, false);
                            FixedMultiHeader("SELECTOR_REWORK|PERF|GOOD_RATE", "SELECTOR_REWORK_YEILD", true, false, sSumType: null);

                            // 2024.03.07 추가
                            DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["SELECTOR_REWORK_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResult.Columns["SELECTOR_REWORK_IN"], dgProdResult.Columns["SELECTOR_REWORK_OUT"]) });

                            AddDefectHeader(dgProdResult, "S", "SELECTOR_REWORK", "SELECTOR_REWORK_", iWidth:75, bVisible: false);
                            FixedMultiHeader("SELECTOR_REWORK|DEFECT|DEFECT", "SELECTOR_REWORK_LOSS", false, false);
                            break;

                        case "EOL":             // EOL
                            FixedMultiHeader("EOL|PERF|TOTAL_GOOD_RATE", "EOL_TOTAL_YEILD", true, false, sSumType: null);  // 2024.02.16  전체양품율 추가
                            FixedMultiHeader("EOL|PERF|INPUT", "EOL_IN", false, false);
                            FixedMultiHeader("EOL|PERF|GOOD_PRD", "EOL_OUT", false, false);
                            FixedMultiHeader("EOL|PERF|GOOD_RATE", "EOL_YEILD", true, false, sSumType: null);

                            // 2024.03.07 추가
                            DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["EOL_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResult.Columns["EOL_IN"], dgProdResult.Columns["EOL_OUT"]) });

                            AddDefectEOLHeader(dgProdResult, "EOL", "EOL_", bVisible: false);
                            // 자동차는 EOL COMMONCODE : FORM_DFCT_GR_TYPE_CODE가 '5', 소형은 'E'로 사용
                            AddDefectNoneEOLHeader(dgProdResult, "E", "EOL", "EOL_", bVisible: false);
                            FixedMultiHeader("EOL|DEFECT|DEFECT", "EOL_LOSS", false, false);
                            break;

                        case "EOLRE":           // EOL 재작업
                            // 2024.02.28 HEADER 변경 EOL_REWORK -> EOL 재작업
                            FixedMultiHeader("EOL 재작업|PERF|INPUT", "EOL_REWORK_IN", false, false);
                            FixedMultiHeader("EOL 재작업|PERF|GOOD_PRD", "EOL_REWORK_OUT", false, false);
                            FixedMultiHeader("EOL 재작업|PERF|GOOD_RATE", "EOL_REWORK_YEILD", true, false, sSumType: null);

                            // 2024.03.07 추가
                            DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["EOL_REWORK_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResult.Columns["EOL_REWORK_IN"], dgProdResult.Columns["EOL_REWORK_OUT"]) });

                            // 2024.03.07 추가
                            if ((dgProdResult.Columns.Contains("EOL_IN") == true) &&
                                (dgProdResult.Columns.Contains("EOL_OUT") == true) &&
                                (dgProdResult.Columns.Contains("EOL_REWORK_OUT") == true))
                            {
                                List<C1.WPF.DataGrid.DataGridColumn> totalColumn = new List<C1.WPF.DataGrid.DataGridColumn>() { dgProdResult.Columns["EOL_IN"] };
                                List<C1.WPF.DataGrid.DataGridColumn> partColumn = new List<C1.WPF.DataGrid.DataGridColumn>() { dgProdResult.Columns["EOL_OUT"], dgProdResult.Columns["EOL_REWORK_OUT"] };
                                DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["EOL_TOTAL_YEILD"], new DataGridAggregatesCollection { new DataGridAggregateRatio(totalColumn, partColumn) });
                            }

                            AddDefectEOLHeader(dgProdResult, "EOL 재작업", "EOL_REWORK_", bVisible: false);
                            // 자동차는 EOL COMMONCODE : FORM_DFCT_GR_TYPE_CODE가 '5', 소형은 'E'로 사용
                            AddDefectNoneEOLHeader(dgProdResult, "E", "EOL 재작업", "EOL_REWORK_", bVisible: false);
                            FixedMultiHeader("EOL 재작업|DEFECT|DEFECT", "EOL_REWORK_LOSS", false, false);
                            break;

                        case "INSP_REQ":        // 검사 의회
                            FixedMultiHeader("INSP_REQ|SAMPLE|GRADE_1", "GRADE_1", false, false);
                            FixedMultiHeader("INSP_REQ|SAMPLE|GRADE_2", "GRADE_2", false, false, bVisible: false);
                            break;

                        // 2023.12.07  추가
                        case "SORTER":          // SORTER
                            FixedMultiHeader("SORTER|PERF|INPUT", "SORTER_IN", false, false);
                            FixedMultiHeader("SORTER|PERF|GOOD_PRD", "SORTER_OUT", false, false);
                            FixedMultiHeader("SORTER|PERF|GOOD_RATE", "SORTER_YEILD", true, false, sSumType: null);

                            // 2024.03.07 추가
                            DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["SORTER_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResult.Columns["SORTER_IN"], dgProdResult.Columns["SORTER_OUT"]) });

                            AddDefectHeader(dgProdResult, "C", "SORTER", "SORTER_", iWidth:75, bVisible: false);
                            FixedMultiHeader("SORTER|DEFECT|DEFECT", "SORTER_LOSS", false, false);
                            break;

                        // 2023.12.07  추가
                        case "SORTERRE":        // SORTER 재작업
                            FixedMultiHeader("SORTER_REWORK|PERF|INPUT", "SORTER_REWORK_IN", false, false);
                            FixedMultiHeader("SORTER_REWORK|PERF|GOOD_PRD", "SORTER_REWORK_OUT", false, false);
                            FixedMultiHeader("SORTER_REWORK|PERF|GOOD_RATE", "SORTER_REWORK_YEILD", true, false, sSumType: null);

                            // 2024.03.07 추가
                            DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["SORTER_REWORK_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResult.Columns["SORTER_REWORK_IN"], dgProdResult.Columns["SORTER_REWORK_OUT"]) });

                            AddDefectHeader(dgProdResult, "C", "SORTER_REWORK", "SORTER_REWORK_", iWidth:75,  bVisible: false);
                            FixedMultiHeader("SORTER_REWORK|DEFECT|DEFECT", "SORTER_REWORK_LOSS", false, false);
                            break;

                        default:
                            break;
                    } 
                }
            }
        }

        private void InitSpreadSummary()
        {
            Util.gridClear(dgProdResultSummary); //Grid clear

            int Header_Row_count = 2;

            // 칼럼 헤더 행 추가
            for (int i = 0; i < Header_Row_count; i++)
            {
                DataGridColumnHeaderRow HR = new DataGridColumnHeaderRow();
                dgProdResultSummary.TopRows.Add(HR);
            }

            // 2024.03.07 수정  sSumType 추가 

            // FIX
            FixedMultiHeader("WORK_DATE|WORK_DATE", "CALDATE", false, true, 100, oHorizonAlign: HorizontalAlignment.Center, sSumType: null);
            FixedMultiHeader("LINE_ID|LINE_ID", "EQSGNAME", false, true, 150, oHorizonAlign: HorizontalAlignment.Center, sSumType: null);
            FixedMultiHeader("MODEL_ID|MODEL_ID", "MDLLOT_ID", false, true, oHorizonAlign: HorizontalAlignment.Center, sSumType: null);
            FixedMultiHeader("MODEL_NAME|MODEL_NAME", "MODEL_NAME", false, true, oHorizonAlign: HorizontalAlignment.Center, sSumType: "합계");

            var procList = dtTemp.AsEnumerable().OrderBy(o => o.Field<string>("ATTR1")).ToList();
            int idx = 1;
            foreach (var Item in procList)
            {
                DataRow[] drcominfo = dtTemp.AsEnumerable().Where(f => f.Field<string>("ATTR1").Equals(idx.ToString())).ToArray();

                idx++;

                foreach (DataRow dr in drcominfo)
                {
                    switch (Util.NVC(dr["COM_CODE"]))
                    {
                        case "LCISELECTOR":     // LCI SELECTOR
                            FixedMultiHeader("LCI_SELECTOR|INPUT", "LCI_SELECTOR_IN", false, true);
                            FixedMultiHeader("LCI_SELECTOR|GOOD_PRD", "LCI_SELECTOR_OUT", false, true);
                            FixedMultiHeader("LCI_SELECTOR|DEFECT", "LCI_SELECTOR_LOSS", false, true);

                            FixedMultiHeader("LCI_SELECTOR|GOOD_RATE", "LCI_SELECTOR_YEILD", true, true, sSumType: null);
                            FixedMultiHeader("LCI_SELECTOR|BAD_RATE", "LCI_SELECTOR_LOSS_YEILD", true, true, sSumType: null);

                            // 2024.03.07 추가
                            DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["LCI_SELECTOR_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["LCI_SELECTOR_IN"], dgProdResultSummary.Columns["LCI_SELECTOR_OUT"]) });

                            DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["LCI_SELECTOR_LOSS_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["LCI_SELECTOR_IN"], dgProdResultSummary.Columns["LCI_SELECTOR_LOSS"]) });
                            break;

                        case "LCISELECTORRE":   // LCI SELECTOR 재작업
                            FixedMultiHeader("LCI_SELECTOR_REWORK|INPUT", "LCI_SELECTOR_REWORK_IN", false, true);
                            FixedMultiHeader("LCI_SELECTOR_REWORK|GOOD_PRD", "LCI_SELECTOR_REWORK_OUT", false, true);
                            FixedMultiHeader("LCI_SELECTOR_REWORK|DEFECT", "LCI_SELECTOR_REWORK_LOSS", false, true);

                            FixedMultiHeader("LCI_SELECTOR_REWORK|GOOD_RATE", "LCI_SELECTOR_REWORK_YEILD", true, true, sSumType: null);
                            FixedMultiHeader("LCI_SELECTOR_REWORK|BAD_RATE", "LCI_SELECTOR_REWORK_LOSS_YEILD", true, true, sSumType: null);

                            // 2024.03.07 추가
                            DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["LCI_SELECTOR_REWORK_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["LCI_SELECTOR_REWORK_IN"], dgProdResultSummary.Columns["LCI_SELECTOR_REWORK_OUT"]) });

                            DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["LCI_SELECTOR_REWORK_LOSS_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["LCI_SELECTOR_REWORK_IN"], dgProdResultSummary.Columns["LCI_SELECTOR_REWORK_LOSS"]) });
                            break;

                        case "SELECTOR":        // SELECTOR
                            FixedMultiHeader("SELECTOR|INPUT", "SELECTOR_IN", false, true);
                            FixedMultiHeader("SELECTOR|GOOD_PRD", "SELECTOR_OUT", false, true);
                            FixedMultiHeader("SELECTOR|DEFECT", "SELECTOR_LOSS", false, true);

                            FixedMultiHeader("SELECTOR|GOOD_RATE", "SELECTOR_YEILD", true, true, sSumType: null);
                            FixedMultiHeader("SELECTOR|BAD_RATE", "SELECTOR_LOSS_YEILD", true, true, sSumType: null);

                            // 2024.03.07 추가
                            DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["SELECTOR_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["SELECTOR_IN"], dgProdResultSummary.Columns["SELECTOR_OUT"]) });

                            DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["SELECTOR_LOSS_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["SELECTOR_IN"], dgProdResultSummary.Columns["SELECTOR_LOSS"]) });
                            break;

                        case "SELECTORRE":      // SELECTOR 재작업
                            FixedMultiHeader("SELECTOR_REWORK|INPUT", "SELECTOR_REWORK_IN", false, true);
                            FixedMultiHeader("SELECTOR_REWORK|GOOD_PRD", "SELECTOR_REWORK_OUT", false, true);
                            FixedMultiHeader("SELECTOR_REWORK|DEFECT", "SELECTOR_REWORK_LOSS", false, true);

                            FixedMultiHeader("SELECTOR_REWORK|GOOD_RATE", "SELECTOR_REWORK_YEILD", true, true, sSumType: null);
                            FixedMultiHeader("SELECTOR_REWORK|BAD_RATE", "SELECTOR_REWORK_LOSS_YEILD", true, true, sSumType: null);

                            // 2024.03.07 추가
                            DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["SELECTOR_REWORK_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["SELECTOR_REWORK_IN"], dgProdResultSummary.Columns["SELECTOR_REWORK_OUT"]) });

                            DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["SELECTOR_REWORK_LOSS_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["SELECTOR_REWORK_IN"], dgProdResultSummary.Columns["SELECTOR_REWORK_LOSS"]) });
                            break;

                        case "EOL":             // EOL
                            FixedMultiHeader("EOL|TOTAL_GOOD_RATE", "EOL_TOTAL_YEILD", true, true, sSumType: null);  // 2024.02.16  전체양품율 추가
                            FixedMultiHeader("EOL|INPUT", "EOL_IN", false, true);
                            FixedMultiHeader("EOL|GOOD_PRD", "EOL_OUT", false, true);
                            FixedMultiHeader("EOL|DEFECT", "EOL_LOSS", false, true);

                            FixedMultiHeader("EOL|GOOD_RATE", "EOL_YEILD", true, true, sSumType: null);
                            FixedMultiHeader("EOL|BAD_RATE", "EOL_LOSS_YEILD", true, true, sSumType: null);

                            // 2024.03.07 추가
                            DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["EOL_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["EOL_IN"], dgProdResultSummary.Columns["EOL_OUT"]) });

                            DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["EOL_LOSS_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["EOL_IN"], dgProdResultSummary.Columns["EOL_LOSS"]) });
                            break;

                        case "EOLRE":           // EOL 재작업
                            // 2024.02.28 HEADER 변경 EOL_REWORK -> EOL 재작업
                            FixedMultiHeader("EOL 재작업|INPUT", "EOL_REWORK_IN", false, true);
                            FixedMultiHeader("EOL 재작업|GOOD_PRD", "EOL_REWORK_OUT", false, true);
                            FixedMultiHeader("EOL 재작업|DEFECT", "EOL_REWORK_LOSS", false, true);

                            FixedMultiHeader("EOL 재작업|GOOD_RATE", "EOL_REWORK_YEILD", true, true, sSumType: null);
                            FixedMultiHeader("EOL 재작업|BAD_RATE", "EOL_REWORK_LOSS_YEILD", true, true, sSumType: null);

                            // 2024.03.07 추가
                            DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["EOL_REWORK_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["EOL_REWORK_IN"], dgProdResultSummary.Columns["EOL_REWORK_OUT"]) });

                            DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["EOL_REWORK_LOSS_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["EOL_REWORK_IN"], dgProdResultSummary.Columns["EOL_REWORK_LOSS"]) });

                            // 2024.03.07 추가
                            if ((dgProdResult.Columns.Contains("EOL_IN") == true) &&
                                (dgProdResult.Columns.Contains("EOL_OUT") == true) &&
                                (dgProdResult.Columns.Contains("EOL_REWORK_OUT") == true))
                            {
                                List<C1.WPF.DataGrid.DataGridColumn> totalColumn = new List<C1.WPF.DataGrid.DataGridColumn>() { dgProdResultSummary.Columns["EOL_IN"] };
                                List<C1.WPF.DataGrid.DataGridColumn> partColumn = new List<C1.WPF.DataGrid.DataGridColumn>() { dgProdResultSummary.Columns["EOL_OUT"], dgProdResultSummary.Columns["EOL_REWORK_OUT"] };
                                DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["EOL_TOTAL_YEILD"], new DataGridAggregatesCollection { new DataGridAggregateRatio(totalColumn, partColumn) });
                            }
                            break;

                        case "INSP_REQ":        // 검사 의회 SAMMAY 제외
                            break;

                        // 2023.12.07  추가
                        case "SORTER":          // SORTER
                            FixedMultiHeader("SORTER|INPUT", "SORTER_IN", false, true);
                            FixedMultiHeader("SORTER|GOOD_PRD", "SORTER_OUT", false, true);
                            FixedMultiHeader("SORTER|DEFECT", "SORTER_LOSS", false, true);

                            FixedMultiHeader("SORTER|GOOD_RATE", "SORTER_YEILD", true, true, sSumType: null);
                            FixedMultiHeader("SORTER|BAD_RATE", "SORTER_LOSS_YEILD", true, true, sSumType: null);

                            // 2024.03.07 추가
                            DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["SORTER_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["SORTER_IN"], dgProdResultSummary.Columns["SORTER_OUT"]) });

                            DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["SORTER_LOSS_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["SORTER_IN"], dgProdResultSummary.Columns["SORTER_LOSS"]) });
                            break;

                        // 2023.12.07  추가
                        case "SORTERRE":        // SORTER 재작업
                            FixedMultiHeader("SORTER_REWORK|INPUT", "SORTER_REWORK_IN", false, true);
                            FixedMultiHeader("SORTER_REWORK|GOOD_PRD", "SORTER_REWORK_OUT", false, true);
                            FixedMultiHeader("SORTER_REWORK|DEFECT", "SORTER_REWORK_LOSS", false, true);

                            FixedMultiHeader("SORTER_REWORK|GOOD_RATE", "SORTER_REWORK_YEILD", true, true, sSumType: null);
                            FixedMultiHeader("SORTER_REWORK|BAD_RATE", "SORTER_REWORK_LOSS_YEILD", true, true, sSumType: null);

                            // 2024.03.07 추가
                            DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["SORTER_REWORK_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["SORTER_REWORK_IN"], dgProdResultSummary.Columns["SORTER_REWORK_OUT"]) });

                            DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["SORTER_REWORK_LOSS_YEILD"],
                                new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["SORTER_REWORK_IN"], dgProdResultSummary.Columns["SORTER_REWORK_LOSS"]) });
                            break;

                        default:
                            break;
                    }
                }
            }
        }
        #endregion

        #region [Method]

        private void FixedMultiHeader(string sName, string sBindName, bool bPercent, bool bSummray
                                        , int iWidth = 75, bool bVisible = true
                                        , HorizontalAlignment oHorizonAlign = HorizontalAlignment.Right
                                        , VerticalAlignment oVerticalAlign = VerticalAlignment.Center
                                        , string sSumType = "SUM"
                                     )
        {
            bool bReadOnly = true;
            bool bEditable = false;

            string[] sColName = sName.Split('|');
            List<string> Multi_Header = new List<string>();
            Multi_Header = sColName.ToList();

            if (bSummray.Equals(true))
            {
                var column_TEXT = CreateTextColumn(null, Multi_Header, sBindName, sBindName, iWidth
                                                , bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible, bPercent: bPercent, bSummray: bSummray
                                                , oHorizonAlign: oHorizonAlign, oVerticalAlign: oVerticalAlign, sSumType: sSumType);
                dgProdResultSummary.Columns.Add(column_TEXT);
            }
            else
            {
                var column_TEXT = CreateTextColumn(null, Multi_Header, sBindName, sBindName, iWidth
                                                , bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible, bPercent: bPercent, bSummray: bSummray
                                                , oHorizonAlign: oHorizonAlign, oVerticalAlign: oVerticalAlign, sSumType: sSumType);
                dgProdResult.Columns.Add(column_TEXT);
            }
        }

        private void AddDefectHeader(C1DataGrid dg, string sEqpKindCd, string sTopHeader, string sPrefix
                                    , int iWidth = 0, bool bVisible = true, string sDefectKind = null, string sDelYN = "N"
                                    , HorizontalAlignment oHorizonAlign = HorizontalAlignment.Right
                                    , VerticalAlignment oVerticalAlign = VerticalAlignment.Center
                                    , string sSumType = "SUM"
                                    )
        {
            DataSet dsDirectInfo = new DataSet();
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
            dtRqst.Columns.Add("DFCT_TYPE_CODE", typeof(string));
            dtRqst.Columns.Add("USE_FLAG", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["DFCT_GR_TYPE_CODE"] = sEqpKindCd;
            dr["DFCT_TYPE_CODE"] = sDefectKind;
            dr["USE_FLAG"] = "Y";
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TM_CELL_DEFECT_MB", "RQSTDT", "RSLTDT", dtRqst);

            foreach (DataRow d in dtRslt.Rows)
            {
                string sCol = sTopHeader + "|" + d["GROUP_NAME"].ToString().Trim() + "|" + "GRADE_" + d["DFCT_CODE"].ToString().Trim();
                string[] sColName = sCol.Split('|');

                // 2024.02.28 Top Header 와 Binding Header를 분리
                //string sBinding = sTopHeader + "_" + d["DFCT_CODE"].ToString().Trim(); // Binding Name 변경
                string sBinding = (String.IsNullOrEmpty(sPrefix) == true ? sTopHeader + "_" : sPrefix) + d["DFCT_CODE"].ToString().Trim(); // Binding Name 변경

                bool bReadOnly = true;
                bool bEditable = false;

                //칼럼 헤더 자동 병합을 위해 Header로 사용할 List
                List<string> Multi_Header = new List<string>();
                Multi_Header = sColName.ToList();

                var column_TEXT = CreateTextColumn(null, Multi_Header, sCol, sBinding, iWidth
                                                , bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible, bPercent: false
                                                , oHorizonAlign: oHorizonAlign, oVerticalAlign: oVerticalAlign, sSumType: sSumType);
                dg.Columns.Add(column_TEXT);

            }
        }

        private void AddDefectNoneEOLHeader(C1DataGrid dg, string sEqpKindCd, string sTopHeader, string sPrefix
                                            , int iWidth = 0, bool bVisible = true, string sDefectKind = null, string sDelYN = "N"
                                            , HorizontalAlignment oHorizonAlign = HorizontalAlignment.Right
                                            , VerticalAlignment oVerticalAlign = VerticalAlignment.Center
                                            , string sSumType = "SUM"
                                            )
        {
            DataSet dsDirectInfo = new DataSet();
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
            dtRqst.Columns.Add("DFCT_TYPE_CODE", typeof(string));
            dtRqst.Columns.Add("USE_FLAG", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["DFCT_GR_TYPE_CODE"] = sEqpKindCd;
            dr["DFCT_TYPE_CODE"] = sDefectKind;
            dr["USE_FLAG"] = "Y";
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TM_CELL_DEFECT_NONE_EOL_MB", "RQSTDT", "RSLTDT", dtRqst);

            foreach (DataRow d in dtRslt.Rows)
            {
                string sCol = sTopHeader + "|" + d["GROUP_NAME"].ToString().Trim() + "|" + "GRADE_" + d["DFCT_CODE"].ToString().Trim();
                string[] sColName = sCol.Split('|');

                // 2024.02.28 Top Header 와 Binding Header를 분리
                //string sBinding = sTopHeader + "_" + d["DFCT_CODE"].ToString().Trim(); // Binding Name 변경
                string sBinding = (String.IsNullOrEmpty(sPrefix) == true ? sTopHeader + "_" : sPrefix) + d["DFCT_CODE"].ToString().Trim(); // Binding Name 변경

                bool bReadOnly = true;
                bool bEditable = false;

                //칼럼 헤더 자동 병합을 위해 Header로 사용할 List
                List<string> Multi_Header = new List<string>();
                Multi_Header = sColName.ToList();

                var column_TEXT = CreateTextColumn(null, Multi_Header, sCol, sBinding, iWidth
                                                , bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible, bPercent: false
                                                , oHorizonAlign: oHorizonAlign, oVerticalAlign: oVerticalAlign, sSumType: sSumType);
                dg.Columns.Add(column_TEXT);

            }
        }

        private void AddDefectEOLHeader(C1DataGrid dg, string sTopHeader, string sPrefix
                                        , int iWidth = 0, bool bVisible = true, string sBindingHeader = ""
                                        , HorizontalAlignment oHorizonAlign = HorizontalAlignment.Right
                                        , VerticalAlignment oVerticalAlign = VerticalAlignment.Center
                                        , string sSumType = "SUM"
                                        )
        {
            DataSet dsDirectInfo = new DataSet();
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;

            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TM_CELL_DEFECT_EOL_MB", "RQSTDT", "RSLTDT", dtRqst);

            foreach (DataRow d in dtRslt.Rows)
            {
                string sCol = sTopHeader + "|" + d["GROUP_NAME"].ToString().Trim() + "|" + "GRADE_" + d["DFCT_CODE"].ToString().Trim();
                string[] sColName = sCol.Split('|');

                // 2024.02.28 Top Header 와 Binding Header를 분리
                //string sBinding = sTopHeader + "_" + d["DFCT_CODE"].ToString().Trim(); // Binding Name 변경
                string sBinding = (String.IsNullOrEmpty(sPrefix) == true ? sTopHeader + "_" : sPrefix) + d["DFCT_CODE"].ToString().Trim(); // Binding Name 변경

                bool bReadOnly = true;
                bool bEditable = false;

                //칼럼 헤더 자동 병합을 위해 Header로 사용할 List
                List<string> Multi_Header = new List<string>();
                Multi_Header = sColName.ToList();

                var column_TEXT = CreateTextColumn(null, Multi_Header, sCol, sBinding, iWidth
                                                , bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible, bPercent: false
                                                , oHorizonAlign: oHorizonAlign, oVerticalAlign: oVerticalAlign, sSumType: sSumType);
                dg.Columns.Add(column_TEXT);

            }
        }

        private C1.WPF.DataGrid.DataGridTextColumn CreateTextColumn(string Single_Header, List<string> Multi_Header, string sName, string sBinding, int iWidth
                                                                    , bool bReadOnly = false
                                                                    , bool bEditable = true
                                                                    , bool bVisible = true
                                                                    , bool bPercent = false
                                                                    , bool bSummray = false
                                                                    , HorizontalAlignment oHorizonAlign = HorizontalAlignment.Right
                                                                    , VerticalAlignment oVerticalAlign = VerticalAlignment.Center
                                                                    , string sSumType = "SUM"
                                                                    )
        {

            C1.WPF.DataGrid.DataGridTextColumn Col = new C1.WPF.DataGrid.DataGridTextColumn();

            Col.Name = sName;
            Col.Binding = new Binding(sBinding);
            Col.IsReadOnly = bReadOnly;
            Col.EditOnSelection = bEditable;
            Col.Visibility = bVisible.Equals(true) ? Visibility.Visible : Visibility.Collapsed;
            Col.HorizontalAlignment = oHorizonAlign;
            Col.VerticalAlignment = oVerticalAlign;

            if (iWidth == 0)
                Col.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
            else
                Col.Width = new C1.WPF.DataGrid.DataGridLength(iWidth, DataGridUnitType.Pixel);

            if (bPercent)
                Col.Format = "P2";

            if (!string.IsNullOrEmpty(Single_Header))
                Col.Header = Single_Header;
            else
                Col.Header = Multi_Header;

            // 2024.03.07 Footer Sum 추가
            switch (sSumType)
            {
                case "합계":
                    DataGridAggregate.SetAggregateFunctions(Col, new DataGridAggregatesCollection { new DataGridAggregateText("합계") { ResultTemplate = grdMain.Resources["ResultTemplateSum"] as DataTemplate } });
                    break;
                case "SUM":
                    DataGridAggregate.SetAggregateFunctions(Col, new DataGridAggregatesCollection { new DataGridAggregateSum { ResultTemplate = grdMain.Resources["ResultTemplate"] as DataTemplate } });
                    break;
                case "EVEN":
                    DataGridAggregate.SetAggregateFunctions(Col, new DataGridAggregatesCollection { new DataGridAggregateEven { ResultTemplate = grdMain.Resources["ResultTemplate"] as DataTemplate } });
                    break;
                default:
                    break;
            }

            // 2023.12.28  미사용
            ////임시 테이블에 헤더값 저장
            ////_dtHeader.Columns.Add(sBinding, typeof(string));
            //if (bSummray.Equals(true))
            //{
            //    _dtHeaderSummary.Columns.Add(sBinding, typeof(string));
            //}
            //else
            //{
            //    _dtHeader.Columns.Add(sBinding, typeof(string));
            //}

            return Col;
        }

        private object GetList(object arg)
        {
            DataSet rtnDataSet = null;

            try
            {
                bgWorker.ReportProgress(0, MessageDic.Instance.GetMessage("10057") + ".....");

                object[] argument = (object[])arg;

                DateTime dFromDate = (DateTime)argument[0];
                DateTime dToDate = (DateTime)argument[1];

                string EQSGID = argument[2] == null ? null : argument[2].ToString();
                string MDLLOT_ID = argument[3] == null ? null : argument[3].ToString();
                string LOTTYPE = argument[4] == null ? null : argument[4].ToString();

                // 2024.02.16 GDW 수집 여부 추가
                string GDW_SUM_FLAG = argument[5] == null ? null : argument[5].ToString();

                TimeSpan tsDateDiff = dToDate - dFromDate;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("MONTHLY_YN", typeof(string));
                dtRqst.Columns.Add("FROMDATE", typeof(string));
                dtRqst.Columns.Add("TODATE", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));          // 2021.08.19 Lot 유형 검색조건 추가
                dtRqst.Columns.Add("AREAID", typeof(string));           // 2022.03.28 제품검사의뢰 수량 산출 추가
                dtRqst.Columns.Add("GDW_SUM_FLAG", typeof(string));     // 2024.02.16 GDW 수집 여부 추가

                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = EQSGID;
                dr["MDLLOT_ID"] = MDLLOT_ID;
                dr["MONTHLY_YN"] = "N";
                dr["FROMDATE"] = dFromDate.ToString("yyyy-MM-dd");
                dr["TODATE"] = dToDate.ToString("yyyy-MM-dd");
                dr["LOTTYPE"] = LOTTYPE;                                // 2021.08.19 Lot 유형 검색조건 추가
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;                   // 2022.03.28 제품검사의뢰 수량 산출 추가
                dr["GDW_SUM_FLAG"] = GDW_SUM_FLAG;                      // 2024.02.16 GDW 수집 여부 추가
                dtRqst.Rows.Add(dr);

                // 2023.12.07 AREA별 BizRule 분기
                String sBizName = "";
                switch (sCellType) //2025.05.28 AREAID -> CellType 변경 , 
                {
                    case "NFF_CYLINDRICAL":      // OC2 Mobile Assy Bldg#2
                        sBizName = "DA_SEL_PROD_PERF_REPORT_MB_NFF";
                        break;
                    default:
                        sBizName = "DA_SEL_PROD_PERF_REPORT_MB";
                        break;
                }

                bgWorker.ReportProgress(0, "[" + dFromDate.ToString("yyyy-MM-dd") + " ~ " + dToDate.ToString("yyyy-MM-dd") + "] - " + ObjectDic.Instance.GetObjectName("WORKING") + ".....");

                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_PROD_PERF_REPORT_MB", "RQSTDT", "RSLTDT", dtRqst);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", dtRqst);

                // 2024.03.07 DataGridSummaryRow 구현으로 삭제
               //SetBottomRow(dtRslt);

                //Summary
                DataTable dtSummary = dtRslt.Copy();
                dtSummary.TableName = "SUMMARY";

                //컬럼추가

                // LCI SELECTOR
                if (dtSummary.Columns.Contains("LCI_SELECTOR_IN") == true)
                    dtSummary.Columns.Add("LCI_SELECTOR_LOSS_YEILD", typeof(decimal));
                if (dtSummary.Columns.Contains("LCI_SELECTOR_REWORK_IN") == true)
                    dtSummary.Columns.Add("LCI_SELECTOR_REWORK_LOSS_YEILD", typeof(decimal));

                // SELECTOR
                if (dtSummary.Columns.Contains("SELECTOR_IN") == true)
                    dtSummary.Columns.Add("SELECTOR_LOSS_YEILD", typeof(decimal));
                if (dtSummary.Columns.Contains("SELECTOR_REWORK_IN") == true)
                    dtSummary.Columns.Add("SELECTOR_REWORK_LOSS_YEILD", typeof(decimal));

                // EOL
                if (dtSummary.Columns.Contains("EOL_IN") == true)
                    dtSummary.Columns.Add("EOL_LOSS_YEILD", typeof(decimal));
                if (dtSummary.Columns.Contains("EOL_REWORK_IN") == true)
                    dtSummary.Columns.Add("EOL_REWORK_LOSS_YEILD", typeof(decimal));

                // 2023.12.07 SORTER 추가
                // SORTOER
                if (dtSummary.Columns.Contains("SORTER_IN") == true)
                    dtSummary.Columns.Add("SORTER_LOSS_YEILD", typeof(decimal));
                if (dtSummary.Columns.Contains("SORTER_REWORK_IN") == true)
                    dtSummary.Columns.Add("SORTER_REWORK_LOSS_YEILD", typeof(decimal));

                for (int i = 0; i < dtSummary.Rows.Count; i++)
                {
                    if (dtSummary.Columns.Contains("LCI_SELECTOR_IN") == true)
                    {
                        decimal LCI_SELECTOR_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["LCI_SELECTOR_IN"].ToString()) ? "0" : dtSummary.Rows[i]["LCI_SELECTOR_IN"].ToString());
                        decimal LCI_SELECTOR_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["LCI_SELECTOR_OUT"].ToString()) ? "0" : dtSummary.Rows[i]["LCI_SELECTOR_OUT"].ToString());
                        decimal LCI_SELECTOR_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["LCI_SELECTOR_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["LCI_SELECTOR_LOSS"].ToString());
                        dtSummary.Rows[i]["LCI_SELECTOR_LOSS_YEILD"] = (LCI_SELECTOR_IN == 0 ? 0 : (LCI_SELECTOR_LOSS == 0 ? 0 : LCI_SELECTOR_LOSS / LCI_SELECTOR_IN));
                    }

                    if (dtSummary.Columns.Contains("LCI_SELECTOR_REWORK_IN") == true)
                    {
                        decimal LCI_SELECTOR_REWORK_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["LCI_SELECTOR_REWORK_IN"].ToString()) ? "0" : dtSummary.Rows[i]["LCI_SELECTOR_REWORK_IN"].ToString());
                        decimal LCI_SELECTOR_REWORK_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["LCI_SELECTOR_REWORK_OUT"].ToString()) ? "0" : dtSummary.Rows[i]["LCI_SELECTOR_REWORK_OUT"].ToString());
                        decimal LCI_SELECTOR_REWORK_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["LCI_SELECTOR_REWORK_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["LCI_SELECTOR_REWORK_LOSS"].ToString());
                        dtSummary.Rows[i]["LCI_SELECTOR_REWORK_LOSS_YEILD"] = (LCI_SELECTOR_REWORK_IN == 0 ? 0 : (LCI_SELECTOR_REWORK_LOSS == 0 ? 0 : LCI_SELECTOR_REWORK_LOSS / LCI_SELECTOR_REWORK_IN));
                    }

                    if (dtSummary.Columns.Contains("SELECTOR_IN") == true)
                    {
                        decimal SELECTOR_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["SELECTOR_IN"].ToString()) ? "0" : dtSummary.Rows[i]["SELECTOR_IN"].ToString());
                        decimal SELECTOR_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["SELECTOR_OUT"].ToString()) ? "0" : dtSummary.Rows[i]["SELECTOR_OUT"].ToString());
                        decimal SELECTOR_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["SELECTOR_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["SELECTOR_LOSS"].ToString());
                        dtSummary.Rows[i]["SELECTOR_LOSS_YEILD"] = (SELECTOR_IN == 0 ? 0 : (SELECTOR_LOSS == 0 ? 0 : SELECTOR_LOSS / SELECTOR_IN));
                    }

                    if (dtSummary.Columns.Contains("SELECTOR_REWORK_IN") == true)
                    {
                        decimal SELECTOR_REWORK_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["SELECTOR_REWORK_IN"].ToString()) ? "0" : dtSummary.Rows[i]["SELECTOR_REWORK_IN"].ToString());
                        decimal SELECTOR_REWORK_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["SELECTOR_REWORK_OUT"].ToString()) ? "0" : dtSummary.Rows[i]["SELECTOR_REWORK_OUT"].ToString());
                        decimal SELECTOR_REWORK_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["SELECTOR_REWORK_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["SELECTOR_REWORK_LOSS"].ToString());
                        dtSummary.Rows[i]["SELECTOR_REWORK_LOSS_YEILD"] = (SELECTOR_REWORK_IN == 0 ? 0 : (SELECTOR_REWORK_LOSS == 0 ? 0 : SELECTOR_REWORK_LOSS / SELECTOR_REWORK_IN));
                    }

                    if (dtSummary.Columns.Contains("EOL_IN") == true)
                    {
                        decimal EOL_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_IN"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_IN"].ToString());
                        decimal EOL_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_OUT"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_OUT"].ToString());
                        decimal EOL_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_LOSS"].ToString());
                        dtSummary.Rows[i]["EOL_LOSS_YEILD"] = (EOL_IN == 0 ? 0 : (EOL_LOSS == 0 ? 0 : EOL_LOSS / EOL_IN));
                    }

                    if (dtSummary.Columns.Contains("EOL_REWORK_IN") == true)
                    {
                        decimal EOL_REWORK_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_REWORK_IN"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_REWORK_IN"].ToString());
                        decimal EOL_REWORK_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_REWORK_OUT"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_REWORK_OUT"].ToString());
                        decimal EOL_REWORK_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_REWORK_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_REWORK_LOSS"].ToString());
                        dtSummary.Rows[i]["EOL_REWORK_LOSS_YEILD"] = (EOL_REWORK_IN == 0 ? 0 : (EOL_REWORK_LOSS == 0 ? 0 : EOL_REWORK_LOSS / EOL_REWORK_IN));
                    }

                    // 2023.12.07 SORTER 추가
                    if (dtSummary.Columns.Contains("SORTER_IN") == true)
                    {
                        decimal SORTER_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["SORTER_IN"].ToString()) ? "0" : dtSummary.Rows[i]["SORTER_IN"].ToString());
                        decimal SORTER_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["SORTER_OUT"].ToString()) ? "0" : dtSummary.Rows[i]["SORTER_OUT"].ToString());
                        decimal SORTER_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["SORTER_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["SORTER_LOSS"].ToString());
                        dtSummary.Rows[i]["SORTER_LOSS_YEILD"] = (SORTER_IN == 0 ? 0 : (SORTER_LOSS == 0 ? 0 : SORTER_LOSS / SORTER_IN));
                    }

                    // 2023.12.07 SORTER 추가
                    if (dtSummary.Columns.Contains("SORTER_REWORK_IN") == true)
                    {
                        decimal SORTER_REWORK_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["SORTER_REWORK_IN"].ToString()) ? "0" : dtSummary.Rows[i]["SORTER_REWORK_IN"].ToString());
                        decimal SORTER_REWORK_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["SORTER_REWORK_OUT"].ToString()) ? "0" : dtSummary.Rows[i]["SORTER_REWORK_OUT"].ToString());
                        decimal SORTER_REWORK_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["SORTER_REWORK_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["SORTER_REWORK_LOSS"].ToString());
                        dtSummary.Rows[i]["SORTER_REWORK_LOSS_YEILD"] = (SORTER_REWORK_IN == 0 ? 0 : (SORTER_REWORK_LOSS == 0 ? 0 : SORTER_REWORK_LOSS / SORTER_REWORK_IN));
                    }
                }

                rtnDataSet = new DataSet();
                DataTable dtResult = dtRslt.Copy();

                rtnDataSet.Tables.Add(dtResult);
                rtnDataSet.Tables.Add(dtSummary);

                bgWorker.ReportProgress(100, "[" + dFromDate.ToString("yyyy-MM-dd") + " ~ " + dToDate.ToString("yyyy-MM-dd") + "] - " + ObjectDic.Instance.GetObjectName("WORKING") + ".....");
                Thread.Sleep(1);
            }
            catch (Exception ex)
            {
                return ex;
            }

            return rtnDataSet;
        }
        
        /// <summary>
        /// 하단 Summary Row
        /// </summary>
        private void SetBottomRow(DataTable dt)
        {
            DataRow dr = dt.NewRow();

            Boolean bSkipColum;
            string[] sColNameList;
            string sColName = String.Empty;
            List<String> ExceptColName = new List<string>
            {
                  "CALDATE", "MDLLOT_ID", "MODEL_NAME", "EQSGNAME"
                , "_YEILD"
            };

            // 모델명 - "합계"
            dr["MODEL_NAME"] = ObjectDic.Instance.GetObjectName("합계");

            int colIdx = dt.Columns.IndexOf("PRE_AGING_IN");
            for (int i = colIdx; i < dt.Columns.Count; i++)
            {
                sColNameList = dt.Columns[i].ColumnName.Split('|');
                sColName = sColNameList[sColNameList.Length - 1];

                bSkipColum = false;
                foreach (String sExcept in ExceptColName)
                {
                    if (sColName.Contains(sExcept) == false)
                        continue;

                    if (sColName.Substring(sColName.IndexOf(sExcept)) != sExcept)
                        continue;

                    bSkipColum = true;
                }

                if (bSkipColum == true)
                    continue;

                int sum = 0;
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    sum += Util.NVC_Int(dt.Rows[j][i]);
                }
                dr[i] = sum;
            }

            if (dt.Columns.Contains("LCI_SELECTOR_IN") == true)
            {
                // LCI_SELECTOR 직행/재작업 Total
                decimal LCI_SELECTOR_IN = Convert.ToDecimal(dr["LCI_SELECTOR_IN"].ToString());
                decimal LCI_SELECTOR_OUT = Convert.ToDecimal(dr["LCI_SELECTOR_OUT"].ToString());
                decimal LCI_SELECTOR_REWORK_IN = Convert.ToDecimal(dr["LCI_SELECTOR_REWORK_IN"].ToString());
                decimal LCI_SELECTOR_REWORK_OUT = Convert.ToDecimal(dr["LCI_SELECTOR_REWORK_OUT"].ToString());

                if (LCI_SELECTOR_IN == 0) dr["LCI_SELECTOR_YEILD"] = 0;
                else dr["LCI_SELECTOR_YEILD"] = LCI_SELECTOR_OUT / LCI_SELECTOR_IN;
                if (LCI_SELECTOR_REWORK_IN == 0) dr["LCI_SELECTOR_REWORK_YEILD"] = 0;
                else dr["LCI_SELECTOR_REWORK_YEILD"] = LCI_SELECTOR_REWORK_OUT / LCI_SELECTOR_REWORK_IN;
            }

            if (dt.Columns.Contains("SELECTOR_IN") == true)
            {
                // SELECTOR 직행/재작업 Total
                decimal SELECTOR_IN = Convert.ToDecimal(dr["SELECTOR_IN"].ToString());
                decimal SELECTOR_OUT = Convert.ToDecimal(dr["SELECTOR_OUT"].ToString());
                decimal SELECTOR_REWORK_IN = Convert.ToDecimal(dr["SELECTOR_REWORK_IN"].ToString());
                decimal SELECTOR_REWORK_OUT = Convert.ToDecimal(dr["SELECTOR_REWORK_OUT"].ToString());

                if (SELECTOR_IN == 0) dr["SELECTOR_YEILD"] = 0;
                else dr["SELECTOR_YEILD"] = SELECTOR_OUT / SELECTOR_IN;
                if (SELECTOR_REWORK_IN == 0) dr["SELECTOR_REWORK_YEILD"] = 0;
                else dr["SELECTOR_REWORK_YEILD"] = SELECTOR_REWORK_OUT / SELECTOR_REWORK_IN;
            }

            if (dt.Columns.Contains("EOL_IN") == true)
            {
                // EOL 직행/재작업 Total
                decimal EOL_IN = Convert.ToDecimal(dr["EOL_IN"].ToString());
                decimal EOL_OUT = Convert.ToDecimal(dr["EOL_OUT"].ToString());
                decimal EOL_REWORK_IN = Convert.ToDecimal(dr["EOL_REWORK_IN"].ToString());
                decimal EOL_REWORK_OUT = Convert.ToDecimal(dr["EOL_REWORK_OUT"].ToString());

                if (EOL_IN == 0)
                {
                    dr["EOL_YEILD"] = 0;
                    // 2024.02.16  전체양품율 추가
                    dr["EOL_TOTAL_YEILD"] = 0;                    
                }
                else
                {
                    dr["EOL_YEILD"] = EOL_OUT / EOL_IN;
                    // 2024.02.16  전체양품율 추가
                    // 전체양품율 : ([직행] 양품 + [재작업] 양품) / [직행] 투입
                    dr["EOL_TOTAL_YEILD"] = (EOL_OUT + EOL_REWORK_OUT) / EOL_IN;
                }

                if (EOL_REWORK_IN == 0) dr["EOL_REWORK_YEILD"] = 0;
                else dr["EOL_REWORK_YEILD"] = EOL_REWORK_OUT / EOL_REWORK_IN;
            }

            // 2023.12.07 SORTER 추가
            if (dt.Columns.Contains("SORTER_IN") == true)
            {
                // SORTER 직행/재작업 Total
                decimal SORTER_IN = Convert.ToDecimal(dr["SORTER_IN"].ToString());
                decimal SORTER_OUT = Convert.ToDecimal(dr["SORTER_OUT"].ToString());
                decimal SORTER_REWORK_IN = Convert.ToDecimal(dr["SORTER_REWORK_IN"].ToString());
                decimal SORTER_REWORK_OUT = Convert.ToDecimal(dr["SORTER_REWORK_OUT"].ToString());

                if (SORTER_IN == 0) dr["SORTER_YEILD"] = 0;
                else dr["SORTER_YEILD"] = SORTER_OUT / SORTER_IN;
                if (SORTER_REWORK_IN == 0) dr["SORTER_REWORK_YEILD"] = 0;
                else dr["SORTER_REWORK_YEILD"] = SORTER_REWORK_OUT / SORTER_REWORK_IN;
            }

            dt.Rows.Add(dr);
        }

        private static List<ResultElement> CheckBoxList(DataTable dt)
        {
            List<ResultElement> lst = new List<ResultElement>();
            var procList = dt.AsEnumerable().OrderBy(o => o.Field<string>("ATTR1")).ToList();
            int idx = 1;
            foreach (var Item in procList)
            {
                DataRow[] drcominfo = dt.AsEnumerable().Where(f => f.Field<string>("ATTR1").Equals(idx.ToString())).ToArray();

                idx++;

                foreach (DataRow dr in drcominfo)
                {
                    lst.Add(new ResultElement
                    {
                        Title = Util.NVC(dr["COM_CODE_NAME"]),
                        Control = new CheckBox()
                        {
                            Name = "chk" + Util.NVC(dr["COM_CODE"]),
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Center,

                            // 2024.02.28 HEADER 변경 EOL_REWORK -> EOL 재작업
                            Content = Util.NVC(dr["COM_CODE_NAME"]) == "EOL_REWORK" ? 
                                ObjectDic.Instance.GetObjectName("EOL 재작업") :
                                ObjectDic.Instance.GetObjectName(Util.NVC(dr["COM_CODE_NAME"]))
                        }
                    });
                }
            }
            return lst;
        }

        private void Set_CheckBox()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("COM_CODE", typeof(string));
                dtRQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = LoginInfo.CFG_AREA_ID;
                drnewrow["COM_TYPE_CODE"] = "PROD_RSLT_RPT_PROC_ORDER";
                drnewrow["COM_CODE"] = DBNull.Value;
                drnewrow["USE_FLAG"] = "Y";
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", dtRQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    dtTemp = dtResult.Copy();
                    List<ResultElement> elemList;
                    elemList = CheckBoxList(dtResult);
                    if (elemList.Count > 0)
                    {
                        SetResult(elemList, Area);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
 
        private void SetResult(List<ResultElement> elementList, Grid grid)
        {
            int elementCol = 0;
            grid.SetValue(Grid.VerticalAlignmentProperty, VerticalAlignment.Bottom);
            int colIndex = 0;

            foreach (ResultElement re in elementList)
            {
                if (re.Control != null)
                {
                    re.chkBox = re.Control as CheckBox;
                    re.chkBox.Style = Application.Current.Resources["SearchCondition_CheckBoxStyle"] as Style;
                    re.chkBox.Margin = new Thickness(10, 0, 5, 0);
                    re.chkBox.IsChecked = true;
                    elementCol++;
                    re.chkBox.SetValue(Grid.ColumnProperty, elementCol);
                    re.chkBox.Checked += chk_Checked;
                    re.chkBox.Unchecked += chk_UnChecked;
                    Area.Children.Add(re.chkBox);
                }
                colIndex += re.SpaceInCharge;
            }
            //Search_Status();
        }

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

        /// <summary>
        /// CheckBox의 선택에따라 dgProdResult 및 dgProdResultSummary의 Column의 Visiable를 설정함
        /// 2024.01.18 추가
        /// </summary>
        private void SetVisibleProdResult(CheckBox chk, Visibility oVisible)
        {
            String sStaColProd = String.Empty;
            String sEndColProd = String.Empty;
            String sStaColSumm = String.Empty;
            String sEndColSumm = String.Empty;

            switch (chk.Name)
            {
                case "chkLCISELECTOR":
                    sStaColProd = "LCI_SELECTOR_IN";
                    sEndColProd = "LCI_SELECTOR_LOSS";
                    sStaColSumm = "LCI_SELECTOR_IN";
                    sEndColSumm = "LCI_SELECTOR_LOSS_YEILD";
                    break;
                case "chkLCISELECTORRE":
                    sStaColProd = "LCI_SELECTOR_REWORK_IN";
                    sEndColProd = "LCI_SELECTOR_REWORK_LOSS";
                    sStaColSumm = "LCI_SELECTOR_REWORK_IN";
                    sEndColSumm = "LCI_SELECTOR_REWORK_LOSS_YEILD";
                    break;

                case "chkSELECTOR":
                    sStaColProd = "SELECTOR_IN";
                    sEndColProd = "SELECTOR_LOSS";
                    sStaColSumm = "SELECTOR_IN";
                    sEndColSumm = "SELECTOR_LOSS_YEILD";
                    break;
                case "chkSELECTORRE":
                    sStaColProd = "SELECTOR_REWORK_IN";
                    sEndColProd = "SELECTOR_REWORK_LOSS";
                    sStaColSumm = "SELECTOR_REWORK_IN";
                    sEndColSumm = "SELECTOR_REWORK_LOSS_YEILD";
                    break;

                case "chkEOL":
                    // 2024.02.16  전체양품율 추가에 따른 수정
                    //sStaColProd = "EOL_IN";
                    sStaColProd = "EOL_TOTAL_YEILD";
                    sEndColProd = "EOL_LOSS";

                    // 2024.02.16  전체양품율 추가에 따른 수정
                    //sStaColSumm = "EOL_IN";
                    sStaColSumm = "EOL_TOTAL_YEILD";
                    sEndColSumm = "EOL_LOSS_YEILD";
                    break;
                case "chkEOLRE":
                    sStaColProd = "EOL_REWORK_IN";
                    sEndColProd = "EOL_REWORK_LOSS";
                    sStaColSumm = "EOL_REWORK_IN";
                    sEndColSumm = "EOL_REWORK_LOSS_YEILD";
                    break;

                case "chkINSP_REQ":
                    sStaColProd = "GRADE_1";
                    sEndColProd = "GRADE_1";
                    break;

                // 2023.12.07 SORTER 추가
                case "chkSORTER":
                    sStaColProd = "SORTER_IN";
                    sEndColProd = "SORTER_LOSS";
                    sStaColSumm = "SORTER_IN";
                    sEndColSumm = "SORTER_LOSS_YEILD";
                    break;
                case "chkSORTERRE":
                    sStaColProd = "SORTER_REWORK_IN";
                    sEndColProd = "SORTER_REWORK_LOSS";
                    sStaColSumm = "SORTER_REWORK_IN";
                    sEndColSumm = "SORTER_REWORK_LOSS_YEILD";
                    break;

                default:
                    return;
            }

            // 1번째 항목 존재 여부 확인
            if (dgProdResult.Columns.Contains(sStaColProd) == false)
                return;

            for (int i = dgProdResult.Columns[sStaColProd].Index; i <= dgProdResult.Columns[sEndColProd].Index; i++)
            {
                SetVisibleProdResultColum(i, oVisible);
            }

            if (String.IsNullOrEmpty(sStaColSumm) == false)
            {
                for (int i = dgProdResultSummary.Columns[sStaColSumm].Index; i <= dgProdResultSummary.Columns[sEndColSumm].Index; i++)
                {
                    dgProdResultSummary.Columns[i].Visibility = oVisible;
                }
            }
        }

        /// <summary>
        /// dgProdResult의 Column의 Visiable를 설정함 
        /// 2024.01.18 추가
        /// </summary>
        private void SetVisibleProdResultColum(int iCol, Visibility oVisible, Boolean bSumCheck = true)
        {
            Visibility oUpdVisible = oVisible;
            string[] sColNameList;
            string sColName = String.Empty;
            Boolean bFixedCol;

            List<String> ExceptColName = new List<string>
            {
                  "_TOTAL_YEILD", "_IN", "_OUT", "_YEILD", "_LOSS"
                , "GRADE_1"
            };

            if (oUpdVisible == Visibility.Visible)
            {
                sColNameList = dgProdResult.Columns[iCol].Name.Split('|');
                sColName = sColNameList[sColNameList.Length - 1];

                // 합계가 0일 경우 Visibility.Collapsed
                if (bSumCheck == true)
                {
                    bFixedCol = false;

                    foreach (String sExcept in ExceptColName)
                    {
                        if (sColName.Contains(sExcept) == false)
                            continue;

                        if (sColName.Substring(sColName.IndexOf(sExcept)) != sExcept)
                            continue;

                        bFixedCol = true;
                        break;
                    }

                    // 고정 Colum : False
                    if (bFixedCol == false)
                    {
                        // 조회 전 또는 조회 결과 0 
                        if ((dgProdResult.TopRows.Count == dgProdResult.Rows.Count) ||
                            (dgProdResult[dgProdResult.Rows.Count - 1, iCol].Text.ToString() == "0"))
                            oUpdVisible = Visibility.Collapsed;
                    }
                }
            }

            dgProdResult.Columns[iCol].Visibility = oUpdVisible;
        }

        /// <summary>
        /// dgProdResult 및 dgProdResultSummary의 모든 Column의 Visiable를 설정함
        /// 2024.01.18 추가
        /// </summary>
        private void SetVisibleProdResultAllColumn()
        {
            Visibility oVisible;

            for (int idx = 0; idx < Area.Children.Count; idx++)
            {
                CheckBox chk = Area.Children[idx] as CheckBox;

                oVisible = chk.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                SetVisibleProdResult(chk, oVisible);
            }
        }

        private void ClearFilterGrid()
        {
            try
            {
                // 2024.03.13 동기화로 변경
                //// 2024.03.07 ClearFilter 추가
                //dgProdResult.ClearFilter(); 
                //dgProdResultSummary.ClearFilter();

                if ((dgProdResult.ItemsSource != null) && (dgProdResult.Columns.Count > 0))
                {
                    dgProdResult.FilterBy(dgProdResult.Columns[0], null);
                }

                if ((dgProdResultSummary.ItemsSource != null) && (dgProdResultSummary.Columns.Count > 0))
                {
                    dgProdResultSummary.FilterBy(dgProdResultSummary.Columns[0], null);
                }
            }
            finally { }
        }

        #endregion

        #region [Event]

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            btnSearch.IsEnabled = false;

            object[] argument = new object[6];
            argument[0] = dtpFromDate.SelectedDateTime;
            argument[1] = dtpToDate.SelectedDateTime;
            argument[2] = Util.GetCondition(cboLine, bAllNull: true);
            argument[3] = Util.GetCondition(cboModel, bAllNull: true);
            argument[4] = Util.GetCondition(cboLotType, bAllNull: true); // 2021.08.19 Lot 유형 검색조건 추가

            // 2024.02.16 GDW 수집 여부 추가
            argument[5] = Util.GetCondition(cboSumFlagGDW, bAllNull: true);

            xProgress.Percent = 0;
            xProgress.ProgressText = string.Empty;
            xProgress.Visibility = Visibility.Visible;

            ClearFilterGrid(); // 2024.03.13 추가

            if (!bgWorker.IsBusy)
            {
                bgWorker.RunWorkerAsync(argument);
            }
        }
        
        private void chk_Checked(object sender, EventArgs e)
        {
            CheckBox chk = (CheckBox)sender;

            SetVisibleProdResult(chk, Visibility.Visible);
        }

        private void chk_UnChecked(object sender, EventArgs e)
        {
            CheckBox chk = (CheckBox)sender;

            SetVisibleProdResult(chk, Visibility.Collapsed);
        }        

        private void chkSummary_Checked(object sender, RoutedEventArgs e)
        {
            this.RowProdResult.Height = new GridLength(0);
            this.RowProdResultSummary.Height = new GridLength(2, GridUnitType.Star);
        }

        private void chkSummary_UnChecked(object sender, RoutedEventArgs e)
        {
            this.RowProdResult.Height = new GridLength(2, GridUnitType.Star);
            this.RowProdResultSummary.Height = new GridLength(0);
        }

        private void chkAll_Checked(object sender, RoutedEventArgs e)
        {
            for (int idx = 0; idx < Area.Children.Count; idx++)
            {
                CheckBox chk = Area.Children[idx] as CheckBox;
                chk.IsChecked = true;
            }
        }

        private void chkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int idx = 0; idx < Area.Children.Count; idx++)
            {
                CheckBox chk = Area.Children[idx] as CheckBox;
                chk.IsChecked = false;
            }
        }

        private void dgProdResultSummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);

                    if (e.Cell.Row.Index == dataGrid.Rows.Count - 1)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF7E9D5"));
                    }
                }));

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        
        private void dgProdResult_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index < dataGrid.Rows.Count - dataGrid.BottomRows.Count)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private void dgProdResult_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Index == dg.Rows.Count - 1)
                {
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF7E9D5"));
                }
            }));
        }

        private void BgWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            e.Result = GetList(e.Argument);
        }

        private void BgWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            xProgress.Percent = e.ProgressPercentage;
            xProgress.ProgressText = e.UserState == null ? "" : e.UserState.ToString();
        }

        private void BgWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null && e.Result is Exception)
                {
                    Util.MessageException((Exception)e.Result);
                }
                else if (e.Result != null && e.Result is DataSet)
                {
                    DataSet dsData = (DataSet)e.Result;

                    if (dsData != null)
                    {
                        if (dsData.Tables.Contains("RSLTDT")) dgProdResult.ItemsSource = DataTableConverter.Convert(dsData.Tables["RSLTDT"]);
                        if (dsData.Tables.Contains("SUMMARY")) dgProdResultSummary.ItemsSource = DataTableConverter.Convert(dsData.Tables["SUMMARY"]);

                        // 2024.01.18  수정: dgProdResult 및 dgProdResultSummary의 모든 Column의 Visiable를 설정함
                        SetVisibleProdResultAllColumn();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            xProgress.Visibility = Visibility.Collapsed;
            xProgress.Percent = 0;

            btnSearch.IsEnabled = true;
        }

        #endregion

    }
}
