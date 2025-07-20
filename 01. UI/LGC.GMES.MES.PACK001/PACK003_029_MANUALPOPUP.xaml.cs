/*************************************************************************************
 Created Date : 2022.02.21
      Creator : 김길용
   Decription :
--------------------------------------------------------------------------------------
 [Change History]
    Date         Author      CSR         Description...
  2022.02.21   김길용        SI           Initial Created.
  2022.08.11   김진수        SI           그룹 조건에 제품 혼입 허용
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.PACK001.Class;
using Microsoft.Win32;
using System.Configuration;
using System.IO;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Controls.Primitives;



namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_029_MANUALPOPUP : C1Window, IWorkArea
    {
        #region "Member Variable & Constructor"
        private StockerDetailDataHelper dataHelper = new StockerDetailDataHelper();
        
        string strLotID = string.Empty;
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK003_029_MANUALPOPUP()
        {
            InitializeComponent();
            intHoldCombo();
        }
        public void intHoldCombo()
        {
            CommonCombo _combo = new CommonCombo();
            
            SetDataGridComboItem(CommonCombo.ComboStatus.NONE, dgGroupList.Columns["SMPL_TRGT_FLAG"], "CBO_CODE", "CBO_NAME");


        }
        public static void SetDataGridComboItem(CommonCombo.ComboStatus status, C1.WPF.DataGrid.DataGridColumn dgcol, string selectedValueText, string displayMemberText)
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("CBO_NAME", typeof(string));
                inDataTable.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["CBO_NAME"] = "Y : " + ObjectDic.Instance.GetObjectName("TARGET");
                dr["CBO_CODE"] = "Y";
                inDataTable.Rows.Add(dr);

                DataRow dr1 = inDataTable.NewRow();
                dr1["CBO_NAME"] = "N : " + ObjectDic.Instance.GetObjectName("미대상");
                dr1["CBO_CODE"] = "N";
                inDataTable.Rows.Add(dr1);
                DataTable dtResult = inDataTable;

                DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { selectedValueText, displayMemberText });
                C1.WPF.DataGrid.DataGridComboBoxColumn dataGridComboBoxColumn = dgcol as C1.WPF.DataGrid.DataGridComboBoxColumn;

                if (dataGridComboBoxColumn != null)
                    dataGridComboBoxColumn.ItemsSource = AddStatus(dtBinding, status, selectedValueText, displayMemberText).Copy().AsDataView();
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private static DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }
        #endregion

        #region "Member Function Lists..."
        private void SyncSearchCondition(MultiSelectionBox multiSelectionBox, string inputID, string searchKey)
        {
            DataTable dt = DataTableConverter.Convert(multiSelectionBox.ItemsSource);
            if (string.IsNullOrEmpty(inputID))
            {
                multiSelectionBox.Check(-1);
            }
            else
            {
                int index = 0;
                foreach (DataRowView drv in dt.AsDataView())
                {
                    if (inputID.Contains(drv[searchKey].ToString()))
                    {
                        multiSelectionBox.Check(index++);
                    }
                    else
                    {
                        multiSelectionBox.Uncheck(index++);
                    }
                }
            }
        }


        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
       
        private void InitializeCombo()
        {
            try
            {
                object[] arrFramOperationParameters = FrameOperation.Parameters;
                if (arrFramOperationParameters == null || arrFramOperationParameters.Length <= 0)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
       
        private void AddSampleData()
        {
            if (string.IsNullOrEmpty(this.txtLOTID.Text))
            {
                return;
            }
            List<string> lstLotID = new List<string>();
            lstLotID.Add(txtLOTID.Text);
            DataTable dt = this.SearchPalletMappingData(lstLotID);
            this.SampleGridDataBinding(dt);
        }
        private void SampleGridDataBinding(DataTable dt)
        {
            // Declarations...
            DataTable dtSampleGroup = DataTableConverter.Convert(this.dgGroupList.ItemsSource);

            if (dtSampleGroup == null || dtSampleGroup.Rows.Count <= 0)
            {
                // 처음 추가되는 거라면.
                dtSampleGroup = dt.Copy();
                Util.gridClear(this.dgGroupList);
                Util.GridSetData(this.dgGroupList, dtSampleGroup, FrameOperation);
            }
            else
            {
                // 기존 Data가 존재한다면, 중복 데이터가 들어가 있는가 체크
                foreach (DataRow dr in dt.Select())
                {
                    var result = dtSampleGroup.AsEnumerable().Where(x => x.Field<string>("LOTID").Equals(dr["LOTID"].ToString()));
                    if (result.Count() <= 0)
                    {
                        dtSampleGroup.ImportRow(dr);
                    }
                }
                dtSampleGroup.AcceptChanges();
                Util.gridClear(this.dgGroupList);
                Util.GridSetData(this.dgGroupList, dtSampleGroup, FrameOperation);
            }
            this.txtLOTID.Clear();
            this.txtLOTID.Focus();
        }

        // Carrier와 재구성된 Pallet 매핑정보 조회하기
        private DataTable SearchPalletMappingData(List<string> lstLotID)
        {
            // Declarations...
            DataTable dtReturn = new DataTable();
            string bizRuleName = "BR_PRD_CHK_LOGIS_SAMPLE_MANUAL_LOT";
            try
            {
                DataSet ds = new DataSet();
                ds.Tables.Add(this.SetSampleData(lstLotID));

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
        
        private DataTable SetSampleData(List<string> lstLotID)
        {
            // Definition Schema of DataTable
            DataTable dt = new DataTable();
            dt.TableName = "INDATA";
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("GR_FLAG", typeof(string));

            // Insert Data
            foreach (string lotID in lstLotID)
            {
                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = lotID;
                dr["GR_FLAG"] = chkDetail.IsChecked == true ? "Y" : "N";
                dt.Rows.Add(dr);
            }

            dt.AcceptChanges();     // Apply
            return dt;
        }
        // BizRule 호출 - DB Time 가져오기
        private DateTime GetSystemTime()
        {
            string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            string outDataSetName = "OUTDATA";
            DateTime dteReturn = DateTime.Now;

            try
            {
                string inDataTableNameList = string.Empty;
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, outDataSetName, dsINDATA);

                if (CommonVerify.HasTableInDataSet(dsOUTDATA))
                {
                    foreach (DataTable dt in dsOUTDATA.Tables.OfType<DataTable>().Where(x => x.TableName.Equals(outDataSetName)))
                    {
                        if (CommonVerify.HasTableRow(dt))
                        {
                            foreach (DataRowView drv in dt.AsDataView())
                            {
                                dteReturn = Convert.ToDateTime(drv["SYSTIME"]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return dteReturn;
            }

            return dteReturn;
        }
        private void SelectLotListProcess(string strLotID)
        {
            try
            {
                Util.gridClear(dgGroupList);

                DataTable INDATA = new DataTable();
                INDATA.TableName = "RQSTDT";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("GR_FLAG", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = string.IsNullOrEmpty(strLotID) ? null : strLotID;
                dr["GR_FLAG"] = chkDetail.IsChecked == true ? "Y" : "N";

                INDATA.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("BR_PRD_CHK_LOGIS_SAMPLE_MANUAL_LOT", "INDATA", "OUTDATA", INDATA, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgGroupList, dtResult, FrameOperation, true);
                    }
                    loadingIndicator.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private void SearchClipboardData()
        {
            try
            {
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
                this.SelectLotListProcess(txtLOTID.Text);
                this.txtLOTID.Text = string.Empty;
                if (this.dgGroupList == null || this.dgGroupList.ItemsSource == null)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region "Events..."
        private void C1Window_Initialized(object sender, EventArgs e)
        {
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                string[] tmp = null;
                this.chkDetail.IsChecked = true;
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtLOTID.Text)) strLotID = txtLOTID.Text;
            this.SelectLotListProcess(strLotID);
        }
        // Validation Check
        private bool ValidationCheckRequest()
        {
            bool returnValue = true;
            if (this.dgGroupList == null || this.dgGroupList.Rows.Count < 0)
            {
                Util.Alert("9059");     // 데이터를 조회 하십시오.
                return false;
            }
            if (this.dgGroupList.ItemsSource == null)
            {
                Util.Alert("9059");     // 데이터를 조회 하십시오.
                return false;
            }
            if (this.chkDetail.IsChecked ==false)
            {
                DataTable dt = DataTableConverter.Convert(this.dgGroupList.ItemsSource);
                var queryValidationCheck = dt.AsEnumerable().Where(x => x.Field<string>("SMPL_TRGT_FLAG").ToUpper().Equals("Y"));
                if (queryValidationCheck.Count() <= 0)
                {
                    Util.Alert("10008");  // 선택된 데이터가 없습니다.
                    return false;
                }
                
                string sTmpLot = Util.NVC(DataTableConverter.GetValue(dgGroupList.Rows[0].DataItem, "LOTID"));
                string sTmpProd = Util.NVC(DataTableConverter.GetValue(dgGroupList.Rows[0].DataItem, "PRODID"));
                string sTmpEqsg = Util.NVC(DataTableConverter.GetValue(dgGroupList.Rows[0].DataItem, "EQSGID"));
                // 투입LOT 중복 체크
                for (int i = 0; i < dgGroupList.Rows.Count - dgGroupList.BottomRows.Count; i++)
                {
                    //샘플 그룹 조건에 제품 인터락 제외 처리
                    //if (!Util.NVC(DataTableConverter.GetValue(dgGroupList.Rows[i].DataItem, "PRODID")).Equals(sTmpProd))
                    //{
                    //    //제품코드가 다릅니다.
                    //    Util.MessageValidation("SFU1897");
                    //    return false;
                    //}
                    if (!Util.NVC(DataTableConverter.GetValue(dgGroupList.Rows[i].DataItem, "EQSGID")).Equals(sTmpEqsg))
                    {
                        //라인정보가 틀립니다.
                        Util.MessageValidation("SFU4618");
                        return false;
                    }
                }

            }
            if (dgGroupList.Rows.Count() > 100)
            {
                Util.MessageValidation("SFU3695");   // 최대 100개 까지 가능합니다.
                return false;
            }
            return returnValue;
        }
        #endregion
        

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                this.AddSampleData();
                //if (!string.IsNullOrEmpty(txtLOTID.Text)) strLotID = txtLOTID.Text;
                //this.SelectLotListProcess(strLotID);
            }
        }
        private void dgGroupList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            DataRowView drv = e.Row.DataItem as DataRowView;

            if (e.Column != dgGroupList.Columns["SMPL_TRGT_FLAG"])
            {
                e.Cancel = true;
                return;
            }

            if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
            {
                e.Cancel = false;
            }
            else
            {
                if (e.Column != this.dgGroupList.Columns["SMPL_TRGT_FLAG"]
                 )
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                }
            }
        }

        private void btnSavebtm_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2875"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    if (ValidationCheckRequest())
                    {
                        setSample_reg();
                    }
                }
            });
        }
        private void setSample_reg()
        {
            try
            {
                if (dgGroupList == null)
                {
                    return;
                }

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("END_FLAG", typeof(string));
                INDATA.Columns.Add("TRGT_FLAG", typeof(string));
                int chk_idx = 0;
                for (int i = 0; i < dgGroupList.GetRowCount(); i++)
                {
                        DataRow dr = INDATA.NewRow();
                        dr["LOTID"] = DataTableConverter.GetValue(dgGroupList.Rows[i].DataItem, "LOTID").ToString();
                        dr["USERID"] = LoginInfo.USERID;
                        dr["END_FLAG"] = chkDetail.IsChecked == true ? "Y" : "N";
                        dr["TRGT_FLAG"] = DataTableConverter.GetValue(dgGroupList.Rows[i].DataItem, "SMPL_TRGT_FLAG").ToString();
                        INDATA.Rows.Add(dr);
                    chk_idx++;
                }

                if (chk_idx == 0)
                {
                    return;
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_INS_LOGIS_SAMPLE_LOT_UI", "INDATA", "OUTDATA", INDATA);
                Util.MessageInfo("SFU1275"); //정상처리되었습니다.

                Util.gridClear(dgGroupList);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void chkDetail_Checked(object sender, RoutedEventArgs e)
        {
            if (this.dgGroupList == null || this.dgGroupList.Rows.Count < 0)
            {
                return;
            }
            if (this.dgGroupList.ItemsSource == null)
            {
                return;
            }
            for (int inx = 0; inx < dgGroupList.GetRowCount(); inx++)
            {
                DataTableConverter.SetValue(dgGroupList.Rows[inx].DataItem, "SMPL_TRGT_FLAG", "Y");
            }
        }

        private void chkDetail_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this.dgGroupList == null || this.dgGroupList.Rows.Count < 0)
            {
                return;
            }
            if (this.dgGroupList.ItemsSource == null)
            {
                return;
            }
            for (int inx = 0; inx < dgGroupList.GetRowCount(); inx++)
            {
                DataTableConverter.SetValue(dgGroupList.Rows[inx].DataItem, "SMPL_TRGT_FLAG", "N");
            }
        }

        //private void txtLOTID_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key.Equals(Key.V) && Keyboard.IsKeyDown(Key.LeftCtrl))
        //    {
        //        this.SearchClipboardData();
        //        e.Handled = true;
        //    }
        //}
    }
}