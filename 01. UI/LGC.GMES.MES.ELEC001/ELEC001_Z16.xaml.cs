/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : SRS 믹서 공정진척
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.

 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ELEC001
{
    /// <summary>
    /// ELEC001_016.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ELEC001_Z16 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        DataTable dtMain = new DataTable();
        DataSet inDataSet = null;
        DataSet _EDCDataSet = null;
        private string selectedEquipment = string.Empty;
        private bool gDefectChangeFlag = false;
        #region 
        private string _LOTID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _WIPSTAT = string.Empty;
        private string _WIPSNAME = string.Empty;
        private string _GOODQTY = string.Empty;
        private string _LOSSQTY = string.Empty;
        private string _CHARGQTY = string.Empty;
        private string _INPUTQTY = string.Empty;
        private string _WORKORDER = string.Empty;
        private string _WORKDATE = string.Empty;
        private string _VERSION = string.Empty;
        private string _PRODID = string.Empty;
        private string _WIPDTTM_ST = string.Empty;
        private string _WIPDTTM_ED = string.Empty;
        private string _REMARK = string.Empty;
        private string _CONFIRMUSER = string.Empty;
        private string _ELECTRODE = string.Empty;
        private string _SHIFT = string.Empty;
        private string _WIPSEQ = string.Empty;
        private string _CLCTSEQ = string.Empty;
        #endregion

        Util _Util = new Util();
        CommonCombo _combo = new CommonCombo();

        private UC_WORKORDER winWorkOrder = new UC_WORKORDER();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ELEC001_Z16()
        {
            InitializeComponent();
            InitCombo();
            //setEnabled();
            dtpEndDateTime.DateTime = System.DateTime.Now;
        }
        #endregion

        #region Initialize

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilter);

            selectedEquipment = LoginInfo.CFG_EQPT_ID;
            Set_Combo_Eqpt(cboEquipment);
        }
        /// <summary>
        /// Main Windows Data Clear 처리
        /// UC_WORKORDER 컨트롤에서 Main Window Data Clear
        /// </summary>
        /// <returns>Clear 완료 여부</returns>
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
            Util.gridClear(dgDefect);
            Util.gridClear(dgQuality);
            Util.gridClear(dgMaterial);
            Util.gridClear(dgLotInfo);
            initCurrentLotInfo();
            InitTxt();
            InitGrid();
        }
        private void initCurrentLotInfo()
        {
            _LOTID = string.Empty;
            _WIPSTAT = string.Empty;
            _WIPSNAME = string.Empty;
            _EQPTID = string.Empty;
            _GOODQTY = string.Empty;
            _LOSSQTY = string.Empty;
            _CHARGQTY = string.Empty;
            _INPUTQTY = string.Empty;
            _WORKORDER = string.Empty;
            _WORKDATE = string.Empty;
            _VERSION = string.Empty;
            _PRODID = string.Empty;
            _WIPDTTM_ST = string.Empty;
            _WIPDTTM_ED = string.Empty;
            _REMARK = string.Empty;
            _CONFIRMUSER = string.Empty;
            _ELECTRODE = string.Empty;
            _SHIFT = string.Empty;
            gDefectChangeFlag = false;
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            ApplyPermissions();
            SetWorkOrderWindow();
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
            GetBeadMill();
        }
        private void chkProductLot_Checked(object sender, RoutedEventArgs e)
        {
            if (dgProductLot.CurrentRow.DataItem == null)
            {
                return;
            }

            //int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

            //for (int i = 0; i < dgProductLot.Rows.Count; i++)
            //{
            //    if (rowIndex != i)
            //        if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null
            //            && (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue)
            //            (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;

            //}
            //DataRowView selectedRow = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem as DataRowView;
            //GetLotInfo(selectedRow);

            _Util.SetDataGridUncheck(dgProductLot, "CHK", ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index);
            GetLotInfo(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
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
        private void chkProductLot_UnChecked(object sender, RoutedEventArgs e)
        {
            InitGrid();
            InitTxt();
        }

        #region Button
        private void btnCommit_Click(object sender, RoutedEventArgs e)
        {
            SetDefect(_LOTID, dgDefect);
        }
        private void btnLossSave_Click(object sender, RoutedEventArgs e)
        {
            SetLoss(_LOTID, dgLoss);
        }
        private void btnChargeSave_Click(object sender, RoutedEventArgs e)
        {
            SetCharge(_LOTID, dgCharge);
        }
        private void btnQualitySave_Click(object sender, RoutedEventArgs e)
        {
            SetQuality(_LOTID);
        }
        private void btnInputMaterial_Click(object sender, RoutedEventArgs e)
        {
            SetMaterial(_LOTID, "A");
        }
        private void btnDelMaterial_Click(object sender, RoutedEventArgs e)
        {
            SetMaterial(_LOTID, "D");
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (dgMaterial.ItemsSource == null || dgMaterial.Rows.Count < 0)
            {
                return;
            }
            if (_WIPSTAT.Equals(""))
            {
                Util.MessageValidation("SFU1381");  //LOT을 선택하세요.
                return;
            }
            DataTable dt = ((DataView)dgMaterial.ItemsSource).Table;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (_Util.GetDataGridCheckValue(dgMaterial, "CHK", i))
                {
                    dt.Rows[i]["CHK"] = false;
                }
            }

            DataRow dr = dt.NewRow();
            dr["CHK"] = true;
            dr["INPUT_DTTM"] = string.Format("{0:yyyy-MM-dd hh:mm}", DateTime.Now);
            dt.Rows.Add(dr);
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (dgMaterial.ItemsSource == null || dgMaterial.Rows.Count < 0)
            {
                return;
            }

            DataTable dt = ((DataView)dgMaterial.ItemsSource).Table;

            for (int i = 0; i < dgMaterial.Rows.Count; i++)
            {
                if (_Util.GetDataGridCheckValue(dgMaterial, "CHK", i))
                {
                    dt.Rows[i].Delete();
                }
            }
        }
        private void txtInputQty_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (dgLotInfo.GetRowCount() < 1)
                {
                    return;
                }
                txtGoodQuntity.Text = string.Format("{0:n0}", (Util.NVC_Decimal(txtProcQuntity.Value) - Util.NVC_Decimal(txtLossQuntity.Text)));
            }
        }
        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            dicParam.Add("PROCID", Process.SRS_MIXING);
            dicParam.Add("EQPTID", Util.NVC(cboEquipment.SelectedValue));
            dicParam.Add("ELEC", _ELECTRODE);//극성정보

            ELEC001_006_LOTSTART _LotStart = new ELEC001_006_LOTSTART(dicParam);
            _LotStart.FrameOperation = FrameOperation;

            if (_LotStart != null)
            {
                _LotStart.Closed += new EventHandler(LotStart_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => _LotStart.ShowModal()));
                _LotStart.CenterOnScreen();
            }

            //ELEC001_006_LOTSTART _LotStart = new ELEC001_006_LOTSTART();
            //_LotStart.FrameOperation = FrameOperation;

            //if (_LotStart != null)
            //{
            //    object[] Parameters = new object[3];
            //    Parameters[0] = Process.SRS_MIXING;
            //    Parameters[1] = Util.NVC(cboEquipment.SelectedValue);
            //    Parameters[2] = _ELECTRODE;
            //    C1WindowExtension.SetParameters(_LotStart, Parameters);

            //    _LotStart.Closed += new EventHandler(LotStart_Closed);
            //    _LotStart.ShowModal();
            //    _LotStart.CenterOnScreen();

            //}
        }
        private void btnRunComplete_Click(object sender, RoutedEventArgs e)
        {
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                return;
            }

            ELEC001_006_LOTEND _LotEnd = new ELEC001_006_LOTEND();
            _LotEnd.FrameOperation = FrameOperation;

            if (_LotEnd != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = Process.SRS_MIXING;
                Parameters[1] = _WORKORDER;
                Parameters[2] = _EQPTID;
                Parameters[3] = _LOTID;
                Parameters[4] = LoginInfo.USERID;
                C1WindowExtension.SetParameters(_LotEnd, Parameters);

                _LotEnd.Closed += new EventHandler(LotEnd_Closed);
                _LotEnd.ShowModal();
                _LotEnd.CenterOnScreen();
            }
        }
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                return;
            }

            //품질입력 Check
            //if (!fn_QualityChk())
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("품질 측정값이 잘못 입력되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            //불량저장
            //SetDefect(_LOTID);
            if (!ValidateConfirm())
            {
                return;
            }
            ConfirmProcess();
        }

        private void btnRunCancel_Click(object sender, RoutedEventArgs e)
        {
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                return;
            }

            //취소 하시겠습니까?
            Util.MessageConfirm("SFU1243", (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    DataTable IndataTable = new DataTable();
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

                    new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_START_LOT_SX", "INDATA", null, IndataTable, (result, ex) =>
                    {
                        try
                        {
                            if (ex != null)
                            {
                                Util.AlertByBiz("BR_PRD_REG_CANCEL_START_LOT_SX", ex.Message, ex.ToString());
                                return;
                            }
                            Util.AlertInfo("SFU1275"); //정상처리되었습니다.

                            ClearControls();
                            GetWorkOrder();
                            GetProductLot();
                        }
                        catch (Exception err)
                        {
                            Util.MessageException(err);
                        }

                    });
                }
            });
        }

        private void btnInputBeadMill_Click(object sender, RoutedEventArgs e)
        {
            SetBeadMill(_LOTID);
        }
        #endregion
        private void txtProcQuntity_KeyUp(object sender, KeyEventArgs e)
        {
            if (!Util.isNumber(txtGoodQuntity.Text))
            {
                return;
            }

            if (e.Key == Key.Enter)
            {
                txtGoodQuntity.Text = (Util.NVC_Decimal(txtProcQuntity.Value) - Util.NVC_Decimal(txtLossQuntity.Text)).ToString();
            }
        }
        private void txtProcQuntity_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!Util.isNumber(txtGoodQuntity.Text))
            {
                return;
            }
            txtGoodQuntity.Text = (Util.NVC_Decimal(txtProcQuntity.Value) - Util.NVC_Decimal(txtLossQuntity.Text)).ToString();

        }
        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (txtProcQuntity.Value == 0)
            {
                Util.MessageValidation("SFU1609");  //생산량을 입력하십시오.
                txtProcQuntity.Focus();
                e.Cell.Value = 0;
                return;
            }

            GetDefectSum();
            _LOSSQTY = string.Empty;
            _GOODQTY = string.Empty;
            _CHARGQTY = string.Empty;
        }
        private void dgLoss_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (txtProcQuntity.Value == 0)
            {
                Util.MessageValidation("SFU1609");  //생산량을 입력하십시오.
                txtProcQuntity.Focus();
                e.Cell.Value = 0;
                return;
            }
            GetDefectSum();
            _LOSSQTY = string.Empty;
            _GOODQTY = string.Empty;
            _CHARGQTY = string.Empty;
        }
        private void dgCharge_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (txtProcQuntity.Value == 0)
            {
                Util.MessageValidation("SFU1609");  //생산량을 입력하십시오.
                txtProcQuntity.Focus();
                e.Cell.Value = 0;
                return;
            }
            GetDefectSum();
            _LOSSQTY = string.Empty;
            _GOODQTY = string.Empty;
            _CHARGQTY = string.Empty;
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
                Parameters[3] = Process.SRS_MIXING;
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
                txtShift.Text = window.SHIFTNAME;
                txtShift.Tag = window.SHIFTCODE;
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
                Parameters[3] = Process.SRS_MIXING;
                Parameters[4] = Util.NVC(txtShift.Tag);
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
                txtWorkPerson.Text = wndPopup.USERNAME;
            }
        }
        private void btnVersion_Click(object sender, RoutedEventArgs e)
        {
            CMM001.Popup.CMM_ELECRECIPE wndPopup = new CMM001.Popup.CMM_ELECRECIPE();
            wndPopup.FrameOperation = this.FrameOperation;

            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                wndPopup.Closed += new EventHandler(wndRecipe_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }
        private void wndRecipe_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_ELECRECIPE window = sender as CMM001.Popup.CMM_ELECRECIPE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                //txtVersion.Text = window._ReturnRecipeNo;
            }
        }
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
            grAnimation.RowDefinitions[0].Height = new GridLength(5);
            grAnimation.RowDefinitions[2].Height = new GridLength(5);
            LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla.From = new GridLength(0);
            gla.To = new GridLength(1);
            gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            grAnimation.RowDefinitions[1].BeginAnimation(RowDefinition.HeightProperty, gla);
        }
        #endregion

        #region Mothod
        private void Set_Combo_Eqpt(C1ComboBox cbo)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_SRSMIX_EQPT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    Util.AlertByBiz("DA_BAS_SEL_SRSMIX_EQPT_CBO", Exception.Message, Exception.ToString());
                    return;
                }
                DataRow dRow = result.NewRow();
                dRow["CBO_NAME"] = "-SELECT-";
                dRow["CBO_CODE"] = "";
                result.Rows.InsertAt(dRow, 0);

                cbo.ItemsSource = DataTableConverter.Convert(result);
                if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedEquipment) select dr).Count() > 0)
                {
                    cbo.SelectedValue = selectedEquipment;
                }
                else if (result.Rows.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
                else if (result.Rows.Count == 0)
                {
                    cbo.SelectedItem = null;
                }
            }
            );
        }
        #region Lot Info
        public void GetProductLot(DataRow drWDInfo)
        {
            try
            {
                if (drWDInfo == null)
                    return;
                // 다른화면 갔다가 다시 오는 경우.. combobox 등 모두 Reset 되는 문제로 조회 가능 여부 체크...
                if (!CanSearch())
                    return;

                string sPrvLot = string.Empty;
                if (dgProductLot.ItemsSource != null && dgProductLot.Rows.Count > 0)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    if (idx >= 0)
                        sPrvLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                }

                ClearControls();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("WOID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["PROCID"] = Process.SRS_MIXING;
                Indata["WOID"] = drWDInfo["WOID"].ToString();
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue.ToString());
                IndataTable.Rows.Add(Indata);


                dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_MIX", "INDATA", "RSLTDT", IndataTable);

                dgProductLot.ItemsSource = DataTableConverter.Convert(dtMain);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void GetProductLot()
        {
            try
            {

                // 다른화면 갔다가 다시 오는 경우.. combobox 등 모두 Reset 되는 문제로 조회 가능 여부 체크...
                if (!CanSearch())
                    return;

                string sPrvLot = string.Empty;
                if (dgProductLot.ItemsSource != null && dgProductLot.Rows.Count > 0)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    if (idx >= 0)
                        sPrvLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                }

                ClearControls();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["PROCID"] = Process.SRS_MIXING;
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue.ToString());
                IndataTable.Rows.Add(Indata);

                dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_MX", "INDATA", "RSLTDT", IndataTable);

                dgProductLot.ItemsSource = DataTableConverter.Convert(dtMain);

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

            Util.gridClear(dgLotInfo);

            DataTable _dtLotInfo = new DataTable();
            _dtLotInfo.Columns.Add("LOTID", typeof(System.String));
            _dtLotInfo.Columns.Add("DEFECTQTY", typeof(decimal));
            _dtLotInfo.Columns.Add("LOSSQTY", typeof(decimal));
            _dtLotInfo.Columns.Add("CHARGEQTY", typeof(decimal));

            DataRow dRow = _dtLotInfo.NewRow();
            dRow["LOTID"] = rowview["LOTID"].ToString();
            _dtLotInfo.Rows.Add(dRow);

            dgLotInfo.ItemsSource = DataTableConverter.Convert(_dtLotInfo);

            if (_WIPSTAT == Wip_State.END)
            {
                _INPUTQTY = Util.NVC_DecimalStr(rowview["INPUTQTY"].ToString());
            }
            else
            {
                _INPUTQTY = Util.NVC_DecimalStr(rowview["GOODQTY"].ToString());
            }

            _LOTID = Util.NVC(rowview["LOTID"].ToString());
            _EQPTID = Util.NVC(rowview["EQPTID"].ToString());
            _WIPSTAT = Util.NVC(rowview["WIPSTAT"].ToString());
            _WIPSNAME = Util.NVC(rowview["WIPSNAME"].ToString());
            _GOODQTY = Util.NVC(rowview["GOODQTY"].ToString());
            _LOSSQTY = Util.NVC(rowview["LOSSQTY"].ToString());
            _WORKORDER = Util.NVC(rowview["WOID"].ToString());
            _WORKDATE = Util.NVC(rowview["WORKDATE"].ToString());
            _WIPDTTM_ST = Util.NVC(rowview["WIPDTTM_ST"].ToString());
            _WIPDTTM_ED = Util.NVC(rowview["WIPDTTM_ED"].ToString());
            _REMARK = Util.NVC(rowview["REMARK"].ToString());
            _CONFIRMUSER = LoginInfo.USERID;
            _VERSION = Util.NVC(rowview["MIXER"].ToString());
            _EQPTID = Util.NVC(rowview["EQPTID"].ToString());
            _SHIFT = Util.NVC(rowview["SHIFT"].ToString());

            GetDefect(_LOTID);
            GetQuality(rowview);
            GetMaterial(_LOTID);

            #region Lot 상세정보
            //txtVersion.Text = Util.NVC(rowview["MIXER"].ToString());
            txtShift.Text = _SHIFT;
            txtStartTime.Text = Util.NVC(rowview["WIPDTTM_ST"].ToString());
            txtEndTime.Text = Util.NVC(rowview["WIPDTTM_ED"].ToString());
            
            txtWorkPerson.Text = Util.NVC(rowview["USERID_ED"].ToString());
            txtIssue.Text = Util.NVC(rowview["REMARK"].ToString());

            #endregion
            //setEnabled(_LOTID);
        }
        #endregion

        #region 불량/품질/자재
        private void GetDefect(string LotID)
        {
            try
            {
                #region Defect
                //Util.gridClear(dgDefect);

                //DataTable IndataTable = new DataTable();
                //IndataTable.Columns.Add("LANGID", typeof(string));
                //IndataTable.Columns.Add("AREAID", typeof(string));
                //IndataTable.Columns.Add("PROCID", typeof(string));

                //DataRow Indata = IndataTable.NewRow();
                //Indata["LANGID"] = LoginInfo.LANGID;
                //Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                //Indata["PROCID"] = Process.MIXING;
                //IndataTable.Rows.Add(Indata);

                //string sLotID = string.Empty;

                //DataTable _Defect = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_DEFECT", "INDATA", "RSLTDT", IndataTable);

                //dgDefect.ItemsSource = DataTableConverter.Convert(_Defect);

                //// Defect Column 생성..
                //if (dgDefect.Rows.Count > 0)
                //{
                //    // 기존 추가된 Col 삭제..                
                //    for (int i = dgDefect.Columns.Count; i-- > 0;)
                //    {
                //        if (i >= 4)
                //            dgDefect.Columns.RemoveAt(i);
                //    }

                //    for (int i = 0; i < dgLotInfo.Rows.Count - dgLotInfo.TopRows.Count; i++)
                //    {
                //        sLotID = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID"));

                //        Util.SetGridColumnNumeric(dgDefect, "RESNQTY", null, sLotID, true, true, true, false, -1, HorizontalAlignment.Right, Visibility.Visible);

                //        if (dgDefect.Rows.Count == 0) continue;

                //        DataTable dt = GetDefectDataByLot(sLotID, "DEFECT_LOT");
                //        if (dt != null)
                //        {
                //            for (int j = 0; j < dt.Rows.Count; j++)
                //            {
                //                if (dt.Rows[j]["RESNQTY"].ToString().Equals(""))
                //                {
                //                    BindingDataGrid(dgDefect, j, dgDefect.Columns.Count, 0);
                //                }
                //                else
                //                {
                //                    BindingDataGrid(dgDefect, j, dgDefect.Columns.Count, dt.Rows[j]["RESNQTY"]);
                //                }
                //            }
                //        }
                //    }
                //}

                #endregion
                string sLotID = string.Empty;

                #region Loss
                Util.gridClear(dgLoss);

                DataTable LossdataTable = new DataTable();
                LossdataTable.Columns.Add("LANGID", typeof(string));
                LossdataTable.Columns.Add("AREAID", typeof(string));
                LossdataTable.Columns.Add("PROCID", typeof(string));

                DataRow Lossdata = LossdataTable.NewRow();
                Lossdata["LANGID"] = LoginInfo.LANGID;
                Lossdata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Lossdata["PROCID"] = Process.MIXING;
                LossdataTable.Rows.Add(Lossdata);

                DataTable _Loss = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_LOSS", "INDATA", "RSLTDT", LossdataTable);

                dgLoss.ItemsSource = DataTableConverter.Convert(_Loss);

                // Defect Column 생성..
                if (dgLoss.Rows.Count > 0)
                {
                    // 기존 추가된 Col 삭제..                
                    for (int i = dgLoss.Columns.Count; i-- > 0;)
                    {
                        if (i >= 6)
                            dgLoss.Columns.RemoveAt(i);
                    }

                    for (int i = 0; i < dgLotInfo.Rows.Count - dgLotInfo.TopRows.Count; i++)
                    {
                        sLotID = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID"));

                        Util.SetGridColumnNumeric(dgLoss, "RESNQTY", null, sLotID, true, true, true, false, -1, HorizontalAlignment.Right, Visibility.Visible);

                        if (dgLoss.Rows.Count == 0) continue;

                        DataTable dt = GetDefectDataByLot(sLotID, "LOSS_LOT");
                        if (dt != null)
                        {
                            for (int j = 0; j < dt.Rows.Count; j++)
                            {
                                if (dt.Rows[j]["RESNQTY"].ToString().Equals(""))
                                {
                                    BindingDataGrid(dgLoss, j, dgLoss.Columns.Count, 0);
                                }
                                else
                                {
                                    BindingDataGrid(dgLoss, j, dgLoss.Columns.Count, dt.Rows[j]["RESNQTY"]);
                                }
                            }
                        }
                    }
                }

                #endregion

                #region Charge
                Util.gridClear(dgCharge);

                DataTable ChargedataTable = new DataTable();
                ChargedataTable.Columns.Add("LANGID", typeof(string));
                ChargedataTable.Columns.Add("AREAID", typeof(string));
                ChargedataTable.Columns.Add("PROCID", typeof(string));

                DataRow Chargedata = ChargedataTable.NewRow();
                Chargedata["LANGID"] = LoginInfo.LANGID;
                Chargedata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Chargedata["PROCID"] = Process.MIXING;
                ChargedataTable.Rows.Add(Chargedata);

                DataTable _Charge = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_CHARGE", "INDATA", "RSLTDT", ChargedataTable);

                dgCharge.ItemsSource = DataTableConverter.Convert(_Charge);

                // Defect Column 생성..
                if (dgCharge.Rows.Count > 0)
                {
                    // 기존 추가된 Col 삭제..                
                    for (int i = dgCharge.Columns.Count; i-- > 0;)
                    {
                        if (i >= 6)
                            dgCharge.Columns.RemoveAt(i);
                    }

                    for (int i = 0; i < dgLotInfo.Rows.Count - dgLotInfo.TopRows.Count; i++)
                    {
                        sLotID = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID"));

                        Util.SetGridColumnNumeric(dgCharge, "RESNQTY", null, sLotID, true, true, true, false, -1, HorizontalAlignment.Right, Visibility.Visible);

                        if (dgCharge.Rows.Count == 0) continue;

                        DataTable dt = GetDefectDataByLot(sLotID, "CHARGE_PROD_LOT");
                        if (dt != null)
                        {
                            for (int j = 0; j < dt.Rows.Count; j++)
                            {
                                if (dt.Rows[j]["RESNQTY"].ToString().Equals(""))
                                {
                                    BindingDataGrid(dgCharge, j, dgCharge.Columns.Count, 0);
                                }
                                else
                                {
                                    BindingDataGrid(dgCharge, j, dgCharge.Columns.Count, dt.Rows[j]["RESNQTY"]);
                                }
                            }
                        }
                    }
                }
                #endregion

                GetDefectSum();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void GetDefectSum()
        {
            decimal ValueToDefect = 0;
            decimal ValueToLoss = 0;
            decimal ValueToCharge = 0;

            SumDefectTotalQty(ref ValueToDefect, ref ValueToLoss, ref ValueToCharge);

            decimal TotalSum = ValueToDefect + ValueToLoss + ValueToCharge;
            _LOSSQTY = string.Format("{0:F2}", TotalSum.ToString());

            txtGoodQuntity.Text = string.Format("{0:F2}", txtProcQuntity.Value == 0 ? "0" : string.Format("{0:F2}", Convert.ToString(txtProcQuntity.Value - Convert.ToDouble(_LOSSQTY))));
            txtLossQuntity.Text = string.Format("{0:F2}", _LOSSQTY);

            DataTable dt = ((DataView)dgLotInfo.ItemsSource).Table;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "DEFECTQTY", ValueToDefect);
                DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "LOSSQTY", ValueToLoss);
                DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "CHARGEQTY", ValueToCharge);
            }

        }
        private DataTable GetDefectDataByLot(string LotId, string sACTID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("ACTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["LOTID"] = LotId;
                Indata["ACTID"] = sACTID;
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
        private void SetDefect(string LotID, C1.WPF.DataGrid.C1DataGrid datagrid)
        {
            if (datagrid.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1585");  //불량정보가 없습니다.
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

            #region Defect Lot
            //DataTable inDefectLot = _DefectLot(LotID, datagrid);
            DataTable dt = ((DataView)datagrid.ItemsSource).Table;

            DataTable IndataTable = inDataSet.Tables.Add("INRESN");
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("ACTID", typeof(string));
            IndataTable.Columns.Add("RESNCODE", typeof(string));
            IndataTable.Columns.Add("RESNQTY", typeof(decimal));

            DataRow DataRow = null;

            foreach (DataRow _iRow in dt.Rows)
            {
                DataRow = IndataTable.NewRow();
                DataRow["LOTID"] = LotID;
                DataRow["ACTID"] = _iRow["ACTID"];
                DataRow["RESNCODE"] = _iRow["RESNCODE"];
                DataRow["RESNQTY"] = _iRow["RESNQTY"].ToString().Equals("") ? 0 : _iRow["RESNQTY"];
                IndataTable.Rows.Add(DataRow);
            }
            #endregion

            new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT_DEFECT", "INDATA,INRESN", null, (result, ex) =>
            {
                if (ex != null)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage(ex.Message), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    Util.AlertByBiz("BR_QCA_REG_WIPREASONCOLLECT_DEFECT", ex.Message, ex.ToString());
                    gDefectChangeFlag = false;
                    return;
                }
                Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                datagrid.EndEdit(true);
                GetDefect(_LOTID);
                gDefectChangeFlag = true;
            }, inDataSet);
        }
        private void SetLoss(string LotID, C1.WPF.DataGrid.C1DataGrid datagrid)
        {
            if (datagrid.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1358");  //Loss정보가 없습니다.
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

            #region Defect Lot
            //DataTable inDefectLot = _DefectLot(LotID, datagrid);
            DataTable dt = ((DataView)datagrid.ItemsSource).Table;

            DataTable IndataTable = inDataSet.Tables.Add("INRESN");
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("ACTID", typeof(string));
            IndataTable.Columns.Add("RESNCODE", typeof(string));
            IndataTable.Columns.Add("RESNQTY", typeof(decimal));

            DataRow DataRow = null;

            foreach (DataRow _iRow in dt.Rows)
            {
                DataRow = IndataTable.NewRow();
                DataRow["LOTID"] = LotID;
                DataRow["ACTID"] = _iRow["ACTID"];
                DataRow["RESNCODE"] = _iRow["RESNCODE"];
                DataRow["RESNQTY"] = _iRow["RESNQTY"].ToString().Equals("") ? 0 : _iRow["RESNQTY"];
                IndataTable.Rows.Add(DataRow);
            }
            #endregion

            new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT_LOSS", "INDATA,INRESN", null, (result, ex) =>
            {
                if (ex != null)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage(ex.Message), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    Util.AlertByBiz("BR_QCA_REG_WIPREASONCOLLECT_LOSS", ex.Message, ex.ToString());
                    gDefectChangeFlag = false;
                    return;
                }
                Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                datagrid.EndEdit(true);
                GetDefect(_LOTID);
                gDefectChangeFlag = true;
            }, inDataSet);
        }
        private void SetCharge(string LotID, C1.WPF.DataGrid.C1DataGrid datagrid)
        {
            if (datagrid.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1535");  //물청정보가 없습니다.
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

            #region Defect Lot
            //DataTable inDefectLot = _DefectLot(LotID, datagrid);
            DataTable dt = ((DataView)datagrid.ItemsSource).Table;

            DataTable IndataTable = inDataSet.Tables.Add("INRESN");
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("ACTID", typeof(string));
            IndataTable.Columns.Add("RESNCODE", typeof(string));
            IndataTable.Columns.Add("RESNQTY", typeof(decimal));
            IndataTable.Columns.Add("COST_CNTR_ID", typeof(string));

            DataRow DataRow = null;

            foreach (DataRow _iRow in dt.Rows)
            {
                DataRow = IndataTable.NewRow();
                DataRow["LOTID"] = LotID;
                DataRow["ACTID"] = _iRow["ACTID"];
                DataRow["RESNCODE"] = _iRow["RESNCODE"];
                DataRow["RESNQTY"] = _iRow["RESNQTY"].ToString().Equals("") ? 0 : _iRow["RESNQTY"];
                DataRow["COST_CNTR_ID"] = _iRow["COST_CNTR_ID"].ToString().Equals("") ? "" : _iRow["COST_CNTR_ID"];
                IndataTable.Rows.Add(DataRow);
            }
            #endregion

            new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT_CHARGE_PROD", "INDATA,INRESN", null, (result, ex) =>
            {
                if (ex != null)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage(ex.Message), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    Util.AlertByBiz("BR_QCA_REG_WIPREASONCOLLECT_CHARGE_PROD", ex.Message, ex.ToString());
                    gDefectChangeFlag = false;
                    return;
                }
                Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                datagrid.EndEdit(true);
                GetDefect(_LOTID);
                gDefectChangeFlag = true;
            }, inDataSet);
        }
        private DataTable _DefectLot(string LotID, C1.WPF.DataGrid.C1DataGrid datagrid)
        {
            DataTable dt = ((DataView)datagrid.ItemsSource).Table;

            DataTable IndataTable = inDataSet.Tables.Add("INRESN");
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("ACTID", typeof(string));
            IndataTable.Columns.Add("RESNCODE", typeof(string));
            IndataTable.Columns.Add("RESNQTY", typeof(decimal));

            DataRow inDataRow = null;

            foreach (DataRow _iRow in dt.Rows)
            {
                inDataRow = IndataTable.NewRow();
                inDataRow["LOTID"] = LotID;
                inDataRow["ACTID"] = _iRow["ACTID"];
                inDataRow["RESNCODE"] = _iRow["RESNCODE"];
                inDataRow["RESNQTY"] = _iRow["RESNQTY"].ToString().Equals("") ? 0 : _iRow["RESNQTY"];
                IndataTable.Rows.Add(inDataRow);
            }
            return IndataTable;
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
                Indata["PROCID"] = Process.SRS_MIXING;
                IndataTable.Rows.Add(Indata);

                DataTable dtqa = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_LOT", "INDATA", "RSLTDT", IndataTable);
                if (dtqa.Rows.Count == 0)
                {
                    dtqa = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROC_CLCTITEM", "INDATA", "RSLTDT", IndataTable);
                    dgQuality.ItemsSource = DataTableConverter.Convert(dtqa);
                }
                else
                {
                    dgQuality.ItemsSource = DataTableConverter.Convert(dtqa);
                }

#if false
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
                        string sLotid = string.Empty;
                        if (dgQuality.Rows.Count > 0)
                        {
                            // 기존 추가된 Col 삭제..                
                            for (int i = dgQuality.Columns.Count; i-- > 0;)
                            {
                                if (i >= 4)
                                    dgQuality.Columns.RemoveAt(i);
                            }

                            for (int i = 0; i < dgLotInfo.Rows.Count; i++)
                            {
                                sLotid = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID"));

                                Util.SetGridColumnNumeric(dgQuality, sLotid, null, sLotid, true, true, true, false, -1, HorizontalAlignment.Right, Visibility.Visible);
                                if (dgQuality.Rows.Count == 0) continue;

                                DataTable dt = GetQualityDataByLot(sLotid);
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
#endif


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetQuality(string LotID)
        {
            if (dgQuality.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2002");  //품질정보가 없습니다.
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
        private void GetWIPSEQ(string sLotID, string sCLCTITEM)
        {
            _WIPSEQ = string.Empty;
            _CLCTSEQ = string.Empty;

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));
            IndataTable.Columns.Add("CLCTITEM", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LOTID"] = sLotID;
            Indata["PROCID"] = Process.MIXING;
            Indata["CLCTITEM"] = sCLCTITEM;
            IndataTable.Rows.Add(Indata);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPDATACOLLECT_WIPSEQ_SL", "INDATA", "RSLTDT", IndataTable);
            if (dtResult.Rows.Count == 0)
            {
                _WIPSEQ = string.Empty;
                _CLCTSEQ = string.Empty;
            }
            else
            {
                _WIPSEQ = dtResult.Rows[0]["WIPSEQ"].ToString();
                _CLCTSEQ = dtResult.Rows[0]["CLCTSEQ"].ToString();
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
            for (int iCol = 4; iCol < dgQuality.Columns.Count; iCol++)
            {
                string sublotid = dgQuality.Columns[iCol].Name;

                foreach (DataRow _iRow in dt.Rows)
                {
                    GetWIPSEQ(sublotid, _iRow["CLCTITEM"].ToString());
                    inData = IndataTable.NewRow();

                    inData["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    inData["LOTID"] = sublotid;
                    inData["EQPTID"] = _EQPTID;
                    inData["USERID"] = LoginInfo.USERID;
                    inData["CLCTITEM"] = _iRow["CLCTITEM"];
                    inData["CLCTVAL01"] = _iRow[sublotid].ToString().Equals("") ? 0 : _iRow[sublotid];
                    inData["WIPSEQ"] = _WIPSEQ == "" ? null : _WIPSEQ;
                    inData["CLCTSEQ"] = _CLCTSEQ == "" ? null : _CLCTSEQ;
                    IndataTable.Rows.Add(inData);

                }
            }
            return IndataTable;
        }
        private void GetMaterial(string sLotID)
        {
            try
            {
                Util.gridClear(dgMaterial);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = sLotID;
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONSUME_MATERIAL", "INDATA", "RSLTDT", IndataTable);

                dgMaterial.ItemsSource = DataTableConverter.Convert(dtResult);

                SetGridCboItem(dgMaterial.Columns["MTRLID"], _WORKORDER);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetMaterial(string LotID)
        {
            if (dgMaterial.Rows.Count < 0)
            {
                return;
            }
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
            inDataTable.Columns.Add("INPUT_DTTM", typeof(DateTime));
            inDataTable.Columns.Add("MTRLID", typeof(string));
            inDataTable.Columns.Add("INPUT_QTY", typeof(decimal));

            DataRow inData = null;
            DataRowView rowview = null;

            foreach (C1.WPF.DataGrid.DataGridRow row in dgMaterial.Rows)
            {

                rowview = row.DataItem as DataRowView;

                inData = inDataTable.NewRow();

                inData["LOTID"] = LotID;
                inData["INPUT_LOTID"] = Util.NVC(rowview["INPUT_LOTID"]);
                inData["INPUT_DTTM"] = rowview["INPUT_DTTM"];
                inData["MTRLID"] = Util.NVC(rowview["MTRLID"]);
                inData["INPUT_QTY"] = Util.NVC_Decimal(rowview["INPUT_QTY"]);

                inDataTable.Rows.Add(inData);
            }

            new ClientProxy().ExecuteService("DA_PRD_INS_WIPMTRL_ELEC", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
            {
                if (ex != null)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage(ex.Message), ex.Message, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    Util.AlertByBiz("DA_PRD_INS_WIPMTRL_ELEC", ex.Message, ex.ToString());
                    return;
                }

                Util.AlertInfo("SFU1985");  //투입자재 저장완료

            });
        }
        private void SetMaterial(string LotID, string PROC_TYPE)
        {
            if (dgMaterial.Rows.Count < 1)
            {
                return;
            }
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
            inDataTable.Columns.Add("MTRLID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("INPUT_QTY", typeof(decimal));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROC_TYPE", typeof(string));

            DataRow inData = null;
            DataTable dt = ((DataView)dgMaterial.ItemsSource).Table;

            foreach (DataRow row in dt.Rows)
            {
                if (Convert.ToBoolean(row["CHK"]))
                {
                    inData = inDataTable.NewRow();
                    inData["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    inData["EQPTID"] = _EQPTID;
                    inData["LOTID"] = LotID;
                    inData["INPUT_LOTID"] = Util.NVC(row["INPUT_LOTID"]);
                    inData["MTRLID"] = Util.NVC(row["MTRLID"]);
                    inData["PROD_LOTID"] = LotID;
                    inData["INPUT_QTY"] = Util.NVC_Decimal(row["INPUT_QTY"]);
                    inData["USERID"] = LoginInfo.USERID;
                    inData["PROC_TYPE"] = PROC_TYPE;
                    inDataTable.Rows.Add(inData);
                }
            }

            new ClientProxy().ExecuteService("BR_PRD_REG_MODIFY_IN_LOT", "INDATA", null, inDataTable, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.AlertByBiz("BR_PRD_REG_MODIFY_IN_LOT", ex.Message, ex.ToString());
                    return;
                }

                Util.AlertInfo("SFU1985");  //투입자재 저장완료
                GetMaterial(LotID);
            });
        }
        private void GetBeadMill()
        {
            DataTable _BeadMill = new DataTable();
            _BeadMill.Columns.Add("BM_ANCHOR_CURR", typeof(string));
            _BeadMill.Columns.Add("BM_400_CURR", typeof(string));
            _BeadMill.Columns.Add("BM_PUMP_CURR", typeof(string));
            _BeadMill.Columns.Add("BM_STANK_CURR", typeof(string));
            _BeadMill.Columns.Add("BM_TIME", typeof(string));

            DataRow _dRow = null;
            for (int i = 0; i < 5; i++)
            {
                _dRow = _BeadMill.NewRow();
                _dRow["BM_ANCHOR_CURR"] = "";
                _dRow["BM_400_CURR"] = "";
                _dRow["BM_PUMP_CURR"] = "";
                _dRow["BM_STANK_CURR"] = "";
                _dRow["BM_TIME"] = "";
                _BeadMill.Rows.Add(_dRow);
            }

            dgBeadMill.ItemsSource = DataTableConverter.Convert(_BeadMill);

        }
        private void SetBeadMill(string LotID)
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("UDF_ATTR01", typeof(string));
            inDataTable.Columns.Add("UDF_ATTR02", typeof(string));
            inDataTable.Columns.Add("UDF_ATTR03", typeof(string));
            inDataTable.Columns.Add("UDF_ATTR04", typeof(string));
            inDataTable.Columns.Add("UDF_ATTR05", typeof(string));
            inDataTable.Columns.Add("REG_USER", typeof(string));

            DataRow inData = null;
            
            string _sTag = string.Empty;
            string _sVal = string.Empty;

            for (int i = 0; i < dgBeadMill.Rows.Count; i++)
            {
                inData = inDataTable.NewRow();
                for (int j = 0; j < dgBeadMill.Columns.Count; j++)
                {
                    _sTag = dgBeadMill.Columns[j].Name + (i + 1).ToString();
                    _sVal = dgBeadMill.Columns[j].GetCellText(dgBeadMill.Rows[i]);

                    if (_sVal.Trim().Equals("")) continue;

                    inData["LOTID"] = _LOTID;
                    inData["UDF_ATTR01"] = LoginInfo.CFG_SHOP_ID;
                    inData["UDF_ATTR02"] = Process.SRS_MIXING;
                    inData["UDF_ATTR03"] = _sTag;
                    inData["UDF_ATTR04"] = _sVal;
                    inData["UDF_ATTR05"] = "";
                    inData["REG_USER"] = "";
                }
                inDataTable.Rows.Add(inData);
            }

            //*MUST_BIZ_APPLY
            new ClientProxy().ExecuteService("SRS_WD_SAVE_DATACOLLECT", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
            {
                if (ex != null)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1737" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning); // 예외발생
                    Util.AlertByBiz("SRS_WD_SAVE_DATACOLLECT", ex.Message, ex.ToString());
                    return;
                }

                Util.AlertInfo("SFU1270");  //저장되었습니다.

            });
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

            winWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
            winWorkOrder.PROCID = Process.SRS_MIXING;

            winWorkOrder.GetWorkOrder();
        }
        private DataRow GetSelectWorkOrderInfo()
        {
            if (winWorkOrder == null)
                return null;

            winWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
            winWorkOrder.PROCID = Process.SRS_MIXING;

            return winWorkOrder.GetSelectWorkOrderRow();
        }
        public bool GetSearchConditions(out string sProc, out string sEqsg, out string sEqpt)
        {
            try
            {
                sProc = Process.SRS_MIXING;
                sEqsg = cboEquipmentSegment.SelectedIndex >= 0 ? cboEquipmentSegment.SelectedValue.ToString() : "";
                sEqpt = cboEquipment.SelectedIndex >= 0 ? cboEquipment.SelectedValue.ToString() : "";

                return true;
            }
            catch (Exception ex)
            {
                sProc = "";
                sEqsg = "";
                sEqpt = "";
                return false;
                throw ex;
            }
        }
#endregion

#region LOT Start / End
        private void LotStart_Closed(object sender, EventArgs e)
        {
            ELEC001_006_LOTSTART window = sender as ELEC001_006_LOTSTART;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                ClearControls();
                GetWorkOrder();
                GetProductLot();
            }
        }

        private void LotEnd_Closed(object sender, EventArgs e)
        {
            ELEC001_006_LOTEND window = sender as ELEC001_006_LOTEND;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                ClearControls();
                GetWorkOrder();
                GetProductLot();
            }
        }
#endregion

#region LOT Confirm
        private bool ValidateConfirm()
        {
            if (!ValidQty()) return false;
            //if (!gDefectChangeFlag)
            //{
            //    Util.Alert("SFU1258");  //불량정보가 변경 후 저장되지 않았습니다.
            //    return false;
            //}
            if (!ValidShift()) return false;
            if (!ValidOperator()) return false;
            //if (!ValidVersion()) return false;
            return true;
        }
        private void ConfirmProcess()
        {
#region Lot Info
            inDataSet = new DataSet();

            DataTable inDataTable = inDataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            //inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPDTTM_ED", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow inDataRow = null;

            inDataRow = inDataTable.NewRow();
            inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
            inDataRow["EQPTID"] = _EQPTID;
            //inDataRow["PROD_VER_CODE"] = txtVersion.Text;
            inDataRow["SHIFT"] = txtShift.Tag;
            inDataRow["WIPDTTM_ED"] = Util.StringToDateTime(dtpEndDateTime.DateTime.Value.ToString("yyyy-MM-dd HH:mm"));
            inDataRow["WIPNOTE"] = txtIssue.Text;
            inDataRow["WRK_USER_NAME"] = txtWorkPerson.Text;
            inDataRow["USERID"] = LoginInfo.USERID;
            inDataTable.Rows.Add(inDataRow);
#endregion

#region Detail Lot
            DataRow inLotDataRow = null;

            DataTable InLotdataTable = inDataSet.Tables.Add("INLOT");
            InLotdataTable.Columns.Add("LOTID", typeof(string));
            InLotdataTable.Columns.Add("INPUTQTY", typeof(decimal));
            InLotdataTable.Columns.Add("OUTPUTQTY", typeof(decimal));
            InLotdataTable.Columns.Add("RESNQTY", typeof(decimal));

            inLotDataRow = InLotdataTable.NewRow();
            inLotDataRow["LOTID"] = _LOTID;
            inLotDataRow["INPUTQTY"] = Util.NVC(txtProcQuntity.Value);
            inLotDataRow["OUTPUTQTY"] = Util.NVC_Decimal(txtGoodQuntity.Text);
            inLotDataRow["RESNQTY"] = Util.NVC_Decimal(txtLossQuntity.Text);
            InLotdataTable.Rows.Add(inLotDataRow);
#endregion

            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_SX", "INDATA,INLOT", null, (result, ex) =>
            {
                try
                {
                    if (ex != null)
                    {
                        Util.AlertByBiz("BR_PRD_REG_END_LOT_SX", ex.Message, ex.ToString());
                        return;
                    }
                    Util.AlertInfo("SFU1275"); //정상처리되었습니다.

                    ClearControls();
                    GetWorkOrder();
                    GetProductLot();
                }
                catch(Exception err)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage(err.Message), err.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }

            }, inDataSet);
        }

