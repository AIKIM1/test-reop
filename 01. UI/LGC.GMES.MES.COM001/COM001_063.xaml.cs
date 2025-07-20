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
  2018.11.16  김동일  : 기준정보에 의한 유동적 컬럼 조회 및 저장 되도록 수정
  2018.11.22  김동일  : SMS GROUP CODE 하드코딩 제거
  2018.11.29  김동일  : 화면 로딩 시 조회 여러번 되는 문제 수정 및 입력 data 수량 배분로직 bug 수정
  2018.11.30  김동일  : 요일 다국어 처리 수정
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

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_063 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public COM001_063()
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
        private string _smsGroupCode = "SMS_KO_ATO_001";
        private DateTime _dtSystemTime = new DateTime();
        private string _senderPhoneNo = string.Empty;
        //private int iElpsTotCnt = 6;
        private int iCategoryColumnCount = 6;
        private bool isElectrodeLine = false;

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

            // 전극은 PLAN수량 소수점 2자리까지 입력 요청으로 수정 [2019-04-11]
            if (string.Equals(GetAreaType(), "E"))
            {
                isElectrodeLine = true;
                ((DataGridNumericColumn)dgEquipmentPlan.Columns["PLAN_QTY"]).Format = "###,###,###,###.##";
                ((DataGridNumericColumn)dgEquipmentPlan.Columns["FP_PLAN_QTY"]).Format = "###,###,###,###.##";
            }

            GetSMSGroupCode();

            this.cboEquipment.SelectedValueChanged += cboEquipment_SelectedValueChanged;
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

            //btnSearch_Click(btnSearch, null);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.GetString() == "SELECT")
            {
                Util.MessageValidation("SFU1673");
                return;
            }
            
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
            if (CommonVerify.IsValidPhoneNumber(txtPhoneNo.Text) && (string)cboEquipment.SelectedValue != "SELECT")
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
            e.Item.SetValue("EQPTID", cboEquipment.SelectedValue);
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
            if (((DataRowView) row.DataItem).Row.RowState.GetString().ToUpper() != "ADDED")
            {
                DataRow dr = _dtPhone.NewRow();
                dr["SMS_GR_ID"] = DataTableConverter.GetValue(dgSMSTargetPhoneList.SelectedItem, "SMS_GR_ID");
                dr["EQPTID"] = DataTableConverter.GetValue(dgSMSTargetPhoneList.SelectedItem, "EQPTID");
                dr["SEND_USER_FLAG"] = DataTableConverter.GetValue(dgSMSTargetPhoneList.SelectedItem, "SEND_USER_FLAG");
                dr["CHARGE_USER_PHONE_NO"] = DataTableConverter.GetValue(dgSMSTargetPhoneList.SelectedItem, "CHARGE_USER_PHONE_NO");
                dr["USE_FLAG"] = "N";
                _dtPhone.Rows.Add(dr);
            }
            dgSMSTargetPhoneList.RemoveRow(row.Index);

        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (cboEquipment.SelectedValue != null && cboEquipment.SelectedValue.GetString() != "SELECT")
            //{
            //    btnSearch_Click(btnSearch, null);
            //}
            //else
            //{
                // Clear.
                _senderPhoneNo = "";

                if (!string.IsNullOrEmpty(txtSendPhoneNo.Text))
                    txtSendPhoneNo.Text = string.Empty;
                
                Util.gridClear(dgEquipmentPlan);
                Util.gridClear(dgSMSTargetPhoneList);
                _dtPhone.Clear();
            //}
        }

        private void txtSendPhoneNo_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                TextBox text = sender as TextBox;
                if (text != null)
                {
                    text.Text = Util.AutoHypenPhone(text.Text);
                    if(!IsValidSendPhoneNo())
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
                btnAdd_Click(btnAdd,null);    
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
                if (cboEquipment.SelectedValue != null && cboEquipment.SelectedValue.GetString() != "SELECT")
                {
                    //btnSearch_Click(btnSearch, null);
                }
            }));
        }

        private void dgEquipmentPlan_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            DataRowView drv = e.Row.DataItem as DataRowView;

            if (drv != null)
            {
                if (drv["WEEKOFDAY"].GetString().Equals(ObjectDic.Instance.GetObjectName("토")))
                {
                    e.Row.Presenter.Background = new SolidColorBrush(System.Windows.Media.Colors.LightBlue);
                }
                else if (drv["WEEKOFDAY"].GetString().Equals(ObjectDic.Instance.GetObjectName("일")))
                {
                    e.Row.Presenter.Background = new SolidColorBrush(System.Windows.Media.Colors.HotPink);
                }
                else if (String.IsNullOrEmpty(drv["ROW_BACKGROUND"].GetString()) == false)
                {
                    string strColorValue = drv["ROW_BACKGROUND"].GetString();
                    e.Row.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF808080"));
                }
                else
                {
                    e.Row.Presenter.Background = null;
                }
                //switch (drv["WEEKOFDAY"].GetString())
                //{
                //    case "토":
                //    case "土":
                //    case "Sat":
                //        e.Row.Presenter.Background = new SolidColorBrush(System.Windows.Media.Colors.LightBlue);
                //        break;

                    //    case "일":
                    //    case "Sun":
                    //    case "日":
                    //        e.Row.Presenter.Background = new SolidColorBrush(System.Windows.Media.Colors.HotPink);
                    //        break;

                    //    default:
                    //        e.Row.Presenter.Background = null;
                    //        break;
                    //}
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
            try
            {
                if (e?.Cell?.Row != null)
                {
                    DataRowView drv = e.Cell.Row.DataItem as DataRowView;
                    if (e.Cell.Column.Name == "PLAN_QTY")
                    {
                        double totalQty = DataTableConverter.GetValue(drv, "PLAN_QTY").GetDouble();
                        double averageQty;
                        double remainderQty = 0;
                        double iElpsTotCnt = 0;

                        for (int i = iCategoryColumnCount - 1; i < dgEquipmentPlan.Columns.Count; i++)
                        {
                            if (dgEquipmentPlan.Columns[i].Name.StartsWith("PLAN_QTY_"))
                                iElpsTotCnt++;
                        }
                        //double iElpsTotCnt = DataTableConverter.Convert(dgEquipmentPlan.ItemsSource).Columns.Cast<DataColumn>().
                        //                  Where(c => c.ColumnName.StartsWith("PLAN_QTY_", StringComparison.InvariantCulture)).ToArray().Length;

                        if (iElpsTotCnt < 1) iElpsTotCnt = 6;

                        
                        if (totalQty == 0)
                        {
                            averageQty = 0;
                            remainderQty = 0;
                        }
                        else
                        {
                            averageQty = System.Math.Truncate(totalQty / iElpsTotCnt);
                            if (averageQty != 0)
                            {
                                remainderQty = totalQty % iElpsTotCnt;
                            }
                        }

                        bool isLastcol = false;

                        for (int i = dgEquipmentPlan.Columns.Count - 1; iCategoryColumnCount - 1 < i; i--)
                        {
                            C1.WPF.DataGrid.DataGridColumn dgcol = dgEquipmentPlan[e.Cell.Row.Index, i].Column;

                            if (dgcol is DataGridNumericColumn)
                            {
                                if (isLastcol == false)
                                {
                                    DataTableConverter.SetValue(drv, dgEquipmentPlan.Columns[i].Name, averageQty + remainderQty);
                                    isLastcol = true;
                                }
                                else
                                {
                                    DataTableConverter.SetValue(drv, dgEquipmentPlan.Columns[i].Name, averageQty);
                                }
                            }
                        }
                        dgEquipmentPlan.EndEditRow(true);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
                            row["PLAN_QTY"] = rowTab[0].GetString().Replace(",","") .Trim();
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

                for (int i = dgEquipmentPlan.Columns.Count - 1; i >= iCategoryColumnCount; i--)
                {
                    if (dgEquipmentPlan.Columns[i].Name.StartsWith("PLAN_QTY_"))
                        dgEquipmentPlan.Columns.RemoveAt(i);
                }

                const string bizRuleName = "BR_PRD_GET_EQUIPMENT_PLAN_RSLT";
                DataTable inDataTable = new DataTable();

                inDataTable.Columns.Add("PLAN_YRM", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("SMS_GR_ID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("LANGID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["PLAN_YRM"] = dtpDateMonth.SelectedDateTime.ToString("yyyyMM");
                inData["EQPTID"] = cboEquipment.SelectedValue;
                inData["SMS_GR_ID"] = _smsGroupCode;  //_smsGroupCode
                inData["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inData["LANGID"] = LoginInfo.LANGID;

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
                        MakeEquipmentPlan();
                        retds.Tables["OUT_PLANRESULT"].Columns.Remove("EQPTID");

                        //Binding 을 위한 컬럼 명칭 변경.
                        foreach (C1.WPF.DataGrid.DataGridColumn col in dgEquipmentPlan.Columns)
                        {
                            if (col.Name.StartsWith("PLAN_QTY_"))
                            {
                                if (retds.Tables["OUT_PLANRESULT"].Columns.Contains("PLAN_QTY_" + col.Name.Right(4)))
                                    retds.Tables["OUT_PLANRESULT"].Columns["PLAN_QTY_" + col.Name.Right(4)].ColumnName = col.Name;
                            }
                        }
                        Util.GridSetData(dgEquipmentPlan, retds.Tables["OUT_PLANRESULT"], null, true);
                    }
                    else
                    {
                        _dtPlanQty.Clear();
                        MakeEquipmentPlan();
                    }

                    var query = (from t in retds.Tables["OUT_PHONELIST"].AsEnumerable()
                        where t.Field<string>("SEND_USER_FLAG") == "Y"
                        select new {SendUserPhoneNo = t.Field<string>("CHARGE_USER_PHONE_NO") }).FirstOrDefault();
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
                if (dgEquipmentPlan.Rows.Count == (dgEquipmentPlan.Rows.TopRows.Count + dgEquipmentPlan.Rows.BottomRows.Count) || !CommonVerify.HasDataGridRow(dgEquipmentPlan) )
                {
                    DataTable bindingTable = new DataTable {TableName = "RQSTDT"};
                    bindingTable.Columns.Add("PRODDATE", typeof(string));
                    bindingTable.Columns.Add("WEEKOFDAY", typeof(string));
                    bindingTable.Columns.Add("PLAN_QTY", typeof(string));
                    bindingTable.Columns.Add("PLAN_QTY_CHG_DTTM", typeof(string));
                    bindingTable.Columns.Add("FP_PLAN_QTY", typeof(string));
                    bindingTable.Columns.Add("FP_PLAN_QTY_CHG_DTTM", typeof(string));
                    bindingTable.Columns.Add("ROW_BACKGROUND", typeof(string));

                    if (CommonVerify.HasTableRow(dtResult))
                    {
                        int iPrvStartHour = 6;
                        int iInterval = 4;

                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            string item = dtResult.Rows[i][0].GetString();
                            string column = "PLAN_QTY_";
                            bindingTable.Columns.Add(column + item, typeof(string));

                            int iTmp = 0;
                            if (dtResult.Columns.Contains("ATTRIBUTE3") && int.TryParse(dtResult.Rows[i]["ATTRIBUTE3"].ToString(), out iTmp))
                                if (iTmp > 0 && i == 0)
                                    iPrvStartHour = iTmp;
                            if (dtResult.Columns.Contains("ATTRIBUTE1") && int.TryParse(dtResult.Rows[i]["ATTRIBUTE1"].ToString(), out iTmp))
                                if (iTmp > 0)
                                    iInterval = iTmp;

                            string sHeader = (iPrvStartHour > 24 ? iPrvStartHour - 24 : iPrvStartHour).ToString("00") + " ~ " + ((iPrvStartHour + iInterval) > 24 ? (iPrvStartHour + iInterval) - 24 : (iPrvStartHour + iInterval)).ToString("00");
                            iPrvStartHour = iPrvStartHour + iInterval;
                            if (!dgEquipmentPlan.Columns.Contains(column + item))
                            {
                                // 전극은 소수점 2자리까지 수정 [2019-04-11] 
                                if (isElectrodeLine == true)
                                    Util.SetGridColumnNumeric(dgEquipmentPlan, column + item, new System.Collections.Generic.List<string>() { "시간별계획수량", sHeader }, "", true, false, true, true, 150, HorizontalAlignment.Right, Visibility.Visible, "###,###,###,###.##");
                                else
                                    Util.SetGridColumnNumeric(dgEquipmentPlan, column + item, new System.Collections.Generic.List<string>() { "시간별계획수량", sHeader }, "", true, false, true, true, 150, HorizontalAlignment.Right, Visibility.Visible, "###,###,###,###");
                            }
                        }

                        //iElpsTotCnt = dtResult.Rows.Count; // ((System.Collections.Generic.List<DataColumn>)DataTableConverter.Convert(dgEquipmentPlan.ItemsSource).Columns.Cast<DataColumn>().Where(c => !c.ColumnName.StartsWith("PLAN_QTY_", StringComparison.InvariantCultureIgnoreCase))).Count;
                    }
                    DateTime dt = new DateTime(dtpDateMonth.SelectedDateTime.Year, dtpDateMonth.SelectedDateTime.Month,1);
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
                                DateTime dtWeekofDay = new DateTime(dtpDateMonth.SelectedDateTime.Year,dtpDateMonth.SelectedDateTime.Month, i);
                                dr[bindingTable.Columns[j].ColumnName] = Util.GetDayOfWeek(dtWeekofDay);
                            }
                            else if (j > 6)
                            {
                                dr[bindingTable.Columns[j].ColumnName] = 0;
                            }
                        }
                        bindingTable.Rows.Add(dr);
                    }
                    _dtBaseTable = bindingTable.Copy();
                    //for (int i = 0; i < bindingTable.Columns.Count; i++)
                    //{
                    //    if (!dgEquipmentPlan.Columns.Contains(bindingTable.Columns[i].ColumnName))
                    //    {
                    //        Util.SetGridColumnNumeric(dgEquipmentPlan, bindingTable.Columns[i].ColumnName, new System.Collections.Generic.List<string>() { "시간별계획수량", bindingTable.Columns[i].ColumnName }, "", true, false, true, true, 150, HorizontalAlignment.Right, Visibility.Visible, "###,###,###,###");
                    //    }
                    //}
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

                                if (j == 2) //PLAN_QTY 컬럼
                                {
                                    if (query != null)
                                    {
                                        bindingTable.Rows[i][j] = query.planQty.GetInt();
                                    }
                                    //iElpsTotCnt = 0;
                                }
                                if (j >= 3 && j <= 26) //PLAN_QTY_TM01 ~ PLAN_QTY_TM24 컬럼들
                                {
                                    Int64 remainderQty = 0;

                                    if (query != null)
                                    {
                                        Int64 totalQty = query.planQty.GetInt();
                                        Int64 averageQty = totalQty / dtResult.Rows.Count;

                                        if (averageQty != 0)
                                        {
                                            remainderQty = totalQty % averageQty;
                                        }

                                        //if (j == bindingTable.Columns.Count - 1)
                                        if (j == 3 + dtResult.Rows.Count -1)   //화면 그리드의 시간대별 계획 수량 마지막 컬럼 데이터
                                        {
                                            bindingTable.Rows[i][j] = averageQty + remainderQty;
                                        }
                                        else if (j < 3 + dtResult.Rows.Count - 1)   //화면 그리드의 시간대별 계획 수량 마지막 컬럼 외 데이터
                                        {
                                            bindingTable.Rows[i][j] = averageQty;
                                        }
                                    }

                                    //iElpsTotCnt = iElpsTotCnt + 1;
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

            if (cell.Column.Name.Contains("PLAN_QTY_") && colIndex >= iCategoryColumnCount)
            {
                //if (!CommonVerify.IsInt(cell.Value.GetString()))
                //{
                //    cell.Value = null;
                //    return;
                //}

                Int64 totalQty = DataTableConverter.GetValue(drv, "PLAN_QTY").GetInt();

                C1.WPF.DataGrid.DataGridColumn dgColumn = dg[rowIndex, colIndex].Column;

                if (dgColumn is DataGridNumericColumn)
                {
                    int iElpsTotCnt = 0;

                    for (int i = iCategoryColumnCount; i < dgEquipmentPlan.Columns.Count; i++)
                    {
                        if (dgEquipmentPlan.Columns[i].Name.StartsWith("PLAN_QTY_"))
                            iElpsTotCnt++;
                    }
                    //double iElpsTotCnt = DataTableConverter.Convert(dgEquipmentPlan.ItemsSource).Columns.Cast<DataColumn>().
                    //                  Where(c => c.ColumnName.StartsWith("PLAN_QTY_", StringComparison.InvariantCulture)).ToArray().Length;

                    if (iElpsTotCnt < 1) iElpsTotCnt = 6;

                    int blockcol = 0;
                    for (int j = iCategoryColumnCount; j < dg.Columns.Count; j++)
                    {
                        string col = dg[rowIndex, j].Column.Name;

                        if (string.IsNullOrEmpty(DataTableConverter.GetValue(drv, col).GetString()) || DataTableConverter.GetValue(drv, col).GetString() == "0")
                            blockcol = blockcol + 1;
                    }

                    if (blockcol > iElpsTotCnt - 2 && DataTableConverter.GetValue(drv, cell.Column.Name).GetString() != "0")
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
                        int activecol = iElpsTotCnt - blockcol;
                        if (activecol == iElpsTotCnt)
                        {
                            averageQty = totalQty / iElpsTotCnt;
                            if (averageQty != 0)
                            {
                                remainderQty = totalQty % averageQty;
                            }
                        }
                        else
                        {
                            averageQty = totalQty / activecol;
                            if (averageQty != 0)
                            {
                                remainderQty = totalQty % averageQty;
                            }
                        }
                    }

                    bool isLastcol = false;

                    for (int i = dg.Columns.Count - 1; iCategoryColumnCount - 1 < i; i--)
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

                if (!CommonVerify.HasDataGridRow(dgSMSTargetPhoneList) && (chksmsYn.IsChecked == true))
                {
                    Util.MessageValidation("SFU3125");
                    return false;
                }

                if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.GetString() == "SELECT")
                {
                    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
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
                const string bizRuleName = "BR_PRD_REG_EQUIPMENT_PLAN";

                DataSet ds = new DataSet();
                DataTable inPlan = ds.Tables.Add("INPLAN");
                inPlan.Columns.Add("PRODDATE", typeof(string));
                inPlan.Columns.Add("EQPTID", typeof(string));
                inPlan.Columns.Add("TIME_ELPS_CODE", typeof(string));
                inPlan.Columns.Add("PLAN_QTY", typeof(Decimal));
                inPlan.Columns.Add("USERID", typeof(string));

                DataTable inPhone = ds.Tables.Add("INPHONE");
                inPhone.Columns.Add("SMS_GR_ID", typeof(string));
                inPhone.Columns.Add("EQPTID", typeof(string));
                inPhone.Columns.Add("CHARGE_USER_PHONE_NO", typeof(string));
                inPhone.Columns.Add("USE_FLAG", typeof(string));
                inPhone.Columns.Add("USERID", typeof(string));
                inPhone.Columns.Add("SEND_USER_FLAG", typeof(string));

                DataTable inGroup = ds.Tables.Add("INGROUP");
                inGroup.Columns.Add("SMS_GR_ID", typeof(string));
                inGroup.Columns.Add("EQPTID", typeof(string));
                inGroup.Columns.Add("USE_FLAG", typeof(string));
                inGroup.Columns.Add("USERID", typeof(string));


                foreach (C1.WPF.DataGrid.DataGridRow row in dgEquipmentPlan.Rows)
                {
                    if (row.Type == DataGridRowType.Top) continue;

                    for (int i = iCategoryColumnCount; i < dgEquipmentPlan.Columns.Count; i++)
                    {
                        DataRow param = inPlan.NewRow();
                        param["PRODDATE"] = dtpDateMonth.SelectedDateTime.ToString("yyyyMM") + DataTableConverter.GetValue(row.DataItem, "PRODDATE");
                        param["EQPTID"] = cboEquipment.SelectedValue;
                        param["TIME_ELPS_CODE"] = dgEquipmentPlan.Columns[i].Name.Replace("PLAN_QTY_", "");
                        param["PLAN_QTY"] = DataTableConverter.GetValue(row.DataItem, dgEquipmentPlan.Columns[i].Name);
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
                        dr["EQPTID"] = cboEquipment.SelectedValue;
                        dr["CHARGE_USER_PHONE_NO"] = txtSendPhoneNo.Text;
                        dr["USE_FLAG"] = "Y";
                        dr["USERID"] = LoginInfo.USERID;
                        dr["SEND_USER_FLAG"] = "Y";
                        inPhone.Rows.Add(dr);
                    }
                }
                else
                {
                    if (_senderPhoneNo.Equals(txtSendPhoneNo.Text))
                    {
                        DataRow dr = inPhone.NewRow();
                        dr["SMS_GR_ID"] = _smsGroupCode;
                        dr["EQPTID"] = cboEquipment.SelectedValue;
                        dr["CHARGE_USER_PHONE_NO"] = txtSendPhoneNo.Text;
                        dr["USE_FLAG"] = "Y";
                        dr["USERID"] = LoginInfo.USERID;
                        dr["SEND_USER_FLAG"] = "Y";
                        inPhone.Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = inPhone.NewRow();
                        dr["SMS_GR_ID"] = _smsGroupCode;
                        dr["EQPTID"] = cboEquipment.SelectedValue;
                        dr["CHARGE_USER_PHONE_NO"] = _senderPhoneNo;
                        dr["USE_FLAG"] = "N";
                        dr["USERID"] = LoginInfo.USERID;
                        dr["SEND_USER_FLAG"] = "N";
                        inPhone.Rows.Add(dr);

                        if (txtSendPhoneNo.Text != "")
                        {
                            DataRow newRow = inPhone.NewRow();
                            newRow["SMS_GR_ID"] = _smsGroupCode;
                            newRow["EQPTID"] = cboEquipment.SelectedValue;
                            newRow["CHARGE_USER_PHONE_NO"] = txtSendPhoneNo.Text;
                            newRow["USE_FLAG"] = "Y";
                            newRow["USERID"] = LoginInfo.USERID;
                            newRow["SEND_USER_FLAG"] = "Y";
                            inPhone.Rows.Add(newRow);
                        }                        
                    }
                }

                foreach (C1.WPF.DataGrid.DataGridRow row in dgSMSTargetPhoneList.Rows)
                {
                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "CHARGE_USER_PHONE_NO").GetString())) continue;
                    DataRow dr = inPhone.NewRow();
                    dr["SMS_GR_ID"] = _smsGroupCode;
                    dr["EQPTID"] = cboEquipment.SelectedValue;
                    dr["CHARGE_USER_PHONE_NO"] = DataTableConverter.GetValue(row.DataItem, "CHARGE_USER_PHONE_NO");
                    dr["USE_FLAG"] = "Y";
                    dr["USERID"] = LoginInfo.USERID;
                    dr["SEND_USER_FLAG"] = DataTableConverter.GetValue(row.DataItem, "SEND_USER_FLAG");
                    inPhone.Rows.Add(dr);
                }

                if (CommonVerify.HasTableRow(_dtPhone))
                {
                    for (int i = 0; i < _dtPhone.Rows.Count; i++)
                    {
                        DataRow dr = inPhone.NewRow();
                        dr["SMS_GR_ID"] = _smsGroupCode;
                        dr["EQPTID"] = cboEquipment.SelectedValue;
                        dr["CHARGE_USER_PHONE_NO"] = _dtPhone.Rows[i]["CHARGE_USER_PHONE_NO"];
                        dr["USE_FLAG"] = _dtPhone.Rows[i]["USE_FLAG"];
                        dr["USERID"] = LoginInfo.USERID;
                        dr["SEND_USER_FLAG"] = "N";
                        inPhone.Rows.Add(dr);
                    }
                }

                DataRow dataRow = inGroup.NewRow();
                dataRow["SMS_GR_ID"] = _smsGroupCode;
                dataRow["EQPTID"] = cboEquipment.SelectedValue;
                dataRow["USE_FLAG"] = chksmsYn.IsChecked != null && (bool) chksmsYn.IsChecked ? "Y" : "N";
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
                    btnSearch_Click(btnSearch,null);

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
            try
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

                    this.dtpDateMonth.SelectedDataTimeChanged += dtpDateMonth_SelectedDataTimeChanged;
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            string[] arrColumn = { "LANGID", "AREAID"};
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID};
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
            string[] arrCondition = { LoginInfo.LANGID, Convert.ToString(cboEquipmentSegment.SelectedValue), Convert.ToString(cboProcess.SelectedValue), null};
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        private static DataTable GetTimeElpsCode()
        {
            const string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTES";
            const string commonCodeType = "TIME_ELPS_CODE";

            DataTable inDataTable = new DataTable {TableName = "RQSTDT"};
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("CMCDTYPE", typeof(string));
            inDataTable.Columns.Add("ATTRIBUTE2", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = commonCodeType;
            dr["ATTRIBUTE2"] = LoginInfo.CFG_SHOP_ID;// "SMS_KO_ATO_001";
            inDataTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);
            return dtResult;
        }


        private void GetSMSGroupCode()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTES";
                const string commonCodeType = "TIME_ELPS_CODE";

                DataTable inDataTable = new DataTable { TableName = "RQSTDT" };
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("CMCDTYPE", typeof(string));
                inDataTable.Columns.Add("ATTRIBUTE2", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = commonCodeType;
                dr["ATTRIBUTE2"] = LoginInfo.CFG_SHOP_ID;// "SMS_KO_ATO_001";
                inDataTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                if (dtResult?.Rows.Count > 0)
                {
                    if (dtResult.Columns.Contains("ATTRIBUTE4"))
                    {
                        _smsGroupCode = Util.NVC(dtResult.Rows[0]["ATTRIBUTE4"]);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string GetAreaType()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREATYPE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return Util.NVC(dtResult.Rows[0]["AREA_TYPE_CODE"]);
            }
            catch (Exception ex) { }
            return "";
        }
        #endregion
    }
}
