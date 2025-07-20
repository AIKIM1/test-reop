/*************************************************************************************
 Created Date : 2016.11.24
      Creator : Jeong Hyeon Sik
   Decription : Pack 반품 화면 (Cell 포장- Cell반품화면[BOX001_017] 수정함)
--------------------------------------------------------------------------------------
 [Change History]
  2016.11.24 Jeong Hyeon Sik                Initial Created.
  2018.09.03 손우석                         Pallet UD 초기화 기능 추가
  2018.11.22 손우석 CSR ID 3840684          반품리스트 제품ID 추가 요청 요청번호 C20181109_40684
  2019.02.27 이상훈 CSR ID 3909890          GMES Pack UI 개선 요청 (4건) 요청번호 C20190129_09890, 반품현황 조회 항목 추가 , palletid, lotid, prodid
  2020.03.18 김민석 CSR ID 43407            PACK 반품 확정 조회 기능 개선 [요청번호] C20200319-000121 "ALL" 검색 조건 오류 수정
  2020.03.30 손우석 CSR ID 42348            VW Group GB/T 정보 전송 자동화 체계 구축 (배포) [요청번호] C20200317-000147
  2020.04.09 이재호 CSR ID 42348            VW Group GB/T 정보 전송 자동화 체계 구축 (배포) [요청번호] C20200317-000147
  2020.04.10 손우석 CSR ID 42348            VW Group GB/T 정보 전송 자동화 체계 구축 (배포) [요청번호] C20200317-000147
  2020.07.16 손우석 서비스 번호 80286       [생산PI팀] GMES 포르쉐/DRX 고객사향 GBT 데이터 전송 RECALL function 신규 개발의 건 [요청번호] C20200713-000190
  2020.09.29 조용수                         곽병욱 사원요청으로 화면 그리드 개선
  2020.10.06 손우석                         서비스 번호 80286 [생산PI팀] GMES 포르쉐/DRX 고객사향 GBT 데이터 전송 RECALL function 신규 개발의 건 [요청번호] C20200713-000190
  2020.10.08 염규범                         곽병욱 사원요청으로 화면 그리드 개선 추가 개선의 건
  2022.07.16 정용석 CSR ID C20220504-000287 [Pack생산]GMES 반품 확정 일괄 처리 기능 추가
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_024_TEMP : UserControl, IWorkArea
    {
        #region Member Variable Lists...
        private bool bRecallYN = false;
        private bool bDelveryYN = false;
        string xmlBody = string.Empty;
        string baseItemID = string.Empty;
        string fileFullName = string.Empty;

        Dictionary<int, Tuple<string, string, string, string, string>> dicCheck = new Dictionary<int, Tuple<string, string, string, string, string>>();
        PACK001_024_TEMP_DataHelper dataHelper = new PACK001_024_TEMP_DataHelper();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        public PACK001_024_TEMP()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function Lists...
        // 초기화
        private void Initialize()
        {
            List<Button> lstButton = new List<Button>();
            lstButton.Add(this.btnConfirm);
            Util.pageAuth(lstButton, FrameOperation.AUTHORITY);

            // 반품 Tab
            PackCommon.SearchRowCount(ref this.txtReturnCount, 0);
            PackCommon.SearchRowCount(ref this.txtReturnPalletCount, 0);
            PackCommon.SearchRowCount(ref this.txtReturnLOTCount, 0);
            this.dtpDateFromReturn.ApplyTemplate();
            this.dtpDateFromReturn.SelectedDateTime = DateTime.Now;
            this.dtpDateToReturn.ApplyTemplate();
            this.dtpDateToReturn.SelectedDateTime = DateTime.Now;

            PackCommon.SetC1ComboBox(this.dataHelper.GetAreaInfo(), this.cboAreaReturn, true, "-ALL-");
            this.cboAreaReturn.SelectedValue = LoginInfo.CFG_AREA_ID;

            // 반품현황 Tab
            PackCommon.SearchRowCount(ref this.txtReturnHistCount, 0);
            this.dtpDateFromReturnHist.ApplyTemplate();
            this.dtpDateFromReturnHist.SelectedDateTime = DateTime.Now;
            this.dtpDateToReturnHist.ApplyTemplate();
            this.dtpDateToReturnHist.SelectedDateTime = DateTime.Now;

            PackCommon.SetC1ComboBox(this.dataHelper.GetAreaInfo(), this.cboAreaReturnHist, true, "-SELECT-");
            this.cboAreaReturnHist.SelectedValue = LoginInfo.CFG_AREA_ID;

            // B2BI
            PackCommon.SetC1ComboBox(this.dataHelper.GetRecallTypeInfo(), this.cboRecallType, true, "-SELECT-");
        }

        // Tab Index 바뀌었을 때
        private void TabControl_SelectionChangedEvent(C1TabControl c1TabControl)
        {
            switch (c1TabControl.SelectedIndex)
            {
                case 0:             // PACK 반품
                case 1:             // PACK 반품 현황
                    break;
                case 2:             // Porsche J1 B2BI Recall
                    if (this.bRecallYN)
                    {
                        this.btnDevivery.IsEnabled = true;
                        this.btnRecall.IsEnabled = false;
                    }
                    else
                    {
                        this.btnDevivery.IsEnabled = false;
                        this.btnRecall.IsEnabled = true;
                    }
                    break;
            }
        }

        // 반품 Tab - 조회질
        private void SearchReturnList()
        {
            // Validation Check...
            TimeSpan timeSpan = this.dtpDateToReturnHist.SelectedDateTime.Date.Subtract(this.dtpDateFromReturnHist.SelectedDateTime.Date);
            if (timeSpan.Days < 0)
            {
                Util.MessageValidation("SFU3569");  // 조회 시작일자는 종료일자를 초과 할 수 없습니다.
                return;
            }

            this.dicCheck.Clear();
            Util.gridClear(this.dgReturn);
            Util.gridClear(this.dgReturnPallet);
            Util.gridClear(this.dgReturnLOT);
            PackCommon.SearchRowCount(ref this.txtReturnCount, 0);
            PackCommon.SearchRowCount(ref this.txtReturnPalletCount, 0);
            PackCommon.SearchRowCount(ref this.txtReturnLOTCount, 0);

            this.txtPalletID.Text = string.Empty;

            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();
            try
            {
                string areaID = this.cboAreaReturn.SelectedValue.ToString();
                string receiveIssueID = this.txtReturnID.Text;
                string fromDate = string.IsNullOrEmpty(receiveIssueID.Trim()) ? string.Format("{0:yyyyMMdd}", this.dtpDateFromReturn.SelectedDateTime.Date) : null;
                string toDate = string.IsNullOrEmpty(receiveIssueID.Trim()) ? string.Format("{0:yyyyMMdd}", this.dtpDateToReturn.SelectedDateTime.Date) : null;
                string Chk = this.chkDetail.IsChecked == true ? "Y" : null;
                DataTable dt = this.dataHelper.GetReturnList(areaID, receiveIssueID, fromDate, toDate, Chk);

                if (CommonVerify.HasTableRow(dt))
                {
                    PackCommon.SearchRowCount(ref this.txtReturnCount, dt.Rows.Count);
                    Util.GridSetData(this.dgReturn, dt, FrameOperation);

                    // Pallet 수량 합계
                    DataGridAggregatesCollection dataGridAggregatesCollection = new DataGridAggregatesCollection();
                    DataGridAggregateSum dataGridAggregateSum = new DataGridAggregateSum();
                    dataGridAggregateSum.ResultTemplate = this.dgReturn.Resources["ResultTemplate"] as DataTemplate;
                    dataGridAggregatesCollection.Add(dataGridAggregateSum);
                    DataGridAggregate.SetAggregateFunctions(this.dgReturn.Columns["PALLET_QTY"], dataGridAggregatesCollection);
                }
                else
                {
                    Util.MessageInfo("SFU2816");    // 조회결과가 없습니다
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            this.loadingIndicator.Visibility = Visibility.Collapsed;
        }

        // 반품 이력 Tab - 조회질
        private void SearchReturnHist()
        {
            // Validation Check...
            TimeSpan timeSpan = this.dtpDateToReturnHist.SelectedDateTime.Date.Subtract(this.dtpDateFromReturnHist.SelectedDateTime.Date);
            if (timeSpan.Days < 0)
            {
                Util.MessageValidation("SFU3569");  // 조회 시작일자는 종료일자를 초과 할 수 없습니다.
                return;
            }

            if (timeSpan.Days > 30)
            {
                Util.MessageValidation("SFU4466");
                return;
            }


            Util.gridClear(this.dgReturnHist);
            PackCommon.SearchRowCount(ref this.txtReturnHistCount, 0);
            this.txtPalletID.Text = string.Empty;

            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();
            try
            {
                string areaID = this.cboAreaReturnHist.SelectedValue.ToString();
                string locationID = this.cboShipToLocation.SelectedValue.ToString();
                string fromDate = string.Format("{0:yyyyMMdd}", this.dtpDateFromReturnHist.SelectedDateTime.Date);
                string toDate = string.Format("{0:yyyyMMdd}", this.dtpDateToReturnHist.SelectedDateTime.Date);
                DataTable dt = this.dataHelper.GetReturnHist(areaID, locationID, fromDate, toDate);

                if (CommonVerify.HasTableRow(dt))
                {
                    PackCommon.SearchRowCount(ref this.txtReturnHistCount, dt.Rows.Count);
                    Util.GridSetData(this.dgReturnHist, dt, FrameOperation);
                }
                else
                {
                    Util.MessageInfo("SFU2816");    // 조회결과가 없습니다
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            this.loadingIndicator.Visibility = Visibility.Collapsed;
        }

        // Porsche J1 B2BI Recall - 조회질 (UI에서 여러 순서도를 호출하는데 해당 내용은 새로운 순서도 만들어서 호출하게끔 변경해야함.)
        private void SearchB2BIRecall(string lotID = null)
        {
            if (lotID == null)
            {
                lotID = this.txtSearchLOTID.Text;
            }

            if (this.bRecallYN)
            {
                this.txtSearchLOTID.Text = string.Empty;
                return;
            }

            if (this.dgReCall.Rows.Count > 500)
            {
                Util.Alert("SFU8102");      // 최대 수량은 500개 입니다.
                return;
            }

            // Grid에 LOTID가 있는지 체크
            DataTable dtLOTIDDupCheck = DataTableConverter.Convert(this.dgReCall.ItemsSource);
            if (dtLOTIDDupCheck.AsEnumerable().Where(x => x.Field<string>("LOTID").Equals(lotID)).Count() > 0)
            {
                Util.MessageValidation("SFU8166");
                return;
            }

            // LOT 정보 유무 확인
            DataTable dtLOTCheck = this.dataHelper.GetMappingLOTID(lotID);
            if (!CommonVerify.HasTableRow(dtLOTCheck))
            {
                Util.MessageValidation("SFU7000", this.txtSearchLOTID.Text);    // LOTID[%1]에 해당하는 LOT이 없습니다.
                return;
            }

            // Porsche J1 제품 Chk
            DataTable dtPorscheRecallCheck = this.dataHelper.GetPorscheRecallCheck(dtLOTCheck.Rows[0]["LOTID"].ToString());
            if (!CommonVerify.HasTableRow(dtPorscheRecallCheck))
            {
                Util.MessageValidation("SFU8225", dtLOTCheck.Rows[0]["PRODID"].ToString());     // 제품코드 [%1], Porsche J1 B2BI Recall 대상이 아닙니다.
                return;
            }

            // GBT Length Check
            DataTable dtPorscheRecallGBTLengthCheck = this.dataHelper.GetPorscheRecallGBTLengthCheck(dtLOTCheck.Rows[0]["LOTID"].ToString());
            if (!CommonVerify.HasTableRow(dtPorscheRecallGBTLengthCheck))
            {
                Util.MessageValidation("SFU7000", dtLOTCheck.Rows[0]["LOTID"].ToString());    // LOTID[%1]에 해당하는 LOT이 없습니다.
                return;
            }

            if (!dtPorscheRecallGBTLengthCheck.Rows[0]["RESULT"].ToString().Trim().Equals("OK"))
            {
                Util.MessageValidation(dtPorscheRecallGBTLengthCheck.Rows[0]["RESULT_MSG"].ToString());
                return;
            }

            // Recall Info
            DataTable dtPorscheRecallInfo = this.dataHelper.GetPorscheRecallInfo(dtLOTCheck.Rows[0]["LOTID"].ToString());
            if (!CommonVerify.HasTableRow(dtPorscheRecallInfo))
            {
                Util.MessageValidation("SFU7000", dtLOTCheck.Rows[0]["LOTID"].ToString());    // LOTID[%1]에 해당하는 LOT이 없습니다.
                return;
            }

            // Grid Binding
            if (this.dgReCall.GetRowCount() <= 0)
            {
                Util.GridSetData(this.dgReCall, dtPorscheRecallInfo, FrameOperation);

                // 이부분은 왜넣었는지 모르겠음.
                this.xmlBody = string.Empty;
                this.xmlBody += dtPorscheRecallInfo.Rows[0]["TAG1"].ToString();
                this.xmlBody += dtPorscheRecallInfo.Rows[0]["GBT_ID"].ToString();
                this.xmlBody += dtPorscheRecallInfo.Rows[0]["TAG2"].ToString();
                this.xmlBody += dtPorscheRecallInfo.Rows[0]["DMC"].ToString();
                this.xmlBody += dtPorscheRecallInfo.Rows[0]["TAG3"].ToString();
                this.xmlBody += dtPorscheRecallInfo.Rows[0]["PARTNUMBER"].ToString();
                this.xmlBody += dtPorscheRecallInfo.Rows[0]["TAG4"].ToString();
                this.xmlBody += dtPorscheRecallInfo.Rows[0]["LOTID"].ToString();
                this.xmlBody += dtPorscheRecallInfo.Rows[0]["TAG5"].ToString();
                this.xmlBody += dtPorscheRecallInfo.Rows[0]["GEN_INDEX"].ToString();
                this.xmlBody += dtPorscheRecallInfo.Rows[0]["TAG6"].ToString();
                this.xmlBody += dtPorscheRecallInfo.Rows[0]["PRODUCTION_DATE"].ToString();
                this.xmlBody += dtPorscheRecallInfo.Rows[0]["TAG7"].ToString();
                this.xmlBody += dtPorscheRecallInfo.Rows[0]["CELL_GBT"].ToString();
                this.xmlBody += dtPorscheRecallInfo.Rows[0]["TAG8"].ToString();
                this.RtxtContent.AppendText(this.xmlBody);
            }
            else
            {
                DataTable dt = DataTableConverter.Convert(this.dgReCall.ItemsSource);
                dt.Merge(dtPorscheRecallInfo);
                Util.GridSetData(this.dgReCall, dt, FrameOperation);

                // 이부분은 왜넣었는지 모르겠음.
                xmlBody += dtPorscheRecallInfo.Rows[0]["TAG1"].ToString();
                xmlBody += dtPorscheRecallInfo.Rows[0]["GBT_ID"].ToString();
                xmlBody += dtPorscheRecallInfo.Rows[0]["TAG2"].ToString();
                xmlBody += dtPorscheRecallInfo.Rows[0]["DMC"].ToString();
                xmlBody += dtPorscheRecallInfo.Rows[0]["TAG3"].ToString();
                xmlBody += dtPorscheRecallInfo.Rows[0]["PARTNUMBER"].ToString();
                xmlBody += dtPorscheRecallInfo.Rows[0]["TAG4"].ToString();
                xmlBody += dtPorscheRecallInfo.Rows[0]["LOTID"].ToString();
                xmlBody += dtPorscheRecallInfo.Rows[0]["TAG5"].ToString();
                xmlBody += dtPorscheRecallInfo.Rows[0]["GEN_INDEX"].ToString();
                xmlBody += dtPorscheRecallInfo.Rows[0]["TAG6"].ToString();
                xmlBody += dtPorscheRecallInfo.Rows[0]["PRODUCTION_DATE"].ToString();
                xmlBody += dtPorscheRecallInfo.Rows[0]["TAG7"].ToString();
                xmlBody += dtPorscheRecallInfo.Rows[0]["CELL_GBT"].ToString();
                xmlBody += dtPorscheRecallInfo.Rows[0]["TAG8"].ToString();
                this.RtxtContent.AppendText(this.xmlBody);
            }

            this.txtSearchLOTID.Text = string.Empty;
        }

        // Porsche J1 B2BI Recall - ReCall
        private void RecallProcess()
        {
            try
            {
                if (this.bRecallYN)
                {
                    return;
                }

                // Validation Check..
                if (string.IsNullOrEmpty(this.txtDevice.Text))
                {
                    Util.MessageValidation("SFU8224");  // Device Name 필수입니다.
                    return;
                }

                if (string.IsNullOrEmpty(this.txtReason.Text))
                {
                    Util.MessageValidation("SFU1594");  // 사유를 입력하세요.
                    return;
                }

                if (this.dgReCall.GetRowCount() <= 0)
                {
                    Util.MessageValidation("SFU1663");  // 선택한 작업대상 LOT이 없습니다.
                    return;
                }

                if (string.IsNullOrEmpty(this.cboRecallType.SelectedValue.ToString()))
                {
                    Util.MessageValidation("SFU8251");  // Recall Type 필수 입니다.
                    return;
                }

                string lotList = DataTableConverter.Convert(this.dgReCall.ItemsSource).AsEnumerable().Select(x => x.Field<string>("LOTID")).Aggregate((current, next) => current + "," + next);
                DataTable dtResult = this.dataHelper.SetProscheRecall(lotList, this.txtReason.Text.ToString(), this.cboRecallType.SelectedValue.ToString(), this.txtDevice.Text.ToString());

                if (CommonVerify.HasTableRow(dtResult))
                {
                    Util.AlertInfo("SFU1270");      //저장되었습니다.

                    this.baseItemID = dtResult.Rows[0]["BAS_ITEM_ID"].ToString();
                    this.fileFullName = dtResult.Rows[0]["FILE_FULL_NAME"].ToString();
                    if (this.cboRecallType.SelectedValue.ToString() == "SCRAPPING")
                    {
                        PorscheB2BITabClear();
                    }
                    else
                    {
                        this.bRecallYN = true;
                        this.bDelveryYN = false;
                        this.btnRecall.IsEnabled = false;
                        this.btnDevivery.IsEnabled = true;
                    }
                }
                else
                {
                    this.baseItemID = string.Empty;
                    this.fileFullName = string.Empty;
                    this.bRecallYN = false;
                    this.bDelveryYN = true;
                    this.btnRecall.IsEnabled = true;
                    this.btnDevivery.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        // Porsche J1 B2BI Recall - Delivery
        private void DeliveryProcess()
        {
            try
            {
                if (this.bDelveryYN)
                {
                    return;
                }

                // Validation Check..
                if (this.dgReCall.GetRowCount() <= 0)
                {
                    Util.MessageValidation("SFU1663");  // 선택한 작업대상 LOT이 없습니다.
                    return;
                }

                DataTable dtIFSB2BICustData = this.dataHelper.GetIFSB2B2CustData(this.baseItemID, this.fileFullName);
                if (CommonVerify.HasTableRow(dtIFSB2BICustData))
                {
                    string lotList = DataTableConverter.Convert(this.dgReCall.ItemsSource).AsEnumerable().Select(x => x.Field<string>("LOTID")).Aggregate((current, next) => current + "," + next);
                    DataTable dtResult = this.dataHelper.SetPorscheDelivery(lotList);
                    Util.AlertInfo("SFU1270");      // 저장되었습니다.

                    this.PorscheB2BITabClear();
                    this.bRecallYN = false;
                    this.bDelveryYN = true;
                    this.baseItemID = string.Empty;
                    this.fileFullName = string.Empty;
                    this.btnRecall.IsEnabled = true;
                    this.btnDevivery.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        // Porsche J1 B2BI Recall - 화면 초기화
        private void PorscheB2BITabClear()
        {
            try
            {
                this.txtSearchLOTID.Text = string.Empty;
                this.txtDevice.Text = string.Empty;
                this.txtReason.Text = string.Empty;
                this.cboRecallType.SelectedIndex = 0;
                Util.gridClear(this.dgReCall);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 반품 Tab - Excel 버튼 클릭시
        private void ExcelUploadProcess()
        {
            this.txtLOTID.Text = string.Empty;
            DataTable dt = LoadExcelData();
            if (!CommonVerify.HasTableRow(dt))
            {
                return;
            }

            // Scan LOTID 입력
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var lotID = dt.Rows[i][0].ToString();
                if (string.IsNullOrEmpty(lotID))
                {
                    continue;
                }

                if (!this.AddReturnLOTProcess(lotID))
                {
                    break;
                }
            }
        }

        // 반품 Tab - Excel Data Load
        private DataTable LoadExcelData()
        {
            DataTable dtExcelData = new DataTable();
            try
            {

                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    openFileDialog.InitialDirectory = @"\\Client\C$";
                }
                openFileDialog.Filter = "Excel Files (*.xlsx,*.xls)|*.xlsx;*.xls";

                if (openFileDialog.ShowDialog() == true)
                {
                    using (Stream stream = openFileDialog.OpenFile())
                    {
                        dtExcelData = LoadExcelHelper.LoadExcelData(stream, 0, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtExcelData;
        }

        // 반품 Tab - LOTID 입력 또는 Excel Upload한 LOT Grid에 Insert (Excel Upload의 경우에는 반품수량 갯수의 LOT만큼 입력되고 그 뒤로는 Interlock침)
        private bool AddReturnLOTProcess(string lotID = null)
        {
            if (lotID == null)
            {
                lotID = this.txtLOTID.Text;
            }

            try
            {
                // Validation Check..
                if (this.dgReturnLOT.ItemsSource == null || this.dgReturnLOT.GetRowCount() <= 0 || this.dgReturnLOT.SelectedIndex < 0)
                {
                    Util.MessageValidation("SFU1651");      // 선택된 항목이 없습니다.
                    return false;
                }

                if (string.IsNullOrEmpty(lotID))
                {
                    Util.MessageValidation("SFU1813");      // 입력한 LOT ID 가 없습니다.
                    return false;
                }

                // 선택한 Pallet의 반품수량 가져오기
                int selectedIndex = this.dgReturnLOT.SelectedIndex;
                string selectedReceivedIssueID = DataTableConverter.GetValue(this.dgReturnLOT.Rows[selectedIndex].DataItem, "RCV_ISS_ID").ToString();
                string selectedPalletID = DataTableConverter.GetValue(this.dgReturnLOT.Rows[selectedIndex].DataItem, "BOXID").ToString();
                var issueQty = DataTableConverter.Convert(this.dgReturn.ItemsSource).AsEnumerable()
                                                 .Where(x => x.Field<string>("RCV_ISS_ID").Equals(selectedReceivedIssueID) &&
                                                             x.Field<string>("PALLETID").Equals(selectedPalletID)).Select(x => x.Field<decimal>("ISS_QTY")).FirstOrDefault();

                var lotQty = DataTableConverter.Convert(this.dgReturnLOT.ItemsSource).AsEnumerable()
                                                 .Where(x => x.Field<string>("RCV_ISS_ID").Equals(selectedReceivedIssueID) &&
                                                             x.Field<string>("BOXID").Equals(selectedPalletID)).Count();

                if (Convert.ToDouble(lotQty) >= Convert.ToDouble(issueQty))
                {
                    Util.MessageValidation("SFU1551");      // 반품 수량을 넘었습니다.
                    return false;
                }

                // Scan LOT 정보 조회
                DataTable dt = this.dataHelper.GetReturnLOTID(lotID, selectedReceivedIssueID, selectedPalletID);
                if (!CommonVerify.HasTableRow(dt))
                {
                    Util.MessageValidation("SFU1386");      // LOT정보가 없습니다.
                    return false;
                }

                // Scan LOT 중복 여부
                var lotCheck = DataTableConverter.Convert(this.dgReturnLOT.ItemsSource).AsEnumerable()
                                                 .Where(x => x.Field<string>("LOTID").Equals(dt.Rows[0]["LOTID"].ToString())).Count();
                if (lotCheck > 0)
                {
                    Util.MessageValidation("SFU1376", lotID); //중복 스캔되었습니다.
                    lotID = string.Empty;
                    return false;
                }

                // Scan LOT 정보 해당 Pallet에 추가
                this.dgReturnLOT.IsReadOnly = false;
                this.dgReturnLOT.CanUserAddRows = true;
                this.dgReturnLOT.BeginNewRow();
                this.dgReturnLOT.EndNewRow(true);
                this.dgReturnLOT.CanUserAddRows = false;
                this.dgReturnLOT.IsReadOnly = true;

                foreach (DataRow dr in dt.Rows)
                {
                    DataTableConverter.SetValue(this.dgReturnLOT.CurrentRow.DataItem, "RCV_ISS_ID", Util.NVC(dr["RCV_ISS_ID"]));
                    DataTableConverter.SetValue(this.dgReturnLOT.CurrentRow.DataItem, "LOTID", Util.NVC(dr["LOTID"]));
                    DataTableConverter.SetValue(this.dgReturnLOT.CurrentRow.DataItem, "BOXID", Util.NVC(dr["BOXID"]));
                    DataTableConverter.SetValue(this.dgReturnLOT.CurrentRow.DataItem, "PRODID", Util.NVC(dr["PRODID"]));
                    DataTableConverter.SetValue(this.dgReturnLOT.CurrentRow.DataItem, "PRDT_CLSS_CODE", Util.NVC(dr["PRDT_CLSS_CODE"]));
                    DataTableConverter.SetValue(this.dgReturnLOT.CurrentRow.DataItem, "NOTE", Util.NVC(dr["NOTE"]));
                }
                PackCommon.SearchRowCount(ref this.txtReturnLOTCount, this.dgReturnLOT.GetRowCount());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

            return true;
        }

        // 반품 Tab - 반품 LOT List에서 PalletID 선택했을 때 처리
        private void GetSelectedPalletID(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid c1DataGrid = (C1DataGrid)sender;
            if (c1DataGrid == null || c1DataGrid.GetRowCount() <= 0)
            {
                return;
            }

            Point point = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = c1DataGrid.GetCellFromPoint(point);
            if (cell == null || cell.Value == null)
            {
                return;
            }

            this.txtPalletID.Text = DataTableConverter.GetValue(c1DataGrid.Rows[cell.Row.Index].DataItem, "BOXID").ToString();
        }

        // 반품 Tab - BOM Check
        private bool ValidationConfirmReturnLOT()
        {
            try
            {
                // Validation Check...
                if (this.dgReturnLOT == null || this.dgReturnLOT.GetRowCount() <= 0)
                {
                    Util.MessageValidation("SFU1651");  // 선택된 항목이 없습니다.
                    return false;
                }

                // Issue ID, Pallet ID 쌍이 여러개임.
                DataTable dt = DataTableConverter.Convert(this.dgReturnLOT.ItemsSource);
                var returnConfirmList = dt.AsEnumerable().GroupBy(x => new
                {
                    RCV_ISS_ID = x.Field<string>("RCV_ISS_ID"),
                    PALLETID = x.Field<string>("BOXID")
                }).Select(grp => new
                {
                    RCV_ISS_ID = grp.Key.RCV_ISS_ID,
                    PALLETID = grp.Key.PALLETID,
                    LOT_QTY = grp.Count()
                });

                // 반품 수량 Validation...
                // Issue Qty & 반품확정할 LOT 갯수 가져오기
                DataTable dtReturnList = DataTableConverter.Convert(this.dgReturn.ItemsSource);
                var returnIssueQtyAndLOTQty = from d1 in dtReturnList.AsEnumerable()
                                              join d2 in returnConfirmList on new { RCV_ISS_ID = d1.Field<string>("RCV_ISS_ID"), PALLETID = d1.Field<string>("PALLETID") } equals new { RCV_ISS_ID = d2.RCV_ISS_ID, PALLETID = d2.PALLETID }
                                              select new
                                              {
                                                  RCV_ISS_ID = d1.Field<string>("RCV_ISS_ID"),
                                                  PALLETID = d1.Field<string>("PALLETID"),
                                                  R_PRODID = d1.Field<string>("PRODID"),
                                                  ISS_QTY = d1.Field<decimal>("ISS_QTY"),
                                                  LOT_QTY = d2.LOT_QTY
                                              };
                foreach (var item in returnIssueQtyAndLOTQty)
                {
                    if (Convert.ToDecimal(item.LOT_QTY).Equals(item.ISS_QTY))
                    {
                        continue;
                    }
                    Util.MessageValidation("SFU1555");      // 반품수량과 LOT 수량이 일치하지 않습니다.
                    return false;
                }

                // 자재 Validation...
                var returnIssueProductData = from d1 in dtReturnList.AsEnumerable()
                                             join d2 in dt.AsEnumerable() on new { RCV_ISS_ID = d1.Field<string>("RCV_ISS_ID"), PALLETID = d1.Field<string>("PALLETID") } equals new { RCV_ISS_ID = d2.Field<string>("RCV_ISS_ID"), PALLETID = d2.Field<string>("BOXID") }
                                             select new
                                             {
                                                 RCV_ISS_ID = d1.Field<string>("RCV_ISS_ID"),
                                                 PALLETID = d1.Field<string>("PALLETID"),
                                                 R_PRODID = d1.Field<string>("PRODID"),
                                                 PRODID = d2.Field<string>("PRODID")
                                             };
                DataSet ds = this.dataHelper.CheckReturnProductID(PackCommon.queryToDataTable(returnIssueProductData.ToList()));

                // 반품확정 확인 Popup
                var returnConfirmPopupData = from d1 in dtReturnList.AsEnumerable()
                                             join d2 in returnIssueQtyAndLOTQty on new { RCV_ISS_ID = d1.Field<string>("RCV_ISS_ID"), PALLETID = d1.Field<string>("PALLETID") } equals new { RCV_ISS_ID = d2.RCV_ISS_ID, PALLETID = d2.PALLETID }
                                             select new
                                             {
                                                 RCV_ISS_ID = d1.Field<string>("RCV_ISS_ID"),
                                                 PALLETID = d1.Field<string>("PALLETID"),
                                                 FROM_AREAID = d1.Field<string>("FROM_AREAID"),
                                                 FROM_SLOC_ID = d1.Field<string>("FROM_SLOC_ID"),
                                                 FROM_SLOC_ID_DESC = d1.Field<string>("FROM_SLOC_ID_DESC"),
                                                 PALLET_QTY = d1.Field<int>("PALLET_QTY"),

                                                 RCV_QTY = d2.LOT_QTY,
                                                 ISS_QTY = d1.Field<decimal>("ISS_QTY"),
                                                 ISS_NOTE = d1.Field<string>("ISS_NOTE")
                                             };

                PACK001_024_CONFIRM wndConfirm = new PACK001_024_CONFIRM();
                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = PackCommon.queryToDataTable(returnConfirmPopupData.ToList());
                    C1WindowExtension.SetParameters(wndConfirm, Parameters);
                    wndConfirm.Closed += new EventHandler(wndConfirm_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

            return true;
        }

        // 반품 Tab - 반품확정 Transaction
        private void ConfirmReturn(string returnNote)
        {
            try
            {
                // Validation Check...
                if (this.dgReturnLOT == null || this.dgReturnLOT.GetRowCount() <= 0)
                {
                    Util.MessageValidation("SFU1651");  // 선택된 항목이 없습니다.
                    return;
                }

                // Issue ID 기준으로 반품확정 돌리기.
                DataTable dt = DataTableConverter.Convert(this.dgReturnLOT.ItemsSource);
                var issueList = dt.AsEnumerable().GroupBy(x => x.Field<string>("RCV_ISS_ID")).Select(grp => new
                {
                    RCV_ISS_ID = grp.Key
                });

                foreach (var item in issueList)
                {
                    // Make INDATA
                    var queryINDATA = dt.AsEnumerable().Where(x => x.Field<string>("RCV_ISS_ID").Equals(item.RCV_ISS_ID)).GroupBy(x => x.Field<string>("RCV_ISS_ID")).Select(grp => new
                    {
                        SRCTYPE = SRCTYPE.SRCTYPE_UI,
                        LANGID = LoginInfo.LANGID,
                        RCV_ISS_ID = grp.Key,
                        AREAID = LoginInfo.CFG_AREA_ID,
                        RCV_QTY = grp.Count(),
                        PROCID = string.Empty,
                        USERID = LoginInfo.USERID,
                        RCV_NOTE = returnNote,
                        TRNF_SHOPID = LoginInfo.CFG_SHOP_ID
                    });

                    // Make INPALLET
                    var queryINPALLET = dt.AsEnumerable().Where(x => x.Field<string>("RCV_ISS_ID").Equals(item.RCV_ISS_ID)).GroupBy(x => x.Field<string>("BOXID")).Select(grp => new
                    {
                        BOXID = grp.Key,
                        RCV_QTY = grp.Count()
                    });

                    // Make INBOX
                    var queryINBOX = dt.AsEnumerable().Where(x => x.Field<string>("RCV_ISS_ID").Equals(item.RCV_ISS_ID)).Select(x => new
                    {
                        BOXID = x.Field<string>("BOXID"),
                        LOTID = x.Field<string>("LOTID")
                    });

                    // Make INLOT
                    var queryINLOT = queryINBOX.Select(x => new
                    {
                        BOXID = x.BOXID,
                        LOTID = x.LOTID,
                        NOTE = returnNote
                    });

                    DataTable dtINDATA = PackCommon.queryToDataTable(queryINDATA.ToList());
                    DataTable dtINPALLET = PackCommon.queryToDataTable(queryINPALLET.ToList());
                    DataTable dtINBOX = PackCommon.queryToDataTable(queryINBOX.ToList());
                    DataTable dtINLOT = PackCommon.queryToDataTable(queryINLOT.ToList());

                    DataSet ds = this.dataHelper.SetReturnList(dtINDATA, dtINPALLET, dtINBOX, dtINLOT);
                }

                Util.MessageInfo("SFU1275");    // 정상처리 되었습니다.
                this.SearchReturnList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
        }

        private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.TabControl_SelectionChangedEvent((C1TabControl)sender);
        }

        private void txtReturnID_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Key.Equals(Key.Enter))
            {
                return;
            }

            this.SearchReturnList();
        }

        private void btnSearchReturn_Click(object sender, RoutedEventArgs e)
        {
            this.SearchReturnList();
        }

        private void btnSearchReturnHist_Click(object sender, RoutedEventArgs e)
        {
            this.SearchReturnHist();
        }

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.AddReturnLOTProcess(null);
                this.txtLOTID.Text = string.Empty;
            }
        }

        private void btnExcelUpload_Click(object sender, RoutedEventArgs e)
        {
            this.loadingIndicator.Visibility = Visibility.Visible;
            this.ExcelUploadProcess();
            this.loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isReceiveIDExists = false;
                Button button = (Button)sender;
                int selectedIndex = ((DataGridCellPresenter)button.Parent).Row.Index;

                // 삭제했을 경우 삭제한 구루마 정보 찾아서
                string selectedReceiveIssueID = Util.NVC(DataTableConverter.GetValue(this.dgReturnLOT.Rows[selectedIndex].DataItem, "RCV_ISS_ID")).ToString();

                // 선택된 행 삭제
                this.dgReturnLOT.IsReadOnly = false;
                this.dgReturnLOT.CanUserRemoveRows = true;
                this.dgReturnLOT.RemoveRow(selectedIndex);
                this.dgReturnLOT.CanUserRemoveRows = false;
                this.dgReturnLOT.IsReadOnly = true;
                PackCommon.SearchRowCount(ref this.txtReturnLOTCount, this.dgReturnLOT.GetRowCount());

                // 삭제한 구루마 정보가 존재하는지 Check...
                for (int i = 0; i < this.dgReturnLOT.GetRowCount(); i++)
                {
                    string issueID = Util.NVC(this.dgReturnLOT.GetCell(i, this.dgReturnLOT.Columns["RCV_ISS_ID"].Index).Value);
                    if (issueID.Equals(selectedReceiveIssueID))
                    {
                        isReceiveIDExists = true;
                        break;
                    }
                }

                if (!isReceiveIDExists)
                {
                    // 구루마 그리드에서 선택한 구루마 삭제.
                    for (int i = this.dgReturnPallet.GetRowCount() - 1; i >= 0; i--)
                    {
                        string issueID = Util.NVC(this.dgReturnPallet.GetCell(i, this.dgReturnPallet.Columns["RCV_ISS_ID"].Index).Value);
                        if (!issueID.Equals(selectedReceiveIssueID))
                        {
                            continue;
                        }

                        // 선택표시 삭제 & 체크해제된것 삭제
                        this.dgReturnPallet.IsReadOnly = false;
                        this.dgReturnPallet.CanUserRemoveRows = true;
                        this.dgReturnPallet.RemoveRow(i);
                        this.dgReturnPallet.CanUserRemoveRows = false;
                        this.dgReturnPallet.IsReadOnly = true;
                    }
                    PackCommon.SearchRowCount(ref this.txtReturnPalletCount, 0);

                    for (int i = this.dgReturn.GetRowCount() - 1; i >= 0; i--)
                    {
                        string issueID = Util.NVC(this.dgReturn.GetCell(i, this.dgReturn.Columns["RCV_ISS_ID"].Index).Value);
                        if (!issueID.Equals(selectedReceiveIssueID))
                        {
                            continue;
                        }

                        // 선택표시 삭제 & 체크해제된것 삭제
                        DataTableConverter.SetValue(this.dgReturn.Rows[i].DataItem, "CHK", false);
                    }
                    return;
                }

                // Master Grid에 선택된거 삭제
                if (this.dgReturnLOT.GetRowCount() <= 0)
                {
                    // 구루마 그리드에서 선택한 구루마 삭제.
                    Util.gridClear(this.dgReturnPallet);
                    PackCommon.SearchRowCount(ref this.txtReturnPalletCount, 0);

                    for (int i = this.dgReturn.GetRowCount() - 1; i >= 0; i--)
                    {
                        // 선택표시 삭제 & 체크해제된것 삭제
                        DataTableConverter.SetValue(this.dgReturn.Rows[i].DataItem, "CHK", false);
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            this.ValidationConfirmReturnLOT();
        }

        private void cboAreaReturnHist_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string fromAreaID = e.NewValue.ToString();
            PackCommon.SetC1ComboBox(this.dataHelper.GetShipToLocation(fromAreaID), this.cboShipToLocation, "-ALL-");
        }

        private void btnLOTSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchB2BIRecall();
        }

        private void btnRecall_Click(object sender, RoutedEventArgs e)
        {
            this.RecallProcess();
        }

        private void btnDevivery_Click(object sender, RoutedEventArgs e)
        {
            this.DeliveryProcess();
        }

        private void txtSearchLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.SearchB2BIRecall();
            }
        }

        private void txtSearchLOTID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // LOT N빵 입력
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (bRecallYN)
                {
                    this.txtSearchLOTID.Text = string.Empty;
                    return;
                }

                this.loadingIndicator.Visibility = Visibility.Visible;

                string[] stringSeparators = new string[] { "\r\n" };
                string sPasteString = Clipboard.GetText();
                string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                if (sPasteStrings.Count() > 100)
                {
                    Util.MessageValidation("SFU3695");   // 최대 100개 까지 가능합니다.
                    return;
                }

                for (int i = 0; i < sPasteStrings.Length; i++)
                {
                    if (string.IsNullOrEmpty(sPasteStrings[i]))
                    {
                        Util.MessageInfo("SFU1190", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                this.loadingIndicator.Visibility = Visibility.Collapsed;
                                return;
                            }
                        });
                    }
                    this.SearchB2BIRecall(sPasteStrings[i]);
                    System.Windows.Forms.Application.DoEvents();
                }
            }
            else
            {
                e.Handled = false;
            }
            //e.Handled = true;
            this.loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
            {
                return;
            }
            CheckBox checkBox = (CheckBox)sender;
            if (checkBox.DataContext == null)
            {
                return;
            }

            int selectedIndex = ((DataGridCellPresenter)checkBox.Parent).Row.Index;
            if (selectedIndex == -1)
            {
                return;
            }
            string selectedReceiveIsueID = Util.NVC(DataTableConverter.GetValue(this.dgReturn.Rows[selectedIndex].DataItem, "RCV_ISS_ID")).ToString();
            string selectedPalletID = Util.NVC(DataTableConverter.GetValue(this.dgReturn.Rows[selectedIndex].DataItem, "PALLETID")).ToString();
            string selectedProductID = Util.NVC(DataTableConverter.GetValue(this.dgReturn.Rows[selectedIndex].DataItem, "PRODID")).ToString();
            string selectedLOTInfo = Util.NVC(DataTableConverter.GetValue(this.dgReturn.Rows[selectedIndex].DataItem, "LOT_INFO")).ToString();
            string selectedOcopFlag = Util.NVC(DataTableConverter.GetValue(this.dgReturn.Rows[selectedIndex].DataItem, "OCOP_RTN_FLAG")).ToString();

            // 이미 체크된것이 있으면 LOT YN값 CHECK
            if (this.dicCheck.Count > 0)
            {
                string checkProductID = this.dicCheck.GroupBy(grp => grp.Value.Item3).Select(y => y.Key).FirstOrDefault();    // Key = gridIndex, Item1 = IssueID, Item2 = PalletID, Item3 = ProductID, Item4 = Lot Info
                string checkLOTInfo = this.dicCheck.GroupBy(grp => grp.Value.Item4).Select(y => y.Key).FirstOrDefault();    // Key = gridIndex, Item1 = IssueID, Item2 = PalletID, Item3 = ProductID, Item4 = Lot Info

                // 이미 체크된 것의 LOT INFO 값이 N이면 다른거 선택 못하게 막기
                if (checkLOTInfo.Equals("N"))
                {
                    DataTableConverter.SetValue(this.dgReturn.Rows[selectedIndex].DataItem, "CHK", false);
                    return;
                }

                // 이미 체크된 것의 LOT INFO 값이 Y이면 선택된 것의 제품정보가 동일한 것만 체크되도록 Valudation...
                if (checkLOTInfo.Equals("Y") && (!checkLOTInfo.Equals(selectedLOTInfo) || !checkProductID.Equals(selectedProductID)))
                {
                    DataTableConverter.SetValue(this.dgReturn.Rows[selectedIndex].DataItem, "CHK", false);
                    return;
                }
            }

            // 체크된것 저장.
            Tuple<string, string, string, string, string> tuple = new Tuple<string, string, string, string, string>(selectedReceiveIsueID, selectedPalletID, selectedProductID, selectedLOTInfo, selectedOcopFlag);
            // 화면 Scroll시에 이 이벤트가 다시 떠버리는 현상이 발생됨.
            if (!this.dicCheck.ContainsKey(selectedIndex))
            {
                this.dicCheck.Add(selectedIndex, tuple);
                // Pallet Grid에 Data Binding
                DataTable dtPalletInfo = this.dataHelper.GetPalletInfo(selectedReceiveIsueID);
                if (this.dgReturnPallet.GetRowCount() <= 0)
                {
                    PackCommon.SearchRowCount(ref this.txtReturnPalletCount, dtPalletInfo.Rows.Count);
                    Util.GridSetData(this.dgReturnPallet, dtPalletInfo, FrameOperation);
                }
                else
                {
                    DataTable dtLoadedPalletList = DataTableConverter.Convert(this.dgReturnPallet.ItemsSource);
                    dtLoadedPalletList.Merge(dtPalletInfo);
                    PackCommon.SearchRowCount(ref this.txtReturnPalletCount, dtLoadedPalletList.Rows.Count);
                    Util.GridSetData(this.dgReturnPallet, dtLoadedPalletList, FrameOperation);
                }

                // LOT List Grid에 DataBinding
                DataTable dt = this.dataHelper.GetReturnLOTList(selectedReceiveIsueID, selectedPalletID, selectedOcopFlag);
                if (this.dgReturnLOT.GetRowCount() <= 0)
                {
                    PackCommon.SearchRowCount(ref this.txtReturnLOTCount, dt.Rows.Count);
                    Util.GridSetData(this.dgReturnLOT, dt, FrameOperation);
                }
                else
                {
                    DataTable dtLoadedLOTList = DataTableConverter.Convert(this.dgReturnLOT.ItemsSource);
                    dtLoadedLOTList.Merge(dt);
                    PackCommon.SearchRowCount(ref this.txtReturnLOTCount, dtLoadedLOTList.Rows.Count);
                    Util.GridSetData(this.dgReturnLOT, dtLoadedLOTList, FrameOperation);
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
            {
                return;
            }
            CheckBox checkBox = (CheckBox)sender;
            if (checkBox.DataContext == null)
            {
                return;
            }

            bool isExists = false;
            int selectedIndex = ((DataGridCellPresenter)checkBox.Parent).Row.Index;
            string selectedReceiveIssueID = Util.NVC(DataTableConverter.GetValue(this.dgReturn.Rows[selectedIndex].DataItem, "RCV_ISS_ID")).ToString();
            string selectedPalletID = Util.NVC(DataTableConverter.GetValue(this.dgReturn.Rows[selectedIndex].DataItem, "PALLETID")).ToString();

            // LOT List Grid에서 선택한 PalletID에 해당하는 LOT List 삭제
            this.dgReturnLOT.IsReadOnly = false;
            this.dgReturnLOT.CanUserRemoveRows = true;
            this.dgReturnLOT.BeginEdit();
            for (int i = this.dgReturnLOT.GetRowCount() - 1; i >= 0; i--)
            {
                string receiveIssueID = Util.NVC(DataTableConverter.GetValue(this.dgReturnLOT.Rows[i].DataItem, "RCV_ISS_ID")).ToString();
                string palletID = Util.NVC(DataTableConverter.GetValue(this.dgReturnLOT.Rows[i].DataItem, "BOXID")).ToString();
                if (!receiveIssueID.Equals(selectedReceiveIssueID) || !palletID.Equals(selectedPalletID))
                {
                    continue;
                }

                this.dgReturnLOT.RemoveRow(i);
                isExists = true;
            }
            this.dgReturnLOT.EndEdit();
            this.dgReturnLOT.CanUserRemoveRows = false;
            this.dgReturnLOT.IsReadOnly = true;
            PackCommon.SearchRowCount(ref this.txtReturnLOTCount, this.dgReturnLOT.GetRowCount());

            // 구루마 Grid에서 선택한 구루마 ID에 해당하는 구루마 List 삭제
            this.dgReturnPallet.IsReadOnly = false;
            this.dgReturnPallet.CanUserRemoveRows = true;
            this.dgReturnPallet.BeginEdit();
            for (int i = this.dgReturnPallet.GetRowCount() - 1; i >= 0; i--)
            {
                string receiveIssueID = Util.NVC(DataTableConverter.GetValue(this.dgReturnPallet.Rows[i].DataItem, "RCV_ISS_ID")).ToString();
                string palletID = Util.NVC(DataTableConverter.GetValue(this.dgReturnPallet.Rows[i].DataItem, "BOXID")).ToString();
                if (!receiveIssueID.Equals(selectedReceiveIssueID) || !palletID.Equals(selectedPalletID))
                {
                    continue;
                }

                this.dgReturnPallet.RemoveRow(i);
                isExists = true;
            }
            this.dgReturnPallet.EndEdit();
            this.dgReturnPallet.CanUserRemoveRows = false;
            this.dgReturnPallet.IsReadOnly = true;
            PackCommon.SearchRowCount(ref this.txtReturnPalletCount, this.dgReturnPallet.GetRowCount());
            
            if (isExists)
            {
                this.dicCheck.Remove(selectedIndex);    // 체크해제된것 삭제
            }
        }

        private void dgReturnLOT_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.GetSelectedPalletID(sender, e);
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            PACK001_024_CONFIRM popUp = sender as PACK001_024_CONFIRM;
            if (popUp.DialogResult == MessageBoxResult.OK)
            {
                this.ConfirmReturn(popUp.RETURNNOTE);
            }
        }
        #endregion
    }

    internal class PACK001_024_TEMP_DataHelper
    {
        #region Member Variable Lists...
        #endregion

        #region Constructor
        internal PACK001_024_TEMP_DataHelper()
        {
        }
        #endregion

        #region Member Function Lists...
        // 순서도 호출 - 동코드 정보
        internal DataTable GetAreaInfo()
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_AREA_BY_AREATYPE_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["AREA_TYPE_CODE"] = Area_Type.PACK;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 출하처 정보
        internal DataTable GetShipToLocation(string fromAreaID)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_SHIPTO_BY_FROMAREAID_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHIP_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("FROM_AREAID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["SHIP_TYPE_CODE"] = Ship_Type.PACK;
                drRQSTDT["FROM_AREAID"] = fromAreaID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - Porsche J1 B2BI Recall Type
        internal DataTable GetRecallTypeInfo()
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_COMMCODE_ATTR_CBO_V3";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                dtRQSTDT.Columns.Add("CMCODE", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CMCDTYPE"] = "PACK_B2BI_IF_INFO";
                drRQSTDT["CMCODE"] = "RECALL_TYPE_";
                drRQSTDT["ATTRIBUTE1"] = null;
                drRQSTDT["ATTRIBUTE2"] = null;
                drRQSTDT["ATTRIBUTE3"] = null;
                drRQSTDT["ATTRIBUTE4"] = null;
                drRQSTDT["ATTRIBUTE5"] = null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 반품리스트 정보
        internal DataTable GetReturnList(string areaID, string receiveIssueID, string fromDate, string toDate, string chk)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_PRD_SEL_RETURN_AS_PACK_LIST_V2_OCOP";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("FROM_DATE", typeof(string));
                dtRQSTDT.Columns.Add("TO_DATE", typeof(string));
                dtRQSTDT.Columns.Add("OCOP_RTN_FLAG", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["AREAID"] = string.IsNullOrEmpty(areaID) ? null : areaID;
                drRQSTDT["RCV_ISS_ID"] = string.IsNullOrEmpty(receiveIssueID) ? null : receiveIssueID;
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["FROM_DATE"] = string.IsNullOrEmpty(fromDate) ? null : fromDate;
                drRQSTDT["TO_DATE"] = string.IsNullOrEmpty(toDate) ? null : toDate;
                drRQSTDT["OCOP_RTN_FLAG"] = string.IsNullOrEmpty(chk) ? null : chk;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 반품ID와 연관된 구루마 조회
        internal DataTable GetPalletInfo(string receiveIssueID)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_PRD_SEL_RETURN_AS_PACK_LIST_BOX_OCOP";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));
                dtRQSTDT.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["RCV_ISS_ID"] = receiveIssueID;
                drRQSTDT["BOX_RCV_ISS_STAT_CODE"] = "SHIPPING";
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }


        // 순서도 호출 - 반품이력
        internal DataTable GetReturnHist(string areaID, string locationID, string fromDate, string toDate)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_PRD_SEL_RETURN_AS_PACK_PALLET_HIST_OCOP";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("TO_AREAID", typeof(string));
                dtRQSTDT.Columns.Add("FROM_SLOC_ID", typeof(string));
                dtRQSTDT.Columns.Add("FROM_DATE", typeof(string));
                dtRQSTDT.Columns.Add("TO_DATE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["TO_AREAID"] = string.IsNullOrEmpty(areaID) ? null : areaID;
                drRQSTDT["FROM_SLOC_ID"] = string.IsNullOrEmpty(locationID) ? null : locationID;
                drRQSTDT["FROM_DATE"] = string.IsNullOrEmpty(fromDate) ? null : fromDate;
                drRQSTDT["TO_DATE"] = string.IsNullOrEmpty(toDate) ? null : toDate;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 반품확정 대기 구루마에 매핑되어있는 LOT LIST 가져오기
        internal DataTable GetReturnLOTList(string receiveIssueID, string palletID, string OcopFlag)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "BR_PRD_REG_RETURN_AS_LOT_INFO_FOROUT";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));
                dtRQSTDT.Columns.Add("BOXID", typeof(string));
                dtRQSTDT.Columns.Add("OCOP_FLAG", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["RCV_ISS_ID"] = receiveIssueID;
                drRQSTDT["BOXID"] = palletID;
                drRQSTDT["OCOP_FLAG"] = OcopFlag;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 반품확정시 Scan LOT 정보 조회
        internal DataTable GetReturnLOTID(string lotID, string receiveIssueID, string palletID)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "BR_PRD_GET_PACK_INFO_FOR_RETURN";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("SRCTYPE", typeof(string));
                dtRQSTDT.Columns.Add("LOTID", typeof(string));
                dtRQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));
                dtRQSTDT.Columns.Add("BOXID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drRQSTDT["LOTID"] = lotID;
                drRQSTDT["RCV_ISS_ID"] = receiveIssueID;
                drRQSTDT["BOXID"] = palletID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - Porsche Mapping LOTID 조회
        internal DataTable GetMappingLOTID(string lotID)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "BR_PRD_GET_MAPPINGLOTID";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LOTID"] = lotID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - Porsche J1 제품 Check
        internal DataTable GetPorscheRecallCheck(string lotID)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_PRD_CHK_PORSCHE_RECALL";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LOTID"] = lotID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - Porsche GBT Length Check
        internal DataTable GetPorscheRecallGBTLengthCheck(string lotID)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_PRD_CHK_PORSCHE_RECALL_GBT_LEN";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LOTID"] = lotID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - Porsche Recall Info
        internal DataTable GetPorscheRecallInfo(string lotID)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_PRD_SEL_PORSCHE_RECALL_INFO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LOTID"] = lotID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - Porsche Recall!!!
        internal DataTable SetProscheRecall(string lotList, string reason, string recallType, string deviceName)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_PRD_REG_PORSCHE_RECALL";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LOT_LIST", typeof(string));
                dtRQSTDT.Columns.Add("REASON", typeof(string));
                dtRQSTDT.Columns.Add("RECALL_TYPE", typeof(string));
                dtRQSTDT.Columns.Add("DEVICE_NAME", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LOT_LIST"] = lotList;
                drRQSTDT["REASON"] = reason;
                drRQSTDT["RECALL_TYPE"] = recallType;
                drRQSTDT["DEVICE_NAME"] = deviceName;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - Porsche Delivery Step 1 !!!
        internal DataTable GetIFSB2B2CustData(string baseItemID, string fileName)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_INF_SEL_IFS_B2BI_CUST_DATA";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("BAS_ITEM_ID", typeof(string));
                dtRQSTDT.Columns.Add("FILE_NAME", typeof(string));
                dtRQSTDT.Columns.Add("BAS_KEY_ATTR1", typeof(string));
                dtRQSTDT.Columns.Add("BAS_KEY_ATTR2", typeof(string));
                dtRQSTDT.Columns.Add("BAS_KEY_ATTR3", typeof(string));
                dtRQSTDT.Columns.Add("BAS_KEY_ATTR4", typeof(string));
                dtRQSTDT.Columns.Add("BAS_KEY_ATTR5", typeof(string));
                dtRQSTDT.Columns.Add("BAS_KEY_ATTR6", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["BAS_ITEM_ID"] = baseItemID;
                drRQSTDT["FILE_NAME"] = fileName;
                drRQSTDT["BAS_KEY_ATTR1"] = null;
                drRQSTDT["BAS_KEY_ATTR2"] = null;
                drRQSTDT["BAS_KEY_ATTR3"] = null;
                drRQSTDT["BAS_KEY_ATTR4"] = null;
                drRQSTDT["BAS_KEY_ATTR5"] = null;
                drRQSTDT["BAS_KEY_ATTR6"] = null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - Porsche Delivery Step 2 !!!
        internal DataTable SetPorscheDelivery(string lotList)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_INF_SEL_IFS_B2BI_CUST_DATA";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LOT_LIST", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LOT_LIST"] = lotList;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 반품확정시 BOM Check
        internal DataSet CheckReturnProductID(DataTable dt)
        {
            DataTable dtReturn = new DataTable();
            string bizRuleName = "BR_PRD_CHK_PROD_MTRL_CHK";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            string outDataSetName = "OUTDATA";

            try
            {
                // Make InData...
                var queryINDATA = dt.AsEnumerable().GroupBy(x => x.Field<string>("R_PRODID")).Select(grp => new
                {
                    SHOPID = LoginInfo.CFG_SHOP_ID,
                    R_PRODID = grp.Key
                });
                DataTable dtINDATA = PackCommon.queryToDataTable(queryINDATA.ToList());
                dtINDATA.TableName = "INDATA";

                // MakE InProdID Data
                var queryIN_PRODID = dt.AsEnumerable().GroupBy(x => x.Field<string>("PRODID")).Select(grp => new
                {
                    PRODID = grp.Key
                });
                DataTable dtIN_PRODID = PackCommon.queryToDataTable(queryIN_PRODID.ToList());
                dtIN_PRODID.TableName = "IN_PRODID";
                dsINDATA.Tables.Add(dtINDATA);
                dsINDATA.Tables.Add(dtIN_PRODID);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, outDataSetName, dsINDATA);

                if (dsOUTDATA == null)
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dsOUTDATA;
        }

        // 순서도 호출 - 반품확정 Transaction
        internal DataSet SetReturnList(DataTable dtINDATA, DataTable dtINPALLET, DataTable dtINBOX, DataTable dtINLOT)
        {
            string bizRuleName = "BR_PRD_REG_RETURN_AS_PACK_OCOP";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            string outDataSetName = string.Empty;

            try
            {
                dtINDATA.TableName = "INDATA";
                dtINPALLET.TableName = "INPALLET";
                dtINBOX.TableName = "INBOX";
                dtINLOT.TableName = "INLOT";

                dsINDATA.Tables.Add(dtINDATA);
                dsINDATA.Tables.Add(dtINPALLET);
                dsINDATA.Tables.Add(dtINBOX);
                dsINDATA.Tables.Add(dtINLOT);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, null, dsINDATA);

                if (dsOUTDATA == null)
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dsOUTDATA;
        }
        #endregion
    }
}