using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.ELEC001
{
    /// <summary>
    /// ELEC001_002_INPUT_JC_MIXING.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ELEC001_002_INPUT_JC_MIXING : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        CommonCombo _combo = new CommonCombo();
        string WO_ID = string.Empty;
        string _EQPTID = string.Empty;
        string _HopperID = string.Empty;
        string _MtrlID = string.Empty;
        string _ProdID = string.Empty;
        string _ProcChkEqptID = string.Empty;
        DataSet inDataSet = null;
        DataTable IndataTable = null;
        bool HopperFlag = false;
        bool InputFlag = false;
        Util _Util = new Util();
        int Select = 0;
        #endregion

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ELEC001_002_INPUT_JC_MIXING()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                return;
            }

            InitControls();
            InitCombo();
            ApplyPermissions();

            ldpDateFrom.SelectedDateTime = Convert.ToDateTime(tmps[0]);
            ldpDateTo.SelectedDateTime = Convert.ToDateTime(tmps[1]);
            cboEquipmentSegment.SelectedValue = tmps[2];
        }

        #region Initialize

        public string PRODID { get; set; }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnRequest);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitControls()
        {
            ldpDate();
        }

        private void ldpDate()
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_PRD_SEL_AREA_DATE", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    ldpDateFrom.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    ldpDateTo.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    return;
                }
                ldpDateFrom.Text = result.Rows[0][0].ToString();
                ldpDateTo.Text = result.Rows[0][0].ToString();
            }
            );
        }


        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
        }
        #endregion


        #region Event
        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Util.NVC(cboEquipmentSegment.SelectedValue) != string.Empty)
            {
                if (LoginInfo.CFG_PROC_ID.Equals(Process.MIXING) || LoginInfo.CFG_PROC_ID.Equals(Process.SRS_MIXING) || LoginInfo.CFG_PROC_ID.Equals(Process.PRE_MIXING))
                {
                    String[] sFilter2 = { cboEquipmentSegment.SelectedValue.ToString(), Process.COATING, null };
                    _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2);
                }
            }
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Util.NVC(cboEquipment.SelectedValue) != string.Empty && Util.NVC(cboEquipment.SelectedValue) != "-SELECT-")
            {
                _EQPTID = Util.NVC(cboEquipment.SelectedValue.ToString().Trim());
                Util.gridClear(dgWorkOrder);
                Util.gridClear(dgProductLot);
            }
            else if (cboEquipment.Items.Count <= 1 && cboEquipment.SelectedIndex < 1)
            {
                Util.AlertInfo("SFU2017");  //해당 라인으로 설정 후 설비가 출력됩니다.
                Util.gridClear(dgWorkOrder);
                Util.gridClear(dgProductLot);
                return;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.Items.Count == 0)
            {
                Util.MessageValidation("SFU2016");   //해당 라인에 설비가 존재하지 않습니다.
                Util.gridClear(dgWorkOrder);
                Util.gridClear(dgProductLot);
                return;
            }

            if (Convert.ToDecimal(Convert.ToDateTime(ldpDateFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(ldpDateTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.MessageValidation("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
                return;
            }

            if (cboEquipment.Items.Count < 1 || cboEquipment.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요
                return;
            }

            GetWorkOrder();//좌측상단 작업지시 리스트 조회
        }

        private void dgProductLot_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                DataRowView drv = e.Row.DataItem as DataRowView;
                bool bchk = Convert.ToBoolean(DataTableConverter.GetValue(e.Row.DataItem, "CHK"));

                if (bchk == true)
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridNumericColumn)
                    {
                        if (Convert.ToString(e.Column.Name) == "MTRL_QTY")
                        {
                            e.Cancel = false;
                        }
                    }
                }
                else
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridNumericColumn)
                    {
                        if (Convert.ToString(e.Column.Name) == "MTRL_QTY")
                        {
                            e.Cancel = true;    // Editing 불가능
                        }
                    }
                    Util.MessageValidation("SFU1828");  //자재를 선택하세요.
                    return;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgProductLot.ItemsSource == null || dgProductLot.Rows.Count == 0)
                    return;

                if (dgProductLot.CurrentRow.DataItem == null)
                    return;
                int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

                DataTable dt = ((DataView)dgProductLot.ItemsSource).Table;

                if (DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "CHK").ToString().Equals("1"))
                {
                    dgProductLot.SelectedIndex = rowIndex;

                    // HOPPER 체크 시 ITEM 갱신되도록 추가 [2018-03-23]
                    C1.WPF.DataGrid.DataGridCell gridCell = dgProductLot.GetCell(rowIndex, dgProductLot.Columns["HOPPER_ID"].Index) as C1.WPF.DataGrid.DataGridCell;

                    if (gridCell != null && gridCell.Presenter != null && gridCell.Presenter.Content != null)
                    {
                        C1ComboBox combo = gridCell.Presenter.Content as C1ComboBox;

                        if (combo != null)
                            SetGridCboItem(combo, _EQPTID, Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "MTRLID")));
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }


        private void dgWorkOrderChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0") || (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("1"))
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                    Select = idx;
                    DataRow dtRow = (rb.DataContext as DataRowView).Row;
                    C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }
                    //row 색 바꾸기
                    dgWorkOrder.SelectedIndex = idx;
                    _ProdID = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "PRODID"));
                    _ProcChkEqptID = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "EQPTID"));

                    GetWOMaterial(Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "WOID")));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgProductLot.Rows.Count < 1 || dgProductLot.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU1833");  //자재정보가 없습니다.
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtWorker.Text) || string.IsNullOrWhiteSpace((string)txtWorker.Tag))
                {
                    // 요청자를 선택해 주세요.
                    Util.MessageValidation("SFU3467");
                    return;
                }

                dgProductLot.EndEdit();
                inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("WOID", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("REQ_USERID", typeof(string));
                inDataTable.Columns.Add("BTCH_ORD_ID", typeof(string));
                inDataTable.Columns.Add("CMC_BINDER_FLAG", typeof(string));

                DataRow inDataRow = null;

                inDataRow = inDataTable.NewRow();
                inDataRow["EQPTID"] = _EQPTID;
                inDataRow["WOID"] = WO_ID;
                inDataRow["NOTE"] = Util.NVC(txtRemark.Text.ToString());
                inDataRow["USERID"] = LoginInfo.USERID;
                inDataRow["REQ_USERID"] = (string)txtWorker.Tag;
                inDataRow["BTCH_ORD_ID"] = WO_ID;
                inDataRow["CMC_BINDER_FLAG"] = "N";
                inDataTable.Rows.Add(inDataRow);

                DataTable inMtrlid = _Mtrlid();
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                if (idx < 0)
                {
                    Util.MessageValidation("SFU1828");  //자재를 선택하세요.
                    return;
                }
                if (!InputFlag)
                {
                    return;
                }
                if (!HopperFlag)
                {
                    Util.MessageValidation("SFU2035");  //호퍼를 선택하세요.
                    return;
                }

                //투입요청 하시겠습니까?
                Util.MessageConfirm("SFU1974", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RMTRL_INPUT_REQ_PROC", "INDATA,INDTLDATA", null, (result, ex) =>
                        {
                            try
                            {
                                if (ex != null)
                                {
                                    Util.AlertByBiz("BR_PRD_REG_RMTRL_INPUT_REQ_PROC", ex.Message, ex.ToString());
                                    return;
                                }
                                Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                                dgProductLot.EndEdit(true);
                                dgWorkOrder.EndEdit(true);
                                ClearGrid();
                                this.txtRemark.Clear();
                                this.txtWorker.Clear();
                                this.txtWorker.Tag = string.Empty;

                                this.DialogResult = MessageBoxResult.OK;
                                this.Close();
                            }
                            catch (Exception ErrEx)
                            {
                                Util.MessageException(ErrEx);
                            }
                        }, inDataSet);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void ClearGrid()
        {
            try
            {
                for (int i = 0; i < dgProductLot.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                    DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "MTRL_QTY", string.Empty);
                    DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "HOPPER_ID", "");
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
        }
        
        private bool FinalInputMtrlCheck(string HopperID, string MtrlID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("HOPPER_ID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = _EQPTID;
                Indata["HOPPER_ID"] = HopperID;
                Indata["PRODID"] = _ProdID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_FINL_INPUT_MTRL", "INDATA", "OUTDATA", IndataTable);

                if (dtMain.Rows[0]["CHK_FLAG"].ToString() == "N")
                {
                    DataTableConverter.SetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "CHK_FLAG", "N");
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void chkProc_Checked(object sender, RoutedEventArgs e)
        {
            GetWorkOrder();
        }

        private void chkProc_Unchecked(object sender, RoutedEventArgs e)
        {
            GetWorkOrder();
        }

        private void txtWorker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void dgProductLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                if (string.Equals(e.Cell.Column.Name, "HOPPER_ID"))
                                {
                                    C1ComboBox combo = e.Cell.Presenter.Content as C1ComboBox;
                                    if (combo != null)
                                    {
                                        combo.IsDropDownOpenChanged -= dgProductLot_DropDownOpenChanged;
                                        combo.IsDropDownOpenChanged += dgProductLot_DropDownOpenChanged;

                                        combo.SelectionCommitted -= dgProductLot_SelectedCommited;
                                        combo.SelectionCommitted += dgProductLot_SelectedCommited;
                                    }
                                }
                            }
                        }
                    }
                }));
            }
        }

        private void dgProductLot_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
        }

        private void dgProductLot_DropDownOpenChanged(object sender, PropertyChangedEventArgs<bool> e)
        {
            if (e.NewValue == true)
            {
                C1ComboBox combo = sender as C1ComboBox;
                if (combo != null)
                {
                    if (Convert.ToBoolean(DataTableConverter.GetValue(dgProductLot.Rows[((DataGridCellPresenter)combo.Parent).Cell.Row.Index].DataItem, "CHK")) == false)
                    {
                        combo.IsDropDownOpen = false;
                        Util.MessageValidation("SFU1828");  //자재를 선택하세요.
                        return;
                    }
                }
            }
        }

        private void dgProductLot_SelectedCommited(object sender, PropertyChangedEventArgs<object> e)
        {
            int iSelectIdx = -1;
            C1ComboBox comboBox = sender as C1ComboBox;
            if (comboBox != null)
            {
                DataGridCellPresenter cellPresenter = comboBox.Parent as DataGridCellPresenter;
                if (cellPresenter != null && cellPresenter.Cell != null && cellPresenter.Cell.Row != null)
                    iSelectIdx = cellPresenter.Cell.Row.Index;
            }

            if (iSelectIdx < 0)
                return;


            if (dgProductLot.CurrentRow == null || iSelectIdx < 0 || string.IsNullOrEmpty(Util.NVC(e.NewValue)))
                return;

            try
            {
                if (dgProductLot.GetRowCount() == 0)
                    return;

                DataTable dtTmp = DataTableConverter.Convert(dgProductLot.ItemsSource);
                _HopperID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iSelectIdx].DataItem, "HOPPER_ID"));
                _MtrlID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iSelectIdx].DataItem, "MTRLID"));

                //같은 자재, 같은 호퍼 투입 불가능
                for (int icnt = 0; icnt < dgProductLot.Rows.Count; icnt++)
                {
                    if (icnt != iSelectIdx) //현재 선택한 row 제외
                    {
                        if (dtTmp.Rows[icnt]["HOPPER_ID"].ToString() == _HopperID)//dtTmp.Rows[icnt]["MTRLID"].ToString() == _MtrlID && //자재상관없이 같은 호퍼는 선택 불가능
                        {
                            //같은 호퍼ID를 선택하셨습니다.
                            Util.MessageInfo("SFU2852", (result) =>
                            {
                                if (result == MessageBoxResult.OK || result == MessageBoxResult.Cancel)
                                {
                                    DataTableConverter.SetValue(dgProductLot.Rows[iSelectIdx].DataItem, "HOPPER_ID", "");
                                }
                            });
                            return;
                        }
                    }
                }

                if (FinalInputMtrlCheck(_HopperID, _MtrlID))
                {
                    //이미 투입요청된 자재입니다. \r\n다시 요청하시겠습니까? -> 해당 HOPPER에 잔량이 존재합니다. 초기화하고 다시 투입하시겠습니까?
                    Util.MessageConfirm("SFU1778", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataTableConverter.SetValue(dgProductLot.Rows[iSelectIdx].DataItem, "CHK_FLAG", "Y");
                        }
                        else
                        {
                            DataTableConverter.SetValue(dgProductLot.Rows[iSelectIdx].DataItem, "CHK_FLAG", "N");
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnReqUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }
        #endregion


        #region Mehod

        private void GetWorkOrder()
        {
            try
            {
                bool isProc = false;

                if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
                {
                    isProc = true;
                    dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Visible;
                }
                else
                    dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;

                Util.gridClear(dgWorkOrder);
                Util.gridClear(dgProductLot);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("STDT", typeof(string));
                IndataTable.Columns.Add("EDDT", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["PROCID"] = Process.COATING;
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue.ToString());
                Indata["EQPTID"] = _EQPTID;
                Indata["STDT"] = ldpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                Indata["EDDT"] = ldpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync(isProc ? "DA_PRD_SEL_WORKORDER_LIST_BY_PROCID_WITH_FP" : "DA_PRD_SEL_WORKORDER_LIST_WITH_FP", "INDATA", "RSLTDT", IndataTable);

                dgWorkOrder.BeginEdit();

                Util.GridSetData(dgWorkOrder, dtMain, FrameOperation, true);
                dgWorkOrder.EndEdit();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void GetWOMaterial(string WOID)
        {
            try
            {
                WO_ID = WOID;
                Util.gridClear(dgProductLot);
                this.txtRemark.Clear();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("WOID", typeof(string));
                IndataTable.AcceptChanges();

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["WOID"] = WO_ID;

                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WO_MTRL_LIST_JC", "INDATA", "OUTDATA", IndataTable, RowSequenceNo: true);
                if (dtMain.Rows.Count == 0)
                {
                    return;
                }
                Util.GridSetData(dgProductLot, dtMain, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private DataTable _Mtrlid()
        {
            IndataTable = inDataSet.Tables.Add("INDTLDATA");
            IndataTable.Columns.Add("MTRLID", typeof(string));
            IndataTable.Columns.Add("MTRL_QTY", typeof(string));
            IndataTable.Columns.Add("HOPPER_ID", typeof(string));
            IndataTable.Columns.Add("CHK_FLAG", typeof(string));

            dgProductLot.EndEdit();
            DataTable dtTop = DataTableConverter.Convert(dgProductLot.ItemsSource);

            foreach (DataRow _iRow in dtTop.Rows)
            {
                if (_iRow["CHK"].Equals(1))
                {
                    if (_iRow["MTRL_QTY"].Equals("") || _iRow["MTRL_QTY"].Equals("0"))
                    {
                        //투입요청수량을 입력 하세요.
                        Util.MessageInfo("SFU1978", (result) =>
                        {
                            Keyboard.Focus(dgProductLot.CurrentCell.DataGrid);
                        });
                        InputFlag = false;
                        return null;
                    }
                    if (Double.Parse(Util.NVC(_iRow["MTRL_QTY"])) < 0)
                    {
                        Util.MessageValidation("SFU1977");   //투입요청수량은 정수만 입력 하세요.
                        InputFlag = false;
                        return null;
                    }
                    else
                    {
                        InputFlag = true;
                    }
                    if (_iRow["HOPPER_ID"].Equals(""))
                    {
                        HopperFlag = false;
                        return null;
                    }
                    else
                    {
                        HopperFlag = true;
                    }

                    IndataTable.ImportRow(_iRow);
                    HopperFlag = true;
                    InputFlag = true;
                }
            }
            return IndataTable;
        }

        
        private void SetGridCboItem(C1ComboBox combo, string sEQPTID, string sMTRLID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("MTRLID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = sEQPTID;
                Indata["MTRLID"] = sMTRLID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_HOPPER_CBO", "INDATA", "RSLTDT", IndataTable);
                combo.ItemsSource = DataTableConverter.Convert(dtMain);

                if (combo != null && combo.Items.Count == 1)
                {
                    DataTable distinctTable = ((DataView)dgProductLot.ItemsSource).Table.DefaultView.ToTable(true, "HOPPER_ID");
                    foreach (DataRow row in distinctTable.Rows)
                        if (string.Equals(row["HOPPER_ID"], dtMain.Rows[0][0]))
                            return;

                    combo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        public T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null)
                return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                T childType = child as T;
                if (childType == null)
                {
                    foundChild = FindChild<T>(child, childName);
                    if (foundChild != null)
                        break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    foundChild = (T)child;
                    break;
                }
            }
            return foundChild;
        }

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtWorker.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPerson.ShowModal()));
                wndPerson.BringToFront();
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = wndPerson.USERNAME;
                txtWorker.Tag = wndPerson.USERID;
            }
        }

        #endregion
    }
}
