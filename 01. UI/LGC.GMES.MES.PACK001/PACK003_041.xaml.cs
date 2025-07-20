/************************************************************************************
  Created Date : 2023.03.29
       Creator : 김선준
   Description : Partial ILT Process Mgmt.
 ------------------------------------------------------------------------------------
  [Change History]
    2023.03.29  김선준 : Initial Created.
 ************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using LGC.GMES.MES.PACK001.Controls;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_041 : UserControl, IWorkArea, IDisposable
    {
        #region #1.Variable
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        Util _Util = new Util();
        CommonCombo _combo = new CommonCombo();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private bool bRackName = true; //랙명 <-> Lot수량
        #endregion //0.1 Variable

        #region #2.Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public PACK003_041()
        {
            InitializeComponent();
            PackILT_DataManager.Instance = new PackILT_DataManager();
        }

        #region Dispose
        bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                if (null != _dispatcherMainTimer)
                {
                    _dispatcherMainTimer.Stop();                    
                }
            }

            disposed = true;
        }

        ~PACK003_041()
        {
            Dispose(false);
        }
        #endregion
        #endregion //0.2 Constructor

        #region #3.Event
        /// <summary>
        /// Load Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
            this.Loaded -= new RoutedEventHandler(this.UserControl_Loaded);
            this.grdAging2ndSum.LoadedCellPresenter += GrdAging2ndSum_LoadedCellPresenter;

            //조회
            SearchProcess();
        }

        /// <summary>
        /// UnLoad Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_UnLoaded(object sender, RoutedEventArgs e)
        {
            this.Dispose();
        }

        /// <summary>
        /// Search Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess(); 
        }

        //private void Lbl_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    this.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        SearchgrdLotList(sender);
        //    }));
        //}
        #endregion //0.3 Event

        #region #4.Function
        /// <summary>
        /// 초기화
        /// </summary>
        private void Initialize()
        {
            SetAreaCombo();
            SetWHCombo();
            SetAutoSearchCombo(cboAutoSearch);// 자동조회
            if (_dispatcherMainTimer != null)
            {
                int second = 0;

                if (cboAutoSearch?.SelectedValue != null && !string.IsNullOrEmpty(cboAutoSearch.SelectedValue.ToString()) && cboAutoSearch.SelectedValue.ToString() != "SELECT")
                    second = int.Parse(cboAutoSearch.SelectedValue.ToString());

                _dispatcherMainTimer.Tick -= DispatcherMainTimer_Tick;
                _dispatcherMainTimer.Tick += DispatcherMainTimer_Tick;
                _dispatcherMainTimer.Interval = new TimeSpan(0, 0, second);
            }            
        }

        #region #4.1 Set Combo
        /// <summary>
        /// 동조회
        /// </summary>
        private void SetAreaCombo()
        {
            String[] sFiltercboArea = { Area_Type.PACK };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sFilter: sFiltercboArea, sCase: "AREA_PACK");
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;
        }

        /// <summary>
        /// 창고조회
        /// </summary>
        private void SetWHCombo()
        {
            String[] sFiltercboArea = { "PARTIAL ILT" };
            C1ComboBox[] cboWHIDRightParent = { cboArea };
            _combo.SetCombo(cboWhId, CommonCombo.ComboStatus.SELECT, cbParent: cboWHIDRightParent, sFilter: sFiltercboArea, sCase: "cboWHID");
            if (cboWhId.Items.Count > 0) cboWhId.SelectedIndex = cboWhId.Items.Count - 1;
        }

        /// <summary>
        /// 자동조회
        /// </summary>
        /// <param name="cbo"></param>
        private void SetAutoSearchCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "DRB_REFRESH_TERM" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);
        }
        #endregion //4.1 Set Combo

        #region #4.2 Search
        /// <summary>
        /// ILT RACK 조회
        /// </summary>
        private void SearchProcess()
        {
            PackILT_DataManager.Instance.SocketEventManager = new SocketEventManager();

            #region #. validatoin and initialize
            string AREAID = Util.GetCondition(cboArea);
            if (cboArea.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("동")); // %1(을)를 선택하세요.
                this.cboArea.Focus();
                return;
            }
            string WHID = Util.GetCondition(cboWhId);
            if (cboWhId.SelectedValue.Equals("SELECT"))
            {
                ms.AlertWarning("SFU2961"); //창고를 먼저 선택해주세요.
                return;
            }
            
            grdMain.ColumnDefinitions.Clear();
            grdMain.RowDefinitions.Clear();
            grdMain.Children.Clear();

            this.grdAging2ndSum.Columns.Clear();

            PackCommon.SearchRowCount(ref this.txtGridDetailRowCount, 0);
            Util.gridClear(this.grdLotList);
            this.txtRackInfo.Text = string.Empty;
            #endregion

            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();

            try
            {
                this.SearchGrdMain("Y","Y","N");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }

            this.loadingIndicator.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// RACK Info 조회
        /// </summary>
        private void SearchGrdMain(string sRACKINFO_YN = "N", string sAGING2NDSUM_YN = "N", string sLOTLIST_YN = "N", string sRackID = "")
        {
            #region Rack Info            
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("WH_ID", typeof(string));
            inDataTable.Columns.Add("RACK_ID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("RACKINFO_YN", typeof(string));
            inDataTable.Columns.Add("LOTLIST_YN", typeof(string));
            inDataTable.Columns.Add("AGING2NDSUM_YN", typeof(string));

            inDataTable = indataSet.Tables["INDATA"];

            DataRow newRow = inDataTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["AREAID"] = Util.GetCondition(cboArea);
            newRow["WH_ID"] = Util.GetCondition(cboWhId);
            if (!string.IsNullOrEmpty(sRackID)) newRow["RACK_ID"] = sRackID;
            newRow["RACKINFO_YN"] = sRACKINFO_YN;
            newRow["AGING2NDSUM_YN"] = sAGING2NDSUM_YN;
            newRow["LOTLIST_YN"] = sLOTLIST_YN;

            inDataTable.Rows.Add(newRow);
            DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_PARTIAL_ILT_RACK_INFO", "INDATA", "OUT_RACKINFO,OUT_COMMON,OUT_2NDAGINGSUM,OUT_LOTLIST", indataSet);

            #endregion // Rack Info

            //Create Rack Info
            if (sRACKINFO_YN.Equals("Y")) CreateRack(dsResult.Tables["OUT_RACKINFO"]);
            //Create 2nd Aging Group
            if (sAGING2NDSUM_YN.Equals("Y")) Create2ndAgingGroup(dsResult.Tables["OUT_COMMON"], dsResult.Tables["OUT_2NDAGINGSUM"]);
            //Rack Lot List
            if (sLOTLIST_YN.Equals("Y")) SearchLotList(dsResult.Tables["OUT_LOTLIST"]);
        }
        #endregion //4.2 Search

        #region #4.3 CreateRack
        /// <summary>
        /// Rack 생성
        /// </summary>
        public void CreateRack(DataTable dtRackInfo)
        {
            try
            {
                #region #. Data Retreive and Convert: 
                int iMaxRow = Convert.ToInt16(dtRackInfo.AsEnumerable().Max(row => row["SCRN_X_POSITN_VALUE"]));
                int iMaxCol = Convert.ToInt16(dtRackInfo.AsEnumerable().Max(row => row["SCRN_Y_POSITN_VALUE"]));

                //grdMain Rack Row
                for (int i = 0; i < iMaxRow; i++)
                {
                    this.grdMain.RowDefinitions.Add(new RowDefinition());
                }
                //grdMain Rack Col
                for (int i = 0; i < iMaxCol; i++)
                {
                    this.grdMain.ColumnDefinitions.Add(new ColumnDefinition());
                }
               
                foreach (DataRow row in dtRackInfo.Rows)
                {
                    Grid stpnlRackList = new Grid();
                    stpnlRackList.VerticalAlignment = VerticalAlignment.Stretch;                    
                    this.BindRack(stpnlRackList, row);

                    Grid.SetColumn(stpnlRackList, Convert.ToInt16(row["SCRN_Y_POSITN_VALUE"]) - 1);
                    Grid.SetRow(stpnlRackList, Convert.ToInt16(row["SCRN_X_POSITN_VALUE"]) - 1);                   

                    UcPartialILT_Rack_Info ucf = new UcPartialILT_Rack_Info();
                    ucf.setRackInfo(row);
                    ucf.rackLotEvent += rackLotEvent;
                     
                    ToolTipService.SetToolTip(stpnlRackList, ucf);
                    ToolTipService.SetBetweenShowDelay(stpnlRackList, 10);
                    ToolTipService.SetInitialShowDelay(stpnlRackList, 10);
                    ToolTipService.SetShowDuration(stpnlRackList, 20000);
                    ToolTipService.SetHorizontalOffset(stpnlRackList, -10.0);
                    ToolTipService.SetVerticalOffset(stpnlRackList, -20.0);
                    ToolTipService.SetHasDropShadow(stpnlRackList, false);
                    ToolTipService.SetIsEnabled(stpnlRackList, true);
                    ToolTipService.SetShowOnDisabled(stpnlRackList, true);

                    this.grdMain.Children.Add(stpnlRackList);
                }

                //외곽그리드
                int iZeroRow = dtRackInfo.AsEnumerable().Where(row => Convert.ToInt16(row["SCRN_X_POSITN_VALUE"]) == 1).Count();
                int iZeroCol = dtRackInfo.AsEnumerable().Where(row => Convert.ToInt16(row["SCRN_Y_POSITN_VALUE"]) == 1).Count();

                if (iZeroRow == 0)
                {
                    this.grdMain.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Pixel);
                }
                if (iZeroCol == 0)
                {
                    this.grdMain.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Pixel);
                }
                else
                {
                    this.grdMain.ColumnDefinitions[0].Width = new GridLength(10, GridUnitType.Pixel);
                }
                 
                //this.grdMain.RowDefinitions[iMaxRow - 1].Height = new GridLength(15, GridUnitType.Pixel);                
                //this.grdMain.ColumnDefinitions[iMaxCol - 1].Width = new GridLength(15, GridUnitType.Pixel);

                //랙명 <-> Lot 수량
                this.btnReverse.Visibility = Visibility.Visible;
                this.bRackName = true;
                this.btnReverse.Content = ObjectDic.Instance.GetObjectName("LOT_CNT");
                #endregion

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Rack 개별 생성
        /// </summary>
        /// <param name="target"></param>
        /// <param name="row"></param>
        void BindRack(Grid target, DataRow row)
        {
            UcPartialILT_Rack uc = new UcPartialILT_Rack();
            uc.Margin = new Thickness(1, 1, 1, 1);
            uc.setRackInfo(row);
            uc.rackEvent += Uc_rackEvent;
            
            target.Children.Add(uc);
        }

        /// <summary>
        /// Rack 개별 Event
        /// </summary> 
        private void Uc_rackEvent(string sRackID, string sRackName = "")
        {
            PackCommon.SearchRowCount(ref this.txtGridDetailRowCount, 0);
            Util.gridClear(this.grdLotList);

            this.txtRackInfo.Text = string.IsNullOrEmpty(sRackName)? sRackID:string.Format("[{0}]{1}", sRackID, sRackName);

            //Rack Lot List 조회
            if (string.IsNullOrEmpty(sRackID)) return;

            LotList(sRackID);
        }

        /// <summary>
        /// Rack List 호출
        /// </summary> 
        private void rackLotEvent(bool bSearch = false, string sRackID = "", string sRackName = "")
        {
            PackCommon.SearchRowCount(ref this.txtGridDetailRowCount, 0);
            Util.gridClear(this.grdLotList);

            this.txtRackInfo.Text = string.IsNullOrEmpty(sRackName) ? sRackID : string.Format("[{0}]{1}", sRackID, sRackName);
            //Rack Lot List 조회
            if (!bSearch || string.IsNullOrEmpty(sRackID)) return; 
             
            LotList(sRackID);
        }

        /// <summary>
        /// Rack List 호출
        /// </summary>
        private void LotList(string sRackID)
        {
            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();

            try
            { 
                this.SearchGrdMain("N", "N", "Y", sRackID);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }

            this.loadingIndicator.Visibility = Visibility.Collapsed;
        } 

        /// <summary>
        /// 수량<->랙명
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReverse_Click(object sender, RoutedEventArgs e)
        {
            if (bRackName)
            {
                this.btnReverse.Content = ObjectDic.Instance.GetObjectName("RACK_NAME");
            }
            else
            {
                this.btnReverse.Content = ObjectDic.Instance.GetObjectName("LOT_CNT");
            }

            bRackName = !bRackName;

            ILTObjectFolder _ILTObjectFolder = new ILTObjectFolder();
            _ILTObjectFolder.bRackName = bRackName;

            if (null != PackILT_DataManager.Instance.SocketEventManager)
            {
                PackILT_DataManager.Instance.SocketEventManager.OnILTObjectNotification(_ILTObjectFolder);
            }
        }
        #endregion 4.3 CreateRack

        #region #4.4 Create 2nd Aging Group
        /// <summary>
        /// 2nd Aging Rack Group Sum
        /// </summary>
        /// <param name="dtCommon"></param>
        /// <param name="dt2ndAgingsum"></param>
        private void Create2ndAgingGroup(DataTable dtCommon, DataTable dt2ndAgingsum)
        {
            this.grdAging2ndSum.Columns.Clear();
            C1.WPF.DataGrid.DataGridTextColumn dataGridTextColumn = new C1.WPF.DataGrid.DataGridTextColumn();
            Binding textBinding = new Binding("ZONE");
            textBinding.Mode = BindingMode.TwoWay;
            dataGridTextColumn.Header = "ZONE";
            dataGridTextColumn.Binding = textBinding;
            dataGridTextColumn.HorizontalAlignment = HorizontalAlignment.Center;
            dataGridTextColumn.VerticalAlignment = VerticalAlignment.Center;
            dataGridTextColumn.Visibility = Visibility.Visible;
            dataGridTextColumn.Width = new C1.WPF.DataGrid.DataGridLength(50, DataGridUnitType.Pixel);
            dataGridTextColumn.IsReadOnly = true;
            this.grdAging2ndSum.Columns.Add(dataGridTextColumn);

            for (int i = 0; i < dtCommon.Rows.Count; i++)
            {
                DataRow row = dtCommon.Rows[i];

                dataGridTextColumn = new C1.WPF.DataGrid.DataGridTextColumn();
                textBinding = new Binding(row["ATTRIBUTE4"].ToString());
                textBinding.Mode = BindingMode.TwoWay;
                dataGridTextColumn.Header = row["CBO_NAME"].ToString();
                dataGridTextColumn.Binding = textBinding;
                dataGridTextColumn.HorizontalAlignment = HorizontalAlignment.Center;
                dataGridTextColumn.VerticalAlignment = VerticalAlignment.Center;
                dataGridTextColumn.Visibility = Visibility.Visible;
                dataGridTextColumn.Tag = row["ATTRIBUTE2"].ToString();
                
                if (row["CBO_NAME"].ToString().Length > 10)
                {
                    dataGridTextColumn.Width = new C1.WPF.DataGrid.DataGridLength(row["CBO_NAME"].ToString().Length * 10, DataGridUnitType.Pixel);
                }
                else
                {
                    dataGridTextColumn.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
                }
                 
                dataGridTextColumn.IsReadOnly = true;                

                this.grdAging2ndSum.Columns.Add(dataGridTextColumn);                
            }
            this.grdAging2ndSum.ItemsSource = DataTableConverter.Convert(dt2ndAgingsum);             
        }

        /// <summary>
        /// Cell Color 설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GrdAging2ndSum_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (null != e.Cell.Column.Tag)
            {
                BrushConverter conv = new BrushConverter();
                e.Cell.Presenter.Background = conv.ConvertFromString(e.Cell.Column.Tag.ToString()) as SolidColorBrush;
            }
            //throw new NotImplementedException();
        }

        #endregion //4.4 Create 2nd Aging Group

        #region #4.5 Rack Lot List
        /// <summary>
        /// Rack Lot List
        /// </summary>
        /// <param name="sender"></param>
        private void SearchLotList(DataTable dtLotList)
        {
            Util.GridSetData(this.grdLotList, dtLotList, FrameOperation);
            PackCommon.SearchRowCount(ref this.txtGridDetailRowCount, this.grdLotList.Rows.Count);
        }
        #endregion //Rack Lot List

        #region 5. Event 
        /// <summary>
        /// Seach Zone Lot List 2023-06-29 seonjun
        /// </summary> 
        private void grdAging2ndSum_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = (sender as C1DataGrid);
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

            if (null != cell)
            {
                if (cell.Column.Name == "ZONE")
                {
                    this.loadingIndicator.Visibility = Visibility.Visible;
                    PackCommon.DoEvents();
                    try
                    {
                        string sZone = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, cell.Column.Name));

                        PackCommon.SearchRowCount(ref this.txtGridDetailRowCount, 0);
                        Util.gridClear(this.grdLotList);

                        this.txtRackInfo.Text = string.Format("[{0} ZONE]", sZone);

                        #region Rack Info            
                        DataSet indataSet = new DataSet();

                        DataTable inDataTable = indataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("LANGID", typeof(string));
                        inDataTable.Columns.Add("AREAID", typeof(string));
                        inDataTable.Columns.Add("WH_ID", typeof(string));
                        inDataTable.Columns.Add("RACK_ID", typeof(string));
                        inDataTable.Columns.Add("LOTID", typeof(string));
                        inDataTable.Columns.Add("RACKINFO_YN", typeof(string));
                        inDataTable.Columns.Add("LOTLIST_YN", typeof(string));
                        inDataTable.Columns.Add("AGING2NDSUM_YN", typeof(string));
                        inDataTable.Columns.Add("ZONE", typeof(string));

                        inDataTable = indataSet.Tables["INDATA"];

                        DataRow newRow = inDataTable.NewRow();
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["AREAID"] = Util.GetCondition(cboArea);
                        newRow["WH_ID"] = Util.GetCondition(cboWhId);
                        newRow["RACKINFO_YN"] = "N";
                        newRow["AGING2NDSUM_YN"] = "N";
                        newRow["LOTLIST_YN"] = "Y";
                        newRow["ZONE"] = sZone;

                        inDataTable.Rows.Add(newRow);
                        DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_PARTIAL_ILT_RACK_INFO", "INDATA", "OUT_RACKINFO,OUT_COMMON,OUT_2NDAGINGSUM,OUT_LOTLIST", indataSet);

                        #endregion // Rack Info

                        //Rack Lot List 
                        SearchLotList(dsResult.Tables["OUT_LOTLIST"]);
                    }
                    catch (Exception ex)
                    {

                        Util.MessageException(ex);
                    }
                    finally
                    {
                        this.loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        //Excel
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        { 
            this.loadingIndicator.Visibility = Visibility.Visible;
            try
            {
                Util.gridClear(this.grdLotExcel);

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("EXCEL_YN", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EXCEL_YN"] = "Y";
                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_PARTIAL_ILT_LOT_LIST", "INDATA", "OUT_LOTLIST", dsInput, null);

                Util.GridSetData(this.grdLotExcel, dsResult.Tables["OUT_LOTLIST"], FrameOperation, true);

                if (this.grdLotExcel.Rows.Count > 0)  new ExcelExporter().Export(this.grdLotExcel);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
            finally
            { 
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #region 5.1 Event Rack Move
        /// <summary>
        /// Rack 이동
        /// </summary>
        private void btnMoveRack_Click(object sender, RoutedEventArgs e)
        {
            MoveRack("MV");
        }

        /// <summary>
        /// Rack 이동확정
        /// </summary>
        private void btnMoveRackConfirm_Click(object sender, RoutedEventArgs e)
        {
            MoveRack("IN");
        }

        /// <summary>
        /// Rack 이동/확정
        /// </summary>
        private void MoveRack(string Gubun)
        {
            PACK003_041_NG_RACK_MOVE ngPopup = new PACK003_041_NG_RACK_MOVE();
            ngPopup.FrameOperation = FrameOperation;

            if (ngPopup != null)
            {
                object[] Parameters;
                Parameters = new object[1];
                Parameters[0] = Gubun;
                C1WindowExtension.SetParameters(ngPopup, Parameters);

                ngPopup.Closed -= new EventHandler(Popup_Closed);
                ngPopup.Closed += new EventHandler(Popup_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => ngPopup.ShowModal()));
                ngPopup.BringToFront();
            }
        }

        /// <summary>
        /// Rack이동 작업 후 재조회
        /// </summary> 
        private void Popup_Closed(object sender, EventArgs e)
        {
            bool bWorked = ((sender as PACK003_041_NG_RACK_MOVE))._bWork;
            if (bWorked)
            {
                this.SearchProcess();
            }
        }
        #endregion
        #endregion

        #region #9.Auto Search
        private bool _isAutoSelectTime = false;
        private readonly System.Windows.Threading.DispatcherTimer _dispatcherMainTimer = new System.Windows.Threading.DispatcherTimer();

        /// <summary>
        /// Timer설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispatcherMainTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null) return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();

                    // 0분이면 skip
                    if (dpcTmr.Interval.TotalSeconds.Equals(0)) return;
                    // data조회
                    SearchProcess();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));
        }

        /// <summary>
        /// 자동Combo 변경시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboAutoSearch_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (_dispatcherMainTimer != null)
                {
                    _dispatcherMainTimer.Stop();

                    int iSec = 0;

                    if (cboAutoSearch?.SelectedValue != null && !cboAutoSearch.SelectedValue.ToString().Equals("") && cboAutoSearch.SelectedValue.ToString() != "SELECT")
                        iSec = int.Parse(cboAutoSearch.SelectedValue.ToString());

                    if (iSec == 0 && _isAutoSelectTime)
                    {
                        _dispatcherMainTimer.Interval = new TimeSpan(0, 0, iSec);
                        // 자동조회가 사용하지 않도록 변경 되었습니다.
                        Util.MessageValidation("SFU8170");
                        return;
                    }

                    if (iSec == 0)
                    {
                        _isAutoSelectTime = true;
                        return;
                    }

                    _dispatcherMainTimer.Interval = new TimeSpan(0, 0, iSec);
                    _dispatcherMainTimer.Start();

                    if (_isAutoSelectTime)
                    {
                        // 자동조회  %1초로 변경 되었습니다.
                        Util.MessageValidation("SFU5127", cboAutoSearch?.SelectedValue?.ToString());
                    }

                    _isAutoSelectTime = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Timer 실행
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            SearchProcess();
        }
        #endregion //9.Auto Search

        #endregion
    }
}