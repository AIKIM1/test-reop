/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 슬리터 공정진척
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.

    미적용 항목
    1. Final Cut 
 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_914 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        DataSet inDataSet = null;
        #region
        private string _ValueWOID = string.Empty;  
        private string _LargeLOTID = string.Empty;

        private string _LOTID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _LOTIDPR = string.Empty;
        private string _CUTID = string.Empty;
        private string _WIPSTAT = string.Empty;
        private string _INPUTQTY = string.Empty;
        private string _OUTPUTQTY = string.Empty;
        private string _CTRLQTY = string.Empty;
        private string _GOODQTY = string.Empty;
        private string _LOSSQTY = string.Empty;
        private string _WORKORDER = string.Empty;
        private string _WORKDATE = string.Empty;
        private string _VERSION = string.Empty;
        private string _PRODID = string.Empty;
        private string _WIPDTTM_ST = string.Empty;
        private string _WIPDTTM_ED = string.Empty;
        private string _REMARK = string.Empty;
        private string _CONFIRMUSER = string.Empty;
        private string _FINALCUT = string.Empty;
        private string _WIPSTAT_NAME = string.Empty;
        private string _LANEQTY = string.Empty;
        private string sEQPTID = string.Empty;
        string cut = string.Empty;
        string ChildLot = string.Empty;
        bool gDefectChangeFlag = false;
        #endregion

        DataTable dtMain = new DataTable();
        Util _Util = new Util();
        CommonCombo _combo = new CommonCombo();
        private UC_WORKORDER winWorkOrder = new UC_WORKORDER();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ELEC001_914()
        {
            InitializeComponent();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            SetWorkOrderWindow();
            InitCombo();
            InitClear();
        }

        #endregion

        #region Initialize
        private void InitCombo()
        {

#if false
            String[] sFilter = { "E2" };
            C1ComboBox[] cboEquipmentChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentChild, sFilter: sFilter);

            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            String[] sFilter2 = { Process.SLITTING };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter2);

#else
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = {  LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);

            String[] sFilter2 = { Process.SLITTING };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter2);
#endif

        }
        private void InitClear()
        {
            _ValueWOID = string.Empty;
            _LargeLOTID = string.Empty;
            _LOTID = string.Empty;
            _EQPTID = string.Empty;
            _LOTIDPR = string.Empty;
            _CUTID = string.Empty;
            _WIPSTAT = string.Empty;
            _INPUTQTY = string.Empty;
            _OUTPUTQTY = string.Empty;
            _CTRLQTY = string.Empty;
            _GOODQTY = string.Empty;
            _LOSSQTY = string.Empty;
            _WORKORDER = string.Empty;
            _WORKDATE = string.Empty;
            _VERSION = string.Empty;
            _PRODID = string.Empty;
            _WIPDTTM_ST = string.Empty;
            _WIPDTTM_ED = string.Empty;
            _REMARK = string.Empty;
            _CONFIRMUSER = string.Empty;
            sEQPTID = string.Empty;
            _FINALCUT = string.Empty;
            _WIPSTAT_NAME = string.Empty;
            _LANEQTY = string.Empty;
            cut = "Y";  // Cut

            chkCutY.IsChecked = true;

            InitTxt();
            InitGrid();
        }
        #endregion

        #region Event
        #region Init
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            //listAuth.Add(btnInReplace);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Workorder
        private void SetWorkOrderWindow()
        {
            if (grdWorkOrder.Children.Count == 0)
            {
                winWorkOrder.FrameOperation = FrameOperation;

                winWorkOrder._UCParent = this;
                grdWorkOrder.Children.Add(winWorkOrder);
            }
        }

        private void GetWorkOrder()
        {
            if (winWorkOrder == null)
                return;

            ClearControls();

            winWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
            winWorkOrder.PROCID = Process.SLITTING;

            winWorkOrder.GetWorkOrder();
        }
        private DataRow GetSelectWorkOrderInfo()
        {
            if (winWorkOrder == null)
                return null;

            winWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
            winWorkOrder.PROCID = Process.SLITTING;

            return winWorkOrder.GetSelectWorkOrderRow();
        }
        public bool ClearControls()
        {
            bool bRet = false;

            try
            {
                Util.gridClear(dgProductLot);
                ClearDetailControls();

                bRet = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                bRet = false;
            }
            return bRet;
        }
        private void ClearDetailControls()
        {
            Util.gridClear(dgLotInfo);
            Util.gridClear(dgDefect);
            Util.gridClear(dgQuality);
            Util.gridClear(dgRemark);
            InitClear();
        }
        #endregion

        #region Transaction
        private void dgProductLot_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                    chk.IsChecked = true;

                                    ClearDetailControls();

                                    if (!SetCheckProdListSameChildSeq(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Row))
                                        return;

                                    GetLotInfo(dgProductLot.Rows[e.Cell.Row.Index].DataItem);

                                    //row 색 바꾸기
                                    dgProductLot.SelectedIndex = e.Cell.Row.Index;
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                         dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                         (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    ClearDetailControls();

                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                    chk.IsChecked = false;

                                    SetCheckProdListSameChildSeq(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Row, true);


                                }

                                break;
                        }

                        if (dgProductLot.CurrentCell != null)
                            dgProductLot.CurrentCell = dgProductLot.GetCell(dgProductLot.CurrentCell.Row.Index, dgProductLot.Columns.Count - 1);
                        else if (dgProductLot.Rows.Count > 0)
                            dgProductLot.CurrentCell = dgProductLot.GetCell(dgProductLot.Rows.Count, dgProductLot.Columns.Count - 1);
                    }
                }
            }));
        }
        private void chkProductLot_Checked(object sender, RoutedEventArgs e)
        {
            //if (dgProductLot.CurrentRow.DataItem == null)
            //{
            //    return;
            //}

            //_Util.SetDataGridUncheck(dgProductLot, "CHK", ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index);
            //// Child Lot 선택
            //if ((sender as CheckBox).IsChecked.HasValue && !(bool)(sender as CheckBox).IsChecked)
            //{
            //    SetCheckProdListSameChildSeq(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row, false);
            //    return;
            //}

            //if (!SetCheckProdListSameChildSeq(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row, true))
            //    return;

            //GetLotInfo(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
        }
        private void chkProductLot_UnChecked(object sender, RoutedEventArgs e)
        {
            InitGrid();
            InitTxt();
        }
        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                //DataTable dt = DataTableConverter.Convert(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.ItemsSource);

                //if (dt != null)
                //{
                //    for (int i = 0; i < dt.Rows.Count; i++)
                //    {
                //        DataRow row = dt.Rows[i];

                //        if (idx == i)
                //            dt.Rows[i]["CHK"] = true;
                //        else
                //            dt.Rows[i]["CHK"] = false;
                //    }
                //    dgProductLot.BeginEdit();
                //    dgProductLot.ItemsSource = DataTableConverter.Convert(dt);
                //    dgProductLot.EndEdit();
                //}

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                //row 색 바꾸기
                dgProductLot.SelectedIndex = idx;

                GetLotInfo(dgProductLot.Rows[idx].DataItem);
            }
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Length < 1)
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Length < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            GetWorkOrder();
            GetProductLot();
        }
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateRun())
            {
                ELEC001_LOTSTART _LotStart = new ELEC001_LOTSTART();
                _LotStart.FrameOperation = FrameOperation;

                if (_LotStart != null)
                {
                    object[] Parameters = new object[5];
                    Parameters[0] = Process.SLITTING;
                    Parameters[1] = Util.NVC(cboEquipment.SelectedValue);
                    Parameters[2] = Util.NVC(cboEquipmentSegment.SelectedValue);
                    Parameters[3] = "";
                    Parameters[4] = "";

                    sEQPTID = Util.NVC(cboEquipment.SelectedValue);  // 작업시작용

                    C1WindowExtension.SetParameters(_LotStart, Parameters);

                    _LotStart.Closed += new EventHandler(LotStart_Closed);
                    _LotStart.ShowModal();
                    _LotStart.CenterOnScreen();
                }
            }
        }
        private void btnEnd_Click(object sender, RoutedEventArgs e)
        {
            //장비완료 UI에서 없음
            ELEC001_014_LOTEND _LotEnd = new ELEC001_014_LOTEND();
            _LotEnd.FrameOperation = FrameOperation;

            if (_LotEnd != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = _PRODID;
                Parameters[1] = _WORKDATE;
                Parameters[2] = _LOTID;
                Parameters[3] = _WIPSTAT;
                Parameters[4] = Util.NVC(cboEquipment.SelectedValue.ToString());

                C1WindowExtension.SetParameters(_LotEnd, Parameters);

                _LotEnd.Closed += new EventHandler(LotEnd_Closed);
                _LotEnd.ShowModal();
                _LotEnd.CenterOnScreen();
            }
        }
        private void LotStart_Closed(object sender, EventArgs e)
        {
            ELEC001_LOTSTART _LotStart = sender as ELEC001_LOTSTART;
            if (_LotStart.DialogResult == MessageBoxResult.OK)
            {
                RunProcess(_LotStart._ReturnLotID);
            }
        }
        private void LotEnd_Closed(object sender, EventArgs e)
        {
            ELEC001_014_LOTEND window = sender as ELEC001_014_LOTEND;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetWorkOrder();
                GetProductLot();
            }
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
#if false
            if (ValidateCancel())
            {
                CancelProcess();
            }

