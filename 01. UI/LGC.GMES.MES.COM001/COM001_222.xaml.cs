/*************************************************************************************
 Created Date : 2018.03.16
      Creator : 오화백
   Decription : 활성화 대차 비용처리 등록/취소 < 특이작업
--------------------------------------------------------------------------------------
 [Change History]
  2018.03.16  오화백 : Initial Created.

 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;


namespace LGC.GMES.MES.COM001
{
    public partial class COM001_222 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();

        public COM001_222()
        {
            InitializeComponent();
            InitCombo();

            this.Loaded += UserControl_Loaded;

            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

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

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        #endregion

        #region Initialize

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            #region [Combo]비용처리 대상 등록  

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //동
            C1ComboBox[] cboAreaChildRegister = { cboProcRegister };
            _combo.SetCombo(cboAreaRegister, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildRegister, sCase: "AREA");


            //공정
            C1ComboBox[] cboProcessParentRegister = { cboAreaRegister };
            C1ComboBox[] cboProcessChildRegisterr = { cboEqsgRegister };
            _combo.SetCombo(cboProcRegister, CommonCombo.ComboStatus.SELECT, sCase: "POLYMER_PROCESS", cbParent: cboProcessParentRegister, cbChild: cboProcessChildRegisterr);



            //라인
            C1ComboBox[] cboLineParentParentRegister = { cboAreaRegister, cboProcRegister };
            _combo.SetCombo(cboEqsgRegister, CommonCombo.ComboStatus.ALL, sCase: "PROCESS_EQUIPMENT", cbParent: cboLineParentParentRegister);
            cboEqsgRegister.SelectedValue = LoginInfo.CFG_EQSG_ID;


            //if (cboEqsgRegister.Items.Count > 0) cboEqsgRegister.SelectedIndex = 0;
            //if (cboProcRegister.Items.Count > 0) cboProcRegister.SelectedIndex = 0;

            //REASON 콤보 MMD - 공정별 Activity reason 조회하도록 수정 2019.04.03 이제섭
            //string[] sFilter1 = { "CHARGE_PROD_LOT" };
            // _combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter1);


            //SetReasonCombo(cboResnCode);

            string[] sFilter2 = { LoginInfo.CFG_SHOP_ID };
            _combo.SetCombo(cboCostSloc, CommonCombo.ComboStatus.SELECT, sCase: "COST_SLOC", sFilter: sFilter2);



            #endregion

            #region [Combo]비용처리 완료 등록

            //String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //동
            C1ComboBox[] cboAreaChildComplete = { cboProcComplete };
            _combo.SetCombo(cboAreaComplete, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildComplete, sCase: "AREA");
            //공정
            C1ComboBox[] cboProcessParentComplete = { cboAreaComplete };
            C1ComboBox[] cboProcessChildComplete = { cboEqsgComplete };
            _combo.SetCombo(cboProcComplete, CommonCombo.ComboStatus.SELECT, sCase: "POLYMER_PROCESS", cbParent: cboProcessParentComplete, cbChild: cboProcessChildComplete);

            //라인
            C1ComboBox[] cboLineParentParentComplet = { cboAreaComplete, cboProcComplete };
            _combo.SetCombo(cboEqsgComplete, CommonCombo.ComboStatus.ALL, sCase: "PROCESS_EQUIPMENT", cbParent: cboLineParentParentComplet);
            cboEqsgComplete.SelectedValue = LoginInfo.CFG_EQSG_ID;


            //if (cboEqsgComplete.Items.Count > 0) cboEqsgComplete.SelectedIndex = 0;
            //if (cboProcComplete.Items.Count > 0) cboProcComplete.SelectedIndex = 0;


            #endregion

            #region [Combo]비용처리 대상 등록 취소

            //동
            C1ComboBox[] cboAreaChildCancel = { cboProcCancel };
            _combo.SetCombo(cboAreaCancel, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildCancel, sCase: "AREA");


            //공정
            C1ComboBox[] cboProcessParentCancel = { cboAreaCancel };
            C1ComboBox[] cboProcessChildCancel = { cboEqsgCancel };
            _combo.SetCombo(cboProcCancel, CommonCombo.ComboStatus.SELECT, sCase: "POLYMER_PROCESS", cbParent: cboProcessParentCancel, cbChild: cboProcessChildCancel);


            //라인
            C1ComboBox[] cboLineParentParentCancel = { cboAreaCancel, cboProcCancel };
            _combo.SetCombo(cboEqsgCancel, CommonCombo.ComboStatus.ALL, sCase: "PROCESS_EQUIPMENT", cbParent: cboLineParentParentCancel);
            cboEqsgCancel.SelectedValue = LoginInfo.CFG_EQSG_ID;



            //if (cboEqsgCancel.Items.Count > 0) cboEqsgCancel.SelectedIndex = 0;
            //if (cboProcCancel.Items.Count > 0) cboProcCancel.SelectedIndex = 0;

            #endregion

            #region [Combo]비용처리 대상 이력 조회

            //등록일시
            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;

            //동
            C1ComboBox[] cboAreaChildHistory = { cboProcHistory };
            _combo.SetCombo(cboAreaHistory, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildHistory, sCase: "AREA");


            //공정
            C1ComboBox[] cboProcessParentHistory = { cboAreaHistory };
            C1ComboBox[] cboProcessChildHistory = { cboEqsgHistory };
            _combo.SetCombo(cboProcHistory, CommonCombo.ComboStatus.SELECT, sCase: "POLYMER_PROCESS", cbParent: cboProcessParentHistory, cbChild: cboProcessChildHistory);

            //라인
            C1ComboBox[] cboLineParentParentHistory = { cboAreaHistory, cboProcHistory };
            _combo.SetCombo(cboEqsgHistory, CommonCombo.ComboStatus.ALL, sCase: "PROCESS_EQUIPMENT", cbParent: cboLineParentParentHistory);
            cboEqsgHistory.SelectedValue = LoginInfo.CFG_EQSG_ID;

            ////cboReqTypeHistory
            //if (cboEqsgHistory.Items.Count > 0) cboEqsgHistory.SelectedIndex = 0;
            //if (cboProcHistory.Items.Count > 0) cboProcHistory.SelectedIndex = 0;

            // 구분
            //String[] sFilterPrdtReqTypeHis = { "", "PRDT_REQ_TYPE_CODE" };
            //_combo.SetCombo(cboPrdtReqTypeHistory, CommonCombo.ComboStatus.ALL, sFilter: sFilterPrdtReqTypeHis, sCase: "COMMCODES");
            #endregion
        }
        #endregion
        #region REASON 콤보 MMD - 공정별 Activity reason 조회하도록 수정 2019.04.03 이제섭
        private void SetReasonCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_PRD_SEL_PROC_DFCT_CODE_CBO";
            string[] arrColumn = { "LANGID","PROCID","ACTID" };
            string[] arrCondition = { LoginInfo.LANGID, cboProcRegister.SelectedValue.ToString(), "CHARGE_PROD_LOT" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText);
        }

        private void cboProcRegister_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetReasonCombo(cboResnCode);
        }
        #endregion
        #region Event

        #region 버튼권한
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            {
                Array ary = FrameOperation.Parameters;

                DataTable dtInfo = ary.GetValue(0) as DataTable;

                foreach (DataRow dr in dtInfo.Rows)
                {
                    txtCtnridRegister.Text = Util.NVC(dr["CTNR_ID"]);
                    GetLotList_Register(false);
                }
            }

            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSaveRegister);
            listAuth.Add(btnSaveComplete);
            listAuth.Add(btnSaveCancel);
            listAuth.Add(btnSaveHistory);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region [조회&저장]비용처리 대상 등록
        private void btnSearchRegister_Click(object sender, RoutedEventArgs e)
        {
            chkAll.IsChecked = false;
            GetLotList_Register();
        }

        private void btnSaveRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_Register())
                {
                    return;
                }
                //비용처리 대상 등록 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4086"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                REQ_Register();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [조회&저장]비용처리 완료 등록 
        private void btnSearchComplete_Click(object sender, RoutedEventArgs e)
        {
            chkAllComplete.IsChecked = false;
            GetLotList_Complete();
        }

        //비용처리 완료 등록
        private void btnSaveComplete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_Complete())
                {
                    return;
                }
                //비용처리 완료 등록 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4087"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                REQ_Complete();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [조회&저장]비용처리 대상 등록 취소
        private void btnSearchCancel_Click(object sender, RoutedEventArgs e)
        {
            GetLotList_Cancel();
        }

        private void btnSaveCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_Cancel())
                {
                    return;
                }
                //비용처리 등록 취소 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4088"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                REQ_Cancel();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region [조회&저장]비용처리 대상 이력 조회
        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {
            GetLotList_History();
        }

        private void btnSaveHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_History())
                {
                    return;
                }
                //비용처리 대상등록 완료 취소 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4133"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                REQ_History();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region [대상 선택하기]

        #region 비용처리 대상 등록
        private void CheckBoxRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
                object objRowIdx = dgListRegister.Rows[idx].DataItem;

                AddToSelectRegister(objRowIdx);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 비용처리 등록 완료
        private void CheckBoxComplete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
                object objRowIdx = dgListComplete.Rows[idx].DataItem;

                AddToSelectComplete(objRowIdx);


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 비용처리 대상 등록 취소
        private void CheckBoxCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
                object objRowIdx = dgListCancel.Rows[idx].DataItem;

                if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals(1))
                {
                    if (DataTableConverter.Convert(dgSelectCancel.ItemsSource).Select("CTNR_ID = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "CTNR_ID")) + "' AND WIPSEQ = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "WIPSEQ")) + "' AND RESNCODE = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "RESNCODE")) + "'").Length == 1)
                    {
                        Util.MessageValidation("SFU2840", Util.NVC(DataTableConverter.GetValue(objRowIdx, "CTNR_ID")));
                        DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                        return;
                    }
                }

                if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals(1))
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectCancel.ItemsSource);
                    DataRow dr = dtTo.NewRow();
                    foreach (DataColumn dc in dtTo.Columns)
                    {
                        dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(objRowIdx, dc.ColumnName));
                    }
                    dtTo.Rows.Add(dr);
                    dgSelectCancel.ItemsSource = DataTableConverter.Convert(dtTo);
                }
                else
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectCancel.ItemsSource);
                    if (dtTo.Rows.Count > 0)
                    {
                        if (dtTo.Select("CTNR_ID = '" + DataTableConverter.GetValue(objRowIdx, "CTNR_ID") + "'").Length > 0)
                        {
                            dtTo.Rows.Remove(dtTo.Select("CTNR_ID = '" + DataTableConverter.GetValue(objRowIdx, "CTNR_ID") + "'")[0]);
                            dgSelectCancel.ItemsSource = DataTableConverter.Convert(dtTo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 비용처리 대상 이력 조회
        private void CheckBoxHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
                object objRowIdx = dgListHistory.Rows[idx].DataItem;

                if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals(1))
                {
                    if (DataTableConverter.Convert(dgSelectHistory.ItemsSource).Select("CTNR_ID = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "CTNR_ID")) + "' AND WIPSEQ = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "WIPSEQ")) + "' AND RESNCODE = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "RESNCODE")) + "'").Length == 1)
                    {
                        Util.MessageValidation("SFU2840", Util.NVC(DataTableConverter.GetValue(objRowIdx, "CTNR_ID")));
                        DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                        return;
                    }
                }

                if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals(1))
                {
                    if (!Util.NVC(DataTableConverter.GetValue(objRowIdx, "PRDT_REQ_PRCS_STAT_CODE")).Equals("COMPLETE"))
                    {
                        Util.MessageValidation("SFU4138", Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")));//해당LOTID[%1]는 완료 등록된 LOTID가 아닙니다.
                        DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                        return;
                    }

                    DataTable dtTo = DataTableConverter.Convert(dgSelectHistory.ItemsSource);
                    DataRow dr = dtTo.NewRow();
                    foreach (DataColumn dc in dtTo.Columns)
                    {
                        dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(objRowIdx, dc.ColumnName));
                    }
                    dtTo.Rows.Add(dr);
                    dgSelectHistory.ItemsSource = DataTableConverter.Convert(dtTo);
                }
                else
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectHistory.ItemsSource);
                    if (dtTo.Rows.Count > 0)
                    {
                        if (dtTo.Select("CTNR_ID = '" + DataTableConverter.GetValue(objRowIdx, "CTNR_ID") + "'").Length > 0)
                        {
                            dtTo.Rows.Remove(dtTo.Select("CTNR_ID = '" + DataTableConverter.GetValue(objRowIdx, "CTNR_ID") + "'")[0]);
                            dgSelectHistory.ItemsSource = DataTableConverter.Convert(dtTo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #endregion

        #region BizWF 처리 담당자
        private void txtPrscUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void btnPrscUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtPrscUser.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }
        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtPrscUser.Text = wndPerson.USERNAME;
                txtPrscUser.Tag = wndPerson.USERID;

                if (string.IsNullOrEmpty(txtDept.Text))
                {
                    txtDept.Text = wndPerson.DEPTNAME;
                    txtDept.Tag = wndPerson.DEPTID;
                }
                else
                {
                    txtDept.Text = string.Empty;
                    txtDept.Tag = string.Empty;
                }
            }
        }
        #endregion

        #region BizWF 부서
        private void txtDept_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetDepartmentWindow();
            }
        }

        private void btnDept_Click(object sender, RoutedEventArgs e)
        {
            GetDepartmentWindow();
        }

        private void GetDepartmentWindow()
        {
            CMM_DEPARTMENT wndDept = new CMM_DEPARTMENT();
            wndDept.FrameOperation = FrameOperation;

            if (wndDept != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtDept.Text;
                C1WindowExtension.SetParameters(wndDept, Parameters);

                wndDept.Closed += new EventHandler(wndDept_Closed);
                grdMain.Children.Add(wndDept);
                wndDept.BringToFront();
            }
        }
        private void wndDept_Closed(object sender, EventArgs e)
        {
            CMM_DEPARTMENT wndDept = sender as CMM_DEPARTMENT;
            if (wndDept.DialogResult == MessageBoxResult.OK)
            {
                txtDept.Text = wndDept.DEPTNAME;
                txtDept.Tag = wndDept.DEPTID;
            }
        }

        #endregion

        #region 처리수량(Lane) 자동계산(처리수량(Roll) * Lane)
        private void dgSelectRegister_LostFocus(object sender, RoutedEventArgs e)
        {
            IInputElement input = Keyboard.FocusedElement;

            if (input != sender)
            {
                var parent = VisualTreeHelper.GetParent(input as FrameworkElement) as FrameworkElement;

                while (parent != null)
                {
                    if (parent == sender)
                    {
                        break;
                    }

                    parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
                }

                if (parent != null && parent is C1.WPF.DataGrid.C1DataGrid)
                {

                }
                else
                {
                    C1.WPF.DataGrid.C1DataGrid datagrid = sender as C1.WPF.DataGrid.C1DataGrid;
                    datagrid.EndEdit();
                    datagrid.EndEditRow(true);
                }
            }
        }

        private void dgSelectRegister_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (dgSelectRegister.GetRowCount() == 0) return;

            try
            {
                if (Convert.ToString(e.Cell.Column.Name) == "ACTQTY")
                {
                    Decimal dACTQTY = Util.NVC_Decimal(DataTableConverter.GetValue(dgSelectRegister.Rows[dgSelectRegister.SelectedIndex].DataItem, "ACTQTY"));
                    Decimal dLANE_QTY = Util.NVC_Decimal(DataTableConverter.GetValue(dgSelectRegister.Rows[dgSelectRegister.SelectedIndex].DataItem, "LANE_QTY"));

                    DataTableConverter.SetValue(dgSelectRegister.Rows[dgSelectRegister.SelectedIndex].DataItem, "ACTQTY2", dACTQTY * dLANE_QTY);
                }
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }
        #endregion

        #region 입력란 표시 -  처리수량(Roll, EA) Grid 색상변경
        private void dgSelectRegister_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Column.Name == "ACTQTY")
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                }));
            }
        }
        #endregion

        #endregion

        #region Mehod

        #region [대상목록 가져오기]

        #region 비용처리 대상 등록 조회

        public void GetLotList_Register(bool bButton = true)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                string sProcID = Util.ConvertEmptyToNull((string)cboProcRegister.SelectedValue);
                string sEqsgID = Util.ConvertEmptyToNull((string)cboEqsgRegister.SelectedValue);
                string sPrjtName = Util.ConvertEmptyToNull(txtPrjtRegister.Text.Trim());
                string sProdID = Util.ConvertEmptyToNull(txtProdRegister.Text.Trim());
                string sLotIDRT = Util.ConvertEmptyToNull(txtLotIdRTRegister.Text.Trim());
                string sCtnrID = Util.ConvertEmptyToNull(txtCtnridRegister.Text.Trim());



                if (cboProcRegister.SelectedIndex == 0)
                {
                    Util.AlertInfo("SFU3207"); //공정을 선택하세요"."
                    return;
                }
                //if(cboEqsgRegister.SelectedIndex == 0)
                //{
                //    Util.AlertInfo("SFU1223"); //라인을 선택하세요"."
                //    return;
                //}

                if (!string.IsNullOrEmpty(sProcID))
                    dr["PROCID"] = sProcID;
                if (!string.IsNullOrEmpty(sEqsgID))
                    dr["EQSGID"] = sEqsgID;
                if (!string.IsNullOrEmpty(sPrjtName))
                    dr["PRJT_NAME"] = sPrjtName;
                if (!string.IsNullOrEmpty(sProdID))
                    dr["PRODID"] = sProdID;
                if (!string.IsNullOrEmpty(sLotIDRT))
                    dr["LOTID_RT"] = sLotIDRT;
                if (!string.IsNullOrEmpty(sCtnrID))
                    dr["CTNR_ID"] = sCtnrID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_COST_CTNR", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0 && bButton == true)
                {
                    //Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListRegister, dtRslt, FrameOperation, false);
                    Util.gridClear(dgSelectRegister);

                    return;
                }
                else if (dtRslt.Rows.Count == 0 && bButton == false)
                {
                    Util.MessageConfirm("SFU1905", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtCtnridRegister.Focus();
                            txtCtnridRegister.Text = string.Empty;
                        }
                    });
                    return;
                }

                if (dtRslt.Rows.Count > 0 && bButton == false)
                {
                    if (dtRslt.Rows.Count == 1)
                    {

                        DataTable dtSource = DataTableConverter.Convert(dgListRegister.ItemsSource);
                        dtSource.Merge(dtRslt);
                        Util.gridClear(dgListRegister);
                        Util.GridSetData(dgListRegister, dtSource, FrameOperation, true);
                        DataTable dtSelect = GetDtRegister();
                        Util.GridSetData(dgSelectRegister, dtSelect, FrameOperation, false);
                        txtCtnridRegister.Text = string.Empty;
                        txtCtnridRegister.Focus();

                    }
                    else
                    {
                        Util.gridClear(dgListRegister);
                        Util.GridSetData(dgListRegister, dtRslt, FrameOperation, true); DataTable dtSelect = GetDtRegister();
                        Util.GridSetData(dgSelectRegister, dtSelect, FrameOperation, false);
                        txtCtnridRegister.Text = string.Empty;
                        txtCtnridRegister.Focus();
                    }


                }
                else
                {
                    Util.GridSetData(dgListRegister, dtRslt, FrameOperation, false);
                    Util.gridClear(dgSelectRegister);

                    DataTable dtSelect = GetDtRegister();
                    Util.GridSetData(dgSelectRegister, dtSelect, FrameOperation, false);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 비용처리 완료 등록 조회
        public void GetLotList_Complete()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROCID"] = Util.GetCondition(cboProcComplete, "SFU3207");//공정을 선택하세요"."  
                if (dr["PROCID"].Equals("")) return;
                //dr["EQSGID"] = Util.GetCondition(cboEqsgComplete, "SFU1223");//라인을 선택하세요"."  
                //if (dr["EQSGID"].Equals("")) return;

                if(cboEqsgComplete.SelectedIndex != 0)
                {
                    dr["EQSGID"] = cboEqsgComplete.SelectedValue.ToString();
                }

                dr["PJT_NAME"] = Util.ConvertEmptyToNull(txtPrjtComplete.Text.Trim());
                dr["PRODID"] = Util.ConvertEmptyToNull(txtProdComplete.Text.Trim());
                dr["LOTID_RT"] = Util.ConvertEmptyToNull(txtLotIdRTComplete.Text.Trim());
                dr["CTNR_ID"] = Util.ConvertEmptyToNull(txtCtnrIdComplete.Text.Trim());
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_COST_CTNR_COMPLETE", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    //Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListComplete, dtRslt, FrameOperation, false);
                    Util.gridClear(dgSelectComplete);

                    return;
                }

                Util.GridSetData(dgListComplete, dtRslt, FrameOperation, false);
                Util.gridClear(dgSelectComplete);

                DataTable dtSelect = GetDtComplete();
                Util.GridSetData(dgSelectComplete, dtSelect, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 비용처리 대상 등록 취소 조회
        private void GetLotList_Cancel()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROCID"] = Util.GetCondition(cboProcCancel, "SFU3207");//공정을 선택하세요"."  
                if (dr["PROCID"].Equals("")) return;
                //dr["EQSGID"] = Util.GetCondition(cboEqsgCancel, "SFU1223");//라인을 선택하세요"."  
                //if (dr["EQSGID"].Equals("")) return;
                if (cboEqsgCancel.SelectedIndex != 0)
                {
                    dr["EQSGID"] = cboEqsgCancel.SelectedValue.ToString();
                }
                dr["PJT_NAME"] = Util.ConvertEmptyToNull(txtPrjtCancel.Text.Trim());
                dr["PRODID"] = Util.ConvertEmptyToNull(txtProdCancel.Text.Trim());
                dr["LOTID_RT"] = Util.ConvertEmptyToNull(txtLotIdRTCancel.Text.Trim());
                dr["CTNR_ID"] = Util.ConvertEmptyToNull(txtCtnrIdCancel.Text.Trim());
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_COST_CTNR_COMPLETE", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListCancel, dtRslt, FrameOperation, false);
                    Util.gridClear(dgSelectCancel);

                    return;
                }

                Util.GridSetData(dgListCancel, dtRslt, FrameOperation, false);
                Util.gridClear(dgSelectCancel);

                DataTable dtSelect = GetDtComplete();
                Util.GridSetData(dgSelectCancel, dtSelect, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 비용처리 대상 이력 조회 조회
        public void GetLotList_History()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROMDATE", typeof(string));
                dtRqst.Columns.Add("TODATE", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROCID"] = Util.GetCondition(cboProcHistory, "SFU3207");//공정을 선택하세요"."  
                if (dr["PROCID"].Equals("")) return;
                //dr["EQSGID"] = Util.GetCondition(cboEqsgHistory, "SFU1223");//라인을 선택하세요"."  
                //if (dr["EQSGID"].Equals("")) return;
                if (cboEqsgHistory.SelectedIndex != 0)
                {
                    dr["EQSGID"] = cboEqsgHistory.SelectedValue.ToString();
                }
                dr["PJT_NAME"] = Util.ConvertEmptyToNull(txtPrjtHistory.Text.Trim());
                dr["PRODID"] = Util.ConvertEmptyToNull(txtProdHistory.Text.Trim());
                dr["LOTID_RT"] = Util.ConvertEmptyToNull(txtLotIdRTHistory.Text.Trim());
                dr["CTNR_ID"] = Util.ConvertEmptyToNull(txtCtnrIdHistory.Text.Trim());
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToShortDateString();
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToShortDateString();
                dr["SUBLOTID"] = Util.ConvertEmptyToNull(txtCellID.Text.Trim());

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_COST_CTNR_HISTORY", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListHistory, dtRslt, FrameOperation, false);
                    Util.gridClear(dgSelectHistory);

                    return;
                }

                Util.GridSetData(dgListHistory, dtRslt, FrameOperation, false);
                Util.gridClear(dgSelectHistory);

                DataTable dtSelect = GetDtHistory();
                Util.GridSetData(dgSelectHistory, dtSelect, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 선택목록 DataTable
        private DataTable GetDtRegister()
        {
            DataTable dtSelect = new DataTable();

            dtSelect.Columns.Add("EQSGID", typeof(string));
            dtSelect.Columns.Add("EQSGNAME", typeof(string));
            dtSelect.Columns.Add("CTNR_ID", typeof(string));
            dtSelect.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
            dtSelect.Columns.Add("WIP_QLTY_TYPE_NAME", typeof(string));
            dtSelect.Columns.Add("LOTID_RT", typeof(string));
            dtSelect.Columns.Add("LOTID", typeof(string));
            dtSelect.Columns.Add("PRJT_NAME", typeof(string));
            dtSelect.Columns.Add("PRODID", typeof(string));
            dtSelect.Columns.Add("MKT_TYPE_CODE", typeof(string));
            dtSelect.Columns.Add("MKT_TYPE_NAME", typeof(string));
            dtSelect.Columns.Add("WIPQTY", typeof(string));
            dtSelect.Columns.Add("UNIT", typeof(string));

            return dtSelect;
        }

        private DataTable GetDtComplete()
        {
            DataTable dtSelect = new DataTable();

            dtSelect.Columns.Add("EQSGID", typeof(string));
            dtSelect.Columns.Add("EQSGNAME", typeof(string));
            dtSelect.Columns.Add("CTNR_ID", typeof(string));
            dtSelect.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
            dtSelect.Columns.Add("WIP_QLTY_TYPE_NAME", typeof(string));
            dtSelect.Columns.Add("LOTID_RT", typeof(string));
            dtSelect.Columns.Add("LOTID", typeof(string));
            dtSelect.Columns.Add("PRJT_NAME", typeof(string));
            dtSelect.Columns.Add("PRODID", typeof(string));
            dtSelect.Columns.Add("MKT_TYPE_CODE", typeof(string));
            dtSelect.Columns.Add("MKT_TYPE_NAME", typeof(string));
            dtSelect.Columns.Add("WIPQTY", typeof(string));
            dtSelect.Columns.Add("UNIT", typeof(string));
            dtSelect.Columns.Add("ACTID", typeof(string));
            dtSelect.Columns.Add("RESNCODE", typeof(string));
            dtSelect.Columns.Add("RESNNAME", typeof(string));
            dtSelect.Columns.Add("BIZ_WF_DEPTNAME", typeof(string));
            dtSelect.Columns.Add("BIZ_WF_PRCS_USERNAME", typeof(string));
            dtSelect.Columns.Add("BIZ_WF_PRCS_SCHD_CMPL_DATE", typeof(string));
            dtSelect.Columns.Add("REG_USERNAME", typeof(string));
            dtSelect.Columns.Add("COST_PRCS_NOTE", typeof(string));
            dtSelect.Columns.Add("WIPSEQ", typeof(string));
            return dtSelect;
        }


        private DataTable GetDtHistory()
        {
            DataTable dtSelect = new DataTable();
            dtSelect.Columns.Add("PRDT_REQ_PRCS_STAT_CODE", typeof(string));
            dtSelect.Columns.Add("EQSGID", typeof(string));
            dtSelect.Columns.Add("EQSGNAME", typeof(string));
            dtSelect.Columns.Add("CTNR_ID", typeof(string));
            dtSelect.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
            dtSelect.Columns.Add("WIP_QLTY_TYPE_NAME", typeof(string));
            dtSelect.Columns.Add("LOTID_RT", typeof(string));
            dtSelect.Columns.Add("LOTID", typeof(string));
            dtSelect.Columns.Add("PRJT_NAME", typeof(string));
            dtSelect.Columns.Add("PRODID", typeof(string));
            dtSelect.Columns.Add("MKT_TYPE_CODE", typeof(string));
            dtSelect.Columns.Add("MKT_TYPE_NAME", typeof(string));
            dtSelect.Columns.Add("WIPQTY", typeof(string));
            dtSelect.Columns.Add("UNIT", typeof(string));
            dtSelect.Columns.Add("ACTID", typeof(string));
            dtSelect.Columns.Add("RESNCODE", typeof(string));
            dtSelect.Columns.Add("RESNNAME", typeof(string));
            dtSelect.Columns.Add("BIZ_WF_DEPTNAME", typeof(string));
            dtSelect.Columns.Add("BIZ_WF_PRCS_USERNAME", typeof(string));
            dtSelect.Columns.Add("BIZ_WF_PRCS_SCHD_CMPL_DATE", typeof(string));
            dtSelect.Columns.Add("REG_USERNAME", typeof(string));
            dtSelect.Columns.Add("CMPL_USERNAME", typeof(string));
            dtSelect.Columns.Add("CMPL_DTTM", typeof(string));
            dtSelect.Columns.Add("COST_PRCS_NOTE", typeof(string));
            dtSelect.Columns.Add("WIPSEQ", typeof(string));
            return dtSelect;
        }


        #endregion
        #endregion

        #region [선택목록 등록]

        #region 비용처리 대상 등록
        private void REQ_Register()
        {
            try
            {

                DataSet inData = new DataSet();

                //INDATA
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("BIZ_WF_DEPTID", typeof(string));
                inDataTable.Columns.Add("BIZ_WF_PRCS_USERID", typeof(string));
                inDataTable.Columns.Add("BIZ_WF_PRCS_SCHD_CMPL_DATE", typeof(string));
                inDataTable.Columns.Add("COST_PRCS_SLOC_ID", typeof(string));
                inDataTable.Columns.Add("RESNCODE", typeof(string));
                inDataTable.Columns.Add("RESNCODE_CAUSE", typeof(string));
                inDataTable.Columns.Add("PROCID_CAUSE", typeof(string));
                inDataTable.Columns.Add("RESNNOTE", typeof(string));
                inDataTable.Columns.Add("COST_CNTR_ID", typeof(string));
                DataRow row = null;
                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["USERID"] = LoginInfo.USERID;
                row["IFMODE"] = "OFF";
                row["BIZ_WF_DEPTID"] = txtDept.Tag;
                row["BIZ_WF_PRCS_USERID"] = txtPrscUser.Tag;
                row["BIZ_WF_PRCS_SCHD_CMPL_DATE"] = ldpCmplDateRegister.SelectedDateTime.ToString("yyyyMMdd");
                row["COST_PRCS_SLOC_ID"] = cboCostSloc.SelectedValue.ToString(); ;
                row["RESNCODE"] = cboResnCode.SelectedValue.ToString();
                row["RESNNOTE"] = txtNoteRegister.Text;
                inDataTable.Rows.Add(row);

                //INCTNR
                DataTable dtSelectRegister = DataTableConverter.Convert(dgSelectRegister.ItemsSource);
                DataTable inCtnr = inData.Tables.Add("INCTNR");
                inCtnr.Columns.Add("CTNR_ID", typeof(string));
                for (int i = 0; i < dtSelectRegister.Rows.Count; i++)
                {
                    row = inCtnr.NewRow();
                    row["CTNR_ID"] = Convert.ToString(dtSelectRegister.Rows[i]["CTNR_ID"]);
                    inCtnr.Rows.Add(row);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                //비용처리 대상 등록 처리
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_COST_PRDT_REQ_CTNR", "INDATA,INCTNR", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU3532");//저장되었습니다.

                        GetLotList_Register();

                        cboResnCode.SelectedIndex = 0;
                        cboCostSloc.SelectedIndex = 0;
                        txtDept.Text = string.Empty;
                        txtDept.Tag = string.Empty;
                        txtPrscUser.Text = string.Empty;
                        txtPrscUser.Tag = string.Empty;
                        ldpCmplDateRegister.SelectedDateTime = DateTime.Now;

                        //비고를 삭제하시겠습니까?
                        Util.MessageConfirm("SFU3816", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtNoteRegister.Text = string.Empty;
                            }
                        });

                        chkAll.IsChecked = false;

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_ACT_REG_COST_PRDT_REQ_CTNR", ex.Message, ex.ToString());
            }
        }
        #endregion

        #region 비용처리 완료 등록
        private void REQ_Complete()
        {
            try
            {
                DataSet inData = new DataSet();
                DataTable dtSelectComplete = DataTableConverter.Convert(dgSelectComplete.ItemsSource);
                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("WIPNOTE", typeof(string));
                inDataTable.Columns.Add("PRDT_REQ_PRCS_STAT_CODE", typeof(string));
                inDataTable.Columns.Add("BIZ_WF_PRCS_END_DATE", typeof(string));
                inDataTable.Columns.Add("ACTID", typeof(string));

                DataRow row = null;

                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["IFMODE"] = "OFF";
                row["USERID"] = LoginInfo.USERID;
                row["WIPNOTE"] = txtNoteComplete.Text;
                row["PRDT_REQ_PRCS_STAT_CODE"] = "COMPLETE";
                row["BIZ_WF_PRCS_END_DATE"] = ldpCmplDateComplete.SelectedDateTime.ToString("yyyyMMdd");
                row["ACTID"] = Convert.ToString(dtSelectComplete.Rows[0]["ACTID"]);
                inDataTable.Rows.Add(row);

                DataTable inCtnr = inData.Tables.Add("INCTNR");
                inCtnr.Columns.Add("CTNR_ID", typeof(string));
                inCtnr.Columns.Add("RESNCODE", typeof(string));


                for (int i = 0; i < dtSelectComplete.Rows.Count; i++)
                {
                    row = inCtnr.NewRow();
                    row["CTNR_ID"] = Convert.ToString(dtSelectComplete.Rows[i]["CTNR_ID"]);
                    row["RESNCODE"] = Convert.ToString(dtSelectComplete.Rows[i]["RESNCODE"]);
                    inCtnr.Rows.Add(row);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                //비용처리 대상 완료등록 처리
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_CANCEL_COST_PRDT_REQ_CTNR", "INDATA,INCTNR", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU3532");//저장되었습니다.

                        GetLotList_Complete();

                        ldpCmplDateComplete.SelectedDateTime = DateTime.Now;

                        //비고를 삭제하시겠습니까?
                        Util.MessageConfirm("SFU3816", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtNoteComplete.Text = string.Empty;
                            }
                        });

                        chkAllComplete.IsChecked = false;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_ACT_REG_CANCEL_COST_PRDT_REQ_CTNR", ex.Message, ex.ToString());
            }
        }

        #endregion

        #region 비용처리 대상 등록 취소
        private void REQ_Cancel()
        {
            try
            {
                DataSet inData = new DataSet();
                DataTable dtSelectCancel = DataTableConverter.Convert(dgSelectCancel.ItemsSource);
                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("WIPNOTE", typeof(string));
                inDataTable.Columns.Add("PRDT_REQ_PRCS_STAT_CODE", typeof(string));
                inDataTable.Columns.Add("ACTID", typeof(string));

                DataRow row = null;

                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["IFMODE"] = "OFF";
                row["USERID"] = LoginInfo.USERID;
                row["WIPNOTE"] = txtNoteCancel.Text;
                row["PRDT_REQ_PRCS_STAT_CODE"] = "CANCEL";
                row["ACTID"] = Convert.ToString(dtSelectCancel.Rows[0]["ACTID"]);


                inDataTable.Rows.Add(row);

                DataTable inCtnr = inData.Tables.Add("INCTNR");
                inCtnr.Columns.Add("CTNR_ID", typeof(string));
                inCtnr.Columns.Add("RESNCODE", typeof(string));


                for (int i = 0; i < dtSelectCancel.Rows.Count; i++)
                {
                    row = inCtnr.NewRow();
                    row["CTNR_ID"] = Convert.ToString(dtSelectCancel.Rows[i]["CTNR_ID"]);
                    row["RESNCODE"] = Convert.ToString(dtSelectCancel.Rows[i]["RESNCODE"]);
                    inCtnr.Rows.Add(row);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                //비용처리 대상 등록 취소 처리
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_CANCEL_COST_PRDT_REQ_CTNR", "INDATA,INCTNR", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU3532");//저장되었습니다.

                        GetLotList_Cancel();
                        //비고를 삭제하시겠습니까?
                        Util.MessageConfirm("SFU3816", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtNoteCancel.Text = string.Empty;
                            }
                        });

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_ACT_REG_CANCEL_COST_PRDT_REQ_CTNR", ex.Message, ex.ToString());
            }
        }

        #endregion

        #region 비용처리 대상 이력 조회
        private void REQ_History()
        {
            try
            {
                DataSet inData = new DataSet();
                DataTable dtSelectHistory = DataTableConverter.Convert(dgSelectHistory.ItemsSource);
                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");

                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("WIPNOTE", typeof(string));
                inDataTable.Columns.Add("PRDT_REQ_PRCS_STAT_CODE", typeof(string));
                inDataTable.Columns.Add("ACTID", typeof(string));


                DataRow row = null;

                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["IFMODE"] = "OFF";
                row["USERID"] = LoginInfo.USERID;
                row["WIPNOTE"] = txtNoteHistory.Text;
                row["PRDT_REQ_PRCS_STAT_CODE"] = "CREATE";
                row["ACTID"] = Convert.ToString(dtSelectHistory.Rows[0]["ACTID"]);

                inDataTable.Rows.Add(row);

                DataTable inCtnr = inData.Tables.Add("INCTNR");
                inCtnr.Columns.Add("CTNR_ID", typeof(string));
                inCtnr.Columns.Add("RESNCODE", typeof(string));


                for (int i = 0; i < dtSelectHistory.Rows.Count; i++)
                {
                    row = inCtnr.NewRow();
                    row["CTNR_ID"] = Convert.ToString(dtSelectHistory.Rows[i]["CTNR_ID"]);
                    row["RESNCODE"] = Convert.ToString(dtSelectHistory.Rows[i]["RESNCODE"]);
                    inCtnr.Rows.Add(row);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                //비용처리 대상 완료취소 처리
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_CANCEL_COST_PRDT_REQ_CTNR", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU3532");//저장되었습니다.

                        GetLotList_History();

                        //비고를 삭제하시겠습니까?
                        Util.MessageConfirm("SFU3816", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtNoteHistory.Text = string.Empty;
                            }
                        });

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_ACT_REG_CANCEL_COST_PRDT_REQ_CTNR", ex.Message, ex.ToString());
            }
        }

        #endregion

        #endregion

        #region [Validation]

        #region 비용처리 대상 등록
        private bool Validation_Register()
        {
            if (dgSelectRegister.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU3538"); //선택된 데이터가 없습니다.
                return false;
            }

            DataTable dtSelectRegister = DataTableConverter.Convert(dgSelectRegister.ItemsSource);


            if (cboResnCode.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU1593"); //사유를 선택하세요.
                return false;
            }

            if (cboCostSloc.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU4131"); //비용저장위치를 선택해 주세요.
                return false;
            }

            //if (String.IsNullOrEmpty((String)txtDept.Tag))
            //{
            //    Util.MessageValidation("SFU4105"); //BizWF 처리 부서를 선택해 주세요.
            //    return false;
            //}

            if (String.IsNullOrEmpty((String)txtPrscUser.Tag))
            {
                Util.MessageValidation("SFU4106"); //BizWF 처리 담당자를 선택해 주세요.
                return false;
            }

            return true;
        }
        #endregion

        #region 비용처리 완료 등록
        private bool Validation_Complete()
        {
            if (dgSelectComplete.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU3538"); //선택된 데이터가 없습니다.
                return false;
            }

            return true;
        }
        #endregion

        #region 비용처리 등록 취소
        private bool Validation_Cancel()
        {
            if (dgSelectCancel.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU3538"); //선택된 데이터가 없습니다.
                return false;
            }

            return true;
        }
        #endregion

        #region 비용처리 대상 이력조회
        private bool Validation_History()
        {
            if (dgSelectHistory.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU3538"); //선택된 데이터가 없습니다.
                return false;
            }

            return true;
        }



        #endregion

        #endregion

        #endregion

        private void txtCtnridRegister_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sCtnrId = txtCtnridRegister.Text.Trim();
                    if (dgListRegister.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListRegister.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListRegister.Rows[i].DataItem, "CTNR_ID").ToString() == sCtnrId)
                            {

                                dgListRegister.SelectedIndex = i;
                                dgListRegister.ScrollIntoView(i, dgListRegister.Columns["CHK"].Index);
                                txtCtnridRegister.Focus();
                                txtCtnridRegister.Text = string.Empty;
                                return;
                            }
                        }
                    }
                    GetLotList_Register(false);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListRegister_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgSelectRegister);

            if ((bool)chkAll.IsChecked)
            {

                // 동일한 PJT 만 전체 선택 가능하도록
                if (dgListRegister.GetRowCount() > 0) {
                    if (DataTableConverter.Convert(dgListRegister.ItemsSource).Select("PRJT_NAME <> '" + Util.NVC(DataTableConverter.GetValue(dgListRegister.Rows[0].DataItem, "PRJT_NAME")) + "'").Length >= 1)
                    {
                        Util.MessageValidation("SFU4492"); //동일한 PJT 만 전체 선택이 가능합니다.
                        chkAll.IsChecked = false;
                        return;
                    }
                }

                for (int inx = 0; inx < dgListRegister.GetRowCount(); inx++)
                {
                    DataTableConverter.SetValue(dgListRegister.Rows[inx].DataItem, "CHK", 1);
                    object objRowIdx = dgListRegister.Rows[inx].DataItem;
                    AddToSelectRegister(objRowIdx);
                }
            }
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgSelectRegister);

            for (int inx = 0; inx < dgListRegister.GetRowCount(); inx++)
            {
                DataTableConverter.SetValue(dgListRegister.Rows[inx].DataItem, "CHK", 0);
            }
        }

        private void AddToSelectRegister(object objRowIdx)
        {
            try
            {
                if (objRowIdx == null) return;

                if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals(1))
                {
                    if (DataTableConverter.Convert(dgSelectRegister.ItemsSource).Select("CTNR_ID = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "CTNR_ID")) + "'").Length == 1)
                    {
                        Util.MessageValidation("SFU2840", Util.NVC(DataTableConverter.GetValue(objRowIdx, "CTNR_ID")));
                        DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                        return;
                    }
                }

                if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals(1))
                {

                    DataTable dtTo = DataTableConverter.Convert(dgSelectRegister.ItemsSource);
                    DataRow dr = dtTo.NewRow();
                    foreach (DataColumn dc in dtTo.Columns)
                    {
                        dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(objRowIdx, dc.ColumnName));
                    }
                    dtTo.Rows.Add(dr);
                    dgSelectRegister.ItemsSource = DataTableConverter.Convert(dtTo);
                }
                else
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectRegister.ItemsSource);
                    if (dtTo.Rows.Count > 0)
                    {
                        if (dtTo.Select("CTNR_ID = '" + DataTableConverter.GetValue(objRowIdx, "CTNR_ID") + "'").Length > 0)
                        {
                            dtTo.Rows.Remove(dtTo.Select("CTNR_ID = '" + DataTableConverter.GetValue(objRowIdx, "CTNR_ID") + "'")[0]);
                            dgSelectRegister.ItemsSource = DataTableConverter.Convert(dtTo);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        void checkAllComplete_Checked(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgSelectComplete);

            if ((bool)chkAllComplete.IsChecked)
            {

                // 동일한 PJT 만 전체 선택 가능하도록
                if (dgListComplete.GetRowCount() > 0)
                {
                    if (DataTableConverter.Convert(dgListComplete.ItemsSource).Select("PRJT_NAME <> '" + Util.NVC(DataTableConverter.GetValue(dgListComplete.Rows[2].DataItem, "PRJT_NAME")) + "'").Length >= 1)
                    {
                        Util.MessageValidation("SFU4492"); //동일한 PJT 만 전체 선택이 가능합니다.
                        chkAllComplete.IsChecked = false;
                        return;
                    }
                }

                for (int inx = 0; inx < dgListComplete.GetRowCount(); inx++)
                {
                    DataTableConverter.SetValue(dgListComplete.Rows[inx + 2].DataItem, "CHK", true);
                    object objRowIdx = dgListComplete.Rows[inx + 2].DataItem;
                    AddToSelectComplete(objRowIdx);
                }
            }
        }

        void checkAllComplete_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgSelectComplete);

            for (int inx = 0; inx < dgListComplete.GetRowCount(); inx++)
            {
                DataTableConverter.SetValue(dgListComplete.Rows[inx + 2].DataItem, "CHK", false);
            }
        }

        private void AddToSelectComplete(object objRowIdx)
        {
            try
            {
                if (objRowIdx == null) return;

                if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals(1))
                {
                    if (DataTableConverter.Convert(dgSelectComplete.ItemsSource).Select("CTNR_ID = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "CTNR_ID")) + "' AND WIPSEQ = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "WIPSEQ")) + "' AND RESNCODE = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "RESNCODE")) + "'").Length == 1)
                    {
                        Util.MessageValidation("SFU2840", Util.NVC(DataTableConverter.GetValue(objRowIdx, "CTNR_ID")));
                        DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                        return;
                    }
                }

                if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals(1))
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectComplete.ItemsSource);
                    DataRow dr = dtTo.NewRow();
                    foreach (DataColumn dc in dtTo.Columns)
                    {
                        dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(objRowIdx, dc.ColumnName));
                    }
                    dtTo.Rows.Add(dr);
                    dgSelectComplete.ItemsSource = DataTableConverter.Convert(dtTo);
                }
                else
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectComplete.ItemsSource);
                    if (dtTo.Rows.Count > 0)
                    {
                        if (dtTo.Select("CTNR_ID = '" + DataTableConverter.GetValue(objRowIdx, "CTNR_ID") + "'").Length > 0)
                        {
                            dtTo.Rows.Remove(dtTo.Select("CTNR_ID = '" + DataTableConverter.GetValue(objRowIdx, "CTNR_ID") + "'")[0]);
                            dgSelectComplete.ItemsSource = DataTableConverter.Convert(dtTo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnCartCellRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRow[] dr;

                if (dgListRegister.GetRowCount() <= 0)
                {
                    Util.MessageValidation("SFU3538"); //선택된 데이터가 없습니다.
                    return;
                }

                dr = DataTableConverter.Convert(dgListRegister.ItemsSource).Select("CHK = 1");

                if (dr.Length > 1)
                {
                    Util.AlertInfo("SFU4468"); //한개의 데이터만 선택하세요
                    return;
                }
                else if (dr.Length == 0)
                {
                    Util.AlertInfo("SFU1636"); // 선택된 대상이 없습니다.
                    return;
                }

                CMM_ASSY_LOSS_CELL_INPUT popupoutput = new CMM_ASSY_LOSS_CELL_INPUT();
                popupoutput.FrameOperation = this.FrameOperation;

                if (popupoutput != null)
                {
                    object[] parameters = new object[1];
                    parameters[0] = Util.NVC(dr[0]["CTNR_ID"]);

                    popupoutput.Closed += new EventHandler(popupoutput_Closed);

                    C1WindowExtension.SetParameters(popupoutput, parameters);
                    grdMain.Children.Add(popupoutput);
                    popupoutput.BringToFront();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnCartCellSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgListHistory.GetRowCount() <= 0)
                {
                    Util.MessageValidation("SFU3791"); //대상목록에서 조회할 대차ID 열을 클릭해 주세요.
                    return;
                }

                if (dgListHistory.SelectedIndex < 2)
                {
                    Util.AlertInfo("SFU3791"); //대상목록에서 조회할 대차ID 열을 클릭해 주세요.
                    return;
                }

                CMM_ASSY_LOSS_CELL_INPUT popupoutput = new CMM_ASSY_LOSS_CELL_INPUT();
                popupoutput.FrameOperation = this.FrameOperation;

                if(popupoutput != null)
                {
                    object[] parameters = new object[2];
                    parameters[0] = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[dgListHistory.SelectedIndex].DataItem, "CTNR_ID"));
                    parameters[1] = "SELECT";

                    popupoutput.Closed += new EventHandler(popupoutput_Closed);

                    C1WindowExtension.SetParameters(popupoutput, parameters);
                    grdMain.Children.Add(popupoutput);
                    popupoutput.BringToFront();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void popupoutput_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_LOSS_CELL_INPUT window = sender as CMM_ASSY_LOSS_CELL_INPUT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(window);
        }
    }
}
