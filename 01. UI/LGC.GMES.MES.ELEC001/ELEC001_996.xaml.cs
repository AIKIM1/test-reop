/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 믹서 공정진척
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.

 [미적용 사항]
     1. 불량/품질 입력 후 마우스 포커스 다른 컨트롤 이동 시 수정값 적용 안됨. 
     2. 품질정보 Check 기능 주석처리 fn_QualityChk
      
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_996 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        DataTable dtMain = new DataTable();
        DataSet inDataSet = null;
        private bool gDefectChangeFlag = false;
        #region CurrentLotInfo
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
        #endregion
        private string _WIPSEQ = string.Empty;
        private string _CLCTSEQ = string.Empty;
        Util _Util = new Util();
        CommonCombo _combo = new CommonCombo();

        private UC_WORKORDER winWorkOrder = new UC_WORKORDER();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ELEC001_996()
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
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);

            String[] sFilter2 = { Process.MIXING };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter2);
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
            Util.gridClear(dgLoss);
            Util.gridClear(dgCharge);
            Util.gridClear(dgQuality);
            Util.gridClear(dgMaterial);
            Util.gridClear(dgLotInfo);
            initCurrentLotInfo();
            InitTxt();
            InitGrid();
            //setEnabled();
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
            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }
            initCurrentLotInfo();
            GetWorkOrder();
            GetProductLot();
            //setEnabled();
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
        private void btnCommit_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateDefect(dgDefect))
            {
                SetDefect(_LOTID, dgDefect);
            }
        }
        private void btnLossSave_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateDefect(dgLoss))
            {
                SetLoss(_LOTID, dgLoss);
            }
        }
        private void btnChargeSave_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateDefect(dgCharge))
            {
                SetCharge(_LOTID, dgCharge);
            }
        }
        private void btnQualitySave_Click(object sender, RoutedEventArgs e)
        {
            SetQuality(_LOTID);
        }
        private void btnInputMaterial_Click(object sender, RoutedEventArgs e)
        {
            SetMaterial(_LOTID);
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (dgMaterial.ItemsSource == null || dgMaterial.Rows.Count < 0)
            {
                return;
            }

            DataTable dt = ((DataView)dgMaterial.ItemsSource).Table;

            DataRow dr = dt.NewRow();
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
        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            dicParam.Add("PROCID", Process.MIXING);
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
            //    Parameters[0] = Process.MIXING;
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
                return ;
            }

            ELEC001_006_LOTEND _LotEnd = new ELEC001_006_LOTEND();
            _LotEnd.FrameOperation = FrameOperation;

            if (_LotEnd != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = Process.MIXING;
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
            ////품질입력 Check
            //if (!fn_QualityChk())
            //{
            //    return;
            //}
            if (!ValidateConfirm())
            {
                return;
            }
            if (chkClean.IsChecked == true)
            {
                //세정이 예약되어 있습니다. 저장하시겠습니까?
                Util.MessageConfirm("SFU1680", (sresult) =>
                {
                    if (sresult == MessageBoxResult.No)
                    {
                        return;
                    }
                });
            }
            else
            {
                //저장하시겠습니까?
                Util.MessageConfirm("SFU1241", (sresult) =>
                {
                    if (sresult == MessageBoxResult.No)
                    {
                        return;
                    }
                    ConfirmProcess();
                });
            }
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

                    new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_START_LOT_MX", "INDATA", null, IndataTable, (result, ex) =>
                    {

                        if (ex != null)
                        {
                            Util.AlertByBiz("BR_PRD_REG_CANCEL_START_LOT_MX", ex.Message, ex.ToString());
                            return;
                        }
                        Util.AlertInfo("SFU1275"); //정상처리되었습니다.

                        ClearControls();
                        GetWorkOrder();
                        GetProductLot();
                    });
                }
            });
        }
        private void txtProcQuntity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                txtGoodQuntity.Text = string.Format("{0:n0}", (Util.NVC_Decimal(txtInputQty.Value) - Util.NVC_Decimal(txtLossQuntity.Text)));
            }
        }
        private bool ValidateDefect(C1.WPF.DataGrid.C1DataGrid datagrid)
        {
            if (txtInputQty.Value == 0)
            {
                Util.MessageValidation("SFU1609");  //생산량을 입력하십시오.
                txtInputQty.Focus();
                return false;
            }
            if (datagrid.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1886");  //정보가 없습니다.
                return false;
            }
            return true;
        }
        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (ValidateDefect(dgDefect))
            {
                GetDefectSum();
                _LOSSQTY = string.Empty;
                _GOODQTY = string.Empty;
                _CHARGQTY = string.Empty;
            }
        }
        private void dgLoss_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (ValidateDefect(dgLoss))
            {
                GetDefectSum();
                _LOSSQTY = string.Empty;
                _GOODQTY = string.Empty;
                _CHARGQTY = string.Empty;
            }
        }
        private void dgCharge_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (ValidateDefect(dgCharge))
            {
                GetDefectSum();
                _LOSSQTY = string.Empty;
                _GOODQTY = string.Empty;
                _CHARGQTY = string.Empty;
            }
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SetCleanState("Y");
            chkClean.Content = "종료예약";
        }
        private void chkClean_Unchecked(object sender, RoutedEventArgs e)
        {
            SetCleanState("N");
            chkClean.Content = "모델종료";
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
                Parameters[3] = Process.MIXING;
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
                Parameters[3] = Process.MIXING;
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
                txtVersion.Text = window._ReturnRecipeNo;
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
        #region Lot Info
        public void GetProductLot(DataRow drWorkOrderInfo)
        {
            try
            {

                if (drWorkOrderInfo == null)
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
                Indata["PROCID"] = Process.MIXING;
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
                Indata["PROCID"] = Process.MIXING;
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
        private void GetLotInfo(object SelectedItem)
        {
            DataRowView rowview = (SelectedItem as DataRowView);

            if (rowview == null)
                return;

            DataTable _dtLotInfo = new DataTable();
            _dtLotInfo.Columns.Add("LOTID", typeof(System.String));
            _dtLotInfo.Columns.Add("DEFECTQTY", typeof(decimal));
            _dtLotInfo.Columns.Add("LOSSQTY", typeof(decimal));
            _dtLotInfo.Columns.Add("CHARGEQTY", typeof(decimal));

            DataRow dRow = _dtLotInfo.NewRow();
            dRow["LOTID"] = Util.NVC(rowview["LOTID"].ToString());
            _dtLotInfo.Rows.Add(dRow);

            dgLotInfo.ItemsSource = DataTableConverter.Convert(_dtLotInfo);


            _LOTID = Util.NVC(rowview["LOTID"].ToString());
            _WIPSTAT = Util.NVC(rowview["WIPSTAT"].ToString());
            _WIPSNAME = Util.NVC(rowview["WIPSNAME"].ToString());

            if (_WIPSTAT == Wip_State.END)
            {
                _INPUTQTY = Util.NVC_DecimalStr(rowview["INPUTQTY"].ToString());
            }
            else
            {
                _INPUTQTY = Util.NVC_DecimalStr(rowview["GOODQTY"].ToString());
            }

            _GOODQTY = Util.NVC(rowview["GOODQTY"].ToString());
            _LOSSQTY = Util.NVC(rowview["LOSSQTY"].ToString());
            _WORKORDER = Util.NVC(rowview["WOID"].ToString());
            _WORKDATE = Util.NVC(rowview["WORKDATE"].ToString());
            _WIPDTTM_ST = Util.NVC(rowview["WIPDTTM_ST"].ToString());
            _WIPDTTM_ED = Util.NVC(rowview["WIPDTTM_ED"].ToString());
            _REMARK = Util.NVC(rowview["REMARK"].ToString());
            _CONFIRMUSER = Util.NVC(rowview["USERID_ED"].ToString());
            _VERSION = Util.NVC(rowview["MIXER"].ToString());
            _EQPTID = Util.NVC(rowview["EQPTID"].ToString());
            _ELECTRODE = Util.NVC(rowview["ELECTRODE"].ToString());
            _SHIFT = Util.NVC(rowview["SHIFT"].ToString());

            GetDefect(_LOTID);
            GetQuality(rowview);
            GetMaterial(rowview);

            txtStartTime.Text = _WIPDTTM_ST; 
            txtEndTime.Text = _WIPDTTM_ED; 
            txtWorkPerson.Text = _CONFIRMUSER; 
            txtIssue.Text = _REMARK; 
            txtShift.Text = _SHIFT;
            txtWorkDate.Text = _WORKDATE;
            txtInputQty.Value = Util.NVC_Decimal(_GOODQTY) < 0 ? 0: Convert.ToDouble(_GOODQTY);

#if false   // 샘플링Loss 삭제
            if (LoginInfo.CFG_AREA_ID == "E6")  // 2공장
            {
                GetSampleLoss(_EQPTID);
            }
#endif

            //setEnabled(_LOTID);
        }
        #endregion

        #region 불량/품질/자재
        private void GetDefect(string LotID) 
        {
            try
            {
                #region Defect
#if false
                Util.gridClear(dgDefect);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROCID"] = Process.MIXING;
                IndataTable.Rows.Add(Indata);

                string sLotID = string.Empty;

                DataTable _Defect = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_DEFECT", "INDATA", "RSLTDT", IndataTable);

                dgDefect.ItemsSource = DataTableConverter.Convert(_Defect);

                // Defect Column 생성..
                if (dgDefect.Rows.Count > 0)
                {
                    // 기존 추가된 Col 삭제..                
                    for (int i = dgDefect.Columns.Count; i-- > 0;)
                    {
                        if (i >= 4)
                            dgDefect.Columns.RemoveAt(i);
                    }

                    for (int i = 0; i < dgLotInfo.Rows.Count - dgLotInfo.TopRows.Count; i++)
                    {
                        sLotID = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID"));

                        Util.SetGridColumnNumeric(dgDefect, "RESNQTY", null, sLotID, true, true, true, false, -1, HorizontalAlignment.Right, Visibility.Visible);

                        if (dgDefect.Rows.Count == 0) continue;

                        DataTable dt = GetDefectDataByLot(sLotID, "DEFECT_LOT");
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
                    }
                }
#endif


                #endregion

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
                string sLotID = string.Empty;
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
            _LOSSQTY = string.Format("{0:n0}", TotalSum);

            txtGoodQuntity.Text = string.Format("{0:N0}", txtInputQty.Value == 0 ? "0": string.Format("{0:N0}", Convert.ToString(txtInputQty.Value - Convert.ToDouble(_LOSSQTY))));
            txtLossQuntity.Text = string.Format("{0:N0}", _LOSSQTY);

            DataTable dt = ((DataView)dgLotInfo.ItemsSource).Table;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "DEFECTQTY", ValueToDefect);  // 항목제거: 사용안함
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
                Indata["PROCID"] = Process.MIXING;
                IndataTable.Rows.Add(Indata);

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
        private void GetMaterial(Object SelectedItem)
        {
            try
            {
                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                {
                    return;
                }
                Util.gridClear(dgMaterial);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = rowview["LOTID"].ToString();
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
            if (dgMaterial.Rows.Count < 1)
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
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1737" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    Util.AlertByBiz("DA_PRD_INS_WIPMTRL_ELEC", ex.Message, ex.ToString());
                    return;
                }

                Util.AlertInfo("SFU1985");  //투입자재 저장완료

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
            winWorkOrder.PROCID = Process.MIXING;

            winWorkOrder.GetWorkOrder();
        }
        private DataRow GetSelectWorkOrderInfo()
        {
            if (winWorkOrder == null)
                return null;

            winWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
            winWorkOrder.PROCID = Process.MIXING;

            return winWorkOrder.GetSelectWorkOrderRow();
        }
        public bool GetSearchConditions(out string sProc, out string sEqsg, out string sEqpt)
        {
            try
            {
                sProc = Process.MIXING;
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

#region LOT Confirmke
        private bool ValidateConfirm()
        {
            if (!CheckMtrl()) return false;
            if (!ValidQty()) return false;
            if (!ValidWorkTime()) return false;
            if (!ValidShift()) return false;
            if (!ValidVersion()) return false;
            if (!ValidOperator()) return false;

#if false   // 불량저장 없이 처리 가능
            if (!gDefectChangeFlag)
            {
                Util.Alert("SFU1258");  //불량정보가 변경 후 저장되지 않았습니다.
                return false;
            }
#endif
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
            inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
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
            inDataRow["PROD_VER_CODE"] = txtVersion.Text;
            inDataRow["SHIFT"] = txtShift.Text;
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
            inLotDataRow["INPUTQTY"] = Util.NVC_Decimal(txtInputQty.Value);
            inLotDataRow["OUTPUTQTY"] = Util.NVC_Decimal(txtGoodQuntity.Text);
            inLotDataRow["RESNQTY"] = Util.NVC_Decimal(txtLossQuntity.Text);
            InLotdataTable.Rows.Add(inLotDataRow);
            #endregion

            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_MX", "INDATA,INLOT", null, (result, ex) =>
            {
                try
                {
                    if (ex != null)
                    {
                        //실적처리 정보 확인
                        LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage("SFU1708"), ex.Message, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    Util.AlertInfo("SFU1275"); //정상처리되었습니다.

                    ClearControls();
                    GetWorkOrder();
                    GetProductLot();
                }
                catch (Exception ErrorMsg)
                {
                    Util.MessageException(ErrorMsg);
                }
            }, inDataSet);

            #region 세정확인
            if (chkClean.IsChecked == true)
            {
                string strPreBatchID = string.Empty;
#if false
                DataTable vIndataTable = new DataTable();
                vIndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow vIndata = vIndataTable.NewRow();
                vIndata["EQPTID"] = _EQPTID;

                vIndataTable.Rows.Add(vIndata);

                dtMain = new ClientProxy().ExecuteServiceSync("QR_MIX_CLEAN_PREBATCHID", "INDATA", "RSLTDT", IndataTable);
                if (dtMain.Rows.Count > 0)
                {
                    strPreBatchID = dtMain.Rows[0]["LOTID"].ToString();
                }

                DataTable zIndataTable = new DataTable();
                zIndataTable.Columns.Add("LOTID", typeof(string));
                zIndataTable.Columns.Add("PREBATCHID", typeof(string));

                DataRow zIndata = zIndataTable.NewRow();
                zIndata["LOTID"] = _LOTID;
                zIndata["PREBATCHID"] = strPreBatchID;

                zIndataTable.Rows.Add(zIndata);

                dtMain = new ClientProxy().ExecuteServiceSync("U_MIX_BATCH_CLEAN", "INDATA", "RSLTDT", IndataTable);

                chkClean.IsChecked = false;
#endif
            }
#endregion
        }
#endregion

#region Function
        private void GetSampleLoss(string EQPTID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = EQPTID;
                IndataTable.Rows.Add(Indata);

                dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIXER_SAMPLE_LOSS", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count > 0)
                {
                    txtSamplingLoss.Text = Util.NVC(dtMain.Rows[0].ToString());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
            txtVersion.Text = string.Empty;
            txtInputQty.Value = 0;
            txtGoodQuntity.Text = string.Empty;
            txtLossQuntity.Text = string.Empty;
            txtStartTime.Text = string.Empty;
            txtEndTime.Text = string.Empty;
            dtpEndDateTime.DateTime = System.DateTime.Now;
            txtWorkPerson.Text = string.Empty;
            txtShift.Text = string.Empty;
            txtIssue.Text = string.Empty;
            txtWorkDate.Text = string.Empty;
        }
        private void setEnabled()
        {
            btnRunStart.IsEnabled = true;
            btnRunCancel.IsEnabled = false;
            btnRunComplete.IsEnabled = false;
            btnConfirm.IsEnabled = false;

            btnCommit.IsEnabled = false;
            btnQualitySave.IsEnabled = false;
            btnInputMaterial.IsEnabled = false;

            btnAdd.IsEnabled = false;
            btnDel.IsEnabled = false;
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
                btnDel.IsEnabled = false;
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
                    btnDel.IsEnabled = true;

                    btnCommit.IsEnabled = true;  //false
                    btnQualitySave.IsEnabled = true;  //false
                    btnInputMaterial.IsEnabled = true;  //false
                }
                else if (_WIPSTAT == Wip_State.EQPT_END || _WIPSTAT == Wip_State.END)
                {
                    btnRunStart.IsEnabled = false;
                    btnRunCancel.IsEnabled = false;
                    btnRunComplete.IsEnabled = false;
                    btnConfirm.IsEnabled = true;

                    btnAdd.IsEnabled = true;
                    btnDel.IsEnabled = true;

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
                    if (value <= 0) return false;
                }
            }
            return true;
        }
        private void SetCleanState(string strFlag)
        {
            try
            {
                Util.AlertInfo("SFU1315");  //BizRuel 없음. Process 확인
                return;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
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
        private void SumDefectTotalQty(ref decimal DefectSum, ref decimal LossSum, ref decimal ChargeSum)
        {

#if false
            DataTable Defectdt = ((DataView)dgDefect.ItemsSource).Table;

            foreach (DataRow drow in Defectdt.Rows)
            {
                 if (!drow["RESNQTY"].ToString().Equals(""))
                {
                    DefectSum += Util.NVC_Decimal(drow["RESNQTY"]);
                }
            }
#endif
            DefectSum = 0; 

            DataTable Lossdt = ((DataView)dgLoss.ItemsSource).Table;

            foreach (DataRow drow in Lossdt.Rows)
            {
                if (!drow["RESNQTY"].ToString().Equals(""))
                {
                    LossSum += Util.NVC_Decimal(drow["RESNQTY"]);
                }
            }

            DataTable Chargedt = ((DataView)dgCharge.ItemsSource).Table;

            foreach (DataRow drow in Chargedt.Rows)
            {
                if (!drow["RESNQTY"].ToString().Equals(""))
                {
                    ChargeSum += Util.NVC_Decimal(drow["RESNQTY"]);
                }
            }

        }
        private bool CheckMtrl()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["LOTID"] = _LOTID;
                inTable.Rows.Add(indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CHKMTRL_MX", "INDATA", "RSLTDT", inTable);
                if (dt.Rows[0]["CNT"].ToString() == "0")
                {
                    Util.MessageValidation("SFU1515");  //등록된 자재가 없습니다.
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        private bool ValidQty()
        {
            if (txtGoodQuntity.Text.Trim() == "" || txtLossQuntity.Text.Trim() == "")
            {
                Util.MessageValidation("SFU2919"); //양품량/Loss량 이 입력되지 않았습니다.
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
#if false
            if (txtVersion.Text.Trim() == "")
            {
                Util.MessageValidation("SFU1218");  //Version 정보를 입력 하세요.
                return false;
            }
#endif
            return true;
        }
        private void CalcWorkTime()
        {
            if (txtStartTime.Text == "") return;
            Double interval = (dtpEndDateTime.DateTime.Value - Convert.ToDateTime(txtStartTime.Text)).TotalMinutes;
            interval = Math.Round(interval, 2);
            txtWorkTime.Text = interval.ToString();
        }
#endregion

#endregion

    }

}
