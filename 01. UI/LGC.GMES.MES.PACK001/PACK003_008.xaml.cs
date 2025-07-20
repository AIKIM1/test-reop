/*************************************************************************************
 Created Date : 2020-10-17
      Creator : ����
  Description : �ڵ����� Cell ��ǰ���� ��û
--------------------------------------------------------------------------------------
 [Change History]
   Date         Author      CSR         Description...
   2020.10.19   ���뼮       SI          Create
   2021.03.04   ����       SI          ��ȸ �׸��忡�� �˾� ���� ����
   2021.03.05   ����       SI          ��������� Multi Box �߰� �� ����
   2021.03.12   ����       SI          RCV_ISS�� CARRIERID �Ǵ� PALLETID�ε� ��ǰ���ο�û �����ϰ� ����, ���˾� ���� (PACK003_008_RETURN_PALLETINFO), ��ȸ ���� ����(BR_PRD_CHK_LOGIS_PALLET_MAPPING_V2)
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
        // UserControl_Loaded Event �߻���
        private void InitializeControl()
        {
            this.dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);
            this.dtpDateTo.SelectedDateTime = DateTime.Now;

            // ����
            List<Button> listAuth = new List<Button>();
            listAuth.Add(this.btnApprReq);
            listAuth.Add(this.btnApprCancel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        // Carrier�� �籸���� Pallet �������� �׸��忡 �߰��ϱ�
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

        // Carrier�� �籸���� Pallet �������� �׸��忡 �߰��ϱ�
        private void PalletMappingGridDataBinding(DataTable dt)
        {
            // Declarations...
            DataTable dtPalletMapping = DataTableConverter.Convert(this.dgPalletMapping.ItemsSource);

            if (dtPalletMapping == null || dtPalletMapping.Rows.Count <= 0)
            {
                // ó�� �߰��Ǵ� �Ŷ��.
                dtPalletMapping = dt.Copy();
                Util.gridClear(this.dgPalletMapping);
                Util.GridSetData(this.dgPalletMapping, dtPalletMapping, FrameOperation);
            }
            else
            {
                // ���� Data�� �����Ѵٸ�, �ߺ� �����Ͱ� �� �ִ°� üũ
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
            // �Ǽ�ǥ��
            this.txtPalletCnt.Text = "[ " + dtPalletMapping.Rows.Count.ToString() + " �� ]";
            if (dt.Rows.Count == 0)
            {
                return;
            }
            // ��ǰ ID, ����â�� ��������
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

        // Carrier�� �籸���� Pallet �������� ��ȸ�ϱ�
        private DataTable SearchPalletMappingData(List<string> lstCarrierID)
        {
            // Declarations...
            DataTable dtReturn = new DataTable();
            string bizRuleName = "BR_PRD_CHK_LOGIS_PALLET_MAPPING_V2"; // �������� - "BR_PRD_CHK_LOGIS_PALLET_MAPPING"; 
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

        // Carrier�� �籸���� Pallet �������� ��ȸ�ϱ� �Էµ����� ����� 1ȣ
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

        // Carrier�� �籸���� Pallet �������� ��ȸ�ϱ� �Էµ����� ����� 2ȣ
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

        // Carrier�� �籸���� Pallet �������� �׸��� ��ü �����ϱ�
        private void ClearApprovalData()
        {
            this.txtCSTID.Text = string.Empty;
            this.txtApprReqNote.Text = string.Empty;
            this.txtFile.Text = string.Empty;
            this.txtProdID.Text = string.Empty;
            this.txtFromBLDG.Text = string.Empty;
            Util.gridClear(this.dgPalletMapping);
            // �Ǽ�ǥ��
            this.txtPalletCnt.Text = "[ 0 �� ]";
        }

        // Carrier�� �籸���� Pallet �������� �׸��� �����߿� ���õȰŸ� �����ϱ�
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
            // �Ǽ�ǥ��
            this.txtPalletCnt.Text = "[ " + dt.Rows.Count.ToString() + " �� ]";
        }

        // Excel ���Ͽ� ����� �����͸� Import�Ͽ� Carrier�� �籸���� Pallet �������� �׸��忡 �߰��ϱ�
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

        // ��ǰ ���� ��û Transaction
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

        // ��ǰ ���� ��� Transaction
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

        // ��ǰ ���� ��û �Ǵ� ��� Transaction
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
                        Util.MessageInfo("SFU1747");        // ��û�Ǿ����ϴ�.
                        break;
                    case "CANCEL":
                        Util.MessageInfo("SFU5032");        // ��ҵǾ����ϴ�.
                        break;
                    default:
                        break;
                }

                returnValue = true;
            }

            return returnValue;
        }

        // ��ǰ ���� ��û �Ǵ� ��� Transaction Validation Check
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

        // ��ǰ ���� ��û Validation
        private bool ValidationCheckApprovalRequest()
        {
            if (string.IsNullOrEmpty(this.txtApprReqNote.Text))
            {
                Util.MessageValidation("SFU1554");  // ��ǰ������ �Է��ϼ���
                this.txtApprReqNote.Focus();
                return false;
            }

            DataTable dt = DataTableConverter.Convert(this.dgPalletMapping.ItemsSource);
            if (dt.Rows.Count <= 0)
            {
                Util.MessageValidation("SFU1411");  // PALLETID�� �Է����ּ���
                return false;
            }

            var query = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE"));
            if (query.Count() <= 0)
            {
                Util.MessageValidation("SFU1654");  // ���õ� ��û�� �����ϴ�.
                return false;
            }

            if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
            {
                Util.Alert("SFU4591"); //�۾��ڸ� �Է��ϼ���
                this.ucPersonInfo.Focus();
                return false;
            }

            return true;
        }

        // ��ǰ ���� ��� Validation
        private bool ValidationCheckApprovalCancel()
        {
            if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
            {
                Util.Alert("SFU4591"); //�۾��ڸ� �Է��ϼ���
                this.ucPersonInfo.Focus();
                return false;
            }

            DataTable dt = DataTableConverter.Convert(this.dgApprHistory.ItemsSource);
            var query = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE"));

            if (query.Count() <= 0)
            {
                Util.MessageValidation("SFU1654");  // ���õ� ��û�� �����ϴ�.
                return false;
            }

            return true;
        }

        // ��ǰ ���� ��û �Ǵ� ��� �Է� ������ ����� 1ȣ
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

        // ��ǰ ���� ��û �Ǵ� ��� �Է� ������ ����� 2ȣ
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

        // ��ǰ ��û ���� ��ȸ
        private void SearchProcess()
        {
            try
            {
                Util.gridClear(this.dgApprHistory);
                this.txRightRowCnt.Text = "[ 0 �� ]";
                DataTable dtReturn = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOGIS_APPR_REQ_LIST", "INDATA", "OUTDATA", this.SetApprHistoryData());

                if (dtReturn == null || dtReturn.Rows.Count <= 0)
                {
                    Util.Alert("101471");  // ��ȸ�� ����� �����ϴ�.
                    return;
                }
                this.txRightRowCnt.Text = "[ " + dtReturn.Rows.Count.ToString() + " �� ]";
                // Data Binding
                Util.GridSetData(this.dgApprHistory, dtReturn, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        // ��ǰ ��û ���� ��ȸ �Է� ������ �����
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

        // Cell Pallet Mapping Data ��ȸ PopUp ȣ��
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

        // ���� ��ǰ���ι�ȣ�� ������ �ִ� ��� Row Select
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

        // ��ǰ ��û ���� Excel Download
        private void ExcelDownload()
        {
            DataTable dt = DataTableConverter.Convert(this.dgApprHistory.ItemsSource);
            if (dt == null || dt.Rows.Count <= 0)
            {
                Util.MessageValidation("SFU3553");        // Excel ������ �����Ͱ� �����ϴ�..
                return;
            }

            try
            {
                ExcelExporter excelExporter = new ExcelExporter();
                C1DataGrid[] dataGridArr = new C1DataGrid[] { this.dgApprHistory };
                string[] tabNameArr = { "��ǰ��û����" };
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
            //������ ��Ƽ�޺�
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
            // ��ǰ ���� ��û�� �Ͻðڽ��ϱ�?
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
            // ��ǰ ���� ��û�� �Ͻðڽ��ϱ�?
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