/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 김준겸
   Decription : 개별 (Wip) Lot을 가지고 Box or Pallet 
--------------------------------------------------------------------------------------
 [Change History]
  2020.06.30  DEVELOPER : Initial Created.
  2020.07.24  김준겸  CSR ID : C20200602-000008 BOX/PALLET 해당에 대해서 저장 위치 등록 및 기타 수정 요청 작업.
  2024.01.23  정용석  CSR ID : E20240119-001767 Rack Mapping / Unmapping시에 매핑일자 Update & 소스 정리 (불필요한 멤버변수 및 함수 제거 및 변수 Refactoring)
  2024.01.23  정용석  CSR ID : E20240111-001626 1. Rack 적용 버튼을 눌렀을 때 Rack에 매핑된 LOT이 얼마간 매핑되었는지 알려주는 컬럼 추가
                                                2. Rack History 조회시 Partial ILT 창고 재고 제외로 인하여 “Partial ILT 창고 재고는 조회되지 않습니다.” 라는 문구 추가 (김선준C)
                                                3. 2와 같은 이유로 LOT & Rack 매핑시에 Combobox에 표출되는 창고중에 Partial ILT 창고 제외 (김선준C)
  2024.05.16  정용석  CSR ID : E20240405-001900 RackMapping / Unmapping시에 입력되는 ID가 LOT뿐만 아니라 다른 ID도 입력되도록 수정
