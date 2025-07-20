/*************************************************************************************
 Created Date : 2020.09.10
      Creator : ����
   Decription : ���� �� ȥ�� ���ؼ���
--------------------------------------------------------------------------------------
 [Change History]
  2020.09.10  DEVELOPER : Initial Created.
  2021.02.16  ���뼮    : Master Grid MouseUp �̺�Ʈ ����, �ʿ���� �ڵ� �� ������ Member�� ����
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_001 : UserControl, IWorkArea
    {
        private CommonCombo _combo = new CommonCombo();
        private string sSHOPID = LoginInfo.CFG_SHOP_ID;
        private string sAREAID = LoginInfo.CFG_AREA_ID;
        private DataTable dtMain = new DataTable();

        private DataTable isCreateTable = new DataTable();
        private int masterGridCurrentIndex = -1;
        private string _EQSGID = string.Empty;
        private string _TCODE = string.Empty;
        private string _PROD = string.Empty;
        private string _MCODE = string.Empty;
        private bool _Cancel = false;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Declaration & Constructor 
        public PACK003_001()
        {
            InitializeComponent();
            setComboBox();      // Combobox ����
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            listAuth.Add(btnActpopup);
            listAuth.Add(btnSavebtm);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        //Act Basic Mater Info Search
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ShowLoadingIndicator();
            DoEvents();
            Util.gridClear(dgActMasList);
            string[] sfilter = new string[] {
                cboActiveCode.SelectedValue.ToString(),
                cboEquipmentSegment.SelectedValue.ToString(),
                cboProdid.SelectedValue.ToString(),
                cboActbas.SelectedValue.ToString(),
                cboUseFlag.SelectedValue.ToString()
            };
            loadingIndicator.Visibility = System.Windows.Visibility.Visible;
            dtMain = GetPrintInfo(sfilter);
            dgActMasList.Refresh();
            txRowCnt.Text = string.IsNullOrEmpty(Convert.ToString(dtMain.Rows.Count)) ? "[ 0" + ObjectDic.Instance.GetObjectName("��") + "]" : "[ " + Convert.ToString(dtMain.Rows.Count) + ObjectDic.Instance.GetObjectName("��") + " ]";
            Util.GridSetData(dgActMasList, dtMain, FrameOperation);
            this.masterGridCurrentIndex = -1;
            loadingIndicator.Visibility = Visibility.Collapsed;
            HiddenLoadingIndicator();
        }

        //Excel
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgActMasList);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnExcel_btm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(this.dgActdetail);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Method
        //��뿩�� Combo ����
        private static DataTable GetUseYNAllData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CBO_NAME", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr_ = dt.NewRow();
            dr_["CBO_NAME"] = "-ALL-";
            dr_["CBO_CODE"] = "ALL";
            dt.Rows.Add(dr_);

            DataRow dr = dt.NewRow();
            dr["CBO_NAME"] = "Y : " + ObjectDic.Instance.GetObjectName("���");
            dr["CBO_CODE"] = "Y";
            dt.Rows.Add(dr);

            DataRow dr1 = dt.NewRow();
            dr1["CBO_NAME"] = "N : " + ObjectDic.Instance.GetObjectName("��� ����");
            dr1["CBO_CODE"] = "N";
            dt.Rows.Add(dr1);

            dt.AcceptChanges();
            return dt;
        }

        //Ȱ�� ���� Master ���� ��뿩���޺�
        private void setUseFlag()
        {
            try
            {
                DataTable dt = GetUseYNAllData();
                cboUseFlag.ItemsSource = DataTableConverter.Convert(dt);
                cboUseFlag.SelectedIndex = 0; //default Y
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Ȱ�� ���� Detail ���� ��뿩���޺�
        private void setUseYN()
        {
            try
            {
                DataTable dt = GetUseYNAllData();
                cboUseYN.ItemsSource = DataTableConverter.Convert(dt);
                cboUseYN.SelectedIndex = 0; //default Y
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setComboBox()
        {
            try
            {
                C1ComboBox cboSHOPID = new C1ComboBox();
                cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
                C1ComboBox cboArea = new C1ComboBox();
                cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;

                //Ȱ������
                String[] sFilter = { "PACK_UI_INPUT_MIX_TYPE_CODE" };
                _combo.SetCombo(cboActiveCode, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "COMMCODE");

                //����            
                //C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
                //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentSegmentParent, sCase: "EQUIPMENTSEGMENT");
                Set_Line_list();
                //��
                Set_Prodid_list();
                //C1ComboBox[] cboProdidParent = { cboEquipmentSegment, cboActiveCode };
                //_combo.SetCombo(cboProdid, CommonCombo.ComboStatus.ALL, cbParent: cboProdidParent, sCase: "INPUTMIXTYPECODE");

                //Ȱ������ 
                String[] sFilter_bas = { "PACK_UI_INPUT_MIX_CHK_MTHD_CODE" };
                _combo.SetCombo(cboActbas, CommonCombo.ComboStatus.ALL, sFilter: sFilter_bas, sCase: "COMMCODE");

                //��뿩��
                setUseFlag();

                setUseYN();
                //Grid ���� �޺��� ���� ����
                CommonCombo.SetDataGridComboItem("DA_BAS_SEL_LOGIS_LINE_CBO", new string[] { "LANGID", "AREAID" }, new string[] { LoginInfo.LANGID, sAREAID }, CommonCombo.ComboStatus.ALL, dgActMasList.Columns["EQSGID"], "CBO_CODE", "CBO_NAME");
                CommonCombo.SetDataGridComboItem("DA_BAS_SEL_PRODUCT_INPUT_MIX_TYPE_CBO", new string[] { "LANGID", "SHOPID", "AREAID" }, new string[] { LoginInfo.LANGID, sSHOPID, null }, CommonCombo.ComboStatus.ALL, dgActMasList.Columns["PRODID"], "CBO_CODE", "CBO_NAME");
                CommonCombo.SetDataGridComboItem("DA_BAS_SEL_COMMCODE_CBO", new string[] { "LANGID", "CMCDTYPE" }, new string[] { LoginInfo.LANGID, "PACK_UI_INPUT_MIX_TYPE_CODE" }, CommonCombo.ComboStatus.ALL, dgActMasList.Columns["INPUT_MIX_TYPE_CODE"], "CBO_CODE", "CBO_NAME");
                CommonCombo.SetDataGridComboItem("DA_BAS_SEL_COMMCODE_CBO", new string[] { "LANGID", "CMCDTYPE" }, new string[] { LoginInfo.LANGID, "PACK_UI_INPUT_MIX_CHK_MTHD_CODE" }, CommonCombo.ComboStatus.ALL, dgActMasList.Columns["INPUT_MIX_CHK_MTHD_CODE"], "CBO_CODE", "CBO_NAME");

                SetDataGridComboItem(CommonCombo.ComboStatus.NONE, dgActMasList.Columns["USE_FLAG"], "CBO_CODE", "CBO_NAME");
                SetDataGridComboItem(CommonCombo.ComboStatus.NONE, dgActdetail.Columns["USE_FLAG"], "CBO_CODE", "CBO_NAME");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetActbas()
        {
            string[] filter = new string[] { cboActiveCode.SelectedValue.ToString() };
            SetcboActbas_sum(cboActbas, CommonCombo.ComboStatus.NONE, filter);
        }

        //Ȱ�������޺�
        private void SetcboActbas_sum(C1ComboBox cbo, CommonCombo.ComboStatus cs, string[] sFilter)
        {
            try
            {
                if (sFilter[0].ToString() == "CHK_INPUT" || sFilter[0].ToString() == "")
                {
                    sFilter[0] = null;
                }
                else
                {
                    sFilter[0] = "ALL";
                }

                DataTable dtAllRow = new DataTable();
                dtAllRow.Columns.AddRange(new DataColumn[] { new DataColumn("CBO_CODE"), new DataColumn("CBO_NAME") });

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_UI_INPUT_MIX_CHK_MTHD_CODE";
                dr["ATTRIBUTE1"] = sFilter[0] == "" ? null : sFilter[0];
                RQSTDT.Rows.Add(dr);

                if (sFilter[0] == null)
                {
                    dtAllRow.Rows.Add(new object[] { "", "-ALL-" });
                }
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dtAllRow.Merge(dtResult);
                }

                cboActbas.ItemsSource = DataTableConverter.Convert(dtAllRow);
                cboActbas.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //�����̵� �ڵ����� ������ �޺�
        private void Set_Linelist(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOGIS_LINE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        //����-���Ա��п� ���� ��ǰ Validation�� ���� �޺�
        private void SetInputMixTypeCode(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("MIX_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("LOGIS_YN", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = sFilter[0] == "" ? null : sFilter[0];
                dr["MODLID"] = null;
                dr["PRDT_CLSS_CODE"] = "CELL";
                dr["MIX_TYPE_CODE"] = sFilter[1] == "" ? null : sFilter[1];
                dr["LOGIS_YN"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_INPUT_MIX_TYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);
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

        public static void SetDataGridComboItem(CommonCombo.ComboStatus status, C1.WPF.DataGrid.DataGridColumn dgcol, string selectedValueText, string displayMemberText)
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("CBO_NAME", typeof(string));
                inDataTable.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["CBO_NAME"] = "Y : " + ObjectDic.Instance.GetObjectName("���");
                dr["CBO_CODE"] = "Y";
                inDataTable.Rows.Add(dr);

                DataRow dr1 = inDataTable.NewRow();
                dr1["CBO_NAME"] = "N : " + ObjectDic.Instance.GetObjectName("��� ����");
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

        private void Set_Line_list()
        {
            //����
            string[] filter = new string[] { LoginInfo.CFG_AREA_ID };
            Set_Linelist(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, filter);
        }

        private void Set_Prodid_list()
        {
            //��
            //string[] filter = new string[] { cboEquipmentSegment.SelectedValue.ToString(), cboActiveCode.SelectedValue.ToString() };
            string[] filter = new string[] { string.Empty, string.Empty };
            SetInputMixTypeCode(cboProdid, CommonCombo.ComboStatus.ALL, filter);
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

        // �� ���� (Master ����)
        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < this.numAddCount.Value.SafeToInt32(); i++)
                {
                    DataRowView drv = dgActMasList.SelectedItem as DataRowView;
                    if (drv != null && (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached))
                    {
                        if (dgActMasList.SelectedIndex > -1)
                        {
                            dgActMasList.EndNewRow(true);
                            dgActMasList.RemoveRow(dgActMasList.SelectedIndex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        // List Search(Master)
        public DataTable GetPrintInfo(string[] filter)
        {
            DataTable result = new DataTable();
            try
            {
                Util.gridClear(dgActdetail);
                DataTable inTable = new DataTable();

                inTable.Columns.Add("CHK", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("INPUT_MIX_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("INPUT_MIX_CHK_MTHD_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["CHK"] = "N";
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["INPUT_MIX_TYPE_CODE"] = String.IsNullOrEmpty(filter[0]) || filter[0].Equals("ALL") ? null : filter[0];
                newRow["EQSGID"] = String.IsNullOrEmpty(filter[1]) || filter[1].Equals("ALL") ? null : filter[1];
                newRow["PRODID"] = String.IsNullOrEmpty(filter[2]) || filter[2].Equals("ALL") ? null : filter[2];
                newRow["INPUT_MIX_CHK_MTHD_CODE"] = String.IsNullOrEmpty(filter[3]) || filter[3].Equals("ALL") ? null : filter[3];
                newRow["USE_FLAG"] = String.IsNullOrEmpty(filter[4]) || filter[4].Equals("ALL") ? null : filter[4];
                inTable.Rows.Add(newRow);

                result = new ClientProxy().ExecuteServiceSync("DA_SEL_TB_SFC_INPUT_MIX_CHK_MST", "RQSTDT", "RSLTDT", inTable);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Logger.Instance.WriteLine(Logger.OPERATION_C + "DA_SEL_TB_SFC_INPUT_MIX_CHK_MST", ex);
                return result;
            }
            finally
            {
                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                Logger.Instance.WriteLine(Logger.OPERATION_C + "DA_SEL_TB_SFC_INPUT_MIX_CHK_MST", Logger.MESSAGE_OPERATION_END);
            }
            return result;
        }

        //Combo �������� : ALL, N/A, SELECT
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

        private void chkHeaderAllList_Checked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgActMasList.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }

            dgActMasList.EndEdit();
            dgActMasList.EndEditRow(true);
        }

        private void chkHeaderAllList_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgActMasList.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", false);
            }
            dgActMasList.EndEdit();
            dgActMasList.EndEditRow(true);
        }

        private void chkHeaderAllList_btm_Checked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgActdetail.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }
            dgActdetail.EndEdit();
            dgActdetail.EndEditRow(true);
        }

        private void chkHeaderAllList_btm_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgActdetail.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", false);
            }
            dgActdetail.EndEdit();
            dgActdetail.EndEditRow(true);
        }

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.SelectedIndex > -1)
            {
                Set_Prodid_list();
            }
        }

        private void cboActiveCode_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboActiveCode.SelectedIndex > -1)
            {
                SetActbas();
                if (cboEquipmentSegment.SelectedValue != null)
                    Set_Prodid_list();
            }
        }

        #region Ȱ�� ���� Master ���� ����(���λ��)
        public void CommonMesMasterDataBaseCombo(string bizRuleName, C1ComboBox cbo, string[] arrColumn, string[] arrCondition, CommonCombo.ComboStatus status, string selectedValueText, string displayMemberText, string selectedValue = null)
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.TableName = "RQSTDT";

                if (arrColumn != null)
                {
                    // ���� �÷� ���� �� Row �߰�
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

        private void UpdateRowView(C1.WPF.DataGrid.DataGridRow dgr, C1.WPF.DataGrid.DataGridColumn dgc)
        {
            try
            {
                DataRowView drv = dgr.DataItem as DataRowView;

                if (drv != null && Convert.ToString(dgc.Name) == "EQSGID")
                {
                    this.dgActMasList.EndEditRow(true);
                }

                if (drv != null && Convert.ToString(dgc.Name) == "INPUT_MIX_TYPE_CODE")
                {
                    this.dgActMasList.EndEditRow(true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgActMasList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        // �� �߰�(Master ����)
        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
            {
                Util.Alert("SFU4591"); //�۾��ڸ� �Է��ϼ���
                this.ucPersonInfo.Focus();
                return;
            }
            if (this.dgActMasList == null || this.dgActMasList.Rows.Count <= 0)
            {
                Util.Alert("9059"); // �����͸� ��ȸ �Ͻʽÿ�.
                this.btnSearch.Focus();
                return;
            }
            try
            {
                DataTable dt = Util.MakeDataTable(dgActMasList, true);
                int addRowCount = Convert.ToInt32(this.numAddCount.Value);

                for (int i = 0; i < addRowCount; i++)
                {
                    dgActMasList.CanUserAddRows = true;
                    dgActMasList.BeginNewRow();
                    dgActMasList.EndNewRow(true);
                    dgActMasList.CanUserAddRows = false;
                }
                //addRowCount = addRowCount - 1;
            }
            catch (Exception ex)
            {
                FrameOperation.PrintMessage(MessageDic.Instance.GetMessage(ex));
            }
        }

        //�� �߰� �� ������
        private void dgActMasList_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            this.masterGridCurrentIndex = -1;
            e.Item.SetValue("CHK", true);
            e.Item.SetValue("USE_FLAG", "Y");
            e.Item.SetValue("INSDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            e.Item.SetValue("UPDDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            e.Item.SetValue("INSUSER", LoginInfo.USERID);
            e.Item.SetValue("UPDUSER", this.ucPersonInfo.UserID);
        }

        //üũ�� ���������� �÷� ó��
        private void dgActMasList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            DataRowView dataRowView = e.Row.DataItem as DataRowView;

            // SearchDetail
            if (this.masterGridCurrentIndex != e.Row.Index)
            {
                this.masterGridCurrentIndex = e.Row.Index;
                string eqsgID = Util.NVC(DataTableConverter.GetValue(dataRowView, "EQSGID"));
                string inputMixTypeCode = Util.NVC(DataTableConverter.GetValue(dataRowView, "INPUT_MIX_TYPE_CODE"));
                string prodID = Util.NVC(DataTableConverter.GetValue(dataRowView, "PRODID"));
                string inputMixChkMthdCode = Util.NVC(DataTableConverter.GetValue(dataRowView, "INPUT_MIX_CHK_MTHD_CODE"));
                switch (dataRowView.Row.RowState)
                {
                    case DataRowState.Added:
                    case DataRowState.Detached:
                        if (e.Column.Name.ToUpper().Equals("CHK") || e.Column.Name.ToUpper().Equals("USE_FLAG"))
                        {
                            SetCellStockDetail(eqsgID, inputMixTypeCode, prodID, inputMixChkMthdCode);
                        }
                        break;
                    default:
                        SetCellStockDetail(eqsgID, inputMixTypeCode, prodID, inputMixChkMthdCode);
                        break;
                }
            }

            if (dataRowView["CHK"].SafeToString() != "True" && e.Column != dgActMasList.Columns["CHK"])
            {
                e.Cancel = true;
                _Cancel = false;
                return;
            }

            if (dataRowView.Row.RowState == DataRowState.Added || dataRowView.Row.RowState == DataRowState.Detached)
            {
                e.Cancel = false;
                _Cancel = true;
            }
            else
            {
                if (e.Column != this.dgActMasList.Columns["CHK"]
                 && e.Column != this.dgActMasList.Columns["USE_FLAG"]
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

        //Master Grid ���� ó���� �� �缳��
        private void dgActMasList_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            try
            {
                C1.WPF.C1ComboBox cbo = e.EditingElement as C1.WPF.C1ComboBox;
                DataTableConverter.SetValue(e.Row.DataItem, "CHK", true);
                if (cbo != null)
                {
                    string selectedValueText = "CBO_CODE";
                    string displayMemberText = "CBO_NAME";
                    string bizRuleName = "";
                    string sInmixcode = string.Empty;
                    //DataTableConverter.GetValue(dgActMasList.Rows[e.Row.Index].DataItem, "INPUT_MIX_TYPE_CODE")
                    string[] arrColumn = new string[] { "", "" };
                    string[] arrCondition = new string[] { "", "" };
                    string sEQSGID = string.Empty;
                    string sLOGIS_YN = string.Empty;
                    string sINPUT_MIX_TYPE_CODE = string.Empty;
                    string sCMCDTYPE = string.Empty;
                    string sPRODID = string.Empty;
                    string sInput = Util.NVC(DataTableConverter.GetValue(dgActMasList.Rows[e.Row.Index].DataItem, "INPUT_MIX_TYPE_CODE"));
                    if (sInput == "CHK_INPUT" || sInput == "" || sInput == null)
                    {
                        sInmixcode = null;
                    }
                    else
                    {
                        sInmixcode = "ALL";
                    }
                    switch (Convert.ToString(e.Column.Name))
                    {
                        case "EQSGID":
                            bizRuleName = "DA_BAS_SEL_LOGIS_LINE_CBO";
                            arrColumn = new string[] { "LANGID", "AREAID" };
                            arrCondition = new string[] { LoginInfo.LANGID, sAREAID };
                            break;
                        case "INPUT_MIX_TYPE_CODE":
                            bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
                            arrColumn = new string[] { "LANGID", "CMCDTYPE" };
                            arrCondition = new string[] { LoginInfo.LANGID, "PACK_UI_INPUT_MIX_TYPE_CODE" };
                            break;
                        case "PRODID":
                            if (sInput == "CHK_INPUT")
                            {
                                sEQSGID = (string)DataTableConverter.GetValue(e.Row.DataItem, "EQSGID");
                                sLOGIS_YN = string.Empty;
                            }
                            else
                            {
                                sEQSGID = null;
                                sLOGIS_YN = "Y";
                            }
                            //sEQSGID = (string)DataTableConverter.GetValue(e.Row.DataItem, "EQSGID");
                            sINPUT_MIX_TYPE_CODE = (string)DataTableConverter.GetValue(e.Row.DataItem, "INPUT_MIX_TYPE_CODE");
                            bizRuleName = "DA_BAS_SEL_PRODUCT_INPUT_MIX_TYPE_CBO";
                            arrColumn = new string[] { "LANGID", "SHOPID", "AREAID", "EQSGID", "MIX_TYPE_CODE", "LOGIS_YN" };
                            arrCondition = new string[] { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, sEQSGID, sINPUT_MIX_TYPE_CODE, sLOGIS_YN };
                            break;
                        case "INPUT_MIX_CHK_MTHD_CODE":
                            bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTE";
                            arrColumn = new string[] { "LANGID", "CMCDTYPE", "ATTRIBUTE1" };
                            arrCondition = new string[] { LoginInfo.LANGID, "PACK_UI_INPUT_MIX_CHK_MTHD_CODE", sInmixcode };
                            break;
                        case "USE_FLAG":
                            DataTable dt = new DataTable();
                            dt.Columns.Add("CBO_NAME", typeof(string));
                            dt.Columns.Add("CBO_CODE", typeof(string));

                            DataRow dr = dt.NewRow();
                            dr["CBO_NAME"] = "Y : " + ObjectDic.Instance.GetObjectName("���");
                            dr["CBO_CODE"] = "Y";
                            dt.Rows.Add(dr);

                            DataRow dr1 = dt.NewRow();
                            dr1["CBO_NAME"] = "N : " + ObjectDic.Instance.GetObjectName("��� ����");
                            dr1["CBO_CODE"] = "N";
                            dt.Rows.Add(dr1);

                            dt.AcceptChanges();

                            cbo.ItemsSource = AddStatus(dt, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText).Copy().AsDataView();
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

        //Master Grid �缳�� �� �ݿ�
        private void dgActMasList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";
            string[] arrColumn = new string[] { "", "" };
            string[] arrCondition = new string[] { "", "" };
            string sEQSGID = string.Empty;
            string sINPUT_MIX_TYPE_CODE = string.Empty;
            string sCMCDTYPE = string.Empty;
            string sPRODID = string.Empty;
            if (!dg.CurrentCell.IsEditing)
            {
                switch (dg.CurrentCell.Column.Name)
                {
                    case "EQSGID":
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGID")?.ToString().Length > 0)
                            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_LOGIS_LINE_CBO", new string[] { "LANGID", "AREAID" }, new string[] { LoginInfo.LANGID, sAREAID }, CommonCombo.ComboStatus.NONE, dgActMasList.Columns["EQSGID"], selectedValueText, displayMemberText);
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGID")?.ToString().Length > 0)
                            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_LOGIS_LINE_CBO", new string[] { "LANGID", "AREAID" }, new string[] { LoginInfo.LANGID, sAREAID }, CommonCombo.ComboStatus.NONE, dgActMasList.Columns["EQSGNAME"], selectedValueText, displayMemberText);
                        break;
                    case "INPUT_MIX_TYPE_CODE":
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGID")?.ToString().Length > 0)
                            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_COMMCODE_CBO", new string[] { "LANGID", "CMCDTYPE" }, new string[] { LoginInfo.LANGID, "PACK_UI_INPUT_MIX_TYPE_CODE" }, CommonCombo.ComboStatus.NONE, dgActMasList.Columns["INPUT_MIX_TYPE_CODE"], selectedValueText, displayMemberText);
                        break;
                    case "PRODID":
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGID")?.ToString().Length > 0 && DataTableConverter.GetValue(e.Cell.Row.DataItem, "INPUT_MIX_TYPE_CODE")?.ToString().Length > 0)
                            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_PRODUCT_INPUT_MIX_TYPE_CBO", new string[] { "LANGID", "SHOPID", "AREAID", "EQSGID", "MIX_TYPE_CODE" }, new string[] { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, null, null }, CommonCombo.ComboStatus.NONE, dgActMasList.Columns["PRODID"], selectedValueText, displayMemberText);
                        break;
                    case "INPUT_MIX_CHK_MTHD_CODE":
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGID")?.ToString().Length > 0)
                            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_COMMCODE_CBO", new string[] { "LANGID", "CMCDTYPE" }, new string[] { LoginInfo.LANGID, "PACK_UI_INPUT_MIX_CHK_MTHD_CODE" }, CommonCombo.ComboStatus.NONE, dgActMasList.Columns["INPUT_MIX_CHK_MTHD_CODE"], selectedValueText, displayMemberText);
                        break;
                    case "USE_FLAG":
                        SetDataGridComboItem(CommonCombo.ComboStatus.NONE, dgActMasList.Columns["USE_FLAG"], selectedValueText, displayMemberText);
                        break;
                    default:
                        break;
                }
            }
            dgActMasList.Refresh();
        }
        #endregion

        #region Ȱ������ Detail ���� ����
        //Detail Search
        private void SearchDetail()
        {
            if (string.IsNullOrEmpty(this._EQSGID) || string.IsNullOrEmpty(this._TCODE) || string.IsNullOrEmpty(this._PROD) || string.IsNullOrEmpty(this._MCODE))
            {
                return;
            }
            SetCellStockDetail(this._EQSGID, this._TCODE, this._PROD, this._MCODE);
        }

        private void SetCellStockDetail(string sEqsgid, string sMixtypecode, string sProdid, string sCrrMixmthdcode)
        {
            try
            {
                txDetailRowCnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";
                Util.gridClear(dgActdetail);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("INPUT_MIX_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("INPUT_MIX_CHK_MTHD_CODE", typeof(string));
                RQSTDT.Columns.Add("INPUT_MIX_CHK_ITEM_CODE", typeof(string));
                RQSTDT.Columns.Add("CHK_VALUE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["INPUT_MIX_TYPE_CODE"] = sMixtypecode;
                dr["EQSGID"] = sEqsgid;
                dr["PRODID"] = sProdid;
                dr["INPUT_MIX_CHK_MTHD_CODE"] = sCrrMixmthdcode;
                dr["INPUT_MIX_CHK_ITEM_CODE"] = null;
                dr["CHK_VALUE"] = null;
                dr["USE_FLAG"] = cboUseYN.SelectedValue.ToString();


                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_SEL_TB_SFC_SEL_INPUT_MIX_DETL_OPT", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    _EQSGID = sEqsgid;
                    _TCODE = sMixtypecode;
                    _PROD = sProdid;
                    _MCODE = sCrrMixmthdcode;

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgActdetail, dtResult, FrameOperation);

                        Util.SetTextBlockText_DataGridRowCount(txDetailRowCnt, Util.NVC(dtResult.Rows.Count));
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
        //Detail Grid ���������� ����
        private void dgActdetail_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            DataRowView drv = e.Row.DataItem as DataRowView;

            if (drv["CHK"].SafeToString() != "True" && e.Column != dgActdetail.Columns["CHK"])
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
                if (e.Column != this.dgActdetail.Columns["CHK"]
                 && e.Column != this.dgActdetail.Columns["USE_FLAG"]
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

        private void cboUseYN_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                txDetailRowCnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";
                if (dgActMasList.CurrentColumn == null || dgActMasList.CurrentRow == null)
                {
                    return;
                }

                int current_row = dgActMasList.CurrentRow.Index;
                DataRowView drv = dgActMasList.Rows[current_row].DataItem as DataRowView;
                if (drv == null)
                {
                    Util.MessageValidation("SFU5158");
                    return;
                }

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.TableName = "RQSTDT";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("INPUT_MIX_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("INPUT_MIX_CHK_MTHD_CODE", typeof(string));
                INDATA.Columns.Add("INPUT_MIX_CHK_ITEM_CODE", typeof(string));
                INDATA.Columns.Add("CHK_VALUE", typeof(string));
                INDATA.Columns.Add("USE_FLAG", typeof(string));

                DataRow drInData = null;
                drInData = INDATA.NewRow();
                drInData["LANGID"] = LoginInfo.LANGID;
                drInData["INPUT_MIX_TYPE_CODE"] = drv.Row.ItemArray[1].ToString();
                drInData["EQSGID"] = drv.Row.ItemArray[2].ToString();
                drInData["PRODID"] = drv.Row.ItemArray[4].ToString();
                drInData["INPUT_MIX_CHK_MTHD_CODE"] = drv.Row.ItemArray[5].ToString();
                drInData["INPUT_MIX_CHK_ITEM_CODE"] = null;
                drInData["CHK_VALUE"] = null;
                drInData["USE_FLAG"] = cboUseYN.SelectedValue.ToString();
                INDATA.Rows.Add(drInData);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_SEL_TB_SFC_SEL_INPUT_MIX_DETL_OPT", "RQSTDT", "RSLTDT", INDATA, (dtResult, ex) =>
                {
                    Util.gridClear(dgActdetail);
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgActdetail, dtResult, FrameOperation);
                        Util.SetTextBlockText_DataGridRowCount(txDetailRowCnt, Util.NVC(dtResult.Rows.Count));
                    }
                    dgActdetail.Refresh();
                    loadingIndicator.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        private bool ValidationMaster()
        {
            foreach (object added in dgActMasList.GetAddedItems())
            {
                if (DataTableConverter.GetValue(added, "CHK").Equals("True"))
                {
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "EQSGID"))))
                    {
                        Util.MessageValidation("MMD0047"); //������ ������ �ּ���.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "INPUT_MIX_TYPE_CODE"))))
                    {
                        Util.MessageValidation("10043"); //Ȱ���� ���� �����ϼ���
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PRODID"))))
                    {
                        Util.MessageValidation("MMD0030"); // ��ǰ�� ������ �ּ���.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "INPUT_MIX_CHK_MTHD_CODE"))))
                    {
                        Util.MessageValidation("SFU1961"); // ���� ������ �� �� �Ǿ����ϴ�.
                        return false;
                    }
                }
            }
            foreach (object added in dgActMasList.GetModifiedItems())
            {
                if (DataTableConverter.GetValue(added, "CHK").Equals("True"))
                {
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "EQSGID"))))
                    {
                        Util.MessageValidation("MMD0047"); //������ ������ �ּ���.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "INPUT_MIX_TYPE_CODE"))))
                    {
                        Util.MessageValidation("10043"); //Ȱ���� ���� �����ϼ���
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PRODID"))))
                    {
                        Util.MessageValidation("MMD0030"); // ��ǰ�� ������ �ּ���.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "INPUT_MIX_CHK_MTHD_CODE"))))
                    {
                        Util.MessageValidation("SFU1961"); // ���� ������ �� �� �Ǿ����ϴ�.
                        return false;
                    }
                }
            }
            return true;
        }

        private bool ValidationDetail()
        {
            foreach (object added in dgActMasList.GetAddedItems())
            {
                if (DataTableConverter.GetValue(added, "CHK").Equals("True"))
                {
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "EQSGID"))))
                    {
                        Util.MessageValidation("MMD0047"); //������ ������ �ּ���.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "INPUT_MIX_TYPE_CODE"))))
                    {
                        Util.MessageValidation("10043"); //Ȱ���� ���� �����ϼ���
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PRODID"))))
                    {
                        Util.MessageValidation("MMD0030"); // ��ǰ�� ������ �ּ���.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "INPUT_MIX_CHK_MTHD_CODE"))))
                    {
                        Util.MessageValidation("SFU1961"); // ���� ������ �� �� �Ǿ����ϴ�.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "INPUT_MIX_CHK_ITEM_CODE"))))
                    {
                        Util.MessageValidation("SFU1801"); //�Է� �����Ͱ� �������� �ʽ��ϴ�.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "CHK_VALUE"))))
                    {
                        Util.MessageValidation("SFU1801"); // �Է� �����Ͱ� �������� �ʽ��ϴ�.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "CHK_SEQNO"))))
                    {
                        Util.MessageValidation("SFU1801"); // �Է� �����Ͱ� �������� �ʽ��ϴ�.
                        return false;
                    }
                }
            }
            foreach (object modified in dgActMasList.GetModifiedItems())
            {
                if (DataTableConverter.GetValue(modified, "CHK").Equals("True"))
                {
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "EQSGID"))))
                    {
                        Util.MessageValidation("MMD0047"); //������ ������ �ּ���.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "INPUT_MIX_TYPE_CODE"))))
                    {
                        Util.MessageValidation("10043"); //Ȱ���� ���� �����ϼ���
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "PRODID"))))
                    {
                        Util.MessageValidation("MMD0030"); // ��ǰ�� ������ �ּ���.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "INPUT_MIX_CHK_MTHD_CODE"))))
                    {
                        Util.MessageValidation("SFU1801"); // �Է� �����Ͱ� �������� �ʽ��ϴ�.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "INPUT_MIX_CHK_ITEM_CODE"))))
                    {
                        Util.MessageValidation("SFU1801"); // �Է� �����Ͱ� �������� �ʽ��ϴ�.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "CHK_VALUE"))))
                    {
                        Util.MessageValidation("SFU1801"); //�Է� �����Ͱ� �������� �ʽ��ϴ�.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "CHK_SEQNO"))))
                    {
                        Util.MessageValidation("SFU1801"); // �Է� �����Ͱ� �������� �ʽ��ϴ�.
                        return false;
                    }
                }
            }
            return true;
        }

        //Act Master Save
        private void SaveMaster()
        {
            ShowLoadingIndicator();
            DoEvents();
            try
            {
                string bizRuleName = "BR_PRD_REG_TB_SFC_INPUT_MIX_CHK_MST";
                isCreateTable = DataTableConverter.Convert(dgActMasList.GetCurrentItems());
                if (!CommonVerify.HasDataGridRow(dgActMasList)) return;

                this.dgActMasList.EndEdit();
                this.dgActMasList.EndEditRow(true);

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("USER", typeof(string));
                inDataTable.Columns.Add("WORKUSER", typeof(string));

                DataTable inMstInfoTable = indataSet.Tables.Add("IN_MST_INFO");
                inMstInfoTable.Columns.Add("SQLTYPE", typeof(string));
                inMstInfoTable.Columns.Add("INPUT_MIX_TYPE_CODE", typeof(string));
                inMstInfoTable.Columns.Add("EQSGID", typeof(string));
                inMstInfoTable.Columns.Add("PRODID", typeof(string));
                inMstInfoTable.Columns.Add("INPUT_MIX_CHK_MTHD_CODE", typeof(string));
                inMstInfoTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow param = inDataTable.NewRow();

                param["LANGID"] = LoginInfo.LANGID;
                param["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                param["AREAID"] = LoginInfo.CFG_AREA_ID;
                param["USER"] = LoginInfo.USERID;
                param["WORKUSER"] = this.ucPersonInfo.UserID;

                inDataTable.Rows.Add(param);

                foreach (object added in dgActMasList.GetAddedItems())
                {
                    if (DataTableConverter.GetValue(added, "CHK").Equals("True"))
                    {
                        DataRow rparam = inMstInfoTable.NewRow();

                        rparam["SQLTYPE"] = "I";
                        rparam["INPUT_MIX_TYPE_CODE"] = DataTableConverter.GetValue(added, "INPUT_MIX_TYPE_CODE");
                        rparam["EQSGID"] = DataTableConverter.GetValue(added, "EQSGID");
                        rparam["PRODID"] = DataTableConverter.GetValue(added, "PRODID");
                        rparam["INPUT_MIX_CHK_MTHD_CODE"] = DataTableConverter.GetValue(added, "INPUT_MIX_CHK_MTHD_CODE");
                        rparam["USE_FLAG"] = DataTableConverter.GetValue(added, "USE_FLAG");

                        inMstInfoTable.Rows.Add(rparam);
                    }
                }

                foreach (object modified in dgActMasList.GetModifiedItems())
                {
                    if (DataTableConverter.GetValue(modified, "CHK").Equals("True"))
                    {
                        DataRow rparam = inMstInfoTable.NewRow();

                        rparam["SQLTYPE"] = "U";
                        rparam["INPUT_MIX_TYPE_CODE"] = DataTableConverter.GetValue(modified, "INPUT_MIX_TYPE_CODE");
                        rparam["EQSGID"] = DataTableConverter.GetValue(modified, "EQSGID");
                        rparam["PRODID"] = DataTableConverter.GetValue(modified, "PRODID");
                        rparam["INPUT_MIX_CHK_MTHD_CODE"] = DataTableConverter.GetValue(modified, "INPUT_MIX_CHK_MTHD_CODE");
                        rparam["USE_FLAG"] = DataTableConverter.GetValue(modified, "USE_FLAG");

                        inMstInfoTable.Rows.Add(rparam);
                    }
                }

                if (inDataTable.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU3538");
                    return;
                }
                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,IN_MST_INFO", null, indataSet);
                Util.MessageInfo("SFU2056", inMstInfoTable.Rows.Count);
                Util.gridClear(dgActMasList);

                inMstInfoTable = new DataTable();
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

        //Act Detail Save
        private void SaveDetail()
        {
            ShowLoadingIndicator();
            DoEvents();
            try
            {
                string bizRuleName = "BR_PRD_REG_TB_SFC_INPUT_MIX_CHK_MST_DETL";

                isCreateTable = DataTableConverter.Convert(dgActdetail.GetCurrentItems());
                if (!CommonVerify.HasDataGridRow(dgActdetail)) return;

                this.dgActdetail.EndEdit();
                this.dgActdetail.EndEditRow(true);

                DataSet indataSet = new DataSet();

                DataRowView drv = dgActMasList.CurrentRow.DataItem as DataRowView;

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("SQLTYPE", typeof(string));
                inDataTable.Columns.Add("INPUT_MIX_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("INPUT_MIX_CHK_MTHD_CODE", typeof(string));
                inDataTable.Columns.Add("USER", typeof(string));
                inDataTable.Columns.Add("WORKUSER", typeof(string));

                DataTable inDtlTable = indataSet.Tables.Add("IN_DTL_INFO");
                inDtlTable.Columns.Add("INPUT_MIX_CHK_ITEM_CODE", typeof(string));
                inDtlTable.Columns.Add("CHK_VALUE", typeof(string));
                inDtlTable.Columns.Add("CHK_SEQNO", typeof(string));
                inDtlTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow param = inDataTable.NewRow();

                param["LANGID"] = LoginInfo.LANGID;
                param["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                param["AREAID"] = LoginInfo.CFG_AREA_ID;
                param["SQLTYPE"] = "U";
                param["INPUT_MIX_TYPE_CODE"] = DataTableConverter.GetValue(drv, "INPUT_MIX_TYPE_CODE");
                param["EQSGID"] = DataTableConverter.GetValue(drv, "EQSGID");
                param["PRODID"] = DataTableConverter.GetValue(drv, "PRODID");
                param["INPUT_MIX_CHK_MTHD_CODE"] = DataTableConverter.GetValue(drv, "INPUT_MIX_CHK_MTHD_CODE");
                param["USER"] = LoginInfo.USERID;
                param["WORKUSER"] = this.ucPersonInfo.UserID;

                inDataTable.Rows.Add(param);

                foreach (object modified in dgActdetail.GetModifiedItems())
                {
                    if (DataTableConverter.GetValue(modified, "CHK").Equals("True"))
                    {
                        DataRow rparam = inDtlTable.NewRow();
                        rparam["INPUT_MIX_CHK_ITEM_CODE"] = DataTableConverter.GetValue(modified, "INPUT_MIX_CHK_ITEM_CODE");
                        rparam["CHK_VALUE"] = DataTableConverter.GetValue(modified, "CHK_VALUE");
                        rparam["CHK_SEQNO"] = DataTableConverter.GetValue(modified, "CHK_SEQNO");
                        rparam["USE_FLAG"] = DataTableConverter.GetValue(modified, "USE_FLAG");

                        inDtlTable.Rows.Add(rparam);
                    }
                }

                if (inDataTable.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU3538");
                    return;
                }
                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,IN_DTL_INFO", null, indataSet);
                Util.MessageInfo("SFU2056", inDtlTable.Rows.Count);
                Util.gridClear(dgActdetail);

                inDataTable = new DataTable();
                inDtlTable = new DataTable();
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

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
            {
                Util.Alert("SFU4591"); // �۾��ڸ� �Է��ϼ���
                this.ucPersonInfo.Focus();
                return;
            }

            if (this.dgActMasList == null || this.dgActMasList.Rows.Count <= 0)
            {
                Util.Alert("9059"); // �����͸� ��ȸ �Ͻʽÿ�.
                this.btnSearch.Focus();
                return;
            }

            if (!ValidationMaster()) return;
            Util.MessageConfirm("SFU3533", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveMaster();
                    DoEvents();
                    btnSearch_Click(null, null);
                }
            });
        }

        private void btnSavebtm_Click(object sender, RoutedEventArgs e)
        {
            if (this.dgActMasList == null || this.dgActMasList.Rows.Count <= 0)
            {
                Util.Alert("9059"); // �����͸� ��ȸ �Ͻʽÿ�.
                this.btnSearch.Focus();
                return;
            }

            if (this.dgActdetail == null || this.dgActdetail.Rows.Count <= 0)
            {
                Util.Alert("SFU2816"); // ��ȸ ����� �����ϴ�.
                this.dgActMasList.Focus();
                return;
            }

            if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
            {
                Util.Alert("SFU4591"); //�۾��ڸ� �Է��ϼ���
                this.ucPersonInfo.Focus();
                return;
            }

            //if (dgActdetail.Rows.Count > 0 && dgActdetail.SelectedIndex > -1)
            if (dgActdetail.Rows.Count > 0)
            {
                DataRowView drv = dgActMasList.CurrentRow.DataItem as DataRowView;

                string sUseFlag = Util.NVC(DataTableConverter.GetValue(dgActMasList.Rows[dgActMasList.SelectedIndex].DataItem, "USE_FLAG"));
                if (sUseFlag != "N")
                {
                    if (!ValidationDetail()) return;
                    Util.MessageConfirm("SFU3533", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            SaveDetail();
                            DoEvents();
                            //btnSearch_Click(null, null);
                            // �����ͱ׸��忡�� ���õ� row�� ������ �������� detail ��ȸ
                            this.SearchDetail();
                        }
                    });
                }
                else
                {
                    Util.MessageValidation("SFU5157");
                }

            }
        }

        //Detail �� �Է� POPUP (Ȱ���� �Է�)
        private void btnActpopup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
                {
                    Util.Alert("SFU4591"); //�۾��ڸ� �Է��ϼ���
                    this.ucPersonInfo.Focus();
                    return;
                }

                if (_Cancel == true)
                {
                    return;
                }
                if (dgActMasList.Rows.Count > 0 && dgActMasList.SelectedIndex > -1)
                {
                    DataRowView drv = dgActMasList.CurrentRow.DataItem as DataRowView;

                    string sUseFlag = Util.NVC(DataTableConverter.GetValue(dgActMasList.Rows[dgActMasList.SelectedIndex].DataItem, "USE_FLAG"));
                    if (sUseFlag != "N")
                    {
                        if (drv != null && drv.Row.ItemArray.Length > 0)
                        {
                            PACK003_001_POPUP popup = new PACK003_001_POPUP();
                            popup.FrameOperation = this.FrameOperation;
                            if (popup != null)
                            {
                                object[] Parameters = new object[6];
                                Parameters[0] = drv.Row.ItemArray[1];
                                Parameters[1] = drv.Row.ItemArray[2];
                                Parameters[2] = drv.Row.ItemArray[4];
                                Parameters[3] = drv.Row.ItemArray[5];
                                Parameters[4] = drv.Row.ItemArray[6];
                                Parameters[5] = this.ucPersonInfo.UserID;

                                C1WindowExtension.SetParameters(popup, Parameters);

                                popup.Closed += new EventHandler(SetEqptend_Closed);

                                popup.ShowModal();
                                popup.CenterOnScreen();
                            }
                        }
                    }
                    else
                    {
                        Util.MessageValidation("SFU5157");
                        Util.gridClear(dgActdetail);
                    }


                }
                else
                {
                    Util.MessageValidation("SFU5158");
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetEqptend_Closed(object sender, EventArgs e)
        {
            PACK003_001 window = sender as PACK003_001;

            SetCellStockDetail(_EQSGID, _TCODE, _PROD, _MCODE);

        }
    }
}
