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
    public partial class ELEC001_906 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        DataTable dtMain = new DataTable();
        DataSet inDataSet = null;
        DataSet _EDCDataSet = null;
        private bool gDefectChangeFlag = false;
        #region CurrentLotInfo
        private string _LOTID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _WIPSTAT = string.Empty;
        private string _WIPSNAME = string.Empty;
        private string _GOODQTY = string.Empty;
        private string _LOSSQTY = string.Empty;
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

        Util _Util = new Util();
        CommonCombo _combo = new CommonCombo();

        private UC_WORKORDER winWorkOrder = new UC_WORKORDER();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ELEC001_906()
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
            Util.gridClear(dgQuality);
            Util.gridClear(dgMaterial);
            Util.gridClear(dgLotInfo);
            initCurrentLotInfo();
            InitTxt();
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
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
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
            initCurrentLotInfo();
            GetWorkOrder();
            GetProductLot();
            //setEnabled();
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
        private void btnCommit_Click(object sender, RoutedEventArgs e)
        {
            SetDefect(_LOTID);
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
            //if (_Util.GetDataGridCheckCnt(dgProductLot, "CHK") == 0)
            //{
            //    Util.MessageValidation("SFU1381");  //LOT을 선택하세요.
            //    return;
            //}

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
            //if (_Util.GetDataGridCheckCnt(dgProductLot, "CHK") == 0)
            //{
            //    Util.MessageValidation("SFU1381");  //LOT을 선택하세요.
            //    return;
            //}

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
            //if (_Util.GetDataGridCheckCnt(dgProductLot, "CHK") < 1)
            //{
            //    Util.MessageValidation("SFU1381");  //LOT을 선택하세요.
            //    return;
            //}

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
                            //Util.Alert("SFU1851");  //작업취소 처리 중 오류가 발생했습니다.
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
            //if (!Util.isNumber(txtGoodQuntity.Text))
            //    return;  string.Format("{0:F2}", txtGoodqty.Value - lossqty);

            if (e.Key == Key.Return)
            {
                txtGoodQuntity.Text = string.Format("{0:F2}", (Util.NVC_Decimal(txtInputQty.Value) - Util.NVC_Decimal(txtLossQuntity.Text)));
            }
        }
        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (txtInputQty.Value == 0)
            {
                Util.MessageValidation("SFU1609");  //생산량을 입력하십시오.
                txtInputQty.Focus();
                e.Cell.Value = 0;
                return;
            }

            decimal sum = SumDefectQty();
            _LOSSQTY = Util.NVC_DecimalStr(sum);
            _GOODQTY = Util.NVC_DecimalStr(Util.NVC_Decimal(txtInputQty.Value) - Util.NVC_Decimal(_LOSSQTY));
            txtGoodQuntity.Text = string.Format("{0:F2}", _GOODQTY);
            txtLossQuntity.Text = string.Format("{0:F2}", _LOSSQTY);
            _LOSSQTY = string.Empty;
            _GOODQTY = string.Empty;
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
            txtVersion.Text = "V01";
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

            Util.gridClear(dgLotInfo);

            DataTable _dtLotInfo = new DataTable();
            _dtLotInfo.Columns.Add("LOTID", typeof(System.String));
            _dtLotInfo.Columns.Add("PRODID", typeof(System.String));
            _dtLotInfo.Columns.Add("PRODNAME", typeof(System.String));

            DataRow dRow = _dtLotInfo.NewRow();
            dRow["LOTID"] = Util.NVC(rowview["LOTID"].ToString());
            dRow["PRODID"] = Util.NVC(rowview["PRODID"].ToString());
            dRow["PRODNAME"] = Util.NVC(rowview["PRODNAME"].ToString());
            _dtLotInfo.Rows.Add(dRow);

            dgLotInfo.ItemsSource = DataTableConverter.Convert(_dtLotInfo);

            _LOTID = Util.NVC(rowview["LOTID"].ToString());
            _WIPSTAT = Util.NVC(rowview["WIPSTAT"].ToString());
            _WIPSNAME = Util.NVC(rowview["WIPSNAME"].ToString());

            if (_WIPSTAT == "END")
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

            if (string.IsNullOrEmpty(_VERSION))
            {
                _VERSION = "V01";
            }
            // 장비완료 없음
            //if (_WIPSTAT == "EQPT_END" || _WIPSTAT == "END")
            //{
            //    GetDefect(rowview);
            //    GetQuality(rowview);
            //}
            //else
            //{
            //    txtVersion.Text = _VERSION;
            //}

            GetDefect(rowview);
            GetQuality(rowview);
            GetMaterial(rowview);

            txtVersion.Text = _VERSION; // Util.NVC(rowview["MIXER"].ToString());
            txtGoodQuntity.Text = string.Format("{0:F2}", _GOODQTY);
            txtLossQuntity.Text = string.Format("{0:F2}", _LOSSQTY); // Util.NVC(rowview["LOSSQTY"].ToString());
            txtStartTime.Text = _WIPDTTM_ST; // Util.NVC(rowview["WIPDTTM_ST"].ToString());
            txtEndTime.Text = _WIPDTTM_ED; // Util.NVC(rowview["WIPDTTM_ED"].ToString());
            txtWorkPerson.Text = _CONFIRMUSER; // Util.NVC(rowview["USERID_ED"].ToString());
            txtIssue.Text = _REMARK; // Util.NVC(rowview["REMARK"].ToString());
            txtShift.Text = _SHIFT;
            txtInputQty.Value = Convert.ToDouble(_GOODQTY);

            if (LoginInfo.CFG_AREA_ID == "E6")  // 2공장
            {
                GetSampleLoss(_EQPTID);
            }

            //setEnabled(_LOTID);
        }
        #endregion

        #region 불량/품질/자재
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
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("RESNPOSITION", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["LOTID"] = rowview["LOTID"].ToString();
                Indata["RESNPOSITION"] = null;
                IndataTable.Rows.Add(Indata);

                dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPREASONCOLLECT_ELEC", "INDATA", "RSLTDT", IndataTable);

                dgDefect.ItemsSource = DataTableConverter.Convert(dtMain);

                // 양품량/Loss량 설정
                decimal sum = SumDefectQty();
                _LOSSQTY = string.Format("{0:F2}", sum.ToString());
                _GOODQTY = string.Format("{0:F2}", Convert.ToString(Convert.ToDouble(_INPUTQTY) - Convert.ToDouble(_LOSSQTY)));

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
            //inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow inDataRow = null;

            inDataRow = inDataTable.NewRow();
            inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
            inDataRow["EQPTID"] = _EQPTID;
            //inDataRow["LOTID"] = _LOTID;
            inDataRow["USERID"] = LoginInfo.USERID;
            inDataTable.Rows.Add(inDataRow);
            #endregion

            #region Defect Lot
            DataTable inDefectLot = _DefectLot(LotID);
            #endregion

            new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT", "INDATA,INRESN", null, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.AlertByBiz("BR_QCA_REG_WIPREASONCOLLECT", ex.Message, ex.ToString());
                    gDefectChangeFlag = false;
                    return;
                }
                Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                dgDefect.EndEdit(true);
                GetDefect(_LOTID);
                gDefectChangeFlag = true;
            }, inDataSet);
        }
        private DataTable _DefectLot(string LotID)
        {
            DataTable dt = ((DataView)dgDefect.ItemsSource).Table;

            DataTable IndataTable = inDataSet.Tables.Add("INRESN");
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("ACTID", typeof(string));
            IndataTable.Columns.Add("RESNCODE", typeof(string));
            IndataTable.Columns.Add("RESNQTY", typeof(decimal));

            foreach (DataRow _iRow in dt.Rows)
            {
                _iRow["LOTID"] = LotID;
                _iRow["ACTID"] = _iRow["ACTID"];
                _iRow["RESNCODE"] = _iRow["RESNCODE"];
                _iRow["RESNQTY"] = _iRow["RESNQTY"].ToString().Equals("") ? 0 : _iRow["RESNQTY"];
                IndataTable.ImportRow(_iRow);
            }
            return IndataTable;
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
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["LOTID"] = rowview["LOTID"].ToString();
                IndataTable.Rows.Add(Indata);

                dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPDATACOLLECT_ELEC", "INDATA", "RSLTDT", IndataTable);

                dgQuality.ItemsSource = DataTableConverter.Convert(dtMain);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetQuality(string LotID)
        {
            if (dgQuality.Rows.Count < 0)
            {
                return;
            }

            _EDCDataSet = new DataSet();

            DataTable inDataTable = _EDCDataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow inDataRow = null;

            inDataRow = inDataTable.NewRow();
            inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            inDataRow["EQPTID"] = _EQPTID;
            inDataRow["LOTID"] = _LOTID;
            inDataRow["USERID"] = LoginInfo.USERID;
            inDataTable.Rows.Add(inDataRow);

            DataTable inEDCLot = _EDCLot();

            new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPDATACOLLECT", "INDATA,INCLCT", null, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.AlertByBiz("BR_QCA_REG_WIPDATACOLLECT", ex.Message, ex.ToString());
                    return;
                }
                Util.AlertInfo("SFU1998");  //품질 정보가 저장되었습니다.
                dgQuality.EndEdit(true);
                GetQuality(_LOTID);

            }, _EDCDataSet);
        }
        private DataTable _EDCLot()
        {
            DataTable dt = ((DataView)dgQuality.ItemsSource).Table;

            DataTable IndataTable = _EDCDataSet.Tables.Add("INCLCT");
            IndataTable.Columns.Add("CLCTITEM", typeof(string));
            IndataTable.Columns.Add("CLCTVAL01", typeof(string));

            foreach (DataRow _iRow in dt.Rows)
            {
                _iRow["CLCTITEM"] = _iRow["CLCTITEM"];
                _iRow["CLCTVAL01"] = _iRow["CLCTVAL01"].ToString().Equals("") ? 0 : _iRow["CLCTVAL01"];
                IndataTable.ImportRow(_iRow);
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
            if (!gDefectChangeFlag)
            {
                Util.Alert("SFU1258");  //불량정보가 변경 후 저장되지 않았습니다.
                return false;
            }
            return true;
        }
        private void ConfirmProcess()
        {

#if false
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("SRCTYPE", typeof(string));
            IndataTable.Columns.Add("IFMODE", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("INPUTQTY", typeof(decimal));
            IndataTable.Columns.Add("OUTPUTQTY", typeof(decimal));
            IndataTable.Columns.Add("RESNQTY", typeof(decimal));
            IndataTable.Columns.Add("PROD_VER_CODE", typeof(string));
            IndataTable.Columns.Add("SHIFT", typeof(string));
            IndataTable.Columns.Add("WIPNOTE", typeof(string));
            IndataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            IndataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
            IndataTable.Columns.Add("USERID", typeof(string));
        
            DataRow Indata = IndataTable.NewRow();
            Indata["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            Indata["IFMODE"] = IFMODE.IFMODE_OFF;
            Indata["EQPTID"] = _EQPTID;
            Indata["LOTID"] = _LOTID;
            Indata["INPUTQTY"] = Util.NVC_Decimal(txtInputQty.Value);
            Indata["OUTPUTQTY"] = Util.NVC_Decimal(txtGoodQuntity.Text);
            Indata["RESNQTY"] = Util.NVC_Decimal(txtLossQuntity.Text);
            Indata["PROD_VER_CODE"] = txtVersion.Text;
            Indata["SHIFT"] = txtShift.Text;
            Indata["WIPNOTE"] = txtIssue.Text;
            Indata["WRK_USER_NAME"] = txtWorkPerson.Text;
            Indata["WIPDTTM_ED"] = Util.StringToDateTime(dtpEndDateTime.DateTime.Value.ToString("yyyy-MM-dd HH:mm"));
            Indata["USERID"] = LoginInfo.USERID;
            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("BR_PRD_REG_END_LOT_MX", "INDATA", null, IndataTable, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }
                Util.AlertInfo("SFU1275"); //정상처리되었습니다.

                ClearControls();
                GetWorkOrder();
                GetProductLot();
            });      
#else
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
                        Util.AlertByBiz("BR_PRD_REG_END_LOT_MX", ex.Message, ex.ToString());
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

#endif

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
            dgLotInfo.ItemsSource = null;
            dgDefect.ItemsSource = null;
            dgQuality.ItemsSource = null;
            dgMaterial.ItemsSource = null;
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
                if (_WIPSTAT == "PROC")
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
                else if (_WIPSTAT == "EQPT_END" || _WIPSTAT == "END")
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
            if (_WIPSTAT == "END")
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
#if true
                Util.AlertInfo("SFU1315");  //BizRuel 없음. Process 확인
                return;
#else
                DataTable zIndataTable = new DataTable();
                zIndataTable.Columns.Add("EQPTID", typeof(string));
                zIndataTable.Columns.Add("FLAG", typeof(string));

                DataRow zIndata = zIndataTable.NewRow();
                zIndata["EQPTID"] = _EQPTID;
                zIndata["FLAG"] = strFlag;

                zIndataTable.Rows.Add(zIndata);
                // MUST_BIZ_APPLY
                dtMain = new ClientProxy().ExecuteServiceSync("U_MIX_EQPT_CLEAN", "INDATA", "RSLTDT", zIndataTable);
#endif
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
            if (txtVersion.Text.Trim() == "")
            {
                Util.MessageValidation("SFU1218");  //Version 정보를 입력 하세요.
                return false;
            }
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

        private void GetEDC()
        {
            DataTable _Slurry = new DataTable();
            _Slurry.Columns.Add("RESNCODE", typeof(string));
            _Slurry.Columns.Add("RESNNAME", typeof(string));
            _Slurry.Columns.Add("RESNQTY", typeof(string));

            DataRow _dRow = null;
            for (int i = 0; i < 5; i++)
            {
                _dRow = _Slurry.NewRow();
                _dRow["RESNCODE"] = "RESNCODE_" + (i + 1).ToString();
                _dRow["RESNNAME"] = "RESNNAME_" + (i + 1).ToString();
                _dRow["RESNQTY"] = 0;
                _Slurry.Rows.Add(_dRow);
            }
            // 
            dgDefect.ItemsSource = DataTableConverter.Convert(_Slurry);
        }

    }

}