#endregion

#region Function
        private void SumDefectTotalQty(ref decimal DefectSum, ref decimal LossSum, ref decimal ChargeSum)
        {
            DataTable Defectdt = DataTableConverter.Convert(dgDefect.ItemsSource); // ((DataView)dgDefect.ItemsSource).Table;

            foreach (DataRow drow in Defectdt.Rows)
            {
                if (!drow["RESNQTY"].ToString().Equals(""))
                {
                    DefectSum += Util.NVC_Decimal(drow["RESNQTY"]);
                }
            }

            DataTable Lossdt = DataTableConverter.Convert(dgLoss.ItemsSource); // ((DataView)dgLoss.ItemsSource).Table;

            foreach (DataRow drow in Lossdt.Rows)
            {
                if (!drow["RESNQTY"].ToString().Equals(""))
                {
                    LossSum += Util.NVC_Decimal(drow["RESNQTY"]);
                }
            }

            DataTable Chargedt = DataTableConverter.Convert(dgCharge.ItemsSource); // ((DataView)dgCharge.ItemsSource).Table;

            foreach (DataRow drow in Chargedt.Rows)
            {
                if (!drow["RESNQTY"].ToString().Equals(""))
                {
                    ChargeSum += Util.NVC_Decimal(drow["RESNQTY"]);
                }
            }

        }
        private bool ValidQty()
        {
            if (txtGoodQuntity.Text.Trim().Equals("0") || txtGoodQuntity.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU2921"); //양품량을 입력해주세요.
                return false;
            }
            if (txtLossQuntity.Text.Trim().Equals("0") || txtLossQuntity.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU1358");  //Loss정보가 없습니다.
                return false;
            }
            if (Util.NVC_Int(txtGoodQuntity.Text) <= 0)
            {
                Util.MessageValidation("SFU1721");  //양품량은 음수가 될 수 없습니다. 값을 맞게 변경하세요.
                return false;
            }
            return true;
        }
        private bool ValidWorkTime()
        {
            //if (Util.NVC_Decimal(txtWorkTime.Text) < 0)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("가동시간을 확인 하세요."), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //    return false;
            //}
            return true;
        }
        private bool ValidShift()
        {
            if (txtShift.Text.Trim() == "")
            {
                Util.MessageValidation("SFU1844");  //작업조를 선택하세요.
                return false;
            }
            return true;
        }
        private bool ValidOperator()
        {
            if (txtWorkPerson.Text.Trim() == "")
            {
                Util.MessageValidation("SFU1843");  //작업자를 입력 해 주세요.
                return false;
            }
            return true;
        }
        private bool ValidVersion()
        {
            //if (txtVersion.Text.Trim() == "")
            //{
            //    Util.MessageValidation("SFU1218");  //Version 정보를 입력 하세요.
            //    return false;
            //}
            return true;
        }
        private bool CanSearch()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        private void InitGrid()
        {

            for (int index = dgDefect.Columns.Count - 1; index > 4; index--)
            {
                dgDefect.Columns.RemoveAt(index);
            }
            for (int index = dgLoss.Columns.Count - 1; index > 6; index--)
            {
                dgLoss.Columns.RemoveAt(index);
            }
            for (int index = dgCharge.Columns.Count - 1; index > 6; index--)
            {
                dgCharge.Columns.RemoveAt(index);
            }
            for (int index = dgQuality.Columns.Count - 1; index > 4; index--)
            {
                dgQuality.Columns.RemoveAt(index);
            }
        }
        private void InitTxt()
        {
            //txtVersion.Text = string.Empty;
            txtProcQuntity.Value = 0;
            txtGoodQuntity.Text = string.Empty;
            txtLossQuntity.Text = string.Empty;
            
            txtStartTime.Text = string.Empty;
            txtEndTime.Text = string.Empty;
            
            txtWorkPerson.Text = string.Empty;
            txtIssue.Text = string.Empty;
            dtpEndDateTime.DateTime = System.DateTime.Now;
            txtWorkPerson.Text = string.Empty;
            txtShift.Text = string.Empty;
        }

        private void setEnabled()
        {
            //btnRunStart.IsEnabled = true;
        }
        private void setEnabled(Object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
            {
                btnRunStart.IsEnabled = true;
                btnRunCancel.IsEnabled = false;
                btnRunComplete.IsEnabled = false;
                btnConfirm.IsEnabled = false;

                btnCommit.IsEnabled = false;
                btnQualitySave.IsEnabled = false;
                btnInputMaterial.IsEnabled = false;

                btnAdd.IsEnabled = false;
                //btnDel.IsEnabled = false;
            }
            else
            {
                if (_WIPSTAT == Wip_State.PROC)
                {
                    btnRunStart.IsEnabled = false;
                    btnRunCancel.IsEnabled = true;
                    btnRunComplete.IsEnabled = true;
                    btnConfirm.IsEnabled = true;  // false

                    btnAdd.IsEnabled = true;
                    //btnDel.IsEnabled = true;

                    btnCommit.IsEnabled = true;  // false
                    btnQualitySave.IsEnabled = true;  //false
                    btnInputMaterial.IsEnabled = true; //false
                }
                else if (_WIPSTAT == Wip_State.EQPT_END || _WIPSTAT == Wip_State.END)
                {
                    btnRunStart.IsEnabled = false;
                    btnRunCancel.IsEnabled = false;
                    btnRunComplete.IsEnabled = false;
                    btnConfirm.IsEnabled = true;

                    btnAdd.IsEnabled = true;
                    //btnDel.IsEnabled = true;

                    btnCommit.IsEnabled = true;
                    btnQualitySave.IsEnabled = true;
                    btnInputMaterial.IsEnabled = true;

                }
            }
            if (_WIPSTAT == Wip_State.END)
            {
                btnCommit.IsEnabled = false;
                btnQualitySave.IsEnabled = false;
                btnInputMaterial.IsEnabled = false;
            }
        }
        private void setEnabled(string SelectedItem)
        {
            if (string.IsNullOrEmpty(SelectedItem))
            {
                btnRunStart.IsEnabled = true;
                btnRunCancel.IsEnabled = false;
                btnRunComplete.IsEnabled = false;
                btnConfirm.IsEnabled = false;

                btnCommit.IsEnabled = false;
                btnQualitySave.IsEnabled = false;
                btnInputMaterial.IsEnabled = false;

                btnAdd.IsEnabled = false;
                //btnDel.IsEnabled = false;
            }
            else
            {
                if (_WIPSTAT == Wip_State.PROC)
                {
                    btnRunStart.IsEnabled = false;
                    btnRunCancel.IsEnabled = true;
                    btnRunComplete.IsEnabled = true;
                    btnConfirm.IsEnabled = true;  // false

                    btnAdd.IsEnabled = true;
                    //btnDel.IsEnabled = true;

                    btnCommit.IsEnabled = true;  //false
                    btnQualitySave.IsEnabled = true;  //false
                    btnInputMaterial.IsEnabled = true;  //false
                }
                else if (_WIPSTAT == Wip_State.EQPT_END|| _WIPSTAT == Wip_State.END)
                {
                    btnRunStart.IsEnabled = false;
                    btnRunCancel.IsEnabled = false;
                    btnRunComplete.IsEnabled = false;
                    btnConfirm.IsEnabled = true;

                    btnAdd.IsEnabled = true;
                    //btnDel.IsEnabled = true;

                    btnCommit.IsEnabled = true;
                    btnQualitySave.IsEnabled = true;
                    btnInputMaterial.IsEnabled = true;

                }
            }
            if (_WIPSTAT == Wip_State.END)
            {
                btnCommit.IsEnabled = false;
                btnQualitySave.IsEnabled = false;
                btnInputMaterial.IsEnabled = false;
            }
        }
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            //listAuth.Add(btnInReplace);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        private void SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col, string sWOID)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("WOID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["WOID"] = sWOID;
            IndataTable.Rows.Add(Indata);

            dtMain = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_TB_SFC_WO_MTRL", "INDATA", "RSLTDT", IndataTable);

            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtMain);
        }
        private bool fn_QualityChk()
        {
            decimal value = 0;

            DataRowView rowview = null; 

            foreach (C1.WPF.DataGrid.DataGridRow row in dgQuality.Rows)
            {
                rowview = row.DataItem as DataRowView;

                if (!rowview["CLCTVAL01"].ToString().Equals(""))
                {
                    value = rowview["CLCTVAL01"].ToString().ToString() == "" ? 0 : decimal.Parse(rowview["CLCTVAL01"].ToString().ToString());
                    if (value < 0) return false;
                }
            }
            return true;
        }
        private decimal SumDefectQty()
        {
            decimal sum = 0;

            DataRowView rowview = null;

            foreach (C1.WPF.DataGrid.DataGridRow row in dgDefect.Rows)
            {
                rowview = row.DataItem as DataRowView;

                if (!rowview["RESNQTY"].ToString().Equals(""))
                {
                    sum += Util.NVC_Decimal(rowview["RESNQTY"]);
                }
            }
            return sum;
        }
#endregion

#endregion

    }

}
