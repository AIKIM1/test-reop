/*************************************************************************************
 Created Date : 2021.09.17
      Creator : 김길용
  Description : 수동 반송요청(모듈-가용)
--------------------------------------------------------------------------------------
 [Change History]
    Date       Author      CSR ID         Description...
  2021.09.17   Create        SI           Initial Created.
  2021.10.28   김길용        SI           수동요청반송에 대한 이력 Tab 오픈
  2021.11.04   김길용        SI           ESWA PACK3 적용을 위한 하드코딩 제거
  2021.11.11   김길용        SI           Pack3동 공통화 수정,하드코딩 제거 및 샘플링,AREAID 컬럼 추가
  2022.03.03   김길용        SI           출고Lot 예약 정보 > 잡삭제 기능 추가 (btnManualTransferCancel)  
  2022.04.14   이태규        SI           물류포장유형 및 유형별 가용수량 추가
  2023.06.05   정용석  E20230110-000011   4싸이클 Column Header 추가, 각 그룹별로 가용수량, Hold, 장기재고로 컬럼 구성
                                          쓸데없는 멤버함수 변수 제거, 순서도 호출부분 분리
**************************************************************************************/
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_027 : UserControl, IWorkArea
    {
        #region Member Variables Lists...
        private PACK003_027_DataHelper dataHelper = new PACK003_027_DataHelper();
        private int equipmentSegmentIDMultiComboBindDataCount = 0;
        private int productIDMultiComboBindDataCount = 0;
        private List<Tuple<string, string>> lstFirstGroup = new List<Tuple<string, string>>();
        private List<Tuple<string, string>> lstSecondGroup = new List<Tuple<string, string>>();
        private string searchFromDate = string.Empty;       // 조회했을 당시의 조회조건의 FROM EOL 일자
        private string searchToDate = string.Empty;         // 조회했을 당시의 조회조건의 TO EOL 일자
        #endregion

        #region Property
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        public PACK003_027()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function Lists...
        private void DefineHeader()
        {
            // 1 Group : 총수량, 2nd OCV (비가용) + 2nd OCV (가용) = 2nd OCV, 트럭, 충방전, 싸이클, OCV + 충방전, Sampling Hold, 포장가능수량
            this.lstFirstGroup.Add(new Tuple<string, string>("TOTAL", ObjectDic.Instance.GetObjectName("총수량")));
            this.lstFirstGroup.Add(new Tuple<string, string>("SAMPLE_HOLD", ObjectDic.Instance.GetObjectName("샘플링Hold")));
            this.lstFirstGroup.Add(new Tuple<string, string>("TRF_AVA", ObjectDic.Instance.GetObjectName("포장가능 수량")));

            this.lstFirstGroup.Add(new Tuple<string, string>("1STOCV", ObjectDic.Instance.GetObjectName("1st OCV")));
            this.lstFirstGroup.Add(new Tuple<string, string>("2NDOCV", ObjectDic.Instance.GetObjectName("2nd OCV")));

            this.lstFirstGroup.Add(new Tuple<string, string>("TRUCK", ObjectDic.Instance.GetObjectName("트럭킹")));
            this.lstFirstGroup.Add(new Tuple<string, string>("CHARGE", ObjectDic.Instance.GetObjectName("충방전")));
            this.lstFirstGroup.Add(new Tuple<string, string>("ILT4CYCLE", ObjectDic.Instance.GetObjectName("ILT4CYCLE")));
            this.lstFirstGroup.Add(new Tuple<string, string>("OCVCHARGE", ObjectDic.Instance.GetObjectName("OCVCHARGE")));

            // 2 Group : 가용, HOLD, 장기매매, 총수량, Sampling, 포장가능수량, 1STOCV 1일전 : WAIT_1STOCV, 1STOCV 1일후 : OVER_1STOCV, 2NDOCV 4일전 : 4, 2NDOCV 3일전 : 3, 2NDOCV 2일전 : 2, 2NDOCV 1일전 : 1
            this.lstSecondGroup.Add(new Tuple<string, string>("TOTAL", ObjectDic.Instance.GetObjectName("총수량")));
            this.lstSecondGroup.Add(new Tuple<string, string>("SAMPLE_HOLD", ObjectDic.Instance.GetObjectName("샘플링Hold")));
            this.lstSecondGroup.Add(new Tuple<string, string>("TRF_AVA", ObjectDic.Instance.GetObjectName("포장가능 수량")));

            this.lstSecondGroup.Add(new Tuple<string, string>("WAIT_1STOCV", ObjectDic.Instance.GetObjectName("1일 전")));
            this.lstSecondGroup.Add(new Tuple<string, string>("OVER_1STOCV", ObjectDic.Instance.GetObjectName("1일 후")));

            this.lstSecondGroup.Add(new Tuple<string, string>("4", ObjectDic.Instance.GetObjectName("3일 이전")));
            this.lstSecondGroup.Add(new Tuple<string, string>("3", ObjectDic.Instance.GetObjectName("3일 전")));
            this.lstSecondGroup.Add(new Tuple<string, string>("2", ObjectDic.Instance.GetObjectName("2일 전")));
            this.lstSecondGroup.Add(new Tuple<string, string>("1", ObjectDic.Instance.GetObjectName("1일 전")));

            this.lstSecondGroup.Add(new Tuple<string, string>("AVA", ObjectDic.Instance.GetObjectName("가용수량")));
            this.lstSecondGroup.Add(new Tuple<string, string>("HOLD", ObjectDic.Instance.GetObjectName("HOLD")));
            this.lstSecondGroup.Add(new Tuple<string, string>("LONG_TERM", ObjectDic.Instance.GetObjectName("장기재고")));
        }

        private void Initialize()
        {
            this.dtpDateFrom.SelectedDateTime = DateTime.Now;
            this.dtpDateTo.SelectedDateTime = DateTime.Now;

            this.dtpDateFromList.SelectedDateTime = DateTime.Now;
            this.dtpDateToList.SelectedDateTime = DateTime.Now;
            this.dgList.Columns["SMPL_QTY"].Visibility = (LoginInfo.CFG_AREA_ID == "PA") ? Visibility.Visible : Visibility.Collapsed;

            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetPackLineInfo(), this.cboMultiEquipmentSegmentID, ref this.equipmentSegmentIDMultiComboBindDataCount);
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetProductInfo(), this.cboMultiProductID, ref this.productIDMultiComboBindDataCount);

            this.DefineHeader();
        }

        private void SearchProcess()
        {
            // Validation of Search Condition
            if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiProductID.SelectedItemsToString)))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("제품")); // %1(을)를 선택하세요.
                this.cboMultiProductID.Focus();
                return;
            }

            if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiEquipmentSegmentID.SelectedItemsToString)))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("라인")); // %1(을)를 선택하세요.
                this.cboMultiEquipmentSegmentID.Focus();
                return;
            }

            if (Convert.ToDecimal(Convert.ToDateTime(this.dtpDateFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(this.dtpDateTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.MessageValidation("SFU1913");      // 종료일자가 시작일자보다 빠릅니다.
                return;
            }

            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            {
                Util.MessageValidation("SFU2042", "31");
                return;
            }

            try
            {
                Util.gridClear(this.dgList);
                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.DoEvents();
                string productID = Convert.ToString(this.cboMultiProductID.SelectedItemsToString);
                string equipmentSegmentID = Convert.ToString(this.cboMultiEquipmentSegmentID.SelectedItemsToString);
                if (this.chkDetail.IsChecked == null || this.chkDetail.IsChecked == false)
                {
                    this.searchFromDate = DateTime.Now.AddYears(-20).ToString("yyyy-MM-dd");
                    this.searchToDate = DateTime.Now.AddYears(20).ToString("yyyy-MM-dd");
                }
                else
                {
                    this.searchFromDate = Convert.ToDateTime(this.dtpDateFrom.SelectedDateTime).ToString("yyyy-MM-dd");
                    this.searchToDate = Convert.ToDateTime(this.dtpDateTo.SelectedDateTime).ToString("yyyy-MM-dd");
                }

                DataTable dt = this.dataHelper.GetStockerSummaryByEOLDate(productID, equipmentSegmentID, this.searchFromDate, this.searchToDate);
                
                if (CommonVerify.HasTableRow(dt))
                {
                    Util.GridSetData(this.dgList, dt, FrameOperation, false);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void SHOW_PACK003_027_REQUEST_POPUPLIST(C1DataGrid c1DataGrid, C1.WPF.DataGrid.DataGridCell dataGridCell)
        {
            if (dataGridCell == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(dataGridCell.Text))
            {
                return;
            }

            string productID = Util.NVC(DataTableConverter.GetValue(dgList.Rows[dataGridCell.Row.Index].DataItem, "PRODID"));
            string equipmentSegmentID = Util.NVC(DataTableConverter.GetValue(dgList.Rows[dataGridCell.Row.Index].DataItem, "EQSGID"));

            // Cell값이 빵떡이면 Return
            if (dataGridCell.Text.Equals("0"))
            {
                return;
            }

            // 합계 Row Double Click 해도 Return
            if (string.IsNullOrEmpty(productID))
            {
                return;
            }

            // Dimension 영역 Double Click 해도 Return
            if (dataGridCell.Column.Index < 4)
            {
                return;
            }

            try
            {
                string firstGroupCode = this.lstFirstGroup.Where(x => x.Item2 == ((List<string>)dataGridCell.Column.Header).ToArray()[0]).Select(x => x.Item1).FirstOrDefault();
                string secondGroupCode = this.lstSecondGroup.Where(x => x.Item2 == ((List<string>)dataGridCell.Column.Header).ToArray()[1]).Select(x => x.Item1).FirstOrDefault();

                // Logis Pack Type
                string logisPackTypeCode = string.Empty;
                if ("TOTAL,SAMPLE_HOLD,TRF_AVA".Contains(firstGroupCode))
                {
                    logisPackTypeCode = Util.NVC(DataTableConverter.GetValue(dgList.Rows[dataGridCell.Row.Index].DataItem, "LOGIS_PACK_TYPE"));
                }
                else if ("CHARGE,ILT4CYCLE,OCVCHARGE".Contains(firstGroupCode))
                {
                    logisPackTypeCode = firstGroupCode;
                }
                else if ("1STOCV,2NDOCV,TRUCK".Contains(firstGroupCode))
                {
                    logisPackTypeCode = "PACK";
                }

                PACK003_027_REQUEST_POPUPLIST popUp = new PACK003_027_REQUEST_POPUPLIST();
                popUp.FrameOperation = this.FrameOperation;
                if (popUp == null)
                {
                    return;
                }

                object[] arrParameter = new object[7];
                arrParameter[0] = productID;
                arrParameter[1] = equipmentSegmentID;
                arrParameter[2] = this.searchFromDate;
                arrParameter[3] = this.searchToDate;
                arrParameter[4] = firstGroupCode;
                arrParameter[5] = secondGroupCode;
                arrParameter[6] = logisPackTypeCode;
                C1WindowExtension.SetParameters(popUp, arrParameter);

                popUp.ShowModal();
                popUp.CenterOnScreen();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SearchOutRackList()
        {
            if (Convert.ToDecimal(Convert.ToDateTime(this.dtpDateFromList.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(this.dtpDateToList.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.Alert("SFU1913");      // 종료일자가 시작일자보다 빠릅니다.
                return;
            }

            try
            {
                Util.gridClear(this.dgOutputInfo);
                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.SearchRowCount(ref this.tbTransferModuleList_Count, 0);
                PackCommon.DoEvents();

                DataTable dt = this.dataHelper.SelectOutRackList(Convert.ToDateTime(this.dtpDateFromList.SelectedDateTime).ToString("yyyyMMdd")
                                                               , Convert.ToDateTime(this.dtpDateToList.SelectedDateTime).ToString("yyyyMMdd"));
                PackCommon.SearchRowCount(ref this.tbTransferModuleList_Count, dt.Rows.Count);
                Util.GridSetData(dgOutputInfo, dt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private bool ValidationManualTransferCancel()
        {
            C1DataGrid dg = dgOutputInfo;

            if (!CommonVerify.HasDataGridRow(dg))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            Util util = new Util();
            if (util.GetDataGridRowCountByCheck(dg, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            return true;
        }

        private void SaveManualTransferCancel()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                DataTable dtRQSTDT = new DataTable("IN_CANCEL_INFO");
                dtRQSTDT.Columns.Add("ORDID", typeof(string));
                dtRQSTDT.Columns.Add("CARRIERID", typeof(string));
                dtRQSTDT.Columns.Add("UPDUSER", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow dataGridRow in this.dgOutputInfo.Rows)
                {
                    if (dataGridRow.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(dataGridRow.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow drRQSTDT = dtRQSTDT.NewRow();
                        drRQSTDT["ORDID"] = DataTableConverter.GetValue(dataGridRow.DataItem, "REQ_TRFID").GetString();
                        drRQSTDT["CARRIERID"] = DataTableConverter.GetValue(dataGridRow.DataItem, "CARRIERID").GetString();
                        drRQSTDT["UPDUSER"] = LoginInfo.USERID;
                        dtRQSTDT.Rows.Add(drRQSTDT);
                    }
                }

                if (CommonVerify.HasTableRow(dtRQSTDT))
                {
                    if (this.dataHelper.SaveManualTransferCancel(dtRQSTDT))
                    {
                        Util.MessageInfo("SFU1275"); // 정상 처리 되었습니다.
                        this.SearchOutRackList();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
            this.Loaded -= new RoutedEventHandler(this.UserControl_Loaded);
        }

        private void chkDetail_Checked(object sender, RoutedEventArgs e)
        {
            this.dtpDateFrom.IsEnabled = true;
            this.dtpDateTo.IsEnabled = true;
        }

        private void chkDetail_Unchecked(object sender, RoutedEventArgs e)
        {
            this.dtpDateFrom.IsEnabled = false;
            this.dtpDateTo.IsEnabled = false;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess();
        }

        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (e.Cell.Column.Name.Equals("AVA_TRF_REQ_QTY"))
                {
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                    if (convertFromString != null)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                }
                else
                {
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                    if (convertFromString != null)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }

        private void dgList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid c1DataGrid = (C1DataGrid)sender;
            C1.WPF.DataGrid.DataGridCell dataGridCell = c1DataGrid.GetCellFromPoint(e.GetPosition(null));
            this.SHOW_PACK003_027_REQUEST_POPUPLIST(c1DataGrid, dataGridCell);
        }

        private void btnTransferModuleList_Search_Click(object sender, RoutedEventArgs e)
        {
            this.SearchOutRackList();
        }

        private void btnManualTransferCancel_Click(object sender, RoutedEventArgs e)
        {

            if (!ValidationManualTransferCancel()) return;

            // 수동출고 예약 취소하시겠습니까?
            Util.MessageConfirm("SFU4544", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    this.SaveManualTransferCancel();
                }
            });
        }
        #endregion
    }

    internal class PACK003_027_DataHelper
    {
        #region Member Variable Lists...
        #endregion

        #region Constructor
        public PACK003_027_DataHelper()
        {
        }
        #endregion

        #region Member Function Lists...
        // 순서도 호출 - Pack Line
        internal DataTable GetPackLineInfo()
        {
            string bizRuleName = "DA_BAS_SEL_LOGIS_LINE_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
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
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - 제품
        internal DataTable GetProductInfo()
        {
            string bizRuleName = "DA_PRD_SEL_SHOP_PRDT_ROUT_MODULE_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
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

                    dtRSLTDT = PackCommon.queryToDataTable(query);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - 일별 스토카 모듈 재고현황
        internal DataTable GetStockerSummaryByEOLDate(string productID, string equipmentSegmentID, string fromDate, string toDate)
        {
            string bizRuleName = "DA_PRD_SEL_LOGIS_STK_LOT_BY_EOLDATE_SUM";
            //string bizRuleName = "DA_PRD_SEL_LOGIS_STK_LOT_BY_EOLDATE_SUM_COPY";        // For Test (폴란드 Pack 2동, 폴란드 Pack 3동 운영)
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("FROM_DAY", typeof(string));
                dtRQSTDT.Columns.Add("TO_DAY", typeof(string));
                dtRQSTDT.Columns.Add("PROD_LIST", typeof(string));
                dtRQSTDT.Columns.Add("EQSG_LIST", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["FROM_DAY"] = string.IsNullOrEmpty(fromDate) ? null : fromDate;
                drRQSTDT["TO_DAY"] = string.IsNullOrEmpty(toDate) ? null : toDate;
                drRQSTDT["EQSG_LIST"] = string.IsNullOrEmpty(equipmentSegmentID) ? null : equipmentSegmentID;
                drRQSTDT["PROD_LIST"] = string.IsNullOrEmpty(productID) ? null : productID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - OutRack List
        internal DataTable SelectOutRackList(string fromDate, string toDate)
        {
            string bizRuleName = "DA_SEL_MCS_REQ_TRF_INFO_BY_MES_GUI";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("FROM_DATE", typeof(string));
                dtRQSTDT.Columns.Add("TO_DATE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["FROM_DATE"] = fromDate;
                drRQSTDT["TO_DATE"] = toDate;
                dtRQSTDT.Rows.Add(drRQSTDT);

                DataTable dtMCSBizActorServerInfo = PackCommon.GetMCSBizActorServerInfo("FP_MCS_AP_LOGIS_CONFIG");
                foreach (DataRowView drvMCSBizActorServerInfo in dtMCSBizActorServerInfo.AsDataView())
                {
                    ClientProxy clientProxy = new ClientProxy(drvMCSBizActorServerInfo["BIZACTORIP"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORPROTOCOL"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORPORT"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORSERVICEMODE"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORSERVICEINDEX"].ToString());
                    dtRSLTDT = clientProxy.ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Save Manual Transfer Cancel
        internal bool SaveManualTransferCancel(DataTable dtRQSTDT)
        {
            bool returnValue = true;
            string bizRuleName = "BR_PRD_REG_TRF_JOB_CANCEL_BY_MES";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();

            try
            {
                dsINDATA.Tables.Add(dtRQSTDT);
                DataTable dtMCSBizActorServerInfo = PackCommon.GetMCSBizActorServerInfo("FP_MCS_AP_LOGIS_CONFIG");

                foreach (DataRowView drvMCSBizActorServerInfo in dtMCSBizActorServerInfo.AsDataView())
                {
                    ClientProxy clientProxy = new ClientProxy(drvMCSBizActorServerInfo["BIZACTORIP"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORPROTOCOL"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORPORT"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORSERVICEMODE"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORSERVICEINDEX"].ToString());
                    dsOUTDATA = clientProxy.ExecuteServiceSync_Multi(bizRuleName, "IN_CANCEL_INFO", "OUT_DATA", dsINDATA);
                }
            }
            catch (Exception ex)
            {
                returnValue = false;
                Util.MessageException(ex);
            }

            return returnValue;
        }
        #endregion
    }
}