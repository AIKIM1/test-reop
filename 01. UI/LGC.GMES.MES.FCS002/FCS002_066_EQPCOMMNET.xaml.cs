using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Globalization;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// FCS002_066_EQPCOMMNET.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_066_EQPCOMMNET : UserControl, IWorkArea
    {

        #region Declaration & Constructor
        private string _lineCode = string.Empty;
        private string _eqptCode = string.Empty;
        private string _eqptName = string.Empty;
        private string _processCode = string.Empty;
        private string _shiftCode = string.Empty;
        private string _shiftName = string.Empty;
        private string _workerCode = string.Empty;
        private string _workerName = string.Empty;

        private string _lotid = string.Empty;
        private string _wipSeq = string.Empty;

        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        /// 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public FCS002_066_EQPCOMMNET()
        {
            InitializeComponent();
        }
        private void InitializeControls()
        {
            InitializedgEquipmentNoteAdd();
        }

        private void InitializedgEquipmentNoteAdd()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_PRODUCT_PRJT_NAME";

                string selectedValueText = (string)((PopupFindDataColumn)dgEquipmentNoteAdd.Columns["PRJT_NAME"]).SelectedValuePath;
              //  string displayMemberText = (string)((PopupFindDataColumn)dgEquipmentNoteAdd.Columns["PRJT_NAME"]).DisplayMemberPath;

                DataTable inDataTable = new DataTable("RQSTDT");

                inDataTable.Columns.Add("PRJT_NAME", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["PRJT_NAME"] = null;
                inDataTable.Rows.Add(dr);
                

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { selectedValueText});

                PopupFindDataColumn column = dgEquipmentNoteAdd.Columns["PRJT_NAME"] as PopupFindDataColumn;
                column.AddMemberPath = "PRJT_NAME";
                column.ItemsSource = DataTableConverter.Convert(dtBinding);

                dr = null;

                DataTable dt = MakeDataTable(dgEquipmentNoteAdd);
                dr = dt.NewRow();
                dr["WRK_DATE"] = GetEquipmentWorkDate();
                dr["EQPTID"] = _eqptCode;

                if (!string.IsNullOrEmpty(_shiftCode))
                    dr["SHFT_ID"] = _shiftCode;
                if (!string.IsNullOrEmpty(_shiftName))
                    dr["SHIFTNAME"] = _shiftName;
                if (!string.IsNullOrEmpty(_workerCode))
                    dr["REG_USERID"] = _workerCode;
                if (!string.IsNullOrEmpty(_workerName))
                    dr["REG_USERNAME"] = _workerName.Replace(",", Environment.NewLine);

                dt.Rows.Add(dr);
                //dgEquipmentNoteAdd.ItemsSource = DataTableConverter.Convert(dt);
                Util.GridSetData(dgEquipmentNoteAdd, dt, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            }
        #endregion

        #region Event



        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //object[] parameters = C1WindowExtension.GetParameters(this);

            object[] parameters = this.FrameOperation.Parameters;

            if (parameters.Length == 6)
            {
                // 기존 파라메터 던지던 폼에서 에러나서 기존 6개 파라메터는 기존대로 SET하게 변경
                _lineCode = Util.NVC(parameters[0]);
                _eqptCode = Util.NVC(parameters[1]);
                _processCode = Util.NVC(parameters[2]);
                _lotid = Util.NVC(parameters[3]);
                _wipSeq = Util.NVC(parameters[4]);
                _eqptName = Util.NVC(parameters[5]);

                txtEqptId.Text = _eqptCode;
                txtEqptName.Text = _eqptName;
            }
            if (parameters.Length > 6)
            {
                _lineCode = Util.NVC(parameters[0]);
                _eqptCode = Util.NVC(parameters[1]);
                _processCode = Util.NVC(parameters[2]);
                _lotid = Util.NVC(parameters[3]);
                _wipSeq = Util.NVC(parameters[4]);
                _eqptName = Util.NVC(parameters[5]);
                _shiftName = Util.NVC(parameters[6]);
                _shiftCode = Util.NVC(parameters[7]);
                _workerName = Util.NVC(parameters[8]);
                _workerCode = Util.NVC(parameters[9]);

                txtEqptId.Text = _eqptCode;
                txtEqptName.Text = _eqptName;
            }

            ApplyPermissions();
            InitializeControls();
            SelectEquiptMentNote();
            //Loaded -= C1Window_Loaded;
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
                    Util.MessageValidation("333");      //삭제하려는 값이 존재하지 않습니다.
                    return;
                }

                //삭제 하시겠습니까?
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

            dgEquipmentNoteAdd.EndEdit();
            dgEquipmentNoteAdd.EndEditRow(true);

            string workDate = string.Format("{0:yyyy-MM-dd}", DataTableConverter.GetValue(dgEquipmentNoteAdd.Rows[0].DataItem, "WRK_DATE"));
            string eqptCode = DataTableConverter.GetValue(dgEquipmentNoteAdd.Rows[0].DataItem, "EQPTID").GetString();
            string shiftCode = DataTableConverter.GetValue(dgEquipmentNoteAdd.Rows[0].DataItem, "SHFT_ID").GetString();
            string eqptNote = DataTableConverter.GetValue(dgEquipmentNoteAdd.Rows[0].DataItem, "EQPT_NOTE").GetString();
            string pjt = DataTableConverter.GetValue(dgEquipmentNoteAdd.Rows[0].DataItem, "PRJT_NAME").GetString();
            string regUserName = DataTableConverter.GetValue(dgEquipmentNoteAdd.Rows[0].DataItem, "REG_USERNAME").GetString();
            string regUserId = DataTableConverter.GetValue(dgEquipmentNoteAdd.Rows[0].DataItem, "REG_USERID").GetString();

            if (workDate != GetEquipmentWorkDate())
            {
                //StringBuilder sb = new StringBuilder();
                //sb.Append("입력하려는 작업일자와 현재의 작업일자가 일치하지 않습니다.");
                //sb.Append(Environment.NewLine);
                //sb.Append("설비 특이사항 입력 메뉴를 다시 호출하시기 바랍니다. ");
                Util.MessageValidation("SFU3122");
                return;
            }

            if (string.IsNullOrEmpty(workDate))
            {
                Util.MessageValidation("10013", ObjectDic.Instance.GetObjectName("WRK_DATE"));
                return;
            }
            if (string.IsNullOrEmpty(eqptCode))
            {
                Util.MessageValidation("10013", ObjectDic.Instance.GetObjectName("EQPTID"));
                return;
            }
            if (string.IsNullOrEmpty(shiftCode))
            {
                Util.MessageValidation("10013", ObjectDic.Instance.GetObjectName("SHFT_ID"));
                return;
            }
            if (string.IsNullOrEmpty(eqptNote))
            {
                Util.MessageValidation("10013", ObjectDic.Instance.GetObjectName("EQPT_NOTE"));
                return;
            }

            if (string.IsNullOrEmpty(pjt))
            {
                Util.MessageValidation("10013", ObjectDic.Instance.GetObjectName("PJT"));
                return;
            }

            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "BR_PRD_REG_EQUIPMENT_NOTE";
                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PRJT_NAME", typeof(string));
                inDataTable.Columns.Add("WRK_DATE", typeof(string));
                inDataTable.Columns.Add("SHFT_ID", typeof(string));
                inDataTable.Columns.Add("EQPT_NOTE", typeof(string));
                inDataTable.Columns.Add("REG_USERID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("DEL_FLAG", typeof(string));
                inDataTable.Columns.Add("NOTE_SEQNO", typeof(string));
                inDataTable.Columns.Add("EQGR_FLAG", typeof(string));

                DataRow param = inDataTable.NewRow();
                param["EQPTID"] = eqptCode;
                param["PRJT_NAME"] = pjt;
                param["WRK_DATE"] = workDate.Replace("-", "");
                param["SHFT_ID"] = shiftCode;
                param["EQPT_NOTE"] = eqptNote;
                param["REG_USERID"] = regUserId;
                param["DEL_FLAG"] = "N";
                param["USERID"] = LoginInfo.USERID;
                param["NOTE_SEQNO"] = string.Empty;
                param["EQGR_FLAG"] = chkGroup.IsChecked == true ? "Y" : "N";

                inDataTable.Rows.Add(param);

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
                        Util.MessageInfo("SFU1270");      //저장되었습니다.
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
                    if (DataTableConverter.GetValue(dgEquipmentNote.SelectedItem, "WRK_DATE").GetString() != GetEquipmentWorkDate())
                    {
                        //Util.MessageValidation("수정 가능한 작업일자의 데이터가 아닙니다.");
                        //Util.MessageValidation("SFU3123");
                        textbox.IsReadOnly = true;
                        return;
                    }
                    else
                    {
                        textbox.IsReadOnly = false;
                        textbox.SelectionStart = textbox.Text.Length;
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

                CMM_SHIFT_USER2 pop = new CMM_SHIFT_USER2 { FrameOperation = this.FrameOperation };
                object[] parameters = new object[8];
                parameters[0] = LoginInfo.CFG_SHOP_ID;
                parameters[1] = LoginInfo.CFG_AREA_ID;
                parameters[2] = _lineCode;
                parameters[3] = _processCode;
                parameters[4] = DataTableConverter.GetValue(dgEquipmentNoteAdd.SelectedItem, "SHFT_ID");
                parameters[5] = DataTableConverter.GetValue(dgEquipmentNoteAdd.SelectedItem, "REG_USERID");
                parameters[6] = _eqptCode;
                parameters[7] = " N";

                C1WindowExtension.SetParameters(pop, parameters);

                pop.Closed += new EventHandler(ShiftUser_Closed);
                Dispatcher.BeginInvoke(new Action(() => pop.ShowModal()));
                //----------------------------
                //C1WindowExtension.SetParameters(pop, parameters);
                //this.FrameOperation.OpenMenu("SFU010130090",true,parameters);
                //===============================
                //pop.Closed += new EventHandler(ShiftUser_Closed);
                //Dispatcher.BeginInvoke(new Action(() => pop.ShowModal()));
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

                if (DataTableConverter.GetValue(dgEquipmentNote.SelectedItem, "WRK_DATE").GetString() != GetEquipmentWorkDate())
                {
                    //Util.MessageValidation("SFU3123");
                    return;
                }

                //if (DataTableConverter.GetValue(dgEquipmentNote.SelectedItem, "CHK").GetString() != "1")
                //    return;

                CMM_SHIFT_USER2 pop = new CMM_SHIFT_USER2 { FrameOperation = FrameOperation };
                object[] parameters = new object[8];
                parameters[0] = LoginInfo.CFG_SHOP_ID;
                parameters[1] = LoginInfo.CFG_AREA_ID;
                parameters[2] = _lineCode;
                parameters[3] = _processCode;
                parameters[4] = DataTableConverter.GetValue(dgEquipmentNote.SelectedItem, "SHFT_ID");
                parameters[5] = DataTableConverter.GetValue(dgEquipmentNote.SelectedItem, "REG_USERID");
                parameters[6] = _eqptCode;
                parameters[7] = "N";
                C1WindowExtension.SetParameters(pop, parameters);

                //pop.Closed += RegUser_Closed;
               // Dispatcher.BeginInvoke(new Action(() => pop.ShowModal()));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 popup = sender as CMM_SHIFT_USER2;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                DataTableConverter.SetValue(dgEquipmentNoteAdd.SelectedItem, "REG_USERID", popup.USERID);
                DataTableConverter.SetValue(dgEquipmentNoteAdd.SelectedItem, "REG_USERNAME", popup.USERNAME.Replace(",", Environment.NewLine));
                DataTableConverter.SetValue(dgEquipmentNoteAdd.SelectedItem, "SHFT_ID", popup.SHIFTCODE);
                DataTableConverter.SetValue(dgEquipmentNoteAdd.SelectedItem, "SHIFTNAME", popup.SHIFTNAME);
                dgEquipmentNoteAdd.EndEdit();
                dgEquipmentNoteAdd.EndEditRow(true);
            }
        }

        private void RegUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 popup = sender as CMM_SHIFT_USER2;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                DataTableConverter.SetValue(dgEquipmentNote.SelectedItem, "REG_USERID", popup.USERID);
                DataTableConverter.SetValue(dgEquipmentNote.SelectedItem, "REG_USERNAME", popup.USERNAME.Replace(",", Environment.NewLine));
                DataTableConverter.SetValue(dgEquipmentNote.SelectedItem, "SHFT_ID", popup.SHIFTCODE);
                DataTableConverter.SetValue(dgEquipmentNote.SelectedItem, "SHIFTNAME", popup.SHIFTNAME);
                dgEquipmentNote.EndEdit();
                dgEquipmentNote.EndEditRow(true);
            }
        }

        //private void btnClose_Click(object sender, RoutedEventArgs e)
        //{
            //this.DialogResult = MessageBoxResult.Cancel;
        //}

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
                            //if (drv["WRK_DATE"].GetString() != GetEquipmentWorkDate())
                            //{
                            //    e.Cancel = true;
                            //}
                            //else
                            //{
                            //    e.Cancel = false;
                            //}
                            e.Cancel = drv["WRK_DATE"].GetString() != GetEquipmentWorkDate();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEquipmentNote_Loaded(object sender, RoutedEventArgs e)
        {
           // DayAgo.IsChecked = true;
        }
        #endregion

        #region Mehod
        private void SelectEquiptMentNote(string from_date = null)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgEquipmentNote);
                const string bizRuleName = "DA_PRD_SEL_EQPT_NOTE_LIST";

                DataTable inDataTable = new DataTable();

                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(string));
                inDataTable.Columns.Add("TO_DATE", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                //inData["WRK_DATE"] = wrk_date == null ? GetEquipmentWorkDate().Replace("-", "") :  ;
                inData["EQPTID"] = _eqptCode;
                inData["AREAID"] = LoginInfo.CFG_AREA_ID;
                inData["EQSGID"] = _lineCode;
                inData["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inData["PROCID"] = _processCode;
                inData["FROM_DATE"] = from_date == null ? GetEquipmentWorkDate().Replace("-", "") : from_date;
                inData["TO_DATE"] = GetEquipmentWorkDate().Replace("-", "");
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
                inDataTable.Columns.Add("DEL_FLAG", typeof(string));
                inDataTable.Columns.Add("NOTE_SEQNO", typeof(string));

                /*
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
                        param["NOTE_SEQNO"] = DataTableConverter.GetValue(row.DataItem, "NOTE_SEQNO");
                        param["USERID"] = LoginInfo.USERID;

                        inDataTable.Rows.Add(param);
                    }
                }
                */

                foreach (C1.WPF.DataGrid.DataGridRow row in dgEquipmentNote.Rows)
                {
                    if (row.Type != DataGridRowType.Item) continue;

                    if (((DataRowView)row.DataItem).Row.RowState == DataRowState.Added || ((DataRowView)row.DataItem).Row.RowState == DataRowState.Modified)
                    {
                        DataRow param = inDataTable.NewRow();
                        param["EQPTID"] = DataTableConverter.GetValue(row.DataItem, "EQPTID");
                        param["WRK_DATE"] = DataTableConverter.GetValue(row.DataItem, "WRK_DATE").GetString().Replace("-", "");
                        param["SHFT_ID"] = DataTableConverter.GetValue(row.DataItem, "SHFT_ID");
                        param["EQPT_NOTE"] = DataTableConverter.GetValue(row.DataItem, "EQPT_NOTE");
                        param["REG_USERID"] = DataTableConverter.GetValue(row.DataItem, "REG_USERID");
                        param["DEL_FLAG"] = "N";
                        param["NOTE_SEQNO"] = DataTableConverter.GetValue(row.DataItem, "NOTE_SEQNO");
                        param["USERID"] = LoginInfo.USERID;

                        inDataTable.Rows.Add(param);
                    }
                }

                if (inDataTable.Rows.Count < 1)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    Util.MessageValidation("MMD0002");      //저장할 데이터가 존재하지 않습니다.
                    return;
                }

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                    }
                    else
                    {
                        Util.MessageInfo("SFU1270");      //저장되었습니다.
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
                inDataTable.Columns.Add("DEL_FLAG", typeof(string));
                inDataTable.Columns.Add("NOTE_SEQNO", typeof(string));

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
                                param["REG_USERID"] = DataTableConverter.GetValue(dgEquipmentNote.Rows[i - 1].DataItem, "REG_USERID");
                                param["USERID"] = LoginInfo.USERID;
                                param["DEL_FLAG"] = "Y";
                                param["NOTE_SEQNO"] = DataTableConverter.GetValue(dgEquipmentNote.Rows[i - 1].DataItem, "NOTE_SEQNO");

                                inDataTable.Rows.Add(param);
                            }
                        }
                    }
                }

                if (inDataTable.Rows.Count < 1)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    Util.MessageInfo("SFU1273");      //삭제되었습니다.
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
                        Util.MessageInfo("SFU1273");      //삭제되었습니다.
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

                    var queryEdit = (from t in dt.AsEnumerable()
                                     where t.RowState == DataRowState.Modified
                                     select t).ToList();

                    if (queryEdit.Any())
                    {
                        foreach (var item in queryEdit)
                        {
                            if (string.IsNullOrEmpty(Util.NVC(item["EQPTID"])))
                            {
                                Util.MessageValidation("10013", ObjectDic.Instance.GetObjectName("EQPTID")); //"설비 ID를 입력하세요.
                                return false;
                            }
                            if (string.IsNullOrEmpty(Util.NVC(item["WRK_DATE"])))
                            {
                                Util.MessageValidation("10013", ObjectDic.Instance.GetObjectName("WRK_DATE")); //작업일자 를 입력하세요.
                                return false;
                            }
                            if (string.IsNullOrEmpty(Util.NVC(item["SHFT_ID"])))
                            {
                                Util.MessageValidation("10013", ObjectDic.Instance.GetObjectName("SHFT_ID")); //작업조 ID를 입력하세요.
                                return false;
                            }

                        }
                    }
                    else
                    {
                        Util.MessageValidation("MMD0002");      //저장할 데이터가 존재하지 않습니다.
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
            List<Button> listAuth = new List<Button> { btnDelete }; //             { btnSave, btnDelete, btnAdd };
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

        private static string GetEquipmentWorkDate()
        {
            string returnWorkDate = string.Empty;
            try
            {
                const string bizRuleName = "DA_PRD_SEL_EQPT_NOTE_WORKDATE";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("AREAID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["AREAID"] = LoginInfo.CFG_AREA_ID;
                inDataTable.Rows.Add(inData);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", inDataTable);
                if (CommonVerify.HasTableRow(dtResult))
                {
                    returnWorkDate = dtResult.Rows[0]["CALDATE_YYYY"] + "-" + dtResult.Rows[0]["CALDATE_MM"] + "-" + dtResult.Rows[0]["CALDATE_DD"];
                }
                return returnWorkDate;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return returnWorkDate;
            }
        }

        private static DataTable MakeDataTable(C1DataGrid dg)
        {
            DataTable dt = new DataTable();
            try
            {

                dt.Columns.Add("WRK_DATE", typeof(string));
                dt.Columns.Add("PRJT_NAME", typeof(string));
                dt.Columns.Add("EQPT_NOTE", typeof(string));
                dt.Columns.Add("SHFT_ID", typeof(string));
                dt.Columns.Add("SHIFTNAME", typeof(string));
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

        private void PeriodOptionSelected(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            radioButton.Name = radioButton.Name == null ? "" : radioButton.Name;
            string work_date = GetEquipmentWorkDate().Replace("-", "");
            DateTime dt = Convert.ToDateTime(string.Format("{0:0000}-{1:00}-{2:00}", work_date.Substring(0, 4), work_date.Substring(4, 2), work_date.Substring(6, 2)));
            string fr_date = work_date;

            switch (radioButton.Name)
            {
                //당일
                case "Today":
                    dt = dt.AddDays(0);
                    break;

                //1주전
                case "WeekAgo":
                    dt = dt.AddDays(-7);
                    break;

                //1달전
                case "MonthAgo":
                    dt = dt.AddMonths(-1);
                    break;

                //1일전
                case "DayAgo":
                default:
                    dt = dt.AddDays(-1);
                    break;
            }
            fr_date = dt.ToString("yyyy-MM-dd").Replace("-", "");
            SelectEquiptMentNote(from_date: fr_date);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgEquipmentNote);
                const string bizRuleName = "DA_PRD_SEL_EQPT_NOTE_LIST";

                DataTable inDataTable = new DataTable();

                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(string));
                inDataTable.Columns.Add("TO_DATE", typeof(string));
                inDataTable.Columns.Add("EQGR_FLAG", typeof(string));
                inDataTable.Columns.Add("PRJT_NAME", typeof(string));
                inDataTable.Columns.Add("EQOT_NOTE", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                //inData["WRK_DATE"] = wrk_date == null ? GetEquipmentWorkDate().Replace("-", "") :  ;
                inData["EQPTID"] = _eqptCode;
                inData["AREAID"] = LoginInfo.CFG_AREA_ID;
                inData["EQSGID"] = _lineCode;
                inData["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inData["PROCID"] = _processCode;
                inData["PRJT_NAME"] = txtPjt.Text;
                inData["EQOT_NOTE"] = txtNote.Text;
                //   inData["FROM_DATE"] = from_date == null ? GetEquipmentWorkDate().Replace("-", "") : from_date;
                //  inData["TO_DATE"] = GetEquipmentWorkDate().Replace("-", "");
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
    }
}