**************************************************************************************/
using C1.WPF;
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
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_067 : UserControl, IWorkArea
    {
        #region Member Variable Lists...
        private PACK001_067_DataHelper dataHelper = new PACK001_067_DataHelper();
        private int whIDMultiComboBindDataCount = 0;
        private int rackIDMultiComboBindDataCount = 0;
        private int maxCopyAndPasteLOTCount = 500;      // 한번에 Copy & Paste할 수 있는 최대 갯수
        private int maxRackMappingLOTCount = 10000;     // Rack Maoping (저장버튼 Click) 시점에 한번에 Rack Mapping을 할 수 있는 Row 갯수 (Rack Mapping Grid에 표출되는 Row 갯수)
        private DataTable dtValidationResult = new DataTable();
        private DataTable dtValidationInvalidResult = new DataTable();
        #endregion

        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        // 스캔시 LOT 정보 담아두기 위한 변수(LOT정보)     
        string scanAreaID = string.Empty;
        string scanAreaName = string.Empty;
        string scanRackID = string.Empty;
        string scanRackName = string.Empty;
        string scanWareHouseID = string.Empty;
        string scanWareHouseName = string.Empty;
        string sccanRackMaxLoadQty = string.Empty;
        string scanRackLoadQty = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_067()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function Lists...
        private void Initialize()
        {
            // LOT SCAN COPY & PASTE시에 최대로 수행할수 있는 갯수 설정
            DataTable dt = this.dataHelper.GetCommonCodeInfo("LIMITED_QTY_PACK", "LIMIT_RACK_PACKING");
            if (CommonVerify.HasTableRow(dt))
            {
                foreach (DataRowView dataRowView in dt.AsDataView())
                {
                    if (!string.IsNullOrEmpty(dataRowView["ATTRIBUTE2"].ToString()))
                    {
                        this.maxCopyAndPasteLOTCount = Convert.ToInt32(dataRowView["ATTRIBUTE2"].ToString());  // 한번에 Copy & Paste할 수 있는 최대 갯수
                    }
                    if (!string.IsNullOrEmpty(dataRowView["ATTRIBUTE1"].ToString()))
                    {
                        this.maxRackMappingLOTCount = Convert.ToInt32(dataRowView["ATTRIBUTE1"].ToString());     // Rack Maoping (저장버튼 Click) 시점에 한번에 Rack Mapping을 할 수 있는 Row 갯수
                    }
                }
            }

            PackCommon.SetC1ComboBox(this.dataHelper.GetWareHouseInfo(LoginInfo.CFG_AREA_ID), this.cboWHIDRackMapping, true, "-SELECT-");
            PackCommon.SearchRowCount(ref this.txtRackMappingCount, 0);
            PackCommon.SearchRowCount(ref this.txtRackMappingSummaryCount, 0);
            PackCommon.SearchRowCount(ref this.txtRackmappingDetailCount, 0);

            this.SetMultiSelectionComboBox(this.dataHelper.GetWareHouseInfo(LoginInfo.CFG_AREA_ID), this.cboMultiWHIDRackMappingHistory, ref this.whIDMultiComboBindDataCount);
            this.lblAREANAME.Text = LoginInfo.CFG_AREA_ID + " : " + LoginInfo.CFG_AREA_NAME;
            this.tbLotInform.Text = "SCAN LOT" + ObjectDic.Instance.GetObjectName("정보");

            this.lblComment.Text = MessageDic.Instance.GetMessage("SFU8278");
        }

        // Rack vs LOT Mapping 현황 조회
        private void SearchProcess(string LOTID = null)
        {
            this.loadingIndicator.Visibility = Visibility.Visible;
            Util.gridClear(this.dgRackMappingSummary);
            Util.gridClear(this.dgRackMappingDetail);
            PackCommon.SearchRowCount(ref this.txtRackMappingSummaryCount, 0);
            PackCommon.SearchRowCount(ref this.txtRackmappingDetailCount, 0);
            PackCommon.DoEvents();

            try
            {
                string areaID = LoginInfo.CFG_AREA_ID;
                string warehouseID = Util.NVC(this.cboMultiWHIDRackMappingHistory.SelectedItemsToString) == "" ? null : this.cboMultiWHIDRackMappingHistory.SelectedItemsToString;
                string rackID = Util.NVC(this.cboMultiRackIDRackMappingHistory.SelectedItemsToString) == "" ? null : this.cboMultiRackIDRackMappingHistory.SelectedItemsToString;
                //string LOTID = string.IsNullOrEmpty(Util.GetCondition(txtLOTID)) ? null : Util.GetCondition(txtLOTID);

                // 조건에 따라서 검색 내용 변경
                if (LOTID == null)
                {
                    // 화면 초기 상태가 아니라면 초기 상태로
                    if (txtRackMappingSummaryCount.Visibility == Visibility.Hidden)
                    {
                        txtRackMappingSummaryCount.Visibility = Visibility.Visible;
                        ContentRight.RowDefinitions[4].Height = new GridLength(1, GridUnitType.Star);
                    }
                    DataTable dtRack = this.dataHelper.GetRackMappinngInfo(areaID, warehouseID, rackID, LOTID);

                    if (CommonVerify.HasTableRow(dtRack))
                    {
                        Util.GridSetData(this.dgRackMappingSummary, dtRack, FrameOperation);
                        PackCommon.SearchRowCount(ref this.txtRackMappingSummaryCount, dtRack.Rows.Count);
                    }
                    else
                    {
                        Util.MessageInfo("SFU3536");  //조회된 결과가 없습니다
                    }
                }
                else
                {
                    DataTable dtLot = this.dataHelper.GetLotIDMappingInfo(areaID, warehouseID, rackID, LOTID);

                    if (CommonVerify.HasTableRow(dtLot))
                    {
                        Util.GridSetData(this.dgRackMappingDetail, dtLot, FrameOperation);
                        PackCommon.SearchRowCount(ref this.txtRackmappingDetailCount, dtLot.Rows.Count);
                        txtRackMappingSummaryCount.Visibility = Visibility.Hidden;
                        ContentRight.RowDefinitions[4].Height = new GridLength(0);
                    }
                    else
                    {
                        // 화면 초기 상태가 아니라면 초기 상태로
                        if (txtRackMappingSummaryCount.Visibility == Visibility.Hidden)
                        {
                            txtRackMappingSummaryCount.Visibility = Visibility.Visible;
                            ContentRight.RowDefinitions[4].Height = new GridLength(1, GridUnitType.Star);
                        }

                        Util.MessageInfo("SFU3536");  //조회된 결과가 없습니다
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
                this.txtLOTID.Text = string.Empty;
            }
        }

        // Grid CheckBox Header Click
        private void GridCheckBoxHeaderClick(CheckBox checkBox, bool isChecked)
        {
            C1DataGrid c1DataGrid = null;
            IList<FrameworkElement> ilist = checkBox.GetAllParents();
            foreach (var item in ilist)
            {
                if (item.GetType().Name.ToUpper() == "C1DATAGRID")
                {
                    c1DataGrid = (C1DataGrid)item;
                    DataTable dt = DataTableConverter.Convert(c1DataGrid.ItemsSource);
                    foreach (DataRow dr in dt.Rows)
                    {
                        dr["CHK"] = isChecked;
                    }
                    c1DataGrid.ItemsSource = DataTableConverter.Convert(dt);
                    break;
                }
            }
        }

        private void SetRackInfo()
        {
            this.txtMaxLoadQty.Text = string.Empty;
            this.txtLoadQty.Text = string.Empty;

            string wareHouseID = Util.GetCondition(this.cboWHIDRackMapping);
            string rackID = Util.GetCondition(cboRackIDRackMapping);
            DataTable dt = this.dataHelper.GetRackMappinngInfo(LoginInfo.CFG_AREA_ID, wareHouseID, rackID, string.Empty);

            if (CommonVerify.HasTableRow(dt))
            {
                foreach (DataRowView dataRowView in dt.AsDataView())
                {
                    this.scanAreaID = dataRowView["AREAID"].ToString();
                    this.scanAreaName = dataRowView["AREANAME"].ToString();
                    this.scanRackID = dataRowView["RACKID"].ToString();
                    this.scanRackName = dataRowView["RACKNAME"].ToString();
                    this.scanWareHouseID = dataRowView["WHID"].ToString();
                    this.scanWareHouseName = dataRowView["WHNAME"].ToString();
                    this.sccanRackMaxLoadQty = dataRowView["BOXMAXCNT"].ToString();
                    this.scanRackLoadQty = dataRowView["CNT"].ToString();

                    this.txtMaxLoadQty.Text = sccanRackMaxLoadQty;
                    this.txtLoadQty.Text = scanRackLoadQty;
                }
            }
        }

        private void RackMappingAreaInitialize()
        {
            PackCommon.SearchRowCount(ref this.txtRackMappingCount, 0);

            Util.gridClear(this.dgRackLOTMapping);
            (this.dgRackLOTMapping.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn).MaxWidth = 40;

            this.txtScanID.Text = string.Empty;
            this.txtBoxInfo.Text = string.Empty;
            this.txtMaxLoadQty.Text = string.Empty;
            this.txtLoadQty.Text = string.Empty;
            this.txtNOTE.Text = string.Empty;
            this.cboWHIDRackMapping.SelectedIndex = 0;
            this.cboRackIDRackMapping.SelectedIndex = 0;
            checkHeaderAll.IsChecked = false;
        }

        private void GetRackMapping(string rackID)
        {
            try
            {
                PackCommon.SearchRowCount(ref this.txtRackmappingDetailCount, 0);
                //Util.gridClear(this.dgRackMappingDetail);
                DataTable dt = this.dataHelper.GetRackLOTMappingInfo(rackID);
                DataTable dtRackMappingDetail = DataTableConverter.Convert(dgRackMappingDetail.ItemsSource);
                if (CommonVerify.HasTableRow(dt))
                {
                    dtRackMappingDetail.Merge(dt);
                }
                PackCommon.SearchRowCount(ref this.txtRackmappingDetailCount, dtRackMappingDetail.Rows.Count);
                Util.GridSetData(this.dgRackMappingDetail, dtRackMappingDetail, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void delRackMapping(string rackID)
        {
            try
            {
                PackCommon.SearchRowCount(ref this.txtRackmappingDetailCount, 0);
                //Util.gridClear(this.dgRackMappingDetail);
                DataTable dt = this.dataHelper.GetRackLOTMappingInfo(rackID);
                DataTable dtRackMappingDetail = DataTableConverter.Convert(dgRackMappingDetail.ItemsSource);
                if (CommonVerify.HasTableRow(dt))
                {
                    dtRackMappingDetail.Select("RACK_ID = '" + dt.Rows[0]["RACK_ID"].ToString() + "'").ToList<DataRow>().ForEach(row => row.Delete());
                }
                PackCommon.SearchRowCount(ref this.txtRackmappingDetailCount, dtRackMappingDetail.Rows.Count);
                Util.GridSetData(this.dgRackMappingDetail, dtRackMappingDetail, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ValidationScanID2(string scanID)
        {
            DataSet ds = this.dataHelper.GetScanIDInfo(scanID);
            if (!CommonVerify.HasTableInDataSet(ds))
            {
                return;
            }

            // 건전 데이터 처리
            if (CommonVerify.HasTableRow(ds.Tables["OUTDATA"]))
            {
                this.DataCopy(this.dtValidationResult, ds.Tables["OUTDATA"]);
            }

            // 불건전 데이터 처리
            if (CommonVerify.HasTableRow(ds.Tables["OUTDATA_INVALID"]))
            {
                this.DataCopy(this.dtValidationInvalidResult, ds.Tables["OUTDATA_INVALID"]);
            }
        }

        private void DataCopy(DataTable dtTarget, DataTable dtSource)
        {
            if (!CommonVerify.HasTableRow(dtTarget))
            {
                dtTarget.Merge(dtSource);
            }
            else
            {
                // LOTID 중복 체크
                bool isDupLOTID = false;
                foreach (DataRow drSrouce in dtSource.Rows)
                {
                    foreach (DataRowView drvTarget in dtTarget.AsDataView())
                    {
                        if (drvTarget["LOTID"].ToString() == drSrouce["LOTID"].ToString())
                        {
                            isDupLOTID = true;
                            break;
                        }
                    }

                    if (!isDupLOTID)
                    {
                        dtTarget.ImportRow(drSrouce);
                    }
                }
            }
        }

        private void DataBinding()
        {
            if (CommonVerify.HasTableRow(this.dtValidationResult))
            {
                if (this.dgRackLOTMapping.GetRowCount() <= 0)
                {

                    DataTable dtRackMapping = new DataTable();
                    dtRackMapping.Columns.Add("CHK", typeof(bool));
                    dtRackMapping.Columns.Add("AREANAME", typeof(string));
                    dtRackMapping.Columns.Add("WHID", typeof(string));
                    dtRackMapping.Columns.Add("WHNAME", typeof(string));
                    dtRackMapping.Columns.Add("RACKID", typeof(string));
                    dtRackMapping.Columns.Add("RACKNAME", typeof(string));
                    dtRackMapping.Columns.Add("LOTID", typeof(string));

                    for (int i = 0; i < this.dtValidationResult.Rows.Count; i++)
                    {
                        DataRow drRackMapping = dtRackMapping.NewRow();

                        drRackMapping["CHK"] = true;
                        drRackMapping["AREANAME"] = scanAreaName;
                        drRackMapping["WHID"] = scanWareHouseID;
                        drRackMapping["WHNAME"] = scanWareHouseName;
                        drRackMapping["RACKID"] = scanRackID;
                        drRackMapping["RACKNAME"] = scanRackName;
                        drRackMapping["LOTID"] = this.dtValidationResult.Rows[i]["LOTID"].ToString();

                        dtRackMapping.Rows.Add(drRackMapping);
                    }

                    this.dgRackLOTMapping.ItemsSource = DataTableConverter.Convert(dtRackMapping);
                }
                else
                {
                    for (int i = 0; i < this.dtValidationResult.Rows.Count; i++)
                    {
                        int currentRow = this.dgRackLOTMapping.GetRowCount();
                        bool isDuplicateData = false;
                        this.dgRackLOTMapping.EndNewRow(true);

                        foreach (C1.WPF.DataGrid.DataGridRow dataGridRow in dgRackLOTMapping.Rows)
                        {
                            if (dataGridRow.DataItem != null)
                            {
                                DataRowView dataRowView = dataGridRow.DataItem as DataRowView;
                                if (dataRowView["LOTID"].ToString() == this.dtValidationResult.Rows[i]["LOTID"].ToString())
                                {
                                    isDuplicateData = true;
                                    break;
                                }
                            }
                        }

                        if (isDuplicateData)
                        {
                            DataRow drInvalid = this.dtValidationInvalidResult.NewRow();
                            drInvalid["SCANID"] = this.dtValidationResult.Rows[i]["SCANID"].ToString();
                            drInvalid["LOTID"] = this.dtValidationResult.Rows[i]["LOTID"].ToString();
                            drInvalid["NOTE"] = MessageDic.Instance.GetMessage("SFU2014");
                            this.dtValidationInvalidResult.Rows.Add(drInvalid);
                            continue;
                        }

                        this.dgRackLOTMapping.BeginNewRow();
                        this.dgRackLOTMapping.EndNewRow(true);
                        DataTableConverter.SetValue(dgRackLOTMapping.Rows[currentRow].DataItem, "CHK", "True");
                        DataTableConverter.SetValue(dgRackLOTMapping.Rows[currentRow].DataItem, "AREANAME", scanAreaName);
                        DataTableConverter.SetValue(dgRackLOTMapping.Rows[currentRow].DataItem, "WHID", scanWareHouseID);
                        DataTableConverter.SetValue(dgRackLOTMapping.Rows[currentRow].DataItem, "WHNAME", scanWareHouseName);
                        DataTableConverter.SetValue(dgRackLOTMapping.Rows[currentRow].DataItem, "RACKID", scanRackID);
                        DataTableConverter.SetValue(dgRackLOTMapping.Rows[currentRow].DataItem, "RACKNAME", scanRackName);
                        DataTableConverter.SetValue(dgRackLOTMapping.Rows[currentRow].DataItem, "LOTID", this.dtValidationResult.Rows[i]["LOTID"].ToString());
                    }
                }
                PackCommon.SearchRowCount(ref this.txtRackMappingCount, this.dgRackLOTMapping.GetRowCount());
            }
        }
        
        private void ValidationScanID(string scanID)
        {
            try
            {
                DataSet ds = this.dataHelper.GetScanIDInfo(scanID);
                if (!CommonVerify.HasTableInDataSet(ds))
                {
                    return;
                }

                if (CommonVerify.HasTableRow(ds.Tables["OUTDATA"]))
                {
                    DataTable dtResult = ds.Tables["OUTDATA"];
                    if (this.dgRackLOTMapping.GetRowCount() <= 0)
                    {

                        DataTable dtRackMapping = new DataTable();
                        dtRackMapping.Columns.Add("CHK", typeof(bool));
                        dtRackMapping.Columns.Add("AREANAME", typeof(string));
                        dtRackMapping.Columns.Add("WHID", typeof(string));
                        dtRackMapping.Columns.Add("WHNAME", typeof(string));
                        dtRackMapping.Columns.Add("RACKID", typeof(string));
                        dtRackMapping.Columns.Add("RACKNAME", typeof(string));
                        dtRackMapping.Columns.Add("LOTID", typeof(string));

                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            DataRow drRackMapping = dtRackMapping.NewRow();

                            drRackMapping["CHK"] = true;
                            drRackMapping["AREANAME"] = scanAreaName;
                            drRackMapping["WHID"] = scanWareHouseID;
                            drRackMapping["WHNAME"] = scanWareHouseName;
                            drRackMapping["RACKID"] = scanRackID;
                            drRackMapping["RACKNAME"] = scanRackName;
                            drRackMapping["LOTID"] = dtResult.Rows[i]["LOTID"].ToString();

                            dtRackMapping.Rows.Add(drRackMapping);
                        }

                        this.dgRackLOTMapping.ItemsSource = DataTableConverter.Convert(dtRackMapping);
                    }
                    else
                    {
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            int currentRow = this.dgRackLOTMapping.GetRowCount();
                            bool isDuplicateData = false;
                            this.dgRackLOTMapping.EndNewRow(true);

                            foreach (C1.WPF.DataGrid.DataGridRow dataGridRow in dgRackLOTMapping.Rows)
                            {
                                if (dataGridRow.DataItem != null)
                                {
                                    DataRowView dataRowView = dataGridRow.DataItem as DataRowView;
                                    if (dataRowView["LOTID"].ToString() == dtResult.Rows[i]["LOTID"].ToString())
                                    {
                                        isDuplicateData = true;
                                        break;
                                    }
                                }
                            }

                            if (isDuplicateData)
                            {
                                DataRow drInvalid = ds.Tables["OUTDATA_INVALID"].NewRow();
                                drInvalid["SCANID"] = dtResult.Rows[i]["SCANID"].ToString();
                                drInvalid["LOTID"] = dtResult.Rows[i]["LOTID"].ToString();
                                drInvalid["NOTE"] = MessageDic.Instance.GetMessage("SFU2014");
                                ds.Tables["OUTDATA_INVALID"].Rows.Add(drInvalid);
                                continue;
                            }

                            this.dgRackLOTMapping.BeginNewRow();
                            this.dgRackLOTMapping.EndNewRow(true);
                            DataTableConverter.SetValue(dgRackLOTMapping.Rows[currentRow].DataItem, "CHK", "True");
                            DataTableConverter.SetValue(dgRackLOTMapping.Rows[currentRow].DataItem, "AREANAME", scanAreaName);
                            DataTableConverter.SetValue(dgRackLOTMapping.Rows[currentRow].DataItem, "WHID", scanWareHouseID);
                            DataTableConverter.SetValue(dgRackLOTMapping.Rows[currentRow].DataItem, "WHNAME", scanWareHouseName);
                            DataTableConverter.SetValue(dgRackLOTMapping.Rows[currentRow].DataItem, "RACKID", scanRackID);
                            DataTableConverter.SetValue(dgRackLOTMapping.Rows[currentRow].DataItem, "RACKNAME", scanRackName);
                            DataTableConverter.SetValue(dgRackLOTMapping.Rows[currentRow].DataItem, "LOTID", dtResult.Rows[i]["LOTID"].ToString());
                        }
                    }
                    PackCommon.SearchRowCount(ref this.txtRackMappingCount, this.dgRackLOTMapping.GetRowCount());
                }

                if (CommonVerify.HasTableRow(ds.Tables["OUTDATA_INVALID"]))
                {
                    PackCommon.Show_EXCEPTION_POPUP(ds.Tables["OUTDATA_INVALID"], this.GetType().Name, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void RackUnMapping()
        {
            if (this.dgRackMappingDetail == null || this.dgRackMappingDetail.GetRowCount() <= 0)
            {
                return;
            }

            try
            {
                var query = DataTableConverter.Convert(this.dgRackMappingDetail.ItemsSource).AsEnumerable().Where(x => x.Field<bool>("CHK"));
                if (query.Count() <= 0)
                {
                    Util.MessageValidation("SFU1651");      // 선택된 항목이 없습니다.
                    return;
                }

                DataTable dt = new DataTable();
                dt.Columns.Add("MAPPING_FLAG", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("WH_ID", typeof(string));
                dt.Columns.Add("RACK_ID", typeof(string));
                dt.Columns.Add("NOTE", typeof(string));

                // Unmapping할 대상 선택
                foreach (var item in query)
                {
                    DataRow dr = dt.NewRow();
                    dr["MAPPING_FLAG"] = "N";
                    dr["LOTID"] = item.Field<string>("LOTID");
                    dr["WH_ID"] = item.Field<string>("WH_ID");
                    dr["RACK_ID"] = item.Field<string>("RACK_ID");
                    dr["NOTE"] = item.Field<string>("NOTE");
                    dt.Rows.Add(dr);
                }

                if (this.dataHelper.SetRackLotMapping(dt))
                {
                    Util.MessageInfo("SFU1270"); // 저장되었습니다.
                    this.SearchProcess();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void RackMapping()
        {
            if (this.dgRackLOTMapping == null || this.dgRackLOTMapping.GetRowCount() <= 0)
            {
                return;
            }

            try
            {
                var query = DataTableConverter.Convert(this.dgRackLOTMapping.ItemsSource).AsEnumerable().Where(x => x.Field<bool>("CHK"));
                if (query.Count() <= 0)
                {
                    return;
                }

                DataTable dt = new DataTable();
                dt.Columns.Add("MAPPING_FLAG", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("WH_ID", typeof(string));
                dt.Columns.Add("RACK_ID", typeof(string));
                dt.Columns.Add("NOTE", typeof(string));

                // Mapping할 대상 선택
                foreach (var item in query)
                {
                    DataRow dr = dt.NewRow();
                    dr["MAPPING_FLAG"] = "Y";
                    dr["LOTID"] = item.Field<string>("LOTID");
                    dr["WH_ID"] = this.cboWHIDRackMapping.SelectedValue.ToString();
                    dr["RACK_ID"] = this.cboRackIDRackMapping.SelectedValue.ToString();
                    dr["NOTE"] = this.txtNOTE.Text;
                    dt.Rows.Add(dr);
                }

                if (this.dataHelper.SetRackLotMapping(dt))
                {
                    Util.MessageInfo("SFU1270"); // 저장되었습니다.
                    this.RackMappingAreaInitialize();
                    this.SearchProcess();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetMultiSelectionComboBox(DataTable dt, MultiSelectionBox multiSelectionBox, ref int bindingRowCount)
        {
            List<string> lstValueMemberPathFilter = new List<string>() { "ID", "CODE", "CD", "USE_FLAG", "TYPE" };
            List<string> lstDisplayMemberPathFilter = new List<string>() { "NAME", "NM", "DESC" };

            if (!CommonVerify.HasTableRow(dt))
            {
                dt = new DataTable();
                dt.Columns.Add("CBO_CODE", typeof(string));
                dt.Columns.Add("CBO_NAME", typeof(string));

                DataRow dr = dt.NewRow();
                dr = dt.NewRow();
                dr.ItemArray = new object[] { string.Empty, "All" };
                dt.Rows.Add(dr);

                multiSelectionBox.isAllUsed = false;
            }
            else
            {
                multiSelectionBox.isAllUsed = true;
            }

            try
            {
                var selectedValuePath = (from d1 in dt.Columns.OfType<DataColumn>()
                                         from d2 in lstValueMemberPathFilter
                                         select new
                                         {
                                             COLUMNNAME = d1.ColumnName,
                                             FILTER = d2
                                         }).Where(x => x.COLUMNNAME.Contains(x.FILTER) || x.COLUMNNAME.EndsWith(x.FILTER)).Select(x => x.COLUMNNAME).FirstOrDefault();

                var displayMemberPath = (from d1 in dt.Columns.OfType<DataColumn>()
                                         from d2 in lstDisplayMemberPathFilter
                                         select new
                                         {
                                             COLUMNNAME = d1.ColumnName,
                                             FILTER = d2
                                         }).Where(x => x.COLUMNNAME.Contains(x.FILTER) || x.COLUMNNAME.EndsWith(x.FILTER)).Select(x => x.COLUMNNAME).FirstOrDefault();

                if (selectedValuePath == null || displayMemberPath == null)
                {
                    return;
                }

                multiSelectionBox.ApplyTemplate();
                multiSelectionBox.ItemsSource = null;
                multiSelectionBox.DisplayMemberPath = displayMemberPath;
                multiSelectionBox.SelectedValuePath = selectedValuePath;

                bindingRowCount = dt.Rows.Count;
                multiSelectionBox.ItemsSource = dt.AsDataView();
                multiSelectionBox.Check(-1);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion        

        #region Event Lists...
        private void PACK001_067_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Initialize();
                this.SearchProcess();
                this.Loaded -= PACK001_067_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string LOTID = string.IsNullOrEmpty(Util.GetCondition(txtLOTID)) ? null : Util.GetCondition(txtLOTID);
                SearchProcess(LOTID);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int dgRackGridCount = 0;

                dgRackGridCount = dgRackLOTMapping.Rows.Count - 1; // 실제 row 해당 Count 숫자  (헤더값 제외)

                if (this.dgRackLOTMapping.ItemsSource == null)
                {
                    return;
                }

                for (int i = dgRackGridCount; 0 < i; i--)
                {
                    var chkYn = DataTableConverter.GetValue(dgRackLOTMapping.Rows[i - 1].DataItem, "CHK");
                    var lot_id = DataTableConverter.GetValue(dgRackLOTMapping.Rows[i - 1].DataItem, "LOTID");

                    if (chkYn == null)
                    {
                        dgRackLOTMapping.RemoveRow(i - 1);
                    }
                    else if (Convert.ToBoolean(chkYn))
                    {
                        dgRackLOTMapping.EndNewRow(true);
                        dgRackLOTMapping.RemoveRow(i - 1);
                    }
                }
                DataTable dt = DataTableConverter.Convert(dgRackLOTMapping.ItemsSource);
                PackCommon.SearchRowCount(ref this.txtRackMappingCount, dt.Rows.Count);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRackLOTMapping.GetRowCount() == 0)
                {
                    return;
                }
                Util.gridClear(dgRackLOTMapping);
                PackCommon.SearchRowCount(ref this.txtRackMappingCount, dgRackLOTMapping.GetRowCount());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnRackMapping_Click(object sender, RoutedEventArgs e)
        {
            if (this.dgRackLOTMapping == null || this.dgRackLOTMapping.GetRowCount() <= 0)
            {
                return;
            }

            var query = DataTableConverter.Convert(this.dgRackLOTMapping.ItemsSource).AsEnumerable().Where(x => x.Field<bool>("CHK"));

            if (this.dgRackLOTMapping == null || query.Count() <= 0)
            {
                return;
            }

            if (query.Count() > this.maxRackMappingLOTCount)
            {
                Util.MessageValidation("SFU8217", this.maxRackMappingLOTCount);   // 최대 [%1]개 까지 등록 가능 합니다.
                return;
            }

            if (int.Parse(txtMaxLoadQty.Text) < query.Count() + int.Parse(this.txtLoadQty.Text)) // 최대적재수량 < 적재된 수량 + 기적재수량
            {
                Util.MessageValidation("SFU1500");  // 수량을 초과하였습니다.
                return;
            }

            // 매핑하시겠습니까?
            Util.MessageInfo("10027", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    this.RackMapping();
                }
            });
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3440"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        this.RackMappingAreaInitialize();
                        Util.MessageInfo("SFU3377");        // 작업이 초기화 됐습니다.
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            try
            {
                C1.WPF.DataGrid.DataGridRow dataGridRow = new C1.WPF.DataGrid.DataGridRow();
                System.Collections.Generic.IList<System.Windows.FrameworkElement> ilist = button.GetAllParents();
                foreach (var item in ilist)
                {
                    C1.WPF.DataGrid.DataGridRowPresenter dataGridRowPresenter = item as C1.WPF.DataGrid.DataGridRowPresenter;
                    if (dataGridRowPresenter != null)
                    {
                        dataGridRow = dataGridRowPresenter.Row;
                    }
                }

                DataRowView dataRowView = dataGridRow.DataItem as DataRowView;
                this.GetRackMapping(dataRowView["RACKID"].ToString());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtScanID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(this.cboWHIDRackMapping.SelectedValue.ToString()))
                {
                    Util.MessageInfo("SFU2961", (action) => { cboRackIDRackMapping.Focus(); }); // 창고를 먼저 선택해 주세요.
                    return;
                }

                if (string.IsNullOrEmpty(this.cboRackIDRackMapping.SelectedValue.ToString()))
                {
                    Util.MessageInfo("SFU4136", (action) => { cboRackIDRackMapping.Focus(); }); // 저장위치를 선택해 주세요.
                    return;
                }

                if (int.Parse(sccanRackMaxLoadQty) <= this.dgRackLOTMapping.GetRowCount() + int.Parse(this.txtLoadQty.Text))
                {
                    Util.MessageValidation("SFU1500");
                    return;
                }

                this.dtValidationInvalidResult.Clear();
                this.dtValidationResult.Clear();
                this.ValidationScanID2(this.txtScanID.Text);
                this.DataBinding();
                if (CommonVerify.HasTableRow(this.dtValidationInvalidResult))
                {
                    PackCommon.Show_EXCEPTION_POPUP(this.dtValidationInvalidResult, this.GetType().Name, FrameOperation);
                }
                this.txtScanID.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex, (action) => { txtScanID.Focus(); });
            }
        }

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key != Key.Enter)
                {
                    return;
                }

                string LOTID = string.IsNullOrEmpty(Util.GetCondition(txtLOTID)) ? null : Util.GetCondition(txtLOTID);
                this.SearchProcess(LOTID);
                this.txtLOTID.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtScanID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Key.Equals(Key.V) || !Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                return;
            }

            if (string.IsNullOrEmpty(Clipboard.GetText()))
            {
                return;
            }

            if (string.IsNullOrEmpty(this.cboWHIDRackMapping.SelectedValue.ToString()))
            {
                Util.MessageInfo("SFU2961", (action) => { cboRackIDRackMapping.Focus(); }); // 창고를 먼저 선택해 주세요.
                return;
            }

            if (string.IsNullOrEmpty(this.cboRackIDRackMapping.SelectedValue.ToString()))
            {
                Util.MessageInfo("SFU4136", (action) => { cboRackIDRackMapping.Focus(); }); // 저장위치를 선택해 주세요.
                return;
            }

            try
            {
                List<string> lstScanID = Clipboard.GetText().TrimEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim()).ToList<string>();
                if (this.maxCopyAndPasteLOTCount > 0 && (lstScanID.Count() > this.maxCopyAndPasteLOTCount))
                {
                    Util.MessageValidation("SFU8217", this.maxCopyAndPasteLOTCount);   // 최대 [%1]개 까지 등록 가능 합니다.
                    e.Handled = true;
                    return;
                }

                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.DoEvents();
                this.dtValidationInvalidResult.Clear();
                this.dtValidationResult.Clear();
                foreach (string scanID in lstScanID)
                {
                    if (string.IsNullOrEmpty(scanID))
                    {
                        continue;
                    }

                    if (int.Parse(sccanRackMaxLoadQty) <= this.dgRackLOTMapping.GetRowCount() + int.Parse(this.txtLoadQty.Text))
                    {
                        Util.MessageValidation("SFU1500"); // 수량을 초과하였습니다.
                        return;
                    }
                    this.ValidationScanID2(scanID);
                }

                this.DataBinding();

                if (CommonVerify.HasTableRow(this.dtValidationInvalidResult))
                {
                    PackCommon.Show_EXCEPTION_POPUP(this.dtValidationInvalidResult, this.GetType().Name, FrameOperation);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Hidden;
            }

            e.Handled = true;
        }

        private void txtLOTID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Key.Equals(Key.V) || !Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                return;
            }

            if (string.IsNullOrEmpty(Clipboard.GetText()))
            {
                return;
            }

            string LOTIDList = Clipboard.GetText().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList<string>().Where(x => !string.IsNullOrEmpty(x)).Aggregate((current, next) => current + "," + next);
            Clipboard.Clear();

            this.SearchProcess(LOTIDList);
            this.txtLOTID.Text = string.Empty;
            e.Handled = true;
        }


        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            this.GridCheckBoxHeaderClick((CheckBox)sender, true);
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            this.GridCheckBoxHeaderClick((CheckBox)sender, false);
        }

        private void cboRackID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (this.cboRackIDRackMapping.SelectedValue == null || string.IsNullOrEmpty(this.cboRackIDRackMapping.SelectedValue.ToString()))
                {
                    this.txtMaxLoadQty.Text = string.Empty;
                    this.txtLoadQty.Text = string.Empty;
                    return;
                }
                this.SetRackInfo();

                if (this.dgRackLOTMapping.GetRowCount() > 0)
                {
                    Util.gridClear(this.dgRackLOTMapping);
                }

                PackCommon.SearchRowCount(ref this.txtRackMappingCount, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboWareHouseIDRackMapping_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string wareHouseID = e.NewValue.ToString();
            DataTable dt = this.dataHelper.GetRackInfo(LoginInfo.CFG_AREA_ID, wareHouseID);
            // 콤보박스 설정
            PackCommon.SetC1ComboBox(dt, this.cboRackIDRackMapping, true, "-SELECT-");

            if (this.dgRackLOTMapping.GetRowCount() > 0)
            {
                Util.gridClear(this.dgRackLOTMapping);
            }
            PackCommon.SearchRowCount(ref this.txtRackMappingCount, 0);
        }

        private void cboWareHouseIDRackMappingHistory_SelectedValueChanged(object sender, EventArgs e)
        {
            MultiSelectionBox multiSelectionBox = (MultiSelectionBox)sender;

            string areaID = LoginInfo.CFG_AREA_ID;
            //string wareHouseID = e.NewValue.ToString();
            string wareHouseID = multiSelectionBox.SelectedItemsToString;
            this.SetMultiSelectionComboBox(this.dataHelper.GetRackInfo(areaID, wareHouseID), this.cboMultiRackIDRackMappingHistory, ref this.rackIDMultiComboBindDataCount);
        }

        private void btnRackUnmappingReset_Click(object sender, RoutedEventArgs e)
        {
            // 매핑하시겠습니까?
            Util.MessageInfo("SFU4525", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    this.RackUnMapping();
                }
            });
        }

        private void dgRackMappingDetail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid c1DataGrid = (C1DataGrid)sender;
                c1DataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    else if (e.Cell.Column.Name == "STORAGE_DATE")
                    {
                        string storageDate = Util.NVC(DataTableConverter.GetValue(c1DataGrid.Rows[e.Cell.Row.Index].DataItem, "STORAGE_DATE"));
                        if (string.IsNullOrEmpty(storageDate))
                        {
                            return;
                        }
                        double temp = 0.0;
                        double.TryParse(storageDate, out temp);

                        if (temp <= 3)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }
                        else if (temp > 3 && temp <= 7)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgGrid1CheckBoxColumn_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (this.dgRackMappingSummary.CurrentColumn == null || this.dgRackMappingSummary.CurrentRow == null)
                {
                    return;
                }
                if (!this.dgRackMappingSummary.CurrentColumn.Name.ToUpper().Equals("CHK"))
                {
                    return;
                }
                if (this.dgRackMappingSummary.GetRowCount() <= 0)
                {
                    return;
                }
                //int currentRowIndex = this.dgRackMappingSummary.CurrentRow.Index;

                //체크값이 True인 경우 다른 Row의 있는 내용은 False
                //for (int i = 0; i < this.dgRackMappingSummary.GetRowCount(); i++)
                //{
                //    if (!i.Equals(currentRowIndex))
                //    {
                //        DataTableConverter.SetValue(this.dgRackMappingSummary.Rows[i].DataItem, "CHK", false);
                //    }
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgRackMappingSummary_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid c1DataGrid = (C1DataGrid)sender;
            DataRowView dataRowView = (DataRowView)e.Cell.Row.DataItem;

            // Detail 조회시에 Check 표시가 해제되었다던가 신규 Row인 경우에는 조회안하게 함.
            if (//!dataRowView.Row["CHK"].SafeToBoolean() ||
                dataRowView.Row.RowState.Equals(DataRowState.Detached) ||
                dataRowView.Row.RowState == DataRowState.Added)
            {
                PackCommon.SearchRowCount(ref this.txtRackmappingDetailCount, 0);
                return;
            }
            if (!dataRowView.Row["CHK"].SafeToBoolean())
            {
                this.delRackMapping(dataRowView["RACKID"].ToString());
                return;
            }
            // Detail 조회
            this.GetRackMapping(dataRowView["RACKID"].ToString());
        }
        #endregion
    }

    internal class PACK001_067_DataHelper
    {
        #region Member Variable Lists...
        #endregion

        #region Constructor
        internal PACK001_067_DataHelper()
        {

        }
        #endregion

        #region Member Function Lists...
        // 순서도 호출 - CommonCode Info
        internal DataTable GetCommonCodeInfo(string cmcdType, string cmCode)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTE";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                dtRQSTDT.Columns.Add("CBO_CODE", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CMCDTYPE"] = cmcdType;
                drRQSTDT["CBO_CODE"] = cmCode;
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

        // 순서도 호출 - 창고 정보
        internal DataTable GetWareHouseInfo(string areaID)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_WHID_CBO_PACK";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("WH_NAME", typeof(string));
                dtRQSTDT.Columns.Add("AUTO_WH_FLAG", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["WH_NAME"] = null;
                drRQSTDT["AUTO_WH_FLAG"] = "N";

                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    // 공산당 팩2동 MEB Partial ILT 창고 제외
                    dtReturn = dtRSLTDT.AsEnumerable().Where(x => !x.Field<string>("CBO_NAME").Contains("MEB Partial ILT")).CopyToDataTable();
                }
                else
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

        // 순서도 호출 - Rack 정보
        internal DataTable GetRackInfo(string areaID, string wareHouseID)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_PACK_RACK_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("WHID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["WHID"] = wareHouseID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                //if (CommonVerify.HasTableRow(dtRSLTDT))
                //{
                //    dtReturn = dtRSLTDT;
                //}
                //else
                //{
                //    return dtReturn;
                //}
                dtReturn = dtRSLTDT;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - Rack 정보
        internal DataTable GetRackMappinngInfo(string areaID, string wareHouseID, string rackID, string LOTID)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_PRD_SEL_RACK_HISTORY";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("WHID", typeof(string));
                dtRQSTDT.Columns.Add("RACKID", typeof(string));
                dtRQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["WHID"] = string.IsNullOrEmpty(wareHouseID) ? null : wareHouseID;
                drRQSTDT["RACKID"] = string.IsNullOrEmpty(rackID) ? null : rackID;
                drRQSTDT["LOTID"] = string.IsNullOrEmpty(LOTID) ? null : LOTID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                //if (CommonVerify.HasTableRow(dtRSLTDT))
                //{
                //    dtReturn = dtRSLTDT;
                //}
                dtReturn = dtRSLTDT;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - LotID 조회
        internal DataTable GetLotIDMappingInfo(string areaID, string wareHouseID, string rackID, string LOTID)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_PRD_SEL_RACK_SEARCH_BY_LOTID";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("WH_ID", typeof(string));
                dtRQSTDT.Columns.Add("RACK_ID", typeof(string));
                dtRQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["WH_ID"] = string.IsNullOrEmpty(wareHouseID) ? null : wareHouseID;
                drRQSTDT["RACK_ID"] = string.IsNullOrEmpty(rackID) ? null : rackID;
                drRQSTDT["LOTID"] = LOTID;
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

        // 순서도 호출 - Rack vs Lot Mapping 조회
        internal DataTable GetRackLOTMappingInfo(string rackID)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_PRD_SEL_RACK_SEARCH";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("WH_ID", typeof(string));
                dtRQSTDT.Columns.Add("RACK_ID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = null;
                drRQSTDT["WH_ID"] = null;
                drRQSTDT["RACK_ID"] = rackID;
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

        // 순서도 호출 - Rack Mapping or Unmapping
        internal bool SetRackLotMapping(DataTable dt)
        {
            // Declarations...
            bool returnValue = true;
            string bizRuleName = "BR_PRD_REG_RACK_LOT_MAPPING_PACK";
            DataSet dsINDATA = new DataSet();

            try
            {
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("SHOPID", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("MAPPING_FLAG", typeof(string));
                dtINDATA.Columns.Add("LOTID", typeof(string));
                dtINDATA.Columns.Add("WH_ID", typeof(string));
                dtINDATA.Columns.Add("RACK_ID", typeof(string));
                dtINDATA.Columns.Add("USERID", typeof(string));
                dtINDATA.Columns.Add("NOTE", typeof(string));

                foreach (DataRow dr in dt.Rows)
                {
                    DataRow drINDATA = dtINDATA.NewRow();
                    drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    drINDATA["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
                    drINDATA["MAPPING_FLAG"] = dr["MAPPING_FLAG"].ToString();
                    drINDATA["LOTID"] = dr["LOTID"].ToString();
                    drINDATA["WH_ID"] = dr["WH_ID"].ToString();
                    drINDATA["RACK_ID"] = dr["RACK_ID"].ToString();
                    drINDATA["USERID"] = LoginInfo.USERID;
                    drINDATA["NOTE"] = dr["NOTE"].ToString();
                    dtINDATA.Rows.Add(drINDATA);
                }

                dsINDATA.Tables.Add(dtINDATA);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, null, dsINDATA);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnValue = false;
            }

            return returnValue;
        }

        // 순서도 호출 - Rack Mapping시 Scan ID Check
        internal DataSet GetScanIDInfo(string scanID)
        {
            string bizRuleName = "BR_PRD_CHK_BOXLOT_RACK";
            string outDataTableNameList = "OUTDATA,OUTDATA_INVALID";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            try
            {

                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("SCANID", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
                drINDATA["SCANID"] = scanID;

                dtINDATA.Rows.Add(drINDATA);
                dsINDATA.Tables.Add(dtINDATA);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, outDataTableNameList, dsINDATA, null);
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