#endif
            CancelProcess();
        }
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
#if false
            if (ValidateConfirm())
            {
                ConfirmProcess();
            }
#endif

            ConfirmProcess();
        }
        private void btnLabelPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintLabel();
        }
        private void btnFinalCut_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckCnt(dgProductLot, "CHK") < 0)
            { return; }

            int _iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            string lotid = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_iRow].DataItem, dgProductLot.Columns["LOTID"].ToString()));
            string lotid_pr = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_iRow].DataItem, dgProductLot.Columns["LOTID_PR"].ToString()));
            string childseq = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_iRow].DataItem, dgProductLot.Columns["CUT_ID"].ToString()));
            string status = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_iRow].DataItem, dgProductLot.Columns["WIPSTAT"].ToString()));

            if (status != "EQPT_END")
            {
                Util.Alert("SFU1864");  //장비완료 상태의 LOT이 아닙니다.
                return;
            }

            try
            {

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID_PR", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["LOTID_PR"] = lotid_pr;
                indata["PROCID"] = Process.SLITTING;
                string _BizRule = "QR_ROLLPRESS_MAXSEQ";
                DataTable _DT = new ClientProxy().ExecuteServiceSync(_BizRule, "INDATA", "RSLTDT", inTable);

                string maxseq = DataTableConverter.GetValue(_DT, "MAXSEQ").ToString();
                if (childseq != maxseq)
                {
                    Util.MessageValidation("SFU1256");  //마지막에 생성된 LOTID를 선택 해 주세요
                    return;
                }

                CMM_FCUT wndFCut = new CMM_FCUT();
                wndFCut.FrameOperation = FrameOperation;

                if (wndFCut != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_iRow].DataItem, dgProductLot.Columns["FINALCUT"].ToString()));
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_iRow].DataItem, dgProductLot.Columns["CHILD_GR_SEQNO"].ToString()));
                    C1WindowExtension.SetParameters(wndFCut, Parameters);

                    wndFCut.Closed += new EventHandler(wndFCut_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => wndFCut.ShowModal()));
                }
                if (wndFCut.FINALCUT == "")
                {
                    return;
                }
                //MUST_BIZ_APPLY
                //Final Cut 상태를 변경 하시겠습니까?
                Util.MessageConfirm("SFU1210", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        DataTable IndataTable = new DataTable();
                        IndataTable.Columns.Add("LOTID", typeof(string));
                        IndataTable.Columns.Add("EQPTID", typeof(string));
                        IndataTable.Columns.Add("PROCID", typeof(string));
                        IndataTable.Columns.Add("FINALCUT", typeof(string));
                        IndataTable.Columns.Add("USERID", typeof(string));

                        DataRow Indata = IndataTable.NewRow();
                        Indata["LOTID"] = lotid;
                        Indata["EQPTID"] = _EQPTID;
                        Indata["PROCID"] = Process.SLITTING;
                        Indata["FINALCUT"] = wndFCut.FINALCUT;
                        Indata["USERID"] = LoginInfo.USERID;

                        IndataTable.Rows.Add(Indata);
                        // MUST_BIZ_APPLY
                        _BizRule = "ECOM_CHANGE_LOTCUTINFO_V01";

                        new ClientProxy().ExecuteService(_BizRule, "INDATA", "RSLTDT", IndataTable, (result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.AlertByBiz(_BizRule, ex.Message, ex.ToString());
                                return;
                            }

                            Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void txtInputqty_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetInputQty();
            }
        }
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {

            CMM001.Popup.CMM_SHIFT wndPopup = new CMM001.Popup.CMM_SHIFT();
            wndPopup.FrameOperation = this.FrameOperation;

            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = cboEquipmentSegment.SelectedValue;
                Parameters[3] = Process.SLITTING;
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }

        }
        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_SHIFT window = sender as CMM001.Popup.CMM_SHIFT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Text = window.SHIFTCODE;
            }
        }
        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            if (Util.NVC(txtShift.Text) == string.Empty)
            {
                // 선택된 작업조가 없습니다.
                Util.MessageValidation("SFU1844");  //작업조를 선택하세요.
                return;
            }

            CMM001.Popup.CMM_SHIFT_USER wndPopup = new CMM001.Popup.CMM_SHIFT_USER();
            wndPopup.FrameOperation = this.FrameOperation;

            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[3] = Process.SLITTING;
                Parameters[4] = Util.NVC(txtShift.Text);
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }
        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_SHIFT_USER wndPopup = sender as CMM001.Popup.CMM_SHIFT_USER;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtOper.Text = wndPopup.USERNAME;
            }
        }
        private void btnVersion_Click(object sender, RoutedEventArgs e)
        {
            txtVersion.Text = "V01";
        }
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnWIPNOTE_Click(object sender, RoutedEventArgs e)
        {
            SetRemark();
        }
        #endregion

        #region 불량/품질/Lot Comment
        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            SetDefect(_LOTID);
        }
        private void btnQualitySave_Click(object sender, RoutedEventArgs e)
        {
            SetQuality(_LOTID);
        }
        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            DefectChange(dgDefect.CurrentCell.Row.Index, dgDefect.CurrentCell.Column.Index);
        }
        private void dgQuality_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            
        }
        private void dgRemark_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Column.Index != 0) return;

            if (dgRemark.Rows.Count == 0) return;
            if (dgRemark.Columns.Count < 2) return;


            string strAll = Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[0].DataItem, dgRemark.Columns[0].Name));

            string strTmp = "";

            for (int i = 1; i < dgRemark.Columns.Count; i++)
            {
                strTmp = Util.NVC(DataTableConverter.GetValue(dgRemark.CurrentRow.DataItem, dgRemark.Columns[i].Name.ToString()));  
                strTmp += "\r\n" + strAll;

                DataTableConverter.SetValue(dgRemark.CurrentRow.DataItem, dgRemark.Columns[i].Name.ToString(), strTmp);
            }

            DataTableConverter.SetValue(dgRemark.Rows[0].DataItem, dgRemark.Columns[0].Name, "");
        }
        #endregion
        private void btnExpandFrame_Checked(object sender, RoutedEventArgs e)
        {
            grAnimation.RowDefinitions[0].Height = new GridLength(0);
            grAnimation.RowDefinitions[2].Height = new GridLength(0);
            LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla.From = new GridLength(1, GridUnitType.Star);
            gla.To = new GridLength(0, GridUnitType.Star); ;
            gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            grAnimation.RowDefinitions[1].BeginAnimation(RowDefinition.HeightProperty, gla);
        }
        private void btnExpandFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            grAnimation.RowDefinitions[0].Height = new GridLength(0);
            grAnimation.RowDefinitions[2].Height = new GridLength(8);
            LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla.From = new GridLength(0);
            gla.To = new GridLength(1);
            gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            grAnimation.RowDefinitions[1].BeginAnimation(RowDefinition.HeightProperty, gla);
        }
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            InitGrid();

            if (sender == null)
                return;

            if ((sender as CheckBox).IsChecked.HasValue && !(bool)(sender as CheckBox).IsChecked)
            {
                InitGrid();
                return;
            }

            int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
            // _Util.SetDataGridUncheck(dgProductLot, "CHK", checkIndex);
            SetCheckProdListSameChildSeq(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row);
            GetLotInfo(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
            //GetLotInfo(Util.NVC(DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem, "LOTID")));
        }
