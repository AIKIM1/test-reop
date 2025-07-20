/*************************************************************************************
 Created Date : 2020.12.22
      Creator : 박수미 feat Mr.Kim
   Decription : 생산 실적 레포트
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.22  NAME : Initial Created
  2021.08.19   KDH : Lot 유형 검색조건 추가
  2021.08.30   KDH : 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동)
  2022.03.28   KDH : 제품검사의뢰 수량 산출 추가
  2022.06.20   KDH : CSR NO(C20220603-000198) 오류수정 (컬럼명칭 매핑에러)
  2022.07.07   JYD : 일자별 집계에서 EQSGID 로 분할 집계 및 프로그래스 처리(속도)
  2022.07.12   JYD : DA_BAS_SEL_EQUIPMENTSEGMENT_FORM 비즈 EQSGID 인수 추가
  2022.07.25   KDH : C20220603-000198 2차 저전압 조건 추가
  2022.09.01   조영대 : 집계시 루프 Merge 처리 MODEL_ID 까지 분리
  2022.12.20   조영대 : 특성양품재작업 추가
  2022.12.22   형준우 : EOL의 전체양품율 추가
  2023.01.03   조영대 : 특성양품재작업 오류 수정.
  2023.01.04   형준우 : EOL의 전체양품율 오류 수정
  2023.01.06   형준우 : EOL의 전체양품율 수치 조정
  2023.02.07   배준호 : LotType MultSelection 
  2023.03.23   이윤중 : [E20230322-001791] 검사의뢰 데이터 표현방식 수정 (PROCID 기준으로 세분화) / DA_SEL_INSP_REQ_ACTDATA
  2023.04.21   조영대 : Summary Table 적용(속도 향상을 위한 집계 테이블 사용으로 수정)
  2023.06.02   조영대 : 2차 저전압 불량컬럼을 2차 충전에 포함
  2023.07.14   이정미 : 불량 Cell 목록 팝업 추가, 생산라인 MultiSelection으로 변경
  2024.01.10   조영대 : 양품률 적용 수정 및 엑셀 다운로드시 양품률 불일치 수정
  2024.02.27   이해령 : EOL 전체양품율 합계 계산식 수정(0이 아닌 전체양품율의 평균 -> (직행양품합계 + 재작업 양품합계) / 직행투입합계)
  2025.04.10   하유승 : 저전압, 2차 저전압 SUMMARY 전체 양품율 추가
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
using System.Windows.Input;
using System.Threading;
using System.IO;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Extensions;
using static LGC.GMES.MES.CMM001.Controls.UcBaseDataGrid;
using LGC.GMES.MES.CMM001.Controls;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_039_SUMMARY : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]

        private bool b2LowVoltFlag = false; //2차 저전압 조건

        private DataTable _dtHeader;
        private DataTable _dtHeaderSummary;
        private DataTable dtTemp;
        private DateTime _sFromDate = Convert.ToDateTime("9999-01-01 00:00:00");
        private DateTime _sToDate = Convert.ToDateTime("9999-01-01 00:00:00");

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
        public FCS001_039_SUMMARY()
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

            Set_CheckBox();

            InitSpread();
            InitSpreadSummary();

            chkSummary.Checked += chkSummary_Checked;
            chkSummary.Unchecked += chkSummary_UnChecked;

            chkAll.Checked += chkAll_Checked;
            chkAll.Unchecked += chkAll_Unchecked;

            SetLotTypeCombo(cboLotType);
            SetLineCombo(cboLine);

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
            SetLineModel(cboModel);
        }

        private void InitControl()
        {
            // Util 에 해당 함수 추가 필요.
            dtpSearchDate.SelectedFromDateTime = DateTime.Now.AddDays(-1);
            dtpSearchDate.SelectedToDateTime = DateTime.Now;

            _dtHeader = new DataTable();
            _dtHeaderSummary = new DataTable();
            dtTemp = new DataTable();

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

            //FIX
            FixedMultiHeader("WORK_DATE|WORK_DATE|WORK_DATE", "CALDATE", false, false, 100, sSumType: null, HorizonAlign: HorizontalAlignment.Center);
            FixedMultiHeader("LINE_ID|LINE_ID|LINE_ID", "LINE_NAME", false, false, 150, sSumType: "합계", HorizonAlign: HorizontalAlignment.Center);
            FixedMultiHeader("MODEL_ID|MODEL_ID|MODEL_ID", "MDLLOT_ID", false, false, sSumType: null, HorizonAlign: HorizontalAlignment.Center);
            FixedMultiHeader("MODEL_NAME|MODEL_NAME|MODEL_NAME", "MODEL_NAME", false, false, 0, sSumType: null, HorizonAlign: HorizontalAlignment.Center);
            FixedMultiHeader("PRE_AGING|PRE_AGING|AGING_INPUT", "AGING_IN", false, false);
                        

            var procList = dtTemp.AsEnumerable().OrderBy(o => o.Field<string>("ATTR1")).ToList();
            int idx = 1;

            b2LowVoltFlag = dtTemp.AsEnumerable().Where(x => x["COM_CODE"].Equals("2LOWVOLT")).Count() > 0 ? true : false;

            foreach (var Item in procList)
            {
                DataRow[] drcominfo = dtTemp.AsEnumerable().Where(f => f.Field<string>("ATTR1").Equals(idx.ToString())).ToArray();

                idx++;

                foreach (DataRow dr in drcominfo)
                {
                    if (Util.NVC(dr["COM_CODE"]).Equals("1CHARGE"))
                    {
                        //1차 충전(Degas 전)
                        FixedMultiHeader("1_CHARGE_DEGAS_B|PERF|INPUT", "1_CHARGE_DEGAS_B_IN", false, false);
                        FixedMultiHeader("1_CHARGE_DEGAS_B|PERF|GOOD_PRD", "1_CHARGE_DEGAS_B_OUT", false, false);
                        FixedMultiHeader("1_CHARGE_DEGAS_B|PERF|GOOD_RATE", "1_CHARGE_DEGAS_B_YEILD", true, false, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["1_CHARGE_DEGAS_B_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResult.Columns["1_CHARGE_DEGAS_B_IN"], dgProdResult.Columns["1_CHARGE_DEGAS_B_OUT"]) });

                        AddDefectHeader(dgProdResult, "A", "1_CHARGE_DEGAS_B", "1_CHARGE_DEGAS_B_", 75);
                        FixedMultiHeader("1_CHARGE_DEGAS_B|TOTAL|DEFECT", "1_CHARGE_DEGAS_B_LOSS", false, false);
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("1CHARGERE"))
                    {
                        //1차 충전(Degas 전) 재작업
                        FixedMultiHeader("1_CHARGE_DEGAS_B_REWORK|PERF|INPUT", "1_CHARGE_DEGAS_B_REWORK_IN", false, false);
                        FixedMultiHeader("1_CHARGE_DEGAS_B_REWORK|PERF|GOOD_PRD", "1_CHARGE_DEGAS_B_REWORK_OUT", false, false);
                        FixedMultiHeader("1_CHARGE_DEGAS_B_REWORK|PERF|GOOD_RATE", "1_CHARGE_DEGAS_B_REWORK_YEILD", true, false, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["1_CHARGE_DEGAS_B_REWORK_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResult.Columns["1_CHARGE_DEGAS_B_REWORK_IN"], dgProdResult.Columns["1_CHARGE_DEGAS_B_REWORK_OUT"]) });

                        AddDefectHeader(dgProdResult, "A", "1_CHARGE_DEGAS_B_REWORK", "1_CHARGE_DEGAS_B_REWORK_", 75, "B");
                        FixedMultiHeader("1_CHARGE_DEGAS_B_REWORK|TOTAL|DEFECT", "1_CHARGE_DEGAS_B_REWORK_LOSS", false, false);
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("DEGAS"))
                    {
                        //Dgas
                        FixedMultiHeader("DEGAS|PERF|INPUT", "DEGAS_IN", false, false);
                        FixedMultiHeader("DEGAS|PERF|GOOD_PRD", "DEGAS_OUT", false, false);
                        FixedMultiHeader("DEGAS|PERF|GOOD_RATE", "DEGAS_YEILD", true, false, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["DEGAS_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResult.Columns["DEGAS_IN"], dgProdResult.Columns["DEGAS_OUT"]) });

                        AddDefectHeader(dgProdResult, "D", "DEGAS", "DEGAS_");
                        FixedMultiHeader("DEGAS|TOTAL|DEFECT", "DEGAS_LOSS", false, false);
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("DEGASRE"))
                    {
                        //Dgas 재작업
                        FixedMultiHeader("DEGAS_REWORK|PERF|INPUT", "DEGAS_REWORK_IN", false, false);
                        FixedMultiHeader("DEGAS_REWORK|PERF|GOOD_PRD", "DEGAS_REWORK_OUT", false, false);
                        FixedMultiHeader("DEGAS_REWORK|PERF|GOOD_RATE", "DEGAS_REWORK_YEILD", true, false, sSumType: "NONE");
                        
                        DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["DEGAS_REWORK_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResult.Columns["DEGAS_REWORK_IN"], dgProdResult.Columns["DEGAS_REWORK_OUT"]) });

                        AddDefectHeader(dgProdResult, "D", "DEGAS_REWORK", "DEGAS_REWORK_");
                        FixedMultiHeader("DEGAS_REWORK|TOTAL|DEFECT", "DEGAS_REWORK_LOSS", false, false);
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("2CHARGE"))
                    {
                        //2차충전(Dgas 후)
                        FixedMultiHeader("2_CHARGE_DEGAS_A|PERF|INPUT", "2_CHARGE_DEGAS_A_IN", false, false);
                        FixedMultiHeader("2_CHARGE_DEGAS_A|PERF|GOOD_PRD", "2_CHARGE_DEGAS_A_OUT", false, false);
                        FixedMultiHeader("2_CHARGE_DEGAS_A|PERF|GOOD_RATE", "2_CHARGE_DEGAS_A_YEILD", true, false, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["2_CHARGE_DEGAS_A_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResult.Columns["2_CHARGE_DEGAS_A_IN"], dgProdResult.Columns["2_CHARGE_DEGAS_A_OUT"]) });

                        AddDefectHeader(dgProdResult, "B", "2_CHARGE_DEGAS_A", "2_CHARGE_DEGAS_A_", 75);
                        FixedMultiHeader("2_CHARGE_DEGAS_A|TOTAL|DEFECT", "2_CHARGE_DEGAS_A_LOSS", false, false);
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("2CHARGERE"))
                    {
                        //2차충전(Dgas 후) 재작업
                        FixedMultiHeader("2_CHARGE_DEGAS_A_REWORK|PERF|INPUT", "2_CHARGE_DEGAS_A_REWORK_IN", false, false);
                        FixedMultiHeader("2_CHARGE_DEGAS_A_REWORK|PERF|GOOD_PRD", "2_CHARGE_DEGAS_A_REWORK_OUT", false, false);
                        FixedMultiHeader("2_CHARGE_DEGAS_A_REWORK|PERF|GOOD_RATE", "2_CHARGE_DEGAS_A_REWORK_YEILD", true, false, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["2_CHARGE_DEGAS_A_REWORK_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResult.Columns["2_CHARGE_DEGAS_A_REWORK_IN"], dgProdResult.Columns["2_CHARGE_DEGAS_A_REWORK_OUT"]) });

                        AddDefectHeader(dgProdResult, "B", "2_CHARGE_DEGAS_A_REWORK", "2_CHARGE_DEGAS_A_REWORK_", 75, "B");
                        FixedMultiHeader("2_CHARGE_DEGAS_A_REWORK|TOTAL|DEFECT", "2_CHARGE_DEGAS_A_REWORK_LOSS", false, false);
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("LOWVOLT"))
                    {
                        // 2025.04.10 전체양품율 추가
                        FixedMultiHeader("LOW_CAPA_BAD_PASS|PERF|TOTAL_GOOD_RATE", "LOW_CAPA_BAD_PASS_TOTAL_YEILD", true, false, sSumType: "NONE");

                        //저전압
                        FixedMultiHeader("LOW_CAPA_BAD_PASS|PERF|INPUT", "LOW_CAPA_BAD_PASS_IN", false, false);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS|PERF|GOOD_PRD", "LOW_CAPA_BAD_PASS_OUT", false, false);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS|PERF|GOOD_RATE", "LOW_CAPA_BAD_PASS_YEILD", true, false, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["LOW_CAPA_BAD_PASS_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResult.Columns["LOW_CAPA_BAD_PASS_IN"], dgProdResult.Columns["LOW_CAPA_BAD_PASS_OUT"]) });

                        AddDefectHeader(dgProdResult, "G", "LOW_CAPA_BAD_PASS", "LOW_CAPA_BAD_PASS_", 75);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS|TOTAL|DEFECT", "LOW_CAPA_BAD_PASS_LOSS", false, false);

                        //저전압 재작업 2021.06.01 추가
                        FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|PERF|INPUT", "LOW_CAPA_BAD_PASS_REWORK_IN", false, false);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|PERF|GOOD_PRD", "LOW_CAPA_BAD_PASS_REWORK_OUT", false, false);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|PERF|GOOD_RATE", "LOW_CAPA_BAD_PASS_REWORK_YEILD", true, false, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["LOW_CAPA_BAD_PASS_REWORK_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResult.Columns["LOW_CAPA_BAD_PASS_REWORK_IN"], dgProdResult.Columns["LOW_CAPA_BAD_PASS_REWORK_OUT"]) });

                        AddDefectHeader(dgProdResult, "G", "LOW_CAPA_BAD_PASS_REWORK", "LOW_CAPA_BAD_PASS_REWORK", 75); //Width 추가
                        FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|TOTAL|DEFECT", "LOW_CAPA_BAD_PASS_REWORK_LOSS", false, false);
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("EOL"))
                    {
                        // 2022.12.22 전체양품율 추가
                        FixedMultiHeader("EOL|PERF|TOTAL_GOOD_RATE", "EOL_TOTAL_YEILD", true, false, sSumType: "NONE"); // 2024.02.27 합계 타입 EVEN -> NONE 수정

                        //특성
                        FixedMultiHeader("EOL|PERF|INPUT", "EOL_IN", false, false);
                        FixedMultiHeader("EOL|PERF|GOOD_PRD", "EOL_OUT", false, false);
                        FixedMultiHeader("EOL|PERF|GOOD_RATE", "EOL_YEILD", true, false, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["EOL_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResult.Columns["EOL_IN"], dgProdResult.Columns["EOL_OUT"]) });


                        AddDefectHeader(dgProdResult, "5", "EOL", "EOL_");
                        FixedMultiHeader("EOL|TOTAL|DEFECT", "EOL_LOSS", false, false);
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("EOLRE"))
                    {
                        //특성 재작업
                        FixedMultiHeader("EOL_REWORK|PERF|INPUT", "EOL_REWORK_IN", false, false);
                        FixedMultiHeader("EOL_REWORK|PERF|GOOD_PRD", "EOL_REWORK_OUT", false, false);
                        FixedMultiHeader("EOL_REWORK|PERF|GOOD_RATE", "EOL_REWORK_YEILD", true, false, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["EOL_REWORK_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResult.Columns["EOL_REWORK_IN"], dgProdResult.Columns["EOL_REWORK_OUT"]) });

                        List<C1.WPF.DataGrid.DataGridColumn> totalColumn = new List<C1.WPF.DataGrid.DataGridColumn>() { dgProdResult.Columns["EOL_IN"] };
                        List<C1.WPF.DataGrid.DataGridColumn> partColumn = new List<C1.WPF.DataGrid.DataGridColumn>() { dgProdResult.Columns["EOL_OUT"], dgProdResult.Columns["EOL_REWORK_OUT"] };
                        DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["EOL_TOTAL_YEILD"], new DataGridAggregatesCollection { new DataGridAggregateRatio(totalColumn, partColumn) });

                        AddDefectHeader(dgProdResult, "5", "EOL_REWORK", "EOL_");
                        FixedMultiHeader("EOL_REWORK|TOTAL|DEFECT", "EOL_REWORK_LOSS", false, false);
                    }
                    //제품검사의뢰 수량 산출
                    else if (Util.NVC(dr["COM_CODE"]).Equals("INSP_REQ"))
                    {
                        //검사의뢰
                        FixedMultiHeader("INSP_REQ|PROC_INSP_REQ|DEGAS", "INSP_REQ_LQC_DEGAS", false, false);
                        FixedMultiHeader("INSP_REQ|PROC_INSP_REQ|EOL", "INSP_REQ_LQC_EOL", false, false);
                        FixedMultiHeader("INSP_REQ|PROC_INSP_REQ|ETC", "INSP_REQ_LQC_ETC", false, false);
                        FixedMultiHeader("INSP_REQ|PQC_INSP_REQ|LOW_VOLT", "INSP_REQ_PQC_LOW_VOLT", false, false);
                        FixedMultiHeader("INSP_REQ|PQC_INSP_REQ|EOL", "INSP_REQ_PQC_EOL", false, false);
                        FixedMultiHeader("INSP_REQ|PQC_INSP_REQ|ETC", "INSP_REQ_PQC_ETC", false, false);
                    }
                    //2차 저전압 조건
                    else if (Util.NVC(dr["COM_CODE"]).Equals("2LOWVOLT"))
                    {
                        // 2025.04.10 전체양품율 추가
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS|PERF|TOTAL_GOOD_RATE", "2_LOW_CAPA_BAD_PASS_TOTAL_YEILD", true, false, sSumType: "NONE");

                        //저전압
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS|PERF|INPUT", "2_LOW_CAPA_BAD_PASS_IN", false, false);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS|PERF|GOOD_PRD", "2_LOW_CAPA_BAD_PASS_OUT", false, false);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS|PERF|GOOD_RATE", "2_LOW_CAPA_BAD_PASS_YEILD", true, false, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["2_LOW_CAPA_BAD_PASS_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResult.Columns["2_LOW_CAPA_BAD_PASS_IN"], dgProdResult.Columns["2_LOW_CAPA_BAD_PASS_OUT"]) });

                        AddDefectHeader(dgProdResult, "G", "2_LOW_CAPA_BAD_PASS", "2_LOW_CAPA_BAD_PASS_", 75);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS|TOTAL|DEFECT", "2_LOW_CAPA_BAD_PASS_LOSS", false, false);

                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS_REWORK|PERF|INPUT", "2_LOW_CAPA_BAD_PASS_REWORK_IN", false, false);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS_REWORK|PERF|GOOD_PRD", "2_LOW_CAPA_BAD_PASS_REWORK_OUT", false, false);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS_REWORK|PERF|GOOD_RATE", "2_LOW_CAPA_BAD_PASS_REWORK_YEILD", true, false, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["2_LOW_CAPA_BAD_PASS_REWORK_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResult.Columns["2_LOW_CAPA_BAD_PASS_REWORK_IN"], dgProdResult.Columns["2_LOW_CAPA_BAD_PASS_REWORK_OUT"]) });

                        AddDefectHeader(dgProdResult, "G", "2_LOW_CAPA_BAD_PASS_REWORK", "2_LOW_CAPA_BAD_PASS_REWORK", 75);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS_REWORK|TOTAL|DEFECT", "2_LOW_CAPA_BAD_PASS_REWORK_LOSS", false, false);
                    }
                    // 특성 양품 재작업
                    else if (Util.NVC(dr["COM_CODE"]).Equals("EOLGRE"))
                    {
                        //특성 재작업
                        FixedMultiHeader("EOL_GREWORK|PERF|INPUT", "EOL_GREWORK_IN", false, false);
                        FixedMultiHeader("EOL_GREWORK|PERF|GOOD_PRD", "EOL_GREWORK_OUT", false, false);
                        FixedMultiHeader("EOL_GREWORK|PERF|GOOD_RATE", "EOL_GREWORK_YEILD", true, false, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["EOL_GREWORK_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResult.Columns["EOL_GREWORK_IN"], dgProdResult.Columns["EOL_GREWORK_OUT"]) });

                        AddDefectHeader(dgProdResult, "5", "EOL_GREWORK", "EOL_G");
                        FixedMultiHeader("EOL_GREWORK|TOTAL|DEFECT", "EOL_GREWORK_LOSS", false, false);
                    }
                }
            }
        }

        private void InitSpreadSummary()
        {
            Util.gridClear(dgProdResultSummary); //Grid clear

            int Header_Row_count = 2;

            //칼럼 헤더 행 추가
            for (int i = 0; i < Header_Row_count; i++)
            {
                DataGridColumnHeaderRow HR = new DataGridColumnHeaderRow();
                dgProdResultSummary.TopRows.Add(HR);
            }

            //FIX
            FixedMultiHeader("WORK_DATE|WORK_DATE", "CALDATE", false, true, 100, sSumType: null, HorizonAlign: HorizontalAlignment.Center);
            FixedMultiHeader("LINE_ID|LINE_ID", "LINE_NAME", false, true, 150, sSumType: "합계", HorizonAlign: HorizontalAlignment.Center);
            FixedMultiHeader("MODEL_ID|MODEL_ID", "MDLLOT_ID", false, true, sSumType: null, HorizonAlign: HorizontalAlignment.Center);
            FixedMultiHeader("MODEL_NAME|MODEL_NAME", "MODEL_NAME", false, true, sSumType: null, HorizonAlign: HorizontalAlignment.Center);

            var procList = dtTemp.AsEnumerable().OrderBy(o => o.Field<string>("ATTR1")).ToList();
            int idx = 1;
            foreach (var Item in procList)
            {
                DataRow[] drcominfo = dtTemp.AsEnumerable().Where(f => f.Field<string>("ATTR1").Equals(idx.ToString())).ToArray();

                idx++;

                foreach (DataRow dr in drcominfo)
                {
                    if (Util.NVC(dr["COM_CODE"]).Equals("1CHARGE"))
                    {
                        //1차 충전
                        FixedMultiHeader("1_CHARGE|INPUT", "1_CHARGE_DEGAS_B_IN", false, true);
                        FixedMultiHeader("1_CHARGE|GOOD_PRD", "1_CHARGE_DEGAS_B_OUT", false, true);
                        FixedMultiHeader("1_CHARGE|DEFECT", "1_CHARGE_DEGAS_B_LOSS", false, true);
                        FixedMultiHeader("1_CHARGE|GOOD_RATE", "1_CHARGE_DEGAS_B_YEILD", true, true, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["1_CHARGE_DEGAS_B_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["1_CHARGE_DEGAS_B_IN"], dgProdResultSummary.Columns["1_CHARGE_DEGAS_B_OUT"]) });

                        FixedMultiHeader("1_CHARGE|BAD_RATE", "1_CHARGE_DEGAS_B_LOSS_YEILD", true, true, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["1_CHARGE_DEGAS_B_LOSS_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["1_CHARGE_DEGAS_B_IN"], dgProdResultSummary.Columns["1_CHARGE_DEGAS_B_LOSS"]) });

                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("1CHARGERE"))
                    {
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("DEGAS"))
                    {
                        //Dgas
                        FixedMultiHeader("DEGAS|INPUT", "DEGAS_IN", false, true);
                        FixedMultiHeader("DEGAS|GOOD_PRD", "DEGAS_OUT", false, true);
                        FixedMultiHeader("DEGAS|DEFECT", "DEGAS_LOSS", false, true);
                        FixedMultiHeader("DEGAS|GOOD_RATE", "DEGAS_YEILD", true, true, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["DEGAS_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["DEGAS_IN"], dgProdResultSummary.Columns["DEGAS_OUT"]) });

                        FixedMultiHeader("DEGAS|BAD_RATE", "DEGAS_LOSS_YEILD", true, true, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["DEGAS_LOSS_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["DEGAS_IN"], dgProdResultSummary.Columns["DEGAS_LOSS"]) });

                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("DEGASRE"))
                    {
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("2CHARGE"))
                    {
                        //2차충전(Dgas 후)
                        FixedMultiHeader("2_CHARGE|INPUT", "2_CHARGE_DEGAS_A_IN", false, true);
                        FixedMultiHeader("2_CHARGE|GOOD_PRD", "2_CHARGE_DEGAS_A_OUT", false, true);
                        FixedMultiHeader("2_CHARGE|DEFECT", "2_CHARGE_DEGAS_A_LOSS", false, true);
                        FixedMultiHeader("2_CHARGE|GOOD_RATE", "2_CHARGE_DEGAS_A_YEILD", true, true, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["2_CHARGE_DEGAS_A_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["2_CHARGE_DEGAS_A_IN"], dgProdResultSummary.Columns["2_CHARGE_DEGAS_A_OUT"]) });

                        FixedMultiHeader("2_CHARGE|BAD_RATE", "2_CHARGE_DEGAS_A_LOSS_YEILD", true, true, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["2_CHARGE_DEGAS_A_LOSS_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["2_CHARGE_DEGAS_A_IN"], dgProdResultSummary.Columns["2_CHARGE_DEGAS_A_LOSS"]) });

                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("2CHARGERE"))
                    {
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("LOWVOLT"))
                    {
                        // 2025.04.10 전체양품율 추가
                        FixedMultiHeader("LOW_CAPA_BAD_PASS|TOTAL_GOOD_RATE", "LOW_CAPA_BAD_PASS_TOTAL_YEILD", true, true, sSumType: "NONE");

                        //저전압
                        FixedMultiHeader("LOW_CAPA_BAD_PASS|INPUT", "LOW_CAPA_BAD_PASS_IN", false, true);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS|GOOD_PRD", "LOW_CAPA_BAD_PASS_OUT", false, true);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS|DEFECT", "LOW_CAPA_BAD_PASS_LOSS", false, true);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS|GOOD_RATE", "LOW_CAPA_BAD_PASS_YEILD", true, true, sSumType: "NONE"); //20220725_컬럼명 수정

                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["LOW_CAPA_BAD_PASS_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["LOW_CAPA_BAD_PASS_IN"], dgProdResultSummary.Columns["LOW_CAPA_BAD_PASS_OUT"]) });

                        FixedMultiHeader("LOW_CAPA_BAD_PASS|BAD_RATE", "LOW_CAPA_BAD_PASS_LOSS_YEILD", true, true, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["LOW_CAPA_BAD_PASS_LOSS_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["LOW_CAPA_BAD_PASS_IN"], dgProdResultSummary.Columns["LOW_CAPA_BAD_PASS_LOSS"]) });


                        FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|INPUT", "LOW_CAPA_BAD_PASS_REWORK_IN", false, true);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|GOOD_PRD", "LOW_CAPA_BAD_PASS_REWORK_OUT", false, true);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|DEFECT", "LOW_CAPA_BAD_PASS_REWORK_LOSS", false, true);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|GOOD_RATE", "LOW_CAPA_BAD_PASS_REWORK_YEILD", true, true, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["LOW_CAPA_BAD_PASS_REWORK_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["LOW_CAPA_BAD_PASS_REWORK_IN"], dgProdResultSummary.Columns["LOW_CAPA_BAD_PASS_REWORK_OUT"]) });

                        FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|BAD_RATE", "LOW_CAPA_BAD_PASS_REWORK_LOSS_YEILD", true, true, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["LOW_CAPA_BAD_PASS_REWORK_LOSS_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["LOW_CAPA_BAD_PASS_REWORK_IN"], dgProdResultSummary.Columns["LOW_CAPA_BAD_PASS_REWORK_LOSS"]) });

                        List<C1.WPF.DataGrid.DataGridColumn> totalColumn = new List<C1.WPF.DataGrid.DataGridColumn>() { dgProdResultSummary.Columns["LOW_CAPA_BAD_PASS_IN"] };
                        List<C1.WPF.DataGrid.DataGridColumn> partColumn = new List<C1.WPF.DataGrid.DataGridColumn>() { dgProdResultSummary.Columns["LOW_CAPA_BAD_PASS_OUT"], dgProdResultSummary.Columns["LOW_CAPA_BAD_PASS_REWORK_OUT"] };
                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["LOW_CAPA_BAD_PASS_TOTAL_YEILD"], new DataGridAggregatesCollection { new DataGridAggregateRatio(totalColumn, partColumn) });

                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("EOL"))
                    {
                        // 2022.12.22 전체양품율 추가
                        FixedMultiHeader("EOL_INSP|TOTAL_GOOD_RATE", "EOL_TOTAL_YEILD", true, true, sSumType: "NONE");  // 2024.02.27 합계 타입 EVEN -> NONE 수정

                        //EOL
                        FixedMultiHeader("EOL_INSP|INPUT", "EOL_IN", false, true);
                        FixedMultiHeader("EOL_INSP|GOOD_PRD", "EOL_OUT", false, true);
                        FixedMultiHeader("EOL_INSP|DEFECT", "EOL_LOSS", false, true);
                        FixedMultiHeader("EOL_INSP|GOOD_RATE", "EOL_YEILD", true, true, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["EOL_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["EOL_IN"], dgProdResultSummary.Columns["EOL_OUT"]) });

                        FixedMultiHeader("EOL_INSP|BAD_RATE", "EOL_LOSS_YEILD", true, true, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["EOL_LOSS_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["EOL_IN"], dgProdResultSummary.Columns["EOL_LOSS"]) });

                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("EOLRE"))
                    {
                        //특성 재작업
                        FixedMultiHeader("EOL_RE_INSP|INPUT", "EOL_REWORK_IN", false, true);
                        FixedMultiHeader("EOL_RE_INSP|GOOD_PRD", "EOL_REWORK_OUT", false, true);
                        FixedMultiHeader("EOL_RE_INSP|DEFECT", "EOL_REWORK_LOSS", false, true);
                        FixedMultiHeader("EOL_RE_INSP|GOOD_RATE", "EOL_REWORK_YEILD", true, true, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["EOL_REWORK_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["EOL_REWORK_IN"], dgProdResultSummary.Columns["EOL_REWORK_OUT"]) });

                        FixedMultiHeader("EOL_RE_INSP|BAD_RATE", "EOL_REWORK_LOSS_YEILD", true, true, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["EOL_REWORK_LOSS_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["EOL_REWORK_IN"], dgProdResultSummary.Columns["EOL_REWORK_LOSS"]) });

                        List<C1.WPF.DataGrid.DataGridColumn> totalColumn = new List<C1.WPF.DataGrid.DataGridColumn>() { dgProdResultSummary.Columns["EOL_IN"] };
                        List<C1.WPF.DataGrid.DataGridColumn> partColumn = new List<C1.WPF.DataGrid.DataGridColumn>() { dgProdResultSummary.Columns["EOL_OUT"], dgProdResultSummary.Columns["EOL_REWORK_OUT"] };
                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["EOL_TOTAL_YEILD"], new DataGridAggregatesCollection { new DataGridAggregateRatio(totalColumn, partColumn) });
                    }
                    //제품검사의뢰 수량 산출
                    else if (Util.NVC(dr["COM_CODE"]).Equals("INSP_REQ"))
                    {
                        //검사의뢰
                        FixedMultiHeader("INSP_REQ|PROC_INSP_REQ", "INSP_REQ_LQC", true, true, sSumType: "EVEN");
                        FixedMultiHeader("INSP_REQ|PQC_INSP_REQ", "INSP_REQ_PQC", true, true, sSumType: "EVEN");
                    }
                    //2차 저전압 조건
                    else if (Util.NVC(dr["COM_CODE"]).Equals("2LOWVOLT"))
                    {
                        // 2025.04.10 전체양품율 추가
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS|TOTAL_GOOD_RATE", "2_LOW_CAPA_BAD_PASS_TOTAL_YEILD", true, true, sSumType: "NONE");

                        //저전압
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS|INPUT", "2_LOW_CAPA_BAD_PASS_IN", false, true);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS|GOOD_PRD", "2_LOW_CAPA_BAD_PASS_OUT", false, true);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS|DEFECT", "2_LOW_CAPA_BAD_PASS_LOSS", false, true);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS|GOOD_RATE", "2_LOW_CAPA_BAD_PASS_YEILD", true, true, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["2_LOW_CAPA_BAD_PASS_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["2_LOW_CAPA_BAD_PASS_IN"], dgProdResultSummary.Columns["2_LOW_CAPA_BAD_PASS_OUT"]) });

                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS|BAD_RATE", "2_LOW_CAPA_BAD_PASS_LOSS_YEILD", true, true, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["2_LOW_CAPA_BAD_PASS_LOSS_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["2_LOW_CAPA_BAD_PASS_IN"], dgProdResultSummary.Columns["2_LOW_CAPA_BAD_PASS_LOSS"]) });


                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS_REWORK|INPUT", "2_LOW_CAPA_BAD_PASS_REWORK_IN", false, true);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS_REWORK|GOOD_PRD", "2_LOW_CAPA_BAD_PASS_REWORK_OUT", false, true);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS_REWORK|DEFECT", "2_LOW_CAPA_BAD_PASS_REWORK_LOSS", false, true);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS_REWORK|GOOD_RATE", "2_LOW_CAPA_BAD_PASS_REWORK_YEILD", true, true, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["2_LOW_CAPA_BAD_PASS_REWORK_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["2_LOW_CAPA_BAD_PASS_REWORK_IN"], dgProdResultSummary.Columns["2_LOW_CAPA_BAD_PASS_REWORK_OUT"]) });

                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS_REWORK|BAD_RATE", "2_LOW_CAPA_BAD_PASS_REWORK_LOSS_YEILD", true, true, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["2_LOW_CAPA_BAD_PASS_REWORK_LOSS_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["2_LOW_CAPA_BAD_PASS_REWORK_IN"], dgProdResultSummary.Columns["2_LOW_CAPA_BAD_PASS_REWORK_LOSS"]) });

                        List<C1.WPF.DataGrid.DataGridColumn> totalColumn = new List<C1.WPF.DataGrid.DataGridColumn>() { dgProdResultSummary.Columns["2_LOW_CAPA_BAD_PASS_IN"] };
                        List<C1.WPF.DataGrid.DataGridColumn> partColumn = new List<C1.WPF.DataGrid.DataGridColumn>() { dgProdResultSummary.Columns["2_LOW_CAPA_BAD_PASS_OUT"], dgProdResultSummary.Columns["2_LOW_CAPA_BAD_PASS_REWORK_OUT"] };
                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["2_LOW_CAPA_BAD_PASS_TOTAL_YEILD"], new DataGridAggregatesCollection { new DataGridAggregateRatio(totalColumn, partColumn) });

                    }
                    // 특성 양품 재작업
                    else if (Util.NVC(dr["COM_CODE"]).Equals("EOLGRE"))
                    {
                        //특성 양품재작업
                        FixedMultiHeader("EOL_GREWORK|INPUT", "EOL_GREWORK_IN", false, true);
                        FixedMultiHeader("EOL_GREWORK|GOOD_PRD", "EOL_GREWORK_OUT", false, true);
                        FixedMultiHeader("EOL_GREWORK|DEFECT", "EOL_GREWORK_LOSS", false, true);
                        FixedMultiHeader("EOL_GREWORK|GOOD_RATE", "EOL_GREWORK_YEILD", true, true, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["EOL_GREWORK_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["EOL_GREWORK_IN"], dgProdResultSummary.Columns["EOL_GREWORK_OUT"]) });

                        FixedMultiHeader("EOL_GREWORK|BAD_RATE", "EOL_GREWORK_LOSS_YEILD", true, true, sSumType: "NONE");

                        DataGridAggregate.SetAggregateFunctions(dgProdResultSummary.Columns["EOL_GREWORK_LOSS_YEILD"],
                            new DataGridAggregatesCollection { new DataGridAggregateRatio(
                                dgProdResultSummary.Columns["EOL_GREWORK_IN"], dgProdResultSummary.Columns["EOL_GREWORK_LOSS"]) });

                    }
                }
            }
        }
        #endregion

        #region [Method]

        private void FixedMultiHeader(string sName, string sBindName, bool bPercent, bool bSummray, int iWidth = 75, string sSumType = "SUM", HorizontalAlignment HorizonAlign = HorizontalAlignment.Right)
        {
            bool bReadOnly = true;
            bool bEditable = false;
            bool bVisible = true;

            string[] sColName = sName.Split('|');

            List<string> Multi_Header = new List<string>();
            Multi_Header = sColName.ToList();

            if (bSummray.Equals(true))
            {
                var column_TEXT = CreateTextColumn(null, Multi_Header, sBindName, sBindName, iWidth, bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible, bPercent: bPercent, bSummray: bSummray, sSumType: sSumType, HorizonAlign: HorizonAlign);
                dgProdResultSummary.Columns.Add(column_TEXT);
            }
            else
            {
                var column_TEXT = CreateTextColumn(null, Multi_Header, sBindName, sBindName, iWidth, bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible, bPercent: bPercent, bSummray: bSummray, sSumType: sSumType, HorizonAlign: HorizonAlign);
                dgProdResult.Columns.Add(column_TEXT);
            }
        }

        private void AddDefectHeader(C1DataGrid dg, string sEqpKindCd, string sTopHeader, string sPrefix, int iWidth = 0, string sDefectKind = null, string sDelYN = "N")
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

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TM_CELL_DEFECT", "RQSTDT", "RSLTDT", dtRqst);

            if (b2LowVoltFlag && sEqpKindCd == "B") // 2차 충전일때 저전압 2차 포함.
            {
                dr["DFCT_GR_TYPE_CODE"] = "G";
                dr["DFCT_TYPE_CODE"] = null;

                DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_SEL_TM_CELL_DEFECT", "RQSTDT", "RSLTDT", dtRqst);
                
                foreach (DataRow drAdd in dtRslt2.Rows)
                {
                    if (!drAdd["DFCT_TYPE_CODE"].Equals("B")) continue;
                    if (dtRslt.AsEnumerable().Where(x => x["DFCT_CODE"].Equals(drAdd["DFCT_CODE"])).Count() > 0) continue;

                    drAdd["DFCT_NAME"] = Util.NVC(drAdd["DFCT_NAME"]) + "(" + ObjectDic.Instance.GetObjectName("2_LOW_VOLT") + ")";
                    drAdd["DFCT_DESC"] = Util.NVC(drAdd["DFCT_DESC"]) + "(" + ObjectDic.Instance.GetObjectName("2_LOW_VOLT") + ")";

                    // MES 2.0 ItemArray 위치 오류 Patch
                    //dtRslt.Rows.Add(drAdd.ItemArray);
                    dtRslt.AddDataRow(drAdd);
                }
                dtRslt.AcceptChanges();
            }

            foreach (DataRow d in dtRslt.Rows)
            {
                string sCol = sTopHeader + "|[*]" + d["GROUP_NAME"].ToString().Trim() + "|[*]" + d["DFCT_NAME"].ToString().Trim();
                string[] sColName = sCol.Split('|');
                string sBinding = sTopHeader + "_" + d["DFCT_CODE"].ToString().Trim(); // Binding Name 변경
                bool bReadOnly = true;
                bool bEditable = false;
                bool bVisible = true;

                //칼럼 헤더 자동 병합을 위해 Header로 사용할 List
                List<string> Multi_Header = new List<string>();
                Multi_Header = sColName.ToList();

                var column_TEXT = CreateTextColumn(null, Multi_Header, sCol, sBinding, iWidth, bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible, bPercent: false);
                dg.Columns.Add(column_TEXT);

            }
        }

        private C1.WPF.DataGrid.DataGridTextColumn CreateTextColumn(string Single_Header
                                                                         , List<string> Multi_Header
                                                                         , string sName
                                                                         , string sBinding
                                                                         , int iWidth
                                                                         , bool bReadOnly = false
                                                                         , bool bEditable = true
                                                                         , bool bVisible = true
                                                                         , bool bPercent = false
                                                                         , bool bSummray = false
                                                                         , string sSumType = "SUM"   // 합계, SUM
                                                                         , HorizontalAlignment HorizonAlign = HorizontalAlignment.Right
                                                                         , VerticalAlignment VerticalAlign = VerticalAlignment.Center
                                                        )
        {

            C1.WPF.DataGrid.DataGridTextColumn Col = new C1.WPF.DataGrid.DataGridTextColumn();

            Col.Name = sBinding;
            Col.Binding = new Binding(sBinding);
            Col.IsReadOnly = bReadOnly;
            Col.EditOnSelection = bEditable;
            Col.Visibility = bVisible.Equals(true) ? Visibility.Visible : Visibility.Collapsed;
            Col.HorizontalAlignment = HorizonAlign;
            Col.VerticalAlignment = VerticalAlign;

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

            // Footer Sum
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
            }


            if (bSummray.Equals(true))
            {
                _dtHeaderSummary.Columns.Add(sBinding, typeof(string));
            }
            else
            {
                _dtHeader.Columns.Add(sBinding, typeof(string));
            }
            return Col;
        }

        private object GetList(object arg)
        {
            try
            {
                bgWorker.ReportProgress(0, MessageDic.Instance.GetMessage("10057") + ".....");

                object[] argument = (object[])arg;

                DateTime dtFromDate = (DateTime)argument[0];
                DateTime dtToDate = (DateTime)argument[1];

                string EQSGID = argument[2] == null ? null : argument[2].ToString();
                string MDLLOT_ID = argument[3] == null ? null : argument[3].ToString();
                string LOTTYPE = argument[4] == null ? null : argument[4].ToString();

                ////양품 재작업 존재여부 체크
                //bool isEolGoodRework = false;
                //if (dtTemp.AsEnumerable().Where(f => f.Field<string>("COM_CODE").Equals("EOLGRE")).Count() > 0) isEolGoodRework = true;
                //int runCount = 0;

                TimeSpan tsDateDiff = dtToDate - dtFromDate;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("MONTHLY_YN", typeof(string));
                dtRqst.Columns.Add("CALDATE_FROM", typeof(string));
                dtRqst.Columns.Add("CALDATE_TO", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = EQSGID;
                dr["MDLLOT_ID"] = MDLLOT_ID; 
                dr["MONTHLY_YN"] = "N";
                dr["CALDATE_FROM"] = dtFromDate.ToString("yyyyMMdd");
                dr["CALDATE_TO"] = dtToDate.ToString("yyyyMMdd");
                dr["LOTTYPE"] = LOTTYPE;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst.Rows.Add(dr);

                DataSet dsRqst = new DataSet();
                dsRqst.Tables.Add(dtRqst);

                DataSet dSRslt = new DataSet();
                int totalCount = (tsDateDiff.Days + 1);

                _sFromDate = dtFromDate;
                _sToDate = dtToDate;

                bgWorker.ReportProgress(0, "[" + dtFromDate.ToString("yyyy-MM-dd") + " ~ "+ dtToDate.ToString("yyyy-MM-dd") + "] - " + ObjectDic.Instance.GetObjectName("WORKING") + ".....");

                dSRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SEL_PROD_PERF_REPORT_BY_DAY_SUMMARY", "INDATA", "AGING,FORMATION1_CHARGE,FORMATION1_CHARGE_REWORK,DEGAS,DEGAS_REWORK,FORMATION2_CHARGE,FORMATION2_CHARGE_REWORK,GRADE,GRADE_REWORK,QUALITY,QUALITY_REWORK,QUALITY_GOOD_REWORK,INSP_REQ,GRADE2,GRADE2_REWORK", dsRqst);

                DataTable dtAging = dSRslt.Tables["AGING"];
                DataTable dtFormation1Charge = dSRslt.Tables["FORMATION1_CHARGE"];
                DataTable dtDegas = dSRslt.Tables["DEGAS"];
                DataTable dtFormation2Charge = dSRslt.Tables["FORMATION2_CHARGE"];
                DataTable dtGrade = dSRslt.Tables["GRADE"];
                DataTable dtQuality = dSRslt.Tables["QUALITY"];
                DataTable dtQualityRework = dSRslt.Tables["QUALITY_REWORK"];
                DataTable dtFormation1Rework = dSRslt.Tables["FORMATION1_CHARGE_REWORK"];
                DataTable dtFormation2Rework = dSRslt.Tables["FORMATION2_CHARGE_REWORK"];
                DataTable dtDegasRework = dSRslt.Tables["DEGAS_REWORK"];
                DataTable dtGradeRework = dSRslt.Tables["GRADE_REWORK"];
                DataTable dtInspReq = dSRslt.Tables["INSP_REQ"];
                DataTable dtGrade2 = dSRslt.Tables["GRADE2"];
                DataTable dtGrade2Rework = dSRslt.Tables["GRADE2_REWORK"];
                DataTable dtQualityGoodRework = dSRslt.Tables["QUALITY_GOOD_REWORK"];

                DataTable dtDistinctMerge = new DataTable();
                dtDistinctMerge.TableName = "RESULT";
                dtDistinctMerge.Columns.Add("CALDATE", typeof(string));
                dtDistinctMerge.Columns.Add("EQSGID", typeof(string));                
                dtDistinctMerge.Columns.Add("MDLLOT_ID", typeof(string));
                
                dtDistinctMerge.Merge(dtAging.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "LINE_NAME", "MDLLOT_ID", "MODEL_NAME" }));
                dtDistinctMerge.Merge(dtFormation1Charge.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "LINE_NAME", "MDLLOT_ID", "MODEL_NAME" }));
                dtDistinctMerge.Merge(dtDegas.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "LINE_NAME", "MDLLOT_ID", "MODEL_NAME" }));
                dtDistinctMerge.Merge(dtFormation2Charge.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "LINE_NAME", "MDLLOT_ID", "MODEL_NAME" }));
                dtDistinctMerge.Merge(dtGrade.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "LINE_NAME", "MDLLOT_ID", "MODEL_NAME" }));
                dtDistinctMerge.Merge(dtQuality.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "LINE_NAME", "MDLLOT_ID", "MODEL_NAME" }));
                dtDistinctMerge.Merge(dtQualityRework.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "LINE_NAME", "MDLLOT_ID", "MODEL_NAME" }));
                dtDistinctMerge.Merge(dtFormation1Rework.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "LINE_NAME", "MDLLOT_ID", "MODEL_NAME" }));
                dtDistinctMerge.Merge(dtFormation2Rework.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "LINE_NAME", "MDLLOT_ID", "MODEL_NAME" }));
                dtDistinctMerge.Merge(dtDegasRework.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "LINE_NAME", "MDLLOT_ID", "MODEL_NAME" }));
                dtDistinctMerge.Merge(dtGradeRework.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "LINE_NAME", "MDLLOT_ID", "MODEL_NAME" }));
                dtDistinctMerge.Merge(dtInspReq.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "LINE_NAME", "MDLLOT_ID", "MODEL_NAME" }));
                dtDistinctMerge.Merge(dtGrade2.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "LINE_NAME", "MDLLOT_ID", "MODEL_NAME" }));
                dtDistinctMerge.Merge(dtGrade2Rework.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID",  "LINE_NAME", "MDLLOT_ID", "MODEL_NAME" }));
                dtDistinctMerge.Merge(dtQualityGoodRework.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "LINE_NAME", "MDLLOT_ID", "MODEL_NAME" }));

                DataTable dtResult = dtDistinctMerge.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "LINE_NAME", "MDLLOT_ID", "MODEL_NAME" });
                dtResult.TableName = "RESULT";

                for (int i = dgProdResult.Columns["MODEL_NAME"].Index + 1; i < dgProdResult.Columns.Count; i++)
                {
                    dtResult.Columns.Add(_dtHeader.Columns[i].ColumnName, typeof(decimal));
                }

                // 2022.12.20 특성 양품 재작업 추가
                if (!dtResult.Columns.Contains("EOL_GREWORK_IN")) dtResult.Columns.Add("EOL_GREWORK_IN", typeof(decimal));
                if (!dtResult.Columns.Contains("EOL_GREWORK_OUT")) dtResult.Columns.Add("EOL_GREWORK_OUT", typeof(decimal));
                if (!dtResult.Columns.Contains("EOL_GREWORK_YEILD")) dtResult.Columns.Add("EOL_GREWORK_YEILD", typeof(decimal));

                SetData(dtResult, dtAging, "AGING_");
                SetData(dtResult, dtFormation1Charge, "1_CHARGE_DEGAS_B_"); //CHARGE1_
                SetData(dtResult, dtDegas, "DEGAS_");  //DEGAS_
                SetData(dtResult, dtFormation2Charge, "2_CHARGE_DEGAS_A_"); //CHARGE2_
                SetData(dtResult, dtGrade, "LOW_CAPA_BAD_PASS_"); //LOWVOLT_
                SetData(dtResult, dtQuality, "EOL_"); //QUALITY_
                SetData(dtResult, dtQualityRework, "EOL_REWORK_"); //QUALITYRE_
                SetData(dtResult, dtFormation1Rework, "1_CHARGE_DEGAS_B_REWORK_");//CHARGE1RE_
                SetData(dtResult, dtFormation2Rework, "2_CHARGE_DEGAS_A_REWORK_"); //CHARGE2RE_
                SetData(dtResult, dtDegasRework, "DEGAS_REWORK_"); //DEGASRE_
                SetData(dtResult, dtGradeRework, "LOW_CAPA_BAD_PASS_REWORK_"); //LOWVOLTRE_
                SetData(dtResult, dtInspReq, "INSP_REQ_");
                SetData(dtResult, dtGrade2, "2_LOW_CAPA_BAD_PASS_"); //LOWVOLT_
                SetData(dtResult, dtGrade2Rework, "2_LOW_CAPA_BAD_PASS_REWORK_"); //LOWVOLTRE_
                SetData(dtResult, dtQualityGoodRework, "EOL_GREWORK_"); //QUALITYGRE_
                
                DataView dv = new DataView(dtResult);
                dv.Sort = "CALDATE ASC, MODEL_NAME ASC, EQSGID ASC";
                dtResult = dv.ToTable();

                for (int i = 0; i < dtResult.Columns.Count; i++)
                {
                    string sMaxLength = dtResult.Columns[i].MaxLength.ToString();
                }

                SetCalculateRow(dtResult);

                //SetBottomRow(dtResult);

                //Summary
                DataTable dtSummary = dtResult.Copy();
                dtSummary.TableName = "SUMMARY";

                //컬럼추가
                dtSummary.Columns.Add("1_CHARGE_DEGAS_B_LOSS_YEILD", typeof(decimal));
                dtSummary.Columns.Add("DEGAS_LOSS_YEILD", typeof(decimal));
                dtSummary.Columns.Add("2_CHARGE_DEGAS_A_LOSS_YEILD", typeof(decimal));
                dtSummary.Columns.Add("LOW_CAPA_BAD_PASS_LOSS_YEILD", typeof(decimal));
                dtSummary.Columns.Add("LOW_CAPA_BAD_PASS_REWORK_LOSS_YEILD", typeof(decimal));

                //20250410 저전압 전체 양품율 추가
                if (!dtSummary.Columns.Contains("LOW_CAPA_BAD_PASS_TOTAL_YEILD")) dtSummary.Columns.Add("LOW_CAPA_BAD_PASS_TOTAL_YEILD", typeof(decimal));

                dtSummary.Columns.Add("EOL_LOSS_YEILD", typeof(decimal));
                dtSummary.Columns.Add("EOL_REWORK_LOSS_YEILD", typeof(decimal));
                dtSummary.Columns.Add("2_LOW_CAPA_BAD_PASS_LOSS_YEILD", typeof(decimal));
                dtSummary.Columns.Add("2_LOW_CAPA_BAD_PASS_REWORK_LOSS_YEILD", typeof(decimal));

                //20250410 2차 저전압 전체 양품율 추가
                if (!dtSummary.Columns.Contains("2_LOW_CAPA_BAD_PASS_TOTAL_YEILD")) dtSummary.Columns.Add("2_LOW_CAPA_BAD_PASS_TOTAL_YEILD", typeof(decimal));

                // 특성 양품 재작업
                if (!dtSummary.Columns.Contains("EOL_GREWORK_IN")) dtSummary.Columns.Add("EOL_GREWORK_IN", typeof(decimal));
                if (!dtSummary.Columns.Contains("EOL_GREWORK_OUT")) dtSummary.Columns.Add("EOL_GREWORK_OUT", typeof(decimal));
                if (!dtSummary.Columns.Contains("EOL_GREWORK_LOSS")) dtSummary.Columns.Add("EOL_GREWORK_LOSS", typeof(decimal));
                if (!dtSummary.Columns.Contains("EOL_GREWORK_YEILD")) dtSummary.Columns.Add("EOL_GREWORK_YEILD", typeof(decimal));
                if (!dtSummary.Columns.Contains("EOL_GREWORK_LOSS_YEILD")) dtSummary.Columns.Add("EOL_GREWORK_LOSS_YEILD", typeof(decimal));

                //전체양품율
                if (!dtSummary.Columns.Contains("EOL_TOTAL_YEILD")) dtSummary.Columns.Add("EOL_TOTAL_YEILD", typeof(decimal));

                for (int i = 0; i < dtSummary.Rows.Count; i++)
                {
                    decimal CHARGE1_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["1_CHARGE_DEGAS_B_IN"].ToString()) ? "0" : dtSummary.Rows[i]["1_CHARGE_DEGAS_B_IN"].ToString());
                    decimal CHARGE1_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["1_CHARGE_DEGAS_B_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["1_CHARGE_DEGAS_B_LOSS"].ToString());
                    dtSummary.Rows[i]["1_CHARGE_DEGAS_B_LOSS_YEILD"] = Math.Round((CHARGE1_IN == 0 ? 0 : (CHARGE1_LOSS == 0 ? 0 : CHARGE1_LOSS / CHARGE1_IN)), 5);

                    decimal DEGAS_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["DEGAS_IN"].ToString()) ? "0" : dtSummary.Rows[i]["DEGAS_IN"].ToString());
                    decimal DEGAS_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["DEGAS_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["DEGAS_LOSS"].ToString());
                    dtSummary.Rows[i]["DEGAS_LOSS_YEILD"] = Math.Round((DEGAS_IN == 0 ? 0 : (DEGAS_LOSS == 0 ? 0 : DEGAS_LOSS / DEGAS_IN)), 5);

                    decimal CHARGE2_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["2_CHARGE_DEGAS_A_IN"].ToString()) ? "0" : dtSummary.Rows[i]["2_CHARGE_DEGAS_A_IN"].ToString());
                    decimal CHARGE2_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["2_CHARGE_DEGAS_A_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["2_CHARGE_DEGAS_A_LOSS"].ToString());

                    dtSummary.Rows[i]["2_CHARGE_DEGAS_A_LOSS_YEILD"] = Math.Round((CHARGE2_IN == 0 ? 0 : (CHARGE2_LOSS == 0 ? 0 : CHARGE2_LOSS / CHARGE2_IN)), 5);


                    decimal LOWVOLT_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_IN"].ToString()) ? "0" : dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_IN"].ToString());
                    decimal LOWVOLT_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_OUT"].ToString()) ? "1" : dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_OUT"].ToString());
                    dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_YEILD"] = Math.Round((LOWVOLT_IN == 0 ? 0 : (LOWVOLT_OUT == 0 ? 0 : LOWVOLT_OUT / LOWVOLT_IN)), 5); //20220725_컬럼명 수정

                    decimal LOWVOLT_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_LOSS"].ToString());
                    dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_LOSS_YEILD"] = Math.Round((LOWVOLT_IN == 0 ? 0 : (LOWVOLT_LOSS == 0 ? 0 : LOWVOLT_LOSS / LOWVOLT_IN)), 5);

                    decimal LOWVOLTRE_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_REWORK_IN"].ToString()) ? "0" : dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_REWORK_IN"].ToString());
                    decimal LOWVOLTRE_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_REWORK_OUT"].ToString()) ? "1" : dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_REWORK_OUT"].ToString());
                    dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_REWORK_YEILD"] = Math.Round((LOWVOLTRE_IN == 0 ? 0 : (LOWVOLTRE_OUT == 0 ? 0 : LOWVOLTRE_OUT / LOWVOLTRE_IN)), 5);

                    decimal LOWVOLTRE_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_REWORK_LOSS"].ToString()) ? "1" : dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_REWORK_LOSS"].ToString());
                    dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_REWORK_LOSS_YEILD"] = Math.Round((LOWVOLTRE_IN == 0 ? 0 : (LOWVOLTRE_LOSS == 0 ? 0 : LOWVOLTRE_LOSS / LOWVOLTRE_IN)), 5);

                    // 2025.04.10 전체양품율 추가
                    //a : LOWVOLT_IN 직행투입
                    //b : LOWVOLT_OUT 직행양품
                    //d : LOWVOLTRE_OUT 재작업양품
                    dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_TOTAL_YEILD"] = Math.Round((LOWVOLT_IN == 0 ? 0 : (((LOWVOLT_OUT + LOWVOLTRE_OUT) == 0 ? 0 : (LOWVOLT_OUT + LOWVOLTRE_OUT) / LOWVOLT_IN))), 5);

                    decimal QUALITY_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_IN"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_IN"].ToString());
                    decimal QUALITY_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_OUT"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_OUT"].ToString());
                    dtSummary.Rows[i]["EOL_YEILD"] = Math.Round((QUALITY_IN == 0 ? 0 : (QUALITY_OUT == 0 ? 0 : QUALITY_OUT / QUALITY_IN)), 5);

                    decimal QUALITY_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_LOSS"].ToString());
                    dtSummary.Rows[i]["EOL_LOSS_YEILD"] = Math.Round((QUALITY_IN == 0 ? 0 : (QUALITY_LOSS == 0 ? 0 : QUALITY_LOSS / QUALITY_IN)), 5);

                    decimal QUALITYRE_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_REWORK_IN"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_REWORK_IN"].ToString());
                    decimal QUALITYRE_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_REWORK_OUT"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_REWORK_OUT"].ToString());
                    dtSummary.Rows[i]["EOL_REWORK_YEILD"] = Math.Round((QUALITYRE_IN == 0 ? 0 : (QUALITYRE_OUT == 0 ? 0 : QUALITYRE_OUT / QUALITYRE_IN)), 5);

                    decimal QUALITYRE_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_REWORK_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_REWORK_LOSS"].ToString());
                    dtSummary.Rows[i]["EOL_REWORK_LOSS_YEILD"] = Math.Round((QUALITYRE_IN == 0 ? 0 : (QUALITYRE_LOSS == 0 ? 0 : QUALITYRE_LOSS / QUALITYRE_IN)), 5);

                    //2차 저전압
                    if (b2LowVoltFlag)
                    {
                        decimal LOWVOLT2_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_IN"].ToString()) ? "0" : dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_IN"].ToString());
                        decimal LOWVOLT2_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_OUT"].ToString()) ? "1" : dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_OUT"].ToString());
                        dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_YEILD"] = Math.Round((LOWVOLT2_IN == 0 ? 0 : (LOWVOLT2_OUT == 0 ? 0 : LOWVOLT2_OUT / LOWVOLT2_IN)), 5);

                        decimal LOWVOLT2_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_LOSS"].ToString());
                        dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_LOSS_YEILD"] = Math.Round((LOWVOLT2_IN == 0 ? 0 : (LOWVOLT2_LOSS == 0 ? 0 : LOWVOLT2_LOSS / LOWVOLT2_IN)), 5);

                        decimal LOWVOLT2RE_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_REWORK_IN"].ToString()) ? "0" : dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_REWORK_IN"].ToString());
                        decimal LOWVOLT2RE_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_REWORK_OUT"].ToString()) ? "1" : dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_REWORK_OUT"].ToString());
                        dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_REWORK_YEILD"] = Math.Round((LOWVOLT2RE_IN == 0 ? 0 : (LOWVOLT2RE_OUT == 0 ? 0 : LOWVOLT2RE_OUT / LOWVOLT2RE_IN)), 5);

                        decimal LOWVOLT2RE_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_REWORK_LOSS"].ToString()) ? "1" : dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_REWORK_LOSS"].ToString());
                        dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_REWORK_LOSS_YEILD"] = Math.Round((LOWVOLT2RE_IN == 0 ? 0 : (LOWVOLT2RE_LOSS == 0 ? 0 : LOWVOLT2RE_LOSS / LOWVOLT2RE_IN)), 5);

                        // 2025.04.10 전체양품율 추가
                        //a : LOWVOLT_IN 직행투입
                        //b : LOWVOLT_OUT 직행양품
                        //d : LOWVOLTRE_OUT 재작업양품
                        dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_TOTAL_YEILD"] = Math.Round((LOWVOLT2_IN == 0 ? 0 : (((LOWVOLT2_OUT + LOWVOLT2RE_OUT) == 0 ? 0 : (LOWVOLT2_OUT + LOWVOLT2RE_OUT) / LOWVOLT2_IN))), 5);
                    }

                    // 2022.12.20 특성 양품 재작업 추가 Start
                    decimal QUALITYGRE_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_GREWORK_IN"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_GREWORK_IN"].ToString());
                    decimal QUALITYGRE_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_GREWORK_OUT"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_GREWORK_OUT"].ToString());
                    dtSummary.Rows[i]["EOL_GREWORK_YEILD"] = Math.Round((QUALITYGRE_IN == 0 ? 0 : (QUALITYGRE_OUT == 0 ? 0 : QUALITYGRE_OUT / QUALITYGRE_IN)), 5);

                    decimal QUALITYGRE_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_GREWORK_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_GREWORK_LOSS"].ToString());
                    dtSummary.Rows[i]["EOL_GREWORK_LOSS_YEILD"] = Math.Round((QUALITYGRE_IN == 0 ? 0 : (QUALITYGRE_LOSS == 0 ? 0 : QUALITYGRE_LOSS / QUALITYGRE_IN)), 5);

                    // 2022.12.22 전체양품율 추가
                    //a : QUALITY_IN 직행투입
                    //b : QUALITY_OUT 직행양품
                    //d : QUALITYRE_OUT 재작업양품
                    dtSummary.Rows[i]["EOL_TOTAL_YEILD"] = Math.Round((QUALITY_IN == 0 ? 0 : (((QUALITY_OUT + QUALITYRE_OUT) == 0 ? 0 : (QUALITY_OUT + QUALITYRE_OUT) / QUALITY_IN))), 5);
                }

                DataSet rtnDataSet = new DataSet();
                rtnDataSet.Tables.Add(dtResult);
                rtnDataSet.Tables.Add(dtSummary);

                bgWorker.ReportProgress(100, "[" + dtFromDate.ToString("yyyy-MM-dd") + " ~ " + dtToDate.ToString("yyyy-MM-dd") + "] - " + ObjectDic.Instance.GetObjectName("WORKING") + ".....");
                Thread.Sleep(1);

                return rtnDataSet;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private void SetData(DataTable dtTo, DataTable dtFrom, string sPrefix)
        {
            for (int i = 0; i < dtFrom.Rows.Count; i++)
            {
                DataRow[] dr = dtTo.Select(" CALDATE = '" + dtFrom.Rows[i]["CALDATE"].ToString() + "' AND EQSGID = '" + dtFrom.Rows[i]["EQSGID"].ToString() + "' AND MDLLOT_ID = '" + dtFrom.Rows[i]["MDLLOT_ID"].ToString() + "'");

                if (dr.Length > 0 && dtTo.Columns.Contains(sPrefix + dtFrom.Rows[i]["CODE"].ToString()))
                {
                    decimal dTmp = 0;
                    decimal dTmp_sub = 0;
                    if (decimal.TryParse(dtFrom.Rows[i]["VALUE"].ToString(), out dTmp))
                    {
                        if(decimal.TryParse(dr[0][sPrefix + dtFrom.Rows[i]["CODE"].ToString()].ToString(), out dTmp_sub)) //동일 CODE 가 존재할 경우, 값을 더해줌
                        {
                            dr[0][sPrefix + dtFrom.Rows[i]["CODE"].ToString()] = dTmp + dTmp_sub;
                        }
                        else
                        {
                            dr[0][sPrefix + dtFrom.Rows[i]["CODE"].ToString()] = dTmp;
                        }
                    }
                    else
                    {
                        dr[0][sPrefix + dtFrom.Rows[i]["CODE"].ToString()] = 0;
                    }
                }
            }
        }

        private void SetCalculateRow(DataTable dt)
        {
            foreach (DataRow dr in dt.Rows)
            {
                // 값이 없을때 0 셋팅.
                foreach (DataColumn col in dt.Columns)
                {
                    if (col.ColumnName.Substring(col.ColumnName.Length - 3, 3).Equals("_IN") ||
                        col.ColumnName.Substring(col.ColumnName.Length - 4, 4).Equals("_OUT") ||
                        col.ColumnName.Substring(col.ColumnName.Length - 6, 6).Equals("_YEILD"))
                    {
                        if (string.IsNullOrEmpty(Util.NVC(dr[col]))) dr[col] = 0;
                    }
                }

                if (!string.IsNullOrEmpty(dr["LOW_CAPA_BAD_PASS_IN"].ToString()) || !string.IsNullOrEmpty(dr["LOW_CAPA_BAD_PASS_OUT"].ToString()))
                {
                    decimal LOWVOLT_IN = decimal.Parse(string.IsNullOrEmpty(dr["LOW_CAPA_BAD_PASS_IN"].ToString()) ? "0" : dr["LOW_CAPA_BAD_PASS_IN"].ToString());
                    decimal LOWVOLT_OUT = decimal.Parse(string.IsNullOrEmpty(dr["LOW_CAPA_BAD_PASS_OUT"].ToString()) ? "0" : dr["LOW_CAPA_BAD_PASS_OUT"].ToString());
                    decimal LOWVOLTRE_OUT = decimal.Parse(string.IsNullOrEmpty(dr["LOW_CAPA_BAD_PASS_REWORK_OUT"].ToString()) ? "0" : dr["LOW_CAPA_BAD_PASS_REWORK_OUT"].ToString());
                    dr["LOW_CAPA_BAD_PASS_TOTAL_YEILD"] = Math.Round((LOWVOLT_IN == 0 ? 0 : (((LOWVOLT_OUT + LOWVOLTRE_OUT) == 0 ? 0 : (LOWVOLT_OUT + LOWVOLTRE_OUT) / LOWVOLT_IN))), 5);
                }

                if (!string.IsNullOrEmpty(dr["2_LOW_CAPA_BAD_PASS_IN"].ToString()) || !string.IsNullOrEmpty(dr["2_LOW_CAPA_BAD_PASS_OUT"].ToString()))
                {
                    decimal LOWVOLT2_IN = decimal.Parse(string.IsNullOrEmpty(dr["2_LOW_CAPA_BAD_PASS_IN"].ToString()) ? "0" : dr["2_LOW_CAPA_BAD_PASS_IN"].ToString());
                    decimal LOWVOLT2_OUT = decimal.Parse(string.IsNullOrEmpty(dr["2_LOW_CAPA_BAD_PASS_OUT"].ToString()) ? "0" : dr["2_LOW_CAPA_BAD_PASS_OUT"].ToString());
                    decimal LOWVOLT2RE_OUT = decimal.Parse(string.IsNullOrEmpty(dr["2_LOW_CAPA_BAD_PASS_REWORK_OUT"].ToString()) ? "0" : dr["2_LOW_CAPA_BAD_PASS_REWORK_OUT"].ToString());
                    dr["2_LOW_CAPA_BAD_PASS_TOTAL_YEILD"] = Math.Round((LOWVOLT2_IN == 0 ? 0 : (((LOWVOLT2_OUT + LOWVOLT2RE_OUT) == 0 ? 0 : (LOWVOLT2_OUT + LOWVOLT2RE_OUT) / LOWVOLT2_IN))), 5);
                }
                
                // 전체양품율
                if (!string.IsNullOrEmpty(dr["EOL_IN"].ToString()) || !string.IsNullOrEmpty(dr["EOL_OUT"].ToString()))
                {
                    decimal QUALITY_IN = decimal.Parse(string.IsNullOrEmpty(dr["EOL_IN"].ToString()) ? "0" : dr["EOL_IN"].ToString());
                    decimal QUALITY_OUT = decimal.Parse(string.IsNullOrEmpty(dr["EOL_OUT"].ToString()) ? "0" : dr["EOL_OUT"].ToString());
                    decimal QUALITYRE_OUT = decimal.Parse(string.IsNullOrEmpty(dr["EOL_REWORK_OUT"].ToString()) ? "0" : dr["EOL_REWORK_OUT"].ToString());
                    dr["EOL_TOTAL_YEILD"] = Math.Round((QUALITY_IN == 0 ? 0 : (((QUALITY_OUT + QUALITYRE_OUT) == 0 ? 0 : (QUALITY_OUT + QUALITYRE_OUT) / QUALITY_IN))), 5);
                }
            }
        }

        /// <summary>
        /// 하단 Summary Row
        /// </summary>
        /// <param name="dt"></param>
        private void SetBottomRow(DataTable dt)
        {

            DataRow dr = dt.NewRow();

            //모델명 - "합계"
            dr["MODEL_NAME"] = ObjectDic.Instance.GetObjectName("합계");

            int colIdx = dt.Columns.IndexOf("AGING_IN");
            for (int i = colIdx; i < dt.Columns.Count; i++)
            {
                if (!dt.Columns[i].ColumnName.Contains("YEILD") &&
                    !dt.Columns[i].ColumnName.Contains("LINE") && !dt.Columns[i].ColumnName.Contains("MODEL"))
                {
                    int sum = 0;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        sum += Util.NVC_Int(dt.Rows[j][i]);
                    }
                    dr[i] = sum;
                }
            }
            // 1차 충전 직행/재작업 Total
            decimal charge1_in = Convert.ToDecimal(dr["1_CHARGE_DEGAS_B_IN"].ToString());
            decimal charge1_out = Convert.ToDecimal(dr["1_CHARGE_DEGAS_B_OUT"].ToString());
            decimal charge1re_in = Convert.ToDecimal(dr["1_CHARGE_DEGAS_B_REWORK_IN"].ToString());
            decimal charge1re_out = Convert.ToDecimal(dr["1_CHARGE_DEGAS_B_REWORK_OUT"].ToString());
            // 디가스 직행/재작업 Total
            decimal degas_in = Convert.ToDecimal(dr["DEGAS_IN"].ToString());
            decimal degas_out = Convert.ToDecimal(dr["DEGAS_OUT"].ToString());
            decimal degasre_in = Convert.ToDecimal(dr["DEGAS_REWORK_IN"].ToString());
            decimal degasre_out = Convert.ToDecimal(dr["DEGAS_REWORK_OUT"].ToString());
            // 2차 충전 직행/재작업 Total
            decimal charge2_in = Convert.ToDecimal(dr["2_CHARGE_DEGAS_A_IN"].ToString());
            decimal charge2_out = Convert.ToDecimal(dr["2_CHARGE_DEGAS_A_OUT"].ToString());
            decimal charge2re_in = Convert.ToDecimal(dr["2_CHARGE_DEGAS_A_REWORK_IN"].ToString());
            decimal charge2re_out = Convert.ToDecimal(dr["2_CHARGE_DEGAS_A_REWORK_OUT"].ToString());
            // 저전압 직행/재작업 Total
            decimal lowvolt_in = Convert.ToDecimal(dr["LOW_CAPA_BAD_PASS_IN"].ToString());
            decimal lowvolt_out = Convert.ToDecimal(dr["LOW_CAPA_BAD_PASS_OUT"].ToString());
            decimal lowvoltre_in = Convert.ToDecimal(dr["LOW_CAPA_BAD_PASS_REWORK_IN"].ToString());
            decimal lowvoltre_out = Convert.ToDecimal(dr["LOW_CAPA_BAD_PASS_REWORK_OUT"].ToString());
            // 특성 직행/재작업 Total
            decimal eol_in = Convert.ToDecimal(dr["EOL_IN"].ToString());
            decimal eol_out = Convert.ToDecimal(dr["EOL_OUT"].ToString());
            decimal eolre_in = Convert.ToDecimal(dr["EOL_REWORK_IN"].ToString());
            decimal eolre_out = Convert.ToDecimal(dr["EOL_REWORK_OUT"].ToString());
            // 특성 양품 재작업 Total
            decimal eolgre_in = Convert.ToDecimal(dr["EOL_GREWORK_IN"].ToString());
            decimal eolgre_out = Convert.ToDecimal(dr["EOL_GREWORK_OUT"].ToString());

            //Total 양품율
            if (charge1_in == 0) dr["1_CHARGE_DEGAS_B_YEILD"] = 0;
            else dr["1_CHARGE_DEGAS_B_YEILD"] = Math.Round(charge1_out / charge1_in, 5);
            if (charge1re_in == 0) dr["1_CHARGE_DEGAS_B_REWORK_YEILD"] = 0;
            else dr["1_CHARGE_DEGAS_B_REWORK_YEILD"] = Math.Round(charge1re_out / charge1re_in, 5);
            if (degas_in == 0) dr["DEGAS_YEILD"] = 0;
            else dr["DEGAS_YEILD"] = Math.Round(degas_out / degas_in, 5);
            if (degasre_in == 0) dr["DEGAS_REWORK_YEILD"] = 0;
            else dr["DEGAS_REWORK_YEILD"] = Math.Round(degasre_out / degasre_in, 5);
            if (charge2_in == 0) dr["2_CHARGE_DEGAS_A_YEILD"] = 0;
            else dr["2_CHARGE_DEGAS_A_YEILD"] = Math.Round(charge2_out / charge2_in, 5);
            if (charge2re_in == 0) dr["2_CHARGE_DEGAS_A_REWORK_YEILD"] = 0;
            else dr["2_CHARGE_DEGAS_A_REWORK_YEILD"] = Math.Round(charge2re_out / charge2re_in, 5);
            if (lowvolt_in == 0) dr["LOW_CAPA_BAD_PASS_YEILD"] = 0;
            else dr["LOW_CAPA_BAD_PASS_YEILD"] = Math.Round(lowvolt_out / lowvolt_in, 5);
            if (lowvoltre_in == 0) dr["LOW_CAPA_BAD_PASS_REWORK_YEILD"] = 0;
            else dr["LOW_CAPA_BAD_PASS_REWORK_YEILD"] = Math.Round(lowvoltre_out / lowvoltre_in, 5);

            // 2025.04.10 전체양품율 추가
            //a : lowvolt_in 직행투입
            //b : lowvolt_out 직행양품
            //d : lowvoltre_out 재작업양품
            if (eol_in == 0) dr["LOW_CAPA_BAD_PASS_TOTAL_YEILD"] = 0;
            else dr["LOW_CAPA_BAD_PASS_TOTAL_YEILD"] = Math.Round((lowvolt_in == 0 ? 0 : ((lowvolt_out + lowvoltre_out) == 0 ? 0 : (lowvolt_out + lowvoltre_out) / lowvolt_in)), 5);

            if (eol_in == 0) dr["EOL_YEILD"] = 0;
            else dr["EOL_YEILD"] = Math.Round(eol_out / eol_in, 5);
            if (eolre_in == 0) dr["EOL_REWORK_YEILD"] = 0;
            else dr["EOL_REWORK_YEILD"] = Math.Round(eolre_out / eolre_in, 5);

            // 2차 저전압 직행/재작업 Total
            if (b2LowVoltFlag)
            {
                decimal lowvolt2_in = Convert.ToDecimal(dr["2_LOW_CAPA_BAD_PASS_IN"].ToString());
                decimal lowvolt2_out = Convert.ToDecimal(dr["2_LOW_CAPA_BAD_PASS_OUT"].ToString());
                decimal lowvolt2re_in = Convert.ToDecimal(dr["2_LOW_CAPA_BAD_PASS_REWORK_IN"].ToString());
                decimal lowvolt2re_out = Convert.ToDecimal(dr["2_LOW_CAPA_BAD_PASS_REWORK_OUT"].ToString());

                if (lowvolt2_in == 0) dr["2_LOW_CAPA_BAD_PASS_YEILD"] = 0;
                else dr["2_LOW_CAPA_BAD_PASS_YEILD"] = Math.Round(lowvolt2_out / lowvolt2_in, 5);
                if (lowvolt2re_in == 0) dr["2_LOW_CAPA_BAD_PASS_REWORK_YEILD"] = 0;
                else dr["2_LOW_CAPA_BAD_PASS_REWORK_YEILD"] = Math.Round(lowvolt2re_out / lowvolt2re_in, 5);

                // 2025.04.10 전체양품율 추가
                //a : lowvolt2_in 직행투입
                //b : lowvolt2_out 직행양품
                //d : lowvolt2re_out 재작업양품
                if (eol_in == 0) dr["2_LOW_CAPA_BAD_PASS_TOTAL_YEILD"] = 0;
                else dr["2_LOW_CAPA_BAD_PASS_TOTAL_YEILD"] = Math.Round((lowvolt2_in == 0 ? 0 : ((lowvolt2_out + lowvolt2re_out) == 0 ? 0 : (lowvolt2_out + lowvolt2re_out) / lowvolt2_in)), 5);
            }

            // 특성 양품 재작업 Total
            if (eolgre_in == 0) dr["EOL_GREWORK_YEILD"] = 0;
            else dr["EOL_GREWORK_YEILD"] = Math.Round(eolgre_out / eolgre_in, 5);

            // 2022.12.22 전체양품율 추가
            //a : eol_in 직행투입
            //b : eol_out 직행양품
            //d : eolre_out 재작업양품
            if (eol_in == 0) dr["EOL_TOTAL_YEILD"] = 0;
            else dr["EOL_TOTAL_YEILD"] = Math.Round((eol_in == 0 ? 0 : ((eol_out + eolre_out) == 0 ? 0 : (eol_out + eolre_out) / eol_in)), 5);

            dt.Rows.Add(dr);
        }

        public static List<ResultElement> CheckBoxList(DataTable dt)
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
                        Control = new UcBaseCheckBox()
                        {
                            Name = "chk" + Util.NVC(dr["COM_CODE"]),
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Center,
                            Content = ObjectDic.Instance.GetObjectName(Util.NVC(dr["COM_CODE_NAME"]))
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

        private void SetLineCombo(MultiSelectionBox mcb)
        {
            try
            {
                DataTable dtRqstA = new DataTable();
                dtRqstA.TableName = "RQSTDT";
                dtRqstA.Columns.Add("LANGID", typeof(string));
                dtRqstA.Columns.Add("AREAID", typeof(string));

                DataRow drA = dtRqstA.NewRow();
                drA["LANGID"] = LoginInfo.LANGID;
                drA["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqstA.Rows.Add(drA);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE", "RQSTDT", "RSLTDT", dtRqstA);

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

        private void SetLineModel(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = cboLine.GetBindValue();
                //!string.IsNullOrEmpty((string)cboLine.GetBindValue());    
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE_MULTI_MODEL", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }
        #endregion

        #region [Event]

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            btnSearch.IsEnabled = false;

            object[] argument = new object[5];
            argument[0] = dtpSearchDate.SelectedFromDateTime;
            argument[1] = dtpSearchDate.SelectedToDateTime;
            argument[2] = cboLine.GetBindValue();
            argument[3] = Util.GetCondition(cboModel, bAllNull: true);

            int iItemCnt = (cboLotType.ItemsSource == null ? 0 : ((DataView)cboLotType.ItemsSource).Count);
            int iSelectedCnt = (cboLotType.ItemsSource == null ? 0 : cboLotType.SelectedItemsToString.Split(',').Length);

            argument[4] = (iItemCnt == iSelectedCnt ? null : Util.ConvertEmptyToNull(cboLotType.SelectedItemsToString));
            
            if (!bgWorker.IsBusy)
            {
                bgWorker.RunWorkerAsync(argument);

                if (chkSummary.IsChecked.Equals(true))
                {
                    dgProdResultSummary.LoadingIndicatorStart(ObjectDic.Instance.GetObjectName("LOADING"));
                }
                else
                {
                    dgProdResult.LoadingIndicatorStart(ObjectDic.Instance.GetObjectName("LOADING"));
                }
            }

     
        }

        private void chk_Checked(object sender, EventArgs e)
        {
            CheckBox chk = (CheckBox)sender;

            if (chk.Name == "chkAll")
            {

            }
            else if (chk.Name == "chk1CHARGE")
            {
                for (int i = dgProdResult.Columns["1_CHARGE_DEGAS_B_IN"].Index; i <= dgProdResult.Columns["1_CHARGE_DEGAS_B_LOSS"].Index; i++)
                {
                    dgProdResult.SetColumnVisible(i, Visibility.Visible);
                }
            }
            else if (chk.Name == "chk1CHARGERE")
            {
                for (int i = dgProdResult.Columns["1_CHARGE_DEGAS_B_REWORK_IN"].Index; i <= dgProdResult.Columns["1_CHARGE_DEGAS_B_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.SetColumnVisible(i, Visibility.Visible);
                }
            }
            else if (chk.Name == "chkDEGAS")
            {
                for (int i = dgProdResult.Columns["DEGAS_IN"].Index; i <= dgProdResult.Columns["DEGAS_LOSS"].Index; i++)
                {
                    dgProdResult.SetColumnVisible(i, Visibility.Visible);
                }
            }
            else if (chk.Name == "chkDEGASRE")
            {
                for (int i = dgProdResult.Columns["DEGAS_REWORK_IN"].Index; i <= dgProdResult.Columns["DEGAS_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.SetColumnVisible(i, Visibility.Visible);
                }
            }
            else if (chk.Name == "chk2CHARGE")
            {
                for (int i = dgProdResult.Columns["2_CHARGE_DEGAS_A_IN"].Index; i <= dgProdResult.Columns["2_CHARGE_DEGAS_A_LOSS"].Index; i++)
                {
                    dgProdResult.SetColumnVisible(i, Visibility.Visible);
                }
            }
            else if (chk.Name == "chk2CHARGERE")
            {
                for (int i = dgProdResult.Columns["2_CHARGE_DEGAS_A_REWORK_IN"].Index; i <= dgProdResult.Columns["2_CHARGE_DEGAS_A_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.SetColumnVisible(i, Visibility.Visible);
                }
            }
            else if (chk.Name == "chkLOWVOLT")
            {
                for (int i = dgProdResult.Columns["LOW_CAPA_BAD_PASS_TOTAL_YEILD"].Index; i <= dgProdResult.Columns["LOW_CAPA_BAD_PASS_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.SetColumnVisible(i, Visibility.Visible);
                }
            }
            else if (chk.Name == "chkEOL")
            {
                for (int i = dgProdResult.Columns["EOL_TOTAL_YEILD"].Index; i <= dgProdResult.Columns["EOL_LOSS"].Index; i++)
                {
                    dgProdResult.SetColumnVisible(i, Visibility.Visible);
                }
            }
            else if (chk.Name == "chkEOLRE")
            {
                for (int i = dgProdResult.Columns["EOL_REWORK_IN"].Index; i <= dgProdResult.Columns["EOL_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.SetColumnVisible(i, Visibility.Visible);
                }
            }
            //제품검사의뢰 수량
            else if (chk.Name == "chkINSP_REQ")
            {
                for (int i = dgProdResult.Columns["INSP_REQ_LQC_DEGAS"].Index; i <= dgProdResult.Columns["INSP_REQ_PQC_ETC"].Index; i++)
                {
                    dgProdResult.SetColumnVisible(i, Visibility.Visible);
                }
            }
            //2차 저전압
            else if (chk.Name == "chk2LOWVOLT")
            {
                for (int i = dgProdResult.Columns["2_LOW_CAPA_BAD_PASS_TOTAL_YEILD"].Index; i <= dgProdResult.Columns["2_LOW_CAPA_BAD_PASS_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.SetColumnVisible(i, Visibility.Visible);
                }
            }
            //특성양품재작업
            else if (chk.Name == "chkEOLGRE")
            {
                for (int i = dgProdResult.Columns["EOL_GREWORK_IN"].Index; i <= dgProdResult.Columns["EOL_GREWORK_LOSS"].Index; i++)
                {
                    dgProdResult.SetColumnVisible(i, Visibility.Visible);
                }
            }
        }

        private void chk_UnChecked(object sender, EventArgs e)
        {
            CheckBox chk = (CheckBox)sender;

            if (chk.Name == "chkAll")
            {

            }
            else if (chk.Name == "chk1CHARGE")
            {
                for (int i = dgProdResult.Columns["1_CHARGE_DEGAS_B_IN"].Index; i <= dgProdResult.Columns["1_CHARGE_DEGAS_B_LOSS"].Index; i++)
                {                    
                    dgProdResult.SetColumnVisible(i, Visibility.Collapsed);
                }
            }
            else if (chk.Name == "chk1CHARGERE")
            {
                for (int i = dgProdResult.Columns["1_CHARGE_DEGAS_B_REWORK_IN"].Index; i <= dgProdResult.Columns["1_CHARGE_DEGAS_B_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.SetColumnVisible(i, Visibility.Collapsed);
                }
            }
            else if (chk.Name == "chkDEGAS")
            {
                for (int i = dgProdResult.Columns["DEGAS_IN"].Index; i <= dgProdResult.Columns["DEGAS_LOSS"].Index; i++)
                {
                    dgProdResult.SetColumnVisible(i, Visibility.Collapsed);
                }
            }
            else if (chk.Name == "chkDEGASRE")
            {
                for (int i = dgProdResult.Columns["DEGAS_REWORK_IN"].Index; i <= dgProdResult.Columns["DEGAS_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.SetColumnVisible(i, Visibility.Collapsed);
                }
            }
            else if (chk.Name == "chk2CHARGE")
            {
                for (int i = dgProdResult.Columns["2_CHARGE_DEGAS_A_IN"].Index; i <= dgProdResult.Columns["2_CHARGE_DEGAS_A_LOSS"].Index; i++)
                {
                    dgProdResult.SetColumnVisible(i, Visibility.Collapsed);
                }
            }
            else if (chk.Name == "chk2CHARGERE")
            {
                for (int i = dgProdResult.Columns["2_CHARGE_DEGAS_A_REWORK_IN"].Index; i <= dgProdResult.Columns["2_CHARGE_DEGAS_A_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.SetColumnVisible(i, Visibility.Collapsed);
                }
            }
            else if (chk.Name == "chkLOWVOLT")
            {
                for (int i = dgProdResult.Columns["LOW_CAPA_BAD_PASS_TOTAL_YEILD"].Index; i <= dgProdResult.Columns["LOW_CAPA_BAD_PASS_REWORK_LOSS"].Index; i++)

                {
                    dgProdResult.SetColumnVisible(i, Visibility.Collapsed);
                }
            }
            else if (chk.Name == "chkEOL")
            {
                for (int i = dgProdResult.Columns["EOL_TOTAL_YEILD"].Index; i <= dgProdResult.Columns["EOL_LOSS"].Index; i++)
                {
                    dgProdResult.SetColumnVisible(i, Visibility.Collapsed);
                }
            }
            else if (chk.Name == "chkEOLRE")
            {
                for (int i = dgProdResult.Columns["EOL_REWORK_IN"].Index; i <= dgProdResult.Columns["EOL_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.SetColumnVisible(i, Visibility.Collapsed);
                }
            }
            //제품검사의뢰
            else if (chk.Name == "chkINSP_REQ")
            {
                for (int i = dgProdResult.Columns["INSP_REQ_LQC_DEGAS"].Index; i <= dgProdResult.Columns["INSP_REQ_PQC_ETC"].Index; i++)
                {
                    dgProdResult.SetColumnVisible(i, Visibility.Collapsed);
                }
            }
            //2차 저전압 조건 추가 START
            else if (chk.Name == "chk2LOWVOLT")
            {
                for (int i = dgProdResult.Columns["2_LOW_CAPA_BAD_PASS_TOTAL_YEILD"].Index; i <= dgProdResult.Columns["2_LOW_CAPA_BAD_PASS_REWORK_LOSS"].Index; i++)

                {
                    dgProdResult.SetColumnVisible(i, Visibility.Collapsed);
                }
            }
            //특성양품재작업
            else if (chk.Name == "chkEOLGRE")
            {
                for (int i = dgProdResult.Columns["EOL_GREWORK_IN"].Index; i <= dgProdResult.Columns["EOL_GREWORK_LOSS"].Index; i++)
                {
                    dgProdResult.SetColumnVisible(i, Visibility.Collapsed);
                }
            }
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

        private void cboLotType_SelectionChanged(object sender, EventArgs e)
        {
            if (cboLotType.SelectedItems.Count == 0)
            {
                cboLotType.CheckAll();
            }
        }

        private void cboLine_Loaded(object sender, RoutedEventArgs e)
        {
            if (cboLine.ItemsSource == null) SetLineModel(cboModel);
        }

        private void cboLine_SelectionChanged(object sender, EventArgs e)
        {
            if(cboLine.SelectedItems.Count == 0)
            {
                cboLine.CheckAll();
            }
        }

        private void cboLine_DropDownClosed(object sender)
        {
            if (sender == null) return;
            SetLineModel(cboModel);
        }

        private void dgProdResultSummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null || e.Cell.Presenter == null) return;

                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                dg.Dispatcher.BeginInvoke(new Action(() =>
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);

                    if (e.Cell.Row.Index == dg.Rows.Count - 1)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF7E9D5"));
                    }
                }));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

            if (e.Row.Index + 1 - dataGrid.TopRows.Count > 0 && e.Row.Index != dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private void dgProdResult_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null || e.Cell.Presenter == null) return;

            C1DataGrid dg = sender as C1DataGrid;

            if (e.Cell.Column.Name.ToString().Equals("CALDATE") || e.Cell.Column.Name.ToString().Equals("LINE_NAME")
                || e.Cell.Column.Name.ToString().Equals("MDLLOT_ID") || e.Cell.Column.Name.ToString().Equals("MODEL_NAME")
                || e.Cell.Column.Name.ToString().Equals("AGING_IN") || e.Cell.Column.Name.ToString().Equals("INSP_REQ")
                || e.Cell.Column.Name.Contains("IN") || e.Cell.Column.Name.Contains("YEILD")
                || e.Cell.Column.Name.Contains("OUT") || e.Cell.Column.Name.Contains("LOSS"))
            {
                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                e.Cell.Presenter.FontWeight = FontWeights.Normal;
            }

            else
            {
                if (!string.IsNullOrEmpty(Util.NVC(e.Cell.Value)))
                {
                    e.Cell.Presenter.Foreground = Brushes.Blue;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                }
            }
        }

        private void BgWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            e.Result = GetList(e.Argument);
        }

        private void BgWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 100)
            {
                if (chkSummary.IsChecked.Equals(true))
                {
                    dgProdResultSummary.LoadingIndicatorStart(ObjectDic.Instance.GetObjectName("APPLYING"));
                }
                else
                {
                    dgProdResult.LoadingIndicatorStart(ObjectDic.Instance.GetObjectName("APPLYING"));
                }
            }
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
                        if (dsData.Tables.Contains("RESULT")) dgProdResult.ItemsSource = DataTableConverter.Convert(dsData.Tables["RESULT"]);
                        if (dsData.Tables.Contains("SUMMARY")) dgProdResultSummary.ItemsSource = DataTableConverter.Convert(dsData.Tables["SUMMARY"]);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    dgProdResultSummary.LoadingIndicatorStop();
                    dgProdResult.LoadingIndicatorStop();

                    btnSearch.IsEnabled = true;
                }));
            }
        }

        private void dgProdResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);
            if(cell == null)
            {
                if (datagrid.CurrentCell == null || datagrid.CurrentRow == null || datagrid.CurrentColumn == null) return;
                if (datagrid.CurrentRow.Type != DataGridRowType.Bottom) return;
                cell = datagrid.CurrentCell;
            }

            else
            {
                if (cell.Row.Type != DataGridRowType.Item) return;
            }

           if (string.IsNullOrEmpty(Util.NVC(cell.Text)) || Util.NVC(cell.Text).Equals("0")) return;

            TimeSpan Datediff = _sToDate.Date - _sFromDate.Date;
            if ((!string.IsNullOrEmpty(Util.NVC(cell.Text)) || !Util.NVC(cell.Text).Equals("0") )&& Datediff.Days > 10)
            {   
                //조회기간은 10일을 초과 할 수 없습니다.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0511"), null, "info", MessageBoxButton.OK, MessageBoxIcon.Warning, isAutoClosing: true);
                return;
            }

            FCS001_054_DFCT_CELL_LIST wndPopup = new FCS001_054_DFCT_CELL_LIST();
            wndPopup.FrameOperation = FrameOperation;

            if(wndPopup != null)
            {
                int row = cell.Row.Index;
                object[] Parameters = new object[14];

                #region [FORM_RSLT_SUM_GR_CODE]
                string[] sColumnName = new string[11];
                string sFormrsltgrcode = string.Empty;
                string sRwkFlag = string.Empty;
                string sWRKLOG_TYPE_CODE = string.Empty;


                sColumnName = cell.Column.Name.Split('_');

                if(sColumnName[0].Equals("2"))
                {
                    if(sColumnName[1].Equals("CHARGE"))
                    {
                        if (sColumnName[4].Equals("REWORK"))
                        {
                            sRwkFlag = "RWK";
                        }
                        else
                        {
                            sRwkFlag = "FRST";
                        }
                        sFormrsltgrcode = "AFTER_DEGAS_FORMEQPT_SELECTOR_MOVE"; //2차 충전/ 2차 충전 재작업 
                        sWRKLOG_TYPE_CODE = "B";

                    }
                    else if(sColumnName[1].Equals("LOW"))
                    {
                        if(sColumnName[5].Equals("REWORK"))
                        {
                            sFormrsltgrcode = "AFTER_DEGAS_OCV2_SELECTOR_RWK_MOVE"; //2차 저전압 재작업
                            sRwkFlag = "RWK";
                        }
                        else
                        {
                            sFormrsltgrcode = "AFTER_DEGAS_OCV2_SELECTOR_FRST_MOVE"; //2차 저전압
                            sRwkFlag = "FRST";
                        }
                        sWRKLOG_TYPE_CODE = "L";
                    }
                }
                else if(sColumnName[0].Equals("1"))
                {
                    if(sColumnName[4].Equals("REWORK"))
                    {
                        sRwkFlag = "RWK";
                    }
                    else
                    {
                        sRwkFlag = "FRST";
                    }
                    sFormrsltgrcode = "PRE_DEGAS_FORMEQPT_SELECTOR_MOVE"; //1차 충전/ 1차 충전 재작업 
                    sWRKLOG_TYPE_CODE = "A";
                }
                else if (sColumnName[0].Equals("DEGAS"))
                {
                    if(sColumnName[1].Equals("REWORK"))
                    {
                        sFormrsltgrcode = "DGS_RWK_MOVE"; //Degas 재작업
                        sRwkFlag = "RWK";
                    }
                    else
                    {
                        sFormrsltgrcode = "DGS_FRST_MOVE"; //Degas 
                        sRwkFlag = "FRST";
                    }
                    sWRKLOG_TYPE_CODE = "D";
                }
                else if(sColumnName[0].Equals("EOL"))
                {
                    if(sColumnName[1].Equals("REWORK"))
                    {
                        sFormrsltgrcode = "EOL_RWK_MOVE"; // EOL 재작업
                        sRwkFlag = "RWK";
                    }
                    else
                    {
                        sFormrsltgrcode = "EOL_FRST_MOVE"; //EOL
                        sRwkFlag = "FRST";
                    }
                    sWRKLOG_TYPE_CODE = "Q";
                }
                else if (sColumnName[0].Equals("LOW"))
                {
                    if (sColumnName[4].Equals("REWORK"))
                    {
                        sFormrsltgrcode = "AFTER_DEGAS_OCV_SELECTOR_RWK_MOVE"; //1차 저전압 재작업
                        sRwkFlag = "RWK";
                    }
                    else
                    {
                        sFormrsltgrcode = "AFTER_DEGAS_OCV_SELECTOR_FRST_MOVE"; // 1차 저전압
                        sRwkFlag = "FRST";
                    }
                    sWRKLOG_TYPE_CODE = "G";
                }
                else
                {
                    sFormrsltgrcode = "";
                    sRwkFlag = "FRST";
                }

                #endregion

                if (datagrid.CurrentRow.Type ==  DataGridRowType.Bottom)
                {
                    for(int i = 2; i <= 7; i++)
                    {
                        Parameters[i] = "";
                    }

                    Parameters[0] = ""; //WORK_DATE
                    Parameters[3] = cboLine.GetBindValue();
                    Parameters[12] = dtpSearchDate.SelectedFromDateTime.ToString("yyyy-MM-dd");//FROM_DATE
                    Parameters[13] = dtpSearchDate.SelectedToDateTime.ToString("yyyy-MM-dd"); //TO_DATE
                }

                else
                {
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "CALDATE"));//CALDATE
                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "EQSGID")); //EQSGID   
                    Parameters[4] = ""; //EQPTID       
                }

                Parameters[1] = ""; //SHFT_ID
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "MDLLOT_ID")); //MDLLOT_ID
                Parameters[6] = cboLotType.GetBindValue(); //LOTTYPE
                Parameters[7] = sColumnName[sColumnName.Count() - 1];  // DFCT_CODE 
                Parameters[8] = string.IsNullOrEmpty(sWRKLOG_TYPE_CODE) ? "" : sWRKLOG_TYPE_CODE; //RWK_FLAG;  //WRKLOG_TYPE_CODE (*)
                Parameters[9] = string.IsNullOrEmpty(sRwkFlag) ? "FRST" : sRwkFlag; //WRK_TYPE (*)
                Parameters[10] = ""; // ROUT_TYPE_CODE
                Parameters[11] = ""; // LOTCOMENT
                C1WindowExtension.SetParameters(wndPopup, Parameters);
                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            FCS001_054_DFCT_CELL_LIST window = sender as FCS001_054_DFCT_CELL_LIST;
        }

        #endregion
    }
}
