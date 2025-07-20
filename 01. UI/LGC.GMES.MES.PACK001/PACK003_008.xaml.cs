/*************************************************************************************
 Created Date : 2020-10-17
      Creator : 김길용
  Description : 자동물류 Cell 반품승인 요청
--------------------------------------------------------------------------------------
 [Change History]
   Date         Author      CSR         Description...
   2020.10.19   정용석       SI          Create
   2021.03.04   김길용       SI          조회 그리드에서 팝업 조건 수정
   2021.03.05   김길용       SI          현진행상태 Multi Box 추가 및 수정
   2021.03.12   김길용       SI          RCV_ISS의 CARRIERID 또는 PALLETID로도 반품승인요청 가능하게 수정, 상세팝업 변경 (PACK003_008_RETURN_PALLETINFO), 조회 비즈 수정(BR_PRD_CHK_LOGIS_PALLET_MAPPING_V2)
**************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
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
    public partial class PACK003_008 : UserControl, IWorkArea
    {
        #region [ Member Variable Lists ]
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        private static string REQUEST = "REQ";
        private static string CANCEL = "CANCEL";
        #endregion

        #region [ Initialize ]
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK003_008()
        {
            InitializeComponent();
        }
        #endregion

        #region [ Global variable ]
        #endregion

        #region [ Member Function Lists ]
        // UserControl_Loaded Event 발생시
        private void InitializeControl()
        {
            this.dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);
            this.dtpDateTo.SelectedDateTime = DateTime.Now;

            // 권한
            List<Button> listAuth = new List<Button>();
            listAuth.Add(this.btnApprReq);
            listAuth.Add(this.btnApprCancel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        // Carrier와 재구성된 Pallet 매핑정보 그리드에 추가하기
        private void AddPalletMappingData()
        {
            if (string.IsNullOrEmpty(this.txtCSTID.Text))
            {
                return;
            }
            List<string> lstCarrierID = new List<string>();
            lstCarrierID.Add(txtCSTID.Text);
            DataTable dt = this.SearchPalletMappingData(lstCarrierID);
            this.PalletMappingGridDataBinding(dt);
        }

        // Carrier와 재구성된 Pallet 매핑정보 그리드에 추가하기
        private void PalletMappingGridDataBinding(DataTable dt)
        {
            // Declarations...
            DataTable dtPalletMapping = DataTableConverter.Convert(this.dgPalletMapping.ItemsSource);

            if (dtPalletMapping == null || dtPalletMapping.Rows.Count <= 0)
            {
                // 처음 추가되는 거라면.
                dtPalletMapping = dt.Copy();
                Util.gridClear(this.dgPalletMapping);
                Util.GridSetData(this.dgPalletMapping, dtPalletMapping, FrameOperation);
            }
            else
            {
                // 기존 Data가 존재한다면, 중복 데이터가 들어가 있는가 체크
                foreach (DataRow dr in dt.Select())
                {
                    var result = dtPalletMapping.AsEnumerable().Where(x => x.Field<string>("PLTID").Equals(dr["PLTID"].ToString()) &&
                                                                           x.Field<string>("CSTID").Equals(dr["CSTID"].ToString()));
                    if (result.Count() <= 0)
                    {
                        dtPalletMapping.ImportRow(dr);
                    }
                }
                dtPalletMapping.AcceptChanges();
                Util.gridClear(this.dgPalletMapping);
                Util.GridSetData(this.dgPalletMapping, dtPalletMapping, FrameOperation);
            }
            // 건수표시
            this.txtPalletCnt.Text = "[ " + dtPalletMapping.Rows.Count.ToString() + " 건 ]";
            if (dt.Rows.Count == 0)
            {
                return;
            }
            // 제품 ID, 출하창고 가져오기
            if (string.IsNullOrEmpty(this.txtProdID.Text))
            {
               
                this.txtProdID.Text = dt.Rows[0]["PRODID"].ToString();
            }

            if (string.IsNullOrEmpty(this.txtFromBLDG.Text))
            {
                this.txtFromBLDG.Text = dt.Rows[0]["FROM_BLDG_CODE"].ToString();
            }

            this.txtCSTID.Clear();
            this.txtCSTID.Focus();
        }

        // Carrier와 재구성된 Pallet 매핑정보 조회하기
        private DataTable SearchPalletMappingData(List<string> lstCarrierID)
        {
            // Declarations...
            DataTable dtReturn = new DataTable();
            string bizRuleName = "BR_PRD_CHK_LOGIS_PALLET_MAPPING_V2"; // 이전버전 - "BR_PRD_CHK_LOGIS_PALLET_MAPPING"; 
            try
            {
                DataSet ds = new DataSet();
                ds.Tables.Add(this.SetPalletMappingMasterData(lstCarrierID));
                ds.Tables.Add(this.SetPalletMappingProductData());

                string inDataTableNameList = string.Join(",", ds.Tables.OfType<DataTable>().Select(dt => dt.TableName).ToList());
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, "OUTDATA", ds);
                if (dsResult == null && dsResult.Tables["OUTDATA"].Rows.Count == 0)
                {
                    return dtReturn;
                }
                if (dsResult != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    dtReturn = dsResult.Tables["OUTDATA"].Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // Carrier와 재구성된 Pallet 매핑정보 조회하기 입력데이터 만들기 1호
        private DataTable SetPalletMappingMasterData(List<string> lstCarrierID)
        {
            // Definition Schema of DataTable
            DataTable dt = new DataTable();
            dt.TableName = "INDATA";
            dt.Columns.Add("SRCTYPE", typeof(string));
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("CSTID", typeof(string));
            dt.Columns.Add("INPUT_ID_FLAG", typeof(string));

            // Insert Data
            foreach (string carrierID in lstCarrierID)
            {
                DataRow dr = dt.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = carrierID;
                dt.Rows.Add(dr);
            }

            dt.AcceptChanges();     // Apply
            return dt;
        }

        // Carrier와 재구성된 Pallet 매핑정보 조회하기 입력데이터 만들기 2호
        private DataTable SetPalletMappingProductData()
        {
            // Definition Schema of DataTable
            DataTable dt = new DataTable();
            dt.TableName = "INMAPPING";
            dt.Columns.Add("PRODID", typeof(string));
            dt.Columns.Add("FROM_BLDG_CODE", typeof(string));

            // Insert Data
            if (!string.IsNullOrEmpty(this.txtProdID.Text) && !string.IsNullOrEmpty(this.txtFromBLDG.Text))
            {
              DataRow dr = dt.NewRow();
              dr["PRODID"] = this.txtProdID.Text == "" ? null : this.txtProdID.Text;
              dr["FROM_BLDG_CODE"] = this.txtFromBLDG.Text == "" ? null : this.txtFromBLDG.Text;
              dt.Rows.Add(dr);
            }

            dt.AcceptChanges();     // Apply
            return dt;
        }

        // Carrier와 재구성된 Pallet 매핑정보 그리드 전체 삭제하기
        private void ClearApprovalData()
        {
            this.txtCSTID.Text = string.Empty;
            this.txtApprReqNote.Text = string.Empty;
            this.txtFile.Text = string.Empty;
            this.txtProdID.Text = string.Empty;
            this.txtFromBLDG.Text = string.Empty;
            Util.gridClear(this.dgPalletMapping);
            // 건수표시
            this.txtPalletCnt.Text = "[ 0 건 ]";
        }

        // Carrier와 재구성된 Pallet 매핑정보 그리드 내용중에 선택된거만 삭제하기
        private void UncheckApprovalData()
        {
            DataTable dt = DataTableConverter.Convert(this.dgPalletMapping.ItemsSource);
            dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE") || x.Field<string>("CHK").ToUpper().Equals("1")).ToList().ForEach(r => r.Delete());
            dt.AcceptChanges();

            if (dt.Rows.Count <= 0)
            {
                this.ClearApprovalData();
                return;
            }
            Util.gridClear(this.dgPalletMapping);
            Util.GridSetData(this.dgPalletMapping, dt, FrameOperation);
            // 건수표시
            this.txtPalletCnt.Text = "[ " + dt.Rows.Count.ToString() + " 건 ]";
        }

        // Excel 파일에 저장된 데이터를 Import하여 Carrier와 재구성된 Pallet 매핑정보 그리드에 추가하기
        private void ImportDataByExcel()
        {
            List<string> lstCarrierID = new List<string>();

            try
            {
                DataTable dtExcelData = new DataTable();
                OpenFileDialog fileDialog = new OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"].ToString().Equals("SBC"))
                {
                    fileDialog.InitialDirectory = @"\\Client\C$";
                }
                fileDialog.Filter = "Excel Files (*.xlsx,*.xls)|*.xlsx;*.xls";

                if (fileDialog.ShowDialog() == true)
                {
                    using (Stream stream = fileDialog.OpenFile())
                    {
                        dtExcelData = LoadExcelHelper.LoadExcelData(stream, 0, 0);
                    }
                }

                if (dtExcelData != null && dtExcelData.Rows.Count > 0)
                {
                    foreach (DataRow dr in dtExcelData.Rows)
                    {
                        lstCarrierID.Add(dr[0].ToString());
                    }

                    DataTable dt = this.SearchPalletMappingData(lstCarrierID);
                    this.PalletMappingGridDataBinding(dt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 반품 승인 요청 Transaction
        private void ApprovalRequest()
        {
            try
            {
                if (!this.ValidationCheck(REQUEST))
                {
                    return;
                }

                if (this.ApprovalTransaction(REQUEST))
                {
                    this.ClearApprovalData();
                    this.SearchProcess();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 반품 승인 취소 Transaction
        private void ApprovalCancel()
        {
            try
            {
                if (!this.ValidationCheck(CANCEL))
                {
                    return;
                }

                if (this.ApprovalTransaction(CANCEL))
                {
                    this.SearchProcess();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 반품 승인 요청 또는 취소 Transaction
        private bool ApprovalTransaction(string requestType)
        {
            // Declarations...
            bool returnValue = false;
            DataSet ds = new DataSet();
            ds.Tables.Add(this.SetApprovalMasterData(requestType));
            ds.Tables.Add(this.SetApprovalDetailData(requestType));

            string inDataTableNameList = string.Join(",", ds.Tables.OfType<DataTable>().Select(dt => dt.TableName).ToList());
            DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_LOGIS_APPR_REQUEST", inDataTableNameList, string.Empty, ds, null);

            if (dsResult != null)
            {
                switch (requestType)
                {
                    case "REQ":
                        Util.MessageInfo("SFU1747");        // 요청되었습니다.
                        break;
                    case "CANCEL":
                        Util.MessageInfo("SFU5032");        // 취소되었습니다.
                        break;
                    default:
                        break;
                }

                returnValue = true;
            }

            return returnValue;
        }

        // 반품 승인 요청 또는 취소 Transaction Validation Check
        private bool ValidationCheck(string requestType)
        {
            bool returnValue = false;

            switch (requestType.ToUpper())
            {
                case "REQ":
                    returnValue = this.ValidationCheckApprovalRequest();
                    break;
                case "CANCEL":
                    returnValue = this.ValidationCheckApprovalCancel();
                    break;
                default:
                    break;
            }

            return returnValue;
        }

        // 반품 승인 요청 Validation
        private bool ValidationCheckApprovalRequest()
        {
            if (string.IsNullOrEmpty(this.txtApprReqNote.Text))
            {
                Util.MessageValidation("SFU1554");  // 반품사유를 입력하세요
                this.txtApprReqNote.Focus();
                return false;
            }

            DataTable dt = DataTableConverter.Convert(this.dgPalletMapping.ItemsSource);
            if (dt.Rows.Count <= 0)
            {
                Util.MessageValidation("SFU1411");  // PALLETID를 입력해주세요
                return false;
            }

            var query = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE"));
            if (query.Count() <= 0)
            {
                Util.MessageValidation("SFU1654");  // 선택된 요청이 없습니다.
                return false;
            }

            if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
            {
                Util.Alert("SFU4591"); //작업자를 입력하세요
                this.ucPersonInfo.Focus();
                return false;
            }

            return true;
        }

        // 반품 승인 취소 Validation
        private bool ValidationCheckApprovalCancel()
        {
            if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
            {
                Util.Alert("SFU4591"); //작업자를 입력하세요
                this.ucPersonInfo.Focus();
                return false;
            }

            DataTable dt = DataTableConverter.Convert(this.dgApprHistory.ItemsSource);
            var query = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE"));

            if (query.Count() <= 0)
            {
                Util.MessageValidation("SFU1654");  // 선택된 요청이 없습니다.
                return false;
            }

            return true;
        }

        // 반품 승인 요청 또는 취소 입력 데이터 만들기 1호
        private DataTable SetApprovalMasterData(string requestType)
        {
            // Definition Schema of DataTable
            DataTable dt = new DataTable();
            dt.TableName = "INDATA";
            dt.Columns.Add("SRCTYPE", typeof(string));
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("PRODID", typeof(string));
            dt.Columns.Add("RESNCODE", typeof(string));
            dt.Columns.Add("REQ_NOTE", typeof(string));
            dt.Columns.Add("INSUSER", typeof(string));
            dt.Columns.Add("REQTYPE", typeof(string));
            dt.Columns.Add("UPDUSER", typeof(string));

            // Insert Data
            if (requestType.Equals(REQUEST))
            {
                DataTable dtRequestData = DataTableConverter.Convert(this.dgPalletMapping.ItemsSource);
                var query = from d1 in dtRequestData.AsEnumerable()
                            where d1.Field<string>("CHK").ToUpper().Equals("TRUE") || d1.Field<string>("CHK").ToUpper().Equals("1")
                            group d1 by 1 into grp
                            select new
                            {
                                //TRF_LOT_QTY = grp.Max(x => x.Field<string>("PLT_LOT_QTY"))
                            };

                foreach (var item in query)
                {
                    DataRow dr = dt.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["PRODID"] = this.txtProdID.Text;
                    dr["RESNCODE"] = string.Empty;
                    dr["REQ_NOTE"] = this.txtApprReqNote.Text;
                    dr["INSUSER"] = this.ucPersonInfo.UserID;
                    dr["REQTYPE"] = requestType;
                    dr["UPDUSER"] = this.ucPersonInfo.UserID;
                    dt.Rows.Add(dr);
                }
            }
            else if (requestType.Equals(CANCEL))
            {
                DataRow dr = dt.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = string.Empty;
                dr["PRODID"] = string.Empty;
                dr["RESNCODE"] = string.Empty;
                dr["REQ_NOTE"] = string.Empty;
                dr["INSUSER"] = this.ucPersonInfo.UserID;
                dr["REQTYPE"] = requestType;
                dr["UPDUSER"] = this.ucPersonInfo.UserID;
                dt.Rows.Add(dr);
            }

            dt.AcceptChanges();     // Apply
            return dt;
        }

        // 반품 승인 요청 또는 취소 입력 데이터 만들기 2호
        private DataTable SetApprovalDetailData(string requestType)
        {
            // Definition Schema of DataTable
            DataTable dt = new DataTable();
            dt.TableName = "INCST";
            dt.Columns.Add("TRF_REQ_NO", typeof(string));
            dt.Columns.Add("CSTID", typeof(string));
            dt.Columns.Add("PLTID", typeof(string));
            dt.Columns.Add("PLT_LOT_QTY", typeof(string));
            dt.Columns.Add("SLOCID", typeof(string));
            dt.Columns.Add("INPUT_ID_FLAG", typeof(string));

            // Insert Data
            if (requestType.Equals(REQUEST))
            {
                DataTable dtData = DataTableConverter.Convert(this.dgPalletMapping.ItemsSource);
                foreach (DataRow drData in dtData.Select())
                {
                    if (drData["CHK"].ToString().Equals("True"))
                    {
                        DataRow dr = dt.NewRow();
                        dr["CSTID"] = drData["CSTID"];
                        dr["PLTID"] = drData["PLTID"];
                        dr["PLT_LOT_QTY"] = drData["PLT_LOT_QTY"];
                        dr["SLOCID"] = drData["FROM_BLDG_CODE"];
                        //dr["INPUT_ID_FLAG"] = sInput_Flag;
                        dt.Rows.Add(dr);
                    }
                }
            }
            else if (requestType.Equals(CANCEL))
            {
                DataTable dtData = DataTableConverter.Convert(this.dgApprHistory.ItemsSource);
                foreach (DataRow drData in dtData.Select())
                {
                    if (drData["CHK"].ToString().Equals("True"))
                    {
                        DataRow dr = dt.NewRow();
                        dr["TRF_REQ_NO"] = drData["TRF_REQ_NO"];
                        dr["CSTID"] = drData["CSTID"];
                        dr["PLTID"] = drData["PLLT_ID"];
                        dr["PLT_LOT_QTY"] = drData["PLLT_LOT_QTY"];
                        dr["SLOCID"] = drData["RETURN_WAREHOUSE"];
                        //dr["INPUT_ID_FLAG"] = sInput_Flag;
                        dt.Rows.Add(dr);
                    }
                }
            }
            dt.AcceptChanges();     // Apply
            return dt;
        }

        // 반품 요청 내역 조회
        private void SearchProcess()
        {
            try
            {
                Util.gridClear(this.dgApprHistory);
                this.txRightRowCnt.Text = "[ 0 건 ]";
                DataTable dtReturn = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOGIS_APPR_REQ_LIST", "INDATA", "OUTDATA", this.SetApprHistoryData());

                if (dtReturn == null || dtReturn.Rows.Count <= 0)
                {
                    Util.Alert("101471");  // 조회된 결과가 없습니다.
                    return;
                }
                this.txRightRowCnt.Text = "[ " + dtReturn.Rows.Count.ToString() + " 건 ]";
                // Data Binding
                Util.GridSetData(this.dgApprHistory, dtReturn, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        // 반품 요청 내역 조회 입력 데이터 만들기
        private DataTable SetApprHistoryData()
        {
            // Definition Schema of DataTable
            DataTable dt = new DataTable();
            dt.TableName = "RQSTDT";
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("REQ_FROM_DATE", typeof(DateTime));
            dt.Columns.Add("REQ_TO_DATE", typeof(DateTime));
            dt.Columns.Add("CSTID", typeof(string));
            dt.Columns.Add("TRF_REQ_STAT_CODE", typeof(string));

            // Insert Data
            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["REQ_FROM_DATE"] = DateTime.ParseExact(this.dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"), "yyyyMMdd", null);
            dr["REQ_TO_DATE"] = DateTime.ParseExact(this.dtpDateTo.SelectedDateTime.AddDays(1).ToString("yyyyMMdd"), "yyyyMMdd", null);
            dr["CSTID"] = string.IsNullOrEmpty(this.txtPLTID2.Text) ? null : this.txtPLTID2.Text;
            dr["TRF_REQ_STAT_CODE"] = Convert.ToString(this.cboStat.SelectedItemsToString) == "" ? null : Convert.ToString(this.cboStat.SelectedItemsToString);

            dt.Rows.Add(dr);

            dt.AcceptChanges();     // Apply
            return dt;
        }

        // Cell Pallet Mapping Data 조회 PopUp 호출
        private void ShowPalletCellMappingPopup(Point point, C1DataGrid c1DataGrid)
        {
            try
            {
                C1.WPF.DataGrid.DataGridCell dataGridCell = c1DataGrid.GetCellFromPoint(point);
                if (dataGridCell == null)
                {
                    return;
                }

                DataRowView dataRowView = this.dgApprHistory.CurrentRow.DataItem as DataRowView;

                if (c1DataGrid.GetRowCount() <= 0 || dataGridCell.Row.Index < 0)
                {
                    return;
                }
                if (dataGridCell.Column.Index == 0)
                {
                    return;
                }
                if (dataRowView == null || dataRowView.Row.ItemArray.Length <= 0)
                {
                    return;
                }
                if (dataGridCell.Column.Name.Equals("CHK"))
                {
                    return;
                }
                if (!dataGridCell.Column.Name.Equals("CSTID") && !dataGridCell.Column.Name.Equals("PLLT_ID"))
                {
                    return;
                }

                // OpenPopup
                PACK003_008_RETURN_PALLETINFO popup = new PACK003_008_RETURN_PALLETINFO();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = dataRowView.Row["CSTID"].ToString();
                    Parameters[1] = dataRowView.Row["PLLT_ID"].ToString();
                    Parameters[2] = dataRowView.Row["TRF_REQ_NO"].ToString();

                    C1WindowExtension.SetParameters(popup, Parameters);
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 같은 반품승인번호를 가지고 있는 모든 Row Select
        private void SelectSameApprNo(object sender)
        {
            if (sender == null)
            {
                return;
            }
            try
            {
                CheckBox checkBox = sender as CheckBox;
                DataTable dt = DataTableConverter.Convert(this.dgApprHistory.ItemsSource);
                string selectedApprReqNo = DataTableConverter.GetValue(checkBox.DataContext, "TRF_REQ_NO").ToString();
                var query = dt.AsEnumerable().Cast<DataRow>().Select((x, i) => new { ROW_NUMBER = i++, TRF_REQ_NO = x.Field<string>("TRF_REQ_NO") }).Where(x => x.TRF_REQ_NO.Equals(selectedApprReqNo));
                foreach (var item in query)
                {
                    DataTableConverter.SetValue(this.dgApprHistory.Rows[item.ROW_NUMBER].DataItem, "CHK", checkBox.IsChecked);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 반품 요청 내역 Excel Download
        private void ExcelDownload()
        {
            DataTable dt = DataTableConverter.Convert(this.dgApprHistory.ItemsSource);
            if (dt == null || dt.Rows.Count <= 0)
            {
                Util.MessageValidation("SFU3553");        // Excel 저장할 데이터가 없습니다..
                return;
            }

            try
            {
                ExcelExporter excelExporter = new ExcelExporter();
                C1DataGrid[] dataGridArr = new C1DataGrid[] { this.dgApprHistory };
                string[] tabNameArr = { "반품요청내역" };
                excelExporter.Export(dataGridArr, tabNameArr);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [ Event ]
        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.InitializeControl();
            InitCombo();
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnApprReq);
            listAuth.Add(btnApprCancel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        private void InitCombo()
        {
            //현상태 멀티콤보
            this.cboStat.isAllUsed = true;
            cboStat.ApplyTemplate();
            this.SetMultiSelectionBoxRequestStatus(this.cboStat);
        }
        private void SetMultiSelectionBoxRequestStatus(MultiSelectionBox cboMulti)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                //RQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_LOGIS_TRF_REQ_STAT_CODE";
                //dr["ATTRIBUTE2"] = "B";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ATTR_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                
                var query = dtResult.AsEnumerable().Select(x => new
                {
                    CBO_CODE = x.Field<string>("CBO_CODE"),
                    CBO_NAME = x.Field<string>("CBO_NAME"),
                }).Where(x => x.CBO_CODE.Equals("CANCELLED_LOGIS") || x.CBO_CODE.Equals("CONFIRMED_LOGIS") || x.CBO_CODE.Equals("REJECTED_LOGIS") || x.CBO_CODE.Equals("REQUEST_LOGIS")
                ).ToList();

                DataTable dtQuery = new DataTable();
                dtQuery.Columns.Add("CBO_CODE", typeof(string));
                dtQuery.Columns.Add("CBO_NAME", typeof(string));
                
                foreach (var item in query)
                {
                    DataRow drIndata = dtQuery.NewRow();
                    drIndata["CBO_CODE"] = item.CBO_CODE;
                    drIndata["CBO_NAME"] = item.CBO_NAME;
                    dtQuery.Rows.Add(drIndata);
                }
                if (dtQuery.Rows.Count != 0)
                {
                    cboMulti.ItemsSource = DataTableConverter.Convert(dtQuery);
                    for (int i = 0; i < dtQuery.Rows.Count; i++)
                    {
                        if ("CANCELLED_LOGIS,REQUEST_LOGIS".Contains(dtQuery.Rows[i]["CBO_CODE"].ToString()))
                        {
                            cboMulti.Check(i);
                        }
                        else
                        {
                            cboMulti.Uncheck(i);
                        }
                    }
                }
                else
                {
                    cboMulti.ItemsSource = null;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void txtCSTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Return))
            {
                this.AddPalletMappingData();
            }
        }

        private void btnInitialize_Click(object sender, RoutedEventArgs e)
        {
            this.ClearApprovalData();
        }

        private void btnSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            this.UncheckApprovalData();
        }

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            this.ImportDataByExcel();
        }

        private void btnApprReq_Click(object sender, RoutedEventArgs e)
        {
            // 반품 승인 요청을 하시겠습니까?
            Util.MessageConfirm("SFU5101", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    this.ApprovalRequest();
                }
            }
            );
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess();
        }

        private void btnLabel_Click(object sender, RoutedEventArgs e)
        {
            // TO-DO
        }

        private void btnApprCancel_Click(object sender, RoutedEventArgs e)
        {
            // 반품 승인 요청을 하시겠습니까?
            Util.MessageConfirm("SFU5102", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    this.ApprovalCancel();
                }
            }
            );
        }

        private void dgApprHistory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.ShowPalletCellMappingPopup(e.GetPosition(null), (C1DataGrid)sender);
        }

        private void chkApprHistory_Click(object sender, RoutedEventArgs e)
        {
            this.SelectSameApprNo(sender);
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            this.ExcelDownload();
        }
        #endregion
        
    }
}