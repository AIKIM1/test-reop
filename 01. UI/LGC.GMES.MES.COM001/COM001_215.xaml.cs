/*************************************************************************************
 Created Date : 2017.01.30
      Creator : 오화백K
   Decription : 활성화 INBOX 재구성
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.04  오화백K : Initial Created.
   
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
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using LGC.GMES.MES.CMM001.Extensions;

using Application = System.Windows.Application;

namespace LGC.GMES.MES.COM001
{

    public partial class COM001_215 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        private readonly BizDataSet _bizDataSet = new BizDataSet();

        private CheckBoxHeaderType _inBoxHeaderType1;
        private CheckBoxHeaderType _inBoxHeaderType2;

        DataTable _Cart;
        DataTable _AssyLot;
        DataTable _MixLotProd;
        DataTable _Inbox;
        DataTable _InboxCreate;

        private string _CreateInboxList = string.Empty;

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
        }

        public COM001_215()
        {
            InitializeComponent();
            InitCombo();
            this.Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            // 동별 Inbox 유형 콤보
            SetAreaInboxType();

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동 AREA
            C1ComboBox[] cboAreaChild = { cboProcessHistory };
            _combo.SetCombo(cboAreaHistory, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sCase: "AREA");
           
            //공정
            C1ComboBox[] cboLineParent = { cboAreaHistory };
             _combo.SetCombo(cboProcessHistory, CommonCombo.ComboStatus.SELECT, sCase: "POLYMER_PROCESS", cbParent: cboLineParent);

        }

        private void InitControlClear(string Select)
        {
            if (Select.Equals("ALL"))
            {
                txtCart.Text = string.Empty;
                rdoCart.IsChecked = true;
                rdoAssyLot.IsChecked = true;
                Util.gridClear(dgCart);
                _Cart.Clear();
            }
            else if (Select.Equals("CART"))
            {
                txtCart.Text = string.Empty;
                Util.gridClear(dgCart);
                _Cart.Clear();
            }
            else if (Select.Equals("ASSYLOT"))
            {
            }

            if (Select.Equals("INBOX"))
            {
                txtAssyLot.Text = string.Empty;
            }

            txtInbox.Text = string.Empty;
            cboInboxType.SelectedValueChanged -= cboInboxType_SelectedValueChanged;
            cboInboxType.SelectedIndex = 0;
            cboInboxType.SelectedValueChanged += cboInboxType_SelectedValueChanged;

            Util.gridClear(dgAssyLot);
            Util.gridClear(dgInbox);
            Util.gridClear(dgInboxCreate);

            _AssyLot.Clear();
            _MixLotProd = new DataTable();
            _Inbox.Clear();
            _InboxCreate.Clear();

            _inBoxHeaderType1 = CheckBoxHeaderType.Two;
            _inBoxHeaderType2 = CheckBoxHeaderType.Two;

            _CreateInboxList = string.Empty;
        }

        private void SetControlReadOnly(bool IReadOnly)
        {
            if (IReadOnly)
            {
                // 병합후
                rdoCart.IsEnabled = false;
                rdoNewCart.IsEnabled = false;
                rdoAssyLot.IsEnabled = false;
                rdoMixAssyLot.IsEnabled = false;

                txtCart.IsEnabled = false;
                txtAssyLot.IsEnabled = false;
                txtInbox.IsEnabled = false;
                cboInboxType.IsEnabled = false;

                btnInboxDelete.IsEnabled = false;
                btnMerge.IsEnabled = false;

                btnPrintCart.IsEnabled = true;
                btnPrintTag.IsEnabled = true;

                dgInbox.IsReadOnly = true;
            }
            else
            {
                // 초기화
                rdoCart.IsEnabled = true;
                rdoNewCart.IsEnabled = true;
                rdoAssyLot.IsEnabled = true;
                rdoMixAssyLot.IsEnabled = true;

                txtCart.IsEnabled = true;
                txtAssyLot.IsEnabled = false;
                txtInbox.IsEnabled = true;
                cboInboxType.IsEnabled = true;

                btnInboxDelete.IsEnabled = true;
                btnMerge.IsEnabled = true;

                btnPrintCart.IsEnabled = false;
                btnPrintTag.IsEnabled = false;

                dgInbox.IsReadOnly = false;
            }

        }

        private void InitDataTable()
        {
            _Cart = new DataTable();
            _Cart.Columns.Add("CTNR_ID", typeof(string));
            _Cart.Columns.Add("PRJT_NAME", typeof(string));
            _Cart.Columns.Add("PRODID", typeof(string));
            _Cart.Columns.Add("MODLID", typeof(string));
            _Cart.Columns.Add("MKT_TYPE_CODE", typeof(string));
            _Cart.Columns.Add("MKT_TYPE_NAME", typeof(string));
            _Cart.Columns.Add("PROCID", typeof(string));
            _Cart.Columns.Add("PROCNAME", typeof(string));

            _AssyLot = new DataTable();
            _AssyLot.Columns.Add("LOTID_RT", typeof(string));
            _AssyLot.Columns.Add("LOTTYPE", typeof(string));
            _AssyLot.Columns.Add("LOTYNAME", typeof(string));
            _AssyLot.Columns.Add("CAPA_GRD_CODE", typeof(string));
            _AssyLot.Columns.Add("INBOX_TYPE_CODE", typeof(string));
            _AssyLot.Columns.Add("INBOX_TYPE_NAME", typeof(string));
            _AssyLot.Columns.Add("INBOX_QTY", typeof(decimal));
            _AssyLot.Columns.Add("CELL_QTY", typeof(decimal));
            _AssyLot.Columns.Add("INBOX_LOAD_QTY", typeof(decimal));
            _AssyLot.Columns.Add("MIXLOT_YN", typeof(string));

            _Inbox = new DataTable();
            _Inbox.Columns.Add("CHK", typeof(Boolean));
            _Inbox.Columns.Add("CTNR_ID", typeof(string));
            _Inbox.Columns.Add("LOTID_RT", typeof(string));
            _Inbox.Columns.Add("CAPA_GRD_CODE", typeof(string));
            _Inbox.Columns.Add("LOTID", typeof(string));
            _Inbox.Columns.Add("WIPSEQ", typeof(string));
            _Inbox.Columns.Add("CELL_QTY", typeof(decimal));
            _Inbox.Columns.Add("LOTTYPE", typeof(string));
            _Inbox.Columns.Add("LOTYNAME", typeof(string));
            _Inbox.Columns.Add("PRODID", typeof(string));
            _Inbox.Columns.Add("PROCID", typeof(string));
            _Inbox.Columns.Add("MKT_TYPE_CODE", typeof(string));
            _Inbox.Columns.Add("INBOX_TYPE_CODE", typeof(string));
            _Inbox.Columns.Add("INBOX_TYPE_NAME", typeof(string));
            _Inbox.Columns.Add("INBOX_LOAD_QTY", typeof(decimal));

            _InboxCreate = new DataTable();
            _InboxCreate.Columns.Add("CHK", typeof(Boolean));
            _InboxCreate.Columns.Add("SEQ", typeof(int));
            _InboxCreate.Columns.Add("LOTID", typeof(string));
            _InboxCreate.Columns.Add("WIPSEQ", typeof(string));
            _InboxCreate.Columns.Add("CAPA_GRD_CODE", typeof(string));
            _InboxCreate.Columns.Add("CELL_QTY", typeof(decimal));
            _InboxCreate.Columns.Add("PRINT_YN", typeof(string));
            _InboxCreate.Columns.Add("WIPDTTM_ST", typeof(string));

        }

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnMerge);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            this.Loaded -= UserControl_Loaded;

            InitDataTable();
            InitControlClear("ALL");
            SetControlReadOnly(false);
        }

        #region Inbox 병합

        /// <summary>
        /// 대차 선택
        /// </summary>
        private void rdoCart_Checked(object sender, RoutedEventArgs e)
        {
            if (txtCart == null) return;

            InitControlClear("CART");
            SetCartInfo(false);

            txtCart.IsEnabled = true;
            txtCart.Focus();
        }

        private void rdoNewCart_Checked(object sender, RoutedEventArgs e)
        {
            if (txtCart == null) return;

            InitControlClear("CART");
            SetCartInfo(true);

            txtCart.IsEnabled = false;
        }

        private void txtCart_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtCart_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // 대차 정보 검색
                InitControlClear("ASSYLOT");
                SetCartInfo(false);
            }
        }

        private void rdoAssyLot_Checked(object sender, RoutedEventArgs e)
        {
            if (txtAssyLot == null) return;

            InitControlClear("ASSYLOT");

            txtAssyLot.IsEnabled = false;
            txtInbox.Focus();
        }

        /// <summary>
        /// 조립LOT 선택
        /// </summary>
        private void rdoMixAssyLot_Checked(object sender, RoutedEventArgs e)
        {
            if (txtAssyLot == null) return;

            InitControlClear("ASSYLOT");

            txtAssyLot.IsEnabled = true;
            txtAssyLot.Focus();
        }

        private void txtAssyLot_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtAssyLot_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ValidationMixAssyLot())
                {
                    //// Mix 조립LOT 정보 검색
                    //SetAssyLot();
                }
            }
        }

        /// <summary>
        /// Inbox Scan
        /// </summary>
        private void txtInbox_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtInbox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ValidationScanInbox())
                {
                    // Inbox 체크 및 생성
                    cboInboxType.SelectedValueChanged -= cboInboxType_SelectedValueChanged;
                    SetInboxInfo();
                    cboInboxType.SelectedValueChanged += cboInboxType_SelectedValueChanged;
                }
            }
        }

        /// <summary>
        /// inbox 유형 변경
        /// </summary>
        private void cboInboxType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (_AssyLot.Rows.Count == 0) return;

            if (cboInboxType.SelectedValue == null || cboInboxType.SelectedValue.ToString().Equals("SELECT"))
            {
                _AssyLot.Rows[0]["INBOX_TYPE_CODE"] = string.Empty;
                _AssyLot.Rows[0]["INBOX_TYPE_NAME"] = string.Empty;
                _AssyLot.Rows[0]["INBOX_LOAD_QTY"] = 0;
                _AssyLot.AcceptChanges();
                Util.GridSetData(dgAssyLot, _AssyLot, null, false);
            }
            else
            {
                DataTable dtInboxType = DataTableConverter.Convert(cboInboxType.ItemsSource);

                _AssyLot.Rows[0]["INBOX_TYPE_CODE"] = dtInboxType.Rows[cboInboxType.SelectedIndex]["CBO_CODE"];
                _AssyLot.Rows[0]["INBOX_TYPE_NAME"] = dtInboxType.Rows[cboInboxType.SelectedIndex]["CBO_NAME"];
                _AssyLot.Rows[0]["INBOX_LOAD_QTY"] = dtInboxType.Rows[cboInboxType.SelectedIndex]["INBOX_LOAD_QTY"];
                _AssyLot.AcceptChanges();
                Util.GridSetData(dgAssyLot, _AssyLot, null, false);

                CreateInbox();
            }

        }

        /// <summary>
        /// Scan Inbox 삭제
        /// </summary>
        private void btnInboxDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ValidationDeleteInbox())
            {
                // Scan Inbox 삭제
                DeleteInbox();
            }
        }

        /// <summary>
        /// Inbox Grid Check All Event
        /// </summary>
        private void tbCheckHeaderAll1_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgInbox;
            if (dg?.ItemsSource == null) return;

            foreach (DataGridRow row in dg.Rows)
            {
                switch (_inBoxHeaderType1)
                {
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.Two:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_inBoxHeaderType1)
            {
                case CheckBoxHeaderType.One:
                    _inBoxHeaderType1 = CheckBoxHeaderType.Two;
                    break;
                case CheckBoxHeaderType.Two:
                    _inBoxHeaderType1 = CheckBoxHeaderType.One;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void tbCheckHeaderAll2_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgInboxCreate;
            if (dg?.ItemsSource == null) return;

            foreach (DataGridRow row in dg.Rows)
            {
                switch (_inBoxHeaderType2)
                {
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.Two:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_inBoxHeaderType2)
            {
                case CheckBoxHeaderType.One:
                    _inBoxHeaderType2 = CheckBoxHeaderType.Two;
                    break;
                case CheckBoxHeaderType.Two:
                    _inBoxHeaderType2 = CheckBoxHeaderType.One;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void dgInboxCreate_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8");

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("Y"))
                    {
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void ddgInboxCreate_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        /// <summary>
        /// 대차 Sheet 발행
        /// </summary>
        private void btnPrintCart_Click(object sender, RoutedEventArgs e)
        {
            //dgCart
            COM001_215_SHEET_PRINT popupTagPrint = new COM001_215_SHEET_PRINT();
            popupTagPrint.FrameOperation = this.FrameOperation;
            object[] parameters = new object[2];
            parameters[0] = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "CTNR_ID"));
            parameters[1] = DataTableConverter.Convert(dgInbox.ItemsSource);

            C1WindowExtension.SetParameters(popupTagPrint, parameters);
            popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);
                     
            grdMain.Children.Add(popupTagPrint);
            popupTagPrint.BringToFront();

        }
        private void popupTagPrint_Closed(object sender, EventArgs e)
        {
            COM001_215_SHEET_PRINT popup = sender as COM001_215_SHEET_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
              
            }

            this.grdMain.Children.Remove(popup);

        }
        /// <summary>
        /// 양품태그발행
        /// </summary>
        private void btnPrintTag_Click(object sender, RoutedEventArgs e)
        {
            if (ValidationPrintTag())
            {
                PrintLabel();
            }
        }

        /// <summary>
        /// Inbox 병합
        /// </summary>
        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            if (ValidationMergeInbox())
            {
                InboxMerge();
            }
        }

        /// <summary>
        /// 초기화 
        /// </summary>
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            InitControlClear("ALL");
            SetControlReadOnly(false);
        }

        #endregion


        #endregion

        #region Mehod

        #region [BizCall]

        #region >>> Inbox 병합

        /// <summary>
        /// Inbox 유형 조회
        /// </summary>
        private void SetAreaInboxType()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREA_INBOX_TYPE_PC", "INDATA", "OUTDATA", inTable);
                DataRow dr = dtResult.NewRow();
                dr[cboInboxType.SelectedValuePath] = "SELECT";
                dr[cboInboxType.DisplayMemberPath] = "-SELECT-";
                dtResult.Rows.InsertAt(dr, 0);

                cboInboxType.ItemsSource = dtResult.Copy().AsDataView();
                cboInboxType.SelectedIndex = 0;
                cboInboxType.SelectedValueChanged += cboInboxType_SelectedValueChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 대차 정보 조회
        /// </summary>
        private void SetCartInfo(bool IsNew)
        {
            try
            {
                _Cart.Clear();
                Util.gridClear(dgCart);

                if (IsNew)
                {
                    DataRow dr = _Cart.NewRow();
                    dr["CTNR_ID"] = "NEW";
                    _Cart.Rows.Add(dr);

                    Util.GridSetData(dgCart, _Cart, null, false);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(txtCart.Text)) return;

                    DataTable inTable = new DataTable();
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("CTNR_ID", typeof(string));

                    DataRow newRow = inTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["CTNR_ID"] = txtCart.Text;
                    inTable.Rows.Add(newRow);

                    ShowLoadingIndicator();

                    new ClientProxy().ExecuteService("DA_PRD_SEL_MERGE_CART_PC", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                    {
                        try
                        {
                            HiddenLoadingIndicator();

                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            _Cart = bizResult;
                            Util.GridSetData(dgCart, _Cart, null, false);

                            if (_Cart.Rows.Count == 0)
                            {
                                // 대차 정보가 없습니다.
                                Util.MessageValidation("SFU4365");
                            }
                        }
                        catch (Exception ex)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(ex);
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Mix Lot 제품 정보 조회
        /// </summary>
        private bool SetMixLotProdInfo(string procid)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = txtAssyLot.Text;
                inTable.Rows.Add(newRow);

                _MixLotProd = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROD_MDLLOT_PC", "INDATA", "OUTDATA", inTable);

                if (_MixLotProd == null || _MixLotProd.Rows.Count == 0)
                {
                    // 입력한 Mix Lot의 모델 LOT 정보가 없습니다.
                    Util.MessageValidation("SFU4511");
                    return false;
                }

                DataTable inTableBOM = new DataTable();
                inTableBOM.Columns.Add("PROCID", typeof(string));
                inTableBOM.Columns.Add("PRODID", typeof(string));

                DataRow newRowBOM = inTableBOM.NewRow();
                newRowBOM["PROCID"] = procid;
                newRowBOM["PRODID"] = Util.NVC(_MixLotProd.Rows[0]["CBO_CODE"]);
                inTableBOM.Rows.Add(newRowBOM);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROD_MDLLOT_BOM_PC", "INDATA", "OUTDATA", inTableBOM);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    inTableBOM.Merge(dtResult);
                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        /// <summary>
        /// Inbox 정보 조회
        /// </summary>
        private void SetInboxInfo()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = txtInbox.Text;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_MERGE_INBOX_PC", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult != null && bizResult.Rows.Count > 0)
                        {
                            if (CheckInbox(bizResult.Rows[0]))
                            {
                                // 생성 Inbox
                                CreateInbox();
                            }
                        }
                        else
                        {
                            // Inbox정보가 없습니다.
                            Util.MessageValidation("SFU4467");
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Inbox 병합
        /// </summary>
        private void InboxMerge()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA Table
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("PRCS_USERID", typeof(string));
                inTable.Columns.Add("LOTTYPE", typeof(string));
                inTable.Columns.Add("ASSY_LOTID", typeof(string));
                inTable.Columns.Add("CAPA_GRD_CODE", typeof(string));
                inTable.Columns.Add("MKT_TYPE_CODE", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("INBOX_TYPE_QTY", typeof(decimal));
                inTable.Columns.Add("INBOX_TYPE_CODE", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                //inTable.Columns.Add("CTNR_TYPE_CODE", typeof(string));

                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("ACTQTY", typeof(decimal));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Util.NVC(_Cart.Rows[0]["PROCID"]);
                newRow["PRODID"] = Util.NVC(_Cart.Rows[0]["PRODID"]);
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PRCS_USERID"] = LoginInfo.USERID;
                newRow["LOTTYPE"] = Util.NVC(_AssyLot.Rows[0]["LOTTYPE"]);
                newRow["ASSY_LOTID"] = Util.NVC(_AssyLot.Rows[0]["LOTID_RT"]);
                newRow["CAPA_GRD_CODE"] = Util.NVC(_AssyLot.Rows[0]["CAPA_GRD_CODE"]);
                newRow["MKT_TYPE_CODE"] = Util.NVC(_Cart.Rows[0]["MKT_TYPE_CODE"]);
                newRow["INBOX_TYPE_QTY"] = Util.NVC(_AssyLot.Rows[0]["INBOX_LOAD_QTY"]);
                newRow["INBOX_TYPE_CODE"] = Util.NVC(_AssyLot.Rows[0]["INBOX_TYPE_CODE"]);
                newRow["CTNR_ID"] = Util.NVC(_Cart.Rows[0]["CTNR_ID"]).Equals("NEW") ? null : Util.NVC(_Cart.Rows[0]["CTNR_ID"]);
                //newRow["CTNR_TYPE_CODE"] ="CART";
                inTable.Rows.Add(newRow);

                foreach (DataRow dr in _Inbox.Rows)
                {
                    newRow = inLot.NewRow();
                    newRow["LOTID"] = dr["LOTID"].ToString();
                    newRow["ACTQTY"] = dr["CELL_QTY"].ToString();
                    inLot.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_COMPOSE_MIX_LOT", "INDATA,INLOT", "OUTLOT,OUTCTNR", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        SetControlReadOnly(true);

                        // 신규 대차 대차 ID표시
                        if (bizResult.Tables["OUTCTNR"].Rows.Count > 0)
                        {
                            if (Util.NVC(_Cart.Rows[0]["CTNR_ID"]).Equals("NEW"))
                            {
                                _Cart.Rows[0]["CTNR_ID"] = Util.NVC(bizResult.Tables["OUTCTNR"].Rows[0]["CTNR_ID"]);

                            }
                        }

                        // 병합 Inbox List 조회
                        if (bizResult.Tables["OUTLOT"].Rows.Count > 0)
                        {
                            _CreateInboxList = string.Empty;
                            SetMergeInboxInfo(bizResult.Tables["OUTLOT"]);
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 병합 Inbox 정보 조회
        /// </summary>
        private void SetMergeInboxInfo(DataTable dt)
        {
            try
            {
                if (dt != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        _CreateInboxList += Util.NVC(dr["LOTID"]) + ",";
                    }
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = _CreateInboxList.Substring(0, _CreateInboxList.Length - 1);
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_MERGE_INBOX_PC", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgInboxCreate, bizResult, null, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        #region [Validation]

        #region >>> Inbox 병합

        private bool ValidationMixAssyLot()
        {
            if (dgCart.Rows.Count <= 0)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            return true;
        }

        private bool ValidationScanInbox()
        {
            if (dgCart.Rows.Count <= 0)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            if ((bool)rdoMixAssyLot.IsChecked)
            {
                if (txtAssyLot.Text.Length != 8)
                {
                    // 조립LOT 정보는 8자리 입니다.
                    Util.MessageValidation("SFU4228");
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(txtInbox.Text))
            {
                // Inbox ID를 입력 하세요.
                Util.MessageValidation("SFU4517");
                return false;
            }

            if (_AssyLot != null && _AssyLot.Rows.Count > 0)
            {
                DataRow[] dr = _Inbox.Select("LOTID ='" + txtInbox.Text + "'");

                if (dr.Length > 0)
                {
                    // 이미 스캔한 Inbox 입니다.
                    Util.MessageValidation("SFU4512");
                    return false;
                }
            }

            return true;
        }

        private bool ValidationDeleteInbox()
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgInbox, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationMergeInbox()
        {
            if (_Cart == null || _Cart.Rows.Count == 0)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            if (_AssyLot == null || _AssyLot.Rows.Count == 0)
            {
                // 조립 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4001");
                return false;
            }

            if (_Inbox == null || _Inbox.Rows.Count == 0)
            {
                // Inbox정보가 없습니다.
                Util.MessageValidation("SFU4467");
                return false;
            }

            if (_InboxCreate == null || _InboxCreate.Rows.Count == 0)
            {
                // Inbox정보가 없습니다.
                Util.MessageValidation("SFU4467");
                return false;
            }

            return true;
        }

        private bool ValidationPrintTag()
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgInboxCreate, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }
        #endregion

        #endregion

        #region [Func]

        #region >>> Inbox 병합
        /// <summary>
        /// Mix Lot 제품 정보 조회
        /// </summary>
        private void SetAssyLot()
        {
            _AssyLot.Clear();
            DataRow dr = _AssyLot.NewRow();
            dr["LOTID_RT"] = txtAssyLot.Text;
            dr["MIXLOT_YN"] = "Y";
            _AssyLot.Rows.Add(dr);
            _AssyLot.AcceptChanges();

            Util.GridSetData(dgAssyLot, _AssyLot, null, false);
        }

        private bool CheckInbox(DataRow drInbox)
        {
            // Mix Lot인 경우 제품 검색  Inbox Scan 처음인 경우만
            if ((bool) rdoMixAssyLot.IsChecked && (_MixLotProd == null || _MixLotProd.Rows.Count == 0))
            {
                if (SetMixLotProdInfo(Util.NVC(drInbox["PROCID"])))
                {
                    if (Util.NVC(_Cart.Rows[0]["CTNR_ID"]).Equals("NEW"))
                    {
                        // 신규대차인 경우 Inbox의 제품 비교
                        DataRow[] dr = _MixLotProd.Select("CBO_CODE = '" + Util.NVC(drInbox["PRODID"]) + "'");
                        if (dr.Length == 0)
                        {
                            // 제품코드가 다릅니다.
                            Util.MessageValidation("SFU1897");
                            return false;
                        }
                    }
                    else
                    {
                        // 기존대차인 경우 대차의 제품 비교
                        DataRow[] dr = _MixLotProd.Select("CBO_CODE = '" + Util.NVC(_Cart.Rows[0]["PRODID"]) + "'");
                        if (dr.Length == 0)
                        {
                            // 제품코드가 다릅니다.
                            Util.MessageValidation("SFU1897");
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(Util.NVC(drInbox["INBOX_TYPE_CODE"])))
            {
                if (cboInboxType.SelectedValue == null || cboInboxType.SelectedValue.ToString().Equals("SELECT"))
                {
                    // InBox 유형을 선택해 주세요.
                    Util.MessageValidation("SFU4005");
                    return false;
                }
            }

            //if (string.IsNullOrWhiteSpace(Util.NVC(drInbox["INBOX_LOAD_QTY"])) || Util.NVC(drInbox["INBOX_LOAD_QTY"]).Equals("0"))
            //{
            //    // Inbox 유형에 Cell 적재수량이 없습니다.
            //    Util.MessageValidation("SFU4515");
            //    return false;
            //}

            // 대차
            if (Util.NVC(_Cart.Rows[0]["CTNR_ID"]).Equals("NEW") && string.IsNullOrWhiteSpace(Util.NVC(_Cart.Rows[0]["PRJT_NAME"])))
            {
                // 신규 대차
                _Cart.Rows[0]["PRJT_NAME"] = Util.NVC(drInbox["PRJT_NAME"]);
                _Cart.Rows[0]["PRODID"] = Util.NVC(drInbox["PRODID"]);
                _Cart.Rows[0]["MODLID"] = Util.NVC(drInbox["MODLID"]);
                _Cart.Rows[0]["MKT_TYPE_CODE"] = Util.NVC(drInbox["MKT_TYPE_CODE"]);
                _Cart.Rows[0]["MKT_TYPE_NAME"] = Util.NVC(drInbox["MKT_TYPE_NAME"]);
                _Cart.Rows[0]["PROCID"] = Util.NVC(drInbox["PROCID"]);
                _Cart.Rows[0]["PROCNAME"] = Util.NVC(drInbox["PROCNAME"]);
                _Cart.AcceptChanges();

                Util.GridSetData(dgCart, _Cart, null, false);
            }

            // 기존 조립LOT 사용
            if (_AssyLot.Rows.Count == 0)
            {
                DataRow dr = _AssyLot.NewRow();
                dr["LOTID_RT"] = (bool)rdoMixAssyLot.IsChecked ? txtAssyLot.Text : Util.NVC(drInbox["LOTID_RT"]);
                dr["LOTTYPE"] = Util.NVC(drInbox["LOTTYPE"]);
                dr["LOTYNAME"] = Util.NVC(drInbox["LOTYNAME"]);
                dr["CAPA_GRD_CODE"] = Util.NVC(drInbox["CAPA_GRD_CODE"]);
                //dr["INBOX_TYPE_CODE"] = Util.NVC(drInbox["INBOX_TYPE_CODE"]);
                //dr["INBOX_TYPE_NAME"] = Util.NVC(drInbox["INBOX_TYPE_NAME"]);
                dr["INBOX_QTY"] = 0;
                dr["CELL_QTY"] = Util.NVC(drInbox["CELL_QTY"]);
                //dr["INBOX_LOAD_QTY"] = Util.NVC(drInbox["INBOX_LOAD_QTY"]);
                dr["MIXLOT_YN"] = (bool)rdoMixAssyLot.IsChecked ?  "Y" : "N";
                _AssyLot.Rows.Add(dr);
                _AssyLot.AcceptChanges();

                Util.GridSetData(dgAssyLot, _AssyLot, null, false);

                // Inbox 유형을 넣어 준다.
                if ((cboInboxType.SelectedValue == null || cboInboxType.SelectedValue.ToString().Equals("SELECT")) &&
                    !Util.NVC(drInbox["INBOX_TYPE_CODE"]).Equals(""))
                {
                    cboInboxType.SelectedValue = Util.NVC(drInbox["INBOX_TYPE_CODE"]);
                }

                DataTable dtInboxType = DataTableConverter.Convert(cboInboxType.ItemsSource);

                dr["INBOX_TYPE_CODE"] = dtInboxType.Rows[cboInboxType.SelectedIndex]["CBO_CODE"];
                dr["INBOX_TYPE_NAME"] = dtInboxType.Rows[cboInboxType.SelectedIndex]["CBO_NAME"];
                dr["INBOX_LOAD_QTY"] = dtInboxType.Rows[cboInboxType.SelectedIndex]["INBOX_LOAD_QTY"];
            }

            // Scan 자료 체크
            if (_Inbox != null || _Inbox.Rows.Count > 0)
            {
                if (Util.NVC(_Cart.Rows[0]["PRODID"]) != Util.NVC(drInbox["PRODID"]))
                {
                    // 제품코드가 다릅니다.
                    Util.MessageValidation("SFU1897");
                    return false;
                }

                if (Util.NVC(_Cart.Rows[0]["MKT_TYPE_CODE"]) != Util.NVC(drInbox["MKT_TYPE_CODE"]))
                {
                    // 동일한 시장유형이 아닙니다.
                    Util.MessageValidation("SFU4271");
                    return false;
                }

                if (Util.NVC(_Cart.Rows[0]["PROCID"]) != Util.NVC(drInbox["PROCID"]))
                {
                    // 동일 공정이 아닙니다.
                    Util.MessageValidation("SFU3600");
                    return false;
                }

                if (Util.NVC(_AssyLot.Rows[0]["MIXLOT_YN"]).Equals("N") &&
                    Util.NVC(_AssyLot.Rows[0]["LOTID_RT"]) != Util.NVC(drInbox["LOTID_RT"]))
                {
                    // 동일한 조립LOT이 아닙니다.
                    Util.MessageValidation("SFU4056");
                    return false;
                }

                if (Util.NVC(_AssyLot.Rows[0]["LOTTYPE"]) != Util.NVC(drInbox["LOTTYPE"]))
                {
                    // 동일 LOT 유형이 아닙니다.
                    Util.MessageValidation("SFU4513");
                    return false;
                }

                if (Util.NVC(_AssyLot.Rows[0]["CAPA_GRD_CODE"]) != Util.NVC(drInbox["CAPA_GRD_CODE"]))
                {
                    // 동일한 등급이 아닙니다.
                    Util.MessageValidation("SFU4060");
                    return false;
                }

                //if (Util.NVC(_AssyLot.Rows[0]["INBOX_TYPE_CODE"]) != Util.NVC(drInbox["INBOX_TYPE_CODE"]))
                //{
                //    // 동일한 Inbox 유형이 아닙니다.
                //    Util.MessageValidation("SFU4514");
                //    return false;
                //}
            }

            DataRow[] drSelect = _Inbox.Select("LOTID = '" + Util.NVC(drInbox["LOTID"]) + "'");
            if (drSelect.Length == 0)
            {
                _Inbox.ImportRow(drInbox);
                _Inbox.AcceptChanges();
                Util.GridSetData(dgInbox, _Inbox, null, false);
            }

            return true;
        }

        private void CreateInbox()
        {
            // Scan한 Inbox의 Cell 수량으로 Inbox 생성
            decimal ScanCellQty = decimal.Parse(_Inbox.Select().AsEnumerable().Sum(r => r.Field<decimal>("CELL_QTY")).GetString());
            decimal InboxLoadQty = Util.NVC_Decimal(_AssyLot.Rows[0]["INBOX_LOAD_QTY"]);
            decimal InboxCreateQty = 0;
            decimal RemainQty = 0;

            if (InboxLoadQty == 0)
            {
                // Inbox 유형에 Cell 적재수량이 없습니다.
                Util.MessageValidation("SFU4515");
                return;
            }

            if (ScanCellQty % InboxLoadQty > 0)
            {
                InboxCreateQty = Convert.ToInt16(Math.Truncate(ScanCellQty / InboxLoadQty));
                RemainQty = ScanCellQty % InboxLoadQty;
            }
            else
            {
                InboxCreateQty = Convert.ToInt16(ScanCellQty / InboxLoadQty);
            }

            _InboxCreate.Clear();

            for (int row = 0; row < InboxCreateQty; row++)
            {
                DataRow dr = _InboxCreate.NewRow();
                dr["CHK"] = 1;
                dr["SEQ"] = (row + 1).ToString();
                dr["LOTID"] = "SEQ" + (row + 1).ToString();
                dr["CAPA_GRD_CODE"] = Util.NVC(_AssyLot.Rows[0]["CAPA_GRD_CODE"]);
                dr["CELL_QTY"] = InboxLoadQty;
                dr["PRINT_YN"] = "N";
                _InboxCreate.Rows.Add(dr);
            }

            if (RemainQty != 0)
            {
                DataRow dr = _InboxCreate.NewRow();
                dr["CHK"] = 1;
                dr["SEQ"] = (_InboxCreate.Rows.Count + 1).ToString();
                dr["LOTID"] = "SEQ" + (_InboxCreate.Rows.Count + 1).ToString();
                dr["CAPA_GRD_CODE"] = Util.NVC(_AssyLot.Rows[0]["CAPA_GRD_CODE"]);
                dr["CELL_QTY"] = RemainQty;
                dr["PRINT_YN"] = "N";
                _InboxCreate.Rows.Add(dr);
            }

            _InboxCreate.AcceptChanges();
            Util.GridSetData(dgInboxCreate, _InboxCreate, null, false);

            // 조립LOT 정보에 합계수량 표시
            _AssyLot.Rows[0]["INBOX_QTY"] = _InboxCreate.Rows.Count;
            _AssyLot.Rows[0]["CELL_QTY"] = ScanCellQty;
            _AssyLot.AcceptChanges();
            Util.GridSetData(dgAssyLot, _AssyLot, null, false);

        }

        private void DeleteInbox()
        {
            _Inbox = DataTableConverter.Convert(dgInbox.ItemsSource);
            _Inbox.Select("CHK = 1").ToList<DataRow>().ForEach(row => row.Delete());
            _Inbox.AcceptChanges();
            Util.GridSetData(dgInbox, _Inbox, null, false);

            CreateInbox();

            if (_Inbox.Rows.Count == 0)
            {
                // 전체 삭제시 Clear
                InitControlClear("INBOX");

                if ((bool)rdoNewCart.IsChecked)
                {
                    SetCartInfo(true);
                }
                else
                {
                    SetCartInfo(false);
                }
            }
        }

        private void PrintLabel()
        {
            string processName = Util.NVC(_Cart.Rows[0]["PROCNAME"]);
            string modelId = Util.NVC(_Cart.Rows[0]["MODLID"]);
            string projectName = Util.NVC(_Cart.Rows[0]["PRJT_NAME"]);
            string marketTypeName = Util.NVC(_Cart.Rows[0]["MKT_TYPE_NAME"]);
            string assyLotId = Util.NVC(_AssyLot.Rows[0]["LOTID_RT"]);
            string calDate = string.Empty;
            string shiftName = string.Empty;
            string equipmentShortName = string.Empty;
            string inspectorId = string.Empty;

            // 불량 태그(라벨) 항목
            DataTable dtLabelItem = new DataTable();
            dtLabelItem.Columns.Add("LABEL_CODE", typeof(string));  //LABEL CODE
            dtLabelItem.Columns.Add("ITEM001", typeof(string));     //PROCESSNAME
            dtLabelItem.Columns.Add("ITEM002", typeof(string));     //MODEL
            dtLabelItem.Columns.Add("ITEM003", typeof(string));     //PKGLOT
            dtLabelItem.Columns.Add("ITEM004", typeof(string));     //DEFECT
            dtLabelItem.Columns.Add("ITEM005", typeof(string));     //QTY
            dtLabelItem.Columns.Add("ITEM006", typeof(string));     //MKTTYPE
            dtLabelItem.Columns.Add("ITEM007", typeof(string));     //PRODDATE
            dtLabelItem.Columns.Add("ITEM008", typeof(string));     //EQUIPMENT
            dtLabelItem.Columns.Add("ITEM009", typeof(string));     //INSPECTOR
            dtLabelItem.Columns.Add("ITEM010", typeof(string));     //
            dtLabelItem.Columns.Add("ITEM011", typeof(string));     //

            // 양품 Tag인 경우 라벨이력 저장
            DataTable inTable = _bizDataSet.GetBR_PRD_REG_LABEL_HIST();

            foreach (DataGridRow row in dgInboxCreate.Rows)
            {
                if (row.Type == DataGridRowType.Item &&
                    (DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "True" ||
                     DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "1"))
                {
                    DataRow dr = dtLabelItem.NewRow();

                    dr["LABEL_CODE"] = "LBL0106";
                    dr["ITEM001"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAPA_GRD_CODE"));
                    dr["ITEM002"] = modelId + "(" + projectName + ") ";
                    dr["ITEM003"] = assyLotId;
                    dr["ITEM004"] = Util.NVC_Int(DataTableConverter.GetValue(row.DataItem, "CELL_QTY")).GetString();
                    dr["ITEM005"] = equipmentShortName;
                    //dr["ITEM006"] = calDate + "(" + shiftName + ")";
                    dr["ITEM006"] = DataTableConverter.GetValue(row.DataItem, "CALDATE").GetString() + "(" + DataTableConverter.GetValue(row.DataItem, "SHFT_NAME").GetString() + ")";
                    //dr["ITEM007"] = inspectorId;
                    //dr["ITEM007"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INSPECTORID"));
                    dr["ITEM007"] = null;
                    dr["ITEM008"] = marketTypeName;
                    dr["ITEM009"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOTID"));
                    dr["ITEM010"] = null;
                    dr["ITEM011"] = null;

                    // 라벨 발행 이력 저장
                    DataRow newRow = inTable.NewRow();
                    newRow["LABEL_PRT_COUNT"] = 1;                                                             // 발행 수량
                    newRow["PRT_ITEM01"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOTID"));       // Cell ID
                    newRow["PRT_ITEM02"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "WIPSEQ"));
                    newRow["PRT_ITEM04"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRINT_YN"));    // 재발행 여부
                    newRow["INSUSER"] = LoginInfo.USERID;
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOTID"));
                    inTable.Rows.Add(newRow);

                    dtLabelItem.Rows.Add(dr);
                }
            }

            string printType;
            string resolution;
            string issueCount;
            string xposition;
            string yposition;
            string darkness;
            DataRow drPrintInfo;

            if (!_Util.GetConfigPrintInfo(out printType, out resolution, out issueCount, out xposition, out yposition, out darkness, out drPrintInfo))
                return;

            bool isLabelPrintResult = Util.PrintLabelPolymerForm(FrameOperation, loadingIndicator, dtLabelItem, printType, resolution, issueCount, xposition, yposition, darkness, drPrintInfo);

            if (!isLabelPrintResult)
            {
                //라벨 발행중 문제가 발생하였습니다.
                Util.MessageValidation("SFU3243");
            }
            else
            {
                // 라벨 발행이력 저장
                new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        SetMergeInboxInfo(null);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                });
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

        #endregion

        #endregion

        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {
            SetmergeHist();
        }
        private void SetmergeHist()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));
                RQSTDT.Columns.Add("PJT_NAME", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("LOTID_RT", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CTNR_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = Util.GetCondition(cboProcessHistory, "SFU1459"); // 공정을 선택하세요
                if (dr["PROCID"].Equals(""))
                {
                    HiddenLoadingIndicator();
                    return;
                }
                //dr["PROCID"] = "F7800";
                dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist_Detail);
                dr["TO_DATE"] = Util.GetCondition(ldpDateToHist_Detail);
                dr["PJT_NAME"] = txtPjtHistory_Detail.Text.ToString();
                dr["PRODID"] = txtProdID_Detail.Text.ToString();
                dr["LOTID_RT"] = txtLotRTHistory_Detail.Text.ToString();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CTNR_ID"] = txtCTNR_IDHistory.Text.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MERGE_INBOX_HIST", "RQSTDT", "RSLTDT", RQSTDT);

                if(dtRslt.Rows.Count > 0)
                {

                    for(int i=0; i < dtRslt.Rows.Count; i++ )
                    {
                        if(dtRslt.Rows[i]["INPUT_TYPE"].ToString() == "INPUT")
                        {
                            dtRslt.Rows[i]["INPUT_TYPE"] = ObjectDic.Instance.GetObjectName("투입");
                        }
                        else
                        {
                            dtRslt.Rows[i]["INPUT_TYPE"] = ObjectDic.Instance.GetObjectName("완성");
                        }

                    }

                }



                Util.GridSetData(dgCartHistory, dtRslt, FrameOperation);
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
    }
}
