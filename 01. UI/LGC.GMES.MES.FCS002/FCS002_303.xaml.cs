/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.22  DEVELOPER : Initial Created.
  2022.05.19  장희만    : C20220509-000185 ASSY_LOT_LINE_MIX_NO 추가
  2023.03.10  이홍주    : SI               소형활성화 MES 복사
  2023.11.03  이홍주    : SI               NFF 증설
  2024.01.29  이홍주    : SI               OUTBOX 추가시 PRDT_GRD_CODE 동일 여부 확인
  2024.02.07  이홍주    : SI               미포장박스 조회 POPUP호출 기능 추가
  2024.04.02  이홍주    : SI               PALLET에 지정된 적재수량과 틀릴 경우 메시지 추가
  2024.05.22  이홍주    : SI               포장대기 LOT 조회 추가
  2025.05.15  이홍주    : SI               MES2.0 대응
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;

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
using LGC.GMES.MES.CMM001.UserControls;
using System.Linq;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_303 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        private static string PACKING = "PACKING,";
        private static string PACKED = "PACKED,";
        private static string SHIPPING = "SHIPPING,";
        private static string SHIP_WAIT = "SHIP_WAIT,";

        private string _searchStat = string.Empty;
        private bool bInit = true;
        /*컨트롤 변수 선언*/
        public UCBoxShift ucBoxShift { get; set; }
        public TextBox txtWorker_Main { get; set; }
        public TextBox txtShift_Main { get; set; }

        #region CheckBox
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

        #endregion


        string _sPrt = string.Empty;
        string _sRes = string.Empty;
        string _sCopy = string.Empty;
        string _sXpos = string.Empty;
        string _sYpos = string.Empty;
        string _sDark = string.Empty;

        DataRow _drPrtInfo = null;

        public FCS002_303()
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


            //TEST 20250501 운영시 삭제필요
            //txtWorker_Main.Text = "이홍주";
            //txtWorker_Main.Tag = "leehongju";

            //txtShift_Main.Text = "A";
            //txtShift_Main.Tag = "A";
            //txtWorker_Main.IsReadOnly = true;


        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            ////listAuth.Add(btnOutAdd);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID, "MCM,MCC,MCR,MCS", Process.CELL_BOXING, LoginInfo.CFG_AREA_ID }, sCase: "LINEBYSHOP");

            if (cboLine.Items.Count > 0)
            { cboLine.SelectedIndex = 1; }
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            /* 공용 작업조 컨트롤 초기화 */
            //ucBoxShift = grdShift.Children[0] as UCBoxShift;
            //txtWorker_Main = ucBoxShift.TextWorker;
            //txtShift_Main = ucBoxShift.TextShift;
            //ucBoxShift.ProcessCode = Process.CELL_BOXING; //작업조 팝업에 넘길 공정
            //ucBoxShift.FrameOperation = this.FrameOperation;

        }

        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
        }
        /// <summary>
        /// 하위 컨트롤 초기화
        /// </summary>
        private void SetDetailClear()
        {
            txtOutBox.Text = string.Empty;
            Util.gridClear(dgOutbox);

        }

        #endregion

        #region Events

        #region 텍스트 박스 포커스 : text_GotFocus()
        /// <summary>
        /// 텍스트 박스 포커스
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
        #endregion

        #region 완성 OUTBOX CheckAll : dgOutbox_LoadedColumnHeaderPresenter
        /// <summary>
        /// 완성 OUTBOX CheckAll
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgOutbox_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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
                            if (e.Column.HeaderPresenter != null)
                            {
                                e.Column.HeaderPresenter.Content = pre;
                            }
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


        #endregion

        #region  체크박스 선택 이벤트 : checkAll_Checked(), checkAll_Unchecked()

        /// <summary>
        /// 전체 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgOutbox.GetRowCount(); i++)
                {
                    if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgOutbox.Rows[i].DataItem, "CHK")))
                        || Util.NVC(DataTableConverter.GetValue(dgOutbox.Rows[i].DataItem, "CHK")).Equals("0")
                        || Util.NVC(DataTableConverter.GetValue(dgOutbox.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgOutbox.Rows[i].DataItem, "CHK", true);
                }
            }
        }

        /// <summary>
        /// 전체 선택 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgOutbox.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgOutbox.Rows[i].DataItem, "CHK", false);
                }
            }
        }


        #endregion

        #region 조회 : btnSearch_Click()
        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            SetDetailClear();
            GetPalletList();
            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region 콤보박스 이벤트로 조회 : cboLine_SelectedValueChanged()
        /// <summary>
        /// 라인 콤보박스로 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboLine_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                SetDetailClear();
                InitControl();
                // 라인 선택 시 자동 조회 처리
                if (cboLine != null && (cboLine.SelectedIndex > 0 && cboLine.Items.Count > cboLine.SelectedIndex))
                {
                    if (cboLine.SelectedValue.GetString() != "SELECT")
                    {
                        this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(btnSearch, null)));
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 포장중, 포장완료, 출고요청 이벤트 : chkSearch_Checked(), chkSearch_Unchecked()
        /// <summary>
        /// 체크박스 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkSearch_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            switch (chk.Name)
            {
                case "chkPacking":
                    _searchStat += PACKING;
                    break;
                case "chkPacked":
                    _searchStat += PACKED;
                    break;
                case "chkShipping":
                    _searchStat += SHIPPING;
                    break;
                case "chkShip_Wait":
                    _searchStat += SHIP_WAIT;
                    break;
                default:
                    break;
            }
            if (!bInit)
                btnSearch_Click(null, null);
            // bInit = false;
        }
        /// <summary>
        /// 체크박스 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkSearch_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            switch (chk.Name)
            {
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
                case "chkShip_Wait":
                    if (_searchStat.Contains(SHIP_WAIT))
                        _searchStat = _searchStat.Replace(SHIP_WAIT, "");
                    break;
                default:
                    break;
            }
            if (!bInit)
                btnSearch_Click(null, null);
            //  bInit = false;
        }



        #endregion

        #region 작업 Pallet 스프레드 이벤트
        /// <summary>
        /// Pallet ID 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgPalletListChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;


            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals(bool.FalseString))
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
                dgPalletList.SelectedIndex = idx;
                SetDetailClear();
                GetCompleteOutbox();
            }
        }
        #endregion

        #region OutBox 추가 : txtOutBox_KeyDown(), btnOutboxAdd_Click()
        /// <summary>
        /// Out Box 텍스트박스 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtOutBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Dispatcher.BeginInvoke(new Action(() => btnOutboxAdd_Click(btnOutboxAdd, null)));
            }
        }
        /// <summary>
        /// OutBox 추가 버튼 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOutboxAdd_Click(object sender, RoutedEventArgs e)
        {


            try
            {
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }
                if (txtOutBox.Text == string.Empty)
                {
                    //OUTBOX를 입력하세요
                    Util.MessageValidation("SFU5008");
                    txtOutBox.Focus();
                    return;
                }

                //（IM：27 bit；Non_IM：12 bit) // 2025.05.18 프로세서 적용 필요
                string sOutBox = string.Empty;
                if ((txtOutBox.Text).Trim().Length == 12)
                {
                    sOutBox = "NonIM";
                }
                else
                {
                    sOutBox = "IM";
                }

                DataSet ds = new DataSet();
                DataTable dtIndata = ds.Tables.Add("INDATA");
                dtIndata.Columns.Add("PALLETID");

                DataRow dr = null;
                dr = dtIndata.NewRow();
                dr["PALLETID"] = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                dtIndata.Rows.Add(dr);

                DataTable dtInbox = ds.Tables.Add("INOUTBOX");
                dtInbox.Columns.Add("BOXID");

                dr = dtInbox.NewRow();
                dr["BOXID"] = txtOutBox.Text;

                dtInbox.Rows.Add(dr);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_INPUT_OUTBOX_MIX_MB", "INDATA,INOUTBOX", "OUTDATA", ds);

                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {

                    DataRow[] drS = dsResult.Tables["OUTDATA"].Select();

                    //object[] param = new object[] { (int)drS[0]["TOTAL_QTY"] };

                    //OCV SPEC CHECK
                    if (!GetCompleteOutboxOcvCheck(dsResult.Tables["OUTDATA"].Rows[0]["BOXID"].ToString()))
                    {
                        return;
                    }


                    AddOutBox(dsResult.Tables["OUTDATA"].Rows[0]["BOXID"].ToString(), dsResult.Tables["OUTDATA"].Rows[0]["PROD_LINE"].ToString(), dsResult.Tables["OUTDATA"].Rows[0]["PKG_LOTID"].ToString(), dsResult.Tables["OUTDATA"].Rows[0]["PRDT_GRD_CODE"].ToString());
                }
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                Util.ShowExceptionMessages(ex);
                txtOutBox.Text = string.Empty;
            }

        }

        #endregion

        #region Pallet 생성 : btnRun_Click(), confirmPopup_Closed()
        /// <summary>
        /// Pallet 생성
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            //{
              //  //SFU1843	작업자를 입력 해 주세요.
                //Util.MessageValidation("SFU1843");
                //return;
            //}

            FCS002_303_CREATEPALLET popup = new FCS002_303_CREATEPALLET();
            popup.FrameOperation = this.FrameOperation;
            if (popup != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = LoginInfo.CFG_AREA_ID;
                Parameters[1] = LoginInfo.USERID;
                Parameters[2] = "";

                C1WindowExtension.SetParameters(popup, Parameters);
                popup.Closed += new EventHandler(puRun_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
            else
            {
                //Message: 팔레트 구성 정보가 없습니다.
            }
        }
        /// <summary>
        /// Pallet 생성 팝업 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void puRun_Closed(object sender, EventArgs e)
        {
            FCS002_303_CREATEPALLET popup = sender as FCS002_303_CREATEPALLET;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                GetPalletList();
                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", popup.PALLET_ID, true);
                GetCompleteOutbox();
            }
            this.grdMain.Children.Remove(popup);
        }

        #endregion

        #region 실적확정 : btnConfirm_Click()
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) == "PACKED")
                {
                    //SFU1235	이미 확정 되었습니다.
                    Util.MessageValidation("SFU1235");
                    return;
                }

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKING")
                {
                    //SFU2048		확정할 수 없는 상태입니다.	
                    Util.MessageValidation("SFU2048");
                    return;
                }

                //decimal inQty = (dgPalletList.GetCell(idxPallet, dgPalletList.Columns["WIPQTY"].Index).Value).SafeToDecimal();
                //decimal boxQty = (dgPalletList.GetCell(idxPallet, dgPalletList.Columns["TOTAL_QTY"].Index).Value).SafeToDecimal();
                //if (!(inQty == boxQty))
                //{
                //    // SFU4417 : 투입수량과 포장수량이 일치하지 않습니다.
                //    Util.MessageValidation("SFU4417");
                //    return;
                //}


                // SFU1716 실적확정 하시겠습니까? 
                Util.MessageConfirm("SFU1716", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);


                        //
                        //LANGID String      (*)
                        //EQSGID  String(*)라인 MCCF1, 2
                        //PROCID  String(*)공정
                        //SRCTYPE String(*)Input Type(UI or EQ)
                        //IFMODE  String  ON(*)인터페이스모드(ON: Online, OFF: Offline)
                        //BOXID   String(*)출하 PALLET ID
                        //USERID  String
                        //CONFIRM_YN  String  Y   Y: 확정, N: 취소로 판단
                        //
                        DataTable RQSTDT = new DataTable("INDATA");
                        RQSTDT.Columns.Add("LANGID");
                        RQSTDT.Columns.Add("EQSGID");
                        RQSTDT.Columns.Add("PROCID");
                        RQSTDT.Columns.Add("SRCTYPE");
                        RQSTDT.Columns.Add("IFMODE");
                        RQSTDT.Columns.Add("BOXID");
                        RQSTDT.Columns.Add("USERID");
                        RQSTDT.Columns.Add("CONFIRM_YN");

                        DataRow newRow = RQSTDT.NewRow();
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["EQSGID"] = cboLine.SelectedValue.ToString();
                        newRow["PROCID"] = Process.CELL_BOXING; // "B1000";
                        newRow["SRCTYPE"] = "UI";
                        newRow["IFMODE"] = "OFF";
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["BOXID"] = sPalletId;
                        newRow["CONFIRM_YN"] = "Y";//Y : 확정 N : 취소
          
                                                
                        RQSTDT.Rows.Add(newRow);

                        new ClientProxy().ExecuteService("BR_REG_END_SHIPMENT_UI_MB", "INDATA", null, RQSTDT, (RSLTDT, ex) =>
                        {
                            try
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                GetPalletList();
                                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                                //GetDetailInfo();

                                //   TagPrint();
                            }
                            catch (Exception ex1)
                            {
                                Util.MessageException(ex1);
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

        #endregion

        #region 실적확정 취소 : btnConfirmCancel_Click()
        private void btnConfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                //{
                //    //SFU1843	작업자를 입력 해 주세요.
                //    Util.MessageValidation("SFU1843");
                //    return;
                //}

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }
                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKED")
                {
                    //SFU4262		실적 확정후 작업 가능합니다.	
                    Util.MessageValidation("SFU4262");
                    return;
                }


                //SFU4263 실적 취소 하시겠습니까?	
                Util.MessageConfirm("SFU4263", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
                        
                        DataTable RQSTDT = new DataTable("INDATA");
                        RQSTDT.Columns.Add("LANGID");
                        RQSTDT.Columns.Add("EQSGID");
                        RQSTDT.Columns.Add("PROCID");
                        RQSTDT.Columns.Add("SRCTYPE");
                        RQSTDT.Columns.Add("IFMODE");
                        RQSTDT.Columns.Add("BOXID");
                        RQSTDT.Columns.Add("USERID");
                        RQSTDT.Columns.Add("CONFIRM_YN");

                        DataRow newRow = RQSTDT.NewRow();
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["EQSGID"] = cboLine.SelectedValue.ToString();
                        newRow["PROCID"] = Process.CELL_BOXING; // "B1000";
                        newRow["SRCTYPE"] = "UI";
                        newRow["IFMODE"] = "OFF";
                        newRow["USERID"] = LoginInfo.USERID; 
                        newRow["BOXID"] = sPalletId;
                        newRow["CONFIRM_YN"] = "N"; //Y : 확정 N : 취소


                        RQSTDT.Rows.Add(newRow);
                        //BR_PRD_REG_CANCEL_END_2ND_PLT_MB
                        new ClientProxy().ExecuteService("BR_REG_END_SHIPMENT_UI_MB", "INDATA", null, RQSTDT, (RSLTDT, ex) =>
                        {
                            try
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                GetPalletList();
                                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                                GetCompleteOutbox();

                                //정상 처리 되었습니다.
                                Util.MessageInfo("SFU1275");
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
        #endregion

        #region 출고 : btnShip_Click()
        private void btnShip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool bPalletBoxCount = true;

                //if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                //{
                //    //SFU1843	작업자를 입력 해 주세요.
                //    Util.MessageValidation("SFU1843");
                //    return;
                //}

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKED")
                {
                    //SFU4413		포장 완료된 팔레트만 출고 가능합니다.	
                    Util.MessageValidation("SFU4413");
                    return;
                }

                if (String.IsNullOrWhiteSpace(Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["SHIPTO_ID"].Index).Value)))
                {
                    //SFU4999		출하처 정보가 없습니다.
                    Util.MessageValidation("SFU4999");
                    return;
                }

                //decimal inQty = (dgPalletList.GetCell(idxPallet, dgPalletList.Columns["WIPQTY"].Index).Value).SafeToDecimal();
                //decimal boxQty = (dgPalletList.GetCell(idxPallet, dgPalletList.Columns["TOTAL_QTY"].Index).Value).SafeToDecimal();
                //if (!(inQty == boxQty))
                //{
                //    // SFU4417 : 투입수량과 포장수량이 일치하지 않습니다.
                //    Util.MessageValidation("SFU4417");
                //    return;
                //}

                #region Pallet 만박스 확인
                int iOutboxCount = dgOutbox.Rows.Count - dgOutbox.FrozenBottomRowsCount;
                //자동포장 구성에 출고시 만박스 가져오는 DA-- > 만박스 아니면 MESSAGE 띄울것.
                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("LANGID", typeof(string));
                RQSTDT1.Columns.Add("AREAID", typeof(string));
                RQSTDT1.Columns.Add("MDLLOT_ID", typeof(string));

                DataRow dr = RQSTDT1.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID.ToString();

                RQSTDT1.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MDLLOT_MULTI_PJT_CBO_MB", "RQSTDT", "RSLTDT", RQSTDT1);
                int iPalletFullBox = 0;

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    iPalletFullBox =  int.Parse(dtResult.Rows[0]["PLLT_BOX_QTY"].ToString());
                }


                if (iOutboxCount != iPalletFullBox && iPalletFullBox > 0)
                {
                    bPalletBoxCount = false;
                }

                #endregion
                if (bPalletBoxCount == true)
                {
                    outGoing(idxPallet);
                }
                else
                {

                    // PALLET에 지정된 적재수량[%1]과 틀립니다. 계속 하시겠습니까?
                    Util.MessageConfirm("SFU10000", result1 =>
                    {

                        if (result1 == MessageBoxResult.OK)
                        {
                            outGoing(idxPallet);
                        }
                        else
                        {
                            return;
                        }
                    }, iPalletFullBox);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region outGoing
        private void outGoing(int idxPallet)
        {
            Util.MessageConfirm("SFU2802", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    loadingIndicator.Visibility = Visibility.Visible;

                    string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
                    string sPACK_VIRT_LOTID = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["PACK_VIRT_LOTID"].Index).Value);
                    string sBoxStat = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value);
                    string sMultiShipToFlag = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["MULTI_SHIPTO_FLAG"].Index).Value);

                    DataTable RQSTDT = new DataTable("INDATA");
                    RQSTDT.Columns.Add("SRCTYPE");
                    RQSTDT.Columns.Add("PALLETID");
                    RQSTDT.Columns.Add("USERID");
                    RQSTDT.Columns.Add("RCV_ISS_TYPE_CODE");
                    RQSTDT.Columns.Add("RCV_ISS_STAT_CODE");

                    DataRow newRow = RQSTDT.NewRow();
                    newRow["SRCTYPE"] = "UI";
                    newRow["USERID"] = LoginInfo.USERID; 
                    newRow["PALLETID"] = sPalletId;
                    newRow["RCV_ISS_TYPE_CODE"] = "SHIP";
                    newRow["RCV_ISS_STAT_CODE"] = "SHIP_WAIT";
                    
                    RQSTDT.Rows.Add(newRow);

                    //PALLETID
                    //USERID  
                    //RCV_ISS_TYPE_CODE
                    //RCV_ISS_STAT_CODE

                    // BR_PRD_REG_SHIPMENT_MB        
                    new ClientProxy().ExecuteService("BR_REG_SHIPMENT_MB", "INDATA", null, RQSTDT, (RSLTDT, ex) =>
                    {
                        try
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }
                            GetPalletList();
                            _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                            GetCompleteOutbox(sPalletId, sMultiShipToFlag);
                            TagPrint(sPalletId, sPACK_VIRT_LOTID, sBoxStat);
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
        #endregion

        #region 출고취소 : btnCancelShip_Click()
        private void btnCancelShip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                //{
                //    //SFU1843	작업자를 입력 해 주세요.
                //    Util.MessageValidation("SFU1843");
                //    return;
                //}

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKED")
                {
                    //SFU4417   실적 확정된 팔레트만 출고 취소 가능합니다.
                    Util.MessageValidation("SFU4417");
                    return;
                }
                //	SFU2805		포장출고를 취소하시겠습니까?	
                Util.MessageConfirm("SFU2805", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                        DataTable RQSTDT = new DataTable("INDATA");
                        RQSTDT.Columns.Add("BOXID");
                        RQSTDT.Columns.Add("USERID");

                        DataRow newRow = RQSTDT.NewRow();
                        newRow["USERID"] = LoginInfo.USERID;   //txtWorker_Main.Tag;
                        newRow["BOXID"] = sPalletId;
                        RQSTDT.Rows.Add(newRow);

                        new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_SHIPMENT_MB", "INDATA", null, RQSTDT, (RSLTDT, ex) =>
                        {
                            try
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                GetPalletList();
                                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                                GetCompleteOutbox();

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
        #endregion

        #region Packing LIst 발행 : btnPrint_Click()

        /// <summary>
        ///  Packing LIst 발행 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            TagPrint();
        }
        /// <summary>
        /// Packing LIst 발행 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void confirmPopup_Closed(object sender, EventArgs e)
        {
            //FCS002_Report_2nd_Boxing popup = sender as FCS002_Report_2nd_Boxing;

            FCS002_PALLET_TAG popup = sender as FCS002_PALLET_TAG;

            string sPalletId = popup.PALLET_ID;
            int idx = 0;

            GetPalletList();

            for (int i = 0; i < dgPalletList.Rows.Count; i++)
            {
                if (Util.NVC(dgPalletList.GetCell(i, dgPalletList.Columns["BOXID"].Index).Value).Equals(sPalletId))
                {
                    idx = i;
                    break;
                }
            }
            GetPalletList(idx);
            dgPalletList.ScrollIntoView(idx, 0);
        }
        #endregion

        #region 완성 Oubox 삭제 : btnBoxDelete_Click()
        /// <summary>
        /// OutBox 삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBoxDelete_Click(object sender, RoutedEventArgs e)
        {
            int idxOutbox = 0;
            List<int> idxBoxList = new List<int>();
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");
                        int iCnt = 0;       //잔량 OUTBOX Count
                        int iSelectCnt = 0; //선택 Count

                        idxOutbox = _util.GetDataGridCheckFirstRowIndex(dgOutbox, "CHK");
                        idxBoxList = _util.GetDataGridCheckRowIndex(dgOutbox, "CHK");

                        if (idxOutbox < 0)
                        {
                            //SFU1645 선택된 작업대상이 없습니다.
                            Util.MessageValidation("SFU1645");
                            return;
                        }
                        if (idxBoxList.Count <= 0)
                        {
                            //SFU1645 선택된 작업대상이 없습니다.
                            Util.MessageValidation("SFU1645");
                            return;
                        }
                        if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) == "PACKED")
                        {
                            //SFU1235	이미 확정 되었습니다.
                            Util.MessageValidation("SFU1235");
                            return;
                        }

                        string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                        DataSet indataSet = new DataSet();
                        DataTable inPalletTable = indataSet.Tables.Add("INDATA");
                        inPalletTable.Columns.Add("SRCTYPE");
                        inPalletTable.Columns.Add("LANGID");
                        inPalletTable.Columns.Add("BOXID");
                        inPalletTable.Columns.Add("USERID");


                        DataTable inBoxTable = indataSet.Tables.Add("INOUTBOX");
                        inBoxTable.Columns.Add("BOXID");

                        DataRow newRow = inPalletTable.NewRow();

                        newRow["SRCTYPE"] = "UI";
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["BOXID"] = sPalletId;
                        newRow["USERID"] = LoginInfo.USERID; 

                        inPalletTable.Rows.Add(newRow);

                        foreach (int idxBox in idxBoxList)
                        {
                            string sBoxId = Util.NVC(dgOutbox.GetCell(idxBox, dgOutbox.Columns["OUTBOXID"].Index).Value);
                            var query = (from t in inBoxTable.AsEnumerable()
                                         where t.Field<string>("BOXID") == sBoxId
                                         select t.Field<string>("BOXID")).ToList();
                            if (query.Any())
                                continue;
                            newRow = inBoxTable.NewRow();
                            newRow["BOXID"] = sBoxId;
                            inBoxTable.Rows.Add(newRow);
                        }
                        new ClientProxy().ExecuteService_Multi("BR_PRD_UNPACK_OUTBOX_MB", "INDATA,INOUTBOX", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }
                                //삭제되었습니다.
                                Util.MessageValidation("SFU1273");
                                int idx = 0;
                                GetPalletList();
                                for (int i = 0; i < dgPalletList.Rows.Count; i++)
                                {
                                    if (Util.NVC(dgPalletList.GetCell(i, dgPalletList.Columns["BOXID"].Index).Value).Equals(sPalletId))
                                    {
                                        idx = i;
                                        break;
                                    }
                                }

                                Util.gridClear(dgOutbox);
                                GetPalletList(idx);
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

                        if (dgOutbox.Rows.Count == 0)
                        {
                            Util.gridClear(dgOutbox);
                        }
                    }
                }
            });
        }

        #endregion

        #region 재발행 : btnRePrint_Click()
        /// <summary>
        /// 재발행
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRePrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                List<int> idxBoxList = new List<int>();
                if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
                    return;

                //if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                //{
                //    //SFU1843	작업자를 입력 해 주세요.
                //    Util.MessageValidation("SFU1843");
                //    return;
                //}
                idxBoxList = _util.GetDataGridCheckRowIndex(dgOutbox, "CHK");


                if (idxBoxList.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }
                Util.MessageConfirm("SFU2059", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataSet ds = new DataSet();
                        DataTable dtIndata = ds.Tables.Add("INDATA");
                        dtIndata.Columns.Add("LANGID");
                        dtIndata.Columns.Add("USERID");
                        DataRow dr = null;
                        dr = dtIndata.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["USERID"] = LoginInfo.USERID;
                        dtIndata.Rows.Add(dr);
                        DataTable dtInbox = ds.Tables.Add("INBOX");
                        dtInbox.Columns.Add("BOXID");

                        foreach (int idxBox in idxBoxList)
                        {
                            dr = dtInbox.NewRow();
                            string sBoxId = Util.NVC(dgOutbox.GetCell(idxBox, dgOutbox.Columns["OUTBOXID"].Index).Value);
                            dr["BOXID"] = sBoxId;
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
                        DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_OUTBOX_REPRT_MB", "INDATA,INBOX,INPRINT", "OUTDATA", ds);
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
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region 작업 Pallet 색깔표시 : dgPalletList_LoadedCellPresenter()

        private void dgPalletList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblShipWait.Tag))
                    {
                        e.Cell.Presenter.Background = lblShipWait.Background;
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

        #region [INBOX 라벨 발행 팝업 호출] - 사용안함
        private void btnBoxLabelPrint_Click(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrEmpty(txtShift_Main.Text))
            //{
            //    Util.MessageValidation("SFU1845");
            //    return;
            //}

            //if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            //{
            //    // 작업자를 입력 해 주세요.
            //    Util.MessageValidation("SFU1843");
            //    return;
            //}

            if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //SFU1223 라인을 선택 하세요
                Util.MessageValidation("SFU1223");
                return;
            }

            FCS002_303_INBOX_LABEL popup = new FCS002_303_INBOX_LABEL();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[1];

                Parameters[0] =  LoginInfo.USERID; // 작업자id


                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puInboxLabel_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }

        private void puInboxLabel_Closed(object sender, EventArgs e)
        {
            FCS002_303_INBOX_LABEL popup = sender as FCS002_303_INBOX_LABEL;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            }
            this.grdMain.Children.Remove(popup);
        }

        #endregion

        #region [포장해체 팝업 호출]
        private void btnUnpackBox_Click(object sender, RoutedEventArgs e)
        {

            //if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            //{
            //    // 작업자를 입력 해 주세요.
            //    Util.MessageValidation("SFU1843");
            //    return;
            //}

            if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //SFU1223 라인을 선택 하세요
                Util.MessageValidation("SFU1223");
                return;
            }

            FCS002_303_UNPACK popup = new FCS002_303_UNPACK();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[2];

                Parameters[0] = LoginInfo.USERID; // 작업자id
                Parameters[1] = "FCS002_303"; // 자동 포장 구성(원/각형)


                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puUnpackBox_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }

        private void puUnpackBox_Closed(object sender, EventArgs e)
        {
            FCS002_303_UNPACK popup = sender as FCS002_303_UNPACK;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            }
            this.grdMain.Children.Remove(popup);
        }
        #endregion

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        #region [Cell 정보조회 팝업 호출]
        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {

            FCS002_303_INFO popup = new FCS002_303_INFO();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[1];

                Parameters[0] = LoginInfo.USERID;  // 작업자id


                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puInfo_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }

        private void puInfo_Closed(object sender, EventArgs e)
        {
            FCS002_303_INFO popup = sender as FCS002_303_INFO;

            this.grdMain.Children.Remove(popup);
        }
        #endregion

        #region [포장 정보 변경 팝업 호출]
        private void btnshiptochange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                //{
                //    //SFU1843	작업자를 입력 해 주세요.
                //    Util.MessageValidation("SFU1843");
                //    return;
                //}

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }
                FCS002_303_CHANGE_SHIPTO puChangeShipto = new FCS002_303_CHANGE_SHIPTO();
                puChangeShipto.FrameOperation = FrameOperation;

                if (puChangeShipto != null)
                {
                    string sBoxID = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
                    string sShipto = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["SHIPTO_ID"].Index).Value);
                    string sShiptoName = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["SHIPTO_NAME"].Index).Value);
                    string sProdID = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["PRODID"].Index).Value);

                    object[] Parameters = new object[4];
                    Parameters[0] = sBoxID;
                    Parameters[1] = sProdID;
                    Parameters[2] = sShiptoName;
                    Parameters[3] = LoginInfo.USERID; 
                    C1WindowExtension.SetParameters(puChangeShipto, Parameters);

                    puChangeShipto.Closed += new EventHandler(puChangeShipto_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndRun.ShowModal()));                    
                    grdMain.Children.Add(puChangeShipto);
                    puChangeShipto.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageValidation(ex.ToString());
            }
        }

        private void puChangeShipto_Closed(object sender, EventArgs e)
        {
            FCS002_303_CHANGE_SHIPTO popup = sender as FCS002_303_CHANGE_SHIPTO;

            if (popup != null)
            {
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");
                string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                GetPalletList();
                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
            }
            this.grdMain.Children.Remove(popup);
        }
        #endregion

        #region [Pallet 분할 팝업 호출]
        private void btnPltSplit_Click(object sender, RoutedEventArgs e)
        {

            //if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            //{
            //    //SFU1843	작업자를 입력 해 주세요.
            //    Util.MessageValidation("SFU1843");
            //    return;
            //}
            FCS002_303_PLT_SPLIT popUp = new FCS002_303_PLT_SPLIT();
            popUp.FrameOperation = this.FrameOperation;

            if (popUp != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = "";
                Parameters[1] = LoginInfo.USERID; 
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

            FCS002_303_PLT_SPLIT popup = sender as FCS002_303_PLT_SPLIT;

            if (popup != null)
            {
                Util.gridClear(dgOutbox);
                GetPalletList();
            }
            this.grdMain.Children.Remove(popup);

        }
        #endregion

        #region [Pallet 작업 취소] btnRunCancel_Click()
        private void btnRunCancel_Click(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            //{
            //    //SFU1843	작업자를 입력 해 주세요.
            //    Util.MessageValidation("SFU1843");
            //    return;
            //}

            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

            if (idxPallet < 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }
            Util.MessageConfirm("SFU1168", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataTable dt = new DataTable();

                        dt.Columns.Add("BOXID");
                        dt.Columns.Add("USERID");
                        dt.Columns.Add("LANGID");

                        DataRow dr = dt.NewRow();

                        dr["BOXID"] = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
                        dr["USERID"] = LoginInfo.USERID; 
                        dr["LANGID"] = LoginInfo.LANGID;

                        dt.Rows.Add(dr);

                        new ClientProxy().ExecuteService("BR_PRD_DEL_TESLA_PLT_MB", "INDATA", null, dt, (bizResult, bizException) =>
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            Util.gridClear(dgOutbox);
                            GetPalletList();
                            //정상 처리 되었습니다.
                            Util.MessageInfo("SFU1275");
                        });
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }
        #endregion

        #region [Cell 교체 팝업 호출]
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {

            //if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            //{
            //    //SFU1843	작업자를 입력 해 주세요.
            //    Util.MessageValidation("SFU1843");
            //    return;
            //}

            if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //SFU1223 라인을 선택 하세요
                Util.MessageValidation("SFU1223");
                return;
            }

            FCS002_303_CHANGE_CELL puChange = new FCS002_303_CHANGE_CELL();
            puChange.FrameOperation = FrameOperation;

            if (puChange != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = Util.NVC(cboLine.SelectedValue);
                Parameters[1] = LoginInfo.USERID; 
                Parameters[2] = "FCS002_303";
                C1WindowExtension.SetParameters(puChange, Parameters);

                puChange.Closed += new EventHandler(puChange_Closed);

                grdMain.Children.Add(puChange);
                puChange.BringToFront();

            }
        }

        private void puChange_Closed(object sender, EventArgs e)
        {
            FCS002_303_CHANGE_CELL popup = sender as FCS002_303_CHANGE_CELL;

            if (popup.DialogResult == MessageBoxResult.OK)
            {
                GetPalletList();
            }
            this.grdMain.Children.Remove(popup);
        }
        # endregion

        #region Outbox 일괄 추가 팝업 호출]
        private void btnOutboxMulti_Click(object sender, RoutedEventArgs e)
        {

            //if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            //{
            //    //SFU1843	작업자를 입력 해 주세요.
            //    Util.MessageValidation("SFU1843");
            //    return;
            //}

            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

            if (idxPallet < 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKING")
            {
                //SFU 2957 진행중인 작업을 선택하세요.	
                Util.MessageValidation("SFU2957");
                return;
            }

            FCS002_303_OUTBOX_MULTI puOutbox = new FCS002_303_OUTBOX_MULTI();
            puOutbox.FrameOperation = FrameOperation;

            if (puOutbox != null)
            {
                object[] Parameters = new object[10];
                Parameters[0] = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value); ;
                Parameters[1] = LoginInfo.USERID; 

                /*C20210906-000208 로 변경
                //C20210305-000498 로 수정. INBOXID 7자리 : 테슬라 재활용 인박스, 8자리 : 테슬라 일회용 인박스. 혼입금지
                if ( dgOutbox.GetRowCount() > 0)
                {
                    Parameters[2] = (Util.NVC(DataTableConverter.GetValue(dgOutbox.Rows[0].DataItem, "INBOXID"))).Trim().Length;  //dgOutbox 그리드에 있는 INBOXID 길이
                }
                else
                {
                    Parameters[2] = 0;
                }
                */

                //C20210906-000208
                if (dgOutbox.GetRowCount() > 0)
                {
                    Parameters[2] = (Util.NVC(DataTableConverter.GetValue(dgOutbox.Rows[0].DataItem, "TYPE_FLAG"))).Trim();
                }
                else
                {
                    Parameters[2] = null;
                }

                Parameters[3] = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["MULTI_SHIPTO_FLAG"].Index).Value);

                Parameters[4] = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["LOTTYPE"].Index).Value);
                //팔레트에 포함된 아웃박스가 있으면 일괄추가 화면에서 다중출하처 체크, 출하처 선택 등 불가(비활성화)
                if (dgOutbox.GetRowCount() > 0)
                {
                    Parameters[5] = "N";
                }
                else
                {
                    Parameters[5] = "Y";
                }
                Parameters[6] = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["PRODID"].Index).Value);
                Parameters[7] = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["SHIPTO_ID"].Index).Value);
                Parameters[8] = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["PALLET_TYPE"].Index).Value);
                Parameters[9] = DataTableConverter.Convert(dgOutbox.ItemsSource);

                C1WindowExtension.SetParameters(puOutbox, Parameters);

                puOutbox.Closed += new EventHandler(puOutbox_Closed);

                grdMain.Children.Add(puOutbox);
                puOutbox.BringToFront();
            }
        }

        private void puOutbox_Closed(object sender, EventArgs e)
        {
            FCS002_303_OUTBOX_MULTI popup = sender as FCS002_303_OUTBOX_MULTI;

            if (popup.DialogResult == MessageBoxResult.OK)
            {
                GetPalletList();
                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", popup.PALLET_ID, true);
                GetCompleteOutbox();
            }
            this.grdMain.Children.Remove(popup);
        }
        #endregion

        #region Non IM OUTBOX label print 팝업 호출]
        private void btnNonIm_Click(object sender, RoutedEventArgs e)
        {

            //if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            //{
            //    //SFU1843	작업자를 입력 해 주세요.
            //    Util.MessageValidation("SFU1843");
            //    return;
            //}

            FCS002_303_NONIMOUTBOX popup = new FCS002_303_NONIMOUTBOX();
            popup.FrameOperation = this.FrameOperation;
            if (popup != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = LoginInfo.CFG_AREA_ID;
                Parameters[1] = LoginInfo.USERID; 
                Parameters[2] = "";

                C1WindowExtension.SetParameters(popup, Parameters);
                popup.Closed += new EventHandler(puNonIm_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
            else
            {
                //Message: 팔레트 구성 정보가 없습니다.
            }

        }

        private void puNonIm_Closed(object sender, EventArgs e)
        {
            FCS002_303_NONIMOUTBOX popup = sender as FCS002_303_NONIMOUTBOX;

            this.grdMain.Children.Remove(popup);
        }
        #endregion

        #region [미포장박스 조회 팝업 호출]
        private void btnUnPackedBoxInfo_Click(object sender, RoutedEventArgs e)
        {
            FCS002_303_UNPACKED_BOX popup = new FCS002_303_UNPACKED_BOX();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[1];

                Parameters[0] = LoginInfo.USERID;  // 작업자id

                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puInfo_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }
        #endregion [미포장박스 조회 팝업 호출] 

        #endregion

        #region [Method]

        #region Pallet 생성 조회 : GetPalletList()
        /// <summary>
        /// Pallet 생성 조회
        /// </summary>
        /// <param name="idx"></param>
        private void GetPalletList(int idx = -1)
        {
            try
            {
                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                //RQSTDT.Columns.Add("PKG_LOTID");
                RQSTDT.Columns.Add("BOXSTAT_LIST", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = Util.NVC(cboLine.SelectedValue);

                if (!string.IsNullOrWhiteSpace(txtPalletID.Text))
                    dr["BOXID"] = txtPalletID.Text;
                else
                {
                    //dr["PKG_LOTID"] = null;
                    dr["BOXSTAT_LIST"] = string.IsNullOrEmpty(_searchStat) ? _searchStat : _searchStat.Remove(_searchStat.Length - 1);
                }
                RQSTDT.Rows.Add(dr);

                DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_2ND_PALLET_LIST_MB", "INDATA", "OUTDATA", RQSTDT);

                if (!RSLTDT.Columns.Contains("CHK"))
                    RSLTDT = _util.gridCheckColumnAdd(RSLTDT, "CHK");

                Util.GridSetData(dgPalletList, RSLTDT, FrameOperation, true);
                if (idx != -1)
                {
                    DataTableConverter.SetValue(dgPalletList.Rows[idx].DataItem, "CHK", true);
                    dgPalletList.SelectedIndex = idx;
                    dgPalletList.ScrollIntoView(idx, 0);
                }
                else
                {
                    dgPalletList.SelectedIndex = -1;
                }

                GetCompleteOutbox();

                if (RSLTDT.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgPalletList.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgPalletList.Columns["BOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }
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

        #region 완성 OUTBOX 조회 : GetCompleteOutbox()
        /// <summary>
        /// 완성 OUTBOX 조회
        /// </summary>
        private void GetCompleteOutbox()
        {
            try
            {
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                    return;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PALLETID", typeof(string));
                inTable.Columns.Add("BOXID", typeof(string));
                inTable.Columns.Add("MULTI_SHIPTO_FLAG", typeof(string));

                DataRow newRow = inTable.NewRow();

                newRow["EQSGID"] = cboLine.SelectedValue.ToString();

                newRow["PALLETID"] = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
                newRow["MULTI_SHIPTO_FLAG"] = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["MULTI_SHIPTO_FLAG"].Index).Value);
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMPLETE_OUTBOX_PALLET_MB", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgOutbox, dtResult, FrameOperation, false);
                if (dtResult != null && dtResult.Rows.Count > 0)
                    dgOutbox.CurrentCell = dgOutbox.GetCell(0, 1);

                string[] sColumnName = new string[] { "OUTBOXID2", "BOXSEQ", "OUTBOXID", "OUTBOXQTY" };
                if (dgOutbox.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgOutbox.Columns["INBOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }

                _util.SetDataGridMergeExtensionCol(dgOutbox, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetCompleteOutbox(string sPalletId, string sMultiShipToFlag)
        {
            try
            {
                //int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                //if (idxPallet < 0)
                //    return;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PALLETID", typeof(string));
                inTable.Columns.Add("BOXID", typeof(string));
                inTable.Columns.Add("MULTI_SHIPTO_FLAG", typeof(string));

                DataRow newRow = inTable.NewRow();

                newRow["EQSGID"] = cboLine.SelectedValue.ToString();
                newRow["PALLETID"] = sPalletId;
                newRow["MULTI_SHIPTO_FLAG"] = sMultiShipToFlag;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMPLETE_OUTBOX_PALLET_MB", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgOutbox, dtResult, FrameOperation, false);
                if (dtResult != null && dtResult.Rows.Count > 0)
                    dgOutbox.CurrentCell = dgOutbox.GetCell(0, 1);

                string[] sColumnName = new string[] { "OUTBOXID2", "BOXSEQ", "OUTBOXID", "OUTBOXQTY" };
                if (dgOutbox.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgOutbox.Columns["INBOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }

                _util.SetDataGridMergeExtensionCol(dgOutbox, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 완성 OUTBOX 조회 : GetCompleteOutbox()
        /// <summary>
        /// 완성 OUTBOX CELL OCV 체크
        /// </summary>
        private bool GetCompleteOutboxOcvCheck(string BoxID)
        {

            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

            DataTable inTable = new DataTable();
            inTable.Columns.Add("BOXID", typeof(string));
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("MULTI_SHIPTO_FLAG", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["BOXID"] = BoxID;
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["MULTI_SHIPTO_FLAG"] = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["MULTI_SHIPTO_FLAG"].Index).Value);
            inTable.Rows.Add(newRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMPLETE_OUTBOX_PALLET_MB", "INDATA", "OUTDATA", inTable);

            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    //NFF의 경우 OCV 검사결과 확인부분 제외함. 추후 MMD 공통코드로 사용여부 사용할 경우 공통코드로 제외 해야함.(2024.04.18)
                    //if (!dtResult.Rows[i]["OCV_SPEC_RESULT"].ToString().Equals("OK"))
                    //{
                    //    //OCV SPEC이 맞지 않아 포장이 불가능합니다.
                    //    Util.MessageValidation("SFU8227");
                    //    return false;
                    //}

                    string sTypeFlag_i = dtResult.Rows[i]["TYPE_FLAG"].ToString().Trim();
                    string sOutBoxID_i = dtResult.Rows[i]["OUTBOXID"].ToString().Trim();

                    for (int inx = 0; inx < dgOutbox.GetRowCount(); inx++)
                    {
                        string sTypeFlag_dg = (Util.NVC(DataTableConverter.GetValue(dgOutbox.Rows[inx].DataItem, "TYPE_FLAG"))).Trim();
                        if (sTypeFlag_i != sTypeFlag_dg)
                        {
                            Util.MessageValidation("SFU3806", sOutBoxID_i);  //유형이 다른 박스는 하나의 팔레트에 혼입할 수 없습니다. [%1]
                            return false;
                        }
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        #endregion

        #region Packing List(Pallet Tag) 발행 : TagPrint()
        private void TagPrint()
        {
            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

            if (idxPallet < 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKED")
            {
                //SFU4262		실적 확정후 작업 가능합니다.	
                Util.MessageValidation("SFU4262");
                return;
            }

            string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
            string sPACK_VIRT_LOTID = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["PACK_VIRT_LOTID"].Index).Value);
            
            //FCS002_Report_2nd_Boxing popup = new FCS002_Report_2nd_Boxing();

            //데이터 생성 후  DataTable을 넘김.


            DataSet ds = new DataSet();
            DataTable dtIndata = ds.Tables.Add("INDATA");
            dtIndata.Columns.Add("BOXID");

            DataRow dr = null;
            dr = dtIndata.NewRow();
            dr["BOXID"] = sPalletId;

            dtIndata.Rows.Add(dr);

            string sBizName = "";

            sBizName = "BR_GET_PALLET_TAG_MB";

            DataSet dsPackingList = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA", "OUTPALLET,OUTLOT", ds);
            if (dsPackingList != null && dsPackingList.Tables.Count > 0 && dsPackingList.Tables["OUTPALLET"].Rows.Count > 0 && dsPackingList.Tables["OUTLOT"].Rows.Count > 0)
            {
                DataTable dtPallet = dsPackingList.Tables["OUTPALLET"];
                DataTable dtLot = dsPackingList.Tables["OUTLOT"];


                FCS002_PALLET_TAG popup = new FCS002_PALLET_TAG();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[4];

                    Parameters[0] = dtPallet;
                    Parameters[1] = dtLot;
                    Parameters[2] = sPalletId;

                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.Closed += new EventHandler(confirmPopup_Closed);
                    grdMain.Children.Add(popup);
                    popup.BringToFront();
                }

            }

        }


        private void TagPrint(string sPalletId, string sPACK_VIRT_LOTID, string sBoxStat)
        {
            //int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

            //if (idxPallet < 0)
            //{
            //    //SFU1645 선택된 작업대상이 없습니다.
            //    Util.MessageValidation("SFU1645");
            //    return;
            //}

            //if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKED")
            //{
            //    //SFU4262		실적 확정후 작업 가능합니다.	
            //    Util.MessageValidation("SFU4262");
            //    return;
            //}

            if (sBoxStat != "PACKED")
            {
                //SFU4262		실적 확정후 작업 가능합니다.	
                Util.MessageValidation("SFU4262");
                return;
            }

            //string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
            //string sPACK_VIRT_LOTID = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["PACK_VIRT_LOTID"].Index).Value);

            DataSet ds = new DataSet();
            DataTable dtIndata = ds.Tables.Add("INDATA");
            dtIndata.Columns.Add("BOXID");

            DataRow dr = null;
            dr = dtIndata.NewRow();
            dr["BOXID"] = sPalletId;

            dtIndata.Rows.Add(dr);

            string sBizName = "";

            sBizName = "BR_GET_PALLET_TAG_MB";

            DataSet dsPackingList = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA", "OUTPALLET,OUTLOT", ds);
            if (dsPackingList != null && dsPackingList.Tables.Count > 0 && dsPackingList.Tables["OUTPALLET"].Rows.Count > 0 && dsPackingList.Tables["OUTLOT"].Rows.Count > 0)
            {
                DataTable dtPallet = dsPackingList.Tables["OUTPALLET"];
                DataTable dtLot = dsPackingList.Tables["OUTLOT"];


                FCS002_PALLET_TAG popup = new FCS002_PALLET_TAG();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[4];

                    Parameters[0] = dtPallet;
                    Parameters[1] = dtLot;
                    Parameters[2] = sPalletId;

                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.Closed += new EventHandler(confirmPopup_Closed);
                    grdMain.Children.Add(popup);
                    popup.BringToFront();
                }

            }

        }
        #endregion

        #region 완성 OutBox 재발행 :  PrintLabel()

        /// <summary>
        /// 완성 OutBox 재발행
        /// </summary>
        /// <param name="zpl"></param>
        /// <param name="drPrtInfo"></param>
        /// <returns></returns>
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

            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }

        #endregion

        #region OutBox 추가 : AddOutBox()

        private void AddOutBox(string pBoxID, string pPROD_LINE, string pPKG_LOTID, string pPRDT_GRD_CODE)
        {
            try
            {
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }
                //if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                //{
                //    //SFU1843	작업자를 입력 해 주세요.
                //    Util.MessageValidation("SFU1843");
                //    return;
                //}

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["PALLET_TYPE"].Index).Value) == "0")
                {

                    bool bLine_Lot_Mix = true;
                    bool bMixLot = false;

                    string sTempPkgLot = pPKG_LOTID.Substring(0, 5) + pPKG_LOTID.Substring(7, 1);




                    DataTable dtSourceTemp = DataTableConverter.Convert(dgOutbox.ItemsSource);

                    DataView view = dtSourceTemp.DefaultView;
                    DataTable distinctTable = view.ToTable(true, new string[] { "PKG_LOTID" });


                    //혼입 가능한 수량을 초과 했을 경우
                    if (distinctTable.Rows.Count >= 7)
                    {

                        var queryPKGLotid = (from t in dtSourceTemp.AsEnumerable()
                                             where t.Field<string>("PKG_LOTID") == pPKG_LOTID
                                             select t).Distinct();


                        //동일 PKG_LOTID가 있으면 추가 가능
                        if (queryPKGLotid.Any() == false)
                        {
                            //SFU4554 LOT 수량을 초과했습니다. 처리수량을 재확인 하세요.                        
                            Util.MessageValidation("SFU4554");
                            return;
                        }
                    }




                    // ASSY_LOT이 같은 월인지 확인
                    for (int i = 0; i < dtSourceTemp.Rows.Count; i++)
                    {
                        string sLOT1 = Util.NVC(dtSourceTemp.Rows[i]["PKG_LOTID"]).Substring(0, 5); //조립월
                        string sLOT2 = Util.NVC(dtSourceTemp.Rows[i]["PKG_LOTID"]).Substring(7, 1); //라인 

                        if (!(sLOT1 + sLOT2).Equals(sTempPkgLot))
                        {
                            bMixLot = true;
                            break;
                        }

                    }

                    if (bMixLot == true)
                    {
                        //혼입불가한 LOT입니다.
                        Util.MessageValidation("SFU9027");
                        return;
                    }



                    //DataTable dtPalletList = DataTableConverter.Convert(dgPalletList.ItemsSource);

                    DataTable dtSource = DataTableConverter.Convert(dgOutbox.ItemsSource);

                    var prdt_grd_code = (from t in dtSource.AsEnumerable()
                                         where t.Field<string>("PRDT_GRD_CODE") != pPRDT_GRD_CODE
                                         select t).Distinct();


                    var query = (from t in dtSource.AsEnumerable()
                                 where t.Field<string>("PROD_LINE") != pPROD_LINE
                                 select t).Distinct();

                    var query_pkg_lot = (from t in dtSource.AsEnumerable()
                                         where t.Field<string>("PKG_LOTID") != pPKG_LOTID
                                         select t).Distinct();

                    //GRADR 혼합 불가
                    if (prdt_grd_code.Any())
                    {
                        //SFU4182 동일한 등급이 아닙니다.
                        //Line이 혼합됩니다. 추가하시겠습니까?
                        Util.MessageValidation("SFU4182");
                        return;
                    }



                    if (query_pkg_lot.Any() == false && query.Any() == false)
                    {
                        RunAddOutBox(pBoxID);
                    }
                    else
                    {


                        //라인 MIX
                        if (query.Any())
                        {
                            //Line이 혼합됩니다. 추가하시겠습니까?
                            Util.MessageConfirm("SFU3821", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    bLine_Lot_Mix = true;
                                }
                                else
                                {
                                    bLine_Lot_Mix = false;
                                }

                                if (query_pkg_lot.Any())
                                {
                                    //PKG LOTID가 혼합됩니다. 추가하시겠습니까?
                                    Util.MessageConfirm("SFU3824", (result1) =>
                                    {
                                        if (result1 == MessageBoxResult.OK)
                                        {
                                            bLine_Lot_Mix = true;
                                        }
                                        else
                                        {
                                            bLine_Lot_Mix = false;
                                        }

                                        //혼입여부 True일 경우
                                        if (bLine_Lot_Mix)
                                        {
                                            RunAddOutBox(pBoxID);
                                        }
                                    });
                                }

                            });
                        }
                        //PKG_LOTID MIX
                        if (query_pkg_lot.Any())
                        {
                            //PKG LOTID가 혼합됩니다. 추가하시겠습니까?
                            Util.MessageConfirm("SFU3824", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {

                                    bLine_Lot_Mix = true;
                                }
                                else
                                {
                                    bLine_Lot_Mix = false;
                                }

                                //혼입여부 True일 경우
                                if (bLine_Lot_Mix)
                                {
                                    RunAddOutBox(pBoxID);
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void RunAddOutBox(string pBoxID)
        {
            try
            {
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                DataSet ds = new DataSet();
                DataTable dtIndata = ds.Tables.Add("INPALLET");
                dtIndata.Columns.Add("SRCTYPE");
                dtIndata.Columns.Add("LANGID");
                dtIndata.Columns.Add("BOXID");
                dtIndata.Columns.Add("USERID");

                DataRow dr = null;
                dr = dtIndata.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
                dr["USERID"] = LoginInfo.USERID; 

                dtIndata.Rows.Add(dr);

                DataTable dtInbox = ds.Tables.Add("INOUTBOX");
                dtInbox.Columns.Add("BOXID");
                dr = dtInbox.NewRow();
                dr["BOXID"] = pBoxID;

                dtInbox.Rows.Add(dr);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_PACK_OUTBOX_MIX_MB", "INPALLET,INOUTBOX", null, ds);

                GetPalletList(idxPallet);
                GetCompleteOutbox();
                txtOutBox.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #endregion


    }
}
