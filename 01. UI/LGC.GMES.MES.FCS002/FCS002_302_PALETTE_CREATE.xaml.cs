/*************************************************************************************
 Created Date : 2017.07.25
      Creator : 
   Decription : Pallet 생성, 수정
--------------------------------------------------------------------------------------
 [Change History]

 2023.03.09  0.3   LEEHJ    SI               소형활성화 MES 복사
**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.UserControls;

using Application = System.Windows.Application;
using DataGridLength = C1.WPF.DataGrid.DataGridLength;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Linq;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Popup;
using System.Windows.Input;
using System.Windows.Data;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_302_PALETTE_CREATE : C1Window, IWorkArea
    {
        #region Declaration
        public UcFormShift UcFormShift { get; set; }

        Util _Util = new Util();
        CommonCombo _combo = new CMM001.Class.CommonCombo();
 
        private string _create = string.Empty;        // 생성, 수정 구분
        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드
        private string _wipSeq = string.Empty;

        DataTable _capa;
        DataTable _rsst;
        DataTable _vltg;

        private bool _load = true;

        public string ShiftID { get; set; }
        public string ShiftName { get; set; }
        public string WorkerName { get; set; }
        public string WorkerID { get; set; }
        public string ShiftDateTime { get; set; }


        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public FCS002_302_PALETTE_CREATE()
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
                SetControlVisibility();
                _load = false;
            }
        }

        private void InitializeUserControls()
        {
            UcFormShift = grdShift.Children[0] as UcFormShift;
        }
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _create = tmps[0] as string;
            _procID = tmps[1] as string;
            _eqptID = tmps[2] as string;

            // SET COMMON
            txtProcess.Text = tmps[4] as string;
            txtEquipment.Text = tmps[5] as string;

            // SET 생산 Lot 정보
            DataRow prodLot = tmps[3] as DataRow;

            if (prodLot == null)
                return;

            DataTable prodLotBind = new DataTable();
            prodLotBind = prodLot.Table.Clone();
            prodLotBind.ImportRow(prodLot);

            Util.GridSetData(dgLot, prodLotBind, null, true);

            // Inbox type
            string[] sFilter = { LoginInfo.CFG_AREA_ID, _procID };
            _combo.SetCombo(cboInboxType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

            //string[] sFilterCapa = { "CAPA_GRD_CODE" };
            //_combo.SetCombo(cboCapaType, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilterCapa);

            //SetCommCode(dgPallet, "CAPA_GRD_CODE", "G");
            //SetCommCode(dgPallet, "RSST_GRD_CODE");
            //SetCommCode(dgPallet, "VLTG_GRD_CODE");
            SetGradeComboCode(dgPallet, "CAPA_GRD_CODE", "G");
            SetGradeComboCode(dgPallet, "RSST_GRD_CODE");
            SetGradeComboCode(dgPallet, "VLTG_GRD_CODE");

            if (string.Equals(_create, "Y"))
            {
                // Inbox type 셋팅
                SetEqptInboxType();

                SetBoxQty(true);

                // 빈 Row 생성(10개)
                for (int row = 0; row < 10; row++)
                {
                    SetPalletRowADD();
                }
            }
            else
            {
                btnRowAdd.IsEnabled = false;
                btnRowDel.IsEnabled = false;

                // 선택된 Pallet 정보
                DataRow drPallet = tmps[6] as DataRow;

                txtPalletID.Text = drPallet["PALLETE_ID"].ToString();
                cboInboxType.SelectedValue = drPallet["INBOX_TYPE_CODE"].ToString();
                txtInboxQty.Text = drPallet["INBOX_LOAD_QTY"].ToString();
                txtNote.Text = drPallet["WIP_NOTE"].ToString();
                _wipSeq = drPallet["WIPSEQ"].ToString();

                //// IsReadOnly = true;
                //dgPallet.Columns["CAPA_GRD_CODE"].IsReadOnly = true;
                //dgPallet.Columns["RSST_GRD_CODE"].IsReadOnly = true;
                //dgPallet.Columns["VLTG_GRD_CODE"].IsReadOnly = true;

                // 등급별 Cell수량/Box 수량(수정)
                SetGridModifyPallet();
            }

            cboInboxType.SelectedIndexChanged += cboInboxType_SelectedIndexChanged;

            // 작업자, 작업조
            UcFormShift.TextShift.Tag = ShiftID;
            UcFormShift.TextShift.Text = ShiftName;
            UcFormShift.TextWorker.Tag = WorkerID;
            UcFormShift.TextWorker.Text = WorkerName;
            UcFormShift.TextShiftDateTime.Text = ShiftDateTime;

            UcFormShift = grdShift.Children[0] as UcFormShift;
            if (UcFormShift != null)
            {
                UcFormShift.ButtonShift.Click += ButtonShift_Click;
            }

            // Focus 
            dgPallet.Focus();
            dgPallet.LoadedCellPresenter -= dgPallet_LoadedCellPresenter;
            if (dgPallet.Rows.Count - dgPallet.FrozenBottomRowsCount > 0)
            {
                dgPallet.CurrentCell = dgPallet.GetCell(0, dgPallet.Columns["CELL_QTY"].Index);
                dgPallet.Selection.Add(dgPallet.GetCell(0, dgPallet.Columns["CELL_QTY"].Index));
            }
            dgPallet.LoadedCellPresenter += dgPallet_LoadedCellPresenter;
        }
        private void SetControlVisibility()
        {
            if (string.Equals(_create, "Y"))
            {
                tbPalletID.Visibility = Visibility.Collapsed;
                txtPalletID.Visibility = Visibility.Collapsed;

                this.Header = ObjectDic.Instance.GetObjectName("Pallet 생성");
                btnCreate.Content = ObjectDic.Instance.GetObjectName("생성");
            }
            else
            {
                this.Header = ObjectDic.Instance.GetObjectName("Pallet 수정");
                btnCreate.Content = ObjectDic.Instance.GetObjectName("수정");
            }

        }

        #endregion

        #region [Pallet 생성, 수정]
        /// <summary>
        /// Pallet 생성, 수정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            // 변경전 자료 반영
            dgPallet.EndEditRow(true);

            if (string.Equals(_create, "Y"))
                CreatePallet();
            else
                ModifyPallet();

        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region [행추가]
        private void btnRowAdd_Click(object sender, RoutedEventArgs e)
        {
            SetPalletRowADD();
        }
        #endregion

        #region [행삭제]
        private void btnRowDel_Click(object sender, RoutedEventArgs e)
        {
            if (dgPallet.SelectedIndex < 0)
            {
                // SFU1651 선택된 항목이 없습니다.
                Util.MessageInfo("SFU1651");
                return;
            }

            DataTable dtDel = DataTableConverter.Convert(dgPallet.ItemsSource);
            dtDel.Rows[dgPallet.SelectedIndex].Delete();
            dtDel.AcceptChanges();

            Util.GridSetData(dgPallet, dtDel, null);

        }
        #endregion

        #region [Box Type 콤보 변경시]
        private void cboInboxType_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            SetBoxQty(false);
        }
        #endregion

        #region [등급별 Cell수량 변경시]
        private void dgPallet_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Column.Name.Equals("CELL_QTY"))
            {
                double BaseInboxQty = 0;
                double CellQty = 0;
                int InboxQty = 0;

                if (!double.TryParse(txtInboxQty.Text, out BaseInboxQty))
                {
                    return;
                }

                CellQty = Convert.ToDouble(Util.NVC_Decimal(DataTableConverter.GetValue(dgPallet.Rows[e.Cell.Row.Index].DataItem, "CELL_QTY")));

                if (CellQty == 0 || BaseInboxQty == 0)
                {
                    InboxQty = 0;
                }
                else
                {
                    if (CellQty % BaseInboxQty > 0)
                        InboxQty = Convert.ToInt16(Math.Truncate(CellQty / BaseInboxQty)) + 1;
                    else
                        InboxQty = Convert.ToInt16(CellQty / BaseInboxQty);

                }

                DataTableConverter.SetValue(dgPallet.Rows[e.Cell.Row.Index].DataItem, "INBOX_QTY", InboxQty);
            }

        }
        #endregion

        #region [등급별 Cell수량/Box 수량시 IsReadOnly 바탕색 처리]: dgPallet_LoadedCellPresenter, PreviewKeyDown 
        private void dgPallet_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Cell.Row.Type.Equals(DataGridRowType.Top))
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.IsReadOnly == true && e.Cell.Column.Visibility == Visibility.Visible)
                {
                    e.Cell.Presenter.Background = new System.Windows.Media.SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                }

            }));

        }

        private void dgPallet_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            C1DataGrid grid = sender as C1DataGrid;

            if (e.Key == Key.Enter)
            {
                int rIdx = 0;
                int cIdx = 0;

                rIdx = grid.CurrentCell.Row.Index;
                cIdx = grid.CurrentCell.Column.Index;

                if (grid.GetRowCount() > ++rIdx)
                {
                    grid.Selection.Clear();
                    grid.CurrentCell = grid.GetCell(rIdx, cIdx);
                    grid.Selection.Add(grid.GetCell(rIdx, cIdx));

                    if (grid.GetRowCount() - 1 != rIdx)
                    {
                        grid.ScrollIntoView(rIdx + 1, cIdx);
                    }
                }
            }
            else
            {
                if (grid.CurrentColumn.Name.Equals("CAPA_GRD_CODE") || grid.CurrentColumn.Name.Equals("RSST_GRD_CODE") || grid.CurrentColumn.Name.Equals("VLTG_GRD_CODE"))
                {
                    DataTable dtSelect = DataTableConverter.Convert(grid.ItemsSource);

                    string KeyValue = string.Empty;

                    if (e.Key.ToString().IndexOf('D') > -1 && (e.Key.ToString().Length == 2))
                    {
                        KeyValue = e.Key.ToString().Substring(1, 1);
                    }
                    else
                    {
                        KeyValue = e.Key.ToString();
                    }

                    DataRow[] drSelect = null;

                    if (grid.CurrentColumn.Name.Equals("CAPA_GRD_CODE"))
                        drSelect = _capa.Select("CBO_CODE ='" + KeyValue + "'");
                    else if (grid.CurrentColumn.Name.Equals("RSST_GRD_CODE"))
                        drSelect = _rsst.Select("CBO_CODE ='" + KeyValue + "'");
                    else
                        drSelect = _vltg.Select("CBO_CODE ='" + KeyValue + "'");

                    if (drSelect.Length > 0)
                    {
                        dtSelect.Rows[grid.CurrentRow.Index][grid.CurrentColumn.Name] = KeyValue;
                        Util.GridSetData(dgPallet, dtSelect, null, true);
                    }
                }

            }

        }
        #endregion

        #endregion

        #region User Method

        #region [BizCall]

        /// <summary>
        /// 용량
        /// </summary>
        private void SetGradeComboCode(C1DataGrid dg, string comTypeCode, string QltyTypeCode = null)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("QLTY_TYPE_CODE", typeof(string));
                inTable.Columns.Add("CELL_GRD_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                newRow["QLTY_TYPE_CODE"] = QltyTypeCode;
                newRow["CELL_GRD_TYPE_CODE"] = comTypeCode;
                inTable.Rows.Add(newRow);
                                                                           
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_GRADE_TYPE_CODE_LINE_CBO", "INDATA", "OUTDATA", inTable);

                DataRow dr = dtResult.NewRow();
                dr["CBO_CODE"] = "";
                dr["CBO_NAME"] = " - SELECT-";
                dtResult.Rows.InsertAt(dr, 0);

                (dgPallet.Columns[comTypeCode] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult.Copy());

                //_capa = dtResult.Copy();
                if (comTypeCode.Equals("CAPA_GRD_CODE"))
                    _capa = dtResult.Copy();
                else if (comTypeCode.Equals("RSST_GRD_CODE"))
                    _rsst = dtResult.Copy();
                else
                    _vltg = dtResult.Copy();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 용량, 저항, 전압등급-----------> 사용 안함 변경
        /// </summary>
        private void SetCommCode(C1DataGrid dg, string comTypeCode, string attr1 = null)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("ATTR1", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["COM_TYPE_CODE"] = comTypeCode;
                newRow["ATTR1"] = attr1;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_GRADE_TYPE_CODE_CBO", "INDATA", "OUTDATA", inTable);

                DataRow dr = dtResult.NewRow();
                dr["CBO_CODE"] = "";
                dr["CBO_NAME"] = " - SELECT-";
                dtResult.Rows.InsertAt(dr, 0);

                (dgPallet.Columns[comTypeCode] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult.Copy());

                if (comTypeCode.Equals("CAPA_GRD_CODE"))
                    _capa = dtResult.Copy();
                else if (comTypeCode.Equals("RSST_GRD_CODE"))
                    _rsst = dtResult.Copy();
                else
                    _vltg = dtResult.Copy();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #region 설비 InBox 유형 조회
        private void SetEqptInboxType()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _eqptID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_INBOX_TYPE", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (cboInboxType.Items.Count > 0)
                        cboInboxType.SelectedValue = dtResult.Rows[0]["INBOX_TYPE"].ToString();
                }

                if (cboInboxType.SelectedValue == null)
                {
                    cboInboxType.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        /// <summary>
        /// 생성 Pallet 그리드 조회
        /// </summary>
        private void SetGridModifyPallet()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = txtPalletID.Text;
                newRow["WIPSEQ"] = _wipSeq;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_VLTG_MODIFY_FO", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgPallet, dtResult, null, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// Pallet 생성
        /// </summary>
        private void SetCreatePallet()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_CREATE_PALLET_CG";

                // DATA SET
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("WIPNOTE", typeof(string));
                inTable.Columns.Add("SHIFT", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));

                DataTable inBox = inDataSet.Tables.Add("IN_BOX");
                inBox.Columns.Add("CAPA_GRD_CODE", typeof(string));
                inBox.Columns.Add("VLTG_GRD_CODE", typeof(string));
                inBox.Columns.Add("RSST_GRD_CODE", typeof(string));
                inBox.Columns.Add("CELL_QTY", typeof(Decimal));
                inBox.Columns.Add("INBOX_QTY", typeof(Decimal));
                inBox.Columns.Add("INBOX_TYPE_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_eqptID);
                newRow["PROD_LOTID"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "LOTID");
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WIPNOTE"] = txtNote.Text;
                newRow["SHIFT"] = UcFormShift.TextShift.Tag;
                newRow["WRK_USERID"] = UcFormShift.TextWorker.Tag;
                newRow["WRK_USER_NAME"] = UcFormShift.TextWorker.Text;
                inTable.Rows.Add(newRow);

                foreach (DataGridRow dRow in dgPallet.Rows)
                {
                    //if (Util.NVC_Int(DataTableConverter.GetValue(dRow.DataItem, "CELL_QTY")) != 0 ||
                    //    Util.NVC_Int(DataTableConverter.GetValue(dRow.DataItem, "INBOX_QTY")) != 0)

                    if (Util.NVC_Int(DataTableConverter.GetValue(dRow.DataItem, "CELL_QTY")) != 0)
                    {
                        newRow = inBox.NewRow();
                        newRow["CAPA_GRD_CODE"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "CAPA_GRD_CODE"));
                        newRow["VLTG_GRD_CODE"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "VLTG_GRD_CODE"));
                        newRow["RSST_GRD_CODE"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "RSST_GRD_CODE"));
                        newRow["CELL_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dRow.DataItem, "CELL_QTY"));
                        newRow["INBOX_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dRow.DataItem, "INBOX_QTY"));
                        newRow["INBOX_TYPE_CODE"] = cboInboxType.SelectedValue == null ? null : cboInboxType.SelectedValue.ToString();
                        inBox.Rows.Add(newRow);
                    }
                }

                //string sTestXml = inDataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_BOX", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;
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

        /// <summary>
        /// Pallet 수정
        /// </summary>
        private void SetModifyPallet()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_MODIFY_PALLET_NEW";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("PALLETID", typeof(string));
                inTable.Columns.Add("WIPQTY", typeof(Decimal));
                inTable.Columns.Add("WIPNOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("SHIPTO_NOTE", typeof(string));
                inTable.Columns.Add("SHIPTO_ID", typeof(string));
                inTable.Columns.Add("SHIFT", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inTable.Columns.Add("CAPA_GRD_CODE", typeof(string));
                inTable.Columns.Add("VLTG_GRD_CODE", typeof(string));
                inTable.Columns.Add("RSST_GRD_CODE", typeof(string));
                inTable.Columns.Add("INBOX_TYPE_CODE", typeof(string));
                inTable.Columns.Add("INBOX_QTY", typeof(Decimal));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["PROCID"] = Util.NVC(_procID);
                newRow["PALLETID"] = txtPalletID.Text;
                newRow["WIPQTY"] = DataTableConverter.GetValue(dgPallet.Rows[0].DataItem, "CELL_QTY");
                newRow["WIPNOTE"] = txtNote.Text;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["SHIFT"] = UcFormShift.TextShift.Tag;
                newRow["WRK_USERID"] = UcFormShift.TextWorker.Tag;
                newRow["WRK_USER_NAME"] = UcFormShift.TextWorker.Text;
                newRow["CAPA_GRD_CODE"] = DataTableConverter.GetValue(dgPallet.Rows[0].DataItem, "CAPA_GRD_CODE");
                newRow["VLTG_GRD_CODE"] = DataTableConverter.GetValue(dgPallet.Rows[0].DataItem, "VLTG_GRD_CODE");
                newRow["RSST_GRD_CODE"] = DataTableConverter.GetValue(dgPallet.Rows[0].DataItem, "RSST_GRD_CODE");
                newRow["INBOX_TYPE_CODE"] = cboInboxType.SelectedValue == null ? null : cboInboxType.SelectedValue.ToString();
                newRow["INBOX_QTY"] = DataTableConverter.GetValue(dgPallet.Rows[0].DataItem, "INBOX_QTY");
                inTable.Rows.Add(newRow);
      
                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;
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

        #region 작업조, 작업자
        private void GetEqptWrkInfo()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _eqptID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                newRow["PROCID"] = _procID; ;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (UcFormShift != null)
                        {
                            if (result.Rows.Count > 0)
                            {
                                if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                                {
                                    UcFormShift.TextShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                                }
                                else
                                {
                                    UcFormShift.TextShiftStartTime.Text = string.Empty;
                                }

                                if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                                {
                                    UcFormShift.TextShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                                }
                                else
                                {
                                    UcFormShift.TextShiftEndTime.Text = string.Empty;
                                }

                                if (!string.IsNullOrEmpty(UcFormShift.TextShiftStartTime.Text) && !string.IsNullOrEmpty(UcFormShift.TextShiftEndTime.Text))
                                {
                                    UcFormShift.TextShiftDateTime.Text = UcFormShift.TextShiftStartTime.Text + " ~ " + UcFormShift.TextShiftEndTime.Text;
                                }
                                else
                                {
                                    UcFormShift.TextShiftDateTime.Text = string.Empty;
                                }

                                if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                                {
                                    UcFormShift.TextWorker.Text = string.Empty;
                                    UcFormShift.TextWorker.Tag = string.Empty;
                                }
                                else
                                {
                                    UcFormShift.TextWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                    UcFormShift.TextWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                                }

                                if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                                {
                                    UcFormShift.TextShift.Tag = string.Empty;
                                    UcFormShift.TextShift.Text = string.Empty;
                                }
                                else
                                {
                                    UcFormShift.TextShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                    UcFormShift.TextShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                                }
                            }
                            else
                            {
                                UcFormShift.ClearShiftControl();
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region[[Validation]

        /// <summary>
        /// Pallet 생성
        /// </summary>
        private void CreatePallet()
        {
            if (!ValidateCreate())
                return;

            // Pallet를 생성 하시겠습니까?
            Util.MessageConfirm("SFU4006", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetCreatePallet();
                }
            });

        }

        /// <summary>
        /// Pallet 수정
        /// </summary>
        private void ModifyPallet()
        {
            if (!ValidateModify())
                return;

            // Pallet를 수정 하시겠습니까?
            Util.MessageConfirm("SFU4007", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetModifyPallet();
                }
            });

        }

        private bool ValidateCreate()
        {
            ////DataRow[] drChk = DataTableConverter.Convert(dgPallet.ItemsSource).Select("CELL_QTY <> 0 OR INBOX_QTY <> 0");
            DataRow[] drChk = DataTableConverter.Convert(dgPallet.ItemsSource).Select("CELL_QTY <> 0");

            if (drChk.Length == 0)
            {
                // 등급별 Cell수량을 입력 하세요.
                Util.MessageValidation("SFU4004");
                return false;
            }

            if (cboInboxType.SelectedValue == null || cboInboxType.SelectedValue.GetString().Equals("SELECT"))
            {
                // Inbox를 선택해 주세요.
                Util.MessageValidation("SFU4005");
                return false;
            }

            int chkCount = 0;
            foreach (DataRow row in drChk)
            {
                chkCount = 0;

                if (string.IsNullOrWhiteSpace(row["CAPA_GRD_CODE"].ToString()))
                {
                    //// 용량등급을 선택해 주세요.
                    //Util.MessageValidation("SFU4022");
                    //return false;
                    chkCount++;
                }

                if (string.IsNullOrWhiteSpace(row["RSST_GRD_CODE"].ToString()))
                {
                    //// 저항등급을 선택해 주세요.
                    //Util.MessageValidation("SFU4242");
                    //return false;
                    chkCount++;
                }

                if (string.IsNullOrWhiteSpace(row["VLTG_GRD_CODE"].ToString()))
                {
                    //// 전압등급을 선택해 주세요.
                    //Util.MessageValidation("SFU4279");
                    //return false;
                    chkCount++;
                }

                if (chkCount == 3)
                {
                    // 한 개 이상의 등급을 입력 하세요.
                    Util.MessageValidation("SFU4302");
                    return false;
                }

            }

            if (UcFormShift.TextShift.Tag == null || string.IsNullOrEmpty(UcFormShift.TextShift.Tag.ToString()))
            {
                // 작업조를 입력해 주세요.
                Util.MessageValidation("SFU1845");
                return false;
            }

            if (UcFormShift.TextWorker.Tag == null || string.IsNullOrEmpty(UcFormShift.TextWorker.Tag.ToString()))
            {
                // 작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return false;
            }

            return true;
        }

        private bool ValidateModify()
        {
            DataTable dtChk = DataTableConverter.Convert(dgPallet.ItemsSource);

            ////DataRow[] drChk = dtChk.Select("CELL_QTY <> 0 OR INBOX_QTY <> 0");
            DataRow[] drChk = dtChk.Select("CELL_QTY <> 0");

            if (drChk.Length == 0)
            {
                // 등급별 Cell수량을 입력 하세요.
                Util.MessageValidation("SFU4004");
                return false;
            }

            if (cboInboxType.SelectedValue == null || cboInboxType.SelectedValue.GetString().Equals("SELECT"))
            {
                // Inbox를 선택해 주세요.
                Util.MessageValidation("SFU4005");
                return false;
            }

            int chkCount = 0;
            foreach (DataRow row in drChk)
            {
                if (string.IsNullOrWhiteSpace(row["CAPA_GRD_CODE"].ToString()))
                {
                    //// 용량등급을 선택해 주세요.
                    //Util.MessageValidation("SFU4022");
                    //return false;
                    chkCount++;
                }

                if (string.IsNullOrWhiteSpace(row["RSST_GRD_CODE"].ToString()))
                {
                    //// 저항등급을 선택해 주세요.
                    //Util.MessageValidation("SFU4242");
                    //return false;
                    chkCount++;
                }

                if (string.IsNullOrWhiteSpace(row["VLTG_GRD_CODE"].ToString()))
                {
                    //// 전압등급을 선택해 주세요.
                    //Util.MessageValidation("SFU4279");
                    //return false;
                    chkCount++;
                }

                if (chkCount == 3)
                {
                    // 한 개 이상의 등급을 입력 하세요.
                    Util.MessageValidation("SFU4302");
                    return false;
                }

            }

            if (UcFormShift.TextShift.Tag == null || string.IsNullOrEmpty(UcFormShift.TextShift.Tag.ToString()))
            {
                // 작업조를 입력해 주세요.
                Util.MessageValidation("SFU1845");
                return false;
            }

            if (UcFormShift.TextWorker.Tag == null || string.IsNullOrEmpty(UcFormShift.TextWorker.Tag.ToString()))
            {
                // 작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return false;
            }

            return true;
        }
        #endregion

        #region [Func]
        private void SetBoxQty(bool FormLoad)
        {
            txtInboxQty.Text = string.Empty;

            if (cboInboxType.SelectedIndex < 0)
                return;

            DataTable dtInboxType = DataTableConverter.Convert(cboInboxType.ItemsSource);

            if (dtInboxType == null || dtInboxType.Rows.Count == 0)
                return;

            txtInboxQty.Text = dtInboxType.Rows[cboInboxType.SelectedIndex]["INBOX_LOAD_QTY"].ToString();

            if (FormLoad)
                return;

            ///////////////////////////////////////////////////////////////// Box 수량 산출
            double BaseInboxQty = 0;
            int InboxQty = 0;

            if (!double.TryParse(txtInboxQty.Text, out BaseInboxQty))
            {
                return;
            }

            // Box수를 선택한 Box 수량으로 다시 산출 하시겠습니까?
            Util.MessageConfirm("SFU4021", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    foreach (DataGridRow dRow in dgPallet.Rows)
                    {
                        double CellQty = Convert.ToDouble(Util.NVC_Decimal(DataTableConverter.GetValue(dRow.DataItem, "CELL_QTY")));

                        if (CellQty != 0)
                        {
                            if (CellQty % BaseInboxQty > 0)
                                InboxQty = Convert.ToInt16(Math.Truncate(CellQty / BaseInboxQty)) + 1;
                            else
                                InboxQty = Convert.ToInt16(CellQty / BaseInboxQty);

                            DataTableConverter.SetValue(dRow.DataItem, "INBOX_QTY", InboxQty);
                        }
                    }

                }
            });
        }

        private void ButtonShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 popupShiftUser = new CMM_SHIFT_USER2 { FrameOperation = this.FrameOperation };

            object[] parameters = new object[8];
            parameters[0] = LoginInfo.CFG_SHOP_ID;
            parameters[1] = LoginInfo.CFG_AREA_ID;
            parameters[2] = LoginInfo.CFG_EQSG_ID;
            parameters[3] = _procID;
            parameters[4] = Util.NVC(UcFormShift.TextShift.Tag);
            parameters[5] = Util.NVC(UcFormShift.TextWorker.Tag);
            parameters[6] = _eqptID;
            parameters[7] = "Y"; // 저장 Flag "Y" 일때만 저장.
            C1WindowExtension.SetParameters(popupShiftUser, parameters);

            popupShiftUser.Closed += new EventHandler(popupShiftUser_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupShiftUser);
                    popupShiftUser.BringToFront();
                    break;
                }
            }
        }

        private void popupShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 popup = sender as CMM_SHIFT_USER2;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetEqptWrkInfo();
            }
            this.Focus();
        }

        private void SetPalletRowADD()
        {
            DataTable dtAdd = DataTableConverter.Convert(dgPallet.ItemsSource);

            if (dtAdd == null || dtAdd.Columns.Count == 0)
            {
                dtAdd.Columns.Add("QLTY_TYPE_CODE", typeof(string));
                dtAdd.Columns.Add("QLTY_TYPE_NAME", typeof(string));
                dtAdd.Columns.Add("CAPA_GRD_CODE", typeof(string));
                dtAdd.Columns.Add("RSST_GRD_CODE", typeof(string));
                dtAdd.Columns.Add("VLTG_GRD_CODE", typeof(string));
                dtAdd.Columns.Add("CELL_QTY", typeof(Int32));
                dtAdd.Columns.Add("INBOX_QTY", typeof(Int32));
            }

            DataRow drAdd = dtAdd.NewRow();
            dtAdd.Rows.Add(drAdd);
            dtAdd.AcceptChanges();

            Util.GridSetData(dgPallet, dtAdd, null, true);
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






        #endregion

        #endregion





    }
}
