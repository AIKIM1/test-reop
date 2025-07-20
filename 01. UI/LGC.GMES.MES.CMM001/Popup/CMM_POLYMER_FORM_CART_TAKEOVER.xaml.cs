/*************************************************************************************
 Created Date : 2018.03.01
      Creator : 정문교
   Decription : 대차 재공 인계/인수
--------------------------------------------------------------------------------------
 [Change History]
    
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
using System.Windows.Media;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_POLYMER_FORM_CART_TAKEOVER.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_CART_TAKEOVER : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _procID = string.Empty;        // 공정코드
        private string _procName = string.Empty;      // 공정명
        private string _eqptID = string.Empty;        // 설비코드
        private string _eqptName = string.Empty;      // 설비명
        private string _lineID = string.Empty;        // 라인코드
        private string _cartID = string.Empty;        // 대차ID

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        private bool _load = true;

        private CheckBoxHeaderType _inBoxHeaderType;

        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_POLYMER_FORM_CART_TAKEOVER()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetControl();
                SetCombo();
                _load = false;
            }

        }

        private void InitializeUserControls()
        {
            _cartID = string.Empty;

            dgListOut.AlternatingRowBackground = null;
            dgListIn.AlternatingRowBackground = null;
        }
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _procName = tmps[1] as string;
            _eqptID = tmps[2] as string;
            _eqptName = tmps[3] as string;
            _lineID = tmps[4] as string;

            txtProcessOut.Text = tmps[1] as string;
            txtProcessIn.Text = tmps[1] as string;
        }
        private void SetCombo()
        {
            CommonCombo _combo = new CommonCombo();

            // 재공품질유형(양불구분)
            string[] sFilter1 = { "WIP_QLTY_TYPE_CODE" };
            _combo.SetCombo(cboQltyTypeOut, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);
            _combo.SetCombo(cboQltyTypeIn, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

            // 대차상태 (대기, 보관, 작업중, 이동중만)
            string[] sFilter2 = { "CTNR_STAT_CODE" };
            _combo.SetCombo(cboCtnrStatOut, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);
            DataTable dt = DataTableConverter.Convert(cboCtnrStatOut.ItemsSource);
            dt.Select("CBO_CODE = '" + "DELETED" + "'").ToList<DataRow>().ForEach(row => row.Delete());
            cboCtnrStatOut.ItemsSource = dt.Copy().AsDataView();

            // 재공처리유형
            string[] sFilter3 = { "WIP_PRCS_TYPE_CODE" };
            _combo.SetCombo(cboWipPrcsTypeOut, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter3);

            // Inbox상태
            _combo.SetCombo(cboInboxStatOut, CommonCombo.ComboStatus.ALL, sCase: "cboWipStat");
            // 라인
            cboEquipmentSegmentOut.ApplyTemplate();
            SetEquipmentSegmentCombo(cboEquipmentSegmentOut);
            cboEquipmentSegmentIn.ApplyTemplate();
            SetEquipmentSegmentCombo(cboEquipmentSegmentIn);
            // 인계동
            SetTakeOverAreaCombo(cboTakeOverAreaIn);
            // 인계공정
            SetTakeOverProcessCombo(cboTakeOverProcessIn);
            // 인계라인
            SetTakeOverEquipmentSegmentCombo(cboTakeOverLineIn);

            cboTakeOverAreaIn.SelectedValueChanged += cboTakeOverAreaIn_SelectedValueChanged;
            cboTakeOverProcessIn.SelectedValueChanged += cboTakeOverProcessIn_SelectedValueChanged;
        }

        #endregion

        private void dgCartChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }

                        //row 색 바꾸기
                        if (((System.Windows.FrameworkElement)ctbTakeOver.SelectedItem).Name.Equals("TakeOverOut"))
                            dgListOut.SelectedIndex = idx;
                        else
                            dgListIn.SelectedIndex = idx;

                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [인계]

        private void cboTakeOverAreaIn_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboTakeOverProcessIn.SelectedValueChanged -= cboTakeOverProcessIn_SelectedValueChanged;

            SetTakeOverProcessCombo(cboTakeOverProcessIn);
            SetTakeOverEquipmentSegmentCombo(cboTakeOverLineIn);

            cboTakeOverProcessIn.SelectedValueChanged += cboTakeOverProcessIn_SelectedValueChanged;
        }

        private void cboTakeOverProcessIn_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetTakeOverEquipmentSegmentCombo(cboTakeOverLineIn);
        }

        /// <summary>
        /// 조회 조건에 Text 입력 후 엔터 시 조회
        /// </summary>
        private void txtPjtNameOut_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationSearch())
                    return;

                OutSearch();
            }
        }

        private void txtProdIDOut_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationSearch())
                    return;

                OutSearch();
            }
        }

        private void txtCtnrIDOut_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationSearch())
                    return;

                OutSearch();
            }
        }

        private void txtAssyLotIDOut_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationSearch())
                    return;

                OutSearch();
            }
        }

        private void txtInboxIDOut_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationSearch())
                    return;

                OutSearch();
            }
        }

        private void dgListOut_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            if (e.Column == null)
                return;

            if (!dgListOut.Columns["CTNR_ID2"].Width.Value.Equals(0))
            {
                dgListOut.Columns["CTNR_ID2"].Width = new C1.WPF.DataGrid.DataGridLength(0);
            }
        }

        /// <summary>
        /// 조회
        /// </summary>
        private void btnSearchOut_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            OutSearch();
        }

        /// <summary>
        /// 재공처리유형변경
        /// </summary>
        private void btnWipPrcsTypeChange_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationWipPrcsTypeChange(dgListOut))
                return;

            WipPrcsTypeChange();
        }

        /// <summary>
        /// Cell 등록
        /// </summary>
        private void btnCellInsert_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCellInsert(dgListOut))
                return;

            CellInsert();
        }

        /// <summary>
        /// 상세조회
        /// </summary>
        private void btnDetailOut_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDetail(dgListOut))
                return;

            CartDetail();
        }

        /// <summary>
        /// 활성화이동
        /// </summary>
        private void btnFCSMove_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationFCSMove())
                return;

            CartFCSMove();
        }

        /// <summary>
        /// 보관취소
        /// </summary>
        private void btnStoredCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationStoreCancel())
                return;

            // 보관취소 하시겠습니까?
            Util.MessageConfirm("SFU4459", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    StoredCancel();
                }
            });
        }

        /// <summary>
        /// 인계취소
        /// </summary>
        private void btnTakeOverCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTakeOverCancel())
                return;

            CartTakeOverCancel();
        }

        /// <summary>
        /// 대차인계
        /// </summary>
        private void btnTakeOverOut_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTakeOverOut())
                return;

            CartTakeOverOut();
        }

        private void btnCloseOut_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region [인수]
        /// <summary>
        /// 조회 조건에 Text 입력 후 엔터 시 조회
        /// </summary>
        private void txtPjtNameIn_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationSearch())
                    return;

                InSearch();
            }
        }

        private void txtProdIDIn_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationSearch())
                    return;

                InSearch();
            }
        }

        private void txtCtnrIDIn_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationSearch())
                    return;

                InSearch();
            }
        }

        private void txtAssyLotIDIn_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationSearch())
                    return;

                InSearch();
            }
        }

        private void dgListIn_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            if (e.Column == null)
                return;

            if (!dgListIn.Columns["MOVE_ORD_ID"].Width.Value.Equals(0))
            {
                dgListIn.Columns["MOVE_ORD_ID"].Width = new C1.WPF.DataGrid.DataGridLength(0);
            }
        }

        private void tbCheckHeaderAll_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgListIn;
            if (dg?.ItemsSource == null) return;

            foreach (DataGridRow row in dg.Rows)
            {
                switch (_inBoxHeaderType)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_inBoxHeaderType)
            {
                case CheckBoxHeaderType.Zero:
                    _inBoxHeaderType = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _inBoxHeaderType = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        /// <summary>
        /// 조회
        /// </summary>
        private void btnSearchIn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            InSearch();
        }

        /// <summary>
        /// 상세조회
        /// </summary>
        private void btnDetailIn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDetail(dgListIn))
                return;

            CartDetail();
        }

        /// <summary>
        /// 대차인수
        /// </summary>
        private void btnTakeOverIn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTakeOverIn())
                return;

            // 대차 인수
            CartTakeOverIn();
        }

        private void btnCloseIn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion


        #endregion

        #region Mehod

        #region [인계]

        private bool ChkProcess(string cmcdtype)
        {
            bool IsProc = false;

            try
            {
                // FORM_INSP_PROCID(최종외관), FORM_REWORK_PROCID((양품화)

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["CMCDTYPE"] = cmcdtype;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_PC", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    DataRow[] dr = dtResult.Select("CBO_CODE = '" + _procID + "'");

                    if (dr.Length > 0)
                    {
                        IsProc = true;
                    }
                }

                return IsProc;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return IsProc;
            }
        }

        /// <summary>
        /// 인계조회
        /// </summary>
        private void OutSearch()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
                inTable.Columns.Add("CTNR_STAT_CODE", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));
                inTable.Columns.Add("PJT_NAME", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("ASSY_LOTID", typeof(string));
                inTable.Columns.Add("INBOX_ID", typeof(string));
                inTable.Columns.Add("WIP_PRCS_TYPE_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _procID;
                newRow["EQSGID"] = Util.NVC(cboEquipmentSegmentOut.SelectedItemsToString);
                newRow["WIP_QLTY_TYPE_CODE"] = Util.NVC(cboQltyTypeOut.SelectedValue);
                newRow["CTNR_STAT_CODE"] = Util.NVC(cboCtnrStatOut.SelectedValue);
                newRow["WIPSTAT"] = Util.NVC(cboInboxStatOut.SelectedValue);
                newRow["PJT_NAME"] = txtPjtNameOut.Text;
                newRow["PRODID"] = txtProdIDOut.Text;
                newRow["CTNR_ID"] = txtCtnrIDOut.Text;
                newRow["ASSY_LOTID"] = txtAssyLotIDOut.Text;
                newRow["INBOX_ID"] = txtInboxIDOut.Text;
                newRow["WIP_PRCS_TYPE_CODE"] = Util.NVC(cboWipPrcsTypeOut.SelectedValue);
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_POLYMER_CART_TAKEOVER_OUT_LIST", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 대차의 Inbox수 산출
                        var summarydata = from row in bizResult.AsEnumerable()
                                          group row by new
                                          {
                                              CartID = row.Field<string>("CTNR_ID"),
                                          } into grp
                                          select new
                                          {
                                              CartID = grp.Key.CartID,
                                              CellSum = grp.Sum(r => r.Field<decimal>("CELL_QTY"))

                                          };

                        foreach (var row in summarydata)
                        {
                            bizResult.Select("CTNR_ID = '" + row.CartID + "'").ToList<DataRow>().ForEach(r => r["CART_CELL_QTY"] = row.CellSum);
                        }
                        bizResult.AcceptChanges();

                        Util.GridSetData(dgListOut, bizResult, FrameOperation, true);

                        // 대차 개수
                        int CtnrCount = bizResult.DefaultView.ToTable(true, "CTNR_ID").Rows.Count;
                        DataGridAggregate.SetAggregateFunctions(dgListOut.Columns["CTNR_ID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("대차") + ": " + CtnrCount.ToString("###,###") } });
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private int GetProductCartCount()
        {
            try
            {
                int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgListOut, "CHK");

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("CTNR_STAT_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgListOut.Rows[rowIndex].DataItem, "CURR_EQPTID"));
                newRow["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgListOut.Rows[rowIndex].DataItem, "PROCID"));
                newRow["CTNR_STAT_CODE"] = "WORKING";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_CTNR_PC", "INDATA", "OUTDATA", inTable);

                if (dtResult == null)
                {
                    return 0;
                }
                else
                {
                    return dtResult.Rows.Count;
                }
            }
            catch (Exception ex)
            {
                return 0;
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Cell 등록 권한
        /// </summary>
        private bool ChkCellAuthority()
        {
            bool IsProc = false;

            try
            {
                // FORM_INSP_PROCID(최종외관), FORM_REWORK_PROCID((양품화)

                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AUTHID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["AUTHID"] = "ASSYMF_CELLID_REG";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_USER_AUTH_PC", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    IsProc = true;
                }

                return IsProc;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return IsProc;
            }
        }

        /// <summary>
        /// 보관 취소
        /// </summary>
        private void StoredCancel()
        {
            try
            {
                ShowLoadingIndicator();

                int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgListOut, "CHK");

                // DATA SET
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));

                DataTable inCNTR = inDataSet.Tables.Add("INCTNR");
                inCNTR.Columns.Add("CTNR_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["PROCID"] = _procID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(newRow);

                newRow = inCNTR.NewRow();
                newRow["CTNR_ID"] = _cartID;
                inCNTR.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_STORE_CTNR", "INDATA,INCTNR", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        OutSearch();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [인수]

        /// <summary>
        /// 인수조회
        /// </summary>
        private void InSearch()
        {
            try
            {
                _inBoxHeaderType = CheckBoxHeaderType.Zero;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("TO_PROCID", typeof(string));
                inTable.Columns.Add("TO_EQSGID", typeof(string));
                inTable.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
                inTable.Columns.Add("PJT_NAME", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("FROM_AREAID", typeof(string));
                inTable.Columns.Add("FROM_PROCID", typeof(string));
                inTable.Columns.Add("FROM_EQSGID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("ASSY_LOTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["TO_PROCID"] = _procID;
                newRow["TO_EQSGID"] = Util.NVC(cboEquipmentSegmentIn.SelectedItemsToString);
                newRow["WIP_QLTY_TYPE_CODE"] = string.IsNullOrWhiteSpace(Util.NVC(cboQltyTypeIn.SelectedValue)) ? null : Util.NVC(cboQltyTypeIn.SelectedValue);
                newRow["PJT_NAME"] = txtPjtNameIn.Text;
                newRow["PRODID"] = txtProdIDIn.Text;
                newRow["FROM_AREAID"] = string.IsNullOrWhiteSpace(Util.NVC(cboTakeOverAreaIn.SelectedValue)) ? null : Util.NVC(cboTakeOverAreaIn.SelectedValue);
                newRow["FROM_PROCID"] = string.IsNullOrWhiteSpace(Util.NVC(cboTakeOverProcessIn.SelectedValue)) ? null : Util.NVC(cboTakeOverProcessIn.SelectedValue);
                newRow["FROM_EQSGID"] = string.IsNullOrWhiteSpace(Util.NVC(cboTakeOverLineIn.SelectedValue)) ? null : Util.NVC(cboTakeOverLineIn.SelectedValue);
                newRow["CTNR_ID"] = txtCtnrIDIn.Text;
                newRow["ASSY_LOTID"] = txtAssyLotIDIn.Text;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_POLYMER_CART_TAKEOVER_IN_LIST", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgListIn, bizResult, FrameOperation, true);

                        // 대차 개수
                        int CtnrCount = bizResult.DefaultView.ToTable(true, "CTNR_ID").Rows.Count;
                        DataGridAggregate.SetAggregateFunctions(dgListIn.Columns["CTNR_ID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("대차") + ": " + CtnrCount.ToString("###,###") } });

                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region [Func]

        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "COATER_EQPT_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, _lineID, _procID, null };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, _eqptID);

            //////////////////// 설비가 N건인 경우 Select 추가
            if (cbo.Items.Count > 1 || cbo.Items.Count == 0)
            {
                DataTable dtEqpt = DataTableConverter.Convert(cbo.ItemsSource);
                DataRow drEqpt = dtEqpt.NewRow();
                drEqpt[selectedValueText] = "SELECT";
                drEqpt[displayMemberText] = "- SELECT -";
                dtEqpt.Rows.InsertAt(drEqpt, 0);

                cbo.ItemsSource = null;
                cbo.ItemsSource = dtEqpt.Copy().AsDataView();

                int Index = 0;

                if (!string.IsNullOrEmpty(_eqptID))
                {
                    DataRow[] drIndex = dtEqpt.Select("CBO_CODE ='" + _eqptID + "'");

                    if (drIndex.Length > 0)
                    {
                        Index = dtEqpt.Rows.IndexOf(drIndex[0]);
                        cbo.SelectedValue = _eqptID;
                    }
                }

                cbo.SelectedIndex = Index;
            }

        }

        private void SetEquipmentSegmentCombo(MultiSelectionBox mcb)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = _procID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_PC", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    mcb.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.Check(-1);
                    }
                    else
                    {
                        mcb.isAllUsed = true;
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            mcb.Check(i);
                        }
                    }
                }
                else
                {
                    mcb.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetTakeOverAreaCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_AREA_CBO";
            string[] arrColumn = { "LANGID", "SHOPID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, null);
        }

        private void SetTakeOverProcessCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_PROCESS_AREA_CBO_PC";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, Util.NVC(cboTakeOverAreaIn.SelectedValue) };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText,null);
        }

        private void SetTakeOverEquipmentSegmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_PC";
            string[] arrColumn = { "LANGID", "AREAID", "PROCID" };
            string[] arrCondition = { LoginInfo.LANGID, Util.NVC(cboTakeOverAreaIn.SelectedValue), Util.NVC(cboTakeOverProcessIn.SelectedValue) };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;
            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, null);
        }

        private bool ValidationWipPrcsTypeChange(C1DataGrid dg)
        {
            if (dg.Rows.Count - dg.FrozenBottomRowsCount <= 0)
            {
                // 조회된 Data가 없습니다.
                Util.MessageValidation("SFU1905");
                return false;
            }

            int rowIndex = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");
            if (rowIndex < 0)
            {
                // "선택된 항목이 없습니다."
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIndex].DataItem, "WIP_QLTY_TYPE_CODE")).Equals("N"))
            {
                // %1 대차만 %2할 수 있습니다.
                object[] parameters = new object[2];
                parameters[0] = ObjectDic.Instance.GetObjectName("불량");
                parameters[1] = ObjectDic.Instance.GetObjectName("재공처리유형변경");

                Util.MessageValidation("SFU4926", parameters);
                return false;
            }

            _cartID = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIndex].DataItem, "CTNR_ID"));

            return true;
        }

        private bool ValidationCellInsert(C1DataGrid dg)
        {
            if (dg.Rows.Count - dg.FrozenBottomRowsCount <= 0)
            {
                // 조회된 Data가 없습니다.
                Util.MessageValidation("SFU1905");
                return false;
            }

            int rowIndex = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");
            if (rowIndex < 0)
            {
                // "선택된 항목이 없습니다."
                Util.MessageValidation("SFU1651");
                return false;
            }

            //if (Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIndex].DataItem, "CTNR_STAT_CODE")).Equals("CREATED") ||
            //    Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIndex].DataItem, "CTNR_STAT_CODE")).Equals("STORED"))
            //{
            //}
            //else
            //{
            //    // Cell등록은 대기, 보관중인 대차만 입력할 수 있습니다.
            //    Util.MessageValidation("SFU4924");
            //    return false;
            //}

            if (!ChkCellAuthority())
            {
                // USER[%1]는 권한[%2]을 가지고 있지 않습니다.
                Util.MessageValidation("SFU3520", LoginInfo.USERID, ObjectDic.Instance.GetObjectName("Cell 등록"));
                return false;
            }

            _cartID = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIndex].DataItem, "CTNR_ID"));

            return true;
        }

        private bool ValidationDetail(C1DataGrid dg)
        {
            if (dg.Rows.Count - dg.FrozenBottomRowsCount <= 0)
            {
                // 조회된 Data가 없습니다.
                Util.MessageValidation("SFU1905");
                return false;
            }

            DataRow[] dr = DataTableConverter.Convert(dg.ItemsSource).Select("CHK = 1");

            int rowIndex = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");

            if (dr.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (dr.Length > 1)
            {
                // %1(은)는 한건씩 처리하세요.
                Util.MessageValidation("SFU3719", ObjectDic.Instance.GetObjectName("대차상세"));
                return false;
            }

            //_cartID = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIndex].DataItem, "CTNR_ID"));
            _cartID = Util.NVC(dr[0]["CTNR_ID"]);

            return true;
        }

        private bool ValidationFCSMove()
        {
            if (dgListOut.Rows.Count - dgListOut.FrozenBottomRowsCount <= 0)
            {
                // 조회된 Data가 없습니다.
                Util.MessageValidation("SFU1905");
                return false;
            }

            int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgListOut, "CHK");
            if (rowIndex < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgListOut.Rows[rowIndex].DataItem, "CTNR_STAT_CODE")).Equals("CREATED") ||
                Util.NVC(DataTableConverter.GetValue(dgListOut.Rows[rowIndex].DataItem, "CTNR_STAT_CODE")).Equals("STORED"))
            {
            }
            else
            { 
                // 대차 이동은 대기,보관중인 대차만 이동할 수 있습니다.
                Util.MessageValidation("SFU4897");
                return false;
            }

            if (!ChkProcess("FCS_MOVE_FROM_PROCID"))
            {
                // 활성화 이동공정이 아닙니다.
                Util.MessageValidation("SFU4904");
                return false;
            }

            _cartID = Util.NVC(DataTableConverter.GetValue(dgListOut.Rows[rowIndex].DataItem, "CTNR_ID"));

            return true;
        }

        private bool ValidationStoreCancel()
        {
            if (dgListOut.Rows.Count - dgListOut.FrozenBottomRowsCount <= 0)
            {
                // 조회된 Data가 없습니다.
                Util.MessageValidation("SFU1905");
                return false;
            }

            int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgListOut, "CHK");
            if (rowIndex < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgListOut.Rows[rowIndex].DataItem, "WIP_QLTY_TYPE_CODE")).Equals("N"))
            {
                object[] parameters = new object[1];
                parameters[0] = ObjectDic.Instance.GetObjectName("보관취소");

                // 불량 대차는 [%1]할 수 없습니다.
                Util.MessageValidation("SFU4899", parameters);
                return false;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgListOut.Rows[rowIndex].DataItem, "CTNR_STAT_CODE")).Equals("STORED"))
            {
                object[] parameters = new object[1];
                parameters[0] = ObjectDic.Instance.GetObjectName("보관취소");

                // 보관중인 대차만 [%1]할 수 있습니다.
                Util.MessageValidation("SFU4898", parameters);
                return false;
            }

            if (GetProductCartCount() > 4)
            {
                // 현재 작업중인 대차가 5개 있습니다. \r\n대차 작업 완료후 보관취소가 가능 합니다.
                Util.MessageValidation("SFU4900");
                return false;
            }

            _cartID = Util.NVC(DataTableConverter.GetValue(dgListOut.Rows[rowIndex].DataItem, "CTNR_ID"));

            return true;
        }

        private bool ValidationTakeOverCancel()
        {
            if (dgListOut.Rows.Count - dgListOut.FrozenBottomRowsCount <= 0)
            {
                // 조회된 Data가 없습니다.
                Util.MessageValidation("SFU1905");
                return false;
            }

            int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgListOut, "CHK");
            if (rowIndex < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgListOut.Rows[rowIndex].DataItem, "CTNR_STAT_CODE")).Equals("MOVING"))
            {
                object[] parameters = new object[1];
                parameters[0] = ObjectDic.Instance.GetObjectName("인계취소");

                // 이동중인 대차만 [%1]할 수 있습니다.
                Util.MessageValidation("SFU4586", parameters);
                return false;
            }

            _cartID = Util.NVC(DataTableConverter.GetValue(dgListOut.Rows[rowIndex].DataItem, "CTNR_ID"));

            return true;
        }

        private bool ValidationTakeOverOut()
        {
            if (dgListOut.Rows.Count - dgListOut.FrozenBottomRowsCount <= 0)
            {
                // 조회된 Data가 없습니다.
                Util.MessageValidation("SFU1905");
                return false;
            }

            int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgListOut, "CHK");
            if (rowIndex < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgListOut.Rows[rowIndex].DataItem, "CTNR_STAT_CODE")).Equals("CREATED") ||
                Util.NVC(DataTableConverter.GetValue(dgListOut.Rows[rowIndex].DataItem, "CTNR_STAT_CODE")).Equals("STORED"))
            {
            }
            else
            {
                // 대차 이동은 대기,보관중인 대차만 이동할 수 있습니다.
                Util.MessageValidation("SFU4897");
                return false;
            }

            _cartID = Util.NVC(DataTableConverter.GetValue(dgListOut.Rows[rowIndex].DataItem, "CTNR_ID"));

            return true;
        }

        private bool ValidationTakeOverIn()
        {
            if (dgListIn.Rows.Count - dgListIn.FrozenBottomRowsCount <= 0)
            {
                // 조회된 Data가 없습니다.
                Util.MessageValidation("SFU1905");
                return false;
            }

            int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgListIn, "CHK");
            if (rowIndex < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            _cartID = Util.NVC(DataTableConverter.GetValue(dgListIn.Rows[rowIndex].DataItem, "CTNR_ID"));

            return true;
        }

        /// <summary>
        /// 재공처리 유형변경 팝업
        /// </summary>
        private void WipPrcsTypeChange()
        {
            int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgListOut, "CHK");

            CMM_POLYMER_FORM_CART_WIP_PRCS_TYPE_CHANGE popupWipPrcsTypeChange = new CMM_POLYMER_FORM_CART_WIP_PRCS_TYPE_CHANGE();
            popupWipPrcsTypeChange.FrameOperation = this.FrameOperation;

            object[] parameters = new object[7];
            parameters[0] = _procID;
            parameters[1] = _procName;
            parameters[2] = _eqptID;
            parameters[3] = _eqptName;
            parameters[4] = Util.NVC(DataTableConverter.GetValue(dgListOut.Rows[rowIndex].DataItem, "CTNR_ID"));
            parameters[5] = Util.NVC(DataTableConverter.GetValue(dgListOut.Rows[rowIndex].DataItem, "WIP_PRCS_TYPE_CODE"));
            parameters[6] = Util.NVC(DataTableConverter.GetValue(dgListOut.Rows[rowIndex].DataItem, "WIP_PRCS_TYPE_NAME"));

            C1WindowExtension.SetParameters(popupWipPrcsTypeChange, parameters);

            popupWipPrcsTypeChange.Closed += new EventHandler(popupWipPrcsTypeChange_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupWipPrcsTypeChange);
                    popupWipPrcsTypeChange.BringToFront();
                    break;
                }
            }
        }

        private void popupWipPrcsTypeChange_Closed(object sender, EventArgs e)
        {
            InitializeUserControls();

            CMM_POLYMER_FORM_CART_WIP_PRCS_TYPE_CHANGE popup = sender as CMM_POLYMER_FORM_CART_WIP_PRCS_TYPE_CHANGE;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                OutSearch();
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }
        }

        /// <summary>
        /// Cell 등록 팝업
        /// </summary>
        private void CellInsert()
        {
            CMM_POLYMER_CELL_INPUT popupCellInsert = new CMM_POLYMER_CELL_INPUT();
            popupCellInsert.FrameOperation = this.FrameOperation;

            popupCellInsert.CTNR_DEFC_LOT_CHK = "Y";

            object[] parameters = new object[1];
            parameters[0] = _cartID;

            C1WindowExtension.SetParameters(popupCellInsert, parameters);

            popupCellInsert.Closed += new EventHandler(popupCellInsert_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCellInsert);
                    popupCellInsert.BringToFront();
                    break;
                }
            }
        }

        private void popupCellInsert_Closed(object sender, EventArgs e)
        {
            InitializeUserControls();

            CMM_POLYMER_CELL_INPUT popup = sender as CMM_POLYMER_CELL_INPUT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                OutSearch();
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }
        }

        /// <summary>
        /// 상세조회 팝업
        /// </summary>
        private void CartDetail()
        {
            CMM_POLYMER_FORM_CART_DETAIL popupCartDetail = new CMM_POLYMER_FORM_CART_DETAIL();
            popupCartDetail.FrameOperation = this.FrameOperation;

            object[] parameters = new object[5];
            parameters[0] = _procID;
            parameters[1] = _procName;
            parameters[2] = _eqptID;
            parameters[3] = _eqptName;
            parameters[4] = _cartID;    

            C1WindowExtension.SetParameters(popupCartDetail, parameters);

            popupCartDetail.Closed += new EventHandler(popupCartDetail_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCartDetail);
                    popupCartDetail.BringToFront();
                    break;
                }
            }
        }

        private void popupCartDetail_Closed(object sender, EventArgs e)
        {
            InitializeUserControls();

            CMM_POLYMER_FORM_CART_DETAIL popup = sender as CMM_POLYMER_FORM_CART_DETAIL;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                if (((System.Windows.FrameworkElement)ctbTakeOver.SelectedItem).Name.Equals("TakeOverOut"))
                    OutSearch();
                else
                    InSearch();
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }
        }

        /// <summary>
        /// 활성화이동 팝업
        /// </summary>
        private void CartFCSMove()
        {
            CMM_POLYMER_FORM_CART_FCS_MOVE popupCartFCSMove = new CMM_POLYMER_FORM_CART_FCS_MOVE();
            popupCartFCSMove.FrameOperation = this.FrameOperation;

            if (ValidationGridAdd(popupCartFCSMove.Name) == false)
                return;

            object[] parameters = new object[6];
            parameters[0] = _procID;
            parameters[1] = _procName;
            parameters[2] = _eqptID;
            parameters[3] = _eqptName;
            parameters[4] = _cartID;
            parameters[5] = _util.GetDataGridFirstRowBycheck(dgListOut, "CHK");

            C1WindowExtension.SetParameters(popupCartFCSMove, parameters);

            popupCartFCSMove.Closed += new EventHandler(popupCartFCSMove_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCartFCSMove);
                    popupCartFCSMove.BringToFront();
                    break;
                }
            }
        }

        private void popupCartFCSMove_Closed(object sender, EventArgs e)
        {
            InitializeUserControls();

            CMM_POLYMER_FORM_CART_FCS_MOVE popup = sender as CMM_POLYMER_FORM_CART_FCS_MOVE;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                if (((System.Windows.FrameworkElement)ctbTakeOver.SelectedItem).Name.Equals("TakeOverOut"))
                    OutSearch();
                else
                    InSearch();
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }
        }

        /// <summary>
        /// 인계취소 팝업
        /// </summary>
        private void CartTakeOverCancel()
        {
            CMM_POLYMER_FORM_CART_TAKEOVER_CANCEL popupCartTakeOverCancel = new CMM_POLYMER_FORM_CART_TAKEOVER_CANCEL();
            popupCartTakeOverCancel.FrameOperation = this.FrameOperation;

            int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgListOut, "CHK");

            object[] parameters = new object[6];
            parameters[0] = _procID;
            parameters[1] = _procName;
            parameters[2] = _eqptID;
            parameters[3] = _eqptName;
            parameters[4] = Util.NVC(DataTableConverter.GetValue(dgListOut.Rows[rowIndex].DataItem, "MOVE_ORD_ID"));
            parameters[5] = Util.NVC(DataTableConverter.GetValue(dgListOut.Rows[rowIndex].DataItem, "CTNR_ID"));

            C1WindowExtension.SetParameters(popupCartTakeOverCancel, parameters);

            popupCartTakeOverCancel.Closed += new EventHandler(popupCartTakeOverCancel_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCartTakeOverCancel);
                    popupCartTakeOverCancel.BringToFront();
                    break;
                }
            }
        }

        private void popupCartTakeOverCancel_Closed(object sender, EventArgs e)
        {
            InitializeUserControls();

            CMM_POLYMER_FORM_CART_TAKEOVER_CANCEL popup = sender as CMM_POLYMER_FORM_CART_TAKEOVER_CANCEL;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                if (((System.Windows.FrameworkElement)ctbTakeOver.SelectedItem).Name.Equals("TakeOverOut"))
                    OutSearch();
                else
                    InSearch();
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }
        }

        /// <summary>
        /// 대차인계 팝업
        /// </summary>
        private void CartTakeOverOut()
        {
            CMM_POLYMER_FORM_CART_TAKEOVER_OUT popupCartTakeOverOut = new CMM_POLYMER_FORM_CART_TAKEOVER_OUT();
            popupCartTakeOverOut.FrameOperation = this.FrameOperation;

            if (ValidationGridAdd(popupCartTakeOverOut.Name) == false)
                return;

            int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgListOut, "CHK");

            object[] parameters = new object[6];
            parameters[0] = _procID;
            parameters[1] = _procName;
            parameters[2] = _eqptID;
            parameters[3] = _eqptName;
            parameters[4] = _cartID;
            parameters[5] = _util.GetDataGridFirstRowBycheck(dgListOut, "CHK");

            C1WindowExtension.SetParameters(popupCartTakeOverOut, parameters);

            popupCartTakeOverOut.Closed += new EventHandler(popupCartTakeOverOut_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCartTakeOverOut);
                    popupCartTakeOverOut.BringToFront();
                    break;
                }
            }
        }

        private void popupCartTakeOverOut_Closed(object sender, EventArgs e)
        {
            InitializeUserControls();

            CMM_POLYMER_FORM_CART_TAKEOVER_OUT popup = sender as CMM_POLYMER_FORM_CART_TAKEOVER_OUT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                if (((System.Windows.FrameworkElement)ctbTakeOver.SelectedItem).Name.Equals("TakeOverOut"))
                    OutSearch();
                else
                    InSearch();
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }
        }

        /// <summary>
        /// 대차인수 팝업
        /// </summary>
        private void CartTakeOverIn()
        {
            CMM_POLYMER_FORM_CART_TAKEOVER_IN popupCartTakeOverIn = new CMM_POLYMER_FORM_CART_TAKEOVER_IN();
            popupCartTakeOverIn.FrameOperation = this.FrameOperation;

            if (ValidationGridAdd(popupCartTakeOverIn.Name) == false)
                return;

            object[] parameters = new object[5];
            parameters[0] = _procID;
            parameters[1] = _procName;
            parameters[2] = _eqptID;
            parameters[3] = _eqptName;
            parameters[4] = DataTableConverter.Convert(dgListIn.ItemsSource).Select("CHK = 1");

            C1WindowExtension.SetParameters(popupCartTakeOverIn, parameters);

            popupCartTakeOverIn.Closed += new EventHandler(popupCartTakeOverIn_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCartTakeOverIn);
                    popupCartTakeOverIn.BringToFront();
                    break;
                }
            }
        }

        private void popupCartTakeOverIn_Closed(object sender, EventArgs e)
        {
            InitializeUserControls();

            CMM_POLYMER_FORM_CART_TAKEOVER_IN popup = sender as CMM_POLYMER_FORM_CART_TAKEOVER_IN;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                if (((System.Windows.FrameworkElement)ctbTakeOver.SelectedItem).Name.Equals("TakeOverOut"))
                    OutSearch();
                else
                    InSearch();
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }
        }

        private bool ValidationSearch()
        {
            if (((System.Windows.FrameworkElement)ctbTakeOver.SelectedItem).Name.Equals("TakeOverOut"))
            {
                if (string.IsNullOrWhiteSpace(txtProcessOut.Text))
                {
                    // 공정 정보가 없습니다.
                    Util.MessageValidation("SFU1456");
                    return false;
                }

                if (cboEquipmentSegmentOut.SelectedItemsToString == string.Empty)
                {
                    // 라인을 선택해주세요.
                    Util.MessageValidation("SFU4050");
                    return false;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtProcessIn.Text))
                {
                    // 공정 정보가 없습니다.
                    Util.MessageValidation("SFU1456");
                    return false;
                }

                if (cboEquipmentSegmentIn.SelectedItemsToString == string.Empty)
                {
                    // 라인을 선택해주세요.
                    Util.MessageValidation("SFU4050");
                    return false;
                }
            }

            return true;
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
        }

        private bool ValidationGridAdd(string popName)
        {
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    foreach (UIElement ui in tmp.Children)
                    {
                        if (((System.Windows.FrameworkElement)ui).Name.Equals(popName))
                        {
                            // 프로그램이 이미 실행 중 입니다. 
                            Util.MessageValidation("SFU3193");
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        #endregion

    }
}