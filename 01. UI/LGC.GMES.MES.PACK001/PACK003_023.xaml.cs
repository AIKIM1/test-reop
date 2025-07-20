/************************************************************************************
  Created Date : 2021.04.06
       Creator : 정용석
   Description : 라인별 Stocker 현황
 ------------------------------------------------------------------------------------
  [Change History]
    2021.04.06  정용석 : Initial Created.
    2021.08.19  정용석 : 장기재고 연동내역 추가
    2021.11.04  김길용 : ESWA PACK3 적용을 위한 하드코딩 제거
    2022.12.01  정용석 : 조회조건에 ROUTID 추가
 ************************************************************************************/
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_023 : UserControl, IWorkArea
    {
        #region Member Variable Lists...
        private StockerSummaryDataHelper dataHelper = new StockerSummaryDataHelper();
        private int stockerIDMultiComboBindDataCount = 0;
        private int equipmentSegmentIDMultiComboBindDataCount = 0;
        private int productIDMultiComboBindDataCount = 0;
        private int processIDMultiComboBindDataCount = 0;
        private int routIDMultiComboBindDataCount = 0;

        private string searchRoutIDList = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Declaration & Constructor
        public PACK003_023()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function Lists...
        private void Initialize()
        {
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetStockerInfo(), this.cboMultiStockerID, ref this.stockerIDMultiComboBindDataCount);
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetProductInfo(), this.cboMultiProductID, ref this.productIDMultiComboBindDataCount);
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetPackLineInfo(), this.cboMultiEquipmentSegmentID, ref this.equipmentSegmentIDMultiComboBindDataCount);
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetPackProcessInfo(), this.cboMultiProcessID, ref this.processIDMultiComboBindDataCount);
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetRouteInfo(), this.cboMultiRoutID, ref this.routIDMultiComboBindDataCount);
            PackCommon.SearchRowCount(ref this.txtGridRowCount, 0);
        }

        // 조회
        private void SearchProcess()
        {
            // Validation of Search Condition
            string stockerIDList = Convert.ToString(this.cboMultiStockerID.SelectedItemsToString);
            if (string.IsNullOrEmpty(stockerIDList))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("호기")); // %1(을)를 선택하세요.
                this.cboMultiProductID.Focus();
                return;
            }

            string productIDList = Convert.ToString(this.cboMultiProductID.SelectedItemsToString);
            if (string.IsNullOrEmpty(productIDList))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("PRODID")); // %1(을)를 선택하세요.
                this.cboMultiProductID.Focus();
                return;
            }

            string equipmentSegmentIDList = Convert.ToString(this.cboMultiEquipmentSegmentID.SelectedItemsToString);
            if (string.IsNullOrEmpty(equipmentSegmentIDList))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("라인")); // %1(을)를 선택하세요.
                this.cboMultiProcessID.Focus();
                return;
            }

            string processIDList = Convert.ToString(this.cboMultiProcessID.SelectedItemsToString);
            if (string.IsNullOrEmpty(processIDList))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("공정")); // %1(을)를 선택하세요.
                this.cboMultiEquipmentSegmentID.Focus();
                return;
            }

            string routIDList = Convert.ToString(this.cboMultiRoutID.SelectedItemsToString);
            if (string.IsNullOrEmpty(routIDList))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("경로")); // %1(을)를 선택하세요.
                this.cboMultiRoutID.Focus();
                return;
            }

            this.searchRoutIDList = routIDList;     // Stocker 현황 상세와 연결된 경우, 선택한 조회조건 연동을 위해 RoutID 선택된 리스트 저장함.

            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();
            try
            {
                this.SearchStockerRatio();
                this.SearchStockerSummary(stockerIDList, productIDList, equipmentSegmentIDList, processIDList, routIDList);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }

            this.loadingIndicator.Visibility = Visibility.Collapsed;
        }

        // 조회 - Stocker 점유율
        private void SearchStockerRatio()
        {
            PackCommon.SearchRowCount(ref this.txtGridRowCount, 0);
            Util.gridClear(this.dgStockerRatio);
            try
            {
                DataTable dt = this.dataHelper.GetStockerSummaryRatio();

                if (CommonVerify.HasTableRow(dt))
                {
                    PackCommon.SearchRowCount(ref this.txtGridRowCount, dt.Rows.Count);
                    Util.GridSetData(this.dgStockerRatio, dt, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 조회 - Stocker Summary 현황
        private void SearchStockerSummary(string stockerIDList, string productIDList, string equipmentSegmentIDList, string processIDList, string routIDList)
        {
            PackCommon.SearchRowCount(ref this.txtdgItem2RowCount, 0);
            Util.gridClear(this.dgStockerSummary);
            try
            {
                DataTable dt = this.dataHelper.GetStockerSummaryList(stockerIDList, productIDList, equipmentSegmentIDList, processIDList, routIDList);
                if (CommonVerify.HasTableRow(dt))
                {
                    PackCommon.SearchRowCount(ref this.txtdgItem2RowCount, dt.Rows.Count);
                    Util.GridSetData(this.dgStockerSummary, dt, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // LoadedCellPresenter 이벤트가 발생했을 때
        private void LoadedCellPresenterEventFireProcess(C1DataGrid c1DataGrid, DataGridCellEventArgs e)
        {
            try
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = e.Cell.Column.Name.ToUpper().Contains("QTY") ? new SolidColorBrush(Colors.Blue) : new SolidColorBrush(Colors.Black);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 그리드 Click 위치에 해당하는 Column에 따른 화면 Open
        private void CallMEBUI(C1DataGrid c1DataGrid, C1.WPF.DataGrid.DataGridCell dataGridCell)
        {
            try
            {
                // Declarations..
                string stockerDetailUIMenuID = this.dataHelper.GetStockerDetailMenuID(this.GetType().Namespace, "PACK003_024");
                if (string.IsNullOrEmpty(stockerDetailUIMenuID))
                {
                    Util.MessageValidation("SFU3562");      // 메뉴ID를 입력하세요.
                    return;
                }

                if ((dataGridCell == null) || (dataGridCell != null && dataGridCell.Row.Index < 2))
                {
                    return;
                }

                // 수량컬럼이 아닌경우는 return
                if (!dataGridCell.Column.Name.ToUpper().Contains("QTY"))
                {
                    return;
                }

                string processID = Util.NVC(DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "PROCID"));
                string equipmentSegmentID = Util.NVC(DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "EQSGID"));
                string productID = Util.NVC(DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "PRODID"));

                // 재공구분
                string requestWipTypeCode = string.Empty;
                switch (dataGridCell.Column.Name.ToUpper())
                {
                    case "ACTIVITY_QTY":                // 가용재고
                        requestWipTypeCode = "G";
                        break;
                    case "HOLD_QTY":                    // Hold
                        requestWipTypeCode = "H";
                        break;
                    case "NG_QTY":                      // NG
                        requestWipTypeCode = "N";
                        break;
                    case "NON_ACTIVITY_QTY":            // 경과일 이전
                        requestWipTypeCode = "B";
                        break;
                    case "EXPIRED_QTY":                 // 장기재고
                        requestWipTypeCode = "O";
                        break;
                    case "SMPL_QTY":                    // 샘플링검사
                        requestWipTypeCode = "S";
                        break;
                    default:
                        requestWipTypeCode = string.Empty;
                        break;
                }

                // Stocker 구분 (Pack 2,3동 구분을 위해 스토커 Commoncode로 구분)
                string requestAreaStockerCode = string.Empty;
                string equipmentID = this.cboMultiStockerID.SelectedItemsToString;
                switch (dataGridCell.Column.Name.ToUpper())
                {
                    case "FIRST_OCV_STOCKER_QTY":
                        requestAreaStockerCode = LoginInfo.CFG_AREA_ID == "P8" ? "OCV1_WAIT_STK" : null;
                        //EQPTID = "P8STK101,P8STK102,P8STK103,P8STK104,P8STK105,P8STK106,P8STK107,P8STK108,P8STK109";
                        break;
                    case "SECOND_OCV_STOCKER_QTY":
                        requestAreaStockerCode = LoginInfo.CFG_AREA_ID == "P8" ? "OCV2_WAIT_STK" : null;
                        //EQPTID = "P8STK111,P8STK112,P8STK113,P8STK114,P8STK115,P8STK116,P8STK117,P8STK118,P8STK119";
                        break;
                    case "NG_STOCKER_QTY":
                        requestAreaStockerCode = LoginInfo.CFG_AREA_ID == "P8" ? "OCV_NG_STK" : null;
                        //EQPTID = "P8STK110";
                        break;
                    case "TOTAL_QTY":
                        equipmentID = string.Empty;
                        break;
                    case "ACTIVITY_QTY":                // 가용재고
                    case "HOLD_QTY":                    // Hold
                    case "NG_QTY":                      // NG
                    case "NON_ACTIVITY_QTY":            // 경과일 이전
                    case "EXPIRED_QTY":                 // 장기재고
                    case "SMPL_QTY":                    // 샘플링검사
                        equipmentID = this.cboMultiStockerID.SelectedItemsToString;
                        break;
                    default:
                        equipmentID = string.Empty;
                        break;
                }
                this.dataHelper.SetStockerIDInfo(requestAreaStockerCode, ref equipmentID);
                this.FrameOperation.OpenMenu(stockerDetailUIMenuID, true, processID, equipmentSegmentID, productID, requestWipTypeCode, equipmentID, this.searchRoutIDList);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetGridDetailColumnText()
        {
            if (LoginInfo.CFG_AREA_ID == "PA")
            {
                dgStockerSummary.Columns["FIRST_OCV_STOCKER_QTY"].Visibility = Visibility.Collapsed;
                dgStockerSummary.Columns["SECOND_OCV_STOCKER_QTY"].Visibility = Visibility.Collapsed;
                dgStockerSummary.Columns["NG_STOCKER_QTY"].Visibility = Visibility.Collapsed;

                dgStockerSummary.Columns["SMPL_QTY"].Visibility = Visibility.Visible;
                //string[] sColumnName = new string[] { "TOTAL_QTY" };
                //_Util.SetDataGridMergeExtensionCol(dgStockerSummary, sColumnName, DataGridMergeMode.HORIZONTAL);
            }
        }
        #endregion

        #region Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
            this.SetGridDetailColumnText();
            this.Loaded -= new RoutedEventHandler(this.UserControl_Loaded);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess();
        }

        private void dgStockerSummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            this.LoadedCellPresenterEventFireProcess((C1DataGrid)sender, e);
        }

        private void dgStockerSummary_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid c1DataGrid = (C1DataGrid)sender;
            this.CallMEBUI(c1DataGrid, c1DataGrid.GetCellFromPoint(e.GetPosition(null)));
        }

        private void dgStockerRatio_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender == null)
            {
                return;
            }
            C1DataGrid dataGrid = (C1DataGrid)sender;
            if (dataGrid == null || dataGrid.CurrentCell == null || dataGrid.CurrentCell.Row == null)
            {
                return;
            }

            Point point = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell dataGridCell = this.dgStockerRatio.GetCellFromPoint(point);
            if (dataGridCell == null)
            {
                return;
            }

            // 선택한 셀의 위치
            int rowIndex = dataGridCell.Row.Index;
            DataRowView dataRowView = dataGrid.Rows[rowIndex].DataItem as DataRowView;

            string EQPTID = Util.NVC(DataTableConverter.GetValue(dataRowView, "EQPTID"));
            string summaryFlag = Util.NVC(DataTableConverter.GetValue(dataRowView, "SUMMARY_FLAG"));
            string totalFlag = Util.NVC(DataTableConverter.GetValue(dataRowView, "TOTAL_FLAG"));

            //Pack 2,3동 구분을 위해 스토커 Commoncode로 구분
            string requestAreaStkcode = string.Empty;

            // Stocker ID 거르기
            if (summaryFlag.Equals("0") && totalFlag.Equals("0"))
            {
                EQPTID = Util.NVC(DataTableConverter.GetValue(dataRowView, "EQPTID"));
            }
            if (!summaryFlag.Equals("0") && EQPTID.ToUpper().Contains("1ST"))
            {
                EQPTID = this.cboMultiStockerID.SelectedItemsToString;
                requestAreaStkcode = LoginInfo.CFG_AREA_ID == "P8" ? "OCV1_WAIT_STK" : null;
                this.dataHelper.SetStockerIDInfo(requestAreaStkcode, ref EQPTID);
            }
            if (!summaryFlag.Equals("0") && EQPTID.ToUpper().Contains("2ND"))
            {
                EQPTID = this.cboMultiStockerID.SelectedItemsToString;
                requestAreaStkcode = LoginInfo.CFG_AREA_ID == "P8" ? "OCV2_WAIT_STK" : null;
                this.dataHelper.SetStockerIDInfo(requestAreaStkcode, ref EQPTID);
            }
            if (!totalFlag.Equals("0"))
            {
                EQPTID = string.Empty;
            }
            // 검색조건 연동
            DataTable dt = DataTableConverter.Convert(this.cboMultiStockerID.ItemsSource);
            if (string.IsNullOrEmpty(EQPTID))
            {
                this.cboMultiStockerID.Check(-1);
            }
            else
            {
                int index = 0;
                foreach (DataRowView drv in dt.AsDataView())
                {
                    if (EQPTID.Contains(drv["EQPTID"].ToString()))
                    {
                        this.cboMultiStockerID.Check(index++);
                    }
                    else
                    {
                        this.cboMultiStockerID.Uncheck(index++);
                    }
                }
            }
            this.cboMultiProductID.Check(-1);
            this.cboMultiProcessID.Check(-1);
            this.cboMultiEquipmentSegmentID.Check(-1);
            this.cboMultiRoutID.Check(-1);

            this.SearchProcess();
        }
        #endregion
    }

    public class StockerSummaryDataHelper
    {
        #region Member Variable Lists...
        #endregion

        #region Constructor
        public StockerSummaryDataHelper()
        {
        }
        #endregion

        #region Member Function Lists...
        // 순서도 호출 - Stocker Equipment
        public void SetStockerIDInfo(string requestStockerArea, ref string equipmentID)
        {
            try
            {
                string bizRuleName = "DA_PRD_LOGIS_STKEQPTID_INFO_PACK";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                dtRQSTDT.Columns.Add("CMCODE", typeof(string));
                dtRQSTDT.Columns.Add("EQPTID", typeof(string));


                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["ATTRIBUTE3"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["CMCODE"] = requestStockerArea;
                drRQSTDT["EQPTID"] = equipmentID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    equipmentID = dtRSLTDT.AsEnumerable().Select(x => x.Field<string>("EQPTID")).Aggregate((current, next) => current + "," + next);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 순서도 호출 - Menu ID
        public string GetStockerDetailMenuID(string nameSpace, string formID)
        {
            string menuID = string.Empty;
            try
            {
                string bizRuleName = "DA_BAS_SEL_MENU_WITH_BOOKMARK";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("USERID", typeof(string));
                dtRQSTDT.Columns.Add("PROGRAMTYPE", typeof(string));
                dtRQSTDT.Columns.Add("MENUIUSE", typeof(string));
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("MENULEVEL", typeof(string));
                dtRQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["USERID"] = LoginInfo.USERID;
                drRQSTDT["PROGRAMTYPE"] = Common.Common.APP_System;
                drRQSTDT["MENUIUSE"] = "Y";
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["MENULEVEL"] = "3";
                drRQSTDT["SYSTEM_ID"] = LoginInfo.SYSID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (!CommonVerify.HasTableRow(dtRSLTDT))
                {
                    return menuID;
                }

                menuID = dtRSLTDT.AsEnumerable().Where(x => x.Field<string>("NAMESPACE") == nameSpace && x.Field<string>("FORMID") == formID).Select(x => x.Field<string>("MENUID")).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return menuID;
            }

            return menuID;
        }

        // 순서도 호출 - Stocker 정보 조회
        public DataTable GetStockerInfo()
        {
            DataTable dtReturn = new DataTable();

            try
            {
                string bizRuleName = "DA_PRD_SEL_LOGIS_STOCKER_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQGRID", typeof(string));
                dtRQSTDT.Columns.Add("EQPTLEVEL", typeof(string));
                dtRQSTDT.Columns.Add("STOCKERTYPE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["EQGRID"] = null;
                drRQSTDT["EQPTLEVEL"] = "M";
                drRQSTDT["STOCKERTYPE"] = "OCV1_WAIT_STK,OCV_NG_STK,OCV2_WAIT_STK,OCV_WAIT_STK";
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return dtReturn;
            }

            return dtReturn;
        }

        // 순서도 호출 - Pack Line
        public DataTable GetPackLineInfo()
        {
            DataTable dtReturn = new DataTable();

            try
            {
                string bizRuleName = "DA_BAS_SEL_LOGIS_LINE_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PACK_CELL_AUTO_LOGIS_FLAG", typeof(string));  // 반송여부(물류타는라인)
                dtRQSTDT.Columns.Add("PACK_MEB_LINE_FLAG", typeof(string));         // MEB 라인 여부
                dtRQSTDT.Columns.Add("PACK_BOX_LINE_FLAG", typeof(string));         // 자동 포장 라인 여부

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["PACK_CELL_AUTO_LOGIS_FLAG"] = DBNull.Value;
                drRQSTDT["PACK_MEB_LINE_FLAG"] = "Y";
                drRQSTDT["PACK_BOX_LINE_FLAG"] = DBNull.Value;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    var query = dtRSLTDT.AsEnumerable().Select(x => new
                    {
                        EQSGID = x.Field<string>("CBO_CODE"),
                        EQSGNAME = x.Field<string>("CBO_NAME")
                    });

                    dtReturn = PackCommon.queryToDataTable(query.ToList());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 제품
        public DataTable GetProductInfo()
        {
            DataTable dtReturn = new DataTable();

            try
            {
                string bizRuleName = "DA_PRD_SEL_SHOP_PRDT_ROUT_MODULE_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["EQSGID"] = null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    var query = dtRSLTDT.AsEnumerable().Select(x => new
                    {
                        PRODID = x.Field<string>("PRODID"),
                        PRODNAME = x.Field<string>("PRODID") + " : " + x.Field<string>("PRODNAME")
                    });

                    dtReturn = PackCommon.queryToDataTable(query.ToList());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - Pack 공정 정보
        public DataTable GetPackProcessInfo()
        {
            DataTable dtReturn = new DataTable();

            try
            {
                string bizRuleName = "DA_PRD_SEL_LOGIS_PROCESS_PACK_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PCSGID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREA_TYPE_CODE"] = Area_Type.PACK;
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["EQSGID"] = null;
                drRQSTDT["PCSGID"] = null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    var query = dtRSLTDT.AsEnumerable().Select(x => new
                    {
                        PROCID = x.Field<string>("CBO_CODE"),
                        PROCNAME = x.Field<string>("CBO_NAME")
                    }).OrderBy(x => x.PROCID);

                    dtReturn = PackCommon.queryToDataTable(query.ToList());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - Route
        public DataTable GetRouteInfo()
        {
            DataTable dtReturn = new DataTable();

            try
            {
                string bizRuleName = "DA_PRD_SEL_LOGIS_SHOP_PRDT_ROUTID_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["EQSGID"] = null;
                drRQSTDT["AREA_TYPE_CODE"] = Area_Type.PACK;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    var query = dtRSLTDT.AsEnumerable().Select(x => new
                    {
                        ROUTID = x.Field<string>("ROUTID"),
                        ROUTNAME = x.Field<string>("ROUTID") + " : " + x.Field<string>("ROUTNAME")
                    });

                    dtReturn = PackCommon.queryToDataTable(query.ToList());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - Stocker 점유율 조회
        public DataTable GetStockerSummaryRatio()
        {
            string bizRuleName = "BR_PRD_SEL_LOGIS_STOCK_SUMMARY_RATIO";
            DataTable dtINDATA = new DataTable("INDATA");
            DataTable dtOUTDATA = new DataTable("OUTDATA");

            dtINDATA.Columns.Add("LANGID", typeof(string));
            dtINDATA.Columns.Add("AREAID", typeof(string));

            DataRow drINDATA = dtINDATA.NewRow();
            drINDATA["LANGID"] = LoginInfo.LANGID;
            drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtINDATA.Rows.Add(drINDATA);

            dtOUTDATA = new ClientProxy().ExecuteServiceSync(bizRuleName, dtINDATA.TableName, dtOUTDATA.TableName, dtINDATA);
            return dtOUTDATA;
        }

        // 순서도 호출 - Stocker 현황 조회
        public DataTable GetStockerSummaryList(string stockerIDList, string productIDList, string equipmentSegmentIDList, string processIDList, string routIDList)
        {
            string bizRuleName = "BR_PRD_SEL_LOGIS_STOCK_SUMMARY";
            //string bizRuleName = "BR_PRD_SEL_LOGIS_STOCK_SUMMARY_COPY";
            DataTable dtINDATA = new DataTable("INDATA");
            DataTable dtOUTDATA = new DataTable("OUTDATA");

            dtINDATA.Columns.Add("LANGID", typeof(string));
            dtINDATA.Columns.Add("AREAID", typeof(string));
            dtINDATA.Columns.Add("EQPTID", typeof(string));
            dtINDATA.Columns.Add("PRODID_LIST", typeof(string));
            dtINDATA.Columns.Add("PROD_PACK_LINE_LIST", typeof(string));
            dtINDATA.Columns.Add("PROCID_LIST", typeof(string));
            dtINDATA.Columns.Add("ROUTID_LIST", typeof(string));

            DataRow drINDATA = dtINDATA.NewRow();
            drINDATA["LANGID"] = LoginInfo.LANGID;
            drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
            drINDATA["EQPTID"] = stockerIDList;
            drINDATA["PRODID_LIST"] = string.IsNullOrEmpty(productIDList) ? null : productIDList;
            drINDATA["PROD_PACK_LINE_LIST"] = string.IsNullOrEmpty(equipmentSegmentIDList) ? null : equipmentSegmentIDList;
            drINDATA["PROCID_LIST"] = string.IsNullOrEmpty(processIDList) ? null : processIDList;
            drINDATA["ROUTID_LIST"] = string.IsNullOrEmpty(routIDList) ? null : routIDList;
            dtINDATA.Rows.Add(drINDATA);

            dtOUTDATA = new ClientProxy().ExecuteServiceSync(bizRuleName, dtINDATA.TableName, dtOUTDATA.TableName, dtINDATA);
            return dtOUTDATA;
        }
        #endregion
    }
}