/*************************************************************************************
 Created Date : 2018-08-14
      Creator : 오화백K
   Decription : OutBox 생성 
--------------------------------------------------------------------------------------
 [Change History]
  2018.08.14  DEVELOPER : Initial Created.
  2022.07.14  김태균      신규 박스 생성후 셀추가시 대상 INBOX의 ROW COUNT 체크하는 부분 수정
  
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
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
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Configuration;
using C1.WPF.Excel;
using System.IO;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_253 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        Util _util = new Util();

        private string _searchStat = string.Empty;
        private bool bInit = true;

        private string _BoxType = string.Empty;

        string _sPGM_ID = "BOX001_253";

        /*컨트롤 변수 선언*/
        public UCBoxShift ucBoxShift { get; set; }
        public TextBox txtWorker_Main { get; set; }
        public TextBox txtShift_Main { get; set; }

        public UCBoxShift UCBoxShift { get; set; }

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

        public BOX001_253()
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

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnBoxDelete);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            // Cell 추가 탭
            _combo.SetCombo(cboLine1, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID, "MCC,MCR,MCS", Process.CELL_BOXING, LoginInfo.CFG_AREA_ID }, sCase: "LINEBYSHOP");

            // OUTBOX 생성탭
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID, "MCC,MCR,MCS", Process.CELL_BOXING, LoginInfo.CFG_AREA_ID }, sCase: "LINEBYSHOP");
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, null, sFilter: new string[] { cboLine.SelectedValue.ToString(), Process.CELL_BOXING, null }, sCase: "EQUIPMENT");

            // 등급 콤보박스 (OUTBOX 생성탭)
            //SetGrdCombo(cboRsstGrdCode, "RSST_GRD_CODE");
            //SetGrdCombo(cboVltgGrdCode, "VLTG_GRD_CODE");
            //SetGrdCombo(cboCapaGrdCode, "CAPA_GRD_CODE", "G");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            /* 공용 작업조 컨트롤 초기화 */
            ucBoxShift = grdShift.Children[0] as UCBoxShift;
            txtWorker_Main = ucBoxShift.TextWorker;
            txtShift_Main = ucBoxShift.TextShift;
            ucBoxShift.ProcessCode = Process.CELL_BOXING; //작업조 팝업에 넘길 공정
            ucBoxShift.FrameOperation = this.FrameOperation;

        }

        /// <summary>
        /// 탭컨트롤 초기화
        /// </summary>
        /// <returns></returns>
        public bool ClearControls(bool bTextBoxClear = false)
        {
            try
            {
                Util.gridClear(dgWaitInbox);
                Util.gridClear(dgOutbox);

                if (bTextBoxClear)
                {
                    txtInbox1.Text = string.Empty;
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
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
        }


        #endregion

        #region Events

        #region GotFocus : text_GotFocus()
        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
        #endregion

        #region Spread 전체 선택 : dgOutbox_LoadedColumnHeaderPresenter()
        /// <summary>
        ///  전체선택
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
                            if (e.Column.HeaderPresenter == null)
                                return;
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




        #endregion

        #region  체크박스 선택 이벤트 : checkAll_Checked(), checkAll_Unchecked()

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
        /// [조회]버튼 클릭 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;
            loadingIndicator.Visibility = Visibility.Visible;
            ClearControls(false);
            InitControl();

            // Inbox TextBox 미입력 시 Outbox 조회
            if (string.IsNullOrWhiteSpace(txtInbox1.Text.Trim()))
            {
                GetCompleteOutbox();
            }
            // Inbox TextBox 입력 시 Inbox 조회 & Outbox 조회
            else
            {
                GetWaitInbox();
            }

            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region 홀수 포장 체크박스 이벤트 : chkBoxing_Checked(), chkBoxing_Unchecked() 
        /// <summary>
        /// 체크박스 체크
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void chkBoxing_Checked(object sender, RoutedEventArgs e)
        {
            // txtInbox2.IsEnabled = false;
        }
        /// <summary>
        /// 체크박스 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkBoxing_Unchecked(object sender, RoutedEventArgs e)
        {
            //txtInbox2.IsEnabled = true;
        }
        #endregion

        #region  OutBox 생성 이벤트 txtInbox1_KeyDown(), txtInbox2_KeyDown()
        /// <summary>
        /// InboxID #1 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtInbox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetWaitInbox();
            }
        }
        #endregion

        #region 라벨 재발행 : btnRePrint_Click()

        /// <summary>
        /// 라벨 재발행 이벤트
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

                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }
                if (btn.Name.Equals("btnRePrint"))
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
                        string sBizRule = "BR_PRD_GET_OUTBOX_REPRINT_FM";
                        string sBoxId = string.Empty;

                        DataSet ds = new DataSet();
                        DataTable dtIndata = ds.Tables.Add("INDATA");
                        dtIndata.Columns.Add("BOXID");
                        dtIndata.Columns.Add("LANGID");

                        DataRow dr = null;

                        foreach (int idxBox in idxBoxList)
                        {
                            dr = dtIndata.NewRow();
                            sBoxId = Util.NVC(dgOutbox.GetCell(idxBox, dgOutbox.Columns["OUTBOXID"].Index).Value);
                            dr["BOXID"] = sBoxId;
                            dr["LANGID"] = sBoxId;
                            dtIndata.Rows.Add(dr);
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

                        //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizRule, "INDATA,INBOX,INPRINT", "OUTDATA", ds);
                        DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizRule, "INDATA,INPRINT", "OUTDATA", ds);

                        if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            DataTable dtResult = dsResult.Tables["OUTDATA"];
                            string zplCode = string.Empty;
                            for (int i = 0; i < dtResult.Rows.Count; i++)
                            {
                                zplCode += dtResult.Rows[i]["ZPLCODE"].ToString();
                            }
                            PrintLabel(zplCode, _drPrtInfo);

                            DataTable dt = ((DataView)dgOutbox.ItemsSource).Table;

                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                if (dt.Rows[i]["CHK"].ToString() == "1")
                                {
                                    dt.Rows[i]["CHK"] = 0;
                                }
                            }
                            Util.GridSetData(dgOutbox, dt, FrameOperation, false);

                            if (dt != null && dt.Rows.Count > 0)
                                dgOutbox.CurrentCell = dgOutbox.GetCell(0, 1);

                            string[] sColumnName = new string[] { "BOXSEQ", "OUTBOXID", "OUTBOXQTY" };
                            if (dgOutbox.Rows.Count > 0)
                            {
                                DataGridAggregate.SetAggregateFunctions(dgOutbox.Columns["INBOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            }
                            _util.SetDataGridMergeExtensionCol(dgOutbox, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

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

        #region 완성 OUTBOX 삭제 btnBoxDelete_Click()
        /// <summary>
        /// [삭제]버튼 클릭시 이벤트
        /// BIZ : BR_PRD_DEL_INBOX_NJ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBoxDelete_Click(object sender, RoutedEventArgs e)
        {

            if (!ValidationDeleteOutBox())
                return;
            Util.MessageConfirm("SFU5000", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeleteOutbox();
                }
            });
        }
        #endregion

        #region InBox Label Reprint btnInBoxRePrint_Click
        private void btnInBoxRePrint_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                Util.MessageValidation("SFU1843");
                return;
            }

            BOX001_253_INBOX_LABEL_REPRINT popup = new BOX001_253_INBOX_LABEL_REPRINT();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = txtWorker_Main.Tag; // 작업자id

                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puInboxLabelReprint_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }

        private void puInboxLabelReprint_Closed(object sender, EventArgs e)
        {
            BOX001_253_INBOX_LABEL_REPRINT popup = sender as BOX001_253_INBOX_LABEL_REPRINT;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                //Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            }
            this.grdMain.Children.Remove(popup);
        }
        #endregion

        #endregion

        #region [Method]

        #region 대기INBOX 조회 : GetWaitInbox()
        /// <summary>
        /// 대기 INBOX 조회
        /// </summary>
        public void GetWaitInbox()
        {
            try
            {
                string sInboxID = txtInbox1.Text.Trim();
                //string sBoxType;

                if (string.IsNullOrWhiteSpace(sInboxID))
                {
                    // Inbox ID를 입력 하세요.
                    Util.MessageInfo("SFU4517");
                    txtInbox1.Focus();
                    txtInbox1.SelectAll();
                    return;
                }

                //sInboxID = Regex.Replace(sInboxID, @"[^0-9]", "");

                ////BoxID 8자리 + 방향구분자 1자리 Or BoxID 8자리
                //if (sInboxID.Length == 8)
                //{
                //    sBoxType = "TN_INBOX"; //1회용 Inbox
                //}
                ////BoxID 7자리 + 방향구분자 1자리 Or BoxID 7자리
                //else if (sInboxID.Length == 7)
                //{
                //    sBoxType = "TR_INBOX"; // 재활용 Inbox
                //}
                //else
                //{
                //    sBoxType = null;
                //}

                //if (sBoxType == null)
                //{
                //    // Inbox ID를 입력 하세요.
                //    Util.MessageInfo("SFU4517");
                //    txtInbox1.Focus();
                //    txtInbox1.SelectAll();
                //    return;
                //}

                DataSet ds = new DataSet();

                DataTable Rqstdt = ds.Tables.Add("RQSTDT");
                Rqstdt.Columns.Add("BOXID", typeof(String));
                
                DataRow dr = Rqstdt.NewRow();
                dr["BOXID"] = sInboxID.Substring(0, 19);
                
                Rqstdt.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INBOX_BY_ID_FM", "RQSTDT", "RSLTDT", Rqstdt);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INBOX_BY_ID_NFF", "RQSTDT", "RSLTDT", Rqstdt);

                if (dtResult.Rows.Count == 0)
                {
                    //조회결과가 없습니다.
                    Util.MessageInfo("SFU2951");
                    return;
                }

                Util.GridSetData(dgWaitInbox, dtResult, FrameOperation, false);

                dgWaitInbox.CurrentCell = dgWaitInbox.GetCell(0, 1);

                DataGridAggregate.SetAggregateFunctions(dgWaitInbox.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum()});
                DataGridAggregate.SetAggregateFunctions(dgWaitInbox.Columns["SUBLOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum()});

                setSOCCombo(cboSoc, dtResult.Rows[0]["PRODID"].ToString());
                GetCompleteOutbox();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }


        #endregion

        #region 완성OUTBOX 조회 : GetCompleteOutbox()
        /// <summary>
        /// 완성 OUTBOX 조회
        /// </summary>
        public void GetCompleteOutbox()
        {
            try
            {
                //chkAll.IsChecked = false;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQSGID"] = cboLine.SelectedValue.ToString();
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMPLETE_OUTBOX", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgOutbox, dtResult, FrameOperation, false);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dgOutbox.CurrentCell = dgOutbox.GetCell(0, 1);

                if (dgOutbox.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgOutbox.Columns["INBOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    string[] sColumnName = new string[] { "BOXSEQ", "OUTBOXID", "OUTBOXQTY" };

                    _util.SetDataGridMergeExtensionCol(dgOutbox, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion
        

        #region 라벨 프린트 발행 : PrintLabel()
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
        #endregion

        #region 완성 OUTBOX 생성 : RegOutBox()

        /// <summary>
        /// 완성 OUTBOX 생성 BIZ 호출
        /// </summary>
        private void RegOutBox()
        {
            try
            {
                //string sBizName = "BR_PRD_REG_OUTBOX_NFF_FM";
                string sBizName = "BR_PRD_REG_OUTBOX_NFF";
                string zplCode = string.Empty;

                // Outbox 생성 Validation
                if (!ValidationRegOutBox())
                {
                    return;
                }

                // Printer Setting 가져오기
                if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
                {
                    return;
                }

                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("USERID");
                inData.Columns.Add("SHFTID");
                inData.Columns.Add("AREAID");
                inData.Columns.Add("RSST_GRD_CODE");
                inData.Columns.Add("VLTG_GRD_CODE");
                inData.Columns.Add("CAPA_GRD_CODE");
                inData.Columns.Add("SOC_VALUE");
                inData.Columns.Add("PGM_ID");
                inData.Columns.Add("BZRULE_ID");
                inData.Columns.Add("LANGID");
                inData.Columns.Add("EQSGID");

                DataTable inInBoxTable = indataSet.Tables.Add("INBOX");
                inInBoxTable.Columns.Add("INBOXID");

                DataTable inPrintTable = indataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");

                DataRow newRow = inData.NewRow();
                newRow["USERID"] = txtWorker_Main.Tag;
                newRow["SHFTID"] = txtShift_Main.Tag;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                //newRow["RSST_GRD_CODE"] = cboRsstGrdCode.SelectedValue.ToString();
                //newRow["VLTG_GRD_CODE"] = cboVltgGrdCode.SelectedValue.ToString();
                //newRow["CAPA_GRD_CODE"] = cboCapaGrdCode.SelectedValue.ToString();
                newRow["SOC_VALUE"] = cboSoc.Text.ToString();
                newRow["PGM_ID"] = _sPGM_ID;
                newRow["BZRULE_ID"] = sBizName;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = Util.NVC(cboLine.SelectedValue); ;

                inData.Rows.Add(newRow);

                newRow = inInBoxTable.NewRow();
                newRow["INBOXID"] = txtInbox1.Text.Trim().Substring(0,19);
                inInBoxTable.Rows.Add(newRow);

                DataRow inPrintDr = inPrintTable.NewRow();
                inPrintDr["PRMK"] = _sPrt;
                inPrintDr["RESO"] = _sRes;
                inPrintDr["PRCN"] = _sCopy;
                inPrintDr["MARH"] = _sXpos;
                inPrintDr["MARV"] = _sYpos;
                inPrintDr["DARK"] = _sDark;
                inPrintTable.Rows.Add(inPrintDr);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi(sBizName, "INDATA,INBOX,INPRINT", "OUTZPL,OUTBOX", (bizResult, bizException) =>
                {
                    try
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //정상 처리 되었습니다
                        Util.MessageInfo("SFU1889");

                        for (int j = 0; j < bizResult.Tables["OUTZPL"].Rows.Count; j++)
                        {
                            zplCode = bizResult.Tables["OUTZPL"].Rows[j]["ZPLCODE"].GetString();
                            if (zplCode.Split(',')[0].Equals("1"))
                            {
                                ControlsLibrary.MessageBox.Show(zplCode.Split(',')[1], "", "Info", MessageBoxButton.OK, MessageBoxIcon.None, null);
                                return;
                            }
                            else
                            {
                                zplCode = zplCode.Substring(2);

                                PrintLabel(zplCode, _drPrtInfo);
                            }
                        }

                        InitControl();
                        ClearControls(true);
                        //GetWaitInbox();
                        GetCompleteOutbox();

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

                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
        }

        private string RePlaceZPL(string sBOXID, string sZPL)
        {
            string rtnZPL = string.Empty;

            rtnZPL = sZPL.Replace(sBOXID, sBOXID + ":ZA");
            rtnZPL = rtnZPL + sZPL.Replace(sBOXID, sBOXID + ":ZB");
            rtnZPL = rtnZPL + sZPL.Replace(sBOXID, sBOXID + ":ZC");
            rtnZPL = rtnZPL + sZPL.Replace(sBOXID, sBOXID + ":ZD");

            return rtnZPL;
        }
        #endregion

        #region 완성 OUTBOX 삭제 : DeleteOutbox()
        /// <summary>
        /// 완성 OUTBOX 삭제 BIZ
        /// </summary>
        private void DeleteOutbox()
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE");
                inData.Columns.Add("USERID");
                inData.Columns.Add("INBOX_DEL_FLAG");

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("BOXID");

                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["USERID"] = txtWorker_Main.Tag;
                newRow["INBOX_DEL_FLAG"] = "N";
                inData.Rows.Add(newRow);

                for (int i = 0; i < dgOutbox.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgOutbox.Rows[i].DataItem, "CHK")).ToString() == "1")
                    {
                        newRow = inBoxTable.NewRow();
                        newRow["BOXID"] = Util.NVC(DataTableConverter.GetValue(dgOutbox.Rows[i].DataItem, "OUTBOXID"));
                        inBoxTable.Rows.Add(newRow);
                    }

                }
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UNPACK_BOX_FOR_TESLA_NJ", "INDATA,INBOX", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU1889");
                        InitControl();
                        ClearControls(true);
                        GetWaitInbox();
                        GetCompleteOutbox();
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



        #endregion

        #region Validation
        /// <summary>
        ///  조회 Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationSearch()
        {

            if (cboLine.SelectedValue == null || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //SFU4050 라인을 선택하세요.
                Util.MessageValidation("SFU4050");
                return false;
            }

            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //SFU1673 설비를 선택하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            return true;
        }
        /// <summary>
        /// 완성 OUTBOX 생성 Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationRegOutBox()
        {
            if (cboEquipment.SelectedIndex == 0)
            {
                //설비를 선택하세요
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                //SFU1843 작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return false;
            }

            if (dgWaitInbox.GetRowCount() == 0)
            {
                //SFU4517 Inbox ID를 입력 하세요.
                Util.MessageValidation("SFU4517");
                return false;
            }

            //if (cboRsstGrdCode.SelectedValue.ToString() == "SELECT" || cboRsstGrdCode.SelectedValue.ToString() == "")
            //{
            //    //SFU4242 저항등급을 선택해 주세요.
            //    Util.MessageValidation("SFU4242");
            //    return false;
            //}

            //if (cboCapaGrdCode.SelectedValue.ToString() == "SELECT" || cboCapaGrdCode.SelectedValue.ToString() == "")
            //{
            //    //SFU4022 용량등급을 선택해 주세요.
            //    Util.MessageValidation("SFU4022");
            //    return false;
            //}

            //if (cboVltgGrdCode.SelectedValue.ToString() == "SELECT" || cboVltgGrdCode.SelectedValue.ToString() == "")
            //{
            //    //SFU4279 전압등급을 선택해 주세요.
            //    Util.MessageValidation("SFU4279");
            //    return false;
            //}

            if (cboSoc.SelectedValue.ToString() == "SELECT" || cboSoc.SelectedValue.ToString() == "")
            {
                //SFU4071 SOC를 선택하세요.
                Util.MessageValidation("SFU4071");
                return false;
            }

            return true;
        }
        /// <summary>
        /// OUTBOX 삭제 Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationDeleteOutBox()
        {
            DataRow[] dr = DataTableConverter.Convert(dgOutbox.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                // 선택한 데이터가 없습니다.
                Util.MessageValidation("SFU3538");
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
        #endregion

        #endregion

        private void btnTagPrint2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnInit2_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 초기화 하시겠습니까?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    AllClear2(true);
                }

            });
        }

        private void AllClear2(bool bAll = false)
        {
            if (bAll)
            {
                txtTagPallet2.Text = string.Empty;
                txtReason2.Text = string.Empty;
            }
            ClearDataGrid(dgPallet2);
            ClearDataGrid(dgSourceCell2);

            txtPalletID2.Text = string.Empty;
            txtSourceCellID2.Text = string.Empty;

            chkNewBox.IsChecked = false;

            txtNewBoxID.Text = string.Empty;
            txtNewBoxID.IsReadOnly = false;

            txtgridqty.Value = 1;
        }

        private void ClearDataGrid(C1.WPF.DataGrid.C1DataGrid dg)
        {
            dg.ItemsSource = null;
        }

        private void btnChange2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(bool)chkNewBox.IsChecked)
                {
                    if (dgPallet2.GetRowCount() == 0)
                    {
                        Util.MessageValidation("SFU1801"); //"입력 데이터가 존재하지 않습니다."
                        return;
                    }
                }
                
                if (dgSourceCell2.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1801"); //"입력 데이터가 존재하지 않습니다."
                    return;
                }

                if (!(bool)chkNewBox.IsChecked)
                {
                    if (txtReason2.Text == string.Empty)
                    {
                        txtReason2.Focus();
                        Util.MessageValidation("SFU1252"); //"교체 사유는 필수 입니다."
                        return;
                    }
                }
                
                if (string.IsNullOrEmpty(txtWorker_Main.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    return;
                }

                //계속 진행하시겠습니까? >> 작업을 진행하시겠습니까?
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1170"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (msgresult) =>
                Util.MessageConfirm("SFU1170", (msgresult) =>
                {
                    if (msgresult == MessageBoxResult.OK)
                    {
                        if ((bool)chkNewBox.IsChecked)
                        {
                            CreateInbox();
                        }
                        else
                        {
                            AddCell();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CreateInbox()
        {
            try
            {
                if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
                    return;

                string sBzRuleID = "BR_PRD_REG_INBOX_NFF_UI";

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("IN_EQPT");
                DataTable inSublotTable = indataSet.Tables.Add("IN_SUBLOT");
                DataTable inPrintTable = indataSet.Tables.Add("INPRINT");

                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("LABELTYPE", typeof(string));
                inDataTable.Columns.Add("PGM_ID", typeof(string));
                inDataTable.Columns.Add("BZRULE_ID", typeof(string));

                inSublotTable.Columns.Add("SUBLOTID", typeof(string));
                inSublotTable.Columns.Add("FORM_TRAY_PSTN_NO", typeof(string));

                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["USERID"] = txtWorker_Main.Tag as string;
                dr["EQSGID"] = Util.NVC(cboLine1.SelectedValue);
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["LABELTYPE"] = "NFF_INBOX";
                dr["PGM_ID"] = _sPGM_ID;
                dr["BZRULE_ID"] = sBzRuleID;
                inDataTable.Rows.Add(dr);
                
                string sublot = string.Empty;
                string position = string.Empty;
                string zplCode = string.Empty;
                string sBoxID = string.Empty;

                for (int i = 0; i < dgSourceCell2.GetRowCount(); i++)
                {
                    sublot = Util.NVC(DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "SUBLOTID")).Trim();
                    position = Util.NVC(DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "FORM_TRAY_PSTN_NO")).Trim();

                    if (string.IsNullOrWhiteSpace(sublot))
                    {
                        Util.MessageInfo("SFU1495"); //"대상 Cell ID가 입력되지 않았습니다."
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(position))
                    {
                        Util.MessageInfo("SFU1234"); // 위치가 입력되지 않았습니다.
                        return;
                    }
                    if (position.Length != 3)
                    {
                        Util.MessageInfo("SFU1234"); // 위치가 입력되지 않았습니다.
                        return;
                    }

                    Regex regex = new Regex(@"^[A-Z]");
                    Regex regex2 = new Regex(@"^[0-9]");
                    Boolean ismatch = regex.IsMatch(position.ToString().Substring(0, 1));
                    Boolean ismatch2 = regex2.IsMatch(position.ToString().Substring(1, 2));

                    if (!ismatch || !ismatch2)
                    {
                        Util.MessageInfo("SFU1234"); // 위치가 입력되지 않았습니다.
                        return;
                    }

                    string matchpstn = Regex.Replace(position, @"[^A-Z0-9]", "");

                    if (matchpstn != position)
                    {
                        // 숫자와 영문대문자만 입력가능합니다.
                        Util.MessageValidation("SFU3674");
                        return;
                    }

                    DataRow drsub = inSublotTable.NewRow();
                    drsub["SUBLOTID"] = sublot;
                    drsub["FORM_TRAY_PSTN_NO"] = position;

                    inSublotTable.Rows.Add(drsub);
                }

                dr = inPrintTable.NewRow();
                dr["PRMK"] = _sPrt; // "ZEBRA"; Print type
                dr["RESO"] = _sRes; // "203"; DPI
                dr["PRCN"] = _sCopy; // "1"; Print Count
                dr["MARH"] = _sXpos; // "0"; Horizone pos
                dr["MARV"] = _sYpos; // "0"; Vertical pos
                dr["DARK"] = _sDark; // darkness
                inPrintTable.Rows.Add(dr);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_INBOX_NFF_UI", "IN_EQPT,IN_SUBLOT,INPRINT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //정상 처리 되었습니다
                        Util.MessageInfo("SFU1889");

                        zplCode = bizResult.Tables["OUTDATA"].Rows[0]["ZPLCODE"].GetString();
                        sBoxID = bizResult.Tables["OUTDATA"].Rows[0]["BOXID"].GetString();

                        if (zplCode.Split(',')[0].Equals("1"))
                        {
                            ControlsLibrary.MessageBox.Show(zplCode.Split(',')[1], "", "Info", MessageBoxButton.OK, MessageBoxIcon.None, null);
                            return;
                        }
                        else
                        {
                            zplCode = zplCode.Substring(2);
                            zplCode = RePlaceZPL(sBoxID, zplCode);
                            PrintLabel(zplCode, _drPrtInfo);
                        }

                        txtPalletID2.IsReadOnly = false;
                        txtPalletID2.Focus();
                        txtPalletID2.SelectAll();

                        chkNewBox.IsChecked = false;
                        cboLine.Visibility = Visibility.Collapsed;
                        cboLinetxt.Visibility = Visibility.Collapsed;

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

                }, indataSet);


                //new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_INBOX_NFF_UI", "IN_EQPT,IN_SUBLOT,INPRINT", "OUTDATA", indataSet);

                //// "정상적으로 처리하였습니다." >>정상처리되었습니다.
                //Util.MessageInfo("SFU1275", (result) =>
                //{
                //    if (result == MessageBoxResult.OK)
                //    {

                //        txtPalletID2.IsReadOnly = false;
                //        txtPalletID2.Focus();
                //        txtPalletID2.SelectAll();

                //        chkNewBox.IsChecked = false;
                //        //txtNewBox.Visibility = Visibility.Collapsed;
                //        //txtNewBoxID.Visibility = Visibility.Collapsed;
                //        cboLine.Visibility = Visibility.Collapsed;
                //        cboLinetxt.Visibility = Visibility.Collapsed;
                //    }
                //});

                //txtTagPallet2.Text = sPalletID;
                AllClear2(true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void AddCell()
        {
            try
            {
                string sBizName = string.Empty;

                sBizName = "BR_PRD_REG_ADD_SUBLOT_FM";

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");

                string sPalletID = string.Empty;

                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("BOXID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("SRCTYPE", typeof(string));

                inSublotTable.Columns.Add("SUBLOTID", typeof(string));
                inSublotTable.Columns.Add("FORM_TRAY_PSTN_NO", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["USERID"] = txtWorker_Main.Tag as string;
                dr["BOXID"] = sPalletID = Util.NVC(DataTableConverter.GetValue(dgPallet2.CurrentRow.DataItem, "BOXID"));
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inDataTable.Rows.Add(dr);

                string sublot = string.Empty;
                string position = string.Empty;

                for (int i = 0; i < dgSourceCell2.GetRowCount(); i++)
                {
                    #region Cell 입력 체크 및 위치 입력 체크
                    sublot = Util.NVC(DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "SUBLOTID")).Trim();
                    position = Util.NVC(DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "FORM_TRAY_PSTN_NO")).Trim();

                    if (string.IsNullOrWhiteSpace(sublot))
                    {
                        Util.MessageInfo("SFU1495"); //"대상 Cell ID가 입력되지 않았습니다."
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(position))
                    {
                        Util.MessageInfo("SFU1234"); // 위치가 입력되지 않았습니다.
                        return;
                    }
                    if (position.Length != 3)
                    {
                        Util.MessageInfo("SFU1234"); // 위치가 입력되지 않았습니다.
                        return;
                    }

                    Regex regex = new Regex(@"^[A-Z]");
                    Regex regex2 = new Regex(@"^[0-9]");
                    Boolean ismatch = regex.IsMatch(position.ToString().Substring(0, 1));
                    Boolean ismatch2 = regex2.IsMatch(position.ToString().Substring(1, 2));

                    if (!ismatch || !ismatch2)
                    {
                        Util.MessageInfo("SFU1234"); // 위치가 입력되지 않았습니다.
                        return;
                    }

                    string matchpstn = Regex.Replace(position, @"[^A-Z0-9]", "");

                    if (matchpstn != position)
                    {
                        // 숫자와 영문대문자만 입력가능합니다.
                        Util.MessageValidation("SFU3674");
                        return;
                    }

                    #endregion

                    DataRow drsub = indataSet.Tables["INSUBLOT"].NewRow();
                    drsub["SUBLOTID"] = sublot;
                    drsub["FORM_TRAY_PSTN_NO"] = position;

                    indataSet.Tables["INSUBLOT"].Rows.Add(drsub);
                }

                new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA,INSUBLOT", string.Empty, indataSet);

                // "정상적으로 처리하였습니다." >>정상처리되었습니다.
                Util.MessageInfo("SFU1275", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtPalletID2.Focus();
                        txtPalletID2.SelectAll();
                    }
                });

                txtTagPallet2.Text = sPalletID;
                AllClear2(true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtPalletID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sPalletID = string.Empty;
                    txtTagPallet2.Text = string.Empty;
                    sPalletID = txtPalletID2.Text.Trim();

                    if (sPalletID == null)
                    {
                        //조회할 BOX ID 를 입력하세요.
                        Util.MessageValidation("SFU1189");
                        return;
                    }

                    if (GetPalletInfo(dgPallet2, sPalletID))
                    {
                        txtSourceCellID2.Focus();
                        txtSourceCellID2.SelectAll();
                    }
                    else
                    {
                        txtPalletID2.SelectAll();
                    }

                    ClearDataGrid(dgSourceCell2);


                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
            }
        }

        private bool GetPalletInfo(C1.WPF.DataGrid.C1DataGrid grid, string sPalletID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("SHOPID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["BOXID"] = sPalletID.Substring(0,19);

                RQSTDT.Rows.Add(dr);

                DataTable dtPallet = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_BOX_INFO_FOR_UNPACK_NJ", "INDATA", "OUTDATA", RQSTDT);

                if (dtPallet == null || dtPallet.Rows.Count == 0)
                {
                    //BOX 정보가 없습니다.
                    Util.MessageInfo("SFU1180");
                    return false;
                }
                Util.GridSetData(grid, dtPallet, FrameOperation);

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void chkNewBox_Checked(object sender, RoutedEventArgs e)
        {
            txtPalletID2.IsReadOnly = true;
            //txtNewBox.Visibility = Visibility.Visible;
            //txtNewBoxID.Visibility = Visibility.Visible;

            cboLine1.Visibility = Visibility.Visible;
            cboLinetxt.Visibility = Visibility.Visible;

            //btnChange2.Content = ObjectDic.Instance.GetObjectName("저장");
        }

        private void chkNewBox_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPalletID2.IsReadOnly = false;
            //txtNewBoxID.IsReadOnly = false;
            //txtNewBoxID.Text = string.Empty;

            //txtNewBox.Visibility = Visibility.Collapsed;
            //txtNewBoxID.Visibility = Visibility.Collapsed;

            cboLine1.Visibility = Visibility.Collapsed;
            cboLinetxt.Visibility = Visibility.Collapsed;

            //btnChange2.Content = ObjectDic.Instance.GetObjectName("추가");

        }

        private void txtSourceCellID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetSourceCellID2(txtSourceCellID2.Text.Trim());
            }
        }

        private bool SetSourceCellID2(string sSouceCellId)
        {
            try
            {

                if (chkNewBox.IsChecked == false)
                {
                    if (dgPallet2.GetRowCount() == 0)
                    {
                        //BoxId 먼저 입력하세요
                        Util.MessageValidation("SFU3387");
                        return false;
                    }
                }

                if (sSouceCellId == null)
                {
                    //교체 Cell ID 가 없습니다.
                    Util.MessageValidation("SFU1462");
                    return false;
                }

                if (string.IsNullOrEmpty(txtWorker_Main.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    return false;
                }

                DataTable dtInfo = DataTableConverter.Convert(dgSourceCell2.ItemsSource);

                if (dtInfo.Rows.Count > 0)
                {
                    DataRow[] drList = dtInfo.Select("SUBLOTID = '" + sSouceCellId + "'");

                    if (drList.Length > 0)
                    {
                        //중북 체크
                        ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtSourceCellID2.Focus();
                                txtSourceCellID2.Text = string.Empty;
                            }
                        });

                        txtSourceCellID2.Text = string.Empty;
                        return false;
                    }
                }

                string sBizName = string.Empty;

                sBizName = "BR_PRD_CHK_SUBLOT_FOR_ADD_BOX_FM";

                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("SUBLOTID");
                RQSTDT.Columns.Add("USERID");
                RQSTDT.Columns.Add("SHOPID");
                RQSTDT.Columns.Add("AREAID");
                RQSTDT.Columns.Add("EQSGID");

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = sSouceCellId;
                dr["USERID"] = txtWorker_Main.Tag as string;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = null;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", RQSTDT);

                dtInfo.Merge(dtRslt);

                Util.GridSetData(dgSourceCell2, dtInfo, FrameOperation, true);


                return true;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                txtSourceCellID2.Focus();
                txtSourceCellID2.SelectAll();
            }
        }

        private void txtSourceCellID2_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && SetSourceCellID2(sPasteStrings[i].Trim()) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }

        private void txtNewBoxID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string NewBox = txtNewBoxID.Text.Trim();

                    if (string.IsNullOrWhiteSpace(NewBox))
                    {
                        // Inbox ID를 입력 하세요.
                        Util.MessageInfo("SFU4517");
                        txtNewBoxID.Focus();
                        txtNewBoxID.SelectAll();
                        return;
                    }

                    NewBox = Regex.Replace(NewBox, @"[^0-9]", "");

                    //BoxID 8자리 + 방향구분자 1자리 Or BoxID 8자리
                    if (NewBox.Length == 8)
                    {
                        _BoxType = "N"; //1회용 Inbox
                    }
                    //BoxID 8자리 + 방향구분자 1자리 Or BoxID 7자리
                    else if (NewBox.Length == 7)
                    {
                        _BoxType = "R"; // 재활용 Inbox
                    }
                    else
                    {
                        _BoxType = null;
                    }

                    if (_BoxType == null)
                    {
                        // Inbox ID를 입력 하세요.
                        Util.MessageInfo("SFU4517");
                        txtNewBoxID.Focus();
                        txtNewBoxID.SelectAll();
                        return;
                    }

                    DataSet ds = new DataSet();

                    DataTable dtEqp = ds.Tables.Add("IN_EQP");
                    dtEqp.Columns.Add("SRCTYPE", typeof(String));
                    dtEqp.Columns.Add("IFMODE", typeof(String));
                    dtEqp.Columns.Add("EQPTID", typeof(String));
                    dtEqp.Columns.Add("USERID", typeof(String));

                    DataTable dtBox = ds.Tables.Add("IN_BOX");
                    dtBox.Columns.Add("TESLA_TRAY_ID", typeof(String));
                    dtBox.Columns.Add("TRAY_TYPE", typeof(String));

                    DataRow dr = dtEqp.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["IFMODE"] = IFMODE.IFMODE_OFF;
                    dr["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                    dr["USERID"] = string.Empty;

                    dtEqp.Rows.Add(dr);

                    DataRow dr1 = dtBox.NewRow();
                    dr1["TESLA_TRAY_ID"] = Util.NVC(NewBox);
                    dr1["TRAY_TYPE"] = Util.NVC(_BoxType);

                    dtBox.Rows.Add(dr1);

                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_DUP_BOX_NJ", "IN_EQP,IN_BOX", "OUTDATA", ds);

                    txtNewBoxID.Text = (string)dsResult.Tables["OUTDATA"].Rows[0]["BOXID"];

                    txtNewBoxID.IsReadOnly = true;

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkNewBox.IsChecked)
            {
                if (dgPallet2.GetRowCount() == 0)
                {
                    // Inbox ID를 입력 하세요.
                    Util.MessageInfo("SFU4517");
                    txtPalletID2.Focus();
                    return;
                }
            }
            else
            {
                //if (string.IsNullOrWhiteSpace(txtNewBoxID.Text.Trim()))
                //{
                //    // Inbox ID를 입력 하세요.
                //    Util.MessageInfo("SFU4517");
                //    txtNewBoxID.Focus();
                //    return;
                //}
            }


            DataTable dtList = new DataTable();

            dtList.Columns.Add("SUBLOTID");
            dtList.Columns.Add("FORM_TRAY_PSTN_NO");

            //TextBox에 입력 된 수량만큼 Row 추가
            for (int i = 0; i < txtgridqty.Value; i++)
            {
                DataRow newRow = dtList.NewRow();
                newRow["SUBLOTID"] = string.Empty;
                newRow["FORM_TRAY_PSTN_NO"] = string.Empty;
                dtList.Rows.Add(newRow);
            }

            dgSourceCell2.ItemsSource = DataTableConverter.Convert(dtList);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ClearDataGrid(dgSourceCell2);
        }

        /// <summary>
        /// 엑셀 업로드 양식 다운로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDownLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog od = new SaveFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    od.InitialDirectory = @"\\Client\C$";
                }
                od.Filter = "Excel Files (.xlsx)|*.xlsx";
                od.FileName = "Add_Cell_Upload_Sample.xlsx";

                if (od.ShowDialog() == true)
                {
                    C1XLBook c1XLBook1 = new C1XLBook();
                    XLSheet sheet = c1XLBook1.Sheets[0];
                    XLStyle styel = new XLStyle(c1XLBook1);
                    styel.AlignHorz = XLAlignHorzEnum.Center;

                    sheet[0, 0].Value = "CELLID";
                    sheet[1, 0].Value = "CELLID_001";
                    sheet[2, 0].Value = "CELLID_002";

                    sheet[0, 1].Value = "POSITION";
                    sheet[1, 1].Value = "A01";
                    sheet[2, 1].Value = "A02";

                    sheet[0, 0].Style = sheet[0, 1].Style = styel;
                    sheet.Columns[0].Width = sheet.Columns[1].Width = 1500;

                    c1XLBook1.Save(od.FileName);

                    //   if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] != "SBC")
                    System.Diagnostics.Process.Start(od.FileName);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 엑셀 업로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((bool)chkNewBox.IsChecked)
                {
                    if (string.IsNullOrWhiteSpace(txtNewBoxID.Text.Trim()) && txtNewBoxID.IsReadOnly == true)
                    {
                        //BoxId 먼저 입력하세요
                        Util.MessageValidation("SFU3387");
                        return;
                    }
                }
                else
                {
                    if (dgPallet2.GetRowCount() == 0)
                    {
                        //BoxId 먼저 입력하세요
                        Util.MessageValidation("SFU3387");
                        return;
                    }
                }


                if (dgSourceCell2.ItemsSource == null)
                {
                    InitCellList(dgSourceCell2);
                }
                DataTable dtInfo = DataTableConverter.Convert(dgSourceCell2.ItemsSource);

                dtInfo.Clear();


                OpenFileDialog fd = new OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";

                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        C1XLBook book = new C1XLBook();
                        book.Load(stream, FileFormat.OpenXml);
                        XLSheet sheet = book.Sheets[0];

                        if (sheet.GetCell(0, 0).Text != "CELLID" || sheet.GetCell(0, 1).Text != "POSITION")
                        {
                            //SFU4424	형식에 맞는 EXCEL파일을 선택해 주세요.
                            Util.MessageValidation("SFU4424");
                            return;
                        }

                        for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            // CELLID, POSITION 미입력시 return;
                            if (sheet.GetCell(rowInx, 0) == null || sheet.GetCell(rowInx, 1) == null)
                                return;

                            DataRow dr = dtInfo.NewRow();
                            dr["SUBLOTID"] = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();
                            dr["FORM_TRAY_PSTN_NO"] = sheet.GetCell(rowInx, 1).Text.Trim();
                            dtInfo.Rows.Add(dr);
                        }

                        if (dtInfo.Rows.Count > 0)
                            dtInfo = dtInfo.DefaultView.ToTable(true);

                        Util.GridSetData(dgSourceCell2, dtInfo, FrameOperation);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitCellList(C1.WPF.DataGrid.C1DataGrid dg)
        {
            DataTable dtInit = new DataTable();

            dtInit.Columns.Add("SUBLOTID", typeof(string));
            dtInit.Columns.Add("FORM_TRAY_PSTN_NO", typeof(string));

            dg.ItemsSource = DataTableConverter.Convert(dtInit);
            ////Util.GridSetData(dgCellList, dtInit, FrameOperation);
        }

        private void btnDelete2_Click(object sender, RoutedEventArgs e)
        {
            //삭제하시겠습니까?
            // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    dgSourceCell2.IsReadOnly = false;
                    dgSourceCell2.RemoveRow(index);


                    for (int cnt = 0; cnt < dgSourceCell2.GetRowCount(); cnt++)
                    {
                        DataTableConverter.SetValue(dgSourceCell2.Rows[cnt].DataItem, "SEQ", cnt + 1);
                    }

                    dgSourceCell2.IsReadOnly = true;
                }
            });
        }

        private void btnInit3_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 초기화 하시겠습니까?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    AllClear3(true);
                }
            });
        }

        private void btnChange3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgPallet3.GetRowCount() == 0 || dgSourceCell3.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1801"); //"입력 데이터가 존재하지 않습니다."
                    return;
                }

                if (txtReason3.Text == string.Empty)
                {
                    txtReason3.Focus();
                    Util.MessageValidation("SFU1252"); //"교체 사유는 필수 입니다."
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker_Main.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    return;
                }

                //계속 진행하시겠습니까? >> 작업을 진행하시겠습니까?
                Util.MessageConfirm("SFU1170", (msgresult) =>
                {
                    if (msgresult == MessageBoxResult.OK)
                    {
                        try
                        {
                            DataSet indataSet = new DataSet();
                            DataTable inDataTable = indataSet.Tables.Add("INDATA");
                            DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");

                            string sPalletID = string.Empty;

                            inDataTable.Columns.Add("USERID", typeof(string));
                            inDataTable.Columns.Add("BOXID", typeof(string));

                            inSublotTable.Columns.Add("SUBLOTID", typeof(string));

                            DataRow dr = inDataTable.NewRow();
                            dr["USERID"] = txtWorker_Main.Tag as string;
                            dr["BOXID"] = sPalletID = Util.NVC(DataTableConverter.GetValue(dgPallet3.CurrentRow.DataItem, "BOXID"));

                            inDataTable.Rows.Add(dr);

                            string sublot = string.Empty;


                            for (int i = 0; i < dgSourceCell3.GetRowCount(); i++)
                            {
                                sublot = Util.NVC(DataTableConverter.GetValue(dgSourceCell3.Rows[i].DataItem, "SUBLOTID")).Trim();

                                if (string.IsNullOrWhiteSpace(sublot))
                                {
                                    Util.MessageInfo("SFU1495"); //"대상 Cell ID가 입력되지 않았습니다."
                                    return;
                                }

                                DataRow drsub = indataSet.Tables["INSUBLOT"].NewRow();
                                drsub["SUBLOTID"] = sublot;

                                indataSet.Tables["INSUBLOT"].Rows.Add(drsub);
                            }

                            new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_REMOVE_SUBLOT_NJ", "INDATA,INSUBLOT", string.Empty, indataSet);

                            // "정상적으로 처리하였습니다." >>정상처리되었습니다.
                            Util.MessageInfo("SFU1275", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtPalletID3.Focus();
                                    txtPalletID3.SelectAll();
                                }
                            });

                            txtTagPallet3.Text = sPalletID;
                            AllClear3(true);

                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtPalletID3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sPalletID = string.Empty;
                    txtTagPallet3.Text = string.Empty;

                    sPalletID = txtPalletID3.Text.ToString().Trim();

                    if (sPalletID == null)
                    {
                        //Pallet ID 가 없습니다. >> PALLETID를 입력해주세요
                        Util.MessageValidation("SFU1411");
                        // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1411"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    GetPalletInfo(dgPallet3, sPalletID);

                    ClearDataGrid(dgSourceCell3);
                    txtSourceCellID3.Focus();
                    txtSourceCellID3.SelectAll();
                    txtSourceCellID3.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        private bool SetSourceCellID3(string sSourceCellId)
        {
            try
            {
                if (dgPallet3.GetRowCount() == 0)
                {
                    //BoxId 먼저 입력하세요
                    Util.MessageValidation("SFU3387");
                    return false;
                }

                if (string.IsNullOrEmpty(sSourceCellId))
                {
                    //교체 Cell ID 가 없습니다.
                    Util.MessageValidation("SFU1462");
                    return false;
                }

                if (string.IsNullOrEmpty(txtWorker_Main.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    return false;
                }

                DataTable dtInfo = DataTableConverter.Convert(dgSourceCell3.ItemsSource);

                if (dtInfo.Rows.Count > 0)
                {
                    DataRow[] drList = dtInfo.Select("SUBLOTID = '" + sSourceCellId + "'");

                    if (drList.Length > 0)
                    {
                        ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtSourceCellID3.Focus();
                                txtSourceCellID3.Text = string.Empty;
                            }
                        });

                        txtSourceCellID3.Text = string.Empty;
                        return false;
                    }
                }

                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("SUBLOTID");
                RQSTDT.Columns.Add("BOXID");

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = sSourceCellId;
                dr["BOXID"] = Util.NVC(DataTableConverter.GetValue(dgPallet3.CurrentRow.DataItem, "BOXID"));

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_SUBLOT_FOR_REMOVE_BOX_NJ", "INDATA", "OUTDATA", RQSTDT);

                dtInfo.Merge(dtRslt);

                Util.GridSetData(dgSourceCell3, dtInfo, FrameOperation, true);

                return true;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                txtSourceCellID3.Focus();
                txtSourceCellID3.SelectAll();
            }
        }

        private void txtSourceCellID3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetSourceCellID3(txtSourceCellID3.Text.Trim());
            }
        }

        private void AllClear3(bool bAll = false)
        {
            if (bAll)
            {
                txtTagPallet3.Text = string.Empty;
                txtReason3.Text = string.Empty;
            }
            ClearDataGrid(dgPallet3);
            ClearDataGrid(dgSourceCell3);


            txtPalletID3.Text = string.Empty;
            txtSourceCellID3.Text = string.Empty;
        }

        private void txtSourceCellID3_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && SetSourceCellID3(sPasteStrings[i].Trim()) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                e.Handled = true;
            }
        }

        private void SetGrdCombo(C1ComboBox cbo, string GrdTypeCode, string QltyTypeCode = null)
        {
            const string bizRuleName = "DA_PRD_SEL_FORM_GRADE_TYPE_CODE_LINE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "EQSGID", "QLTY_TYPE_CODE", "CELL_GRD_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, LoginInfo.CFG_EQSG_ID, QltyTypeCode, GrdTypeCode };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText);
        }

        private void setSOCCombo(C1ComboBox cbo, string prodID)
        {
            if (!string.IsNullOrWhiteSpace(prodID))
            {
                const string bizRuleName = "DA_BAS_SEL_PRDT_SOC_VLTG_COMBO";
                string[] arrColumn = { "SHOPID", "PRODID" };
                string[] arrCondition = { LoginInfo.CFG_SHOP_ID, prodID };
                string selectedValueText = cbo.SelectedValuePath;
                string displayMemberText = cbo.DisplayMemberPath;

                CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            RegOutBox();
        }

        private void btnCreateInbox_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                Util.MessageValidation("SFU1843");
                return;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //SFU1223 라인을 선택 하세요
                Util.MessageValidation("SFU1223");
                return;
            }

            BOX001_253_INBOX_LABEL popup = new BOX001_253_INBOX_LABEL();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = Util.NVC(cboLine.SelectedValue);
                Parameters[1] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[2] = txtWorker_Main.Tag; // 작업자id

                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puInboxLabel_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }

        private void puInboxLabel_Closed(object sender, EventArgs e)
        {
            BOX001_253_INBOX_LABEL popup = sender as BOX001_253_INBOX_LABEL;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            }
            this.grdMain.Children.Remove(popup);
        }

        
    }
}
