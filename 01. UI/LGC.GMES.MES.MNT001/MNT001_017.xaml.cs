/*************************************************************************************
 Created Date : 2020.11.13
      Creator : 염규범
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.13 염규범 Initial Created. CSR ID 4087361  [요청] Cell 실시간 공급 정보 제공 |[요청번호]C20190918_87361 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using DataGridCell = C1.WPF.DataGrid.DataGridCell;
using DataGridColumn = C1.WPF.DataGrid.DataGridColumn;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.IO;
using System.Configuration;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MNT001
{
    public partial class MNT001_017 : Window
    {
        #region Declaration & Constructor

        private Storyboard sbExpandLeft;
        private Storyboard sbCollapseLeft;

        private static string selectedShop;
        private static string selectedArea;
        private static string selectedEquipmentSegment;
        private static string selectedDisplayName;
        private static int selectedDisplayTime = 1;
        private static int selectedDisplayTimeSub = 10;
        private static int seletedViewRowCnt = 7;

        private static DateTime dtMainStartTime;
        private static DateTime dtSubStartTime;

        DataTable dtMain = new DataTable();

        int oneColumn_width = 0;
        int oneRow_width = 0;
        private DispatcherTimer _timer_NowDate;         //현재시간 가져오기(1초당)
        private DispatcherTimer _timer_DataSearch;      //data 새로 가져오기(기준:분)      

        public DataTable DefectData { get; set; }
        private DataTable[] dtDiv; //로테이션할 DataTable 갯수
        int div = 0; //로테이션할 화면 갯수
        int realViewCnt = 0; //현재 보이는 화면 번호



        DateTime shop_open_time;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion Declaration & Constructor

        #region Initialize 
        public MNT001_017()
        {
            try
            {
                InitializeComponent();

                this.Loaded += UserControl_Loaded;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;

            Initialize();

            //화면 사이즈 조정
            InitColumnWidth();

            _timer_NowDate = new DispatcherTimer();
            _timer_NowDate.Interval = TimeSpan.FromSeconds(1);
            _timer_NowDate.Tick += timer_Tick;
            _timer_NowDate.Start();

           // _timer_DataSearch = new DispatcherTimer();
            
            searchData();

            _timer_DataSearch = new DispatcherTimer();
            _timer_DataSearch.Tick -= timer_SeachData_Tick;
            _timer_DataSearch.Stop();
            _timer_DataSearch.Interval = TimeSpan.FromSeconds(selectedDisplayTime == 0 ? 60 : selectedDisplayTime * 60);
            _timer_DataSearch.Tick += new EventHandler(timer_SeachData_Tick);
            _timer_DataSearch.Start();
        }

        private void Initialize()
        {
            try
            {
                InitCombo();

                //Storyboard =======================================================================================
                sbExpandLeft = (Storyboard)this.Resources["ExpandLeftFrameStoryboard"];
                sbCollapseLeft = (Storyboard)this.Resources["CollapseLeftFrameStoryboard"];

                if (existsMntConfig())
                {
                    DataSet ds = LGC.GMES.MES.MNT001.MNT_Common.GetConfigXML_PACK();

                    selectedShop = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["SHOP"]);
                    selectedArea = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["AREA"]);
                    selectedEquipmentSegment = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["EQUIPMENTSEGMENT"]);

                    selectedDisplayTime = Convert.ToInt32(ds.Tables["MNT_CONFIG"].Rows[0]["DISPLAYTIME"]);
                    selectedDisplayTimeSub = ds.Tables["MNT_CONFIG"].Columns["DISPLAYTIMESUB"] == null ? selectedDisplayTimeSub : Convert.ToInt32(ds.Tables["MNT_CONFIG"].Rows[0]["DISPLAYTIMESUB"]);

                    cboShop.SelectedValue = selectedShop;
                    cboArea.SelectedValue = selectedArea;

                    numRefresh.Value = selectedDisplayTime;

                    // 나중에 오픈
                    //DataSearch_Start_Timer(); //data Search 타이머


                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }

        #region [ 화면 사이즈 조정 ]
        private void InitColumnWidth()
        {
            int W = Screen.PrimaryScreen.Bounds.Width; //모니터 스크린 가로크기
            int H = Screen.PrimaryScreen.Bounds.Height; //모니터 스크린 세로크기

            double w_temp = (W - 950 - 2); //전체 넓이에서 고정된 column의 넓이 뺌.
            double h_temp = (H - 70);  //전체 높이에서 고정된 Grid의 높이 뺌

            oneColumn_width = Convert.ToInt32(Math.Round(w_temp / 5)); // 반올림 : 고정되지 않은 컬럼수로 나눔
            oneRow_width = Convert.ToInt32(Math.Round(h_temp / 12)); // 반올림 : 고정되지 않은 GRID 수로 나눔

            fullSize.Width = new GridLength(W);
            dgStock.Width = W;
            dgStock.Height = Convert.ToInt32(Math.Round(h_temp)) - 55;
        }
        #endregion

        #region [ 좌측 설정 표시 ]
        private void btnLeftFrame_Checked(object sender, RoutedEventArgs e)
        {
            sbExpandLeft.Begin();
        }

        private void btnLeftFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            sbCollapseLeft.Begin();
        }
        #endregion

        #region [ 설정 파일 유무 확인 ] 
        private Boolean existsMntConfig()
        {

            try
            {
                string settingFileName = "GMES.MES.MNT_Config_PACK.config";
                string customConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\GMES\\SFU\\";

                FileInfo customConfigFile;
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
                {
                    customConfigFile = new FileInfo(customConfigPath + settingFileName);
                }
                else
                {
                    customConfigPath = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_PATH"];

                    string[] directoryNames = customConfigPath.Split('\\');
                    string current = string.Empty;
                    for (int inx = 0; inx < directoryNames.Length - 1; inx++)
                    {
                        string directoryName = directoryNames[inx];
                        if (string.IsNullOrEmpty(current))
                        {
                            current = directoryName;
                        }
                        else
                        {
                            current += @"\" + directoryName;
                        }
                        current = current.Replace("C:", @"\\Client\C$");
                        DirectoryInfo directoryInfo = new DirectoryInfo(current);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                    }
                    customConfigFile = new FileInfo(current + @"\" + settingFileName);
                }

                if (customConfigFile.Exists)
                {
                    return true;
                }
                else
                {
                    DataSet ds = new DataSet();

                    DataTable dt = new DataTable("MNT_CONFIG");
                    dt.Columns.Add("SHOP", typeof(string));
                    dt.Columns.Add("AREA", typeof(string));
                    dt.Columns.Add("EQUIPMENTSEGMENT", typeof(string));
                    dt.Columns.Add("PROCESS", typeof(string));
                    dt.Columns.Add("EQUIPMENT", typeof(string));
                    dt.Columns.Add("DISPLAYNAME", typeof(string));
                    dt.Columns.Add("DISPLAYTIME", typeof(int));

                    DataRow dr = dt.NewRow();
                    dr["SHOP"] = "";
                    dr["AREA"] = "";
                    dr["EQUIPMENTSEGMENT"] = "";
                    dr["PROCESS"] = "";
                    dr["EQUIPMENT"] = "";
                    dr["DISPLAYNAME"] = "";
                    dr["DISPLAYTIME"] = 1;

                    dt.Rows.Add(dr);
                    ds.Tables.Add(dt);

                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);

                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }



        }
        #endregion

        #region [ InitCombo ] 
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //샵(공장)  
            SetShop(cboShop);

            //동    
            SetArea(cboArea);

            //라인
            //SetLine(cboEquipmentSegment);
        }

        #endregion

        #endregion 

        #region Event

        #region [ Combo Event ] 
        private void cboShop_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!(string.IsNullOrEmpty(cboShop.SelectedIndex.ToString())) || cboShop.SelectedIndex > -1)
                {
                    selectedShop = Convert.ToString(cboShop.SelectedValue);

                    SetArea(cboArea);
                }
                else
                {
                    selectedShop = string.Empty;
                }
            }));
        }

        private void cboArea_SeletedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!(string.IsNullOrEmpty(cboArea.SelectedIndex.ToString())) || cboArea.SelectedIndex > -1)
                {
                    SetLine(cboEquipmentSegment);
                    if (!(string.IsNullOrEmpty(selectedEquipmentSegment)))
                    {
                        string[] splitEqsg = selectedEquipmentSegment.Split(',');
                        foreach (string str in splitEqsg)
                        {
                            cboEquipmentSegment.Check(str);
                        }
                    }
                }
            }));
        }

        #endregion Combo

        #region [ 화면 종료 ] 
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            timers_dispose(); //화면 종료시 타이머 모두 죽임.            
        }
        #endregion

        #region 타이머

        #region [ 현재 시간 알려주는 타이머 ] 

        void timer_Tick(object sender, EventArgs e)
        {
            setTitle();
            tbRefreshTime.Text = DateTime.Now.ToString();
        }

        void timer_SeachData_Tick(object sender, EventArgs e)
        {
            searchData();
        }

        #endregion

        #region [ 타이머 종료 ]
        private void timers_dispose()
        {
            if (_timer_DataSearch != null)
            {
                _timer_DataSearch.Stop();
                _timer_DataSearch = null;
            }

            if (_timer_NowDate != null)
            {
                _timer_NowDate.Stop();
                _timer_NowDate = null;
            }

        }
        #endregion

        #endregion

        #region [ 화면 타이틀 설정 ]
        private void setTitle()
        {
            try
            {
                tbTitle.Text = ObjectDic.Instance.GetObjectName("Cell 재고 모니터링") + " ( " + (shop_open_time >= DateTime.Now ? (DateTime.Now.AddDays(-1).Month + "/" + DateTime.Now.AddDays(-1).Day) : (DateTime.Now.Month + "/" + DateTime.Now.Day)) + " ) ";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region Button Event 

        #region [ 설정 저장 ]
        private void btnSetSave_Click(object sender, RoutedEventArgs e)
        {
            selectedShop = Convert.ToString(cboShop.SelectedValue);
            selectedArea = Convert.ToString(cboArea.SelectedValue);
            //selectedEquipmentSegment = Convert.ToString(cboEquipmentSegment.SelectedItemsToString);

            selectedDisplayTime = Convert.ToInt32(numRefresh.Value);

            DataSet ds = new DataSet();

            DataTable dt = new DataTable("MNT_CONFIG");
            dt.Columns.Add("SHOP", typeof(string));
            dt.Columns.Add("AREA", typeof(string));
            dt.Columns.Add("EQUIPMENTSEGMENT", typeof(string));
            dt.Columns.Add("DISPLAYTIME", typeof(int));

            DataRow dr = dt.NewRow();
            dr["SHOP"] = selectedShop;
            dr["AREA"] = selectedArea;
           // dr["EQUIPMENTSEGMENT"] = selectedEquipmentSegment;
            dr["DISPLAYTIME"] = selectedDisplayTime;

            dt.Rows.Add(dr);
            ds.Tables.Add(dt);

            LGC.GMES.MES.MNT001.MNT_Common.SetConfigXML_PACK(ds);

            object[] obj = this.Tag as object[];
            if (string.IsNullOrWhiteSpace(selectedDisplayName) == true)
            {
                if (obj.Length != 0)
                {
                    tbTitle.Text = obj[0] as string;
                }
            }
            else
            {
                tbTitle.Text = selectedDisplayName;
            }

            dtMainStartTime = System.DateTime.Now;
            dtSubStartTime = dtMainStartTime;

            btnLeftFrame.IsChecked = false;
        }
        #endregion

        #region [ 설정 닫기 ]
        private void btnSetClose_Click(object sender, RoutedEventArgs e)
        {
            btnLeftFrame.IsChecked = false;
        }
        #endregion

        #region [ 화면 닫기 ] 
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region [ 설정 적용 ]
        private void btnSetData_Click(object sender, RoutedEventArgs e)
        {
            reSet();
        }
        #endregion

        #endregion

        #region Biz

        #region Combo
        private void SetShop(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHOP_M_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cbo.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_SHOP_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_SHOP_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetArea(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = Convert.ToString(cboShop.SelectedValue);
                dr["USERID"] = LoginInfo.USERID;
                dr["SYSTEM_ID"] = LoginInfo.SYSID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AUTH_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cbo.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_AREA_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_AREA_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetLine(MultiSelectionBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Convert.ToString(cboArea.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cbo.ItemsSource = dtResult.Copy().AsDataView();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion Combo

        #region SearchData
        private void searchData()
        {
            try
            {
                //DoEvents();

                DataSet ds = new DataSet();

                DataTable indata = new DataTable();
                indata.TableName = "INDATA";

                indata.Columns.Add("LANGID", typeof(string));
                indata.Columns.Add("SHOPID", typeof(string));
                indata.Columns.Add("AREAID", typeof(string));
                indata.Columns.Add("EQSGID", typeof(string));

                DataRow dr = indata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = string.IsNullOrEmpty(cboArea.SelectedValue.ToString()) ?  LoginInfo.CFG_AREA_ID : cboArea.SelectedValue.ToString();
                dr["EQSGID"] = string.IsNullOrEmpty(cboEquipmentSegment.SelectedItemsToString) ?  selectedEquipmentSegment : cboEquipmentSegment.SelectedItemsToString;

                indata.Rows.Add(dr);

                ds.Tables.Add(indata);

                new ClientProxy().ExecuteService_Multi("BR_REPORT_REQ_CELL_V2", "INDATA", "REQ_CELL", (dsRslt, bizException) =>
                {
                    try
                    {
                        

                        dgStock.LoadedCellPresenter += dgStock_LoadedCellPresenter;

                        btnSetData.IsEnabled = true;

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.GridSetData(dgStock, dsRslt.Tables["REQ_CELL"], FrameOperation, false);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }

                    if (dsRslt.Tables["REQ_CELL"].Rows.Count > 0)
                    {

                        string[] strColName = dsRslt.Tables["REQ_CELL"].Columns.Cast<DataColumn>()
                                                                                .Select(x => x.ColumnName)
                                                                                .ToArray();

                        for (int i = 0; strColName.Length > i; i++)
                        {
                            int j = 0;

                            if (int.TryParse(strColName[i], out j) && string.IsNullOrEmpty(dsRslt.Tables["REQ_CELL"].Rows[0][i].ToString()))
                            {
                                if (int.TryParse(dgStock.Columns[i - 1].Name, out j))
                                {
                                    dgStock.Columns[i - 1].Visibility = Visibility.Collapsed;
                                }
                            }

                        }   
                    }

                    dgStock.LoadedCellPresenter -= dgStock_LoadedCellPresenter;
                    
                }, ds);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region Grid

        #region 색칠 ... 다시 하자
        private void dgStock_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
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

                    int i = 0;

                    if (e.Cell.Row.Type == DataGridRowType.Item &&  int.TryParse(e.Cell.Column.Name, out i))
                    {
                        if(Util.NVC_Decimal(e.Cell.Value) > 0)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                            dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.Black);
                            dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.Black);
                        }
                        else
                        {

                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                            dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.Black);
                            dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.Black);

                            for (int j=1; 2 >= j; j++)
                            {
                                if (int.TryParse(dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Column.Name, out i))
                                {
                                    if (dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Value.GetDecimal() > 0)
                                    {                                                           
                                        dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                        dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                        dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                        dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                        dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                        dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Yellow);
                        e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                        dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.Black);
                        dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.Black);
                    }
                }));


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }



        }
        #endregion


        #endregion Event

        #region Mehod



        private List<DataGridCellsRange> Merge(System.Windows.Controls.Orientation orientation, DataGridRow[] rows, DataGridColumn[] columns, bool hierarchical)
        {
            var merges = new List<DataGridCellsRange>();

            if ((rows.Length == 0) || (columns.Length == 0))
                return merges;

            var datagrid = rows[0].DataGrid;
            DataGridCellsRange currentRange = null;
            var iterationLength = (orientation == System.Windows.Controls.Orientation.Vertical) ? rows.Length : columns.Length;
            int i = 0;

            while (i < iterationLength)
            {
                // skip empty cells 
                DataGridCell nextCell = null;
                while (nextCell == null && i < iterationLength)
                {
                    nextCell = (orientation == System.Windows.Controls.Orientation.Vertical) ? datagrid[rows[i], columns[0]] : datagrid[rows[0], columns[i]];
                    i++;
                }

                // there are no more cell in this column, end iteration
                if (nextCell == null)
                    break;

                // can expand the merge?
                if (CanMerge(orientation, currentRange, nextCell))
                {
                    // expand the merged range
                    currentRange = ExpandRange(currentRange, nextCell);
                }
                else
                {
                    // cannot merge anymore, add the last range we have
                    merges.Add(currentRange);
                    currentRange = ExpandRange(null, nextCell);
                }
            }

            // add last merge to the collection
            if (currentRange != null)
                merges.Add(currentRange);


            // recursion
            var pendingColumns = (orientation == System.Windows.Controls.Orientation.Vertical) ? columns.Skip(1).ToArray() : columns;
            var pendingRows = (orientation == System.Windows.Controls.Orientation.Vertical) ? rows : rows.Skip(1).ToArray();
            var innerMerges = new List<DataGridCellsRange>();

            if (!hierarchical)
            {
                // treat each row independently
                // and add inner merges to the results
                var tmp = Merge(orientation, pendingRows, pendingColumns, hierarchical);
                merges.AddRange(tmp);
            }
            else
            {
                // merge in the other direction, bounding to the parent range limits
                foreach (var range in new List<DataGridCellsRange>(merges))
                {
                    innerMerges = (orientation == System.Windows.Controls.Orientation.Vertical)
                                ? Merge(orientation, range.Rows.ToArray(), pendingColumns, hierarchical)
                                : Merge(orientation, pendingRows, range.Columns.ToArray(), hierarchical);

                    // look into the inner merged ranges, to check if possible to expand the current merge in the other direction
                    var continueMerging = true;
                    var expandedRange = range;

                    while (innerMerges.Count > 0 && continueMerging)
                    {
                        var tmp = innerMerges[0];
                        if (CanMerge(orientation.Opposite(), expandedRange, tmp))
                        {
                            expandedRange = ExpandRange(expandedRange, tmp.BottomRightCell);
                            innerMerges.Remove(tmp);
                        }
                        else
                        {
                            continueMerging = false;
                        }
                    }

                    // replace range for the expanded one
                    if (expandedRange != range)
                    {
                        merges[merges.IndexOf(range)] = expandedRange;
                    }

                    // and add inner merges to the results
                    merges.AddRange(innerMerges);
                }
            }

            return merges;
        }

        private bool CanMerge(System.Windows.Controls.Orientation orientation, DataGridCellsRange currentRange, DataGridCellsRange newRange)
        {
            if (currentRange == null)
                return true;

            var datagrid = newRange.TopLeftCell.DataGrid;

            if (orientation == System.Windows.Controls.Orientation.Vertical)
            {
                return (object.Equals(GetCellValue(currentRange.TopLeftCell), GetCellValue(newRange.TopLeftCell))
                        && (currentRange.TopLeftCell.Column == newRange.TopLeftCell.Column)
                        && (currentRange.BottomRightCell.Column == newRange.BottomRightCell.Column));
            }
            else
            {
                return (object.Equals(GetCellValue(currentRange.TopLeftCell), GetCellValue(newRange.TopLeftCell))
                        && (currentRange.TopLeftCell.Row == newRange.TopLeftCell.Row)
                        && (currentRange.BottomRightCell.Row == newRange.BottomRightCell.Row));
            }
        }

        private bool CanMerge(System.Windows.Controls.Orientation orientation, DataGridCellsRange currentRange, DataGridCell cell)
        {
            if (currentRange == null)
                return true;

            var datagrid = cell.DataGrid;
            var last = currentRange.BottomRightCell;
            var first = currentRange.TopLeftCell;

            if (orientation == System.Windows.Controls.Orientation.Vertical)
            {
                return (object.Equals(GetCellValue(first), GetCellValue(cell)))
                           && (last.Row.Index == cell.Row.Index - 1)
                           && (last.Row.ParentGroup == cell.Row.ParentGroup)
                           && (datagrid.FreezingSection(cell) == datagrid.FreezingSection(last));
            }
            else
            {
                return (object.Equals(GetCellValue(first), GetCellValue(cell)))
                       && (last.Column.DisplayIndex == cell.Column.DisplayIndex - 1)
                       && (last.Row.ParentGroup == cell.Row.ParentGroup)
                       && (datagrid.FreezingSection(cell) == datagrid.FreezingSection(last));
            }
        }

        public DataGridCellsRange ExpandRange(DataGridCellsRange currentRange, DataGridCell cell)
        {
            if (currentRange == null)
            {
                return new DataGridCellsRange(cell);
            }
            else
            {
                return new DataGridCellsRange(currentRange.TopLeftCell, cell);
            }
        }

        private object GetCellValue(DataGridCell cell)
        {
            // We used the binding here previously, but that doesn't work for column headers.
            if (cell.Row.Index < cell.DataGrid.TopRows.Count)
            {
                return (cell.Column.Header is List<string> && (cell.Column.Header as List<string>).Count > cell.Row.Index) ?
                    (cell.Column.Header as List<string>)[cell.Row.Index] : cell.Column.Header;
            }
            else
            {
                object content = cell.Presenter;
                while (true)
                {
                    if (content is ContentControl)
                    {
                        content = (content as ContentControl).Content;
                    }
                    else if (content is TextBlock)
                    {
                        return (content as TextBlock).Text;
                    }
                    else if (content is System.Windows.Controls.TextBox)
                    {
                        return (content as System.Windows.Controls.TextBox).Text;
                    }
                    else
                    {
                        return content;
                    }
                }
            }
        }

        private void reSet()
        {
            btnSetData.IsEnabled = false;
            searchData();
            
            _timer_DataSearch.Tick -= timer_SeachData_Tick;
            _timer_DataSearch.Stop();
            _timer_DataSearch = null;
            _timer_DataSearch = new DispatcherTimer();
            _timer_DataSearch.Interval = TimeSpan.FromSeconds(selectedDisplayTime == 0 ? 60 : selectedDisplayTime * 60);
            _timer_DataSearch.Tick += timer_SeachData_Tick;
            _timer_DataSearch.Start();
        }


        #endregion Mehod

        #region unmanaged code for window maximizing

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);


        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr MonitorFromPoint(POINT pt, MonitorOptions dwFlags);

        enum MonitorOptions : uint
        {
            MONITOR_DEFAULTTONULL = 0x00000000,
            MONITOR_DEFAULTTOPRIMARY = 0x00000001,
            MONITOR_DEFAULTTONEAREST = 0x00000002
        }

        [DllImport("user32.dll")]
        static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
            }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr mWindowHandle = (new WindowInteropHelper(this)).Handle;
            HwndSource.FromHwnd(mWindowHandle).AddHook(new HwndSourceHook(WindowProc));

            this.WindowState = System.Windows.WindowState.Maximized;
        }

        private static System.IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    break;
            }

            return IntPtr.Zero;
        }

        private static void WmGetMinMaxInfo(System.IntPtr hwnd, System.IntPtr lParam)
        {
            POINT lMousePosition;
            GetCursorPos(out lMousePosition);

            IntPtr lPrimaryScreen = MonitorFromPoint(new POINT(0, 0), MonitorOptions.MONITOR_DEFAULTTOPRIMARY);
            MONITORINFO lPrimaryScreenInfo = new MONITORINFO();
            if (GetMonitorInfo(lPrimaryScreen, lPrimaryScreenInfo) == false)
            {
                return;
            }

            IntPtr lCurrentScreen = MonitorFromPoint(lMousePosition, MonitorOptions.MONITOR_DEFAULTTONEAREST);

            MINMAXINFO lMmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            if (lPrimaryScreen.Equals(lCurrentScreen) == true)
            {
                lMmi.ptMaxPosition.X = lPrimaryScreenInfo.rcWork.Left;
                lMmi.ptMaxPosition.Y = lPrimaryScreenInfo.rcWork.Top;
                lMmi.ptMaxSize.X = lPrimaryScreenInfo.rcWork.Right - lPrimaryScreenInfo.rcWork.Left;
                lMmi.ptMaxSize.Y = lPrimaryScreenInfo.rcWork.Bottom - lPrimaryScreenInfo.rcWork.Top;
            }
            else
            {
                lMmi.ptMaxPosition.X = lPrimaryScreenInfo.rcMonitor.Left;
                lMmi.ptMaxPosition.Y = lPrimaryScreenInfo.rcMonitor.Top;
                lMmi.ptMaxSize.X = lPrimaryScreenInfo.rcMonitor.Right - lPrimaryScreenInfo.rcMonitor.Left;
                lMmi.ptMaxSize.Y = lPrimaryScreenInfo.rcMonitor.Bottom - lPrimaryScreenInfo.rcMonitor.Top;
            }

            Marshal.StructureToPtr(lMmi, lParam, true);
        }

        #endregion unmanaged code for window maximizing

        private void DoEvents()
        {
            //System.Windows.Application.Current.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));

        }
    }
}


//추후 필요
//dgStock.LoadedCellPresenter += dgStock_LoadedCellPresenter;