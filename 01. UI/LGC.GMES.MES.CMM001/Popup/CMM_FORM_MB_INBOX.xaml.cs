/*************************************************************************************
 Created Date : 2018.12.17
      Creator : 정문교
   Decription : 원각형 특성/Grading 공정진척 - Inbox 생성 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2023.03.27  이홍주 : 소형활성화MES 용 CMM_FORM_INBOX --> CMM_FORM_MB_INBOX로 복사
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
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;
using System.Collections.Generic;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_FORM_MB_INBOX.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_FORM_MB_INBOX : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _procID = string.Empty;        // 공정코드
        private string _eqsgID = string.Empty;        // 라인코드
        private string _eqptID = string.Empty;        // 설비코드
        private string _palletID = string.Empty;      // Pallet ID

        // 프린트 설정용
        string _sPrt = string.Empty;
        string _sRes = string.Empty;
        string _sCopy = string.Empty;
        string _sXpos = string.Empty;
        string _sYpos = string.Empty;
        string _sDark = string.Empty;
        DataRow _drPrtInfo = null;

        private int _bfInboxQty = 0;

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        string _sPGM_ID = "CMM_FORM_MB_INBOX";

        public string ShiftID { get; set; }
        public string ShiftName { get; set; }
        public string WorkerName { get; set; }
        public string WorkerID { get; set; }

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

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_FORM_MB_INBOX()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeUserControls();
            SetControl();
            GetPallet();

            Loaded -= C1Window_Loaded;
        }

        private void InitializeUserControls()
        {
            // 현재 Inbox 라벨 발행 안함.   잔량인경우만 라벨 발행
            btnPrintAdd.Visibility = Visibility.Collapsed;

            // Inbox 포장 수량은 100개로 Setting
            txtCellQty.Value = 100;
        }
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _eqsgID = tmps[1] as string;
            _eqptID = tmps[2] as string;
            _palletID = tmps[3] as string;
        }

        #endregion

        /// <summary>
        /// Inbox
        /// </summary>
        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtBoxId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(txtBoxId.Text))
            {
                if (ValidationInbox())
                {
                    RegInbox(txtBoxId.Text, txtCellQty.Value);
                    Getinbox();

                    txtBoxId.Text = string.Empty;
                    txtBoxId.Focus();
                }
            }
        }

        /// <summary>
        /// 추가라벨발행
        /// </summary>
        private void btnPrintAdd_Click(object sender, RoutedEventArgs e)
        {
            // 현재 Inbox 라벨 발행 안함.   잔량인경우만 라벨 발행
            if (!ValidationAddInbox())
                return;

            //BOX001_202_INBOX_LABEL_ADD popupLabelAdd = new BOX001_202_INBOX_LABEL_ADD { FrameOperation = FrameOperation };
            //if (ValidationGridAdd(popupLabelAdd.Name) == false)
            //    return;

            //DataRow[] dr = DataTableConverter.Convert(dgInbox.ItemsSource).Select("CHK = '1' or CHK = 'True'");

            //object[] parameters = new object[3];
            //parameters[0] = Util.NVC(txtWorker_Main.Tag);
            //parameters[1] = "";
            //parameters[2] = dr;
            //C1WindowExtension.SetParameters(popupLabelAdd, parameters);

            //popupLabelAdd.Closed += popupLabelAdd_Closed;
            //grdMain.Children.Add(popupLabelAdd);
            //popupLabelAdd.BringToFront();
        }

        /// <summary>
        /// 발행
        /// </summary>
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (ValidationPrint())
            {
                // 발행 하시겠습니까?	
                Util.MessageConfirm("SFU2873", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        PrintInboxLabel();
                    }
                });
            }
        }

        /// <summary>
        /// 일괄발행
        /// </summary>
        private void btnPrintAll_Click(object sender, RoutedEventArgs e)
        {
            if (ValidationPrintAll())
            {
                // 일괄 발행 하시겠습니까?	
                Util.MessageConfirm("SFU4258", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        PrintInboxLabel(true);
                    }
                });
            }
        }

        /// <summary>
        /// 재발행
        /// </summary>
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
                string sBizRule = "BR_PRD_GET_OUTBOX_REPRT_MB_CIRCULAR";

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
                dr["PRODID"] = Util.NVC(dgPallet.GetCell(0, dgPallet.Columns["PRODID"].Index).Value);
                dr["PRDT_GRD_CODE"] = Util.NVC(dgPallet.GetCell(0, dgPallet.Columns["PRDT_GRD_CODE"].Index).Value);
                dr["PKG_LOTID"] = Util.NVC(dgPallet.GetCell(0, dgPallet.Columns["PKG_LOTID"].Index).Value);
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

        /// <summary>
        /// Inbox 수정
        /// </summary>
        private void btnBoxUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (ValidationUpdate())
            {
                // 수정 하시겠습니까?
                Util.MessageConfirm("SFU4340", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        UpdateInbox();
                    }
                });
            }
        }

        /// <summary>
        /// Inbox 삭제
        /// </summary>
        private void btnBoxDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ValidationDelete())
            {
                //  삭제하시겠습니까?
                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DeleteInbox();
                    }
                });
            }
        }

        #region Pallet Grid Event
        private void dgPallet_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("WIPQTY") || e.Cell.Column.Name.Equals("TOTAL_QTY") || e.Cell.Column.Name.Equals("BOXQTY"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                }
            }));
        }
        #endregion

        #region Inbox Grid Event
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

        private void checkAll_Checked(object sender, RoutedEventArgs e)
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
                _bfInboxQty = Util.NVC_Int(DataTableConverter.GetValue(dgInbox.Rows[e.Row.Index].DataItem, "TOTAL_QTY"));
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
                int inputQty = Util.NVC_Int(DataTableConverter.GetValue(dgPallet.Rows[0].DataItem, "WIPQTY"));
                int sumQty = Util.NVC_Int(DataTableConverter.Convert(dgInbox.ItemsSource).Compute("sum(TOTAL_QTY)", "").GetString());

                if (inputQty < sumQty)
                {
                    // SFU4224 전체수량과 입력수량의 합이 일치하지 않습니다.
                    Util.MessageValidation("SFU4224");

                    DataTableConverter.SetValue(dgInbox.Rows[e.Row.Index].DataItem, "TOTAL_QTY", _bfInboxQty);
                    _bfInboxQty = 0;
                    return;
                }
            }
        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #endregion

        #region Mehod

        /// <summary>
        /// Pallet 조회
        /// </summary>
        private void GetPallet()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID");
                RQSTDT.Columns.Add("EQSGID");
                RQSTDT.Columns.Add("EQPTID");
                RQSTDT.Columns.Add("BOXID");

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = _eqsgID;
                dr["BOXID"] = _palletID;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("BR_PRD_GET_INPALLET_LIST_MB", "RQSTDT", "RSLTDT", RQSTDT, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgPallet, bizResult, FrameOperation, true);

                        if (bizResult != null && bizResult.Rows.Count > 0)
                        {
                            Getinbox();
                        }
                        CalculationRemain();
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
        /// Inbox 생성 : inbox 텍스트 박스에서 스캔또는 Keyin
        /// </summary>
        private void RegInbox(string boxId, double qty, string zplCode = null)
        {
            try
            {
                string sBoxId = Util.NVC(dgPallet.GetCell(0, dgPallet.Columns["BOXID"].Index).Value);

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
                newRow["EQPTID"] = _eqptID;
                newRow["BOXID"] = sBoxId;
                newRow["SHFTID"] = ShiftID;
                newRow["USERID"] = WorkerID;

                inPalletTable.Rows.Add(newRow);

                newRow = inBoxTable.NewRow();
                newRow["TOTAL_QTY"] = qty;
                newRow["BOXID"] = boxId;
                inBoxTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_INBOX_NEW_MB", "INPALLET,INBOX", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetPallet();
                        Getinbox();
                        CalculationRemain();

                        if ((bool)chkPrintYN.IsChecked)
                        {
                            PrintLabel(zplCode, _drPrtInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, indataSet); 

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Inbox 생성 : 발행, 일괄발행 버튼 클릭
        /// </summary>
        private void PrintInboxLabel(bool isPrintAll = false)
        {
            try
            {
                ShowLoadingIndicator();

                if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
                {
                    return;
                }

                int inputQty = (int)double.Parse(dgPallet.GetCell(0, dgPallet.Columns["WIPQTY"].Index).Value.ToString());
                int packedQty = (int)double.Parse(dgPallet.GetCell(0, dgPallet.Columns["TOTAL_QTY"].Index).Value.ToString());
                int prtQty = isPrintAll ? (int)Math.Ceiling((inputQty - packedQty) / txtCellQty.Value) : 1;
                string sPalletId = Util.NVC(dgPallet.GetCell(0, dgPallet.Columns["BOXID"].Index).Value);

                if (isPrintAll == true)
                {
                    string sBizRule = "BR_PRD_REG_INBOX_BATCH_NEW_MB";

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
                    newRow["EQPTID"] = _eqptID;
                    newRow["BOXID"] = sPalletId;
                    newRow["SHFTID"] = ShiftID;
                    newRow["PACKQTY"] = txtCellQty.Value;
                    newRow["PRINTQTY"] = prtQty;
                    newRow["LABELTYPE"] = "CB_NORMAL";
                    newRow["USERID"] = WorkerID;
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
                            }

                            if ((bool)chkPrintYN.IsChecked)
                            {
                                PrintLabel(zplCode, _drPrtInfo);
                            }
                        }
                    }

                    GetPallet();
                    Getinbox();
                    CalculationRemain();
                }
                else
                {
                    string sBizRule = "BR_PRD_GET_INBOX_LABEL_MB_CIRCULAR";

                    DataSet indataSet = new DataSet();
                    DataTable inDataTable = indataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("AREAID");
                    inDataTable.Columns.Add("EQPTID");
                    inDataTable.Columns.Add("PRINTQTY");
                    inDataTable.Columns.Add("LABELTYPE");
                    inDataTable.Columns.Add("USERID");
                    inDataTable.Columns.Add("PRDT_GRD_CODE");
                    inDataTable.Columns.Add("PRODID");
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
                    newRow["EQPTID"] = _eqptID;
                    newRow["PRINTQTY"] = prtQty;
                    newRow["LABELTYPE"] = "CB_NORMAL";
                    newRow["USERID"] = WorkerID;
                    newRow["PRDT_GRD_CODE"] = Util.NVC(dgPallet.GetCell(0, dgPallet.Columns["PRDT_GRD_CODE"].Index).Value);
                    newRow["PRODID"] = Util.NVC(dgPallet.GetCell(0, dgPallet.Columns["PRODID"].Index).Value);
                    newRow["PKG_LOTID"] = Util.NVC(dgPallet.GetCell(0, dgPallet.Columns["PKG_LOTID"].Index).Value);
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
                                RegInbox(boxId, txtCellQty.Value < txtRemainQty.Value ? txtCellQty.Value : txtRemainQty.Value, zplCode);
                            }
                        }
                    }
                }

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
       }

        /// <summary>
        /// 인박스 수정
        /// </summary>
        private void UpdateInbox()
        {
            try
            {
                List<int> idxBoxList = _util.GetDataGridCheckRowIndex(dgInbox, "CHK");

                string sPalletId = Util.NVC(dgPallet.GetCell(0, dgPallet.Columns["BOXID"].Index).Value);

                DataSet indataSet = new DataSet();
                DataTable inPalletTable = indataSet.Tables.Add("INDATA");
                inPalletTable.Columns.Add("BOXID");
                inPalletTable.Columns.Add("USERID");

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("BOXID");
                inBoxTable.Columns.Add("TOTAL_QTY");

                DataRow newRow = inPalletTable.NewRow();
                newRow["BOXID"] = sPalletId;
                newRow["USERID"] = WorkerID;

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

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_UPD_INBOX_NEW_MB", "INPALLET,INBOX", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetPallet();
                        Getinbox();
                        CalculationRemain();

                        // 수정되었습니다.
                        Util.MessageInfo("SFU1265");
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }

                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 인박스 삭제
        /// </summary>
        private void DeleteInbox()
        {
            try
            {
                List<int> idxBoxList = _util.GetDataGridCheckRowIndex(dgInbox, "CHK");

                string sPalletId = Util.NVC(dgPallet.GetCell(0, dgPallet.Columns["BOXID"].Index).Value);

                DataSet indataSet = new DataSet();
                DataTable inPalletTable = indataSet.Tables.Add("INDATA");
                inPalletTable.Columns.Add("BOXID");
                inPalletTable.Columns.Add("USERID");

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("BOXID");
                inBoxTable.Columns.Add("TOTAL_QTY");

                DataRow newRow = inPalletTable.NewRow();
                newRow["BOXID"] = sPalletId;
                newRow["USERID"] = WorkerID;

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

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_DEL_INBOX_NEW_MB", "INPALLET,INBOX", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetPallet();
                        Getinbox();
                        CalculationRemain();

                        // 삭제되었습니다.
                        Util.MessageInfo("SFU1273");
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }

                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 인박스 조회
        /// </summary>
        private void Getinbox()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("OUTER_BOXID2", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["OUTER_BOXID2"] = Util.NVC(dgPallet.GetCell(0, dgPallet.Columns["BOXID"].Index).Value);
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("BR_PRD_GET_INBOX_LIST_MB", "RQSTDT", "RSLTDT", RQSTDT, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (!bizResult.Columns.Contains("CHK"))
                        {
                            bizResult.Columns.Add("CHK");
                        }

                        Util.GridSetData(dgInbox, bizResult, FrameOperation);
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

        #endregion

        #region [Func]

        /// <summary>
        /// 잔량 산출
        /// </summary>
        private void CalculationRemain()
        {
            int PalletQty = (int)double.Parse(dgPallet.GetCell(0, dgPallet.Columns["WIPQTY"].Index).Value.ToString());
            int InboxQty = (int)double.Parse(dgPallet.GetCell(0, dgPallet.Columns["TOTAL_QTY"].Index).Value.ToString());

            txtRemainQty.Value = PalletQty - InboxQty;
        }

        /// <summary>
        /// 라벨 프린트
        /// </summary>
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

        private bool ValidationInbox()
        {
            if (txtCellQty.Value > txtRemainQty.Value)
            {
                //SFU1859 SFU 잔량이 없습니다.
                Util.MessageValidation("SFU1859", (action) =>
                {
                    txtCellQty.Focus();
                });
                return false;
            }

            return true;
        }

        private bool ValidationAddInbox()
        {
            if (dgInbox.ItemsSource == null || dgInbox.Rows.Count < 1)
            {
                // 선택된 데이터가 없습니다.
                Util.MessageValidation("SFU3538");
                return false;
            }

            int idx = _util.GetDataGridCheckFirstRowIndex(dgInbox, "CHK");

            if (idx < 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            return true;
        }

        private bool ValidationPrint()
        {
            if (dgPallet.Rows.Count == 0)
            {
                // Pallet 정보가 없습니다.
                Util.MessageValidation("SFU4245");
                return false;
            }

            //if (Util.NVC(dgPallet.GetCell(0, dgPallet.Columns["BOXSTAT"].Index).Value) == "PACKED")
            //{
            //    //SFU3610	이미 포장 완료 됐습니다.[BOX의 정보 확인]	
            //    Util.MessageValidation("SFU3610");
            //    return false;
            //}

            if (txtRemainQty.Value <= 0)
            {
                //SFU1859 SFU 잔량이 없습니다.
                Util.MessageValidation("SFU1859");
                return false;
            }

            return true;
        }

        private bool ValidationPrintAll()
        {
            if (dgPallet.Rows.Count == 0)
            {
                // Pallet 정보가 없습니다.
                Util.MessageValidation("SFU4245");
                return false;
            }

            //if (Util.NVC(dgPallet.GetCell(0, dgPallet.Columns["BOXSTAT"].Index).Value) == "PACKED")
            //{
            //    //SFU3610	이미 포장 완료 됐습니다.[BOX의 정보 확인]	
            //    Util.MessageValidation("SFU3610");
            //    return false;
            //}

            if (txtRemainQty.Value <= 0)
            {
                //SFU1859 SFU 잔량이 없습니다.
                Util.MessageValidation("SFU1859");
                return false;
            }

            return true;
        }

        private bool ValidationUpdate()
        {
            if (dgPallet.Rows.Count == 0)
            {
                // Pallet 정보가 없습니다.
                Util.MessageValidation("SFU4245");
                return false;
            }

            //if (Util.NVC(dgPallet.GetCell(0, dgPallet.Columns["BOXSTAT"].Index).Value) == "PACKED")
            //{
            //    //SFU3610	이미 포장 완료 됐습니다.[BOX의 정보 확인]	
            //    Util.MessageValidation("SFU3610");
            //    return false;
            //}

            List<int> idxBoxList = _util.GetDataGridCheckRowIndex(dgInbox, "CHK");

            if (idxBoxList.Count <= 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            return true;
        }

        private bool ValidationDelete()
        {
            if (dgPallet.Rows.Count == 0)
            {
                // Pallet 정보가 없습니다.
                Util.MessageValidation("SFU4245");
                return false;
            }

            //if (Util.NVC(dgPallet.GetCell(0, dgPallet.Columns["BOXSTAT"].Index).Value) == "PACKED")
            //{
            //    //SFU3610	이미 포장 완료 됐습니다.[BOX의 정보 확인]	
            //    Util.MessageValidation("SFU3610");
            //    return false;
            //}

            List<int> idxBoxList = _util.GetDataGridCheckRowIndex(dgInbox, "CHK");

            if (idxBoxList.Count <= 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }
            return true;
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