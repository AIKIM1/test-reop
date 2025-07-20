/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.22  DEVELOPER : Initial Created.
  2023.01.31 김린겸 C20221221-000550 Added GMES DB data check for packaged cell logins
  2023.11.30 이병윤 
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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_201 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        /*컨트롤 변수 선언*/
        public UCBoxShift ucBoxShift { get; set; }
        public TextBox txtWorker_Main { get; set; }
        public TextBox txtShift_Main { get; set; }
        private static string CREATED = "CREATED,";
        private static string PACKING = "PACKING,";
        private static string PACKED = "PACKED,";
        private static string SHIPPING = "SHIPPING,";
        private string _searchStat = string.Empty;

        private string _processName = string.Empty;
        //private string _rcvStat = string.Empty;
        private bool bInit = true;
        // 프린트 설정용
        string _sPrt = string.Empty;
        string _sRes = string.Empty;
        string _sCopy = string.Empty;
        string _sXpos = string.Empty;
        string _sYpos = string.Empty;
        string _sDark = string.Empty;

        DataRow _drPrtInfo = null;

        bool _AommGrdChkFlag = false;

        Util _util = new Util();

        string _sPGM_ID = "BOX001_201";

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre2 = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
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

        CheckBox chkAll2 = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };


        public BOX001_201()
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
            txtinBoxId.IsEnabled = false;

            this.Loaded -= UserControl_Loaded;
        }

        private void ApplyPermissions()
        {
            btnCancelShip.IsEnabled = LoginInfo.LOGGEDBYSSO == true? true : false;
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID, "MCP", Process.CELL_BOXING }, sCase: "LINEBYSHOP");            
            //_combo.SetCombo(cboEquipment_Search, CommonCombo.ComboStatus.SELECT, cbParent: new C1ComboBox[] { cboLine }, sFilter: new string[] { Process.CELL_BOXING }, sCase: "EQUIPMENT_BY_EQSGID_PROCID");
         
            _combo.SetCombo(cboExpDomType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "EXP_DOM_TYPE_CODE" }, sCase: "COMMCODE_WITHOUT_CODE");
            _combo.SetCombo(cboProcType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "B" }, sCase: "PROCBYPCSGID");
            _combo.SetCombo(cboLabelType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "MOBILE_INBOX_LABEL_TYPE" }, sCase: "COMMCODE_WITHOUT_CODE");
            //_combo.SetCombo(cboInboxType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "INBOX_TYPE_CODE" }, sCase: "COMMCODE_WITHOUT_CODE");

            SetAommGradeCombo();
        }

        public void SetAommGradeCombo()
        {
            try
            {
                cboAommType.ItemsSource = null;
                cboAommType.Text = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "AOMM_GRADE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                cboAommType.DisplayMemberPath = "CBO_NAME";
                cboAommType.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);


                cboAommType.ItemsSource = dtResult.Copy().AsDataView();
                cboAommType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetAommGrdVisibility(string sProdID)
        {
            try
            {
                if (string.IsNullOrEmpty(sProdID))
                {
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "GRADE_CHK_PROD";
                dr["CMCODE"] = sProdID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _AommGrdChkFlag = true;
                    tbAommType.Visibility = Visibility.Visible;
                    cboAommType.Visibility = Visibility.Visible;
                }
                else
                {
                    _AommGrdChkFlag = false;
                    tbAommType.Visibility = Visibility.Collapsed;
                    cboAommType.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboLine_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            setEquipmentCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, (string)cboLine.SelectedValue, (string)cboEquipment_Search.SelectedValue);
            setEquipmentCombo(cboEquipment_Search, CommonCombo.ComboStatus.SELECT, (string)cboLine.SelectedValue, LoginInfo.CFG_EQPT_ID);
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            txtBoxCellQty.Value = 100;

            /* 공용 작업조 컨트롤 초기화 */
            ucBoxShift = grdShift.Children[0] as UCBoxShift;
            txtWorker_Main = ucBoxShift.TextWorker;
            txtShift_Main = ucBoxShift.TextShift;
            ucBoxShift.ProcessCode = Process.CELL_BOXING; //작업조 팝업에 넘길 공정
            ucBoxShift.UcParentControl = this;

            if (_processName == string.Empty)
                SelectProcessName();
            //// 프린터 정보 조회
            //if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
            //{
            //    return;
            //}
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

        private void btn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        #region  체크박스 선택 이벤트     
        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgInbox.GetRowCount(); i++)
                {
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

        void checkAll2_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll2.IsChecked)
            {
                for (int i = 0; i < dgInTray.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgInTray.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll2_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll2.IsChecked)
            {
                for (int i = 0; i < dgInTray.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgInTray.Rows[i].DataItem, "CHK", false);
                }
            }
        }
        #endregion

        #endregion

        #region [Biz]

        /// <summary>
        /// 작업 대상 조회
        /// BIZ :BR_PRD_GET_INPALLET_LIST_NJ
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

                if (cboEquipment_Search.SelectedIndex < 0 || cboEquipment_Search.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    //SFU1673 설비를 선택 하세요.
                    Util.MessageValidation("SFU1673");
                    return;
                }

                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID");
               // RQSTDT.Columns.Add("EQSGID");
                RQSTDT.Columns.Add("EQPTID");
                RQSTDT.Columns.Add("BOXSTAT_LIST");
                //RQSTDT.Columns.Add("RCV_ISS_STAT_CODE");

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
               // dr["EQSGID"] = Util.NVC(cboLine.SelectedValue);
                dr["EQPTID"] = Util.NVC(cboEquipment_Search.SelectedValue);
                dr["BOXSTAT_LIST"] = string.IsNullOrEmpty(_searchStat) ? _searchStat : _searchStat.Remove(_searchStat.Length - 1);
               // dr["RCV_ISS_STAT_CODE"] =string.IsNullOrEmpty(_rcvStat)?null:_rcvStat;

                RQSTDT.Rows.Add(dr);

                DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_INPALLET_LIST_NJ", "INDATA", "OUTDATA", RQSTDT);

                if (!RSLTDT.Columns.Contains("CHK"))
                    RSLTDT = _util.gridCheckColumnAdd(RSLTDT, "CHK");

                Util.GridSetData(dgInPallet, RSLTDT, FrameOperation, true);
              
                if (dgInPallet.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgInPallet.Columns["WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgInPallet.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgInPallet.Columns["RESNQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgInPallet.Columns["BOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 인박스 조회
        ///  BIZ : BR_PRD_GET_INBOX_LIST_NJ
        /// </summary>
        /// <param name="isRefreshGrid"></param>
        private void Getinbox()
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
      
                string sPalletID = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);
                int iRestQty = Util.NVC_Int(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["RESTQTY"].Index).Value);
             
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

                Util.GridSetData(dgInbox, dtResult, FrameOperation, true);
               
                if (dgInbox.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgInbox.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgInbox.Columns["SUBLOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }
                txtRestCellQty.Value = iRestQty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
                    int sQty = Util.NVC_Int(dgInbox.GetCell(idxBox, dgInbox.Columns["TOTAL_QTY"].Index).Value);

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
                        _util.SetDataGridCheck(dgInPallet, "CHK", "BOXID", sPalletId, true);

                        //Getinbox();
                        SetDetailInfo();
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
                        SetDetailInfo();
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

                DataSet resultDs = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_TAG_PALLET_NJ", "INDATA", "OUTDATA,OUTBOX,OUTLOT", inDataSet);
                return resultDs;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        #endregion

        #region [상단 버튼 이벤트]
        private void btnBoxLabelPrint_Click(object sender, RoutedEventArgs e)
        {
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

            // 프린터 정보 조회
            if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
            {
                return;
            }

            BOX001_201_INBOX_LABEL popup = new BOX001_201_INBOX_LABEL();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = Util.NVC(cboLine.SelectedValue);
                Parameters[1] = Util.NVC(cboEquipment_Search.SelectedValue);
                Parameters[2] = txtWorker_Main.Tag; // 작업자id

                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puInboxLabel_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }
        private void puInboxLabel_Closed(object sender, EventArgs e)
        {
            BOX001_201_INBOX_LABEL popup = sender as BOX001_201_INBOX_LABEL;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                txtBoxId.Focus();                
            }
            this.grdMain.Children.Remove(popup);
        }

        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                BOX001_201_RUNSTART puRunStart = new BOX001_201_RUNSTART();
                puRunStart.FrameOperation = FrameOperation;

                if (puRunStart != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = cboLine.SelectedValue;
                    Parameters[1] = cboEquipment_Search.SelectedValue;
                    Parameters[2] = txtWorker_Main.Tag;
                    Parameters[3] = txtShift_Main.Tag;
                    C1WindowExtension.SetParameters(puRunStart, Parameters);

                    puRunStart.Closed += new EventHandler(puRunStart_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndRun.ShowModal()));                    
                    grdMain.Children.Add(puRunStart);
                    puRunStart.BringToFront();
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

        private void puRunStart_Closed(object sender, EventArgs e)
        {
            BOX001_201_RUNSTART popup = sender as BOX001_201_RUNSTART;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                cboLine.SelectedValue = popup.EQSGID;
                cboEquipment_Search.SelectedValue = popup.EQPTID;

                GetPalletList();

                _util.SetDataGridCheck(dgInPallet, "CHK", "BOXID", popup.PALLETID, true);
                SetDetailInfo();
            }
            this.grdMain.Children.Remove(popup);
        }

        private void btnInboxMatching_Click(object sender, RoutedEventArgs e)
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

                CMM_POLYMER_FORM_BOX_INBOX_MATCHING popMatching = new CMM_POLYMER_FORM_BOX_INBOX_MATCHING();
                popMatching.FrameOperation = FrameOperation;

                object[] Parameters = new object[3];
                Parameters[0] = Process.CELL_BOXING;
                Parameters[1] = Util.NVC(cboEquipment_Search.SelectedValue);
                Parameters[2] = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);
                C1WindowExtension.SetParameters(popMatching, Parameters);

                popMatching.Closed += new EventHandler(popMatching_Closed);
                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndRun.ShowModal()));                    
                grdMain.Children.Add(popMatching);
                popMatching.BringToFront();
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

        private void popMatching_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_BOX_INBOX_MATCHING popup = sender as CMM_POLYMER_FORM_BOX_INBOX_MATCHING;

            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

            GetPalletList();

            //미 선택 후 팝업 작업하는 경우가 있음.
            if (idxPallet >= 0)
            {
                string sPalletId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);
                _util.SetDataGridCheck(dgInPallet, "CHK", "BOXID", sPalletId, true);
                SetDetailInfo();
            }
            this.grdMain.Children.Remove(popup);
        }

        private void btnRegCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                //{
                //    //SFU1843	작업자를 입력 해 주세요.
                //    Util.MessageValidation("SFU1843");
                //    return;
                //}

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                int idxBoxList = _util.GetDataGridCheckFirstRowIndex(dgInbox, "CHK");

                //List<int> idxBoxList = _util.GetDataGridCheckRowIndex(dgInbox, "CHK");

                //if (idxBoxList.Count <= 0)
                //{
                //    //SFU1645 선택된 작업대상이 없습니다.
                //    Util.MessageValidation("SFU1645");
                //    return;
                //}

                //if (idxBoxList.Count >1)
                //{                    
                //    Util.MessageValidation("하나의 박스만 선택해주세요.");
                //    return;
                //}

                BOX001_201_CELL_DETL puCellDetl = new BOX001_201_CELL_DETL();
                puCellDetl.FrameOperation = FrameOperation;

                if (puCellDetl != null)
                {
                    string sBoxID = idxBoxList < 0? string.Empty:  Util.NVC(dgInbox.GetCell(idxBoxList, dgInbox.Columns["BOXID"].Index).Value);
                    string sPalletID = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);
                    string sAommGrade = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["AOMM_GRD_CODE"].Index).Value);
                    string sLowSoc = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["LOW_SOC_FLAG"].Index).Value);

                    object[] Parameters = new object[6];
                    Parameters[0] = sBoxID;
                    Parameters[1] = txtWorker_Main.Tag;
                    Parameters[2] = sPalletID;
                    Parameters[3] = sAommGrade;
                    Parameters[4] = sLowSoc;

                    C1WindowExtension.SetParameters(puCellDetl, Parameters);

                    puCellDetl.Closed += new EventHandler(puCellDetl_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndRun.ShowModal()));                    
                    grdMain.Children.Add(puCellDetl);
                    puCellDetl.BringToFront();
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

        private void puCellDetl_Closed(object sender, EventArgs e)
        {
            BOX001_201_CELL_DETL popup = sender as BOX001_201_CELL_DETL;
                    
            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

            GetPalletList();

            //미 선택 후 팝업 작업하는 경우가 있음.
            if (idxPallet >= 0)
            {
                string sPalletId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);
                _util.SetDataGridCheck(dgInPallet, "CHK", "BOXID", sPalletId, true);
                SetDetailInfo();
            }
            this.grdMain.Children.Remove(popup);
        }

        private void btnRunCancel_Click(object sender, RoutedEventArgs e)
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
            
            if (Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXSTAT"].Index).Value) != "CREATED")
            {
                //	SFU4404   포장 대기 상태의 팔레트만 작업 취소 가능합니다.	
                Util.MessageValidation("SFU4404");
                return;
            }

            //SFU3135		포장취소 하시겠습니까?	
            Util.MessageConfirm("SFU3135", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DataTable dtRQSTDT = new DataTable("INDATA");
                    dtRQSTDT.Columns.Add("USERID");
                    dtRQSTDT.Columns.Add("BOXID");

                    DataRow newRow = dtRQSTDT.NewRow();
                    newRow["USERID"] = txtWorker_Main.Tag;
                    newRow["BOXID"] = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);
                    dtRQSTDT.Rows.Add(newRow);

                    loadingIndicator.Visibility = Visibility.Visible;
                    new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_INPALLET_NJ", "INDATA", null, dtRQSTDT, (bizResult, bizException) =>
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
                            ClearDetailInfo();
                        }
                        catch (Exception ex)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        }
                        finally
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        }
                    });
                }
            });
                // 

            }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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
                    //SFU1235	이미 확정 되었습니다.
                    Util.MessageValidation("SFU1235");
                    return;
                }

                if (Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXSTAT"].Index).Value) != "PACKING")
                {
                    //SFU2048		확정할 수 없는 상태입니다.	
                    Util.MessageValidation("SFU2048");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["RCV_ISS_STAT_CODE"].Index).Value))
                 && Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["RCV_ISS_STAT_CODE"].Index).Value) == "SHIPPING")
                {
                    //	SFU4414	출고중 팔레트는 실적확정 불가합니다. 	
                    Util.MessageValidation("SFU4414");
                    return;
                }

                if (Util.NVC_Int(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["WIPQTY"].Index).Value)
                    != Util.NVC_Int(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["TOTAL_QTY"].Index).Value))
                {
                    //SFU4417	투입수량과 포장수량이 일치하지 않습니다.	
                    Util.MessageValidation("SFU4417");
                    return;

                }
                int iTotalQty = Util.NVC_Int(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["TOTAL_QTY"].Index).Value);
                int iCellQty = Util.NVC_Int(txtCellQty.Text);

                if (iCellQty > 0
                    && iTotalQty != iCellQty)
                {
                    //SFU4392	셀 등록이 완료되지 않았습니다.	
                    Util.MessageValidation("SFU4392");
                    return;
                }

                // SFU3156 실적 확정시 수정이 불가합니다.그래도 확정 하시겠습니까 ?
                Util.MessageConfirm("SFU3156", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        string sPalletId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);

                        DataSet indataSet = new DataSet();
                        DataTable inDataTable = indataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SHIP_YN");
                        // inDataTable.Columns.Add("EXP_DOM_TYPE_CODE;
                        inDataTable.Columns.Add("EQPTID");
                        inDataTable.Columns.Add("BOXID");
                        inDataTable.Columns.Add("SHFTID");
                        inDataTable.Columns.Add("USERID");
                        inDataTable.Columns.Add("USERNAME");
                        inDataTable.Columns.Add("PACK_NOTE");

                        DataRow newRow = inDataTable.NewRow();
                        newRow["SHIP_YN"] = "N";    // 원각형: "Y" / 파우치: "N"
                        //   newRow["EXP_DOM_TYPE_CODE"] = Util.NVC(dgInPallet.GetCell(dgInPallet.SelectedIndex, dgInPallet.Columns["EXP_DOM_TYPE_CODE"].Index).Value);

                        newRow["EQPTID"] = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["EQPTID"].Index).Value);
                        newRow["BOXID"] = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);
                        newRow["SHFTID"] = txtShift_Main.Tag;
                        newRow["USERID"] = txtWorker_Main.Tag;
                        newRow["USERNAME"] = txtWorker_Main.Text;
                        newRow["PACK_NOTE"] = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["PACK_NOTE"].Index).Value);

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

                                GetPalletList();
                                _util.SetDataGridCheck(dgInPallet, "CHK", "BOXID", sPalletId, true);

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
            { }
        }

        private void btnConfirmCancel_Click(object sender, RoutedEventArgs e)
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

                //SFU4263		실적 취소 하시겠습니까?	
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

                                _util.SetDataGridCheck(dgInPallet, "CHK", "BOXID", sPalletId, true);
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
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            TagPrint();
        }

        #endregion

        #region [Method]
        /// <summary>
        /// 선택된 작업 팔레트의 라인에 존재하는 설비로 콤보박스 셋팅
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="eqsgID"></param>
        /// <param name="eqptID"></param>
        private void setEquipmentCombo(C1ComboBox cbo, CommonCombo.ComboStatus cs, string eqsgID = null, string eqptID = null)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_BY_EQGRID_CBO_NJ";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "EQGRID" };
            string[] arrCondition = { LoginInfo.LANGID, eqsgID, Process.CELL_BOXING , "BOX" };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, cs, selectedValueText, displayMemberText, eqptID);
        }

        private bool RegInBox(string boxid, double qty)
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

                if ((bool)chkMode.IsChecked)
                {
                    if(string.IsNullOrWhiteSpace(txtinBoxId.Text))
                    {
                        // Inbox ID를 입력 하세요.
                        Util.MessageValidation("SFU4517");
                        return false;

                    }
                }

                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return false;
                }

                string sBoxId = Util.NVC(dgInPallet.GetCell(row, dgInPallet.Columns["BOXID"].Index).Value);
              
                DataSet indataSet = new DataSet();
                DataTable inPalletTable = indataSet.Tables.Add("INPALLET");
                inPalletTable.Columns.Add("EQPTID");
                inPalletTable.Columns.Add("BOXID");
                inPalletTable.Columns.Add("SHFTID");
                inPalletTable.Columns.Add("USERID");
                inPalletTable.Columns.Add("MODE_FLAG");

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("BOXID");
                inBoxTable.Columns.Add("TOTAL_QTY");
                inBoxTable.Columns.Add("FORM_INBOX");

                DataRow newRow = inPalletTable.NewRow();
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["BOXID"] = sBoxId;
                newRow["SHFTID"] = txtShift_Main.Tag;
                newRow["USERID"] = txtWorker_Main.Tag;
                newRow["MODE_FLAG"] = (bool)chkMode.IsChecked ? "Y" : "N";

                inPalletTable.Rows.Add(newRow);

                newRow = inBoxTable.NewRow();
                newRow["TOTAL_QTY"] = qty;
                newRow["BOXID"] = boxid;
                newRow["FORM_INBOX"] = string.IsNullOrWhiteSpace(txtinBoxId.Text) ? null : txtinBoxId.Text;
                inBoxTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_INBOX_BY_FORM_INBOX_NJ", "INPALLET,INBOX", null, indataSet);
              
                return true;
            }
            catch (Exception ex)
            {  
                Util.MessageExceptionNoEnter(ex, result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtBoxId.Focus();
                        txtBoxId.Text = string.Empty;
                        txtinBoxId.Text = string.Empty;
                    }
                });
                return false;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void AutoPrintBoxLabel(double qty)
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

                string sLabelType = Util.NVC(cboLabelType.SelectedValue); // Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["WIPQTY"].Index).Value);
                if (sLabelType == "SELECT")
                {
                    sLabelType = "NORMAL";
                }

                int prtQty = 1; // isPrintAll ? (int)Math.Ceiling((inputQty - packedQty) / txtBoxQty.Value) : 1;

                string sBizRule = "BR_PRD_GET_INBOX_LABEL_NJ";

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("EQPTID");
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
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["PRINTQTY"] = prtQty;
                newRow["LABELTYPE"] = sLabelType;
                newRow["USERID"] = txtWorker_Main.Text;
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

                //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_INBOX_LABEL_NJ", "INDATA,INPRINT", "OUTDATA", indataSet);
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizRule, "INDATA,INPRINT", "OUTDATA", indataSet);

                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"] != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    string sBoxId = (string)dsResult.Tables["OUTDATA"].Rows[0]["BOXID"];
                    string sZplCode = (string)dsResult.Tables["OUTDATA"].Rows[0]["ZPLCODE"];
                    RegInBox(sBoxId, qty);
                    PrintLabel(sZplCode, _drPrtInfo);
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
        private void ClearDetailInfo()
        {
            try
            {
                chkAll.IsChecked = false;
                chkMode.IsChecked = false;
                txtBoxId.Text = string.Empty;
                txtinBoxId.Text = string.Empty;
                txtCellQty.Text = string.Empty; // 숨김상태이나, 셀수량 사용중

                Util.gridClear(dgInbox);
                Util.gridClear(dgInTray);
                Util.gridClear(dgDefect);

                cboProcType.SelectedValue = "SELECT";
                txtPkgLotID.Text = string.Empty;
                setEquipmentCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, (string)cboLine.SelectedValue, (string)cboEquipment_Search.SelectedValue);
                cboExpDomType.SelectedValue = "SELECT";
                cboLabelType.SelectedValue = "SELECT";
                //cboInboxType.SelectedValue = "SELECT";
                txtPrdGrade.Text = string.Empty;
                txtPRODID.Text = string.Empty;
                txtInputQty.Value = 0;
                txtRestCellQty.Value = 0;
                txtBoxCellQty.Value = 100;
                txtSoc.Text = string.Empty; 
                new TextRange(txtNote.Document.ContentStart, txtNote.Document.ContentEnd).Text = string.Empty; 
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetDetailInfo()
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
                
                string sPalletId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);

                chkAll.IsChecked = false;
                
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID");
                inDataTable.Columns.Add("BOXID");              

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["BOXID"] = sPalletId;
                inDataTable.Rows.Add(newRow);
                
                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_INPALLET_DETAIL_NJ","INDATA", "OUTDATA,OUTCTNR,OUTINBOX", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult.Tables.Contains("OUTDATA"))
                        {
                            DataTable dtInfo = bizResult.Tables["OUTDATA"];
                           
                            //  SFU2051 중복 데이터가 존재 합니다. % 1
                            if (dtInfo.Rows.Count > 1)
                            {
                                Util.MessageValidation("SFU2051", new object[] { sPalletId });
                                return;
                            }
                            
                            chkAll.IsChecked = false;

                            string sBoxId = Util.NVC(dtInfo.Rows[0]["BOXID"]);
                            string sProdId = Util.NVC(dtInfo.Rows[0]["PRODID"]);
                            string sProject = Util.NVC(dtInfo.Rows[0]["PROJECT"]);
                            string sEqptId = Util.NVC(dtInfo.Rows[0]["EQPTID"]);
                            string sEqptName = Util.NVC(dtInfo.Rows[0]["EQPTNAME"]);
                            string sLabelType = Util.NVC(dtInfo.Rows[0]["LABEL_ID"]);
                            string sInboxType = Util.NVC(dtInfo.Rows[0]["INBOX_TYPE"]);
                            string sWrkType = Util.NVC(dtInfo.Rows[0]["PACK_WRK_TYPE_CODE"]);
                            string sPrdtGrd = Util.NVC(dtInfo.Rows[0]["PRDT_GRD_CODE"]);
                            string sLotId = Util.NVC(dtInfo.Rows[0]["PKG_LOTID"]);
                            string sPackDttm = Util.NVC(dtInfo.Rows[0]["PACKDTTM"]);
                            string sNote = Util.NVC(dtInfo.Rows[0]["PACK_NOTE"]);
                            string sExpDom = Util.NVC(dtInfo.Rows[0]["EXP_DOM_TYPE_CODE"]);
                            string sEqsgId = Util.NVC(dtInfo.Rows[0]["EQSGID"]);
                            string sProcID = Util.NVC(dtInfo.Rows[0]["PROCID"]);
                            string sSOC = Util.NVC(dtInfo.Rows[0]["SOC_VALUE"]);
                            int iWipQty = Util.NVC_Int(dtInfo.Rows[0]["WIPQTY"]);
                            int iRestQty = Util.NVC_Int(dtInfo.Rows[0]["RESTQTY"]);
                            string sInboxLoadQty = Util.NVC(dtInfo.Rows[0]["INBOX_LOAD_QTY"]);
                            int iCellQTy = Util.NVC_Int(dtInfo.Rows[0]["CELL_QTY"]);

                            txtCellQty.Text = iCellQTy.ToString();
                            cboProcType.SelectedValue = string.IsNullOrEmpty(sProcID) ? "SELECT" : sProcID;
                            txtPkgLotID.Text = sLotId;
                            setEquipmentCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, null, sEqptId);
                            cboExpDomType.SelectedValue = string.IsNullOrEmpty(sExpDom) ? "SELECT" : sExpDom;
                            cboLabelType.SelectedValue = string.IsNullOrEmpty(sLabelType) ? "SELECT" : sLabelType;
                            //cboInboxType.SelectedValue = string.IsNullOrEmpty(sInboxType) ? "SELECT" : sInboxType;
                            txtPrdGrade.Text = sPrdtGrd;
                            txtPRODID.Text = sProdId;
                            txtInputQty.Value = iWipQty;
                            txtRestCellQty.Value = iRestQty;
                         //   txtRemainQty.Value = iRestQty;
                            if(!string.IsNullOrWhiteSpace(sInboxLoadQty)) txtBoxCellQty.Value = Util.NVC_Int(sInboxLoadQty);
                            txtSoc.Text = sSOC;
                            new TextRange(txtNote.Document.ContentStart, txtNote.Document.ContentEnd).Text = sNote;

                        }

                        SetAommGrdVisibility(txtPRODID.Text);

                        if (bizResult.Tables.Contains("OUTINBOX"))
                        {
                            DataTable dtInbox = bizResult.Tables["OUTINBOX"];
                            if (!dtInbox.Columns.Contains("CHK"))
                                dtInbox.Columns.Add("CHK");

                            Util.GridSetData(dgInbox, dtInbox, FrameOperation, true);
                            if (dgInbox.Rows.Count > 0)
                            {
                                DataGridAggregate.SetAggregateFunctions(dgInbox.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                                DataGridAggregate.SetAggregateFunctions(dgInbox.Columns["SUBLOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            }
                        }

                        if (bizResult.Tables.Contains("OUTCTNR"))
                        {
                            DataTable dtCtnr = bizResult.Tables["OUTCTNR"];
                            if (!dtCtnr.Columns.Contains("CHK"))
                                dtCtnr.Columns.Add("CHK");

                            if (!dtCtnr.Columns.Contains("RESTQTY"))
                            {
                                DataColumn dc = new DataColumn("RESTQTY");
                                dc.DefaultValue = 0;
                                dtCtnr.Columns.Add(dc);
                            }

                            Util.GridSetData(dgInTray, dtCtnr, FrameOperation, true);

                            if (dgInTray.Rows.Count > 0)
                            {
                                DataGridAggregate.SetAggregateFunctions(dgInTray.Columns["WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                                DataGridAggregate.SetAggregateFunctions(dgInTray.Columns["RESTQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            }
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

                }, indataSet);
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

                //System.Threading.Thread.Sleep(200);
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }
        private void TagPrint()
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
                //SFU4262		실적 확정후 작업 가능합니다.	
                Util.MessageValidation("SFU4262");
                return;
            }

            string sPalletId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);

            Report_1st_Boxing popup = new Report_1st_Boxing();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = sPalletId;
                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(confirmPopup_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }

        private void confirmPopup_Closed(object sender, EventArgs e)
        {
            Report_1st_Boxing popup = sender as Report_1st_Boxing;
            if (popup != null)
            {
                grdMain.Children.Remove(popup);
                string sPalletId = popup.PALLET_ID;
                int idx = 0;

                GetPalletList();
                _util.SetDataGridCheck(dgInPallet, "CHK", "BOXID", sPalletId, true);

                SetDetailInfo();
            }
        }

        #endregion

        #region 이벤트
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

        private void chkMode_Checked(object sender, RoutedEventArgs e)
        {
            txtinBoxId.IsEnabled = true;
            txtBoxId.SelectAll();
            txtBoxId.Focus();
        }

        private void chkMode_Unchecked(object sender, RoutedEventArgs e)
        {
            txtinBoxId.IsEnabled = false;
            txtinBoxId.Text = string.Empty;
            txtBoxId.SelectAll();
            txtBoxId.Focus();
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearDetailInfo();
            GetPalletList();
        }

        private void txtBoxId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

                    if (idxPallet < 0)
                    {
                        //SFU1645 선택된 작업대상이 없습니다.
                        Util.MessageValidation("SFU1645");
                        return;
                    }

                    string sPalletId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);
                    int restQty = int.Parse(Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["RESTQTY"].Index).Value));
                    string sBoxId = txtBoxId.Text.Trim();
                    //txtBoxId.Text = string.Empty;

                    if (sBoxId.Length < 10)
                        return;

                    if (chkMode.IsChecked == false
                       || (chkMode.IsChecked == true && !String.IsNullOrWhiteSpace(txtinBoxId.Text)))
                    {
                        try
                        {
                            if (dgInbox.GetRowCount() > 0)
                            {
                                DataTable dtInfo = DataTableConverter.Convert(dgInbox.ItemsSource);
                                DataRow[] drList = dtInfo.Select("BOXID = '" + sBoxId + "'");
                                if (drList.Length > 0)
                                {
                                    //SFU3263	SFU : SFU	해당 박스는 이미 리스트에 추가되었습니다. 동일한 Box를 리스트에 추가할 수 없습니다.
                                    Util.MessageValidation("SFU3263");
                                }
                            }

                            if ((bool)chkMode.IsChecked)
                            {
                                if (string.IsNullOrWhiteSpace(txtinBoxId.Text))
                                {
                                    // Inbox ID를 입력 하세요.
                                    Util.MessageValidation("SFU4517");
                                    return;
                                }
                            }

                            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                            {
                                //SFU1843	작업자를 입력 해 주세요.
                                Util.MessageValidation("SFU1843");
                                return;
                            }

                            DataSet indataSet = new DataSet();
                            DataTable inPalletTable = indataSet.Tables.Add("INPALLET");
                            inPalletTable.Columns.Add("EQPTID");
                            inPalletTable.Columns.Add("BOXID");
                            inPalletTable.Columns.Add("SHFTID");
                            inPalletTable.Columns.Add("USERID");
                            inPalletTable.Columns.Add("MODE_FLAG");

                            DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                            inBoxTable.Columns.Add("BOXID");
                            inBoxTable.Columns.Add("TOTAL_QTY");
                            inBoxTable.Columns.Add("FORM_INBOX");

                            DataRow newRow = inPalletTable.NewRow();
                            newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                            newRow["BOXID"] = sPalletId;
                            newRow["SHFTID"] = txtShift_Main.Tag;
                            newRow["USERID"] = txtWorker_Main.Tag;
                            newRow["MODE_FLAG"] = (bool)chkMode.IsChecked ? "Y" : "N";

                            inPalletTable.Rows.Add(newRow);

                            newRow = inBoxTable.NewRow();
                            newRow["TOTAL_QTY"] = txtBoxCellQty.Value > restQty ? restQty : txtBoxCellQty.Value;
                            newRow["BOXID"] = sBoxId;
                            newRow["FORM_INBOX"] = String.IsNullOrWhiteSpace(txtinBoxId.Text) ? null : txtinBoxId.Text;
                            inBoxTable.Rows.Add(newRow);

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_INBOX_BY_FORM_INBOX_NJ", "INPALLET,INBOX", null, (bizResult, bizException) =>
                            {
                                try
                                {
                                    if (bizException != null)
                                    {
                                        Util.MessageExceptionNoEnter(bizException, msgResult =>
                                        {
                                            if (msgResult == MessageBoxResult.OK)
                                            {
                                                txtinBoxId.Focus();
                                                txtBoxId.Text = string.Empty;
                                                txtinBoxId.Text = string.Empty;
                                            }
                                        });
                                        return;
                                    }

                                    GetPalletList();

                                    _util.SetDataGridCheck(dgInPallet, "CHK", "BOXID", sPalletId, true);
                                    SetDetailInfo();

                                    txtinBoxId.Focus();
                                    txtBoxId.Text = string.Empty;
                                    txtinBoxId.Text = string.Empty;
                                }
                                catch (Exception ex)
                                {
                                }
                            }, indataSet);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else
                    {
                        txtinBoxId.Focus();
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        private void txtinBoxId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

                    if (idxPallet < 0)
                    {
                        //SFU1645 선택된 작업대상이 없습니다.
                        Util.MessageValidation("SFU1645");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(txtBoxId.Text))
                    {

                        txtBoxId.Focus();
                        return;
                    }

                    string sPalletId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);
                    int restQty = int.Parse(Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["RESTQTY"].Index).Value));
                    string sBoxId = txtBoxId.Text.Trim();
                    //txtBoxId.Text = string.Empty;

                    if (sBoxId.Length < 10)
                        return;

                    if (String.IsNullOrWhiteSpace(txtBoxId.Text))
                    {
                        txtBoxId.Focus();
                        return;
                    }
                    else
                    {
                        try
                        {
                            if (dgInbox.GetRowCount() > 0)
                            {
                                DataTable dtInfo = DataTableConverter.Convert(dgInbox.ItemsSource);
                                DataRow[] drList = dtInfo.Select("BOXID = '" + sBoxId + "'");
                                if (drList.Length > 0)
                                {
                                    //SFU3263	SFU : SFU	해당 박스는 이미 리스트에 추가되었습니다. 동일한 Box를 리스트에 추가할 수 없습니다.
                                    Util.MessageValidation("SFU3263");
                                }
                            }

                            if ((bool)chkMode.IsChecked)
                            {
                                if (string.IsNullOrWhiteSpace(txtinBoxId.Text))
                                {
                                    // Inbox ID를 입력 하세요.
                                    Util.MessageValidation("SFU4517");
                                    return;
                                }
                            }

                            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                            {
                                //SFU1843	작업자를 입력 해 주세요.
                                Util.MessageValidation("SFU1843");
                                return;
                            }

                            DataSet indataSet = new DataSet();
                            DataTable inPalletTable = indataSet.Tables.Add("INPALLET");
                            inPalletTable.Columns.Add("EQPTID");
                            inPalletTable.Columns.Add("BOXID");
                            inPalletTable.Columns.Add("SHFTID");
                            inPalletTable.Columns.Add("USERID");
                            inPalletTable.Columns.Add("MODE_FLAG");

                            DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                            inBoxTable.Columns.Add("BOXID");
                            inBoxTable.Columns.Add("TOTAL_QTY");
                            inBoxTable.Columns.Add("FORM_INBOX");

                            DataRow newRow = inPalletTable.NewRow();
                            newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                            newRow["BOXID"] = sPalletId;
                            newRow["SHFTID"] = txtShift_Main.Tag;
                            newRow["USERID"] = txtWorker_Main.Tag;
                            newRow["MODE_FLAG"] = (bool)chkMode.IsChecked ? "Y" : "N";

                            inPalletTable.Rows.Add(newRow);

                            newRow = inBoxTable.NewRow();
                            newRow["TOTAL_QTY"] = txtBoxCellQty.Value > restQty ? restQty : txtBoxCellQty.Value;
                            newRow["BOXID"] = sBoxId;
                            newRow["FORM_INBOX"] = String.IsNullOrWhiteSpace(txtinBoxId.Text) ? null : txtinBoxId.Text;
                            inBoxTable.Rows.Add(newRow);

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_INBOX_BY_FORM_INBOX_NJ", "INPALLET,INBOX", null, (bizResult, bizException) =>
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
                                                txtinBoxId.Text = string.Empty;
                                            }
                                        });
                                        return;
                                    }

                                    GetPalletList();

                                    _util.SetDataGridCheck(dgInPallet, "CHK", "BOXID", sPalletId, true);
                                    //Getinbox();
                                    SetDetailInfo();

                                    txtBoxId.Focus();
                                    txtBoxId.Text = string.Empty;
                                    txtinBoxId.Text = string.Empty;
                                }
                                catch (Exception ex)
                                {
                                }
                            }, indataSet);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
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

        private void dgInPalletChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

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
                
                SetDetailInfo();
            }
        }

        private void btnRemain_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtInfo = DataTableConverter.Convert(dgInTray.ItemsSource);
            int restCellQty = 0;
            foreach (DataRow dr in dtInfo.Rows)
            {
                restCellQty += Util.NVC_Int(dr["RESTQTY"]);
            }

            txtRemainQty.Value = restCellQty;

            //BR_PRD_REM_INPUT_CTNR_NJ

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

            string sPalletId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);

            if (Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXSTAT"].Index).Value) == "PACKED")
            {
                //SFU3610	이미 포장 완료 됐습니다.[BOX의 정보 확인]	
                Util.MessageValidation("SFU3610");
                return;
            }

            int restQty = Util.NVC_Int(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["RESTQTY"].Index).Value);

            if (restQty <= 0)
            {
                //SFU1859 SFU 잔량이 없습니다.
                Util.MessageValidation("SFU1859");
                return;
            }

            if (txtRemainQty.Value > restQty)
            {
                //SFU4336		남은 수량이내에서 잔량 처리 가능합니다.	
                Util.MessageValidation("SFU4336");
                return;
            }

            //if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
            //{
            //    return;
            //}


            List<int> idxBoxList = _util.GetDataGridCheckRowIndex(dgInTray, "CHK");

            if (idxBoxList.Count <= 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            //SFU1862	잔량처리 하시겠습니까?
            Util.MessageConfirm("SFU1862", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetRemain();
                }
            });
        }
        #endregion

        private void btnProdID_Click(object sender, RoutedEventArgs e)
        {
            CMM001.Popup.CMM_BOX_PROD popup = new CMM001.Popup.CMM_BOX_PROD();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = LoginInfo.CFG_AREA_ID;
                Parameters[1] = txtPRODID.Text;
                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puProduct_Closed);

                //grdMain.Children.Add(popup);
                //popup.BringToFront();
                this.Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
            }
        }

        private void puProduct_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_BOX_PROD popup = sender as CMM001.Popup.CMM_BOX_PROD;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                txtPRODID.Text = popup.PRODID;
                //btnPRODID.Tag = popup.PRODID;

                SetAommGrdVisibility(txtPRODID.Text);
            }

            //this.grdMain.Children.Remove(popup);
        }

        private void btnLabel_Click(object sender, RoutedEventArgs e)
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

            string sPalletId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);

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

            if (txtBoxCellQty.Value > txtRestCellQty.Value)
            {
                //SFU4319	포장가능한 잔량이 부족합니다.
                Util.MessageValidation("SFU4319");
                return;
            }

            if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
            {
                return;
            }

            //SFU4258	발행 하시겠습니까?	
            Util.MessageConfirm("SFU2873", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    AutoPrintBoxLabel(txtBoxCellQty.Value);
                    GetPalletList();
                    _util.SetDataGridCheck(dgInPallet, "CHK", "BOXID", sPalletId, true);

                    SetDetailInfo();
                    Util.MessageInfo("SFU1275");
                }
            });
        }

        private void btnLabelAll_Click(object sender, RoutedEventArgs e)
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

            int restQty = Util.NVC_Int(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["RESTQTY"].Index).Value);

            if(restQty <= 0)
            {
                //SFU1859 SFU 잔량이 없습니다.
                Util.MessageValidation("SFU1859");
                return;
            }

            if (txtRemainQty.Value > restQty)
            {
                //SFU4336		남은 수량이내에서 잔량 처리 가능합니다.	
                Util.MessageValidation("SFU4336");
                return;
            }

            if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
            {
                return;
            }

            //SFU4258	발행 하시겠습니까?	
            Util.MessageConfirm("SFU2873", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {

                    int iInputQty = Util.NVC_Int(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["WIPQTY"].Index).Value);
                    string sPalletId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);

                    int iBoxCellQty = Util.NVC_Int(txtBoxCellQty.Value);
                    int iRestQty = Util.NVC_Int(txtRestCellQty.Value);

                    for (int cnt = iInputQty; 0 < cnt; cnt = cnt - iBoxCellQty)
                    {
                        double qty = cnt <= iBoxCellQty ? cnt : iBoxCellQty;
                        AutoPrintBoxLabel(qty);
                    }
                 
                    GetPalletList();

                    _util.SetDataGridCheck(dgInPallet, "CHK", "BOXID", sPalletId, true);

                    Getinbox();
                    Util.MessageInfo("SFU1275");
                }
            });
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
                        string textRange = new TextRange(txtNote.Document.ContentStart, txtNote.Document.ContentEnd).Text;
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
                        newRow["WIPQTY"] = txtInputQty.Value;
                        newRow["EQPTID"] = cboEquipment.SelectedValue;
                        newRow["PROCID"] = cboProcType.SelectedValue;
                        newRow["SOC_VALUE"] = txtSoc.Text;
                        newRow["EXP_DOM_TYPE_CODE"] = cboExpDomType.SelectedValue;
                        newRow["PACK_NOTE"] = textRange.LastIndexOf(System.Environment.NewLine) < 0? textRange : textRange.Substring(0, textRange.LastIndexOf(System.Environment.NewLine));
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

                                //if (bizResult.Tables.Contains("OUTDATA"))
                                //{
                                //    DataTable dtInfo = bizResult.Tables["OUTDATA"];

                                //    //  SFU2051 중복 데이터가 존재 합니다. % 1
                                //    if (dtInfo.Rows.Count > 1)
                                //    {
                                //        Util.MessageValidation("SFU2051", new object[] { sPalletId });
                                //        return;
                                //    }

                                //    chkAll.IsChecked = false;

                                //    string sBoxId = Util.NVC(dtInfo.Rows[0]["BOXID"]);
                                //    string sProdId = Util.NVC(dtInfo.Rows[0]["PRODID"]);
                                //    string sProject = Util.NVC(dtInfo.Rows[0]["PROJECT"]);
                                //    string sEqptId = Util.NVC(dtInfo.Rows[0]["EQPTID"]);
                                //    string sEqptName = Util.NVC(dtInfo.Rows[0]["EQPTNAME"]);
                                //    string sLabelType = Util.NVC(dtInfo.Rows[0]["LABEL_ID"]);
                                //    string sInboxType = Util.NVC(dtInfo.Rows[0]["INBOX_TYPE"]);
                                //    string sWrkType = Util.NVC(dtInfo.Rows[0]["PACK_WRK_TYPE_CODE"]);
                                //    string sPrdtGrd = Util.NVC(dtInfo.Rows[0]["PRDT_GRD_CODE"]);
                                //    string sLotId = Util.NVC(dtInfo.Rows[0]["PKG_LOTID"]);
                                //    string sPackDttm = Util.NVC(dtInfo.Rows[0]["PACKDTTM"]);
                                //    string sNote = Util.NVC(dtInfo.Rows[0]["PACK_NOTE"]);
                                //    string sExpDom = Util.NVC(dtInfo.Rows[0]["EXP_DOM_TYPE_CODE"]);
                                //    string sEqsgId = Util.NVC(dtInfo.Rows[0]["EQSGID"]);
                                //    string sProcId = Util.NVC(dtInfo.Rows[0]["PROCID"]);
                                //    string sSOC = Util.NVC(dtInfo.Rows[0]["SOC_VALUE"]);
                                //    int iWipQty = Util.NVC_Int(dtInfo.Rows[0]["WIPQTY"]);
                                //    int iRestQty = Util.NVC_Int(dtInfo.Rows[0]["RESTQTY"]);
                                //    int iInboxLoadQty = Util.NVC_Int(dtInfo.Rows[0]["INBOX_LOAD_QTY"]);

                                //    cboProcType.SelectedValue = string.IsNullOrEmpty(sProcId) ? "SELECT" : sProcId;
                                //    txtPkgLotID.Text = sLotId;
                                //    setEquipmentCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, sEqsgId, sEqptId);
                                //    cboExpDomType.SelectedValue = string.IsNullOrEmpty(sExpDom) ? "SELECT" : sExpDom;
                                //    cboLabelType.SelectedValue = string.IsNullOrEmpty(sLabelType) ? "SELECT" : sLabelType;
                                //    //cboInboxType.SelectedValue = string.IsNullOrEmpty(sInboxType) ? "SELECT" : sInboxType;
                                //    txtPrdGrade.Text = sPrdtGrd;
                                //    txtPRODID.Text = sProdId;
                                //    txtInputQty.Value = iWipQty;
                                //    txtRestCellQty.Value = iRestQty;
                                //    txtBoxCellQty.Value = iInboxLoadQty;
                                //    txtSoc.Text = sSOC;
                                //    new TextRange(txtNote.Document.ContentStart, txtNote.Document.ContentEnd).Text = sNote;

                                //}

                                //if (bizResult.Tables.Contains("OUTINBOX"))
                                //{
                                //    Util.GridSetData(dgInbox, bizResult.Tables["OUTINBOX"], FrameOperation);
                                //}

                                //if (bizResult.Tables.Contains("OUTCTNR"))
                                //{
                                //    Util.GridSetData(dgInTray, bizResult.Tables["OUTCTNR"], FrameOperation, true);
                                //}

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

        private void btnDeleteBox_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                //SFU1843	작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return;
            }

            // SFU1230  삭제 하시겠습니까?
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeleteInbox();
                }
            });
        }

        private void btnUpdateBox_Click(object sender, RoutedEventArgs e)
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
                string boxStat = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXSTAT"].Index).Value);

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }
              
                if (boxStat.Equals("PACKED"))
                {
                    //SFU4296 포장 완료된 팔레트입니다. 수정 불가합니다.
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

                //SFU2913  수량변경하시겠습니까?	
                Util.MessageConfirm("SFU2913", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
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
                            int iQty = Util.NVC_Int(dgInbox.GetCell(idxBox, dgInbox.Columns["TOTAL_QTY"].Index).Value);
                            int iCellQty = Util.NVC_Int(dgInbox.GetCell(idxBox, dgInbox.Columns["SUBLOT_QTY"].Index).Value);

                            if (iQty < iCellQty)
                            {
                                //SFU4425		셀 등록 수량보다 적은 수량으로 수정할 수 없습니다.
                                Util.MessageValidation("SFU4425");
                                dgInbox.SelectedItem = dgInbox.GetCell(idxBox, dgInbox.Columns["BOXID"].Index);
                                dgInbox.ScrollIntoView(idxBox, 0);
                                return;
                            }
                            newRow = inBoxTable.NewRow();
                            newRow["BOXID"] = sBoxId;
                            newRow["TOTAL_QTY"] = iQty;
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
                                _util.SetDataGridCheck(dgInPallet, "CHK", "BOXID", sPalletId, true);
                                Getinbox();
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

        private void dgInbox_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    else if (e.Cell.Column.Name == "SUBLOT_QTY")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgInbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell == null || datagrid.CurrentRow == null)
                {
                    return;
                }

                if (datagrid.CurrentColumn.Name == "SUBLOT_QTY")
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

                        BOX001_201_CELL_DETL puCellDetl = new BOX001_201_CELL_DETL();
                        puCellDetl.FrameOperation = FrameOperation;

                        if (puCellDetl != null)
                        {
                            string sBoxID = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["BOXID"].Index).Text);
                            string sProdID = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["PRODID"].Index).Value);
                            string sProdName = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["PRODNAME"].Index).Value);
                            string sPkgLotID = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["PKG_LOTID"].Index).Value);
                            string sProject = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["PROJECT"].Index).Value);

                            object[] Parameters = new object[6];
                            Parameters[0] = sBoxID;
                            Parameters[1] = txtWorker_Main.Tag;
                            Parameters[2] = sProdID;
                            Parameters[3] = sPkgLotID;
                            Parameters[4] = sProject;
                            Parameters[5] = sProdName;
                            C1WindowExtension.SetParameters(puCellDetl, Parameters);

                            puCellDetl.Closed += new EventHandler(puCellDetl_Closed);                
                            grdMain.Children.Add(puCellDetl);
                            puCellDetl.BringToFront();
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
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        

        private void dgInbox_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name == "CHK")
            {
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
                    e.Cancel = true;
                    return;
                }

                return;
            }
            
            if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "CHK")) != bool.TrueString)
            {
                e.Cancel = true;
                return;
            }            
        }

        private void SetRemain()
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

                string sPalletID = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);
                
                DataSet indataSet = new DataSet();
                DataTable inPalletTable = indataSet.Tables.Add("INDATA");
                inPalletTable.Columns.Add("LANGID");
                inPalletTable.Columns.Add("BOXID");
                inPalletTable.Columns.Add("USERID");

                DataTable inBoxTable = indataSet.Tables.Add("INCTNR");
                inBoxTable.Columns.Add("CTNR_ID");
                inBoxTable.Columns.Add("LOTID");
                inBoxTable.Columns.Add("WIPQTY");
                inBoxTable.Columns.Add("RESTQTY");
                if(_AommGrdChkFlag == true)
                {
                    inBoxTable.Columns.Add("AOMM_GRD_CODE");
                }

                DataRow newRow = inPalletTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["BOXID"] = sPalletID;
                newRow["USERID"] = txtWorker_Main.Tag;

                inPalletTable.Rows.Add(newRow);

                List<int> idxBoxList = _util.GetDataGridCheckRowIndex(dgInTray, "CHK");

                if (idxBoxList.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                foreach (int idxBox in idxBoxList)
                {
                    int iWipQty = Util.NVC_Int(dgInTray.GetCell(idxBox, dgInTray.Columns["WIPQTY"].Index).Value);
                    int iRestQty = Util.NVC_Int(dgInTray.GetCell(idxBox, dgInTray.Columns["RESTQTY"].Index).Value);

                    newRow = inBoxTable.NewRow();
                    newRow["CTNR_ID"] = Util.NVC(dgInTray.GetCell(idxBox, dgInTray.Columns["CTNR_ID"].Index).Value);
                    newRow["LOTID"] = Util.NVC(dgInTray.GetCell(idxBox, dgInTray.Columns["LOTID"].Index).Value);
                    newRow["WIPQTY"] = iWipQty;
                    newRow["RESTQTY"] = iRestQty;
                    if (_AommGrdChkFlag == true)
                    {
                        newRow["AOMM_GRD_CODE"] = Util.NVC(cboAommType.SelectedValue);
                    }
                    inBoxTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REM_INPUT_CTNR_NJ", "INDATA,INCTNR", "OUTCTNR,OUTLOT", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // SFU1275 정상처리되었습니다.
                        Util.MessageInfo("SFU1275");

                        GetPalletList();
                        _util.SetDataGridCheck(dgInPallet, "CHK", "BOXID", sPalletID, true);
                        SetDetailInfo();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            { }
        }

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            //BR_PRD_REG_INPUT_CTNR_NJ
            try
            {
                if (e.Key == Key.Enter)
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

                    if (string.IsNullOrWhiteSpace(txtPalletID.Text))
                    {
                        //SFU3350	입력오류 : PALLETID 를 입력해 주세요.
                        Util.MessageValidation("SFU3350");
                        return;
                    }

                    //SFU1987	투입처리 하시겠습니까?        
                    Util.MessageConfirm("SFU1987", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            string sPalletId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);
                            string sProdId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["PRODID"].Index).Value);

                            DataSet indataSet = new DataSet();
                            DataTable inDataTable = indataSet.Tables.Add("INDATA");
                            inDataTable.Columns.Add("LANGID");
                            inDataTable.Columns.Add("BOXID");
                            inDataTable.Columns.Add("USERID");

                            DataTable inCtnrTable = indataSet.Tables.Add("INCTNR");
                            inCtnrTable.Columns.Add("LOTID");

                            DataRow newRow = inDataTable.NewRow();
                            newRow["LANGID"] = LoginInfo.LANGID;
                            newRow["BOXID"] = sPalletId;
                            newRow["USERID"] = txtWorker_Main.Tag;
                            inDataTable.Rows.Add(newRow);

                            newRow = inCtnrTable.NewRow();
                            newRow["LOTID"] = txtPalletID.Text;
                            inCtnrTable.Rows.Add(newRow);

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_INPUT_CTNR_NJ", "INDATA,INCTNR", "OUTCTNR", (bizResult, bizException) =>
                            {
                                try
                                {
                                    if (bizException != null)
                                    {
                                        Util.MessageException(bizException);
                                        return;
                                    }

                                    //SFU1973	투입완료되었습니다.
                                    Util.MessageInfo("SFU1973");

                                    GetPalletList();
                                    _util.SetDataGridCheck(dgInPallet, "CHK", "BOXID", sPalletId, true);
                                    SetDetailInfo();
                                    txtBoxId.Text = string.Empty;
                                    txtRemainQty.Value = 0;
                                   
                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                }
                                finally
                                {
                                    txtPalletID.Text = string.Empty;
                                    txtPalletID.Focus();
                                }
                            }, indataSet);
                        }
                        else
                            loadingIndicator.Visibility = Visibility.Collapsed;
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgInTray_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name == "CHK")
            {
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
                    e.Cancel = true;
                    return;
                }

                return;
            }

            if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "CHK")) != bool.TrueString)
            {
                e.Cancel = true;
                return;
            }
        }

        private void dgInTray_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
           
            if (e.Cell.Column.Name == "RESTQTY")
            {
                int wipqty = Util.NVC_Int(dgInTray.GetCell(e.Cell.Row.Index, dgInTray.Columns["WIPQTY"].Index).Value);

                if (wipqty < Util.NVC_Int(e.Cell.Value))
                {
                    //SFU4053	잔량은 투입수량보다 작아야 합니다.	
                    Util.MessageValidation("SFU4053");
                    e.Cell.Value = 0;
                }

                DataTable dtInfo = DataTableConverter.Convert(dgInTray.ItemsSource);
                int restQty = 0;
                foreach (DataRow dr in dtInfo.Rows)
                {
                    restQty += Util.NVC_Int(dr["RESTQTY"]);
                }

                //SFU4336		남은 수량이내에서 잔량 처리 가능합니다.	
                if (restQty > txtRestCellQty.Value)
                {
                    Util.MessageValidation("SFU4336");
                    txtRemainQty.Value = restQty - Util.NVC_Int(e.Cell.Value);
                    e.Cell.Value = 0;
                }
                else
                  txtRemainQty.Value = restQty;
            }
        }

        /// <summary>
        /// 출고
        /// Biz : BR_PRD_REG_SHIPMENT_NJ
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
                        RQSTDT.Columns.Add("USERID");

                        DataRow newRow = RQSTDT.NewRow();
                        newRow["USERID"] = txtWorker_Main.Tag;
                        newRow["BOXID"] = sPalletId;
                        RQSTDT.Rows.Add(newRow);

                        loadingIndicator.Visibility = Visibility.Visible;

                        new ClientProxy().ExecuteService("BR_PRD_REG_SHIPMENT_NJ", "INDATA", null, RQSTDT, (RSLTDT, ex) =>
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
        /// Biz : BR_PRD_REG_CANCEL_SHIPMENT_NJ
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

                        new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_SHIPMENT_NJ", "INDATA", null, RQSTDT, (RSLTDT, ex) =>
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

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        private void btnPackingList_Click(object sender, RoutedEventArgs e)
        {
            PackingListPrint();
        }

        private void PackingListPrint()
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
                //SFU4262		실적 확정후 작업 가능합니다.	
                Util.MessageValidation("SFU4262");
                return;
            }

            string sPalletId = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);

            Report_2nd_Boxing popup = new Report_2nd_Boxing();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = sPalletId;
                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(packingPopup_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }

        private void packingPopup_Closed(object sender, EventArgs e)
        {
            Report_2nd_Boxing popup = sender as Report_2nd_Boxing;
            if (popup != null)
            {
                string sPalletId = popup.PALLET_ID;

                GetPalletList();
                _util.SetDataGridCheck(dgInPallet, "CHK", "BOXID", sPalletId, true);
                // GetDetailInfo();
            }
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
        
        private void dgInTray_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre2.Content = chkAll2;
                            e.Column.HeaderPresenter.Content = pre2;
                            chkAll2.Checked -= new RoutedEventHandler(checkAll2_Checked);
                            chkAll2.Unchecked -= new RoutedEventHandler(checkAll2_Unchecked);
                            chkAll2.Checked += new RoutedEventHandler(checkAll2_Checked);
                            chkAll2.Unchecked += new RoutedEventHandler(checkAll2_Unchecked);
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

        private void btnCancelInput_Click(object sender, RoutedEventArgs e)
        {
            //BR_PRD_REG_CANCEL_INPUT_CTNR_NJ
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

                List<int> idxBoxList = _util.GetDataGridCheckRowIndex(dgInTray, "CHK");

                if (idxBoxList.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                // SFU1988  투입취소 하시겠습니까? 
                Util.MessageConfirm("SFU1988", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        string sPalletID = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);

                        DataSet indataSet = new DataSet();
                        DataTable inPalletTable = indataSet.Tables.Add("INDATA");
                        inPalletTable.Columns.Add("LANGID");
                        inPalletTable.Columns.Add("BOXID");
                        inPalletTable.Columns.Add("USERID");

                        DataTable inBoxTable = indataSet.Tables.Add("INCTNR");
                        inBoxTable.Columns.Add("CTNR_ID");
                        inBoxTable.Columns.Add("LOTID");
                        inBoxTable.Columns.Add("PROCID");

                        DataRow newRow = inPalletTable.NewRow();
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["BOXID"] = sPalletID;
                        newRow["USERID"] = txtWorker_Main.Tag;

                        inPalletTable.Rows.Add(newRow);

                        foreach (int idxBox in idxBoxList)
                        {
                            int iWipQty = Util.NVC_Int(dgInTray.GetCell(idxBox, dgInTray.Columns["WIPQTY"].Index).Value);
                            int iRestQty = Util.NVC_Int(dgInTray.GetCell(idxBox, dgInTray.Columns["RESTQTY"].Index).Value);

                            newRow = inBoxTable.NewRow();
                            newRow["CTNR_ID"] = Util.NVC(dgInTray.GetCell(idxBox, dgInTray.Columns["CTNR_ID"].Index).Value);
                            newRow["LOTID"] = Util.NVC(dgInTray.GetCell(idxBox, dgInTray.Columns["LOTID"].Index).Value);
                            newRow["PROCID"] = Util.NVC(dgInTray.GetCell(idxBox, dgInTray.Columns["PROCID"].Index).Value);
                            inBoxTable.Rows.Add(newRow);
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_INPUT_CTNR_NJ", "INDATA,INCTNR", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                        // SFU1275 정상처리되었습니다.
                        Util.MessageInfo("SFU1275");

                                GetPalletList();
                                _util.SetDataGridCheck(dgInPallet, "CHK", "BOXID", sPalletID, true);
                                SetDetailInfo();
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
            { }
        }
        

        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
        

        private void btnCancel_Click(object sender, RoutedEventArgs e)
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
                //SFU4262		실적 확정후 작업 가능합니다.	
                Util.MessageValidation("SFU4262");
                return;
            }

            if (!string.IsNullOrWhiteSpace(Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["RCV_ISS_STAT_CODE"].Index).Value))
                  && Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["RCV_ISS_STAT_CODE"].Index).Value) == "SHIPPING")
            {
                //	SFU4416	이미 출고된 팔레트 입니다.
                Util.MessageValidation("SFU4416");
                return;
            }

            string sPalletID = Util.NVC(dgInPallet.GetCell(idxPallet, dgInPallet.Columns["BOXID"].Index).Value);
            
            this.FrameOperation.OpenMenu("SFU010060520", true, new object[] { sPalletID, txtShift_Main.Tag, txtShift_Main.Text, txtWorker_Main.Tag, txtWorker_Main.Text, ucBoxShift.TextShiftDateTime.Text});

        }

        private void btnCellPassFCS_Click(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            //{   
            //    Util.MessageValidation("SFU1843");  //작업자를 입력 해 주세요.
            //    return;
            //}

            //CMM_BOX_FORM_CELL_PASS_FCS popup = new CMM_BOX_FORM_CELL_PASS_FCS();
            //popup.FrameOperation = this.FrameOperation;
            CMM_BOX_FORM_CELL_PASS_FCS popup = new CMM_BOX_FORM_CELL_PASS_FCS { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popup.Name) == false)
                return;

            if (popup != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = LoginInfo.USERID;
                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puCellPassFCS_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }

        private void puCellPassFCS_Closed(object sender, EventArgs e)
        {
            CMM_BOX_FORM_CELL_PASS_FCS popup = sender as CMM_BOX_FORM_CELL_PASS_FCS;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            grdMain.Children.Remove(popup);
        }

        private void SelectProcessName()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.CELL_BOXING;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_FO", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    _processName = dtResult.Rows[0]["PROCNAME"].ToString();
                else
                    _processName = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnTakeOver_Click(object sender, RoutedEventArgs e)
        {
            CMM_POLYMER_FORM_CART_TAKEOVER popupTakeOver = new CMM_POLYMER_FORM_CART_TAKEOVER { FrameOperation = FrameOperation };
            if (ValidationGridAdd(popupTakeOver.Name) == false)
                return;

            object[] parameters = new object[5];
            parameters[0] = Process.CELL_BOXING;
            parameters[1] = _processName;
            parameters[2] = string.Empty;
            parameters[3] = string.Empty;
            parameters[4] = string.Empty;
            C1WindowExtension.SetParameters(popupTakeOver, parameters);

            popupTakeOver.Closed += popupTakeOver_Closed;
            grdMain.Children.Add(popupTakeOver);
            popupTakeOver.BringToFront();
        }

        private void popupTakeOver_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_CART_TAKEOVER popup = sender as CMM_POLYMER_FORM_CART_TAKEOVER;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            grdMain.Children.Remove(popup);
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
    }
}
