/*************************************************************************************
 Created Date : 2017.08.15
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - C 생산 관리 화면
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.15  INS 김동일K : Initial Created.
  2017.09.29  CNS 고현영S : BR,DA추가 및 입고,재작업 개발
  2017.12.04  CNS 고현영S : 현장 업무 프로세스 변경에 따른 UI재구성
  2017.12.21  INS 김동일K : 업무 이관에 따른 프로그램 미개발 처리 및 오류 수정
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
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
using System.Windows.Threading;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_014.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_014 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();
        private String _LineID = String.Empty;
        private String _EqptID = String.Empty;
        //private String _txtUserID = String.Empty;
        private String _TmptxtUserID = String.Empty;

        private int _dgWorkHist_RecycBeginIndex = -1;
        private int _dgRecycFcColumnIndex_Boxtab = 14;

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

        #region Popup 처리 로직 변경

        ASSY003_014_IN_CPROD wndInCProd;
        ASSY003_014_REWORK wndRework;
        ASSY003_014_MAG_CREATE wndMagCreate;
        ASSY003_014_HIST_DETAIL wndHistDetail;
        ASSY003_014_CONFIRM wndConfirm;
        Report_CProd_Out rptCProdOut;
        CMM_SHIFT_USER2 wndShiftUser;
        ASSY003_014_CHG_QTY wndChgQty;
        ASSY003_014_BICELL_IN wndBicellIn;        
        #endregion

        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY003_014()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //라인 Combo
            String[] sFilter1 = { LoginInfo.CFG_AREA_ID, null, Process.CPROD };
            C1ComboBox[] cboLineChild1 = { cboEquipment };
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild1, sCase: "cboEquipmentSegmentAssy", sFilter: sFilter1);
            
            //설비 Combo
            String[] sFilter2 = { Process.CPROD};
            C1ComboBox[] cboEquipmentParent1 = { cboLine };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent1, sCase: "EQUIPMENT_MAIN_LEVEL", sFilter: sFilter2);


            #region C생산입고 Tab

            //재작업 유형 Combo
            String[] sFilter3 = { "CPROD_WRK_TYPE_CODE" };
            C1ComboBox[] cboProcessChild = { cboLine_tabCProdIn };
            _combo.SetCombo(cboProcess_tabCProdIn, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChild, sCase: "COMMCODE", sFilter: sFilter3);

            //인계라인 Combo
            String[] sFilter4 = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineParent2 = { cboProcess_tabCProdIn };
            C1ComboBox[] cboLineChild2 = { cboEquipment_tabCProdIn };
            _combo.SetCombo(cboLine_tabCProdIn, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent2, cbChild: cboLineChild2, sCase: "FROM_EQUIPMENTSEGMENT_CPROD", sFilter: sFilter4);
            //C1ComboBox[] cboLineParent2 = { cboLine, cboProcess_tabCProdIn };
            //C1ComboBox[] cboLineChild2 = { cboEquipment_tabCProdIn };
            //_combo.SetCombo(cboLine_tabCProdIn, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent2, cbChild: cboLineChild2, sCase: "FROM_EQUIPMENTSEGMENT_CPROD");

            //인계설비 Combo
            //String[] sFilter5 = { Process.STACKING_FOLDING };
            C1ComboBox[] cboEquipmentParent2 = { cboProcess_tabCProdIn, cboLine_tabCProdIn };
            _combo.SetCombo(cboEquipment_tabCProdIn, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent2, sCase: "FROM_EQUIPMENT_CPROD");

            #endregion

            #region Lami Cell 재공/출고 Tab

            //셀타입 Combo
            String[] sFilter6 = { "BICELL_TYPE_FD" };
            _combo.SetCombo(cboBiCell_tabBiCell, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter6);

            String[] sFilter7 = { "MKT_TYPE_CODE" };
            _combo.SetCombo(cboMarket_tabBiCell, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter7);

            #endregion

            #region Folding Box 재공/출고 Tab

            //재작업 구분
            String[] sFilter8 = { "CPROD_WRK_TYPE_CODE" };
            C1ComboBox[] cboProcessChild2 = { cboLine_tabFoldingBox };
            _combo.SetCombo(cboRecyc_tabFoldingBox, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChild2, sCase: "COMMCODE", sFilter: sFilter8);

            //인계라인 
            String[] sFilter9 = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineParent3 = { cboRecyc_tabFoldingBox };
            _combo.SetCombo(cboLine_tabFoldingBox, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent3, sCase: "FROM_EQUIPMENTSEGMENT_CPROD", sFilter: sFilter9);

            #endregion

            #region 입고이력조회 Tab

            //재작업 구분
            String[] sFilter10 = { "CPROD_WRK_TYPE_CODE" };
            C1ComboBox[] cboProcessChild3 = { cboLine_tabInHist };
            _combo.SetCombo(cboRecyc_tabInHist, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChild3, sCase: "COMMCODE", sFilter: sFilter8);

            //인계라인
            String[] sFilter11 = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineParent4 = { cboRecyc_tabInHist };
            C1ComboBox[] cboLineChild4 = { cboEquipment_tabInHist };
            _combo.SetCombo(cboLine_tabInHist, CommonCombo.ComboStatus.ALL, cbChild: cboLineChild4, cbParent: cboLineParent4,  sCase: "FROM_EQUIPMENTSEGMENT_CPROD", sFilter: sFilter11);

            //인계설비
            C1ComboBox[] cboEquipmentParent3 = { cboRecyc_tabInHist, cboLine_tabInHist };
            _combo.SetCombo(cboEquipment_tabInHist, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent3, sCase: "FROM_EQUIPMENT_CPROD");
            #endregion

            #region 재작업실적조회 Tab

            //재작업구분
            String[] sFilter12 = { "CPROD_WRK_TYPE_CODE" };
            C1ComboBox[] cboProcessChild4 = { cboEquipment_tabWorkHist };
            _combo.SetCombo(cboRecyc_tabWorkHist, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChild4, sCase: "COMMCODE", sFilter: sFilter12);

            //인계라인
            String[] sFilter13 = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineParent5 = { cboRecyc_tabWorkHist };
            _combo.SetCombo(cboEquipment_tabWorkHist, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent5, sCase: "FROM_EQUIPMENTSEGMENT_CPROD", sFilter: sFilter13);

            #endregion

            #region 출고이력조회 Tab

            //인계공정
            String[] sFilter17 = { LoginInfo.CFG_AREA_ID }; 
            _combo.SetCombo(cboProcess_tabOutHist, CommonCombo.ComboStatus.ALL, sCase: "TO_PROCID_CPROD", sFilter: sFilter17);

            //재작업구분
            String[] sFilter14 = { "CPROD_WRK_TYPE_CODE" };
            C1ComboBox[] cboProcessChild5 = { cboLine_tabOutHist };
            _combo.SetCombo(cboRecyc_tabOutHist, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChild5, sCase: "COMMCODE", sFilter: sFilter14);


            //인계라인 
            String[] sFilter15 = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineParent6 = { cboRecyc_tabOutHist };
            _combo.SetCombo(cboLine_tabOutHist, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent6, sCase: "FROM_EQUIPMENTSEGMENT_CPROD", sFilter: sFilter15);
            

            //이동상태
            String[] sFilter16 = { "MOVE_ORD_STAT_CODE" };
            _combo.SetCombo(cboState_tabOutHist, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter16);


            #endregion
        }
        #endregion

        #region [공통]

        #region Event
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            try
            {
                InitCombo();

                _dgWorkHist_RecycBeginIndex = dgLotList_tabWorkHist.Columns.Count;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                #region Popup 처리 로직 변경

                if (wndInCProd != null)
                    wndInCProd.BringToFront();

                if (wndMagCreate != null)
                    wndMagCreate.BringToFront();

                if (wndRework != null)
                    wndRework.BringToFront();

                if (wndHistDetail != null)
                    wndHistDetail.BringToFront();

                if (wndShiftUser != null)
                    wndShiftUser.BringToFront();

                if (wndChgQty != null)
                    wndChgQty.BringToFront();

                if (wndBicellIn != null)
                    wndBicellIn.BringToFront();

                #endregion

                ApplyPermissions();

                    //SetControls();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                switch (tabControl.SelectedIndex)
                {
                    //C생산입고
                    case 0:
                        //if (CanSearch(false))
                        //btnSearch_tabCProdIn_Click(sender, null);
                        break;

                    //Lami Cell 재공/출고
                    case 1:
                        //if (CanSearchBiCell(false))
                        //    btnSearch_tabBiCell_Click(sender, null);
                        break;

                    //Folding BOX 재공/출고
                    case 2:
                        break;

                    //입고이력조회
                    case 3:
                        break;

                    //재작업실적조회
                    case 4:
                        //if (CanHistSearch(false) && dtpFrom_tabWorkHist.IsMeasureValid && dtpTo_tabWorkHist.IsMeasureValid)
                            //btnSearchHist_Click(sender, null);
                        break;

                    //출고이력조회
                    case 5:
                        //if (dtpFrom_tabWorkHist.IsMeasureValid && dtpTo_tabWorkHist.IsMeasureValid)
                            //btnSearchHist_Click(sender, null);
                        break;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtUserName_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetUserInfo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtUserName_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtUserName == null) return;

                if (!txtUserName.Text.Trim().Equals("") && Util.NVC(txtUserName.Tag).Equals(""))
                    GetUserInfo();

                if (Util.NVC(txtUserName.Tag).Equals(""))
                    txtUserName.Text = "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtUserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (txtUserName == null) return;

                txtUserName.Tag = "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgGratorChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                txtUserName.TextChanged -= txtUserName_TextChanged;
                txtUserName.Tag = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID"));
                txtUserName.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME"));
                txtUserName.TextChanged += txtUserName_TextChanged;

                dgGratorSelect.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtUserName_GotFocus(object sender, RoutedEventArgs e)
        {
            dgGratorSelect.Visibility = Visibility.Collapsed;
        }

        private void btnPerson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CMM_PERSON wndPerson = new CMM_PERSON();
                wndPerson.FrameOperation = this.FrameOperation;

                if (wndPerson != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = txtUserName.Text;

                    C1WindowExtension.SetParameters(wndPerson, Parameters);

                    wndPerson.Closed += new EventHandler(wndPerson_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPerson.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndPerson_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPopup = sender as CMM_PERSON;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = wndPopup.USERNAME;
                txtUserName.Tag = wndPopup.USERID;
            }
        }
        #endregion

        #region Func
        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void SetChkBoxControls(DataGridCellEventArgs e, C1DataGrid dg)
        {
            try
            {
                int preValue = (int)e.Cell.Value;

                Util.DataGridCheckAllUnChecked(dg);

                if (preValue > 0) e.Cell.Value = true;
                else e.Cell.Value = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnInCProd_tabCProdIn);
            listAuth.Add(btnSearch_taCProdIn);
            listAuth.Add(btnIn_tabBiCell);
            listAuth.Add(btnOut_tabBiCell);
            listAuth.Add(btnChgQty_tabBiCell);
            listAuth.Add(btnSearch_tabBiCell);
            listAuth.Add(btnOut_tabFoldingBox);
            listAuth.Add(btnSearch_tabFoldingBox);
            listAuth.Add(btnInCancel_tabInHist);
            listAuth.Add(btnSearch_tabInHist);
            listAuth.Add(btnSearch_tabWorkHist);
            listAuth.Add(btnSearch_tabOutHist);
            listAuth.Add(btnPrint_tabOutHist);
            listAuth.Add(btnOutCancel_tabOutHist);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void GetUserInfo()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("USERNAME", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["USERNAME"] = txtUserName.Text;
                dr["LANGID"] = LoginInfo.LANGID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.Alert("SFU1592");  //사용자 정보가 없습니다.
                }
                else if (dtRslt.Rows.Count == 1)
                {
                    txtUserName.Tag = Util.NVC(dtRslt.Rows[0]["USERID"]);
                }
                else
                {
                    dgGratorSelect.Visibility = Visibility.Visible;

                    Util.gridClear(dgGratorSelect);

                    dgGratorSelect.ItemsSource = DataTableConverter.Convert(dtRslt);

                    this.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region [C 생산 입고 영역]

        #region Event
        private void btnInCProd_tabCProdIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanIn_tabCProdIn())
                    return;
                
                Util.MessageConfirm("SFU2073", (result) =>  // 입고 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        InCProdProcess();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_taCProdIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanSearch_tbCProdIn())
                    return;
                
                GetCProdInList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCProdLot_tabCProdIn_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    // 권한 없으면 Skip.
                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                        return;

                    this.Dispatcher.BeginInvoke(new Action(() => btnSearch_taCProdIn_Click(null, null)));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgLotList_tabCProdIn_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                //C1DataGrid dg = sender as C1DataGrid;

                //if (e.Cell != null &&
                //    e.Cell.Presenter != null &&
                //    e.Cell.Presenter.Content != null)
                //{
                //    CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                //    if (chk != null)
                //    {
                //        switch (Convert.ToString(e.Cell.Column.Name))
                //        {
                //            case "CHK":

                //                SetChkBoxControls(e, dgLotList_tabCProdIn);
                //                break;
                //        }
                //    }
                //}
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboProcess_tabCProdIn_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboLine.SelectedIndex >= 0 && !cboLine.SelectedValue.ToString().Trim().Equals("SELECT") &&
                cboEquipment.SelectedIndex >= 0 && !cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                this.Dispatcher.BeginInvoke(new Action(() => btnSearch_taCProdIn_Click(null, null)));
            }
        }

        private void cboLine_tabCProdIn_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboLine.SelectedIndex >= 0 && !cboLine.SelectedValue.ToString().Trim().Equals("SELECT") &&
                cboEquipment.SelectedIndex >= 0 && !cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                this.Dispatcher.BeginInvoke(new Action(() => btnSearch_taCProdIn_Click(null, null)));
            }
        }

        private void cboEquipment_tabCProdIn_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboLine.SelectedIndex >= 0 && !cboLine.SelectedValue.ToString().Trim().Equals("SELECT") &&
                cboEquipment.SelectedIndex >= 0 && !cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                this.Dispatcher.BeginInvoke(new Action(() => btnSearch_taCProdIn_Click(null, null)));
            }
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgLotList_tabCProdIn.ItemsSource == null) return;

            DataTable dt = DataTableConverter.Convert(dgLotList_tabCProdIn.ItemsSource);
            foreach (DataRow row in dt.Rows)
            {
                //if (Util.NVC(row["DISPATCH_YN"]).Equals("N") && !Util.NVC(row["WIPSTAT"]).Equals("PROC"))
                //{
                    row["CHK"] = true;
                //}
            }
            dgLotList_tabCProdIn.ItemsSource = DataTableConverter.Convert(dt);
        }


        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgLotList_tabCProdIn.ItemsSource == null) return;

            DataTable dt = DataTableConverter.Convert(dgLotList_tabCProdIn.ItemsSource);
            foreach (DataRow row in dt.Rows)
            {
                row["CHK"] = false;
            }
            dgLotList_tabCProdIn.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void dgLotList_tabCProdIn_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e == null || e.Column == null || e.Column.Name == null || e.Column.HeaderPresenter == null) return;

                    if (pre == null) return;

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
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }
        #endregion

        #region Biz
        private void GetCProdInList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("TO_EQSGID", typeof(string));
                inDataTable.Columns.Add("CPROD_WRK_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("CPROD_RWK_LOT_EQSGID", typeof(string));
                inDataTable.Columns.Add("CPROD_RWK_LOT_EQPTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("TO_PROCID", typeof(string)); 

                DataRow newRow = inDataTable.NewRow();

                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["TO_EQSGID"] = Util.NVC(cboLine.SelectedValue);
                newRow["CPROD_WRK_TYPE_CODE"] = Util.NVC(cboProcess_tabCProdIn.SelectedValue).Equals("") ? null : Util.NVC(cboProcess_tabCProdIn.SelectedValue);
                newRow["CPROD_RWK_LOT_EQSGID"] = Util.NVC(cboLine_tabCProdIn.SelectedValue).Equals("") ? null : Util.NVC(cboLine_tabCProdIn.SelectedValue);
                newRow["CPROD_RWK_LOT_EQPTID"] = Util.NVC(cboEquipment_tabCProdIn.SelectedValue).Equals("") ? null : Util.NVC(cboEquipment_tabCProdIn.SelectedValue);
                newRow["LOTID"] = Util.NVC(txtCProdLot_tabCProdIn.Text).Equals("") ? null : Util.NVC(txtCProdLot_tabCProdIn.Text);
                newRow["TO_PROCID"] = Process.CPROD;

                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_MOVING_CPROD_LIST", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgLotList_tabCProdIn, searchResult, FrameOperation, false);
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

        private void InCProdProcess()
        {
            try
            {
                #region 1건 주석...
                //ShowLoadingIndicator();

                //DataSet indataSet = new DataSet();

                //DataTable inDataTable = indataSet.Tables.Add("INDATA");
                //inDataTable.Columns.Add("SRCTYPE", typeof(string));
                //inDataTable.Columns.Add("IFMODE", typeof(string));
                //inDataTable.Columns.Add("EQPTID", typeof(string));
                //inDataTable.Columns.Add("MOVE_ORD_ID", typeof(string));
                //inDataTable.Columns.Add("RCPT_USERID", typeof(string));
                //inDataTable.Columns.Add("NOTE", typeof(string));
                //inDataTable.Columns.Add("USERID", typeof(string));

                //DataTable inCst = indataSet.Tables.Add("IN_LOT");
                //inCst.Columns.Add("LOTID", typeof(string));

                //DataRow newRow = inDataTable.NewRow();
                //newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                //newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                //newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                //newRow["MOVE_ORD_ID"] = Util.NVC(DataTableConverter.GetValue(dgLotList_tabCProdIn.Rows[_Util.GetDataGridCheckFirstRowIndex(dgLotList_tabCProdIn, "CHK")].DataItem, "MOVE_ORD_ID"));
                //newRow["RCPT_USERID"] = Util.NVC(txtUserName.Tag);
                //newRow["NOTE"] = "";
                //newRow["USERID"] = LoginInfo.USERID;

                //inDataTable.Rows.Add(newRow);


                //for (int i = 0; i < dgLotList_tabCProdIn.Rows.Count - dgLotList_tabCProdIn.TopRows.Count - dgLotList_tabCProdIn.BottomRows.Count; i++)
                //{
                //    if (!_Util.GetDataGridCheckValue(dgLotList_tabCProdIn, "CHK", i)) continue;

                //    newRow = null;
                //    newRow = inCst.NewRow();

                //    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotList_tabCProdIn.Rows[i].DataItem, "LOTID"));

                //    inCst.Rows.Add(newRow);
                //}

                //new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RECEIVE_CPROD_RWK_OUT_LOT", "INDATA,IN_LOT", "OUTDATA", (searchResult, searchException) =>
                //{
                //    try
                //    {
                //        if (searchException != null)
                //        {
                //            Util.MessageException(searchException);
                //            return;
                //        }

                //        GetCProdInList();
                //        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                //    }
                //    catch (Exception ex)
                //    {
                //        Util.MessageException(ex);
                //    }
                //    finally
                //    {
                //        HideLoadingIndicator();
                //    }
                //}, indataSet
                //);

                #endregion

                ShowLoadingIndicator();

                for (int i = 0; i < dgLotList_tabCProdIn.Rows.Count - dgLotList_tabCProdIn.TopRows.Count - dgLotList_tabCProdIn.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgLotList_tabCProdIn, "CHK", i)) continue;
                    

                    DataSet indataSet = new DataSet();

                    DataTable inDataTable = indataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("SRCTYPE", typeof(string));
                    inDataTable.Columns.Add("IFMODE", typeof(string));
                    inDataTable.Columns.Add("EQPTID", typeof(string));
                    inDataTable.Columns.Add("MOVE_ORD_ID", typeof(string));
                    inDataTable.Columns.Add("RCPT_USERID", typeof(string));
                    inDataTable.Columns.Add("NOTE", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));

                    DataTable inCst = indataSet.Tables.Add("IN_LOT");
                    inCst.Columns.Add("LOTID", typeof(string));

                    DataRow newRow = inDataTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                    newRow["MOVE_ORD_ID"] = Util.NVC(DataTableConverter.GetValue(dgLotList_tabCProdIn.Rows[i].DataItem, "MOVE_ORD_ID"));
                    newRow["RCPT_USERID"] = Util.NVC(txtUserName.Tag);
                    newRow["NOTE"] = "";
                    newRow["USERID"] = LoginInfo.USERID;

                    inDataTable.Rows.Add(newRow);

                    newRow = null;
                    newRow = inCst.NewRow();

                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotList_tabCProdIn.Rows[i].DataItem, "LOTID"));

                    inCst.Rows.Add(newRow);
                    
                    DataSet dsRtn = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RECEIVE_CPROD_RWK_OUT_LOT", "INDATA,IN_LOT", "OUTDATA", indataSet);
                    
                }

                GetCProdInList();

                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                HideLoadingIndicator();
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
                GetCProdInList();
            }
        }
        #endregion

        #region Validation
        private bool CanSearch_tbCProdIn(bool doPopUp = true)
        {
            bool bRet = false;

            //if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            //{
            //    //Util.Alert("작업장을 선택 하세요.");
            //    if (doPopUp)
            //        Util.MessageValidation("SFU4206");
            //    return bRet;
            //}

            if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                if (doPopUp)
                    Util.MessageValidation("SFU1673");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanIn_tabCProdIn(bool doPopUp = true)
        {
            bool bRet = false;

            //if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
            //{
            //    //Util.Alert("작업장을 선택 하세요.");
            //    if (doPopUp)
            //        Util.MessageValidation("SFU4206");
            //    return bRet;
            //}

            if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                if (doPopUp)
                    Util.MessageValidation("SFU1673");
                return bRet;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgLotList_tabCProdIn, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (Util.NVC(txtUserName.Text).Equals("") || Util.NVC(txtUserName.Tag).Equals(""))
            {
                Util.MessageValidation("SFU4011");
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        #endregion

        #endregion

        #region [Lami Cell 재공/출고]

        #region Event
        private void btnSearch_tabBiCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanSearchBiCell()) return;

                GetBiCellList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnChgQty_tabBiCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanOut_tabBiCell())
                    return;

                if (wndChgQty != null)
                    wndChgQty = null;

                wndChgQty = new ASSY003_014_CHG_QTY();
                wndChgQty.FrameOperation = FrameOperation;

                if (wndChgQty != null)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgBiCellList, "CHK");
                                        
                    object[] Parameters = new object[8];                    
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgBiCellList.Rows[idx].DataItem, "PRJT_NAME"));
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgBiCellList.Rows[idx].DataItem, "PRODID"));
                    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgBiCellList.Rows[idx].DataItem, "PRODUCT_LEVEL3_CODE"));
                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgBiCellList.Rows[idx].DataItem, "PRODUCT_LEVEL3_NAME"));
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgBiCellList.Rows[idx].DataItem, "MKT_TYPE_CODE"));
                    Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgBiCellList.Rows[idx].DataItem, "MKT_TYPE_NAME"));
                    Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgBiCellList.Rows[idx].DataItem, "WIP_QTY"));
                    Parameters[7] = Util.NVC(cboEquipment.SelectedValue);

                    C1WindowExtension.SetParameters(wndChgQty, Parameters);

                    wndChgQty.Closed += new EventHandler(wndChgQty_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndChgQty.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndChgQty_Closed(object sender, EventArgs e)
        {
            wndChgQty = null;
            ASSY003_014_CHG_QTY window = sender as ASSY003_014_CHG_QTY;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetBiCellList();
            }
        }

        private void btnOut_tabBiCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanOut_tabBiCell())
                    return;

                if (wndMagCreate != null)
                    wndMagCreate = null;

                wndMagCreate = new ASSY003_014_MAG_CREATE();
                wndMagCreate.FrameOperation = FrameOperation;

                if (wndMagCreate != null)
                {
                    object[] Parameters = new object[13];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgBiCellList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgBiCellList, "CHK")].DataItem, "EQPTID"));
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgBiCellList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgBiCellList, "CHK")].DataItem, "EQPTNAME"));
                    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgBiCellList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgBiCellList, "CHK")].DataItem, "AREAID"));
                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgBiCellList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgBiCellList, "CHK")].DataItem, "PRODUCT_LEVEL3_NAME"));
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgBiCellList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgBiCellList, "CHK")].DataItem, "PRODID"));
                    Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgBiCellList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgBiCellList, "CHK")].DataItem, "PRJT_NAME"));
                    Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgBiCellList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgBiCellList, "CHK")].DataItem, "MKT_TYPE_CODE"));
                    Parameters[7] = Util.NVC(DataTableConverter.GetValue(dgBiCellList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgBiCellList, "CHK")].DataItem, "MKT_TYPE_NAME"));
                    Parameters[8] = Util.NVC(DataTableConverter.GetValue(dgBiCellList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgBiCellList, "CHK")].DataItem, "WIP_QTY"));
                    Parameters[9] = Util.NVC(txtUserName.Tag);
                    Parameters[10] = Util.NVC(txtUserName.Text);
                    Parameters[11] = Util.NVC(cboLine.SelectedValue);
                    Parameters[12] = Util.NVC(cboEquipment.SelectedValue);

                    C1WindowExtension.SetParameters(wndMagCreate, Parameters);

                    wndMagCreate.Closed += new EventHandler(wndMagCreate_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndMagCreate.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndMagCreate_Closed(object sender, EventArgs e)
        {
            wndRework = null;
            ASSY003_014_MAG_CREATE window = sender as ASSY003_014_MAG_CREATE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetBiCellList();
            }
        }

        private void dgBiCellList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
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

                                SetChkBoxControls(e, dgBiCellList);

                                break;
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgBicellChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                    DataRow dtRow = (rb.DataContext as DataRowView).Row;

                    for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                            DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                        else
                            DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                    }

                    //row 색 바꾸기
                    dgBiCellList.SelectedIndex = idx;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnIn_tabBiCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanIn_tabBiCell())
                    return;

                if (wndBicellIn != null)
                    wndBicellIn = null;

                wndBicellIn = new ASSY003_014_BICELL_IN();
                wndBicellIn.FrameOperation = FrameOperation;

                if (wndBicellIn != null)
                {
                    object[] Parameters = new object[5];
                    Parameters[0] = Util.NVC(cboLine.SelectedValue);
                    Parameters[1] = Util.NVC(cboLine.Text);
                    Parameters[2] = Util.NVC(cboEquipment.SelectedValue);
                    Parameters[3] = Util.NVC(txtUserName.Text);
                    Parameters[4] = Util.NVC(txtUserName.Tag);
                    
                    C1WindowExtension.SetParameters(wndBicellIn, Parameters);

                    wndBicellIn.Closed += new EventHandler(wndBicellIn_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndBicellIn.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndBicellIn_Closed(object sender, EventArgs e)
        {
            wndChgQty = null;
            ASSY003_014_BICELL_IN window = sender as ASSY003_014_BICELL_IN;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }
        #endregion

        #region Biz
        private void GetBiCellList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PRJT_NAME", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("PRODUCT_LEVEL3_CODE", typeof(string));
                inDataTable.Columns.Add("MKT_TYPE_CODE", typeof(string));
                DataRow newRow = inDataTable.NewRow();

                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["PRODUCT_LEVEL3_CODE"] = Util.NVC(cboBiCell_tabBiCell.SelectedValue).Equals("") ? null : Util.NVC(cboBiCell_tabBiCell.SelectedValue);
                newRow["PRODID"] = Util.NVC(txtProdId_tabBiCell.Text).Equals("") ? null : txtProdId_tabBiCell.Text;
                newRow["PRJT_NAME"] = Util.NVC(txtPjtName_tabBiCell.Text).Equals("") ? null : txtPjtName_tabBiCell.Text;
                newRow["MKT_TYPE_CODE"] = Util.NVC(cboMarket_tabBiCell.SelectedValue).Equals("") ? null : Util.NVC(cboMarket_tabBiCell.SelectedValue);

                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_CPROD_CELL_WIP", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgBiCellList, searchResult, FrameOperation, false);                        
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
        #endregion

        #region Validation
        private bool CanSearchBiCell(bool doPopUp = true)
        {
            bool bRet = false;

            //if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            //{
            //    //Util.Alert("작업장을 선택 하세요.");
            //    if (doPopUp)
            //        Util.MessageValidation("SFU4206");
            //    return bRet;
            //}

            if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                if (doPopUp)
                    Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                if (doPopUp)
                    Util.MessageValidation("SFU1673");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanOut_tabBiCell()
        {
            bool bRet = false;
            
            if (_Util.GetDataGridCheckFirstRowIndex(dgBiCellList, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
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

            //if (Convert.ToDouble(DataTableConverter.GetValue(dgBiCellList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgBiCellList, "CHK")].DataItem, "WIP_QTY")) < 1)
            //{
            //    Util.MessageValidation("SFU3371");  // 수량이 0이상 이어야 합니다.
            //    return bRet;
            //}

            if (Util.NVC(txtUserName.Text).Equals("") || Util.NVC(txtUserName.Tag).Equals(""))
            {
                Util.MessageValidation("SFU4011");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanIn_tabBiCell()
        {
            bool bRet = false;
            
            if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
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
            
            if (Util.NVC(txtUserName.Text).Equals("") || Util.NVC(txtUserName.Tag).Equals(""))
            {
                Util.MessageValidation("SFU4011");
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        #endregion
        #endregion

        #region [Folding Box 재공/출고]

        #region Event
        private void btnOut_tabFoldingBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanOutFoldingBox())
                    return;

                if (wndConfirm != null)
                    wndConfirm = null;

                wndConfirm = new ASSY003_014_CONFIRM();
                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {
                    object[] Parameters = new object[5];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[0].DataItem, "CPROD_RWK_LOT_EQSGID"));
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[0].DataItem, "CPROD_RWK_LOT_EQSGNAME"));
                    Parameters[2] = (from t in DataTableConverter.Convert(dgTransList_tabFoldingBox.ItemsSource).AsEnumerable()
                                     select t.Field<decimal>("WIPQTY")).Sum();
                    Parameters[3] = (from t in DataTableConverter.Convert(dgTransList_tabFoldingBox.ItemsSource).AsEnumerable()
                                     select t.Field<string>("LOTID")).ToList<string>();
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[0].DataItem, "CPROD_WRK_TYPE_CODE"));
                    //Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[0].DataItem, "CPROD_WRK_TYPE_CODE"));

                    C1WindowExtension.SetParameters(wndConfirm, Parameters);

                    wndConfirm.Closed += new EventHandler(wndConfirm_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            wndConfirm = null;
            ASSY003_014_CONFIRM window = sender as ASSY003_014_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {   
                OutFoldingBox(window.EQSGID);
            }
        }
        

        private void dgTransWaiting_tabFoldingBox_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();

                C1.WPF.DataGrid.DataGridRow selectedRow = e.Cell.Row;
                string selectedLotId = Util.NVC(DataTableConverter.GetValue(selectedRow.DataItem, "LOTID"));

                //UNCHECK -> CHECK
                if (Util.NVC_Int(e.Cell.Value) > 0)
                {
                    //if (!CanAddTransferList(selectedLotId))
                    //{
                    //    e.Cell.Value = 0;
                    //    return;
                    //}

                    #region TEMP DATA

                    DataTable dt = new DataTable("INDATA");
                    dt.Columns.Add("LOTID", typeof(string));
                    dt.Columns.Add("CSTID", typeof(string));
                    dt.Columns.Add("PRJT_NAME", typeof(string));
                    dt.Columns.Add("PRODID", typeof(string));
                    dt.Columns.Add("MKT_TYPE_CODE", typeof(string));
                    dt.Columns.Add("MKT_TYPE_NAME", typeof(string));
                    dt.Columns.Add("WIPQTY", typeof(decimal));
                    dt.Columns.Add("CPROD_RWK_LOT_EQSGID", typeof(string));
                    dt.Columns.Add("CPROD_RWK_LOT_EQPTID", typeof(string));
                    dt.Columns.Add("CPROD_WRK_TYPE_CODE", typeof(string));
                    dt.Columns.Add("CPROD_RWK_LOT_EQSGNAME", typeof(string));
                    dt.Columns.Add("CPROD_RWK_LOT_EQPTNAME", typeof(string));
                    dt.Columns.Add("LOTTYPE", typeof(string));
                    dt.Columns.Add("LOTYNAME", typeof(string));

                    DataRow dr = dt.NewRow();
                    dr["LOTID"] = selectedLotId;
                    dr["CSTID"] = Util.NVC(DataTableConverter.GetValue(selectedRow.DataItem, "CSTID"));
                    dr["PRJT_NAME"] = Util.NVC(DataTableConverter.GetValue(selectedRow.DataItem, "PRJT_NAME"));
                    dr["PRODID"] = Util.NVC(DataTableConverter.GetValue(selectedRow.DataItem, "PRODID"));
                    dr["MKT_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(selectedRow.DataItem, "MKT_TYPE_CODE"));
                    dr["MKT_TYPE_NAME"] = Util.NVC(DataTableConverter.GetValue(selectedRow.DataItem, "MKT_TYPE_NAME"));
                    dr["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(selectedRow.DataItem, "WIPQTY")).Equals("") ? 0 : decimal.Parse(Util.NVC(DataTableConverter.GetValue(selectedRow.DataItem, "WIPQTY")));
                    dr["CPROD_RWK_LOT_EQSGID"] = Util.NVC(DataTableConverter.GetValue(selectedRow.DataItem, "CPROD_RWK_LOT_EQSGID"));
                    dr["CPROD_RWK_LOT_EQPTID"] = Util.NVC(DataTableConverter.GetValue(selectedRow.DataItem, "CPROD_RWK_LOT_EQPTID"));
                    dr["CPROD_WRK_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(selectedRow.DataItem, "CPROD_WRK_TYPE_CODE"));
                    dr["CPROD_RWK_LOT_EQSGNAME"] = Util.NVC(DataTableConverter.GetValue(selectedRow.DataItem, "CPROD_RWK_LOT_EQSGNAME"));
                    dr["CPROD_RWK_LOT_EQPTNAME"] = Util.NVC(DataTableConverter.GetValue(selectedRow.DataItem, "CPROD_RWK_LOT_EQPTNAME"));
                    dr["LOTTYPE"] = Util.NVC(DataTableConverter.GetValue(selectedRow.DataItem, "LOTTYPE"));
                    dr["LOTYNAME"] = Util.NVC(DataTableConverter.GetValue(selectedRow.DataItem, "LOTYNAME"));

                    dt.Rows.Add(dr);

                    if (dgTransList_tabFoldingBox.GetRowCount() == 0)
                    {
                        dgTransList_tabFoldingBox.ItemsSource = DataTableConverter.Convert(dt);
                    }
                    else
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgTransList_tabFoldingBox.ItemsSource);
                        dtSource.Merge(dt);

                        Util.gridClear(dgTransList_tabFoldingBox);
                        dgTransList_tabFoldingBox.ItemsSource = DataTableConverter.Convert(dtSource);
                    }

                    #endregion

                    //new ClientProxy().ExecuteService("DA_PRD_SEL_MOVED_BOX_LOT_LIST", "INDATA", "OUTDATA", dt, (searchResult, searchException) =>
                    //{
                    //    try
                    //    {
                    //        if (searchException != null)
                    //        {
                    //            Util.MessageException(searchException);
                    //            return;
                    //        }

                    //        if (searchResult.Rows.Count > 0 && CanAddTransferList(Util.NVC(searchResult.Rows[0]["LOTID"])))
                    //        {
                    //            if (dgdTransList_tabBoxOut.GetRowCount() == 0)
                    //            {
                    //                dgdTransList_tabBoxOut.ItemsSource = DataTableConverter.Convert(searchResult);
                    //            }
                    //            else
                    //            {
                    //                DataTable dtSource = DataTableConverter.Convert(dgdTransList_tabBoxOut.ItemsSource);
                    //                dtSource.Merge(searchResult);

                    //                Util.gridClear(dgdTransList_tabBoxOut);
                    //                dgdTransList_tabBoxOut.ItemsSource = DataTableConverter.Convert(dtSource);
                    //            }
                    //        }
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        Util.MessageException(ex);
                    //    }
                    //    finally
                    //    {
                    //        HiddenLoadingIndicator();
                    //    }
                    //});
                }

                //CHECK -> UNCHECK
                else
                {
                    DataTable dtSource = DataTableConverter.Convert(dgTransList_tabFoldingBox.ItemsSource);

                    for (int i = 0; i < dtSource.Rows.Count; i++)
                    {
                        if (dtSource.Rows[i]["LOTID"].Equals(selectedLotId))
                        {
                            dtSource.Rows.RemoveAt(i);
                            break;
                        }
                    }

                    for (int i = 0; i < dgTransList_tabFoldingBox.Rows.Count; i++)
                    {
                        DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[i].DataItem, "LOTID");
                    }

                    Util.gridClear(dgTransList_tabFoldingBox);
                    dgTransList_tabFoldingBox.ItemsSource = DataTableConverter.Convert(dtSource);
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
        }

        private void btnSearch_tabFoldingBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanSearchFoldingBox()) return;

                GetFoldingBoxList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDelete_tabFoldingBox_Click(object sender, RoutedEventArgs e)
        {
            dgTransList_tabFoldingBox.IsReadOnly = true;

            int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
            string lotId = Util.NVC(DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[index].DataItem, "LOTID"));
            DataTable dt = DataTableConverter.Convert(dgTransList_tabFoldingBox.ItemsSource);
            dt.Rows.RemoveAt(index);

            dgTransList_tabFoldingBox.ItemsSource = DataTableConverter.Convert(dt);

            foreach (C1.WPF.DataGrid.DataGridRow boxListRow in dgTransList_tabFoldingBox.Rows)
            {
                if (Util.NVC(DataTableConverter.GetValue(boxListRow.DataItem, "LOTID")).Equals(lotId))
                {
                    DataTableConverter.SetValue(boxListRow.DataItem, "CHK", 0);

                    break;
                }
            }

            dgTransList_tabFoldingBox.IsReadOnly = false;
        }

        private void repCProdOut_Closed(object sender, EventArgs e)
        {
            Report_CProd_Out window = sender as Report_CProd_Out;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }
        #endregion

        #region Biz
        private void GetFoldingBoxList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("CPROD_WRK_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("CPROD_RWK_LOT_EQSGID", typeof(string)); 
                inDataTable.Columns.Add("LOTID", typeof(string));
                DataRow newRow = inDataTable.NewRow();

                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.CPROD;
                newRow["EQSGID"] = Util.NVC(cboLine.SelectedValue);
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["CPROD_WRK_TYPE_CODE"] = Util.NVC(cboRecyc_tabFoldingBox.SelectedValue).Equals("") ? null : Util.NVC(cboRecyc_tabFoldingBox.SelectedValue);
                newRow["CPROD_RWK_LOT_EQSGID"] = Util.NVC(cboLine_tabFoldingBox.SelectedValue).Equals("") ? null : cboLine_tabFoldingBox.SelectedValue;
                newRow["LOTID"] = Util.NVC(tbxBoxId_tabFoldingBox.Text).Equals("") ? null : tbxBoxId_tabFoldingBox.Text;

                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_CPROD_BOX_WIP", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.gridClear(dgTransWaiting_tabFoldingBox);
                        Util.gridClear(dgTransList_tabFoldingBox);

                        Util.GridSetData(dgTransWaiting_tabFoldingBox, searchResult, FrameOperation, false);
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

        private bool IsLotStateWait(object lotid)
        {
            DataTable dt = new DataTable("INDATA");
            dt.Columns.Add("LOTID");

            DataRow dr = dt.NewRow();
            dr["LOTID"] = lotid.ToString();
            dt.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync("COR_SEL_WIP", "INDATA", "OUTDATA", dt);

            return searchResult.Rows[0]["WIPSTAT"].Equals("WAIT");
        }

        private void OutFoldingBox(string sEqsgID)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("TO_EQSGID", typeof(string));
                inDataTable.Columns.Add("TO_PROCID", typeof(string));
                inDataTable.Columns.Add("MOVE_USERID", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));


                DataTable inLot = indataSet.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("WIPQTY", typeof(decimal));
                inLot.Columns.Add("CSTID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["PROCID"] = Process.CPROD;
                newRow["TO_EQSGID"] = sEqsgID;// Util.NVC(DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[0].DataItem, "CPROD_RWK_LOT_EQSGID"));
                newRow["TO_PROCID"] = Process.PACKAGING; //Util.NVC(DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[0].DataItem, "CPROD_WRK_TYPE_CODE")).Equals("F") ? Process.STACKING_FOLDING : Process.PACKAGING;
                newRow["MOVE_USERID"] = txtUserName.Tag;
                newRow["NOTE"] = "";
                newRow["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(newRow);
                newRow = null;

                for (int i = 0; i < dgTransList_tabFoldingBox.Rows.Count; i++)
                {
                    newRow = inLot.NewRow();

                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[i].DataItem, "LOTID"));
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[i].DataItem, "WIPQTY")));
                    newRow["CSTID"] = "";

                    inLot.Rows.Add(newRow);
                }                

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SEND_CPROD_OUT_LOT", "INDATA,IN_LOT", "OUTDATA", (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult != null && searchResult.Tables.Contains("OUTDATA") && searchResult.Tables["OUTDATA"].Rows.Count > 0)
                            PrintCProdOutCard(Util.NVC(searchResult.Tables["OUTDATA"].Rows[0]["MOVE_ORD_ID"]));
                        else
                            Util.MessageInfo("SFU2930");    // 인계처리 되었습니다. 이력카드는 발행되지 않았습니다.


                        GetFoldingBoxList();
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                        Util.gridClear(dgTransList_tabFoldingBox);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void PrintCProdOutCard(string sMoveID)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable indataTable = ds.Tables.Add("IN_DATA");
                indataTable.Columns.Add("LANGID", typeof(string));
                indataTable.Columns.Add("MOVE_ORD_ID", typeof(string));
                
                DataRow indata = indataTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["MOVE_ORD_ID"] = sMoveID;
                indataTable.Rows.Add(indata);
                //ds.Tables.Add(indataTable);

                new ClientProxy().ExecuteService_Multi("BR_PRD_SEL_CPROD_OUTBOX_CARD", "IN_DATA", "OUT_HEADER,OUT_DATA", (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        // 출력 처리
                        Report_CProd_Out repCProdOut = new Report_CProd_Out();
                        repCProdOut.FrameOperation = FrameOperation;
                        object[] parameters = new object[3];
                        parameters[0] = "CProd_OutBoxList";
                        parameters[1] = result;
                        //parameters[1] = DataTableConverter.Convert(dgTransList_tabFoldingBox.ItemsSource);  // datatable
                        //parameters[2] = new string[] {Util.NVC(searchResult.Tables[0].Rows[0][""]),                                                             // 이동ID
                        //                              Util.NVC(cboLine.Text),                                                                                   // 인계작업장
                        //                              Util.NVC(DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[0].DataItem, "CPROD_WRK_TYPE_NAME")), // 재작업유형
                        //                              Util.NVC(cboLine_tabFoldingBox.Text),                                                                     // 인수라인
                        //                              ((from t in DataTableConverter.Convert(dgTransList_tabFoldingBox.ItemsSource).AsEnumerable()
                        //                               select t.Field<decimal>("WIPQTY")).Sum()).ToString() ,                                                   // 인계수량
                        //                              txtUserName.Text,                                                                                         // 인계자
                        //                              System.DateTime.Now.ToString()                                                                            // 인계일시
                        //                             }; // Header 정보

                        C1WindowExtension.SetParameters(repCProdOut, parameters);
                        repCProdOut.Closed += new EventHandler(repCProdOut_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => repCProdOut.ShowModal()));
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, ds);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Validation
        private bool CanSearchFoldingBox(bool doPopUp = true)
        {
            bool bRet = false;

            //if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            //{
            //    //Util.Alert("작업장을 선택 하세요.");
            //    if (doPopUp)
            //        Util.MessageValidation("SFU4206");
            //    return bRet;
            //}

            if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                if (doPopUp)
                    Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                if (doPopUp)
                    Util.MessageValidation("SFU1673");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanAddTransferList(string lotId)
        {
            bool bRet = false;

            foreach (C1.WPF.DataGrid.DataGridRow row in dgTransList_tabFoldingBox.Rows)
            {
                if (lotId.Equals(DataTableConverter.GetValue(row.DataItem, "LOTID")))
                {
                    //LOT이 이미 선택되었습니다.
                    Util.MessageValidation("SFU2840");
                    return bRet;
                }
            }

            if (!IsLotStateWait(lotId))
            {
                //LOT [%1]은 인계할수 없는 상태입니다.
                Util.MessageValidation("SFU4295", lotId);
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanOutFoldingBox()
        {
            bool bRet = false;

            if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
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

            if (cboLine_tabFoldingBox.SelectedIndex < 0 || cboLine_tabFoldingBox.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU4283");  // 인계 라인을 선택하세요.
                return bRet;
            }
            
            if (dgTransList_tabFoldingBox.Rows.Count < 1)
            {
                Util.MessageValidation("SFU4294");  // 인계할 LOT이 없습니다.
                return bRet;
            }

            if (Util.NVC(txtUserName.Text).Equals("") || Util.NVC(txtUserName.Tag).Equals(""))
            {
                Util.MessageValidation("SFU4011");
                return bRet;
            }


            //동일 인계 라인 선택 여부 체크 필요.        
            string sWrkTypeCode = "";                        
            for (int i = 0; i < dgTransList_tabFoldingBox.Rows.Count; i++)
            {
                if (i > 0)
                {
                    //if (!sWrkTypeCode.Equals("F"))   // 폴딩 공정인 경우이에는 출고 라인 선택 처리.
                    //{
                    //    if (!DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[i].DataItem, "CPROD_RWK_LOT_EQSGID").Equals(DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[i - 1].DataItem, "CPROD_RWK_LOT_EQSGID")))
                    //    {
                    //        Util.MessageValidation("SFU4401");  // 동일한 인계라인만 출고 가능합니다.
                    //        return bRet;
                    //    }
                    //}
                    

                    if (!DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[i].DataItem, "PRODID").Equals(DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[i - 1].DataItem, "PRODID")))
                    {
                        Util.MessageValidation("SFU4402");  // 동일한 제품만 출고 가능합니다.
                        return bRet;
                    }

                    if (!DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[i].DataItem, "MKT_TYPE_CODE").Equals(DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[i - 1].DataItem, "MKT_TYPE_CODE")))
                    {
                        Util.MessageValidation("SFU4403");  // 동일한 시장유형만 출고 가능합니다.
                        return bRet;
                    }

                    if (!DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[i].DataItem, "CPROD_WRK_TYPE_CODE").Equals(DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[i - 1].DataItem, "CPROD_WRK_TYPE_CODE")))
                    {
                        Util.MessageValidation("SFU4409");  // 동일한 재작업구분만 출고 가능합니다.
                        return bRet;
                    }
                }
                else
                {
                    sWrkTypeCode = Util.NVC(DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[i].DataItem, "CPROD_WRK_TYPE_CODE"));
                }
            }


            bRet = true;
            return bRet;
        }
        #endregion

        #endregion

        #region [입고 이력 조회]

        #region Event
        private void btnSearch_tabInHist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanSearchInHist()) return;

                GetCprodInHist();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }                     
        }
        #endregion

        #region Biz
        private void GetCprodInHist()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("FRDT", typeof(string));
                inDataTable.Columns.Add("TODT", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("CPROD_WRK_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("SEND_EQSGID", typeof(string));
                inDataTable.Columns.Add("SEND_EQTPID", typeof(string));
                DataRow newRow = inDataTable.NewRow();

                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FRDT"] = dtpFrom_tabInHist.SelectedDateTime.ToString("yyyyMMdd");
                newRow["TODT"] = dtpTo_tabInHist.SelectedDateTime.ToString("yyyyMMdd");
                newRow["EQSGID"] = Util.NVC(cboLine.SelectedValue);
                newRow["CPROD_WRK_TYPE_CODE"] = Util.NVC(cboRecyc_tabInHist.SelectedValue).Equals("") ? null : Util.NVC(cboRecyc_tabInHist.SelectedValue);
                newRow["SEND_EQSGID"] = Util.NVC(cboLine_tabInHist.SelectedValue).Equals("") ? null : Util.NVC(cboLine_tabInHist.SelectedValue);
                newRow["SEND_EQTPID"] = Util.NVC(cboEquipment_tabInHist.SelectedValue).Equals("") ? null : Util.NVC(cboEquipment_tabInHist.SelectedValue);

                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_CPROD_RCPT_HIST", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgLotList_tabInHist, searchResult, FrameOperation, false);
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
        #endregion

        #region Validation
        private bool CanSearchInHist()
        {
            bool bRet = false;

            if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
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

            bRet = true;
            return bRet;
        }
        #endregion
        #endregion

        #region [재작업 실적 조회]

        #region Event
        private void btnSearch_tabWorkHist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanSearchInReWorkHist()) return;

                //ClearValue();

                GetCprodReWorkHist();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }            
        }

        private void txtProdid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!CanSearchInReWorkHist()) return;

                if (txtProdid.Text != "")
                {
                    GetCprodReWorkHist();
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void txtPjt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!CanSearchInReWorkHist()) return;

                if (txtPjt.Text != "")
                {
                    GetCprodReWorkHist();
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                dgLotList_tabWorkHist.Selection.Clear();

                RadioButton rb = sender as RadioButton;

                //최초 체크시에만 로직 타도록 구현
                if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
                {
                    //체크시 처리될 로직
                    string sLotId = DataTableConverter.GetValue(rb.DataContext, "LOTID").ToString();
                    string sProdId = DataTableConverter.GetValue(rb.DataContext, "PRODID").ToString();
                    string sWipSeq = DataTableConverter.GetValue(rb.DataContext, "WIPSEQ").ToString();
                    
                    foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                    {
                        DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                    }

                    DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                    //row 색 바꾸기
                    ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                    ClearValue();
                    SetValue(rb.DataContext);

                    GetOutBox(sLotId);
                    GetCellResult(sLotId, sProdId);
                    GetDfctResult(sLotId, sWipSeq);
                    GetInputHist(sLotId, sWipSeq);
                    //GetDefectInfo();
                    //GetInputHistory();
                    //GetQuality();
                    //GetColor();
                    //GetInputMaterial();
                    //GetEqpFaultyData();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Biz
        private void GetCprodReWorkHist()
        {
            try
            {
                if ((dtpTo_tabWorkHist.SelectedDateTime - dtpFrom_tabWorkHist.SelectedDateTime).TotalDays > 31)
                {
                    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                ShowLoadingIndicator();
                DoEvents();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("FRDT", typeof(string));
                inDataTable.Columns.Add("TODT", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("CPROD_WRK_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("FROM_EQSGID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("PRJT_NAME", typeof(string));
                DataRow newRow = inDataTable.NewRow();

                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FRDT"] = dtpFrom_tabWorkHist.SelectedDateTime.ToString("yyyyMMdd");
                newRow["TODT"] = dtpTo_tabWorkHist.SelectedDateTime.ToString("yyyyMMdd");
                newRow["EQSGID"] = Util.NVC(cboLine.SelectedValue);
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["CPROD_WRK_TYPE_CODE"] = Util.NVC(cboRecyc_tabWorkHist.SelectedValue).Equals("") ? null : Util.NVC(cboRecyc_tabWorkHist.SelectedValue);
                newRow["FROM_EQSGID"] = Util.NVC(cboEquipment_tabWorkHist.SelectedValue).Equals("") ? null : Util.NVC(cboEquipment_tabWorkHist.SelectedValue);
                newRow["PRODID"] = txtProdid.Text == "" ? null : txtProdid.Text;
                newRow["PRJT_NAME"] = txtPjt.Text == "" ? null : txtPjt.Text;

                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_CPROD_RWK_HIST", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgLotList_tabWorkHist, searchResult, FrameOperation, true);
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

        private void GetOutBox(string sLotId)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("PR_LOTID");
                dt.Columns.Add("LANGID");
                dt.Columns.Add("PROCID");
                dt.Columns.Add("AREAID");

                DataRow dr = dt.NewRow();
                dr["PR_LOTID"] = sLotId;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Process.CPROD;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dt.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_CPROD_OUT_LOT_LIST_HIST", "INDATA", "OUTDATA", dt, (result, exception) =>
                {
                    try
                    {
                        ShowLoadingIndicator();

                        Util.GridSetData(dgBoxLotResult_tabWorkHist, result, FrameOperation, false);
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
            }
        }

        private void GetCellResult(string sLotId, string sProdid)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("PROD_LOTID");
                dt.Columns.Add("PRODID");
                //dt.Columns.Add("PRODUCT_LEVEL2_CODE");

                DataRow dr = dt.NewRow();
                dr["PROD_LOTID"] = sLotId;
                dr["PRODID"] = sProdid;
                //dr["PRODUCT_LEVEL2_CODE"] = "FC,ST";  //구분자 ','
                dt.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_BAS_SEL_CPROD_CELL_RSLT", "INDATA", "OUTDATA", dt, (result, exception) =>
                {
                    try
                    {
                        ShowLoadingIndicator();

                        Util.GridSetData(dgCellResult_tabWorkHist, result, FrameOperation, false);
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
            }
        }

        private void GetDfctResult(string sLotId, string sWipSeq)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID");
                dt.Columns.Add("PROCID");
                dt.Columns.Add("LOTID");
                dt.Columns.Add("WIPSEQ");
                dt.Columns.Add("AREAID");
                dt.Columns.Add("EQPTID");
                dt.Columns.Add("ACTID");

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Process.CPROD;
                dr["LOTID"] = sLotId;
                dr["WIPSEQ"] = sWipSeq;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                dr["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                dt.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_QCA_SEL_WIPRESONCOLLECT", "INDATA", "OUTDATA", dt, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        Util.GridSetData(dgDefectResult_tabWorkHist, result, FrameOperation, false);
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

        private void GetInputHist(string sLotId, string sWipSeq)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("EQPTID");
                dt.Columns.Add("LOTID");

                DataRow dr = dt.NewRow();
                dr["EQPTID"] = cboEquipment.SelectedValue.ToString();
                dr["LOTID"] = sLotId;
                dt.Rows.Add(dr);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_INPUT_MTRL_HIST_CPROD", "INDATA", "OUTDATA", dt);

                Util.GridSetData(dgCurrIn_tabWorkHist, searchResult, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HideLoadingIndicator();
            }
        }
        #endregion

        #region Validation
        private bool CanSearchInReWorkHist(bool doPopUp = true)
        {
            bool bRet = false;

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("작업장을 선택 하세요.");
                if (doPopUp)
                    Util.MessageValidation("SFU4206");
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        #endregion

        #region Func
        private void ClearValue()
        {
            txtLot.Text = "";
            txtWrkType.Text = "";
            txtWrkDate.Text = "";
            txtShift.Text = "";
            txtInQty.Text = "0";
            txtWorker.Text = "";
            txtReworkQtyFC.Text = "0";
            txtWorkTime.Text = "";
            txtReworkQtyLC.Text = "0";
            txtEndDTTM.Text = "";
            txtInputQtyLC.Text = "0";
            txtRemark.Text = "";
            txtProdid.Text = "";
            txtPjt.Text = "";

            Util.gridClear(dgBoxLotResult_tabWorkHist);
            Util.gridClear(dgCellResult_tabWorkHist);
            Util.gridClear(dgDefectResult_tabWorkHist);
            Util.gridClear(dgCurrIn_tabWorkHist);
        }

        private void SetValue(object oContext)
        {

            txtLot.Text = Util.NVC(DataTableConverter.GetValue(oContext, "LOTID"));
            txtWrkType.Text = Util.NVC(DataTableConverter.GetValue(oContext, "CPROD_WRK_TYPE"));
            txtWrkDate.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WIPDTTM_ST"));
            txtShift.Text = Util.NVC(DataTableConverter.GetValue(oContext, "SHIFTNAME"));
            txtInQty.Text = Util.NVC(DataTableConverter.GetValue(oContext, "INPUT_QTY"));
            txtWorker.Text = Util.NVC(DataTableConverter.GetValue(oContext, "USERNAME_ED"));
            txtReworkQtyFC.Text = Util.NVC(DataTableConverter.GetValue(oContext, "FC_QTY"));
            txtWorkTime.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WIPDTTM_ST"));
            txtReworkQtyLC.Text = Util.NVC(DataTableConverter.GetValue(oContext, "LC_QTY"));
            txtEndDTTM.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WIPDTTM_ED"));
            txtInputQtyLC.Text = Util.NVC(DataTableConverter.GetValue(oContext, "LC_ADD_QTY"));
            
            //_AREAID = Util.NVC(DataTableConverter.GetValue(oContext, "AREAID"));
            //_PROCID = Util.NVC(DataTableConverter.GetValue(oContext, "PROCID"));
            //_EQSGID = Util.NVC(DataTableConverter.GetValue(oContext, "EQSGID"));
            //_EQPTID = Util.NVC(DataTableConverter.GetValue(oContext, "EQPTID"));
            //_LOTID = Util.NVC(DataTableConverter.GetValue(oContext, "LOTID"));
            //_WIPSEQ = Util.NVC(DataTableConverter.GetValue(oContext, "WIPSEQ"));
            //_LANEPTNQTY = Util.NVC(DataTableConverter.GetValue(oContext, "LANE_PTN_QTY"));
            //_WIP_NOTE = Util.NVC(DataTableConverter.GetValue(oContext, "WIP_NOTE"));

            txtRemark.Text = GetWipNote(Util.NVC(DataTableConverter.GetValue(oContext, "WIP_NOTE")));
        }

        private string GetWipNote(string _WIP_NOTE)
        {
            string sReturn;
            string[] sWipNote = _WIP_NOTE.Split('|');

            if (sWipNote.Length == 0)
            {
                sReturn = _WIP_NOTE;
            }
            else
            {
                sReturn = sWipNote[0];
            }
            return sReturn;
        }
        #endregion

        #endregion

        #region [출고 이력 조회]

        #region Event
        private void btnSearch_tabOutHist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CanTransferListSearch())
                {
                    TransferListSearch();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOutCancel_tabOutHist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanDelete())
                    return;

                Util.MessageConfirm("SFU4398", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DeleteCProdLot();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgLotList_tabOutHist_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
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

                                string sChk = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK"));


                                if (sChk == "1")
                                {
                                    SetChkBoxControls(e, dg);

                                    string sMoveID = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "MOVE_ORD_ID"));
                                    _Util.SetDataGridCheck(dg, "CHK", "MOVE_ORD_ID", sMoveID);
                                }
                                else
                                {
                                    Util.DataGridCheckAllUnChecked(dg);
                                }



                                //if (_Util.GetDataGridCheckCnt(dgLotList_tabOutHist, "CHK") == 1)
                                //{
                                //    SetChkBoxControls(e, dgLotList_tabOutHist);
                                //}
                                //else
                                //{
                                //    string sMoveID = Util.NVC(DataTableConverter.GetValue(dgLotList_tabOutHist.Rows[e.Cell.Row.Index].DataItem, "MOVE_ORD_ID"));
                                //    _Util.SetDataGridCheck(dgLotList_tabOutHist, "CHK", "MOVE_ORD_ID", sMoveID);
                                //}                                
                                break;
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgLotList_tabOutHist_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (dg == null)
                    return;

                int iStdx = 0;
                int iEndx = 0;
                string sTmpMoveOrd = string.Empty;
                for (int i = 0; i < dg.Rows.Count; i++)
                {
                    if (i == 0)
                    {
                        iStdx = i;
                        sTmpMoveOrd = Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "MOVE_ORD_ID"));

                        continue;
                    }

                    if (sTmpMoveOrd.Equals(Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "MOVE_ORD_ID"))))
                    {
                        iEndx = i;
                    }
                    else
                    {
                        if (iStdx < iEndx)
                            e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["MOVE_ORD_QTY"].Index), dg.GetCell(iEndx, dg.Columns["MOVE_ORD_QTY"].Index)));

                        iStdx = i;
                        sTmpMoveOrd = Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "MOVE_ORD_ID"));
                    }
                }

                if (iStdx < iEndx)
                    e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["MOVE_ORD_QTY"].Index), dg.GetCell(iEndx, dg.Columns["MOVE_ORD_QTY"].Index)));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Biz
        private void TransferListSearch()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("FRDT", typeof(string));
                inDataTable.Columns.Add("TODT", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("MOVE_ORD_STAT_CODE", typeof(string)); 
                inDataTable.Columns.Add("CPROD_WRK_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("TO_PROCID", typeof(string));
                inDataTable.Columns.Add("TO_EQSGID", typeof(string)); 
                DataRow newRow = inDataTable.NewRow();

                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FRDT"] = dtpFrom_tabOutHist.SelectedDateTime.ToString("yyyyMMdd");
                newRow["TODT"] = dtpTo_tabOutHist.SelectedDateTime.ToString("yyyyMMdd");
                newRow["EQSGID"] = Util.NVC(cboLine.SelectedValue);
                //newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);  // 설비 코드 안들어가고 있음.
                newRow["MOVE_ORD_STAT_CODE"] = Util.NVC(cboState_tabOutHist.SelectedValue).Equals("") ? null : Util.NVC(cboState_tabOutHist.SelectedValue);
                newRow["CPROD_WRK_TYPE_CODE"] = Util.NVC(cboRecyc_tabOutHist.SelectedValue).Equals("") ? null : Util.NVC(cboRecyc_tabOutHist.SelectedValue);
                
                newRow["TO_PROCID"] = Util.NVC(cboProcess_tabOutHist.SelectedValue).Equals("") ? null : Util.NVC(cboProcess_tabOutHist.SelectedValue);
                newRow["TO_EQSGID"] = Util.NVC(cboLine_tabOutHist.SelectedValue).Equals("") ? null : Util.NVC(cboLine_tabOutHist.SelectedValue);

                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_CPROD_SHIP_HIST", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgLotList_tabOutHist, searchResult, FrameOperation, false);

                        dgLotList_tabOutHist.MergingCells -= dgLotList_tabOutHist_MergingCells;
                        dgLotList_tabOutHist.MergingCells += dgLotList_tabOutHist_MergingCells;
                        
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
            }
        }

        private void DeleteCProdLot()
        {
            try
            {
                ShowLoadingIndicator();

                int idx = _Util.GetDataGridCheckFirstRowIndex(dgLotList_tabOutHist, "CHK");
                DataSet ds = new DataSet();

                DataTable dt = ds.Tables.Add("INDATA");
                dt.Columns.Add("SRCTYPE", typeof(string));
                dt.Columns.Add("IFMODE", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("MOVE_ORD_ID", typeof(string));
                dt.Columns.Add("NOTE", typeof(string));
                dt.Columns.Add("USERID", typeof(string));

                DataTable dtLot = ds.Tables.Add("IN_LOT");
                dtLot.Columns.Add("LOTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                dr["MOVE_ORD_ID"] = Util.NVC(DataTableConverter.GetValue(dgLotList_tabOutHist.Rows[idx].DataItem, "MOVE_ORD_ID"));
                dr["NOTE"] = "";
                dr["USERID"] = LoginInfo.USERID;

                dt.Rows.Add(dr);

                for (int i = 0; i < dgLotList_tabOutHist.Rows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgLotList_tabOutHist, "CHK", i)) continue;

                    dr = dtLot.NewRow();
                    dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotList_tabOutHist.Rows[i].DataItem, "LOTID"));

                    dtLot.Rows.Add(dr);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_SEND_CPROD_LOT", "INDATA,IN_LOT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageValidation("SFU1937");

                        TransferListSearch();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                },
                ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HideLoadingIndicator();
            }
        }
        #endregion

        #region Validation
        private bool CanTransferListSearch()
        {
            bool bRet = false;

            if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
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

            bRet = true;
            return bRet;
        }


        private bool CanDelete()
        {
            bool bRet = false;

            int index = _Util.GetDataGridCheckFirstRowIndex(dgLotList_tabOutHist, "CHK");

            if (index < 0)
            {
                //선택된 LOT이 없습니다.
                Util.MessageValidation("SFU3529");
                return bRet;
            }

            if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
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
            
            bRet = true;
            return bRet;
        }


        #endregion

        #endregion

        #region 발행
        #region A4 발행
        private void btnPrint_tabOutHist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgLotList_tabOutHist, "CHK");
                if (iRow < 0)
                    return;

                DataSet ds = new DataSet();
                DataTable indataTable = ds.Tables.Add("IN_DATA");
                indataTable.Columns.Add("LANGID", typeof(string));
                indataTable.Columns.Add("MOVE_ORD_ID", typeof(string));

                DataRow indata = indataTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["MOVE_ORD_ID"] = Util.NVC(DataTableConverter.GetValue(dgLotList_tabOutHist.Rows[_Util.GetDataGridCheckFirstRowIndex(dgLotList_tabOutHist, "CHK")].DataItem, "MOVE_ORD_ID"));
                indataTable.Rows.Add(indata);
                //ds.Tables.Add(indataTable);

                new ClientProxy().ExecuteService_Multi("BR_PRD_SEL_CPROD_OUTBOX_CARD", "IN_DATA", "OUT_HEADER,OUT_DATA", (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        // 출력 처리
                        Report_CProd_Out repCProdOut = new Report_CProd_Out();
                        repCProdOut.FrameOperation = FrameOperation;
                        object[] parameters = new object[3];
                        parameters[0] = "CProd_OutBoxList";
                        parameters[1] = result;
                        //parameters[1] = DataTableConverter.Convert(dgTransList_tabFoldingBox.ItemsSource);  // datatable
                        //parameters[2] = new string[] {Util.NVC(searchResult.Tables[0].Rows[0][""]),                                                             // 이동ID
                        //                              Util.NVC(cboLine.Text),                                                                                   // 인계작업장
                        //                              Util.NVC(DataTableConverter.GetValue(dgTransList_tabFoldingBox.Rows[0].DataItem, "CPROD_WRK_TYPE_NAME")), // 재작업유형
                        //                              Util.NVC(cboLine_tabFoldingBox.Text),                                                                     // 인수라인
                        //                              ((from t in DataTableConverter.Convert(dgTransList_tabFoldingBox.ItemsSource).AsEnumerable()
                        //                               select t.Field<decimal>("WIPQTY")).Sum()).ToString() ,                                                   // 인계수량
                        //                              txtUserName.Text,                                                                                         // 인계자
                        //                              System.DateTime.Now.ToString()                                                                            // 인계일시
                        //                             }; // Header 정보

                        C1WindowExtension.SetParameters(repCProdOut, parameters);
                        repCProdOut.Closed += new EventHandler(repCProdOut_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => repCProdOut.ShowModal()));
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 감열지 발행
        private void btnPrint_Label_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgLotList_tabOutHist, "CHK");
                if (iRow < 0)
                    return;

                string sLine = string.Empty;
                
                // 매거진 감열지 발행.
                int iCopys = 1;

                if (LoginInfo.CFG_THERMAL_COPIES > 0)
                {
                    iCopys = LoginInfo.CFG_THERMAL_COPIES;
                }

                List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();


                for (int i = 0; i < dgLotList_tabOutHist.Rows.Count - dgLotList_tabOutHist.TopRows.Count - dgLotList_tabOutHist.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgLotList_tabOutHist, "CHK", i)) continue;

                    DataTable dtRslt = GetThermalPaperPrintingInfo(Util.NVC(DataTableConverter.GetValue(dgLotList_tabOutHist.Rows[i].DataItem, "LOTID")));

                    sLine = Util.NVC(DataTableConverter.GetValue(dgLotList_tabOutHist.Rows[i].DataItem, "TO_EQSGID"));

                    if (dtRslt == null || dtRslt.Rows.Count < 1) continue;

                    Dictionary<string, string> dicParam = new Dictionary<string, string>();

                    //라미
                    dicParam.Add("reportName", "Lami"); //dicParam.Add("reportName", "Fold");
                    dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                    dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                    dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("CASTID", Util.NVC(dtRslt.Rows[0]["CSTID"])); // 카세트 ID 컬럼은??
                    dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                    dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                    dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                    dicParam.Add("CELLTYPE", Util.NVC(dtRslt.Rows[0]["PRODUCT_LEVEL3_CODE"]));
                    dicParam.Add("TITLEX", "MAGAZINE ID");

                    dicParam.Add("PRINTQTY", iCopys.ToString());  // 발행 수

                    dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

                    dicParam.Add("RE_PRT_YN", "N"); // 재발행 여부.

                    if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
                    {
                        //dicParam.Add("MKT_TYPE_CODE", Util.NVC(DataTableConverter.GetValue(winWorkOrder.dgWorkOrder.Rows[_Util.GetDataGridCheckFirstRowIndex(winWorkOrder.dgWorkOrder, "CHK")].DataItem, "MKT_TYPE_CODE")));
                    }

                    dicList.Add(dicParam);
                }

                CMM_THERMAL_PRINT_LAMI print = new CMM_THERMAL_PRINT_LAMI();
                print.FrameOperation = FrameOperation;

                if (print != null)
                {
                    object[] Parameters = new object[7];
                    Parameters[0] = dicList;
                    Parameters[1] = Process.LAMINATION;
                    Parameters[2] = sLine;
                    Parameters[3] = "";// cboEquipment.SelectedValue.ToString();
                    Parameters[4] = "N";   // 완료 메시지 표시 여부.
                    Parameters[5] = "N";   // 디스패치 처리.
                    Parameters[6] = "MAGAZINE";   // 발행 Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)

                    C1WindowExtension.SetParameters(print, Parameters);

                    print.Closed += new EventHandler(print_Closed);

                    print.Show();
                }                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable GetThermalPaperPrintingInfo(string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_THERMAL_PAPER_PRT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = sLotID;

                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO_CPRD_LAMI", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void print_Closed(object sender, EventArgs e)
        {
            CMM_THERMAL_PRINT_LAMI window = sender as CMM_THERMAL_PRINT_LAMI;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }
        #endregion

        #endregion
    }
}