#endregion

#region Mehod
        private bool SetCheckProdListSameChildSeq(C1.WPF.DataGrid.DataGridRow dataitem, bool bUncheckAll = false)
        {
            if (dataitem == null || dataitem.Index < 0 || dataitem.DataItem == null)
                return false;

            string sInputLot = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "LOTID_PR"));
            string sChildSeq = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "CUT_ID"));

            // 모두 Uncheck 처리 및 동일 자랏의 경우는 Check 처리.
            for (int i = 0; i < dgProductLot.Rows.Count; i++)
            {
                if (dataitem.Index != i)
                {
                    if (sInputLot == Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID_PR")) &&
                        sChildSeq == Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CUT_ID")))
                    {
                        if (sInputLot.Equals(""))
                        {
                            if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                            {
                                (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
                            }
                            DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                        }
                        else
                        {
                            if (bUncheckAll)
                            {
                                if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                {
                                    (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
                                }
                                DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                            }
                            else
                            {
                                if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                {
                                    (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = true;
                                }
                                DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", true);
                            }
                        }

                    }
                    else
                    {
                        if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                            dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                            (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                        {
                            (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
                        }
                        DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                    }
                }
            }

            return true;
        }
#region Lot Info
        private void GetProductLot(DataRow drWDInfo)
        {
            try
            {
                if (drWDInfo == null)
                    return;

                Util.gridClear(dgProductLot);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("WOID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["WOID"] = _ValueWOID;
                Indata["PROCID"] = Process.COATING;
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_SL", "INDATA", "RSLTDT", IndataTable);

                dgProductLot.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void GetProductLot(bool bSelPrv = true)
        {
            try
            {
                string sPrvLot = string.Empty;
                if (dgProductLot.ItemsSource != null && dgProductLot.Rows.Count > 0)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    if (idx >= 0)
                        sPrvLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                }

                string ValueToCondition = string.Empty;
                var sCond = new List<string>();
                
                if ((bool)chkRun.IsChecked)
                {
                    sCond.Add(chkRun.Tag.ToString());
                }

                if ((bool)chkEqpEnd.IsChecked)
                {
                    sCond.Add(chkEqpEnd.Tag.ToString());
                }
                ValueToCondition = string.Join(",", sCond);

                if (ValueToCondition.Trim().Equals(""))
                {
                    Util.AlertInfo("SFU1438");  //WIP 상태를 선택하세요.
                    return;
                }

                Util.gridClear(dgProductLot);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["PROCID"] = Process.SLITTING;
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                Indata["WIPSTAT"] = ValueToCondition;
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_SL", "INDATA", "RSLTDT", IndataTable);

                dgProductLot.ItemsSource = DataTableConverter.Convert(dtResult);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (!sPrvLot.Equals("") && bSelPrv)
                    {
                        int idx = _Util.GetDataGridRowIndex(dgProductLot, "LOTID", sPrvLot);

                        if (idx >= 0)
                        {
                            dtResult.Rows[idx]["CHK"] = true;

                            dgProductLot.BeginEdit();
                            dgProductLot.ItemsSource = DataTableConverter.Convert(dtResult);
                            dgProductLot.EndEdit();

                            DataTableConverter.SetValue(dgProductLot.Rows[idx].DataItem, "CHK", true);

                            dgProductLot.SelectedIndex = idx;

                            dgProductLot.CurrentCell = dgProductLot.GetCell(idx, dgProductLot.Columns.Count - 1);
                        }
                    }
                    else
                    {
                        if (dgProductLot.Rows.Count > 0)
                        {
                            // Checked Event 사용 불가로 인해 CurrentCellChanged 사용함으로 발생하는 동일 Cell Cheked  후 Unchecked 시 Event 안타는 문제로 처리..
                            dgProductLot.CurrentCell = dgProductLot.GetCell(0, dgProductLot.Columns.Count - 1);
                        }
                    }
                }
                }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void GetLotInfo(Object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
            {
                return;
            }

            _LOTID = rowview["LOTID"].ToString();
            _INPUTQTY = rowview["WIPQTY"].ToString();
            _WIPSTAT = rowview["WIPSTAT"].ToString();
            _WIPDTTM_ST = rowview["WIPDTTM_ST"].ToString();
            _WIPDTTM_ED = rowview["WIPDTTM_ED"].ToString();
            _REMARK = rowview["REMARK"].ToString();
            _PRODID = rowview["PRODID"].ToString();
            _VERSION = rowview["MIXER"].ToString();
            _EQPTID = rowview["EQPTID"].ToString();
            _WIPSTAT_NAME = rowview["WIPSNAME"].ToString();
            _LOTIDPR = rowview["LOTID_PR"].ToString();
            _CUTID = rowview["CUT_ID"].ToString();

            if (_WIPSTAT != "WAIT")
            {
                txtVersion.Text = _VERSION;
                txtStartTime.Text = _WIPDTTM_ST;
                dtpEndDateTime.DateTime = Util.StringToDateTime(_WIPDTTM_ED);

                GetDefect(rowview);
                GetQuality(rowview);
                // 투입LOT 적용
                _LOTID = _WIPSTAT == "EQPT_END" ? _LOTIDPR : _LOTID;

                GetLotDetail(_LOTID, _WIPSTAT);
            }
        }
#endregion

#region LOT Start / Cancel / End
        private bool ValidateRun()
        {
            return true;
        }
        private void RunProcess(string ValueToLotID)
        {
            try
            {
                //저장하시겠습니까?
                Util.MessageConfirm("SFU1241", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        DataSet inDataSet = null;

                        inDataSet = new DataSet();

#region Lot Info
                        DataTable inLotDataTable = inDataSet.Tables.Add("IN_EQP");
                        inLotDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inLotDataTable.Columns.Add("IFMODE", typeof(string));
                        inLotDataTable.Columns.Add("EQPTID", typeof(string));
                        inLotDataTable.Columns.Add("LANE_QTY", typeof(Int32));
                        inLotDataTable.Columns.Add("USERID", typeof(string));                        

                        DataRow inLotDataRow = null;
                        inLotDataRow = inLotDataTable.NewRow();
                        inLotDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        inLotDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        inLotDataRow["EQPTID"] = sEQPTID;
                        inLotDataRow["LANE_QTY"] = 3;    
                        inLotDataRow["USERID"] = LoginInfo.USERID;
                        inLotDataTable.Rows.Add(inLotDataRow);
#endregion

#region Material
                        DataTable InMtrldataTable = inDataSet.Tables.Add("IN_INPUT");
                        InMtrldataTable.Columns.Add("INPUT_LOTID", typeof(string));
                        InMtrldataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        InMtrldataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));

                        DataRow inMtrlDataRow = null;

                        inMtrlDataRow = InMtrldataTable.NewRow();
                        inMtrlDataRow["INPUT_LOTID"] = ValueToLotID;
                        inMtrlDataRow["EQPT_MOUNT_PSTN_ID"] = "MTRL_MOUNT_PSTN01";
                        inMtrlDataRow["EQPT_MOUNT_PSTN_STATE"] = "A";

                        InMtrldataTable.Rows.Add(inMtrlDataRow);
#endregion

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_LOT_SL", "IN_EQP,IN_INPUT", "RSLTDT", (result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.AlertByBiz("BR_PRD_REG_START_LOT_SL", ex.Message, ex.ToString());
                                return;
                            }
                            Util.AlertInfo("SFU1275"); //정상처리되었습니다.

                            ClearDetailControls();
                            GetWorkOrder();
                            GetProductLot();

                        }, inDataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private bool ValidateCancel()
        {
            return true;
        }
        private void CancelProcess()
        {
            try
            {
                //취소 하시겠습니까?
                Util.MessageConfirm("SFU1243", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        DataTable IndataTable = new DataTable("INDATA");
                        IndataTable.Columns.Add("SRCTYPE", typeof(string));
                        IndataTable.Columns.Add("IFMODE", typeof(string));
                        IndataTable.Columns.Add("EQPTID", typeof(string));
                        IndataTable.Columns.Add("LOTID", typeof(string));
                        IndataTable.Columns.Add("USERID", typeof(string));

                        DataRow Indata = IndataTable.NewRow();
                        Indata["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        Indata["IFMODE"] = IFMODE.IFMODE_OFF;
                        Indata["EQPTID"] = _EQPTID;
                        Indata["LOTID"] = _LOTID;
                        Indata["USERID"] = LoginInfo.USERID;
                        IndataTable.Rows.Add(Indata);

                        for (int i = 0; i < dgLotInfo.Rows.Count; i++)
                        {
                            Indata = null;
                            Indata = IndataTable.NewRow();

                            Indata["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            Indata["IFMODE"] = IFMODE.IFMODE_OFF;
                            Indata["EQPTID"] = _EQPTID;
                            Indata["LOTID"] = DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID").ToString();
                            Indata["USERID"] = LoginInfo.USERID;
                            IndataTable.Rows.Add(Indata);
                        }

                        new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_START_LOT_SL", "INDATA", null, IndataTable, (result, ex) =>
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;

                            if (ex != null)
                            {
                                Util.AlertByBiz("BR_PRD_REG_CANCEL_START_LOT_SL", ex.Message, ex.ToString());
                                return;
                            }
                            Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                            GetWorkOrder();
                            GetProductLot();
                        });
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        private bool ValidateEqpEnd()
        {
            return true;
        }
       
#endregion

#region LOT Confirm
        private bool ValidateConfirm()
        {
            if (dgLotInfo.Rows.Count == 0)
            {
                Util.MessageValidation("SFU2047");  //확정할 Lot이 없습니다.
                return false;
            }
            int chkfirstidx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

            string parentLot = DataTableConverter.GetValue(dgProductLot.Rows[chkfirstidx].DataItem, "LOTID_PR").ToString();
            int iCut;
            if (!int.TryParse(DataTableConverter.GetValue(dgProductLot.Rows[chkfirstidx].DataItem, "CUT_ID").ToString(), out iCut))
            {
                Util.MessageValidation("SFU1330");  //CUT이 숫자가 아닙니다.
                return false;
            }
            if (iCut != 1)
            {
                for (int i = 0; i < dgProductLot.Rows.Count; i++)
                {
                    if (DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID_PR").ToString() == parentLot)
                    {
                        if (Int32.Parse(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CUT_ID").ToString()) < iCut)
                        {
                            Util.MessageValidation("SFU1785");  //이전 CUT이 실적확정 되지 않았습니다.
                            return false;
                        }
                    }
                }
            }
            if (txtVersion.Text.Trim() == "")
            {
                Util.MessageValidation("SFU1218");  //Version 정보를 입력 하세요.
                return false;
            }
            if (txtShift.Text.Trim() == "")
            {
                Util.MessageValidation("SFU1845");  //작업조를 입력해 주세요.
                return false;
            }
            return true;
        }
        private void ConfirmProcess()
        {
            try
            {
                //저장하시겠습니까?
                Util.MessageConfirm("SFU1241", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        DataSet inDataSet = null;
                        inDataSet = new DataSet();

#region Lot Info
                        DataTable inLotDataTable = inDataSet.Tables.Add("INDATA");
                        inLotDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inLotDataTable.Columns.Add("IFMODE", typeof(string));
                        inLotDataTable.Columns.Add("EQPTID", typeof(string));
                        inLotDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        inLotDataTable.Columns.Add("SHIFT", typeof(string));
                        inLotDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                        inLotDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
                        inLotDataTable.Columns.Add("USERID", typeof(string));

                        DataRow inLotDataRow = null;
                        inLotDataRow = inLotDataTable.NewRow();
                        inLotDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        inLotDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        inLotDataRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                        inLotDataRow["PROD_VER_CODE"] = txtVersion.Text;
                        inLotDataRow["SHIFT"] = "A"; // txtShift.Text;
                        inLotDataRow["WIPDTTM_ED"] = Util.StringToDateTime(dtpEndDateTime.DateTime.Value.ToString("yyyy-MM-dd HH:mm"));
                        inLotDataRow["WRK_USER_NAME"] = LoginInfo.USERID;
                        inLotDataRow["USERID"] = LoginInfo.USERID;
                        inLotDataTable.Rows.Add(inLotDataRow);
#endregion

#region Lot Detail
                        DataTable inLotDetail = inDataSet.Tables.Add("INLOT");
                        inLotDetail.Columns.Add("LOTID", typeof(string));
                        inLotDetail.Columns.Add("INPUTQTY", typeof(decimal));
                        inLotDetail.Columns.Add("OUTPUTQTY", typeof(decimal));
                        inLotDetail.Columns.Add("RESNQTY", typeof(decimal));

                        decimal inputqty = 0;
                        decimal goodqty = 0;
                        decimal lossqty = 0;

                        //string finalCut = "";  
                        //if (_FINALCUT == "F" || _FINALCUT == "Y")
                        //    finalCut = "Y";
                        //else
                        //    finalCut = "N";

                        for (int i = 0; i < dgLotInfo.Rows.Count; i++)
                        {
                            inputqty = decimal.Parse(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "INPUTQTY").ToString());
                            goodqty = decimal.Parse(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "GOODQTY").ToString());
                            lossqty = decimal.Parse(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOSSQTY").ToString());

                            DataRow inLotDetailDataRow = null;
                            inLotDetailDataRow = inLotDetail.NewRow();
                            inLotDetailDataRow["LOTID"] = DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID").ToString();
                            inLotDetailDataRow["INPUTQTY"] = inputqty;
                            inLotDetailDataRow["OUTPUTQTY"] = goodqty;
                            inLotDetailDataRow["RESNQTY"] = lossqty;
                            inLotDetail.Rows.Add(inLotDetailDataRow);
                        }
#endregion

#region Input Lot
                        DataTable inPLotDataTable = inDataSet.Tables.Add("IN_PRLOT");
                        inPLotDataTable.Columns.Add("LOTID", typeof(string));
                        inPLotDataTable.Columns.Add("OUTQTY", typeof(string));
                        inPLotDataTable.Columns.Add("CUTYN", typeof(string));

                        DataRow inPLotDataRow = null;
                        inPLotDataRow = inPLotDataTable.NewRow();
                        inPLotDataRow["LOTID"] = _LOTID;
                        inPLotDataRow["OUTQTY"] = txtRemainqty.Text;
                        inPLotDataRow["CUTYN"] = cut;
                        inPLotDataTable.Rows.Add(inPLotDataRow);
#endregion
                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_SL", "INDATA,INLOT,IN_PRLOT", null, (result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.AlertByBiz("BR_PRD_REG_END_LOT_SL", ex.Message, ex.ToString());
                                //ClearControls();
                                //GetWorkOrder();
                                //GetProductLot();
                                return;
                            }
                            Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                            ClearControls();
                            GetWorkOrder();
                            GetProductLot();
                        }, inDataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void ConfirmLotList(string msg)
        {
            ELEC001_CONFIRM _LotConfirm = new ELEC001_CONFIRM();
            _LotConfirm.FrameOperation = FrameOperation;

            if (_LotConfirm != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = _LOTID;
                Parameters[1] = msg;
                C1WindowExtension.SetParameters(_LotConfirm, Parameters);

                _LotConfirm.Closed += new EventHandler(LotConfirm_Closed);
                _LotConfirm.ShowModal();
                _LotConfirm.CenterOnScreen();

            }
        }
        private void LotConfirm_Closed(object sender, EventArgs e)
        {
            ELEC001_CONFIRM window = sender as ELEC001_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                
            }
        }
#endregion

#region 불량/품질/Lot Comment
        private void GetDefect(Object SelectedItem)
        {
            try
            {
                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                {
                    return;
                }

                Util.gridClear(dgDefect);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROCID"] = Process.SLITTING;
                IndataTable.Rows.Add(Indata);

                //DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPREASONCOLLECT_ELEC", "INDATA", "RSLTDT", IndataTable);

                //dgDefect.ItemsSource = DataTableConverter.Convert(dt);

                new ClientProxy().ExecuteService("DA_PRD_SEL_DEFECT", "INDATA", "OUTDATA", IndataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                            return;
                        }

                        dgDefect.ItemsSource = DataTableConverter.Convert(searchResult);

                        // Defect Column 생성..
                        if (dgDefect.Rows.Count > 0)
                        {
                            // 기존 추가된 Col 삭제..                
                            for (int i = dgDefect.Columns.Count; i-- > 0;)
                            {
                                //if (dgDefect.Columns[i].Name.ToString().StartsWith("DEFECTQTY"))
                                //    dgDefect.Columns.RemoveAt(i);

                                if ( i >= 4)
                                    dgDefect.Columns.RemoveAt(i);
                            }

                            for (int i = 0; i < dgLotInfo.Rows.Count; i++)
                            {
                                //string sColName = "DEFECTQTY" + (i + 1).ToString();
                                
                                ChildLot = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID"));

                                if (i == 0) Util.SetGridColumnNumeric(dgDefect, "ALL", null, "ALL", false, false, false, false, -1, HorizontalAlignment.Right, Visibility.Visible);

                                Util.SetGridColumnNumeric(dgDefect, ChildLot + "CNT", null, "태그수", false, false, false, false, -1, HorizontalAlignment.Right, Visibility.Visible);
                                Util.SetGridColumnNumeric(dgDefect, ChildLot, null, ChildLot, true, true, true, false, -1, HorizontalAlignment.Right, Visibility.Visible);

                                //========================================================================================================================
                                //DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                                //DataGridAggregateSum dagsum = new DataGridAggregateSum();
                                //dagsum.ResultTemplate = dgDefect.Resources["ResultTemplate"] as DataTemplate;
                                //dac.Add(dagsum);
                                //DataGridAggregate.SetAggregateFunctions(dgDefect.Columns[sColName], dac);
                                //========================================================================================================================

                                if (dgDefect.Rows.Count == 0) continue;

                                DataTable dt = GetDefectDataByLot(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID")));
                                if (dt != null)
                                {
                                    for (int j = 0; j < dt.Rows.Count; j++)
                                    {
                                        if (dt.Rows[j]["RESNQTY"].ToString().Equals(""))
                                        {
                                            BindingDataGrid(dgDefect, j, dgDefect.Columns.Count, 0);
                                        }
                                        else
                                        {
                                            BindingDataGrid(dgDefect, j, dgDefect.Columns.Count, dt.Rows[j]["RESNQTY"]);
                                        }
                                    }
                                }
                                //lotinfo gird다시 셋팅
                                double lossqty = SumDefectQty(i + 1, ChildLot);
                                DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "LOSSQTY", lossqty);//lotinfo에 loss설정
                                SetLotInfoCalc();//lotinfo에 goodqty설정
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }

                );

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private double SumDefectQty(int index, string lotid)
        {
            double sum = 0;

            for (int i = 0; i < dgDefect.GetRowCount(); i++)
            {
                //if (!Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "DEFECTQTY" + (index).ToString())).Equals(""))
                //{
                //    sum += double.Parse(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "DEFECTQTY" + (index).ToString()).ToString());
                //}
                if (!Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, lotid)).Equals(""))
                {
                    sum += double.Parse(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, lotid).ToString());
                }
            }
            return sum;
        }
        private void SetLotInfoCalc()
        {
            if (dgLotInfo.ItemsSource == null)
                return;

            double inputQty = 0;
            double goodQty = 0;
            double lossQty = 0;

            for (int i = 0; i < dgLotInfo.Rows.Count; i++)
            {
                inputQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "INPUTQTY")).Equals("") ? "0" : Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "INPUTQTY")));
                lossQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOSSQTY")).Equals("") ? "0" : Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOSSQTY")));
                goodQty = inputQty - lossQty;

                DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "GOODQTY", goodQty);
            }
        }
        private void BindingDataGrid(C1.WPF.DataGrid.C1DataGrid datagrid, int iRow, int iCol, object sValue)
        {
            if (datagrid.ItemsSource == null)
                return;
            else
            {

                DataTable dt = DataTableConverter.Convert(datagrid.ItemsSource);
                if (dt.Columns.Count < datagrid.Columns.Count)
                {
                    for (int i = dt.Columns.Count; i < datagrid.Columns.Count; i++)
                    {
                        dt.Columns.Add(datagrid.Columns[i].Name);
                    }
                }
                if (sValue.Equals("") || sValue.Equals(null))
                {
                    sValue = 0;
                }
                dt.Rows[iRow][iCol - 1] = sValue;

                datagrid.BeginEdit();
                datagrid.ItemsSource = DataTableConverter.Convert(dt);
                datagrid.EndEdit();
            }
        }
        private DataTable GetDefectDataByLot(string LotId)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("RESNPOSITION", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["LOTID"] = LotId;
                Indata["RESNPOSITION"] = null;
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPREASONCOLLECT_ELEC", "INDATA", "RSLTDT", IndataTable);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        private DataTable GetQualityDataByLot(string LotId)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["LOTID"] = LotId;
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPDATACOLLECT_ELEC", "INDATA", "RSLTDT", IndataTable);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        private void GetQuality(Object SelectedItem)
        {
            try
            {
                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                {
                    return;
                }
                Util.gridClear(dgQuality);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROCID"] = Process.SLITTING;
                IndataTable.Rows.Add(Indata);

                //dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_QUALITY", "INDATA", "RSLTDT", IndataTable);

                //dgQuality.ItemsSource = DataTableConverter.Convert(dtMain);

                new ClientProxy().ExecuteService("DA_PRD_SEL_ELEC_QUALITY", "INDATA", "OUTDATA", IndataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                            return;
                        }

                        dgQuality.ItemsSource = DataTableConverter.Convert(searchResult);

                        if (dgQuality.Rows.Count > 0)
                        {
                            // 기존 추가된 Col 삭제..                
                            for (int i = dgQuality.Columns.Count; i-- > 0;)
                            {
                                if (i >= 5)
                                    dgQuality.Columns.RemoveAt(i);
                            }

                            for (int i = 0; i < dgLotInfo.Rows.Count; i++)
                            {
                                ChildLot = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID"));

                                Util.SetGridColumnNumeric(dgQuality, ChildLot, null, ChildLot, true, true, true, false, -1, HorizontalAlignment.Right, Visibility.Visible);

                                if (dgQuality.Rows.Count == 0) continue;

                                DataTable dt = GetQualityDataByLot(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID")));
                                if (dt != null)
                                {
                                    for (int j = 0; j < dt.Rows.Count; j++)
                                    {
                                        if (dt.Rows[j]["CLCTVAL01"].ToString().Equals(""))
                                        {
                                            BindingDataGrid(dgQuality, j, dgQuality.Columns.Count, 0);
                                        }
                                        else
                                        {
                                            BindingDataGrid(dgQuality, j, dgQuality.Columns.Count, dt.Rows[j]["CLCTVAL01"]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
               );

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetDefect(string LotID)
        {
            if (dgDefect.Rows.Count < 0)
            {
                return;
            }

#region Lot Info
            inDataSet = new DataSet();

            DataTable inDataTable = inDataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow inDataRow = null;

            inDataRow = inDataTable.NewRow();
            inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
            inDataRow["EQPTID"] = _EQPTID;
            inDataRow["USERID"] = LoginInfo.USERID;
            inDataTable.Rows.Add(inDataRow);
#endregion

#region Lot Defect

            DataTable inDefectLot = _DefectLot();

#region 기존소스
            //string resnCode = "";
            //string resnQty = "";
            //string resnType = "";
            //string tagCnt = "";


            //DataRow inData = null;
            //DataTable dt = ((DataView)dgDefect.ItemsSource).Table;

            //for (int iCol = 5; iCol < dgDefect.Columns.Count; iCol += 2)
            //{
            //    string sublotid = dgDefect.Columns[iCol + 1].Name;


            //    ////int rowidx = 0; // GridUtil.SearchColumn(spdLotInfo, "LOTID", sublotid);
            //    ////string prodid = ""; // GridUtil.GetValue(spdLotInfo, rowidx, "PRODID").ToString();

            //    //for (int i = 0; i < dgDefect.GetRowCount(); i++)
            //    //{
            //    //    resnCode =  dt.Rows[i]["RESNCODE"].ToString();

            //    //    tagCnt = DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, dgDefect.Columns[iCol].Name).ToString();

            //    //    resnQty = DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, dgDefect.Columns[sublotid].Name).ToString();

            //    //    resnQty = resnQty == "" ? "0" : resnQty;

            //    //    resnType = dt.Rows[i]["RESNTYPE"].ToString();

            //    //    if (resnQty != "0" || (tagCnt != "0" && tagCnt != ""))
            //    //    {
            //    //        inData = inDataTable.NewRow();

            //    //        inData["LOTID"] = _LOTID;
            //    //        inData["PROCID"] = Process.SLITTING;
            //    //        inData["RESNCODE"] = resnCode;
            //    //        inData["RESNQTY"] = resnQty;
            //    //        inData["ATYPEQTY"] = tagCnt;
            //    //        inDataTable.Rows.Add(inData);
            //    //    }
            //    //}
            //}
#endregion

#endregion

            new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT", "INDATA,INRESN", null, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.AlertByBiz("BR_QCA_REG_WIPREASONCOLLECT", ex.Message, ex.ToString());
                    return;
                }
                Util.AlertInfo("SFU1582");  //불량정보 저장 되었습니다.
                dgDefect.EndEdit(true);
                GetDefect(_LOTID);
                gDefectChangeFlag = false;

            }, inDataSet);
        }
        private DataTable _DefectLot()
        {
            DataTable IndataTable = inDataSet.Tables.Add("INRESN");
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("ACTID", typeof(string));
            IndataTable.Columns.Add("RESNCODE", typeof(string));
            IndataTable.Columns.Add("RESNQTY", typeof(decimal));
            IndataTable.Columns.Add("DFCT_TAG_QTY", typeof(Int32));

            DataTable dt = ((DataView)dgDefect.ItemsSource).Table;
            DataRow inData = null;
            for (int iCol = 5; iCol < dgDefect.Columns.Count; iCol += 2)
            {
                string sublotid = dgDefect.Columns[iCol + 1].Name;

                foreach (DataRow _iRow in dt.Rows)
                {
                    inData = IndataTable.NewRow();

                    inData["LOTID"] = sublotid;
                    inData["ACTID"] = _iRow["ACTID"];
                    inData["RESNCODE"] = _iRow["RESNCODE"];
                    inData["RESNQTY"] = _iRow[sublotid].ToString().Equals("") ? 0 : _iRow[sublotid];
                    inData["DFCT_TAG_QTY"] = _iRow[sublotid+"CNT"].ToString().Equals("") ? 0 : _iRow[sublotid + "CNT"];
                    IndataTable.Rows.Add(inData);
                }
            }
            return IndataTable;
        }
        private void SetQuality(string LotID)
        {
            if (dgQuality.Rows.Count < 0)
            {
                return;
            }

            DataTable inEDCLot = _EDCLot();
            new ClientProxy().ExecuteService("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, inEDCLot, (result, ex) =>
            {
                if (ex != null)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1737" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    Util.AlertByBiz("BR_QCA_REG_WIP_DATA_CLCT", ex.Message, ex.ToString());
                    return;
                }

                Util.AlertInfo("SFU1998");  //품질정보 저장 되었습니다.

            });

        }
        private string _WIPSEQ(string sLotID, string sCLCTITEM)
        {
            string wipseq = string.Empty;
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));
            IndataTable.Columns.Add("CLCTITEM", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LOTID"] = sLotID;
            Indata["PROCID"] = Process.SLITTING;
            Indata["CLCTITEM"] = sCLCTITEM;
            IndataTable.Rows.Add(Indata);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPDATACOLLECT_WIPSEQ_SL", "INDATA", "RSLTDT", IndataTable);
            if (dtResult.Rows.Count == 0)
            {
                return "";
            }
            else
            {
                return dtResult.Rows[0]["WIPSEQ"].ToString();
            }            
        }
        private DataTable _EDCLot()
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("SRCTYPE", typeof(string));
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));
            IndataTable.Columns.Add("CLCTITEM", typeof(string));
            IndataTable.Columns.Add("CLCTVAL01", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(string));
            IndataTable.Columns.Add("CLCTSEQ", typeof(string));

            DataTable dt = ((DataView)dgQuality.ItemsSource).Table;
            DataRow inData = null;
            for (int iCol = 5; iCol < dgQuality.Columns.Count; iCol++)
            {
                string sublotid = dgQuality.Columns[iCol].Name;

                foreach (DataRow _iRow in dt.Rows)
                {
                    inData = IndataTable.NewRow();

                    inData["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    inData["LOTID"] = sublotid;
                    inData["EQPTID"] = _EQPTID;
                    inData["USERID"] = LoginInfo.USERID;
                    inData["CLCTITEM"] = _iRow["CLCTITEM"];
                    inData["CLCTVAL01"] = _iRow[sublotid].ToString().Equals("") ? 0 : _iRow[sublotid];
                    inData["WIPSEQ"] = _WIPSEQ(sublotid, _iRow["CLCTITEM"].ToString()) == "" ? null : _WIPSEQ(sublotid, _iRow["CLCTITEM"].ToString());
                    inData["CLCTSEQ"] = _iRow["CLCTSEQ"].ToString() == "0" ? null : _iRow["CLCTSEQ"];
                    IndataTable.Rows.Add(inData);

                }
            }
            return IndataTable;
        }
        
        private void DefectChange(int iRow, int iCol)
        {
            gDefectChangeFlag = true;
            if (iCol == 4)
            {
                for (int i = 6; i < dgDefect.Columns.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        string _ValueToFind = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[iRow].DataItem, dgDefect.Columns[iCol].Name));
                        DataTableConverter.SetValue(dgDefect.Rows[iRow].DataItem, dgDefect.Columns[i].Name, _ValueToFind);
                        DefectQty_Process(iRow, i);
                    }
                }
            }
            else if (iCol >= 5 && iCol % 2 != 0)
            {
                if (DataTableConverter.GetValue(dgDefect.Rows[iRow].DataItem, "RESNDESC").ToString() == "2" || DataTableConverter.GetValue(dgDefect.Rows[iRow].DataItem, "RESNDESC").ToString() == "4")
                {
                    double dPitch;
                    if (DataTableConverter.GetValue(dgDefect.Rows[iRow], "RESNDESC").ToString() == "RESN9405") // 원단연결
                    {
                        dPitch = 0.5;
                    }
                    else    // 원단연결 외
                    {
                        dPitch = 0.1;
                    }
                    
                    double dblChgVal = double.Parse(DataTableConverter.GetValue(dgDefect.Rows[iRow], "RESNDESC").ToString()) * dPitch;
                    DefectQty_Process(iRow, iCol + 1);
                }
            }
            else if (iCol >= 6 && iCol % 2 == 0)
            {
                DefectQty_Process(iRow, iCol);
            }
        }
        private void DefectQty_Process(int rowidx, int colidx)
        {
             string sublotid = dgDefect.Columns[colidx].Name.ToString();

            double sum = SumDefectQty(colidx, sublotid);

            int lotinfo_rowidx = Util.gridFindDataRow(ref dgLotInfo, "LOTID", sublotid, false);

            if (_WIPSTAT == "EQPT_END")
            {
                double inputqty = Convert.ToDouble(DataTableConverter.GetValue(dgLotInfo.Rows[lotinfo_rowidx].DataItem, "INPUTQTY").ToString());

                if (inputqty < sum)
                {
                    Util.Alert("SFU1608");  //생산량보다 불량이 크게 입력 될 수 없습니다.
                    DataTableConverter.SetValue(dgDefect.Rows[rowidx].DataItem, sublotid, 0);
                    DataTableConverter.SetValue(dgLotInfo.Rows[rowidx].DataItem, "LOSSQTY", 0);
                }
                else
                {
                    DataTableConverter.SetValue(dgLotInfo.Rows[lotinfo_rowidx].DataItem, "LOSSQTY", sum);
                }

                SetLotinfo_Calc();
            }
            else if (_WIPSTAT == "PROC")
            {
                DataTableConverter.SetValue(dgLotInfo.Rows[lotinfo_rowidx].DataItem, "LOSSQTY", sum);
                SetLotinfo_Calc();
            }


        }
        private void SetRemark()
        {
            try
            {
                if (dgRemark.Rows.Count == 0) return;
                if (dgRemark.Columns.Count < 2) return;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIP_NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow indata;

                DataTable dt = ((DataView)dgRemark.ItemsSource).Table;
                DataRow inData = null;
                for (int iCol = 1; iCol < dgRemark.Columns.Count; iCol++)
                {
                    string sublotid = dgRemark.Columns[iCol].Name;

                    foreach (DataRow _iRow in dt.Rows)
                    {
                        inData = inTable.NewRow();

                        inData["LOTID"] = sublotid;
                        inData["WIP_NOTE"] = _iRow[sublotid].ToString().Equals("") ? 0 : _iRow[sublotid];
                        inData["USERID"] = LoginInfo.USERID;
                        inTable.Rows.Add(inData);
                    }
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_WIPHISTORY_NOTE", "INDATA", null, inTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.AlertByBiz("BR_PRD_REG_WIPHISTORY_NOTE", ex.Message, ex.ToString());
                        return;
                    }

                    Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

#endregion

#region Function
        private void GetLotDetail(string lotid, string status)
        {
            DataTable dt = SearchLotInfo(lotid, status);

            #region 불량 처리 확인(태그수, 특정불량 코드 처리)
#if false
            for (int i = dgDefect.Columns.Count - 1; i >= 4; i--)
            {
                if (i >= 4)
                    dgDefect.Columns.RemoveAt(i);
            }

            string childLot = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                childLot = DataTableConverter.GetValue(dt.Rows[i], "LOTID").ToString();//  MLBUtil.getVal(ds, "RSLTDT", i, "LOTID").ToString();

                if (i == 0) Util.SetGridColumnNumeric(dgDefect, "ALL", null, "ALL", false, false, false, false, 80, HorizontalAlignment.Right, Visibility.Visible);

                Util.SetGridColumnNumeric(dgDefect, childLot + "CNT", null, "태그수", false, false, false, false, 80, HorizontalAlignment.Right, Visibility.Visible);
                Util.SetGridColumnNumeric(dgDefect, childLot, null, childLot, false, false, false, false, 80, HorizontalAlignment.Right, Visibility.Visible);

                if (dgDefect.Rows.Count == 0) continue;
                //Lot별 불량수량
                DataTable dsDefect = SearchDefectDataLot(childLot);
                int colidx = dgDefect.Columns.Count - 1;

                for (int j = 0; j < dsDefect.Rows.Count; j++)
                {
                    if (DataTableConverter.GetValue(dsDefect.Rows[j], "RESNQTY").ToString() != "0")
                        DataTableConverter.SetValue(dsDefect.Rows[colidx], "RESNQTY", DataTableConverter.GetValue(dsDefect.Rows[j], "RESNQTY"));
                    if (DataTableConverter.GetValue(dsDefect.Rows[j], "ATYPEQTY").ToString() != "0")
                        DataTableConverter.SetValue(dsDefect.Rows[colidx - 1], "ATYPEQTY", DataTableConverter.GetValue(dsDefect.Rows[j], "ATYPEQTY"));
                }

                DataTableConverter.SetValue(dgLotInfo.Rows[i], "LOSSQTY", SumDefectQty(colidx));
            }
            // Defect
            for (int i = 0; i < dgDefect.Rows.Count; i++)
            {
                string resndesc = DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNDESC").ToString();
                if (resndesc == "1")
                {
                    // 태그수 및 M(LOT별 수량) 별도입력
                }
                else if (resndesc == "2")
                {
                    //spdDefect.ActiveSheet.Cells[i, 3].Locked = true;
                    //spdDefect.ActiveSheet.Cells[i, 3].BackColor = colorLock;

                }
                else if (resndesc == "3")
                {
                    // 태그수 입력 안함
                    for (int j = 5; j < dgDefect.Columns.Count; j++)
                    {
                        if (j % 2 != 0)
                        {
                            //spdDefect.ActiveSheet.Cells[i, j].Locked = true;
                            //spdDefect.ActiveSheet.Cells[i, j].BackColor = colorLock;
                        }
                    }
                }
                else if (resndesc == "4")
                {

                }
            }
            //Quality
            for (int i = dgQuality.Columns.Count - 1; i >= 5; i--)
            {
                if (i >= 5)
                    dgQuality.Columns.RemoveAt(i);
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                childLot = DataTableConverter.GetValue(dt.Rows[i], "LOTID").ToString();
               
                Util.SetGridColumnNumeric(dgQuality, childLot, null, childLot, false, false, false, false, 100, HorizontalAlignment.Right, Visibility.Visible);

                if (dgQuality.Rows.Count == 0) continue;

                DataTable dsQuality = SearchQualityDataLot(childLot);
                int colidx = dgQuality.Columns.Count - 1;

                for (int j = 0; j < dsQuality.Rows.Count; j++)
                {
                    DataTableConverter.SetValue(dgQuality.Rows[j], dgQuality.Columns[colidx].Name, DataTableConverter.GetValue(dsQuality.Rows[j], "VALUE"));
                }
            }
#endif
            #endregion

            SetLotinfo_Calc();

            DataTable dtCopy = dt.Copy();
            SetSublotCombo(dtCopy);

#if false
            if (status == "EQPT_END")
            {
                SetParentQty(lotid, status);
                //txtWorkPart.Text = MLBUtil.GetShiftCurrent(lotid, CurrentInfo.shop, CurrentInfo.line, sProcid);
            }
#else
            SetParentQty(lotid, status);
#endif
        }
        private DataTable SearchLotInfo(string lotid, string status)
        {
            DataTable dt = new DataTable();
            try
            {
                Util.gridClear(dgLotInfo);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID_PR", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["LOTID_PR"] = lotid;
                indata["LOTID"] = null;

                // ********************** 확인 ***********************
                //if (status == "EQPT_END")
                //{
                //    indata["LOTID_PR"] = null;
                //    indata["LOTID"] = lotid;
                //}
                //else
                //{
                //    indata["LOTID_PR"] = lotid;
                //    indata["LOTID"] = null;
                //}
                // ********************** 확인 ***********************

                inTable.Rows.Add(indata);

                dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RUNLOT_SL", "INDATA", "RSLTDT", inTable);

                dgLotInfo.ItemsSource = DataTableConverter.Convert(dt);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return dt;
        }
        private DataTable SearchDefectDataLot(string sLot)
        {
            DataTable dt = new DataTable();
                 
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["LOTID"] = sLot;
                inTable.Rows.Add(indata);

                dt = new ClientProxy().ExecuteServiceSync("QR_COM_DEFECT_LIST", "INDATA", "RSLTDT", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return dt;
        }
        private DataTable SearchQualityDataLot(string sLot)
        {
            DataTable dt = new DataTable();

            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["LOTID"] = sLot;
                inTable.Rows.Add(indata);

                dt = new ClientProxy().ExecuteServiceSync("QR_COM_QUALITY_LIST", "INDATA", "RSLTDT", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return dt;
        }
#region Conversion 
        //private decimal SumDefectQty(int colidx)
        //{
        //    decimal sum = 0;

        //    for (int i = 0; i < dgDefect.Rows.Count; i++)
        //    {
        //        sum += Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, dgDefect.Columns[colidx].Name));
        //    }

        //    return sum;
        //}
#endregion


        private void SetParentQty(string lotid, string status)
        {
            try
            {
                //string _Biz = string.Empty;
                //if (status == "EQPT_END")
                //{
                //    _Biz = "QR_COM_LOTID_PR";
                //}
                //else
                //{
                //    _Biz = "QR_COM_LOTID";
                //}
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["LOTID"] = lotid;
                inTable.Rows.Add(indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QTY", "INDATA", "RSLTDT", inTable);

                if (dt.Rows.Count > 0)
                {
                    txtParentqty.Text = string.Format("{0:F2}", Util.NVC_Decimal(dt.Rows[0]["WIPQTY"].ToString())); 
                    //if (_FINALCUT == "F" || _FINALCUT == "Y")
                    //{
                    //    txtInputqty.Value = Convert.ToDouble(txtParentqty.Text);
                    //    SetInputQty();
                    //}
                    SetParentRemainQty();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetInputQty()
        {
            //if (!Util.CheckDecimal(txtInputqty.Value, 0)) return;
            if (dgLotInfo.Rows.Count == 0) return;

            decimal inputQty = Util.NVC_Decimal(txtInputqty.Value); 
            decimal lossQty = 0;

            for (int i = 0; i < dgLotInfo.Rows.Count; i++)
            {
                lossQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOSSQTY").ToString());

                DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "INPUTQTY", inputQty);
                DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "GOODQTY", inputQty - lossQty);
            }

            txtInputqty.Value = 0;

            SetParentRemainQty();
        }
        private void SetParentRemainQty()
        {
            decimal parentqty = 0;
            decimal inputqty = 0;
            parentqty = Util.NVC_Decimal(txtParentqty.Text == "" ? "0" : txtParentqty.Text);
            inputqty = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[0].DataItem, "INPUTQTY").ToString());
            txtRemainqty.Text = string.Format("{0:F2}", (parentqty - inputqty));
        }
        private void SetLotinfo_Calc()
        {
            decimal inputQty = 0;
            decimal goodQty = 0;
            decimal lossQty = 0;
            for (int i = 0; i < dgLotInfo.Rows.Count; i++)
            {
                inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "INPUTQTY").ToString());
                lossQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOSSQTY").ToString());
                goodQty = inputQty - lossQty;

                // 적용필
                //DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "GOODQTY", goodQty);
            }
        }
        private void SetSublotCombo(DataTable dt)
        {
            if (dgRemark.Rows.Count > 0) return;

            DataTable _dtRemark = new DataTable();

            _dtRemark.Columns.Add("REMARK_ALL", typeof(System.String));
            DataRow inDataRow = null;

            inDataRow = _dtRemark.NewRow();

            foreach (DataRow dRow in dt.Rows)
            {
                _dtRemark.Columns.Add(dRow["LOTID"].ToString(), typeof(System.String));
                Util.SetGridColumnText(dgRemark, dRow["LOTID"].ToString(), null, dRow["LOTID"].ToString(), false, false, false, false, 100, HorizontalAlignment.Left, Visibility.Visible);
                inDataRow[dRow["LOTID"].ToString()] = GetRemark(dRow["LOTID"].ToString());
            }
            _dtRemark.Rows.Add(inDataRow);

            dgRemark.ItemsSource = DataTableConverter.Convert(_dtRemark);

        }
        private string GetRemark(string sublotid)
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["LOTID"] = sublotid;
            inTable.Rows.Add(indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPHISTORYATTR_WIPNOTE", "INDATA", "RSLTDT", inTable);
            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0]["WIP_NOTE"].ToString();
            }
            else
            {
                return "";
            }
        }
        private void setEnabled(Object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
            {
                btnStart.IsEnabled = false;
                btnCancel.IsEnabled = false;
                btnEnd.IsEnabled = false;
                btnConfirm.IsEnabled = false;

                txtInputqty.IsEnabled = false;
            }
            else
            {
                if (_WIPSTAT == "WAIT")
                {
                    btnStart.IsEnabled = true;
                    btnCancel.IsEnabled = false;
                    btnEnd.IsEnabled = true;
                    btnConfirm.IsEnabled = false;

                    btnDefectSave.IsEnabled = false;
                    btnQualitySave.IsEnabled = false;

                    txtInputqty.IsEnabled = false;  
                }
                else if (_WIPSTAT == "PROC")
                {
                    btnStart.IsEnabled = false;
                    btnCancel.IsEnabled = true;
                    btnEnd.IsEnabled = true;
                    btnConfirm.IsEnabled = false;

                    btnDefectSave.IsEnabled = true;
                    btnQualitySave.IsEnabled = true;

                    txtInputqty.IsEnabled = false;
                }
                else if (_WIPSTAT == "EQPT_END")
                {
                    btnStart.IsEnabled = false;
                    btnCancel.IsEnabled = false;
                    btnEnd.IsEnabled = false;
                    btnConfirm.IsEnabled = true;

                    btnDefectSave.IsEnabled = true;
                    btnQualitySave.IsEnabled = true;

                    txtInputqty.IsEnabled = true;
                }
            }
        }
        private void InitGrid()
        {

            for (int index = dgDefect.Columns.Count - 1; index > 4; index--)
            {
                dgDefect.Columns.RemoveAt(index);
            }

            for (int index = dgQuality.Columns.Count - 1; index > 5; index--)
            {
                dgQuality.Columns.RemoveAt(index);
            }

            for (int index = dgRemark.Columns.Count - 1; index > 0; index--)
            {
                dgRemark.Columns.RemoveAt(index);
            }
        }
        private void InitTxt()
        {
            txtVersion.Text = string.Empty;
            txtStartTime.Text = string.Empty;
            txtParentqty.Text = string.Empty;
            txtRemainqty.Text = string.Empty;
            txtShift.Text = string.Empty;
            txtOper.Text = string.Empty;

            txtInputqty.Value = 0;
            txtStartTime.Text = string.Empty;
            dtpEndDateTime.DateTime = System.DateTime.Now;
        }
        private void wndFCut_Closed(object sender, EventArgs e)
        {
            CMM_COATERFCUT window = sender as CMM_COATERFCUT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                if (window.FINALCUT.Equals(""))
                    return;
            }
        }
        private void PrintLabel()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["LOTID"] = _LOTID;
                inTable.Rows.Add(indata);

                dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PROCESS_LOT_LABEL_SL", "INDATA", "RSLTDT", inTable);
                if (dtMain.Rows.Count < 1)
                {
                    return;
                }
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(dtMain.Rows[0]["I_ATTVAL"].ToString()), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void chkCutY_Click(object sender, RoutedEventArgs e)
        {
            if (!validationCut())
            {
                return;
            }
        }
        private bool validationCut()
        {
            if (txtRemainqty.Text.ToString().Equals("0"))
            {
                if (chkCutY.IsChecked == true)
                {
                    //Util.Alert("잔량이 없습니다.");
                    chkCutY.IsChecked = false;
                    chkCutN.IsChecked = true;
                    //cut = "N";
                    //return false;
                }
                cut = "N";

            }
            else
            {
                if (chkCutN.IsChecked == true)
                {
                    //Util.Alert("잔량이 남았습니다.");
                    chkCutY.IsChecked = true;
                    chkCutN.IsChecked = false;
                    //cut = "Y";
                    //return false;
                }
                cut = "Y";
            }

            return true;
        }
#endregion

#endregion

#region 테스트 Data
        private void GetEDC(C1.WPF.DataGrid.C1DataGrid _grid)
        {
            DataTable _Slurry = new DataTable();
            _Slurry.Columns.Add("CLCTITEM", typeof(string));
            _Slurry.Columns.Add("CLCTNAME", typeof(string));
            _Slurry.Columns.Add("CLCTUSL", typeof(string));
            _Slurry.Columns.Add("CLCTLSL", typeof(string));
            _Slurry.Columns.Add("CLCTVAL", typeof(string));
            DataRow _dRow = null;
            for (int i = 0; i < 5; i++)
            {
                _dRow = _Slurry.NewRow();
                _dRow["CLCTITEM"] = "CLCTITEM_" + (i + 1).ToString();
                _dRow["CLCTNAME"] = "CLCTNAME_" + (i + 1).ToString();
                _dRow["CLCTUSL"] = 100;
                _dRow["CLCTLSL"] = 20;
                _Slurry.Rows.Add(_dRow);
            }
            // 
            _grid.ItemsSource = DataTableConverter.Convert(_Slurry);
        }



        #endregion

    }
}
