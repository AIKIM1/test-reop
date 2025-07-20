/*************************************************************************************
 Created Date : 2017.08.22
      Creator : 
   Decription : 설비별 작업모델 설정 < 계획/지시
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.22  J.S HONG : Initial Created.
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_030 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();

        private string _sSHOPID = string.Empty;
        private string _sAREAID = string.Empty;
        private string _sEQSGID = string.Empty;
        private string _sPROCID = string.Empty;
        private string _sEQPTID = string.Empty;
        private string _sELECTYPE = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public ELEC001_030()
        {
            InitializeComponent();
           // InitGrid();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;

            InitCombo();
            InitGrid();
        }

        private void InitGrid()
        {
            Util.gridClear(dgCurrentList);
            Util.gridClear(dgProductList);
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbParent: cboProcessParent, cbChild: cboProcessChild, sCase: "PROCESS");

            //극성
            String[] sFilter3 = { "", "ELTR_TYPE_CODE" };
            C1ComboBox[] cboLineChild2 = { cboEquipment };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, cbChild: cboLineChild2, sFilter: sFilter3, sCase: "COMMCODES");

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboElecType, cboProcess};
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent, sCase: "EQUIPMENT_NT_NEW");

            //재공기준
            chkWipFlag.IsChecked = true;

            //SHOP ID
            SetDataGridShiftCombo(dgCurrentList.Columns["SHOPID"], CommonCombo.ComboStatus.NONE);
        }

        #endregion

        C1.WPF.DataGrid.DataGridRowHeaderPresenter preCurr = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        C1.WPF.DataGrid.DataGridRowHeaderPresenter preProd = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll_CURR = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        CheckBox chkAll_PROD = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchData();
        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            if (dgProductList.ItemsSource == null) return;
            if (dgProductList.GetRowCount() == 0) return;

            DataTable dtCurrentList = DataTableConverter.Convert(dgCurrentList.ItemsSource);
            DataTable dtProductList = DataTableConverter.Convert(dgProductList.ItemsSource);
            DataRow newRow = null;

            for (int i = 0; i < dtProductList.Rows.Count; i++)
            {
                if (string.Equals(dtProductList.Rows[i]["CHK"].ToString(), "True") || string.Equals(dtProductList.Rows[i]["CHK"].ToString(), "1"))
                {
                    if (DataTableConverter.Convert(dgCurrentList.ItemsSource).Select("PRODID = '" + Util.NVC(DataTableConverter.GetValue(dgProductList.Rows[i].DataItem, "PRODID")) + "'").Length == 0)
                    {
                        newRow = dtCurrentList.NewRow();
                        if(dtProductList.Rows[i]["CHK"].ToString() == "1")
                            newRow["CHK"] = "0";
                        else
                            newRow["CHK"] = "False";
                        newRow["SHOPID"] = _sSHOPID;
                        newRow["MODLID"] = dtProductList.Rows[i]["MODLID"].ToString();
                        newRow["PRJT_NAME"] = dtProductList.Rows[i]["PRJT_NAME"].ToString();
                        newRow["PRODID"] = dtProductList.Rows[i]["PRODID"].ToString();
                        dtCurrentList.Rows.Add(newRow);
                    }
                }
            }

            dgCurrentList.ItemsSource = DataTableConverter.Convert(dtCurrentList);

            chkAll_CURR.IsChecked = false;
            chkAll_PROD.IsChecked = false;

            // 선택가능 Order에서 추가된 Order 제외
            SubtractCurrList();

            DataGridCheckAllUnChecked(dgProductList);

        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            if (dgCurrentList.ItemsSource == null) return;
            if (dgCurrentList.GetRowCount() == 0) return;        

            DataTable dtCurrentList = DataTableConverter.Convert(dgCurrentList.ItemsSource);

            for (int i = dtCurrentList.Rows.Count; i > 0; i--)
            {
                if (string.Equals(dtCurrentList.Rows[i-1]["CHK"].ToString(), "True") || string.Equals(dtCurrentList.Rows[i - 1]["CHK"].ToString(), "1"))
                {
                    dtCurrentList.Rows[i-1].Delete();
                }
            }

            dgCurrentList.ItemsSource = DataTableConverter.Convert(dtCurrentList);

            chkAll_CURR.IsChecked = false;
            chkAll_PROD.IsChecked = false;

            // 현재 존재하는 Order 이외에 가능한 Order를 읽어와 제외한다.
            GetPlanProductList();
            SubtractCurrList();
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {

            if (dgCurrentList.ItemsSource == null) return;
            if (dgCurrentList.GetRowCount() == 0) return;
            if (dgCurrentList.SelectedIndex == -1) return;

            int currentidx = dgCurrentList.SelectedIndex;
            int preidx = currentidx - 1;
            if (preidx == -1) return;


            //string tmp;

            //for (int i = 0; i < dgCurrentList.Columns.Count; i++)
            //{
            //    tmp = dgCurrentList.GetCell(currentidx - 1, i).Value.ToString();
            //    dgCurrentList.GetCell(currentidx - 1, i).Value = dgCurrentList.GetCell(currentidx, i).Value;
            //    dgCurrentList.GetCell(currentidx, i).Value = tmp;
            //}

            //dgCurrentList.SelectedIndex = currentidx - 1;

            DataTable dtCurrent = DataTableConverter.Convert(dgCurrentList.ItemsSource);
            DataRowView dvCurrent = dgCurrentList.Rows[currentidx].DataItem as DataRowView;
            DataRow drCurrent = dtCurrent.NewRow();
            drCurrent.ItemArray = dvCurrent.Row.ItemArray;
            dtCurrent.Rows.RemoveAt(currentidx);
            dtCurrent.Rows.InsertAt(drCurrent, currentidx-1);
            dgCurrentList.ItemsSource = DataTableConverter.Convert(dtCurrent);

            dgCurrentList.SelectedIndex = currentidx - 1;
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            if (dgCurrentList.ItemsSource == null) return;
            if (dgCurrentList.GetRowCount() == 0) return;
            if (dgCurrentList.SelectedIndex == -1) return;

            int currentidx = dgCurrentList.SelectedIndex;
            int nextidx = currentidx + 1;
            if (nextidx == dgCurrentList.GetRowCount()) return;
            //string tmp;

            //for (int i = 0; i < dgCurrentList.Columns.Count; i++)
            //{
            //    tmp = dgCurrentList.GetCell(currentidx + 1, i).Value.ToString();
            //    dgCurrentList.GetCell(currentidx + 1, i).Value = dgCurrentList.GetCell(currentidx, i).Value;
            //    dgCurrentList.GetCell(currentidx, i).Value = tmp;
            //}

            //dgCurrentList.SelectedIndex = currentidx + 1;

            DataTable dtCurrent = DataTableConverter.Convert(dgCurrentList.ItemsSource);
            DataRowView dvCurrent = dgCurrentList.Rows[currentidx].DataItem as DataRowView;
            DataRow drCurrent = dtCurrent.NewRow();
            drCurrent.ItemArray = dvCurrent.Row.ItemArray;
            dtCurrent.Rows.RemoveAt(currentidx);
            dtCurrent.Rows.InsertAt(drCurrent, currentidx + 1);
            dgCurrentList.ItemsSource = DataTableConverter.Convert(dtCurrent);

            dgCurrentList.SelectedIndex = currentidx + 1;
        }

        /* -- 전체 선택/취소 기능 부여로 사용하지 않음. 실제 btnLeft 로 옮길때 검사함.
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;

            if (DataTableConverter.Convert(dgCurrentList.ItemsSource).Select("PRODID = '" + Util.NVC(DataTableConverter.GetValue(dgProductList.Rows[idx].DataItem, "PRODID")) + "'").Length == 1)
            {
                Util.MessageValidation("SFU3471", Util.NVC(DataTableConverter.GetValue(dgProductList.Rows[idx].DataItem, "PRODID")));
                DataTableConverter.SetValue(dgProductList.Rows[idx].DataItem, "CHK", 0);
                return;
            }
        }
        */
        private void btnElectPlan_Click(object sender, RoutedEventArgs e)
        {
            if (dgCurrentList.ItemsSource == null) return;
            if (dgCurrentList.GetRowCount() == 0) return;

            //등록된 작업 Order를 저장하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4041"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    DataSet ds = new DataSet();

                    DataTable indata = ds.Tables.Add("INDATA");
                    indata.Columns.Add("EQSGID", typeof(string));
                    indata.Columns.Add("PROCID", typeof(string));
                    indata.Columns.Add("EQPTID", typeof(string));
                    indata.Columns.Add("USERID", typeof(string));
                    indata.Columns.Add("ELEC_TYPE_CODE", typeof(string));

                    DataRow row = indata.NewRow();
                    row["EQSGID"] = _sEQSGID;
                    row["PROCID"] = _sPROCID;

                    if (!string.IsNullOrEmpty(_sEQPTID))
                        row["EQPTID"] = _sEQPTID;

                    row["USERID"] = LoginInfo.USERID;

                    if (!string.IsNullOrEmpty(_sELECTYPE))
                        row["ELEC_TYPE_CODE"] = _sELECTYPE;

                    indata.Rows.Add(row);


                    DataTable inplan = ds.Tables.Add("INPLAN");
                    inplan.Columns.Add("ROWNUM", typeof(string));
                    inplan.Columns.Add("SHOPID", typeof(string));
                    inplan.Columns.Add("PRODID", typeof(string));

                    DataRow rowPlan = null;
                    int iRowNum = 0;
                    for (int i = 0; i < dgCurrentList.GetRowCount(); i++)
                    {
                        iRowNum = i + 1;
                        rowPlan = inplan.NewRow();
                        rowPlan["ROWNUM"] = iRowNum;
                        rowPlan["SHOPID"] = Util.NVC(DataTableConverter.GetValue(dgCurrentList.Rows[i].DataItem, "SHOPID"));
                        rowPlan["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgCurrentList.Rows[i].DataItem, "PRODID"));
                        inplan.Rows.Add(rowPlan);
                    }

                    loadingIndicator.Visibility = Visibility.Visible;

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_ELECT_PLAN", "INDATA,INPLAN", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            Util.MessageInfo("SFU3532");//저장되었습니다.

                            SearchData();
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        }
                    }, ds);
                }
            });
        }
        #endregion

        #region Mehod
        private void SearchData()
        {
            _sAREAID = Convert.ToString(cboArea.SelectedValue);
            _sEQSGID = Convert.ToString(cboEquipmentSegment.SelectedValue);
            _sPROCID = Convert.ToString(cboProcess.SelectedValue);
            _sEQPTID = Convert.ToString(cboEquipment.SelectedValue);
            _sELECTYPE = Convert.ToString(cboElecType.SelectedValue);

            GetPlanCurrentList();
            GetPlanProductList();
            GetPlanCurrentShopid();
            SubtractCurrList();
        }
        private void GetPlanCurrentList()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("ELEC_TYPE_CODE", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQSGID"] = _sEQSGID;
                row["PROCID"] = _sPROCID;

                if (!string.IsNullOrEmpty(_sEQPTID))
                    row["EQPTID"] = _sEQPTID;

                if (!string.IsNullOrEmpty(_sELECTYPE))
                    row["ELEC_TYPE_CODE"] = _sELECTYPE;

                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_PLAN_CURRENT", "RQSTDT", "RSLTDT", dt);

                if (result != null)
                {
                    Util.GridSetData(dgCurrentList, result, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        private void GetPlanProductList()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("ELEC_TYPE_CODE", typeof(string));
                dt.Columns.Add("PRJT_NAME", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));
                dt.Columns.Add("WIP_FLAG", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQSGID"] = _sEQSGID;
                row["PROCID"] = _sPROCID;

                if (!string.IsNullOrEmpty(_sEQPTID))
                    row["EQPTID"] = _sEQPTID;

                if (!string.IsNullOrEmpty(_sELECTYPE))
                    row["ELEC_TYPE_CODE"] = _sELECTYPE;

                row["PRJT_NAME"] = Util.ConvertEmptyToNull(txtPJT.Text);
                row["PRODID"] = Util.ConvertEmptyToNull(txtProdId.Text);
                row["WIP_FLAG"] = chkWipFlag.IsChecked == true ? "Y": "N";
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_PLAN_PRODID", "RQSTDT", "RSLTDT", dt);

                if (result != null)
                {
                    Util.GridSetData(dgProductList, result, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SubtractCurrList()
        {
            if (dgCurrentList.ItemsSource == null) return;
            if (dgCurrentList.GetRowCount() == 0) return;
            if (dgProductList.ItemsSource == null) return;
            if (dgProductList.GetRowCount() == 0) return;

            DataTable dtProductList = DataTableConverter.Convert(dgProductList.ItemsSource);

            for (int i = dtProductList.Rows.Count; i > 0; i--)
            {
                if (DataTableConverter.Convert(dgCurrentList.ItemsSource).Select("PRODID = '" + dtProductList.Rows[i - 1]["PRODID"].ToString() + "'").Length > 0)
                {
                    dtProductList.Rows[i - 1].Delete();
                }
            }

            dgProductList.ItemsSource = DataTableConverter.Convert(dtProductList);
        }

        private void GetPlanCurrentShopid()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("AREAID", typeof(string));

                DataRow row = dt.NewRow();
                row["AREAID"] = _sAREAID;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_PLAN_SHOPID", "RQSTDT", "RSLTDT", dt);

                if (result != null)
                {
                    if (result.Rows.Count > 0)
                    {
                        _sSHOPID = result.Rows[0]["SHOPID"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public static void DataGridCheckAllUnChecked(C1DataGrid dg, bool ischeckType = true)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                DataTableConverter.SetValue(row.DataItem, "CHK", false);

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void SetDataGridShiftCombo(C1.WPF.DataGrid.DataGridColumn dgcol, CommonCombo.ComboStatus status)
        {
            const string bizRuleName = "DA_BAS_SEL_SHOP_CBO";
            string[] arrColumn = { "LANGID" };
            string[] arrCondition = { LoginInfo.LANGID };
            string selectedValueText = ((C1.WPF.DataGrid.DataGridComboBoxColumn)dgcol).SelectedValuePath;
            string displayMemberText = ((C1.WPF.DataGrid.DataGridComboBoxColumn)dgcol).DisplayMemberPath;
            CommonCombo.SetDataGridComboItem(bizRuleName, arrColumn, arrCondition, status, dgcol, selectedValueText, displayMemberText);
        }
        #endregion

        /* -- 전체 선택/취소 기능 사용으로 사용하지 않음.
        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            CheckBox chk = sender as CheckBox;
            DataRowView drv = chk.DataContext as DataRowView;
            if (Convert.ToBoolean(drv.Row["CHK"]) == true)
            {
                int idx = ((DataGridCellPresenter)chk.Parent).Row.Index;
                dgCurrentList.SelectedIndex = idx;
            }

        }
        */

        #region 전체 선택/취소 기능 구현
        private void dgCurrentList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        preCurr.Content = chkAll_CURR;
                        e.Column.HeaderPresenter.Content = preCurr;
                        chkAll_CURR.Checked -= new RoutedEventHandler(checkAll_CURR_Checked);
                        chkAll_CURR.Unchecked -= new RoutedEventHandler(checkAll_CURR_Unchecked);
                        chkAll_CURR.Checked += new RoutedEventHandler(checkAll_CURR_Checked);
                        chkAll_CURR.Unchecked += new RoutedEventHandler(checkAll_CURR_Unchecked);
                    }
                }
            }));
        }

        private void dgCurrentList_CommittedEdit(object sender, DataGridCellEventArgs e)
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

                            chkAll_CURR.Checked -= new RoutedEventHandler(checkAll_CURR_Checked);
                            chkAll_CURR.Unchecked -= new RoutedEventHandler(checkAll_CURR_Unchecked);
                            DataTable dt = DataTableConverter.Convert(dg.ItemsSource);

                            if (!chk.IsChecked.Value)
                            {
                                chkAll_CURR.IsChecked = false;
                            }
                            else
                            {
                                dgCurrentList.SelectedIndex = e.Cell.Row.Index;

                                if (dt.AsEnumerable().Where(row => Convert.ToBoolean(row["CHK"]) == true).Count() == dt.Rows.Count)
                                    chkAll_CURR.IsChecked = true;
                            }

                            chkAll_CURR.Checked += new RoutedEventHandler(checkAll_CURR_Checked);
                            chkAll_CURR.Unchecked += new RoutedEventHandler(checkAll_CURR_Unchecked);
                            break;
                    }
                }
            }
        }

        private void dgProductList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        preProd.Content = chkAll_PROD;
                        e.Column.HeaderPresenter.Content = preProd;
                        
                        chkAll_PROD.Checked -= new RoutedEventHandler(checkAll_PROD_Checked);
                        chkAll_PROD.Unchecked -= new RoutedEventHandler(checkAll_PROD_Unchecked);
                        chkAll_PROD.Checked += new RoutedEventHandler(checkAll_PROD_Checked);
                        chkAll_PROD.Unchecked += new RoutedEventHandler(checkAll_PROD_Unchecked);
                    }
                }
            }));
        }

        private void dgProductList_CommittedEdit(object sender, DataGridCellEventArgs e)
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

                            chkAll_PROD.Checked -= new RoutedEventHandler(checkAll_PROD_Checked);
                            chkAll_PROD.Unchecked -= new RoutedEventHandler(checkAll_PROD_Unchecked);
                            DataTable dt = DataTableConverter.Convert(dg.ItemsSource);

                            if (!chk.IsChecked.Value)
                            {
                                chkAll_PROD.IsChecked = false;
                            }
                            else if (dt.AsEnumerable().Where(row => Convert.ToBoolean(row["CHK"]) == true).Count() == dt.Rows.Count)
                            {
                                chkAll_PROD.IsChecked = true;
                            }

                            chkAll_PROD.Checked += new RoutedEventHandler(checkAll_PROD_Checked);
                            chkAll_PROD.Unchecked += new RoutedEventHandler(checkAll_PROD_Unchecked);
                            break;
                    }
                }
            }
        }

        void checkAll_CURR_Checked(object sender, RoutedEventArgs e)
        {
            if (dgCurrentList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgCurrentList.ItemsSource).Table;

            dt.Select("CHK = false").ToList<DataRow>().ForEach(r => r["CHK"] = true);
            dt.AcceptChanges();
        }

        void checkAll_CURR_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgCurrentList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgCurrentList.ItemsSource).Table;

            dt.Select("CHK = true").ToList<DataRow>().ForEach(r => r["CHK"] = false);
            dt.AcceptChanges();
        }

        void checkAll_PROD_Checked(object sender, RoutedEventArgs e)
        {
            if (dgProductList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgProductList.ItemsSource).Table;

            dt.Select("CHK = false").ToList<DataRow>().ForEach(r => r["CHK"] = true);
            dt.AcceptChanges();
        }

        void checkAll_PROD_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgProductList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgProductList.ItemsSource).Table;

            dt.Select("CHK = true").ToList<DataRow>().ForEach(r => r["CHK"] = false);
            dt.AcceptChanges();
        }
        #endregion

    }
}
