/*************************************************************************************
 Created Date : 2017.01.19
      Creator : 
   Decription : 원자재 투입이력 조회 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.19  : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ELEC001
{
    /// <summary>
    /// ELEC001_003_INPUT_HIST
    /// </summary>
    public partial class ELEC001_003_INPUT_HIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _WORKORDER = string.Empty;
        private string _PROCID = string.Empty;
        private string _PROCNAME = string.Empty;
        private string _EQSGID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _EQPTNAME = string.Empty;
        private string _LOTID = string.Empty;
        private string _MTRLID = string.Empty;
        decimal _CurrQty = 0;

        Util _Util = new Util();

        public string _ReturnLotID //사용안함
        {
            get { return _LOTID; }
        }
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ELEC001_003_INPUT_HIST()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                return;
            }

            _EQSGID = Util.NVC(tmps[0]);
            _EQPTID = Util.NVC(tmps[1]);
            
            InitControls();
            //SetCboUseFlag(cboCnfmFlag);
            InitCombo();
        }
        private void InitControls()
        {
            ldpDateFrom.Text = DateTime.Now.ToString("yyyy-MM-dd");
            ldpDateTo.Text = DateTime.Now.ToString("yyyy-MM-dd");
        }
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            if (LoginInfo.CFG_PROC_ID.Equals(Process.MIXING) || LoginInfo.CFG_PROC_ID.Equals(Process.SRS_MIXING) || LoginInfo.CFG_PROC_ID.Equals(Process.PRE_MIXING))
            {
                String[] sFilter = { _EQSGID, LoginInfo.CFG_PROC_ID, null };
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
                cboEquipment.SelectedValue = _EQPTID;
            }
            else
            {
                Util.MessageValidation("SFU2841");  //Mixing공정에서 사용가능합니다.
                return;
            }
        }
        private void SetCboHopper(C1ComboBox cbo)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = cboEquipment.SelectedValue.ToString().Trim();
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_HOPPER_CBO", "INDATA", "RSLTDT", IndataTable);
                if (dtMain.Rows.Count == 0) { return; }

                DataRow dRow = dtMain.NewRow();
                dRow["NAME"] = "ALL";
                dRow["CODE"] = "ALL";
                dtMain.Rows.InsertAt(dRow, 0);

                cbo.ItemsSource = DataTableConverter.Convert(dtMain);
                cboHopper.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        private void SetCboUseFlag(C1ComboBox cbo)
        {
            DataSet dsData = new DataSet();
            DataTable dtData = dsData.Tables.Add("ALL");
            dtData.Columns.Add("CBO_NAME", typeof(string));
            dtData.Columns.Add("CBO_CODE", typeof(string));

            DataRow drnewrow = dtData.NewRow();

            drnewrow = dtData.NewRow();
            drnewrow["CBO_NAME"] = "ALL";
            drnewrow["CBO_CODE"] = "ALL";
            dtData.Rows.Add(drnewrow);

            drnewrow = dtData.NewRow();
            drnewrow["CBO_NAME"] = "Y";
            drnewrow["CBO_CODE"] = "Y";
            dtData.Rows.Add(drnewrow);

            drnewrow = dtData.NewRow();
            drnewrow["CBO_NAME"] = "N";
            drnewrow["CBO_CODE"] = "N";
            dtData.Rows.Add(drnewrow);
            
            cbo.ItemsSource = DataTableConverter.Convert(dtData);

            cbo.SelectedIndex = 0;
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(dgMtrlInfo.GetRowCount() <= 0)
                {
                    Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgMtrlInfo, "CHK");
                if (idx < 0)
                {
                    Util.MessageValidation("SFU1828");  //자재를 선택하세요.
                    return;
                }
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("HOPPER_ID", typeof(string));
                dtRqst.Columns.Add("RMTRL_LABEL_ID", typeof(string));
                dtRqst.Columns.Add("CURR_QTY", typeof(decimal));
                dtRqst.Columns.Add("USERID", typeof(string));
                DataTable dt = ((DataView)dgMtrlInfo.ItemsSource).Table;

                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToBoolean(row["CHK"]))
                    {
                        DataRow dr = dtRqst.NewRow();
                        dr["HOPPER_ID"] = Util.NVC(row["HOPPER_ID"]);
                        dr["RMTRL_LABEL_ID"] = Util.NVC(row["RMTRL_LABEL_ID"]);
                        dr["CURR_QTY"] = Util.NVC_Decimal(row["CURR_QTY"]);
                        dr["USERID"] = LoginInfo.USERID;
                        //dtRqst.ImportRow(dr);
                        dtRqst.Rows.Add(dr);
                    }
                }
                    

                DataTable dtRslt = new DataTable();
                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_TB_SFC_MIXER_INPUT_RMTRL_CURR_QTY", "RQSTDT", null, dtRqst);

                GetInputList();

                Util.AlertInfo("SFU1265");  //수정되었습니다.
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
                if (Convert.ToDecimal(Convert.ToDateTime(ldpDateFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(ldpDateTo.SelectedDateTime).ToString("yyyyMMdd")))
                {
                    Util.MessageValidation("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
                    return;
                }
                GetInputList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void dgMtrlInfo_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e) //사용안함
        {
            try
            {
                DataRowView drv = e.Row.DataItem as DataRowView;
                bool bchk = Convert.ToBoolean(DataTableConverter.GetValue(e.Row.DataItem, "CHK"));
                string _UseFlag = Convert.ToString(DataTableConverter.GetValue(e.Row.DataItem, "USE_CMPL_FLAG"));
                if (_UseFlag == "Y")
                {
                    e.Cancel = true;    // Editing 불가능
                    //현재 사용중인 자재수량은 수정할수 없습니다.
                    Util.MessageInfo("SFU2992", (result) =>
                    {
                        DataTableConverter.SetValue(dgMtrlInfo.Rows[e.Row.Index].DataItem, "CHK", false);
                    });
                    return;
                }
                if (bchk == true)
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridNumericColumn)
                    {
                        if (Convert.ToString(e.Column.Name) == "CURR_QTY")
                        {
                            e.Cancel = false;  // Editing 가능
                        }
                    }
                }
                else
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridNumericColumn)
                    {
                        if (Convert.ToString(e.Column.Name) == "CURR_QTY")
                        {
                            e.Cancel = true;    // Editing 불가능
                        }
                    }
                    Util.MessageValidation("SFU2908");    //선택후 입력 가능합니다.
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void ldpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ldpDateFrom.Text == string.Empty || ldpDateTo.Text == string.Empty || ldpDateFrom.Text == null || ldpDateTo.Text == null)
            {
                return;
            }

            if (Convert.ToDecimal(Convert.ToDateTime(ldpDateFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(ldpDateTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.MessageValidation("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
                return;
            }
            this.btnSearch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnSearch));
        }

        private void ldpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ldpDateFrom.Text == string.Empty || ldpDateTo.Text == string.Empty || ldpDateFrom.Text == null || ldpDateTo.Text == null)
            {
                return;
            }
            if (Convert.ToDecimal(Convert.ToDateTime(ldpDateFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(ldpDateTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.MessageValidation("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
                return;
            }
            this.btnSearch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnSearch));
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipment.Text == null || cboEquipment.Text == "") return;
            
            SetCboHopper(cboHopper);
            cboHopper.Focus();
            this.btnSearch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnSearch));
        }
        private void cboHopper_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboHopper.Text == null || cboHopper.Text == "") return;
                this.btnSearch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnSearch));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void txtMtrlID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.btnSearch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnSearch));
            }
        }
        private void cboUseFlag_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboHopper.Text == null || cboHopper.Text == "") return;
                this.btnSearch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnSearch));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.DialogResult = MessageBoxResult.OK;
        }
        private void dgMtrlInfoChoice_Checked(object sender, RoutedEventArgs e) //사용안함
        {
            CheckBox cb = sender as CheckBox;

            if (cb.DataContext == null)
                return;
           
            if ((bool)cb.IsChecked && (cb.DataContext as DataRowView).Row["CHK"].ToString().Equals("1"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
                DataRow dtRow = (cb.DataContext as DataRowView).Row;
                
                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).DataGrid;
                string _UseFlag = DataTableConverter.GetValue(dgMtrlInfo.Rows[idx].DataItem, "USE_CMPL_FLAG").ToString();
                
                if (_UseFlag == "Y")
                {
                    DataTableConverter.SetValue(dgMtrlInfo.Rows[idx].DataItem, "CHK", false);
                    DataTableConverter.SetValue(dg.Rows[idx].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Column.Name, false);
                    return;
                }
                dgMtrlInfo.SelectedIndex = idx;
            }
        }
        private void dgMtrlInfo_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                string _UseFlag = "";
                string _chk = "";
                if (e.Cell.Column is C1.WPF.DataGrid.DataGridNumericColumn)
                {
                    _UseFlag = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "USE_CMPL_FLAG"));
                    _chk = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK"));
                    //row 색 바꾸기
                    dgMtrlInfo.SelectedIndex = e.Cell.Row.Index;
                    if (_UseFlag == "Y")
                    {
                        Util.MessageValidation("SFU2992");  //사용완료된자재수량은수정할수없습니다.
                        DataTableConverter.SetValue(dgMtrlInfo.Rows[e.Cell.Row.Index].DataItem, "CURR_QTY", _CurrQty);
                        return;
                    }
                    if (_chk.Equals("0"))
                    {
                        Util.MessageValidation("SFU2908");
                        DataTableConverter.SetValue(dgMtrlInfo.Rows[e.Cell.Row.Index].DataItem, "CURR_QTY", _CurrQty);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void dgMtrlInfo_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            dgMtrlInfo.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                    {
                        C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                        CheckBox cb = cell.Presenter.Content as CheckBox;
                        int index = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
                        //사용여부가 Y이면 선택 불가능 하도록...
                        string _UseFlag = Util.NVC(DataTableConverter.GetValue(dgMtrlInfo.Rows[e.Row.Index].DataItem, "USE_CMPL_FLAG"));
                        if (_UseFlag == "Y")
                        {
                            cb.IsChecked = false;
                            Util.MessageValidation("SFU2992");  //사용완료된자재수량은수정할수없습니다.
                            return;
                        }
                    }
                    if (e.Column is C1.WPF.DataGrid.DataGridNumericColumn)
                    {
                        _CurrQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgMtrlInfo.Rows[e.Row.Index].DataItem, "CURR_QTY"));
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }
        #endregion

        #region Mehod
        private void GetInputList()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("HOPPER_ID", typeof(string));
                IndataTable.Columns.Add("MTRLID", typeof(string));
                IndataTable.Columns.Add("STDT", typeof(string));
                IndataTable.Columns.Add("EDDT", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = cboEquipment.SelectedValue.ToString();
                Indata["HOPPER_ID"] = cboHopper.SelectedValue.ToString() == "ALL" ? null : cboHopper.SelectedValue.ToString(); //cboHopper.SelectedValue.ToString();
                Indata["MTRLID"] = txtMtrlID.Text.ToString().Trim() == "" ? null : txtMtrlID.Text.ToString().Trim();
                Indata["STDT"] = Convert.ToDateTime(ldpDateFrom.SelectedDateTime).ToString("yyyyMMdd"); 
                Indata["EDDT"] = Convert.ToDateTime(ldpDateTo.SelectedDateTime).ToString("yyyyMMdd");   
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RMTRL_INPUT_HIST", "INDATA", "RSLTDT", IndataTable);
                Util.GridSetData(dgMtrlInfo, dtMain, FrameOperation, true);
                //dgMtrlInfo.ItemsSource = DataTableConverter.Convert(dtMain);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
