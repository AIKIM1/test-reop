/*************************************************************************************
 Created Date : 2017.02.16
      Creator : 신 광희
   Decription : Package 공정 일별/시간별 생산계획 입력(입력된 데이터를 통하여 SMS 전송 대상자에게 메세지를 발송 함)
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.16  신 광희 : Initial Created.
  2017.03.02  신 광희 : 생산계획 셀 선택 시 이벤트 수정
  2017.03.10  신 광희 : TB_MMD_SMS_GR_EQPT 테이블 추가로 인한 Biz 및 UI 수정
  2017.05.22  신 광희 : MMD 설비 담당 전화 번호 컬럼(SEND_USER_FLAG) 추가로 인한 프로그램 수정
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using Button = System.Windows.Controls.Button;
using TextBox = System.Windows.Controls.TextBox;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_117 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public ELEC001_117()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private DataTable _dtBaseTable = new DataTable();
        private readonly DataTable _dtPlanQty = new DataTable();
        private readonly DataTable _dtPhone = new DataTable();
        private readonly string _smsGroupCode = "SMS_KO_ELTR_001";
        private DateTime _dtSystemTime = new DateTime();
        private string _senderPhoneNo = string.Empty;
        private string _chkGorup = "";

        private Util _Util = new Util();

        #endregion

        #region Initialize
        private void Initialize()
        {
            //dtpDateMonth.SelectedDateTime = DateTime.Now;
            GetSystemTime();
            InitializeCombo();
            _dtPlanQty.Columns.Add("SEQ", typeof(string));
            _dtPlanQty.Columns.Add("PLAN_QTY", typeof(string));

            _dtPhone.Columns.Add("SMS_GR_ID", typeof(string));
            _dtPhone.Columns.Add("EQPTID", typeof(string));
            _dtPhone.Columns.Add("CHARGE_USER_PHONE_NO", typeof(string));
            _dtPhone.Columns.Add("USE_FLAG", typeof(string));
            _dtPhone.Columns.Add("SEND_USER_FLAG", typeof(string));
        }

        private void InitializeCombo()
        {
            SetEquipmentSegmentCombo(cboEquipmentSegment);
            SetProcessCombo(cboProcess);
            SetEquipmentCombo(cboEquipment);

        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            Initialize();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.GetString() == "SELECT")
            //{
            //    Util.MessageValidation("SFU1673");
            //    return;
            //}

            SelectEquipmentPlan();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            dgEquipmentPlan.EndEdit();
            dgEquipmentPlan.EndEditRow(true);

            dgSMSTargetPhoneList.EndEdit();
            dgSMSTargetPhoneList.EndEditRow(true);

            if (!Validation())
                return;

            if (!IsValidSendPhoneNo())
            {
                txtSendPhoneNo.Select(0, txtSendPhoneNo.Text.Length);
                return;
            }

            SaveEquipmentPlan();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (CommonVerify.IsValidPhoneNumber(txtPhoneNo.Text))
            {
                if (txtSendPhoneNo.Text.Equals(txtPhoneNo.Text))
                {
                    Util.MessageValidation("SFU3611");
                    txtPhoneNo.Select(0, txtPhoneNo.Text.Length);
                    return;
                }

                DataTable dt = Util.MakeDataTable(dgSMSTargetPhoneList, true);
                DataRow dr = dt.NewRow();
                dr["CHARGE_USER_PHONE_NO"] = txtPhoneNo.Text;
                dr["EQPTID"] = cboEquipment.SelectedValue;
                dr["SMS_GR_ID"] = _smsGroupCode;
                dr["SEND_USER_FLAG"] = "N";

                dt.Rows.Add(dr);
                dgSMSTargetPhoneList.ItemsSource = DataTableConverter.Convert(dt);
            }
            else
            {
                Util.MessageValidation("SFU3179");
                txtPhoneNo.Select(0, txtPhoneNo.Text.Length);
                return;
            }
        }

        private void dgSMSTargetPhoneList_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            e.Item.SetValue("CHARGE_USER_PHONE_NO", txtPhoneNo.Text);
            e.Item.SetValue("EQPTID", cboEquipment.SelectedValue); //수정
            e.Item.SetValue("SMS_GR_ID", _smsGroupCode);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            C1.WPF.DataGrid.DataGridRow row = new C1.WPF.DataGrid.DataGridRow();
            System.Collections.Generic.IList<FrameworkElement> ilist = btn.GetAllParents();

            foreach (var item in ilist)
            {
                DataGridRowPresenter presenter = item as DataGridRowPresenter;
                if (presenter != null)
                {
                    row = presenter.Row;
                }
            }
            dgSMSTargetPhoneList.SelectedItem = row.DataItem;
            if (((DataRowView)row.DataItem).Row.RowState.GetString().ToUpper() != "ADDED")
            {
                DataRow dr = _dtPhone.NewRow();
                dr["SMS_GR_ID"] = DataTableConverter.GetValue(dgSMSTargetPhoneList.SelectedItem, "SMS_GR_ID");
                dr["EQPTID"] = DataTableConverter.GetValue(dgSMSTargetPhoneList.SelectedItem, "EQPTID"); // 수정
                dr["SEND_USER_FLAG"] = DataTableConverter.GetValue(dgSMSTargetPhoneList.SelectedItem, "SEND_USER_FLAG");
                dr["CHARGE_USER_PHONE_NO"] = DataTableConverter.GetValue(dgSMSTargetPhoneList.SelectedItem, "CHARGE_USER_PHONE_NO");
                dr["USE_FLAG"] = "N";
                _dtPhone.Rows.Add(dr);
            }
            dgSMSTargetPhoneList.RemoveRow(row.Index);

        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipment.SelectedValue != null && cboEquipment.SelectedValue.GetString() != "SELECT")
            {
                btnSearch_Click(btnSearch, null);
            }
        }

        private void txtSendPhoneNo_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                TextBox text = sender as TextBox;
                if (text != null)
                {
                    text.Text = Util.AutoHypenPhone(text.Text);
                    if (!IsValidSendPhoneNo())
                    {
                        text.Select(0, text.Text.Length);
                        return;
                    }

                    text.Select(text.Text.Length, 0);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtPhoneNo_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                btnAdd_Click(btnAdd, null);
            }

            txtPhoneNo.Text = Util.AutoHypenPhone(txtPhoneNo.Text);
            txtPhoneNo.Select(txtPhoneNo.Text.Length, 0);
        }

        private void txtPhoneNo_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string phoneNo = txtPhoneNo.Text.Trim();
            foreach (char c in phoneNo)
            {
                if (!char.IsDigit(c) && c != '-')
                {
                    e.Handled = true;
                    break;
                }
            }
        }
        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetProcessCombo(cboProcess);
            SetEquipmentCombo(cboEquipment);
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEquipmentCombo(cboEquipment);
        }

        private void dtpDateMonth_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (dtPik != null && Convert.ToDecimal(_dtSystemTime.ToString("yyyyMM")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMM")))
                {
                    dtPik.Text = _dtSystemTime.ToString("yyyyMM");
                    //dtPik.SelectedDateTime = System.DateTime.Now;
                    dtPik.SelectedDateTime = _dtSystemTime;
                    dtPik.DatepickerType = LGCDataPickerType.Month;

                    //오늘 이전 날짜는 선택할 수 없습니다.
                    Util.MessageValidation("SFU3124");
                    //e.Handled = false;
                    return;
                }
                //if (cboEquipment.SelectedValue != null && cboEquipment.SelectedValue.GetString() != "SELECT")
                //{
                    btnSearch_Click(btnSearch, null);
                //}
            }));
        }

        private void dgEquipmentPlan_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            DataRowView drv = e.Row.DataItem as DataRowView;
            if (drv != null)
            {
                switch (drv["WEEKOFDAY"].GetString())
                {
                    case "토":
                    case "土":
                    case "Sat":
                        e.Row.Presenter.Background = new SolidColorBrush(System.Windows.Media.Colors.LightBlue);
                        break;

                    case "일":
                    case "Sun":
                    case "日":
                        e.Row.Presenter.Background = new SolidColorBrush(System.Windows.Media.Colors.HotPink);
                        break;

                    default:
                        e.Row.Presenter.Background = null;
                        break;
                }
            }
        }

        private void dgEquipmentPlan_UnloadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.Presenter != null)
            {
                e.Row.Presenter.Background = null;
            }
        }

        private void dgEquipmentPlan_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e?.Cell?.Row != null)
            {
                DataRowView drv = e.Cell.Row.DataItem as DataRowView;
                if (e.Cell.Column.Name == "PLAN_QTY")
                {
                    Int64 totalQty = DataTableConverter.GetValue(drv, "PLAN_QTY").GetInt();
                    Int64 averageQty;

                    if (totalQty == 0)
                    {
                        averageQty = 0;
                    }
                    else
                    {
                        averageQty = totalQty;
                    }
             

                    for (int i = dgEquipmentPlan.Columns.Count - 1; 2 < i; i--)
                    {
                        C1.WPF.DataGrid.DataGridColumn dgcol = dgEquipmentPlan[e.Cell.Row.Index, i].Column;

                        if (dgcol is DataGridNumericColumn)
                        {
                                DataTableConverter.SetValue(drv, dgEquipmentPlan.Columns[i].Name, averageQty);                            
                        }
                    }
                    dgEquipmentPlan.EndEditRow(true);
                }
            }
        }

        private void dgEquipmentPlan_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.V && KeyboardUtil.Ctrl)
                {
                    string clipboard = Clipboard.GetText();
                    if (string.IsNullOrWhiteSpace(clipboard) || dgEquipmentPlan.Selection.SelectedRanges.Count != 1 || ((C1.WPF.DataGrid.C1DataGrid)sender).CurrentColumn.Name != "PLAN_QTY")
                    {
                        e.Handled = true;
                        return;
                    }

                    if (!CommonVerify.HasDataGridRow(dgEquipmentPlan)) return;

                    int rowIdx = dgEquipmentPlan.CurrentRow.Index - dgEquipmentPlan.TopRows.Count;

                    string[] stringSeparators = new string[] { "\r\n" };
                    string[] table = clipboard.Split(stringSeparators, StringSplitOptions.None);

                    if (table.Length == 1)
                    {
                        //다시 복사 해주세요.
                        Util.MessageValidation("SFU1482");
                        e.Handled = true;
                    }
                    else
                    {
                        _dtPlanQty.Clear();

                        for (int i = 0; i < table.Length - 1; i++)
                        {
                            string[] rowTab = table[i].Split('\t');
                            if (rowTab.Length > 1)
                            {
                                Util.MessageValidation("SFU1482");
                                e.Handled = true;
                                return;
                            }

                            DataRow row = _dtPlanQty.NewRow();
                            row["SEQ"] = (rowIdx + i).GetString();
                            row["PLAN_QTY"] = rowTab[0].GetString().Replace(",", "").Trim();
                            _dtPlanQty.Rows.Add(row);
                        }
                        MakeEquipmentPlan();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                e.Handled = true;
                //Util.GridSetData(dgEquipmentPlan, _dtBaseTable, FrameOperation, true);
            }
        }

        private void dgEquipmentPlan_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgEquipmentPlan.GetCellFromPoint(pnt);

            if (cell?.Value == null)
            {
                return;
            }

            CalculationdgEquipmentPlan(dgEquipmentPlan, cell);
        }


        #endregion

        #region Mehod

        private void SelectEquipmentPlan()
        {
            try
            {
                _senderPhoneNo = "";

                if (!string.IsNullOrEmpty(txtSendPhoneNo.Text))
                    txtSendPhoneNo.Text = string.Empty;

                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgEquipmentPlan);
                Util.gridClear(dgSMSTargetPhoneList);
                _dtPhone.Clear();

                const string bizRuleName = "BR_PRD_GET_ELEC_EQUIPMENT_PLAN_RSLT";
                DataTable inDataTable = new DataTable();

                inDataTable.Columns.Add("PLAN_YRM", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("SMS_GR_ID", typeof(string));
                inDataTable.Columns.Add("SMS_DETL_GR_ID", typeof(string));
                inDataTable.Columns.Add("ATTR01", typeof(string));
                inDataTable.Columns.Add("ATTR02", typeof(string));
                inDataTable.Columns.Add("ATTR03", typeof(string));
                inDataTable.Columns.Add("ATTR04", typeof(string));
                inDataTable.Columns.Add("ATTR05", typeof(string));
                inDataTable.Columns.Add("GRCHK", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["PLAN_YRM"] = dtpDateMonth.SelectedDateTime.ToString("yyyyMM");
                if (!_chkGorup.Equals("Y"))
                {
                    inData["ATTR03"] = cboEquipment.SelectedValue;
                }
                else
                {
                    inData["GRCHK"] = "Y";
                }                
                inData["EQPTID"] = cboEquipment.SelectedValue;
                inData["PROCID"] = cboProcess.SelectedValue;
                inData["SMS_GR_ID"] = _smsGroupCode;  //_smsGroupCode
                if (_chkGorup.Equals("Y"))
                {
                    inData["SMS_DETL_GR_ID"] = LoginInfo.CFG_AREA_ID.ToString() + "_" + cboProcess.SelectedValue;
                }
                else
                {
                    inData["SMS_DETL_GR_ID"] = LoginInfo.CFG_AREA_ID.ToString() + "_" + cboProcess.SelectedValue + "_" + cboEquipment.SelectedValue;
                }
                inData["ATTR01"] = LoginInfo.CFG_AREA_ID;
                inData["ATTR02"] = cboProcess.SelectedValue;
                inData["ATTR04"] = null;
                inData["ATTR05"] = null;

                inDataTable.Rows.Add(inData);
                DataSet ds = new DataSet();
                ds.Tables.Add(inDataTable);
                string xml = ds.GetXml();


                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", "OUT_PLANRESULT,OUT_PHONELIST,OUT_SMSGROUP", (retds, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (CommonVerify.HasTableRow(retds.Tables["OUT_PLANRESULT"]))
                    {
                        retds.Tables["OUT_PLANRESULT"].Columns.Remove("EQPTID");
                        Util.GridSetData(dgEquipmentPlan, retds.Tables["OUT_PLANRESULT"], null, true);
                    }
                    else
                    {
                        _dtPlanQty.Clear();
                        MakeEquipmentPlan();
                    }

                    var query = (from t in retds.Tables["OUT_PHONELIST"].AsEnumerable()
                                 where t.Field<string>("SEND_USER_FLAG") == "Y"
                                 select new { SendUserPhoneNo = t.Field<string>("CHARGE_USER_PHONE_NO") }).FirstOrDefault();
                    if (query != null)
                    {
                        txtSendPhoneNo.Text = query.SendUserPhoneNo;
                        _senderPhoneNo = query.SendUserPhoneNo;
                    }

                    //Util.GridSetData(dgSMSTargetPhoneList, retds.Tables["OUT_PHONELIST"], null, true);

                    if (CommonVerify.HasTableRow(retds.Tables["OUT_PHONELIST"]))
                    {
                        DataRow[] drs = retds.Tables["OUT_PHONELIST"].Select("SEND_USER_FLAG= 'N' ");
                        dgSMSTargetPhoneList.ItemsSource = drs != null && drs.Length > 0 ? DataTableConverter.Convert(drs.CopyToDataTable()) : null;
                    }

                    if (CommonVerify.HasTableRow(retds.Tables["OUT_SMSGROUP"]))
                    {
                        chksmsYn.IsChecked = retds.Tables["OUT_SMSGROUP"].Rows[0]["USE_FLAG"].GetString() == "Y";
                    }
                    else
                    {
                        chksmsYn.IsChecked = false;
                    }

                }, ds, FrameOperation.MENUID);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MakeEquipmentPlan()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                DataTable dtResult = GetTimeElpsCode();

                //if (!CommonVerify.HasDataGridRow(dgEquipmentPlan))
                if (dgEquipmentPlan.Rows.Count == (dgEquipmentPlan.Rows.TopRows.Count + dgEquipmentPlan.Rows.BottomRows.Count) || !CommonVerify.HasDataGridRow(dgEquipmentPlan))
                {
                    DataTable bindingTable = new DataTable { TableName = "RQSTDT" };
                    bindingTable.Columns.Add("PRODDATE", typeof(string));
                    bindingTable.Columns.Add("WEEKOFDAY", typeof(string));
                    bindingTable.Columns.Add("PLAN_QTY", typeof(string));

                    if (CommonVerify.HasTableRow(dtResult))
                    {
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            string item = dtResult.Rows[i][0].GetString();
                            string column = "PLAN_QTY_";
                            bindingTable.Columns.Add(column + item, typeof(string));
                        }
                    }
                    DateTime dt = new DateTime(dtpDateMonth.SelectedDateTime.Year, dtpDateMonth.SelectedDateTime.Month, 1);
                    for (int i = 1; i <= dt.AddMonths(1).AddDays(-1).Day; i++)
                    {
                        DataRow dr = bindingTable.NewRow();
                        for (int j = 0; j < bindingTable.Columns.Count; j++)
                        {
                            if (j == 0)
                            {
                                dr[bindingTable.Columns[j].ColumnName] = i.GetString().Length == 1 ? "0" + i.GetString() : i.GetString();
                            }
                            else if (j == 1)
                            {
                                DateTime dtWeekofDay = new DateTime(dtpDateMonth.SelectedDateTime.Year, dtpDateMonth.SelectedDateTime.Month, i);
                                dr[bindingTable.Columns[j].ColumnName] = Util.GetDayOfWeek(dtWeekofDay);
                            }
                            else
                            {
                                dr[bindingTable.Columns[j].ColumnName] = 0;
                            }
                        }
                        bindingTable.Rows.Add(dr);
                    }
                    _dtBaseTable = bindingTable.Copy();
                    Util.GridSetData(dgEquipmentPlan, bindingTable, FrameOperation, true);
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
                else
                {
                    DataTable bindingTable = Util.MakeDataTable(dgEquipmentPlan, true);
                    for (int i = 0; i < bindingTable.Rows.Count; i++)
                    {
                        for (int j = 0; j < bindingTable.Columns.Count; j++)
                        {
                            int rowIndex = i;

                            if (CommonVerify.HasTableRow(_dtPlanQty))
                            {
                                var query = (from t in _dtPlanQty.AsEnumerable()
                                             where t.Field<string>("SEQ") == rowIndex.GetString()
                                             select new
                                             {
                                                 planQty = t.Field<string>("PLAN_QTY")
                                             }).FirstOrDefault();

                                if (j == 2)
                                {
                                    if (query != null)
                                    {
                                        bindingTable.Rows[i][j] = query.planQty.GetInt();
                                    }
                                }
                                if (j > 2)
                                {
                                    Int64 remainderQty = 0;

                                    if (query != null)
                                    {
                                        Int64 totalQty = query.planQty.GetInt();
                                        Int64 averageQty = totalQty / dtResult.Rows.Count;

                                        if (averageQty != 0)
                                        {
                                            remainderQty = totalQty;
                                        }

                                        if (j == bindingTable.Columns.Count - 1)
                                        {
                                            bindingTable.Rows[i][j] = averageQty + remainderQty;
                                        }
                                        else
                                        {
                                            bindingTable.Rows[i][j] = averageQty;
                                        }
                                    }
                                }

                            }
                        }
                    }
                    _dtBaseTable = bindingTable.Copy();
                    Util.GridSetData(dgEquipmentPlan, bindingTable, FrameOperation, true);
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }


            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }

        }

        private void CalculationdgEquipmentPlan(C1DataGrid dg, C1.WPF.DataGrid.DataGridCell cell)
        {
            int rowIndex = cell.Row.Index;
            int colIndex = cell.Column.Index;

            DataRowView drv = dg.Rows[rowIndex].DataItem as DataRowView;
            //DataRowView drv = dgEquipmentNote.CurrentRow.DataItem as DataRowView;

            if (drv == null) return;

            if (cell.Column.Name.Contains("PLAN_QTY_TM"))
            {
                if (!CommonVerify.IsInt(cell.Value.GetString()))
                {
                    cell.Value = null;
                    return;
                }

                Int64 totalQty = DataTableConverter.GetValue(drv, "PLAN_QTY").GetInt();

                C1.WPF.DataGrid.DataGridColumn dgColumn = dg[rowIndex, colIndex].Column;

                if (dgColumn is DataGridNumericColumn)
                {
                    int blockcol = 0;
                    for (int j = 3; j < dg.Columns.Count; j++)
                    {
                        string col = dg[rowIndex, j].Column.Name;

                        if (string.IsNullOrEmpty(DataTableConverter.GetValue(drv, col).GetString()) || DataTableConverter.GetValue(drv, col).GetString() == "0")
                            blockcol = blockcol + 1;
                    }

                    if (blockcol > 4 && DataTableConverter.GetValue(drv, cell.Column.Name).GetString() != "0")
                    {
                        return;
                    }

                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(drv, cell.Column.Name).GetString()) || DataTableConverter.GetValue(drv, cell.Column.Name).GetString() == "0")
                    {
                        DataTableConverter.SetValue(dg.Rows[rowIndex].DataItem, cell.Column.Name, 1);
                        blockcol = blockcol - 1;
                    }
                    else
                    {
                        DataTableConverter.SetValue(dg.Rows[rowIndex].DataItem, cell.Column.Name, 0);
                        blockcol = blockcol + 1;
                    }

                    Int64 averageQty;
                    Int64 remainderQty = 0;

                    if (totalQty == 0)
                    {
                        averageQty = 0;
                        remainderQty = 0;
                    }
                    else
                    {
                        int activecol = 6 - blockcol;
                        if (activecol == 6)
                        {
                            averageQty = totalQty;
                            if (averageQty != 0)
                            {
                                remainderQty = totalQty;
                            }
                        }
                        else
                        {
                            averageQty = totalQty;
                            if (averageQty != 0)
                            {
                                remainderQty = totalQty;
                            }
                        }
                    }

                    bool isLastcol = false;

                    for (int i = dg.Columns.Count - 1; 2 < i; i--)
                    {
                        C1.WPF.DataGrid.DataGridColumn dgcol = dg[rowIndex, i].Column;
                        if (dgcol is DataGridNumericColumn)
                        {
                            if (string.IsNullOrEmpty(DataTableConverter.GetValue(drv, dgcol.Name).GetString()) ||
                                DataTableConverter.GetValue(drv, dgcol.Name).GetString() == "0")
                            {
                                DataTableConverter.SetValue(dg.Rows[rowIndex].DataItem, dg.Columns[i].Name, 0);
                            }
                            else
                            {
                                if (isLastcol == false)
                                {
                                    //DataTableConverter.SetValue(drv, dg.Columns[i].Name, averageQty + remainderQty);
                                    DataTableConverter.SetValue(dg.Rows[rowIndex].DataItem, dg.Columns[i].Name, averageQty + remainderQty);
                                    isLastcol = true;
                                }
                                else
                                {
                                    //DataTableConverter.SetValue(drv, dg.Columns[i].Name, averageQty);
                                    DataTableConverter.SetValue(dg.Rows[rowIndex].DataItem, dg.Columns[i].Name, averageQty);
                                }
                            }
                        }
                    }
                    dgEquipmentPlan.EndEdit();
                    dgEquipmentPlan.EndEditRow(true);
                }
            }
        }

        private bool Validation()
        {
            try
            {
                if (!CommonVerify.HasDataGridRow(dgEquipmentPlan))
                {
                    Util.MessageValidation("MMD0002");
                    return false;
                }

                //if (!CommonVerify.HasDataGridRow(dgSMSTargetPhoneList) && (chksmsYn.IsChecked == true))
                //{
                //    Util.MessageValidation("SFU3125");
                //    return false;
                //}

                //if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.GetString() == "SELECT")
                //{
                //    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                //    return false;
                //}

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

        }

        private bool IsValidSendPhoneNo()
        {
            bool bRet = false;

            try
            {
                // #1 전송대상 리스트에 존재하는지 확인
                if (txtSendPhoneNo.Text.Length > 0 && dgSMSTargetPhoneList.Rows.Count > 0)
                {
                    int idx = _Util.GetDataGridRowIndex(dgSMSTargetPhoneList, "CHARGE_USER_PHONE_NO", txtSendPhoneNo.Text);
                    if (idx >= 0)
                    {
                        Util.MessageValidation("SFU3611");
                        return bRet;
                    }
                }

                bRet = true;
            }
            catch (Exception ex)
            {
                ;
            }

            return bRet;
        }

        private void SaveEquipmentPlan()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "BR_PRD_REG_ELEC_EQUIPMENT_PLAN";

                DataSet ds = new DataSet();
                DataTable inPlan = ds.Tables.Add("INPLAN");
                inPlan.Columns.Add("PRODDATE", typeof(string));
                inPlan.Columns.Add("EQPTID", typeof(string));
                inPlan.Columns.Add("TIME_ELPS_CODE", typeof(string));
                inPlan.Columns.Add("PLAN_QTY", typeof(Decimal));
                inPlan.Columns.Add("USERID", typeof(string));

                DataTable inPhone = ds.Tables.Add("INPHONE");
                inPhone.Columns.Add("SMS_GR_ID", typeof(string));
                inPhone.Columns.Add("SMS_DETL_GR_ID", typeof(string));
                //inPhone.Columns.Add("EQPTID", typeof(string));
                inPhone.Columns.Add("CHARGE_USER_PHONE_NO", typeof(string));
                inPhone.Columns.Add("USE_FLAG", typeof(string));
                inPhone.Columns.Add("USERID", typeof(string));
                inPhone.Columns.Add("SEND_USER_FLAG", typeof(string));
                inPhone.Columns.Add("ATTR01", typeof(string));
                inPhone.Columns.Add("ATTR02", typeof(string));
                inPhone.Columns.Add("ATTR03", typeof(string));
                inPhone.Columns.Add("ATTR04", typeof(string));
                inPhone.Columns.Add("ATTR05", typeof(string));

                DataTable inGroup = ds.Tables.Add("INGROUP");
                inGroup.Columns.Add("SMS_GR_ID", typeof(string));
                inGroup.Columns.Add("EQPTID", typeof(string));
                inGroup.Columns.Add("USE_FLAG", typeof(string));
                inGroup.Columns.Add("USERID", typeof(string));


                foreach (C1.WPF.DataGrid.DataGridRow row in dgEquipmentPlan.Rows)
                {
                    if (row.Type == DataGridRowType.Top) continue;

                    for (int i = 3; i < dgEquipmentPlan.Columns.Count; i++)
                    {
                        DataRow param = inPlan.NewRow();
                        param["PRODDATE"] = dtpDateMonth.SelectedDateTime.ToString("yyyyMM") + DataTableConverter.GetValue(row.DataItem, "PRODDATE");
                        param["EQPTID"] = cboEquipment.SelectedValue;
                        param["TIME_ELPS_CODE"] = dgEquipmentPlan.Columns[i].Name.Right(7);
                        param["PLAN_QTY"] = DataTableConverter.GetValue(row.DataItem, dgEquipmentPlan.Columns[i].Name) == null ? DBNull.Value : DataTableConverter.GetValue(row.DataItem, dgEquipmentPlan.Columns[i].Name);
                        param["USERID"] = LoginInfo.USERID;
                        inPlan.Rows.Add(param);
                    }
                }

                if (string.IsNullOrEmpty(_senderPhoneNo))
                {
                    if (!string.IsNullOrEmpty(txtSendPhoneNo.Text))
                    {
                        DataRow dr = inPhone.NewRow();
                        dr["SMS_GR_ID"] = _smsGroupCode;
                        if (_chkGorup.Equals("Y"))
                        {
                            dr["SMS_DETL_GR_ID"] = LoginInfo.CFG_AREA_ID.ToString() + "_" + cboProcess.SelectedValue;
                        }
                        else
                        {
                            dr["SMS_DETL_GR_ID"] = LoginInfo.CFG_AREA_ID.ToString() + "_" + cboProcess.SelectedValue + "_" + cboEquipment.SelectedValue;
                        }                        
                        //dr["EQPTID"] = cboEquipment.SelectedValue;
                        dr["CHARGE_USER_PHONE_NO"] = txtSendPhoneNo.Text;
                        dr["USE_FLAG"] = "Y";
                        dr["USERID"] = LoginInfo.USERID;
                        dr["SEND_USER_FLAG"] = "Y";
                        dr["ATTR01"] = LoginInfo.CFG_AREA_ID.ToString();
                        dr["ATTR02"] = cboProcess.SelectedValue;
                        if (_chkGorup.Equals("Y"))
                        {
                            dr["ATTR03"] = null;
                        }
                        else
                        {                            
                            dr["ATTR03"] = cboEquipment.SelectedValue;
                        }                        
                        dr["ATTR04"] = null;
                        dr["ATTR05"] = null;
                        inPhone.Rows.Add(dr);
                    }
                }
                else
                {
                    if (_senderPhoneNo.Equals(txtSendPhoneNo.Text))
                    {
                        DataRow dr = inPhone.NewRow();
                        dr["SMS_GR_ID"] = _smsGroupCode;
                        if (_chkGorup.Equals("Y"))
                        {
                            dr["SMS_DETL_GR_ID"] = LoginInfo.CFG_AREA_ID.ToString() + "_" + cboProcess.SelectedValue;
                        }
                        else
                        {
                            dr["SMS_DETL_GR_ID"] = LoginInfo.CFG_AREA_ID.ToString() + "_" + cboProcess.SelectedValue + "_" + cboEquipment.SelectedValue;
                        }
                        //dr["EQPTID"] = cboEquipment.SelectedValue;
                        dr["CHARGE_USER_PHONE_NO"] = txtSendPhoneNo.Text;
                        dr["USE_FLAG"] = "Y";
                        dr["USERID"] = LoginInfo.USERID;
                        dr["SEND_USER_FLAG"] = "Y";
                        dr["ATTR01"] = LoginInfo.CFG_AREA_ID.ToString();
                        dr["ATTR02"] = cboProcess.SelectedValue;
                        if (_chkGorup.Equals("Y"))
                        {
                            dr["ATTR03"] = null;
                        }
                        else
                        {
                            dr["ATTR03"] = cboEquipment.SelectedValue;                            
                        }
                        dr["ATTR04"] = null;
                        dr["ATTR05"] = null;
                        inPhone.Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = inPhone.NewRow();
                        dr["SMS_GR_ID"] = _smsGroupCode;
                        if (_chkGorup.Equals("Y"))
                        {
                            dr["SMS_DETL_GR_ID"] = LoginInfo.CFG_AREA_ID.ToString() + "_" + cboProcess.SelectedValue;
                        }
                        else
                        {
                            dr["SMS_DETL_GR_ID"] = LoginInfo.CFG_AREA_ID.ToString() + "_" + cboProcess.SelectedValue + "_" + cboEquipment.SelectedValue;
                        }
                        //dr["EQPTID"] = cboEquipment.SelectedValue;
                        dr["CHARGE_USER_PHONE_NO"] = _senderPhoneNo;
                        dr["USE_FLAG"] = "N";
                        dr["USERID"] = LoginInfo.USERID;
                        dr["SEND_USER_FLAG"] = "N";
                        dr["ATTR01"] = LoginInfo.CFG_AREA_ID.ToString();
                        dr["ATTR02"] = cboProcess.SelectedValue;
                        if (_chkGorup.Equals("Y"))
                        {
                            dr["ATTR03"] = null;
                        }
                        else
                        {                            
                            dr["ATTR03"] = cboEquipment.SelectedValue;
                        }
                        dr["ATTR04"] = null;
                        dr["ATTR05"] = null;
                        inPhone.Rows.Add(dr);

                        if (txtSendPhoneNo.Text != "")
                        {
                            DataRow newRow = inPhone.NewRow();
                            newRow["SMS_GR_ID"] = _smsGroupCode;
                            if (_chkGorup.Equals("Y"))
                            {
                                newRow["SMS_DETL_GR_ID"] = LoginInfo.CFG_AREA_ID.ToString() + "_" + cboProcess.SelectedValue;
                            }
                            else
                            {
                                newRow["SMS_DETL_GR_ID"] = LoginInfo.CFG_AREA_ID.ToString() + "_" + cboProcess.SelectedValue + "_" + cboEquipment.SelectedValue;
                            }
                            //dr["EQPTID"] = cboEquipment.SelectedValue;
                            newRow["CHARGE_USER_PHONE_NO"] = txtSendPhoneNo.Text;
                            newRow["USE_FLAG"] = "Y";
                            newRow["USERID"] = LoginInfo.USERID;
                            newRow["SEND_USER_FLAG"] = "Y";
                            newRow["ATTR01"] = LoginInfo.CFG_AREA_ID.ToString();
                            newRow["ATTR02"] = cboProcess.SelectedValue;
                            if (_chkGorup.Equals("Y"))
                            {
                                newRow["ATTR03"] = null;
                            }
                            else
                            {                                
                                newRow["ATTR03"] = cboEquipment.SelectedValue;
                            }
                            newRow["ATTR04"] = null;
                            newRow["ATTR05"] = null;
                            inPhone.Rows.Add(newRow);
                        }
                    }
                }

                foreach (C1.WPF.DataGrid.DataGridRow row in dgSMSTargetPhoneList.Rows)
                {
                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "CHARGE_USER_PHONE_NO").GetString())) continue;
                    DataRow dr = inPhone.NewRow();
                    dr["SMS_GR_ID"] = _smsGroupCode;
                    if (_chkGorup.Equals("Y"))
                    {
                        dr["SMS_DETL_GR_ID"] = LoginInfo.CFG_AREA_ID.ToString() + "_" + cboProcess.SelectedValue;
                    }
                    else
                    {
                        dr["SMS_DETL_GR_ID"] = LoginInfo.CFG_AREA_ID.ToString() + "_" + cboProcess.SelectedValue + "_" + cboEquipment.SelectedValue;
                    }
                    //dr["EQPTID"] = cboEquipment.SelectedValue;
                    dr["CHARGE_USER_PHONE_NO"] = DataTableConverter.GetValue(row.DataItem, "CHARGE_USER_PHONE_NO");
                    dr["USE_FLAG"] = "Y";
                    dr["USERID"] = LoginInfo.USERID;
                    dr["SEND_USER_FLAG"] = DataTableConverter.GetValue(row.DataItem, "SEND_USER_FLAG");
                    dr["ATTR01"] = LoginInfo.CFG_AREA_ID.ToString();
                    dr["ATTR02"] = cboProcess.SelectedValue;
                    if (_chkGorup.Equals("Y"))
                    {
                        dr["ATTR03"] = null;
                    }
                    else
                    {
                        dr["ATTR03"] = cboEquipment.SelectedValue;
                    }
                    dr["ATTR04"] = null;
                    dr["ATTR05"] = null;
                    inPhone.Rows.Add(dr);
                }

                if (CommonVerify.HasTableRow(_dtPhone))
                {
                    for (int i = 0; i < _dtPhone.Rows.Count; i++)
                    {
                        DataRow dr = inPhone.NewRow();
                        dr["SMS_GR_ID"] = _smsGroupCode;
                        if (_chkGorup.Equals("Y"))
                        {
                            dr["SMS_DETL_GR_ID"] = LoginInfo.CFG_AREA_ID.ToString() + "_" + cboProcess.SelectedValue;
                        }
                        else
                        {
                            dr["SMS_DETL_GR_ID"] = LoginInfo.CFG_AREA_ID.ToString() + "_" + cboProcess.SelectedValue + "_" + cboEquipment.SelectedValue;
                        }
                        //dr["EQPTID"] = cboEquipment.SelectedValue;
                        dr["CHARGE_USER_PHONE_NO"] = _dtPhone.Rows[i]["CHARGE_USER_PHONE_NO"];
                        dr["USE_FLAG"] = _dtPhone.Rows[i]["USE_FLAG"];
                        dr["USERID"] = LoginInfo.USERID;
                        dr["SEND_USER_FLAG"] = "N";
                        dr["ATTR01"] = LoginInfo.CFG_AREA_ID.ToString();
                        dr["ATTR02"] = cboProcess.SelectedValue;
                        if (_chkGorup.Equals("Y"))
                        {
                            dr["ATTR03"] = null;
                        }
                        else
                        {                            
                            dr["ATTR03"] = cboEquipment.SelectedValue;
                        }
                        dr["ATTR04"] = null;
                        dr["ATTR05"] = null;
                        inPhone.Rows.Add(dr);
                    }
                }

                DataRow dataRow = inGroup.NewRow();
                dataRow["SMS_GR_ID"] = _smsGroupCode;
                dataRow["EQPTID"] = cboEquipment.SelectedValue;
                dataRow["USE_FLAG"] = chksmsYn.IsChecked != null && (bool)chksmsYn.IsChecked ? "Y" : "N";
                dataRow["USERID"] = LoginInfo.USERID;
                inGroup.Rows.Add(dataRow);

                string xml = ds.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INPLAN,INPHONE,INGROUP", null, (retds, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("10004");
                    btnSearch_Click(btnSearch, null);

                }, ds, FrameOperation.MENUID);

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void GetSystemTime()
        {
            const string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataTable inDataTable = new DataTable("INDATA");
            new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inDataTable, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }
                else
                {
                    dtpDateMonth.SelectedDateTime = Convert.ToDateTime(result.Rows[0][0]);
                    _dtSystemTime = Convert.ToDateTime(result.Rows[0][0]);
                }
            });
        }

        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
        }

        private void SetProcessCombo(C1ComboBox cbo)
        {
            #region 공정 전체
            const string bizRuleName = "DA_BAS_SEL_PROCESS_CBO";
            string[] arrColumn = { "LANGID", "EQSGID" };
            string[] arrCondition = { LoginInfo.LANGID, Convert.ToString(cboEquipmentSegment.SelectedValue) };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_PROC_ID);
            #endregion 공정 전체
        }

        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "COATER_EQPT_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, Convert.ToString(cboEquipmentSegment.SelectedValue), Convert.ToString(cboProcess.SelectedValue), null };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        private static DataTable GetTimeElpsCode()
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_ATTR_CBO";
            const string commonCodeType = "TIME_ELPS_CODE";

            DataTable inDataTable = new DataTable { TableName = "RQSTDT" };
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("CMCDTYPE", typeof(string));
            inDataTable.Columns.Add("ATTRIBUTE2", typeof(string));
            DataRow dr = inDataTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = commonCodeType;
            dr["ATTRIBUTE2"] = "SMS_KO_ELTR_001";


            inDataTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);
            return dtResult;
        }


        #endregion

        private void chkgroupYn_Checked(object sender, RoutedEventArgs e)
        {
            _chkGorup = "Y";
            SelectEquipmentPlan();

        }

        private void chkgroupYn_UnChecked(object sender, RoutedEventArgs e)
        {
            _chkGorup = "";
            SelectEquipmentPlan();
        }
    }
}
