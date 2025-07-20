using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_050_PROC.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_050_PROC : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        private BizDataSet _Biz;
        private UserControl _UCParent;
        private Util _Util;
        private RadioButton rbSelectedType;
        private int selectedProductLotIdx;

        public ASSY004_050_PROC(UserControl parent)
        {
            _UCParent = parent;
            _Biz = new BizDataSet();
            _Util = new Util();
            rbSelectedType = null;
            selectedProductLotIdx = -1;
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Event]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            //동 설정
            txtArea.Text = LoginInfo.CFG_AREA_NAME;

            InitCombo();

            //Reload방지
            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSearch())
            {
                HideLoadingIndicator();
                return;
            }

            selectedProductLotIdx = -1;
            ClearGrid();
            GetProductLot();
            GetInputMountInfo();
           // GetOutLot();
        }

        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (!CanRunStart())
            {
                //HideLoadingIndicator();
                return;
            }

            ASSY004_050_RUNSTART wndRun = new ASSY004_050_RUNSTART();
            wndRun.FrameOperation = FrameOperation;

            if (wndRun != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                //Parameters[2] = Util.NVC(popSearchProdID.SelectedValue);

                C1WindowExtension.SetParameters(wndRun, Parameters);

                wndRun.Closed += new EventHandler(wndRun_Closed);
                grdMain.Children.Add(wndRun);
                wndRun.BringToFront();
            }

        }

        private void wndRun_Closed(object sender, EventArgs e)
        {
            ASSY004_050_RUNSTART window = sender as ASSY004_050_RUNSTART;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
                GetInputMountInfo();
            }
            this.grdMain.Children.Remove(window);
        }

        private void wndInputEnd_Closed(object sender, EventArgs e)
        {
            ASSY004_050_INPUT_LOT_END window = sender as ASSY004_050_INPUT_LOT_END;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetInputMountInfo();
                if(rbSelectedType != null)
                    rbType_Checked(rbSelectedType, null);
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnEqptEnd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanSearch())
                {
                    HideLoadingIndicator();
                    return;
                }

                ShowLoadingIndicator();

                DataTable inData = new DataTable();
                inData.Columns.Add("SRCTYPE");
                inData.Columns.Add("IFMODE");
                inData.Columns.Add("EQPTID");
                inData.Columns.Add("USERID");
                inData.Columns.Add("PROD_LOTID");
                inData.Columns.Add("OUTPUT_QTY");
                inData.Columns.Add("END_DTTM");

                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = DataTableConverter.GetValue((dgProductLot.SelectedItem as DataRowView), "LOTID") as string;
                newRow["OUTPUT_QTY"] = null;
                newRow["END_DTTM"] = null;

                inData.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_EQPT_END_PROD_LOT_RWK_ST_L", "INDATA", null, inData, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상 처리 되었습니다.
                        ClearGrid();
                        GetProductLot();
                        GetInputMountInfo();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void rbProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            //rb클릭시 Row선택한 것으로 되도록 설정
            //클릭한 Row의 PRODID를 가져옴
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb == null || rb.DataContext == null)
                return;

            if (rb.IsChecked.HasValue && rb.IsChecked.Value)
            {
                //rb.Parent는 부모가 보는 선택된 한 줄을 의미한다. 따라서 부모가 봤을 때는 선택된 줄이 몇번째인지 알 수 있다.
                int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                dgProductLot.SelectedIndex = idx;
                selectedProductLotIdx = idx;
            }

            GetAllGrid();
        }

        private void btnSaveOutLot_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (!CanSearch())
                {
                    HideLoadingIndicator();
                    return;
                }

                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("PROD_LOTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));

                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("LOTID", typeof(string));
                inInput.Columns.Add("WIPQTY_ED", typeof(int));

                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.SelectedItem as DataRowView, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;
                inData.Rows.Add(newRow);
                newRow = null;

                newRow = inInput.NewRow();
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutLot.SelectedItem as DataRowView, "LOTID"));
                newRow["WIPQTY_ED"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgOutLot.SelectedItem as DataRowView, "WIPQTY"));
                inInput.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UPD_OUT_LOT_RWK_ST_L", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상 처리 되었습니다.

                        //GetWaitList();
                        GetProductLot();
                        GetOutLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void btnDeleteOutLot_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (!CanSearch())
                {
                    HideLoadingIndicator();
                    return;
                }

                if(SelectCntCHK(dgOutLot) == 0)
                {
                    Util.MessageInfo("SFU1154");
                    return;
                }

                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("PROD_LOTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));

                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("LOTID", typeof(string));
                inInput.Columns.Add("WIPQTY_ED", typeof(int));

                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.SelectedItem as DataRowView, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;
                inData.Rows.Add(newRow);
                newRow = null;

                foreach (DataRowView i in dgOutLot.ItemsSource)
                {
                    if ((DataTableConverter.GetValue(i, "CHK") as int?).Value == 1)
                    {
                        newRow = inInput.NewRow();
                        newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(i, "LOTID"));
                        inInput.Rows.Add(newRow);
                        newRow = null;
                    }
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DELETE_OUT_LOT_RWK_ST_L", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상 처리 되었습니다.

                        //GetWaitList();
                        GetProductLot();
                        GetOutLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void btnInputComplete_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSearch())
                return;

            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgInput, "CHK");
            if (iRow >= 0)
            {
                if(string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInput.SelectedItem as DataRowView, "INPUT_LOTID"))))
                {
                    //투입된 LOT이 없습니다.
                    Util.MessageValidation("SFU1969");
                    return;
                }

                ASSY004_050_INPUT_LOT_END wndInputEnd = new ASSY004_050_INPUT_LOT_END();
                wndInputEnd.FrameOperation = FrameOperation;

                if (wndInputEnd != null)
                {
                    object[] Parameters = new object[6];
                    Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                    Parameters[1] = cboEquipment.SelectedValue.ToString();
                    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgInput.Rows[iRow].DataItem as DataRowView, "EQPT_MOUNT_PSTN_ID"));
                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgInput.Rows[iRow].DataItem as DataRowView, "INPUT_LOTID"));
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.SelectedItem as DataRowView, "LOTID"));
                    Parameters[5] = Util.NVC_Int(DataTableConverter.GetValue(dgInput.Rows[iRow].DataItem as DataRowView, "WIPQTY"));

                    C1WindowExtension.SetParameters(wndInputEnd, Parameters);

                    wndInputEnd.Closed += new EventHandler(wndInputEnd_Closed);                   
                    grdMain.Children.Add(wndInputEnd);
                    wndInputEnd.BringToFront();
                }
            }
            else
            {
                //선택된 LOT이 존재하지 않습니다.
                Util.MessageValidation("SFU1137");
                return;
            }
        }

        private void btnCancelInput_Click(object sender, RoutedEventArgs e)
        {
           
            if (SelectCntCHK(dgInput) == 0)
            {
                Util.MessageInfo("SFU1261");
                return;
            }

            CancelInput();
        }

        private void dgInput_CurrentCellChanged(object sender, DataGridCellEventArgs e)
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

                                    for (int idx = 0; idx < dg.Rows.Count; idx++)
                                    {
                                        if (e.Cell.Row.Index != idx)
                                        {
                                            if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                            {
                                                (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                            }
                                            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                        }
                                    }
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                            dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                            (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                            (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                            (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                    chk.IsChecked = false;
                                }
                                break;
                        }
                    }
                    else if (e.Cell.Column.Index != dg.Columns.Count - 1) // 선택 후 Curr.Col.idx를 맨뒤로 보내므로.. 다시타는 문제.
                    {
                        if (!dg.Columns.Contains("CHK"))
                            return;

                        CheckBox chk2 = dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox;

                        if (chk2 != null)
                        {
                            if (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
                            {
                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                chk2.IsChecked = true;

                                for (int idx = 0; idx < dg.Rows.Count; idx++)
                                {
                                    if (e.Cell.Row.Index != idx)
                                    {
                                        if (dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter != null &&
                                            dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter.Content != null &&
                                            (dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                        {
                                            (dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
                                        }
                                        DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                    }
                                }
                            }
                            else if (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter != null &&
                                     dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content != null &&
                                     (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox) != null &&
                                     (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                     (bool)(dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
                            {
                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                chk2.IsChecked = false;
                            }
                        }
                    }

                    if (dg.CurrentCell != null)
                        dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                    else if (dg.Rows.Count > 0)
                        dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
                }
            }));
        }

        private void dgOutLot_CurrentCellChanged(object sender, DataGridCellEventArgs e)
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

                                    for (int idx = 0; idx < dg.Rows.Count; idx++)
                                    {
                                        if (e.Cell.Row.Index != idx)
                                        {
                                            if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                            {
                                                (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                            }
                                            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                        }
                                    }
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                            dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                            (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                            (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                            (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                    chk.IsChecked = false;
                                }
                                break;
                        }
                    }
                    else if (e.Cell.Column.Index != dg.Columns.Count - 1) // 선택 후 Curr.Col.idx를 맨뒤로 보내므로.. 다시타는 문제.
                    {
                        if (!dg.Columns.Contains("CHK"))
                            return;

                        CheckBox chk2 = dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox;

                        if (chk2 != null)
                        {
                            if (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
                            {
                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                chk2.IsChecked = true;

                                for (int idx = 0; idx < dg.Rows.Count; idx++)
                                {
                                    if (e.Cell.Row.Index != idx)
                                    {
                                        if (dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter != null &&
                                            dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter.Content != null &&
                                            (dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                        {
                                            (dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
                                        }
                                        DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                    }
                                }
                            }
                            else if (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter != null &&
                                     dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content != null &&
                                     (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox) != null &&
                                     (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                     (bool)(dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
                            {
                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                chk2.IsChecked = false;
                            }
                        }
                    }

                    if (dg.CurrentCell != null)
                        dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                    else if (dg.Rows.Count > 0)
                        dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
                }
            }));
        }

        private void dgWaitMagazine_CurrentCellChanged(object sender, DataGridCellEventArgs e)
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

                                //row 색 바꾸기
                                dg.SelectedIndex = e.Cell.Row.Index;
                            }
                            else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                               dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                               (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                               (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                               (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                            {
                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                chk.IsChecked = false;

                            }

                            for (int idx = 0; idx < dg.Rows.Count; idx++)
                            {
                                if (e.Cell.Row.Index != idx)
                                {
                                    if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                        dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                        (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                    {
                                        (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                    }
                                    DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                }
                            }
                            break;
                    }

                    if (dg.CurrentCell != null)
                        dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                    else if (dg.Rows.Count > 0)
                        dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
                }
            }
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if ((sender as C1ComboBox).SelectedIndex == -1 || e.NewValue.ToString().Trim().Equals("SELECT") || (sender as C1ComboBox).SelectedIndex == 0)
            {
                ClearGrid();
                return;
            }
            else
            {
                btnSearch_Click(null, null);
               // GetInputMountInfo();
            }
        }

        private void btnEnd_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (!CanSearch())
                {
                    HideLoadingIndicator();
                    return;
                }
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("PROD_LOTID", typeof(string));

                inData.Columns.Add("INPUTQTY", typeof(int));
                inData.Columns.Add("OUTPUTQTY", typeof(int));
                inData.Columns.Add("RESNQTY", typeof(int));

                inData.Columns.Add("SHIFT", typeof(string));
                inData.Columns.Add("WIPNOTE", typeof(string));
                inData.Columns.Add("WRK_USERID", typeof(string));
                inData.Columns.Add("WRK_USER_NAME", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("WIPDTTM_ED", typeof(string));

                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInput.Columns.Add("INPUT_LOTID", typeof(int));

                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.SelectedItem as DataRowView, "LOTID"));
                newRow["OUTPUTQTY"] = Util.NVC_Int(DataTableConverter.GetValue((dgProductLot.SelectedItem as DataRowView), "WIPQTY")) + Util.NVC_Int(DataTableConverter.GetValue((dgProductLot.SelectedItem as DataRowView), "EQPT_END_QTY"));
                newRow["WIPNOTE"] = "이상 없음";
                newRow["USERID"] = LoginInfo.USERID;
                inData.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_RWK_ST_L", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상 처리 되었습니다.

                        GetProductLot();
                        Util.gridClear(dgOutLot);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void btnCreateOutLot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanSearch())
                {
                    HideLoadingIndicator();
                    return;
                }

                if (txtCreateLotCnt.Text == null || txtCreateLotCnt.Text == "")
                {
                    Util.MessageInfo("SFU1154");
                    return;
                }
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("PROD_LOTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));

                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("WIPQTY", typeof(int));
                inInput.Columns.Add("CSTID", typeof(string));

                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.SelectedItem as DataRowView, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;
                inData.Rows.Add(newRow);

                DataRow newRow2 = inInput.NewRow();
                newRow2["WIPQTY"] = Convert.ToInt32(txtCreateLotCnt.Text);
                newRow2["CSTID"] = txtCarrierID.Text;
                inInput.Rows.Add(newRow2);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CREATE_OUT_LOT_RWK_ST_L", "INDATA,IN_INPUT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상 처리 되었습니다.

                        GetProductLot();
                        GetOutLot();
                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void rbType_Checked(object sender, RoutedEventArgs e)
        {
            if (!CanSearch())
            {
                return;
            }

            rbSelectedType = sender as RadioButton;
            string sType = string.Empty;

            if ((sender as RadioButton).Equals(rbHalf))
            {
                sType = "HC";
            }
            else if ((sender as RadioButton).Equals(rbMono))
            {
                sType = "MC";
            }
            else
                return;

            GetWaitMagazinesByType(sType);
        }

        private void dgProductLot_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            GetDefectInfo();
        }

        private void dgEqpFaulty_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    //Grid Data Binding 이용한 Background 색 변경
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (e.Cell.Column.Name.Equals("PORT_NAME"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void dgEqpFaulty_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null) return;

                C1DataGrid dataGrid = (sender as C1DataGrid);
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "PORT_NAME")
                    {
                        if (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, cell.Column.Name))))
                        {
                            CMM_ASSY_EQPT_DFCT_CELL_INFO wndEqptDfctCell = new CMM_ASSY_EQPT_DFCT_CELL_INFO();
                            wndEqptDfctCell.FrameOperation = FrameOperation;

                            if (wndEqptDfctCell != null)
                            {
                                object[] Parameters = new object[3];

                                int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                                if (idx < 0) return;

                                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                                Parameters[1] = cboEquipment.SelectedValue;
                                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "PORT_ID"));

                                C1WindowExtension.SetParameters(wndEqptDfctCell, Parameters);

                                wndEqptDfctCell.Closed += new EventHandler(wndEqptDfctCell_Closed);

                                this.Dispatcher.BeginInvoke(new Action(() => wndEqptDfctCell.ShowModal()));
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

        private void dgEqpFaulty_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void wndEqptDfctCell_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_DFCT_CELL_INFO window = sender as CMM_ASSY_EQPT_DFCT_CELL_INFO;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveDefect())
                return;

            //불량정보를 저장하시겠습니까?            
            Util.MessageConfirm("SFU1587", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetDefect();
                }
            });
        }

        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (!e.Cell.Column.Name.Equals("ACTNAME"))
                    {
                        //if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "RSLT_EXCL_FLAG")).Equals("Y"))
                        //{
                        //    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        //    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        //}
                        //else
                        //{
                            string sFlag = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                            if (sFlag == "Y")
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D4D4D4"));
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                            }
                            else
                            {
                                e.Cell.Presenter.Background = null;// new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                            }
                        //}
                    }
                }
            }));
        }

        private void dgDefect_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                if (e.Column.Name.Equals("RESNQTY"))
                {
                    string sFlag = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                    if (sFlag == "Y")
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDefect_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void btnHistSearch_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (dgProductLot.SelectedIndex < 0)
            {
                Util.MessageValidation("SFU1645");
                return;
            }
            */
            GetInputHistory();
        }

        private void cboHistMountPstsID_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            if (e.OldValue == null)
                return;

            try
            {
                GetInputHistory();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnInBoxInputCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanSearch() || _Util.GetDataGridFirstRowIndexByCheck(dgInputHist,"CHK") < 0)
                {
                    HideLoadingIndicator();
                    return;
                }

                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("PROD_LOTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));

                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInput.Columns.Add("INPUT_LOTID", typeof(string));
                inInput.Columns.Add("WIPNOTE", typeof(string));
                inInput.Columns.Add("INPUT_SEQNO", typeof(int));
                inInput.Columns.Add("CSTID", typeof(string));

                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = dgProductLot.SelectedIndex == -1 ? null : Util.NVC(DataTableConverter.GetValue(dgProductLot.SelectedItem as DataRowView, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;
                inData.Rows.Add(newRow);
                newRow = null;

                newRow = inInput.NewRow();
                int checkedIdx = _Util.GetDataGridFirstRowIndexByCheck(dgInputHist, "CHK");
                newRow["EQPT_MOUNT_PSTN_ID"] = DataTableConverter.GetValue(dgInputHist.Rows[checkedIdx].DataItem as DataRowView, "EQPT_MOUNT_PSTN_ID") as string;
                newRow["INPUT_LOTID"] = DataTableConverter.GetValue(dgInputHist.Rows[checkedIdx].DataItem as DataRowView, "INPUT_LOTID") as string;
                inInput.Rows.Add(newRow);


                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_INPUT_IN_LOT_RWK_ST_L", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상 처리 되었습니다.

                        GetInputMountInfo();
                        GetInputHistory();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Method]
        //자재투입
        private void InsertMtrl(string input_lotID, string mtrlID)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet dataSet = new DataSet();

                DataTable inData = dataSet.Tables.Add("INDATA");
                DataTable inInput = dataSet.Tables.Add("IN_INPUT");

                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("PROD_LOTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));

                inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInput.Columns.Add("INPUT_LOTID", typeof(string));
                inInput.Columns.Add("MTRLID", typeof(string));
                inInput.Columns.Add("ACTQTY", typeof(int));

                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                //선택한 것이 없으면 null이 되도록 수정
                newRow["PROD_LOTID"] = selectedProductLotIdx == -1 ? null : Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[selectedProductLotIdx].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;
                inData.Rows.Add(newRow);
                newRow = null;

                newRow = inInput.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(cboMountPstsID.SelectedValue);
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = input_lotID;
                newRow["MTRLID"] = mtrlID;
                newRow["ACTQTY"] = 0;
                inInput.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_INPUT_LOT_RWK_ST_L", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상 처리 되었습니다.
                        GetInputMountInfo();
                        RecheckRbType();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, dataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HideLoadingIndicator();
            }
        }

        //생산랏 조회
        private void GetProductLot()
        {
            try
            {
                if (!CanSearch())
                {
                    HideLoadingIndicator();
                    return;
                }

                ShowLoadingIndicator();

                DataTable inData = new DataTable();
                inData.Columns.Add("LANGID");
                inData.Columns.Add("EQPTID");
                inData.Columns.Add("PROCID");
                inData.Columns.Add("EQSGID");
                inData.Columns.Add("PRODID");

                DataRow newRow = inData.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue;
                newRow["PROCID"] = Process.RWK_LNS;

                inData.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_RWK_L", "INDATA", "OUTDATA", inData, (searchResult, searchException) =>
                {
                    try
                    {
                        Util.GridSetData(dgProductLot, searchResult, this.FrameOperation);

                        //Proc상태이면 자동 선택
                        int procIdx = _Util.GetDataGridRowIndex(dgProductLot, "WIPSTAT", "PROC");
                        if (procIdx > -1)
                        {
                            DataTableConverter.SetValue(dgProductLot.Rows[procIdx].DataItem, "CHK", false);
                            DataTableConverter.SetValue(dgProductLot.Rows[procIdx].DataItem, "CHK", true);

                            selectedProductLotIdx = procIdx;
                            dgProductLot.SelectedIndex = procIdx;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        //투입위치 조회
        private void GetInputMountInfo()
        {
            try
            {
                if (!CanSearch())
                {
                    HideLoadingIndicator();
                    return;
                }

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_INPUT_POS_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = cboEquipment.SelectedValue;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_EQPT_MOUNT_INFO_RWK", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        dgInput.CurrentCellChanged -= dgInput_CurrentCellChanged;
                        Util.GridSetData(dgInput, searchResult, null, true);
                        dgInput.CurrentCellChanged += dgInput_CurrentCellChanged;

                        if (dgInput.CurrentCell != null)
                            dgInput.CurrentCell = dgInput.GetCell(dgInput.CurrentCell.Row.Index, dgInput.Columns.Count - 1);
                        else if (dgInput.Rows.Count > 0 && dgInput.GetCell(dgInput.Rows.Count, dgInput.Columns.Count - 1) != null)
                            dgInput.CurrentCell = dgInput.GetCell(dgInput.Rows.Count, dgInput.Columns.Count - 1);


                        int halfCnt = 0;
                        int monoCnt = 0;
                        foreach(DataRowView drv in searchResult.DefaultView)
                        {
                            if(!string.IsNullOrEmpty(DataTableConverter.GetValue(drv,"INPUT_LOTID") as string))
                            {
                                if ((DataTableConverter.GetValue(drv, "MTRL_CLSS_CODE") as string).Equals("HC"))
                                    halfCnt++;
                                else if ((DataTableConverter.GetValue(drv, "MTRL_CLSS_CODE") as string).Equals("MC"))
                                    monoCnt++;
                            }
                        }

                        tbHalfCnt.Text = Convert.ToString(halfCnt);
                        tbMonoCnt.Text = Convert.ToString(monoCnt);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetOutLot()
        {
            try
            {
                if (!CanSearch())
                {
                    HideLoadingIndicator();
                    return;
                }

                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inData = new DataTable();
                inData.Columns.Add("EQSGID", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));
                inData.Columns.Add("PR_LOTID", typeof(string));
                inData.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inData.NewRow();
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROCID"] = Process.RWK_LNS;
                newRow["PR_LOTID"] = selectedProductLotIdx == -1 ? null : Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[selectedProductLotIdx].DataItem, "LOTID"));
                inData.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_RWK_OUT_LOT_L", "INDATA", "OUTDATA", inData, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        dgOutLot.ItemsSource = DataTableConverter.Convert(searchResult);
                        int wipQty = 0;
                        foreach (DataRow dr in searchResult.Rows)
                        {
                            wipQty += Convert.ToInt32((decimal)dr["WIPQTY"]);
                        }
                        string boxCnt = Convert.ToString(searchResult.Rows.Count);
                        DataTableConverter.SetValue(dgProductLot.SelectedItem as DataRowView, "WIPQTY", wipQty);
                        DataTableConverter.SetValue(dgProductLot.SelectedItem as DataRowView, "BOXCNT", boxCnt);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetDefectInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Process.RWK_LNS;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.SelectedItem as DataRowView, "LOTID"));
                newRow["WIPSEQ"] = 1;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_QCA_SEL_WIPRESONCOLLECT_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgDefect, searchResult, null, false);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HideLoadingIndicator();
            }

        }

        private void SetDefect()
        {
            try
            {
                dgDefect.EndEdit();
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable inDEFECT_LOT = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = null;

                    newRow = inDEFECT_LOT.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.SelectedItem as DataRowView, "LOTID"));
                    newRow["WIPSEQ"] = 1;
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNCODE"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")));
                    newRow["RESNCODE_CAUSE"] = "";
                    newRow["PROCID_CAUSE"] = "";
                    newRow["RESNNOTE"] = "";
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID")).GetString() == "CHARGE_PROD_LOT")
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = "";
                    }

                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;
                    //newRow["A_TYPE_DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "A_TYPE_DFCT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "A_TYPE_DFCT_QTY")));
                    //newRow["C_TYPE_DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "C_TYPE_DFCT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "C_TYPE_DFCT_QTY")));

                    inDEFECT_LOT.Rows.Add(newRow);
                }

                if (inDEFECT_LOT.Rows.Count < 1)
                {
                    return;
                }

                new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");

                        GetProductLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                HideLoadingIndicator();
            }
        }

        private void GetInputHistory()
        {
            try
            {

                DataTable inTable = _Biz.GetUC_DA_PRD_SEL_INPUT_HIST();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.SelectedItem as DataRowView, "LOTID"));
                newRow["PROD_WIPSEQ"] = 1;
                newRow["INPUT_LOTID"] = txtHistLotID.Text.Trim().Equals("") ? null : txtHistLotID.Text;
                newRow["EQPT_MOUNT_PSTN_ID"] = cboHistMountPstsID.SelectedValue.ToString().Equals("") ? null : cboHistMountPstsID.SelectedValue.ToString();
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_INPUT_MTRL_HIST_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgInputHist.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgInputHist, searchResult, FrameOperation, true);

                        if (dgInputHist.CurrentCell != null)
                            dgInputHist.CurrentCell = dgInputHist.GetCell(dgInputHist.CurrentCell.Row.Index, dgInputHist.Columns.Count - 1);
                        else if (dgInputHist.Rows.Count > 0 && dgInputHist.GetCell(dgInputHist.Rows.Count, dgInputHist.Columns.Count - 1) != null)
                            dgInputHist.CurrentCell = dgInputHist.GetCell(dgInputHist.Rows.Count, dgInputHist.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        txtHist_CST.Text = "";
                        txtHistLotID.Text = "";
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetWaitMagazinesByType(string sType)
        {
            try
            {
                if (!CanSearch())
                {
                    HideLoadingIndicator();
                    return;
                }

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID");
                inTable.Columns.Add("PROCID");
                inTable.Columns.Add("EQSGID");
                inTable.Columns.Add("PRODID");
                inTable.Columns.Add("PRDT_LEVEL2_CODE");

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.RWK_LNS;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue;
                newRow["PRODID"] = selectedProductLotIdx == -1 ? null : DataTableConverter.GetValue((dgProductLot.Rows[selectedProductLotIdx].DataItem as DataRowView), "PRODID");
                newRow["PRDT_LEVEL2_CODE"] = sType;

                inTable.Rows.Add(newRow);

                //2019.07.19 김대근 DA_PRD_SEL_RWK_WAIT_MAG_BY_MBOM -> DA_PRD_SEL_RWK_WAIT_MAG_L
                new ClientProxy().ExecuteService("DA_PRD_SEL_RWK_WAIT_MAG_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.gridClear(dgWaitMagazine);
                        Util.GridSetData(dgWaitMagazine, searchResult, FrameOperation);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetAllGrid()
        {
            GetOutLot();
            GetDefectInfo();
            GetInputHistory();
           //GetInputMountInfo();

            string sType = string.Empty;

            if (rbHalf.IsChecked == true)
                sType = "HC";
            else if (rbMono.IsChecked == true)
                sType = "MC";
            else
                return;
            GetWaitMagazinesByType(sType);
        }
        #endregion

        #region [Util & Init Method]
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID, Process.RWK_LNS };
            C1ComboBox[] cboEquipmentSegmentChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, sFilter: sFilter, sCase: "PROCESSEQUIPMENTSEGMENT");

            String[] sFilter2 = { Process.RWK_LNS };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            C1ComboBox[] cboEquipmentChild = { cboMountPstsID, cboHistMountPstsID };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentChild, cbParent: cboEquipmentParent, sFilter: sFilter2, sCase: "EQUIPMENT_BY_EQSGID_PROCID");

            String[] sFilter3 = { "PROD" };
            C1ComboBox[] cboMountPstParent = { cboEquipment };
            _combo.SetCombo(cboMountPstsID, CommonCombo.ComboStatus.SELECT, cbParent: cboMountPstParent, sFilter: sFilter3, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
            _combo.SetCombo(cboHistMountPstsID, CommonCombo.ComboStatus.ALL, cbParent: cboMountPstParent, sFilter: sFilter3, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");

            //if (cboEquipmentSegment.Items.Count >= 2)
            //    cboEquipmentSegment.SelectedIndex = 1;

            //GetModelList();
        }

        private void ClearGrid()
        {
            Util.gridClear(dgInput);
            Util.gridClear(dgProductLot);
            Util.gridClear(dgOutLot);
            Util.gridClear(dgWaitMagazine);
            Util.gridClear(dgInputHist);
        }

        private int SelectCntCHK(C1DataGrid dt)
        {
            int cnt = 0;

            foreach (DataRowView i in dt.ItemsSource)
            {
                if ((DataTableConverter.GetValue(i, "CHK") as int?).Value == 1)
                {
                    cnt++;
                    break;
                }
            }

            return cnt;
        }

        private bool CanSearch()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1153");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanRunStart()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            /*
            if (Util.NVC(popSearchProdID.SelectedValue).Equals(""))
            {
                Util.MessageValidation("SFU1638");
                return bRet;
            }
            */

            if (dgProductLot?.ItemsSource != null)
            {
                foreach (DataRowView i in dgProductLot.ItemsSource)
                {
                    if ((DataTableConverter.GetValue(i, "WIPSTAT") as string).Equals("PROC"))
                    {
                        //장비에 진행 중 인 LOT이 존재 합니다.
                        Util.MessageValidation("SFU1863");
                        return bRet;
                    }
                }
            }

            bRet = true;
            return bRet;
        }

        private bool CanInsertMtrl_WaitMagTab()
        {
            bool bRet = false;

            if (cboMountPstsID.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //투입위치를 선택하세요.
                Util.MessageValidation("SFU1981");
                return bRet;
            }

            int index = _Util.GetDataGridRowIndex(dgInput, "EQPT_MOUNT_PSTN_ID", cboMountPstsID.SelectedValue as string);

            if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInput.Rows[index].DataItem, "INPUT_LOTID"))))
            {
                //%1 에 이미 투입된 자재가 존재 합니다.
                string[] param = new string[1];
                param[0] = Util.NVC(DataTableConverter.GetValue((dgInput.Rows[index].DataItem), "EQPT_MOUNT_PSTN_NAME"));
                Util.MessageValidation("SFU1288", param);
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanInsertMtrl_InputMtrlTab()
        {
            bool bRet = false;
            if (_Util.GetDataGridCheckFirstRowIndex(dgInput, "CHK") == -1)
            {
                //투입위치를 선택하세요.
                Util.MessageValidation("SFU1981");
                return bRet;
            }

            if (string.IsNullOrEmpty(txtInsertLotID_InputMtrlTab.Text))
            {
                Util.MessageValidation("SFU1137");
                return bRet;
            }

            int index = _Util.GetDataGridCheckFirstRowIndex(dgInput, "CHK");

            if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInput.Rows[index].DataItem, "INPUT_LOTID"))))
            {
                //%1 에 이미 투입된 자재가 존재 합니다.
                string[] param = new string[1];
                param[0] = Util.NVC(DataTableConverter.GetValue(dgInput.Rows[index].DataItem, "EQPT_MOUNT_PSTN_NAME"));
                Util.MessageValidation("SFU1288", param);
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanSaveDefect()
        {
            if (!CommonVerify.HasDataGridRow(dgDefect))
            {
                Util.MessageValidation("SFU1578");      //불량 항목이 없습니다.
                return false;
            }

            if (string.IsNullOrEmpty(DataTableConverter.GetValue(dgProductLot.SelectedItem as DataRowView, "LOTID").ToString()))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private void RecheckRbType()
        {
            if (rbSelectedType == null)
                return;

            rbSelectedType.IsChecked = false;
            rbSelectedType.IsChecked = true;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnCancelInput);
            listAuth.Add(btnCreateOutLot);
            listAuth.Add(btnDeleteOutLot);
            listAuth.Add(btnEnd);

            listAuth.Add(btnEqptEnd);
            listAuth.Add(btnInputComplete);
            listAuth.Add(btnRunStart);
            listAuth.Add(btnSaveOutLot);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                if (loadingIndicator.Visibility != Visibility.Visible)
                    loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                if (loadingIndicator.Visibility != Visibility.Collapsed)
                    loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        private void txtInsertLotID_InputMtrlTab_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            if (!CanSearch() || !CanInsertMtrl_InputMtrlTab())
            {
                return;
            }

            TextBox txt = sender as TextBox;
            if (txt == null)
                return;

            string input_lotID = txt.Text;

            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LOTID"] = input_lotID;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_LOTID_PRODID", "INDATA", "OUTDATA", dt);

                if (result == null)
                {
                    //존재하는 랏 아이디인지 체크
                    return;
                }
                InsertMtrl(input_lotID, result.Rows[0]["PRODID"] as string);
                txt.Clear();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtInsertLotID_WaitMagTab_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            if (!CanSearch() || !CanInsertMtrl_WaitMagTab())
            {
                return;
            }

            TextBox txt = sender as TextBox;
            if (txt == null)
                return;

            string input_lotID = txt.Text;

            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LOTID"] = input_lotID;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_LOTID_PRODID", "INDATA", "OUTDATA", dt);

                if (result == null)
                {
                    //존재하는 랏 아이디인지 체크
                    return;
                }
                InsertMtrl(input_lotID, result.Rows[0]["PRODID"] as string);
                txt.Clear();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnInsertMtrl_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSearch() || !CanInsertMtrl_WaitMagTab())
            {
                HideLoadingIndicator();
                return;
            }

            if (dgWaitMagazine.SelectedIndex < 0)
            {
                //선택된 LOT이 존재하지 않습니다.
                Util.MessageValidation("SFU1137");
                return;
            }

            string input_lotID = Util.NVC(DataTableConverter.GetValue(dgWaitMagazine.Rows[dgWaitMagazine.SelectedIndex].DataItem, "LOTID"));
            string mtrlID = Util.NVC(DataTableConverter.GetValue(dgWaitMagazine.Rows[dgWaitMagazine.SelectedIndex].DataItem, "PRODID"));

            InsertMtrl(input_lotID, mtrlID);
        }

        //투입취소
        private void CancelInput()
        {
            try
            {
                if (!CanSearch())
                {
                    HideLoadingIndicator();
                    return;
                }

                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("PROD_LOTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));

                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInput.Columns.Add("INPUT_LOTID", typeof(string));
                inInput.Columns.Add("WIPNOTE", typeof(string));
                inInput.Columns.Add("INPUT_SEQNO", typeof(int));
                inInput.Columns.Add("CSTID", typeof(string));

                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.SelectedItem as DataRowView, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;
                inData.Rows.Add(newRow);
                newRow = null;

                foreach (DataRowView i in dgInput.ItemsSource)
                {
                    if ((DataTableConverter.GetValue(i, "CHK") as int?).Value == 1)
                    {
                        newRow = inInput.NewRow();
                        newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(i, "EQPT_MOUNT_PSTN_ID"));
                        newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(i, "INPUT_LOTID"));
                        inInput.Rows.Add(newRow);
                        newRow = null;
                    }
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_INPUT_IN_LOT_RWK_ST_L", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상 처리 되었습니다.

                        GetInputMountInfo();
                        RecheckRbType();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }
    }
}
