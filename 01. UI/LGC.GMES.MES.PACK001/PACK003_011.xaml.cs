/*************************************************************************************
 Created Date : 2021.04.04
      Creator : 김길용
   Decription : Pack2동 EMPTY PALLET 반송현황
--------------------------------------------------------------------------------------
 [Change History]
       Date         Author      CSR         Description...
  2021.04.04        담당자      SI          Initialize
  2021.04.13        김길용      SI          현황 상단 전체적인 구성 수정( 동/Tray Type 별 이동 중, 이동완료 현황조회에서 날짜 일별 수량으로 나열)
  2021.04.20        김길용      SI          색상(TOTAL_SUM) 적용 및 조회되도록 수정
  2021.08.05        김길용      SI          조회 조건-기간별에서 시간까지 추가(콤보박스 24시까지 선택되도록 추가)
  2021.12.13        강호운      SI          조회 내역 미존재시 에러 팝업 처리 이상 처리 [SelectRequest 결과 미존재시 처리 제외]
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_011 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private readonly Util _util = new Util();
        //비즈 Config
        private string _bizRuleIp;
        private string _bizRuleProtocol;
        private string _bizRulePort;
        private string _bizRuleServiceMode;
        private string _bizRuleServiceIndex;
        //Detail 변수
        private string _selectedType = string.Empty; //Tray Type
        private string _selectedTransferType = string.Empty; //Curr, Hist  * Transfering 컬럼 이외에 나머지는 Hist로 던짐
        private string _selectedBldgcode = string.Empty; // 동코드
        //날짜 변수
        private string FromDT = string.Empty;
        private string ToDT = string.Empty;

        private string s_fromDT = string.Empty;
        private string s_toDT = string.Empty;
        string _CalenderFlag = "N";
        #endregion

        #region Initialize 
        public PACK003_011()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            GetBizActorServerInfo();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dtpDateFrom.SelectedDateTime = DateTime.Now.Date.AddDays(-1);
            dtpDateTo.SelectedDateTime = DateTime.Now.Date;
            Util.Set_Pack_cboTimeList(cboTimeFrom, "CBO_NAME", "CBO_CODE", "00:00:00");
            Util.Set_Pack_cboTimeList(cboTimeTo, "CBO_NAME", "CBO_CODE", "23:59:59");
            ClearControl();
            SelectRequest();
            Util.SetTextBlockText_DataGridRowCount(txRowCnt, "0");
            Loaded -= UserControl_Loaded;
        }



        #endregion

        #region 비즈 CONFIG 설정
        private void GetBizActorServerInfo()
        {
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("KEYGROUPID", typeof(string));
            DataRow dr = inTable.NewRow();
            //dr["KEYGROUPID"] = "MCS_AP_TEST_CONFIG";    // TEST
            dr["KEYGROUPID"] = "FP_MCS_AP_CONFIG";      // 운영
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MCS_CONFIG_INFO", "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                foreach (DataRow newRow in dtResult.Rows)
                {
                    if (newRow["KEYID"].ToString() == "BizActorIP")
                        _bizRuleIp = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorPort")
                        _bizRulePort = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorProtocol")
                        _bizRuleProtocol = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorServiceIndex")
                        _bizRuleServiceIndex = newRow["KEYVALUE"].ToString();
                    else
                        _bizRuleServiceMode = newRow["KEYVALUE"].ToString();
                }
            }
        }
        #endregion

        #region Event
        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {

                    Util.MessageValidation("SFU2042", "31");   //기간은 {0}일 이내 입니다.
                    return;
                }
                TimeSpan timeSpan = dtpDateTo.SelectedDateTime.Date - dtpDateFrom.SelectedDateTime.Date;

                if (timeSpan.Days < 0)
                {
                    //조회 시작일자는 종료일자를 초과 할 수 없습니다.
                    Util.MessageValidation("SFU3569");
                    return;
                }
                ClearControl();
                SelectRequest();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region Method

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
        private void ClearControl()
        {
            _selectedType = string.Empty;
            _selectedTransferType = string.Empty;
            _selectedBldgcode = string.Empty;

            FromDT = string.Empty;
            ToDT = string.Empty;
            s_fromDT = string.Empty;
            s_toDT = string.Empty;
            Util.SetTextBlockText_DataGridRowCount(txRowCnt, "0");
            Util.gridClear(dgRequest);
            Util.gridClear(dgTransferRequestDetail);
        }
        //좌측 합계
        private void SelectRequest()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "DA_SEL_MCS_EMPTY_CST_LOGIS_SUMMARY_V2";
                FromDT = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                ToDT = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd");

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("FROMDATE", typeof(string));
                inDataTable.Columns.Add("START_TIME", typeof(string));
                inDataTable.Columns.Add("TODATE", typeof(string));
                inDataTable.Columns.Add("END_TIME", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["FROMDATE"] = FromDT;
                inData["START_TIME"] = cboTimeFrom.SelectedValue.ToString();
                inData["TODATE"] = ToDT;
                inData["END_TIME"] = cboTimeTo.SelectedValue.ToString();

                inDataTable.Rows.Add(inData);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    int SerchDt = 0;
                    if (result != null)
                    {
                        SerchDt = int.Parse(result.Rows[0][6].ToString());
                        ColumsVisible(SerchDt);
                        ColumsGridChange();
                        Util.GridSetData(dgRequest, result, FrameOperation, true);
                    }

                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        //상단 검색일수따라 Visible처리
        private void ColumsVisible(int Cnt)
        {
            if (Cnt >= 1) dgRequest.Columns["DAY01"].Visibility = Visibility.Visible;
            if (Cnt >= 2) dgRequest.Columns["DAY02"].Visibility = Visibility.Visible;
            if (Cnt >= 3) dgRequest.Columns["DAY03"].Visibility = Visibility.Visible;
            if (Cnt >= 4) dgRequest.Columns["DAY04"].Visibility = Visibility.Visible;
            if (Cnt >= 5) dgRequest.Columns["DAY05"].Visibility = Visibility.Visible;
            if (Cnt >= 6) dgRequest.Columns["DAY06"].Visibility = Visibility.Visible;
            if (Cnt >= 7) dgRequest.Columns["DAY07"].Visibility = Visibility.Visible;
            if (Cnt >= 8) dgRequest.Columns["DAY08"].Visibility = Visibility.Visible;
            if (Cnt >= 9) dgRequest.Columns["DAY09"].Visibility = Visibility.Visible;
            if (Cnt >= 10) dgRequest.Columns["DAY10"].Visibility = Visibility.Visible;
            if (Cnt >= 11) dgRequest.Columns["DAY11"].Visibility = Visibility.Visible;
            if (Cnt >= 12) dgRequest.Columns["DAY12"].Visibility = Visibility.Visible;
            if (Cnt >= 13) dgRequest.Columns["DAY13"].Visibility = Visibility.Visible;
            if (Cnt >= 14) dgRequest.Columns["DAY14"].Visibility = Visibility.Visible;
            if (Cnt >= 15) dgRequest.Columns["DAY15"].Visibility = Visibility.Visible;
            if (Cnt >= 16) dgRequest.Columns["DAY16"].Visibility = Visibility.Visible;
            if (Cnt >= 17) dgRequest.Columns["DAY17"].Visibility = Visibility.Visible;
            if (Cnt >= 18) dgRequest.Columns["DAY18"].Visibility = Visibility.Visible;
            if (Cnt >= 19) dgRequest.Columns["DAY19"].Visibility = Visibility.Visible;
            if (Cnt >= 20) dgRequest.Columns["DAY20"].Visibility = Visibility.Visible;
            if (Cnt >= 21) dgRequest.Columns["DAY21"].Visibility = Visibility.Visible;
            if (Cnt >= 22) dgRequest.Columns["DAY22"].Visibility = Visibility.Visible;
            if (Cnt >= 23) dgRequest.Columns["DAY23"].Visibility = Visibility.Visible;
            if (Cnt >= 24) dgRequest.Columns["DAY24"].Visibility = Visibility.Visible;
            if (Cnt >= 25) dgRequest.Columns["DAY25"].Visibility = Visibility.Visible;
            if (Cnt >= 26) dgRequest.Columns["DAY26"].Visibility = Visibility.Visible;
            if (Cnt >= 27) dgRequest.Columns["DAY27"].Visibility = Visibility.Visible;
            if (Cnt >= 28) dgRequest.Columns["DAY28"].Visibility = Visibility.Visible;
            if (Cnt >= 29) dgRequest.Columns["DAY29"].Visibility = Visibility.Visible;
            if (Cnt >= 30) dgRequest.Columns["DAY30"].Visibility = Visibility.Visible;
            if (Cnt >= 31) dgRequest.Columns["DAY31"].Visibility = Visibility.Visible;
        }

        //검색 시작일시 기준으로 현황 컬럼 Header 변경
        private void ColumsGridChange()
        {
            dgRequest.Columns["DAY01"].Header = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
            dgRequest.Columns["DAY02"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(1).ToString("yyyyMMdd");
            dgRequest.Columns["DAY03"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(2).ToString("yyyyMMdd");
            dgRequest.Columns["DAY04"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(3).ToString("yyyyMMdd");
            dgRequest.Columns["DAY05"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(4).ToString("yyyyMMdd");
            dgRequest.Columns["DAY06"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(5).ToString("yyyyMMdd");
            dgRequest.Columns["DAY07"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(6).ToString("yyyyMMdd");
            dgRequest.Columns["DAY08"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(7).ToString("yyyyMMdd");
            dgRequest.Columns["DAY09"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(8).ToString("yyyyMMdd");
            dgRequest.Columns["DAY10"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(9).ToString("yyyyMMdd");
            dgRequest.Columns["DAY11"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(10).ToString("yyyyMMdd");
            dgRequest.Columns["DAY12"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(11).ToString("yyyyMMdd");
            dgRequest.Columns["DAY13"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(12).ToString("yyyyMMdd");
            dgRequest.Columns["DAY14"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(13).ToString("yyyyMMdd");
            dgRequest.Columns["DAY15"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(14).ToString("yyyyMMdd");
            dgRequest.Columns["DAY16"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(15).ToString("yyyyMMdd");
            dgRequest.Columns["DAY17"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(16).ToString("yyyyMMdd");
            dgRequest.Columns["DAY18"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(17).ToString("yyyyMMdd");
            dgRequest.Columns["DAY19"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(18).ToString("yyyyMMdd");
            dgRequest.Columns["DAY20"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(19).ToString("yyyyMMdd");
            dgRequest.Columns["DAY21"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(20).ToString("yyyyMMdd");
            dgRequest.Columns["DAY22"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(21).ToString("yyyyMMdd");
            dgRequest.Columns["DAY23"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(22).ToString("yyyyMMdd");
            dgRequest.Columns["DAY24"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(23).ToString("yyyyMMdd");
            dgRequest.Columns["DAY25"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(24).ToString("yyyyMMdd");
            dgRequest.Columns["DAY26"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(25).ToString("yyyyMMdd");
            dgRequest.Columns["DAY27"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(26).ToString("yyyyMMdd");
            dgRequest.Columns["DAY28"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(27).ToString("yyyyMMdd");
            dgRequest.Columns["DAY29"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(28).ToString("yyyyMMdd");
            dgRequest.Columns["DAY30"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(29).ToString("yyyyMMdd");
            dgRequest.Columns["DAY31"].Header = dtpDateFrom.SelectedDateTime.Date.AddDays(30).ToString("yyyyMMdd");
        }

        //우측 상세내역
        private void SelectTransferRequestDetail()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "DA_SEL_MCS_EMPTY_CST_LOGIS_DETAIL";


                DataTable inDataTable = new DataTable("RQSTDT");
                //inDataTable.Columns.Add("LANGID", typeof(string));

                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("MDL_TP", typeof(string));
                inDataTable.Columns.Add("FROMDATE", typeof(string));
                inDataTable.Columns.Add("TODATE", typeof(string));
                inDataTable.Columns.Add("SEARCH_FLAG", typeof(string));
                inDataTable.Columns.Add("BLDG_CODE", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["MDL_TP"] = _selectedType;
                inData["FROMDATE"] = _CalenderFlag =="N" ?  FromDT : s_fromDT; // dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                inData["TODATE"] = _CalenderFlag == "N" ? ToDT : s_toDT; //dtpDateTo.SelectedDateTime.Date.ToString("yyyy-MM-dd");
                inData["SEARCH_FLAG"] = _selectedTransferType;      // HIST, CURR
                inData["BLDG_CODE"] = _selectedBldgcode;        // W06, W07
                inDataTable.Rows.Add(inData);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        _CalenderFlag = "N";
                        Util.SetTextBlockText_DataGridRowCount(txRowCnt, "0");
                        Util.MessageException(ex);
                        return;
                    }
                    _CalenderFlag = "N";
                    Util.SetTextBlockText_DataGridRowCount(txRowCnt, Util.NVC(result.Rows.Count));
                    Util.GridSetData(dgTransferRequestDetail, result, FrameOperation, true);

                });
            }
            catch (Exception ex)
            {
                _CalenderFlag = "N";
                Util.SetTextBlockText_DataGridRowCount(txRowCnt, "0");
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void dgRequest_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                // TOTAL_SUM 행 색상처리
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BLDG_NAME")).Equals("TOTAL_SUM"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);
                    }
                }
            }));
        }

        private void dgRequest_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null) return;

                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;
                
                _selectedType = null;
                _selectedTransferType = null;
                _selectedBldgcode = null;


                _selectedType = DataTableConverter.GetValue(drv, "MDL_TP").GetString();
                _selectedBldgcode = DataTableConverter.GetValue(drv, "BLDG_CODE").GetString();
                if (string.Equals(cell.Column.Name, "BLDG_NAME") || string.Equals(cell.Column.Name, "MDL_TP_NAME"))
                {
                    return;
                }
                //현황의 마지막 합계(TOTAL_SUM) 는 상세조회가 안되도록 적용 -> Tray Type and BLDG_CODE = ALL로 수정
                if (DataTableConverter.GetValue(drv, "BLDG_NAME").GetString() == "TOTAL_SUM")
                {
                    _selectedType = "ALL";
                    _selectedBldgcode = "ALL";
                }
                //Transfering and Transferred - 날짜조회를 최상단 기준으로 함
                if (string.Equals(cell.Column.Name, "CURR_QTY_SUM"))
                {
                    _selectedTransferType = "CURR";
                }
                if (string.Equals(cell.Column.Name, "HIST_QTY_SUM"))
                {
                    _selectedTransferType = "HIST";
                }
                //Transfering and Transferred 이외 - 날짜 조건을 해당 컬럼 날짜 조회조건으로 함
                if (!(string.Equals(cell.Column.Name, "CURR_QTY_SUM") || string.Equals(cell.Column.Name, "HIST_QTY_SUM")))
                {
                    _selectedTransferType = "HIST";
                    //컬럼항목(string->int 로 변환) 날짜형식을 구하기 위해
                    string v_from = cell.Column.Header.ToString();
                    int i_from = int.Parse(cell.Column.Header.ToString());
                    int i_toD = i_from + 1;
                    string S_toD = Convert.ToString(i_toD);
                    
                    s_fromDT = v_from.Substring(0, 4) + "-" + v_from.Substring(4, 2) + "-" + v_from.Substring(6, 2);
                    s_toDT = S_toD.Substring(0, 4) + "-" + S_toD.Substring(4, 2) + "-" + S_toD.Substring(6, 2);
                    //CURR, HIST 컬럼이 아닌 날짜를 클릭할 경우 클릭한 날짜, 날짜+1로 DATE를 넣기위해 구분 FLAG (Y 이면 날짜항목 클릭, N 이면 CURR,HIST 항목 클릭)
                    _CalenderFlag = "Y";
                }
                Util.gridClear(dgTransferRequestDetail);

                tabItemReqList.IsSelected = true;
                SelectTransferRequestDetail();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

      
    }
}
