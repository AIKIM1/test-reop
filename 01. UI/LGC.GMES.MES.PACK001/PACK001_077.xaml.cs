/*************************************************************************************
 Created Date : 2021.02.03
      Creator : 김건식
   Decription : 오창/CWA ERP전송방식이 PUSH 방식으로 바뀌면서 기존 생산실적 화면 수량과 맞지 않아 신규 화면 추가함.
                TB_SFC_ERP_INPUT_RSLT_SUM , TB_SFC_ERP_OUTPUT_RSLT_SUM
--------------------------------------------------------------------------------------
 [Change History]
 2021.02.03  |  김건식  | 최초작성
 2022.08.08  |  정용석  | 완성실적 Tab 추가
 2024.01.19  |  김영택  | ERP 오류 유형 추가
 2024.08.07  |  권성혁  | Excel 다운 추가, Total의 Detail 추가
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System.Collections.Generic;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_077 : UserControl, IWorkArea
    {
        #region Member Variable Lists...
        private int endOutputEquipmentSegmentCount = 0;
        private int endOutputProcessIDCount = 0;
        private int endOutputActIDCount = 0;
        private int inputProcessIDCount = 0;
        private int outputProcessIDCount = 0;
        private int endOutputProjectAbbreviationName = 0;
        private int endOutputLOTType = 0;        
        DataTable dtDetailDataSet = new DataTable();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        public PACK001_077()
        {
            InitializeComponent();
            this.Initialize();
        }
        #endregion

        #region Member Function Lists...
        private void Initialize()
        {
            // Input Tab
            PackCommon.SearchRowCount(ref this.txtInputResultCount, 0);
            PackCommon.SetC1ComboBox(this.GetAreaInfo(), this.cboInputAreaID);
            PackCommon.SetC1ComboBox(this.GetActivityID("PUSH_ERP_ACTID_PACK", "INPUT"), this.cboInputActID, "-ALL-");
            PackCommon.SetC1ComboBox(this.GetCommonCodeInfo("INPUT_LOT_TYPE_CODE"), this.cboInputLotType, "-ALL-");

            // Output Tab
            PackCommon.SearchRowCount(ref this.txtOutputResultCount, 0);
            PackCommon.SetC1ComboBox(this.GetAreaInfo(), this.cboOutputAreaID);
            PackCommon.SetC1ComboBox(this.GetActivityID("PUSH_ERP_ACTID_PACK", "OUTPUT"), this.cboOutputActID, "-ALL-");

            // Endoutput Tab
            PackCommon.SearchRowCount(ref this.txtEndOutputResultCount, 0);
            PackCommon.SearchRowCount(ref this.txtEndOutputDetailCount, 0);
            PackCommon.SetC1ComboBox(this.GetAreaInfo(), this.cboEndOutputAreaID);
            PackCommon.SetMultiSelectionComboBox(this.GetERPActivityProcessInfo("N"), this.cboMultiEndOutputProcessID, ref this.endOutputProcessIDCount);
            PackCommon.SetMultiSelectionComboBox(this.GetLOTTypeInfo(), this.cboMultiEndOutputLotType, ref this.endOutputLOTType);

            this.cboInputAreaID.SelectedValue = LoginInfo.CFG_AREA_ID;
            this.cboInputEquipmentSegmentID.SelectedValue = LoginInfo.CFG_EQSG_ID;
            this.cboOutputAreaID.SelectedValue = LoginInfo.CFG_AREA_ID;
            this.cboOutputEquipmentSegmentID.SelectedValue = LoginInfo.CFG_EQSG_ID;
            this.cboEndOutputAreaID.SelectedValue = LoginInfo.CFG_AREA_ID;
        }

        // 조회질 1호 - Input 실적
        private void SearchERPInputSummary()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("ACTID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("FORMDATE", typeof(string));
                dtRQSTDT.Columns.Add("TODATE", typeof(string));
                dtRQSTDT.Columns.Add("PJT_ABBR", typeof(string));
                dtRQSTDT.Columns.Add("EXCL_FLAG", typeof(string));
                dtRQSTDT.Columns.Add("ERP_IF_MTRLID", typeof(string));
                dtRQSTDT.Columns.Add("INPUT_LOT_TYPE_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["EQSGID"] = string.IsNullOrWhiteSpace(this.cboInputEquipmentSegmentID.SelectedValue.ToString()) ? null : Util.GetCondition(this.cboInputEquipmentSegmentID);
                drRQSTDT["PROCID"] = Util.NVC(this.cboMultiInputProcessID.SelectedItemsToString) == "" ? null : this.cboMultiInputProcessID.SelectedItemsToString;
                drRQSTDT["ACTID"] = string.IsNullOrWhiteSpace(this.cboInputActID.SelectedValue.ToString()) ? null : Util.GetCondition(this.cboInputActID);
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["FORMDATE"] = Util.GetCondition(this.dtpDateFromInput);
                drRQSTDT["TODATE"] = Util.GetCondition(this.dtpDateToInput);
                drRQSTDT["PJT_ABBR"] = string.IsNullOrWhiteSpace(this.cboInputProjectAbbreviationName.SelectedValue.ToString()) ? null : Util.GetCondition(this.cboInputProjectAbbreviationName);
                drRQSTDT["EXCL_FLAG"] = this.chkInputExclFlag.IsChecked == true ? "Y" : null;
                drRQSTDT["ERP_IF_MTRLID"] = string.IsNullOrWhiteSpace(this.txtInputERPInterfaceMaterialID.Text.Trim()) ? null : Util.NVC(this.txtInputERPInterfaceMaterialID.Text.ToString());
                drRQSTDT["INPUT_LOT_TYPE_CODE"] = string.IsNullOrWhiteSpace(this.cboInputLotType.SelectedValue.ToString()) ? null : this.cboInputLotType.SelectedValue.ToString();
                dtRQSTDT.Rows.Add(drRQSTDT);

                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.SearchRowCount(ref this.txtInputResultCount, 0);
                Util.gridClear(this.dgListInputResult);
                PackCommon.DoEvents();

                DataTable dt = this.GetERPInputResultSummary(dtRQSTDT);
                if (CommonVerify.HasTableRow(dt))
                {
                    PackCommon.SearchRowCount(ref this.txtInputResultCount, dt.Rows.Count);
                    Util.GridSetData(this.dgListInputResult, dt, FrameOperation);
                }
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        // 조회질 2호 - Output 실적
        private void SearchERPOutputSummary()
        {
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("ACTID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("FORMDATE", typeof(string));
                dtRQSTDT.Columns.Add("TODATE", typeof(string));
                dtRQSTDT.Columns.Add("PJT_ABBR", typeof(string));
                dtRQSTDT.Columns.Add("EXCL_FLAG", typeof(string));
                dtRQSTDT.Columns.Add("ERP_IF_MTRLID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["EQSGID"] = string.IsNullOrWhiteSpace(this.cboOutputEquipmentSegmentID.SelectedValue.ToString()) ? null : Util.GetCondition(this.cboOutputEquipmentSegmentID);
                drRQSTDT["PROCID"] = Util.NVC(this.cboMultiOutputProcessID.SelectedItemsToString) == "" ? null : this.cboMultiOutputProcessID.SelectedItemsToString;
                drRQSTDT["ACTID"] = string.IsNullOrWhiteSpace(this.cboOutputActID.SelectedValue.ToString()) ? null : Util.GetCondition(this.cboOutputActID);
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["FORMDATE"] = Util.GetCondition(this.dtpDateFromOutput);
                drRQSTDT["TODATE"] = Util.GetCondition(this.dtpDateToOutput);
                drRQSTDT["PJT_ABBR"] = string.IsNullOrWhiteSpace(this.cboOutputProjectAbbreviationName.SelectedValue.ToString()) ? null : Util.GetCondition(this.cboOutputProjectAbbreviationName);
                drRQSTDT["EXCL_FLAG"] = this.chkOutputExclFlag.IsChecked == true ? "Y" : null;
                drRQSTDT["ERP_IF_MTRLID"] = string.IsNullOrWhiteSpace(this.txtOutputERPInterfaceMaterialID.Text.Trim()) ? null : Util.NVC(this.txtOutputERPInterfaceMaterialID.Text.ToString());
                dtRQSTDT.Rows.Add(drRQSTDT);

                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.SearchRowCount(ref this.txtOutputResultCount, 0);
                Util.gridClear(this.dgListOutputResult);
                PackCommon.DoEvents();

                DataTable dt = this.GetERPOutputResultSummary(dtRQSTDT);
                if (CommonVerify.HasTableRow(dt))
                {
                    PackCommon.SearchRowCount(ref this.txtOutputResultCount, dt.Rows.Count);
                    Util.GridSetData(this.dgListOutputResult, dt, FrameOperation);
                }
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        // 조회질 3호 - 제품 완성 실적
        private void SearchERPEndOutputSummary()
        {
            // Validation of Search Condition
            TimeSpan timeSpan = this.dtpDateToEndOutput.SelectedDateTime.Date.Subtract(this.dtpDateFromEndOutput.SelectedDateTime.Date);
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

            if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiEndOutputProcessID.SelectedItemsToString)))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("공정")); // %1(을)를 선택하세요.
                this.cboMultiEndOutputProcessID.Focus();
                return;
            }
            if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiEndOutputActID.SelectedItemsToString)))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("실적구분")); // %1(을)를 선택하세요.
                this.cboMultiEndOutputActID.Focus();
                return;
            }
            if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiEndOutputEquipmentSegmentID.SelectedItemsToString)))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("라인")); // %1(을)를 선택하세요.
                this.cboMultiEndOutputEquipmentSegmentID.Focus();
                return;
            }
            if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiEndOutputLotType.SelectedItemsToString)))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("LOTTYPE")); // %1(을)를 선택하세요.
                this.cboMultiEndOutputLotType.Focus();
                return;
            }
            if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiEndOutputProjectAbbrName.SelectedItemsToString)))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("PJT")); // %1(을)를 선택하세요.
                this.cboMultiEndOutputProjectAbbrName.Focus();
                return;
            }

            DataTable dtRQSTDT = new DataTable("RQSTDT");
            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("FROMDATE", typeof(string));
                dtRQSTDT.Columns.Add("TODATE", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("ACTID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("LOTTYPE", typeof(string));
                dtRQSTDT.Columns.Add("PROJECT_ABBR_NAME", typeof(string));
                dtRQSTDT.Columns.Add("ERP_INTERFACE_MTRLID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = this.cboEndOutputAreaID.SelectedValue.ToString();
                drRQSTDT["FROMDATE"] = Util.GetCondition(this.dtpDateFromEndOutput);
                drRQSTDT["TODATE"] = Util.GetCondition(this.dtpDateToEndOutput);
                drRQSTDT["PROCID"] = string.IsNullOrWhiteSpace(this.cboMultiEndOutputProcessID.SelectedItemsToString) ? null : this.cboMultiEndOutputProcessID.SelectedItemsToString;
                drRQSTDT["ACTID"] = string.IsNullOrWhiteSpace(this.cboMultiEndOutputActID.SelectedItemsToString) ? null : this.cboMultiEndOutputActID.SelectedItemsToString;
                drRQSTDT["EQSGID"] = string.IsNullOrWhiteSpace(this.cboMultiEndOutputEquipmentSegmentID.SelectedItemsToString) ? null : this.cboMultiEndOutputEquipmentSegmentID.SelectedItemsToString;
                drRQSTDT["LOTTYPE"] = string.IsNullOrWhiteSpace(this.cboMultiEndOutputLotType.SelectedItemsToString) ? null : this.cboMultiEndOutputLotType.SelectedItemsToString;
                drRQSTDT["PROJECT_ABBR_NAME"] = string.IsNullOrWhiteSpace(this.cboMultiEndOutputProjectAbbrName.SelectedItemsToString) ? null : this.cboMultiEndOutputProjectAbbrName.SelectedItemsToString;
                drRQSTDT["ERP_INTERFACE_MTRLID"] = string.IsNullOrWhiteSpace(this.txtEndOutputErpInterfaceMaterialID.Text.Trim()) ? null : Util.NVC(this.txtEndOutputErpInterfaceMaterialID.Text.ToString());
                dtRQSTDT.Rows.Add(drRQSTDT);

                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.SearchRowCount(ref this.txtEndOutputResultCount, 0);
                PackCommon.SearchRowCount(ref this.txtEndOutputDetailCount, 0);
                Util.gridClear(this.dgListEndOutputDetail);
                Util.gridClear(this.dgListEndOutputResult);
                PackCommon.DoEvents();

                dtDetailDataSet = dtRQSTDT;

                DataTable dt = this.GetERPEndOutputResultSummary(dtRQSTDT);
                if (CommonVerify.HasTableRow(dt))
                {
                    PackCommon.SearchRowCount(ref this.txtEndOutputResultCount, dt.Rows.Count);
                    Util.GridSetData(this.dgListEndOutputResult, dt, FrameOperation);
                }
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        // 조회질 4호 - 제품 완성 실적 상세
        private void SearchERPEndOutputDetail(C1.WPF.DataGrid.DataGridCell dataGridCell)
        {
            // Declarations...
            int leveru = 0;
            int total = 0;
            string equipmentSegmentID = string.Empty;
            string processID = string.Empty;
            string workorderID = string.Empty;
            string actID = string.Empty;
            string progressSequence = string.Empty;
            string resultSequence = string.Empty;
            string productID = string.Empty;
            string calDate = string.Empty;
            string actqty = string.Empty;

            try
            {
                if (dataGridCell == null || dataGridCell.Value == null)
                {
                    return;
                }

                int currentRow = dataGridCell.Row.Index;
                if (this.dgListEndOutputResult.Rows[currentRow].DataItem == null)
                {
                    return;
                }

                // 합계 Row 선택했을 경우 Return
                if (DataTableConverter.GetValue(this.dgListEndOutputResult.Rows[currentRow].DataItem, "LEVERU") == null)
                {
                    return;
                }

                // 소계Row 선택시와 Data Row 선택시 값이 바뀜.
                leveru = Convert.ToInt32(DataTableConverter.GetValue(this.dgListEndOutputResult.Rows[currentRow].DataItem, "LEVERU"));
                if (leveru.Equals(3))
                {
                    actqty = DataTableConverter.GetValue(this.dgListEndOutputResult.Rows[currentRow].DataItem, "ACTQTY").ToString().Replace(",", "");
                    total = Convert.ToInt32(actqty);
                    SearchERPEndOutputToDetail(total);
                    return;
                }

                equipmentSegmentID = DataTableConverter.GetValue(this.dgListEndOutputResult.Rows[currentRow].DataItem, "EQSGID").ToString();
                processID = DataTableConverter.GetValue(this.dgListEndOutputResult.Rows[currentRow].DataItem, "PROCID").ToString();
                actID = DataTableConverter.GetValue(this.dgListEndOutputResult.Rows[currentRow].DataItem, "ACTID").ToString();
                productID = DataTableConverter.GetValue(this.dgListEndOutputResult.Rows[currentRow].DataItem, "PRODID").ToString();
                switch (leveru)
                {
                    case 1:
                        workorderID = DataTableConverter.GetValue(this.dgListEndOutputResult.Rows[currentRow].DataItem, "WOID").ToString();
                        progressSequence = DataTableConverter.GetValue(this.dgListEndOutputResult.Rows[currentRow].DataItem, "PROG_SEQS").ToString();
                        resultSequence = DataTableConverter.GetValue(this.dgListEndOutputResult.Rows[currentRow].DataItem, "RSLT_SEQS").ToString();
                        calDate = DataTableConverter.GetValue(this.dgListEndOutputResult.Rows[currentRow].DataItem, "CALDATE").ToString();
                        break;
                    case 2:
                        DataTable dtEndOutputResult = DataTableConverter.Convert(this.dgListEndOutputResult.ItemsSource);
                        workorderID = dtEndOutputResult.AsEnumerable().Where(x => x.Field<string>("EQSGID") == equipmentSegmentID && x.Field<string>("PROCID") == processID &&
                                                                                  x.Field<string>("ACTID") == actID && x.Field<string>("PRODID") == productID && x.Field<long>("LEVERU").Equals(1)) // 2024.11.15. 김영국 - LEVERU DataType이 long으로 올라와 Type변경. int -> long
                                                                      .GroupBy(x => x.Field<string>("WOID")).Select(grp => grp.Key)
                                                                      .Aggregate((current, next) => current + "," + next);
                        progressSequence = dtEndOutputResult.AsEnumerable().Where(x => x.Field<string>("EQSGID") == equipmentSegmentID && x.Field<string>("PROCID") == processID &&
                                                                                       x.Field<string>("ACTID") == actID && x.Field<string>("PRODID") == productID && x.Field<long>("LEVERU").Equals(1)) // 2024.11.15. 김영국 - LEVERU DataType이 long으로 올라와 Type변경. int -> long
                                                                           .GroupBy(x => x.Field<string>("PROG_SEQS")).Select(grp => grp.Key)
                                                                           .Aggregate((current, next) => current + "," + next);
                        resultSequence = dtEndOutputResult.AsEnumerable().Where(x => x.Field<string>("EQSGID") == equipmentSegmentID && x.Field<string>("PROCID") == processID &&
                                                                                     x.Field<string>("ACTID") == actID && x.Field<string>("PRODID") == productID && x.Field<long>("LEVERU").Equals(1)) // 2024.11.15. 김영국 - LEVERU DataType이 long으로 올라와 Type변경. int -> long
                                                                         .GroupBy(x => x.Field<string>("RSLT_SEQS")).Select(grp => grp.Key)
                                                                         .Aggregate((current, next) => current + "," + next);
                        calDate = dtEndOutputResult.AsEnumerable().Where(x => x.Field<string>("EQSGID") == equipmentSegmentID && x.Field<string>("PROCID") == processID &&
                                                                              x.Field<string>("ACTID") == actID && x.Field<string>("PRODID") == productID && x.Field<long>("LEVERU").Equals(1)) // 2024.11.15. 김영국 - LEVERU DataType이 long으로 올라와 Type변경. int -> long
                                                                  .GroupBy(x => x.Field<string>("CALDATE")).Select(grp => grp.Key)
                                                                  .Aggregate((current, next) => current + "," + next);
                        break;
                    default:
                        break;
                }

                DataTable dtRQSTDT = new DataTable("RQSTDT");
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("WOID", typeof(string));
                dtRQSTDT.Columns.Add("ACTID", typeof(string));
                dtRQSTDT.Columns.Add("PROG_SEQS", typeof(string));
                dtRQSTDT.Columns.Add("RSLT_SEQS", typeof(string));
                dtRQSTDT.Columns.Add("PRODID", typeof(string));
                dtRQSTDT.Columns.Add("CALDATE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["EQSGID"] = equipmentSegmentID;
                drRQSTDT["PROCID"] = processID;
                drRQSTDT["WOID"] = workorderID;
                drRQSTDT["ACTID"] = actID;
                drRQSTDT["PROG_SEQS"] = progressSequence;
                drRQSTDT["RSLT_SEQS"] = resultSequence;
                drRQSTDT["PRODID"] = productID;
                drRQSTDT["CALDATE"] = calDate;
                dtRQSTDT.Rows.Add(drRQSTDT);

                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.SearchRowCount(ref this.txtEndOutputDetailCount, 0);
                Util.gridClear(this.dgListEndOutputDetail);
                PackCommon.DoEvents();

                DataTable dt = this.GetERPEndOutputResultDetail(dtRQSTDT);
                if (CommonVerify.HasTableRow(dt))
                {
                    PackCommon.SearchRowCount(ref this.txtEndOutputDetailCount, dt.Rows.Count);
                    Util.GridSetData(this.dgListEndOutputDetail, dt, FrameOperation);
                }
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        // 제품 완성 실적 Total  상세 정보 표시
        private void SearchERPEndOutputToDetail(int total)
        {
            try
            {
                int limit_qty = Limit_Detail_Qty();


                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("COUNT", typeof(int));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("ACTID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROJECT_ABBR_NAME", typeof(string));
                RQSTDT.Columns.Add("LOTTYPE", typeof(string));
                RQSTDT.Columns.Add("ERP_INTERFACE_MTRLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["COUNT"] = limit_qty;
                dr["FROMDATE"] = dtDetailDataSet.Rows[0]["FROMDATE"];
                dr["TODATE"] = dtDetailDataSet.Rows[0]["TODATE"];
                dr["PROCID"] = dtDetailDataSet.Rows[0]["PROCID"];
                dr["ACTID"] = dtDetailDataSet.Rows[0]["ACTID"];
                dr["EQSGID"] = dtDetailDataSet.Rows[0]["EQSGID"];
                dr["PROJECT_ABBR_NAME"] = dtDetailDataSet.Rows[0]["PROJECT_ABBR_NAME"];
                dr["LOTTYPE"] = dtDetailDataSet.Rows[0]["LOTTYPE"];
                dr["ERP_INTERFACE_MTRLID"] = dtDetailDataSet.Rows[0]["ERP_INTERFACE_MTRLID"];

                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_PRD_PACK_ERP_ENDOUTPUT_RSLT_DETL_TOTAL", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    PackCommon.SearchRowCount(ref this.txtEndOutputDetailCount, dtResult.Rows.Count);
                    Util.GridSetData(dgListEndOutputDetail, dtResult, FrameOperation, true);
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (limit_qty == 0)
                    {
                        return;
                    }
                    if (total > limit_qty)
                    {
                        Util.MessageValidation("SFU1909", limit_qty);
                    }

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        this.loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                    
                });
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        //공통코드에서 제한수량 가져옴
        private int Limit_Detail_Qty()
        {
            int limit_qty = 0;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "LIMITED_QTY_PACK";
                dr["CBO_CODE"] = "LIMIT_DETAIL_QTY";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    limit_qty = Int32.Parse(Util.NVC(dtResult.Rows[0]["ATTRIBUTE2"]));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return limit_qty;
        }


        // Input, Output 실적 공정 조회조건에서 공정 Combo Data가 바뀌었을 경우 이전 체크한거 연관하여 체크되게끔 하는것.. (왜 넣었는지 모르겠음)
        private void SetMultiSelectionBoxCheck(DataTable dt, MultiSelectionBox multiSelectionBox, ref int multiSelectionBoxBindingCount)
        {
            if (multiSelectionBox.ItemsSource == null)
            {
                // Binding 하고 Login Info의 공정코드 체크
                PackCommon.SetMultiSelectionComboBox(dt, multiSelectionBox, ref multiSelectionBoxBindingCount);

                int i = 0;
                multiSelectionBox.UncheckAll();
                foreach (DataRow dr in dt.Select())
                {
                    if (dr["CBO_CODE"].ToString().Equals(LoginInfo.CFG_PROC_ID))
                    {
                        multiSelectionBox.Check(i);
                    }
                    i++;
                }
            }
            else
            {
                // 기존 Binding Data 가져와서 두 Binding DataTable이 같은치 체크후
                // 기존 체크정보 있으면 체크정보 살리고 없으면 Login Info의 공정코드 체크
                DataTable dtCurrentData = DataTableConverter.Convert(multiSelectionBox.ItemsSource);
                if (PackCommon.IsSameDataTable(dtCurrentData, dt))
                {
                    return;
                }

                string selectedItem = multiSelectionBox.SelectedItemsToString;
                int i = 0;
                PackCommon.SetMultiSelectionComboBox(dt, multiSelectionBox, ref multiSelectionBoxBindingCount);
                multiSelectionBox.UncheckAll();
                if (string.IsNullOrEmpty(selectedItem))
                {
                    foreach (DataRow dr in dt.Select())
                    {
                        if (dr["CBO_CODE"].ToString().Equals(LoginInfo.CFG_PROC_ID))
                        {
                            multiSelectionBox.Check(i);
                        }
                        i++;
                    }
                }
                else
                {
                    foreach (DataRow dr in dt.Select())
                    {
                        if (selectedItem.Contains(dr["CBO_CODE"].ToString()))
                        {
                            multiSelectionBox.Check(i);
                        }
                        i++;
                    }
                }
            }
        }

        // 순서도 호출 - Input 실적
        private DataTable GetERPInputResultSummary(DataTable dtRQSTDT)
        {
            string bizRuleName = "DA_PRD_PACK_ERP_INPUT_RSLT_SUM";
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.TableName = "RQSTDT";
                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Output 실적
        private DataTable GetERPOutputResultSummary(DataTable dtRQSTDT)
        {
            string bizRuleName = "DA_PRD_PACK_ERP_OUTPUT_RSLT_SUM";
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.TableName = "RQSTDT";
                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - 완성실적 Summary
        private DataTable GetERPEndOutputResultSummary(DataTable dtRQSTDT)
        {
            string bizRuleName = "DA_PRD_PACK_ERP_ENDOUTPUT_RSLT_SUM";
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.TableName = "RQSTDT";
                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - 완성실적 Detail
        private DataTable GetERPEndOutputResultDetail(DataTable dtRQSTDT)
        {
            string bizRuleName = "DA_PRD_PACK_ERP_ENDOUTPUT_RSLT_DETL";
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.TableName = "RQSTDT";
                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - ERP 전송 Activity ID
        private DataTable GetActivityID(string cmcdType, string gubun)
        {
            DataTable dtReturn = new DataTable();
            DataTable dt = this.GetCommonCodeInfo(cmcdType);

            if (CommonVerify.HasTableRow(dt))
            {
                switch (gubun.ToUpper())
                {
                    case "INPUT":
                        dtReturn = dt.Rows.Cast<DataRow>().Where(x => x.Field<string>("ATTRIBUTE1") == gubun).CopyToDataTable();
                        break;
                    case "OUTPUT":
                        dtReturn = dt.Rows.Cast<DataRow>().Where(x => x.Field<string>("ATTRIBUTE2") == gubun).CopyToDataTable();
                        break;
                    default:
                        break;
                }
            }

            return dtReturn;
        }

        // 순서도 호출 - CommonCode
        private DataTable GetCommonCodeInfo(string cmcdType)
        {
            string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTES";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
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
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - 동코드 정보
        private DataTable GetAreaInfo()
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

        // 순서도 호출 - EquipmentSegment 정보
        private DataTable GetEquipmentSegmentInfo(string areaID)
        {
            string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");
            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EXCEPT_GROUP", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["EXCEPT_GROUP"] = null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - 공정정보 1호
        private DataTable GetProcessInfo(string equipmentSegmentID)
        {
            string bizRuleName = "DA_BAS_SEL_PROCESS_PACK_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["EQSGID"] = string.IsNullOrEmpty(equipmentSegmentID) ? null : equipmentSegmentID;
                drRQSTDT["AREA_TYPE_CODE"] = Area_Type.PACK;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - 공정정보 2호
        private DataTable GetERPActivityProcessInfo(string checkFlag)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_PRD_SEL_PROCESS_ENDOUTPUT_PACK";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREATYPE", typeof(string));
                dtRQSTDT.Columns.Add("P5200_CHECK_FLAG", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["AREATYPE"] = Area_Type.PACK;
                drRQSTDT["P5200_CHECK_FLAG"] = checkFlag;
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

        // 순서도 호출 - 모델 코드 정보
        private DataTable GetProjectAbbreviationNameInfo(string areaID, string equipmentSegmentID)
        {
            string bizRuleName = "DA_BAS_SEL_PRJMODEL_MULTI_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = (string.IsNullOrEmpty(areaID) || areaID.Equals("ALL")) ? null : areaID;
                drRQSTDT["EQSGID"] = (string.IsNullOrEmpty(equipmentSegmentID) || equipmentSegmentID.Equals("ALL")) ? null : equipmentSegmentID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Pack ERP 전송 Activity 정보
        private DataTable GetERPActivityID(int getType)
        {
            const string PACKING_TYPE = "PACKING";
            const string ENDOUTPUT_TYPE = "ENDOUTPUT";
            DataTable dtReturn = new DataTable();
            DataTable dt = this.GetCommonCodeInfo("PUSH_ERP_ACTID_PACK");

            if (!CommonVerify.HasTableRow(dt))
            {
                return null;
            }

            switch (getType)
            {
                case 1:     // 선택한 공정들의 공정유형이 오로지 포장공정인 경우
                    dtReturn = dt.Rows.Cast<DataRow>().Where(x => !string.IsNullOrEmpty(x.Field<string>("ATTRIBUTE3")) && (x.Field<string>("ATTRIBUTE3") == PACKING_TYPE)).OrderBy(x => x.Field<string>("CBO_CODE")).CopyToDataTable();
                    break;
                case 2:     // 선택한 공정들의 공정유형중에 포장공정이 포함된 경우
                    dtReturn = dt.Rows.Cast<DataRow>().Where(x => !string.IsNullOrEmpty(x.Field<string>("ATTRIBUTE3"))).OrderBy(x => x.Field<string>("CBO_CODE")).CopyToDataTable();
                    break;
                case 3:     // 선택한 공정들의 공정유형중에 포장공정이 없는 경우
                    dtReturn = dt.Rows.Cast<DataRow>().Where(x => !string.IsNullOrEmpty(x.Field<string>("ATTRIBUTE3")) && (x.Field<string>("ATTRIBUTE3") == ENDOUTPUT_TYPE)).OrderBy(x => x.Field<string>("CBO_CODE")).CopyToDataTable();
                    break;
                default:
                    break;
            }

            return dtReturn;
        }

        // 순서도 호출 - LOT Type 정보
        private DataTable GetLOTTypeInfo()
        {
            string bizRuleName = "DA_BAS_SEL_LOTTYPE_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    var query = dtRSLTDT.AsEnumerable().Select(x => new
                    {
                        LOTTYPE = x.Field<string>("CBO_CODE"),
                        LOTYNAME = x.Field<string>("CBO_CODE") + " : " + x.Field<string>("CBO_NAME")
                    });

                    dtRSLTDT = PackCommon.queryToDataTable(query.ToList());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }
        #endregion

        #region Event Lists...
        private void btnInputSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchERPInputSummary();
        }

        private void btnOutputSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchERPOutputSummary();
        }

        private void btnEndOutputSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchERPEndOutputSummary();
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgListEndOutputDetail);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void txtInputERPInterfaceMaterialID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.SearchERPInputSummary();
            }
        }

        private void txtOutputERPInterfaceMaterialID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.SearchERPOutputSummary();
            }
        }

        private void txtEndOutputERPInterfaceMaterialID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.SearchERPEndOutputSummary();
            }
        }

        private void cboInputAreaID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string areaID = e.NewValue.ToString();
            string equipmentSegmentID = (this.cboInputEquipmentSegmentID.SelectedValue == null) ? string.Empty : this.cboInputEquipmentSegmentID.SelectedValue.ToString();
            PackCommon.SetC1ComboBox(this.GetEquipmentSegmentInfo(areaID), this.cboInputEquipmentSegmentID, "-ALL-");
            PackCommon.SetC1ComboBox(this.GetProjectAbbreviationNameInfo(areaID, equipmentSegmentID), this.cboInputProjectAbbreviationName, "-ALL-");
        }

        private void cboInputEquipmentSegmentID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string areaID = this.cboInputAreaID.SelectedValue.ToString();
            string equipmentSegmentID = e.NewValue.ToString();
            this.SetMultiSelectionBoxCheck(this.GetProcessInfo(equipmentSegmentID), this.cboMultiInputProcessID, ref this.inputProcessIDCount);
            PackCommon.SetC1ComboBox(this.GetProjectAbbreviationNameInfo(areaID, equipmentSegmentID), this.cboInputProjectAbbreviationName, "-ALL-");
        }

        private void cboOutputAreaID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string areaID = e.NewValue.ToString();
            string equipmentSegmentID = (this.cboOutputEquipmentSegmentID.SelectedValue == null) ? string.Empty : this.cboOutputEquipmentSegmentID.SelectedValue.ToString();
            PackCommon.SetC1ComboBox(this.GetEquipmentSegmentInfo(areaID), this.cboOutputEquipmentSegmentID, "-ALL-");
            PackCommon.SetC1ComboBox(this.GetProjectAbbreviationNameInfo(areaID, equipmentSegmentID), this.cboOutputProjectAbbreviationName, "-ALL-");
        }

        private void cboOutputEquipmentSegmentID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string areaID = this.cboOutputAreaID.SelectedValue.ToString();
            string equipmentSegmentID = e.NewValue.ToString();
            this.SetMultiSelectionBoxCheck(this.GetProcessInfo(equipmentSegmentID), this.cboMultiOutputProcessID, ref this.outputProcessIDCount);
            PackCommon.SetC1ComboBox(this.GetProjectAbbreviationNameInfo(areaID, equipmentSegmentID), this.cboOutputProjectAbbreviationName, "-ALL-");
        }

        private void cboEndOutputAreaID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string areaID = e.NewValue.ToString();
            PackCommon.SetMultiSelectionComboBox(this.GetEquipmentSegmentInfo(areaID), this.cboMultiEndOutputEquipmentSegmentID, ref this.endOutputEquipmentSegmentCount);
        }

        private void cboMultiEndOutputEquipmentSegmentID_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                MultiSelectionBox multiSelectionBox = (MultiSelectionBox)sender;
                string selectedItems = multiSelectionBox.SelectedItemsToString;
                if (string.IsNullOrEmpty(selectedItems))
                {
                    return;
                }

                string areaID = this.cboEndOutputAreaID.SelectedValue.ToString();
                PackCommon.SetMultiSelectionComboBox(this.GetProjectAbbreviationNameInfo(areaID, selectedItems), this.cboMultiEndOutputProjectAbbrName, ref this.endOutputProjectAbbreviationName);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboMultiEndOutputProcessID_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                MultiSelectionBox multiSelectionBox = (MultiSelectionBox)sender;
                int getType = 0;
                string BOX_PROCESS_TYPE = "B";
                string selectedItems = multiSelectionBox.SelectedItemsToString;
                if (string.IsNullOrEmpty(selectedItems))
                {
                    return;
                }

                DataTable dt = DataTableConverter.Convert(this.cboMultiEndOutputProcessID.ItemsSource);
                string selectedProcessType = dt.AsEnumerable().Where(x => selectedItems.Contains(x.Field<string>("PROCID")))
                                                              .GroupBy(x => x.Field<string>("PROCTYPE")).Select(grp => grp.Key)
                                                              .Aggregate((current, next) => current + "," + next);

                // 선택한 공정들의 공정유형이 오로지 포장공정인 경우
                if (selectedProcessType.Equals(BOX_PROCESS_TYPE))
                {
                    getType = 1;
                }
                // 선택한 공정들의 공정유형중에 포장공정이 포함된 경우
                else if (selectedProcessType.Contains(BOX_PROCESS_TYPE))
                {
                    getType = 2;
                }
                // 선택한 공정들의 공정유형중에 포장공정이 없는 경우
                else
                {
                    getType = 3;
                }

                PackCommon.SetMultiSelectionComboBox(this.GetERPActivityID(getType), this.cboMultiEndOutputActID, ref endOutputActIDCount);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkEndOutputP5200ApplyLine_Checked(object sender, RoutedEventArgs e)
        {
            if (this.chkEndOutputExclFlag.IsChecked == true)
            {
                PackCommon.SetMultiSelectionComboBox(this.GetERPActivityProcessInfo("Y"), this.cboMultiEndOutputProcessID, ref this.endOutputProcessIDCount);
            }
        }

        private void chkEndOutputP5200ApplyLine_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this.chkEndOutputExclFlag.IsChecked != true)
            {
                PackCommon.SetMultiSelectionComboBox(this.GetERPActivityProcessInfo("N"), this.cboMultiEndOutputProcessID, ref this.endOutputProcessIDCount);
            }
        }

        private void dgListEndOutputResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid c1DataGrid = (C1DataGrid)sender;
                Point point = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell dataGridCell = c1DataGrid.GetCellFromPoint(point);

                if (dataGridCell == null)
                {
                    return;
                }

                this.SearchERPEndOutputDetail(dataGridCell);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListEndOutputResult_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    int leveru = Convert.ToInt32(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LEVERU"));
                    if (e.Cell.Column.Name.ToUpper().Equals("ACTQTY"))
                    {
                        switch (leveru)
                        {
                            case 1:
                            case 2:
                            case 3:
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                break;
                            default:
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListEndOutputResult_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid c1DataGrid = (C1DataGrid)sender;

            try
            {
                if (c1DataGrid.GetRowCount() == 0)
                {
                    return;
                }

                for (int i = 0; i < c1DataGrid.GetRowCount() + 2; i++)
                {
                    if (Convert.ToInt32(DataTableConverter.GetValue(c1DataGrid.Rows[i].DataItem, "LEVERU")) <= 1)
                    {
                        continue;
                    }

                    e.Merge(new DataGridCellsRange(c1DataGrid.GetCell(i, 0), c1DataGrid.GetCell(i, 17)));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListEndOutputResult_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            DataRowView dataRowView = (DataRowView)e.Row.DataItem;

            if (dataRowView == null)
            {
                return;
            }

            int leveru = Convert.ToInt32(DataTableConverter.GetValue(dataRowView, "LEVERU"));
            if (leveru > 1)
            {
                e.Row.Presenter.Background = new SolidColorBrush(Colors.Yellow);
            }
        }
        #endregion
    }
}