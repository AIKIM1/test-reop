/*************************************************************************************
 Created Date : 2017.07.25
      Creator : 
   Decription : Pallet 생성, 수정
--------------------------------------------------------------------------------------
 [Change History]

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

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_002_PALETTE_CREATE : C1Window, IWorkArea
    {
        #region Declaration
        public UcFormShift UcFormShift { get; set; }

        Util _Util = new Util();
        CommonCombo _combo = new CMM001.Class.CommonCombo();
 
        private string _create = string.Empty;        // 생성, 수정 구분
        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드
        private string _wipSeq = string.Empty;

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

        public FORM001_002_PALETTE_CREATE()
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

            // Pallet 구분
            SetPalletGubun();

            // Inbox type
            string[] sFilter = { LoginInfo.CFG_AREA_ID, _procID };
            _combo.SetCombo(cboInboxType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

            // 용량구분
            //string[] sFilterCapa = { LoginInfo.CFG_AREA_ID, "CAPA_GRD_CODE", null };
            //_combo.SetCombo(cboCapaType, CommonCombo.ComboStatus.NONE, sCase: "FORM_GRADE_TYPE_CODE", sFilter: sFilterCapa);
            string[] sFilterCapa = { LoginInfo.CFG_AREA_ID, LoginInfo.CFG_EQSG_ID, null };
            _combo.SetCombo(cboCapaType, CommonCombo.ComboStatus.NONE, sCase: "FORM_GRADE_TYPE_CODE_LINE", sFilter: sFilterCapa);

            if (string.Equals(_create, "Y"))
            {
                // Inbox type 셋팅
                SetEqptInboxType();

                SetBoxQty(true);

                // 등급별 Cell수량/Box 수량(생성)
                SetGridCreatePallet();
            }
            else
            {
                // 선택된 Pallet 정보
                DataRow drPallet = tmps[6] as DataRow;

                txtPalletID.Text = drPallet["PALLETE_ID"].ToString();
                cboQultType.SelectedValue = drPallet["QLTY_TYPE_CODE"].ToString();
                cboInboxType.SelectedValue = drPallet["INBOX_TYPE_CODE"].ToString();
                cboCapaType.SelectedValue = drPallet["CAPA_GRD_CODE"].ToString();
                txtInboxQty.Text = drPallet["INBOX_LOAD_QTY"].ToString();
                txtShipping.Text = drPallet["SHIPTO_NOTE"].ToString();
                txtShipTo.Tag = drPallet["SHIPTO_ID"].ToString();
                txtShipTo.Text = drPallet["SHIPTO_NAME"].ToString();
                txtNote.Text = drPallet["WIP_NOTE"].ToString();
                _wipSeq = drPallet["WIPSEQ"].ToString();

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
            if (e.Key == Key.Enter)
            {
                int rIdx = 0;
                int cIdx = 0;

                C1DataGrid grid = sender as C1DataGrid;

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

        }
        #endregion

        #region [출하처 제한 팝업]
        private void btnShipTo_Click(object sender, RoutedEventArgs e)
        {
            FORM001_SHIPTO popupShipTo = new FORM001_SHIPTO { FrameOperation = this.FrameOperation };

            object[] parameters = new object[1];
            parameters[0] = txtShipTo.Tag == null ? "" : txtShipTo.Tag.ToString();
            C1WindowExtension.SetParameters(popupShipTo, parameters);

            popupShipTo.Closed += new EventHandler(popupPalletRemainWait_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupShipTo);
                    popupShipTo.BringToFront();
                    break;
                }
            }
        }

        private void popupPalletRemainWait_Closed(object sender, EventArgs e)
        {
            FORM001_SHIPTO popup = sender as FORM001_SHIPTO;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                txtShipTo.Text = popup.ShipTo_Name;
                txtShipTo.Tag = popup.ShipTo_ID;
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

            this.Focus();
        }

        #endregion

        #endregion

        #region User Method

        #region [BizCall]

        /// <summary>
        /// Pallet 구분
        /// </summary>
        private void SetPalletGubun()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "WIP_QLTY_TYPE_CODE";
                newRow["CMCODE"] = "G";

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMMONCODE_FO", "INDATA", "OUTDATA", inTable);

                cboQultType.DisplayMemberPath = cboQultType.DisplayMemberPath.ToString();
                cboQultType.SelectedValuePath = cboQultType.SelectedValuePath.ToString();
                cboQultType.ItemsSource = dtResult.Copy().AsDataView();

                if (dtResult != null && dtResult.Rows.Count > 0)
                    cboQultType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 용량등급
        /// </summary>
        private void SetCommCode(C1ComboBox cb, string cmcdType, string cmCode, string attr1 = null)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string));
                inTable.Columns.Add("ATTRIBUTE1", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = cmcdType;
                newRow["CMCODE"] = cmCode;
                newRow["ATTRIBUTE1"] = attr1;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMMONCODE_FO", "INDATA", "OUTDATA", inTable);

                cb.DisplayMemberPath = cb.DisplayMemberPath.ToString();
                cb.SelectedValuePath = cb.SelectedValuePath.ToString();
                cb.ItemsSource = dtResult.Copy().AsDataView();

                if (dtResult != null && dtResult.Rows.Count > 0)
                    cb.SelectedIndex = 0;
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
        private void SetGridCreatePallet()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("QLTY_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_VLTG_CREATE_CELL_FO", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgPallet, dtResult, null);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

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

                Util.GridSetData(dgPallet, dtResult, null);

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

                const string bizRuleName = "BR_PRD_REG_CREATE_PALLET_CM";

                // DATA SET
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("SHIPTO_NOTE", typeof(string));
                inTable.Columns.Add("SHIPTO_ID", typeof(string));
                inTable.Columns.Add("WIPNOTE", typeof(string));
                inTable.Columns.Add("SHIFT", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inTable.Columns.Add("FORM_WRK_TYPE_NAME", typeof(string));

                DataTable inBox = inDataSet.Tables.Add("IN_BOX");
                inBox.Columns.Add("CAPA_GRD_CODE", typeof(string));
                inBox.Columns.Add("VLTG_GRD_CODE", typeof(string));
                inBox.Columns.Add("CELL_QTY", typeof(Decimal));
                inBox.Columns.Add("INBOX_QTY", typeof(Decimal));
                inBox.Columns.Add("INBOX_TYPE_CODE", typeof(string));
                inBox.Columns.Add("WIPNOTE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_eqptID);
                newRow["PROD_LOTID"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "LOTID");
                newRow["USERID"] = LoginInfo.USERID;
                newRow["SHIPTO_NOTE"] = txtShipping.Text;
                newRow["SHIPTO_ID"] = txtShipTo.Tag == null ? null : txtShipTo.Tag.ToString();
                newRow["WIPNOTE"] = txtNote.Text;
                newRow["SHIFT"] = UcFormShift.TextShift.Tag;
                newRow["WRK_USERID"] = UcFormShift.TextWorker.Tag;
                newRow["WRK_USER_NAME"] = UcFormShift.TextWorker.Text;
                newRow["FORM_WRK_TYPE_NAME"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "FORM_WRK_TYPE_NAME");
                inTable.Rows.Add(newRow);

                foreach (DataGridRow dRow in dgPallet.Rows)
                {
                    //if (Util.NVC_Int(DataTableConverter.GetValue(dRow.DataItem, "CELL_QTY")) != 0 ||
                    //    Util.NVC_Int(DataTableConverter.GetValue(dRow.DataItem, "INBOX_QTY")) != 0)

                    if (Util.NVC_Int(DataTableConverter.GetValue(dRow.DataItem, "CELL_QTY")) != 0)
                    {
                        newRow = inBox.NewRow();
                        newRow["CAPA_GRD_CODE"] = cboCapaType.SelectedValue.ToString().Equals("SELECT") ? null : cboCapaType.SelectedValue.ToString();
                        newRow["VLTG_GRD_CODE"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "VLTG_GRD_CODE"));
                        newRow["CELL_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dRow.DataItem, "CELL_QTY"));
                        newRow["INBOX_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dRow.DataItem, "INBOX_QTY"));
                        newRow["INBOX_TYPE_CODE"] = cboInboxType.SelectedValue == null ? null : cboInboxType.SelectedValue.ToString();
                        newRow["WIPNOTE"] = txtNote.Text;

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

                const string bizRuleName = "BR_PRD_REG_MODIFY_PALLET_SPEC";

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
                newRow["SHIPTO_NOTE"] = txtShipping.Text;
                newRow["SHIPTO_ID"] = txtShipTo.Tag == null ? null : txtShipTo.Tag.ToString();
                newRow["SHIFT"] = UcFormShift.TextShift.Tag;
                newRow["WRK_USERID"] = UcFormShift.TextWorker.Tag;
                newRow["WRK_USER_NAME"] = UcFormShift.TextWorker.Text;
                newRow["CAPA_GRD_CODE"] = cboCapaType.SelectedValue.ToString().Equals("SELECT") ? null : cboCapaType.SelectedValue.ToString();
                newRow["VLTG_GRD_CODE"] = DataTableConverter.GetValue(dgPallet.Rows[0].DataItem, "VLTG_GRD_CODE");
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

            //if (cboCapaType.SelectedValue == null || cboCapaType.SelectedValue.GetString().Equals("SELECT"))
            //{
            //    // 등급을 선택해 주세요.
            //    Util.MessageValidation("SFU4022");
            //    return false;
            //}

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

            //if (cboCapaType.SelectedValue == null || cboCapaType.SelectedValue.GetString().Equals("SELECT"))
            //{
            //    // 등급을 선택해 주세요.
            //    Util.MessageValidation("SFU4022");
            //    return false;
            //}

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
