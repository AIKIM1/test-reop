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
 Created Date : 2024.01.26
      Creator : 박나연
   Decription : 포장 Pallet 라벨 프린터 관리 
--------------------------------------------------------------------------------------
 [Change History]
  2024.01.26  박나연 : 최초 생성
**************************************************************************************/

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_327.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_327 : UserControl
    {
        CommonCombo _combo = new CommonCombo();
        CommonCombo_Form _combo_F = new CommonCombo_Form();
        DataTable dtMain = new DataTable();
        string sSHOPID = LoginInfo.CFG_SHOP_ID;
        string sAREAID = LoginInfo.CFG_AREA_ID;
        private bool _manualCommit = false;
        private DataTable isCreateTable = new DataTable();
        string sBeforeUse_flag = null;

        public BOX001_327()
        {
            InitializeComponent();
            //combobox 설정
            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        //컨트롤 상속 내역
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 미처리 
        }

        //combobox 설정
        private void InitCombo()
        {
            try
            {
                //동
                _combo_F.SetCombo(cboAreaByAreaType, CommonCombo_Form.ComboStatus.SELECT, sCase: "AREA_CP");
                //라인
                String[] sFilter = { cboAreaByAreaType.SelectedValue.ToString() };
                _combo_F.SetCombo(cboEquipmentSegment, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE");
                //설비
                String[] sFilter2 = new string[] { cboEquipmentSegment.SelectedValue.ToString(), "B1000", null };
                _combo_F.SetCombo(cboEquipment, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilter2, sCase: "EQUIPMENT");
                //제품코드
                String[] sFilter3 = new string[] { cboEquipmentSegment.SelectedValue.ToString(), null };
                _combo.SetCombo(cboProdid, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "PROD_MDL");
                //라벨
                String[] sFilter4 = new string[] { null, null, null, "PLT" };
                _combo.SetCombo(cboLabelCode, CommonCombo.ComboStatus.ALL, sFilter: sFilter4, sCase: "LABELCODE_BY_PROD");
                cboLabelCode.SelectedIndex = 0;
                //사용여부0
                setUseYN();
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
                cboUseFlag.SelectedIndex = 1; //default Y
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Combobox 선택 이벤트

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            String[] sFilter = new string[] { cboEquipmentSegment.SelectedValue.ToString(), "B1000", null };
            _combo_F.SetCombo(cboEquipment, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilter, sCase: "EQUIPMENT");

            String[] sFilter2 = new string[] { cboEquipmentSegment.SelectedValue.ToString(), null };
            _combo.SetCombo(cboProdid, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "PROD_MDL");
        }

        private void cboProdid_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProdid.SelectedIndex > -1)
            {
                String[] sFilter = new string[] { cboProdid.SelectedValue.ToString(), null, null, "PLT" };
                _combo.SetCombo(cboLabelCode, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "LABELCODE_BY_PROD");
                cboLabelCode.SelectedIndex = 0;
            }
        }

        #endregion


        //조회
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ShowLoadingIndicator();
            DoEvents();
            string[] sfilter = new string[] {
                cboEquipmentSegment.SelectedValue.ToString(),
                "B1000",
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

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
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

                DataRow newRow = inTable.NewRow();
                newRow["EQSGID"] = String.IsNullOrEmpty(filter[0]) || filter[0].Equals("ALL") ? null : filter[0];
                newRow["PROCID"] = String.IsNullOrEmpty(filter[1]) || filter[1].Equals("ALL") ? null : filter[1];
                newRow["EQPTID"] = String.IsNullOrEmpty(filter[2]) || filter[2].Equals("ALL") ? null : filter[2];
                newRow["USE_FLAG"] = String.IsNullOrEmpty(filter[3]) || filter[3].Equals("ALL") ? null : filter[3];
                newRow["LABEL_CODE"] = String.IsNullOrEmpty(filter[4]) || filter[4].Equals("ALL") ? null : filter[4];
                newRow["PRODID"] = String.IsNullOrEmpty(filter[5]) || filter[5].Equals("ALL") ? null : filter[5];
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_EQPT_LABEL", "RQSTDT", "RSLTDT", inTable);

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

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
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
                    if (e.Cell.Column.Name == "EQPTID" || e.Cell.Column.Name == "LABEL_CODE")
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

        //체크시 수정가능한 컬럼 처리
        private void dgPrintList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column == this.dgPrintList.Columns["USE_FLAG"])
            {
                sBeforeUse_flag = Convert.ToString(dgPrintList.CurrentCell.Value);
            }
            DataRowView drv = e.Row.DataItem as DataRowView;

            if (drv["CHK"].SafeToString() != "True" && e.Column != dgPrintList.Columns["CHK"])
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
                if (e.Column != this.dgPrintList.Columns["CHK"]
                 && e.Column != this.dgPrintList.Columns["USE_FLAG"]
                 && e.Column != this.dgPrintList.Columns["PRODID"]
                 && e.Column != this.dgPrintList.Columns["LABEL_CODE"]
                 && e.Column != this.dgPrintList.Columns["LABEL_PRT_NAME"]
                 && e.Column != this.dgPrintList.Columns["TURN_TYPE_CODE"]
                 && e.Column != this.dgPrintList.Columns["PRTR_DPI"]
                 && e.Column != this.dgPrintList.Columns["PRT_X"]
                 && e.Column != this.dgPrintList.Columns["PRT_Y"]
                 && e.Column != this.dgPrintList.Columns["PRT_DARKNESS"]
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

        //셀내용 적용시 처리
        private void dgPrintList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            string[] arrColumn = new string[] { "", "" };
            string[] arrCondition = new string[] { "", "" };
            string sEQSGID = string.Empty;

            if (!dg.CurrentCell.IsEditing)
            {
                switch (dg.CurrentCell.Column.Name)
                {
                    case "EQSGID":
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGID")?.ToString().Length > 0)
                            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_COMBO_LINE", new string[] { "LANGID", "AREAID" }, new string[] { LoginInfo.LANGID, sAREAID }, CommonCombo.ComboStatus.NONE, dgPrintList.Columns["EQSGID"], "CBO_CODE", "CBO_NAME");
                        break;
                    case "EQPTID":
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGID")?.ToString().Length > 0)
                            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_EQUIPMENT_CBO", new string[] { "LANGID", "EQSGID", "PROCID" }, new string[] { LoginInfo.LANGID, null, "B1000" }, CommonCombo.ComboStatus.NONE, dgPrintList.Columns["EQPTID"], "CBO_CODE", "CBO_NAME");
                        break;
                    case "PRODID":
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGID")?.ToString().Length > 0)
                            CommonCombo.SetDataGridComboItem("DA_PRD_SEL_PRODID_CBO_CP", new string[] { "LANGID", "EQSGID", "MDLLOT_ID" }, new string[] { LoginInfo.LANGID, null, null }, CommonCombo.ComboStatus.NONE, dgPrintList.Columns["PRODID"], "CBO_CODE", "CBO_NAME");
                        DataTableConverter.SetValue(dgPrintList.Rows[dgPrintList.CurrentCell.Row.Index].DataItem, "LABEL_CODE", "");
                        e.Cell.SetValue("LABEL_CODE", string.Empty);
                        isLabelDuplication(); //[사용여부], [라벨코드] true 인것만 중복 체크
                        this.dgPrintList.EndNewRow(true);
                        break;
                    case "LABEL_CODE":
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGID")?.ToString().Length > 0)
                            CommonCombo.SetDataGridComboItem("DA_PRD_SEL_LABELCODE_BY_PRODID_CBO", new string[] { "LANGID", "PRODID", "SHIPTO_ID", "PROCID", "SHOPID", "LABEL_TYPE_CODE", "LABEL_CODE" }, new string[] { LoginInfo.LANGID, null, null, "B1000", LoginInfo.CFG_SHOP_ID, "PLT", null }, CommonCombo.ComboStatus.NONE, dgPrintList.Columns["LABEL_CODE"], "CBO_CODE", "CBO_NAME");
                        DataTableConverter.SetValue(e.Cell.Row.DataItem, "LABEL_PRT_NAME", Convert.ToString(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTID")) + "_" + Convert.ToString(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LABEL_CODE")) + "_" + Convert.ToString(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRODID")));
                        break;
                    case "TURN_TYPE_CODE":
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGID")?.ToString().Length > 0)
                            CommonCombo.SetDataGridComboItem("DA_PRD_SEL_SHIPTO_CBO_CP", new string[] { "LANGID", "SHOPID", "EQSGID", "AREAID" }, new string[] { LoginInfo.LANGID, sSHOPID, null, sAREAID }, CommonCombo.ComboStatus.NONE, dgPrintList.Columns["TURN_TYPE_CODE"], "CBO_CODE", "CBO_NAME");
                        break;
                    case "USE_FLAG":
                        SetDataGridComboItem(CommonCombo.ComboStatus.NONE, dgPrintList.Columns["USE_FLAG"], "CBO_CODE", "CBO_NAME");
                        break;
                    case "PRTR_DPI":
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGID")?.ToString().Length > 0)
                            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_COMMCODE_CBO", new string[] { "LANGID", "CMCDTYPE" }, new string[] { LoginInfo.LANGID, "PRINTER_RESOLUTION" }, CommonCombo.ComboStatus.NONE, dgPrintList.Columns["PRTR_DPI"], "CBO_CODE", "CBO_NAME");
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
                    default:
                        break;
                }
            }
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

        public Boolean GetIsDuplicationLabel()
        {
            DataTable result = new DataTable();
            Boolean isDuplSkip = false;
            try
            {
                int rowidx = dgPrintList.CurrentRow.Index;
                string sEQSGID = Util.ToString(DataTableConverter.GetValue(dgPrintList.Rows[rowidx].DataItem, "EQSGID"));
                string sEQPTID = Util.ToString(DataTableConverter.GetValue(dgPrintList.Rows[rowidx].DataItem, "EQPTID"));
                string sPRODID = Util.ToString(DataTableConverter.GetValue(dgPrintList.Rows[rowidx].DataItem, "PRODID"));
                string sUSE_YN = Util.ToString(DataTableConverter.GetValue(dgPrintList.Rows[rowidx].DataItem, "USE_FLAG"));

                string[] sfilter = new string[]
                {
                    sEQSGID,
                    "B1000",
                    sEQPTID,
                    sUSE_YN,
                    null,
                    sPRODID,
               };
                //DB 상 동일 내역이 있는지 조회
                result = GetPrintInfo(sfilter);
                if (result.Rows.Count > 0)
                {
                    isDuplSkip = true;
                }
                else
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
                                Util.ToString(DataTableConverter.GetValue(dgPrintList.Rows[i].DataItem, "EQPTID")),
                                Util.ToString(DataTableConverter.GetValue(dgPrintList.Rows[i].DataItem, "PRODID"))
                            };
                            if (sfilter[0].Equals(targetRow[0]) &&
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
            }
            return isNumber;
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
                    string sLABELCODE = string.Empty;
                    string sPRODID = string.Empty;
                    switch (Convert.ToString(e.Column.Name))
                    {
                        case "EQSGID":
                            bizRuleName = "DA_BAS_SEL_COMBO_LINE";
                            arrColumn = new string[] { "LANGID", "AREAID" };
                            arrCondition = new string[] { LoginInfo.LANGID, sAREAID };
                            break;
                        case "EQPTID":
                            sEQSGID = (string)DataTableConverter.GetValue(e.Row.DataItem, "EQSGID");
                            bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
                            arrColumn = new string[] { "LANGID", "EQSGID", "PROCID" };
                            arrCondition = new string[] { LoginInfo.LANGID, sEQSGID, "B1000" };
                            break;
                        case "PRODID":
                            sEQSGID = (string)DataTableConverter.GetValue(e.Row.DataItem, "EQSGID");
                            bizRuleName = "DA_PRD_SEL_PRODID_CBO_CP";
                            arrColumn = new string[] { "LANGID", "EQSGID", "MDLLOT_ID" };
                            arrCondition = new string[] { LoginInfo.LANGID, sEQSGID, null };
                            break;
                        case "LABEL_CODE":
                            sPRODID = (string)DataTableConverter.GetValue(e.Row.DataItem, "PRODID");
                            bizRuleName = "DA_PRD_SEL_LABELCODE_BY_PRODID_CBO";
                            arrColumn = new string[] { "LANGID", "PRODID", "SHIPTO_ID", "PROCID", "SHOPID", "LABEL_TYPE_CODE", "LABEL_CODE" };
                            arrCondition = new string[] { LoginInfo.LANGID, sPRODID, null, "B1000", sSHOPID, "PLT", null };
                            break;
                        case "TURN_TYPE_CODE":
                            sEQSGID = (string)DataTableConverter.GetValue(e.Row.DataItem, "EQSGID");
                            bizRuleName = "DA_PRD_SEL_SHIPTO_CBO_CP";
                            arrColumn = new string[] { "LANGID", "SHOPID", "EQSGID", "AREAID" };
                            arrCondition = new string[] { LoginInfo.LANGID, sSHOPID, sEQSGID, sAREAID };
                            break;
                        case "PRTR_DPI":
                            bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
                            arrColumn = new string[] { "LANGID", "CMCDTYPE" };
                            arrCondition = new string[] { LoginInfo.LANGID, "PRINTER_RESOLUTION" };
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
            }
            finally
            {
                _manualCommit = false;
            }
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
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "TURN_TYPE_CODE"))))
                    {
                        Util.MessageValidation("SFU4096"); // 출하처를 선택하세요.
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
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "TURN_TYPE_CODE"))))
                    {
                        Util.MessageValidation("SFU4096"); // 출하처를 선택하세요.
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
                }
            }

            return true;
        }

        private void Save()
        {
            ShowLoadingIndicator();
            DoEvents();

            try
            {
                string bizRuleName = "BR_SET_PALLET_LABEL_PRINT";

                isCreateTable = DataTableConverter.Convert(dgPrintList.GetCurrentItems());
                if (!CommonVerify.HasDataGridRow(dgPrintList)) return;

                this.dgPrintList.EndEdit();
                this.dgPrintList.EndEditRow(true);

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("LABEL_CODE", typeof(string));
                inDataTable.Columns.Add("LABEL_PRT_NAME", typeof(string));
                inDataTable.Columns.Add("TURN_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("PRTR_DPI", typeof(string));
                inDataTable.Columns.Add("PRT_X", typeof(string));
                inDataTable.Columns.Add("PRT_Y", typeof(string));
                inDataTable.Columns.Add("PRT_DARKNESS", typeof(string));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));
                inDataTable.Columns.Add("USER", typeof(string));
                inDataTable.Columns.Add("SQLTYPE", typeof(string));

                foreach (object added in dgPrintList.GetAddedItems())
                {
                    if (DataTableConverter.GetValue(added, "CHK").Equals("True"))
                    {
                        DataRow param = inDataTable.NewRow();

                        param["EQSGID"] = DataTableConverter.GetValue(added, "EQSGID");
                        param["EQPTID"] = DataTableConverter.GetValue(added, "EQPTID");
                        param["PRODID"] = DataTableConverter.GetValue(added, "PRODID");
                        param["LABEL_CODE"] = DataTableConverter.GetValue(added, "LABEL_CODE");
                        param["LABEL_PRT_NAME"] = DataTableConverter.GetValue(added, "LABEL_PRT_NAME");
                        param["TURN_TYPE_CODE"] = DataTableConverter.GetValue(added, "TURN_TYPE_CODE");
                        param["PRTR_DPI"] = DataTableConverter.GetValue(added, "PRTR_DPI");
                        param["PRT_X"] = DataTableConverter.GetValue(added, "PRT_X");
                        param["PRT_Y"] = DataTableConverter.GetValue(added, "PRT_Y");
                        param["PRT_DARKNESS"] = DataTableConverter.GetValue(added, "PRT_DARKNESS");
                        param["USE_FLAG"] = DataTableConverter.GetValue(added, "USE_FLAG");
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
                        param["EQPTID"] = DataTableConverter.GetValue(modified, "EQPTID");
                        param["PRODID"] = DataTableConverter.GetValue(modified, "PRODID");
                        param["LABEL_CODE"] = DataTableConverter.GetValue(modified, "LABEL_CODE");
                        param["LABEL_PRT_NAME"] = DataTableConverter.GetValue(modified, "LABEL_PRT_NAME");
                        param["TURN_TYPE_CODE"] = DataTableConverter.GetValue(modified, "TURN_TYPE_CODE");
                        param["PRTR_DPI"] = DataTableConverter.GetValue(modified, "PRTR_DPI");
                        param["PRT_X"] = DataTableConverter.GetValue(modified, "PRT_X");
                        param["PRT_Y"] = DataTableConverter.GetValue(modified, "PRT_Y");
                        param["PRT_DARKNESS"] = DataTableConverter.GetValue(modified, "PRT_DARKNESS");
                        param["USE_FLAG"] = DataTableConverter.GetValue(modified, "USE_FLAG");
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
            e.Item.SetValue("USERNAME", LoginInfo.USERNAME);
            e.Item.SetValue("INSDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            e.Item.SetValue("UPDDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            e.Item.SetValue("INSUSER", LoginInfo.USERNAME);
            e.Item.SetValue("UPDUSER", LoginInfo.USERNAME);
        }

        // 행 삭제
        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < this.numAddCount.Value.SafeToInt32(); i++)
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
    }
}
