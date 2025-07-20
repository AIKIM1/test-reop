/**************************************************************************************************************************
 Created Date : 2020.11.23
      Creator : Kang Dong Hee
   Decription : 특성측정기 Cell Data 조회
---------------------------------------------------------------------------------------------------------------------------
 [Change History]
  2020.11.23  NAME : Initial Created
  2021.03.31  KDH : 컬럼명 변경
  2021.04.13  KDH : 화면간 이동 시 초기화 현상 제거
  2022.06.03  KDH : COLDPRESS_IR_VALUE 컬럼 추가
  2022.07.05  이제섭 : 날짜 형식 변경(yyyyMMddHHmmss -> yyyy-MM-dd HH:mm:ss)
  2022.07.15  조영대 : 불량항목 체크리스트 콤보박스 양산과 양산이외 체크시 양산 체크 제외
  2022.08.23  강동희 : 에러 메시지 변경
  2022.08.24  조영대 : 이력 체크박스 체크 후 조회시 Merge 데이터 타입 오류 수정
  2022.09.01  김령호 : NB1동 VOC EOL7월7일 41번 요구사항 적용 (조회결과 CellID 좌측에 No 표시되도록)
  2022.09.01  조영대 : 데이터 없을시 sort 오류 수정.
  2022.09.07  조영대 : 엑셀 검색 추가
  2022.10.28  조영대 : 엑셀 검색 후 취소시 처리
  2022.12.28  이정미 : 법인마다 불량항목 체크리스트가 상이하여 수정(공통코드 -> 동별공통코드 있을 시 동별공통코드로 보여줌) 
                       IR 값이 DCIR 값으로 보이는 오류 수정  
  2023.08.15  손동혁 : NA 1동 요청 (무게 판정 상한 , 무게 판정 하한 , 잔존량 상한 , 잔존량 하한 ) 컬럼 동별공통코드 사용 NA 1동만 보이게 추가                   
  2023.09.12  조영대 : 오류 수정
  2023.10.22  김호선 : 2차 IR 측정값 표시 ESNJ ESS 전용
  2023.11.16  조영대 : 시작 종료 시간 오류 수정
  2023.12.18  조영대 : Index 오류 수정
  2024.01.09  지광현 : 검색 조건으로 SublotId 입력한 경우 SublotId 개수가 5000개를 초과하면 Alert 띄우도록 수정
  2024.01.31  윤가히 : ESOC 고전압 활성화 전용 조회조건 추가
***************************************************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// TSK_120.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_063 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();

        bool bUseFlag = false; //2023.08.15 컬럼 동별공통코드 사용 NA1동만 보이게 추가 테스트 후 삭제
        #endregion

        #region [Initialize]
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        System.ComponentModel.BackgroundWorker bgWorker = null;

        public FCS001_063()
        {
            InitializeComponent();

            bgWorker = new System.ComponentModel.BackgroundWorker();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.ProgressChanged += BgWorker_ProgressChanged;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            InitControl();
            SetEquipmentCombo(cboDefectItem);

            ///2023.08.15
            bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_062"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
            if (bUseFlag)
            {
                dgDefectCell.Columns["EL_RMN_UCL"].Visibility = System.Windows.Visibility.Visible;
                dgDefectCell.Columns["EL_RMN_LCL"].Visibility = System.Windows.Visibility.Visible;
                dgDefectCell.Columns["EOL_LCL_VALUE"].Visibility = System.Windows.Visibility.Visible;
                dgDefectCell.Columns["EOL_UCL_VALUE"].Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                dgDefectCell.Columns["EL_RMN_UCL"].Visibility = System.Windows.Visibility.Collapsed;
                dgDefectCell.Columns["EL_RMN_LCL"].Visibility = System.Windows.Visibility.Collapsed;
                dgDefectCell.Columns["EOL_LCL_VALUE"].Visibility = System.Windows.Visibility.Collapsed;
                dgDefectCell.Columns["EOL_UCL_VALUE"].Visibility = System.Windows.Visibility.Collapsed;

            }
            bool bColUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "IR_VAL_2ND"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.

            //ESNJ 전용 2차 IR 측정값 표시 여부
            if(bColUseFlag)
            {
                dgDefectCell.Columns["IR_VAL_2ND"].Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                dgDefectCell.Columns["IR_VAL_2ND"].Visibility = System.Windows.Visibility.Collapsed;
            }

            this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거
        }

        private void InitCombo()
        {
            try
            {
                CommonCombo_Form _combo = new CommonCombo_Form();

                C1ComboBox[] cboLineChild = { cboModel };
                _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

                C1ComboBox[] cboModelParent = { cboLine };
                _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitControl()
        {
            try
            {
                dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-1);
                dtpFromTime.DateTime = DateTime.Now.AddDays(-1);
                dtpToDate.SelectedDateTime = DateTime.Now;
                dtpToTime.DateTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void InitGrid()
        {
            dgDefectCell.ClearRows();

            if (chkmVDayView.IsChecked == true)
            {
                dgDefectCell.Columns["MV_START_OP"].Visibility = Visibility.Visible;
                dgDefectCell.Columns["MV_END_OP"].Visibility = Visibility.Visible;
                dgDefectCell.Columns["MV_START_TIME"].Visibility = Visibility.Visible;
                dgDefectCell.Columns["MV_END_TIME"].Visibility = Visibility.Visible;
                dgDefectCell.Columns["MV_DAY"].Visibility = Visibility.Visible;
                dgDefectCell.Columns["ALPHA"].Visibility = Visibility.Visible;
                dgDefectCell.Columns["BETA"].Visibility = Visibility.Visible;
                dgDefectCell.Columns["GAMMA"].Visibility = Visibility.Visible;
                dgDefectCell.Columns["EL_RMN_UCL"].Visibility = Visibility.Collapsed;
                dgDefectCell.Columns["EL_RMN_LCL"].Visibility = Visibility.Collapsed;
                dgDefectCell.Columns["EOL_LCL_VALUE"].Visibility = Visibility.Collapsed;
                dgDefectCell.Columns["EOL_UCL_VALUE"].Visibility = Visibility.Collapsed;
            }
            else
            {
                dgDefectCell.Columns["MV_START_OP"].Visibility = Visibility.Hidden;
                dgDefectCell.Columns["MV_END_OP"].Visibility = Visibility.Hidden;
                dgDefectCell.Columns["MV_START_TIME"].Visibility = Visibility.Hidden;
                dgDefectCell.Columns["MV_END_TIME"].Visibility = Visibility.Hidden;
                dgDefectCell.Columns["MV_DAY"].Visibility = Visibility.Hidden;
                dgDefectCell.Columns["ALPHA"].Visibility = Visibility.Hidden;
                dgDefectCell.Columns["BETA"].Visibility = Visibility.Hidden;
                dgDefectCell.Columns["GAMMA"].Visibility = Visibility.Hidden;
                if (bUseFlag)
                {
                    dgDefectCell.Columns["EL_RMN_UCL"].Visibility = Visibility.Visible;
                    dgDefectCell.Columns["EL_RMN_LCL"].Visibility = Visibility.Visible;
                    dgDefectCell.Columns["EOL_LCL_VALUE"].Visibility = Visibility.Visible;
                    dgDefectCell.Columns["EOL_UCL_VALUE"].Visibility = Visibility.Visible;
                }
            }
        }

        private void SetEquipmentCombo(MultiSelectionBox mcb)
        {
            try
            {
                DataTable dtRqstA = new DataTable();
                dtRqstA.TableName = "RQSTDT";
                dtRqstA.Columns.Add("LANGID", typeof(string));
                dtRqstA.Columns.Add("TYPE_CODE", typeof(string));
                
                DataRow drA = dtRqstA.NewRow();
                drA["LANGID"] = LoginInfo.LANGID;
                drA["TYPE_CODE"] = "EOL_JUDG_RSLT_CODE";
                dtRqstA.Rows.Add(drA);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_CMN_ALL_ITEMS", "RQSTDT", "RSLTDT", dtRqstA);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_CMN_ALL_ITEMS_CBO", "RQSTDT", "RSLTDT", dtRqstA);

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
                        for (int row = 0; row < dtResult.Rows.Count; row++)
                        {
                            //if (dtResult.Rows[row]["CBO_CODE"].ToString() == LoginInfo.CFG_EQPT_ID)
                            //{
                            //    mcb.Check(row);
                            //}
                        }
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

        #region [Method]
                
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

                string chkDAYVIEW = argument[5] == null ? null : argument[5].ToString();
                string sEQSGID = argument[6] == null ? null : argument[6].ToString();
                string sMDLLOT_ID = argument[7] == null ? null : argument[7].ToString();
                string sPROD_LOTID = argument[8] == null ? null : argument[8].ToString();
                string sCONTACTHIC_NG = argument[9] == null ? null : argument[9].ToString();
                string sSUBLOTID = argument[10] == null ? null : argument[10].ToString();
                string rdGRADEALL = argument[11] == null ? null : argument[11].ToString();
                string rdA = argument[12] == null ? null : argument[12].ToString();
                string rdNOTA = argument[13] == null ? null : argument[13].ToString();
                string rdCHOICE = argument[14] == null ? null : argument[14].ToString();
                string cboDEFECTITEM = argument[15] == null ? null : argument[15].ToString();
                string chkHISTORY = argument[16] == null ? null : argument[16].ToString();
                string txtGRADE = argument[17] == null ? null : argument[17].ToString();

                TimeSpan tsDateDiff = dToDate - dFromDate;

                string sBizId = string.Empty;
                string sBizIdH = string.Empty;

                //2024.01.31 윤가히S : ESOC 고전압 활성화 전용 조회조건 추가
                if (LoginInfo.CFG_AREA_ID == "AF" && (sEQSGID == null || sEQSGID == "AFHVF"))
                {
                    if (chkDAYVIEW != null)
                    {
                        sBizId = "DA_SEL_IROCV_NG_CELL_MVDAY_HVF";
                        sBizIdH = "DA_SEL_TH_IROCV_NG_CELL_MVDAY_HVF";
                    }
                    else
                    {
                        sBizId = "DA_SEL_IROCV_NG_CELL_HVF";
                        sBizIdH = "DA_SEL_TH_IROCV_NG_CELL_HVF";
                    }
                }
                else
                {
                    if (chkDAYVIEW != null)
                    {
                        sBizId = "DA_SEL_IROCV_NG_CELL_MVDAY";
                        sBizIdH = "DA_SEL_TH_IROCV_NG_CELL_MVDAY";
                    }
                    else
                    {
                        sBizId = "DA_SEL_IROCV_NG_CELL";
                        sBizIdH = "DA_SEL_TH_IROCV_NG_CELL";
                    }
                }
                

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("GRADE_A", typeof(string));
                dtRqst.Columns.Add("GRADE_B", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("CONTACTHIC_NG", typeof(string));
                dtRqst.Columns.Add("JUDGEVAL_LIST", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("CMCDTYPE", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (sExcelCell == null || string.IsNullOrEmpty(sExcelCell))
                {
                    if (string.IsNullOrEmpty(sSUBLOTID))
                    {
                        dr["FROM_DATE"] = dFromDate.ToString("yyyy-MM-dd") + " " + dFromTime.ToString("HH:mm:00");
                        dr["TO_DATE"] = dToDate.ToString("yyyy-MM-dd") + " " + dToTime.ToString("HH:mm:59");

                        dr["EQSGID"] = sEQSGID;
                        dr["MDLLOT_ID"] = sMDLLOT_ID;
                        dr["PROD_LOTID"] = sPROD_LOTID;
                        dr["CONTACTHIC_NG"] = sCONTACTHIC_NG;
                    }
                    else
                    {
                        dr["SUBLOTID"] = sSUBLOTID;
                    }
                }
                else
                {
                    dr["SUBLOTID"] = sExcelCell;
                }

                if (rdGRADEALL != null)
                {
                    dr["GRADE_A"] = null;
                    dr["GRADE_B"] = null;
                }
                else if (rdA != null)
                {
                    dr["GRADE_A"] = "A";
                    dr["GRADE_B"] = null;
                }
                else if (rdNOTA != null)
                {
                    dr["GRADE_A"] = null;
                    dr["GRADE_B"] = "A";
                }
                else if (rdCHOICE != null)
                {
                    dr["GRADE_A"] = txtGRADE;
                    dr["GRADE_B"] = null;
                }

                dr["JUDGEVAL_LIST"] = cboDEFECTITEM;

                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                //dr["CMCDTYPE"] = "EOL_JUDG_RSLT_CODE";
                dtRqst.Rows.Add(dr);

                DataTable dtRqstEQSGID = new DataTable();
                dtRqstEQSGID.TableName = "RQSTDT";
                dtRqstEQSGID.Columns.Add("LANGID", typeof(string));
                dtRqstEQSGID.Columns.Add("AREAID", typeof(string));
                dtRqstEQSGID.Columns.Add("EQSGID", typeof(string));

                DataRow drRqstEQSGID = dtRqstEQSGID.NewRow();
                drRqstEQSGID["LANGID"] = LoginInfo.LANGID;
                drRqstEQSGID["AREAID"] = LoginInfo.CFG_AREA_ID;
                if (!string.IsNullOrEmpty(sEQSGID))
                {
                    drRqstEQSGID["EQSGID"] = sEQSGID;
                }
                dtRqstEQSGID.Rows.Add(drRqstEQSGID);

                DataTable dtEQSGID = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_FORM", "RQSTDT", "RSLTDT", dtRqstEQSGID);

                DataSet dSRslt = new DataSet();
                int totalCount = (tsDateDiff.Days + 1);
                if (dtEQSGID.Rows.Count > 0) totalCount = (tsDateDiff.Days + 1) * dtEQSGID.Rows.Count + 1;
                int runCount = 0;

                for (int day = 0; day <= tsDateDiff.Days; day++)
                {
                    if (dr["SUBLOTID"] == null || string.IsNullOrEmpty(dr["SUBLOTID"].ToString()))
                    {
                        if (dFromDate.AddDays(day).ToString("yyyy-MM-dd").Equals(dFromDate.ToString("yyyy-MM-dd")))
                        {
                            dr["FROM_DATE"] = dFromDate.AddDays(day).ToString("yyyy-MM-dd") + " " + dFromTime.ToString("HH:mm:00");
                        }
                        else
                        {
                            dr["FROM_DATE"] = dFromDate.AddDays(day).ToString("yyyy-MM-dd") + " 00:00:00";
                        }

                        if (dFromDate.AddDays(day).ToString("yyyy-MM-dd").Equals(dToDate.ToString("yyyy-MM-dd")))
                        {
                            dr["TO_DATE"] = dFromDate.AddDays(day).ToString("yyyy-MM-dd") + " " + dToTime.ToString("HH:mm:59");
                        }
                        else
                        {
                            dr["TO_DATE"] = dFromDate.AddDays(day).ToString("yyyy-MM-dd") + " 23:59:59";
                        }
                    }

                    foreach (DataRow drEqsgid in dtEQSGID.Rows)
                    {
                        runCount++;

                        // SUBLOTID 가 있으면 라인 없이 한번만 실행.
                        if (dr["SUBLOTID"] != null && !string.IsNullOrEmpty(dr["SUBLOTID"].ToString()))
                        {
                            dr["EQSGID"] = null;
                        }
                        else
                        {
                            dr["EQSGID"] = drEqsgid["EQSGID"];
                        }
                        
                        bgWorker.ReportProgress(runCount * 100 / totalCount, "[" + dFromDate.AddDays(day).ToString("yyyy-MM-dd") + "] - " + (drEqsgid["EQSGID"] == null ? "" : drEqsgid["EQSGID"].ToString() + " : " + drEqsgid["EQSGNAME"].ToString()));

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizId, "RQSTDT", "RSLTDT", dtRqst);
                        if (dtResult == null)
                        {
                            dtResult = dtRslt;
                        }
                        else
                        {
                            dtResult.Merge(dtRslt, false, MissingSchemaAction.Ignore);
                        }
                        
                        if (chkHISTORY != null)
                        {
                            DataTable dtRsltH = new ClientProxy().ExecuteServiceSync(sBizIdH, "RQSTDT", "RSLTDT", dtRqst);
                            if (dtResult == null)
                            {
                                dtResult = dtRsltH;
                            }
                            else
                            {
                                dtResult.Merge(dtRsltH, false, MissingSchemaAction.Ignore);
                            }
                        }

                        // SUBLOTID 가 있으면 한번만 실행.
                        if (dr["SUBLOTID"] != null && !string.IsNullOrEmpty(dr["SUBLOTID"].ToString())) break;
                    }

                    // SUBLOTID 가 있으면 한번만 실행.
                    if (dr["SUBLOTID"] != null && !string.IsNullOrEmpty(dr["SUBLOTID"].ToString())) break;
                }

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dtResult.DefaultView.Sort = "CELL_ID ASC, MDF_TIME ASC";
                }
                else
                {
                    dtResult = null;
                }
                return dtResult;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private string LoadExcel()
        {

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
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region [Event]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            #region Validation
            //// 양품이 선택 체크
            //bool isGoodProduct = false;
            //DataTable dtList = DataTableConverter.Convert(cboDefectItem.ItemsSource);
            //foreach (var checkItem in cboDefectItem.SelectedItems)
            //{
            //    foreach (var item in dtList.AsEnumerable().Where(row => row["CBO_CODE"].Equals(checkItem)))
            //    {
            //        if (Util.NVC(item["ATTRIBUTE2"]).Equals("Y")) isGoodProduct = true;
            //    }
            //}

            //DateTime startDate = new DateTime(dtpFromDate.SelectedDateTime.Year, dtpFromDate.SelectedDateTime.Month, dtpFromDate.SelectedDateTime.Day,
            //                                  dtpFromTime.DateTime.Value.Hour, dtpFromTime.DateTime.Value.Minute, 0);
            //DateTime endDate = new DateTime(dtpToDate.SelectedDateTime.Year, dtpToDate.SelectedDateTime.Month, dtpToDate.SelectedDateTime.Day,
            //                                dtpToTime.DateTime.Value.Hour, dtpToTime.DateTime.Value.Minute, 0);

            //// 양품이 선택되고 날짜기간이 1일 이상일때 메세지 처리.
            //if (isGoodProduct && startDate.AddDays(1) < endDate)
            //{
            //    //불량항목 : 양품의 경우, 반드시 1개의 생산라인 및 조회기간(1일이내)을 선택해야 합니다.
            //    //Util.Alert("FM_ME_0439");

            //    //양품/불량은 함께 조회할 수 없습니다.\r\n양품 조회 시 한 개의 라인, 1일 이내로 조건을 설정해 주시기 바랍니다.
            //    Util.MessageInfo("FM_ME_0439");  
            //    return;
            //}

            if (string.IsNullOrEmpty(cboDefectItem.SelectedItemsToString))
            {
                Util.Alert("FM_ME_0150");  //불량항목을 두 개 이상 선택해야 합니다.
                return;
            }
            
            if (txtCellId.GetBindValue() != null) // SublotID가 입력되었고,
            {
                MatchCollection matches = Regex.Matches(txtCellId.GetBindValue().ToString(), ",");
                int cnt = matches.Count;
                if (cnt >= 5000) // 입력된 Sublot 개수가 5000개 초과하는 경우 alert
                {
                    Util.Alert("FM_ME_0458", "5000");  //5000개 까지 조회 가능합니다.
                    return;
                }
            }
            
            #endregion

            InitGrid(); // 그리드 초기화

            object[] argument = new object[18];
            argument[0] = null;
            argument[1] = dtpFromDate.SelectedDateTime;
            argument[2] = dtpToDate.SelectedDateTime;
            argument[3] = dtpFromTime.DateTime.Value;
            argument[4] = dtpToTime.DateTime.Value;
            argument[5] = chkmVDayView.IsChecked.Equals(true) ? "Y" : null;
            argument[6] = cboLine.GetBindValue();
            argument[7] = cboModel.GetBindValue();
            argument[8] = txtLotId.GetBindValue();
            argument[9] = chkCTLessThicNG.IsChecked.Equals(true) ? "Y" : null;
            argument[10] = txtCellId.GetBindValue();
            argument[11] = rdoGradeAll.IsChecked.Equals(true) ? "Y" : null;
            argument[12] = rdoA.IsChecked.Equals(true) ? "Y" : null;
            argument[13] = rdoNotA.IsChecked.Equals(true) ? "Y" : null;
            argument[14] = rdoChoice.IsChecked.Equals(true) ? "Y" : null;
            argument[15] = cboDefectItem.SelectedItemsToString;
            argument[16] = chkHistory.IsChecked.Equals(true) ? "Y" : null;
            argument[17] = txtGrade.GetBindValue();

            xProgress.Percent = 0;
            xProgress.ProgressText = string.Empty;
            xProgress.Visibility = Visibility.Visible;

            if (!bgWorker.IsBusy)
            {
                btnSearch.IsEnabled = false;
                bgWorker.RunWorkerAsync(argument);
            }
        }

        private void btnExcelSearch_Click(object sender, RoutedEventArgs e)
        {
            #region Validation            
            if (string.IsNullOrEmpty(cboDefectItem.SelectedItemsToString))
            {
                Util.Alert("FM_ME_0150");  //불량항목을 두 개 이상 선택해야 합니다.
                return;
            }
            #endregion

            InitGrid(); // 그리드 초기화

            object[] argument = new object[18];
            argument[0] = null;
            argument[1] = dtpFromDate.SelectedDateTime;
            argument[2] = dtpToDate.SelectedDateTime;
            argument[3] = dtpFromTime.DateTime.Value;
            argument[4] = dtpToTime.DateTime.Value;
            argument[5] = chkmVDayView.IsChecked.Equals(true) ? "Y" : null;
            argument[6] = cboLine.GetBindValue();
            argument[7] = cboModel.GetBindValue();
            argument[8] = txtLotId.GetBindValue();
            argument[9] = chkCTLessThicNG.IsChecked.Equals(true) ? "Y" : null;

            string cellList = LoadExcel();
            if (cellList.Equals(string.Empty)) return;
            argument[10] = cellList;

            argument[11] = rdoGradeAll.IsChecked.Equals(true) ? "Y" : null;
            argument[12] = rdoA.IsChecked.Equals(true) ? "Y" : null;
            argument[13] = rdoNotA.IsChecked.Equals(true) ? "Y" : null;
            argument[14] = rdoChoice.IsChecked.Equals(true) ? "Y" : null;
            argument[15] = cboDefectItem.SelectedItemsToString;
            argument[16] = chkHistory.IsChecked.Equals(true) ? "Y" : null;
            argument[17] = txtGrade.GetBindValue();

            xProgress.Percent = 0;
            xProgress.ProgressText = string.Empty;
            xProgress.Visibility = Visibility.Visible;

            if (!bgWorker.IsBusy)
            {
                btnSearch.IsEnabled = false;
                bgWorker.RunWorkerAsync(argument);
            }
        }

        private void txtCellId_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtCellId.Text)) && (e.Key == Key.Enter))
            {
                btnSearch_Click(null, null);
            }
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtLotId.Text)) && (e.Key == Key.Enter))
            {
                btnSearch_Click(null, null);
            }
        }

        private void txtGrade_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtGrade.Text)) && (e.Key == Key.Enter))
            {
                rdoChoice.IsChecked = true;
                btnSearch_Click(null, null);
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            new LGC.GMES.MES.Common.ExcelExporter().Export(dgDefectCell);
        }
        
        private void cboDefectItem_SelectionChanged(object sender, EventArgs e)
        {            
            //if (cboDefectItem.ItemsSource == null ||
            //    cboDefectItem.SelectedItems == null ||
            //    cboDefectItem.SelectedItems.Count == 0) return;

            //DataTable dtList = DataTableConverter.Convert(cboDefectItem.ItemsSource);
            //bool existsDivide = false;
            //bool existsOther = false;
            //foreach (var checkItem in cboDefectItem.SelectedItems)
            //{
            //    foreach (var item in dtList.AsEnumerable().Where(row => row["CBO_CODE"].Equals(checkItem)))
            //    {
            //        if (Util.NVC(item["ATTRIBUTE2"]).Equals("Y"))
            //        {
            //            existsDivide = true;
            //        }
            //        else
            //        {
            //            existsOther = true;
            //        }
            //    }
            //}

            //// 분리가 같이 선택되어져 있으면 분리 클리어
            //bool isGoodProduct = false;
            //if (existsDivide && existsOther)
            //{
            //    foreach (var checkItem in cboDefectItem.SelectedItems)
            //    {
            //        foreach (var item in dtList.AsEnumerable().Where(row => row["CBO_CODE"].Equals(checkItem)))
            //        {
            //            if (Util.NVC(item["ATTRIBUTE2"]).Equals("Y"))
            //            {
            //                cboDefectItem.Uncheck(checkItem);
            //                isGoodProduct = true;
            //            }
            //        }
            //    }
            //}

            //if (isGoodProduct)
            //{
            //    //불량항목 : 양품의 경우, 반드시 1개의 생산라인 및 조회기간(1일이내)을 선택해야 합니다.
            //    //Util.Alert("FM_ME_0439");
            //    Util.MessageInfo("FM_ME_0439");  //양품/불량은 함께 조회할 수 없습니다.\r\n양품 조회 시 한 개의 라인, 1일 이내로 조건을 설정해 주시기 바랍니다.
            //}

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
                            Util.GridSetData(dgDefectCell, dtData, this.FrameOperation, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            xProgress.Visibility = Visibility.Collapsed;
            xProgress.Percent = 0;

            btnSearch.IsEnabled = true;
        }

        //2022.09.01  김령호 : NB1동 VOC EOL7월7일 41번 요구사항 적용
        private void dgDefectCell_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            try
            {
                if (e.Row.HeaderPresenter == null)
                {
                    return;
                }

                e.Row.HeaderPresenter.Content = null;

                TextBlock tb = new TextBlock();

                int num = e.Row.Index + 1 - dgDefectCell.TopRows.Count;
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

        #endregion


    }
}
