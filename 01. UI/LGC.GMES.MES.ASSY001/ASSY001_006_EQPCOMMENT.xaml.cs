/*************************************************************************************
 Created Date : 2017.01.17
      Creator : 신광희
   Decription : 전지 5MEGA-GMES 구축 - Stacking 공정진척 화면 - 설비특이사항 입력 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.17  신광희 : Initial Created.
  2017.01.31  신광희 : Validation Check Logic 수정(중복데이터가 존재합니다.), 팝업 호출 시 조회 메소드 검색 미적용 검색조건 수정(BizRule : DA_PRD_SEL_EQPT_NOTE_LIST)
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Globalization;
using System.Text;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;


namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_006_EQPCOMMENT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_006_EQPCOMMENT : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _lineCode = string.Empty;
        private string _eqptCode = string.Empty;
        private string _eqptName = string.Empty;
        private string _processCode = string.Empty;
        private string _lotid = string.Empty;
        private string _wipSeq = string.Empty;
        private string _shiftCode = string.Empty;

        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ASSY001_006_EQPCOMMENT()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            InitializeCombo();
            InitializedgEquipmentNoteAdd();
        }

        private void InitializeCombo()
        {
            SetDataGridShiftCombo(dgEquipmentNoteAdd.Columns["SHFT_ID"], CommonCombo.ComboStatus.NONE);
            SetDataGridShiftCombo(dgEquipmentNote.Columns["SHFT_ID"], CommonCombo.ComboStatus.NONE);
        }

        private void InitializedgEquipmentNoteAdd()
        {
            DataTable dt = MakeDataTable(dgEquipmentNoteAdd);
            DataRow dr = dt.NewRow();
            dr["WRK_DATE"] = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            dr["EQPTID"] = _eqptCode;
            dt.Rows.Add(dr);
            dgEquipmentNoteAdd.ItemsSource = DataTableConverter.Convert(dt);
            //Util.GridSetData(dgEquipmentNoteAdd, dt, null, true);
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 5)
            {
                _lineCode = Util.NVC(tmps[0]);
                _eqptCode = Util.NVC(tmps[1]);
                _processCode = Util.NVC(tmps[2]);
                _lotid = Util.NVC(tmps[3]);
                _wipSeq = Util.NVC(tmps[4]);
                _eqptName = Util.NVC(tmps[5]);

                txtEqptId.Text = _eqptCode;
                txtEqptName.Text = _eqptName;
            }
            ApplyPermissions();
            InitializeControls();
            SelectEquiptMentNote();
            Loaded -= C1Window_Loaded;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CommonVerify.HasDataGridRow(dgEquipmentNote)) return;
                dgEquipmentNote.EndEdit();
                dgEquipmentNote.EndEditRow(true);
                if (!Validation()) return;
                SaveEquiptMentNote();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CommonVerify.HasDataGridRow(dgEquipmentNote)) return;
                dgEquipmentNote.EndEdit();
                dgEquipmentNote.EndEditRow(true);

                DataTable dt = ((DataView)dgEquipmentNote.ItemsSource).Table;
                var query = (from t in dt.AsEnumerable()
                             where t.Field<Int32>("CHK") == 1
                             select t).ToList();

                if (!query.Any())
                {
                    Util.Alert("333");
                    return;
                }

                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DeleteEquiptMentNote();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgEquipmentNoteAdd.EndEdit();
                dgEquipmentNoteAdd.EndEditRow(true);
                dgEquipmentNote.EndEdit();
                dgEquipmentNote.EndEditRow(true);

                dgEquipmentNote.BeginNewRow();
                dgEquipmentNote.EndNewRow(true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEquipmentNote_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Column != null)
                {
                    DataRowView drv = e.Row.DataItem as DataRowView;
                    if (drv != null)
                    {
                        if (e.Column.Name == "CHK")
                        {
                            if (drv["RANK"].GetString() == "1" || string.IsNullOrEmpty(drv["RANK"].GetString()))
                            {
                                e.Cancel = false;
                            }
                            else
                            {
                                e.Cancel = true;
                            }

                            
                        }
                        else
                        {
                            e.Cancel = drv["CHK"].GetString() != "1";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEquipmentNote_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void EQPT_NOTE_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox textbox = sender as TextBox;

                C1.WPF.DataGrid.DataGridRow row = new C1.WPF.DataGrid.DataGridRow();
                IList<FrameworkElement> ilist = textbox.GetAllParents();

                foreach (var item in ilist)
                {
                    DataGridRowPresenter presenter = item as DataGridRowPresenter;
                    if (presenter != null)
                    {
                        row = presenter.Row;
                    }
                }
                dgEquipmentNote.SelectedItem = row.DataItem;

                if (textbox != null)
                {
                    if (DataTableConverter.GetValue(dgEquipmentNote.SelectedItem, "CHK").GetString() == "1")
                    {
                        textbox.IsReadOnly = false;
                        //textbox.AppendText("string");
                        //textbox.CaretIndex = textbox.Text.Length;
                        //textbox.Focus();
                        //textbox.SelectionStart = textbox.Text.Length;
                        //FocusManager.SetFocusedElement(this, textbox);
                        //textbox.ScrollToEnd();
                        //textbox.Select(textbox.Text.Length, 0);
                        // save current cursor position and selection 
                        int start = textbox.SelectionStart;
                        int length = textbox.SelectionLength;

                        // restore cursor position and selection
                        textbox.Select(start, length);
                    }
                    else
                    {
                        textbox.IsReadOnly = true;
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnWorker_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;

                C1.WPF.DataGrid.DataGridRow row = new C1.WPF.DataGrid.DataGridRow();
                IList<FrameworkElement> ilist = btn.GetAllParents();

                foreach (var item in ilist)
                {
                    DataGridRowPresenter presenter = item as DataGridRowPresenter;
                    if (presenter != null)
                    {
                        row = presenter.Row;
                    }
                }
                dgEquipmentNoteAdd.SelectedItem = row.DataItem;

                if (string.IsNullOrEmpty(DataTableConverter.GetValue(dgEquipmentNoteAdd.SelectedItem, "SHFT_ID").GetString()))
                {
                    //Util.AlertInfo("선택된 작업조가 없습니다.");
                    Util.MessageValidation("SFU1646");
                    return;
                }

                CMM_SHIFT_USER pop = new CMM_SHIFT_USER { FrameOperation = FrameOperation };

                object[] parameters = new object[5];
                parameters[0] = LoginInfo.CFG_SHOP_ID;
                //parameters[1] = "A2";
                parameters[1] = LoginInfo.CFG_AREA_ID;
                parameters[2] = _lineCode;
                //parameters[3] = "A7000";
                parameters[3] = _processCode;
                parameters[4] = string.Empty;
                C1WindowExtension.SetParameters(pop, parameters);

                pop.Closed += ShiftUser_Closed;
                Dispatcher.BeginInvoke(new Action(() => pop.ShowModal()));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnRegUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;

                C1.WPF.DataGrid.DataGridRow row = new C1.WPF.DataGrid.DataGridRow();
                IList<FrameworkElement> ilist = btn.GetAllParents();

                foreach (var item in ilist)
                {
                    DataGridRowPresenter presenter = item as DataGridRowPresenter;
                    if (presenter != null)
                    {
                        row = presenter.Row;
                    }
                }
                dgEquipmentNote.SelectedItem = row.DataItem;

                if (string.IsNullOrEmpty(DataTableConverter.GetValue(dgEquipmentNote.SelectedItem, "SHFT_ID").GetString()))
                {
                    //Util.AlertInfo("선택된 작업조가 없습니다.");
                    Util.MessageValidation("SFU1646");
                    return;
                }
                if (DataTableConverter.GetValue(dgEquipmentNote.SelectedItem, "CHK").GetString() != "1")
                    return;

                CMM_SHIFT_USER pop = new CMM_SHIFT_USER { FrameOperation = FrameOperation };

                object[] parameters = new object[5];
                parameters[0] = LoginInfo.CFG_SHOP_ID;
                //parameters[1] = "A2";
                parameters[1] = LoginInfo.CFG_AREA_ID;
                parameters[2] = _lineCode;
                //parameters[3] = "A7000";
                parameters[3] = _processCode;
                parameters[4] = string.Empty;
                C1WindowExtension.SetParameters(pop, parameters);

                pop.Closed += RegUser_Closed;
                Dispatcher.BeginInvoke(new Action(() => pop.ShowModal()));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER popup = sender as CMM_SHIFT_USER;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                DataTableConverter.SetValue(dgEquipmentNoteAdd.SelectedItem, "REG_USERID", popup.USERID);
                DataTableConverter.SetValue(dgEquipmentNoteAdd.SelectedItem, "REG_USERNAME", popup.USERNAME);
                dgEquipmentNoteAdd.EndEdit();
                dgEquipmentNoteAdd.EndEditRow(true);
            }
        }

        private void RegUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER popup = sender as CMM_SHIFT_USER;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                DataTableConverter.SetValue(dgEquipmentNote.SelectedItem, "REG_USERID", popup.USERID);
                DataTableConverter.SetValue(dgEquipmentNote.SelectedItem, "REG_USERNAME", popup.USERNAME);
                dgEquipmentNote.EndEdit();
                dgEquipmentNote.EndEditRow(true);
            }
        }

        private void dgEquipmentNote_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                DataRowView drv = dgEquipmentNote.CurrentRow.DataItem as DataRowView;
                if (e.Cell.Column.Name == "CHK")
                {
                    if (DataTableConverter.GetValue(drv, "CHK").GetString() == "1")
                    {
                        dgEquipmentNote.SelectedIndex = e.Cell.Row.Index;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEquipmentNote_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            try
            {
                e.Item.SetValue("CHK", 1);
                e.Item.SetValue("WRK_DATE", DataTableConverter.GetValue(dgEquipmentNoteAdd.Rows[0].DataItem, "WRK_DATE").GetString());
                e.Item.SetValue("EQPT_NOTE", DataTableConverter.GetValue(dgEquipmentNoteAdd.Rows[0].DataItem, "EQPT_NOTE").GetString());
                e.Item.SetValue("SHFT_ID", DataTableConverter.GetValue(dgEquipmentNoteAdd.Rows[0].DataItem, "SHFT_ID").GetString());
                e.Item.SetValue("REG_USERID", DataTableConverter.GetValue(dgEquipmentNoteAdd.Rows[0].DataItem, "REG_USERID").GetString());
                e.Item.SetValue("REG_USERNAME", DataTableConverter.GetValue(dgEquipmentNoteAdd.Rows[0].DataItem, "REG_USERNAME").GetString());
                e.Item.SetValue("EQPTID", _eqptCode);
                e.Item.SetValue("RANK", string.Empty);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEquipmentNote_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }


        #endregion

        #region Mehod

        private void SelectEquiptMentNote()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgEquipmentNote);
                const string bizRuleName = "DA_PRD_SEL_EQPT_NOTE_LIST";

                DataTable inDataTable = new DataTable();

                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("WRK_DATE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["WRK_DATE"] = DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                inData["EQPTID"] = _eqptCode;
                inData["AREAID"] = LoginInfo.CFG_AREA_ID;
                inData["EQSGID"] = _lineCode;
                inData["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inData["PROCID"] = _processCode;
                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    dgEquipmentNote.ItemsSource = DataTableConverter.Convert(result);
                    //Util.GridSetData(dgEquipmentNote, result, null, true);

                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void SaveEquiptMentNote()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "BR_PRD_REG_EQUIPMENT_NOTE";

                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("WRK_DATE", typeof(string));
                inDataTable.Columns.Add("SHFT_ID", typeof(string));
                inDataTable.Columns.Add("EQPT_NOTE", typeof(string));
                inDataTable.Columns.Add("REG_USERID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                //inDataTable.Columns.Add("DATASTATE", typeof(string));
                inDataTable.Columns.Add("DEL_FLAG", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgEquipmentNote.Rows)
                {
                    if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "1")
                    {
                        DataRow param = inDataTable.NewRow();
                        param["EQPTID"] = DataTableConverter.GetValue(row.DataItem, "EQPTID");
                        param["WRK_DATE"] = DataTableConverter.GetValue(row.DataItem, "WRK_DATE").GetString().Replace("-", "");
                        param["SHFT_ID"] = DataTableConverter.GetValue(row.DataItem, "SHFT_ID");
                        param["EQPT_NOTE"] = DataTableConverter.GetValue(row.DataItem, "EQPT_NOTE");
                        param["REG_USERID"] = DataTableConverter.GetValue(row.DataItem, "REG_USERNAME");
                        //param["DATASTATE"] = ((DataRowView)row.DataItem).Row.RowState.ToString().ToUpper() != "ADDED" ? DataRowState.Modified.ToString().ToUpper() : DataRowState.Added.ToString().ToUpper();
                        param["DEL_FLAG"] = "N";
                        param["USERID"] = LoginInfo.USERID;

                        inDataTable.Rows.Add(param);
                    }
                }

                if (inDataTable.Rows.Count < 1)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    Util.AlertInfo("MMD0002");
                    return;
                }

                //DataSet ds = new DataSet();
                //ds.Tables.Add(inDataTable);
                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                    }
                    else
                    {
                        Util.MessageInfo("10004");
                        SelectEquiptMentNote();
                    }
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void DeleteEquiptMentNote()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "BR_PRD_REG_EQUIPMENT_NOTE";

                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("WRK_DATE", typeof(string));
                inDataTable.Columns.Add("SHFT_ID", typeof(string));
                inDataTable.Columns.Add("EQPT_NOTE", typeof(string));
                inDataTable.Columns.Add("REG_USERID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                //inDataTable.Columns.Add("DATASTATE", typeof(string));
                inDataTable.Columns.Add("DEL_FLAG", typeof(string));

                for (int i = dgEquipmentNote.Rows.Count; 0 < i; i--)
                {
                    DataRowView drv = dgEquipmentNote.Rows[i - 1].DataItem as DataRowView;

                    if (Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i - 1].DataItem, "CHK")) == "1")
                    {
                        if (drv != null)
                        {
                            if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
                            {
                                dgEquipmentNote.RemoveRow(i - 1);
                            }
                            else
                            {
                                DataRow param = inDataTable.NewRow();
                                param["EQPTID"] = DataTableConverter.GetValue(dgEquipmentNote.Rows[i - 1].DataItem, "EQPTID");
                                param["WRK_DATE"] = DataTableConverter.GetValue(dgEquipmentNote.Rows[i - 1].DataItem, "WRK_DATE").GetString().Replace("-", "");
                                param["SHFT_ID"] = DataTableConverter.GetValue(dgEquipmentNote.Rows[i - 1].DataItem, "SHFT_ID");
                                param["EQPT_NOTE"] = DataTableConverter.GetValue(dgEquipmentNote.Rows[i - 1].DataItem, "EQPT_NOTE");
                                param["REG_USERID"] = DataTableConverter.GetValue(dgEquipmentNote.Rows[i - 1].DataItem, "REG_USERNAME");
                                param["USERID"] = LoginInfo.USERID;
                                //param["DATASTATE"] = ((DataRowView)dgEquipmentNote.Rows[i - 1].DataItem).Row.RowState.ToString().ToUpper() != "ADDED" ? DataRowState.Modified.ToString().ToUpper() : DataRowState.Added.ToString().ToUpper();
                                param["DEL_FLAG"] = "Y";

                                inDataTable.Rows.Add(param);
                            }
                        }
                    }
                }

                if (inDataTable.Rows.Count < 1)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    Util.MessageInfo("SFU1273");
                    return;
                }

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    else
                    {
                        Util.MessageInfo("SFU1273");
                        SelectEquiptMentNote();
                    }
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private bool Validation()
        {
            try
            {
                if (CommonVerify.HasDataGridRow(dgEquipmentNote))
                {
                    DataTable dt = ((DataView)dgEquipmentNote.ItemsSource).Table;

                    var queryGroup = (from t in dt.AsEnumerable()
                                      where t.Field<Int32>("CHK") == 1
                                      group t by new
                                      {
                                          eqptCode = t.Field<string>("EQPTID"),
                                          wrkDate = t.Field<string>("WRK_DATE"),
                                          shiftId = t.Field<string>("SHFT_ID")
                                      }
                        into grp
                                      select new
                                      {
                                          EqptCode = grp.Key.eqptCode,
                                          WorkDate = grp.Key.wrkDate,
                                          ShiftId = grp.Key.shiftId,
                                          grpCount = grp.Count()
                                      }).ToList();

                    string eqptId = ObjectDic.Instance.GetObjectName("EQPTID");
                    string workdt = ObjectDic.Instance.GetObjectName("WRK_DATE");
                    string shiftId = ObjectDic.Instance.GetObjectName("SHFT_ID");

                    if (queryGroup.Any(g => g.grpCount > 1))
                    {
                        //Util.AlertInfo("MMD0067");
                        StringBuilder sb = new StringBuilder();
                        sb.Append(Environment.NewLine);
                        sb.Append(eqptId + " : " + queryGroup.Select(r => r.EqptCode).FirstOrDefault() + Environment.NewLine);
                        sb.Append(workdt + " : " + queryGroup.Select(r => r.WorkDate).FirstOrDefault() + Environment.NewLine);
                        sb.Append(shiftId + " : " + queryGroup.Select(r => r.ShiftId).FirstOrDefault() + Environment.NewLine);

                        Util.MessageValidation("SFU2051", sb.ToString());
                        return false;
                    }

                    var queryEdit = (from t in dt.AsEnumerable()
                                     where t.Field<Int32>("CHK") == 1
                                     select t).ToList();

                    if (queryEdit.Any())
                    {
                        foreach (var item in queryEdit)
                        {
                            if (string.IsNullOrEmpty(Util.NVC(item["EQPTID"])))
                            {
                                //ControlsLibrary.MessageBox.Show("설비 ID를 입력하세요.", null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                Util.MessageValidation("10013", ObjectDic.Instance.GetObjectName("EQPTID"));
                                return false;
                            }
                            if (string.IsNullOrEmpty(Util.NVC(item["WRK_DATE"])))
                            {
                                //ControlsLibrary.MessageBox.Show("작업일자 를 입력하세요.", null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                Util.MessageValidation("10013", ObjectDic.Instance.GetObjectName("WRK_DATE"));
                                return false;
                            }
                            if (string.IsNullOrEmpty(Util.NVC(item["SHFT_ID"])))
                            {
                                //ControlsLibrary.MessageBox.Show("작업조 ID를 입력하세요.", null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                Util.MessageValidation("10013", ObjectDic.Instance.GetObjectName("SHFT_ID"));
                                return false;
                            }

                            // 필수 값 중복체크
                            var query = (from t in dt.AsEnumerable()
                                         where (t.Field<Int32>("CHK") != 1)
                                               && (t.Field<string>("RANK") != null && !string.IsNullOrEmpty(t.Field<string>("RANK")))
                                               && t.Field<string>("EQPTID") == (string)item["EQPTID"]
                                               && t.Field<string>("WRK_DATE") == (string)item["WRK_DATE"]
                                               && t.Field<string>("SHFT_ID") == (string)item["SHFT_ID"]
                                         select t).Distinct();
                            if (query.Any())
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.Append(Environment.NewLine);
                                sb.Append(eqptId + " : " + (string)item["EQPTID"] + Environment.NewLine);
                                sb.Append(workdt + " : " + (string)item["WRK_DATE"] + Environment.NewLine);
                                sb.Append(shiftId + " : " + (string)item["SHFT_ID"] + Environment.NewLine);

                                Util.MessageValidation("SFU2051", sb.ToString());
                                return false;
                            }

                        }
                    }
                    else
                    {
                        Util.MessageValidation("MMD0002");
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button> { btnSave, btnDelete, btnAdd };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SetDataGridShiftCombo(C1.WPF.DataGrid.DataGridColumn dgcol, CommonCombo.ComboStatus status)
        {
            const string bizRuleName = "DA_BAS_SEL_SHIFT_CBO";
            string[] arrColumn = { "SHOPID", "AREAID", "EQSGID", "PROCID", "SHFT_ID", "USE_FLAG" };
            string[] arrCondition = { LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, _lineCode, _processCode, null, null };
            string selectedValueText = dgcol.SelectedValuePath();
            string displayMemberText = dgcol.DisplayMemberPath();
            CommonCombo.SetDataGridComboItem(bizRuleName, arrColumn, arrCondition, status, dgcol, selectedValueText, displayMemberText);
        }

        private static DataTable MakeDataTable(C1DataGrid dg)
        {
            DataTable dt = new DataTable();
            try
            {

                dt.Columns.Add("WRK_DATE", typeof(string));
                dt.Columns.Add("EQPT_NOTE", typeof(string));
                dt.Columns.Add("SHFT_ID", typeof(string));
                dt.Columns.Add("REG_USERID", typeof(string));
                dt.Columns.Add("REG_USERNAME", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));

                //if (dg.ItemsSource == null)
                //{
                //    foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                //    {
                //        if (!string.IsNullOrEmpty(col.Name))
                //        {
                //            dt.Columns.Add(col.Name);
                //        }
                //    }
                //    return dt;
                //}
                //dt = ((DataView)dg.ItemsSource).Table;
                return dt;
            }
            catch (Exception)
            {
                return dt;
            }
        }
        #endregion

        private enum TransactionType
        {
            Added,
            Modified,
            Deleted
        }



    }
}
