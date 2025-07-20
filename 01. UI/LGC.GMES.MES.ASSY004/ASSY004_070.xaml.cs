/*****************************************
 Created Date : 2020.05.08
      Creator : 
   Decription : 재와인딩 공정진척
------------------------------------------
 [Change History]
 
******************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using System.Globalization;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF;
using C1.WPF.DataGrid;
using System.Linq;
using System.Threading;

namespace LGC.GMES.MES.ASSY004
{
    public partial class ASSY004_070 : UserControl, IWorkArea
    {
        #region Initialize
        private Util _Util = new Util();

        private GridLength ExpandFrame;

        private string _LDR_LOT_IDENT_BAS_CODE;
        private string _UNLDR_LOT_IDENT_BAS_CODE;
        private string _IS_POSTING_HOLD;

        private string _CURRENT_OUTLOT;
        string _WIPDTTM_ST = string.Empty;
        string _WIPDTTM_ED = string.Empty;

        private const string ITEM_CODE_LEN_EXCEED = "LENGTH_EXCEED";
        DataSet inDataSet;     // 완성LOT 불량
        DataSet inputDataSet;  // 투입LOT 불량

        private DataTable _CURRENT_LOTINFO = new DataTable();
        decimal exceedLengthQty;

        private bool isChangeRemark = false;
        private bool isChangeWipReason = false;

        public ASSY004_070()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation { get; set; }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;

            InitComboBox();
            //InitDataTable();

            ApplyPermissions();
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            //foreach (Button button in Util.FindVisualChildren<Button>(mainGrid))
            //    listAuth.Add(button);

            //Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitComboBox()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);

            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: new string[] { Process.ASSY_REWINDER });
        }

        //private void InitDataTable()
        //{
        //    _CURRENT_LOTINFO.Columns.Add("LOTID", typeof(string));
        //    _CURRENT_LOTINFO.Columns.Add("WIPSEQ", typeof(Int32));
        //    _CURRENT_LOTINFO.Columns.Add("WIPSTAT", typeof(string));
        //    _CURRENT_LOTINFO.Columns.Add("CSTID", typeof(string));
        //    _CURRENT_LOTINFO.Columns.Add("PRODID", typeof(string));
        //}

        private void InitAllClearControl()
        {
            Util.gridClear(dgOutLot);
            Util.gridClear(dgProductLot);
            this.InitClearControl();
        }

        private void InitClearControl()
        {
            Util.gridClear(dgOutLotInfo);
            Util.gridClear(dgInputLotInfo);
            Util.gridClear(dgWipReason);
            Util.gridClear(dgRemark);
            txtInputQty.Value = 0;
            txtParentQty.Value = 0;

            txtStartDateTime.Text = string.Empty;
            txtEndDateTime.Text = string.Empty;
            txtWorkDate.Text = string.Empty;
            txtElec.Text = string.Empty;

            _CURRENT_OUTLOT = string.Empty;

            isChangeWipReason = false;
            isChangeRemark = false;
            

            if (!string.IsNullOrEmpty(txtShiftEndTime.Text) && txtShiftEndTime.Text.Length == 19)
            {
                // 현재시간보다 근무종료 시간이 작으면 클리어
                string sShiftTime = System.DateTime.Now.ToString("yyyy-MM-dd") + " " + txtShiftEndTime.Text.Substring(txtShiftEndTime.Text.IndexOf(' ') + 1, 8);

                if (Convert.ToDateTime(sShiftTime) < System.DateTime.Now)
                {
                    txtShift.Text = string.Empty;
                    txtShift.Tag = string.Empty;
                    txtWorker.Text = string.Empty;
                    txtWorker.Tag = string.Empty;
                    txtShiftDateTime.Text = string.Empty;
                    txtShiftStartTime.Text = string.Empty;
                    txtShiftEndTime.Text = string.Empty;
                }
            }
        }
        #endregion

        #region Event Definition
        private void cboEquipment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            DataRowView drv = e.NewValue as DataRowView;

            if (drv != null)
            {
                InitAllClearControl();

                GetOutLot();                                               // 생산LOT 조회
                SetLotAutoSelected();                                       // LOT자동선택
                GetWrkShftUser();                                           // SHIFT_CODE, SHIFT_USER 자동 SET
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return;
            }

            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            InitAllClearControl();
             
            GetOutLot();                                               // 생산LOT 조회
            SetLotAutoSelected();                                       // LOT자동선택
            GetWrkShftUser();                                           // SHIFT_CODE, SHIFT_USER 자동 SET
        }

        private void RefreshData(bool isConfirm = false)
        {
            InitAllClearControl();
            GetOutLot();                                               // 생산LOT 조회
            SetLotAutoSelected();                                       // LOT자동선택
            GetWrkShftUser();                                           // SHIFT_CODE, SHIFT_USER 자동 SET
        }

        private void dgOutLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            dgOutLot.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            {
                //_CURRENT_OUTLOT = DataTableConverter.GetValue(rb.DataContext, "LOTID").ToString();
                //_WIPDTTM_ST = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "WIPDTTM_ST"));
                //_WIPDTTM_ED = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "WIPDTTM_ED"));

                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                GetLot(rb.DataContext);

                //GetProductLot(_CURRENT_OUTLOT);
                //GetLotInfo(rb.DataContext);
                //GetInputLotInfo(rb.DataContext);
                //GetDefectList(rb.DataContext);
                //GetRemark(rb.DataContext);
                //SetCalDate();

                //txtStartDateTime.Text = Convert.ToDateTime(_WIPDTTM_ST).ToString("yyyy-MM-dd HH:mm");
                //if (string.IsNullOrEmpty(_WIPDTTM_ED))
                //    txtEndDateTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                //else
                //    txtEndDateTime.Text = Convert.ToDateTime(_WIPDTTM_ED).ToString("yyyy-MM-dd HH:mm");
            }                
        }
        private void GetLot(object SelectedItem)
        {
            try
            {
                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                    return;

                _CURRENT_OUTLOT = DataTableConverter.GetValue(SelectedItem, "LOTID").ToString();
                _WIPDTTM_ST = Util.NVC(DataTableConverter.GetValue(SelectedItem, "WIPDTTM_ST"));
                _WIPDTTM_ED = Util.NVC(DataTableConverter.GetValue(SelectedItem, "WIPDTTM_ED"));

                GetProductLot(_CURRENT_OUTLOT);
                GetLotInfo(SelectedItem);
                GetInputLotInfo(SelectedItem);
                GetDefectList(SelectedItem);
                GetRemark(SelectedItem);
                SetCalDate();

                txtStartDateTime.Text = Convert.ToDateTime(_WIPDTTM_ST).ToString("yyyy-MM-dd HH:mm");
                if (string.IsNullOrEmpty(_WIPDTTM_ED))
                    txtEndDateTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                else
                    txtEndDateTime.Text = Convert.ToDateTime(_WIPDTTM_ED).ToString("yyyy-MM-dd HH:mm");

                txtElec.Text = Util.NVC(DataTableConverter.GetValue(SelectedItem, "FROM_ELEC"));
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void dgProductLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            #region CHK
            //C1DataGrid dataGrid = sender as C1DataGrid;
            //if (dataGrid != null)
            //{
            //    dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        if (e != null && e.Cell != null && e.Cell.Presenter != null)
            //        {
            //            if (e.Cell.Row.Type == DataGridRowType.Item)
            //            {
            //                if (string.Equals(e.Cell.Column.Name, "LOTID") && string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "CUT"), "1"))
            //                {
            //                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
            //                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
            //                }
            //                else
            //                {
            //                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
            //                }
            //            }
            //        }
            //    }));
            //}
            #endregion
        }
        
        private void dgProductLot_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            e.Cell.Presenter.Background = null;
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                }));
            }
        }

        private void dgLotInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;

                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (e.Cell.Column.Name.Equals("REMAIN_QTY") && !Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REMAIN_QTY")).Equals(0))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        }
                    }
                }));
            }
        }

        private void dgLotInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            e.Cell.Presenter.Background = null;
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                }));
            }
        }

        private void dgWipReason_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgWipReason.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type != DataGridRowType.Item)
                {
                    return;
                }

                if (e.Cell.Column.Name.Equals("RESNTOTQTY") | e.Cell.Column.Name.Equals("RESNCODE") || e.Cell.Column.Name.Equals("RESNNAME") || e.Cell.Column.Name.Equals("ACTNAME"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.WhiteSmoke);
                }
            }));
        }

        private void dgWipReason_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (dgInputLotInfo.Rows.Count > 0)
            {
                C1DataGrid dataGrid = sender as C1DataGrid;

                if (dataGrid != null)
                {
                    DefectChange(dataGrid, dataGrid.CurrentCell.Row.Index, dataGrid.CurrentCell.Column.Index);
                    dgOutLotInfo.Refresh(false);
                    dgInputLotInfo.Refresh(false);

                    isChangeWipReason = true;
                }
            }
        }

        private void dgRemark_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (dgInputLotInfo.Rows.Count > 0)
            {
                C1DataGrid dataGrid = sender as C1DataGrid;

                if (dataGrid != null)
                {
                    isChangeRemark = true;
                }
            }
        }

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        private void dgRemark_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                if (e.Cell.Row.Index == 0 && e.Cell.Column.Index == 1)
                {
                    Grid grid = e.Cell.Presenter.Content as Grid;

                    if (grid != null)
                    {
                        TextBox remarkText = grid.Children[0] as TextBox;

                        if (remarkText != null)
                        {
                            remarkText.LostKeyboardFocus -= OnRemarkLostKeyboardFocus;
                            remarkText.LostKeyboardFocus += OnRemarkLostKeyboardFocus;
                        }
                    }
                }
                else if (e.Cell.Row.Index > 0 && e.Cell.Column.Index == 1)
                {
                    Grid grid = e.Cell.Presenter.Content as Grid;

                    if (grid != null)
                    {
                        TextBox remarkText = grid.Children[0] as TextBox;

                        if (remarkText != null)
                        {
                            remarkText.LostKeyboardFocus -= OnRemarkChildLostKeyboardFocus;
                            remarkText.LostKeyboardFocus += OnRemarkChildLostKeyboardFocus;
                        }
                    }
                }
            }
        }

        private void OnRemarkLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (dgRemark.Rows.Count < 1)
                return;

            TextBox remarkText = sender as TextBox;

            if (remarkText != null && !string.IsNullOrEmpty(remarkText.Text))
                isChangeRemark = true;
        }

        private void OnRemarkChildLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (dgRemark.Rows.Count < 1)
                return;

            TextBox remarkText = sender as TextBox;

            if (remarkText != null && !string.IsNullOrEmpty(remarkText.Text))
                isChangeRemark = true;
        }
        #endregion

        #region User Method

        private void SetLotAutoSelected()
        {
            if (dgOutLot != null && dgOutLot.Rows.Count > 0)
            {

                //C1.WPF.DataGrid.DataGridCell currCell = dgOutLot.GetCell(0, dgOutLot.Columns["CHK"].Index);
                //if (currCell != null)
                //{
                //    dgOutLot.SelectedIndex = currCell.Row.Index;
                //    dgOutLot.CurrentCell = currCell;
                //}
                int iRow = 0;
                for (int i = 0; i < dgOutLot.GetRowCount(); i++)
                {
                    if (dgOutLot.GetRowCount() ==  1)
                    {
                        DataTableConverter.SetValue(dgOutLot.Rows[0].DataItem, "CHK", 1);
                    }
                    else
                    {
                        if (DataTableConverter.GetValue(dgOutLot.Rows[i].DataItem, "WIPSTAT").Equals("PROC"))
                        {
                            DataTableConverter.SetValue(dgOutLot.Rows[i].DataItem, "CHK", 1);
                            iRow = i;
                        }
                    }
                }
                GetLot(dgOutLot.Rows[iRow].DataItem);
            }
        }
        private bool IsValidGoodQty()
        {
            if (dgOutLotInfo.GetRowCount() > 0)
            {
                for (int i = 0; i < dgOutLotInfo.GetRowCount(); i++)
                {
                    if (Util.NVC_Decimal(DataTableConverter.GetValue(dgOutLotInfo.Rows[i + dgOutLotInfo.TopRows.Count].DataItem, "GOODQTY")) < 0)
                    {
                        Util.MessageValidation("SFU5129");  //양품량이 0보다 작습니다.
                        return false;
                    }
                }
            }
            return true;
        }

        private bool ValidShift()
        {
            if (string.IsNullOrEmpty(txtShift.Text.Trim()))
            {
                Util.MessageValidation("SFU1845");  //작업조를 입력하세요.
                return false;
            }

            return true;
        }

        private bool ValidOperator()
        {
            if (string.IsNullOrEmpty(txtWorker.Text.Trim()))
            {
                Util.MessageValidation("SFU1843");  //작업자를 입력 해 주세요.
                return false;
            }

            return true;
        }

        private Dictionary<int, string> getSamplingLabelInfo(string sLotID)
        {
            return new Dictionary<int, string> { { 1, string.Empty } };
        }

        private void GetLotInfo(object SelectedItem)
        {
            try
            {
                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                    return;

                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = Process.ASSY_REWINDER;
                dr["LOTID"] = Util.NVC(rowview["LOTID"]);
                RQSTDT.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_OUT_LOT", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgOutLotInfo, dt, FrameOperation);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void GetInputLotInfo(object SelectedItem)
        {
            try
            {
                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                    return;

                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["PROCID"] = Process.ASSY_REWINDER;
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                dr["LOTID"] = Util.NVC(rowview["LOTID"]);
                RQSTDT.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_OUT_INPUT_RW", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgInputLotInfo, dt, FrameOperation);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private bool GetSumCutDefectQty(C1DataGrid dg, int rowIdx, int colIdx)
        {
            string actId = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, "ACTID"));
            double eqptendQty = 0;
            double inputQty = 0;
            double inputQty_in = 0;
            double actSum = 0;
            double totalSum = 0;
            double rowSum = 0;

            eqptendQty = Convert.ToDouble(DataTableConverter.GetValue(dgOutLotInfo.Rows[dgOutLotInfo.TopRows.Count].DataItem, "EQPT_END_QTY"));  // INPUT_QTY
            inputQty = Convert.ToDouble(DataTableConverter.GetValue(dgOutLotInfo.Rows[dgOutLotInfo.TopRows.Count].DataItem, "INPUT_QTY"));  // INPUT_QTY
            //inputQty_in = Convert.ToDouble(DataTableConverter.GetValue(dgOutLotInfo.Rows[dgOutLotInfo.TopRows.Count].DataItem, "INPUT_QTY_O"));  // INPUT_QTY

            for (int i = dg.Columns["TAG_CONV_RATE"].Index; i < dg.Columns["COSTCENTERID"].Index; i++)
            {
                rowSum += Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, dg.Columns[i].Name));
                actSum += SumDefectQty(dg, i, dg.Columns[i].Name, actId);
            }

            totalSum = actSum;
            if (!string.Equals(actId, "DEFECT_LOT"))
                totalSum += Convert.ToDouble(DataTableConverter.GetValue(dgOutLotInfo.Rows[dgOutLotInfo.TopRows.Count].DataItem, "DEFECT_QTY"));
            if (!string.Equals(actId, "LOSS_LOT"))
                totalSum += Convert.ToDouble(DataTableConverter.GetValue(dgOutLotInfo.Rows[dgOutLotInfo.TopRows.Count].DataItem, "LOSS_QTY"));
            if (!string.Equals(actId, "CHARGE_PROD_LOT"))
                totalSum += Convert.ToDouble(DataTableConverter.GetValue(dgOutLotInfo.Rows[dgOutLotInfo.TopRows.Count].DataItem, "CHARGE_QTY"));

            DataTableConverter.SetValue(dgOutLotInfo.Rows[dgOutLotInfo.TopRows.Count].DataItem, string.Equals(actId, "DEFECT_LOT") ? "DEFECT_QTY" : string.Equals(actId, "LOSS_LOT") ? "LOSS_QTY" : "CHARGE_QTY", actSum);
            DataTableConverter.SetValue(dgOutLotInfo.Rows[dgOutLotInfo.TopRows.Count].DataItem, "DEFECT_SUM", totalSum);
            DataTableConverter.SetValue(dgOutLotInfo.Rows[dgOutLotInfo.TopRows.Count].DataItem, "REMAIN_QTY", (inputQty) - (eqptendQty + totalSum));
            DataTableConverter.SetValue(dgOutLotInfo.Rows[dgOutLotInfo.TopRows.Count].DataItem, "GOODQTY", (inputQty) - totalSum);

            DataTableConverter.SetValue(dg.Rows[rowIdx].DataItem, "RESNTOTQTY", rowSum);

            return true;
        }

        private bool GetSumDefectQty(C1DataGrid dg, int rowIdx, int colIdx)
        {
            string subLotId = dg.Columns[colIdx].Name;
            string actId = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, "ACTID"));
            double inputqty = 0;
            double eqptendqty = 0;
            double actSum = 0;
            double rowSum = 0;
            double totalSum = 0;
            int iLotIdx = Util.gridFindDataRow(ref dgInputLotInfo, "INPUT_LOTID", subLotId, false);

            inputqty = Convert.ToDouble(DataTableConverter.GetValue(dgInputLotInfo.Rows[iLotIdx].DataItem, "INPUT_QTY"));

            eqptendqty = Convert.ToDouble(DataTableConverter.GetValue(dgInputLotInfo.Rows[iLotIdx].DataItem, "EQPT_END_QTY"));

            for (int i = dg.Columns["TAG_CONV_RATE"].Index; i < dg.Columns["COSTCENTERID"].Index; i ++)
                rowSum += Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, dg.Columns[i].Name));

            actSum = SumDefectQty(dg, colIdx, subLotId, actId);

            totalSum = actSum;
            if (!string.Equals(actId, "DEFECT_LOT"))
                totalSum += Convert.ToDouble(DataTableConverter.GetValue(dgInputLotInfo.Rows[iLotIdx].DataItem, "DEFECT_QTY"));
            if (!string.Equals(actId, "LOSS_LOT"))
                totalSum += Convert.ToDouble(DataTableConverter.GetValue(dgInputLotInfo.Rows[iLotIdx].DataItem, "LOSS_QTY"));
            if (!string.Equals(actId, "CHARGE_PROD_LOT"))
                totalSum += Convert.ToDouble(DataTableConverter.GetValue(dgInputLotInfo.Rows[iLotIdx].DataItem, "CHARGE_QTY"));


            DataTableConverter.SetValue(dgInputLotInfo.Rows[iLotIdx].DataItem, string.Equals(actId, "DEFECT_LOT") ? "DEFECT_QTY" : string.Equals(actId, "LOSS_LOT") ? "LOSS_QTY" : "CHARGE_QTY", actSum);
            DataTableConverter.SetValue(dgInputLotInfo.Rows[iLotIdx].DataItem, "DEFECT_SUM", totalSum);
            DataTableConverter.SetValue(dgInputLotInfo.Rows[iLotIdx].DataItem, "GOODQTY", eqptendqty - totalSum);

            DataTableConverter.SetValue(dg.Rows[rowIdx].DataItem, "RESNTOTQTY", rowSum);

            SetExceedLength(dgInputLotInfo, rowIdx, subLotId);

            return true;
        }

        private void DefectChange(C1DataGrid dg, int iRow, int iCol)
        {
            GetSumDefectQty(dg, iRow, iCol);
            GetSumCutDefectQty(dg, iRow, iCol);
        }

        private void SetExceedLength(C1DataGrid dg, int rowIdx, string subLotId)
        {
            decimal inputqty = 0;
            decimal totalLengthQty = 0;
            int iLotIdx = Util.gridFindDataRow(ref dgInputLotInfo, "INPUT_LOTID", subLotId, false);
            inputqty = Convert.ToDecimal(DataTableConverter.GetValue(dg.Rows[iLotIdx].DataItem, "INPUT_QTY_O"));

            DataTable dt = ((DataView)dgWipReason.ItemsSource).Table;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (string.Equals(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "PRCS_ITEM_CODE"), ITEM_CODE_LEN_EXCEED))
                {
                    exceedLengthQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, dgWipReason.Columns[subLotId].Name));
                    totalLengthQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "RESNTOTQTY"));
                    break;
                }
            }

            if (exceedLengthQty >= 0)
            {
                DataTableConverter.SetValue(dg.Rows[iLotIdx].DataItem, "INPUT_QTY", inputqty + exceedLengthQty);

                decimal oInputqty = 0;
                oInputqty = Util.NVC_Decimal(DataTableConverter.GetValue(dgOutLotInfo.Rows[dgOutLotInfo.TopRows.Count].DataItem, "INPUT_QTY_O"));
                DataTableConverter.SetValue(dgOutLotInfo.Rows[dgOutLotInfo.TopRows.Count].DataItem, "INPUT_QTY", oInputqty + totalLengthQty);
            }
        }

        #endregion

        #region DA Biz Call

        private void SetCalDate()
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CALDATE_EQPTID", "RQSTDT", "OUTDATA", RQSTDT);

                if (result.Rows.Count > 0 && !string.IsNullOrEmpty(Util.NVC(result.Rows[0]["CALDATE"])))
                {
                    txtWorkDate.Text = Convert.ToDateTime(result.Rows[0]["CALDATE"]).ToString("yyyy-MM-dd");
                    txtWorkDate.Tag = Convert.ToDateTime(result.Rows[0]["CALDATE"]).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    txtWorkDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    txtWorkDate.Tag = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // OUT LOT 조회
        private void GetOutLot(DataRowView drv = null)
        {
            try
            {

                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PROCID"] = Process.ASSY_REWINDER;
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_OUT_RW", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgOutLot, dtResult, FrameOperation);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }
        private void GetProductLot(string OutLot)
        {
            try
            {

                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PROCID"] = Process.ASSY_REWINDER;
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                dr["LOTID"] = OutLot;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_INPUT_RW", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgProductLot, dtResult, FrameOperation);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // 작업자 정보 SET
        private void GetWrkShftUser()
        {
            DataTable RQSTDT = new DataTable("RQSTDT");
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("SHOPID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PROCID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
            dr["PROCID"] = Process.ASSY_REWINDER;
            RQSTDT.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "RQSTDT", "RSLTDT", RQSTDT, (result, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                        {
                            txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                        }
                        else
                        {
                            txtShiftStartTime.Text = string.Empty;
                        }

                        if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                        {
                            txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                        }
                        else
                        {
                            txtShiftEndTime.Text = string.Empty;
                        }

                        if (!string.IsNullOrEmpty(txtShiftStartTime.Text) && !string.IsNullOrEmpty(txtShiftEndTime.Text))
                        {
                            txtShiftDateTime.Text = txtShiftStartTime.Text + " ~ " + txtShiftEndTime.Text;
                        }
                        else
                        {
                            txtShiftDateTime.Text = string.Empty;
                        }

                        if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                        {
                            txtWorker.Text = string.Empty;
                            txtWorker.Tag = string.Empty;
                        }
                        else
                        {
                            txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                            txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                        }

                        if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                        {
                            txtShift.Tag = string.Empty;
                            txtShift.Text = string.Empty;
                        }
                        else
                        {
                            txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                            txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                        }
                    }
                    else
                    {
                        txtWorker.Text = string.Empty;
                        txtWorker.Tag = string.Empty;
                        txtShift.Text = string.Empty;
                        txtShift.Tag = string.Empty;
                        txtShiftStartTime.Text = string.Empty;
                        txtShiftEndTime.Text = string.Empty;
                        txtShiftDateTime.Text = string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }
        
        private void GetDefectList(object SelectedItem)
        {
            try
            {
                string childLotId;
                string WipSeq; 

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROCID"] = Process.ASSY_REWINDER;
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_DEFECT", "INDATA", "OUTDATA", IndataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        DataTable totalWipReason = searchResult.DataSet.Tables["OUTDATA"];
                        DataTable dsWipReason = totalWipReason.DefaultView.ToTable(false, "RESNCODE", "ACTID", "ACTNAME", "RESNNAME", "PRCS_ITEM_CODE", "RSLT_EXCL_FLAG", "RESNTOTQTY", "PARTNAME", "TAG_CONV_RATE");

                        Util.GridSetData(dgWipReason, dsWipReason, FrameOperation);

                        if (dgWipReason.Rows.Count > 0)
                        {
                            if (dgInputLotInfo.Rows.Count > 0)
                            {
                                for (int i = dgWipReason.Columns.Count - 1; i > dgWipReason.Columns["TAG_CONV_RATE"].Index; i--)
                                    dgWipReason.Columns.RemoveAt(i);

                                if (dgInputLotInfo.Rows.Count > 0)
                                {
                                    double defectQty = 0;
                                    double lossQty = 0;
                                    double chargeQty = 0;

                                    for (int i = dgInputLotInfo.TopRows.Count; i < (dgInputLotInfo.Rows.Count - dgInputLotInfo.BottomRows.Count); i++)
                                    {
                                        childLotId = Util.NVC(DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "INPUT_LOTID"));
                                        WipSeq = Util.NVC(DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "WIPSEQ"));

                                        if (dgWipReason.Rows.Count == 0)
                                            continue;

                                        Util.SetGridColumnNumeric(dgWipReason, childLotId, null, childLotId,
                                            true, true, false, false, 50, HorizontalAlignment.Right, Visibility.Visible, "F0", false, false);

                                        // WipReasoncollect (투입LOT, 완성LOT)
                                        //DataTable dt = GetDefectDataByLot(_CURRENT_OUTLOT);

                                        // 투입LOT Defect (TB_SFC_WIP_RSN_PROG_HIST)
                                        DataTable dt = GetDefectDataByInputLot(childLotId, WipSeq);

                                        defectQty = 0;
                                        lossQty = 0;
                                        chargeQty = 0;

                                        if (dt != null)
                                            for (int j = 0; j < dt.Rows.Count; j++)
                                                BindingDataGrid(dgWipReason, j, dgWipReason.Columns.Count, dt.Rows[j]["RESNQTY"]);

                                        DataTable distinctDt = DataTableConverter.Convert(dgWipReason.ItemsSource).DefaultView.ToTable(true, "ACTID");
                                        foreach (DataRow _row in distinctDt.Rows)
                                        {
                                            if (string.Equals(_row["ACTID"], "DEFECT_LOT"))
                                                defectQty += SumDefectQty(dgWipReason, i, childLotId, Util.NVC(_row["ACTID"]));
                                            else if (string.Equals(_row["ACTID"], "LOSS_LOT"))
                                                lossQty += SumDefectQty(dgWipReason, i, childLotId, Util.NVC(_row["ACTID"]));
                                            else if (string.Equals(_row["ACTID"], "CHARGE_PROD_LOT"))
                                                chargeQty += SumDefectQty(dgWipReason, i, childLotId, Util.NVC(_row["ACTID"]));
                                        }

                                        DataTableConverter.SetValue(dgInputLotInfo.Rows[i].DataItem, "DEFECT_QTY", defectQty);
                                        DataTableConverter.SetValue(dgInputLotInfo.Rows[i].DataItem, "LOSS_QTY", lossQty);
                                        DataTableConverter.SetValue(dgInputLotInfo.Rows[i].DataItem, "CHARGE_QTY", chargeQty);
                                        DataTableConverter.SetValue(dgInputLotInfo.Rows[i].DataItem, "DEFECT_SUM", (defectQty + lossQty + chargeQty));
                                        DataTableConverter.SetValue(dgInputLotInfo.Rows[i].DataItem, "GOODQTY", Convert.ToDouble(DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "EQPT_END_QTY")) - Convert.ToDouble(DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "DEFECT_SUM")));
                                    }
                                }

                                Util.SetGridColumnText(dgWipReason, "COSTCENTERID", null, "COSTCENTERID", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);
                                Util.SetGridColumnText(dgWipReason, "COSTCENTER", null, "COSTCENTER", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);

                                double rowSum = 0;
                                for (int i = 0; i < dgWipReason.Rows.Count; i++)
                                {
                                    rowSum = 0;
                                    for (int j = dgWipReason.Columns["TAG_CONV_RATE"].Index; j < dgWipReason.Columns["COSTCENTERID"].Index; j ++)
                                        rowSum += Convert.ToDouble(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, dgWipReason.Columns[j].Name));

                                    DataTableConverter.SetValue(dgWipReason.Rows[i].DataItem, "RESNTOTQTY", rowSum);
                                }


                                Util.GridSetData(dgWipReason, DataTableConverter.Convert(dgWipReason.ItemsSource), FrameOperation, true);
                                //dgWipReason.Refresh(false);
                            }
                        }
                    }
                    catch (Exception ex) { Util.MessageException(ex); }
                });
            }
            catch (Exception ex) { Util.MessageException(ex); }
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
                IndataTable.Columns.Add("CODE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["LOTID"] = LotId;
                Indata["RESNPOSITION"] = null;

                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ACTIVITYREASON_ELEC", "INDATA", "RSLTDT", IndataTable);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private DataTable GetDefectDataByInputLot(string LotId, string WipSeq)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["LOTID"] = LotId;
                Indata["WIPSEQ"] = WipSeq;

                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_WIPREASON_BY_LOTID", "INDATA", "RSLTDT", IndataTable);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private double SumDefectQty(C1DataGrid dg, int index, string lotId, string actId)
        {
            double sum = 0;

            if (dg.Rows.Count > 0)
                for (int i = 0; i < dg.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, lotId))))
                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                            if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "ACTID").ToString().Equals(actId))
                                sum += (Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[i].DataItem, lotId)));

            return sum;
        }
        private void BindingDataGrid(C1DataGrid dg, int iRow, int iCol, object sValue)
        {
            try
            {
                if (dg.ItemsSource == null)
                {
                    return;
                }
                else
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);

                    if (dt.Columns.Count < dg.Columns.Count)
                        for (int i = dt.Columns.Count; i < dg.Columns.Count; i++)
                            dt.Columns.Add(dg.Columns[i].Name);

                    if (sValue.Equals("") || sValue.Equals(null))
                        sValue = 0;

                    dt.Rows[iRow][iCol - 1] = sValue;

                    dg.BeginEdit();
                    Util.GridSetData(dg, dt, FrameOperation, false);
                    dg.EndEdit();
                }
            }
            catch { }
        }

        private void GetRemark(object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
                return;

            Util.gridClear(dgRemark);
            DataTable dt = DataTableConverter.Convert(dgInputLotInfo.ItemsSource);
            DataTable dtRemark = new DataTable();

            dtRemark.Columns.Add("LOTID", typeof(String));
            dtRemark.Columns.Add("REMARK", typeof(String));
            DataRow inDataRow = null;
            inDataRow = dtRemark.NewRow();
            inDataRow["LOTID"] = ObjectDic.Instance.GetObjectName("공통특이사항");

            if (dt.Rows.Count > 0)
            {
                string[] sWipNote = GetRemarkData(Util.NVC(dt.Rows[0]["INPUT_LOTID"])).Split('|');
                if (sWipNote.Length > 1)
                    inDataRow["REMARK"] = sWipNote[1];
            }
            dtRemark.Rows.Add(inDataRow);

            foreach (DataRow _row in dt.Rows)
            {
                inDataRow = dtRemark.NewRow();
                inDataRow["LOTID"] = Util.NVC(_row["INPUT_LOTID"]);
                inDataRow["REMARK"] = GetRemarkData(Util.NVC(_row["INPUT_LOTID"])).Split('|')[0];
                dtRemark.Rows.Add(inDataRow);
            }
            Util.GridSetData(dgRemark, dtRemark, FrameOperation);
        }

        private string GetRemarkData(string sLotID)
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["LOTID"] = sLotID;
            inTable.Rows.Add(indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPHISTORYATTR_WIPNOTE", "INDATA", "RSLTDT", inTable);
            if (dt.Rows.Count > 0)
            {
                return Util.NVC(dt.Rows[0]["WIP_NOTE"]);
            }
            else
            {
                return "";
            }
        }


        private DataTable GetPrintCount(string sLotID, string sProcID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                Indata["PROCID"] = sProcID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_LOT_LABEL_COUNT", "INDATA", "RSLTDT", IndataTable);

                return result;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        #endregion

        #region BR Biz Call
        private void SaveWIPHistory()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("LANE_PTN_QTY", typeof(decimal));
            inDataTable.Columns.Add("LANE_QTY", typeof(decimal));
            inDataTable.Columns.Add("PROD_QTY", typeof(decimal));
            inDataTable.Columns.Add("SRS1QTY", typeof(decimal));
            inDataTable.Columns.Add("SRS2QTY", typeof(decimal));
            inDataTable.Columns.Add("SRS3QTY", typeof(decimal));
            inDataTable.Columns.Add("PROTECT_FILM_TYPE_CODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("OUT_CSTID", typeof(string));

            foreach (DataRow dr in _CURRENT_LOTINFO.Rows)
            {
                DataRow inLotDetailDataRow = null;
                inLotDetailDataRow = inDataTable.NewRow();
                inLotDetailDataRow["LOTID"] = Util.NVC(dr["LOTID"]);
                //inLotDetailDataRow["PROD_VER_CODE"] = txtVersion != null ? Util.NVC(txtVersion.Text) : null;
                inLotDetailDataRow["SHIFT"] = Util.NVC(txtShift.Tag);
                inLotDetailDataRow["WRK_USER_NAME"] = Util.NVC(txtWorker.Text);
                inLotDetailDataRow["WRK_USERID"] = Util.NVC(txtWorker.Tag);
                inLotDetailDataRow["LANE_PTN_QTY"] = 1;
                //inLotDetailDataRow["LANE_QTY"] = Util.NVC_Decimal(txtLaneQty.Value);
                inLotDetailDataRow["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(inLotDetailDataRow);
            }

            new ClientProxy().ExecuteService("BR_ACT_REG_SAVE_LOT", "INDATA", null, inDataTable, (result, resultEx) =>
            {
                try
                {
                    if (resultEx != null)
                    {
                        Util.MessageException(resultEx);
                        return;
                    }
                    int iRow = new Util().GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    //if (iRow >= 0)
                    //    DataTableConverter.SetValue(dgProductLot.Rows[iRow].DataItem, "PROD_VER_CODE", Util.NVC(txtVersion.Text));
                }
                catch (Exception ex) { Util.MessageException(ex); }
            });
        }

        // 불량/Loss/물청 저장
        private void SaveDefect(C1DataGrid dg)
        {
            if (dg.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1578");  //불량 항목이 없습니다.
                return;
            }

            #region SAVE DEFECT
            inDataSet = new DataSet();

            DataTable inDataTable = inDataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            DataRow inDataRow = null;

            inDataRow = inDataTable.NewRow();
            inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
            inDataRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
            inDataRow["USERID"] = LoginInfo.USERID;
            inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            inDataRow["PROCID"] = Process.ASSY_REWINDER;
            inDataTable.Rows.Add(inDataRow);

            DataTable inDefectLot = dtDataCollectOfChildDefect(dg);
            #endregion
            try
            {
                new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "IINDATA,INRESN", null, inDataSet);
                dg.EndEdit(true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void SaveInputDefect(C1DataGrid dg)
        {
            try
            {
                inputDataSet = new DataSet();
                DataTable inData = inputDataSet.Tables.Add("INDATA");
                inData.Columns.Add("WIPSEQ", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));

                DataRow inDataRow = null;
                inDataRow = inData.NewRow();
                inDataRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgInputLotInfo.Rows[dgInputLotInfo.TopRows.Count].DataItem, "WIPSEQ"));
                inDataRow["PROCID"] = Process.ASSY_REWINDER;
                inDataRow["USERID"] = LoginInfo.USERID;
                inData.Rows.Add(inDataRow);

                DataTable inDefectLot = dtDataCollectOfInputDefect(dg);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PROC_WIPREASON", "INDATA,IN_RESN", null, inputDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 특이사항 저장
        private void SaveWipNote()
        {
            if (dgRemark.GetRowCount() < 1)
                return;

            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIP_NOTE", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));

            DataTable dt = ((DataView)dgRemark.ItemsSource).Table;
            DataRow inData = null;
            for (int i = 1; i < dt.Rows.Count; i++)
            {
                inData = inTable.NewRow();

                inData["LOTID"] = Util.NVC(dt.Rows[i]["LOTID"]);

                if (dgRemark.Rows[0].Visibility == Visibility.Visible)
                    inData["WIP_NOTE"] = Util.NVC(dt.Rows[i]["REMARK"]) + "|" + Util.NVC(dt.Rows[0]["REMARK"]);
                else
                    inData["WIP_NOTE"] = Util.NVC(dt.Rows[i]["REMARK"]);

                inData["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(inData);
            }

            try
            {
                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_WIPHISTORY_NOTE", "INDATA", null, inTable);
                isChangeRemark = false;
            }
            catch (Exception ex) { Util.MessageException(ex); }

        }

        private DataTable dtDataCollectOfChildDefect(C1DataGrid dg)
        {
            DataTable IndataTable = inDataSet.Tables.Add("INRESN");

            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(Int32));
            IndataTable.Columns.Add("ACTID", typeof(string));
            IndataTable.Columns.Add("RESNCODE", typeof(string));
            IndataTable.Columns.Add("RESNQTY", typeof(double));
            IndataTable.Columns.Add("COST_CNTR_ID", typeof(string));

            DataTable dt = (dg.ItemsSource as DataView).Table;
            DataRow inData = null;
            //int iLotCount = 0;

            //for (int iCol = dg.Columns["TAG_CONV_RATE"].Index + 1; iCol < dg.Columns["COSTCENTERID"].Index; iCol++)
            //{
            //    string sublotid = dg.Columns[iCol].Name;

            //    foreach (DataRow _iRow in dt.Rows)
            //    {
            //        inData = IndataTable.NewRow();

            //        inData["LOTID"] = sublotid;
            //        inData["WIPSEQ"] = DataTableConverter.GetValue(dgInputLotInfo.Rows[dgInputLotInfo.TopRows.Count].DataItem, "WIPSEQ");
            //        inData["ACTID"] = _iRow["ACTID"];
            //        inData["RESNCODE"] = _iRow["RESNCODE"];
            //        inData["RESNQTY"] = _iRow[sublotid].ToString().Equals("") ? 0 : _iRow[sublotid];
            //        //inData["COST_CNTR_ID"] = _iRow["COSTCENTERID"];

            //        IndataTable.Rows.Add(inData);
            //    }
            //    iLotCount++;
            //}

            // 완성LOT
            foreach (DataRow _iRow in dt.Rows)
            {
                inData = IndataTable.NewRow();
                inData["LOTID"] = DataTableConverter.GetValue(dgOutLotInfo.Rows[dgOutLotInfo.TopRows.Count].DataItem, "LOTID");
                inData["WIPSEQ"] = DataTableConverter.GetValue(dgOutLotInfo.Rows[dgOutLotInfo.TopRows.Count].DataItem, "WIPSEQ");
                inData["ACTID"] = _iRow["ACTID"];
                inData["RESNCODE"] = _iRow["RESNCODE"];
                inData["RESNQTY"] = _iRow["RESNTOTQTY"].ToString().Equals("") ? 0 : _iRow["RESNTOTQTY"];
                IndataTable.Rows.Add(inData);
            }

            return IndataTable;
        }

        private DataTable dtDataCollectOfInputDefect(C1DataGrid dg)
        {
            DataTable IndataTable = inputDataSet.Tables.Add("INRESN");

            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("ACTID", typeof(string));
            IndataTable.Columns.Add("RESNCODE", typeof(string));
            IndataTable.Columns.Add("RESNQTY", typeof(decimal));
            IndataTable.Columns.Add("ACTDTTM", typeof(string));



            DataTable dt = (dg.ItemsSource as DataView).Table;
            DataRow inData = null;
            int iLotCount = 0;

            for (int iCol = dg.Columns["TAG_CONV_RATE"].Index + 1; iCol < dg.Columns["COSTCENTERID"].Index; iCol++)
            {
                string sublotid = dg.Columns[iCol].Name;

                foreach (DataRow _iRow in dt.Rows)
                {
                    inData = IndataTable.NewRow();

                    inData["LOTID"] = sublotid;
                    inData["ACTID"] = _iRow["ACTID"];
                    //inData["RESNSEQNO"] = 0;
                    inData["RESNCODE"] = _iRow["RESNCODE"];
                    inData["RESNQTY"] = _iRow[sublotid].ToString().Equals("") ? 0 : _iRow[sublotid];
                    inData["ACTDTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    IndataTable.Rows.Add(inData);
                }
                iLotCount++;
            }
            return IndataTable;
        }

        private bool ValidEnd()
        {
            if (isChangeWipReason)
            {
                Util.MessageValidation("SFU2900");  //불량/Loss/물청 정보를 저장하세요.
                return false;
            }

            if (isChangeRemark)
            {
                Util.MessageValidation("SFU2977");  //특이사항 정보를 저장하세요.
                return false;
            }

            return true;
        }

        #endregion

        #region Button Event
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            if (CommonVerify.HasDataGridRow(dgOutLot))
            {
                DataTable dt = ((DataView)dgOutLot.ItemsSource).Table;
                var queryEdit = (from t in dt.AsEnumerable()
                                 where t.Field<string>("WIPSTAT") == "PROC"
                                 select t).ToList();

                if (queryEdit.Any())
                {
                    Util.MessageValidation("SFU1917");   // 진행중인 LOT이 있습니다.
                    return;
                }
            }

            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            dicParam.Add("EQSGID", Util.NVC(cboEquipmentSegment.SelectedValue));
            dicParam.Add("EQPTID", Util.NVC(cboEquipment.SelectedValue));
            dicParam.Add("RUNLOT", Util.NVC(_CURRENT_OUTLOT));

            ASSY004_070_LOTSTART _LotStart = new ASSY004_070_LOTSTART(dicParam);
            _LotStart.FrameOperation = FrameOperation;
            if (_LotStart != null)
            {
                _LotStart.Closed += new EventHandler(LotStart_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => _LotStart.ShowModal()));
                _LotStart.CenterOnScreen();
            }
        }

        private void LotStart_Closed(object sender, EventArgs e)
        {
            ASSY004_070_LOTSTART window = sender as ASSY004_070_LOTSTART;

            if (window.DialogResult == MessageBoxResult.OK)
                RefreshData();
        }

        private void btnCancelInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CommonVerify.HasDataGridRow(dgOutLot))
                {
                    DataTable dt = ((DataView)dgOutLot.ItemsSource).Table;
                    var queryEdit = (from t in dt.AsEnumerable()
                                     where t.Field<string>("WIPSTAT") == "EQPT_END" && t.Field<string>("LOTID") == _CURRENT_OUTLOT
                                     select t).ToList();

                    if (queryEdit.Any())
                    {
                        Util.MessageValidation("PSS9120");   // 작업중인 경우만 취소할 수 있습니다.
                        return;
                    }
                }
                //투입취소 하시겠습니까?
                Util.MessageConfirm("SFU1988", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        DataSet indataSet = new DataSet();
                        #region # IN_EQP
                        DataTable IN_EQP = indataSet.Tables.Add("INDATA");
                        IN_EQP.Columns.Add("SRCTYPE", typeof(string));
                        IN_EQP.Columns.Add("IFMODE", typeof(string));
                        IN_EQP.Columns.Add("EQPTID", typeof(string));
                        IN_EQP.Columns.Add("USERID", typeof(string));

                        DataRow row = IN_EQP.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                        row["USERID"] = LoginInfo.USERID;
                        IN_EQP.Rows.Add(row);
                        #endregion

                        #region # IN_INPUT
                        DataTable IN_INPUT = indataSet.Tables.Add("INLOT");
                        IN_INPUT.Columns.Add("LOTID", typeof(string));
                        IN_INPUT.Columns.Add("INPUT_LOTID", typeof(string));
                        for (int i = 0; i < dgProductLot.GetRowCount(); i++)
                        {
                            DataRow newRow = IN_INPUT.NewRow();
                            newRow["LOTID"] = _CURRENT_OUTLOT;
                            newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "INPUT_LOTID"));
                            IN_INPUT.Rows.Add(newRow);
                        }
                        #endregion

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RW_INPUT_CANCEL", "INDATA,INLOT", null, (Result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            RefreshData();
                        }, indataSet);
                    }
                });
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                C1DataGrid dg = dgProductLot;
                DataRow[] dr = Util.gridGetChecked(ref dg, "CHK");

                if (dr == null || dr.Length < 1 || dg == null)
                {
                    Util.MessageValidation("SFU1938");  //취소할 LOT을 선택하세요.
                    return;
                }

                if (CommonVerify.HasDataGridRow(dgProductLot))
                {
                    DataTable dt = ((DataView)dgProductLot.ItemsSource).Table;
                    var queryEdit = (from t in dt.AsEnumerable()
                                     where t.Field<string>("WIPSTAT") == "EQPT_END" && t.Field<int>("CHK") == 1
                                     select t).ToList();

                    if (queryEdit.Any())
                    {
                        Util.MessageValidation("SFU1675");   // 설비완공 Lot이 있습니다.
                        return;
                    }
                }

                //선택된 LOT을 작업 취소하시겠습니까?
                Util.MessageConfirm("SFU3151", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        DataSet indataSet = new DataSet();
                        #region # IN_EQP
                        DataTable IN_EQP = indataSet.Tables.Add("INDATA");
                        IN_EQP.Columns.Add("SRCTYPE", typeof(string));
                        IN_EQP.Columns.Add("IFMODE", typeof(string));
                        IN_EQP.Columns.Add("EQPTID", typeof(string));
                        IN_EQP.Columns.Add("USERID", typeof(string));

                        DataRow row = IN_EQP.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                        row["USERID"] = LoginInfo.USERID;
                        IN_EQP.Rows.Add(row);
                        #endregion

                        #region # IN_INPUT
                        DataTable IN_INPUT = indataSet.Tables.Add("INLOT");
                        IN_INPUT.Columns.Add("LOTID", typeof(string));
                        IN_INPUT.Columns.Add("INPUT_LOTID", typeof(string));
                        for (int i = 0; i < dgProductLot.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CHK").Equals(1) && DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "WIPSTAT").Equals("PROC"))
                            {
                                DataRow newRow = IN_INPUT.NewRow();
                                newRow["LOTID"] = _CURRENT_OUTLOT;
                                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "INPUT_LOTID"));
                                IN_INPUT.Rows.Add(newRow);
                            }
                        }
                        #endregion

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RW_CANCEL_START", "INDATA,INLOT", null, (Result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            RefreshData();
                        }, indataSet);
                    }
                });
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void btnEqptEnd_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            if (dgProductLot.Rows.Count == 0)
            {
                Util.MessageValidation("SFU4996");  //Lot을 선택하세요.
                return;
            }

            ASSY004_070_EQPTEND _EqptEnd = new ASSY004_070_EQPTEND();
            _EqptEnd.FrameOperation = FrameOperation;
            if (_EqptEnd != null)
            {
                object[] parameters = new object[10];

                parameters[0] = DataTableConverter.Convert(dgOutLotInfo.ItemsSource);       //완성Lot
                parameters[1] = DataTableConverter.Convert(dgInputLotInfo.ItemsSource);     //투입Lot
                parameters[2] = Process.ASSY_REWINDER;
                parameters[3] = Util.NVC(cboEquipment.SelectedValue);
                parameters[4] = Util.NVC(cboEquipmentSegment.SelectedValue);
                parameters[5] = _CURRENT_OUTLOT;
                parameters[6] = _WIPDTTM_ST;

                C1WindowExtension.SetParameters(_EqptEnd, parameters);

                _EqptEnd.Closed += new EventHandler(OnCloseEqptEnd);
                this.Dispatcher.BeginInvoke(new Action(() => _EqptEnd.ShowModal()));
                _EqptEnd.CenterOnScreen();
            }

            //Dictionary<string, string> dicParam = new Dictionary<string, string>();

            //dicParam.Add("PROCID", Process.ASSY_REWINDER);
            //dicParam.Add("EQPTID", Util.NVC(cboEquipment.SelectedValue));
            //dicParam.Add("EQSGID", Util.NVC(cboEquipmentSegment.SelectedValue));
            //dicParam.Add("RUNLOT", _CURRENT_OUTLOT);

            //ASSY004_070_EQPTEND _EqptEnd = new ASSY004_070_EQPTEND(dicParam);
            //_EqptEnd.FrameOperation = FrameOperation;
            //if (_EqptEnd != null)
            //{
            //    _EqptEnd.Closed += new EventHandler(OnCloseEqptEnd);
            //    this.Dispatcher.BeginInvoke(new Action(() => _EqptEnd.ShowModal()));
            //    _EqptEnd.CenterOnScreen();
            //}
        }

        private void OnCloseEqptEnd(object sender, EventArgs e)
        {
            ASSY004_070_EQPTEND window = sender as ASSY004_070_EQPTEND;

            if (window.DialogResult == MessageBoxResult.OK)
                RefreshData();
        }

        private void btnEqptEndCancel_Click(object sender, RoutedEventArgs e)
        {
           try
            {
                C1DataGrid dg = dgProductLot;
                DataRow[] dr = Util.gridGetChecked(ref dg, "CHK");

                if (dr == null || dr.Length < 1 || dg == null)
                {
                    Util.MessageValidation("SFU1938");  //취소할 LOT을 선택하세요.
                    return;
                }

                if (CommonVerify.HasDataGridRow(dgProductLot))
                {
                    DataTable dt = ((DataView)dgProductLot.ItemsSource).Table;
                    var queryEdit = (from t in dt.AsEnumerable()
                                     where t.Field<string>("WIPSTAT") == "PROC" && t.Field<int>("CHK") == 1
                                     select t).ToList();

                    if (queryEdit.Any())
                    {
                        Util.MessageValidation("SFU1939");   // 취소 할 수 있는 상태가 아닙니다.
                        return;
                    }
                }

                //선택된 LOT을 작업 취소하시겠습니까?
                Util.MessageConfirm("SFU3151", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        DataSet indataSet = new DataSet();
                        #region # IN_EQP
                        DataTable IN_EQP = indataSet.Tables.Add("INDATA");
                        IN_EQP.Columns.Add("SRCTYPE", typeof(string));
                        IN_EQP.Columns.Add("IFMODE", typeof(string));
                        IN_EQP.Columns.Add("USERID", typeof(string));
                        IN_EQP.Columns.Add("LOTID", typeof(string));
                        IN_EQP.Columns.Add("PROCID", typeof(string));
                        IN_EQP.Columns.Add("EQPTID", typeof(string));
                        
                        DataRow row = IN_EQP.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["USERID"] = LoginInfo.USERID;
                        row["LOTID"] = _CURRENT_OUTLOT;
                        row["PROCID"] = Process.ASSY_REWINDER;
                        row["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                        
                        IN_EQP.Rows.Add(row);
                        #endregion

                        #region # IN_INPUT
                        DataTable IN_INPUT = indataSet.Tables.Add("INLOT");
                        IN_INPUT.Columns.Add("INPUT_LOTID", typeof(string));
                        for (int i = 0; i < dgProductLot.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CHK").Equals(1) && DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "WIPSTAT").Equals("EQPT_END"))
                            {
                                DataRow newRow = IN_INPUT.NewRow();
                                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "INPUT_LOTID"));
                                IN_INPUT.Rows.Add(newRow);
                            }
                        }
                        #endregion

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RW_CANCEL_EQPT_END_LOT", "INDATA,INLOT", null, (Result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }
                            
                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            RefreshData();
                        }, indataSet);

                    }
                });
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void btnEnd_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }
            if (dgProductLot.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1707");  //실적 확정할 대상이 없습니다.
                return;
            }
            DataTable dt = (dgProductLot.ItemsSource as DataView).Table;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (Util.NVC(dt.Rows[i]["WIPSTAT"]) == "PROC")
                {
                    Util.MessageValidation("SFU3194");  //실적확정 Lot 선택 오류 [선택한 Lot이 장비완료상태 인지 확인 후 처리]
                    return;
                }
            }

            if (!ValidShift()) { return; }
            if (!ValidOperator()) { return; }
            if (!ValidEnd()) { return; }

            ASSY004_070_END _End = new ASSY004_070_END();
            _End.FrameOperation = FrameOperation;
            if (_End != null)
            {
                object[] parameters = new object[10];

                parameters[0] = DataTableConverter.Convert(dgOutLotInfo.ItemsSource);       //완성Lot
                parameters[1] = DataTableConverter.Convert(dgInputLotInfo.ItemsSource);     //투입Lot
                parameters[2] = Process.ASSY_REWINDER;
                parameters[3] = Util.NVC(cboEquipment.SelectedValue);
                parameters[4] = Util.NVC(cboEquipmentSegment.SelectedValue);
                parameters[5] = _CURRENT_OUTLOT;
                parameters[6] = Util.NVC(txtShift.Tag);
                parameters[7] = Util.NVC(txtWorker.Tag);
                parameters[8] = Util.NVC(txtWorker.Text);

                C1WindowExtension.SetParameters(_End, parameters);

                _End.Closed += new EventHandler(OnCloseEnd);
                this.Dispatcher.BeginInvoke(new Action(() => _End.ShowModal()));
                _End.CenterOnScreen();
            }
        }

        private void OnCloseEnd(object sender, EventArgs e)
        {
            ASSY004_070_END window = sender as ASSY004_070_END;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                if (LoginInfo.CFG_LABEL_AUTO.Equals("Y"))
                    Util.PrintLabel_Elec(FrameOperation, loadingIndicator, window._ReturnLotID, Process.ASSY_REWINDER);
                RefreshData();
            }                
        }

        private void btnBarcodeLabel_Click(object sender, RoutedEventArgs e)
        {
            if (dgOutLotInfo.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1559");  //발행할 LOT을 선택하십시오.
                return;
            }

            if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2003"); // 프린트 환경 설정값이 없습니다.
                return;
            }
            DataTable dt = ((DataView)dgOutLotInfo.ItemsSource).Table;
            DataTable printDT = GetPrintCount(Util.NVC(DataTableConverter.GetValue(dgOutLotInfo.Rows[dgOutLotInfo.TopRows.Count].DataItem, "LOTID")), Process.ASSY_REWINDER);

            if (printDT.Rows.Count > 0 && Util.NVC_Decimal(printDT.Rows[0]["PRT_COUNT1"]) > 0)
            {
                // 이미 해당 공정에서 발행된 Lot인데 재 발행하시겠습니까?
                Util.MessageConfirm("SFU3463", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        try
                        {
                            for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                            {
                                foreach (DataRow _iRow in dt.Rows)
                                {
                                    Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(_iRow["LOTID"]), Process.ASSY_REWINDER);
                                }
                            }
                        }
                        catch (Exception ex) { Util.MessageException(ex); }
                    }
                });
            }
            else
            {
                try
                {
                    foreach (DataRow _iRow in dt.Rows)
                        Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(_iRow["LOTID"]), Process.ASSY_REWINDER);
                }
                catch (Exception ex) { Util.MessageException(ex); }
            }
        }

        private void btnPrintLabel_Click(object sender, RoutedEventArgs e)
        {
            //인쇄할 항목이 없는 경우 발행 팝업 출력.
            if (!CanPrintPopup())
                return;

            ASSY004_001_HIST wndPrint = new ASSY004_001_HIST();
            wndPrint.FrameOperation = FrameOperation;

            if (wndPrint != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = Process.ASSY_REWINDER;
                Parameters[1] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[2] = cboEquipment.SelectedValue.ToString();
                //_UNLDR코드를 wndPrint로 보낸다.
                //Parameters[3] = _UNLDR_LOT_IDENT_BAS_CODE;

                C1WindowExtension.SetParameters(wndPrint, Parameters);

                wndPrint.Closed += new EventHandler(wndPrint_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPrint.ShowModal()));
            }
        }
        private void wndPrint_Closed(object sender, EventArgs e)
        {
            ASSY004_001_HIST window = sender as ASSY004_001_HIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private bool CanPrintPopup()
        {
            bool bRet = false;
            if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                //Util.Alert("프린트 환경 설정값이 없습니다.");
                Util.MessageValidation("SFU2003");
                return bRet;
            }

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageInfo("SFU1673");
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        // 임시 저장 기능
        private void btnSaveWipHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgOutLotInfo.GetRowCount() == 0)
                    return;

                if (cboEquipment.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                    return;
                }

                SaveWIPHistory();
                Util.MessageInfo("SFU1270");    //저장되었습니다.

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 작업자 선택
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");
                return;
            }

            CMM_SHIFT_USER2 wndPopup = new CMM_SHIFT_USER2();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[3] = Process.ASSY_REWINDER;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[7] = "Y"; // 저장 플래그 "Y" 일때만 저장.
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(OnCloseShift);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void OnCloseShift(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = sender as CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker.Tag = Util.NVC(wndPopup.USERID);
                txtShiftDateTime.Text = Util.NVC(wndPopup.WRKSTRTTIME) + " ~ " + Util.NVC(wndPopup.WRKENDTTIME);
                txtShiftStartTime.Text = Util.NVC(wndPopup.WRKSTRTTIME);
                txtShiftEndTime.Text = Util.NVC(wndPopup.WRKENDTTIME);
            }
        }

        // 좌측 확장 버튼
        private void btnLeftExpandFrame_Click(object sender, RoutedEventArgs e)
        {
            if (btnLeftExpandFrame.IsChecked == true)
            {
                Content.ColumnDefinitions[0].Width = new GridLength(0);
            }
            else
            {
                Content.ColumnDefinitions[0].Width = ExpandFrame;
            }
        }

        // 상하 확장 버튼
        private void btnExpandFrame_Click(object sender, RoutedEventArgs e)
        {
            if (btnExpandFrame.IsChecked == true)
            {
                Content.RowDefinitions[1].Height = new GridLength(0);
            }
            else
            {
                Content.RowDefinitions[1].Height = ExpandFrame;
            }
        }
        

        // 불량/Loss/물청 저장
        private void btnSaveWipReason_Click(object sender, RoutedEventArgs e)
        {
            if (dgInputLotInfo.GetRowCount() == 0)
                return;
            try
            {
                SaveDefect(dgWipReason);       // 완성LOT
                SaveInputDefect(dgWipReason);  // 투입LOT
                isChangeWipReason = false;

                Util.MessageInfo("SFU1270");    //저장되었습니다.
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // 특이사항 저장
        private void btnSaveRemark_Click(object sender, RoutedEventArgs e)
        {
            if (dgInputLotInfo.GetRowCount() == 0)
                return;
            try
            {
                SaveWipNote();
                isChangeRemark = false;

                Util.MessageInfo("SFU1270");    //저장되었습니다.
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        #region 작업자 실명관리 기능 추가
        private bool CheckRealWorkerCheckFlag()
        {
            try
            {
                bool bRet = false;
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["PROCID"] = Process.ASSY_REWINDER;
                dtRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("REAL_WRKR_CHK_FLAG"))
                {
                    if (Util.NVC(dtRslt.Rows[0]["REAL_WRKR_CHK_FLAG"]).Equals("Y"))
                        bRet = true;
                }
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void wndRealWorker_Closed(object sender, EventArgs e)
        {
            try
            {
                CMM001.CMM_COM_INPUT_USER window = sender as CMM001.CMM_COM_INPUT_USER;

                if (window.DialogResult == MessageBoxResult.OK)
                {
                    SaveRealWorker(window.USER_NAME);

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveRealWorker(string sWrokerName)
        {
            try
            {
                DataTable inTable = new DataTable();

                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("WORKER_NAME", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dgOutLotInfo.GetRowCount(); i++)
                {
                    if (!string.Equals(DataTableConverter.GetValue(dgOutLotInfo.Rows[i + dgOutLotInfo.TopRows.Count].DataItem, "CHG_BLOCK_FLAG"), "Y"))
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutLotInfo.Rows[i + dgOutLotInfo.TopRows.Count].DataItem, "LOTID"));
                        //newRow["WIPSEQ"] = null;
                        newRow["WORKER_NAME"] = sWrokerName;
                        newRow["USERID"] = LoginInfo.USERID;

                        inTable.Rows.Add(newRow);
                    }
                }

                if (inTable.Rows.Count < 1) return;

                new ClientProxy().ExecuteService("BR_PRD_REG_WIPHISTORYATTR_REAL_WORKER", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

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
    }
}