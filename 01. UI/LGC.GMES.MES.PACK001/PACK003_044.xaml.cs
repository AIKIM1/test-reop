/************************************************************************************
  Created Date : 2023.06.26
       Creator : 김선준
   Description : PACK STK Process Mgmt.
 ------------------------------------------------------------------------------------
  [Change History]
    2023.06.26  김선준 : Initial Created.
    2023.07.27  김선준 : 디자인변경, 동별공통코드 변경
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
    public partial class PACK003_044 : UserControl, IWorkArea
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
        private const string S_Y = "Y";
        private const string S_N = "N";
        #endregion //0.1 Variable

        #region #2.Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public PACK003_044()
        {
            InitializeComponent();
            PackSTK_DataManager.Instance = new PackSTK_DataManager();
        }
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

            //조회
            SearchProcess(); 
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
        #endregion //0.3 Event

        #region #4.Function
        /// <summary>
        /// 초기화
        /// </summary>
        private void Initialize()
        {
            this.SearchGrdMain(S_N, S_Y, S_N, string.Empty, S_Y); //설비셋팅, Color셋팅, SetMode설정             
            SetAutoSearchCombo(cboAutoSearch);      //자동조회
             
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
        #region 4.0 InitControl
        /// <summary>
        /// 하위 Control Clear
        /// </summary> 
        private void InitRightGrid(FrameworkElement obj )
        {
            foreach (object o in LogicalTreeHelper.GetChildren(obj))
            {
                if (null != o)
                {
                    switch (o.GetType().Name)
                    {
                        case "TextBox":
                            (o as TextBox).Clear();
                            break;
                        case "C1ComboBox":
                            if ((o as C1ComboBox).Items.Count > 0) (o as C1ComboBox).SelectedIndex = 0;
                            break;
                    }
                }
            }
        }
        #endregion // InitControl

        #region #4.1 Set Combo
        /// <summary>
        /// 동조회
        /// </summary>
        private void SetScLineCombo(DataTable dtResult)
        { 
            cboSCLine.DisplayMemberPath = "CBO_NAME";
            cboSCLine.SelectedValuePath = "CBO_CODE";
            cboSCLine.ItemsSource =  dtResult.AsDataView();

            if (cboSCLine.Items.Count > 0)
            {
                cboSCLine.SelectedIndex = cboSCLine.Items.Count - 1;
            } 
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

        /// <summary>
        /// 동조회
        /// </summary>
        private void SetRackSetCombo(DataTable dtResult)
        {
            cboSetMode.DisplayMemberPath = "CBO_NAME";
            cboSetMode.SelectedValuePath = "CBO_CODE";
            cboSetMode.ItemsSource = dtResult.AsDataView(); 
        }
        #endregion //4.1 Set Combo

        #region 4.2 Rack Status Color 
        /// <summary>
        /// Rack Color Set
        /// </summary>
        /// <param name="dt"></param>
        private void SetColorGrid(DataTable dt)
        {   //grdMain Rack Col
            for (int i = 0; i < dt.Rows.Count + 2; i++)
            {
                ColumnDefinition c1 = new ColumnDefinition();
                c1.Width = new GridLength(0, GridUnitType.Auto);
                this.grdColor.ColumnDefinitions.Add(c1);
            }

            this.grdColor.ColumnDefinitions[0].Width = new GridLength(5, GridUnitType.Pixel); 
            this.grdColor.ColumnDefinitions[dt.Rows.Count + 1].Width = new GridLength(5, GridUnitType.Pixel);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                C1DataGrid dgNew = new C1DataGrid();

                C1.WPF.DataGrid.DataGridTextColumn textColumn1 = new C1.WPF.DataGrid.DataGridTextColumn();
                textColumn1.Header = "Color";
                textColumn1.Binding = new Binding("COM_CODE_NAME");
                textColumn1.HorizontalAlignment = HorizontalAlignment.Center;
                textColumn1.IsReadOnly = true;

                C1.WPF.DataGrid.DataGridTextColumn textColumn2 = new C1.WPF.DataGrid.DataGridTextColumn();
                textColumn2.Header = "Color";
                textColumn2.Binding = new Binding("ATTR1");
                textColumn2.HorizontalAlignment = HorizontalAlignment.Center;
                textColumn2.IsReadOnly = true;
                textColumn2.Visibility = Visibility.Collapsed;

                C1.WPF.DataGrid.DataGridTextColumn textColumn3 = new C1.WPF.DataGrid.DataGridTextColumn();
                textColumn3.Header = "Color";
                textColumn3.Binding = new Binding("ATTR2");
                textColumn3.HorizontalAlignment = HorizontalAlignment.Center;
                textColumn3.IsReadOnly = true;
                textColumn3.Visibility = Visibility.Collapsed;

                dgNew.Columns.Add(textColumn1);
                dgNew.Columns.Add(textColumn2);
                dgNew.Columns.Add(textColumn3);
                 
                dgNew.HeadersVisibility = C1.WPF.DataGrid.DataGridHeadersVisibility.None;
                dgNew.FrozenColumnCount = 0;
                dgNew.SelectionMode = C1.WPF.DataGrid.DataGridSelectionMode.SingleRow;
                dgNew.LoadedCellPresenter += grdColorLegend_LoadedCellPresenter;
                dgNew.SelectedBackground = null;
                
                Grid.SetRow(dgNew, 0);
                Grid.SetColumn(dgNew, i + 1);

                grdColor.Children.Add(dgNew);

                DataTable dtRow = new DataTable();
                dtRow.Columns.Add("COM_CODE_NAME", typeof(string));
                dtRow.Columns.Add("ATTR1", typeof(string));
                dtRow.Columns.Add("ATTR2", typeof(string));

                DataRow drRow = dtRow.NewRow();
                drRow["COM_CODE_NAME"] = dt.Rows[i]["COM_CODE_NAME"];
                drRow["ATTR1"] = dt.Rows[i]["ATTR1"];
                drRow["ATTR2"] = dt.Rows[i]["ATTR2"];
                dtRow.Rows.Add(drRow);

                Util.GridSetData(dgNew, dtRow, FrameOperation, true);

            }
        }

        /// <summary>
        /// Cell Color 
        /// </summary>
        private void grdColorLegend_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(e.Cell.Column.Name) == "COM_CODE_NAME")
                    {
                        e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Background = new BrushConverter().ConvertFromString(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ATTR1").ToString()) as SolidColorBrush;

                        if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ATTR2")).ToString()))
                        {
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Foreground = new BrushConverter().ConvertFromString(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ATTR2")).ToString()) as SolidColorBrush;
                        }
                    }
                }

            }));
        }
        #endregion //4.2 Rack Status Color

        #region #4.3 Search
        /// <summary>
        /// STK RACK 조회
        /// </summary>
        private void SearchProcess()
        {
            PackSTK_DataManager.Instance.SocketEventManager = new SocketEventManager();

            #region #. validatoin and initialize
            string AREAID = Util.GetCondition(cboSCLine);
            if (cboSCLine.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("SC_LINE")); // %1(을)를 선택하세요.
                this.cboSCLine.Focus();
                return;
            }

            //title초기화
            this.tbTitle.Text = ObjectDic.Instance.GetObjectName("SEL_RACK_INFO");

            this.grdColor.ColumnDefinitions.Clear();
            this.grdColor.RowDefinitions.Clear();
            this.grdColor.Children.Clear();

            this.grdMain.ColumnDefinitions.Clear();
            this.grdMain.RowDefinitions.Clear();
            this.grdMain.Children.Clear();

            InitRightGrid(this.grdRightMain1);
            InitRightGrid(this.grdRightMain2);
            #endregion

            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();

            try
            {
                this.SearchGrdMain(S_Y, S_N, S_N);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }

            this.loadingIndicator.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Rack Info Design
        /// </summary>
        private void SearchGrdMain(string sRACKINFO_YN = S_N, string sEQPT_YN = S_N, string sRACK_YN = S_N, string sRackID = "", string sSET_YN = S_N)
        { 
            DataSet dsResult = SearchRackInfo(sRACKINFO_YN, sEQPT_YN, sRACK_YN, sRackID, sSET_YN);

            if (sEQPT_YN.Equals(S_Y))
            {
                SetScLineCombo(dsResult.Tables["OUT_EQPT"]);      //설비콤보셋팅
            }

            if (sSET_YN.Equals(S_Y))
            {
                SetRackSetCombo(dsResult.Tables["OUT_RACK_SET"]);      //SetMode
            }
            
            if (sRACKINFO_YN.Equals(S_Y))
            {
                SetColorGrid(dsResult.Tables["OUT_COLORLEGEND"]); //Rack Color Setting
                CreateRack(dsResult.Tables["OUT_RACKINFO"], dsResult.Tables["OUT_MAX_XYZ"]); //Rack settting
                CreateRackStatistics(dsResult.Tables["OUT_RACK_CNT"]); //Rack사용통계
            } 
        }

        /// <summary>
        /// RACK Info 조회
        /// </summary>
        private DataSet SearchRackInfo(string sRACKINFO_YN = S_N, string sEQPT_YN = S_N, string sRACK_YN = S_N, string sRackID = "", string sSET_YN = S_N)
        {
            #region Rack Info            
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("RACK_ID", typeof(string));
            inDataTable.Columns.Add("RACKINFO_YN", typeof(string));
            inDataTable.Columns.Add("EQPT_YN", typeof(string));
            inDataTable.Columns.Add("RACKSUM_YN", typeof(string));
            inDataTable.Columns.Add("RACK_YN", typeof(string));
            inDataTable.Columns.Add("SET_YN", typeof(string));

            inDataTable = indataSet.Tables["INDATA"];

            DataRow newRow = inDataTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            newRow["EQPTID"] = cboSCLine.SelectedValue;
            if (!string.IsNullOrEmpty(sRackID)) newRow["RACK_ID"] = sRackID;
            newRow["RACKINFO_YN"] = sRACKINFO_YN;
            newRow["EQPT_YN"] = sEQPT_YN; 
            newRow["RACK_YN"] = sRACK_YN;
            newRow["SET_YN"] = sSET_YN;

            inDataTable.Rows.Add(newRow);
            return new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_PACK_STK_RACK_INFO", "INDATA", "OUT_MAX_XYZ,OUT_RACKINFO,OUT_EQPT,OUT_COLORLEGEND,OUT_RACK_CNT,OUT_RACK_SET", indataSet);
            #endregion // Rack Info
        }
        #endregion //4.3 Search

        #region #4.4 CreateRack
        /// <summary>
        /// Rack 생성
        /// </summary>
        public void CreateRack(DataTable dtRackInfo, DataTable dtMaxXYZ)
        {
            try
            { 
                #region Grid설정
                int xMax = Convert.ToInt16(dtMaxXYZ.Rows[0]["X_ROW"]);
                int yMax = Convert.ToInt16(dtMaxXYZ.Rows[0]["Y_COL"]);
                int zMax = Convert.ToInt16(dtMaxXYZ.Rows[0]["Z_LAY"]);

                int iMaxRow = xMax * zMax + 5;
                int iMaxCol = yMax + 4;

                int i1stHeader = 1;
                int iRowSpace = i1stHeader + zMax * 2 + 1;
                int i2ndHeader = iRowSpace + 1; 

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
                #endregion //Grid설정
                 

                #region Rack설정               
                //Create Rack
                foreach (DataRow row in dtRackInfo.Rows)
                {
                    Grid stpnlRackList = new Grid();
                    stpnlRackList.VerticalAlignment = VerticalAlignment.Stretch;
                    this.BindRack(stpnlRackList, row);

                    int iRow = Convert.ToInt16(row["X_ROW"]);
                    int iCol = Convert.ToInt16(row["Y_COL"]);

                    Grid.SetColumn(stpnlRackList, iCol);
                    Grid.SetRow(stpnlRackList, iRow);
                    if (row["MCS_CST_ID"].ToString().Equals("ROW"))
                    {
                        Grid.SetRowSpan(stpnlRackList, zMax);
                    }

                    if (row["DISP_REVERSE"].ToString().Equals("Y"))
                    {
                        UcPackSTK_Rack_Info ucf = new UcPackSTK_Rack_Info();
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
                    }

                    this.grdMain.Children.Add(stpnlRackList);
                }

                //Header
                this.grdMain.RowDefinitions[i1stHeader].Height = new GridLength(27, GridUnitType.Pixel);
                this.grdMain.RowDefinitions[i2ndHeader - 1].Height = new GridLength(8, GridUnitType.Pixel);
                this.grdMain.RowDefinitions[i2ndHeader].Height = new GridLength(27, GridUnitType.Pixel);

                //외곽그리드
                this.grdMain.RowDefinitions[0].Height = new GridLength(5, GridUnitType.Pixel);
                this.grdMain.ColumnDefinitions[0].Width = new GridLength(5, GridUnitType.Pixel);
                this.grdMain.RowDefinitions[iMaxRow - 1].Height = new GridLength(5, GridUnitType.Pixel);
                this.grdMain.ColumnDefinitions[iMaxCol - 1].Width = new GridLength(5, GridUnitType.Pixel);

                //랙명 <-> Lot 수량
                //this.btnReverse.Visibility = Visibility.Visible;
                this.bRackName = true;
                //this.btnReverse.Content = ObjectDic.Instance.GetObjectName("LOT_CNT");
                #endregion //Rack설정 
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
            UcPackSTK_Rack uc = new UcPackSTK_Rack();
            uc.Margin = new Thickness(1, 1, 1, 1);
            uc.setRackInfo(row);
            uc.rackEvent += Uc_rackEvent;

            target.Children.Add(uc);
        }

        /// <summary>
        /// Rack 개별 Event By Rack Click
        /// </summary> 
        private void Uc_rackEvent(string sRackID, string sRackName = "")
        { 
            this.txtRack_ID.Text = sRackID;

            //Rack Lot List 조회
            if (string.IsNullOrEmpty(sRackID)) return;

            RackInfo(sRackID);
        }

        /// <summary>
        /// Rack List 호출 By Tag Info
        /// </summary> 
        private void rackLotEvent(bool bSearch = false, string sRackID = "", string sRackName = "")
        { 
            this.txtRack_ID.Text = sRackID;
            //Rack Lot List 조회
            if (!bSearch || string.IsNullOrEmpty(sRackID)) return;

            RackInfo(sRackID);
        }

        #endregion //4.4 CreateRack

        #region #4.4.1 Rack Statistics
        /// <summary>
        /// Rack사용통계
        /// </summary>
        /// <param name="dtRackStats"></param>
        private void CreateRackStatistics(DataTable dtRackStats)
        {
            //Control 초기화
            InitRightGrid(this.grdRightMain1);
            if (null == dtRackStats || dtRackStats.Rows.Count == 0) return;

            #region Control Value            
            this.txtRack_Cnt.Text = dtRackStats.Rows[0]["RACK_CNT"].ToString();                     //전체Rack
            this.txtRack_Cnt_Use_All.Text = dtRackStats.Rows[0]["RACK_CNT_USE_ALL"].ToString();     //전체사용Rack
            this.txtRack_Cnt_Use_Full.Text = dtRackStats.Rows[0]["RACK_CNT_USE_FULL"].ToString();   //입고사용Rack
            this.txtRack_Cnt_Use_Empty.Text = dtRackStats.Rows[0]["RACK_CNT_USE_EMPTY"].ToString(); //공Tray사용Rack
            this.txtRack_Cnt_Iss_Rcv.Text = dtRackStats.Rows[0]["RACK_CNT_ISS_RCV"].ToString();     //출고예약Rack
            this.txt1RackCnt.Text = dtRackStats.Rows[0]["RACK_CNT_LAY1"].ToString();                //1단사용Rack
            this.txt2RackCnt.Text = dtRackStats.Rows[0]["RACK_CNT_LAY2"].ToString();                //2단사용Rack
            this.txt3RackCnt.Text = dtRackStats.Rows[0]["RACK_CNT_LAY3"].ToString();                //3단사용Rack
            this.txt4RackCnt.Text = dtRackStats.Rows[0]["RACK_CNT_LAY4"].ToString();                //4단사용Rack
            this.txt5RackCnt.Text = dtRackStats.Rows[0]["RACK_CNT_LAY5"].ToString();                //5단사용Rack
            this.txtPossibleCnt.Text = dtRackStats.Rows[0]["RACK_CNT_USABLE"].ToString();           //사용가능Rack
            this.txtImpossibleCnt.Text = dtRackStats.Rows[0]["RACK_CNT_UNUSE"].ToString();          //입고금지Rack
            this.txtTroubleCnt.Text = dtRackStats.Rows[0]["RACK_CNT_ABNORMAL"].ToString();          //비정상Rack
            
            this.txtLot_Cnt.Text = dtRackStats.Rows[0]["LOT_CNT"].ToString();                       //전체Cell갯수
            this.txtRack_Cnt_Use_All_R.Text = dtRackStats.Rows[0]["RACK_CNT_USE_ALL_R"].ToString();    
            this.txtRack_Cnt_Use_Full_R.Text = dtRackStats.Rows[0]["RACK_CNT_USE_FULL_R"].ToString();   
            this.txtRack_Cnt_Use_Empty_R.Text = dtRackStats.Rows[0]["RACK_CNT_USE_EMPTY_R"].ToString();  
            this.txtRack_Cnt_Iss_Rcv_R.Text = dtRackStats.Rows[0]["RACK_CNT_ISS_RCV_R"].ToString();    
            this.txt1RackCnt_R.Text = dtRackStats.Rows[0]["RACK_CNT_LAY1_R"].ToString();            
            this.txt2RackCnt_R.Text = dtRackStats.Rows[0]["RACK_CNT_LAY2_R"].ToString();            
            this.txt3RackCnt_R.Text = dtRackStats.Rows[0]["RACK_CNT_LAY3_R"].ToString();            
            this.txt4RackCnt_R.Text = dtRackStats.Rows[0]["RACK_CNT_LAY4_R"].ToString();            
            this.txt5RackCnt_R.Text = dtRackStats.Rows[0]["RACK_CNT_LAY5_R"].ToString();            
            this.txtPossibleCnt_R.Text = dtRackStats.Rows[0]["RACK_CNT_USABLE_R"].ToString();         
            this.txtImpossibleCnt_R.Text = dtRackStats.Rows[0]["RACK_CNT_UNUSE_R"].ToString();
            this.txtTroubleCnt_R.Text = dtRackStats.Rows[0]["RACK_CNT_ABNORMAL_R"].ToString();
            #endregion 
        }
        #endregion //#4.4.1 Rack Statistics

        #region #4.4.2 Rack Info
        /// <summary>
        /// Rack 정보 조회
        /// </summary> 
        private void RackInfo(string sRackID)
        {
            //Control 초기화
            InitRightGrid(this.grdRightMain2);

            DataSet dsResult = SearchRackInfo(S_Y, S_N, S_Y, sRackID);
            if (null == dsResult.Tables["OUT_RACKINFO"] || dsResult.Tables["OUT_RACKINFO"].Rows.Count == 0) return;

            #region Control Value 
            if (string.IsNullOrEmpty(dsResult.Tables["OUT_RACKINFO"].Rows[0]["LOT_INFO"].ToString()))
            {
                this.tbTitle.Text = ObjectDic.Instance.GetObjectName("SEL_RACK_INFO");                  //title초기화  
            }
            else
            {
                this.tbTitle.Text = string.Format("{0}  [ {1} ]", ObjectDic.Instance.GetObjectName("SEL_RACK_INFO"), dsResult.Tables["OUT_RACKINFO"].Rows[0]["LOT_INFO"].ToString());                  //title초기화  
            }          
            this.txtRack_ID.Text = dsResult.Tables["OUT_RACKINFO"].Rows[0]["RACK_ID"].ToString();
            this.txtLocation.Text = string.Format("{0}{1}-{2}{3}-{4}{5}", dsResult.Tables["OUT_RACKINFO"].Rows[0]["X_PSTN"].ToString(), ObjectDic.Instance.GetObjectName("열"),
                dsResult.Tables["OUT_RACKINFO"].Rows[0]["Y_PSTN"].ToString(), ObjectDic.Instance.GetObjectName("연"),
                dsResult.Tables["OUT_RACKINFO"].Rows[0]["Z_PSTN"].ToString(), ObjectDic.Instance.GetObjectName("단"));
            this.txtRackStat.Text = dsResult.Tables["OUT_RACKINFO"].Rows[0]["RACK_STAT_NAME"].ToString();
            this.txtAbnormStat.Text = dsResult.Tables["OUT_RACKINFO"].Rows[0]["ABNORM_STAT_NAME"].ToString();
            this.txtCstId.Text = dsResult.Tables["OUT_RACKINFO"].Rows[0]["CURR_CST_ID"].ToString();
            this.txtInDate.Text = dsResult.Tables["OUT_RACKINFO"].Rows[0]["RCV_DTTM"].ToString();
            this.txtDeepFlag.Text = dsResult.Tables["OUT_RACKINFO"].Rows[0]["DBLRCHSTK_DEEP_RACK_FLAG"].ToString();              
            #endregion 
        }
        #endregion //#4.4.1 Rack Statistics

        #region #5. 출고예약
        /// <summary>
        /// 공Tray 출고예약 
        /// </summary>
        private void btnIssReserve_Click(object sender, RoutedEventArgs e)
        {
            string[] parm = new string[4];
            parm[0] = cboSCLine.SelectedValue.ToString();
            parm[1] = "EMPTY";
            parm[2] = "ISS";
            ISS_CANCEL(parm);
        }

        /// <summary>
        /// 공Tray 출고예약 취소
        /// </summary>
        private void btnIssReserveCancel_Click(object sender, RoutedEventArgs e)
        {
            string[] parm = new string[4];
            parm[0] = cboSCLine.SelectedValue.ToString();
            parm[1] = "EMPTY";
            parm[2] = "CANCEL";
            ISS_CANCEL(parm);
        }

        /// <summary>
        /// 출고예약/취소 
        /// </summary> 
        private void ISS_CANCEL(string[] parm)
        {
            PACK003_044_ISS_POPUP wndPopup = new PACK003_044_ISS_POPUP();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] parameters = new object[4];
                parameters[0] = parm[0];
                parameters[1] = parm[1];
                parameters[2] = parm[2];
                C1WindowExtension.SetParameters(wndPopup, parameters);
                wndPopup.Closed -= new EventHandler(Popup_Closed);
                wndPopup.Closed += new EventHandler(Popup_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        /// <summary>
        /// 출고예약/취소 후 재조회
        /// </summary> 
        private void Popup_Closed(object sender, EventArgs e)
        {
            MessageBoxResult messageBoxResult = ((sender as PACK003_044_ISS_POPUP)).DialogResult;
            if (messageBoxResult == MessageBoxResult.OK)
            {
                this.SearchProcess();
            }
        }
        #endregion //출고예약

        #region #8. Rack상태 변경
        private void cboSetMode_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Util.GetCondition(cboSetMode).Equals("UNUSE"))  //입고금지
            {
                txtRemark.IsEnabled = true;
                this.tbReason.Text = ObjectDic.Instance.GetObjectName("RCV_BAN_REASON");
            }
            else if (Util.GetCondition(cboSetMode).Equals("CHECK_ISS"))  //출고금지
            {
                txtRemark.IsEnabled = true;
                this.tbReason.Text = ObjectDic.Instance.GetObjectName("ISS_BAN_REASON");
            }
            else
            {
                txtRemark.Text = string.Empty;
                txtRemark.IsEnabled = false;
            }
        }

        /// <summary>
        /// Rack상태저장
        /// </summary>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtRack_ID.Text.Trim())) return;

            string sStatus = string.Empty;

            if (string.IsNullOrEmpty(Util.GetCondition(cboSetMode)))
            {
                Util.MessageValidation("FM_ME_0137");  //변경할 상태를 선택해주세요.
                return;
            }
            else
            {
                sStatus = Util.GetCondition(cboSetMode);

                if (sStatus.Equals("UNUSE") && string.IsNullOrEmpty(txtRemark.Text.Trim()))
                {
                    Util.MessageValidation("FM_ME_0196");  //입고금지 내용을 입력해주세요.
                    return;
                }
                else if (sStatus.Equals("CHECK_ISS") && string.IsNullOrEmpty(txtRemark.Text.Trim()))
                {
                    Util.MessageValidation("FM_ME_486");  //출고금지 내용을 입력해주세요.
                    return;
                }
            }

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("RACK_STAT_CODE", typeof(string));
                dtRqst.Columns.Add("RACK_ID", typeof(string));
                dtRqst.Columns.Add("RACK_INFO_DEL_FLAG", typeof(string));
                dtRqst.Columns.Add("RCV_PRHB_RSN", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["RACK_STAT_CODE"] = sStatus;
                dr["RACK_ID"] = this.txtRack_ID.Text.Trim();

                dr["RCV_PRHB_RSN"] = Util.GetCondition(txtRemark);
                dr["USERID"] = LoginInfo.USERID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_PACK_STK_RACK_STAT_SET", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows[0]["RESULT"].ToString().Equals("0"))
                {
                    if (sStatus.ToString().Equals("UNUSE"))
                    {
                        Util.MessageValidation("FM_ME_0062");  //Rack 설정 변경에 실패하였습니다.\r\n입고 금지 Rack만 설정 가능합니다.
                    }
                    else
                    {
                        Util.MessageValidation("FM_ME_0061");  //Rack 설정 변경에 실패하였습니다.
                    }
                }
                else
                {
                    Util.MessageValidation("FM_ME_0063");  //Rack 설정 변경을 완료하였습니다.
                    this.SearchProcess();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion //8. Rack상태 변경

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