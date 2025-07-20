/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.22  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_202 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        DataTable _dtPallet = new DataTable();

        // 프린트 설정용
        string _sPrt = string.Empty;
        string _sRes = string.Empty;
        string _sCopy = string.Empty;
        string _sXpos = string.Empty;
        string _sYpos = string.Empty;
        string _sDark = string.Empty;

        DataRow _drPrtInfo = null;
        /*컨트롤 변수 선언*/
        public UCBoxShift ucBoxShift { get; set; }
        public TextBox txtWorker_Main { get; set; }
        public TextBox txtShift_Main { get; set; }

        private static string CREATED = "CREATED,";
        private static string PACKING = "PACKING,";
        private static string PACKED = "PACKED,";
        private static string SHIPPING = "SHIPPING,";
        private string _searchStat = string.Empty;
        //private string _rcvStat = string.Empty;
        private bool bInit = true;

        string _sPGM_ID = "BOX001_202";

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

        public BOX001_202()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
            bInit = false;
        }

        /// <summary>
        /// 메뉴 권한 설정
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            ////listAuth.Add(btnOutAdd);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 콤보박스 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID, "MCC,MCR,MCS", Process.CELL_BOXING, LoginInfo.CFG_AREA_ID}, sCase: "LINEBYSHOP");
            //_combo.SetCombo(cboLine, CommonCombo.ComboStatus.ALL, sFilter: new string[] { LoginInfo.CFG_AREA_ID, "B" }, sCase: "PROCESSSEGMENTLINE");
            //_combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, null, sFilter: new string[] { LoginInfo.CFG_AREA_ID }, sCase: "LINE_FCS");
            // _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, null, sFilter: new string[] { Process.CELL_BOXING }, sCase: "EQUIPMENT");


        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            cboEquipment.SelectedValue = "SELECT";
            txtBoxQty.Value = 100;
            ucBoxShift = grdShift.Children[0] as UCBoxShift;
            txtWorker_Main = ucBoxShift.TextWorker;
            txtShift_Main = ucBoxShift.TextShift;
            ucBoxShift.ProcessCode = Process.CELL_BOXING; //작업조 팝업에 넘길 공정
            ucBoxShift.FrameOperation = this.FrameOperation;
        }

        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
        }

        #endregion

        #region Events
        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void chkSearch_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            switch (chk.Name)
            {
                case "chkCreated":
                    _searchStat += CREATED;
                    break;
                case "chkPacking":
                    _searchStat += PACKING;
                    break;
                case "chkPacked":
                    _searchStat += PACKED;
                    break;
                case "chkShipping":
                    _searchStat += SHIPPING;
                    break;
                default:
                    break;
            }
            if (!bInit)
                btnSearch_Click(null, null);
            // bInit = false;
        }
        private void chkSearch_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            switch (chk.Name)
            {
                case "chkCreated":
                    if (_searchStat.Contains(CREATED))
                        _searchStat = _searchStat.Replace(CREATED, "");
                    break;
                case "chkPacking":
                    if (_searchStat.Contains(PACKING))
                        _searchStat = _searchStat.Replace(PACKING, "");
                    break;
                case "chkPacked":
                    if (_searchStat.Contains(PACKED))
                        _searchStat = _searchStat.Replace(PACKED, "");
                    break;
                case "chkShipping":
                    if (_searchStat.Contains(SHIPPING))
                        _searchStat = _searchStat.Replace(SHIPPING, "");
                    break;
                default:
                    break;
            }
            if (!bInit)
                btnSearch_Click(null, null);
            //  bInit = false;
        }
        private void dgInbox_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        #region  체크박스 선택 이벤트
        private void dgInPalletChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            // Clear();

            if ((bool)rb.IsChecked && ((rb.DataContext as DataRowView).Row["CHK"].ToString().Equals(bool.FalseString) || string.IsNullOrEmpty((rb.DataContext as DataRowView).Row["CHK"].ToString())))
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
                dgInPallet.SelectedIndex = idx;

                Getinbox();
                SetDetailInfo();
            }
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgInbox.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgInbox.Rows[i].DataItem, "CHK"))) || Util.NVC(DataTableConverter.GetValue(dgInbox.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgInbox.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgInbox.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgInbox.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgInbox.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        #endregion

    
        private void confirmPopup_Closed(object sender, EventArgs e)
        {
            Report_1st_Boxing popup = sender as Report_1st_Boxing;
            string sPalletId = popup.PALLET_ID;
            int idx = 0;

            grdMain.Children.Remove(popup);

            GetPalletList();
            for (int i = 0; i < dgInPallet.Rows.Count; i++)
            {
                if (Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["BOXID"].Index).Value).Equals(sPalletId))
                {
                    idx = i;
                    break;
                }
            }
            DataTableConverter.SetValue(dgInPallet.Rows[idx].DataItem, "CHK", true);
            // Util.MessageInfo("SFU1275");
            Getinbox();
            dgInPallet.ScrollIntoView(idx, 0);
        }


        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(txtPalletID.Text))
            {
                AddToPalletList();
                if (dgInPallet.Rows.Count > 1)
                {
                   // DataTableConverter.SetValue(dgInPallet.Rows[0].DataItem, "CHK", true);
                    Getinbox();
                    SetDetailInfo();
                    txtPalletID.Clear();
                }
            }
        }

        private void btnPrintTag_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TagPrint();                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TagPrint()
        {
            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

            if (idxPallet < 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            //if (Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXSTAT"].Index).Value) != "PACKED")
            //{
            //    //SFU4262		실적 확정후 작업 가능합니다.	
            //    Util.MessageValidation("SFU4262");
            //    return;
            //}


            string sPalletId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);

            Report_1st_Boxing popup = new Report_1st_Boxing();
            popup.FrameOperation = this.FrameOperation;
            //  DataSet ds = GetPalletDataSet();
            if (popup != null)
            {
                object[] Parameters = new object[3];

                Parameters[0] = sPalletId;
                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(confirmPopup_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }
        private void dgInPallet_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == C1.WPF.DataGrid.DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXSTAT_LIST")).Equals(lblCreated.Tag))
                    {
                        e.Cell.Presenter.Background = lblCreated.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXSTAT_LIST")).Equals(lblPacking.Tag))
                    {
                        e.Cell.Presenter.Background = lblPacking.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXSTAT_LIST")).Equals(lblPacked.Tag))
                    {
                        e.Cell.Presenter.Background = lblPacked.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXSTAT_LIST")).Equals(lblShipping.Tag))
                    {
                        e.Cell.Presenter.Background = lblShipping.Background;
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }
        #endregion

        #region 인팔레트 이벤트
        private void btnBoxLabelPrint_Click(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrEmpty(txtShift_Main.Text))
            //{
            //    Util.MessageValidation("SFU1845");
            //    return;
            //}

            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                Util.MessageValidation("SFU1843");
                return;
            }

            if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //SFU1223 라인을 선택 하세요
                Util.MessageValidation("SFU1223");
                return;
            }

            BOX001_202_INBOX_LABEL popup = new BOX001_202_INBOX_LABEL();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = Util.NVC(cboLine.SelectedValue);
                Parameters[1] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[2] = txtWorker_Main.Tag; // 작업자id

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");
                Parameters[3] = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["PKG_LOTID"].Index).Value);
                Parameters[4] = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["PRDT_GRD_CODE"].Index).Value);
                Parameters[5] = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["PRODID"].Index).Value);
                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puInboxLabel_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }

        private void puInboxLabel_Closed(object sender, EventArgs e)
        {
            BOX001_202_INBOX_LABEL popup = sender as BOX001_202_INBOX_LABEL;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            }
            this.grdMain.Children.Remove(popup);
        }
        private void btnRunCancel_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (string.IsNullOrEmpty(txtShift_Main.Text))
                //{
                //    //SFU1845	작업조를 입력해 주세요.
                //    Util.MessageValidation("SFU1845");
                //    return;
                //}

                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    //SFU1673 설비를 선택 하세요.
                    Util.MessageValidation("SFU1673", (action) =>
                    {
                        cboEquipment.Focus();
                    });
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXSTAT"].Index).Value) == "PACKED")
                {
                    //SFU3610	이미 포장 완료 됐습니다.[BOX의 정보 확인]	
                    Util.MessageValidation("SFU3610");
                    return;
                }


                if (Util.NVC_Int(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["WIPQTY"].Index).Value) != Util.NVC_Int(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["TOTAL_QTY"].Index).Value))
                {
                    //SFU4417	투입수량과 포장수량이 일치하지 않습니다.	
                    Util.MessageValidation("SFU4417");
                    return;

                }
                //SFU2802	포장출고를 하시겠습니까?
                Util.MessageConfirm("SFU3156", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        DataSet indataSet = new DataSet();
                        DataTable inDataTable = indataSet.Tables.Add("INDATA");
                        //   inDataTable.Columns.Add("SHIP_YN");
                        // inDataTable.Columns.Add("EXP_DOM_TYPE_CODE;
                        inDataTable.Columns.Add("EQPTID");
                        inDataTable.Columns.Add("BOXID");
                        inDataTable.Columns.Add("SHFTID");
                        inDataTable.Columns.Add("USERID");
                        inDataTable.Columns.Add("USERNAME");
                        inDataTable.Columns.Add("PACK_NOTE");
                        inDataTable.Columns.Add("SHIP_YN");

                        DataRow newRow = inDataTable.NewRow();
                        //   newRow["SHIP_YN"] = "Y";    // 원각형: "Y" / 파우치: "N"
                        //   newRow["EXP_DOM_TYPE_CODE"] = Util.NVC(dgInPallet.GetCell(dgInPallet.SelectedIndex, dgInPallet.Columns["EXP_DOM_TYPE_CODE"].Index).Value);

                        newRow["EQPTID"] = cboEquipment.SelectedValue;
                        newRow["BOXID"] = txtPalletID_DETL.Text;
                        newRow["SHFTID"] = txtShift_Main.Tag;
                        newRow["USERID"] = txtWorker_Main.Tag;
                        newRow["USERNAME"] = txtWorker_Main.Text;
                        newRow["PACK_NOTE"] = new TextRange(txtNote_DETL.Document.ContentStart, txtNote_DETL.Document.ContentEnd).Text;
                        newRow["SHIP_YN"] = "N";

                        inDataTable.Rows.Add(newRow);
                        loadingIndicator.Visibility = Visibility.Visible;

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_INPALLET_NJ", "INDATA", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }
                                TagPrint();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }, indataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private void btnConfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (string.IsNullOrEmpty(txtShift_Main.Text))
                //{
                //    //SFU1845	작업조를 입력해 주세요.
                //    Util.MessageValidation("SFU1845");
                //    return;
                //}

                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXSTAT"].Index).Value) != "PACKED")
                {
                    //SFU4262		실적 확정후 작업 가능합니다.	
                    Util.MessageValidation("SFU4262");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["RCV_ISS_STAT_CODE"].Index).Value))
                && Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["RCV_ISS_STAT_CODE"].Index).Value) == "SHIPPING")
                {
                    //	SFU4415	출고중 팔레트는 실적취소 불가합니다. 	
                    Util.MessageValidation("SFU4415");
                    return;
                }

                string sPalletId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);


                //	SFU2805		포장출고를 취소하시겠습니까?	
                Util.MessageConfirm("SFU4263", (result) =>
                { 
                    if (result == MessageBoxResult.OK)
                    {
                        DataSet indataSet = new DataSet();
                        DataTable inDataTable = indataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("BOXID");
                        inDataTable.Columns.Add("USERID");

                        DataRow newRow = inDataTable.NewRow();
                        newRow["BOXID"] = sPalletId;
                        newRow["USERID"] = txtWorker_Main.Tag;

                        inDataTable.Rows.Add(newRow);

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_CANCEL_INPALLET_NJ", "INDATA", null, (bizResult, bizException) =>
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
                                GetPalletList();

                                int idx;
                                for (int i = 0; i < dgInPallet.Rows.Count; i++)
                                {
                                    if (Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["BOXID"].Index).Value).Equals(sPalletId))
                                    {
                                        idx = i;
                                        DataTableConverter.SetValue(dgInPallet.Rows[idx].DataItem, "CHK", true);
                                        dgInPallet.ScrollIntoView(idx, 0);
                                        break;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }, indataSet);
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
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            GetPalletList();            
        }
        #endregion

        #region 인박스 이벤트
        private void txtBoxId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(txtBoxId.Text))
            {
                // 작업조,작업자 체크
                //if (string.IsNullOrEmpty(txtShift_Main.Text))
                //{
                //    //SFU1845	작업조를 입력해 주세요.
                //    Util.MessageValidation("SFU1845");
                //    return;
                //}

                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }
                if (txtBoxQty.Value > txtRestCellQty.Value)
                {
                    //SFU1859 SFU 잔량이 없습니다.
                    Util.MessageValidation("SFU1859", (action) =>
                    {
                        txtBoxQty.Focus();
                    });
                    return;
                }

                if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    //SFU1673 설비를 선택 하세요.
                    Util.MessageValidation("SFU1673", (action) =>
                    {
                        cboEquipment.Focus();
                    });
                    return;
                }

                RegInbox(txtBoxId.Text, txtBoxQty.Value);
                Getinbox(true);
                txtBoxId.Text = string.Empty;
                txtBoxId.Focus();
            }
        }
        private void btnBoxUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (string.IsNullOrEmpty(txtShift_Main.Text))
                //{
                //    //SFU1845	작업조를 입력해 주세요.
                //    Util.MessageValidation("SFU1845");
                //    return;
                //}

                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");
                string boxStat = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXSTAT"].Index).Value);
                
                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (boxStat.Equals("PACKED"))
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU4296");
                    return;
                }

                List<int> idxBoxList = _util.GetDataGridCheckRowIndex(dgInbox, "CHK");

                if (idxBoxList.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                string sPalletId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);
                string sProdId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["PRODID"].Index).Value);
                int idx = 0;

                DataSet indataSet = new DataSet();
                DataTable inPalletTable = indataSet.Tables.Add("INDATA");
                inPalletTable.Columns.Add("PRODID");
                inPalletTable.Columns.Add("BOXID");
                inPalletTable.Columns.Add("USERID");

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("BOXID");
                inBoxTable.Columns.Add("TOTAL_QTY");

                DataRow newRow = inPalletTable.NewRow();
                newRow["PRODID"] = sProdId;
                newRow["BOXID"] = sPalletId;
                newRow["USERID"] = txtWorker_Main.Tag;

                inPalletTable.Rows.Add(newRow);

                foreach (int idxBox in idxBoxList)
                {
                    string sBoxId = Util.NVC(dgInbox.GetCell(idxBox, dgInbox.Columns["BOXID"].Index).Value);
                    string sQty = Util.NVC(dgInbox.GetCell(idxBox, dgInbox.Columns["TOTAL_QTY"].Index).Value);

                    newRow = inBoxTable.NewRow();
                    newRow["BOXID"] = sBoxId;
                    newRow["TOTAL_QTY"] = sQty;
                    inBoxTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_UPD_INBOX_NJ", "INPALLET,INBOX", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //SFU1265	수정되었습니다.
                        Util.MessageInfo("SFU1265");

                        GetPalletList();
                        for (int i = 0; i < dgInPallet.Rows.Count; i++)
                        {
                            if (Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["BOXID"].Index).Value).Equals(sPalletId))
                            {
                                idx = i;
                                break;
                            }
                        }
                        DataTableConverter.SetValue(dgInPallet.Rows[idx].DataItem, "CHK", true);
                        Getinbox();
                        dgInPallet.ScrollIntoView(idx, 0);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private void btnBoxDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (string.IsNullOrEmpty(txtShift_Main.Text))
                //{
                //    //SFU1845	작업조를 입력해 주세요.
                //    Util.MessageValidation("SFU1845");

                //    return;
                //}

                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");


                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXSTAT"].Index).Value) == "PACKED")
                {
                    //SFU3610	이미 포장 완료 됐습니다.[BOX의 정보 확인]	
                    Util.MessageValidation("SFU3610");
                    return;
                }

                List<int> idxBoxList = _util.GetDataGridCheckRowIndex(dgInbox, "CHK");

                if (idxBoxList.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                // SFU1230 삭제하시겠습니까?
                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DeleteInbox();
                        //Getinbox();
                    }
                    else
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
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
        }
        private void btnPrintAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (string.IsNullOrEmpty(txtShift_Main.Text))
                //{
                //    //SFU1845	작업조를 입력해 주세요.
                //    Util.MessageValidation("SFU1845");
                //    return;
                //}

                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXSTAT"].Index).Value) == "PACKED")
                {
                    //SFU3610	이미 포장 완료 됐습니다.[BOX의 정보 확인]	
                    Util.MessageValidation("SFU3610");
                    return;
                }


                if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    //SFU1673 설비를 선택 하세요.
                    Util.MessageValidation("SFU1673", (action) =>
                    {
                        cboEquipment.Focus();
                    });
                    return;
                }

                if (txtRestCellQty.Value <= 0)
                {
                    //SFU1859 SFU 잔량이 없습니다.
                    Util.MessageValidation("SFU1859");
                    return;
                }


                //SFU4258	일괄 발행 하시겠습니까?	
                Util.MessageConfirm("SFU4258", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        PrintInboxLabel(true);
                    }
                    else
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
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
        }
        #endregion



        #region BIZ
        /// <summary>
        /// 작업 대상 조회
        /// BIZ : BR_PRD_GET_INPALLET_LIST_NJ
        /// </summary>
        private void GetPalletList()
        {
            try
            {
                if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    //SFU1223 라인을 선택 하세요
                    Util.MessageValidation("SFU1223");
                    return;
                }

                //if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
                //{
                //    //SFU1673 설비를 선택 하세요.
                //    Util.MessageValidation("SFU1673");
                //    return;
                //}

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID");
                RQSTDT.Columns.Add("EQSGID");
                RQSTDT.Columns.Add("EQPTID");
                RQSTDT.Columns.Add("BOXID");
                RQSTDT.Columns.Add("BOXSTAT_LIST");
                //RQSTDT.Columns.Add("RCV_ISS_STAT_CODE");
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.NVC(cboLine.SelectedValue);
                dr["BOXID"] = Util.NVC(txtPalletID.Text);
                dr["BOXSTAT_LIST"] = string.IsNullOrEmpty(_searchStat) ? _searchStat : _searchStat.Remove(_searchStat.Length - 1);
                // dr["RCV_ISS_STAT_CODE"] =string.IsNullOrEmpty(_rcvStat)?null:_rcvStat;
                RQSTDT.Rows.Add(dr);

                DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_INPALLET_LIST_NJ", "INDATA", "OUTDATA", RQSTDT);

                if (!RSLTDT.Columns.Contains("CHK"))
                    RSLTDT = _util.gridCheckColumnAdd(RSLTDT, "CHK");

                Util.GridSetData(dgInPallet, RSLTDT, FrameOperation, true);
                //Clear();

                if (dgInPallet.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgInPallet.Columns["WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgInPallet.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgInPallet.Columns["RESNQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void AddToPalletList()
        {
            try
            {
                if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    //SFU1223 라인을 선택 하세요
                    Util.MessageValidation("SFU1223");
                    return;
                }

                //if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
                //{
                //    //SFU1673 설비를 선택 하세요.
                //    Util.MessageValidation("SFU1673");
                //    return;
                //}

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID");
                RQSTDT.Columns.Add("EQSGID");
                RQSTDT.Columns.Add("EQPTID");
                RQSTDT.Columns.Add("BOXID");

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.NVC(cboLine.SelectedValue);
                dr["BOXID"] = Util.NVC(txtPalletID.Text);

                RQSTDT.Rows.Add(dr);

                DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_INPALLET_LIST_NJ", "INDATA", "OUTDATA", RQSTDT);
                if(_dtPallet!=null)
                {
                    if (_dtPallet.Rows.Count > 0)
                    {
                        for (int i = 0; i < _dtPallet.Rows.Count; i++)
                        {
                            if(_dtPallet.Rows[i]["BOXID"].Equals(txtPalletID.Text)) // 입력한 팔렛아이디 == _dtPallet에 있는 팔렛아이디
                            {
                                Util.MessageValidation("SFU1914"); //중복 스캔되었습니다.
                                return;
                            }
                        }
                    }
                        _dtPallet.Merge(RSLTDT);
                }
                if (!_dtPallet.Columns.Contains("CHK"))
                    _dtPallet = _util.gridCheckColumnAdd(RSLTDT, "CHK");
                Util.GridSetData(dgInPallet, _dtPallet, FrameOperation, true);
                DataTableConverter.SetValue(dgInPallet.Rows[_dtPallet.Rows.Count-1].DataItem, "CHK", true);
                if (dgInPallet.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgInPallet.Columns["WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgInPallet.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgInPallet.Columns["RESNQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                txtPalletID.Focus();
            }
        }
        /// <summary>
        /// 인박스 등록 
        ///  BIZ : BR_PRD_REG_INBOX_NJ
        /// </summary>
        /// <param name="boxId"></param>
        private bool RegInbox(string boxId, double qty, string zplCode = null)
        {
            try
            {
                int row = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

                if (row < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return false;
                }

                string sBoxId = Util.NVC(dgInPallet.GetCell(row, dgInPallet.Columns["BOXID"].Index).Value);

                DataSet indataSet = new DataSet();
                DataTable inPalletTable = indataSet.Tables.Add("INPALLET");
                inPalletTable.Columns.Add("EQPTID");
                inPalletTable.Columns.Add("BOXID");
                inPalletTable.Columns.Add("SHFTID");
                inPalletTable.Columns.Add("USERID");

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("BOXID");
                inBoxTable.Columns.Add("TOTAL_QTY");

                DataRow newRow = inPalletTable.NewRow();
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["BOXID"] = sBoxId;
                newRow["SHFTID"] = txtShift_Main.Tag;
                newRow["USERID"] = txtWorker_Main.Tag;

                inPalletTable.Rows.Add(newRow);

                newRow = inBoxTable.NewRow();
                newRow["TOTAL_QTY"] = qty;
                newRow["BOXID"] = boxId;
                inBoxTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_INBOX_NJ", "INPALLET,INBOX", null, indataSet);
                Getinbox(false);

                if ((bool)chkPrintYN.IsChecked)
                {
                    PrintLabel(zplCode, _drPrtInfo);
                }

                return true;
            }
            catch (Exception ex)
            {
                // loadingIndicator.Visibility = Visibility.Collapsed;               
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 인박스 조회
        ///  BIZ : BR_PRD_GET_INBOX_LIST_NJ
        /// </summary>
        /// <param name="isRefreshGrid"></param>
        private void Getinbox(bool isRefreshGrid = true)
        {
            try
            {
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                int inputQty = (int)double.Parse(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["WIPQTY"].Index).Value.ToString());
                int sum = 0;
                string sPalletID = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);
              
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("OUTER_BOXID2", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["OUTER_BOXID2"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_INBOX_LIST_NJ", "RQSTDT", "RSLTDT", RQSTDT);

                if (!dtResult.Columns.Contains("CHK"))
                {
                    dtResult.Columns.Add("CHK");    
                }
                if (isRefreshGrid)
                {
                    Util.GridSetData(dgInbox, dtResult, FrameOperation);
                    //SetDetailInfo();
                    if (dgInbox.GetRowCount() > 0)
                    {
                        DataGridAggregate.SetAggregateFunctions(dgInbox.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });                    
                    }
                    // txtBoxQty.Value = inBoxQty;
                  
                }

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    //sum += (int)double.Parse(dgInbox.GetCell(i, dgInbox.Columns["TOTAL_QTY"].Index).Value.ToString());
                    sum += (int)double.Parse(dtResult.Rows[i]["TOTAL_QTY"].ToString());
                }
                txtRestCellQty.Value = inputQty - sum;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 태그 발행용 데이터 조회
        ///  BIZ : BR_PRD_GET_TAG_INPALLET_NJ
        /// </summary>
        /// <returns></returns>
        private DataSet GetPalletDataSet()
        {
            try
            {
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

                DataSet inDataSet = new DataSet();
                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("BOXID");
                inDataTable.Columns.Add("LANGID");
                DataRow inDataRow = inDataTable.NewRow();
                inDataRow["BOXID"] = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);
                inDataRow["LANGID"] = LoginInfo.LANGID;
                inDataTable.Rows.Add(inDataRow);

                DataSet resultDs = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_TAG_INPALLET_NJ", "INDATA", "OUTDATA,OUTBOX", inDataSet);
                return resultDs;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void PrintInboxLabel(bool isPrintAll = false)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
                {
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                int inputQty = (int)double.Parse(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["WIPQTY"].Index).Value.ToString());
                int packedQty = (int)double.Parse(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["TOTAL_QTY"].Index).Value.ToString());
                int prtQty = isPrintAll ? (int)Math.Ceiling((inputQty - packedQty) / txtBoxQty.Value) : 1;
                string sPalletId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);
                int idx = 0;

                if (isPrintAll == true)
                {
                    string sBizRule = "BR_PRD_REG_INBOX_BATCH_NJ";

                    DataSet indataSet = new DataSet();
                    DataTable inDataTable = indataSet.Tables.Add("INPALLET");
                    inDataTable.Columns.Add("AREAID");
                    inDataTable.Columns.Add("EQPTID");
                    inDataTable.Columns.Add("BOXID");
                    inDataTable.Columns.Add("SHFTID");
                    inDataTable.Columns.Add("PACKQTY");
                    inDataTable.Columns.Add("PRINTQTY");
                    inDataTable.Columns.Add("LABELTYPE");
                    inDataTable.Columns.Add("USERID");
                    inDataTable.Columns.Add("PRODID");
                    inDataTable.Columns.Add("PRDT_GRD_CODE");
                    inDataTable.Columns.Add("PKG_LOTID");
                    inDataTable.Columns.Add("PGM_ID");    //라벨 이력 저장용
                    inDataTable.Columns.Add("BZRULE_ID"); //라벨 이력 저장용

                    DataTable inPrintTable = indataSet.Tables.Add("INPRINT");
                    inPrintTable.Columns.Add("PRMK");
                    inPrintTable.Columns.Add("RESO");
                    inPrintTable.Columns.Add("PRCN");
                    inPrintTable.Columns.Add("MARH");
                    inPrintTable.Columns.Add("MARV");
                    inPrintTable.Columns.Add("DARK");

                    DataRow newRow = inDataTable.NewRow();
                    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                    newRow["BOXID"] = sPalletId;
                    newRow["SHFTID"] = txtShift_Main.Tag;
                    newRow["PACKQTY"] = txtBoxQty.Value;
                    newRow["PRINTQTY"] = prtQty;
                    newRow["LABELTYPE"] = "CB_NORMAL";
                    newRow["USERID"] = txtWorker_Main.Text;
                    newRow["PRODID"] = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["PRODID"].Index).Value);
                    newRow["PRDT_GRD_CODE"] = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["PRDT_GRD_CODE"].Index).Value);
                    newRow["PKG_LOTID"] = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["PKG_LOTID"].Index).Value);
                    newRow["PGM_ID"] = _sPGM_ID;
                    newRow["BZRULE_ID"] = sBizRule;

                    inDataTable.Rows.Add(newRow);

                    newRow = inPrintTable.NewRow();
                    newRow["PRMK"] = _sPrt; // "ZEBRA"; Print type
                    newRow["RESO"] = _sRes; // "203"; DPI
                    newRow["PRCN"] = _sCopy; // "1"; Print Count
                    newRow["MARH"] = _sXpos; // "0"; Horizone pos
                    newRow["MARV"] = _sYpos; // "0"; Vertical pos
                    newRow["DARK"] = _sDark; // darkness
                    inPrintTable.Rows.Add(newRow);

                    //inbox, zplcode 리스트
                    //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_INBOX_BATCH_NJ", "INPALLET,INPRINT", "OUTDATA", indataSet);
                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizRule, "INPALLET,INPRINT", "OUTDATA", indataSet);

                    if (dsResult != null && dsResult.Tables["OUTDATA"] != null)
                    {
                        DataTable dtInBox = dsResult.Tables["OUTDATA"];

                        if (dtInBox != null && dtInBox.Rows.Count > 0)
                        {
                            string boxId = string.Empty;
                            string zplCode = string.Empty;
                            for (int i = 0; i < prtQty; i++)
                            {
                                boxId = dtInBox.Rows[i]["BOXID"].ToString();
                                zplCode += dtInBox.Rows[i]["ZPLCODE"].ToString();
                                //if (!RegInbox(boxId, txtBoxQty.Value < txtRestCellQty.Value ? txtBoxQty.Value : txtRestCellQty.Value, zplCode))
                                //    return;
                            }

                            if ((bool)chkPrintYN.IsChecked)
                            {
                                PrintLabel(zplCode, _drPrtInfo);
                            }
                        }
                    }
                }
                else
                {
                    string sBizRule = "BR_PRD_GET_INBOX_LABEL_NJ_CIRCULAR";

                    DataSet indataSet = new DataSet();
                    DataTable inDataTable = indataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("AREAID");
                    inDataTable.Columns.Add("EQPTID");
                    inDataTable.Columns.Add("PRINTQTY");
                    inDataTable.Columns.Add("LABELTYPE");
                    inDataTable.Columns.Add("USERID");
                    inDataTable.Columns.Add("PRODID");
                    inDataTable.Columns.Add("PRDT_GRD_CODE");
                    inDataTable.Columns.Add("PKG_LOTID");
                    inDataTable.Columns.Add("PGM_ID");    //라벨 이력 저장용
                    inDataTable.Columns.Add("BZRULE_ID"); //라벨 이력 저장용

                    DataTable inPrintTable = indataSet.Tables.Add("INPRINT");
                    inPrintTable.Columns.Add("PRMK");
                    inPrintTable.Columns.Add("RESO");
                    inPrintTable.Columns.Add("PRCN");
                    inPrintTable.Columns.Add("MARH");
                    inPrintTable.Columns.Add("MARV");
                    inPrintTable.Columns.Add("DARK");

                    DataRow newRow = inDataTable.NewRow();
                    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                    newRow["PRINTQTY"] = prtQty;
                    newRow["LABELTYPE"] = "CB_NORMAL";
                    newRow["USERID"] = txtWorker_Main.Text;
                    newRow["PRODID"] = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["PRODID"].Index).Value);
                    newRow["PRDT_GRD_CODE"] = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["PRDT_GRD_CODE"].Index).Value);
                    newRow["PKG_LOTID"] = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["PKG_LOTID"].Index).Value);
                    newRow["PGM_ID"] = _sPGM_ID;
                    newRow["BZRULE_ID"] = sBizRule;

                    inDataTable.Rows.Add(newRow);

                    newRow = inPrintTable.NewRow();
                    newRow["PRMK"] = _sPrt; // "ZEBRA"; Print type
                    newRow["RESO"] = _sRes; // "203"; DPI
                    newRow["PRCN"] = _sCopy; // "1"; Print Count
                    newRow["MARH"] = _sXpos; // "0"; Horizone pos
                    newRow["MARV"] = _sYpos; // "0"; Vertical pos
                    newRow["DARK"] = _sDark; // darkness
                    inPrintTable.Rows.Add(newRow);

                    //inbox, zplcode 리스트
                    //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_INBOX_LABEL_NJ_CIRCULAR", "INDATA,INPRINT", "OUTDATA", indataSet);
                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizRule, "INDATA,INPRINT", "OUTDATA", indataSet);

                    if (dsResult != null && dsResult.Tables["OUTDATA"] != null)
                    {
                        DataTable dtInBox = dsResult.Tables["OUTDATA"];

                        if (dtInBox != null && dtInBox.Rows.Count > 0)
                        {
                            string boxId = string.Empty;
                            string zplCode = string.Empty;
                            for (int i = 0; i < prtQty; i++)
                            {
                                boxId = dtInBox.Rows[i]["BOXID"].ToString();
                                zplCode = dtInBox.Rows[i]["ZPLCODE"].ToString();
                                if (!RegInbox(boxId, txtBoxQty.Value < txtRestCellQty.Value ? txtBoxQty.Value : txtRestCellQty.Value, zplCode))
                                    return;
                            }

                            //GetPalletList();
                            //for (int i = 0; i < dgInPallet.Rows.Count; i++)
                            //{
                            //    if (Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["BOXID"].Index).Value).Equals(sPalletId))
                            //    {
                            //        idx = i;
                            //        break;
                            //    }
                            //}
                            //DataTableConverter.SetValue(dgInPallet.Rows[idx].DataItem, "CHK", true);
                            //Util.MessageInfo("SFU1275");
                            //Getinbox();
                            //dgInPallet.ScrollIntoView(idx, 0);
                        }
                    }
                }

                GetPalletList();
                for (int i = 0; i < dgInPallet.Rows.Count; i++)
                {
                    if (Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["BOXID"].Index).Value).Equals(sPalletId))
                    {
                        idx = i;
                        break;
                    }
                }
                DataTableConverter.SetValue(dgInPallet.Rows[idx].DataItem, "CHK", true);
                Util.MessageInfo("SFU1275");
                Getinbox();
                dgInPallet.ScrollIntoView(idx, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 인박스 삭제
        /// BIZ : BR_PRD_DEL_INBOX_NJ
        /// </summary>
        private void DeleteInbox()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");
                List<int> idxBoxList = _util.GetDataGridCheckRowIndex(dgInbox, "CHK");

                string sPalletId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);
                string sProdId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["PRODID"].Index).Value);
                int idx = 0;
                DataSet indataSet = new DataSet();
                DataTable inPalletTable = indataSet.Tables.Add("INDATA");
                inPalletTable.Columns.Add("PRODID");
                inPalletTable.Columns.Add("BOXID");
                inPalletTable.Columns.Add("USERID");

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("BOXID");
                inBoxTable.Columns.Add("TOTAL_QTY");

                DataRow newRow = inPalletTable.NewRow();
                newRow["PRODID"] = sProdId;
                newRow["BOXID"] = sPalletId;
                newRow["USERID"] = txtWorker_Main.Tag;

                inPalletTable.Rows.Add(newRow);

                foreach (int idxBox in idxBoxList)
                {
                    string sBoxId = Util.NVC(dgInbox.GetCell(idxBox, dgInbox.Columns["BOXID"].Index).Value);
                    string sQty = Util.NVC(dgInbox.GetCell(idxBox, dgInbox.Columns["TOTAL_QTY"].Index).Value);

                    newRow = inBoxTable.NewRow();
                    newRow["BOXID"] = sBoxId;
                    newRow["TOTAL_QTY"] = sQty;
                    inBoxTable.Rows.Add(newRow);
                }
                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService_Multi("BR_PRD_DEL_INBOX_NJ", "INPALLET,INBOX", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetPalletList();
                        for (int i = 0; i < dgInPallet.Rows.Count; i++)
                        {
                            if (Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["BOXID"].Index).Value).Equals(sPalletId))
                            {
                                idx = i;
                                break;
                            }
                        }
                        DataTableConverter.SetValue(dgInPallet.Rows[idx].DataItem, "CHK", true);
                        dgInPallet.ScrollIntoView(idx, 0);
                        Getinbox();
                        //SFU1273	삭제되었습니다.
                        Util.MessageInfo("SFU1273");
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }

                }, indataSet);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {

            }

        }

        #endregion

        #region Method

        private void setEquipmentCombo(C1ComboBox cbo, CommonCombo.ComboStatus cs, string eqsgID = null)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "COATER_EQPT_TYPE" };
            string[] arrCondition = { LoginInfo.LANGID, eqsgID, "B1000", null };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, cs, selectedValueText, displayMemberText, null);
        }

        private bool PrintLabel(string zpl, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }

                System.Threading.Thread.Sleep(300);
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }

        private void SetDetailInfo()
        {
            try
            {
                int idx = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

                if (idx < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                string sinBoxQty = Util.NVC(dgInPallet.GetCell(idx, dgInPallet.Columns["INBOX_LOAD_QTY"].Index).Value);
                int inBoxQty = Util.NVC_Int(sinBoxQty);
                if (!string.IsNullOrWhiteSpace(sinBoxQty)) txtBoxQty.Value = inBoxQty;

                chkAll.IsChecked = false;


           

                string sBoxId = Util.NVC(dgInPallet.GetCell(idx, dgInPallet.Columns["BOXID"].Index).Value);
                string sProdId = Util.NVC(dgInPallet.GetCell(idx, dgInPallet.Columns["PRODID"].Index).Value);
                string sProject = Util.NVC(dgInPallet.GetCell(idx, dgInPallet.Columns["PROJECT"].Index).Value);
                string sEqptId = Util.NVC(dgInPallet.GetCell(idx, dgInPallet.Columns["EQPTID"].Index).Value);
                string sEqptName = Util.NVC(dgInPallet.GetCell(idx, dgInPallet.Columns["EQPTNAME"].Index).Value);
                string sWrkType = Util.NVC(dgInPallet.GetCell(idx, dgInPallet.Columns["PACK_WRK_TYPE_CODE"].Index).Value);
                string sPrdtGrd = Util.NVC(dgInPallet.GetCell(idx, dgInPallet.Columns["PRDT_GRD_CODE"].Index).Value);
                string sLotId = Util.NVC(dgInPallet.GetCell(idx, dgInPallet.Columns["PKG_LOTID"].Index).Value);
                string sPackDttm = Util.NVC(dgInPallet.GetCell(idx, dgInPallet.Columns["PACKDTTM"].Index).Value);
                string sNote = Util.NVC(dgInPallet.GetCell(idx, dgInPallet.Columns["PACK_NOTE"].Index).Value);
                string sExpDom = Util.NVC(dgInPallet.GetCell(idx, dgInPallet.Columns["EXP_DOM_TYPE_NAME"].Index).Value);
                string sEqsgId = Util.NVC(dgInPallet.GetCell(idx, dgInPallet.Columns["EQSGID"].Index).Value);
                txtPalletID_DETL.Text = sBoxId;
                txtLotID_DETL.Text = sLotId;
                txtPackDttm_DETL.Text = sPackDttm;
                txtPrdtGrd_DETL.Text = sPrdtGrd;
                txtPrjtName_DETL.Text = sProject;
                txtProdId_DETL.Text = sProdId;
                txtExpDom_DETL.Text = sExpDom;
                

                //cboEquipment.SelectedValue = sEqptId;

                setEquipmentCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, sEqsgId);

                if (string.IsNullOrWhiteSpace(Util.NVC(cboEquipment.SelectedValue)))
                    cboEquipment.SelectedValue = "SELECT";
                if (!string.IsNullOrWhiteSpace(sEqptId))
                    cboEquipment.SelectedValue = sEqptId;

                new TextRange(txtNote_DETL.Document.ContentStart, txtNote_DETL.Document.ContentEnd).Text = sNote;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void Clear()
        {
            // Util.gridClear(dgInbox);
            dgInbox.ItemsSource = null;
            txtPalletID_DETL.Text = string.Empty;
            txtLotID_DETL.Text = string.Empty;
            txtPackDttm_DETL.Text = string.Empty;
            txtPrdtGrd_DETL.Text = string.Empty;
            txtPrjtName_DETL.Text = string.Empty;
            txtProdId_DETL.Text = string.Empty;
            txtNote_DETL.Document.Blocks.Clear();
            cboEquipment.ItemsSource = null;
            cboEquipment.SelectedValue = null;
            _dtPallet.Clear();
            txtBoxQty.Value = 100;
        }

        #endregion

        int iStart = 0;
        private void dgInbox_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name == "CHK")
            {
                e.Cancel = false;
                return;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgInbox.Rows[e.Row.Index].DataItem, "CHK")) != bool.TrueString)
            {
                e.Cancel = true;
                return;
            }

            else
            {
                iStart = Util.NVC_Int(DataTableConverter.GetValue(dgInbox.Rows[e.Row.Index].DataItem, "TOTAL_QTY"));
            }
        }

        private void dgInbox_CommittingEdit(object sender, C1.WPF.DataGrid.DataGridEndingEditEventArgs e)
        {
            if (e.Column.Name == "CHK")
            {
                e.Cancel = false;
                return;
            }

            if (e.Column.Name == "TOTAL_QTY")
            {
                int inputQty = Util.NVC_Int(DataTableConverter.GetValue(dgInPallet.Rows[_util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK")].DataItem, "WIPQTY"));
                int sumQty = Util.NVC_Int(DataTableConverter.Convert(dgInbox.ItemsSource).Compute("sum(TOTAL_QTY)", "").GetString());

                if (inputQty < sumQty)
                {
                    // SFU4224 전체수량과 입력수량의 합이 일치하지 않습니다.
                    Util.MessageValidation("SFU4224");

                    DataTableConverter.SetValue(dgInbox.Rows[e.Row.Index].DataItem, "TOTAL_QTY", iStart);
                    iStart = 0;
                    return;
                }
            }
        }

        #region 추가라벨발행
        private bool ValidationAddInbox()
        {
            if (dgInbox.ItemsSource == null || dgInbox.Rows.Count < 1)
            {
                Util.MessageValidation("SFU3538");//선택된 데이터가 없습니다.
                return false;
            }

            int idx = _util.GetDataGridCheckFirstRowIndex(dgInbox, "CHK");

            if (idx < 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                //SFU1843	작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return false;
            }

            return true;
        }

        private bool ValidationGridAdd(string popName)
        {
            foreach (UIElement ui in grdMain.Children)
            {
                if (((System.Windows.FrameworkElement)ui).Name.Equals(popName))
                {
                    // 프로그램이 이미 실행 중 입니다. 
                    Util.MessageValidation("SFU3193");
                    return false;
                }
            }

            return true;
        }

        private void btnPrintAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationAddInbox())
                return;

            BOX001_202_INBOX_LABEL_ADD popupLabelAdd = new BOX001_202_INBOX_LABEL_ADD { FrameOperation = FrameOperation };
            if (ValidationGridAdd(popupLabelAdd.Name) == false)
                return;

            DataRow[] dr = DataTableConverter.Convert(dgInbox.ItemsSource).Select("CHK = '1' or CHK = 'True'");

            object[] parameters = new object[3];
            parameters[0] = Util.NVC(txtWorker_Main.Tag);
            parameters[1] = "";
            parameters[2] = dr;
            C1WindowExtension.SetParameters(popupLabelAdd, parameters);

            popupLabelAdd.Closed += popupLabelAdd_Closed;
            grdMain.Children.Add(popupLabelAdd);
            popupLabelAdd.BringToFront();

        }

        private void popupLabelAdd_Closed(object sender, EventArgs e)
        {
            BOX001_202_INBOX_LABEL_ADD popup = sender as BOX001_202_INBOX_LABEL_ADD;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            grdMain.Children.Remove(popup);
        }
        #endregion

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (string.IsNullOrEmpty(txtShift_Main.Text))
                //{
                //    //SFU1845	작업조를 입력해 주세요.
                //    Util.MessageValidation("SFU1845");
                //    return;
                //}

                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXSTAT"].Index).Value) == "PACKED")
                {
                    //SFU3610	이미 포장 완료 됐습니다.[BOX의 정보 확인]	
                    Util.MessageValidation("SFU3610");
                    return;
                }


                if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    //SFU1673 설비를 선택 하세요.
                    Util.MessageValidation("SFU1673", (action) =>
                    {
                        cboEquipment.Focus();
                    });
                    return;
                }

                if (txtRestCellQty.Value <= 0)
                {
                    //SFU1859 SFU 잔량이 없습니다.
                    Util.MessageValidation("SFU1859");
                    return;
                }


                //SFU4258	발행 하시겠습니까?	
                Util.MessageConfirm("SFU2873", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        PrintInboxLabel();
                    }
                    else
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
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
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            dgInPallet.ItemsSource = null;
            Clear();
        }

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        /// <summary>
        /// 출고
        /// Biz : BR_PRD_REG_END_INPALLET_NJ
        private void btnShip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXSTAT"].Index).Value) != "PACKED")
                {
                    //SFU4413		포장 완료된 팔레트만 출고 가능합니다.	
                    Util.MessageValidation("SFU4413");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["RCV_ISS_STAT_CODE"].Index).Value))
                    && Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["RCV_ISS_STAT_CODE"].Index).Value) == "SHIPPING")
                {
                    //	SFU4416	이미 출고된 팔레트 입니다.
                    Util.MessageValidation("SFU4416");
                    return;
                }

                //SFU2802	포장출고를 하시겠습니까?
                Util.MessageConfirm("SFU2802", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sPalletId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);

                        DataTable RQSTDT = new DataTable("INDATA");
                        RQSTDT.Columns.Add("BOXID");
                        RQSTDT.Columns.Add("EQPTID");
                        RQSTDT.Columns.Add("SHFTID");
                        RQSTDT.Columns.Add("USERID");
                        RQSTDT.Columns.Add("USERNAME");
                        RQSTDT.Columns.Add("PACK_NOTE");
                        RQSTDT.Columns.Add("SHIP_YN");

                        DataRow newRow = RQSTDT.NewRow();
                        newRow["BOXID"] = sPalletId;
                        newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                        newRow["SHFTID"] = Util.NVC(txtShift_Main.Tag);
                        newRow["USERID"] = Util.NVC(txtWorker_Main.Tag);
                        newRow["USERNAME"] = txtWorker_Main.Text;
                        newRow["PACK_NOTE"] = new TextRange(txtNote_DETL.Document.ContentStart, txtNote_DETL.Document.ContentEnd).Text;
                        newRow["SHIP_YN"] = "Y";
                        RQSTDT.Rows.Add(newRow);

                        loadingIndicator.Visibility = Visibility.Visible;

                        new ClientProxy().ExecuteService("BR_PRD_REG_END_INPALLET_NJ", "INDATA", null, RQSTDT, (RSLTDT, ex) =>
                        {
                            try
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                GetPalletList();
                                _util.SetDataGridCheck(dgInPallet, "CHK", "BOXID", sPalletId, true);
                                //GetDetailInfo();

                                // PackingListPrint();
                            }
                            catch (Exception ex1)
                            {
                                Util.MessageException(ex1);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 출고취소
        /// Biz : BR_PRD_REG_END_CANCEL_INPALLET_NJ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancelShip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXSTAT"].Index).Value) != "PACKED")
                {
                    //SFU4417   실적 확정된 팔레트만 출고 취소 가능합니다.
                    Util.MessageValidation("SFU4417");
                    return;
                }

                if (Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["RCV_ISS_STAT_CODE"].Index).Value) != "SHIPPING")
                {
                    //SFU3717		출고중 상태인 팔레트만 출고취소 가능합니다.	
                    Util.MessageValidation("SFU3717");
                    return;
                }

                //	SFU2805		포장출고를 취소하시겠습니까?	
                Util.MessageConfirm("SFU2805", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sPalletId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);

                        DataTable RQSTDT = new DataTable("INDATA");
                        RQSTDT.Columns.Add("BOXID");
                        RQSTDT.Columns.Add("USERID");

                        DataRow newRow = RQSTDT.NewRow();
                        newRow["USERID"] = txtWorker_Main.Tag;
                        newRow["BOXID"] = sPalletId;
                        RQSTDT.Rows.Add(newRow);

                        new ClientProxy().ExecuteService("BR_PRD_REG_END_CANCEL_INPALLET_NJ", "INDATA", null, RQSTDT, (RSLTDT, ex) =>
                        {
                            try
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                GetPalletList();
                                _util.SetDataGridCheck(dgInPallet, "CHK", "BOXID", sPalletId, true);
                                SetDetailInfo();

                                Util.MessageInfo("SFU3431");
                            }
                            catch (Exception ex1)
                            {
                                Util.MessageException(ex1);
                            }
                            finally
                            {

                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnPltSplitMerge_Click(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrEmpty(txtShift_Main.Text))
            //{
            //    //SFU1845	작업조를 입력해 주세요.
            //    Util.MessageValidation("SFU1845");
            //    return;
            //}

            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                //SFU1843	작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return;
            }
            BOX001_202_PLT_SPLIT popUp = new BOX001_202_PLT_SPLIT();
            popUp.FrameOperation = this.FrameOperation;

            if (popUp != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = txtShift_Main.Tag;
                Parameters[1] = txtWorker_Main.Tag;
                C1WindowExtension.SetParameters(popUp, Parameters);

                popUp.Closed += new EventHandler(split_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(popUp);
                popUp.BringToFront();
            }
        }

        private void split_Closed(object sender, EventArgs e)
        {
            GetPalletList();
        }
        private void lblCreated_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (chkCreated.IsChecked == true)
                chkCreated.IsChecked = false;
            else
                chkCreated.IsChecked = true;
        }
        private void lblPacking_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (chkPacking.IsChecked == true)
                chkPacking.IsChecked = false;
            else
                chkPacking.IsChecked = true;
        }
        private void lblPacked_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (chkPacked.IsChecked == true)
                chkPacked.IsChecked = false;
            else
                chkPacked.IsChecked = true;
        }
        private void lblShipping_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (chkShipping.IsChecked == true)
                chkShipping.IsChecked = false;
            else
                chkShipping.IsChecked = true;
        }

        private void btnRePrint_Click(object sender, RoutedEventArgs e)
        {

            if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
                return;

            List<int> idxBoxList = _util.GetDataGridCheckRowIndex(dgInbox, "CHK");
            if (idxBoxList.Count <= 0)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            try
            {
                string sBizRule = "BR_PRD_GET_OUTBOX_REPRT_NJ_CIRCULAR";

                DataSet ds = new DataSet();
                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("LANGID");
                dtInData.Columns.Add("USERID");
                dtInData.Columns.Add("PRODID");
                dtInData.Columns.Add("PRDT_GRD_CODE");
                dtInData.Columns.Add("PKG_LOTID");
                dtInData.Columns.Add("PGM_ID");    //라벨 이력 저장용
                dtInData.Columns.Add("BZRULE_ID"); //라벨 이력 저장용

                DataRow dr = dtInData.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERID"] = LoginInfo.USERID;
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");
                dr["PRODID"] = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["PRODID"].Index).Value);
                dr["PRDT_GRD_CODE"] = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["PRDT_GRD_CODE"].Index).Value);
                dr["PKG_LOTID"] = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["PKG_LOTID"].Index).Value);
                dr["PGM_ID"] = _sPGM_ID;
                dr["BZRULE_ID"] = sBizRule;

                dtInData.Rows.Add(dr);

                DataTable dtInbox = ds.Tables.Add("INBOX");
                dtInbox.Columns.Add("BOXID");

                foreach (int idxBox in idxBoxList)
                {
                    string boxID = Util.NVC(dgInbox.GetCell(idxBox, dgInbox.Columns["BOXID"].Index).Value);
                  
                    dr = dtInbox.NewRow();
                    dr["BOXID"] = boxID;
                    dtInbox.Rows.Add(dr);
                }              

                DataTable dtInPrint = ds.Tables.Add("INPRINT");
                dtInPrint.Columns.Add("PRMK");
                dtInPrint.Columns.Add("RESO");
                dtInPrint.Columns.Add("PRCN");
                dtInPrint.Columns.Add("MARH");
                dtInPrint.Columns.Add("MARV");
                dtInPrint.Columns.Add("DARK");
                dr = dtInPrint.NewRow();
                dr["PRMK"] = _sPrt;
                dr["RESO"] = _sRes;
                dr["PRCN"] = _sCopy;
                dr["MARH"] = _sXpos;
                dr["MARV"] = _sYpos;
                dr["DARK"] = _sDark;
                dtInPrint.Rows.Add(dr);

                //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_OUTBOX_REPRT_NJ_CIRCULAR", "INDATA,INBOX,INPRINT", "OUTDATA", ds);
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizRule, "INDATA,INBOX,INPRINT", "OUTDATA", ds);

                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    DataTable dtResult = dsResult.Tables["OUTDATA"];
                    string zplCode = string.Empty;
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        zplCode += dtResult.Rows[i]["ZPLCODE"].ToString();
                    }
                    PrintLabel(zplCode, _drPrtInfo);
                }
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }

        }

        private void btnUpdatePallet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                string boxStat = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXSTAT"].Index).Value);

                if (boxStat.Equals("PACKED"))
                {
                    //SFU4296	포장 완료된 팔레트입니다. 수정 불가합니다.
                    Util.MessageValidation("SFU4296");
                    return;
                }

                // SFU4007 Pallet를 수정 하시겠습니까?	
                Util.MessageConfirm("SFU4007", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sPalletId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);
                        string textRange = new TextRange(txtNote_DETL.Document.ContentStart, txtNote_DETL.Document.ContentEnd).Text;
                        DataSet indataSet = new DataSet();
                        DataTable inDataTable = indataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("LANGID");
                        inDataTable.Columns.Add("USERID");

                        DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                        inBoxTable.Columns.Add("BOXID");
                        inBoxTable.Columns.Add("WIPQTY");
                        inBoxTable.Columns.Add("EQPTID");
                        inBoxTable.Columns.Add("PROCID");
                        inBoxTable.Columns.Add("SOC_VALUE");
                        inBoxTable.Columns.Add("EXP_DOM_TYPE_CODE");
                        inBoxTable.Columns.Add("PACK_NOTE");

                        DataRow newRow = inDataTable.NewRow();
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["USERID"] = txtWorker_Main.Tag;
                        inDataTable.Rows.Add(newRow);

                        newRow = inBoxTable.NewRow();
                        newRow["BOXID"] = sPalletId;
                        //newRow["WIPQTY"] = txtInputQty.Value;
                        newRow["EQPTID"] = cboEquipment.SelectedValue;
                       // newRow["PROCID"] = cboProcType.SelectedValue;
                       // newRow["SOC_VALUE"] = txtSoc.Text;
                       // newRow["EXP_DOM_TYPE_CODE"] = cboExpDomType.SelectedValue;
                        newRow["PACK_NOTE"] = textRange.LastIndexOf(System.Environment.NewLine) < 0 ? textRange : textRange.Substring(0, textRange.LastIndexOf(System.Environment.NewLine));
                        inBoxTable.Rows.Add(newRow);

                        loadingIndicator.Visibility = Visibility.Visible;
                        new ClientProxy().ExecuteService_Multi("BR_PRD_UPD_INPALLET_DETAIL_NJ", "INDATA,INBOX", "OUTDATA,OUTINBOX,OUTCTNR", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageExceptionNoEnter(bizException, msgResult =>
                                    {
                                        if (msgResult == MessageBoxResult.OK)
                                        {
                                            txtBoxId.Focus();
                                            txtBoxId.Text = string.Empty;
                                        }
                                    });
                                    return;
                                }

                                GetPalletList();
                                _util.SetDataGridCheck(dgInPallet, "CHK", "BOXID", sPalletId, true);
                                SetDetailInfo();

                                //SFU1265 수정되었습니다.	
                                Util.MessageInfo("SFU1265");
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }

                        }, indataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {

            }
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            BOX001_202_INFO popup = new BOX001_202_INFO();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[1];

                Parameters[0] = txtWorker_Main.Tag; // 작업자id


                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puInfo_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }
        private void puInfo_Closed(object sender, EventArgs e)
        {
            BOX001_202_INFO popup = sender as BOX001_202_INFO;

            this.grdMain.Children.Remove(popup);
        }

        #region [Cell 교체 팝업 호출]
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                //SFU1843	작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return;
            }

            if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //SFU1223 라인을 선택 하세요
                Util.MessageValidation("SFU1223");
                return;
            }

            BOX001_240_CHANGE_CELL puChange = new BOX001_240_CHANGE_CELL();
            puChange.FrameOperation = FrameOperation;

            if (puChange != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = Util.NVC(cboLine.SelectedValue);
                Parameters[1] = txtWorker_Main.Tag;
                Parameters[2] = "BOX001_202";
                C1WindowExtension.SetParameters(puChange, Parameters);

                puChange.Closed += new EventHandler(puChange_Closed);

                grdMain.Children.Add(puChange);
                puChange.BringToFront();

            }
        }

        private void puChange_Closed(object sender, EventArgs e)
        {
            BOX001_240_CHANGE_CELL popup = sender as BOX001_240_CHANGE_CELL;

            if (popup.DialogResult == MessageBoxResult.OK)
            {
                GetPalletList();
            }
            this.grdMain.Children.Remove(popup);
        }
        # endregion

        #region [포장해체 팝업 호출]
        private void btnUnpackBox_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                // 작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return;
            }

            if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //SFU1223 라인을 선택 하세요
                Util.MessageValidation("SFU1223");
                return;
            }

            BOX001_240_UNPACK popup = new BOX001_240_UNPACK();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[2];

                Parameters[0] = txtWorker_Main.Tag; // 작업자id
                Parameters[1] = _sPGM_ID; // 1차 포장 구성(원/각형)


                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puUnpackBox_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }

        private void puUnpackBox_Closed(object sender, EventArgs e)
        {
            BOX001_240_UNPACK popup = sender as BOX001_240_UNPACK;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            }
            this.grdMain.Children.Remove(popup);
        }
        #endregion
    }
}
