/*************************************************************************************
 Created Date : 2018-08-14
      Creator : 오화백K
   Decription : OutBox 생성 
--------------------------------------------------------------------------------------
 [Change History]
  2018.08.14  DEVELOPER : Initial Created.

  
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

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_235 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 
        
        Util _util = new Util();
     
        private string _searchStat = string.Empty;
        private bool bInit = true;

        string _sPGM_ID = "BOX001_235";

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

        public BOX001_235()
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
           _combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID, "MCC,MCR,MCS", Process.CELL_BOXING, LoginInfo.CFG_AREA_ID }, sCase: "LINEBYSHOP");
           _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, null, sFilter: new string[] {cboLine.SelectedValue.ToString(), Process.CELL_BOXING, null}, sCase: "EQUIPMENT");
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
        public bool ClearControls()
        {
            try
            {
                Util.gridClear(dgWaitInbox);
                Util.gridClear(dgOutbox);
             
                txtInbox1.Text = string.Empty;
                txtInbox2.Text = string.Empty;
                //cboEquipment.SelectedIndex = 0;
                //txtShipto.Text = string.Empty;

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

        #region 조회 : btnSearch_Click(), cboLine_SelectedValueChanged()

        /// <summary>
        /// [조회]버튼 클릭 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;
            loadingIndicator.Visibility = Visibility.Visible;
            ClearControls();
            InitControl();
            cboEquipment.SelectedIndex = 0;
            txtShipto.Text = string.Empty;
            chkBoxing.IsChecked = false;
            chkLabel.IsChecked = true;
            GetWaitInbox();
        
            loadingIndicator.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// [조회] 라인 콤보박스 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboLine_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
             
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

        #region 출하처 조회  : cboEquipment_SelectedValueChanged()
        /// <summary>
        /// 설비콤보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetEquipmentShipTo();
        }

        #endregion

        #region 출하처 설정  : btnShipTo_Click(), PopShipTo_Closed()
        /// <summary>
        /// 출하처 설정 팝업 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnShipTo_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex == 0)
            {
                // "설비를 선택 하세요."
                Util.MessageValidation("SFU1673");
                return;
            }
            SettingShipTo();
        }
        /// <summary>
        /// 출하처 팝업 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PopShipTo_Closed(object sender, EventArgs e)
        {
            BOX001_235_SHIPTO_SETTING window = sender as BOX001_235_SHIPTO_SETTING;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetEquipmentShipTo();
            }
            this.grdMain.Children.Remove(window);
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
            txtInbox2.IsEnabled = false;
        }
        /// <summary>
        /// 체크박스 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkBoxing_Unchecked(object sender, RoutedEventArgs e)
        {
            txtInbox2.IsEnabled = true;
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
                if (chkBoxing.IsChecked == true)
                {
                    RegOutBox();
                    txtInbox1.Focus();
                }
                else
                {
                    txtInbox2.Focus();
                }
            }
        }
        /// <summary>
        /// InboxID #2 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtInbox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RegOutBox();
                txtInbox1.Focus();
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
                        string sBizRule = "BR_PRD_GET_OUTBOX_REPRT_NJ";

                        DataSet ds = new DataSet();
                        DataTable dtIndata = ds.Tables.Add("INDATA");
                        dtIndata.Columns.Add("LANGID");
                        dtIndata.Columns.Add("USERID");
                        dtIndata.Columns.Add("PGM_ID");    //라벨 이력 저장용
                        dtIndata.Columns.Add("BZRULE_ID"); //라벨 이력 저장용

                        DataRow dr = null;
                        dr = dtIndata.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["USERID"] = txtWorker_Main.Tag;
                        dr["PGM_ID"] = _sPGM_ID;
                        dr["BZRULE_ID"] = sBizRule;
                        dtIndata.Rows.Add(dr);

                        DataTable dtInbox = ds.Tables.Add("INBOX");
                        dtInbox.Columns.Add("BOXID");

                        if (btn.Name.Equals("btnRePrint"))
                        {
                            foreach (int idxBox in idxBoxList)
                            {
                                dr = dtInbox.NewRow();
                                string sBoxId = Util.NVC(dgOutbox.GetCell(idxBox, dgOutbox.Columns["OUTBOXID"].Index).Value);
                                dr["BOXID"] = sBoxId;
                                dtInbox.Rows.Add(dr);
                            }
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

                        //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_OUTBOX_REPRT_NJ", "INDATA,INBOX,INPRINT", "OUTDATA", ds);
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

                            DataTable dt = ((DataView)dgOutbox.ItemsSource).Table;
                           
                            for(int i=0; i< dt.Rows.Count; i++)
                            {
                                if(dt.Rows[i]["CHK"].ToString() == "1")
                                {
                                    dt.Rows[i]["CHK"] = 0; 
                                }
                            }


                            Util.GridSetData(dgOutbox, dt, FrameOperation, false);

                            if (dt != null && dt.Rows.Count > 0)
                                dgOutbox.CurrentCell = dgOutbox.GetCell(0, 1);

                            string[] sColumnName = new string[] { "CHK", "OUTBOXID2", "BOXSEQ", "OUTBOXID", "OUTBOXQTY" };
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
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQSGID"] = cboLine.SelectedValue.ToString();
                newRow["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_INBOX", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.GridSetData(dgWaitInbox, bizResult, FrameOperation, true);
                        if (bizResult != null && bizResult.Rows.Count > 0)
                            dgWaitInbox.CurrentCell = dgWaitInbox.GetCell(0, 1);

                        if (dgWaitInbox.Rows.Count > 0)
                        {
                            DataGridAggregate.SetAggregateFunctions(dgWaitInbox.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        }
                        GetCompleteOutbox();

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

                string[] sColumnName = new string[] { "CHK", "OUTBOXID2", "BOXSEQ", "OUTBOXID", "OUTBOXQTY" };
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

        #region 설비에 따른 출하처 조회 : GetEquipmentShipTo()
        public void GetEquipmentShipTo()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQUIPMENT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQUIPMENT"] = cboEquipment.SelectedValue.ToString();

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_SHIP_TO", "INDATA", "OUTDATA", inTable);


                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    txtShipto.Text = dtResult.Rows[0]["BOXING_SHIPTO_NAME"].ToString();
                    txtShipto.Tag = dtResult.Rows[0]["BOXING_SHIPTO_ID"].ToString();
                }
                else
                {
                    txtShipto.Text = string.Empty;
                    txtShipto.Tag = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region 출하처 설정 팝업 : SettingShipTo()
        private void SettingShipTo()
        {
            try
            {
                BOX001_235_SHIPTO_SETTING PopShipTo = new BOX001_235_SHIPTO_SETTING();
                PopShipTo.FrameOperation = FrameOperation;

                if (PopShipTo != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = cboLine.SelectedValue.ToString();// 라인정보
                    Parameters[1] = cboEquipment.SelectedValue.ToString(); // 설비정보
                    Parameters[2] = txtShipto.Text.ToString();// 출하처

                    C1WindowExtension.SetParameters(PopShipTo, Parameters);

                    PopShipTo.Closed += new EventHandler(PopShipTo_Closed);
                    grdMain.Children.Add(PopShipTo);
                    PopShipTo.BringToFront();
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
                if (!ValidationRegOutBox())
                {
                    return;
                }

                string zplCode = string.Empty;
                if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
                    return;


                DataSet indataSet = new DataSet();
                DataTable inEQP = indataSet.Tables.Add("IN_EQP");
                inEQP.Columns.Add("SRCTYPE");
                inEQP.Columns.Add("IFMODE");
                inEQP.Columns.Add("EQPTID");
                inEQP.Columns.Add("USERID");

                DataTable inBoxTable = indataSet.Tables.Add("IN_BOX");
                inBoxTable.Columns.Add("INBOXID1");
                inBoxTable.Columns.Add("INBOXID2");


                DataTable inPrintTable = indataSet.Tables.Add("IN_PRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");


                DataRow newRow = inEQP.NewRow();
                newRow["SRCTYPE"] = "UI";
                newRow["IFMODE"] = "OFF";
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["USERID"] = txtWorker_Main.Tag;
                inEQP.Rows.Add(newRow);

                newRow = inBoxTable.NewRow();
                if (chkBoxing.IsChecked == true)
                {
                    newRow["INBOXID1"] = txtInbox1.Text.ToString();
                    newRow["INBOXID2"] = "DUMMY00000";

                }
                else
                {
                    newRow["INBOXID1"] = txtInbox1.Text.ToString();
                    newRow["INBOXID2"] = txtInbox2.Text.ToString();
                }
                inBoxTable.Rows.Add(newRow);


                newRow = inPrintTable.NewRow();
                newRow["PRMK"] = _sPrt;
                newRow["RESO"] = _sRes;
                newRow["PRCN"] = _sCopy;
                newRow["MARH"] = _sXpos;
                newRow["MARV"] = _sYpos;
                newRow["DARK"] = _sDark;
                inPrintTable.Rows.Add(newRow);



                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_OUTBOX_NEW_NJ", "IN_EQP,IN_BOX,IN_PRINT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {

                        loadingIndicator.Visibility = Visibility.Collapsed;
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (chkLabel.IsChecked == true)
                        {
                            if (bizResult != null && bizResult.Tables.Count > 0 && bizResult.Tables["OUTDATA"].Rows.Count > 0)
                            {
                                DataTable dtResult = bizResult.Tables["OUTDATA"];
                                for (int i = 0; i < dtResult.Rows.Count; i++)
                                {
                                    zplCode += dtResult.Rows[i]["ZPLCODE"].ToString();
                                }
                                PrintLabel(zplCode, _drPrtInfo);
                            }
                        }
                        else
                        {
                            //Util.AlertInfo("정상 처리 되었습니다.");
                            Util.MessageInfo("SFU1889");
                        }
                        InitControl();
                        ClearControls();
                        GetWaitInbox();
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
                inData.Columns.Add("EQP_TYPE");

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("BOXID");

                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["USERID"] = txtWorker_Main.Tag;
                newRow["EQP_TYPE"] = null;
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
                new ClientProxy().ExecuteService_Multi("BR_PRD_DEL_OUTBOX_NEW_NJ", "INDATA,INBOX", "OUTDATA", (bizResult, bizException) =>
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
                        ClearControls();
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
                // "라인을 선택하세요."
                Util.MessageValidation("SFU4050");
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

            //if (txtShipto.Text == string.Empty)
            //{
            //    //출하처 정보가 없습니다.
            //    Util.MessageValidation("SFU4999");
            //    return false;
            //}

            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                //SFU1843	작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
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

    }

}
