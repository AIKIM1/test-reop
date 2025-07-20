/*************************************************************************************
 Created Date : 2020.06.01
      Creator : KIL YONG KIM
   Decription : Pack Lot이력- OFFLINE-MODE DATA 교체 팝업
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Threading;
using System.Windows.Media;
using System.Linq;
using System.IO;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_005_CHANGEOFFDATA : C1Window, IWorkArea
    {
        CommonCombo _combo = new CommonCombo();
        string sBeforeUse_flag = null;
        int TotalRow;
        string sCHGTYPE = "PACK_UI_INFO_AFT_CHG_SHIP_HIST";
        private DataTable isCreateTable = new DataTable();
        string sSEQ = string.Empty;

        string dt1;
        string dt2;

        int rows1 = 0;

        #region Declaration & Constructor 

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_005_CHANGEOFFDATA()
        {
            InitializeComponent();
            //combobox 설정
            InitCombo();
        }
        #endregion

        #region Initialize

        #endregion

        #region ComboBox
        private void InitCombo()
        {
            try
            {
                //사용여부0
                setUseYN();
                CommonCombo.SetDataGridComboItem("DA_BAS_SEL_COMMCODE_CBO", new string[] { "LANGID", "CMCDTYPE" }, new string[] { LoginInfo.LANGID, sCHGTYPE }, CommonCombo.ComboStatus.NONE, dgKeyPart.Columns["CHG_TYPE"], "CBO_CODE", "CBO_NAME");

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
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

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnOK);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null)
                {
                    DataTable dtText = tmps[0] as DataTable;
                    if (dtText.Rows.Count > 0)
                    {
                        txtLot.Text = Util.NVC(dtText.Rows[0]["LOTID"]);
                        getKeypart(true, txtLot.Text);

                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //사용여부 Combo처리
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

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation()) return;
            Util.MessageConfirm("SFU3533", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save();
                    getKeypart(true, txtLot.Text);
                    DoEvents();
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //if(this.DialogResult != MessageBoxResult.OK)
            //{
            //    this.DialogResult = MessageBoxResult.OK;
            //}
            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }

        private void txtLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLot.Text.Length > 0)
                    {
                        getKeypart(true, txtLot.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                clearLotInfoText();
                Util.Alert(ex.ToString());
            }
        }


        private void txtSEQ_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        #endregion

        #region Mehod

        private void getKeypart(bool bMainLot_SubLot_Flag, string sLotid)
        {


            DataSet dsResult = null;
            try
            {
                if (bMainLot_SubLot_Flag)
                {
                    clearLotInfoText();
                }

                txtLot.Text = sLotid;
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "RQSTDT";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotid;
                INDATA.Rows.Add(dr);

                dsInput.Tables.Add(INDATA);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_TB_SFC_AFTER_SHIP_LOT_CHG_HIST", "INDATA", "LOT_OFFMODE_HIST,LOT_WIP", dsInput, null);

                if (dsResult != null && dsResult.Tables.Count > 0)
                {
                    int iInfoExistChk = 0;

                    txtLot.Tag = sLotid; //조회후 lot id 기억.

                    if ((dsResult.Tables.IndexOf("LOT_WIP") > -1) && dsResult.Tables["LOT_WIP"].Rows.Count > 0)
                    {
                        if (bMainLot_SubLot_Flag)
                        {
                            setLotInfoText(dsResult.Tables["LOT_WIP"]);
                        }

                        //sLotid = Util.NVC(dsResult.Tables["LOTID"].Rows[0]["LOTID"]);
                        //txtLot.Tag = sLotid;

                        //iInfoExistChk += dsResult.Tables["LOTID"].Rows.Count;

                    }

                    if ((dsResult.Tables.IndexOf("LOT_OFFMODE_HIST") > -1) && dsResult.Tables["LOT_OFFMODE_HIST"].Rows.Count > 0)
                    {
                        //dgDetailData.ItemsSource = DataTableConverter.Convert(dsResult.Tables["WIPDATACOLLECT_E"]);
                        Util.GridSetData(dgKeyPart, dsResult.Tables["LOT_OFFMODE_HIST"], FrameOperation, true);
                        iInfoExistChk += dsResult.Tables["LOT_OFFMODE_HIST"].Rows.Count;
                    }
                }
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void clearLotInfoText()
        {
            try
            {
                txtLotInfoCreateDate.Text = "";
                txtLotInfoProductId.Text = "";
                txtLotInfoWipState.Text = "";
                txtLotInfoWipProcess.Text = "";
                txtLotInfoWipLine.Text = "";
                txtLotInfoWipLine.Tag = null;

                Util.gridClear(dgKeyPart);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setLotInfoText(DataTable dtLotInfo)
        {
            try
            {
                if (dtLotInfo != null)
                {
                    if (dtLotInfo.Rows.Count > 0)
                    {
                        txtLotInfoCreateDate.Text = Util.NVC(dtLotInfo.Rows[0]["LOTDTTM_CR"]);
                        txtLotInfoProductId.Text = Util.NVC(dtLotInfo.Rows[0]["PRODID"]);
                        txtLotInfoWipState.Text = Util.NVC(dtLotInfo.Rows[0]["WIPSNAME"]);
                        txtLotInfoWipProcess.Text = Util.NVC(dtLotInfo.Rows[0]["PROCNAME"]);
                        txtLotInfoWipLine.Text = Util.NVC(dtLotInfo.Rows[0]["EQSGNAME"]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
        //행 추가
        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = Util.MakeDataTable(dgKeyPart,true);
                int addRowCount = Convert.ToInt32(this.numAddCount.Value);

                if (dt == null || dt.Rows.Count.ToString().Equals("0"))
                {
                    DataRow dr = dt.NewRow();
                    dr["CHK"] = true;
                    dr["LOTID"] = txtLot.Text.ToString();
                    dr["CHG_TYPE"] = "";
                    dr["SEQ"] = "";
                    dr["NOTE"] = "";
                    dr["PRE_VALUE"] = "";
                    dr["INSUSER"] = LoginInfo.USERID;
                    dr["INSDTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    dr["AFTER_VALUE"] = "";
                    dr["UPDUSER"] = LoginInfo.USERID;
                    dr["UPDDTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    dt.Rows.Add(dr);

                    Util.GridSetData(dgKeyPart, dt, FrameOperation);
                    addRowCount = addRowCount - 1;
                }

                for (int i = 0; i < addRowCount; i++)
                {
                    dgKeyPart.CanUserAddRows = true;
                    dgKeyPart.BeginNewRow();
                    dgKeyPart.EndNewRow(true);
                    dgKeyPart.CanUserAddRows = false;
                }
            }
            catch (Exception ex)
            {
                FrameOperation.PrintMessage(MessageDic.Instance.GetMessage(ex));
            }
        }
        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < this.numAddCount.Value.SafeToInt32(); i++)
                {
                    DataRowView drv = dgKeyPart.SelectedItem as DataRowView;
                    if (drv != null && (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached))
                    {
                        if (dgKeyPart.SelectedIndex > -1)
                        {
                            dgKeyPart.EndNewRow(true);
                            dgKeyPart.RemoveRow(dgKeyPart.SelectedIndex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgKeyPart_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            e.Item.SetValue("CHK", true);
            e.Item.SetValue("LOTID", txtLot.Text);
            e.Item.SetValue("CHG_TYPE", null);
            e.Item.SetValue("SEQ", null);
            e.Item.SetValue("NOTE", null);
            e.Item.SetValue("PRE_VALUE", null);
            e.Item.SetValue("INSUSER", LoginInfo.USERID);
            e.Item.SetValue("INSDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            e.Item.SetValue("AFTER_VALUE", null);
            e.Item.SetValue("UPDUSER", LoginInfo.USERID);
            e.Item.SetValue("UPDDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        }

        private void dgKeyPart_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            DataRowView drv = e.Row.DataItem as DataRowView;
            //if (drv["CHK"].SafeToString() != "True" && e.Column != dgKeyPart.Columns["CHK"])
            //{
            //    e.Cancel = true;
            //    return;
            //}
            //if (!drv["CHG_TYPE"].IsNullOrEmpty())
            //{
            //    e.Column.IsReadOnly = true;
            //}
            if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
            {
                e.Cancel = false;
            }
            if (dgKeyPart.Columns["CHG_TYPE"] == e.Column || dgKeyPart.Columns["NOTE"] == e.Column|| dgKeyPart.Columns["PRE_VALUE"] == e.Column|| dgKeyPart.Columns["AFTER_VALUE"] == e.Column)
            {
                if (!drv["SEQ"].IsNullOrEmpty())
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    e.Cancel = false;
                }
            }
            else
            {
                if (e.Column != this.dgKeyPart.Columns["CHK"]
                 && e.Column != this.dgKeyPart.Columns["LOTID"]
                 && e.Column != this.dgKeyPart.Columns["CHG_TYPE"]
                 && e.Column != this.dgKeyPart.Columns["SEQ"]
                 && e.Column != this.dgKeyPart.Columns["NOTE"]
                 && e.Column != this.dgKeyPart.Columns["PRE_VALUE"]
                 && e.Column != this.dgKeyPart.Columns["INSUSER"]
                 && e.Column != this.dgKeyPart.Columns["INSDTTM"]
                 && e.Column != this.dgKeyPart.Columns["AFTER_VALUE"]
                 && e.Column != this.dgKeyPart.Columns["UPDUSER"]
                 && e.Column != this.dgKeyPart.Columns["UPDDTTM"]
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

        public void SetGridComboItem(C1.WPF.DataGrid.DataGridColumn Col, CommonCombo.ComboStatus ComboStatus)
        {
            if (Col == null) return;
            string bizRuleName = "";
            string[] arrColumn = new string[] { "", "" };
            string[] arrCondition = new string[] { "", "" };
            string sCHGTYPE = string.Empty;
            switch (Convert.ToString(Col.Name))
            {
                case "CHG_TYPE":
                    bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
                    arrColumn = new string[] { "LANGID", "CMCDTYPE" };
                    arrCondition = new string[] { LoginInfo.LANGID, "PACK_UI_INFO_AFT_CHG_SHIP_HIST" };
                    break;
                default:
                    break;
            }

            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";
            SetGridComboItem(bizRuleName, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, Col, selectedValueText, displayMemberText);
        }

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

        private void SetCHGCode_sum(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CHG_TYPE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CHG_TYPE"] = sFilter[0] == "" ? null : sFilter[0];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {

            foreach (C1.WPF.DataGrid.DataGridRow row in dgKeyPart.Rows)
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


            dgKeyPart.EndEdit();
            dgKeyPart.EndEditRow(true);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgKeyPart.Rows)
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
            dgKeyPart.EndEdit();
            dgKeyPart.EndEditRow(true);
        }

        private void dgKeyPart_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //string sColName = dgKeyPart.CurrentColumn.Name;
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgKeyPart.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    if (cell.Column.Name == "CHG_TYPE")
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgKeyPart_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name == "NOTE" || e.Cell.Column.Name == "PRE_VALUE" || e.Cell.Column.Name == "AFTER_VALUE")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    //else if (e.Cell.Column.Name == "SEQ" || e.Cell.Column.Name == "INSUSER" || e.Cell.Column.Name == "INSDTTM" || e.Cell.Column.Name == "UPDUSER" || e.Cell.Column.Name == "UPDDTTM")
                    //{
                    //    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                    //    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    //}
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
        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
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
        private void Save()
        {
            ShowLoadingIndicator();
            DoEvents();

            try
            {
                string bizRuleName = "BR_PRD_REG_TB_SFC_LOT_CHG_HIST";

                isCreateTable = DataTableConverter.Convert(dgKeyPart.GetCurrentItems());
                if (!CommonVerify.HasDataGridRow(dgKeyPart)) return;

                this.dgKeyPart.EndEdit();
                this.dgKeyPart.EndEditRow(true);

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                //inDataTable.Columns.Add("INPUT_SEQNO", typeof(decimal));
                inDataTable.Columns.Add("INPUT_SEQNO", typeof(string)); // 2024.11.01. 김영국 - Datatype 변경 (decimal -> string) : 임성운 선임 요청.
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("CHG_TYPE", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("PRE_VALUE", typeof(string));
                inDataTable.Columns.Add("AFTER_VALUE", typeof(string));
                inDataTable.Columns.Add("CHG_DTTM", typeof(DateTime));
                inDataTable.Columns.Add("INSUSER", typeof(string));
                inDataTable.Columns.Add("INSDTTM", typeof(DateTime));
                inDataTable.Columns.Add("UPDUSER", typeof(string));
                inDataTable.Columns.Add("UPDDTTM", typeof(DateTime));
                inDataTable.Columns.Add("SQLTYPE", typeof(string));


                foreach (object added in dgKeyPart.GetAddedItems())
                {
                    if (DataTableConverter.GetValue(added, "CHK").Equals("True"))
                    {
                        DataRow param = inDataTable.NewRow();

                        param["LOTID"] = DataTableConverter.GetValue(added, "LOTID");
                        param["CHG_TYPE"] = DataTableConverter.GetValue(added, "CHG_TYPE");
                        param["NOTE"] = DataTableConverter.GetValue(added, "NOTE");
                        param["PRE_VALUE"] = DataTableConverter.GetValue(added, "PRE_VALUE");
                        param["AFTER_VALUE"] = DataTableConverter.GetValue(added, "AFTER_VALUE");
                        //param["CHG_DTTM"] = DataTableConverter.GetValue(added, DateTime.Now.ToString("CHG_DTTM"));
                        param["INSUSER"] = DataTableConverter.GetValue(added, "INSUSER");
                        //param["INSDTTM"] = DataTableConverter.GetValue(added, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        param["UPDUSER"] = DataTableConverter.GetValue(added, "UPDUSER");
                        //param["UPDDTTM"] = DataTableConverter.GetValue(added, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        param["SQLTYPE"] = "I";
                        inDataTable.Rows.Add(param);
                    }
                }

                foreach (object modified in dgKeyPart.GetModifiedItems())
                {
                    if (DataTableConverter.GetValue(modified, "CHK").Equals("True"))
                    {
                        DataRow param = inDataTable.NewRow();
                        param["INPUT_SEQNO"] = DataTableConverter.GetValue(modified, "INPUT_SEQNO");
                        param["LOTID"] = DataTableConverter.GetValue(modified, "LOTID");
                        param["CHG_TYPE"] = DataTableConverter.GetValue(modified, "CHG_TYPE");
                        param["NOTE"] = DataTableConverter.GetValue(modified, "NOTE");
                        param["PRE_VALUE"] = DataTableConverter.GetValue(modified, "PRE_VALUE");
                        param["AFTER_VALUE"] = DataTableConverter.GetValue(modified, "AFTER_VALUE");
                        //param["CHG_DTTM"] = DataTableConverter.GetValue(modified, );
                        //param["INSUSER"] = DataTableConverter.GetValue(modified, "INSUSER");
                        //param["INSDTTM"] = DataTableConverter.GetValue(modified, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        param["UPDUSER"] = DataTableConverter.GetValue(modified, "UPDUSER");
                        //param["UPDDTTM"] = DataTableConverter.GetValue(modified, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
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
                Util.gridClear(dgKeyPart);

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
        private bool Validation()
        {
            foreach (object added in dgKeyPart.GetAddedItems())
            {
                if (DataTableConverter.GetValue(added, "CHK").Equals("True"))
                {
                    //gridDistinctCheck();
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "CHG_TYPE"))))
                    {
                        Util.MessageValidation("SFU1642"); //선택된 유형코드가 없습니다
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "NOTE"))))
                    {
                        Util.MessageValidation("SFU5140"); // 사유코드 정보가 없습니다.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PRE_VALUE"))))
                    {
                        Util.MessageValidation("SFU1564"); // 버전 항목 값이 없습니다.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "AFTER_VALUE"))))
                    {
                        Util.MessageValidation("SFU1564"); // 버전 항목 값이 없습니다.
                        return false;
                    }
                }
            }

            foreach (object added in dgKeyPart.GetModifiedItems())
            {
                if (DataTableConverter.GetValue(added, "CHK").Equals("True"))
                {
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "CHG_TYPE"))))
                    {
                        Util.MessageValidation("SFU1642"); //선택된 유형코드가 없습니다
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "NOTE"))))
                    {
                        Util.MessageValidation("SFU5140"); // 사유코드 정보가 없습니다.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PRE_VALUE"))))
                    {
                        Util.MessageValidation("SFU1564"); // 버전 항목 값이 없습니다.
                        return false;
                    }
                    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "AFTER_VALUE"))))
                    {
                        Util.MessageValidation("SFU1564"); // 버전 항목 값이 없습니다.
                        return false;
                    }
                }
            }
            return true;
        }

        //private bool gridDistinctCheck()
        //{
        //    try
        //    {
        //        DataRowView rowview = null;

        //        if (dgKeyPart.GetRowCount() == 0)
        //        {
        //            return true;
        //        }

        //        foreach (C1.WPF.DataGrid.DataGridRow row in dgKeyPart.Rows)
        //        {

        //            if (row.DataItem != null)
        //            {
        //                rowview = row.DataItem as DataRowView;
                     
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        private void btnExcelLoad_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                DataTable dtDataGrid = new DataTable();
                dtDataGrid.TableName = "dtDataTable";

                dtDataGrid.Columns.Add("CHK", typeof(string));
                dtDataGrid.Columns.Add("LOTID", typeof(string));
                dtDataGrid.Columns.Add("CHG_TYPE", typeof(string));
                dtDataGrid.Columns.Add("NOTE", typeof(string));
                dtDataGrid.Columns.Add("PRE_VALUE", typeof(string));
                dtDataGrid.Columns.Add("AFTER_VALUE", typeof(string));


                CHGExcelImportEditor frm = new CHGExcelImportEditor(dtDataGrid);
                DataTable dtChild = new DataTable();

                frm.FrameOperation = this.FrameOperation;
                //frm.FormClosed -= frm.FormClosed;

                frm.FormClosed += delegate ()
                {
                    if (frm.DialogResult.Equals(MessageBoxResult.OK))
                    {
                        dgKeyPart.ItemsSource = null;
                        CellGridAdd(frm.dtIfMethod);

                        getKeypart(true, txtLot.Text);
                    }
                };
                

                frm.ShowModal();
                frm.CenterOnScreen();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private DataRow CellGridAdd(DataTable dt)
        {
            try
            {
                if (TotalRow == 0)
                {
                    Util.GridSetData(dgKeyPart, dt, FrameOperation);
                    TotalRow = dt.Rows.Count;
                    //++TotalRow;
                    return null;
                }
                DataRow dr = dt.NewRow();
                Util.DataGridRowAdd(dgKeyPart, 1);

                DataTableConverter.SetValue(dgKeyPart.Rows[TotalRow].DataItem, "CHK", dt.Rows[0]["CHK"].ToString());
                DataTableConverter.SetValue(dgKeyPart.Rows[TotalRow].DataItem, "LOTID", dt.Rows[0]["LOTID"].ToString());
                DataTableConverter.SetValue(dgKeyPart.Rows[TotalRow].DataItem, "CHG_TYPE", dt.Rows[0]["CHG_TYPE"].ToString());
                DataTableConverter.SetValue(dgKeyPart.Rows[TotalRow].DataItem, "NOTE", dt.Rows[0]["NOTE"].ToString());
                DataTableConverter.SetValue(dgKeyPart.Rows[TotalRow].DataItem, "PRE_VALUE", dt.Rows[0]["PRE_VALUE"].ToString());
                DataTableConverter.SetValue(dgKeyPart.Rows[TotalRow].DataItem, "AFTER_VALUE", dt.Rows[0]["AFTER_VALUE"].ToString());


                ++TotalRow;
                return dr;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        //이전값 사용안함
        //public void setInspectionBefore()
        //{
        //    try
        //    {
        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "INDATA";
        //        RQSTDT.Columns.Add("LOTID", typeof(string));
        //        RQSTDT.Columns.Add("CHG_TYPE", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LOTID"] = txtLot.Text;
        //        dr["CHG_TYPE"] = dt1;
        //        RQSTDT.Rows.Add(dr);

        //        DataTable dtReturn = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_AFTER_SHIP_BY_MAX", "INDATA", "OUTDATA", RQSTDT);

        //        int iClctIndex = dgKeyPart.Columns["CHG_TYPE"].Index;

        //        if (dtReturn.Rows.Count > 0)
        //        {

        //            bool bSetChk = false;

        //            for (int i = 0; i < dtReturn.Rows.Count; i++)
        //            {
        //                int iRow = Util.gridFindDataRow(ref dgKeyPart, "CHG_TYPE", Util.NVC(dtReturn.Rows[i]["CHG_TYPE"]), false);
                        
                        
        //                if (iRow > -1)
        //                {
        //                    C1.WPF.DataGrid.DataGridCell cell = dgKeyPart.GetCell(iRow, dgKeyPart.Columns["AFTER_VALUE"].Index);

        //                    DataTableConverter.SetValue(dgKeyPart.Rows[iRow].DataItem, "AFTER_VALUE", Util.NVC(dtReturn.Rows[i]["CLCFVAL01"]));
        //                    double p1 = 0;
        //                    bool canConvertLCL = double.TryParse(Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[iRow].DataItem, "AFTER_VALUE")), out p1);
        //                    bool canConvertVal = double.TryParse(Util.NVC(dtReturn.Rows[i]["CLCFVAL01"]), out p1);
        //                    dt2 =cell.Value.ToString();
        //                    //DataTableConverter.SetValue(dgKeyPart.Rows[iRow].DataItem, "PRE_VALUE", dt2);

        //                    if (canConvertLCL == canConvertVal)
        //                    {
                                
        //                            DataTableConverter.SetValue(dgKeyPart.Rows[iRow].DataItem, "PRE_VALUE", dt2);

        //                            if (cell.Presenter != null)
        //                            {
        //                                if (cell.Column.Name.Equals("AFTER_VALUE"))
        //                                {
        //                                    cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
        //                                    cell.Presenter.FontWeight = FontWeights.Bold;
        //                                }
        //                                else
        //                                {
        //                                    cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
        //                                    cell.Presenter.FontWeight = FontWeights.Normal;
        //                                }
        //                            }
        //                    }
        //                    //Util.NVC(dtReturn.Rows[i]["PRE_VALUE"]),cell.Text);
        //                    bSetChk = true;
        //                }
        //            }
        //        }
        //        dgKeyPart.CanUserSort = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}


        private void dgKeyPart_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                DataTable dt = DataTableConverter.Convert(dataGrid.ItemsSource);

                
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "CHG_TYPE")
                    {
                        
                        if (e.Cell.Value == null || string.IsNullOrWhiteSpace(e.Cell.Value.ToString())) return;

                        int j = dt.Select("CHG_TYPE = '" + e.Cell.Value.ToString() + " '" + "and CHK = true").Count();
                        if(j > 1)
                        {
                            Util.MessageValidation("MMD0067");
                            DataRowView drv = dgKeyPart.SelectedItem as DataRowView;
                            if (drv != null && (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached))
                            {
                                if (dgKeyPart.SelectedIndex > -1)
                                {
                                    dgKeyPart.EndNewRow(true);
                                    dgKeyPart.RemoveRow(dgKeyPart.SelectedIndex);
                                }
                            }
                            return;

                        }
                            for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (i == e.Cell.Row.Index) break;
                            
                            if (e.Cell.Value.ToString().Equals((dt.Rows[i]).ItemArray[0].ToString()))
                            {
                                DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "PRE_VALUE", dt.Rows[i]["AFTER_VALUE"].ToString());
                            }
                        }
                        //DataTable dtDistinct_dr = DataTableConverter.Convert(dataGrid.ItemsSource);

                        //dtDistinct_dr = 
                        ////dv.ToTable(true, new string[] { "CHG_TYPE" });

                        //if (dtDistinct_dr.Rows.Count > 2)
                        //{
                        //    return;
                        //}
                       

                        dataGrid.Refresh();
                    }
                }));
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
    }
}
