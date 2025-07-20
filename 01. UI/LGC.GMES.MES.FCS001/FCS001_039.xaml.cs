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


namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_039 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;
        private bool b2LowVoltFlag = false; //20220725_C20220603-000198_2차 저전압 조건 추가

        private DataTable _dtHeader;
        private DataTable _dtHeaderSummary; //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동)
        private DataTable dtTemp; //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동)

        //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) START
        public class ResultElement
        {
            public CheckBox chkBox = null;
            public string Title = string.Empty;
            public Control Control;
            public bool Visibility = true;
            public int SpaceInCharge = 1;
        }
        //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) END

        System.ComponentModel.BackgroundWorker bgWorker = null;

        #endregion

        #region [Initialize]
        public FCS001_039()
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

            Set_CheckBox(); //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동)

            InitSpread();
            InitSpreadSummary(); //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동)

            chkSummary.Checked += chkSummary_Checked;
            chkSummary.Unchecked += chkSummary_UnChecked;

            chkAll.Checked += chkAll_Checked;
            chkAll.Unchecked += chkAll_Unchecked;

            SetLotTypeCombo(cboLotType);

            //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) START
            //chk1Charge.Checked += chk_Checked;
            //chk1Charge.Unchecked += chk_UnChecked;
            //chk1ChargeRe.Checked += chk_Checked;
            //chk1ChargeRe.Unchecked += chk_UnChecked;
            //chkDegas.Checked += chk_Checked;
            //chkDegas.Unchecked += chk_UnChecked;
            //chkDegasRe.Checked += chk_Checked;
            //chkDegasRe.Unchecked += chk_UnChecked;
            //chk2Charge.Checked += chk_Checked;
            //chk2Charge.Unchecked += chk_UnChecked;
            //chk2ChargeRe.Checked += chk_Checked;
            //chk2ChargeRe.Unchecked += chk_UnChecked;
            //chkLowVolt.Checked += chk_Checked;
            //chkLowVolt.Unchecked += chk_UnChecked;
            //chkEol.Checked += chk_Checked;
            //chkEol.Unchecked += chk_UnChecked;
            //chkEolRework.Checked += chk_Checked;
            //chkEolRework.Unchecked += chk_UnChecked;
            //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) END

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
            CommonCombo_Form _combo = new CommonCombo_Form();

            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent);

            // Lot 유형
            // _combo.SetCombo(cboLotType, CommonCombo_Form.ComboStatus.ALL, sCase: "LOTTYPE"); // 2021.08.19 Lot 유형 검색조건 추가
        }

        private void InitControl()
        {
            // Util 에 해당 함수 추가 필요.
            dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-1);
            dtpToDate.SelectedDateTime = DateTime.Now;

            _dtHeader = new DataTable();
            _dtHeaderSummary = new DataTable(); //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동)
            dtTemp = new DataTable(); //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동)

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
            FixedMultiHeader("WORK_DATE|WORK_DATE|WORK_DATE", "CALDATE", false, false, 100);
            //FixedMultiHeader("LINE_ID|LINE_ID|LINE_ID", "LINE_ID");
            FixedMultiHeader("LINE_ID|LINE_ID|LINE_ID", "LINE_NAME", false, false, 150);
            FixedMultiHeader("MODEL_ID|MODEL_ID|MODEL_ID", "MDLLOT_ID", false, false);
            FixedMultiHeader("MODEL_NAME|MODEL_NAME|MODEL_NAME", "MODEL_NAME", false, false);
            FixedMultiHeader("PRE_AGING|PRE_AGING|AGING_INPUT", "AGING_IN", false, false);

            //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) START
            ////1차 충전(Degas 전)
            //FixedMultiHeader("1_CHARGE_DEGAS_B|PERF|INPUT", "1_CHARGE_DEGAS_B_IN", false, false);
            //FixedMultiHeader("1_CHARGE_DEGAS_B|PERF|GOOD_PRD", "1_CHARGE_DEGAS_B_OUT", false, false);
            //FixedMultiHeader("1_CHARGE_DEGAS_B|PERF|GOOD_RATE", "1_CHARGE_DEGAS_B_YEILD", true, false);

            //AddDefectHeader(dgProdResult, "A", "1_CHARGE_DEGAS_B", "1_CHARGE_DEGAS_B_", 75);
            //FixedMultiHeader("1_CHARGE_DEGAS_B|TOTAL|DEFECT", "1_CHARGE_DEGAS_B_LOSS", false, false);

            ////1차 충전(Degas 전) 재작업
            //FixedMultiHeader("1_CHARGE_DEGAS_B_REWORK|PERF|INPUT", "1_CHARGE_DEGAS_B_REWORK_IN", false, false);
            //FixedMultiHeader("1_CHARGE_DEGAS_B_REWORK|PERF|GOOD_PRD", "1_CHARGE_DEGAS_B_REWORK_OUT", false, false);
            //FixedMultiHeader("1_CHARGE_DEGAS_B_REWORK|PERF|GOOD_RATE", "1_CHARGE_DEGAS_B_REWORK_YEILD", true, false);

            //AddDefectHeader(dgProdResult, "A", "1_CHARGE_DEGAS_B_REWORK", "1_CHARGE_DEGAS_B_REWORK_", 75, "B");
            //FixedMultiHeader("1_CHARGE_DEGAS_B_REWORK|TOTAL|DEFECT", "1_CHARGE_DEGAS_B_REWORK_LOSS", false, false);

            ////Dgas
            //FixedMultiHeader("DEGAS|PERF|INPUT", "DEGAS_IN", false, false);
            //FixedMultiHeader("DEGAS|PERF|GOOD_PRD", "DEGAS_OUT", false, false);
            ////FixedMultiHeader("DEGAS|PERF|YIELD", "DEGAS_YEILD2");
            //FixedMultiHeader("DEGAS|PERF|GOOD_RATE", "DEGAS_YEILD", true, false);

            //AddDefectHeader(dgProdResult, "D", "DEGAS", "DEGAS_");
            //FixedMultiHeader("DEGAS|TOTAL|DEFECT", "DEGAS_LOSS", false, false);

            ////Dgas 재작업
            //FixedMultiHeader("DEGAS_REWORK|PERF|INPUT", "DEGAS_REWORK_IN", false, false);
            //FixedMultiHeader("DEGAS_REWORK|PERF|GOOD_PRD", "DEGAS_REWORK_OUT", false, false);
            ////FixedMultiHeader("DEGAS_REWORK|PERF|YIELD", "DEGASRE_YEILD2");
            //FixedMultiHeader("DEGAS_REWORK|PERF|GOOD_RATE", "DEGAS_REWORK_YEILD", true, false);

            //AddDefectHeader(dgProdResult, "D", "DEGAS_REWORK", "DEGAS_REWORK_");
            //FixedMultiHeader("DEGAS_REWORK|TOTAL|DEFECT", "DEGAS_REWORK_LOSS", false, false);

            ////2차충전(Dgas 후)
            //FixedMultiHeader("2_CHARGE_DEGAS_A|PERF|INPUT", "2_CHARGE_DEGAS_A_IN", false, false);
            //FixedMultiHeader("2_CHARGE_DEGAS_A|PERF|GOOD_PRD", "2_CHARGE_DEGAS_A_OUT", false, false);
            //FixedMultiHeader("2_CHARGE_DEGAS_A|PERF|GOOD_RATE", "2_CHARGE_DEGAS_A_YEILD", true, false);

            //AddDefectHeader(dgProdResult, "B", "2_CHARGE_DEGAS_A", "2_CHARGE_DEGAS_A_", 75);
            //FixedMultiHeader("2_CHARGE_DEGAS_A|TOTAL|DEFECT", "2_CHARGE_DEGAS_A_LOSS", false, false);

            ////2차충전(Dgas 후) 재작업
            //FixedMultiHeader("2_CHARGE_DEGAS_A_REWORK|PERF|INPUT", "2_CHARGE_DEGAS_A_REWORK_IN", false, false);
            //FixedMultiHeader("2_CHARGE_DEGAS_A_REWORK|PERF|GOOD_PRD", "2_CHARGE_DEGAS_A_REWORK_OUT", false, false);
            //FixedMultiHeader("2_CHARGE_DEGAS_A_REWORK|PERF|GOOD_RATE", "2_CHARGE_DEGAS_A_REWORK_YEILD", true, false);

            //AddDefectHeader(dgProdResult, "B", "2_CHARGE_DEGAS_A_REWORK", "2_CHARGE_DEGAS_A_REWORK_", 75, "B");
            //FixedMultiHeader("2_CHARGE_DEGAS_A_REWORK|TOTAL|DEFECT", "2_CHARGE_DEGAS_A_REWORK_LOSS", false, false);

            ////저전압
            ////FixedMultiHeader("LOW_CAPA_BAD_PASS|PERF|TOTAL_YIELD", "LOWVOLT_TOTAL_YEILD");
            //FixedMultiHeader("LOW_CAPA_BAD_PASS|PERF|INPUT", "LOW_CAPA_BAD_PASS_IN", false, false);
            //FixedMultiHeader("LOW_CAPA_BAD_PASS|PERF|GOOD_PRD", "LOW_CAPA_BAD_PASS_OUT", false, false);
            //FixedMultiHeader("LOW_CAPA_BAD_PASS|PERF|GOOD_RATE", "LOW_CAPA_BAD_PASS_YEILD", true, false);

            //AddDefectHeader(dgProdResult, "G", "LOW_CAPA_BAD_PASS", "LOW_CAPA_BAD_PASS_", 75); //Width 추가
            //FixedMultiHeader("LOW_CAPA_BAD_PASS|TOTAL|DEFECT", "LOW_CAPA_BAD_PASS_LOSS", false, false);

            ////저전압 재작업 2021.06.01 추가
            //FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|PERF|INPUT", "LOW_CAPA_BAD_PASS_REWORK_IN", false, false);
            //FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|PERF|GOOD_PRD", "LOW_CAPA_BAD_PASS_REWORK_OUT", false, false);
            //FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|PERF|GOOD_RATE", "LOW_CAPA_BAD_PASS_REWORK_YEILD", true, false);

            //AddDefectHeader(dgProdResult, "G", "LOW_CAPA_BAD_PASS_REWORK", "LOW_CAPA_BAD_PASS_REWORK", 75); //Width 추가
            //FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|TOTAL|DEFECT", "LOW_CAPA_BAD_PASS_REWORK_LOSS", false, false, false);

            ////특성
            //FixedMultiHeader("EOL|PERF|INPUT", "EOL_IN", false, false);
            //FixedMultiHeader("EOL|PERF|GOOD_PRD", "EOL_OUT", false, false);
            //FixedMultiHeader("EOL|PERF|GOOD_RATE", "EOL_YEILD", true, false);

            //AddDefectHeader(dgProdResult, "5", "EOL", "EOL_");
            //FixedMultiHeader("EOL|TOTAL|DEFECT", "EOL_LOSS", false, false);

            ////특성 재작업
            //FixedMultiHeader("EOL_REWORK|PERF|INPUT", "EOL_REWORK_IN", false, false);
            //FixedMultiHeader("EOL_REWORK|PERF|GOOD_PRD", "EOL_REWORK_OUT", false, false);
            //FixedMultiHeader("EOL_REWORK|PERF|GOOD_RATE", "EOL_REWORK_YEILD", true, false);

            ////AddDefectHeader(dgProdResult, "5", "EOL_REWORK", "QUALITYRE_");
            //AddDefectHeader(dgProdResult, "5", "EOL_REWORK", "EOL_");
            //FixedMultiHeader("EOL_REWORK|TOTAL|DEFECT", "EOL_REWORK_LOSS", false, false);

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
                        //1차 충전(Degas 전)
                        FixedMultiHeader("1_CHARGE_DEGAS_B|PERF|INPUT", "1_CHARGE_DEGAS_B_IN", false, false);
                        FixedMultiHeader("1_CHARGE_DEGAS_B|PERF|GOOD_PRD", "1_CHARGE_DEGAS_B_OUT", false, false);
                        FixedMultiHeader("1_CHARGE_DEGAS_B|PERF|GOOD_RATE", "1_CHARGE_DEGAS_B_YEILD", true, false);

                        AddDefectHeader(dgProdResult, "A", "1_CHARGE_DEGAS_B", "1_CHARGE_DEGAS_B_", 75);
                        FixedMultiHeader("1_CHARGE_DEGAS_B|TOTAL|DEFECT", "1_CHARGE_DEGAS_B_LOSS", false, false);
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("1CHARGERE"))
                    {
                        //1차 충전(Degas 전) 재작업
                        FixedMultiHeader("1_CHARGE_DEGAS_B_REWORK|PERF|INPUT", "1_CHARGE_DEGAS_B_REWORK_IN", false, false);
                        FixedMultiHeader("1_CHARGE_DEGAS_B_REWORK|PERF|GOOD_PRD", "1_CHARGE_DEGAS_B_REWORK_OUT", false, false);
                        FixedMultiHeader("1_CHARGE_DEGAS_B_REWORK|PERF|GOOD_RATE", "1_CHARGE_DEGAS_B_REWORK_YEILD", true, false);

                        AddDefectHeader(dgProdResult, "A", "1_CHARGE_DEGAS_B_REWORK", "1_CHARGE_DEGAS_B_REWORK_", 75, "B");
                        FixedMultiHeader("1_CHARGE_DEGAS_B_REWORK|TOTAL|DEFECT", "1_CHARGE_DEGAS_B_REWORK_LOSS", false, false);
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("DEGAS"))
                    {
                        //Dgas
                        FixedMultiHeader("DEGAS|PERF|INPUT", "DEGAS_IN", false, false);
                        FixedMultiHeader("DEGAS|PERF|GOOD_PRD", "DEGAS_OUT", false, false);
                        //FixedMultiHeader("DEGAS|PERF|YIELD", "DEGAS_YEILD2");
                        FixedMultiHeader("DEGAS|PERF|GOOD_RATE", "DEGAS_YEILD", true, false);

                        AddDefectHeader(dgProdResult, "D", "DEGAS", "DEGAS_");
                        FixedMultiHeader("DEGAS|TOTAL|DEFECT", "DEGAS_LOSS", false, false);
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("DEGASRE"))
                    {
                        //Dgas 재작업
                        FixedMultiHeader("DEGAS_REWORK|PERF|INPUT", "DEGAS_REWORK_IN", false, false);
                        FixedMultiHeader("DEGAS_REWORK|PERF|GOOD_PRD", "DEGAS_REWORK_OUT", false, false);
                        //FixedMultiHeader("DEGAS_REWORK|PERF|YIELD", "DEGASRE_YEILD2");
                        FixedMultiHeader("DEGAS_REWORK|PERF|GOOD_RATE", "DEGAS_REWORK_YEILD", true, false);

                        AddDefectHeader(dgProdResult, "D", "DEGAS_REWORK", "DEGAS_REWORK_");
                        FixedMultiHeader("DEGAS_REWORK|TOTAL|DEFECT", "DEGAS_REWORK_LOSS", false, false);
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("2CHARGE"))
                    {
                        //2차충전(Dgas 후)
                        FixedMultiHeader("2_CHARGE_DEGAS_A|PERF|INPUT", "2_CHARGE_DEGAS_A_IN", false, false);
                        FixedMultiHeader("2_CHARGE_DEGAS_A|PERF|GOOD_PRD", "2_CHARGE_DEGAS_A_OUT", false, false);
                        FixedMultiHeader("2_CHARGE_DEGAS_A|PERF|GOOD_RATE", "2_CHARGE_DEGAS_A_YEILD", true, false);

                        AddDefectHeader(dgProdResult, "B", "2_CHARGE_DEGAS_A", "2_CHARGE_DEGAS_A_", 75);
                        FixedMultiHeader("2_CHARGE_DEGAS_A|TOTAL|DEFECT", "2_CHARGE_DEGAS_A_LOSS", false, false);
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("2CHARGERE"))
                    {
                        //2차충전(Dgas 후) 재작업
                        FixedMultiHeader("2_CHARGE_DEGAS_A_REWORK|PERF|INPUT", "2_CHARGE_DEGAS_A_REWORK_IN", false, false);
                        FixedMultiHeader("2_CHARGE_DEGAS_A_REWORK|PERF|GOOD_PRD", "2_CHARGE_DEGAS_A_REWORK_OUT", false, false);
                        FixedMultiHeader("2_CHARGE_DEGAS_A_REWORK|PERF|GOOD_RATE", "2_CHARGE_DEGAS_A_REWORK_YEILD", true, false);

                        AddDefectHeader(dgProdResult, "B", "2_CHARGE_DEGAS_A_REWORK", "2_CHARGE_DEGAS_A_REWORK_", 75, "B");
                        FixedMultiHeader("2_CHARGE_DEGAS_A_REWORK|TOTAL|DEFECT", "2_CHARGE_DEGAS_A_REWORK_LOSS", false, false);
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("LOWVOLT"))
                    {
                        //저전압
                        //FixedMultiHeader("LOW_CAPA_BAD_PASS|PERF|TOTAL_YIELD", "LOWVOLT_TOTAL_YEILD");
                        FixedMultiHeader("LOW_CAPA_BAD_PASS|PERF|INPUT", "LOW_CAPA_BAD_PASS_IN", false, false);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS|PERF|GOOD_PRD", "LOW_CAPA_BAD_PASS_OUT", false, false);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS|PERF|GOOD_RATE", "LOW_CAPA_BAD_PASS_YEILD", true, false);

                        AddDefectHeader(dgProdResult, "G", "LOW_CAPA_BAD_PASS", "LOW_CAPA_BAD_PASS_", 75); //Width 추가
                        FixedMultiHeader("LOW_CAPA_BAD_PASS|TOTAL|DEFECT", "LOW_CAPA_BAD_PASS_LOSS", false, false);

                        //저전압 재작업 2021.06.01 추가
                        FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|PERF|INPUT", "LOW_CAPA_BAD_PASS_REWORK_IN", false, false);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|PERF|GOOD_PRD", "LOW_CAPA_BAD_PASS_REWORK_OUT", false, false);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|PERF|GOOD_RATE", "LOW_CAPA_BAD_PASS_REWORK_YEILD", true, false);

                        AddDefectHeader(dgProdResult, "G", "LOW_CAPA_BAD_PASS_REWORK", "LOW_CAPA_BAD_PASS_REWORK", 75); //Width 추가
                        FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|TOTAL|DEFECT", "LOW_CAPA_BAD_PASS_REWORK_LOSS", false, false);
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("EOL"))
                    {
                        // 2022.12.22 전체양품율 추가
                        FixedMultiHeader("EOL|PERF|TOTAL_GOOD_RATE", "EOL_TOTAL_YEILD", true, false);

                        //특성
                        FixedMultiHeader("EOL|PERF|INPUT", "EOL_IN", false, false);
                        FixedMultiHeader("EOL|PERF|GOOD_PRD", "EOL_OUT", false, false);
                        FixedMultiHeader("EOL|PERF|GOOD_RATE", "EOL_YEILD", true, false);

                        AddDefectHeader(dgProdResult, "5", "EOL", "EOL_");
                        FixedMultiHeader("EOL|TOTAL|DEFECT", "EOL_LOSS", false, false);
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("EOLRE"))
                    {
                        //특성 재작업
                        FixedMultiHeader("EOL_REWORK|PERF|INPUT", "EOL_REWORK_IN", false, false);
                        FixedMultiHeader("EOL_REWORK|PERF|GOOD_PRD", "EOL_REWORK_OUT", false, false);
                        FixedMultiHeader("EOL_REWORK|PERF|GOOD_RATE", "EOL_REWORK_YEILD", true, false);

                        AddDefectHeader(dgProdResult, "5", "EOL_REWORK", "EOL_");
                        FixedMultiHeader("EOL_REWORK|TOTAL|DEFECT", "EOL_REWORK_LOSS", false, false);
                    }
                    //20220328_제품검사의뢰 수량 산출 추가 START
                    else if (Util.NVC(dr["COM_CODE"]).Equals("INSP_REQ"))
                    {
                        // 20230323 - 제품검사의뢰 표현방식 수정 (PROCID 구분으로 세분화)
                        //검사의뢰
                        FixedMultiHeader("INSP_REQ|PROC_INSP_REQ|DEGAS", "INSP_REQ_LQC_DEGAS", false, false);
                        FixedMultiHeader("INSP_REQ|PROC_INSP_REQ|EOL", "INSP_REQ_LQC_EOL", false, false);
                        FixedMultiHeader("INSP_REQ|PROC_INSP_REQ|ETC", "INSP_REQ_LQC_ETC", false, false);
                        FixedMultiHeader("INSP_REQ|PQC_INSP_REQ|LOW_VOLT", "INSP_REQ_PQC_LOW_VOLT", false, false);
                        FixedMultiHeader("INSP_REQ|PQC_INSP_REQ|EOL", "INSP_REQ_PQC_EOL", false, false);
                        FixedMultiHeader("INSP_REQ|PQC_INSP_REQ|ETC", "INSP_REQ_PQC_ETC", false, false);
                    }
                    //20220328_제품검사의뢰 수량 산출 추가 END
                    //20220725_C20220603-000198_2차 저전압 조건 추가 START
                    else if (Util.NVC(dr["COM_CODE"]).Equals("2LOWVOLT"))
                    {
                        b2LowVoltFlag = true;
                        //저전압
                        //FixedMultiHeader("LOW_CAPA_BAD_PASS|PERF|TOTAL_YIELD", "LOWVOLT_TOTAL_YEILD");
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS|PERF|INPUT", "2_LOW_CAPA_BAD_PASS_IN", false, false);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS|PERF|GOOD_PRD", "2_LOW_CAPA_BAD_PASS_OUT", false, false);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS|PERF|GOOD_RATE", "2_LOW_CAPA_BAD_PASS_YEILD", true, false);

                        AddDefectHeader(dgProdResult, "G", "2_LOW_CAPA_BAD_PASS", "2_LOW_CAPA_BAD_PASS_", 75); //Width 추가
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS|TOTAL|DEFECT", "2_LOW_CAPA_BAD_PASS_LOSS", false, false);

                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS_REWORK|PERF|INPUT", "2_LOW_CAPA_BAD_PASS_REWORK_IN", false, false);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS_REWORK|PERF|GOOD_PRD", "2_LOW_CAPA_BAD_PASS_REWORK_OUT", false, false);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS_REWORK|PERF|GOOD_RATE", "2_LOW_CAPA_BAD_PASS_REWORK_YEILD", true, false);

                        AddDefectHeader(dgProdResult, "G", "2_LOW_CAPA_BAD_PASS_REWORK", "2_LOW_CAPA_BAD_PASS_REWORK", 75); //Width 추가
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS_REWORK|TOTAL|DEFECT", "2_LOW_CAPA_BAD_PASS_REWORK_LOSS", false, false);
                    }
                    //20220725_C20220603-000198_2차 저전압 조건 추가 END
                    // 2022.12.20 특성 양품 재작업 추가 Start
                    else if (Util.NVC(dr["COM_CODE"]).Equals("EOLGRE"))
                    {
                        //특성 재작업
                        FixedMultiHeader("EOL_GREWORK|PERF|INPUT", "EOL_GREWORK_IN", false, false);
                        FixedMultiHeader("EOL_GREWORK|PERF|GOOD_PRD", "EOL_GREWORK_OUT", false, false);
                        FixedMultiHeader("EOL_GREWORK|PERF|GOOD_RATE", "EOL_GREWORK_YEILD", true, false);

                        AddDefectHeader(dgProdResult, "5", "EOL_GREWORK", "EOL_G");
                        FixedMultiHeader("EOL_GREWORK|TOTAL|DEFECT", "EOL_GREWORK_LOSS", false, false);
                    }
                    // 2022.12.20 특성 양품 재작업 추가 End
                }
            }
            //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) END
        }

        //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) START
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
            FixedMultiHeader("WORK_DATE|WORK_DATE", "CALDATE", false, true, 100);
            FixedMultiHeader("LINE_ID|LINE_ID", "LINE_NAME", false, true, 150);
            FixedMultiHeader("MODEL_ID|MODEL_ID", "MDLLOT_ID", false, true);
            FixedMultiHeader("MODEL_NAME|MODEL_NAME", "MODEL_NAME", false, true);

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
                        FixedMultiHeader("1_CHARGE|GOOD_RATE", "1_CHARGE_DEGAS_B_YEILD", true, true);
                        FixedMultiHeader("1_CHARGE|BAD_RATE", "1_CHARGE_DEGAS_B_LOSS_YEILD", true, true);
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
                        FixedMultiHeader("DEGAS|GOOD_RATE", "DEGAS_YEILD", true, true);
                        FixedMultiHeader("DEGAS|BAD_RATE", "DEGAS_LOSS_YEILD", true, true);
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
                        FixedMultiHeader("2_CHARGE|GOOD_RATE", "2_CHARGE_DEGAS_A_YEILD", true, true);
                        FixedMultiHeader("2_CHARGE|BAD_RATE", "2_CHARGE_DEGAS_A_LOSS_YEILD", true, true);
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("2CHARGERE"))
                    {
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("LOWVOLT"))
                    {
                        //저전압
                        FixedMultiHeader("LOW_CAPA_BAD_PASS|INPUT", "LOW_CAPA_BAD_PASS_IN", false, true);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS|GOOD_PRD", "LOW_CAPA_BAD_PASS_OUT", false, true);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS|DEFECT", "LOW_CAPA_BAD_PASS_LOSS", false, true);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS|GOOD_RATE", "LOW_CAPA_BAD_PASS_YEILD", true, true); //20220725_컬럼명 수정
                        FixedMultiHeader("LOW_CAPA_BAD_PASS|BAD_RATE", "LOW_CAPA_BAD_PASS_LOSS_YEILD", true, true);

                        FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|INPUT", "LOW_CAPA_BAD_PASS_REWORK_IN", false, true);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|GOOD_PRD", "LOW_CAPA_BAD_PASS_REWORK_OUT", false, true);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|DEFECT", "LOW_CAPA_BAD_PASS_REWORK_LOSS", false, true);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|GOOD_RATE", "LOW_CAPA_BAD_PASS_REWORK_YEILD", true, true);
                        FixedMultiHeader("LOW_CAPA_BAD_PASS_REWORK|BAD_RATE", "LOW_CAPA_BAD_PASS_REWORK_LOSS_YEILD", true, true);
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("EOL"))
                    {
                        // 2022.12.22 전체양품율 추가
                        FixedMultiHeader("EOL_INSP|TOTAL_GOOD_RATE", "EOL_TOTAL_YEILD", true, true);

                        //EOL
                        FixedMultiHeader("EOL_INSP|INPUT", "EOL_IN", false, true);
                        FixedMultiHeader("EOL_INSP|GOOD_PRD", "EOL_OUT", false, true);
                        FixedMultiHeader("EOL_INSP|DEFECT", "EOL_LOSS", false, true);
                        FixedMultiHeader("EOL_INSP|GOOD_RATE", "EOL_YEILD", true, true); //20220725_컬럼명 수정
                        FixedMultiHeader("EOL_INSP|BAD_RATE", "EOL_LOSS_YEILD", true, true);
                    }
                    else if (Util.NVC(dr["COM_CODE"]).Equals("EOLRE"))
                    {
                        //특성 재작업
                        //20220620_C20220603-000198 START
                        FixedMultiHeader("EOL_RE_INSP|INPUT", "EOL_REWORK_IN", false, true);
                        FixedMultiHeader("EOL_RE_INSP|GOOD_PRD", "EOL_REWORK_OUT", false, true);
                        FixedMultiHeader("EOL_RE_INSP|DEFECT", "EOL_REWORK_LOSS", false, true);
                        FixedMultiHeader("EOL_RE_INSP|GOOD_RATE", "EOL_REWORK_YEILD", true, true); //20220725_컬럼명 수정
                        FixedMultiHeader("EOL_RE_INSP|BAD_RATE", "EOL_REWORK_LOSS_YEILD", true, true);
                        //20220620_C20220603-000198 END
                    }
                    //20220328_제품검사의뢰 수량 산출 추가 START
                    else if (Util.NVC(dr["COM_CODE"]).Equals("INSP_REQ"))
                    {
                        //검사의뢰
                        FixedMultiHeader("INSP_REQ|INSP_REQ|PROC_INSP_REQ", "INSP_REQ_LQC", true, true);
                        FixedMultiHeader("INSP_REQ|INSP_REQ|PQC_INSP_REQ", "INSP_REQ_PQC", true, true);
                    }
                    //20220328_제품검사의뢰 수량 산출 추가 END
                    //20220725_C20220603-000198_2차 저전압 조건 추가 START
                    else if (Util.NVC(dr["COM_CODE"]).Equals("2LOWVOLT"))
                    {
                        //저전압
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS|INPUT", "2_LOW_CAPA_BAD_PASS_IN", false, true);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS|GOOD_PRD", "2_LOW_CAPA_BAD_PASS_OUT", false, true);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS|DEFECT", "2_LOW_CAPA_BAD_PASS_LOSS", false, true);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS|GOOD_RATE", "2_LOW_CAPA_BAD_PASS_YEILD", true, true);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS|BAD_RATE", "2_LOW_CAPA_BAD_PASS_LOSS_YEILD", true, true);

                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS_REWORK|INPUT", "2_LOW_CAPA_BAD_PASS_REWORK_IN", false, true);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS_REWORK|GOOD_PRD", "2_LOW_CAPA_BAD_PASS_REWORK_OUT", false, true);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS_REWORK|DEFECT", "2_LOW_CAPA_BAD_PASS_REWORK_LOSS", false, true);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS_REWORK|GOOD_RATE", "2_LOW_CAPA_BAD_PASS_REWORK_YEILD", true, true);
                        FixedMultiHeader("2_LOW_CAPA_BAD_PASS_REWORK|BAD_RATE", "2_LOW_CAPA_BAD_PASS_REWORK_LOSS_YEILD", true, true);
                    }
                    //20220725_C20220603-000198_2차 저전압 조건 추가 END
                    // 2022.12.20 특성 양품 재작업 추가 Start
                    else if (Util.NVC(dr["COM_CODE"]).Equals("EOLGRE"))
                    {
                        //특성 양품재작업
                        FixedMultiHeader("EOL_GREWORK|INPUT", "EOL_GREWORK_IN", false, true);
                        FixedMultiHeader("EOL_GREWORK|GOOD_PRD", "EOL_GREWORK_OUT", false, true);
                        FixedMultiHeader("EOL_GREWORK|DEFECT", "EOL_GREWORK_LOSS", false, true);
                        FixedMultiHeader("EOL_GREWORK|GOOD_RATE", "EOL_GREWORK_YEILD", true, true);
                        FixedMultiHeader("EOL_GREWORK|BAD_RATE", "EOL_GREWORK_LOSS_YEILD", true, true);
                    }
                    // 2022.12.20 특성 양품 재작업 추가 End
                }
            }
        }
        //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) END
        #endregion

        #region [Method]

        //private void FixedMultiHeader(string sName, string sBindName, bool bPercent, int iWidth = 75) //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동)
        private void FixedMultiHeader(string sName, string sBindName, bool bPercent, bool bSummray, int iWidth = 75) //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동)
        {
            bool bReadOnly = true;
            bool bEditable = false;
            bool bVisible = true;

            string[] sColName = sName.Split('|');

            List<string> Multi_Header = new List<string>();
            Multi_Header = sColName.ToList();

            //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) START
            //var column_TEXT = CreateTextColumn(null, Multi_Header, sBindName, sBindName, iWidth, bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible, bPercent: bPercent);
            //dgProdResult.Columns.Add(column_TEXT);

            if (bSummray.Equals(true))
            {
                var column_TEXT = CreateTextColumn(null, Multi_Header, sBindName, sBindName, iWidth, bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible, bPercent: bPercent, bSummray: bSummray);
                dgProdResultSummary.Columns.Add(column_TEXT);
            }
            else
            {
                var column_TEXT = CreateTextColumn(null, Multi_Header, sBindName, sBindName, iWidth, bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible, bPercent: bPercent, bSummray: bSummray);
                dgProdResult.Columns.Add(column_TEXT);
            }
            //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) END
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
                                                                         , bool bSummray = false ///2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동)
                                                                         , HorizontalAlignment HorizonAlign = HorizontalAlignment.Center
                                                                         , VerticalAlignment VerticalAlign = VerticalAlignment.Center
                                                        )
        {

            //C1.WPF.DataGrid.DataGridTextColumn Col = new C1.WPF.DataGrid.DataGridTextColumn()
            //{
            //    Name = sName,
            //    Binding = new Binding(sBinding),
            //    IsReadOnly = bReadOnly,
            //    EditOnSelection = bEditable,
            //    Visibility = bVisible.Equals(true) ? Visibility.Visible : Visibility.Collapsed,
            //    HorizontalAlignment = HorizonAlign,
            //    VerticalAlignment = VerticalAlign,
            //    Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto)
            //};

            C1.WPF.DataGrid.DataGridTextColumn Col = new C1.WPF.DataGrid.DataGridTextColumn();

            Col.Name = sName;
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

            //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) START
            //임시 테이블에 헤더값 저장
            //_dtHeader.Columns.Add(sBinding, typeof(string));
            if (bSummray.Equals(true))
            {
                _dtHeaderSummary.Columns.Add(sBinding, typeof(string));
            }
            else
            {
                _dtHeader.Columns.Add(sBinding, typeof(string));
            }
            //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) END
            return Col;
        }

        private object GetList(object arg)
        {
            try
            {
                object[] argument = (object[])arg;

                DateTime dFromDate = (DateTime)argument[0];
                DateTime dToDate = (DateTime)argument[1];

                string EQSGID = argument[2] == null ? null : argument[2].ToString();
                string MDLLOT_ID = argument[3] == null ? null : argument[3].ToString();
                string LOTTYPE = argument[4] == null ? null : argument[4].ToString();


                TimeSpan tsDateDiff = dToDate - dFromDate;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("MONTHLY_YN", typeof(string));
                dtRqst.Columns.Add("CALDATE", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string)); // 2021.08.19 Lot 유형 검색조건 추가
                dtRqst.Columns.Add("AREAID", typeof(string)); //20220328_제품검사의뢰 수량 산출 추가


                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = EQSGID;
                dr["MDLLOT_ID"] = MDLLOT_ID;
                dr["MONTHLY_YN"] = "N";
                dr["CALDATE"] = "";
                dr["LOTTYPE"] = LOTTYPE;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID; //20220328_제품검사의뢰 수량 산출 추가
                dtRqst.Rows.Add(dr);

                DataSet dsRqst = new DataSet();
                dsRqst.Tables.Add(dtRqst);

                DataTable dtRqstEQSGID = new DataTable();
                dtRqstEQSGID.TableName = "RQSTDT";
                dtRqstEQSGID.Columns.Add("LANGID", typeof(string));
                dtRqstEQSGID.Columns.Add("AREAID", typeof(string));
                dtRqstEQSGID.Columns.Add("EQSGID", typeof(string));

                DataRow drRqstEQSGID = dtRqstEQSGID.NewRow();
                drRqstEQSGID["LANGID"] = LoginInfo.LANGID;
                drRqstEQSGID["AREAID"] = LoginInfo.CFG_AREA_ID;
                if (!string.IsNullOrEmpty(EQSGID))
                {
                    drRqstEQSGID["EQSGID"] = EQSGID;
                }
                dtRqstEQSGID.Rows.Add(drRqstEQSGID);

                DataTable dtEQSGID = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_FORM", "RQSTDT", "RSLTDT", dtRqstEQSGID);

                DataTable dtModelRqst = new DataTable();
                dtModelRqst.TableName = "RQSTDT";
                dtModelRqst.Columns.Add("EQSGID", typeof(string));
                dtModelRqst.Columns.Add("AREAID", typeof(string));

                DataRow drRqstModel = dtModelRqst.NewRow();
                drRqstModel["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRqstModel["EQSGID"] = "";
                dtModelRqst.Rows.Add(drRqstModel);

                DataSet dSRslt = new DataSet();
                int totalCount = (tsDateDiff.Days + 1);
                if (dtEQSGID.Rows.Count > 0) totalCount = (tsDateDiff.Days + 1) * dtEQSGID.Rows.Count;
                int runCount = 0;

                for (int i = 0; i <= tsDateDiff.Days; i++)
                {
                    dsRqst.Tables[0].Rows[0]["CALDATE"] = dFromDate.AddDays(i).ToString("yyyyMMdd");

                    DataSet dsDate = null;
                    if (dtEQSGID.Rows.Count > 0)
                    {
                        foreach (DataRow drEqsgid in dtEQSGID.Rows)
                        {
                            runCount++;

                            drRqstModel["EQSGID"] = drEqsgid["EQSGID"];
                            DataTable dtModel = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE_MODEL", "RQSTDT", "RSLTDT", dtModelRqst);
                            if (MDLLOT_ID != null && !string.IsNullOrEmpty(MDLLOT_ID))
                            {
                                DataView dvModel = dtModel.DefaultView;
                                dvModel.RowFilter = "CBO_CODE = '" + MDLLOT_ID + "'";
                                dtModel = dvModel.ToTable();
                            }

                            foreach (DataRow drModel in dtModel.Rows)
                            {
                                dsRqst.Tables[0].Rows[0]["EQSGID"] = drEqsgid["EQSGID"];
                                dsRqst.Tables[0].Rows[0]["MDLLOT_ID"] = drModel["CBO_CODE"];
                                bgWorker.ReportProgress(runCount * 100 / totalCount, "[" + dFromDate.AddDays(i).ToString("yyyy-MM-dd") + "] - " + (drEqsgid["EQSGID"] == null ? "" : drEqsgid["EQSGID"].ToString() + " : " + drEqsgid["EQSGNAME"].ToString() + " - " + drModel["CBO_CODE"].ToString()));

                                dsDate = new ClientProxy().ExecuteServiceSync_Multi("BR_SEL_PROD_PERF_REPORT_BY_DAY_CNB", "INDATA", "ROUTE,AGING,FORMATION1_CHARGE,FORMATION1_CHARGE_REWORK,DEGAS,DEGAS_REWORK,FORMATION2_CHARGE,FORMATION2_CHARGE_REWORK,GRADE,GRADE_REWORK,QUALITY,QUALITY_REWORK,QUALITY_GOOD_REWORK,INSP_REQ,GRADE2,GRADE2_REWORK", dsRqst); //20220725_C20220603-000198_2차 저전압 조건 추가
                                dSRslt.Merge(dsDate);
                            }
                        }
                    }
                    else
                    {
                        // 라인이 선택되었을때
                        bgWorker.ReportProgress(runCount++ * 100 / totalCount, "[" + dFromDate.AddDays(i).ToString("yyyy-MM-dd") + "]");

                        dsDate = new ClientProxy().ExecuteServiceSync_Multi("BR_SEL_PROD_PERF_REPORT_BY_DAY_CNB", "INDATA", "ROUTE,AGING,FORMATION1_CHARGE,FORMATION1_CHARGE_REWORK,DEGAS,DEGAS_REWORK,FORMATION2_CHARGE,FORMATION2_CHARGE_REWORK,GRADE,GRADE_REWORK,QUALITY,QUALITY_REWORK,QUALITY_GOOD_REWORK,INSP_REQ,GRADE2,GRADE2_REWORK", dsRqst); //20220725_C20220603-000198_2차 저전압 조건 추가
                        dSRslt.Merge(dsDate);
                    }
                }

                DataTable dtRoute = dSRslt.Tables["ROUTE"];
                DataTable dtAging = dSRslt.Tables["AGING"];
                DataTable dtFormation1Charge = dSRslt.Tables["FORMATION1_CHARGE"];
                //DataTable dtFormation1Discharge = dSRslt.Tables["FORMATION1_DISCHARGE"];
                DataTable dtDegas = dSRslt.Tables["DEGAS"];
                DataTable dtFormation2Charge = dSRslt.Tables["FORMATION2_CHARGE"];
                DataTable dtGrade = dSRslt.Tables["GRADE"];                           //GRADE 작업일지 데이터
                DataTable dtQuality = dSRslt.Tables["QUALITY"];
                DataTable dtQualityRework = dSRslt.Tables["QUALITY_REWORK"];
                DataTable dtFormation1Rework = dSRslt.Tables["FORMATION1_CHARGE_REWORK"];
                DataTable dtFormation2Rework = dSRslt.Tables["FORMATION2_CHARGE_REWORK"];
                DataTable dtDegasRework = dSRslt.Tables["DEGAS_REWORK"];
                DataTable dtGradeRework = dSRslt.Tables["GRADE_REWORK"];
                DataTable dtInspReq = dSRslt.Tables["INSP_REQ"]; //20220328_제품검사의뢰 수량 산출 추가

                //20220725_C20220603-000198_2차 저전압 조건 추가 START
                DataTable dtGrade2 = dSRslt.Tables["GRADE2"];                           //GRADE 작업일지 데이터
                DataTable dtGrade2Rework = dSRslt.Tables["GRADE2_REWORK"];
                //20220725_C20220603-000198_2차 저전압 조건 추가 END

                // 2022.12.20 특성 양품 재작업 추가
                DataTable dtQualityGoodRework = dSRslt.Tables["QUALITY_GOOD_REWORK"];

                DataTable dtDistinctMerge = new DataTable();
                dtDistinctMerge.Columns.Add("CALDATE", typeof(string));
                dtDistinctMerge.Columns.Add("EQSGID", typeof(string));
                dtDistinctMerge.Columns.Add("MDLLOT_ID", typeof(string));

                dtDistinctMerge.Merge(dtAging.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "MDLLOT_ID" }));
                dtDistinctMerge.Merge(dtFormation1Charge.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "MDLLOT_ID" }));
                //dtDistinctMerge.Merge(dtFormation1Discharge.DefaultView.ToTable(true, new string[] { "WORK_DATE", "LINE_ID", "MODEL_ID" }));
                dtDistinctMerge.Merge(dtDegas.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "MDLLOT_ID" }));
                dtDistinctMerge.Merge(dtFormation2Charge.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "MDLLOT_ID" }));
                dtDistinctMerge.Merge(dtGrade.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "MDLLOT_ID" }));
                dtDistinctMerge.Merge(dtQuality.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "MDLLOT_ID" }));
                dtDistinctMerge.Merge(dtQualityRework.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "MDLLOT_ID" }));
                dtDistinctMerge.Merge(dtFormation1Rework.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "MDLLOT_ID" }));
                dtDistinctMerge.Merge(dtFormation2Rework.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "MDLLOT_ID" }));
                dtDistinctMerge.Merge(dtDegasRework.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "MDLLOT_ID" }));
                dtDistinctMerge.Merge(dtGradeRework.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "MDLLOT_ID" }));
                dtDistinctMerge.Merge(dtInspReq.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "MDLLOT_ID" })); //20220328_제품검사의뢰 수량 산출 추가

                //20220725_C20220603-000198_2차 저전압 조건 추가 START
                dtDistinctMerge.Merge(dtGrade2.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "MDLLOT_ID" }));
                dtDistinctMerge.Merge(dtGrade2Rework.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "MDLLOT_ID" }));
                //20220725_C20220603-000198_2차 저전압 조건 추가 END

                // 2022.12.20 특성 양품 재작업 추가
                if (dtQualityGoodRework != null)
                {
                    dtDistinctMerge.Merge(dtQualityGoodRework.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "MDLLOT_ID" }));
                }

                DataTable dtResult = dtDistinctMerge.DefaultView.ToTable(true, new string[] { "CALDATE", "EQSGID", "MDLLOT_ID" });
                dtResult.TableName = "RESULT";

                for (int i = dgProdResult.Columns["MODEL_NAME"].Index + 1; i < dgProdResult.Columns.Count; i++)
                {
                    dtResult.Columns.Add(_dtHeader.Columns[i].ColumnName, typeof(decimal));
                }

                dtResult.Columns.Add("MODEL_NAME", typeof(string));
                dtResult.Columns.Add("LINE_NAME", typeof(string));

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
                SetData(dtResult, dtInspReq, "INSP_REQ_"); //20220328_제품검사의뢰 수량 산출 추가

                //20220725_C20220603-000198_2차 저전압 조건 추가 START
                SetData(dtResult, dtGrade2, "2_LOW_CAPA_BAD_PASS_"); //LOWVOLT_
                SetData(dtResult, dtGrade2Rework, "2_LOW_CAPA_BAD_PASS_REWORK_"); //LOWVOLTRE_
                                                                                  //20220725_C20220603-000198_2차 저전압 조건 추가 END

                // 2022.12.20 특성 양품 재작업 추가
                if (dtQualityGoodRework != null)
                {
                    SetData(dtResult, dtQualityGoodRework, "EOL_GREWORK_"); //QUALITYGRE_
                }

                DataTable dtLine = ((DataView)cboLine.ItemsSource).Table;

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    dtResult.Rows[i]["MODEL_NAME"] = dtRoute.Select("MDLLOT_ID = '" + dtResult.Rows[i]["MDLLOT_ID"] + "'")[0]["PRJT_NAME"];
                    dtResult.Rows[i]["LINE_NAME"] = dtLine.Select("CBO_CODE = '" + dtResult.Rows[i]["EQSGID"] + "'")[0]["CBO_NAME"];
                    dtResult.Rows[i]["CALDATE"] = Convert.ToInt32(dtResult.Rows[i]["CALDATE"].ToString()).ToString("####-##-##");
                }

                DataView dv = new DataView(dtResult);
                dv.Sort = "CALDATE ASC, MODEL_NAME ASC";
                dtResult = dv.ToTable();

                for (int i = 0; i < dtResult.Columns.Count; i++)
                {
                    string sMaxLength = dtResult.Columns[i].MaxLength.ToString();
                }

                //var maxLength = Enumerable.Range(0, dtDistinct.Columns.Count)
                //    .Select(col => dtDistinct.AsEnumerable()
                //                    .Select(row => row[col]).OfType<string>()
                //                    .Max(val => val?.Length)).ToList();


                //Util.GridSetData(dgProdResult, dtDistinct, FrameOperation, true);
                //GridSetData(dgProdResult, dtDistinct, FrameOperation, true);

                //2021-05-14 Bottom Summary Row 추가
                SetBottomRow(dtResult);


                //DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["MDLLOT_ID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("Total") } });
                // DataGridAggregate.SetAggregateFunctions(dgProdResult.Columns["TRAYCNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });



                //foreach (var col in dgProdResult.Columns)
                //{
                //    col.MinWidth = col.ActualWidth;
                //    col.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.AutoStar);
                //}

                //Summary
                DataTable dtSummary = dtResult.Copy();
                dtSummary.TableName = "SUMMARY";

                //컬럼추가
                dtSummary.Columns.Add("1_CHARGE_DEGAS_B_LOSS_YEILD", typeof(decimal));
                dtSummary.Columns.Add("DEGAS_LOSS_YEILD", typeof(decimal));
                dtSummary.Columns.Add("2_CHARGE_DEGAS_A_LOSS_YEILD", typeof(decimal));
                dtSummary.Columns.Add("LOW_CAPA_BAD_PASS_LOSS_YEILD", typeof(decimal));
                dtSummary.Columns.Add("LOW_CAPA_BAD_PASS_REWORK_LOSS_YEILD", typeof(decimal));
                dtSummary.Columns.Add("EOL_LOSS_YEILD", typeof(decimal));
                dtSummary.Columns.Add("EOL_REWORK_LOSS_YEILD", typeof(decimal));

                //20220725_C20220603-000198_2차 저전압 조건 추가 START
                dtSummary.Columns.Add("2_LOW_CAPA_BAD_PASS_LOSS_YEILD", typeof(decimal));
                dtSummary.Columns.Add("2_LOW_CAPA_BAD_PASS_REWORK_LOSS_YEILD", typeof(decimal));
                //20220725_C20220603-000198_2차 저전압 조건 추가 END

                // 2022.12.20 특성 양품 재작업 추가                
                if (!dtSummary.Columns.Contains("EOL_GREWORK_IN")) dtSummary.Columns.Add("EOL_GREWORK_IN", typeof(decimal));
                if (!dtSummary.Columns.Contains("EOL_GREWORK_OUT")) dtSummary.Columns.Add("EOL_GREWORK_OUT", typeof(decimal));
                if (!dtSummary.Columns.Contains("EOL_GREWORK_LOSS")) dtSummary.Columns.Add("EOL_GREWORK_LOSS", typeof(decimal));
                if (!dtSummary.Columns.Contains("EOL_GREWORK_YEILD")) dtSummary.Columns.Add("EOL_GREWORK_YEILD", typeof(decimal));
                if (!dtSummary.Columns.Contains("EOL_GREWORK_LOSS_YEILD")) dtSummary.Columns.Add("EOL_GREWORK_LOSS_YEILD", typeof(decimal));

                //2023.01.04 전체양품율 추가
                if (!dtSummary.Columns.Contains("EOL_TOTAL_YEILD")) dtSummary.Columns.Add("EOL_TOTAL_YEILD", typeof(decimal));

                for (int i = 0; i < dtSummary.Rows.Count; i++)
                {
                    decimal CHARGE1_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["1_CHARGE_DEGAS_B_IN"].ToString()) ? "0" : dtSummary.Rows[i]["1_CHARGE_DEGAS_B_IN"].ToString());
                    decimal CHARGE1_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["1_CHARGE_DEGAS_B_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["1_CHARGE_DEGAS_B_LOSS"].ToString());
                    dtSummary.Rows[i]["1_CHARGE_DEGAS_B_LOSS_YEILD"] = (CHARGE1_IN == 0 ? 0 : (CHARGE1_LOSS == 0 ? 0 : CHARGE1_LOSS / CHARGE1_IN));

                    decimal DEGAS_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["DEGAS_IN"].ToString()) ? "0" : dtSummary.Rows[i]["DEGAS_IN"].ToString());
                    decimal DEGAS_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["DEGAS_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["DEGAS_LOSS"].ToString());
                    dtSummary.Rows[i]["DEGAS_LOSS_YEILD"] = (DEGAS_IN == 0 ? 0 : (DEGAS_LOSS == 0 ? 0 : DEGAS_LOSS / DEGAS_IN));

                    decimal CHARGE2_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["2_CHARGE_DEGAS_A_IN"].ToString()) ? "0" : dtSummary.Rows[i]["2_CHARGE_DEGAS_A_IN"].ToString());
                    decimal CHARGE2_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["2_CHARGE_DEGAS_A_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["2_CHARGE_DEGAS_A_LOSS"].ToString());

                    dtSummary.Rows[i]["2_CHARGE_DEGAS_A_LOSS_YEILD"] = (CHARGE2_IN == 0 ? 0 : (CHARGE2_LOSS == 0 ? 0 : CHARGE2_LOSS / CHARGE2_IN));


                    decimal LOWVOLT_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_IN"].ToString()) ? "0" : dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_IN"].ToString());
                    decimal LOWVOLT_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_OUT"].ToString()) ? "1" : dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_OUT"].ToString());
                    //dt.Rows[i]["LOWVOLT_YIELD"] = LOWVOLT_OUT / LOWVOLT_IN;
                    dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_YEILD"] = (LOWVOLT_IN == 0 ? 0 : (LOWVOLT_OUT == 0 ? 0 : LOWVOLT_OUT / LOWVOLT_IN)); //20220725_컬럼명 수정

                    decimal LOWVOLT_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_LOSS"].ToString());
                    //dt.Rows[i]["LOWVOLT_LOSS_YEILD"] = LOWVOLT_LOSS / LOWVOLT_IN;
                    dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_LOSS_YEILD"] = (LOWVOLT_IN == 0 ? 0 : (LOWVOLT_LOSS == 0 ? 0 : LOWVOLT_LOSS / LOWVOLT_IN));

                    decimal LOWVOLTRE_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_REWORK_IN"].ToString()) ? "0" : dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_REWORK_IN"].ToString());
                    decimal LOWVOLTRE_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_REWORK_OUT"].ToString()) ? "1" : dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_REWORK_OUT"].ToString());
                    //dt.Rows[i]["LOWVOLTRE_YIELD"] = LOWVOLTRE_OUT / LOWVOLTRE_IN;
                    dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_REWORK_YEILD"] = (LOWVOLTRE_IN == 0 ? 0 : (LOWVOLTRE_OUT == 0 ? 0 : LOWVOLTRE_OUT / LOWVOLTRE_IN));

                    decimal LOWVOLTRE_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_REWORK_LOSS"].ToString()) ? "1" : dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_REWORK_LOSS"].ToString());
                    //dt.Rows[i]["LOWVOLTRE_LOSS_YEILD"] = LOWVOLTRE_LOSS / LOWVOLTRE_IN;
                    dtSummary.Rows[i]["LOW_CAPA_BAD_PASS_REWORK_LOSS_YEILD"] = (LOWVOLTRE_IN == 0 ? 0 : (LOWVOLTRE_LOSS == 0 ? 0 : LOWVOLTRE_LOSS / LOWVOLTRE_IN));

                    decimal QUALITY_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_IN"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_IN"].ToString());
                    decimal QUALITY_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_OUT"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_OUT"].ToString());
                    dtSummary.Rows[i]["EOL_YEILD"] = (QUALITY_IN == 0 ? 0 : (QUALITY_OUT == 0 ? 0 : QUALITY_OUT / QUALITY_IN));

                    decimal QUALITY_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_LOSS"].ToString());
                    dtSummary.Rows[i]["EOL_LOSS_YEILD"] = (QUALITY_IN == 0 ? 0 : (QUALITY_LOSS == 0 ? 0 : QUALITY_LOSS / QUALITY_IN));

                    decimal QUALITYRE_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_REWORK_IN"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_REWORK_IN"].ToString());
                    decimal QUALITYRE_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_REWORK_OUT"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_REWORK_OUT"].ToString());
                    dtSummary.Rows[i]["EOL_REWORK_YEILD"] = (QUALITYRE_IN == 0 ? 0 : (QUALITYRE_OUT == 0 ? 0 : QUALITYRE_OUT / QUALITYRE_IN));

                    decimal QUALITYRE_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_REWORK_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_REWORK_LOSS"].ToString());
                    dtSummary.Rows[i]["EOL_REWORK_LOSS_YEILD"] = (QUALITYRE_IN == 0 ? 0 : (QUALITYRE_LOSS == 0 ? 0 : QUALITYRE_LOSS / QUALITYRE_IN));

                    //20220725_C20220603-000198_2차 저전압 조건 추가 START
                    if (b2LowVoltFlag)
                    {
                        decimal LOWVOLT2_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_IN"].ToString()) ? "0" : dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_IN"].ToString());
                        decimal LOWVOLT2_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_OUT"].ToString()) ? "1" : dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_OUT"].ToString());
                        dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_YEILD"] = (LOWVOLT2_IN == 0 ? 0 : (LOWVOLT2_OUT == 0 ? 0 : LOWVOLT2_OUT / LOWVOLT2_IN));

                        decimal LOWVOLT2_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_LOSS"].ToString());
                        dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_LOSS_YEILD"] = (LOWVOLT2_IN == 0 ? 0 : (LOWVOLT2_LOSS == 0 ? 0 : LOWVOLT2_LOSS / LOWVOLT2_IN));

                        decimal LOWVOLT2RE_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_REWORK_IN"].ToString()) ? "0" : dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_REWORK_IN"].ToString());
                        decimal LOWVOLT2RE_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_REWORK_OUT"].ToString()) ? "1" : dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_REWORK_OUT"].ToString());
                        dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_REWORK_YEILD"] = (LOWVOLT2RE_IN == 0 ? 0 : (LOWVOLT2RE_OUT == 0 ? 0 : LOWVOLT2RE_OUT / LOWVOLT2RE_IN));

                        decimal LOWVOLT2RE_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_REWORK_LOSS"].ToString()) ? "1" : dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_REWORK_LOSS"].ToString());
                        dtSummary.Rows[i]["2_LOW_CAPA_BAD_PASS_REWORK_LOSS_YEILD"] = (LOWVOLT2RE_IN == 0 ? 0 : (LOWVOLT2RE_LOSS == 0 ? 0 : LOWVOLT2RE_LOSS / LOWVOLT2RE_IN));
                    }
                    //20220725_C20220603-000198_2차 저전압 조건 추가 END

                    // 2022.12.20 특성 양품 재작업 추가 Start
                    decimal QUALITYGRE_IN = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_GREWORK_IN"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_GREWORK_IN"].ToString());
                    decimal QUALITYGRE_OUT = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_GREWORK_OUT"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_GREWORK_OUT"].ToString());
                    dtSummary.Rows[i]["EOL_GREWORK_YEILD"] = (QUALITYGRE_IN == 0 ? 0 : (QUALITYGRE_OUT == 0 ? 0 : QUALITYGRE_OUT / QUALITYGRE_IN));

                    decimal QUALITYGRE_LOSS = decimal.Parse(string.IsNullOrEmpty(dtSummary.Rows[i]["EOL_GREWORK_LOSS"].ToString()) ? "0" : dtSummary.Rows[i]["EOL_GREWORK_LOSS"].ToString());
                    dtSummary.Rows[i]["EOL_GREWORK_LOSS_YEILD"] = (QUALITYGRE_IN == 0 ? 0 : (QUALITYGRE_LOSS == 0 ? 0 : QUALITYGRE_LOSS / QUALITYGRE_IN));
                    // 2022.12.20 특성 양품 재작업 추가 End

                    // 2022.12.22 전체양품율 추가
                    //a : QUALITY_IN 직행투입
                    //b : QUALITY_OUT 직행양품
                    //d : QUALITYRE_OUT 재작업양품
                    dtSummary.Rows[i]["EOL_TOTAL_YEILD"] = (QUALITY_IN == 0 ? 0 : (((QUALITY_OUT + QUALITYRE_OUT) == 0 ? 0 : (QUALITY_OUT + QUALITYRE_OUT) / QUALITY_IN)));
                }

                DataSet rtnDataSet = new DataSet();
                rtnDataSet.Tables.Add(dtResult);
                rtnDataSet.Tables.Add(dtSummary);

                return rtnDataSet;
            }
            catch (Exception ex)
            {
                return ex;
            }

            return null;
        }

        private void SetData(DataTable dtTo, DataTable dtFrom, string sPrefix)
        {
            for (int i = 0; i < dtFrom.Rows.Count; i++)
            {
                DataRow[] dr = dtTo.Select(" CALDATE = '" + dtFrom.Rows[i]["CALDATE"].ToString() + "' AND EQSGID = '" + dtFrom.Rows[i]["EQSGID"].ToString() + "' AND MDLLOT_ID = '" + dtFrom.Rows[i]["MDLLOT_ID"].ToString() + "'");

                if (dr.Length > 0 && dtTo.Columns.Contains(sPrefix + dtFrom.Rows[i]["CODE"].ToString()))
                {
                    //dr[0][sPrefix + dtFrom.Rows[i]["CODE"].ToString()] = dtFrom.Rows[i]["VALUE"];
                    decimal dTmp = 0;
                    decimal dTmp_sub = 0;
                    if (decimal.TryParse(dtFrom.Rows[i]["VALUE"].ToString(), out dTmp))
                    {
                        //dr[0][sPrefix + dtFrom.Rows[i]["CODE"].ToString()] = dTmp;
                        //decimal.TryParse(dr[0][sPrefix + dtFrom.Rows[i]["CODE"].ToString()].ToString(), out dTmp_sub) + dTmp;
                        if (decimal.TryParse(dr[0][sPrefix + dtFrom.Rows[i]["CODE"].ToString()].ToString(), out dTmp_sub)) //동일 CODE 가 존재할 경우, 값을 더해줌
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
            else dr["1_CHARGE_DEGAS_B_YEILD"] = charge1_out / charge1_in;
            if (charge1re_in == 0) dr["1_CHARGE_DEGAS_B_REWORK_YEILD"] = 0;
            else dr["1_CHARGE_DEGAS_B_REWORK_YEILD"] = charge1re_out / charge1re_in;
            if (degas_in == 0) dr["DEGAS_YEILD"] = 0;
            else dr["DEGAS_YEILD"] = degas_out / degas_in;
            if (degasre_in == 0) dr["DEGAS_REWORK_YEILD"] = 0;
            else dr["DEGAS_REWORK_YEILD"] = degasre_out / degasre_in;
            if (charge2_in == 0) dr["2_CHARGE_DEGAS_A_YEILD"] = 0;
            else dr["2_CHARGE_DEGAS_A_YEILD"] = charge2_out / charge2_in;
            if (charge2re_in == 0) dr["2_CHARGE_DEGAS_A_REWORK_YEILD"] = 0;
            else dr["2_CHARGE_DEGAS_A_REWORK_YEILD"] = charge2re_out / charge2re_in;
            if (lowvolt_in == 0) dr["LOW_CAPA_BAD_PASS_YEILD"] = 0;
            else dr["LOW_CAPA_BAD_PASS_YEILD"] = lowvolt_out / lowvolt_in;
            if (lowvoltre_in == 0) dr["LOW_CAPA_BAD_PASS_REWORK_YEILD"] = 0;
            else dr["LOW_CAPA_BAD_PASS_REWORK_YEILD"] = lowvoltre_out / lowvoltre_in;
            if (eol_in == 0) dr["EOL_YEILD"] = 0;
            else dr["EOL_YEILD"] = eol_out / eol_in;
            if (eolre_in == 0) dr["EOL_REWORK_YEILD"] = 0;
            else dr["EOL_REWORK_YEILD"] = eolre_out / eolre_in;

            //20220725_C20220603-000198_2차 저전압 조건 추가 START
            // 2차 저전압 직행/재작업 Total
            if (b2LowVoltFlag)
            {
                decimal lowvolt2_in = Convert.ToDecimal(dr["2_LOW_CAPA_BAD_PASS_IN"].ToString());
                decimal lowvolt2_out = Convert.ToDecimal(dr["2_LOW_CAPA_BAD_PASS_OUT"].ToString());
                decimal lowvolt2re_in = Convert.ToDecimal(dr["2_LOW_CAPA_BAD_PASS_REWORK_IN"].ToString());
                decimal lowvolt2re_out = Convert.ToDecimal(dr["2_LOW_CAPA_BAD_PASS_REWORK_OUT"].ToString());

                if (lowvolt2_in == 0) dr["2_LOW_CAPA_BAD_PASS_YEILD"] = 0;
                else dr["2_LOW_CAPA_BAD_PASS_YEILD"] = lowvolt2_out / lowvolt2_in;
                if (lowvolt2re_in == 0) dr["2_LOW_CAPA_BAD_PASS_REWORK_YEILD"] = 0;
                else dr["2_LOW_CAPA_BAD_PASS_REWORK_YEILD"] = lowvolt2re_out / lowvolt2re_in;
            }
            //20220725_C20220603-000198_2차 저전압 조건 추가 END

            // 특성 양품 재작업 Total
            if (eolgre_in == 0) dr["EOL_GREWORK_YEILD"] = 0;
            else dr["EOL_GREWORK_YEILD"] = eolgre_out / eolgre_in;

            // 2022.12.22 전체양품율 추가
            //a : eol_in 직행투입
            //b : eol_out 직행양품
            //d : eolre_out 재작업양품
            if (eol_in == 0) dr["EOL_TOTAL_YEILD"] = 0;
            else dr["EOL_TOTAL_YEILD"] = (eol_in == 0 ? 0 : ((eol_out + eolre_out) == 0 ? 0 : (eol_out + eolre_out) / eol_in));

            dt.Rows.Add(dr);
        }

        //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) START
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
                        Control = new CheckBox()
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
        //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) END

        //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) START
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
        //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) END

        //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) START
        void SetResult(List<ResultElement> elementList, Grid grid)
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
        //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) END

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

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_CMN_ALL_ITEMS", "RQSTDT", "RSLTDT", dtRqstA);
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

        #region [Event]
        //private void btnSearch_Click(object sender, EventArgs e)
        //{
        //    GetList();
        //}

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            btnSearch.IsEnabled = false;

            object[] argument = new object[5];
            argument[0] = dtpFromDate.SelectedDateTime;
            argument[1] = dtpToDate.SelectedDateTime;
            argument[2] = Util.GetCondition(cboLine, bAllNull: true);
            argument[3] = Util.GetCondition(cboModel, bAllNull: true);
            //argument[4] = Util.GetCondition(cboLotType, bAllNull: true); // 2021.08.19 Lot 유형 검색조건 추가

            int iItemCnt = (cboLotType.ItemsSource == null ? 0 : ((DataView)cboLotType.ItemsSource).Count);
            int iSelectedCnt = (cboLotType.ItemsSource == null ? 0 : cboLotType.SelectedItemsToString.Split(',').Length);

            argument[4] = (iItemCnt == iSelectedCnt ? null : Util.ConvertEmptyToNull(cboLotType.SelectedItemsToString)); // 2023.02.07 LotType MultSelection 

            xProgress.Percent = 0;
            xProgress.ProgressText = string.Empty;
            xProgress.Visibility = Visibility.Visible;

            if (!bgWorker.IsBusy)
            {
                bgWorker.RunWorkerAsync(argument);
            }
        }

        //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) START
        //private void chk_Checked(object sender, EventArgs e)
        //{
        //    CheckBox chk = (CheckBox)sender;

        //    if (chk.Name == "chkAll")
        //    {

        //    }
        //    else if (chk.Name == "chk1Charge")
        //    {
        //        for (int i = dgProdResult.Columns["1_CHARGE_DEGAS_B_IN"].Index; i <= dgProdResult.Columns["1_CHARGE_DEGAS_B_LOSS"].Index; i++)
        //        {
        //            dgProdResult.Columns[i].Visibility = Visibility.Visible;
        //        }
        //    }
        //    else if (chk.Name == "chk1ChargeRe")
        //    {
        //        for (int i = dgProdResult.Columns["1_CHARGE_DEGAS_B_REWORK_IN"].Index; i <= dgProdResult.Columns["1_CHARGE_DEGAS_B_REWORK_LOSS"].Index; i++)
        //        {
        //            dgProdResult.Columns[i].Visibility = Visibility.Visible;
        //        }
        //    }
        //    else if (chk.Name == "chkDegas")
        //    {
        //        for (int i = dgProdResult.Columns["DEGAS_IN"].Index; i <= dgProdResult.Columns["DEGAS_LOSS"].Index; i++)
        //        {
        //            dgProdResult.Columns[i].Visibility = Visibility.Visible;
        //        }
        //    }
        //    else if (chk.Name == "chkDegasRe")
        //    {
        //        for (int i = dgProdResult.Columns["DEGAS_REWORK_IN"].Index; i <= dgProdResult.Columns["DEGAS_REWORK_LOSS"].Index; i++)
        //        {
        //            dgProdResult.Columns[i].Visibility = Visibility.Visible;
        //        }
        //    }
        //    else if (chk.Name == "chk2Charge")
        //    {
        //        for (int i = dgProdResult.Columns["2_CHARGE_DEGAS_A_IN"].Index; i <= dgProdResult.Columns["2_CHARGE_DEGAS_A_LOSS"].Index; i++)
        //        {
        //            dgProdResult.Columns[i].Visibility = Visibility.Visible;
        //        }
        //    }
        //    else if (chk.Name == "chk2ChargeRe")
        //    {
        //        for (int i = dgProdResult.Columns["2_CHARGE_DEGAS_A_REWORK_IN"].Index; i <= dgProdResult.Columns["2_CHARGE_DEGAS_A_REWORK_LOSS"].Index; i++)
        //        {
        //            dgProdResult.Columns[i].Visibility = Visibility.Visible;
        //        }
        //    }
        //    else if (chk.Name == "chkLowVolt")
        //    {
        //        //for (int i = dgProdResult.Columns["LOWVOLT_TOTAL_YEILD"].Index; i <= dgProdResult.Columns["LOWVOLT_LOSS"].Index; i++)
        //        //{
        //        //    dgProdResult.Columns[i].Visibility = Visibility.Visible;
        //        //}
        //        //for (int i = dgProdResult.Columns["LOW_CAPA_BAD_PASS_IN"].Index; i <= dgProdResult.Columns["LOW_CAPA_BAD_PASS_LOSS"].Index; i++)
        //        for (int i = dgProdResult.Columns["LOW_CAPA_BAD_PASS_IN"].Index; i <= dgProdResult.Columns["LOW_CAPA_BAD_PASS_REWORK_LOSS"].Index; i++)
        //        {
        //            dgProdResult.Columns[i].Visibility = Visibility.Visible;
        //        }
        //    }
        //    else if (chk.Name == "chkEol")
        //    {
        //        for (int i = dgProdResult.Columns["EOL_IN"].Index; i <= dgProdResult.Columns["EOL_LOSS"].Index; i++)
        //        {
        //            dgProdResult.Columns[i].Visibility = Visibility.Visible;
        //        }
        //    }
        //    else if (chk.Name == "chkEolRework")
        //    {
        //        for (int i = dgProdResult.Columns["EOL_REWORK_IN"].Index; i <= dgProdResult.Columns["EOL_REWORK_LOSS"].Index; i++)
        //        {
        //            dgProdResult.Columns[i].Visibility = Visibility.Visible;
        //        }
        //    }
        //}

        //private void chk_UnChecked(object sender, EventArgs e)
        //{
        //    CheckBox chk = (CheckBox)sender;

        //    if (chk.Name == "chkAll")
        //    {

        //    }
        //    else if (chk.Name == "chk1Charge")
        //    {
        //        for (int i = dgProdResult.Columns["1_CHARGE_DEGAS_B_IN"].Index; i <= dgProdResult.Columns["1_CHARGE_DEGAS_B_LOSS"].Index; i++)
        //        {
        //            dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
        //        }
        //    }
        //    else if (chk.Name == "chk1ChargeRe")
        //    {
        //        for (int i = dgProdResult.Columns["1_CHARGE_DEGAS_B_REWORK_IN"].Index; i <= dgProdResult.Columns["1_CHARGE_DEGAS_B_REWORK_LOSS"].Index; i++)
        //        {
        //            dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
        //        }
        //    }
        //    else if (chk.Name == "chkDegas")
        //    {
        //        for (int i = dgProdResult.Columns["DEGAS_IN"].Index; i <= dgProdResult.Columns["DEGAS_LOSS"].Index; i++)
        //        {
        //            dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
        //        }
        //    }
        //    else if (chk.Name == "chkDegasRe")
        //    {
        //        for (int i = dgProdResult.Columns["DEGAS_REWORK_IN"].Index; i <= dgProdResult.Columns["DEGAS_REWORK_LOSS"].Index; i++)
        //        {
        //            dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
        //        }
        //    }
        //    else if (chk.Name == "chk2Charge")
        //    {
        //        for (int i = dgProdResult.Columns["2_CHARGE_DEGAS_A_IN"].Index; i <= dgProdResult.Columns["2_CHARGE_DEGAS_A_LOSS"].Index; i++)
        //        {
        //            dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
        //        }
        //    }
        //    else if (chk.Name == "chk2ChargeRe")
        //    {
        //        for (int i = dgProdResult.Columns["2_CHARGE_DEGAS_A_REWORK_IN"].Index; i <= dgProdResult.Columns["2_CHARGE_DEGAS_A_REWORK_LOSS"].Index; i++)
        //        {
        //            dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
        //        }
        //    }
        //    else if (chk.Name == "chkLowVolt")
        //    {
        //        //for (int i = dgProdResult.Columns["LOWVOLT_TOTAL_YEILD"].Index; i <= dgProdResult.Columns["LOWVOLT_LOSS"].Index; i++)
        //        //{
        //        //    dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
        //        //}

        //        //for (int i = dgProdResult.Columns["LOW_CAPA_BAD_PASS_IN"].Index; i <= dgProdResult.Columns["LOW_CAPA_BAD_PASS_LOSS"].Index; i++)
        //        for (int i = dgProdResult.Columns["LOW_CAPA_BAD_PASS_IN"].Index; i <= dgProdResult.Columns["LOW_CAPA_BAD_PASS_REWORK_LOSS"].Index; i++)

        //        {
        //            dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
        //        }
        //    }
        //    else if (chk.Name == "chkEol")
        //    {
        //        for (int i = dgProdResult.Columns["EOL_IN"].Index; i <= dgProdResult.Columns["EOL_LOSS"].Index; i++)
        //        {
        //            dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
        //        }
        //    }
        //    else if (chk.Name == "chkEolRework")
        //    {
        //        for (int i = dgProdResult.Columns["EOL_REWORK_IN"].Index; i <= dgProdResult.Columns["EOL_REWORK_LOSS"].Index; i++)
        //        {
        //            dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
        //        }
        //    }
        //}
        //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) END

        //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) START
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
                    dgProdResult.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else if (chk.Name == "chk1CHARGERE")
            {
                for (int i = dgProdResult.Columns["1_CHARGE_DEGAS_B_REWORK_IN"].Index; i <= dgProdResult.Columns["1_CHARGE_DEGAS_B_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else if (chk.Name == "chkDEGAS")
            {
                for (int i = dgProdResult.Columns["DEGAS_IN"].Index; i <= dgProdResult.Columns["DEGAS_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else if (chk.Name == "chkDEGASRE")
            {
                for (int i = dgProdResult.Columns["DEGAS_REWORK_IN"].Index; i <= dgProdResult.Columns["DEGAS_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else if (chk.Name == "chk2CHARGE")
            {
                for (int i = dgProdResult.Columns["2_CHARGE_DEGAS_A_IN"].Index; i <= dgProdResult.Columns["2_CHARGE_DEGAS_A_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else if (chk.Name == "chk2CHARGERE")
            {
                for (int i = dgProdResult.Columns["2_CHARGE_DEGAS_A_REWORK_IN"].Index; i <= dgProdResult.Columns["2_CHARGE_DEGAS_A_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else if (chk.Name == "chkLOWVOLT")
            {
                for (int i = dgProdResult.Columns["LOW_CAPA_BAD_PASS_IN"].Index; i <= dgProdResult.Columns["LOW_CAPA_BAD_PASS_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else if (chk.Name == "chkEOL")
            {
                for (int i = dgProdResult.Columns["EOL_TOTAL_YEILD"].Index; i <= dgProdResult.Columns["EOL_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else if (chk.Name == "chkEOLRE")
            {
                for (int i = dgProdResult.Columns["EOL_REWORK_IN"].Index; i <= dgProdResult.Columns["EOL_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Visible;
                }
            }
            //20220328_제품검사의뢰 수량 산출 추가 START
            else if (chk.Name == "chkINSP_REQ")
            {
                for (int i = dgProdResult.Columns["INSP_REQ_LQC_DEGAS"].Index; i <= dgProdResult.Columns["INSP_REQ_PQC_ETC"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Visible;
                }
            }
            //20220328_제품검사의뢰 수량 산출 추가 END
            //20220725_C20220603-000198_2차 저전압 조건 추가 START
            else if (chk.Name == "chk2LOWVOLT")
            {
                for (int i = dgProdResult.Columns["2_LOW_CAPA_BAD_PASS_IN"].Index; i <= dgProdResult.Columns["2_LOW_CAPA_BAD_PASS_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Visible;
                }
            }
            //20220725_C20220603-000198_2차 저전압 조건 추가 END
            //2022.12.20 특성양품재작업 추가 Start
            else if (chk.Name == "chkEOLGRE")
            {
                for (int i = dgProdResult.Columns["EOL_GREWORK_IN"].Index; i <= dgProdResult.Columns["EOL_GREWORK_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Visible;
                }
            }
            //2022.12.20 특성양품재작업 추가 End
        }
        //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) END

        //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) START
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
                    dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else if (chk.Name == "chk1CHARGERE")
            {
                for (int i = dgProdResult.Columns["1_CHARGE_DEGAS_B_REWORK_IN"].Index; i <= dgProdResult.Columns["1_CHARGE_DEGAS_B_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else if (chk.Name == "chkDEGAS")
            {
                for (int i = dgProdResult.Columns["DEGAS_IN"].Index; i <= dgProdResult.Columns["DEGAS_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else if (chk.Name == "chkDEGASRE")
            {
                for (int i = dgProdResult.Columns["DEGAS_REWORK_IN"].Index; i <= dgProdResult.Columns["DEGAS_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else if (chk.Name == "chk2CHARGE")
            {
                for (int i = dgProdResult.Columns["2_CHARGE_DEGAS_A_IN"].Index; i <= dgProdResult.Columns["2_CHARGE_DEGAS_A_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else if (chk.Name == "chk2CHARGERE")
            {
                for (int i = dgProdResult.Columns["2_CHARGE_DEGAS_A_REWORK_IN"].Index; i <= dgProdResult.Columns["2_CHARGE_DEGAS_A_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else if (chk.Name == "chkLOWVOLT")
            {
                for (int i = dgProdResult.Columns["LOW_CAPA_BAD_PASS_IN"].Index; i <= dgProdResult.Columns["LOW_CAPA_BAD_PASS_REWORK_LOSS"].Index; i++)

                {
                    dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else if (chk.Name == "chkEOL")
            {
                for (int i = dgProdResult.Columns["EOL_TOTAL_YEILD"].Index; i <= dgProdResult.Columns["EOL_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else if (chk.Name == "chkEOLRE")
            {
                for (int i = dgProdResult.Columns["EOL_REWORK_IN"].Index; i <= dgProdResult.Columns["EOL_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            //20220328_제품검사의뢰 수량 산출 추가 START
            else if (chk.Name == "chkINSP_REQ")
            {
                for (int i = dgProdResult.Columns["INSP_REQ_LQC_DEGAS"].Index; i <= dgProdResult.Columns["INSP_REQ_PQC_ETC"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            //20220328_제품검사의뢰 수량 산출 추가 END
            //20220725_C20220603-000198_2차 저전압 조건 추가 START
            else if (chk.Name == "chk2LOWVOLT")
            {
                for (int i = dgProdResult.Columns["2_LOW_CAPA_BAD_PASS_IN"].Index; i <= dgProdResult.Columns["2_LOW_CAPA_BAD_PASS_REWORK_LOSS"].Index; i++)

                {
                    dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            //20220725_C20220603-000198_2차 저전압 조건 추가 END
            //2022.12.20 특성양품재작업 추가 Start
            else if (chk.Name == "chkEOLGRE")
            {
                for (int i = dgProdResult.Columns["EOL_GREWORK_IN"].Index; i <= dgProdResult.Columns["EOL_GREWORK_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            //2022.12.20 특성양품재작업 추가 End
        }
        //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) END

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
            //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) START
            //chk1Charge.IsChecked = true;
            //chk1ChargeRe.IsChecked = true;
            //chkDegas.IsChecked = true;
            //chkDegasRe.IsChecked = true;
            //chk2Charge.IsChecked = true;
            //chk2ChargeRe.IsChecked = true;
            //chkLowVolt.IsChecked = true;
            //chkEol.IsChecked = true;
            //chkEolRework.IsChecked = true;
            for (int idx = 0; idx < Area.Children.Count; idx++)
            {
                CheckBox chk = Area.Children[idx] as CheckBox;
                chk.IsChecked = true;
            }
            //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) END
        }

        private void chkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) START
            //chk1Charge.IsChecked = false;
            //chk1ChargeRe.IsChecked = false;
            //chkDegas.IsChecked = false;
            //chkDegasRe.IsChecked = false;
            //chk2Charge.IsChecked = false;
            //chk2ChargeRe.IsChecked = false;
            //chkLowVolt.IsChecked = false;
            //chkEol.IsChecked = false;
            //chkEolRework.IsChecked = false;
            for (int idx = 0; idx < Area.Children.Count; idx++)
            {
                CheckBox chk = Area.Children[idx] as CheckBox;
                chk.IsChecked = false;
            }
            //2021.08.30 저전압 위치 변경(Degas 재작업과 2차충전(Degas 후) 사이로 이동) END
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
                        if (dsData.Tables.Contains("RESULT")) dgProdResult.ItemsSource = DataTableConverter.Convert(dsData.Tables["RESULT"]);
                        if (dsData.Tables.Contains("SUMMARY")) dgProdResultSummary.ItemsSource = DataTableConverter.Convert(dsData.Tables["SUMMARY"]);
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
