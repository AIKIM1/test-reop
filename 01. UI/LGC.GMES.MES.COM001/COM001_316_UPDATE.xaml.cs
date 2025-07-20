/*************************************************************************************
 Created Date : 2020.02.27
      Creator : 최상민
   Decription : 전지 5MEGA-GMES 구축 - 보류대상관리 lot이력관리
--------------------------------------------------------------------------------------
 [Change History]
 2020.02.27 최상민  최초생성

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Windows.Media;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using System.Linq;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_316_UPDATE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string _holdTrgtCode = string.Empty;
        DataTable tmmp01;
        String sAreaID = "";
        String sEgsgID = "";

        Util _util = new Util();

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_316_UPDATE()
        {
            InitializeComponent();
            Loaded += COM001_316_UPDATE_Loaded;
        }

        private void COM001_316_UPDATE_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= COM001_316_UPDATE_Loaded;

            object[] tmps = C1WindowExtension.GetParameters(this);
            tmmp01 = tmps[0] as DataTable;
            sAreaID = Util.NVC(tmps[1]);
            sEgsgID = Util.NVC(tmps[2]);
            //dgLotList.Visibility = Visibility.Collapsed;
            // dgLotList2.Visibility = Visibility.Visible;
            InitCombo();
            getLotIDInfo();
        }

        #endregion

        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void dgLotList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            string[] sFilter2 = { "HOLD_STCK_TYPE" };

            _combo.SetCombo(cboHold_Stck_Type, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "COMMCODE_WITHOUT_CODE");
            cboHold_Stck_Type.SelectedIndex = 1;

            SetProcess();
        }

        private void SetProcess()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = string.IsNullOrWhiteSpace(sAreaID) ? null : sAreaID;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEgsgID) ? null : sEgsgID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboOccr_Proc.DisplayMemberPath = "CBO_NAME";
                cboOccr_Proc.SelectedValuePath = "CBO_CODE";

                //DataRow drIns = dtResult.NewRow();
                //drIns["CBO_NAME"] = "-SELECT-";
                //drIns["CBO_CODE"] = "SELECT";
                //dtResult.Rows.InsertAt(drIns, 0);
                //DataTable dtResult2 = new DataTable();

                DataView view = dtResult.AsDataView();
                view.RowFilter = "CBO_CODE NOT LIKE 'F%'";
                cboOccr_Proc.ItemsSource = view;

                // cboOccr_Proc.ItemsSource = dtResult.Copy().AsDataView().RowFilter = "CBO_CODE Like '%F%'";



                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cboOccr_Proc.SelectedValue = LoginInfo.CFG_PROC_ID;

                    if (cboOccr_Proc.SelectedIndex < 0)
                        cboOccr_Proc.SelectedIndex = 0;
                }
                else
                {
                    if (cboOccr_Proc.Items.Count > 0)
                        cboOccr_Proc.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitControl()
        {

            cboHold_Stck_Type.SelectedValue = "SELECT";
            cboHold_Stck_Type.SelectedIndex = 0;
            txtOccrCause_Cntt.Text = string.Empty;
            txtPrcs_Mthd_Cntt.Text = string.Empty;
            txtProg_Stat_Cntt.Text = string.Empty;
            txtUser.Text = string.Empty;
            txtDept.Text = string.Empty;

        }

        #endregion

        #region UI 이벤트 : 저장/닫기 버튼 이벤트

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetUserWindow();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgLotListChk_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb?.DataContext == null) return;
                if (cb.IsChecked == null) return;

                DataRowView drv = cb.DataContext as DataRowView;
                int idx = ((DataGridCellPresenter)cb.Parent).Row.Index;

                foreach (DataRowView item in dgLotList.ItemsSource)
                {
                    if (drv["HOLD_GR_ID"].ToString().Equals(item["HOLD_GR_ID"].ToString()))
                    {
                        item["CHK"] = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgLotListChk_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb?.DataContext == null) return;
                if (cb.IsChecked == null) return;

                DataRowView drv = cb.DataContext as DataRowView;

                foreach (DataRowView item in dgLotList.ItemsSource)
                {
                    if (drv["HOLD_GR_ID"].ToString().Equals(item["HOLD_GR_ID"].ToString()))
                    {
                        item["CHK"] = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgLotList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell == null || datagrid.CurrentRow == null)
            {
                //datagrid.Selection.Add(datagrid.GetCell(datagrid.FrozenTopRowsCount, 0), datagrid.GetCell(datagrid.Rows.Count - 1, datagrid.Columns.Count - 2));
                return;
            }

            if (datagrid.CurrentColumn.Name == "HOLD_STCK_TYPE_NAME")
            {
                if (Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["HOLD_STCK_TYPE_CODE"].Index).Value) != ""
                    && Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["HOLD_STCK_TYPE_CODE"].Index).Value) != null)
                {
                    //Hold 재고 구분
                    cboHold_Stck_Type.SelectedValue = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["HOLD_STCK_TYPE_CODE"].Index).Value);
                }
            }

            if (datagrid.CurrentColumn.Name == "OCCR_PROCID_NAME")
            {
                if (Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["OCCR_PROCID"].Index).Value) != ""
                    && Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["OCCR_PROCID"].Index).Value) != null)
                {
                    //Hold 발생공정
                    cboOccr_Proc.SelectedValue = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["OCCR_PROCID"].Index).Value);
                }
            }

            if (datagrid.CurrentColumn.Name == "HOLD_ISSUE_CNTT")
            {
                if (Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["HOLD_ISSUE_CNTT"].Index).Value) != ""
                    && Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["HOLD_ISSUE_CNTT"].Index).Value) != null)
                {
                    //이슈명
                    txtIssue_Cntt.Text = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["HOLD_ISSUE_CNTT"].Index).Value);
                }
            }

            if (datagrid.CurrentColumn.Name == "HOLD_ADJ_QTY")
            {
                if (Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["HOLD_ADJ_QTY"].Index).Value) != ""
                    && Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["HOLD_ADJ_QTY"].Index).Value) != null)
                {
                    //Hold수량(보류) double.Parse
                    txtAdj_Qty.Value = Int32.Parse(Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["HOLD_ADJ_QTY"].Index).Value));
                }
            }

            if (datagrid.CurrentColumn.Name == "OCCR_CAUSE_CNTT")
            {
                if (Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["OCCR_CAUSE_CNTT"].Index).Value) != ""
                    && Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["OCCR_CAUSE_CNTT"].Index).Value) != null)
                {
                    //발생원인
                    txtOccrCause_Cntt.Text = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["OCCR_CAUSE_CNTT"].Index).Value);
                }
            }

            if (datagrid.CurrentColumn.Name == "PRCS_MTHD_CNTT")
            {
                if (Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["PRCS_MTHD_CNTT"].Index).Value) != ""
                    && Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["PRCS_MTHD_CNTT"].Index).Value) != null)
                {
                    //처리방안
                    txtPrcs_Mthd_Cntt.Text = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["PRCS_MTHD_CNTT"].Index).Value);
                }
            }

            if (datagrid.CurrentColumn.Name == "PROG_STAT_CNTT")
            {
                if (Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["PROG_STAT_CNTT"].Index).Value) != ""
                    && Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["PROG_STAT_CNTT"].Index).Value) != null)
                {
                    //진행현황
                    txtProg_Stat_Cntt.Text = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["PROG_STAT_CNTT"].Index).Value);
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgLotList.ItemsSource);

            List<DataRow> drList = dt.Select("CHK = 'True'")?.ToList();
            if (drList.Count > 0)
            {
                foreach (DataRow dr in drList)
                {
                    dt.Rows.Remove(dr);
                }
                Util.GridSetData(dgLotList, dt, FrameOperation);
                // chkAll.IsChecked = false;
            }
        }

        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationModify())
                return;

            // 수정 하시겠습니까?
            Util.MessageConfirm("SFU4340", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ModifyProcess();
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        #endregion

        #region 기타 기능 : Method

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtUser.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                //grdChMain.Children.Add(wndPerson);
                //wndPerson.BringToFront();

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(wndPerson);
                        wndPerson.BringToFront();
                        break;
                    }
                }
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtUser.Text = wndPerson.USERNAME;
                txtUser.Tag = wndPerson.USERID;
                txtDept.Text = wndPerson.DEPTNAME;
                txtDept.Tag = wndPerson.DEPTID;

            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(wndPerson);
                    break;
                }
            }
        }

        private void getLotIDInfo()
        {
            try
            {
                //  DataRow dr = RQSTDT.NewRow();
                // dr["LANGID"] = LoginInfo.LANGID;
                //dr["HOLD_GR_ID"] = sHoldGRid;
                //RQSTDT.Rows.Add(dr);

                DataTable dtResult = new DataTable();
                String sHoldGrID = "";
                if (tmmp01 != null)
                {
                    DataTable RQSTDT = new DataTable();

                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("HOLD_GR_ID", typeof(string));

                    DataRow dRow = RQSTDT.NewRow();
                    dRow = RQSTDT.NewRow();
                    dRow["LANGID"] = LoginInfo.LANGID;

                    for (int i = 0; i < tmmp01.Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            sHoldGrID += tmmp01.Rows[i]["HOLD_GR_ID"].ToString();
                        }
                        else
                        {
                            sHoldGrID += "," + tmmp01.Rows[i]["HOLD_GR_ID"].ToString();
                        }
                    }

                    dRow["HOLD_GR_ID"] = sHoldGrID;
                    RQSTDT.Rows.Add(dRow);

                    // ASSY001_043_EQPTWIN merge 참조                        
                    dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_ASSY_LOT_HOLD_GR_HIST_LIST", "RQSTDT", "OUTDATA", RQSTDT);
                    // List<DataRow> drList = dtResult.Select("HOLD_GR_ID <> 'NULL'")?.ToList();                   
                    Util.GridSetData(dgLotList, dtResult, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidationModify()
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgLotList.ItemsSource);
                List<DataRow> list = dtInfo.Select("LOT_NUM = '1'").ToList();
                if (list.Count == 0)
                {
                    // 선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                if (cboHold_Stck_Type.SelectedValue.ToString().Equals("SELECT"))
                {
                    // % 1이 입력되지 않았습니다.
                    Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("HOLD재고구분"));
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtUser.Text))
                {
                    // 작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtDept.Text))
                {
                    // % 1이 입력되지 않았습니다.
                    Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("담당부서"));
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void ModifyProcess()
        {
            try
            {
                // DATA SET 
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("HOLD_GR_ID", typeof(string));
                inTable.Columns.Add("HOLD_GR_ID_SEQNO", typeof(string));
                inTable.Columns.Add("HOLD_STCK_TYPE_CODE", typeof(string));
                inTable.Columns.Add("OCCR_PROCID", typeof(string));

                inTable.Columns.Add("HOLD_ISSUE_CNTT", typeof(string));
                inTable.Columns.Add("HOLD_ADJ_QTY", typeof(int));

                inTable.Columns.Add("OCCR_CAUSE_CNTT", typeof(string));
                inTable.Columns.Add("PRCS_MTHD_CNTT", typeof(string));
                inTable.Columns.Add("PROG_STAT_CNTT", typeof(string));
                inTable.Columns.Add("CHARGE_DEPT_NAME", typeof(string));
                inTable.Columns.Add("CHARGE_USERID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));

                /////////////////////////////////////////////////////////////////
                DataTable dtInfo = DataTableConverter.Convert(dgLotList.ItemsSource);
                List<DataRow> drList = dtInfo.Select("LOT_NUM = 1").ToList();

                foreach (DataRow row in drList)
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["HOLD_GR_ID"] = Util.NVC(row["HOLD_GR_ID"]); // dtInfo.Rows[i]["HOLD_GR_ID"].ToString(); 
                    newRow["HOLD_GR_ID_SEQNO"] = Util.NVC(row["HOLD_GR_ID_SEQNO"]);  //dtInfo.Rows[i]["HOLD_GR_ID_SEQNO"].ToString();

                    newRow["HOLD_STCK_TYPE_CODE"] = cboHold_Stck_Type.SelectedValue.ToString();
                    newRow["OCCR_PROCID"] = cboOccr_Proc.SelectedValue.ToString();

                    newRow["HOLD_ISSUE_CNTT"] = string.IsNullOrEmpty(txtIssue_Cntt.Text.Trim()) ? null : txtIssue_Cntt.Text.Trim();
                    newRow["HOLD_ADJ_QTY"] = txtAdj_Qty.Value.Equals(null) ? 0 : txtAdj_Qty.Value;
                    //newRow["HOLD_ADJ_QTY"] = string.IsNullOrEmpty(txtAdj_Qty.Text.Trim()) ? 0 : txtAdj_Qty.Text.Trim();

                    newRow["OCCR_CAUSE_CNTT"] = string.IsNullOrEmpty(txtOccrCause_Cntt.Text.Trim()) ? null : txtOccrCause_Cntt.Text.Trim();
                    newRow["PRCS_MTHD_CNTT"] = string.IsNullOrEmpty(txtPrcs_Mthd_Cntt.Text.Trim()) ? null : txtPrcs_Mthd_Cntt.Text.Trim();
                    newRow["PROG_STAT_CNTT"] = string.IsNullOrEmpty(txtProg_Stat_Cntt.Text.Trim()) ? null : txtProg_Stat_Cntt.Text.Trim();
                    newRow["CHARGE_DEPT_NAME"] = string.IsNullOrEmpty(txtDept.Text.Trim()) ? null : txtDept.Text.Trim();
                    newRow["CHARGE_USERID"] = txtUser.Tag;
                    newRow["UPDUSER"] = LoginInfo.USERID;
                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService("DA_BAS_UPD_TB_SFC_ASSY_LOT_HOLD_STCK_HIST", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            // HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            this.DialogResult = MessageBoxResult.Cancel;
                            return;
                        }
                        /*
                                                getLotIDInfo();
                                                InitControl();
                                                HiddenLoadingIndicator();
                        */
                        Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.

                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        // HiddenLoadingIndicator();
                        Util.MessageException(ex);
                        this.DialogResult = MessageBoxResult.Cancel;
                    }
                });
            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion
    }
}
