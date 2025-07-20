using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Threading;
using System.Windows.Media;


/*************************************************************************************
 Created Date : 2019.07.01
      Creator : KANG HOWUN
   Decription : 바코드 프린터 관리(PACK) 
--------------------------------------------------------------------------------------
 [Change History]
  2019.10.10  KANG HOWUN : 설비별 1건에서 설비-제품코드별 1건으로 처리 , GetPrintInfo, isLabelDuplication, GetIsDuplicationLabel, dgPrintList_CommittedEdit 부분 추가
  2024.03.28  김민석       [E20240328-001482] 그리드 데이터 중 설비 콤보 박스 데이터 조회 시 라인 정보 들어가도록 수정
**************************************************************************************/

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// PACK001_043.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK001_043 : UserControl
    {
        CommonCombo _combo = new CommonCombo();
        DataTable dtMain = new DataTable();
        string sAREAID = LoginInfo.CFG_SHOP_ID;
        private bool _manualCommit = false;
        private DataTable isCreateTable = new DataTable();
        string sBeforeUse_flag = null;

        public PACK001_043()
        {
            InitializeComponent();

            txRowCnt.Text = ObjectDic.Instance.GetObjectName("건");
            //combobox 설정
            InitCombo();
        }

        //combobox 설정
        private void InitCombo()
        {
            try
            {
                //동
                SetAreaByAreaType();
                //라인
                SetEquipmentSegment();
                //공정
                SetProdess();
                //설비
                SetEquipment();
                //설비
                SetProcid();
                //라벨
                SetLabelCode();
                //사용여부0
                setUseYN();
                //프린트 DPI                

                CommonCombo.SetDataGridComboItem("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", new string[] { "LANGID", "AREAID" }, new string[] { LoginInfo.LANGID, sAREAID }, CommonCombo.ComboStatus.NONE, dgPrintList.Columns["EQSGID"], "CBO_CODE", "CBO_NAME");
                CommonCombo.SetDataGridComboItem("DA_BAS_SEL_PROCESS_PACK_CBO_BY_LABELPRINTIUSE", new string[] { "LANGID", "AREAID", "LABELPRINTIUSE" }, new string[] { LoginInfo.LANGID, sAREAID, "Y" }, CommonCombo.ComboStatus.NONE, dgPrintList.Columns["PROCID"], "CBO_CODE", "CBO_NAME");
                //2024.03.28 KIM MIN SEOK 그리드 설비 콤보 박스 데이터 조회 시 라인 조건 추가
                CommonCombo.SetDataGridComboItem("DA_BAS_SEL_EQUIPMENT_BY_SHOPID_FOR_PACK_CBO", new string[] { "LANGID", "SHOPID" }, new string[] { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID}, CommonCombo.ComboStatus.NONE, dgPrintList.Columns["EQPTID"], "CBO_CODE", "CBO_NAME");
                CommonCombo.SetDataGridComboItem("DA_PRD_SEL_LABELCODE_BY_PRODID_CD_CBO", new string[] { "LANGID", "PRODID", "SHOPID" }, new string[] { LoginInfo.LANGID, null, LoginInfo.CFG_SHOP_ID }, CommonCombo.ComboStatus.NONE, dgPrintList.Columns["LABEL_CODE"], "CBO_CODE", "CBO_NAME");
                CommonCombo.SetDataGridComboItem("DA_PRD_SEL_LABELCODE_BY_PRODID_CD_CBO", new string[] { "LANGID", "PRODID", "SHOPID" }, new string[] { LoginInfo.LANGID, null, LoginInfo.CFG_SHOP_ID }, CommonCombo.ComboStatus.NONE, dgPrintList.Columns["TURN_LABEL_CODE"], "CBO_CODE", "CBO_NAME");
                CommonCombo.SetDataGridComboItem("DA_BAS_SEL_COMMCODE_CBO", new string[] { "LANGID", "CMCDTYPE" }, new string[] { LoginInfo.LANGID, "PRINTER_RESOLUTION" }, CommonCombo.ComboStatus.NA, dgPrintList.Columns["PRTR_DPI"], "CBO_CODE", "CBO_NAME");
                CommonCombo.SetDataGridComboItem("DA_BAS_SEL_COMMCODE_CBO", new string[] { "LANGID", "CMCDTYPE" }, new string[] { LoginInfo.LANGID, "TURN_TYPE_CODE" }, CommonCombo.ComboStatus.NA, dgPrintList.Columns["TURN_TYPE_CODE"], "CBO_CODE", "CBO_NAME");
                CommonCombo.SetDataGridComboItem("DA_BAS_SEL_PRODUCT_MULTI_CBO", new string[] { "LANGID", "SHOPID", "AREAID", "EQSGID", "PROCID", "PRDT_CLSS_CODE" }, new string[] { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, null, null, "CMA,BMA" }, CommonCombo.ComboStatus.NONE, dgPrintList.Columns["PRODID"], "CBO_CODE", "CBO_NAME");
                SetDataGridComboItem(CommonCombo.ComboStatus.NONE, dgPrintList.Columns["USE_FLAG"], "CBO_CODE", "CBO_NAME");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region 메뉴COMBO DA

        private void SetAreaByAreaType()
        {
            String[] sFilter = { sAREAID, Area_Type.PACK };
            _combo.SetCombo(cboAreaByAreaType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);
        }

        private void SetEquipmentSegment()
        {
            String[] sFilter = { cboAreaByAreaType.SelectedValue.ToString() };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);
        }

        private void SetProdess()
        {
            string[] sFilter = new string[] { cboEquipmentSegment.SelectedValue.ToString(), "Y" };
            SetProcessPack_LabelPrint(cboProcess, CommonCombo.ComboStatus.ALL, sFilter);
        }

        private void SetEquipment()
        {
            string[] filter = new string[] { cboEquipmentSegment.SelectedValue.ToString(), cboProcess.SelectedValue.ToString() };
            SetEquipment_sum(cboEquipment, CommonCombo.ComboStatus.ALL, filter);
        }

        private void SetProcid()
        {
            string[] filter = new string[] { LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID,  cboEquipmentSegment.SelectedValue.ToString(), cboProcess.SelectedValue.ToString(), "CMA,BMA" };
            SetProcid_sum(cboProdid, CommonCombo.ComboStatus.ALL, filter);
        }

        private void SetLabelCode()
        {
            string[] filter = new string[] { cboProdid.SelectedValue.ToString()};
            SetLabelCode_sum(cboLabelCode, CommonCombo.ComboStatus.ALL, filter);
        }

        //private void SetLabelDpi()
        //{
        //    string[] filter = new string[] { cboEquipmentSegment.SelectedValue.ToString(), cboProcess.SelectedValue.ToString() };
        //    SetLabelDPI_sum(cboLabelCode, CommonCombo.ComboStatus.ALL, filter);
        //}
        #endregion

        public static void SetDataGridComboItem(CommonCombo.ComboStatus status, C1.WPF.DataGrid.DataGridColumn dgcol, string selectedValueText, string displayMemberText)
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("CBO_NAME", typeof(string));
                inDataTable.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["CBO_NAME"] = "Y : " + ObjectDic.Instance.GetObjectName("사용");
                dr["CBO_CODE"] = "Y";
                inDataTable.Rows.Add(dr);

                DataRow dr1 = inDataTable.NewRow();
                dr1["CBO_NAME"] = "N : " + ObjectDic.Instance.GetObjectName("사용 안함");
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

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        //DataGridCombo_리스트값 설정
        public void SetGridComboItem(C1.WPF.DataGrid.DataGridColumn Col, CommonCombo.ComboStatus ComboStatus)
        {
            if (Col == null) return;
            string bizRuleName = "";
            string[] arrColumn = new string[] { "", "" };
            string[] arrCondition = new string[] { "", "" };
            string sEQSGID = string.Empty;
            string sPROCESS = string.Empty;
            string sLABELCODE = string.Empty;
            switch (Convert.ToString(Col.Name))
            {
                case "EQSGID":
                    bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
                    arrColumn = new string[] { "LANGID", "AREAID" };
                    arrCondition = new string[] { LoginInfo.LANGID, sAREAID };
                    break;
                case "PROCID":
                    sEQSGID = cboEquipmentSegment.SelectedItem.ToString();
                    bizRuleName = "DA_BAS_SEL_PROCESS_PACK_CBO_BY_LABELPRINTIUSE";
                    arrColumn = new string[] { "LANGID", "AREAID", "LABELPRINTIUSE" };
                    arrCondition = new string[] { LoginInfo.LANGID, sAREAID, "Y" };
                    break;
                case "EQPTID":
                    sEQSGID = cboEquipmentSegment.SelectedItem.ToString();
                    sPROCESS = cboProcess.SelectedItem.ToString();
                    bizRuleName = "DA_BAS_SEL_EQUIPMENT_BY_EQSGID_PROCID_CBO";
                    arrColumn = new string[] { "LANGID", "EQSGID", "PROCID" };
                    arrCondition = new string[] { LoginInfo.LANGID, sEQSGID, sPROCESS };
                    break;
                case "LABEL_CODE":
                    sEQSGID = cboEquipmentSegment.SelectedItem.ToString();
                    sPROCESS = cboProcess.SelectedItem.ToString();
                    bizRuleName = "DA_PRD_SEL_LABELCODE_BY_PRODID_CD_CBO";
                    arrColumn = new string[] { "LANGID", "PRODID", "SHOPID" };
                    arrCondition = new string[] { LoginInfo.LANGID, null, LoginInfo.CFG_SHOP_ID };
                    break;
                case "TURN_LABEL_CODE":
                    sEQSGID = cboEquipmentSegment.SelectedItem.ToString();
                    sPROCESS = cboProcess.SelectedItem.ToString();
                    bizRuleName = "DA_PRD_SEL_LABELCODE_BY_PRODID_CD_CBO";
                    arrColumn = new string[] { "LANGID", "PRODID", "SHOPID" };
                    arrCondition = new string[] { LoginInfo.LANGID, null, LoginInfo.CFG_SHOP_ID };
                    break;
                case "PRODID":
                    bizRuleName = "DA_BAS_SEL_PRODUCT_MULTI_CBO";
                    arrColumn = new string[] {"LANGID","SHOPID","AREAID","EQSGID","PROCID","PRDT_CLSS_CODE" };
                    arrCondition = new string[] { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, null, null, "CMA,BMA" };
                    break;
                case "USE_FLAG":
                    DataTable dt = new DataTable();
                    dt.Columns.Add("CBO_NAME", typeof(string));
                    dt.Columns.Add("CBO_CODE", typeof(string));

                    DataRow dr = dt.NewRow();
                    dr["CBO_NAME"] = "Y : " + ObjectDic.Instance.GetObjectName("사용");
                    dr["CBO_CODE"] = "Y";
                    dt.Rows.Add(dr);

                    DataRow dr1 = dt.NewRow();
                    dr1["CBO_NAME"] = "N : " + ObjectDic.Instance.GetObjectName("사용 안함");
                    dr1["CBO_CODE"] = "N";
                    dt.Rows.Add(dr1);

                    dt.AcceptChanges();
                    break;
                default:
                    break;
            }

            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";
            SetGridComboItem(bizRuleName, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, Col, selectedValueText, displayMemberText);
        }

        //DataGridCombo_리스트값 biz 호출 
        public static void SetGridComboItem(string bizRuleName, string[] arrColumn, string[] arrCondition, CommonCombo.ComboStatus status, C1.WPF.DataGrid.DataGridColumn dgcol, string selectedValueText, string displayMemberText)
        {

            DataTable inDataTable = new DataTable();
            inDataTable.TableName = "RQSTDT";

            if (arrColumn != null)
            {
                // 동적 컬럼 생성 및 Row 추가
                foreach (string col in arrColumn)
                {
                    inDataTable.Columns.Add(col, typeof(string));
                }

                DataRow dr = inDataTable.NewRow();
                for (int i = 0; i < inDataTable.Columns.Count; i++)
                {
                    dr[inDataTable.Columns[i].ColumnName] = arrCondition[i];
                }
                inDataTable.Rows.Add(dr);
            }
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);
            DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { selectedValueText, displayMemberText });
            C1.WPF.DataGrid.DataGridComboBoxColumn dataGridComboBoxColumn = dgcol as C1.WPF.DataGrid.DataGridComboBoxColumn;
            if (dataGridComboBoxColumn != null)
               dataGridComboBoxColumn.ItemsSource = AddStatus(dtBinding, status, selectedValueText, displayMemberText).Copy().AsDataView();
        }

        //조회
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ShowLoadingIndicator();
            DoEvents();
            string[] sfilter = new string[] {
                cboEquipmentSegment.SelectedValue.ToString(),
                cboProcess.SelectedValue.ToString(),
                cboEquipment.SelectedValue.ToString(),
                cboUseFlag.SelectedValue.ToString(),
                cboLabelCode.SelectedValue.ToString(),
                cboProdid.SelectedValue.ToString()
            };
            loadingIndicator.Visibility = System.Windows.Visibility.Visible;
            dtMain = GetPrintInfo(sfilter);
            txRowCnt.Text = string.IsNullOrEmpty(Convert.ToString(dtMain.Rows.Count)) ? "[총 0건]" : "[총 " + Convert.ToString(dtMain.Rows.Count) + "건]";
            Util.GridSetData(dgPrintList, dtMain, FrameOperation);
            loadingIndicator.Visibility = Visibility.Collapsed;
            HiddenLoadingIndicator();
        }

        // List Search
        public DataTable GetPrintInfo(string[] filter)
        {
            DataTable result = new DataTable();
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                inTable.Columns.Add("LABEL_CODE", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                if (cboEquipmentSegment.SelectedValue.ToString().Contains("SELECT")) {
                    Util.MessageValidation("SFU3206");
                }
                if (cboProcess.SelectedValue.ToString().Contains("SELECT"))
                {
                    Util.MessageValidation("SFU4050");
                }


                DataRow newRow = inTable.NewRow();
                newRow["EQSGID"] = filter[0];
                newRow["PROCID"]     = String.IsNullOrEmpty(filter[1]) || filter[1].Equals("ALL") ? null : filter[1];
                newRow["EQPTID"]     = String.IsNullOrEmpty(filter[2]) || filter[2].Equals("ALL") ? null : filter[2];
                newRow["USE_FLAG"]   = String.IsNullOrEmpty(filter[3]) || filter[3].Equals("ALL") ? null : filter[3];
                newRow["LABEL_CODE"] = String.IsNullOrEmpty(filter[4]) || filter[4].Equals("ALL") ? null : filter[4];
                newRow["PRODID"]     = String.IsNullOrEmpty(filter[5]) || filter[5].Equals("ALL") ? null : filter[5];
                newRow["LANGID"]     = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                result =   new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_EQPT_LABEL", "RQSTDT", "RSLTDT", inTable);

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Logger.Instance.WriteLine(Logger.OPERATION_C + "DA_PRD_SEL_TB_SFC_EQPT_LABEL", ex);
                return result;
            }
            finally
            {
                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                Logger.Instance.WriteLine(Logger.OPERATION_C + "DA_PRD_SEL_TB_SFC_EQPT_LABEL", Logger.MESSAGE_OPERATION_END);
            }
            return result;
        }

        //컨트롤 상속 내역
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 미처리 
        }

        //Combo 상태정보 : ALL, N/A, SELECT
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

        //사용여부 Combo 설정
        private void setUseYN()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_NAME", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr_ = dt.NewRow();
                dr_["CBO_NAME"] = "ALL";
                dr_["CBO_CODE"] = "ALL";
                dt.Rows.Add(dr_);

                DataRow dr = dt.NewRow();
                dr["CBO_NAME"] = "Y : " + ObjectDic.Instance.GetObjectName("사용");
                dr["CBO_CODE"] = "Y";
                dt.Rows.Add(dr);

                DataRow dr1 = dt.NewRow();
                dr1["CBO_NAME"] = "N : " + ObjectDic.Instance.GetObjectName("사용 안함");
                dr1["CBO_CODE"] = "N";
                dt.Rows.Add(dr1);

                dt.AcceptChanges();

                cboUseFlag.ItemsSource = DataTableConverter.Convert(dt);
                cboUseFlag.SelectedIndex = 0; //default Y
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

           #region Combobox 선택 이벤트
        private void cboAreaByAreaType_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboAreaByAreaType.SelectedIndex > -1)
            {
                 sAREAID = Convert.ToString(cboAreaByAreaType.SelectedValue);
                SetEquipmentSegment();
                Util.gridClear(dgPrintList);
                CommonCombo.SetDataGridComboItem("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", new string[] { "LANGID", "AREAID" }, new string[] { LoginInfo.LANGID, sAREAID }, CommonCombo.ComboStatus.NONE, dgPrintList.Columns["EQSGID"], "CBO_CODE", "CBO_NAME");
            }
            else
            {
                sAREAID = string.Empty;
            }
        }

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.SelectedIndex > -1)
            {
                SetProdess();
            }
        }

        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.SelectedIndex > -1)
            {
                SetEquipment();
                SetProcid();
            }
        }

        private void cboEquipment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
        }

        //데이타 저장
        private void btnSave(object sender, RoutedEventArgs e)
        {
            if (!Validation()) return;
            Util.MessageConfirm("SFU3533", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save();
                    DoEvents();
                    btnSearch_Click(null, null);
                }
            });
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private void Save()
        {
            ShowLoadingIndicator();
            DoEvents();
            
            try
            {
                string bizRuleName = "BR_PRD_REG_TB_SFC_EQPT_LABEL";

                isCreateTable = DataTableConverter.Convert(dgPrintList.GetCurrentItems());
                if (!CommonVerify.HasDataGridRow(dgPrintList)) return;

                this.dgPrintList.EndEdit();
                this.dgPrintList.EndEditRow(true);

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("LABEL_CODE", typeof(string));
                inDataTable.Columns.Add("TURN_LABEL_CODE", typeof(string));
                inDataTable.Columns.Add("TURN_TYPE_CODE", typeof(string));
                
                inDataTable.Columns.Add("LABEL_PRT_NAME", typeof(string));
                inDataTable.Columns.Add("PRTR_DPI", typeof(string));
                inDataTable.Columns.Add("PRT_X", typeof(string));
                inDataTable.Columns.Add("PRT_Y", typeof(string));
                inDataTable.Columns.Add("PRT_DARKNESS", typeof(string));
                inDataTable.Columns.Add("PRTR_IP", typeof(string));
                inDataTable.Columns.Add("PRTR_PORT", typeof(string));
                inDataTable.Columns.Add("PRT_QTY", typeof(string));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));
                inDataTable.Columns.Add("USER", typeof(string));
                inDataTable.Columns.Add("SQLTYPE", typeof(string));
                inDataTable.Columns.Add("SCAN_ID_PRFX", typeof(string));
                inDataTable.Columns.Add("TURN_STRT_DTTM", typeof(string));
                inDataTable.Columns.Add("TURN_END_DTTM", typeof(string));

                foreach (object added in dgPrintList.GetAddedItems())
                {
                    if (DataTableConverter.GetValue(added, "CHK").Equals("True"))
                    {
                        DataRow param = inDataTable.NewRow();

                        param["EQSGID"] = DataTableConverter.GetValue(added, "EQSGID");
                        param["PROCID"] = DataTableConverter.GetValue(added, "PROCID");
                        param["EQPTID"] = DataTableConverter.GetValue(added, "EQPTID");
                        param["PRODID"] = DataTableConverter.GetValue(added, "PRODID");
                        param["LABEL_CODE"] = DataTableConverter.GetValue(added, "LABEL_CODE");
                        param["TURN_LABEL_CODE"] = DataTableConverter.GetValue(added, "TURN_LABEL_CODE");
                        param["TURN_TYPE_CODE"] = DataTableConverter.GetValue(added, "TURN_TYPE_CODE");
                        param["LABEL_PRT_NAME"] = DataTableConverter.GetValue(added, "LABEL_PRT_NAME");
                        param["PRTR_DPI"] = DataTableConverter.GetValue(added, "PRTR_DPI");
                        param["PRT_X"] = DataTableConverter.GetValue(added, "PRT_X");
                        param["PRT_Y"] = DataTableConverter.GetValue(added, "PRT_Y");
                        param["PRT_DARKNESS"] = DataTableConverter.GetValue(added, "PRT_DARKNESS");
                        param["PRTR_IP"] = DataTableConverter.GetValue(added, "PRTR_IP");
                        param["PRTR_PORT"] = DataTableConverter.GetValue(added, "PRTR_PORT");
                        param["PRT_QTY"] = DataTableConverter.GetValue(added, "PRT_QTY");
                        param["USE_FLAG"] = DataTableConverter.GetValue(added, "USE_FLAG");
                        param["SCAN_ID_PRFX"] = DataTableConverter.GetValue(added, "SCAN_ID_PRFX");
                        param["TURN_STRT_DTTM"] = (DataTableConverter.GetValue(added, "TURN_STRT_DTTM") == null) ? null : Convert.ToDateTime(DataTableConverter.GetValue(added, "TURN_STRT_DTTM")).ToString("yyyy-MM-dd HH:mm:ss");
                        param["TURN_END_DTTM"] = (DataTableConverter.GetValue(added, "TURN_END_DTTM") == null) ? null : Convert.ToDateTime(DataTableConverter.GetValue(added, "TURN_END_DTTM")).ToString("yyyy-MM-dd HH:mm:ss");
                        param["USER"] = LoginInfo.USERID;
                        param["SQLTYPE"] = "I";
                        inDataTable.Rows.Add(param);
                    }
                }

                foreach (object modified in dgPrintList.GetModifiedItems())
                {
                    if (DataTableConverter.GetValue(modified, "CHK").Equals("True"))
                    {
                        DataRow param = inDataTable.NewRow();
                        param["EQSGID"] = DataTableConverter.GetValue(modified, "EQSGID");
                        param["PROCID"] = DataTableConverter.GetValue(modified, "PROCID");
                        param["EQPTID"] = DataTableConverter.GetValue(modified, "EQPTID");
                        param["PRODID"] = DataTableConverter.GetValue(modified, "PRODID");
                        param["LABEL_CODE"] = DataTableConverter.GetValue(modified, "LABEL_CODE");
                        param["TURN_LABEL_CODE"] = DataTableConverter.GetValue(modified, "TURN_LABEL_CODE");
                        param["TURN_TYPE_CODE"] = DataTableConverter.GetValue(modified, "TURN_TYPE_CODE");
                        param["LABEL_PRT_NAME"] = DataTableConverter.GetValue(modified, "LABEL_PRT_NAME");
                        param["PRTR_DPI"] = DataTableConverter.GetValue(modified, "PRTR_DPI");
                        param["PRT_X"] = DataTableConverter.GetValue(modified, "PRT_X");
                        param["PRT_Y"] = DataTableConverter.GetValue(modified, "PRT_Y");
                        param["PRT_DARKNESS"] = DataTableConverter.GetValue(modified, "PRT_DARKNESS");
                        param["PRTR_IP"] = DataTableConverter.GetValue(modified, "PRTR_IP");
                        param["PRTR_PORT"] = DataTableConverter.GetValue(modified, "PRTR_PORT");
                        param["PRT_QTY"] = DataTableConverter.GetValue(modified, "PRT_QTY");
                        param["USE_FLAG"] = DataTableConverter.GetValue(modified, "USE_FLAG");
                        param["SCAN_ID_PRFX"] = DataTableConverter.GetValue(modified, "SCAN_ID_PRFX");
                        param["TURN_STRT_DTTM"] = (DataTableConverter.GetValue(modified, "TURN_STRT_DTTM") == null) ? null : Convert.ToDateTime(DataTableConverter.GetValue(modified, "TURN_STRT_DTTM")).ToString("yyyy-MM-dd HH:mm:ss");
                        param["TURN_END_DTTM"] = (DataTableConverter.GetValue(modified, "TURN_END_DTTM") == null) ? null : Convert.ToDateTime(DataTableConverter.GetValue(modified, "TURN_END_DTTM")).ToString("yyyy-MM-dd HH:mm:ss");
                        param["USER"] = LoginInfo.USERID;
                        param["SQLTYPE"] = "U";
                        inDataTable.Rows.Add(param);
                    }
                }

                if (inDataTable.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU3538");
                    return;
                }

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", null, indataSet);
                Util.MessageInfo("SFU2056", inDataTable.Rows.Count);
                Util.gridClear(dgPrintList);

                inDataTable = new DataTable();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        //Validation
        private bool Validation()
        {
            foreach (object added in dgPrintList.GetAddedItems())
            {
                if (DataTableConverter.GetValue(added, "CHK").Equals("True"))
                {
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "EQSGID"))))
                    {
                        Util.MessageValidation("SFU3206");
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PROCID"))))
                    {
                        Util.MessageValidation("SFU4050"); // 공정ID를 선택해 주세요.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "EQPTID"))))
                    {
                        Util.MessageValidation("SFU1673"); // 설비를 선택하세요.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PRODID"))))
                    {
                        Util.MessageValidation("SFU7008"); // 제품코드를 선택하세요.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "LABEL_CODE"))))
                    {
                        Util.MessageValidation("SFU3732"); // 라벨 타입을 선택하세요.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "LABEL_PRT_NAME"))))
                    {
                        Util.MessageValidation("SFU3733"); // 프린터 환경설정 정보가 없습니다
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PRTR_DPI"))))
                    {
                        Util.MessageValidation("SFU7334"); // DPI
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PRT_X"))))
                    {
                        Util.MessageValidation("SFU7335"); // 프린터 환경설정 정보가 없습니다
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PRT_Y"))))
                    {
                        Util.MessageValidation("SFU7336"); // 프린터 환경설정 정보가 없습니다
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PRTR_IP"))))
                    {
                        Util.MessageValidation("SFU7338");
                        return false;
                    }else { 
                        if (!IsIPv4(Util.NVC(DataTableConverter.GetValue(added, "PRTR_IP")))){
                            return false;
                        }
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PRTR_PORT"))))
                    {
                        Util.MessageValidation("SFU7339");
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PRT_QTY"))))
                    {
                        Util.MessageValidation("SFU1684");
                        return false;
                    }
                    //if (!String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "TURN_STRT_DTTM"))))
                    //{
                    //    string sStDate = Util.NVC(DataTableConverter.GetValue(added, "TURN_STRT_DTTM"));
                    //    string sEdDate = Util.NVC(DataTableConverter.GetValue(added, "TURN_END_DTTM"));
                    //    return IsDateTime( sStDate,  sEdDate);
                    //}
                }
            }

            foreach (object added in dgPrintList.GetModifiedItems())
            {
                if (DataTableConverter.GetValue(added, "CHK").Equals("True"))
                {
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "EQSGID"))))
                    {
                        Util.MessageValidation("SFU3206");
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PROCID"))))
                    {
                        Util.MessageValidation("SFU4050"); // 공정ID를 선택해 주세요.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "EQPTID"))))
                    {
                        Util.MessageValidation("SFU1673"); // 설비를 선택하세요.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PRODID"))))
                    {
                        Util.MessageValidation("SFU7008"); // 제품코드를 선택하세요.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "LABEL_CODE"))))
                    {
                        Util.MessageValidation("SFU3732"); // 라벨 타입을 선택하세요.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "LABEL_PRT_NAME"))))
                    {
                        Util.MessageValidation("SFU3733"); // 프린터 환경설정 정보가 없습니다
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PRTR_DPI"))))
                    {
                        Util.MessageValidation("SFU7334"); // DPI
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PRT_X"))))
                    {
                        Util.MessageValidation("SFU7335"); // 프린터 환경설정 정보가 없습니다
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PRT_Y"))))
                    {
                        Util.MessageValidation("SFU7336"); // 프린터 환경설정 정보가 없습니다
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PRTR_IP"))))
                    {
                        Util.MessageValidation("SFU7338");
                        return false;
                    }
                    else
                    {
                        if (!IsIPv4(Util.NVC(DataTableConverter.GetValue(added, "PRTR_IP"))))
                        {
                            return false;
                        }
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PRTR_PORT"))))
                    {
                        Util.MessageValidation("SFU7339");
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PRT_QTY"))))
                    {
                        Util.MessageValidation("SFU1684");
                        return false;
                    }
                    //if (!String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "TURN_STRT_DTTM"))))
                    //{
                    //    string sStDate = Util.NVC(DataTableConverter.GetValue(added, "TURN_STRT_DTTM"));
                    //    string sEdDate = Util.NVC(DataTableConverter.GetValue(added, "TURN_END_DTTM"));
                    //    return IsDateTime(sStDate, sEdDate);
                    //}
                }
            }

            return true;
        }

        public Boolean IsDateTime(string sDate)
        {
            bool result = false;
            try
            {
                if (sDate == null || sDate == "")
                {
                    result = true;
                }else { 
                    DateTime.Parse(sDate);
                    result = true;
                }
            }
            catch (Exception)
            {
                Util.Alert("SFU3566"); 
                result = false;
            }
            return result;
        }

        public Boolean IsIPv4(string value)
        {
            Boolean isIP4 = true;
            var quads = value.Split('.');

            // if we do not have 4 quads, return false
            if (!(quads.Length == 4)) isIP4=false;

            // for each quad
            foreach (var quad in quads)
            {
                int q;
                if (!Int32.TryParse(quad, out q)
                    || !q.ToString().Length.Equals(quad.Length)
                    || q < 0
                    || q > 255) { isIP4 = false; }
            }
            if (!isIP4)
            {
                Util.MessageValidation("SFU3465");
            }
            return isIP4;
        }

        public Boolean isNumber(string value)
        {
            Boolean isNumber = true;
            for (int i = 0; i < value.Length; i++)
            {
                char val = Convert.ToChar(value[i]);
                if (!(char.IsDigit(val)))    //숫자와 백스페이스를 제외한 나머지를 바로 처리
                {
                    isNumber = false;
                }
            }
            if (!isNumber)
            {
                Util.MessageValidation("SFU3465");
                //e.Cancel = true;
            }
            return isNumber;
        }

        public void isLabelDuplication()
        {
            if (GetIsDuplicationLabel())
            {
               Util.MessageValidation("SFU7341");
                DataRowView drv = dgPrintList.SelectedItem as DataRowView;
                if (drv != null && (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached))
                {
                    if (dgPrintList.SelectedIndex > -1)
                    {
                        dgPrintList.EndNewRow(true);
                        dgPrintList.RemoveRow(dgPrintList.SelectedIndex);
                    }
                }
            }
            dgPrintList.Focus();
        }

        #endregion

        #region 메뉴산출 함수 (내부사용)
        private void SetProcessPack_LabelPrint(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("LABELPRINTIUSE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = string.IsNullOrEmpty(sAREAID) ? null : sAREAID;
                dr["EQSGID"] = sFilter[0] == "" ? null : sFilter[0];
                dr["LABELPRINTIUSE"] = sFilter[1] == "" ? null : sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_PACK_CBO_BY_LABELPRINTIUSE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
  
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public Boolean GetIsDuplicationLabel()
        {
            DataTable result = new DataTable();
            Boolean isDuplSkip = false;
            try
            {
                int rowidx = dgPrintList.CurrentRow.Index;
                string sEQSGID = Util.ToString(DataTableConverter.GetValue(dgPrintList.Rows[rowidx].DataItem, "EQSGID"));
                string sPROCID = Util.ToString(DataTableConverter.GetValue(dgPrintList.Rows[rowidx].DataItem, "PROCID"));
                string sEQPTID = Util.ToString(DataTableConverter.GetValue(dgPrintList.Rows[rowidx].DataItem, "EQPTID"));
                string sPRODID = Util.ToString(DataTableConverter.GetValue(dgPrintList.Rows[rowidx].DataItem, "PRODID"));

                string[] sfilter = new string[]
                {
                    sEQSGID,
                    sPROCID,
                    sEQPTID,
                    null,
                    null,
                    sPRODID
               };
                //DB 상 동일 내역이 있는지 조회
                result = GetPrintInfo(sfilter);
                if (result.Rows.Count > 0)
                {
                    isDuplSkip = true;
                }else
                {
                    isDuplSkip = false;
                }
                int itrueCnt = 0;
                int ifalseCnt = 0;
                if (!isDuplSkip)
                {
                    //GRID 상 동일 내역이 있는지 조회
                    for (int i = 0; i < dgPrintList.Rows.Count; i++)
                    {
                        if (dgPrintList.CurrentRow.Index != i)
                        {
                            string[] targetRow = new string[]
                            {
                                Util.ToString(DataTableConverter.GetValue(dgPrintList.Rows[i].DataItem, "EQSGID")),
                                Util.ToString(DataTableConverter.GetValue(dgPrintList.Rows[i].DataItem, "PROCID")),
                                Util.ToString(DataTableConverter.GetValue(dgPrintList.Rows[i].DataItem, "EQPTID")),
                                Util.ToString(DataTableConverter.GetValue(dgPrintList.Rows[i].DataItem, "PRODID"))
                            };
                            if (sfilter[0].Equals(targetRow[0]) &&
                                sfilter[1].Equals(targetRow[1]) &&
                                sfilter[2].Equals(targetRow[2]) &&
                                sfilter[5].Equals(targetRow[3])
                                )
                            {
                                itrueCnt++;
                            }
                            else
                            {
                                ifalseCnt++;
                            }
                        }
                    }
                    if (itrueCnt > 0)
                    {
                        isDuplSkip = true;
                    }
                    else
                    {
                        isDuplSkip = false;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return isDuplSkip;
            }
            return isDuplSkip;
        }

        private void SetEquipment_sum(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[0] == "" ? null : sFilter[0];
                dr["PROCID"] = sFilter[1] == "" ? null : sFilter[1];
                RQSTDT.Rows.Add(dr);
                
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_EQSGID_PROCID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                
                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetProcid_sum(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = sFilter[0] == "" ? null : sFilter[0];
                dr["AREAID"] = sFilter[1] == "" ? null : sFilter[1];
                dr["EQSGID"] = sFilter[2] == "" ? null : sFilter[2];
                dr["PROCID"] = sFilter[3] == "" ? null : sFilter[3];
                dr["PRDT_CLSS_CODE"] = sFilter[4] == "" ? null : sFilter[4];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetLabelCode_sum(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRODID"] = sFilter[0] == "" ? null : sFilter[0];
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABELCODE_BY_PRODID_CD_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
 
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetLabelDPI_sum(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PRINTER_RESOLUTION";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();


                    cbo.SelectedValue = LoginInfo.CFG_PROC_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetProdid_sum(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LABEL_CODE"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODID_BY_LABELCOD_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();


                cbo.SelectedValue = LoginInfo.CFG_PROC_ID;
                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        public void CommonMesMasterDataBaseCombo(string bizRuleName, C1ComboBox cbo, string[] arrColumn, string[] arrCondition, CommonCombo.ComboStatus status, string selectedValueText, string displayMemberText, string selectedValue = null)
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.TableName = "RQSTDT";

                if (arrColumn != null)
                {
                    // 동적 컬럼 생성 및 Row 추가
                    foreach (string col in arrColumn)
                    {
                        inDataTable.Columns.Add(col, typeof(string));
                    }

                    DataRow dr = inDataTable.NewRow();
                    for (int i = 0; i < inDataTable.Columns.Count; i++)
                    {
                        dr[inDataTable.Columns[i].ColumnName] = arrCondition[i];
                    }
                    inDataTable.Rows.Add(dr);
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { selectedValueText, displayMemberText });

                cbo.ItemsSource = AddStatus(dtBinding, status, selectedValueText, displayMemberText).Copy().AsDataView();
                cbo.SelectedIndex = 0;

                if (!string.IsNullOrEmpty(selectedValue))
                    cbo.SelectedValue = selectedValue;
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        void UpdateRowView(C1.WPF.DataGrid.DataGridRow dgr, C1.WPF.DataGrid.DataGridColumn dgc)
        {
            try
            {
                DataRowView drv = dgr.DataItem as DataRowView;

                if (drv != null && Convert.ToString(dgc.Name) == "EQSGID")
                {
                    _manualCommit = true;
                    this.dgPrintList.EndEditRow(true);
                }

                if (drv != null && Convert.ToString(dgc.Name) == "PROCID")
                {
                    _manualCommit = true;
                    this.dgPrintList.EndEditRow(true);
                }
            }
            finally
            {
                _manualCommit = false;
            }
        }

        //행추가
        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int addRowCount = Convert.ToInt32(this.numAddCount.Value);

                for (int i = 0; i < addRowCount; i++)
                {
                    this.dgPrintList.BeginNewRow();
                    this.dgPrintList.EndNewRow(true);
                }
            }
            catch (Exception ex)
            {
                FrameOperation.PrintMessage(MessageDic.Instance.GetMessage(ex));
            }
        }

        //신규 행 추가 처리시 컬럼별 기본값 설정
        private void dgPrintList_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            e.Item.SetValue("CHK", true);
            e.Item.SetValue("USE_FLAG", "Y");
            e.Item.SetValue("EQSGID", (string)cboEquipmentSegment.SelectedValue);
            e.Item.SetValue("PROCID", (string)cboProcess.SelectedValue);
            e.Item.SetValue("USERNAME", LoginInfo.USERNAME);
            e.Item.SetValue("INSDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            e.Item.SetValue("UPDDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            e.Item.SetValue("INSUSER", LoginInfo.USERNAME);
            e.Item.SetValue("UPDUSER", LoginInfo.USERNAME);
        }

        //체크시 수정가능한 컬럼 처리
        private void dgPrintList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if(e.Column == this.dgPrintList.Columns["USE_FLAG"])
            {
                sBeforeUse_flag = Convert.ToString(dgPrintList.CurrentCell.Value);
            }
            DataRowView drv = e.Row.DataItem as DataRowView;

            if (drv["CHK"].SafeToString() != "True" && e.Column != dgPrintList.Columns["CHK"])
            {
                e.Cancel = true;
                return;
            }
            if (e.Column == this.dgPrintList.Columns["TURN_LABEL_CODE"]) { 
                if (drv["TURN_TYPE_CODE"].SafeToString().Equals("L"))
            {
                e.Cancel = false;
            }else { 
                e.Cancel = true;
                return;
            }
            }

            if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
            {
                e.Cancel = false;
            }
            else
            {
                if (e.Column != this.dgPrintList.Columns["CHK"]
                 && e.Column != this.dgPrintList.Columns["USE_FLAG"]
                 && e.Column != this.dgPrintList.Columns["PRODID"]
                 && e.Column != this.dgPrintList.Columns["LABEL_CODE"]
                 && e.Column != this.dgPrintList.Columns["TURN_LABEL_CODE"]
                 && e.Column != this.dgPrintList.Columns["TURN_TYPE_CODE"]
                 && e.Column != this.dgPrintList.Columns["LABEL_PRT_NAME"]
                 && e.Column != this.dgPrintList.Columns["PRTR_DPI"]
                 && e.Column != this.dgPrintList.Columns["PRT_X"]
                 && e.Column != this.dgPrintList.Columns["PRT_Y"]
                 && e.Column != this.dgPrintList.Columns["PRT_DARKNESS"]
                 && e.Column != this.dgPrintList.Columns["PRTR_IP"]
                 && e.Column != this.dgPrintList.Columns["PRTR_PORT"]
                 && e.Column != this.dgPrintList.Columns["PRT_QTY"]
                 && e.Column != this.dgPrintList.Columns["SCAN_ID_PRFX"]
                 && e.Column != this.dgPrintList.Columns["TURN_STRT_DTTM"]
                 && e.Column != this.dgPrintList.Columns["TURN_END_DTTM"]
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

        //편집 처리후 값 재설정
        private void dgPrintList_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            try
            {
                C1.WPF.C1ComboBox cbo = e.EditingElement as C1.WPF.C1ComboBox;

                if (cbo != null)
                {
                    string selectedValueText = "CBO_CODE";
                    string displayMemberText = "CBO_NAME";
                    string bizRuleName = "";
                    string[] arrColumn = new string[] { "", "" };
                    string[] arrCondition = new string[] { "", "" };
                    string sEQSGID = string.Empty;
                    string sPROCESS = string.Empty;
                    string sLABELCODE = string.Empty;
                    string sPRODID = string.Empty;
                    switch (Convert.ToString(e.Column.Name))
                    {
                        case "EQSGID":
                            bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
                            arrColumn = new string[] { "LANGID", "AREAID" };
                            arrCondition = new string[] { LoginInfo.LANGID, sAREAID };
                            break;
                        case "PROCID":
                            sEQSGID = (string)DataTableConverter.GetValue(e.Row.DataItem, "EQSGID");
                            bizRuleName = "DA_BAS_SEL_PROCESS_PACK_CBO_BY_LABELPRINTIUSE";
                            arrColumn = new string[] { "LANGID", "AREAID", "EQSGID", "LABELPRINTIUSE" };
                            arrCondition = new string[] { LoginInfo.LANGID, sAREAID, sEQSGID, "Y" };
                            break;
                        case "EQPTID":
                            sEQSGID = (string)DataTableConverter.GetValue(e.Row.DataItem, "EQSGID");
                            sPROCESS = (string)DataTableConverter.GetValue(e.Row.DataItem, "PROCID");
                            bizRuleName = "DA_BAS_SEL_EQUIPMENT_BY_EQSGID_PROCID_CBO";
                            arrColumn = new string[] { "LANGID", "EQSGID", "PROCID" };
                            arrCondition = new string[] { LoginInfo.LANGID, sEQSGID, sPROCESS };
                            break;
                        case "PRODID":
                            sEQSGID = (string)DataTableConverter.GetValue(e.Row.DataItem, "EQSGID");
                            sPROCESS = (string)DataTableConverter.GetValue(e.Row.DataItem, "PROCID");
                            bizRuleName = "DA_BAS_SEL_PRODUCT_MULTI_CBO";
                            arrColumn = new string[] { "LANGID", "SHOPID", "AREAID", "EQSGID", "PROCID", "PRDT_CLSS_CODE" };
                            arrCondition = new string[] { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, sEQSGID, sPROCESS, "CMA,BMA" };
                            break;
                        case "LABEL_CODE":
                            sPRODID = (string)DataTableConverter.GetValue(e.Row.DataItem, "PRODID");
                            sPROCESS = (string)DataTableConverter.GetValue(e.Row.DataItem, "PROCID");
                            bizRuleName = "DA_PRD_SEL_LABELCODE_BY_PRODID_CD_CBO";
                            arrColumn = new string[] { "LANGID", "PRODID", "SHOPID" };
                            arrCondition = new string[] { LoginInfo.LANGID, sPRODID, LoginInfo.CFG_SHOP_ID };
                            break;
                        case "TURN_LABEL_CODE":
                            sPRODID = (string)DataTableConverter.GetValue(e.Row.DataItem, "PRODID");
                            sPROCESS = (string)DataTableConverter.GetValue(e.Row.DataItem, "PROCID");
                            bizRuleName = "DA_PRD_SEL_LABELCODE_BY_PRODID_CD_CBO";
                            arrColumn = new string[] { "LANGID", "PRODID", "SHOPID" };
                            arrCondition = new string[] { LoginInfo.LANGID, sPRODID, LoginInfo.CFG_SHOP_ID };
                            break;
                        case "TURN_TYPE_CODE":
                            bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
                            arrColumn = new string[] { "LANGID", "CMCDTYPE" };
                            arrCondition = new string[] { LoginInfo.LANGID, "TURN_TYPE_CODE" };
                            break;
                        case "PRTR_DPI":
                            bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
                            arrColumn = new string[] { "LANGID", "CMCDTYPE"};
                            arrCondition = new string[] { LoginInfo.LANGID, "PRINTER_RESOLUTION"};
                            break;
                        case "USE_FLAG":
                            DataTable dt = new DataTable();
                            dt.Columns.Add("CBO_NAME", typeof(string));
                            dt.Columns.Add("CBO_CODE", typeof(string));

                            DataRow dr = dt.NewRow();
                            dr["CBO_NAME"] = "Y : " + ObjectDic.Instance.GetObjectName("사용");
                            dr["CBO_CODE"] = "Y";
                            dt.Rows.Add(dr);

                            DataRow dr1 = dt.NewRow();
                            dr1["CBO_NAME"] = "N : " + ObjectDic.Instance.GetObjectName("사용 안함");
                            dr1["CBO_CODE"] = "N";
                            dt.Rows.Add(dr1);

                            dt.AcceptChanges();

                            cbo.ItemsSource = AddStatus(dt, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText).Copy().AsDataView();
                            cbo.SelectedIndex = 0;
                            break;
                        default:
                            break;
                    }
                    if (!Convert.ToString(e.Column.Name).Equals("USE_FLAG"))
                    {
                        CommonMesMasterDataBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText);
                    }
                    cbo.EditCompleted += delegate (object sender1, EventArgs e1)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this.UpdateRowView(e.Row, e.Column);
                        }));
                    };
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //셀내용 적용시 처리
        private void dgPrintList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            string[] arrColumn = new string[] { "", "" };
            string[] arrCondition = new string[] { "", "" };
            string sEQSGID = string.Empty;
            string sPROCESS = string.Empty;
            if (!dg.CurrentCell.IsEditing)
            {
                switch (dg.CurrentCell.Column.Name)
                {
                    case "EQSGID":
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGID")?.ToString().Length > 0)
                            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", new string[] { "LANGID", "AREAID" }, new string[] { LoginInfo.LANGID, sAREAID }, CommonCombo.ComboStatus.NONE, dgPrintList.Columns["EQSGID"], "CBO_CODE", "CBO_NAME");
                        break;
                    case "PROCID":
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGID")?.ToString().Length > 0)
                            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_PROCESS_PACK_CBO_BY_LABELPRINTIUSE", new string[] { "LANGID", "AREAID", "LABELPRINTIUSE" }, new string[] { LoginInfo.LANGID, sAREAID, "Y" }, CommonCombo.ComboStatus.NONE, dgPrintList.Columns["PROCID"], "CBO_CODE", "CBO_NAME");
                        break;
                    case "EQPTID":
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGID")?.ToString().Length > 0)
                            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_EQUIPMENT_BY_EQSGID_PROCID_CBO", new string[] { "LANGID" }, new string[] { LoginInfo.LANGID }, CommonCombo.ComboStatus.NONE, dgPrintList.Columns["EQPTID"], "CBO_CODE", "CBO_NAME");
                        break;
                    case "PRODID":
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGID")?.ToString().Length > 0)
                        CommonCombo.SetDataGridComboItem("DA_BAS_SEL_PRODUCT_MULTI_CBO", new string[] { "LANGID", "SHOPID", "AREAID", "EQSGID", "PROCID", "PRDT_CLSS_CODE" }, new string[] { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, null, null, "CMA,BMA" }, CommonCombo.ComboStatus.NONE, dgPrintList.Columns["PRODID"], "CBO_CODE", "CBO_NAME");
                        DataTableConverter.SetValue(dgPrintList.Rows[dgPrintList.CurrentCell.Row.Index].DataItem, "LABEL_CODE", "");
                        e.Cell.SetValue("LABEL_CODE", string.Empty);
                        isLabelDuplication(); //[사용여부], [라벨코드] true 인것만 중복 체크
                        this.dgPrintList.EndNewRow(true);
                        
                        break;
                    case "LABEL_CODE":
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGID")?.ToString().Length > 0)
                            CommonCombo.SetDataGridComboItem("DA_PRD_SEL_LABELCODE_BY_PRODID_CD_CBO", new string[] { "LANGID", "PRODID", "SHOPID" }, new string[] { LoginInfo.LANGID, null, LoginInfo.CFG_SHOP_ID }, CommonCombo.ComboStatus.NONE, dgPrintList.Columns["LABEL_CODE"], "CBO_CODE", "CBO_NAME");
                        DataTableConverter.SetValue(e.Cell.Row.DataItem, "LABEL_PRT_NAME", Convert.ToString(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTID")) + "_"+ Convert.ToString(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LABEL_CODE")) + "_" + Convert.ToString(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRODID")));
                        break;
                    case "TURN_LABEL_CODE":
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGID")?.ToString().Length > 0)
                            CommonCombo.SetDataGridComboItem("DA_PRD_SEL_LABELCODE_BY_PRODID_CD_CBO", new string[] { "LANGID", "PRODID", "SHOPID" }, new string[] { LoginInfo.LANGID, null, LoginInfo.CFG_SHOP_ID }, CommonCombo.ComboStatus.NONE, dgPrintList.Columns["TRUN_LABEL_CODE"], "CBO_CODE", "CBO_NAME");
                        break;
                    case "TURN_TYPE_CODE":
                        CommonCombo.SetDataGridComboItem("DA_BAS_SEL_COMMCODE_CBO", new string[] { "LANGID", "CMCDTYPE" }, new string[] { LoginInfo.LANGID, "TURN_TYPE_CODE" }, CommonCombo.ComboStatus.NONE, dgPrintList.Columns["TURN_TYPE_CODE"], "CBO_CODE", "CBO_NAME");
                        if (!string.IsNullOrEmpty(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TURN_LABEL_CODE")?.ToString()))
                        {
                            if (!(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TURN_LABEL_CODE")?.ToString()).Equals("L"))
                            {
                                DataTableConverter.SetValue(dgPrintList.Rows[dgPrintList.CurrentCell.Row.Index].DataItem, "TURN_LABEL_CODE", "");
                                e.Cell.SetValue("TURN_LABEL_CODE", string.Empty);
                                this.dgPrintList.EndNewRow(true);
                            }
                        }
                        break;
                    case "USE_FLAG":
                        SetDataGridComboItem(CommonCombo.ComboStatus.NONE, dgPrintList.Columns["USE_FLAG"], "CBO_CODE", "CBO_NAME");
                        break;
                    case "PRTR_DPI":
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGID")?.ToString().Length > 0)
                            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_COMMCODE_CBO", new string[] { "LANGID","CMCDTYPE" }, new string[] { LoginInfo.LANGID, "PRINTER_RESOLUTION" }, CommonCombo.ComboStatus.NONE, dgPrintList.Columns["PRTR_DPI"], "CBO_CODE", "CBO_NAME");
                        isNumber(Convert.ToString(dg.CurrentCell.Value));
                        break;
                    case "PRT_X":
                        isNumber(Convert.ToString(dg.CurrentCell.Value));
                        break;
                    case "PRT_Y":
                        isNumber(Convert.ToString(dg.CurrentCell.Value));
                        break;
                    case "PRT_DARKNESS":
                        isNumber(Convert.ToString(dg.CurrentCell.Value));
                        break;
                    case "PRTR_PORT":
                        isNumber(Convert.ToString(dg.CurrentCell.Value));
                        break;
                    case "PRTR_IP":
                        IsIPv4(Convert.ToString(dg.CurrentCell.Value));
                        break;
                    case "TURN_STRT_DTTM":
                        IsDateTime(Convert.ToString(dg.CurrentCell.Value));
                        break;
                    case "TURN_END_DTTM" :
                        IsDateTime(Convert.ToString(dg.CurrentCell.Value));
                        break;
                    default:
                        break;
                }
            }
        }
        

        // 행 삭제
        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < this.numAddCount.Value.SafeToInt32() ; i++)
                {
                    DataRowView drv = dgPrintList.SelectedItem as DataRowView;
                    if (drv != null && (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached))
                    {
                        if (dgPrintList.SelectedIndex > -1)
                        {
                            dgPrintList.EndNewRow(true);
                            dgPrintList.RemoveRow(dgPrintList.SelectedIndex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //전체 행 체크
        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgPrintList.Rows)
            {
                if (true)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", true);
                }
                else
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", "Y");
                }
            }
            dgPrintList.EndEdit();
            dgPrintList.EndEditRow(true);
        }

        //전체 행 체크 해제
        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgPrintList.Rows)
            {
                if (true)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", false);
                }
                else
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", "N");
                }
            }
            dgPrintList.EndEdit();
            dgPrintList.EndEditRow(true);
        }

        private void dgPrintList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgPrintList.GetCellFromPoint(pnt);
            if (cell != null)
            {
                if (cell.Row.Index > -1)
                {
                    string sLABEL_CODE = Util.NVC(DataTableConverter.GetValue(dgPrintList.Rows[cell.Row.Index].DataItem, "LABEL_CODE"));
                    string sTURN_LABEL_CODE = Util.NVC(DataTableConverter.GetValue(dgPrintList.Rows[cell.Row.Index].DataItem, "TURN_LABEL_CODE"));

                    string sEQSGID = Util.NVC(DataTableConverter.GetValue(dgPrintList.Rows[cell.Row.Index].DataItem, "EQSGID"));
                    string sPRODID = Util.NVC(DataTableConverter.GetValue(dgPrintList.Rows[cell.Row.Index].DataItem, "PRODID"));
                    string sPROCID = Util.NVC(DataTableConverter.GetValue(dgPrintList.Rows[cell.Row.Index].DataItem, "PROCID"));
                    string sEQPTID = Util.NVC(DataTableConverter.GetValue(dgPrintList.Rows[cell.Row.Index].DataItem, "EQPTID"));
                    if (cell.Column.Name == "EQPTID")
                    {
                        popUpOpenPalletInfo(sEQSGID, sPROCID, sEQPTID);
                    }
                    else if (cell.Column.Name == "LABEL_CODE")
                    {
                        if(!string.IsNullOrEmpty(sLABEL_CODE) && !string.IsNullOrEmpty(sPRODID))
                        popUpOpenPrintItem(sLABEL_CODE, sEQSGID, sPRODID);
                    }
                    else if (cell.Column.Name == "TURN_LABEL_CODE")
                    {
                        if (!string.IsNullOrEmpty(sTURN_LABEL_CODE) && !string.IsNullOrEmpty(sPRODID))
                            popUpOpenPrintItem(sTURN_LABEL_CODE, sEQSGID, sPRODID);
                    }
                }
            }
        }
        private void popUpOpenPalletInfo(string sEQSGID, string sPROCID, string sEQPTID)
        {
            try
            {
                PACK001_043_POPUP popup = new PACK001_043_POPUP();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = sEQSGID;
                    Parameters[1] = sPROCID;
                    Parameters[2] = sEQPTID;
                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void popUpOpenPrintItem(string sLABELCODE, string sEQSGID, string sPRODID)
        {
            try
            {
                PACK001_000_LABEL_CHECK_POPUP popup = new PACK001_000_LABEL_CHECK_POPUP();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[5];
                    Parameters[0] = sLABELCODE;
                    Parameters[1] = sEQSGID;
                    Parameters[2] = sPRODID;
                    Parameters[3] = "";
                    Parameters[4] = "";
                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dgPrintList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "EQPTID" || e.Cell.Column.Name == "LABEL_CODE" || e.Cell.Column.Name == "TURN_LABEL_CODE")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
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
                Util.Alert(ex.ToString());
            }
        }

        private void cboProdid_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetLabelCode();
        }
    }
}
