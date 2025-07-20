/*************************************************************************************
 Created Date : 2020.12.15
      Creator : 박수미
   Decription : Degas 작업 Cell 정보조회
   --------------------------------------------------------------------------------------
 [Change History]
  2020.12.15  DEVELOPER : Initial Created.
  2022.08.22  조영대 : Cell ID 에서 엔터키 입력시 조회
  2022.08.24  조영대 : 불량 일때 색상 붉은색 표시, 집계시 설비 분리
  2022.11.08  조영대 : LOTID 조회 조건 추가
  2023.01.13  형준우 : 엑셀 조회 시 조회되지 않는 현상 수정
  2023.04.13  권순범 : PROD_LOTID 컬럼 추가 및 LOT ID -> Tray LOT ID 컬럼명칭 수정
  2023.04.24  권순범 : 메인 설비 콤보박스 추가 및 설비콤보박스 연동
  2023.04.26  권순범 : 메인 설비 라벨 추가 및 ComboStatus SELECT 수정, 모델아이디 ComboStatus SELECT 수정
  2023.05.08  권순범 : SubLotID or LOT ID로 조회 시 모델ID와 메인설비 상관없이 조회 가능하게 수정
  2023.08.15  손동혁 : NA 1동 요청 (진공 유지시간 ,진공 도달시간 , 무게 판정 상한 , 무게 판정 하한 ) 컬럼 동별공통코드 사용 NA1동만 보이게 추가  
  2023.11.16  조영대 : 시작 종료 시간 오류 수정
  2024.01.02  손동혁 : ESNA 특화 요청 절연전압 , 절연전압 불량여부 , 절연저항 -> 절연저항 , 절연저항 불량여부 , 절연전압 표시 추가
  2024.02.01  조영대 : 엑셀조회 시 배열 Index 오류 수정.
  2024.02.07  유진호 : 오창 고전압 활성화 주액 데이터 미존재로 인하여 DA 비즈룰 분기점 생성 (BizRuleID = "DA_SEL_TC_CELL_WEIGHT_MEAS_HVF)
  2024.03.18  최석준 : VACM_REACH_TIME, VACM_1ST_VENT_TIME, VACM_2ND_VENT_TIME, MAIN_SEAL_LOAD_CELL_PRESS_VALUE (MAIN_SEAL_WEIGHT_VALUE) 표시 추가
  2025.06.02  이원용 : IV, IR 값 바뀌어서 표현되던 오류 수정
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Configuration;
using System.IO;
using C1.WPF.Excel;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Input;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_062 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string _ModelId = string.Empty;
        private string _EqpId = string.Empty;
        private string _ChamberNo = string.Empty;
        private DateTime _dtFromDate = DateTime.Now.AddDays(-1);
        private DateTime _dtFromTime = DateTime.Now.AddDays(-1);
        private DateTime _dtToDate = DateTime.Now;
        private DateTime _dtToTime = DateTime.Now;
        Util _Util = new Util();

        bool bUseFlag = false; //2023.08.15 컬럼 동별공통코드 사용 NA1동만 보이게 추가 테스트 후 삭제
        bool bESNJUseFlag = false; // 2023.12.26 ESNJ 동별 컬럼 사용

        //기본 설정시간 parameter 추가
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;

        System.ComponentModel.BackgroundWorker bgWorker = null;

        public FCS001_062()
        {
            InitializeComponent();

            bgWorker = new System.ComponentModel.BackgroundWorker();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.ProgressChanged += BgWorker_ProgressChanged;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
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

        private void InitCombo()
        {

            CommonCombo_Form _combo = new CommonCombo_Form();

            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.SELECT, sCase: "MODEL");

            //C1ComboBox[] cboEqpChild = { cboChmNum };
            //_combo.SetCombo(cboEqp, CommonCombo_Form.ComboStatus.ALL, sCase: "DEGAS_SUBEQP", cbChild: cboEqpChild);

            //string[] sFilter = { "D", null, "M" };
            //_combo.SetCombo(cboEqp_Main, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPID", sFilter: sFilter);

            string[] sFilter = { "D", null, "M" };
            C1ComboBox[] cboEQP_MAINChild = { cboEqp };
            _combo.SetCombo(cboEqp_Main, CommonCombo_Form.ComboStatus.SELECT, sCase: "EQPID", cbChild: cboEQP_MAINChild, sFilter : sFilter);

            C1ComboBox[] cboEQPParent = { cboEqp_Main };
            _combo.SetCombo(cboEqp, CommonCombo_Form.ComboStatus.ALL, sCase: "DEGAS_SUBEQP", cbParent: cboEQPParent);
            
            string[] sBad = { "DEGAS_JUDG_RSLT_CODE" };
            _combo.SetCombo(cboBadYN, CommonCombo_Form.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sBad);

            // 2023.12.17 DEGAS 판정코드 공통과 상이하여 동별판정코드로 변경
            CommonCombo _combo2 = new CommonCombo();
            string[] sNewBad = { LoginInfo.CFG_AREA_ID, "DEGAS_JUDG_RSLT_CODE" };
            if (bESNJUseFlag)
            {
                
                _combo2.SetCombo(cboBadYN, CommonCombo.ComboStatus.ALL, sCase: "AREA_COMMON_CODE_BY_SLOC", sFilter: sNewBad);
            }
            else
            {
                _combo.SetCombo(cboBadYN, CommonCombo_Form.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sBad);
            }

            // C1ComboBox[] cboChamberParent = { cboEqp };
            //  string[] sFilterChamber = { "CHM" };
            // _combo.SetCombo(cboChmNum, CommonCombo_Form.ComboStatus.ALL, sCase: "SUBEQP", cbParent: cboChamberParent, sFilter: sFilterChamber);
            _combo.SetCombo(cboChmNum, CommonCombo_Form.ComboStatus.ALL, sCase: "DEGASCHM");
        }
        private void InitControl()
        {
            // Util 에 해당 함수 추가 필요.
            dtpFromDate.SelectedDateTime = GetJobDateFrom();
            dtpFromTime.DateTime = GetJobDateFrom();
            dtpToDate.SelectedDateTime = GetJobDateTo();
            dtpToTime.DateTime = GetJobDateTo();

            // 시간 제거
            dtpFromDate.SelectedDateTime = new DateTime(dtpFromDate.SelectedDateTime.Year, dtpFromDate.SelectedDateTime.Month, dtpFromDate.SelectedDateTime.Day);
            dtpToDate.SelectedDateTime = new DateTime(dtpToDate.SelectedDateTime.Year, dtpToDate.SelectedDateTime.Month, dtpToDate.SelectedDateTime.Day);
        }
        #endregion

        #region Method
        private void SetWorkResetTime()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREAATTR_FOR_OPENTIME", "RQSTDT", "RSLTDT", dtRqst);
            sWorkReSetTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
            sWorkEndTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
        }

        private DateTime GetJobDateFrom(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkReSetTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkReSetTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private DateTime GetJobDateTo(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkEndTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime, "yyyyMMdd HHmmss", null).AddSeconds(-1);
            return dJobDate;
        }

        private void GetList()
        {
            dgDegasCell.ClearRows();

            object[] argument = new object[13];
            argument[0] = null;
            argument[1] = dtpFromDate.SelectedDateTime;
            argument[2] = dtpToDate.SelectedDateTime;
            argument[3] = dtpFromTime.DateTime.Value;
            argument[4] = dtpToTime.DateTime.Value;
            argument[5] = cboModel.GetBindValue();
            argument[6] = cboEqp.GetBindValue();
            argument[7] = cboChmNum.GetBindValue();
            argument[8] = cboBadYN.GetBindValue();
            argument[9] = txtCellID.GetBindValue();
            argument[10] = txtLotID.GetBindValue();
            argument[11] = chkHist.IsChecked.Equals(true) ? "Y" : null;
            argument[12] = cboEqp_Main.GetBindValue();

            string sMDLID      = argument[5] == null ? null : argument[5].ToString();
            string sSUBLOTID   = argument[9] == null ? null : argument[9].ToString();
            string sLOTID      = argument[10] == null ? null : argument[10].ToString();
            string sMainEQPTID = argument[12] == null ? null : argument[12].ToString();
                     
            if (sSUBLOTID == null && sLOTID == null)
            {
                if (string.IsNullOrEmpty(sMDLID))
                {
                    //모델을 선택하세요.
                    Util.Alert("SFU1225");
                    return;
                }

                if (string.IsNullOrEmpty(sMainEQPTID))
                {
                    //설비를 선택하세요.
                    Util.Alert("SFU1153");
                    return;
                }  
            }

            txtCellID.Text = txtLotID.Text = string.Empty;

            xProgress.Percent = 0;
            xProgress.ProgressText = string.Empty;
            xProgress.Visibility = Visibility.Visible;

            if (!bgWorker.IsBusy)
            {
                btnSearch.IsEnabled = false;
                btnExcel.IsEnabled = false;

                bgWorker.RunWorkerAsync(argument);
            }
        }

        private void GetListExcel()
        {
            dgDegasCell.ClearRows();

            object[] argument = new object[13];
            argument[0] = LoadExcel();
            argument[1] = dtpFromDate.SelectedDateTime;
            argument[2] = dtpToDate.SelectedDateTime;
            argument[3] = dtpFromTime.DateTime.Value;
            argument[4] = dtpToTime.DateTime.Value;
            argument[5] = cboModel.GetBindValue();
            argument[6] = cboEqp.GetBindValue();
            argument[7] = cboChmNum.GetBindValue();
            argument[8] = cboBadYN.GetBindValue();
            argument[9] = txtCellID.GetBindValue();
            argument[10] = txtLotID.GetBindValue();
            argument[11] = chkHist.IsChecked.Equals(true) ? "Y" : null;

            xProgress.Percent = 0;
            xProgress.ProgressText = string.Empty;
            xProgress.Visibility = Visibility.Visible;

            if (!bgWorker.IsBusy)
            {
                btnSearch.IsEnabled = false;
                btnExcel.IsEnabled = false;

                bgWorker.RunWorkerAsync(argument);
            }
        }

        private object GetList(object arg)
        {
            try
            {
                DataTable dtResult = null;

                object[] argument = (object[])arg;

                string sExcelCell = argument[0] == null ? null : argument[0].ToString();

                DateTime dFromDate = (DateTime)argument[1];
                DateTime dToDate = (DateTime)argument[2];

                DateTime dFromTime = (DateTime)argument[3];
                DateTime dToTime = (DateTime)argument[4];

                string sMDLLOT_ID = argument[5] == null ? null : argument[5].ToString();
                string sEQPTID = argument[6] == null ? null : argument[6].ToString();                
                string sCHAMBER_LOCATION_NO = argument[7] == null ? null : argument[7].ToString();
                string sBAD_YN = argument[8] == null ? null : argument[8].ToString();
                string sSUBLOTID = argument[9] == null ? null : argument[9].ToString();
                string sLOTID = argument[10] == null ? null : argument[10].ToString();
                string chkHIST = argument[11] == null ? null : argument[11].ToString();
                string sEQPTID_MAIN = argument[12] == null ? null : argument[12].ToString();

                TimeSpan tsDateDiff = dToDate - dFromDate;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("FROM_TIME", typeof(string));
                dtRqst.Columns.Add("TO_TIME", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("EQPTID_MAIN", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("CHAMBER_LOCATION_NO", typeof(string));
                dtRqst.Columns.Add("BAD_YN", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("PAGE_NUM", typeof(string));                

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;

                if (sExcelCell == null || string.IsNullOrEmpty(sExcelCell))
                {
                    if (string.IsNullOrEmpty(sSUBLOTID) && string.IsNullOrEmpty(sLOTID))
                    {
                        dr["FROM_TIME"] = dFromDate.ToString("yyyy-MM-dd") + " " + dFromTime.ToString("HH:mm:00");
                        dr["TO_TIME"] = dToDate.ToString("yyyy-MM-dd") + " " + dToTime.ToString("HH:mm:59");
                        dr["MDLLOT_ID"] = sMDLLOT_ID;
                        dr["EQPTID"] = sEQPTID;
                        dr["CHAMBER_LOCATION_NO"] = sCHAMBER_LOCATION_NO;
                        dr["BAD_YN"] = sBAD_YN;
                    }
                    else
                    {
                        dr["SUBLOTID"] = sSUBLOTID;
                        dr["LOTID"] = sLOTID;
                    }
                }
                else
                {
                    dr["SUBLOTID"] = sExcelCell;
                }
                dtRqst.Rows.Add(dr);

                DataTable dtRqstEqpt = new DataTable();
                dtRqstEqpt.TableName = "RQSTDT";
                dtRqstEqpt.Columns.Add("LANGID", typeof(string));
                dtRqstEqpt.Columns.Add("AREAID", typeof(string));
                dtRqstEqpt.Columns.Add("EQPTID", typeof(string));
   
                DataRow drRqstEqpt = dtRqstEqpt.NewRow();
                drRqstEqpt["LANGID"] = LoginInfo.LANGID;
                drRqstEqpt["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRqstEqpt["EQPTID"] = sEQPTID_MAIN;  //MAIN 설비

                dtRqstEqpt.Rows.Add(drRqstEqpt);

                DataTable dtEqpt = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_DEGAS_SUBEQP", "RQSTDT", "RSLTDT", dtRqstEqpt);
                if (sEQPTID != null && !string.IsNullOrEmpty(sEQPTID))
                {
                    DataView dvEqpt = dtEqpt.DefaultView;
                    dvEqpt.RowFilter = "CBO_CODE = '" + sEQPTID + "'";
                    dtEqpt = dvEqpt.ToTable();
                }

                int totalCount = (tsDateDiff.Days + 1);
                if (dtEqpt.Rows.Count > 0) totalCount = (tsDateDiff.Days + 1) * dtEqpt.Rows.Count + 1;
                int runCount = 0;
                for (int day = 0; day <= tsDateDiff.Days; day++)
                {
                    if (string.IsNullOrEmpty(Util.NVC(dr["SUBLOTID"])) && string.IsNullOrEmpty(Util.NVC(dr["LOTID"])))
                    {
                        if (dFromDate.AddDays(day).ToString("yyyy-MM-dd").Equals(dFromDate.ToString("yyyy-MM-dd")))
                        {
                            dr["FROM_TIME"] = dFromDate.AddDays(day).ToString("yyyy-MM-dd") + " " + dFromTime.ToString("HH:mm:00");
                        }
                        else
                        {
                            dr["FROM_TIME"] = dFromDate.AddDays(day).ToString("yyyy-MM-dd") + " 00:00:00";
                        }

                        if (dFromDate.AddDays(day).ToString("yyyy-MM-dd").Equals(dToDate.ToString("yyyy-MM-dd")))
                        {
                            dr["TO_TIME"] = dFromDate.AddDays(day).ToString("yyyy-MM-dd") + " " + dToTime.ToString("HH:mm:59");
                        }
                        else
                        {
                            dr["TO_TIME"] = dFromDate.AddDays(day).ToString("yyyy-MM-dd") + " 23:59:59";
                        }
                    }

                    if (dtEqpt.Rows.Count == 0)
                    {    
                        dr["EQPTID"] = null;

                        DataTable dtRsltCnt = new ClientProxy().ExecuteServiceSync("DA_SEL_TC_CELL_WEIGHT_MEAS_TOTAL_CNT", "RQSTDT", "RSLTDT", dtRqst);
                        DataTable dtRsltCntH = new DataTable();

                        if (chkHIST != null)
                        {
                            dtRsltCntH = new ClientProxy().ExecuteServiceSync("DA_SEL_TH_CELL_WEIGHT_MEAS_TOTAL_CNT", "RQSTDT", "RSLTDT", dtRqst);
                        }
                        if (dtRsltCnt.Rows.Count > 0)
                        {
                            int iRowNumMax = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(dtRsltCnt.Rows[0]["ROW_NUM"]) / 10000));
                            int iRowNumMaxH = 0;
                            if (chkHIST != null && dtRsltCntH.Rows.Count > 0)
                            {
                                iRowNumMaxH = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(dtRsltCntH.Rows[0]["ROW_NUM"]) / 10000));
                            }

                            string BizRuleID = null;

                            // 오창 고전압 활성화 주액 데이터 미존재로 인하여 DA 비즈룰 분기점 생성
                            if (sEQPTID_MAIN == "A1FDEG701" && LoginInfo.CFG_AREA_ID == "AF")
                            {
                                BizRuleID = "DA_SEL_TC_CELL_WEIGHT_MEAS_HVF";
                            }
                            else
                            {
                                BizRuleID = "DA_SEL_TC_CELL_WEIGHT_MEAS";
                            }

                            for (int i = 1; i <= iRowNumMax; i++)
                            {
                                dtRqst.Rows[0]["PAGE_NUM"] = i * 10000;
                                if (dtResult == null)
                                {
                                    dtResult = new ClientProxy().ExecuteServiceSync(BizRuleID, "RQSTDT", "RSTDT", dtRqst);
                                }
                                else
                                {
                                    DataTable dtResultTc = new ClientProxy().ExecuteServiceSync(BizRuleID, "RQSTDT", "RSTDT", dtRqst);
                                    dtResult.Merge(dtResultTc, false, MissingSchemaAction.Ignore);
                                }
                            }

                            if (chkHIST != null)
                            {
                                // 오창 고전압 활성화 주액 데이터 미존재로 인하여 DA 비즈룰 분기점 생성
                                if (sEQPTID_MAIN == "A1FDEG701" && LoginInfo.CFG_AREA_ID == "AF")
                                {
                                    BizRuleID = "DA_SEL_TH_CELL_WEIGHT_MEAS_HVF";
                                }
                                else
                                {
                                    BizRuleID = "DA_SEL_TH_CELL_WEIGHT_MEAS";
                                }

                                for (int i = 1; i <= iRowNumMaxH; i++)
                                {
                                    bool bLast = false;
                                    if (i == iRowNumMaxH) bLast = true;
                                    dtRqst.Rows[0]["PAGE_NUM"] = i * 10000;
                                    if (dtResult == null)
                                    {
                                        dtResult = new ClientProxy().ExecuteServiceSync(BizRuleID, "RQSTDT", "RSTDT", dtRqst);
                                    }
                                    else
                                    {
                                        DataTable dtResultHc = new ClientProxy().ExecuteServiceSync(BizRuleID, "RQSTDT", "RSTDT", dtRqst);
                                        dtResult.Merge(dtResultHc, false, MissingSchemaAction.Ignore);
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        foreach (DataRow drEqpt in dtEqpt.Rows)
                        {
                            runCount++;

                            // SUBLOTID, LOTID 가 있으면 설비 없이 한번만 실행.
                            if (string.IsNullOrEmpty(Util.NVC(dr["SUBLOTID"])) && string.IsNullOrEmpty(Util.NVC(dr["LOTID"])))
                            {
                                dr["EQPTID"] = drEqpt["CBO_CODE"];
                            }
                            else
                            {
                                dr["EQPTID"] = null;
                            }

                            bgWorker.ReportProgress(runCount * 100 / totalCount, "[" + dFromDate.AddDays(day).ToString("yyyy-MM-dd") + "] - " + (drEqpt["CBO_CODE"] == null ? "" : drEqpt["CBO_CODE"].ToString() + " : " + drEqpt["CBO_NAME"].ToString()));

                            DataTable dtRsltCnt = new ClientProxy().ExecuteServiceSync("DA_SEL_TC_CELL_WEIGHT_MEAS_TOTAL_CNT", "RQSTDT", "RSLTDT", dtRqst);
                            DataTable dtRsltCntH = new DataTable();

                            if (chkHIST != null)
                            {
                                dtRsltCntH = new ClientProxy().ExecuteServiceSync("DA_SEL_TH_CELL_WEIGHT_MEAS_TOTAL_CNT", "RQSTDT", "RSLTDT", dtRqst);
                            }
                            if (dtRsltCnt.Rows.Count > 0)
                            {
                                int iRowNumMax = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(dtRsltCnt.Rows[0]["ROW_NUM"]) / 10000));
                                int iRowNumMaxH = 0;
                                if (chkHIST != null && dtRsltCntH.Rows.Count > 0)
                                {
                                    iRowNumMaxH = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(dtRsltCntH.Rows[0]["ROW_NUM"]) / 10000));
                                }

                                string BizRuleID = null;

                                // 오창 고전압 활성화 주액 데이터 미존재로 인하여 DA 비즈룰 분기점 생성
                                if (sEQPTID_MAIN == "A1FDEG701" && LoginInfo.CFG_AREA_ID == "AF")
                                {
                                    BizRuleID = "DA_SEL_TC_CELL_WEIGHT_MEAS_HVF";
                                }
                                else
                                {
                                    BizRuleID = "DA_SEL_TC_CELL_WEIGHT_MEAS";
                                }

                                for (int i = 1; i <= iRowNumMax; i++)
                                {
                                    dtRqst.Rows[0]["PAGE_NUM"] = i * 10000;
                                    if (dtResult == null)
                                    {
                                        dtResult = new ClientProxy().ExecuteServiceSync(BizRuleID, "RQSTDT", "RSTDT", dtRqst);
                                    }
                                    else
                                    {
                                        DataTable dtResultTc = new ClientProxy().ExecuteServiceSync(BizRuleID, "RQSTDT", "RSTDT", dtRqst);
                                        dtResult.Merge(dtResultTc, false, MissingSchemaAction.Ignore);
                                    }
                                }

                                if (chkHIST != null)
                                {
                                    // 오창 고전압 활성화 주액 데이터 미존재로 인하여 DA 비즈룰 분기점 생성
                                    if (sEQPTID_MAIN == "A1FDEG701" && LoginInfo.CFG_AREA_ID == "AF")
                                    {
                                        BizRuleID = "DA_SEL_TH_CELL_WEIGHT_MEAS_HVF";
                                    }
                                    else
                                    {
                                        BizRuleID = "DA_SEL_TH_CELL_WEIGHT_MEAS";
                                    }

                                    for (int i = 1; i <= iRowNumMaxH; i++)
                                    {
                                        bool bLast = false;
                                        if (i == iRowNumMaxH) bLast = true;
                                        dtRqst.Rows[0]["PAGE_NUM"] = i * 10000;
                                        if (dtResult == null)
                                        {
                                            dtResult = new ClientProxy().ExecuteServiceSync(BizRuleID, "RQSTDT", "RSTDT", dtRqst);
                                        }
                                        else
                                        {
                                            DataTable dtResultHc = new ClientProxy().ExecuteServiceSync(BizRuleID, "RQSTDT", "RSTDT", dtRqst);
                                            dtResult.Merge(dtResultHc, false, MissingSchemaAction.Ignore);
                                        }
                                    }
                                }
                            }
                            // SUBLOTID, LOTID 가 있으면 한번만 실행.
                            if (!string.IsNullOrEmpty(Util.NVC(dr["SUBLOTID"])) || !string.IsNullOrEmpty(Util.NVC(dr["LOTID"]))) break;
                        }
                        // SUBLOTID, LOTID 가 있으면 한번만 실행.
                        if (!string.IsNullOrEmpty(Util.NVC(dr["SUBLOTID"])) || !string.IsNullOrEmpty(Util.NVC(dr["LOTID"]))) break;
                    }

                    // SUBLOTID, LOTID 가 있으면 한번만 실행.
                    if (!string.IsNullOrEmpty(Util.NVC(dr["SUBLOTID"])) || !string.IsNullOrEmpty(Util.NVC(dr["LOTID"]))) break;
                }
                return dtResult;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
        
        private DataTable GetData(DataTable dtRqst, string BizRuleID)
        {
            try
            {
                DataTable result = new ClientProxy().ExecuteServiceSync(BizRuleID, "RQSTDT", "RSTDT", dtRqst);
                if (result.Rows.Count > 0)
                {
                    return result;
                }
                else
                {
                    return null;
                }                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return null;
        }
        
        private string LoadExcel()
        {

            DataTable dtInfo = DataTableConverter.Convert(dgDegasCell.ItemsSource);

            dtInfo.Clear();

            OpenFileDialog fd = new OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (.xlsx)|*.xlsx";

            string sColData = string.Empty;

            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    C1XLBook book = new C1XLBook();
                    book.Load(stream, FileFormat.OpenXml);
                    XLSheet sheet = book.Sheets[0];

                    for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                    {
                        // CELL ID;
                        if (sheet.GetCell(rowInx, 0) == null)
                            return sColData;
                        sColData += Util.NVC(sheet.GetCell(rowInx, 0).Text) + ",";
                    }
                }
            }

            return sColData;
        }


        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // 2023.12.17
                bESNJUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_062_NJ"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.

                InitCombo();
                SetWorkResetTime();
                InitControl();

                
               
               

                ///2023.08.15
                bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_062"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
                if (bUseFlag)
                {
                    //dgDegasCell.Columns["VACM_REACH_TIME"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["VACM_KEEP_TIME"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["VAC_VALUE_KPA"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["EL_RMN_UCL"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["EL_RMN_LCL"].Visibility = System.Windows.Visibility.Visible;

                    dgDegasCell.Columns["IVLTG_VALUE"].Header = "IV_VAL";
                    dgDegasCell.Columns["IVLTG_RSLT_CODE"].Header = "IV_BAD_YN";
                    dgDegasCell.Columns["IR_MEASR_VALUE"].Header = "IR_VAL";

                    dgDegasCell.Columns["DEGAS_FLOW_VALUE"].Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    //dgDegasCell.Columns["VACM_REACH_TIME"].Visibility = System.Windows.Visibility.Collapsed;
                    dgDegasCell.Columns["VACM_KEEP_TIME"].Visibility = System.Windows.Visibility.Collapsed;
                    dgDegasCell.Columns["VAC_VALUE_KPA"].Visibility = System.Windows.Visibility.Collapsed;
                    dgDegasCell.Columns["EL_RMN_UCL"].Visibility = System.Windows.Visibility.Collapsed;
                    dgDegasCell.Columns["EL_RMN_LCL"].Visibility = System.Windows.Visibility.Collapsed;

                }

                
                if (bESNJUseFlag)
                {
                    //dgDegasCell.Columns["VACM_REACH_TIME"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["VACM_KEEP_TIME"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["VAC_VALUE_KPA"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["VACM_REL_TIME"].Visibility = System.Windows.Visibility.Visible;
                    //dgDegasCell.Columns["VACM_DGR_VALUE"].Visibility = System.Windows.Visibility.Visible;  // 2023.12.26 진공도와 같은 값으로 제외
                    dgDegasCell.Columns["VACM_VENT_TIME"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["CHAMBER_CYCL_TIME"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["DEGAS_PRESS_VALUE"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["DEGAS_PRESS_TIME"].Visibility = System.Windows.Visibility.Visible;
                    // Pre Sealing
                    dgDegasCell.Columns["PRE_SEAL_PRESS_VALUE"].Visibility = System.Windows.Visibility.Visible;
                    // Main Sealing
                    //dgDegasCell.Columns["MAIN_SEAL_WEIGHT_VALUE"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["IVLTG_MEASR_PSTN_NO"].Visibility = System.Windows.Visibility.Visible;
                    // Degas
                    dgDegasCell.Columns["WEIGHT_MEASR_PSTN_NO"].Visibility = System.Windows.Visibility.Visible;
                    //dgDegasCell.Columns["EL_RMN_UCL"].Visibility = System.Windows.Visibility.Visible;
                    //dgDegasCell.Columns["EL_RMN_LCL"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["DEGAS_FLOW_VALUE"].Visibility = System.Windows.Visibility.Visible; // 2024.06.13 ESNJ ESS DEGAS 유량계 추가
                    dgDegasCell.Columns["EOL_UCL_VALUE"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["EOL_LCL_VALUE"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["DEGAS_ELCTRLT_RMN_QTY"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["DEGAS_DSCHG_VALUE"].Visibility = System.Windows.Visibility.Visible;
                    // HotPress
                    dgDegasCell.Columns["PRESS_TMPR"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["PRESS_TIME"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["PRESS_VALUE"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["INI_VLTG_VALUE"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["MIN_VLTG_VALUE"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["END_VLTG_VALUE"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["DOCV_PRESS_VALUE"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["DOCV_JUDG_RSLT"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["PRESS_VLTG_VALUE1"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["PRESS_VLTG_VALUE2"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["PRESS_VLTG_VALUE3"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["PRESS_VLTG_VALUE4"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["PRESS_VLTG_VALUE5"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["PRESS_VLTG_VALUE6"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["PRESS_VLTG_VALUE7"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["PRESS_VLTG_VALUE8"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["PRESS_VLTG_VALUE9"].Visibility = System.Windows.Visibility.Visible;
                    dgDegasCell.Columns["PRESS_VLTG_VALUE10"].Visibility = System.Windows.Visibility.Visible;
                }
         
                this.Loaded -= UserControl_Loaded;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (bgWorker.IsBusy)
            {
                bgWorker.CancelAsync();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            GetListExcel();
        }

        private void dgDegasCell_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            try
            {
                if (e.Row.HeaderPresenter == null)
                {
                    return;
                }

                e.Row.HeaderPresenter.Content = null;

                TextBlock tb = new TextBlock();

                int num = e.Row.Index + 1 - dgDegasCell.TopRows.Count;
                if (num > 0)
                {
                    tb.Text = num.ToString();
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    e.Row.HeaderPresenter.Content = tb;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgDegasCell_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null) return;

            dgDegasCell?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell == null || e.Cell.Presenter == null) return;

                e.Cell.Presenter.Foreground = System.Windows.Media.Brushes.Black;

                if (e.Cell.Column != null && e.Cell.Column.Name.ToString().Equals("IVLTG_RSLT_CODE"))
                {
                    if (e.Cell.Value != null && e.Cell.Value.Equals(ObjectDic.Instance.GetObjectName("DEFECT")))
                    {
                        e.Cell.Presenter.Foreground = System.Windows.Media.Brushes.Red;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = System.Windows.Media.Brushes.Black;
                    }
                }
            }));
        }

        private void txtCellID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtCellID.Text.Trim() == string.Empty) return;

                btnSearch.PerformClick();
            }
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtLotID.Text.Trim() == string.Empty) return;

                btnSearch.PerformClick();
            }
        }

        private void CellLotID_ClipboardPasted(object sender, DataObjectPastingEventArgs e, string text)
        {
            if (text.Contains(",")) btnSearch.PerformClick();
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
                else if (e.Result != null && e.Result is DataTable)
                {
                    {
                        DataTable dtData = (DataTable)e.Result;

                        if (dtData != null)
                        {
                            if (!dtData.Columns.Contains("LOTID")) dgDegasCell.Columns["LOTID"].Visibility = Visibility.Collapsed;
                            Util.GridSetData(dgDegasCell, dtData, this.FrameOperation, true);
                        }
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
            btnExcel.IsEnabled = true;
        }


        #endregion
    }
}
