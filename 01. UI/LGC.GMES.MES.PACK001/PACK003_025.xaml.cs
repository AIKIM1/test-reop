/************************************************************************************
  Created Date : 2021.04.06
       Creator : 정용석
   Description : 모듈 반송 요청
 ------------------------------------------------------------------------------------
  [Change History]
    2021.04.06  정용석 : Initial Created.
    2021.08.19  정용석 : 장기재고 연동내역 추가
    2021.10.12  정용석 : 반송이력 LOT별 검색 TAB 추가
    2021.11.08  김길용 : 공통화면으로인한 샘플링 컬럼 추가 및 하드코딩 제거
    2021.11.11  김길용 : Pack3동 공통화 수정,하드코딩 제거 및 샘플링,AREAID 컬럼 추가
    2022.03.30  김길용 : 반송요청현황정보 Logis_pack_type 컬럼 추가(FA신승동요구사항)
    2022.06.22  김길용 : 반송 요청 이력 조회 (LOT별) 시 포장기설비 조회조건 적용되도록 수정
    2023.05.08  정용석 : [E20230110-000011] [공정PI팀] GMES 시스템의 ESWA MEB Stocker 생산성 향상 및 관리를 위한 시스템 기능 개선 件
                         반송요청현황 - 조회시에 Pack LIne이 전체가 아닌경우 색상 & 강조 표시
                                      - LOT 생성일자 (BOX_LOT_SET_DATE) 추가
                         모듈반송이력 - 일시는 시간까지 입력가능하도록 변경, RoutID, ProcessID 조회조건 추가
                                      - 장표Grid에 RoutID, ProcessID, RoutName, ProcessName 컬럼 추가
    2024.01.19  김길용 : 모듈반송이력 시간까지만 입력하도록 UI변경(분,초 삭제)
 ************************************************************************************/
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
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_025 : UserControl, IWorkArea
    {
        #region Member Variable Lists...
        private const string TRANSFER_REQUEST = "TRANSFER_REQUEST";
        private const string TRANSFER_HISTORY = "TRANSFER_HISTORY";
        private const string TRANSFER_HISTORY_BY_LOT = "TRANSFER_HISTORY_BY_LOT";
        private PACK003_025_DataHelper dataHelper = new PACK003_025_DataHelper();

        private int productIDMultiComboCount = 0;
        private int assyAreaMultiComboCount = 0;
        private int assyLineMultiComboCount = 0;
        private int elecLineMultiComboCount = 0;
        private int packLineMultiComboCount = 0;
        private int transferRequestStatusMultiComboCount = 0;
        private int packEquipmentMultiComboCount = 0;
        private int routIDMultiComboCount = 0;
        private int processIDMultiComboCount = 0;
        #endregion

        #region Property
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        public PACK003_025()
        {
            InitializeComponent();
            PackCommon.SetPopupDraggable(this.popupBaseInfo, this.pnlTitleBaseInfo);
        }
        #endregion

        #region Member Function Lists...
        // 초기화
        private void Initialize()
        {
            try
            {
                PackCommon.SearchRowCount(ref this.txtRowCountRequestHistory, 0);
                PackCommon.SearchRowCount(ref this.txtRowCountTransferRequest, 0);
                this.SetDatePicker();
                this.SetComboBox();
                this.SetColumnsVisible();
                this.SetTagControl();
                Util.pageAuth(this.grdRoot.Children.OfType<Button>().Where(x => !x.Tag.ToString().Contains("SEARCH")).ToList(), FrameOperation.AUTHORITY);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 날짜 Setting
        private void SetDatePicker()
        {
            // 반송이력 영역
            this.dtpFromDate.ApplyTemplate();
            this.dtpToDate.ApplyTemplate();
            this.dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-3);
            this.dtpToDate.SelectedDateTime = DateTime.Now;

            // 모듈반송이력 영역
            //this.dtpFromDateByLOT.ApplyTemplate();
            //this.dtpToDateByLOT.ApplyTemplate();
            //this.dtpFromDateByLOT.DateTime = PackCommon.TruncateDateTime(DateTime.Now.AddDays(-3), TimeSpan.FromDays(1.0));
            //this.dtpToDateByLOT.DateTime = PackCommon.TruncateDateTime(DateTime.Now, TimeSpan.FromDays(1.0));
            //날자 초기값 세팅
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7); //일주일 전 날짜
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;//오늘 날짜

            //2018.05.28
            Util.Set_Pack_cboTimeList(cboTimeFrom, "CBO_NAME", "CBO_CODE", "06:00:00");
            Util.Set_Pack_cboTimeList(cboTimeTo, "CBO_NAME", "CBO_CODE", "23:59:59");
        }

        // 조회영역 ComboBox Binding
        private void SetComboBox()
        {
            // 반송 영역
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetProductInfo(), this.cboMultiProdIDTransferRequest, ref this.productIDMultiComboCount);
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetAssyAreaInfo(), this.cboMultiProdAssyArea, ref this.assyAreaMultiComboCount);
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetAssyEquipmentInfo(), this.cboMultiProdAssyLine, ref this.assyLineMultiComboCount);
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetElecEquipmentInfo(), this.cboMultiProdElecLine, ref this.elecLineMultiComboCount);
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetPackLineInfo(), this.cboMultiProdPackLine, ref this.packLineMultiComboCount);

            // 반송이력 영역
            PackCommon.SetC1ComboBox(this.dataHelper.GetProductInfo(), this.cboProdIDRequestHistory, "-ALL-");
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetTrasnferRequestStatusCodeInfo(), this.cboMultiTransferRequestStatusCode, ref this.transferRequestStatusMultiComboCount);
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetTrasnferRequestStatusCodeInfo(), this.cboMultiTransferRequestStatusCodePopUp, ref this.transferRequestStatusMultiComboCount);
            this.SetMultiSelectionComboBox(this.cboMultiTransferRequestStatusCode);
            this.SetMultiSelectionComboBox(this.cboMultiTransferRequestStatusCodePopUp);

            // 모듈반송이력 영역
            PackCommon.SetC1ComboBox(this.dataHelper.GetProductInfo(), this.cboProdIDRequestHistoryByLOT, "-ALL-");
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetPackEquipmentInfo(), this.cboMultiPackEquipmentIDRequestHistoryByLOT, ref this.packEquipmentMultiComboCount);
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetTrasnferRequestStatusCodeInfo(), this.cboMultiTransferRequestStatusCodeByLOT, ref this.transferRequestStatusMultiComboCount);
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetPackProcessInfo(), this.cboMultiProcessIDRequestHistoryByLOT, ref this.processIDMultiComboCount);
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetRouteInfo(), this.cboMultiRoutIDRequestHistoryByLOT, ref this.routIDMultiComboCount);
            this.SetMultiSelectionComboBox(this.cboMultiTransferRequestStatusCodeByLOT);
        }

        // 컬럼 조회
        private void SetColumnsVisible()
        {
            if (LoginInfo.CFG_AREA_ID == "PA") dgTransferRequest.Columns["SMPL_HOLD_QTY"].Visibility = Visibility.Visible;
        }

        // Initialize - 반송상태 코드 Multi ComboBox Set
        private void SetMultiSelectionComboBox(MultiSelectionBox cboMulti)
        {
            try
            {
                if (cboMulti.ItemsSource == null)
                {
                    return;
                }

                DataTable dt = DataTableConverter.Convert(cboMulti.ItemsSource);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if ("REQUEST_LOGIS,RECEIVING_LOGIS,CLOSED_LOGIS".Contains(dt.Rows[i]["PACK_LOGIS_TRF_REQ_STAT_CODE"].ToString()))
                    {
                        cboMulti.Check(i);
                    }
                    else
                    {
                        cboMulti.Uncheck(i);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Initialize - Control에 Tag값 설정
        private void SetTagControl()
        {
            this.dgTransferRequest.Tag = TRANSFER_REQUEST;
            this.cboMultiProdIDTransferRequest.Tag = TRANSFER_REQUEST;
            this.cboMultiProdAssyArea.Tag = TRANSFER_REQUEST;
            this.cboMultiProdElecLine.Tag = TRANSFER_REQUEST;
            this.cboMultiProdAssyLine.Tag = TRANSFER_REQUEST;
            this.cboMultiProdPackLine.Tag = TRANSFER_REQUEST;
            this.txtRowCountTransferRequest.Tag = TRANSFER_REQUEST;
            this.btnSearchTransferRequest.Tag = TRANSFER_REQUEST + "|" + "SEARCH";

            this.dgTransferRequestHistory.Tag = TRANSFER_HISTORY;
            this.cboProdIDRequestHistory.Tag = TRANSFER_HISTORY;
            this.cboMultiTransferRequestStatusCode.Tag = TRANSFER_HISTORY;
            this.txtRowCountRequestHistory.Tag = TRANSFER_HISTORY;
            this.txtTransferRequestNo.Tag = TRANSFER_HISTORY;
            this.btnSearchTransferHistory.Tag = TRANSFER_HISTORY + "|" + "SEARCH";

            this.dgTransferRequestHistoryByLOT.Tag = TRANSFER_HISTORY_BY_LOT;
            this.cboProdIDRequestHistoryByLOT.Tag = TRANSFER_HISTORY_BY_LOT;
            this.cboMultiTransferRequestStatusCodeByLOT.Tag = TRANSFER_HISTORY_BY_LOT;
            this.txtRowCountRequestHistoryByLOT.Tag = TRANSFER_HISTORY_BY_LOT;
            this.btnSearchTransferHistoryByLOT.Tag = TRANSFER_HISTORY_BY_LOT + "|" + "SEARCH";
        }

        // Popup - 수동반송 및 수동포트배출 Popup 호출
        private void ShowPopUpTransferRequest(C1DataGrid c1DataGrid, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.DataGridCell dataGridCell = c1DataGrid.GetCellFromPoint(e.GetPosition(null));
                if (dataGridCell == null)
                {
                    return;
                }

                // 합계 Row는 안됨.
                if (Convert.ToBoolean(DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "TOTAL_FLAG")))
                {
                    return;
                }

                // HOLD나 장기재고 수량 컬럼이 아닌 경우는 Interlock
                if (!dataGridCell.Column.Name.Equals("HOLD_QTY") && !dataGridCell.Column.Name.Equals("EXPIRED_QTY"))
                {
                    return;
                }

                // 수량 관련 Column인 경우 Popup 표출이나 수량이 0보다 작은 경우 Interlock
                if (c1DataGrid.Columns[dataGridCell.Column.Index].Name.Contains("QTY"))
                {
                    int requestQty = Convert.ToInt32(DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, dataGridCell.Column.Name));
                    if (requestQty <= 0)
                    {
                        Util.MessageInfo("SFU1683");
                        return;
                    }
                }

                // 포장기중에 수동반송설정이 되어 있는 포장기가 없으면 Interlock
                if (!CommonVerify.HasTableRow(this.dataHelper.GetPackEquipmentInfo("N")))
                {
                    Util.MessageInfo("SFU8385");    // 수동반송으로 설정된 포장기가 없습니다. 포장기 설정 내역을 확인해주세요.
                    return;
                }

                PACK003_025_REQUEST popUp = new PACK003_025_REQUEST();
                popUp.FrameOperation = this.FrameOperation;
                if (popUp == null)
                {
                    return;
                }

                object[] obj = new object[13];
                obj[0] = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "PRODID").ToString();
                obj[1] = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "PRJT_NAME").ToString();
                obj[2] = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "PROD_ASSY_AREA_LIST").ToString();
                obj[3] = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "PROD_ELTR_LINE_LIST").ToString();
                obj[4] = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "PROD_ASSY_LINE_LIST").ToString();
                obj[5] = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "PROD_PACK_LINE_LIST").ToString();
                obj[6] = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "PROD_ASSY_AREA_LIST_NAME").ToString();
                obj[7] = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "PROD_ELTR_LINE_LIST_NAME").ToString();
                obj[8] = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "PROD_ASSY_LINE_LIST_NAME").ToString();
                obj[9] = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "PROD_PACK_LINE_LIST_NAME").ToString();

                // 재공구분
                switch (dataGridCell.Column.Name.ToUpper())
                {
                    case "ACTIVITY_QTY":                // 가용재고
                        obj[10] = "G";
                        break;
                    case "HOLD_QTY":                    // Hold
                        obj[10] = "H";
                        break;
                    case "NG_QTY":                      // NG
                        obj[10] = "N";
                        break;
                    case "NON_ACTIVITY_QTY":            // 경과일 이전
                        obj[10] = "B";
                        break;
                    case "EXPIRED_QTY":                 // 장기재고
                        obj[10] = "O";
                        break;
                    case "SMPL_HOLD_QTY":                 // 샘플링 Hold(Pack3)
                        obj[10] = "S";
                        break;
                    default:
                        obj[10] = string.Empty;
                        break;
                }

                if (!string.IsNullOrEmpty(obj[10].ToString()))
                {
                    DataTable dt = this.dataHelper.GetRequestWipTypeCodeInfo(new List<string> { obj[10].ToString() }, false);
                    foreach (DataRowView drv in dt.AsDataView())
                    {
                        obj[11] = drv["REQ_WIP_TYPE_CODE_NAME"].ToString();
                    }
                }

                // 수량
                if (dataGridCell.Column.Name.ToUpper().Contains("QTY"))
                {
                    obj[12] = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, dataGridCell.Column.Name).ToString();
                }
                else
                {
                    obj[12] = Convert.ToString(0);
                }

                C1WindowExtension.SetParameters(popUp, obj);
                popUp.Closed += new EventHandler(this.popUp_Closed);
                popUp.ShowModal();
                popUp.CenterOnScreen();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Popup - 반송요청 상세정보 Popup 호출
        private void ShowPopupTransferDetailHistory(C1DataGrid c1DataGrid, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                this.HidePopUp();

                C1.WPF.DataGrid.DataGridCell dataGridCell = c1DataGrid.GetCellFromPoint(e.GetPosition(null));
                if (dataGridCell == null)
                {
                    return;
                }

                this.txtRequestNo.Text = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "TRF_REQ_NO").ToString();
                this.txtAssyAreaList.Text = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "PROD_ASSY_AREA_LIST_NAME").ToString();
                this.txtAssyLineList.Text = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "PROD_ASSY_LINE_LIST_NAME").ToString();
                this.txtEltrLineList.Text = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "PROD_ELTR_LINE_LIST_NAME").ToString();
                this.txtPackLineList.Text = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "PROD_PACK_LINE_LIST_NAME").ToString();
                this.txtPackMixTypeCode.Text = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "PACK_MIX_TYPE_CODE")?.ToString() + " : " +
                                               DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "PACK_MIX_TYPE_CODE_NAME")?.ToString();
                this.txtRequestUser.Text = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "REQ_USER").ToString();
                this.txtRequestDate.Text = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "REQ_DTTM").ToString();
                this.txtRequestQty.Text = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "TRF_LOT_QTY").ToString();
                this.txtReceivingLotQty.Text = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "RECEIVING_LOT_QTY").ToString();

                this.SearchTransferRequestDetail();

                // Popup 표출 위치 (정가운데)
                this.popupBaseInfo.Placement = PlacementMode.Center;
                this.popupBaseInfo.IsOpen = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Popup - Close Popup
        private void HidePopUp()
        {
            this.popupBaseInfo.IsOpen = false;
            this.popupBaseInfo.HorizontalOffset = 0;
            this.popupBaseInfo.VerticalOffset = 0;
        }

        // 조회
        private void SearchProcess(Button button)
        {
            this.HidePopUp();

            string[] arrTag = button.Tag.ToString().Split('|').ToArray<string>();
            try
            {
                if (arrTag[0].Equals(TRANSFER_REQUEST))
                {
                    // Validation of Search Condition
                    if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiProdIDTransferRequest.SelectedItemsToString)))
                    {
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("PRODID")); // %1(을)를 선택하세요.
                        this.cboMultiProdIDTransferRequest.Focus();
                        return;
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiProdAssyArea.SelectedItemsToString)))
                    {
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("조립동")); // %1(을)를 선택하세요.
                        this.cboMultiProdAssyArea.Focus();
                        return;
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiProdAssyLine.SelectedItemsToString)))
                    {
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("조립설비")); // %1(을)를 선택하세요.
                        this.cboMultiProdAssyLine.Focus();
                        return;
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiProdElecLine.SelectedItemsToString)))
                    {
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("전극설비")); // %1(을)를 선택하세요.
                        this.cboMultiProdElecLine.Focus();
                        return;
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiProdPackLine.SelectedItemsToString)))
                    {
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("PACK_LINE")); // %1(을)를 선택하세요.
                        this.cboMultiProdPackLine.Focus();
                        return;
                    }
                    this.SearchTransferRequest();
                }

                if (arrTag[0].Equals(TRANSFER_HISTORY))
                {
                    this.SearchTransferHistory();
                }

                if (arrTag[0].Equals(TRANSFER_HISTORY_BY_LOT))
                {
                    if (string.IsNullOrEmpty(this.txtLOTID.Text))
                    {
                        this.SearchTransferHistoryByLOT();
                    }
                    else
                    {
                        this.SearchTransferHistoryByLOT(true);
                    }
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

        // 조회 - 수동반송요청(모듈)
        private void SearchTransferRequest()
        {
            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();

            PackCommon.SearchRowCount(ref this.txtRowCountTransferRequest, 0);
            Util.gridClear(this.dgTransferRequest);

            // 조립동, 조립설비, 전극설비는 Unchecked 체크만 하고 전체선택한 경우에는 null로 넘김.
            string productID = Convert.ToString(this.cboMultiProdIDTransferRequest.SelectedItemsToString);
            string prodAssyAreaSelectedItem = this.cboMultiProdAssyArea.SelectedItems.Count().Equals(this.assyAreaMultiComboCount) ? string.Empty : Convert.ToString(this.cboMultiProdAssyArea.SelectedItemsToString);
            string prodAssyLineSelectedItem = this.cboMultiProdAssyLine.SelectedItems.Count().Equals(this.assyLineMultiComboCount) ? string.Empty : Convert.ToString(this.cboMultiProdAssyLine.SelectedItemsToString);
            string prodElecLineSelectedItem = this.cboMultiProdElecLine.SelectedItems.Count().Equals(this.elecLineMultiComboCount) ? string.Empty : Convert.ToString(this.cboMultiProdElecLine.SelectedItemsToString);
            string prodPackLineSelectedItem = Convert.ToString(this.cboMultiProdPackLine.SelectedItemsToString);

            DataTable dt = this.dataHelper.GetModuleTransferRequestList(productID, prodAssyAreaSelectedItem, prodAssyLineSelectedItem, prodElecLineSelectedItem, prodPackLineSelectedItem, this.chkDetail.IsChecked);
            if (CommonVerify.HasTableRow(dt))
            {
                PackCommon.SearchRowCount(ref this.txtRowCountTransferRequest, dt.Rows.Count);
                Util.GridSetData(this.dgTransferRequest, dt, FrameOperation);
            }

            this.loadingIndicator.Visibility = Visibility.Collapsed;
        }

        // 조회 - 반송요청 현황
        private void SearchTransferHistory()
        {
            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();
            PackCommon.SearchRowCount(ref this.txtRowCountRequestHistory, 0);
            Util.gridClear(this.dgTransferRequestHistory);

            string productID = this.cboProdIDRequestHistory.SelectedValue.ToString();
            DateTime fromDate = this.dtpFromDate.SelectedDateTime;
            DateTime toDate = this.dtpToDate.SelectedDateTime;
            string transferRequestStatusCode = Convert.ToString(this.cboMultiTransferRequestStatusCode.SelectedItemsToString);
            string transferRequestNo = this.txtTransferRequestNo.Text;

            DataTable dt = this.dataHelper.GetModuleTransferHistoryList(productID, fromDate, toDate, transferRequestStatusCode, transferRequestNo);
            if (CommonVerify.HasTableRow(dt))
            {
                PackCommon.SearchRowCount(ref this.txtRowCountRequestHistory, dt.Rows.Count);
                Util.GridSetData(this.dgTransferRequestHistory, dt, FrameOperation);
            }

            this.loadingIndicator.Visibility = Visibility.Collapsed;
        }

        // 조회 - 모듈반송이력
        private void SearchTransferHistoryByLOT(bool isSearchbyLOT = false)
        {
            // Declarations...
            string productID = string.Empty;
            DateTime? fromRequestDate = DateTime.MinValue;
            DateTime? toRequestDate = DateTime.MaxValue;
            string fromRequestDateSev = string.Empty;
            string toRequestDateSev = string.Empty;
            string transferCSTStatusCodeList = string.Empty;
            string transferPackEquipmentList = string.Empty;
            string LOTID = string.Empty;
            string processID = string.Empty;
            string routID = string.Empty;

            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.DoEvents();
                PackCommon.SearchRowCount(ref this.txtRowCountRequestHistoryByLOT, 0);
                Util.gridClear(this.dgTransferRequestHistoryByLOT);

                if (!isSearchbyLOT)
                {
                    productID = this.cboProdIDRequestHistoryByLOT.SelectedValue.ToString();
                    //fromRequestDate = this.dtpDateFrom.SelectedDateTime;// this.cboTimeFrom.SelectedDateTime;
                    //toRequestDate = this.dtpToDateByLOT.DateTime;
                    fromRequestDateSev = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeFrom.SelectedValue.ToString();
                    toRequestDateSev = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeTo.SelectedValue.ToString();

                    transferCSTStatusCodeList = Convert.ToString(this.cboMultiTransferRequestStatusCodeByLOT.SelectedItemsToString);
                    transferPackEquipmentList = Convert.ToString(this.cboMultiPackEquipmentIDRequestHistoryByLOT.SelectedItemsToString);
                    LOTID = this.txtLOTID.Text;

                    processID = Convert.ToString(this.cboMultiProcessIDRequestHistoryByLOT.SelectedItemsToString);
                    routID = Convert.ToString(this.cboMultiRoutIDRequestHistoryByLOT.SelectedItemsToString);
                }

                DataTable dt = this.dataHelper.GetModuleTransferHistoryListByLOT(productID, DateTime.Parse(fromRequestDateSev), DateTime.Parse(toRequestDateSev), transferCSTStatusCodeList, LOTID, transferPackEquipmentList, processID, routID);
                if (CommonVerify.HasTableRow(dt))
                {
                    PackCommon.SearchRowCount(ref this.txtRowCountRequestHistoryByLOT, dt.Rows.Count);
                    Util.GridSetData(this.dgTransferRequestHistoryByLOT, dt, FrameOperation);
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

        // 조회 - 수동반송요청(모듈) 상세
        private void SearchTransferRequestDetail()
        {
            string trfStatusCodeSelectedItem = this.cboMultiTransferRequestStatusCodePopUp.SelectedItems.Count().Equals(this.transferRequestStatusMultiComboCount) ? string.Empty : Convert.ToString(this.cboMultiTransferRequestStatusCodePopUp.SelectedItemsToString);
            DataTable dt = this.dataHelper.GetModuleTransferDetailList(this.txtRequestNo.Text, trfStatusCodeSelectedItem);
            Util.GridSetData(this.dgTransferRequestDetail, dt, FrameOperation);
        }

        // 반송종료 처리 Validation
        private bool ValidationCheckTransferComplete(DataTable dt)
        {
            if (!CommonVerify.HasTableRow(dt))
            {
                Util.MessageValidation("SFU3538");
                return false;
            }

            // 사용자 체크
            if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
            {
                Util.MessageValidation("SFU4591");  // 작업자를 입력하세요
                this.ucPersonInfo.Focus();
                return false;
            }

            // 사유 입력
            if (string.IsNullOrEmpty(this.txtRemark.Text))
            {
                Util.MessageValidation("SFU2076"); // 변경사유는 필수 입력항목입니다.
                return false;
            }

            // Request Logis, Receiving Logis, Closed Logis 상태 이외의 상태인 것을 선택한 경우에는 Interlock
            var query = dt.AsEnumerable().Where(x => x.Field<bool>("CHK") && "REQUEST_LOGIS,RECEIVING_LOGIS,CLOSED_LOGIS".Contains(x.Field<string>("TRF_REQ_STAT_CODE").ToUpper()));
            if (query.Count() <= 0)
            {
                Util.MessageValidation("SFU8386");      // 반송상태가 (REQUEST_LOGIS, RECEIVING_LOGIS, CLOSED_LOGIS) 반송요청건만 반송종료가 가능합니다.
                return false;
            }
            var query1 = dt.AsEnumerable().Where(x => x.Field<bool>("CHK") && !"REQUEST_LOGIS,RECEIVING_LOGIS,CLOSED_LOGIS".Contains(x.Field<string>("TRF_REQ_STAT_CODE").ToUpper()));
            if (query1.Count() > 0)
            {
                Util.MessageValidation("SFU8386");      // 반송상태가 (REQUEST_LOGIS, RECEIVING_LOGIS, CLOSED_LOGIS) 반송요청건만 반송종료가 가능합니다.
                return false;
            }

            return true;
        }

        // 반송종료 처리
        private void SetTransferComplete()
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(this.dgTransferRequestHistory.ItemsSource);
                if (!this.ValidationCheckTransferComplete(dt))
                {
                    return;
                }

                // REQUEST_LOGIS,RECEIVING_LOGIS,CLOSED_LOGIS 상태인 것들만 보냄.
                DataTable dtTarget = dt.AsEnumerable().Where(x => x.Field<bool>("CHK") && "REQUEST_LOGIS,RECEIVING_LOGIS,CLOSED_LOGIS".Contains(x.Field<string>("TRF_REQ_STAT_CODE").ToUpper())).CopyToDataTable();

                Util.MessageConfirm("SFU5164", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (this.dataHelper.SaveModuleTransferComplete(dtTarget, this.ucPersonInfo.UserID, this.txtRemark.Text))
                        {
                            this.SearchProcess(this.btnSearchTransferHistory);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // BeginningEdit Event가 발생했을 때
        private void BeginningEditEventFireProcess(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid c1DataGrid = (C1DataGrid)sender;
            DataRowView dataRowView = (DataRowView)e.Row.DataItem;

            if (!e.Column.Name.ToUpper().Equals("CHK") && !dataRowView.Row["CHK"].SafeToBoolean())
            {
                e.Cancel = true;
            }
        }

        // Began Event가 발생했을 때 (현재는 아무짓 안함)
        private void BeganEditEventFireProcess(object sender, DataGridBeganEditEventArgs e)
        {

        }

        // CommittedEdit Event가 발생했을 때 (현재는 아무짓 안함)
        private void CommittedEditEventFireProcess(object sender, DataGridCellEventArgs e)
        {

        }

        // Merge Event가 발생했을 때
        private void MergeCellsEventFireProcess(DataGridMergingCellsEventArgs e)
        {
            try
            {
                if (this.dgTransferRequest.GetRowCount() == 0)
                {
                    return;
                }

                // 소계 Row랑 합계 Row 찾기
                var query = DataTableConverter.Convert(this.dgTransferRequest.ItemsSource).AsEnumerable().Select((x, rowNumber) => new
                {
                    ROW_NUMBER = rowNumber + 2,
                    TOTAL_FLAG = Convert.ToBoolean(x.Field<int>("TOTAL_FLAG")),
                    SUMMARY_FLAG = Convert.ToBoolean(x.Field<int>("SUMMARY_FLAG"))
                }).Where(x => x.SUMMARY_FLAG || x.TOTAL_FLAG);

                foreach (var item in query)
                {
                    if (this.chkDetail.IsChecked.Equals(true))
                    {
                        if (item.SUMMARY_FLAG && !item.TOTAL_FLAG)
                        {
                            e.Merge(new DataGridCellsRange(this.dgTransferRequest.GetCell(item.ROW_NUMBER, 8), this.dgTransferRequest.GetCell(item.ROW_NUMBER, 11)));
                        }
                    }
                    if (item.TOTAL_FLAG)
                    {
                        e.Merge(new DataGridCellsRange(this.dgTransferRequest.GetCell(item.ROW_NUMBER, 6), this.dgTransferRequest.GetCell(item.ROW_NUMBER, 11)));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Tab 선택한게 바뀌었을 때
        private void TabItemSelectedChangedEventFireProcess()
        {
            // TODO:
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

                if (c1DataGrid.Tag == null)
                {
                    return;
                }

                switch (c1DataGrid.Tag.ToString())
                {
                    case TRANSFER_REQUEST:
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e.Cell.Column.Name.ToUpper().Contains("QTY"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            }
                        }
                        break;
                    case TRANSFER_HISTORY:
                        if (e.Cell.Column.Name.ToUpper().Equals("PROD_PACK_LINE_LIST_NAME") && !e.Cell.Value.ToString().Trim().Equals("ALL"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }

                        if ((e.Cell.Row.Type == DataGridRowType.Item && e.Cell.Column.Name.Equals("TRF_REQ_STAT_CODE")) ||
                            (e.Cell.Row.Type == DataGridRowType.Item && e.Cell.Column.Name.Equals("TRF_CST_STAT_CODE")))
                        {
                            switch (e.Cell.Text.ToUpper())
                            {
                                // REQUEST_LOGIS : 파랑, RECEIVING_LOGIS : 녹색, CLOSED_LOGIS : 회색, CANCELLED_LOGIS : 빨강, COMPLETED_LOGIS : 빨강, RECEIVED_LOGIS : 검정, RESERVED_COMP_LOGIS : 녹색
                                case "REQUEST_LOGIS":
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    break;
                                case "RECEIVING_LOGIS":
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Green);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    break;
                                case "CLOSED_LOGIS":
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Gray);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    break;
                                case "CANCELLED_LOGIS":
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    break;
                                case "COMPLETED_LOGIS":
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    break;
                                case "RECEIVED_LOGIS":
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    break;
                                case "RESERVED_COMP_LOGIS":
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Green);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case TRANSFER_HISTORY_BY_LOT:
                        if ((e.Cell.Row.Type == DataGridRowType.Item && e.Cell.Column.Name.Equals("TRF_REQ_STAT_CODE")) ||
                            (e.Cell.Row.Type == DataGridRowType.Item && e.Cell.Column.Name.Equals("TRF_CST_STAT_CODE")))
                        {
                            switch (e.Cell.Text.ToUpper())
                            {
                                // REQUEST_LOGIS : 파랑, RECEIVING_LOGIS : 녹색, CLOSED_LOGIS : 회색, CANCELLED_LOGIS : 빨강, COMPLETED_LOGIS : 빨강, RECEIVED_LOGIS : 검정, RESERVED_COMP_LOGIS : 녹색
                                case "REQUEST_LOGIS":
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    break;
                                case "RECEIVING_LOGIS":
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Green);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    break;
                                case "CLOSED_LOGIS":
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Gray);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    break;
                                case "CANCELLED_LOGIS":
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    break;
                                case "COMPLETED_LOGIS":
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    break;
                                case "RECEIVED_LOGIS":
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    break;
                                case "RESERVED_COMP_LOGIS":
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Green);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Grid MouseDoubleClick 이벤트가 발생했을 때
        private void MouseDoubleClickEventFireProcess(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid c1DataGrid = (C1DataGrid)sender;
            switch (c1DataGrid.Tag.ToString())
            {
                case TRANSFER_REQUEST:
                    this.ShowPopUpTransferRequest(c1DataGrid, e);
                    break;
                case TRANSFER_HISTORY:
                    this.ShowPopupTransferDetailHistory(c1DataGrid, e);
                    break;
                default:
                    break;
            }
        }

        // Grid의 Header CheckBox Check 또는 Uncheck했을때
        private void SetGridRowChecked(bool check)
        {
            PackCommon.GridCheckAllFlag(this.dgTransferRequestHistory, check, "CHK");
        }

        // 조회
        private void SearchClipboardData()
        {
            try
            {
                this.txtLOTID.Text = string.Empty;
                string[] separators = new string[] { "\r\n" };
                string clipboardText = Clipboard.GetText();
                string[] arrLOTIDList = clipboardText.Split(separators, StringSplitOptions.None);

                if (arrLOTIDList.Count() > 100)
                {
                    Util.MessageValidation("SFU3695");   // 최대 100개 까지 가능합니다.
                    return;
                }

                for (int i = 0; i < arrLOTIDList.Length; i++)
                {
                    if (string.IsNullOrEmpty(arrLOTIDList[i]))
                    {
                        Util.MessageInfo("SFU1190", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                return;
                            }
                        });
                    }
                }

                this.txtLOTID.Text = string.Join(",", arrLOTIDList);
                this.SearchTransferHistoryByLOT(true);
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
            this.Loaded -= new RoutedEventHandler(this.UserControl_Loaded);
        }

        private void dgGrid_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            this.BeginningEditEventFireProcess(sender, e);
        }

        private void dgGrid_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            this.BeganEditEventFireProcess(sender, e);
        }

        private void dgGrid_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            this.CommittedEditEventFireProcess(sender, e);
        }

        private void dgGrid_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            this.LoadedCellPresenterEventFireProcess((C1DataGrid)sender, e);
        }

        private void dgGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.MouseDoubleClickEventFireProcess(sender, e);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess((Button)sender);
        }

        private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.TabItemSelectedChangedEventFireProcess();
        }

        private void dgGrid_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            this.MergeCellsEventFireProcess(e);
        }

        private void popUp_Closed(object sender, EventArgs e)
        {
            //this.SearchProcess(this.btnSearchTransferRequest);
        }

        private void btnHide_Click(object sender, RoutedEventArgs e)
        {
            this.HidePopUp();
        }

        private void popupBaseInfo_LostFocus(object sender, RoutedEventArgs e)
        {
            this.popupBaseInfo.StaysOpen = true;
        }

        private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.HidePopUp();
        }

        private void chkAll_Checked(object sender, RoutedEventArgs e)
        {
            this.SetGridRowChecked(true);
        }

        private void chkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            this.SetGridRowChecked(false);
        }

        private void cboMultiTransferRequestStatusCodePopUp_SelectionChanged(object sender, EventArgs e)
        {
            this.SearchTransferRequestDetail();
        }

        private void btnTransferComplete_Click(object sender, RoutedEventArgs e)
        {
            this.SetTransferComplete();
        }

        private void chkDetail_Checked(object sender, RoutedEventArgs e)
        {
            if (this.dgTransferRequest.ItemsSource == null)
            {
                return;
            }
            this.SearchTransferRequest();
        }

        private void chkDetail_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this.dgTransferRequest.ItemsSource == null)
            {
                return;
            }
            this.SearchTransferRequest();
        }

        private void txtLOTID_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key.Equals(Key.V) && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                this.SearchClipboardData();
                e.Handled = true;
            }
        }
        #endregion
    }

    internal class PACK003_025_DataHelper
    {
        #region Member Variable Lists...
        private DataTable dtAssyLineInfo = new DataTable();                 // 조립라인
        private DataTable dtElecLineInfo = new DataTable();                 // 전극라인
        private DataTable dtPackLineInfo = new DataTable();                 // Pack MEB 라인
        private DataTable dtProductInfo = new DataTable();                  // MEB 제품
        private DataTable dtPackEquipmentInfo = new DataTable();            // 포장기 설비정보
        private DataTable dtTransferRequestStatusCode = new DataTable();    // 반송상태코드정보
        private DataTable dtPackMixTypeCodeInfo = new DataTable();          // 포장혼입유형코드정보
        private DataTable dtRequestWipTypeCodeInfo = new DataTable();       // 반송재공상태코드정보
        #endregion

        #region Constructor
        public PACK003_025_DataHelper()
        {
            this.GetBasicInfo(ref this.dtAssyLineInfo, "A");
            this.GetBasicInfo(ref this.dtElecLineInfo, "E");
            this.GetPackLineInfo(ref this.dtPackLineInfo);
            this.GetProductInfo(ref this.dtProductInfo);
            this.GetCommonCodeInfo(ref this.dtTransferRequestStatusCode, "PACK_LOGIS_TRF_REQ_STAT_CODE");
            this.GetCommonCodeInfo(ref this.dtPackMixTypeCodeInfo, "PACK_MIX_TYPE_CODE");
            this.GetCommonCodeInfo(ref this.dtRequestWipTypeCodeInfo, "REQ_WIP_TYPE_CODE");
        }
        #endregion

        #region Member Function Lists...
        // 순서도 호출 - CommonCode
        internal void GetCommonCodeInfo(ref DataTable dtReturn, string cmcdType)
        {
            try
            {
                string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTES";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CMCDTYPE"] = cmcdType;
                drRQSTDT["ATTRIBUTE1"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE2"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE3"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE4"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE5"] = DBNull.Value;
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
            }
        }

        // 순서도 호출 - 포장기 (반송구분이 수동인것만 가져오기)
        internal void GetPackEquipmentInfo(ref DataTable dtReturn, string autoTransferFlag = null)
        {
            try
            {
                string bizRuleName = "DA_BAS_SEL_LOGIS_PACK_EQPT_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PACK_CELL_AUTO_LOGIS_FLAG", typeof(string));
                dtRQSTDT.Columns.Add("PACK_MEB_LINE_FLAG", typeof(string));
                dtRQSTDT.Columns.Add("PACK_BOX_LINE_FLAG", typeof(string));
                dtRQSTDT.Columns.Add("AUTO_TRF_FLAG", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["PACK_CELL_AUTO_LOGIS_FLAG"] = DBNull.Value;
                drRQSTDT["PACK_MEB_LINE_FLAG"] = DBNull.Value;
                drRQSTDT["PACK_BOX_LINE_FLAG"] = "Y";
                drRQSTDT["AUTO_TRF_FLAG"] = autoTransferFlag;

                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    this.dtPackEquipmentInfo = dtRSLTDT.Copy();
                }
                else
                {
                    this.dtPackEquipmentInfo.Clear();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 순서도 호출 - 조립동 및 전극동 Shop, Line, Equipment
        internal void GetBasicInfo(ref DataTable dtReturn, string areaTypeCode)
        {
            try
            {
                string bizRuleName = "DA_PRD_SEL_LOGIS_BASICINFO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["SHOPID"] = null;
                drRQSTDT["AREAID"] = null;
                drRQSTDT["AREA_TYPE_CODE"] = areaTypeCode;
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
            }
        }

        // 순서도 호출 - Pack Line
        internal void GetPackLineInfo(ref DataTable dtReturn)
        {
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
                    dtPackLineInfo = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 순서도 호출 - 제품
        internal void GetProductInfo(ref DataTable dtReturn)
        {
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
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 순서도 호출 - 반송 요청 목록 조회
        internal DataTable GetModuleTransferRequestList(string productID, string prodAssyAreaList, string prodAssyLineList, string prodEltrLineList, string prodPackLineList, bool? detailFlag)
        {
            string bizRuleName = "DA_PRD_SEL_LOGIS_TRF_REQ_LIST";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("AREAID", typeof(string));
            dtRQSTDT.Columns.Add("PRODID", typeof(string));
            dtRQSTDT.Columns.Add("PROD_ASSY_AREA_LIST", typeof(string));
            dtRQSTDT.Columns.Add("PROD_ASSY_LINE_LIST", typeof(string));
            dtRQSTDT.Columns.Add("PROD_ELTR_LINE_LIST", typeof(string));
            dtRQSTDT.Columns.Add("PROD_PACK_LINE_LIST", typeof(string));
            dtRQSTDT.Columns.Add("DETAIL_FLAG", typeof(bool));

            DataRow drRQSTDT = dtRQSTDT.NewRow();
            drRQSTDT["LANGID"] = LoginInfo.LANGID;
            drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
            drRQSTDT["PRODID"] = string.IsNullOrEmpty(productID) ? null : productID;
            drRQSTDT["PROD_ASSY_AREA_LIST"] = string.IsNullOrEmpty(prodAssyAreaList) ? null : prodAssyAreaList;
            drRQSTDT["PROD_ASSY_LINE_LIST"] = string.IsNullOrEmpty(prodAssyLineList) ? null : prodAssyLineList;
            drRQSTDT["PROD_ELTR_LINE_LIST"] = string.IsNullOrEmpty(prodEltrLineList) ? null : prodEltrLineList;
            drRQSTDT["PROD_PACK_LINE_LIST"] = string.IsNullOrEmpty(prodPackLineList) ? null : prodPackLineList;
            drRQSTDT["DETAIL_FLAG"] = detailFlag;
            dtRQSTDT.Rows.Add(drRQSTDT);

            return new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
        }

        // 순서도 호출 - 반송 요청 이력 조회
        internal DataTable GetModuleTransferHistoryList(object productID, DateTime fromRequestDate, DateTime toRequestDate, object transferRequestStatusCode, string transferRequestNo)
        {
            string bizRuleName = "DA_PRD_SEL_LOGIS_TRF_REQ_HISTORY";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("FROM_REQ_DATE", typeof(DateTime));
            dtRQSTDT.Columns.Add("TO_REQ_DATE", typeof(DateTime));
            dtRQSTDT.Columns.Add("PRODID", typeof(string));
            dtRQSTDT.Columns.Add("TRF_REQ_STAT_CODE", typeof(string));
            dtRQSTDT.Columns.Add("TRF_REQ_NO", typeof(string));
            dtRQSTDT.Columns.Add("TRF_REQ_GNRT_TYPE_CODE", typeof(string));
            dtRQSTDT.Columns.Add("PACK_EQPTID", typeof(string));

            DataRow drRQSTDT = dtRQSTDT.NewRow();
            drRQSTDT["LANGID"] = LoginInfo.LANGID;
            drRQSTDT["FROM_REQ_DATE"] = fromRequestDate;
            drRQSTDT["TO_REQ_DATE"] = toRequestDate;
            drRQSTDT["PRODID"] = string.IsNullOrEmpty(Util.NVC(productID)) ? null : Util.NVC(productID);
            drRQSTDT["TRF_REQ_STAT_CODE"] = transferRequestStatusCode;
            drRQSTDT["TRF_REQ_NO"] = transferRequestNo;
            dtRQSTDT.Rows.Add(drRQSTDT);

            return new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
        }

        // 순서도 호출 - 반송 요청 이력 조회 (LOT별)
        internal DataTable GetModuleTransferHistoryListByLOT(string productID, DateTime? dteFromDate, DateTime? dteToDate, string transferCSTStatusCode, string LOTID, string packEquipmentID, string processID, string routID)
        {
            string bizRuleName = "DA_PRD_SEL_LOGIS_TRF_REQ_HISTORY_BY_LOT";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("AREAID", typeof(string));
            dtRQSTDT.Columns.Add("FROM_REQ_DATE", typeof(DateTime));
            dtRQSTDT.Columns.Add("TO_REQ_DATE", typeof(DateTime));
            dtRQSTDT.Columns.Add("PRODID", typeof(string));
            dtRQSTDT.Columns.Add("TRF_CST_STAT_CODE", typeof(string));
            dtRQSTDT.Columns.Add("LOTID", typeof(string));
            dtRQSTDT.Columns.Add("PACK_EQPTID", typeof(string));
            dtRQSTDT.Columns.Add("PROCID", typeof(string));
            dtRQSTDT.Columns.Add("ROUTID", typeof(string));

            DataRow drRQSTDT = dtRQSTDT.NewRow();
            drRQSTDT["LANGID"] = LoginInfo.LANGID;
            drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
            drRQSTDT["FROM_REQ_DATE"] = dteFromDate;
            drRQSTDT["TO_REQ_DATE"] = dteToDate;
            drRQSTDT["PRODID"] = string.IsNullOrEmpty(Util.NVC(productID)) ? null : Util.NVC(productID);
            drRQSTDT["TRF_CST_STAT_CODE"] = string.IsNullOrEmpty(transferCSTStatusCode) ? null : transferCSTStatusCode;
            drRQSTDT["LOTID"] = string.IsNullOrEmpty(LOTID) ? null : LOTID;
            drRQSTDT["PACK_EQPTID"] = string.IsNullOrEmpty(packEquipmentID) ? null : packEquipmentID;
            drRQSTDT["PROCID"] = string.IsNullOrEmpty(processID) ? null : processID;
            drRQSTDT["ROUTID"] = string.IsNullOrEmpty(routID) ? null : routID;
            dtRQSTDT.Rows.Add(drRQSTDT);

            return new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
        }

        // 순서도 호출 - 반송 요청 상세 이력 조회
        internal DataTable GetModuleTransferDetailList(object trfReqNo, object objTrfReqStatusCode)
        {
            string bizRuleName = "DA_PRD_SEL_LOGIS_REQ_DATA_DETL";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("TRF_REQ_NO", typeof(string));
            dtRQSTDT.Columns.Add("TRF_CST_STAT_CODE", typeof(string));

            DataRow drRQSTDT = dtRQSTDT.NewRow();
            drRQSTDT["LANGID"] = LoginInfo.LANGID;
            drRQSTDT["TRF_REQ_NO"] = trfReqNo;
            drRQSTDT["TRF_CST_STAT_CODE"] = string.IsNullOrEmpty(objTrfReqStatusCode.ToString()) ? null : objTrfReqStatusCode.ToString();
            dtRQSTDT.Rows.Add(drRQSTDT);

            return new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
        }

        // 순서도 호출 - 반송 종료 처리
        internal bool SaveModuleTransferComplete(DataTable dt, string updUser, string remark)
        {
            bool returnValue = true;
            string bizRuleName = "BR_PRD_REG_LOGIS_MANUAL_REQ";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();

            try
            {
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("SHOPID", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("INSUSER", typeof(string));
                dtINDATA.Columns.Add("REQ_TYPE", typeof(string));
                dtINDATA.Columns.Add("NOTE", typeof(string));
                dtINDATA.Columns.Add("UPDUSER", typeof(string));
                dtINDATA.Columns.Add("TRF_REQ_TYPE_CODE", typeof(string));

                DataTable dtINREQ = new DataTable("IN_REQ");
                dtINREQ.Columns.Add("EQSGID", typeof(string));
                dtINREQ.Columns.Add("MTRLID", typeof(string));
                dtINREQ.Columns.Add("AREA_ASSY", typeof(string));
                dtINREQ.Columns.Add("PKG_EQPT", typeof(string));
                dtINREQ.Columns.Add("COT_EQPT", typeof(string));
                dtINREQ.Columns.Add("ASSY_STOCK_QTY", typeof(string));
                dtINREQ.Columns.Add("REQ_POSSIBLITY_QTY", typeof(string));
                dtINREQ.Columns.Add("WOID", typeof(string));
                dtINREQ.Columns.Add("INPUT_MIX_CHK_MTHD_CODE", typeof(string));

                DataTable dtINUPD = new DataTable("IN_UPD");
                dtINUPD.Columns.Add("TRF_REQ_NO", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["INSUSER"] = updUser;
                drINDATA["REQ_TYPE"] = "C";
                drINDATA["NOTE"] = remark;
                drINDATA["UPDUSER"] = updUser;
                drINDATA["TRF_REQ_TYPE_CODE"] = "REQ_MODULE_MOVE";
                dtINDATA.Rows.Add(drINDATA);

                foreach (DataRowView drv in dt.AsDataView())
                {
                    DataRow drINUPD = dtINUPD.NewRow();
                    drINUPD["TRF_REQ_NO"] = drv["TRF_REQ_NO"].ToString();
                    dtINUPD.Rows.Add(drINUPD);
                }

                dsINDATA.Tables.Add(dtINDATA);
                dsINDATA.Tables.Add(dtINREQ);
                dsINDATA.Tables.Add(dtINUPD);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, "OUTDATA", dsINDATA);
                if (dsOUTDATA != null)
                {
                    Util.MessageInfo("SFU8357");        // 반송 요청이 저장되었습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnValue = false;
                return returnValue;
            }

            return returnValue;
        }

        // 순서도 호출 - Pack 공정 정보
        internal DataTable GetPackProcessInfo()
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
        internal DataTable GetRouteInfo()
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

        // 포장기
        internal DataTable GetPackEquipmentInfo(string autoTransferFlag = null)
        {
            this.GetPackEquipmentInfo(ref this.dtPackEquipmentInfo, autoTransferFlag);
            if (!CommonVerify.HasTableRow(this.dtPackEquipmentInfo))
            {
                return null;
            }

            var query = this.dtPackEquipmentInfo.AsEnumerable().Select(x => new
            {
                PACK_EQPTID = x.Field<string>("CBO_CODE"),
                PACK_EQPTNAME = x.Field<string>("CBO_NAME")
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // 조립동
        internal DataTable GetAssyAreaInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtAssyLineInfo))
            {
                return null;
            }

            var query = this.dtAssyLineInfo.AsEnumerable().GroupBy(x => new
            {
                AREAID = x.Field<string>("AREAID")
            }).Select(grp => new
            {
                CHK = false,
                AREAID = grp.Key.AREAID,
                AREANAME = grp.Key.AREAID + " : " + grp.Max(x => x.Field<string>("AREANAME"))
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // 조립설비
        internal DataTable GetAssyEquipmentInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtAssyLineInfo))
            {
                return null;
            }

            var query = this.dtAssyLineInfo.AsEnumerable().GroupBy(x => new
            {
                EQPTID = x.Field<string>("EQPTID")
            }).Select(grp => new
            {
                CHK = false,
                EQPTID = grp.Key.EQPTID,
                EQPTNAME = grp.Key.EQPTID + " : " + grp.Max(x => x.Field<string>("EQPTNAME"))
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // 전극설비
        internal DataTable GetElecEquipmentInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtElecLineInfo))
            {
                return null;
            }

            var query = this.dtElecLineInfo.AsEnumerable().GroupBy(x => new
            {
                EQPTID = x.Field<string>("EQPTID")
            }).Select(grp => new
            {
                CHK = false,
                EQPTID = grp.Key.EQPTID,
                EQPTNAME = grp.Key.EQPTID + " : " + grp.Max(x => x.Field<string>("EQPTNAME"))
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // Pack Line
        internal DataTable GetPackLineInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtPackLineInfo))
            {
                return null;
            }

            var query = this.dtPackLineInfo.AsEnumerable().Select(x => new
            {
                CHK = false,
                EQSGID = x.Field<string>("CBO_CODE"),
                EQSGNAME = x.Field<string>("CBO_NAME")
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // 제품
        internal DataTable GetProductInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtProductInfo))
            {
                return null;
            }

            var query = this.dtProductInfo.AsEnumerable().Select(x => new
            {
                CHK = false,
                PRODID = x.Field<string>("PRODID"),
                PRODNAME = x.Field<string>("PRODID") + " : " + x.Field<string>("PRODNAME")
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // 반송상태코드
        internal DataTable GetTrasnferRequestStatusCodeInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtTransferRequestStatusCode))
            {
                return null;
            }

            var query = this.dtTransferRequestStatusCode.AsEnumerable().Select(x => new
            {
                CHK = false,
                PACK_LOGIS_TRF_REQ_STAT_CODE = x.Field<string>("CBO_CODE"),
                PACK_LOGIS_TRF_REQ_STAT_CODE_NAME = x.Field<string>("CBO_NAME")
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // 포장혼입유형코드
        internal DataTable GetPackMixTypeCodeInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtPackMixTypeCodeInfo))
            {
                return null;
            }

            var query = this.dtPackMixTypeCodeInfo.AsEnumerable().Select(x => new
            {
                CHK = false,
                PACK_MIX_TYPE_CODE = x.Field<string>("CBO_CODE"),
                PACK_MIX_TYPE_CODE_NAME = x.Field<string>("CBO_NAME")
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // 포장혼입유형코드
        internal DataTable GetPackMixTypeCodeInfo(List<string> lstSelect, bool isGridMapping = true)
        {
            DataTable dt = this.GetPackMixTypeCodeInfo();
            DataTable dtReturn = new DataTable();

            if (isGridMapping)
            {
                var query = (from d1 in dt.AsEnumerable()
                             join d2 in lstSelect on d1.Field<string>("PACK_MIX_TYPE_CODE") equals d2 into outerJoin
                             from d3 in outerJoin.DefaultIfEmpty()
                             select new
                             {
                                 BASEDATA = d1,
                                 SELECTEDDATA = d3
                             }).Select(x => new
                             {
                                 CHK = x.SELECTEDDATA == null ? false : true,
                                 PACK_MIX_TYPE_CODE = x.BASEDATA.Field<string>("PACK_MIX_TYPE_CODE"),
                                 PACK_MIX_TYPE_CODE_NAME = x.BASEDATA.Field<string>("PACK_MIX_TYPE_CODE_NAME")
                             });

                dtReturn = PackCommon.queryToDataTable(query.ToList());
            }
            else
            {
                dtReturn = dt.AsEnumerable().Where(x => lstSelect.Where(y => y.Equals(x.Field<string>("PACK_MIX_TYPE_CODE"))).Any()).CopyToDataTable();
            }

            return dtReturn;
        }

        // 반송재공유형코드
        internal DataTable GetRequestWipTypeCodeInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtRequestWipTypeCodeInfo))
            {
                return null;
            }

            var query = this.dtRequestWipTypeCodeInfo.AsEnumerable().Select(x => new
            {
                CHK = false,
                REQ_WIP_TYPE_CODE = x.Field<string>("CBO_CODE"),
                REQ_WIP_TYPE_CODE_NAME = x.Field<string>("CBO_NAME")
            });

            DataTable dt = PackCommon.queryToDataTable(query.ToList());
            DataRow dr = dt.NewRow();
            dr["CHK"] = false;
            dr["REQ_WIP_TYPE_CODE"] = string.Empty;
            dr["REQ_WIP_TYPE_CODE_NAME"] = ObjectDic.Instance.GetObjectName("ALL");
            dt.Rows.InsertAt(dr, 0);
            dt.AcceptChanges();

            return dt;
        }

        // 반송재공유형코드
        internal DataTable GetRequestWipTypeCodeInfo(List<string> lstSelect, bool isGridMapping = true)
        {
            DataTable dt = this.GetRequestWipTypeCodeInfo();
            DataTable dtReturn = new DataTable();

            if (isGridMapping)
            {
                var query = (from d1 in dt.AsEnumerable()
                             join d2 in lstSelect on d1.Field<string>("REQ_WIP_TYPE_CODE") equals d2 into outerJoin
                             from d3 in outerJoin.DefaultIfEmpty()
                             select new
                             {
                                 BASEDATA = d1,
                                 SELECTEDDATA = d3
                             }).Select(x => new
                             {
                                 CHK = x.SELECTEDDATA == null ? false : true,
                                 REQ_WIP_TYPE_CODE = x.BASEDATA.Field<string>("REQ_WIP_TYPE_CODE"),
                                 REQ_WIP_TYPE_CODE_NAME = x.BASEDATA.Field<string>("REQ_WIP_TYPE_CODE_NAME")
                             });

                dtReturn = PackCommon.queryToDataTable(query.ToList());
            }
            else
            {
                dtReturn = dt.AsEnumerable().Where(x => lstSelect.Where(y => y.Equals(x.Field<string>("REQ_WIP_TYPE_CODE"))).Any()).CopyToDataTable();
            }

            return dtReturn;
        }
        #endregion
    }
}