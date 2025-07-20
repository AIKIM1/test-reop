/*************************************************************************************
 Created Date : 2024.04.05
      Creator : 정용석
  Description : 자재 투입 취소
--------------------------------------------------------------------------------------
 [Change History]
  2024.04.05 정용석 : [CSRID : E20240312-000381] Initial Created.
  2024.07.01 정용석 : [CSRID : E20240628-001501] 자재투입취소 전송중 인 경우 인터락 추가 건
  2025.04.25 유현민 : MI1_OSS_0123 CHK 컬럼 첫번째 Row 자동 true 해제
**************************************************************************************/
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_102 : UserControl, IWorkArea
    {
        #region Member Variable Lists...
        private const string MATERIAL_INPUT_SEARCH = "MATERIAL_INPUT_SEARCH";
        private const string MATERIAL_INPUT_CANCEL_HISTORY = "MATERIAL_INPUT_CANCEL_HISTORY";
        private PACK001_102_DataHelper dataHelper = new PACK001_102_DataHelper();
        #endregion

        #region Property
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        public PACK001_102()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function Lists...
        // 공장날짜 가져오기
        private DateTime GetCaldate()
        {
            // 공장날짜 가져오기
            DateTime dtpCaldate = new DateTime();
            DataTable dt = this.dataHelper.GetCaldate(LoginInfo.CFG_AREA_ID);
            if (CommonVerify.HasTableRow(dt))
            {
                foreach (DataRowView dataRowView in dt.AsDataView())
                {
                    dtpCaldate = Convert.ToDateTime(Util.NVC(dataRowView["CALDATE"]));
                }
            }
            else
            {
                dtpCaldate = DateTime.Now.AddDays(1 - DateTime.Now.Day);
            }

            return dtpCaldate;
        }

        // Initialize
        private void Initialize()
        {
            this.dtpPostYYYYMM.ApplyTemplate();
            this.dtpDateFrom.ApplyTemplate();
            this.dtpDateTo.ApplyTemplate();
            this.dtpPostYYYYMM.SelectedDateTime = this.GetCaldate();
            this.dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);
            this.dtpDateTo.SelectedDateTime = DateTime.Now;
            this.dtpPostYYYYMM.IsEnabled = false;

            // For Test
            //this.dtpPostYYYYMM.IsEnabled = true;
            //this.dtpPostYYYYMM.SelectedDateTime = new DateTime(2020, 11, 1);

            this.txtMaterialInputMaterialID.Tag = MATERIAL_INPUT_SEARCH;
            this.cboMaterialInputEquipmentSegment.Tag = MATERIAL_INPUT_SEARCH;
            this.btnMaterialInputSearch.Tag = MATERIAL_INPUT_SEARCH;
            this.txtMaterialInputCancelHistoryMaterialID.Tag = MATERIAL_INPUT_CANCEL_HISTORY;
            this.cboMaterialInputCancelHistoryEquipmentSegment.Tag = MATERIAL_INPUT_CANCEL_HISTORY;
            this.btnMaterialInputCancelHistorySearch.Tag = MATERIAL_INPUT_CANCEL_HISTORY;

            this.ComboBoxBindingInC1DataGridColumn();
            PackCommon.SetC1ComboBox(this.dataHelper.GetEquipmentSegmentInfo(LoginInfo.CFG_AREA_ID), this.cboMaterialInputEquipmentSegment, true, "ALL");
            PackCommon.SetC1ComboBox(this.dataHelper.GetEquipmentSegmentInfo(LoginInfo.CFG_AREA_ID), this.cboMaterialInputCancelHistoryEquipmentSegment, true, "ALL");
        }

        // Grid Column ComboBox Binding
        private void ComboBoxBindingInC1DataGridColumn()
        {
            C1.WPF.DataGrid.DataGridColumn dataGridColumn = null;

            dataGridColumn = this.dgMaterialInputCancel.Columns["RESN_DEPT_CODE"];
            (dataGridColumn as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = this.dataHelper.GetCommonCodeInfo("RESP_DEPT").AsDataView();
            dataGridColumn = this.dgMaterialInputCancel.Columns["WORKTYPE"];
            (dataGridColumn as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = this.dataHelper.GetActivityInfo().AsDataView();
        }

        // 조회
        private void SearchProcess(string btnCallerIdentifier)
        {
            switch (btnCallerIdentifier)
            {
                case MATERIAL_INPUT_SEARCH:
                    this.MaterialInputSearch();
                    break;
                case MATERIAL_INPUT_CANCEL_HISTORY:
                    this.MaterialInputCancelHistorySearch();
                    break;
                default:
                    break;
            }
        }

        // 조회 1호 : 자재 투입 이력 조회
        private void MaterialInputSearch()
        {
            string postyyyyMM = this.dtpPostYYYYMM.SelectedDateTime.Date.ToString("yyyyMM");
            string materialID = this.txtMaterialInputMaterialID.Text;
            string equipmentSegmentID = this.cboMaterialInputEquipmentSegment.SelectedValue.ToString();
            string waitCheckFlag = "N";
            try
            {
                Util.gridClear(this.dgMaterialInput);
                Util.gridClear(this.dgMaterialInputCancel);
                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.DoEvents();
                DataTable dt = this.dataHelper.GetMaterialInputSummary(postyyyyMM, string.Empty, materialID, equipmentSegmentID, waitCheckFlag);

                if (CommonVerify.HasTableRow(dt))
                {
                    Util.GridSetData(this.dgMaterialInput, dt, FrameOperation, true);
                }
                else
                {
                    Util.MessageInfo("SFU3536");        // 조회된 결과가 없습니다
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

        // 조회 2호 : 자재 투입 취소 이력 조회
        private void MaterialInputCancelHistorySearch()
        {
            string fromDate = this.dtpDateFrom.SelectedDateTime.Date.ToString("yyyyMMdd");
            string toDate = this.dtpDateTo.SelectedDateTime.Date.ToString("yyyyMMdd");
            string materialID = this.txtMaterialInputCancelHistoryMaterialID.Text;
            string equipmentSegmentID = this.cboMaterialInputCancelHistoryEquipmentSegment.SelectedValue.ToString();

            try
            {
                Util.gridClear(this.dgMaterialInputCancelHistory);
                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.SearchRowCount(ref this.txtMaterialInputCancelRequestHistoryCount, 0);
                PackCommon.DoEvents();
                DataTable dt = this.dataHelper.GetMaterialCancelList(fromDate, toDate, materialID, equipmentSegmentID);

                if (!CommonVerify.HasTableRow(dt))
                {
                    Util.MessageInfo("SFU3536");        // 조회된 결과가 없습니다
                }
                else
                {
                    PackCommon.SearchRowCount(ref this.txtMaterialInputCancelRequestHistoryCount, dt.Rows.Count);
                    Util.GridSetData(this.dgMaterialInputCancelHistory, dt, FrameOperation, true);
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

        // Transaction 1호 : 자재 투입 취소
        private void MaterialInputCancelRequestTransaction()
        {
            bool validationCheckFlag = true;

            // Validation Check...
            if (this.dgMaterialInputCancel.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1651");      // 선택된 항목이 없습니다.
                return;
            }

            if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
            {
                Util.MessageValidation("SFU4591");      // 작업자를 입력하세요
                this.ucPersonInfo.Focus();
                return;
            }

            // 취소수량, 귀책부서, WorkType 체크
            var queryValidationCancelQty = DataTableConverter.Convert(this.dgMaterialInputCancel.ItemsSource).AsEnumerable();
            foreach (var item in queryValidationCancelQty)
            {
                if (Convert.ToInt32(item.Field<int>("CANCEL_QTY")) <= 0)
                {
                    Util.MessageValidation("SFU8930");      // 취소요청수량을 입력하세요.
                    validationCheckFlag = false;
                    break;
                }

                // 자재투입취소 Transaction시에 취소수량 Validation
                bool returnValue = false;
                string workorderID = item.Field<string>("WOID");
                string materialID = item.Field<string>("MTRLID");
                int inputQty = 0;
                int requestQty = 0;
                int cancelRequestQty = 0;
                int cancelAvailableQty = 0;
                string waitCheckFlag = "Y";

                DataTable dt = this.dataHelper.GetMaterialInputSummary(item.Field<string>("POST_YM"), item.Field<string>("WOID"), item.Field<string>("MTRLID"), string.Empty, waitCheckFlag);
                if (!CommonVerify.HasTableRow(dt))
                {
                    returnValue = true;
                }
                else
                {
                    foreach (DataRowView dataRowView in dt.AsDataView())
                    {
                        inputQty = Convert.ToInt32(dataRowView["INPUT_QTY"].ToString());
                        requestQty = Convert.ToInt32(dataRowView["REQ_QTY"].ToString());
                        cancelRequestQty = Convert.ToInt32(dataRowView["CANCEL_REQ_QTY"].ToString());
                        cancelAvailableQty = inputQty - requestQty + cancelRequestQty;
                        returnValue = (Convert.ToInt32(item.Field<int>("CANCEL_QTY")) > cancelAvailableQty) ? false : true;
                    }
                }

                // Data Update
                if (!returnValue)
                {
                    this.DataUpdate(item.Field<string>("WOID"), item.Field<string>("MTRLID"), this.dgMaterialInput, dt);
                    this.DataUpdate(item.Field<string>("WOID"), item.Field<string>("MTRLID"), this.dgMaterialInputCancel, dt);
                }

                if (!returnValue)
                {
                    Util.MessageValidation("SFU8929", workorderID, materialID, inputQty, requestQty, cancelRequestQty, cancelAvailableQty);      // 취소요청수량이 투입수량보다 큽니다.
                    validationCheckFlag = false;
                    break;
                }

                // 자재투입취소요청 또는 자재투입취소의취소요청시 요청시간이 20분 안에 요청된 건이 있다면 Interlock
                foreach (DataRowView dataRowView in dt.AsDataView())
                {
                    if (dataRowView["WAIT_FLAG"].ToString() == "Y")
                    {
                        returnValue = false;
                        break;
                    }
                }

                if (!returnValue)
                {
                    Util.MessageValidation("SFU8937", workorderID, materialID);      // WO : [%1] MaterialID : [%2] 은 ERP 전송 중 입니다. 잠시 후 다시 시도하세요.
                    validationCheckFlag = false;
                    break;
                }

                if (string.IsNullOrEmpty(item.Field<string>("RESN_DEPT_CODE")))
                {
                    Util.MessageValidation("SFU3296");      // 선택오류 : 귀책부서(필수조건) 콤보를 선택하지 않았습니다.[콤보선택]
                    validationCheckFlag = false;
                    break;
                }
                if (string.IsNullOrEmpty(item.Field<string>("WORKTYPE")))
                {
                    Util.MessageValidation("SFU8931");      // 선택오류 : 작업유형을 선택하지 않았습니다.[콤보선택]
                    validationCheckFlag = false;
                    break;
                }
            }

            if (!validationCheckFlag)
            {
                return;
            }

            try
            {
                bool transactionFlag = true;

                DataTable dt = DataTableConverter.Convert(this.dgMaterialInputCancel.ItemsSource);
                foreach (DataRowView drv in dt.AsDataView())
                {
                    // INDATA
                    DataTable dtINDATA = new DataTable("INDATA");
                    dtINDATA.Columns.Add("WOID", typeof(string));
                    dtINDATA.Columns.Add("SCRP_USERID", typeof(string));
                    dtINDATA.Columns.Add("NOTE", typeof(string));
                    dtINDATA.Columns.Add("POST_DATE", typeof(string));
                    dtINDATA.Columns.Add("AREAID", typeof(string));

                    DataRow drINDATA = dtINDATA.NewRow();
                    drINDATA["WOID"] = drv["WOID"].ToString();
                    drINDATA["SCRP_USERID"] = this.ucPersonInfo.UserID;
                    drINDATA["NOTE"] = DateTime.Now.ToString("yyyyMMdd") + "/" + this.ucPersonInfo.UserID + "/" + drv["WOID"].ToString();
                    drINDATA["POST_DATE"] = this.GetCaldate().ToString("yyyyMMdd");
                    drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dtINDATA.Rows.Add(drINDATA);

                    // IN_MTRL
                    DataTable dtIN_MTRL = new DataTable("IN_MTRL");
                    dtIN_MTRL.Columns.Add("MTRLID", typeof(string));
                    dtIN_MTRL.Columns.Add("SCRP_QTY", typeof(decimal));
                    dtIN_MTRL.Columns.Add("RESP_DEPT_CODE", typeof(string));
                    dtIN_MTRL.Columns.Add("SCRP_RSN_NOTE", typeof(string));
                    dtIN_MTRL.Columns.Add("COST_TYPE_CODE", typeof(string));
                    dtIN_MTRL.Columns.Add("ACTID", typeof(string));
                    dtIN_MTRL.Columns.Add("RESNCODE", typeof(string));

                    DataRow drIN_MTRL = dtIN_MTRL.NewRow();
                    drIN_MTRL["MTRLID"] = drv["MTRLID"].ToString();
                    drIN_MTRL["SCRP_QTY"] = drv["CANCEL_QTY"].ToString();
                    drIN_MTRL["RESP_DEPT_CODE"] = drv["RESN_DEPT_CODE"].ToString();
                    drIN_MTRL["SCRP_RSN_NOTE"] = DateTime.Now.ToString("yyyyMMdd") + "/" + this.ucPersonInfo.UserID + "/" + drv["WOID"].ToString();
                    drIN_MTRL["COST_TYPE_CODE"] = string.Empty;
                    drIN_MTRL["ACTID"] = drv["WORKTYPE"].ToString();
                    drIN_MTRL["RESNCODE"] = null;
                    dtIN_MTRL.Rows.Add(drIN_MTRL);

                    if (!this.dataHelper.SetMaterialInputCancel(dtINDATA, dtIN_MTRL))
                    {
                        transactionFlag = false;
                        return;
                    }
                }

                if (transactionFlag)
                {
                    Util.MessageInfo("SFU1880");        // 전송 완료 되었습니다.
                    Util.gridClear(this.dgMaterialInputCancel);
                    this.MaterialInputSearch();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Transaction 2호 : 자재 투입 취소요청한거 취소
        private void MaterialInputCancelRequestCancelTransaction()
        {
            var query = DataTableConverter.Convert(this.dgMaterialInputCancelHistory.ItemsSource).AsEnumerable().Where(x => x.Field<bool>("CHK"));
            if (query.Count() <= 0)
            {
                Util.MessageValidation("SFU1651");      // 선택된 항목이 없습니다.
                return;
            }

            try
            {
                bool transactionFlag = true;

                Util.MessageConfirm("SFU1243", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        // ERP 전송번호 추출
                        List<string> lstERPTransferSequenceNo = query.Where(x => x.Field<bool>("CHK") && x.Field<string>("CNCL_FLAG") == "N").Select(x => x.Field<string>("ERP_TRNF_SEQNO")).ToList();
                        if (lstERPTransferSequenceNo.Count <= 0)
                        {
                            Util.MessageValidation("SFU1651");      // 선택된 항목이 없습니다.
                            return;
                        }

                        transactionFlag = this.dataHelper.SetMaterialInputCancelRequestCancel(lstERPTransferSequenceNo);

                        if (transactionFlag)
                        {
                            Util.MessageInfo("SFU1937");        // 전송 완료 되었습니다.
                            Util.gridClear(this.dgMaterialInputCancelHistory);
                            this.MaterialInputCancelHistorySearch();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 자재투입 Grid에서 자재투입취소 Grid로 선택한거 Copy
        private void DataCopy(CheckBox checkBox, int selectedIndex)
        {
            int cancelAvailableQty = 0;
            try
            {
                if (this.dgMaterialInputCancel.GetRowCount() <= 0)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("POST_YM", typeof(string));
                    dt.Columns.Add("MTRLID", typeof(string));
                    dt.Columns.Add("EQSGID", typeof(string));
                    dt.Columns.Add("EQSGNAME", typeof(string));
                    dt.Columns.Add("WOID", typeof(string));
                    dt.Columns.Add("PRODID", typeof(string));
                    dt.Columns.Add("PROD_CLSS_CODE", typeof(string));
                    dt.Columns.Add("FRST_POST_DATE", typeof(string));
                    dt.Columns.Add("FINL_POST_DATE", typeof(string));
                    dt.Columns.Add("INPUT_QTY", typeof(int));
                    dt.Columns.Add("REQ_QTY", typeof(int));
                    dt.Columns.Add("CANCEL_REQ_QTY", typeof(int));
                    dt.Columns.Add("CANCEL_QTY", typeof(int));
                    dt.Columns.Add("RESN_DEPT_CODE", typeof(string));
                    dt.Columns.Add("WORKTYPE", typeof(string));


                    DataRow dr = dt.NewRow();
                    dr["POST_YM"] = DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "POST_YM")?.ToString();
                    dr["MTRLID"] = DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "MTRLID")?.ToString();
                    dr["EQSGID"] = DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "EQSGID")?.ToString();
                    dr["EQSGNAME"] = DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "EQSGNAME")?.ToString();
                    dr["WOID"] = DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "WOID")?.ToString();
                    dr["PRODID"] = DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "PRODID")?.ToString();
                    dr["PROD_CLSS_CODE"] = DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "PROD_CLSS_CODE")?.ToString();
                    dr["FRST_POST_DATE"] = DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "FRST_POST_DATE")?.ToString();
                    dr["FINL_POST_DATE"] = DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "FINL_POST_DATE")?.ToString();
                    dr["INPUT_QTY"] = DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "INPUT_QTY")?.ToString();
                    dr["REQ_QTY"] = DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "REQ_QTY")?.ToString();
                    dr["CANCEL_REQ_QTY"] = DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "CANCEL_REQ_QTY")?.ToString();
                    cancelAvailableQty = Convert.ToInt32(DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "INPUT_QTY")?.ToString())
                                         - Convert.ToInt32(DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "REQ_QTY")?.ToString())
                                         + Convert.ToInt32(DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "CANCEL_REQ_QTY")?.ToString());
                    dr["CANCEL_QTY"] = cancelAvailableQty.ToString();
                    dt.Rows.Add(dr);
                    Util.GridSetData(this.dgMaterialInputCancel, dt, FrameOperation);
                    return;
                }

                this.dgMaterialInputCancel.IsReadOnly = false;
                this.dgMaterialInputCancel.CanUserAddRows = true;
                this.dgMaterialInputCancel.BeginNewRow();
                this.dgMaterialInputCancel.EndNewRow(true);

                DataTableConverter.SetValue(this.dgMaterialInputCancel.CurrentRow.DataItem, "POST_YM", DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "POST_YM")?.ToString());
                DataTableConverter.SetValue(this.dgMaterialInputCancel.CurrentRow.DataItem, "MTRLID", DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "MTRLID")?.ToString());
                DataTableConverter.SetValue(this.dgMaterialInputCancel.CurrentRow.DataItem, "EQSGID", DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "EQSGID")?.ToString());
                DataTableConverter.SetValue(this.dgMaterialInputCancel.CurrentRow.DataItem, "EQSGNAME", DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "EQSGNAME")?.ToString());
                DataTableConverter.SetValue(this.dgMaterialInputCancel.CurrentRow.DataItem, "WOID", DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "WOID")?.ToString());
                DataTableConverter.SetValue(this.dgMaterialInputCancel.CurrentRow.DataItem, "PRODID", DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "PRODID")?.ToString());
                DataTableConverter.SetValue(this.dgMaterialInputCancel.CurrentRow.DataItem, "PROD_CLSS_CODE", DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "PROD_CLSS_CODE")?.ToString());
                DataTableConverter.SetValue(this.dgMaterialInputCancel.CurrentRow.DataItem, "FRST_POST_DATE", DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "FRST_POST_DATE")?.ToString());
                DataTableConverter.SetValue(this.dgMaterialInputCancel.CurrentRow.DataItem, "FINL_POST_DATE", DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "FINL_POST_DATE")?.ToString());
                DataTableConverter.SetValue(this.dgMaterialInputCancel.CurrentRow.DataItem, "INPUT_QTY", DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "INPUT_QTY")?.ToString());
                DataTableConverter.SetValue(this.dgMaterialInputCancel.CurrentRow.DataItem, "REQ_QTY", DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "REQ_QTY")?.ToString());
                DataTableConverter.SetValue(this.dgMaterialInputCancel.CurrentRow.DataItem, "CANCEL_REQ_QTY", DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "CANCEL_REQ_QTY")?.ToString());
                cancelAvailableQty = Convert.ToInt32(DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "INPUT_QTY")?.ToString())
                                     - Convert.ToInt32(DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "REQ_QTY")?.ToString())
                                     + Convert.ToInt32(DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "CANCEL_REQ_QTY")?.ToString());
                DataTableConverter.SetValue(this.dgMaterialInputCancel.CurrentRow.DataItem, "CANCEL_QTY", cancelAvailableQty.ToString());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 자재투입 Grid에서 선택한거 풀면 자재투입취소 Grid에 복사된거 찾아서 지우기
        private void DataRemove(CheckBox checkBox, int selectedIndex)
        {
            try
            {
                string equipmentSegmentID = DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "EQSGID").ToString();
                string workorderID = DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "WOID").ToString();
                string materialID = DataTableConverter.GetValue(this.dgMaterialInput.Rows[selectedIndex].DataItem, "MTRLID").ToString();

                for (int i = this.dgMaterialInputCancel.GetRowCount() - 1; i >= 0; i--)
                {
                    if ((DataTableConverter.GetValue(this.dgMaterialInputCancel.Rows[i].DataItem, "EQSGID").ToString() == equipmentSegmentID) &&
                        (DataTableConverter.GetValue(this.dgMaterialInputCancel.Rows[i].DataItem, "WOID").ToString() == workorderID) &&
                        (DataTableConverter.GetValue(this.dgMaterialInputCancel.Rows[i].DataItem, "MTRLID").ToString() == materialID))
                    {
                        this.dgMaterialInputCancel.IsReadOnly = false;
                        this.dgMaterialInputCancel.CanUserRemoveRows = true;
                        this.dgMaterialInputCancel.RemoveRow(i);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 자재투입취소 Grid에서 삭제버튼 누르면 자재투입 Grid에 지운거 찾아서 체크표시 해제하기
        private void DataRemove(Button button, int selectedIndex)
        {
            try
            {
                string equipmentSegmentID = DataTableConverter.GetValue(this.dgMaterialInputCancel.Rows[selectedIndex].DataItem, "EQSGID").ToString();
                string workorderID = DataTableConverter.GetValue(this.dgMaterialInputCancel.Rows[selectedIndex].DataItem, "WOID").ToString();
                string materialID = DataTableConverter.GetValue(this.dgMaterialInputCancel.Rows[selectedIndex].DataItem, "MTRLID").ToString();

                for (int i = 0; i < this.dgMaterialInput.GetRowCount(); i++)
                {
                    if ((DataTableConverter.GetValue(this.dgMaterialInput.Rows[i].DataItem, "EQSGID").ToString() == equipmentSegmentID) &&
                        (DataTableConverter.GetValue(this.dgMaterialInput.Rows[i].DataItem, "WOID").ToString() == workorderID) &&
                        (DataTableConverter.GetValue(this.dgMaterialInput.Rows[i].DataItem, "MTRLID").ToString() == materialID))
                    {
                        DataTableConverter.SetValue(this.dgMaterialInput.Rows[i].DataItem, "CHK", false);
                    }
                }

                this.dgMaterialInputCancel.IsReadOnly = false;
                this.dgMaterialInputCancel.CanUserRemoveRows = true;
                this.dgMaterialInputCancel.RemoveRow(selectedIndex);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Data Update
        private void DataUpdate(string workorderID, string materialID, C1DataGrid c1DataGrid, DataTable dt)
        {
            if (!CommonVerify.HasTableRow(dt))
            {
                return;
            }

            int cancelAvailableQty = 0;

            foreach (DataRowView dataRowView in dt.AsDataView())
            {
                for (int i = 0; i < c1DataGrid.GetRowCount(); i++)
                {
                    if (workorderID.Equals(Util.NVC(c1DataGrid.GetCell(i, c1DataGrid.Columns["WOID"].Index).Value)) &&
                        materialID.Equals(Util.NVC(c1DataGrid.GetCell(i, c1DataGrid.Columns["MTRLID"].Index).Value)))
                    {
                        DataTableConverter.SetValue(c1DataGrid.Rows[i].DataItem, "INPUT_QTY", Convert.ToInt32(dataRowView["INPUT_QTY"].ToString()));
                        DataTableConverter.SetValue(c1DataGrid.Rows[i].DataItem, "REQ_QTY", Convert.ToInt32(dataRowView["REQ_QTY"].ToString()));
                        DataTableConverter.SetValue(c1DataGrid.Rows[i].DataItem, "CANCEL_REQ_QTY", Convert.ToInt32(dataRowView["CANCEL_REQ_QTY"].ToString()));
                        cancelAvailableQty = Convert.ToInt32(dataRowView["INPUT_QTY"].ToString())
                                             - Convert.ToInt32(dataRowView["REQ_QTY"].ToString())
                                             + Convert.ToInt32(dataRowView["CANCEL_REQ_QTY"].ToString());
                        DataGridColumnCollection dataGridColumnCollection = c1DataGrid.Columns;
                        foreach (C1.WPF.DataGrid.DataGridColumn dataGridColumn in dataGridColumnCollection)
                        {
                            if (dataGridColumn.Name == "CANCEL_QTY")
                            {
                                DataTableConverter.SetValue(c1DataGrid.Rows[i].DataItem, "CANCEL_QTY", cancelAvailableQty.ToString());
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> lstButton = new List<Button>();
            Util.pageAuth(lstButton, FrameOperation.AUTHORITY);

            this.Initialize();
            this.Loaded -= UserControl_Loaded;
        }

        private void txtMaterialID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrEmpty(textBox.Text))
            {
                Util.MessageValidation("10013", ObjectDic.Instance.GetObjectName("자재ID")); // %1(을)를 입력하여 주십시오.
                return;
            }

            switch (textBox.Tag.ToString())
            {
                case MATERIAL_INPUT_SEARCH:
                    this.MaterialInputSearch();
                    break;
                case MATERIAL_INPUT_CANCEL_HISTORY:
                    this.MaterialInputCancelHistorySearch();
                    break;
                default:
                    break;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            this.SearchProcess(button.Tag.ToString());
        }

        private void btnMaterialInputCancelRequest_Click(object sender, RoutedEventArgs e)
        {
            this.MaterialInputCancelRequestTransaction();
        }

        private void btnMaterialInputCancelRequestCancel_Click(object sender, RoutedEventArgs e)
        {
            this.MaterialInputCancelRequestCancelTransaction();
        }

        private void chkMaterialInput_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            if (DataTableConverter.GetValue(checkBox.DataContext, "CHK").Equals(true))
            {
                this.DataCopy(checkBox, ((DataGridCellPresenter)checkBox.Parent).Row.Index);
            }

            if (DataTableConverter.GetValue(checkBox.DataContext, "CHK").Equals(false))
            {
                this.DataRemove(checkBox, ((DataGridCellPresenter)checkBox.Parent).Row.Index);
            }
        }

        private void chkMaterialInputCancel_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            int selectedIndex = ((DataGridCellPresenter)checkBox.Parent).Row.Index;

            // CancelFlag = Y인지 체크
            if (Util.NVC(DataTableConverter.GetValue(this.dgMaterialInputCancelHistory.Rows[selectedIndex].DataItem, "CNCL_FLAG")).ToString() == "Y")
            {
                Util.MessageValidation("SFU4520");
                DataTableConverter.SetValue(this.dgMaterialInputCancelHistory.Rows[selectedIndex].DataItem, "CHK", false);
                return;
            }

            // 전기일이 금월인 것만 체크가능하도록 설정
            string selectedPostDate = Util.NVC(DataTableConverter.GetValue(this.dgMaterialInputCancelHistory.Rows[selectedIndex].DataItem, "POST_DATE")).ToString();
            DateTime date = DateTime.ParseExact(selectedPostDate, "yyyyMMdd", new System.Globalization.CultureInfo(LoginInfo.LANGID));
            if (date.ToString("yyyyMM") != DateTime.Now.ToString("yyyyMM"))
            {
                Util.MessageValidation("SFU9018");
                DataTableConverter.SetValue(this.dgMaterialInputCancelHistory.Rows[selectedIndex].DataItem, "CHK", false);
                return;
            }

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            this.DataRemove((Button)sender, ((DataGridCellPresenter)button.Parent).Row.Index);
        }

        private void dgMaterialInputCancel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                e.Handled = true;
            }
        }
        #endregion
    }

    internal class PACK001_102_DataHelper
    {
        #region Member Variable Lists...
        #endregion

        #region Constructor
        internal PACK001_102_DataHelper()
        {

        }
        #endregion

        #region Member Function Lists...
        // 순서도 호출 - CommonCode 정보
        internal DataTable GetCommonCodeInfo(string cmcdType)
        {
            DataTable dtReturn = new DataTable();
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

            return dtReturn;
        }

        // 순서도 호출 - EquipmentSegment 정보
        internal DataTable GetEquipmentSegmentInfo(string areaID)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EXCEPT_GROUP", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["EXCEPT_GROUP"] = null;
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

            return dtReturn;
        }

        // 순서도 호출 - 자재투입 이력 조회 (전기일 기준 금월 1달)
        internal DataTable GetMaterialInputSummary(string postyyyyMM, string workorderID, string materialID, string equipmentSegmentID, string waitCheckFlag)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_PRD_SEL_ERP_INPUT_RSLT_SUM_BY_WO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("POST_YM", typeof(string));
                dtRQSTDT.Columns.Add("WOID", typeof(string));
                dtRQSTDT.Columns.Add("MTRLID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("WAIT_CHECK_FLAG", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["POST_YM"] = string.IsNullOrEmpty(postyyyyMM) ? null : postyyyyMM;
                drRQSTDT["WOID"] = string.IsNullOrEmpty(workorderID) ? null : workorderID;
                drRQSTDT["MTRLID"] = string.IsNullOrEmpty(materialID) ? null : materialID;
                drRQSTDT["EQSGID"] = string.IsNullOrEmpty(equipmentSegmentID) ? null : equipmentSegmentID;
                drRQSTDT["WAIT_CHECK_FLAG"] = string.IsNullOrEmpty(waitCheckFlag) ? "N" : waitCheckFlag;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtRSLTDT.Rows[0]["CHK"] = false;  //20250425
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 자재투입취소요청 이력
        internal DataTable GetMaterialCancelList(string fromDate, string toDate, string materialID, string equipmentSegmentID)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_PRD_SEL_TRANMATERIAL_SEARCH";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("FROM_DATE", typeof(string));
                dtRQSTDT.Columns.Add("TO_DATE", typeof(string));
                dtRQSTDT.Columns.Add("MTRLID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["FROM_DATE"] = string.IsNullOrEmpty(fromDate) ? null : fromDate;
                drRQSTDT["TO_DATE"] = string.IsNullOrEmpty(toDate) ? null : toDate;
                drRQSTDT["MTRLID"] = string.IsNullOrEmpty(materialID) ? null : materialID;
                drRQSTDT["EQSGID"] = string.IsNullOrEmpty(equipmentSegmentID) ? null : equipmentSegmentID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtRSLTDT.Rows[0]["CHK"] = false; //20250425
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 자재투입취소 Transaction
        internal bool SetMaterialInputCancel(DataTable dtINDATA, DataTable dtIN_MTRL)
        {
            bool returnValue = false;
            string bizRuleName = "BR_PRD_REG_SCRAP_HIST";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            string outDataSetName = string.Empty;

            try
            {
                dsINDATA.Tables.Add(dtINDATA);
                dsINDATA.Tables.Add(dtIN_MTRL);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, null, dsINDATA);
                returnValue = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnValue = false;
            }

            return returnValue;
        }

        // 순서도 호출 - 자재투입취소한거 취소요청 Transaction
        internal bool SetMaterialInputCancelRequestCancel(List<string> lstERPTransferSequenceNo)
        {
            bool returnValue = false;
            string bizRuleName = "BR_ACT_REG_CANCLESEND_ERP_PROD";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            string outDataSetName = string.Empty;

            try
            {
                // INDATA
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("ERP_TRNF_SEQNO", typeof(string));

                foreach (string ERPTransferSequenceNo in lstERPTransferSequenceNo)
                {
                    DataRow drINDATA = dtINDATA.NewRow();
                    drINDATA["ERP_TRNF_SEQNO"] = ERPTransferSequenceNo;
                    dtINDATA.Rows.Add(drINDATA);
                }
                dsINDATA.Tables.Add(dtINDATA);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, null, dsINDATA);
                returnValue = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnValue = false;
            }

            return returnValue;
        }

        // 순서도 호출 - ActivityReason 정보
        internal DataTable GetActivityReasonDefectCode()
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_PRD_SEL_ACTIVITYREASON_DFCT_CODE";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
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

            return dtReturn;
        }

        // 순서도 호출 - Activity 정보
        internal DataTable GetActivityInfo()
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_PRD_SEL_ACTIVITY";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("ACTID", typeof(string));

                DataRow drRQSTDT = null;
                drRQSTDT = dtRQSTDT.NewRow(); drRQSTDT["LANGID"] = LoginInfo.LANGID; drRQSTDT["ACTID"] = "QUALITY_ISSUE"; dtRQSTDT.Rows.Add(drRQSTDT);
                drRQSTDT = dtRQSTDT.NewRow(); drRQSTDT["LANGID"] = LoginInfo.LANGID; drRQSTDT["ACTID"] = "REMAIN_RETURN"; dtRQSTDT.Rows.Add(drRQSTDT);
                drRQSTDT = dtRQSTDT.NewRow(); drRQSTDT["LANGID"] = LoginInfo.LANGID; drRQSTDT["ACTID"] = "GI_CANCEL_RETURN"; dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
                else
                {
                    dtReturn.Columns.Add("ACTID");
                    dtReturn.Columns.Add("ACTNAME");

                    DataRow drReturn = null;
                    drReturn = dtReturn.NewRow(); drReturn["ACTID"] = "QUALITY_ISSUE"; drReturn["ACTNAME"] = "품질이슈/불량"; dtReturn.Rows.Add(drReturn);
                    drReturn = dtReturn.NewRow(); drReturn["ACTID"] = "REMAIN_RETURN"; drReturn["ACTNAME"] = "잔량회수"; dtReturn.Rows.Add(drReturn);
                    drReturn = dtReturn.NewRow(); drReturn["ACTID"] = "GI_CANCEL_RETURN"; drReturn["ACTNAME"] = "사용취소 및 자재회수"; dtReturn.Rows.Add(drReturn);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 공장날짜 가져오기
        internal DataTable GetCaldate(string areaID)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_COM_SEL_CALDATE";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["AREAID"] = areaID;
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

            return dtReturn;
        }
        #endregion
    }
}