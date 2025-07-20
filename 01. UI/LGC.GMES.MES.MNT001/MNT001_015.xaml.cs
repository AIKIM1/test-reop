/*************************************************************************************
 Created Date : 2019.09.05
      Creator : 손우석
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2019.09.05 손우석 Initial Created. CSR ID 4087361  [요청] Cell 실시간 공급 정보 제공 |[요청번호]C20190918_87361 
  2019.09.17 손우석 색깔 표시  CSR ID 4087361  [요청] Cell 실시간 공급 정보 제공 |[요청번호]C20190918_87361
  2019.09.20  손우석 CSR ID 4087361  [요청] Cell 실시간 공급 정보 제공 |[요청번호]C20190918_87361 용어 변경
  2019.09.25  손우석 CSR ID 4087361  [요청] Cell 실시간 공급 정보 제공 |[요청번호]C20190918_87361  Grid 색상 변경
  2019.09.30  손우석 CSR ID 4087361  [요청] Cell 실시간 공급 정보 제공 |[요청번호]C20190918_87361 조회시 파라미터 초기화 수정 및 명칭 변경
  2019.10.01  손우석 CSR ID 4087361  [요청] Cell 실시간 공급 정보 제공 |[요청번호]C20190918_87361  Grid 색상 변경
  2020.05.19  염규범 오류성 내용 수정 DoEvent() 추가 처리
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
using System.Threading;
using System.IO;
using System.Configuration;

namespace LGC.GMES.MES.MNT001
{
    public partial class MNT001_015 : Window
    {
        #region Declaration & Constructor

        System.Timers.Timer tmrMainTimer;

        private Storyboard sbExpandLeft;
        private Storyboard sbCollapseLeft;

        private static string selectedShop;
        private static string selectedArea;
        private static string selectedEquipmentSegment;
        private static string selectedProcess;

        private static string selectedDisplayName;
        private static int selectedDisplayTime = 1;
        private static int selectedDisplayTimeSub = 10;
        private static int seletedViewRowCnt = 7;
        //private static string selectedColorDisplayFlag;

        private static DateTime dtMainStartTime;
        private static DateTime dtSubStartTime;

        //private int subTime = 10;

        DataTable dtMain = new DataTable();

        //private int currentPage = 0;

        int oneColumn_width = 0;
        int oneRow_width = 0;

        DataTable dtSearchTable;
        private System.Timers.Timer _timer_NowDate;         //현재시간 가져오기(1초당)
        private System.Timers.Timer _timer_DataSearch;      //data 새로 가져오기(기준:분)
        private System.Timers.Timer _timer_View_Rotation;   //화면전환하기(기준:초)       

        public DataTable DefectData { get; set; }
        DataTable dtSortTable;

        private DataTable[] dtDiv; //로테이션할 DataTable 갯수
        int div = 0; //로테이션할 화면 갯수
        int viewRowCnt = 0; //한번에 보여줄 Row 갯수
        int bindTableRowCnt = 0; //Binding할 DataTable의 Row수
        int rotationView = 0; // 화면전환속도
        int realViewCnt = 0; //현재 보이는 화면 번호

        bool first_access = true;

        //그리드 헤더에 적용할 날짜
        string now_date_header = string.Empty;
        string yester_date_header = string.Empty;
        string tomorrow_date_header = string.Empty;

        DateTime shop_open_time;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion Declaration & Constructor

        #region Initialize
        public MNT001_015()
        {
            try
            {
                InitializeComponent();

                this.Loaded += UserControl_Loaded;
                //dgInput.LoadedCellPresenter += dgInput_LoadedCellPresenter;
                //dgStock.LoadedCellPresenter += dgStock_LoadedCellPresenter;

                dgInput2.LoadedCellPresenter += dgInput_LoadedCellPresenter;
                dgStock2.LoadedCellPresenter += dgStock_LoadedCellPresenter;
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

            InitColumnWidth();

            InitSetting();
        }

        private void InitColumnWidth()
        {
            int W = Screen.PrimaryScreen.Bounds.Width; //모니터 스크린 가로크기
            int H = Screen.PrimaryScreen.Bounds.Height; //모니터 스크린 세로크기

            double w_temp = (W - 950 - 2); //전체 넓이에서 고정된 column의 넓이 뺌.
            double h_temp = (H - 70);  //전체 높이에서 고정된 Grid의 높이 뺌

            oneColumn_width = Convert.ToInt32(Math.Round(w_temp / 5)); // 반올림 : 고정되지 않은 컬럼수로 나눔
            oneRow_width = Convert.ToInt32(Math.Round(h_temp / 12)); // 반올림 : 고정되지 않은 GRID 수로 나눔

            fullSize.Width = new GridLength(W);
            dgInput2.Width = W;
            dgStock2.Width = W;
            dgInput2.Height = Convert.ToInt32(Math.Round(h_temp / 2)) - 55;
            dgStock2.Height = Convert.ToInt32(Math.Round(h_temp / 2)) - 55;
        }

        private void InitSetting()
        {
            try
            {
                setTitle();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            timers_dispose(); //화면 종료시 타이머 모두 죽임.            
        }

        private void timers_dispose()
        {
            if (_timer_DataSearch != null)
            {
                _timer_DataSearch.Stop();
                _timer_DataSearch.Dispose();
                _timer_DataSearch = null;
            }

            if (_timer_NowDate != null)
            {
                _timer_NowDate.Stop();
                _timer_NowDate.Dispose();
                _timer_NowDate = null;
            }

            if (_timer_View_Rotation != null)
            {
                _timer_View_Rotation.Stop();
                _timer_View_Rotation.Dispose();
                _timer_View_Rotation = null;
            }
        }

        private void setTitle()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrWhiteSpace(selectedDisplayName) == true)
                {
                    tbTitle.Text = ObjectDic.Instance.GetObjectName("Cell 재고 모니터링") + " ( " + (shop_open_time >= DateTime.Now ? (DateTime.Now.AddDays(-1).Month + "/" + DateTime.Now.AddDays(-1).Day) : (DateTime.Now.Month + "/" + DateTime.Now.Day)) + " ) ";
                }
                else
                {
                    tbTitle.Text = selectedDisplayName;
                }
            }));
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

                    seletedViewRowCnt = Convert.ToInt32(ds.Tables["MNT_CONFIG"].Rows[0]["DISPLAYVIEWROWCNT"]);
                    selectedDisplayTime = Convert.ToInt32(ds.Tables["MNT_CONFIG"].Rows[0]["DISPLAYTIME"]);
                    selectedDisplayTimeSub = ds.Tables["MNT_CONFIG"].Columns["DISPLAYTIMESUB"] == null ? selectedDisplayTimeSub : Convert.ToInt32(ds.Tables["MNT_CONFIG"].Rows[0]["DISPLAYTIMESUB"]);

                    cboShop.SelectedValue = selectedShop;
                    cboArea.SelectedValue = selectedArea;

                    numViewRowCnt.Value = seletedViewRowCnt;
                    numRefresh.Value = selectedDisplayTime;
                    numRefreshSub.Value = selectedDisplayTimeSub;

                    //realData(); //data 가져와서 그리드에 바인딩 시켜준 후 화면전환 타이머 작동

                    //NowDate_Start_Timer();

                    DataSearch_Start_Timer(); //data Search 타이머
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //샵(공장)  
            SetShop(cboShop);

            //동    
            SetArea(cboArea);

            //라인
            SetLine(cboEquipmentSegment);
        }

        private void NowDate_Start_Timer()
        {
            _timer_NowDate = new System.Timers.Timer(1000); //기준 : 초
            _timer_NowDate.AutoReset = true;
            _timer_NowDate.Start();
        }

        #endregion Initialize

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

        private void SetProcess(MultiSelectionBox cboMulti)
        {

        }

        private void SetElec(MultiSelectionBox cbo)
        {

        }
        #endregion Combo

        #region Event

        #region Grid
        private void grHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (this.WindowState == System.Windows.WindowState.Normal)
                {
                    this.WindowState = System.Windows.WindowState.Maximized;
                }
                else
                {
                    this.WindowState = System.Windows.WindowState.Normal;
                }
            }
            else
            {
                try
                {
                    this.DragMove();
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void dgPlaning_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dataGrid = (sender as C1DataGrid);

                if (!dataGrid.Dispatcher.CheckAccess()) //main UI 에서 호출
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    SetCellColor(dataGrid, e);
                }
                else //UI가 아닌 다른 Thread 호출시
                {
                    Action act = () =>
                    {
                        if (e.Cell.Presenter == null)
                        {
                            return;
                        }

                        SetCellColor(dataGrid, e);
                    };

                    dataGrid.Dispatcher.BeginInvoke(act);
                }

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgInput_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dataGrid = (sender as C1DataGrid);

                if (!dataGrid.Dispatcher.CheckAccess()) //main UI 에서 호출
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    SetCellColor(dataGrid, e);
                }
                else //UI가 아닌 다른 Thread 호출시
                {
                    Action act = () =>
                    {
                        if (e.Cell.Presenter == null)
                        {
                            return;
                        }

                        SetCellColor(dataGrid, e);
                    };

                    dataGrid.Dispatcher.BeginInvoke(act);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgStock_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dataGrid = (sender as C1DataGrid);

                if (!dataGrid.Dispatcher.CheckAccess()) //main UI 에서 호출
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    SetCellColor(dataGrid, e);
                }
                else //UI가 아닌 다른 Thread 호출시
                {
                    Action act = () =>
                    {
                        if (e.Cell.Presenter == null)
                        {
                            return;
                        }

                        SetCellColor(dataGrid, e);
                    };

                    dataGrid.Dispatcher.BeginInvoke(act);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion Grid

        #region Button
        private void btnSetSave_Click(object sender, RoutedEventArgs e)
        {
            selectedShop = Convert.ToString(cboShop.SelectedValue);
            selectedArea = Convert.ToString(cboArea.SelectedValue);
            selectedEquipmentSegment = Convert.ToString(cboEquipmentSegment.SelectedItemsToString);
            //selectedProcess = Convert.ToString(cboProcess.SelectedItemsToString);
            //selectedElec = Convert.ToString(cboElec.SelectedItemsToString);

            seletedViewRowCnt = Convert.ToInt32(numViewRowCnt.Value);
            selectedDisplayTime = Convert.ToInt32(numRefresh.Value);
            selectedDisplayTimeSub = Convert.ToInt32(numRefreshSub.Value); //
            //selectedColorDisplayFlag = Convert.ToString(cboColor.SelectedValue);

            DataSet ds = new DataSet();

            DataTable dt = new DataTable("MNT_CONFIG");
            dt.Columns.Add("SHOP", typeof(string));
            dt.Columns.Add("AREA", typeof(string));
            dt.Columns.Add("EQUIPMENTSEGMENT", typeof(string));
            dt.Columns.Add("PROCESS", typeof(string));
            dt.Columns.Add("ELEC_TYPE", typeof(string));
            dt.Columns.Add("DISPLAYVIEWROWCNT", typeof(int));
            dt.Columns.Add("DISPLAYTIME", typeof(int));
            dt.Columns.Add("DISPLAYTIMESUB", typeof(int));
            //dt.Columns.Add("COLOR_DISP_FLAG", typeof(string));

            DataRow dr = dt.NewRow();
            dr["SHOP"] = selectedShop;
            dr["AREA"] = selectedArea;
            dr["EQUIPMENTSEGMENT"] = selectedEquipmentSegment;
            dr["PROCESS"] = "";
            dr["ELEC_TYPE"] = "";
            dr["DISPLAYVIEWROWCNT"] = seletedViewRowCnt;
            dr["DISPLAYTIME"] = selectedDisplayTime;
            dr["DISPLAYTIMESUB"] = selectedDisplayTimeSub;
            //dr["COLOR_DISP_FLAG"] = selectedColorDisplayFlag;

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

        private void btnSetClose_Click(object sender, RoutedEventArgs e)
        {
            btnLeftFrame.IsChecked = false;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnLeftFrame_Checked(object sender, RoutedEventArgs e)
        {
            sbExpandLeft.Begin();
        }

        private void btnLeftFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            sbCollapseLeft.Begin();
        }

        private void btnSetData_Click(object sender, RoutedEventArgs e)
        {
            reSet();
        }

        #region [ 사용 안함 ]
        private void btnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            realViewCnt--;

            if (realViewCnt <= 0)
            {
                realViewCnt = div;
            }

            setChart(realViewCnt - 1);

            string msg = realViewCnt + " / " + div;
            SetUiCondition(tbPage, msg);
            SetUiCondition(tbRefreshTime, DateTime.Now.ToString());
        }

        private void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (realViewCnt >= div)
            {
                realViewCnt = 0;
            }

            setChart(realViewCnt);
            realViewCnt++;

            string msg = realViewCnt + " / " + div;
            SetUiCondition(tbPage, msg);
            SetUiCondition(tbRefreshTime, DateTime.Now.ToString());
        }
        #endregion

        #endregion Button

        #region Combo
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
                if (!(string.IsNullOrEmpty(cboArea.SelectedIndex.ToString())) ||  cboArea.SelectedIndex > -1)
                {
                    SetLine(cboEquipmentSegment);
                    GetOpenTime(Convert.ToString(cboArea.SelectedValue));

                    if (!(string.IsNullOrEmpty(selectedEquipmentSegment))){ 
                        string[] splitEqsg = selectedEquipmentSegment.Split(',');
                        foreach (string str in splitEqsg)
                        {
                            cboEquipmentSegment.Check(str);
                        }
                    }
                    else
                    {
                        selectedArea = string.Empty;
                    }
                }
                else
                {
                    selectedArea = string.Empty;
                }
            }));
        }

        private void cboEquipmentSegment_SelectionChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboEquipmentSegment.SelectedItems.Count > 0)
                {
                    //SetProcess(cboProcess);

                    //string[] splitProc = selectedProcess.Split(',');
                    //foreach (string str in splitProc)
                    //{
                    //    cboProcess.Check(str);
                    //}
                }
                else
                {
                    //selectedArea = string.Empty;
                }
            }));
        }

        #endregion Combo

        private void numRefresh_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // reSet();            
        }

        #endregion Event

        #region Mehod

        private void realData()
        {
            try
            {
                setTitle();

                //end_Rotation(); //화면전환 timer 종료

                

                searchData();

                if (dtSearchTable == null || dtSearchTable.Rows.Count == 0)
                {
                    return;
                }

                //bind();

                //RotationView_Start_Timer();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetUIControl(object obj, string sMsg = "", bool bAllNull = false)
        {
            string sRet = string.Empty;
            int idx = 0;
            string type = "";

            switch (obj.GetType().Name)
            {
                case "LGCDatePicker":
                    LGCDatePicker lgcDp = obj as LGCDatePicker;

                    if (lgcDp.Dispatcher.CheckAccess())
                    {
                        type = lgcDp.DatepickerType.ToString();

                        if (type.Equals("Month"))
                        {
                            sRet = lgcDp.SelectedDateTime.ToString("yyyyMM");
                        }
                        else
                        {
                            sRet = lgcDp.SelectedDateTime.ToString("yyyyMMdd");
                        }
                    }
                    else
                    {
                        lgcDp.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            type = lgcDp.DatepickerType.ToString();
                        }));

                        if (type.Equals("Month"))
                        {
                            lgcDp.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                            {
                                sRet = lgcDp.SelectedDateTime.ToString("yyyyMM");
                            }));
                        }
                        else
                        {
                            lgcDp.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                            {
                                sRet = lgcDp.SelectedDateTime.ToString("yyyyMMdd");
                            }));
                        }
                    }

                    break;
                case "C1ComboBox":
                    C1ComboBox cb = obj as C1ComboBox;

                    if (cb.Dispatcher.CheckAccess())
                    {
                        idx = cb.SelectedIndex;

                        if (idx < 0)
                        {
                            if (!sMsg.Equals(""))
                            {
                                Util.Alert(sMsg);
                            }
                            break;
                        }

                        sRet = cb.SelectedValue.ToString();

                    }
                    else
                    {
                        cb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            idx = cb.SelectedIndex;
                        }));

                        if (idx < 0)
                        {
                            if (!sMsg.Equals(""))
                            {
                                Util.Alert(sMsg);
                            }
                            break;
                        }

                        cb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            sRet = cb.SelectedValue.ToString();
                        }));
                    }

                    if (sRet.Equals("SELECT"))
                    {
                        sRet = "";
                        if (!sMsg.Equals(""))
                        {
                            Util.Alert(sMsg);
                        }
                        break;
                    }
                    else if (sRet.Equals(""))
                    {
                        if (bAllNull)
                        {
                            sRet = null;
                        }
                    }

                    break;
                case "MultiSelectionBox":
                    MultiSelectionBox msb = obj as MultiSelectionBox;

                    if (msb.Dispatcher.CheckAccess())
                    {
                        sRet = msb.SelectedItemsToString;
                    }
                    else
                    {
                        msb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            sRet = msb.SelectedItemsToString;
                        }));
                    }

                    break;
                case "TextBox":
                    System.Windows.Controls.TextBox tb = obj as System.Windows.Controls.TextBox;

                    if (tb.Dispatcher.CheckAccess())
                    {
                        sRet = tb.Text;
                    }
                    else
                    {
                        tb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            sRet = tb.Text;
                        }));
                    }

                    if (sRet.Equals("") && !sMsg.Equals(""))
                    {
                        Util.Alert(sMsg);
                        break;
                    }

                    break;
                case "C1NumericBox":
                    C1NumericBox nb = obj as C1NumericBox;

                    if (nb.Dispatcher.CheckAccess())
                    {
                        sRet = nb.Value.ToString();
                    }
                    else
                    {
                        nb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            sRet = nb.Value.ToString();
                        }));
                    }

                    break;
                case "CheckBox":

                    System.Windows.Controls.CheckBox chk = obj as System.Windows.Controls.CheckBox;

                    if (chk.Dispatcher.CheckAccess())
                    {
                        sRet = chk.IsChecked.ToString();
                    }
                    else
                    {
                        chk.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            sRet = chk.IsChecked.ToString();
                        }));
                    }

                    break;
            }

            return sRet;
        }

        public static void SetUiCondition(object oCondition, string sMsg, bool bAllNull = false)
        {
            switch (oCondition.GetType().Name)
            {
                case "LGCDatePicker":
                    LGCDatePicker lgcDp = oCondition as LGCDatePicker;

                    if (lgcDp.Dispatcher.CheckAccess())
                    {
                        lgcDp.SelectedDateTime = Convert.ToDateTime(sMsg);
                    }
                    else
                    {
                        lgcDp.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            lgcDp.SelectedDateTime = Convert.ToDateTime(sMsg);
                        }));
                    }

                    lgcDp.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        lgcDp.SelectedDateTime = Convert.ToDateTime(sMsg);
                    }));
                    break;
                case "C1ComboBox":
                    C1ComboBox cb = oCondition as C1ComboBox;

                    if (cb.Dispatcher.CheckAccess())
                    {
                        cb.SelectedValue = sMsg;
                    }
                    else
                    {
                        cb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            cb.SelectedValue = sMsg;
                        }));
                    }

                    break;
                case "TextBox":
                    System.Windows.Controls.TextBox tb = oCondition as System.Windows.Controls.TextBox;

                    if (tb.Dispatcher.CheckAccess())
                    {
                        tb.Text = sMsg;
                    }
                    else
                    {
                        tb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            tb.Text = sMsg;
                        }));
                    }

                    break;
                case "TextBlock":

                    TextBlock tbk = oCondition as TextBlock;

                    if (tbk.Dispatcher.CheckAccess())
                    {
                        tbk.Text = sMsg;
                    }
                    else
                    {
                        tbk.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            tbk.Text = sMsg;
                        }));
                    }

                    break;
                case "C1NumericBox":
                    C1NumericBox nb = oCondition as C1NumericBox;

                    if (nb.Dispatcher.CheckAccess())
                    {
                        nb.Value = Convert.ToInt32(sMsg);
                    }
                    else
                    {
                        nb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            nb.Value = Convert.ToInt32(sMsg);
                        }));
                    }

                    break;
            }
        }

        private void bind()
        {

            int row_idx = 0;

            dtSortTable = dtSearchTable.Select("", "EQSGNAME asc").CopyToDataTable<DataRow>();
            dtSortTable.AcceptChanges();

            if (dgInput2.Dispatcher.CheckAccess())
            {
                dgInput2.ItemsSource = null;
            }
            else
            {
                dgInput2.Dispatcher.BeginInvoke(new Action(() => dgInput2.ItemsSource = null));
            }

            if (dgStock2.Dispatcher.CheckAccess())
            {
                dgStock2.ItemsSource = null;
            }
            else
            {
                dgStock2.Dispatcher.BeginInvoke(new Action(() => dgStock2.ItemsSource = null));
            }

            SetGridBind(dtSortTable); //화면 VIEW Row수에 설정된 row 갯수를 가지고 lotation 화면수 결정해서 번갈아 가며 띄움.
            SetUiCondition(tbRefreshTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            dtSearchTable = null;
        }

        private void SetGridBind(DataTable dtBind)
        {
            viewRowCnt = Convert.ToInt32(GetUIControl(numViewRowCnt));
            bindTableRowCnt = dtBind.Rows.Count;
            rotationView = Convert.ToInt32(GetUIControl(numRefreshSub));

            if (viewRowCnt >= bindTableRowCnt || viewRowCnt == 0) //보고싶은 Row가 bindTable의 row갯수보다 크면 그냥 bind함.
            {
                realViewCnt = 1;

                div = 1;
            }
            else
            {
                if (bindTableRowCnt % viewRowCnt == 0)
                {
                    div = (bindTableRowCnt / viewRowCnt);
                }
                else
                {
                    double temp = Math.Floor((Convert.ToDouble(bindTableRowCnt) / Convert.ToDouble(viewRowCnt)));
                    div = Convert.ToInt32(temp) + 1;
                }
            }

            if (div != 0) // ex) 화면수 1개 - dtDiv[0] / 화면수 2개 - dtDiv[0],dtDiv[1] ..... : 두번째 화면은 rotation timer 돌면 보여진다.
            {
                dtDiv = null;

                dtDiv = new DataTable[div]; //table 5개

                int start_idx = 0;
                int end_idx = viewRowCnt;

                for (int i = 0; i < dtDiv.Length; i++) //5번 roof
                {
                    dtDiv[i] = dtBind.Clone();

                    for (int j = start_idx; j < end_idx; j++) //4
                    {
                        if (j < dtBind.Rows.Count)
                        {
                            dtDiv[i].ImportRow(dtBind.Rows[j]);
                        }
                    }

                    start_idx = end_idx;
                    end_idx = end_idx + viewRowCnt;
                }

                if (realViewCnt >= div)
                {
                    realViewCnt = 0;
                }

                setChart(realViewCnt);          // realViewCnt 는 다음 rotationViewPlay 시 보여질 페이지
                realViewCnt++;

            }

            string msg = realViewCnt + " / " + div;
            SetUiCondition(tbPage, msg);
            SetUiCondition(tbRefreshTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

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

        private void DataSearch_Start_Timer()
        {
            int refresh = Convert.ToInt32(GetUIControl(numRefresh)) * 60000; //기준 : 분
            //int refresh = (Convert.ToInt32(numRefresh.Value)) * 1000; //테스트 위해 초로 바꿈

            _timer_DataSearch = new System.Timers.Timer(refresh);
            _timer_DataSearch.AutoReset = true;
            _timer_DataSearch.Elapsed += (s, arg) =>
            {
                try
                {
                    realData();
                }
                catch (Exception ex)
                {
                    Util.Alert(ex.Message);
                }
            };
            _timer_DataSearch.Start();
        }

        private void RotationView_Start_Timer()
        {
            int refresh = rotationView * 1000; //기준 : 초
            try
            {
                _timer_View_Rotation = new System.Timers.Timer(refresh);
                _timer_View_Rotation.AutoReset = true;
                _timer_View_Rotation.Elapsed += (s, arg) =>
                {
                    if (_timer_View_Rotation != null)
                    {
                        RotationViewPlay();
                    }
                };
                _timer_View_Rotation.Start();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }

        private void RotationViewPlay()
        {
            try
            {
                if (first_access)
                {
                    reSet();

                    first_access = false;

                    return;
                }

                if (div == 1)
                {
                    return;
                }

                int nowviewPoint = 0;

                if (realViewCnt < div)
                {
                    nowviewPoint = realViewCnt;
                }
                else if (realViewCnt == div)
                {
                    nowviewPoint = 0;
                    realViewCnt = 0;
                }
                else
                {
                    realViewCnt = 0;
                }

                setChart(nowviewPoint);

                realViewCnt++;

                string msg = realViewCnt + " / " + div;

                SetUiCondition(tbPage, msg);
                SetUiCondition(tbRefreshTime, DateTime.Now.ToString());
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }

        }

        private void SetCellColor(C1DataGrid dataGrid, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
            e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.White);
            e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.White);
            e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.White);
            e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.White);
            dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.White);
            dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.White);

            if (e.Cell.Row.DataItem != null)
            {
                if (dataGrid.Name.Equals("dgStock") || dataGrid.Name.Equals("dgStock2"))
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                if (e.Cell.Column.Name.Equals("8") || e.Cell.Column.Name.Equals("9") || e.Cell.Column.Name.Equals("10") ||
                                    e.Cell.Column.Name.Equals("11") || e.Cell.Column.Name.Equals("12") || e.Cell.Column.Name.Equals("13") ||
                                    e.Cell.Column.Name.Equals("14") || e.Cell.Column.Name.Equals("15") || e.Cell.Column.Name.Equals("16") ||
                                    e.Cell.Column.Name.Equals("17") || e.Cell.Column.Name.Equals("18") || e.Cell.Column.Name.Equals("19") ||
                                    e.Cell.Column.Name.Equals("20") || e.Cell.Column.Name.Equals("21") || e.Cell.Column.Name.Equals("22") ||
                                    e.Cell.Column.Name.Equals("23") || e.Cell.Column.Name.Equals("24") || e.Cell.Column.Name.Equals("1") ||
                                    e.Cell.Column.Name.Equals("2") || e.Cell.Column.Name.Equals("3") || e.Cell.Column.Name.Equals("4") ||
                                    e.Cell.Column.Name.Equals("5") || e.Cell.Column.Name.Equals("6") || e.Cell.Column.Name.Equals("7")
                                    )
                                {
                                    #region 08
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")) != "")
                                    {
                                        double n08 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")));

                                        #region 08 음
                                        if (n08 < 0 && string.Equals(e.Cell.Column.Name, "8"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")) != "")
                                            {
                                                double nChk9 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")));
                                                double nChk10 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")));

                                                #region 09 음 10 음
                                                if (nChk9 < 0 && nChk10 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 음 10 음

                                                #region 09 양 10 움
                                                else if (nChk9 > 0 && nChk10 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 양 10 음

                                                #region 09 양 10 양
                                                else if (nChk9 > 0 && nChk10 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 양 10 양

                                                #region 09 음 10 양
                                                else if (nChk9 < 0 && nChk10 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 음 10 양

                                                #region 09 0 10 0
                                                else if (nChk9 < 0 && nChk10 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 0 10 0
                                            }
                                            else
                                            {
                                                double nChk9 = 0;
                                                double nChk10 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")) != "")
                                                {
                                                    nChk9 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")) != "")
                                                {
                                                    nChk10 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")));
                                                }

                                                #region 07 음 08 음
                                                if (nChk9 < 0 && nChk10 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 음 10 음

                                                #region 09 양 10 움
                                                else if (nChk9 > 0 && nChk10 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 양 10 음

                                                #region 09 양 10 양
                                                else if (nChk9 > 0 && nChk10 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 양 10 양

                                                #region 09 음 10 양
                                                else if (nChk9 < 0 && nChk10 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 음 10 양

                                                #region 09 0 10 0
                                                else if (nChk9 < 0 && nChk10 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 0 10 0
                                            }
                                        }
                                        #endregion 08 음

                                        #region 08 양
                                        else if (n08 > 0 && string.Equals(e.Cell.Column.Name, "8"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")) != "")
                                            {
                                                double nChk9 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")));
                                                double nChk10 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")));

                                                #region 07 음 08 음
                                                if (nChk9 < 0 && nChk10 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 음 10 음

                                                #region 09 양 10 움
                                                else if (nChk9 > 0 && nChk10 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 양 10 음

                                                #region 09 양 10 양
                                                else if (nChk9 > 0 && nChk10 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 양 10 양

                                                #region 09 음 10 양
                                                else if (nChk9 < 0 && nChk10 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 음 10 양

                                                #region 09 0 10 0
                                                else if (nChk9 < 0 && nChk10 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 0 10 0
                                            }
                                            else
                                            {
                                                double nChk9 = 0;
                                                double nChk10 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")) != "")
                                                {
                                                    nChk9 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")) != "")
                                                {
                                                    nChk10 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")));
                                                }

                                                #region 07 음 08 음
                                                if (nChk9 < 0 && nChk10 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 음 10 음

                                                #region 09 양 10 움
                                                else if (nChk9 > 0 && nChk10 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 양 10 음

                                                #region 09 양 10 양
                                                else if (nChk9 > 0 && nChk10 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 양 10 양

                                                #region 09 음 10 양
                                                else if (nChk9 < 0 && nChk10 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 음 10 양

                                                #region 09 0 10 0
                                                else if (nChk9 < 0 && nChk10 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 0 10 0
                                            }
                                        }
                                        #endregion 08 양
                                    }
                                    #endregion 08

                                    #region 09
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")) != "")
                                    {
                                        double n09 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")));

                                        #region 09 음
                                        if (n09 < 0 && string.Equals(e.Cell.Column.Name, "9"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")) != "")
                                            {
                                                double nChk10 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")));
                                                double nChk11 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")));

                                                #region 10 음 11 음
                                                if (nChk10 < 0 && nChk11 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 음 11 음

                                                #region 10 양 11 움
                                                else if (nChk10 > 0 && nChk11 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 양 11 음

                                                #region 10 양 11 양
                                                else if (nChk10 > 0 && nChk11 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 양 11 양

                                                #region 10 음 11 양
                                                else if (nChk10 < 0 && nChk11 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 음 11 양

                                                #region 10 0 11 0
                                                else if (nChk10 < 0 && nChk11 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 0 11 0
                                            }
                                            else
                                            {
                                                double nChk10 = 0;
                                                double nChk11 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")) != "")
                                                {
                                                    nChk10 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")) != "")
                                                {
                                                    nChk11 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")));
                                                }

                                                #region 10 음 11 음
                                                if (nChk10 < 0 && nChk11 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 음 11 음

                                                #region 10 양 11 움
                                                else if (nChk10 > 0 && nChk11 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 양 11 음

                                                #region 10 양 11 양
                                                else if (nChk10 > 0 && nChk11 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 양 11 양

                                                #region 10 음 11 양
                                                else if (nChk10 < 0 && nChk11 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 음 11 양

                                                #region 10 0 11 0
                                                else if (nChk10 < 0 && nChk11 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 0 11 0
                                            }
                                        }
                                        #endregion 09 음

                                        #region 09 양
                                        else if (n09 > 0 && string.Equals(e.Cell.Column.Name, "9"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")) != "")
                                            {
                                                double nChk10 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")));
                                                double nChk11 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")));

                                                #region 10 음 11 음
                                                if (nChk10 < 0 && nChk11 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 음 11 음

                                                #region 10 양 11 움
                                                else if (nChk10 > 0 && nChk11 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 양 11 음

                                                #region 10 양 11 양
                                                else if (nChk10 > 0 && nChk11 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 양 11 양

                                                #region 10 음 11 양
                                                else if (nChk10 < 0 && nChk11 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 음 11 양

                                                #region 10 0 11 0
                                                else if (nChk10 < 0 && nChk11 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 0 11 0

                                            }
                                            else
                                            {
                                                double nChk10 = 0;
                                                double nChk11 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")) != "")
                                                {
                                                    nChk10 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")) != "")
                                                {
                                                    nChk11 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")));
                                                }

                                                #region 10 음 11 음
                                                if (nChk10 < 0 && nChk11 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 음 11 음

                                                #region 10 양 11 움
                                                else if (nChk10 > 0 && nChk11 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 양 11 음

                                                #region 10 양 11 양
                                                else if (nChk10 > 0 && nChk11 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 양 11 양

                                                #region 10 음 11 양
                                                else if (nChk10 < 0 && nChk11 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 음 11 양

                                                #region 10 0 11 0
                                                else if (nChk10 < 0 && nChk11 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 0 11 0
                                            }
                                        }
                                        #endregion 09 양                                        
                                    }
                                    #endregion 09

                                    #region 10
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")) != "")
                                    {
                                        double n10 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")));

                                        #region 10 음
                                        if (n10 < 0 && string.Equals(e.Cell.Column.Name, "10"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")) != "")
                                            {
                                                double nChk11 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")));
                                                double nChk12 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")));

                                                #region 11 음 12 음
                                                if (nChk11 < 0 && nChk12 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 음 12 음

                                                #region 11 양 12 움
                                                else if (nChk11 > 0 && nChk12 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 양 12 음

                                                #region 11 양 12 양
                                                else if (nChk11 > 0 && nChk12 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 양 12 양

                                                #region 11 음 12 양
                                                else if (nChk11 < 0 && nChk12 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 음 12 양

                                                #region 11 0 12 0
                                                else if (nChk11 < 0 && nChk12 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 0 12 0
                                            }
                                            else
                                            {
                                                double nChk11 = 0;
                                                double nChk12 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")) != "")
                                                {
                                                    nChk11 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")) != "")
                                                {
                                                    nChk12 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")));
                                                }

                                                #region 11 음 12 음
                                                if (nChk11 < 0 && nChk12 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 음 12 음

                                                #region 11 양 12 움
                                                else if (nChk11 > 0 && nChk12 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 양 12 음

                                                #region 11 양 12 양
                                                else if (nChk11 > 0 && nChk12 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 양 12 양

                                                #region 11 음 12 양
                                                else if (nChk11 < 0 && nChk12 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 음 12 양

                                                #region 11 0 12 0
                                                else if (nChk11 < 0 && nChk12 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 0 12 0
                                            }
                                        }
                                        #endregion 10 음

                                        #region 10 양
                                        else if (n10 > 0 && string.Equals(e.Cell.Column.Name, "10"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")) != "")
                                            {
                                                double nChk11 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")));
                                                double nChk12 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")));

                                                #region 11 음 12 음
                                                if (nChk11 < 0 && nChk12 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 음 12 음

                                                #region 11 양 12 움
                                                else if (nChk11 > 0 && nChk12 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 양 12 음

                                                #region 11 양 12 양
                                                else if (nChk11 > 0 && nChk12 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 양 12 양

                                                #region 11 음 12 양
                                                else if (nChk11 < 0 && nChk12 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 음 12 양

                                                #region 11 0 12 0
                                                else if (nChk11 < 0 && nChk12 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 0 12 0
                                            }
                                            else
                                            {
                                                double nChk11 = 0;
                                                double nChk12 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")) != "")
                                                {
                                                    nChk11 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")) != "")
                                                {
                                                    nChk12 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")));
                                                }

                                                #region 11 음 12 음
                                                if (nChk11 < 0 && nChk12 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 음 12 음

                                                #region 11 양 12 움
                                                else if (nChk11 > 0 && nChk12 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 양 12 음

                                                #region 11 양 12 양
                                                else if (nChk11 > 0 && nChk12 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 양 12 양

                                                #region 11 음 12 양
                                                else if (nChk11 < 0 && nChk12 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 음 12 양

                                                #region 11 0 12 0
                                                else if (nChk11 < 0 && nChk12 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 0 12 0
                                            }
                                        }
                                        #endregion 10 양                                              
                                    }
                                    #endregion 10

                                    #region 11
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")) != "")
                                    {
                                        double n11 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")));

                                        #region 11 음
                                        if (n11 < 0 && string.Equals(e.Cell.Column.Name, "11"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")) != "")
                                            {
                                                double nChk12 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")));
                                                double nChk13 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")));

                                                #region 12 음 13 음
                                                if (nChk12 < 0 && nChk13 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 음 13 음

                                                #region 12 양 13 움
                                                else if (nChk12 > 0 && nChk13 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 양 13 음

                                                #region 12 양 13 양
                                                else if (nChk12 > 0 && nChk13 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 양 13 양

                                                #region 12 음 13 양
                                                else if (nChk12 < 0 && nChk13 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 음 13 양

                                                #region 12 0 13 0
                                                else if (nChk12 < 0 && nChk13 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 0 13 0
                                            }
                                            else
                                            {
                                                double nChk12 = 0;
                                                double nChk13 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")) != "")
                                                {
                                                    nChk12 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")) != "")
                                                {
                                                    nChk13 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")));
                                                }

                                                #region 12 음 13 음
                                                if (nChk12 < 0 && nChk13 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 음 13 음

                                                #region 12 양 13 움
                                                else if (nChk12 > 0 && nChk13 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 양 13 음

                                                #region 12 양 13 양
                                                else if (nChk12 > 0 && nChk13 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 양 13 양

                                                #region 12 음 13 양
                                                else if (nChk12 < 0 && nChk13 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 음 13 양

                                                #region 12 0 13 0
                                                else if (nChk12 < 0 && nChk13 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 0 13 0
                                            }
                                        }
                                        #endregion 11 음

                                        #region 11 양
                                        else if (n11 > 0 && string.Equals(e.Cell.Column.Name, "11"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")) != "")
                                            {
                                                double nChk12 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")));
                                                double nChk13 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")));

                                                #region 12 음 13 음
                                                if (nChk12 < 0 && nChk13 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 음 13 음

                                                #region 12 양 13 움
                                                else if (nChk12 > 0 && nChk13 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 양 13 음

                                                #region 12 양 13 양
                                                else if (nChk12 > 0 && nChk13 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 양 13 양

                                                #region 12 음 13 양
                                                else if (nChk12 < 0 && nChk13 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 음 13 양

                                                #region 12 0 13 0
                                                else if (nChk12 < 0 && nChk13 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 0 13 0
                                            }
                                            else
                                            {
                                                double nChk12 = 0;
                                                double nChk13 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")) != "")
                                                {
                                                    nChk12 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")) != "")
                                                {
                                                    nChk13 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")));
                                                }

                                                #region 12 음 13 음
                                                if (nChk12 < 0 && nChk13 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 음 13 음

                                                #region 12 양 13 움
                                                else if (nChk12 > 0 && nChk13 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 양 13 음

                                                #region 12 양 13 양
                                                else if (nChk12 > 0 && nChk13 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 양 13 양

                                                #region 12 음 13 양
                                                else if (nChk12 < 0 && nChk13 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 음 13 양

                                                #region 12 0 13 0
                                                else if (nChk12 < 0 && nChk13 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 0 13 0
                                            }
                                        }
                                        #endregion 11 양
                                    }
                                    #endregion 11

                                    #region 12
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")) != "")
                                    {
                                        double n12 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")));

                                        #region 12 음
                                        if (n12 < 0 && string.Equals(e.Cell.Column.Name, "12"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")) != "")
                                            {
                                                double nChk13 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")));
                                                double nChk14 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")));

                                                #region 13 음 14 음
                                                if (nChk13 < 0 && nChk14 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 음 14 음

                                                #region 13 양 14 움
                                                else if (nChk13 > 0 && nChk14 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 양 14 음

                                                #region 13 양 14 양
                                                else if (nChk13 > 0 && nChk14 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 양 14 양

                                                #region 13 음 14 양
                                                else if (nChk13 < 0 && nChk14 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 음 14 양

                                                #region 13 0 14 0
                                                else if (nChk13 < 0 && nChk14 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 0 14 0
                                            }
                                            else
                                            {
                                                double nChk13 = 0;
                                                double nChk14 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")) != "")
                                                {
                                                    nChk13 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")) != "")
                                                {
                                                    nChk14 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")));
                                                }

                                                #region 13 음 14 음
                                                if (nChk13 < 0 && nChk14 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 음 14 음

                                                #region 13 양 14 움
                                                else if (nChk13 > 0 && nChk14 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 양 14 음

                                                #region 13 양 14 양
                                                else if (nChk13 > 0 && nChk14 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 양 14 양

                                                #region 13 음 14 양
                                                else if (nChk13 < 0 && nChk14 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 음 14 양

                                                #region 13 0 14 0
                                                else if (nChk13 < 0 && nChk14 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 0 14 0
                                            }
                                        }
                                        #endregion 12 음

                                        #region 12 양
                                        else if (n12 > 0 && string.Equals(e.Cell.Column.Name, "12"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")) != "")
                                            {
                                                double nChk13 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")));
                                                double nChk14 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")));

                                                #region 13 음 14 음
                                                if (nChk13 < 0 && nChk14 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 음 14 음

                                                #region 13 양 14 움
                                                else if (nChk13 > 0 && nChk14 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 양 14 음

                                                #region 13 양 14 양
                                                else if (nChk13 > 0 && nChk14 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 양 14 양

                                                #region 13 음 14 양
                                                else if (nChk13 < 0 && nChk14 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 음 14 양

                                                #region 13 0 14 0
                                                else if (nChk13 < 0 && nChk14 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 0 14 0
                                            }
                                            else
                                            {
                                                double nChk13 = 0;
                                                double nChk14 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")) != "")
                                                {
                                                    nChk13 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")) != "")
                                                {
                                                    nChk14 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")));
                                                }

                                                #region 13 음 14 음
                                                if (nChk13 < 0 && nChk14 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 음 14 음

                                                #region 13 양 14 움
                                                else if (nChk13 > 0 && nChk14 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 양 14 음

                                                #region 13 양 14 양
                                                else if (nChk13 > 0 && nChk14 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 양 14 양

                                                #region 13 음 14 양
                                                else if (nChk13 < 0 && nChk14 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 음 14 양

                                                #region 13 0 14 0
                                                else if (nChk13 < 0 && nChk14 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 0 14 0
                                            }
                                        }
                                        #endregion 12 양
                                    }
                                    #endregion 12

                                    #region 13
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")) != "")
                                    {
                                        double n13 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")));

                                        #region 13 음
                                        if (n13 < 0 && string.Equals(e.Cell.Column.Name, "13"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")) != "")
                                            {
                                                double nChk14 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")));
                                                double nChk15 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")));

                                                #region 14 음 15 음
                                                if (nChk14 < 0 && nChk15 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 음 15 음

                                                #region 14 양 15 움
                                                else if (nChk14 > 0 && nChk15 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 양 15 음

                                                #region 14 양 15 양
                                                else if (nChk14 > 0 && nChk15 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 양 15 양

                                                #region 14 음 15 양
                                                else if (nChk14 < 0 && nChk15 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 음 15 양

                                                #region 14 0 15 0
                                                else if (nChk14 < 0 && nChk15 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 0 15 0
                                            }
                                            else
                                            {
                                                double nChk14 = 0;
                                                double nChk15 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")) != "")
                                                {
                                                    nChk14 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")) != "")
                                                {
                                                    nChk15 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")));
                                                }

                                                #region 14 음 15 음
                                                if (nChk14 < 0 && nChk15 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 음 15 음

                                                #region 14 양 15 움
                                                else if (nChk14 > 0 && nChk15 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 양 15 음

                                                #region 14 양 15 양
                                                else if (nChk14 > 0 && nChk15 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 양 15 양

                                                #region 14 음 15 양
                                                else if (nChk14 < 0 && nChk15 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 음 15 양

                                                #region 14 0 15 0
                                                else if (nChk14 < 0 && nChk15 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 0 15 0
                                            }
                                        }
                                        #endregion 13 음

                                        #region 13 양
                                        else if (n13 > 0 && string.Equals(e.Cell.Column.Name, "13"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")) != "")
                                            {
                                                double nChk14 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")));
                                                double nChk15 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")));

                                                #region 14 음 15 음
                                                if (nChk14 < 0 && nChk15 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 음 15 음

                                                #region 14 양 15 움
                                                else if (nChk14 > 0 && nChk15 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 양 15 음

                                                #region 14 양 15 양
                                                else if (nChk14 > 0 && nChk15 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 양 15 양

                                                #region 14 음 15 양
                                                else if (nChk14 < 0 && nChk15 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 음 15 양

                                                #region 14 0 15 0
                                                else if (nChk14 < 0 && nChk15 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 0 15 0
                                            }
                                            else
                                            {
                                                double nChk14 = 0;
                                                double nChk15 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")) != "")
                                                {
                                                    nChk14 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")) != "")
                                                {
                                                    nChk15 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")));
                                                }

                                                #region 14 음 15 음
                                                if (nChk14 < 0 && nChk15 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 음 15 음

                                                #region 14 양 15 움
                                                else if (nChk14 > 0 && nChk15 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 양 15 음

                                                #region 14 양 15 양
                                                else if (nChk14 > 0 && nChk15 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 양 15 양

                                                #region 14 음 15 양
                                                else if (nChk14 < 0 && nChk15 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 음 15 양

                                                #region 14 0 15 0
                                                else if (nChk14 < 0 && nChk15 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 0 15 0
                                            }
                                        }
                                        #endregion 13 양
                                    }
                                    #endregion 13

                                    #region 14
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")) != "")
                                    {
                                        double n14 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")));

                                        #region 14 음
                                        if (n14 < 0 && string.Equals(e.Cell.Column.Name, "14"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")) != "")
                                            {
                                                double nChk15 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")));
                                                double nChk16 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")));

                                                #region 15 음 16 음
                                                if (nChk15 < 0 && nChk16 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 음 16 음

                                                #region 15 양 16 움
                                                else if (nChk15 > 0 && nChk16 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 양 16 음

                                                #region 15 양 16 양
                                                else if (nChk15 > 0 && nChk16 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 양 16 양

                                                #region 15 음 16 양
                                                else if (nChk15 < 0 && nChk16 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 음 16 양

                                                #region 15 0 16 0
                                                else if (nChk15 < 0 && nChk16 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 0 16 0
                                            }
                                            else
                                            {
                                                double nChk15 = 0;
                                                double nChk16 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")) != "")
                                                {
                                                    nChk15 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")) != "")
                                                {
                                                    nChk16 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")));
                                                }

                                                #region 15 음 16 음
                                                if (nChk15 < 0 && nChk16 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 음 16 음

                                                #region 15 양 16 움
                                                else if (nChk15 > 0 && nChk16 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 양 16 음

                                                #region 15 양 16 양
                                                else if (nChk15 > 0 && nChk16 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 양 16 양

                                                #region 15 음 16 양
                                                else if (nChk15 < 0 && nChk16 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 음 16 양

                                                #region 15 0 16 0
                                                else if (nChk15 < 0 && nChk16 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 0 16 0
                                            }
                                        }
                                        #endregion 14 음

                                        #region 14 양
                                        else if (n14 > 0 && string.Equals(e.Cell.Column.Name, "14"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")) != "")
                                            {
                                                double nChk15 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")));
                                                double nChk16 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")));

                                                #region 15 음 16 음
                                                if (nChk15 < 0 && nChk16 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 음 16 음

                                                #region 15 양 16 움
                                                else if (nChk15 > 0 && nChk16 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 양 16 음

                                                #region 15 양 16 양
                                                else if (nChk15 > 0 && nChk16 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 양 16 양

                                                #region 15 음 16 양
                                                else if (nChk15 < 0 && nChk16 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 음 16 양

                                                #region 15 0 16 0
                                                else if (nChk15 < 0 && nChk16 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 0 16 0
                                            }
                                            else
                                            {
                                                double nChk15 = 0;
                                                double nChk16 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")) != "")
                                                {
                                                    nChk15 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")) != "")
                                                {
                                                    nChk16 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")));
                                                }

                                                #region 15 음 16 음
                                                if (nChk15 < 0 && nChk16 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 음 16 음

                                                #region 15 양 16 움
                                                else if (nChk15 > 0 && nChk16 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 양 16 음

                                                #region 15 양 16 양
                                                else if (nChk15 > 0 && nChk16 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 양 16 양

                                                #region 15 음 16 양
                                                else if (nChk15 < 0 && nChk16 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 음 16 양

                                                #region 15 0 16 0
                                                else if (nChk15 < 0 && nChk16 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 0 16 0
                                            }
                                        }
                                        #endregion 14 양
                                    }
                                    #endregion 14

                                    #region 15
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")) != "")
                                    {
                                        double n15 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")));

                                        #region 15 음
                                        if (n15 < 0 && string.Equals(e.Cell.Column.Name, "15"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")) != "")
                                            {
                                                double nChk16 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")));
                                                double nChk17 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")));

                                                #region 16 음 17 음
                                                if (nChk17 < 0 && nChk17 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 음 17 음

                                                #region 16 양 17 움
                                                else if (nChk16 > 0 && nChk17 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 양 17 음

                                                #region 16 양 17 양
                                                else if (nChk16 > 0 && nChk17 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 양 17 양

                                                #region 16 음 17 양
                                                else if (nChk16 < 0 && nChk17 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 음 17 양

                                                #region 16 0 17 0
                                                else if (nChk16 < 0 && nChk17 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 0 17 0
                                            }
                                            else
                                            {
                                                double nChk16 = 0;
                                                double nChk17 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")) != "")
                                                {
                                                    nChk16 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")) != "")
                                                {
                                                    nChk17 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")));
                                                }

                                                #region 16 음 17 음
                                                if (nChk16 < 0 && nChk17 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 음 17 음

                                                #region 16 양 17 움
                                                else if (nChk16 > 0 && nChk17 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 양 17 음

                                                #region 16 양 17 양
                                                else if (nChk16 > 0 && nChk17 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 양 17 양

                                                #region 16 음 17 양
                                                else if (nChk16 < 0 && nChk17 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 음 17 양

                                                #region 16 0 17 0
                                                else if (nChk16 < 0 && nChk17 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 0 17 0
                                            }
                                        }
                                        #endregion 15 음

                                        #region 15 양
                                        else if (n15 > 0 && string.Equals(e.Cell.Column.Name, "15"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")) != "")
                                            {
                                                double nChk16 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")));
                                                double nChk17 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")));

                                                #region 16 음 17 음
                                                if (nChk16 < 0 && nChk17 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 음 17 음

                                                #region 16 양 17 움
                                                else if (nChk16 > 0 && nChk17 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 양 17 음

                                                #region 16 양 17 양
                                                else if (nChk16 > 0 && nChk17 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 양 17 양

                                                #region 16 음 17 양
                                                else if (nChk16 < 0 && nChk17 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 음 17 양

                                                #region 16 0 17 0
                                                else if (nChk16 < 0 && nChk17 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 0 17 0
                                            }
                                            else
                                            {
                                                double nChk16 = 0;
                                                double nChk17 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")) != "")
                                                {
                                                    nChk16 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")) != "")
                                                {
                                                    nChk17 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")));
                                                }

                                                #region 16 음 17 음
                                                if (nChk16 < 0 && nChk17 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 음 17 음

                                                #region 16 양 17 움
                                                else if (nChk16 > 0 && nChk17 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 양 17 음

                                                #region 16 양 17 양
                                                else if (nChk16 > 0 && nChk17 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 양 17 양

                                                #region 16 음 17 양
                                                else if (nChk16 < 0 && nChk17 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 음 17 양

                                                #region 16 0 17 0
                                                else if (nChk16 < 0 && nChk17 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 0 17 0
                                            }
                                        }
                                        #endregion 15 양
                                    }
                                    #endregion 15

                                    #region 16
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")) != "")
                                    {
                                        double n16 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")));

                                        #region 16 음
                                        if (n16 < 0 && string.Equals(e.Cell.Column.Name, "16"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")) != "")
                                            {
                                                double nChk17 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")));
                                                double nChk18 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")));

                                                #region 17 음 18 음
                                                if (nChk17 < 0 && nChk18 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 음 18 음

                                                #region 17 양 18 움
                                                else if (nChk17 > 0 && nChk18 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 양 18 음

                                                #region 17 양 18 양
                                                else if (nChk17 > 0 && nChk18 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 양 18 양

                                                #region 17 음 18 양
                                                else if (nChk17 < 0 && nChk18 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 음 18 양

                                                #region 17 0 18 0
                                                else if (nChk17 < 0 && nChk18 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 0 18 0
                                            }
                                            else
                                            {
                                                double nChk17 = 0;
                                                double nChk18 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")) != "")
                                                {
                                                    nChk17 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")) != "")
                                                {
                                                    nChk18 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")));
                                                }

                                                #region 17 음 18 음
                                                if (nChk17 < 0 && nChk18 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 음 18 음

                                                #region 17 양 18 움
                                                else if (nChk17 > 0 && nChk18 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 양 18 음

                                                #region 17 양 18 양
                                                else if (nChk17 > 0 && nChk18 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 양 18 양

                                                #region 17 음 18 양
                                                else if (nChk17 < 0 && nChk18 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 음 18 양

                                                #region 17 0 18 0
                                                else if (nChk17 < 0 && nChk18 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 0 18 0
                                            }
                                        }
                                        #endregion 16 음

                                        #region 16 양
                                        else if (n16 > 0 && string.Equals(e.Cell.Column.Name, "16"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")) != "")
                                            {
                                                double nChk17 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")));
                                                double nChk18 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")));

                                                #region 17 음 18 음
                                                if (nChk17 < 0 && nChk18 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 음 18 음

                                                #region 17 양 18 움
                                                else if (nChk17 > 0 && nChk18 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 양 18 음

                                                #region 17 양 18 양
                                                else if (nChk17 > 0 && nChk18 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 양 18 양

                                                #region 17 음 18 양
                                                else if (nChk17 < 0 && nChk18 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 음 18 양

                                                #region 17 0 18 0
                                                else if (nChk17 < 0 && nChk18 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 0 18 0
                                            }
                                            else
                                            {
                                                double nChk17 = 0;
                                                double nChk18 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")) != "")
                                                {
                                                    nChk17 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")) != "")
                                                {
                                                    nChk18 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")));
                                                }

                                                #region 17 음 18 음
                                                if (nChk17 < 0 && nChk18 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 음 18 음

                                                #region 17 양 18 움
                                                else if (nChk17 > 0 && nChk18 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 양 18 음

                                                #region 17 양 18 양
                                                else if (nChk17 > 0 && nChk18 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 양 18 양

                                                #region 17 음 18 양
                                                else if (nChk17 < 0 && nChk18 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 음 18 양

                                                #region 17 0 18 0
                                                else if (nChk17 < 0 && nChk18 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 0 18 0
                                            }
                                        }
                                        #endregion 16 양
                                    }
                                    #endregion 16

                                    #region 17
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")) != "")
                                    {
                                        double n17 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")));

                                        #region 17 음
                                        if (n17 < 0 && string.Equals(e.Cell.Column.Name, "17"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")) != "")
                                            {
                                                double nChk18 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")));
                                                double nChk19 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")));

                                                #region 18 음 19 음
                                                if (nChk18 < 0 && nChk19 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 음 19 음

                                                #region 18 양 19 움
                                                else if (nChk18 > 0 && nChk19 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 양 19 음

                                                #region 18 양 19 양
                                                else if (nChk18 > 0 && nChk19 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 양 19 양

                                                #region 18 음 19 양
                                                else if (nChk18 < 0 && nChk19 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 음 19 양

                                                #region 18 0 19 0
                                                else if (nChk18 < 0 && nChk19 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 0 19 0
                                            }
                                            else
                                            {
                                                double nChk18 = 0;
                                                double nChk19 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")) != "")
                                                {
                                                    nChk18 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")) != "")
                                                {
                                                    nChk19 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")));
                                                }

                                                #region 18 음 19 음
                                                if (nChk18 < 0 && nChk19 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 음 19 음

                                                #region 18 양 19 움
                                                else if (nChk18 > 0 && nChk19 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 양 19 음

                                                #region 18 양 19 양
                                                else if (nChk18 > 0 && nChk19 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 양 19 양

                                                #region 18 음 19 양
                                                else if (nChk18 < 0 && nChk19 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 음 19 양

                                                #region 18 0 19 0
                                                else if (nChk18 < 0 && nChk19 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 0 19 0
                                            }
                                        }
                                        #endregion 17 음

                                        #region 17 양
                                        else if (n17 > 0 && string.Equals(e.Cell.Column.Name, "17"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")) != "")
                                            {
                                                double nChk18 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")));
                                                double nChk19 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")));

                                                #region 18 음 19 음
                                                if (nChk18 < 0 && nChk19 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 음 19 음

                                                #region 18 양 19 움
                                                else if (nChk18 > 0 && nChk19 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 양 19 음

                                                #region 18 양 19 양
                                                else if (nChk18 > 0 && nChk19 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 양 19 양

                                                #region 18 음 19 양
                                                else if (nChk18 < 0 && nChk19 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 음 19 양

                                                #region 18 0 19 0
                                                else if (nChk18 < 0 && nChk19 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 0 19 0
                                            }
                                            else
                                            {
                                                double nChk18 = 0;
                                                double nChk19 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")) != "")
                                                {
                                                    nChk18 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")) != "")
                                                {
                                                    nChk19 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")));
                                                }

                                                #region 18 음 19 음
                                                if (nChk18 < 0 && nChk19 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 음 19 음

                                                #region 18 양 19 움
                                                else if (nChk18 > 0 && nChk19 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 양 19 음

                                                #region 18 양 19 양
                                                else if (nChk18 > 0 && nChk19 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 양 19 양

                                                #region 18 음 19 양
                                                else if (nChk18 < 0 && nChk19 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 음 19 양

                                                #region 18 0 19 0
                                                else if (nChk18 < 0 && nChk19 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 0 19 0
                                            }
                                        }
                                        #endregion 17 양
                                    }
                                    #endregion 17

                                    #region 18
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")) != "")
                                    {
                                        double n18 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")));

                                        #region 18 음
                                        if (n18 < 0 && string.Equals(e.Cell.Column.Name, "18"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")) != "")
                                            {
                                                double nChk19 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")));
                                                double nChk20 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")));

                                                #region 19 음 20 음
                                                if (nChk19 < 0 && nChk20 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 음 20 음

                                                #region 19 양 20 움
                                                else if (nChk19 > 0 && nChk20 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 양 20 음

                                                #region 19 양 20 양
                                                else if (nChk19 > 0 && nChk20 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 양 20 양

                                                #region 19 음 20 양
                                                else if (nChk19 < 0 && nChk20 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 음 20 양

                                                #region 19 0 20 0
                                                else if (nChk19 < 0 && nChk20 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 0 20 0
                                            }
                                            else
                                            {
                                                double nChk19 = 0;
                                                double nChk20 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")) != "")
                                                {
                                                    nChk19 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")) != "")
                                                {
                                                    nChk20 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")));
                                                }

                                                #region 19 음 20 음
                                                if (nChk19 < 0 && nChk20 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 음 20 음

                                                #region 19 양 20 움
                                                else if (nChk19 > 0 && nChk20 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 양 20 음

                                                #region 19 양 20 양
                                                else if (nChk19 > 0 && nChk20 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 양 20 양

                                                #region 19 음 20 양
                                                else if (nChk19 < 0 && nChk20 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 음 20 양

                                                #region 19 0 20 0
                                                else if (nChk19 < 0 && nChk20 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 0 20 0
                                            }
                                        }
                                        #endregion 18 음

                                        #region 18 양
                                        else if (n18 > 0 && string.Equals(e.Cell.Column.Name, "18"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")) != "")
                                            {
                                                double nChk19 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")));
                                                double nChk20 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")));

                                                #region 19 음 20 음
                                                if (nChk19 < 0 && nChk20 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 음 20 음

                                                #region 19 양 20 움
                                                else if (nChk19 > 0 && nChk20 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 양 20 음

                                                #region 19 양 20 양
                                                else if (nChk19 > 0 && nChk20 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 양 20 양

                                                #region 19 음 20 양
                                                else if (nChk19 < 0 && nChk20 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 음 20 양

                                                #region 19 0 20 0
                                                else if (nChk19 < 0 && nChk20 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 0 20 0
                                            }
                                            else
                                            {
                                                double nChk19 = 0;
                                                double nChk20 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")) != "")
                                                {
                                                    nChk19 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")) != "")
                                                {
                                                    nChk20 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")));
                                                }

                                                #region 19 음 20 음
                                                if (nChk19 < 0 && nChk20 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 음 20 음

                                                #region 19 양 20 움
                                                else if (nChk19 > 0 && nChk20 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 양 20 음

                                                #region 19 양 20 양
                                                else if (nChk19 > 0 && nChk20 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 양 20 양

                                                #region 19 음 20 양
                                                else if (nChk19 < 0 && nChk20 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 음 20 양

                                                #region 19 0 20 0
                                                else if (nChk19 < 0 && nChk20 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 0 20 0
                                            }
                                        }
                                        #endregion 18 양
                                    }
                                    #endregion 18

                                    #region 19
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")) != "")
                                    {
                                        double n19 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")));

                                        #region 19 음
                                        if (n19 < 0 && string.Equals(e.Cell.Column.Name, "19"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")) != "")
                                            {
                                                double nChk20 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")));
                                                double nChk21 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")));

                                                #region 20 음 21 음
                                                if (nChk20 < 0 && nChk21 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 음 21 음

                                                #region 20 양 21 움
                                                else if (nChk20 > 0 && nChk21 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 양 21 음

                                                #region 20 양 21 양
                                                else if (nChk20 > 0 && nChk21 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 양 21 양

                                                #region 20 음 21 양
                                                else if (nChk20 < 0 && nChk21 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 음 21 양

                                                #region 20 0 21 0
                                                else if (nChk20 < 0 && nChk21 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 0 21 0
                                            }
                                            else
                                            {
                                                double nChk20 = 0;
                                                double nChk21 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")) != "")
                                                {
                                                    nChk20 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")) != "")
                                                {
                                                    nChk21 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")));
                                                }

                                                #region 20 음 21 음
                                                if (nChk20 < 0 && nChk21 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 음 21 음

                                                #region 20 양 21 움
                                                else if (nChk20 > 0 && nChk21 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 양 21 음

                                                #region 20 양 21 양
                                                else if (nChk20 > 0 && nChk21 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 양 21 양

                                                #region 20 음 21 양
                                                else if (nChk20 < 0 && nChk21 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 음 21 양

                                                #region 20 0 21 0
                                                else if (nChk20 < 0 && nChk21 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 0 21 0
                                            }
                                        }
                                        #endregion 19 음

                                        #region 19 양
                                        else if (n19 > 0 && string.Equals(e.Cell.Column.Name, "19"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")) != "")
                                            {
                                                double nChk20 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")));
                                                double nChk21 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")));

                                                #region 20 음 21 음
                                                if (nChk20 < 0 && nChk21 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 음 21 음

                                                #region 20 양 21 움
                                                else if (nChk20 > 0 && nChk21 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 양 21 음

                                                #region 20 양 21 양
                                                else if (nChk20 > 0 && nChk21 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 양 21 양

                                                #region 20 음 21 양
                                                else if (nChk20 < 0 && nChk21 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 음 21 양

                                                #region 20 0 21 0
                                                else if (nChk20 < 0 && nChk21 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 0 21 0
                                            }
                                            else
                                            {
                                                double nChk20 = 0;
                                                double nChk21 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")) != "")
                                                {
                                                    nChk20 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")) != "")
                                                {
                                                    nChk21 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")));
                                                }

                                                #region 20 음 21 음
                                                if (nChk20 < 0 && nChk21 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 음 21 음

                                                #region 20 양 21 움
                                                else if (nChk20 > 0 && nChk21 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 양 21 음

                                                #region 20 양 21 양
                                                else if (nChk20 > 0 && nChk21 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 양 21 양

                                                #region 20 음 21 양
                                                else if (nChk20 < 0 && nChk21 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 음 21 양

                                                #region 20 0 21 0
                                                else if (nChk20 < 0 && nChk21 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 0 21 0
                                            }
                                        }
                                        #endregion 19 양
                                    }
                                    #endregion 19

                                    #region 20
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")) != "")
                                    {
                                        double n20 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")));

                                        #region 20 음
                                        if (n20 < 0 && string.Equals(e.Cell.Column.Name, "20"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")) != "")
                                            {
                                                double nChk21 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")));
                                                double nChk22 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")));

                                                #region 21 음 22 음
                                                if (nChk21 < 0 && nChk22 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 음 22 음

                                                #region 21 양 22 움
                                                else if (nChk21 > 0 && nChk22 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 양 22 음

                                                #region 21 양 22 양
                                                else if (nChk21 > 0 && nChk22 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 양 22 양

                                                #region 21 음 22 양
                                                else if (nChk21 < 0 && nChk22 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 음 22 양

                                                #region 21 0 22 0
                                                else if (nChk21 < 0 && nChk22 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 0 22 0
                                            }
                                            else
                                            {
                                                double nChk21 = 0;
                                                double nChk22 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")) != "")
                                                {
                                                    nChk21 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")) != "")
                                                {
                                                    nChk22 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")));
                                                }

                                                #region 21 음 22 음
                                                if (nChk21 < 0 && nChk22 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 음 22 음

                                                #region 21 양 22 움
                                                else if (nChk21 > 0 && nChk22 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 양 22 음

                                                #region 21 양 22 양
                                                else if (nChk21 > 0 && nChk22 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 양 22 양

                                                #region 21 음 22 양
                                                else if (nChk21 < 0 && nChk22 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 음 22 양

                                                #region 21 0 22 0
                                                else if (nChk21 < 0 && nChk22 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 0 22 0
                                            }
                                        }
                                        #endregion 20 음

                                        #region 20 양
                                        else if (n20 > 0 && string.Equals(e.Cell.Column.Name, "20"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")) != "")
                                            {
                                                double nChk21 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")));
                                                double nChk22 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")));

                                                #region 21 음 22 음
                                                if (nChk21 < 0 && nChk22 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 음 22 음

                                                #region 21 양 22 움
                                                else if (nChk21 > 0 && nChk22 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 양 22 음

                                                #region 21 양 22 양
                                                else if (nChk21 > 0 && nChk22 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 양 22 양

                                                #region 21 음 22 양
                                                else if (nChk21 < 0 && nChk22 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 음 22 양

                                                #region 21 0 22 0
                                                else if (nChk21 < 0 && nChk22 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 0 22 0
                                            }
                                            else
                                            {
                                                double nChk21 = 0;
                                                double nChk22 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")) != "")
                                                {
                                                    nChk21 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")) != "")
                                                {
                                                    nChk22 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")));
                                                }

                                                #region 21 음 22 음
                                                if (nChk21 < 0 && nChk22 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 음 22 음

                                                #region 21 양 22 움
                                                else if (nChk21 > 0 && nChk22 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 양 22 음

                                                #region 21 양 22 양
                                                else if (nChk21 > 0 && nChk22 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 양 22 양

                                                #region 21 음 22 양
                                                else if (nChk21 < 0 && nChk22 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 음 22 양

                                                #region 21 0 22 0
                                                else if (nChk21 < 0 && nChk22 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 0 22 0
                                            }
                                        }
                                        #endregion 20 양
                                    }
                                    #endregion 20

                                    #region 21
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")) != "")
                                    {
                                        double n21 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")));

                                        #region 21 음
                                        if (n21 < 0 && string.Equals(e.Cell.Column.Name, "21"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")) != "")
                                            {
                                                double nChk22 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")));
                                                double nChk23 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")));

                                                #region 22 음 23 음
                                                if (nChk22 < 0 && nChk23 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 음 23 음

                                                #region 22 양 23 움
                                                else if (nChk22 > 0 && nChk23 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 양 23 음

                                                #region 22 양 22 양
                                                else if (nChk22 > 0 && nChk23 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 양 23 양

                                                #region 22 음 23 양
                                                else if (nChk22 < 0 && nChk23 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 음 23 양

                                                #region 22 0 23 0
                                                else if (nChk22 < 0 && nChk23 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 0 23 0
                                            }
                                            else
                                            {
                                                double nChk22 = 0;
                                                double nChk23 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")) != "")
                                                {
                                                    nChk22 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")) != "")
                                                {
                                                    nChk23 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")));
                                                }


                                                #region 22 음 23 음
                                                if (nChk22 < 0 && nChk23 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 음 23 음

                                                #region 22 양 23 움
                                                else if (nChk22 > 0 && nChk23 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 양 23 음

                                                #region 22 양 23 양
                                                else if (nChk22 > 0 && nChk23 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 양 23 양

                                                #region 22 음 23 양
                                                else if (nChk22 < 0 && nChk23 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 음 23 양

                                                #region 22 0 23 0
                                                else if (nChk22 < 0 && nChk23 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 0 23 0
                                            }
                                        }
                                        #endregion 21 음

                                        #region 21 양
                                        else if (n21 > 0 && string.Equals(e.Cell.Column.Name, "21"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")) != "")
                                            {
                                                double nChk22 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")));
                                                double nChk23 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")));

                                                #region 22 음 23 음
                                                if (nChk22 < 0 && nChk23 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 음 23 음

                                                #region 22 양 23 움
                                                else if (nChk22 > 0 && nChk23 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 양 23 음

                                                #region 22 양 23 양
                                                else if (nChk22 > 0 && nChk23 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 양 23 양

                                                #region 22 음 23 양
                                                else if (nChk22 < 0 && nChk23 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 음 23 양

                                                #region 22 0 23 0
                                                else if (nChk22 < 0 && nChk23 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 0 23 0
                                            }
                                            else
                                            {
                                                double nChk22 = 0;
                                                double nChk23 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")) != "")
                                                {
                                                    nChk22 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")) != "")
                                                {
                                                    nChk23 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")));
                                                }


                                                #region 22 음 23 음
                                                if (nChk22 < 0 && nChk23 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 음 23 음

                                                #region 22 양 23 움
                                                else if (nChk22 > 0 && nChk23 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 양 23 음

                                                #region 22 양 23 양
                                                else if (nChk22 > 0 && nChk23 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 양 23 양

                                                #region 22 음 23 양
                                                else if (nChk22 < 0 && nChk23 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 음 23 양

                                                #region 22 0 23 0
                                                else if (nChk22 < 0 && nChk23 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 0 23 0
                                            }
                                        }
                                        #endregion 21 양
                                    }
                                    #endregion 21

                                    #region 22
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")) != "")
                                    {
                                        double n22 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")));

                                        #region 22 음
                                        if (n22 < 0 && string.Equals(e.Cell.Column.Name, "22"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")) != "")
                                            {
                                                double nChk23 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")));
                                                double nChk24 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")));

                                                #region 23 음 24 음
                                                if (nChk23 < 0 && nChk24 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 음 24 음

                                                #region 23 양 24 움
                                                else if (nChk23 > 0 && nChk24 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 양 24 음

                                                #region 23 양 24 양
                                                else if (nChk23 > 0 && nChk24 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 양 24 양

                                                #region 23 음 24 양
                                                else if (nChk23 < 0 && nChk24 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 음 24 양

                                                #region 23 0 24 0
                                                else if (nChk23 < 0 && nChk24 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 0 24 0
                                            }
                                            else
                                            {
                                                double nChk23 = 0;
                                                double nChk24 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")) != "")
                                                {
                                                    nChk23 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")) != "")
                                                {
                                                    nChk24 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")));
                                                }

                                                #region 23 음 24 음
                                                if (nChk23 < 0 && nChk24 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 음 24 음

                                                #region 23 양 24 움
                                                else if (nChk23 > 0 && nChk24 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 양 24 음

                                                #region 23 양 24 양
                                                else if (nChk23 > 0 && nChk24 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 양 24 양

                                                #region 23 음 24 양
                                                else if (nChk23 < 0 && nChk24 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 음 24 양

                                                #region 23 0 24 0
                                                else if (nChk23 < 0 && nChk24 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 0 24 0
                                            }
                                        }
                                        #endregion 22 음

                                        #region 22 양
                                        else if (n22 > 0 && string.Equals(e.Cell.Column.Name, "22"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")) != "")
                                            {
                                                double nChk23 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")));
                                                double nChk24 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")));

                                                #region 23 음 24 음
                                                if (nChk23 < 0 && nChk24 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 음 24 음

                                                #region 23 양 24 움
                                                else if (nChk23 > 0 && nChk24 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 양 24 음

                                                #region 23 양 24 양
                                                else if (nChk23 > 0 && nChk24 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 양 24 양

                                                #region 23 음 24 양
                                                else if (nChk23 < 0 && nChk24 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 음 24 양

                                                #region 23 0 24 0
                                                else if (nChk23 < 0 && nChk24 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 0 24 0
                                            }
                                            else
                                            {
                                                double nChk23 = 0;
                                                double nChk24 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")) != "")
                                                {
                                                    nChk23 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")) != "")
                                                {
                                                    nChk24 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")));
                                                }

                                                #region 23 음 24 음
                                                if (nChk23 < 0 && nChk24 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 음 24 음

                                                #region 23 양 24 움
                                                else if (nChk23 > 0 && nChk24 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 양 24 음

                                                #region 23 양 24 양
                                                else if (nChk23 > 0 && nChk24 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 양 24 양

                                                #region 23 음 24 양
                                                else if (nChk23 < 0 && nChk24 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 음 24 양

                                                #region 23 0 24 0
                                                else if (nChk23 < 0 && nChk24 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 0 24 0
                                            }
                                        }
                                        #endregion 22 양
                                    }
                                    #endregion 22

                                    #region 23
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")) != "")
                                    {
                                        double n23 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")));

                                        #region 23 음
                                        if (n23 < 0 && string.Equals(e.Cell.Column.Name, "23"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")) != "")
                                            {
                                                double nChk24 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")));
                                                double nChk1 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")));

                                                #region 24 음 1 음
                                                if (nChk24 < 0 && nChk1 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 음 1 음

                                                #region 24 양 1 움
                                                else if (nChk24 > 0 && nChk1 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 양 1 음

                                                #region 24 양 1 양
                                                else if (nChk24 > 0 && nChk1 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 양 1 양

                                                #region 24 음 1 양
                                                else if (nChk24 < 0 && nChk1 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 음 1 양

                                                #region 24 0 1 0
                                                else if (nChk24 < 0 && nChk1 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 0 1 0
                                            }
                                            else
                                            {
                                                double nChk24 = 0;
                                                double nChk1 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")) != "")
                                                {
                                                    nChk24 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")) != "")
                                                {
                                                    nChk1 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")));
                                                }

                                                #region 24 음 1 음
                                                if (nChk24 < 0 && nChk1 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 음 1 음

                                                #region 24 양 1 움
                                                else if (nChk24 > 0 && nChk1 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 양 1 음

                                                #region 24 양 1 양
                                                else if (nChk24 > 0 && nChk1 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 양 1 양

                                                #region 24 음 1 양
                                                else if (nChk24 < 0 && nChk1 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 음 1 양

                                                #region 24 0 1 0
                                                else if (nChk24 < 0 && nChk1 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 0 1 0
                                            }
                                        }
                                        #endregion 23 음

                                        #region 23 양
                                        else if (n23 > 0 && string.Equals(e.Cell.Column.Name, "23"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")) != "")
                                            {
                                                double nChk24 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")));
                                                double nChk1 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")));

                                                #region 24 음 1 음
                                                if (nChk24 < 0 && nChk1 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 음 1 음

                                                #region 24 양 1 움
                                                else if (nChk24 > 0 && nChk1 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 양 1 음

                                                #region 24 양 1 양
                                                else if (nChk24 > 0 && nChk1 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 양 1 양

                                                #region 24 음 1 양
                                                else if (nChk24 < 0 && nChk1 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 음 1 양

                                                #region 24 0 1 0
                                                else if (nChk24 < 0 && nChk1 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 0 1 0
                                            }
                                            else
                                            {
                                                double nChk24 = 0;
                                                double nChk1 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")) != "")
                                                {
                                                    nChk24 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")) != "")
                                                {
                                                    nChk1 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")));
                                                }

                                                #region 24 음 1 음
                                                if (nChk24 < 0 && nChk1 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 음 1 음

                                                #region 24 양 1 움
                                                else if (nChk24 > 0 && nChk1 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 양 1 음

                                                #region 24 양 1 양
                                                else if (nChk24 > 0 && nChk1 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 양 1 양

                                                #region 24 음 1 양
                                                else if (nChk24 < 0 && nChk1 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 음 1 양

                                                #region 24 0 1 0
                                                else if (nChk24 < 0 && nChk1 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 0 1 0
                                            }
                                        }
                                        #endregion 23 양
                                    }
                                    #endregion 23

                                    #region 24
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")) != "")
                                    {
                                        double n24 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")));

                                        #region 24 음
                                        if (n24 < 0 && string.Equals(e.Cell.Column.Name, "24"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")) != "")
                                            {
                                                double nChk1 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")));
                                                double nChk2 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")));

                                                #region 1 음 2 음
                                                if (nChk1 < 0 && nChk2 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 음 2 음

                                                #region 1 양 2 움
                                                else if (nChk1 > 0 && nChk2 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 양 2 음

                                                #region 1 양 2 양
                                                else if (nChk1 > 0 && nChk2 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 양 2 양

                                                #region 1 음 2 양
                                                else if (nChk1 < 0 && nChk2 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 음 2 양

                                                #region 1 0 2 0
                                                else if (nChk1 < 0 && nChk2 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 0 2 0
                                            }
                                            else
                                            {
                                                double nChk1 = 0;
                                                double nChk2 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")) != "")
                                                {
                                                    nChk1 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")) != "")
                                                {
                                                    nChk2 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")));
                                                }

                                                #region 1 음 2 음
                                                if (nChk1 < 0 && nChk2 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 음 2 음

                                                #region 1 양 2 움
                                                else if (nChk1 > 0 && nChk2 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 양 2 음

                                                #region 1 양 2 양
                                                else if (nChk1 > 0 && nChk2 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 양 2 양

                                                #region 1 음 2 양
                                                else if (nChk1 < 0 && nChk2 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 음 2 양

                                                #region 1 0 2 0
                                                else if (nChk1 < 0 && nChk2 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 0 2 0
                                            }
                                        }
                                        #endregion 24 음

                                        #region 24 양
                                        else if (n24 > 0 && string.Equals(e.Cell.Column.Name, "24"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")) != "")
                                            {
                                                double nChk1 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")));
                                                double nChk2 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")));

                                                #region 1 음 2 음
                                                if (nChk1 < 0 && nChk2 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 음 2 음

                                                #region 1 양 2 움
                                                else if (nChk1 > 0 && nChk2 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 양 2 음

                                                #region 1 양 2 양
                                                else if (nChk1 > 0 && nChk2 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 양 2 양

                                                #region 1 음 2 양
                                                else if (nChk1 < 0 && nChk2 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 음 2 양

                                                #region 1 0 2 0
                                                else if (nChk1 < 0 && nChk2 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 0 2 0
                                            }
                                            else
                                            {
                                                double nChk1 = 0;
                                                double nChk2 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")) != "")
                                                {
                                                    nChk1 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")) != "")
                                                {
                                                    nChk2 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")));
                                                }

                                                #region 1 음 2 음
                                                if (nChk1 < 0 && nChk2 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 음 2 음

                                                #region 1 양 2 움
                                                else if (nChk1 > 0 && nChk2 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 양 2 음

                                                #region 1 양 2 양
                                                else if (nChk1 > 0 && nChk2 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 양 2 양

                                                #region 1 음 2 양
                                                else if (nChk1 < 0 && nChk2 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 음 2 양

                                                #region 1 0 2 0
                                                else if (nChk1 < 0 && nChk2 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 0 2 0
                                            }
                                        }
                                        #endregion 24 양
                                    }
                                    #endregion 24

                                    #region 01
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")) != "")
                                    {
                                        double n01 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")));

                                        #region 01 음
                                        if (n01 < 0 && string.Equals(e.Cell.Column.Name, "1"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")) != "")
                                            {
                                                double nChk2 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")));
                                                double nChk3 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")));

                                                #region 02 음 03 음
                                                if (nChk2 < 0 && nChk3 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 음 03 음

                                                #region 02 양 03 움
                                                else if (nChk2 > 0 && nChk3 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 양 03 음

                                                #region 02 양 03 양
                                                else if (nChk2 > 0 && nChk3 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 양 03 양

                                                #region 02 음 03 양
                                                else if (nChk2 < 0 && nChk3 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 음 03 양

                                                #region 02 0 03 0
                                                else if (nChk2 < 0 && nChk3 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 0 03 0
                                            }
                                            else
                                            {
                                                double nChk2 = 0;
                                                double nChk3 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")) != "")
                                                {
                                                    nChk2 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")) != "")
                                                {
                                                    nChk3 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")));
                                                }

                                                #region 02 음 03 음
                                                if (nChk2 < 0 && nChk3 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 음 03 음

                                                #region 02 양 03 움
                                                else if (nChk2 > 0 && nChk3 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 양 03 음

                                                #region 02 양 03 양
                                                else if (nChk2 > 0 && nChk3 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 양 03 양

                                                #region 02 음 03 양
                                                else if (nChk2 < 0 && nChk3 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 음 03 양

                                                #region 02 0 03 0
                                                else if (nChk2 < 0 && nChk3 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 0 03 0
                                            }
                                        }
                                        #endregion 01 음

                                        #region 01 양
                                        else if (n01 > 0 && string.Equals(e.Cell.Column.Name, "1"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")) != "")
                                            {
                                                double nChk2 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")));
                                                double nChk3 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")));

                                                #region 02 음 03 음
                                                if (nChk2 < 0 && nChk3 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 음 03 음

                                                #region 02 양 03 움
                                                else if (nChk2 > 0 && nChk3 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 양 03 음

                                                #region 02 양 03 양
                                                else if (nChk2 > 0 && nChk3 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 양 03 양

                                                #region 02 음 03 양
                                                else if (nChk2 < 0 && nChk3 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 음 03 양

                                                #region 02 0 03 0
                                                else if (nChk2 < 0 && nChk3 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 0 03 0
                                            }
                                            else
                                            {
                                                double nChk2 = 0;
                                                double nChk3 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")) != "")
                                                {
                                                    nChk2 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")) != "")
                                                {
                                                    nChk3 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")));
                                                }

                                                #region 02 음 03 음
                                                if (nChk2 < 0 && nChk3 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 음 03 음

                                                #region 02 양 03 움
                                                else if (nChk2 > 0 && nChk3 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 양 03 음

                                                #region 02 양 03 양
                                                else if (nChk2 > 0 && nChk3 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 양 03 양

                                                #region 02 음 03 양
                                                else if (nChk2 < 0 && nChk3 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 음 03 양

                                                #region 02 0 03 0
                                                else if (nChk2 < 0 && nChk3 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 0 03 0
                                            }
                                        }
                                        #endregion 01 양
                                    }
                                    #endregion 01

                                    #region 02
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")) != "")
                                    {
                                        double n02 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")));

                                        #region 21 음
                                        if (n02 < 0 && string.Equals(e.Cell.Column.Name, "2"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")) != "")
                                            {
                                                double nChk3 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")));
                                                double nChk4 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")));

                                                #region 03 음 04 음
                                                if (nChk3 < 0 && nChk4 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 음 04 음

                                                #region 03 양 04 움
                                                else if (nChk3 > 0 && nChk4 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 양 04 음

                                                #region 03 양 04 양
                                                else if (nChk3 > 0 && nChk4 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 양 04 양

                                                #region 03 음 04 양
                                                else if (nChk3 < 0 && nChk4 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 음 04 양

                                                #region 03 0 04 0
                                                else if (nChk3 < 0 && nChk4 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 0 04 0
                                            }
                                            else
                                            {
                                                double nChk3 = 0;
                                                double nChk4 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")) != "")
                                                {
                                                    nChk3 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")) != "")
                                                {
                                                    nChk4 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")));
                                                }

                                                #region 03 음 04 음
                                                if (nChk3 < 0 && nChk4 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 음 04 음

                                                #region 03 양 04 움
                                                else if (nChk3 > 0 && nChk4 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 양 04 음

                                                #region 03 양 04 양
                                                else if (nChk3 > 0 && nChk4 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 양 04 양

                                                #region 03 음 04 양
                                                else if (nChk3 < 0 && nChk4 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 음 04 양

                                                #region 03 0 04 0
                                                else if (nChk3 < 0 && nChk4 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 0 04 0
                                            }
                                        }
                                        #endregion 02 음

                                        #region 02 양
                                        else if (n02 > 0 && string.Equals(e.Cell.Column.Name, "2"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")) != "")
                                            {
                                                double nChk3 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")));
                                                double nChk4 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")));

                                                #region 03 음 04 음
                                                if (nChk3 < 0 && nChk4 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 음 04 음

                                                #region 03 양 04 움
                                                else if (nChk3 > 0 && nChk4 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 양 04 음

                                                #region 03 양 04 양
                                                else if (nChk3 > 0 && nChk4 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 양 04 양

                                                #region 03 음 04 양
                                                else if (nChk3 < 0 && nChk4 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 음 04 양

                                                #region 03 0 04 0
                                                else if (nChk3 < 0 && nChk4 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 0 04 0
                                            }
                                            else
                                            {
                                                double nChk3 = 0;
                                                double nChk4 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")) != "")
                                                {
                                                    nChk3 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")) != "")
                                                {
                                                    nChk4 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")));
                                                }

                                                #region 03 음 04 음
                                                if (nChk3 < 0 && nChk4 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 음 04 음

                                                #region 03 양 04 움
                                                else if (nChk3 > 0 && nChk4 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 양 04 음

                                                #region 03 양 04 양
                                                else if (nChk3 > 0 && nChk4 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 양 04 양

                                                #region 03 음 04 양
                                                else if (nChk3 < 0 && nChk4 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 음 04 양

                                                #region 03 0 04 0
                                                else if (nChk3 < 0 && nChk4 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 0 04 0
                                            }
                                        }
                                        #endregion 02 양
                                    }
                                    #endregion 02

                                    #region 03
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")) != "")
                                    {
                                        double n03 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")));

                                        #region 03 음
                                        if (n03 < 0 && string.Equals(e.Cell.Column.Name, "3"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")) != "")
                                            {
                                                double nChk4 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")));
                                                double nChk5 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")));

                                                #region 04 음 05 음
                                                if (nChk4 < 0 && nChk5 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 음 05 음

                                                #region 04 양 05 움
                                                else if (nChk4 > 0 && nChk5 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 양 05 음

                                                #region 04 양 05 양
                                                else if (nChk4 > 0 && nChk5 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 양 05 양

                                                #region 04 음 05 양
                                                else if (nChk4 < 0 && nChk5 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 음 05 양

                                                #region 04 0 05 0
                                                else if (nChk4 < 0 && nChk5 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 0 05 0
                                            }
                                            else
                                            {
                                                double nChk4 = 0;
                                                double nChk5 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")) != "")
                                                {
                                                    nChk4 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")) != "")
                                                {
                                                    nChk5 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")));
                                                }


                                                #region 04 음 05 음
                                                if (nChk4 < 0 && nChk5 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 음 05 음

                                                #region 04 양 05 움
                                                else if (nChk4 > 0 && nChk5 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 양 05 음

                                                #region 04 양 05 양
                                                else if (nChk4 > 0 && nChk5 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 양 05 양

                                                #region 04 음 05 양
                                                else if (nChk4 < 0 && nChk5 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 음 05 양

                                                #region 04 0 05 0
                                                else if (nChk4 < 0 && nChk5 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 0 05 0
                                            }
                                        }
                                        #endregion 03 음

                                        #region 03 양
                                        else if (n03 > 0 && string.Equals(e.Cell.Column.Name, "3"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")) != "")
                                            {
                                                double nChk4 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")));
                                                double nChk5 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")));

                                                #region 04 음 05 음
                                                if (nChk4 < 0 && nChk5 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 음 05 음

                                                #region 04 양 05 움
                                                else if (nChk4 > 0 && nChk5 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 양 05 음

                                                #region 04 양 05 양
                                                else if (nChk4 > 0 && nChk5 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 양 05 양

                                                #region 04 음 05 양
                                                else if (nChk4 < 0 && nChk5 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 음 05 양

                                                #region 04 0 05 0
                                                else if (nChk4 < 0 && nChk5 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 0 05 0
                                            }
                                            else
                                            {
                                                double nChk4 = 0;
                                                double nChk5 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")) != "")
                                                {
                                                    nChk4 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")) != "")
                                                {
                                                    nChk5 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")));
                                                }


                                                #region 04 음 05 음
                                                if (nChk4 < 0 && nChk5 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 음 05 음

                                                #region 04 양 05 움
                                                else if (nChk4 > 0 && nChk5 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 양 05 음

                                                #region 04 양 05 양
                                                else if (nChk4 > 0 && nChk5 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 양 05 양

                                                #region 04 음 05 양
                                                else if (nChk4 < 0 && nChk5 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 음 05 양

                                                #region 04 0 05 0
                                                else if (nChk4 < 0 && nChk5 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 0 05 0
                                            }
                                        }
                                        #endregion 03 양
                                    }
                                    #endregion 03

                                    #region 04
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")) != "")
                                    {
                                        double n04 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")));

                                        #region 04 음
                                        if (n04 < 0 && string.Equals(e.Cell.Column.Name, "4"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")) != "")
                                            {
                                                double nChk5 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")));
                                                double nChk6 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")));

                                                #region 05 음 06 음
                                                if (nChk5 < 0 && nChk6 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 음 06 음

                                                #region 05 양 06 움
                                                else if (nChk5 > 0 && nChk6 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 양 06 음

                                                #region 05 양 06 양
                                                else if (nChk5 > 0 && nChk6 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 양 06 양

                                                #region 05 음 06 양
                                                else if (nChk5 < 0 && nChk6 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 음 06 양

                                                #region 05 0 06 0
                                                else if (nChk5 < 0 && nChk6 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 0 06 0
                                            }
                                            else
                                            {
                                                double nChk5 = 0;
                                                double nChk6 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")) != "")
                                                {
                                                    nChk5 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")) != "")
                                                {
                                                    nChk6 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")));
                                                }

                                                #region 05 음 06 음
                                                if (nChk5 < 0 && nChk6 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 음 06 음

                                                #region 05 양 06 움
                                                else if (nChk5 > 0 && nChk6 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 양 06 음

                                                #region 05 양 06 양
                                                else if (nChk5 > 0 && nChk6 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 양 06 양

                                                #region 05 음 06 양
                                                else if (nChk5 < 0 && nChk6 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 음 06 양

                                                #region 05 0 06 0
                                                else if (nChk5 < 0 && nChk6 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 0 06 0
                                            }
                                        }
                                        #endregion 04 음

                                        #region 04 양
                                        else if (n04 > 0 && string.Equals(e.Cell.Column.Name, "4"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")) != "")
                                            {
                                                double nChk5 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")));
                                                double nChk6 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")));

                                                #region 05 음 06 음
                                                if (nChk5 < 0 && nChk6 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 음 06 음

                                                #region 05 양 06 움
                                                else if (nChk5 > 0 && nChk6 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 양 06 음

                                                #region 05 양 06 양
                                                else if (nChk5 > 0 && nChk6 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 양 06 양

                                                #region 05 음 06 양
                                                else if (nChk5 < 0 && nChk6 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 음 06 양

                                                #region 05 0 06 0
                                                else if (nChk5 < 0 && nChk6 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 0 06 00
                                            }
                                            else
                                            {
                                                double nChk5 = 0;
                                                double nChk6 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")) != "")
                                                {
                                                    nChk5 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")) != "")
                                                {
                                                    nChk6 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")));
                                                }

                                                #region 05 음 06 음
                                                if (nChk5 < 0 && nChk6 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 음 06 음

                                                #region 05 양 06 움
                                                else if (nChk5 > 0 && nChk6 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 양 06 음

                                                #region 05 양 06 양
                                                else if (nChk5 > 0 && nChk6 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 양 06 양

                                                #region 05 음 06 양
                                                else if (nChk5 < 0 && nChk6 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 음 06 양

                                                #region 05 0 06 0
                                                else if (nChk5 < 0 && nChk6 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 0 06 0
                                            }
                                        }
                                        #endregion 04 양
                                    }
                                    #endregion 04

                                    #region 05
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")) != "")
                                    {
                                        double n05 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")));

                                        #region 05 음
                                        if (n05 < 0 && string.Equals(e.Cell.Column.Name, "5"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")) != "")
                                            {
                                                double nChk6 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")));
                                                double nChk7 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")));

                                                #region 06 음 07 음
                                                if (nChk6 < 0 && nChk7 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 음 07 음

                                                #region 06 양 07 움
                                                else if (nChk6 > 0 && nChk7 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 양 07 음

                                                #region 06 양 07 양
                                                else if (nChk6 > 0 && nChk7 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 양 07 양

                                                #region 06 음 07 양
                                                else if (nChk6 < 0 && nChk7 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 음 07 양

                                                #region 06 0 07 0
                                                else if (nChk6 == 0 && nChk7 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 0 07 0
                                            }
                                            else
                                            {
                                                double nChk6 = 0;
                                                double nChk7 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")) != "")
                                                {
                                                    nChk6 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")) != "")
                                                {
                                                    nChk7 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")));
                                                }

                                                #region 06 음 07 음
                                                if (nChk6 < 0 && nChk7 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 음 07 음

                                                #region 06 양 07 움
                                                else if (nChk6 > 0 && nChk7 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 양 07 음

                                                #region 06 양 07 양
                                                else if (nChk6 > 0 && nChk7 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 양 07 양

                                                #region 06 음 07 양
                                                else if (nChk6 < 0 && nChk7 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 음 07 양

                                                #region 06 0 07 0
                                                else if (nChk6 == 0 && nChk7 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 0 07 0
                                            }
                                        }
                                        #endregion 05 음

                                        #region 05 양
                                        else if (n05 > 0 && string.Equals(e.Cell.Column.Name, "5"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")) != "")
                                            {
                                                double nChk6 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")));
                                                double nChk7 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")));

                                                #region 06 음 07 음
                                                if (nChk6 < 0 && nChk7 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 음 07 음

                                                #region 06 양 07 움
                                                else if (nChk6 > 0 && nChk7 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 양 07 음

                                                #region 06 양 07 양
                                                else if (nChk6 > 0 && nChk7 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 양 07 양

                                                #region 06 음 07 양
                                                else if (nChk6 < 0 && nChk7 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 음 07 양

                                                #region 06 0 07 0
                                                else if (nChk6 == 0 && nChk7 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 0 07 0
                                            }
                                            else
                                            {
                                                double nChk6 = 0;
                                                double nChk7 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")) != "")
                                                {
                                                    nChk6 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")) != "")
                                                {
                                                    nChk7 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")));
                                                }

                                                #region 06 음 07 음
                                                if (nChk6 < 0 && nChk7 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 음 07 음

                                                #region 06 양 07 움
                                                else if (nChk6 > 0 && nChk7 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 양 07 음

                                                #region 06 양 07 양
                                                else if (nChk6 > 0 && nChk7 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 양 07 양

                                                #region 06 음 07 양
                                                else if (nChk6 < 0 && nChk7 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 음 07 양

                                                #region 06 0 07 0
                                                else if (nChk6 == 0 && nChk7 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 0 07 0
                                            }
                                        }
                                        #endregion 05 양
                                    }
                                    #endregion 05

                                    #region 06
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")) != "")
                                    {
                                        double n06 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")));

                                        #region 06 음
                                        if (n06 < 0 && string.Equals(e.Cell.Column.Name, "6"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")) != "")
                                            {
                                                double nChk7 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")));
                                                double nChk8 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")));

                                                #region 07 음 08 음
                                                if (nChk7 < 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 음

                                                #region 07 양 08 움
                                                else if (nChk7 > 0 && nChk8 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 음

                                                #region 07 양 08 양
                                                else if (nChk7 > 0 && nChk8 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 양

                                                #region 07 음 08 양
                                                else if (nChk7 < 0 && nChk8 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 양

                                                #region 07 0 08 0
                                                else if (nChk7 == 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 0

                                                #region 07 양 08 0
                                                else if (nChk7 > 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 0

                                                #region 07 음 08 0
                                                else if (nChk7 < 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 0

                                                #region 07 0 08 양
                                                else if (nChk7 == 0 && nChk8 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 양

                                                #region 07 0 08 음
                                                else if (nChk7 == 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 음
                                            }
                                            else
                                            {
                                                double nChk7 = 0;
                                                double nChk8 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")) != "")
                                                {
                                                    nChk7 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")) != "")
                                                {
                                                    nChk8 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")));
                                                }


                                                #region 07 음 08 음
                                                if (nChk7 < 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 음

                                                #region 07 양 08 움
                                                else if (nChk7 > 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 음

                                                #region 07 양 08 양
                                                else if (nChk7 > 0 && nChk8 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 양

                                                #region 07 음 08 양
                                                else if (nChk7 < 0 && nChk8 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 양

                                                #region 07 0 08 0
                                                else if (nChk7 == 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 0

                                                #region 07 양 08 0
                                                else if (nChk7 > 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 0

                                                #region 07 음 08 0
                                                else if (nChk7 < 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 0

                                                #region 07 0 08 양
                                                else if (nChk7 == 0 && nChk8 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 양

                                                #region 07 0 08 음
                                                else if (nChk7 == 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 음

                                            }
                                        }
                                        #endregion 06 음

                                        #region 06 양
                                        else if (n06 > 0 && string.Equals(e.Cell.Column.Name, "6"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")) != "")
                                            {
                                                double nChk7 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")));
                                                double nChk8 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")));

                                                #region 07 음 08 음
                                                if (nChk7 < 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 음

                                                #region 07 양 08 움
                                                else if (nChk7 > 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 음

                                                #region 07 양 08 양
                                                else if (nChk7 > 0 && nChk8 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 양

                                                #region 07 음 08 양
                                                else if (nChk7 < 0 && nChk8 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 양

                                                #region 07 0 08 0
                                                else if (nChk7 == 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 0

                                                #region 07 양 08 0
                                                else if (nChk7 > 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 0

                                                #region 07 음 08 0
                                                else if (nChk7 < 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 0

                                                #region 07 0 08 양
                                                else if (nChk7 == 0 && nChk8 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 양

                                                #region 07 0 08 음
                                                else if (nChk7 == 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 음
                                            }
                                            else
                                            {
                                                double nChk7 = 0;
                                                double nChk8 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")) != "")
                                                {
                                                    nChk7 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")) != "")
                                                {
                                                    nChk8 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")));
                                                }

                                                #region 07 음 08 음
                                                if (nChk7 < 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 음

                                                #region 07 양 08 움
                                                else if (nChk7 > 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 음

                                                #region 07 양 08 양
                                                else if (nChk7 > 0 && nChk8 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 양

                                                #region 07 음 08 양
                                                else if (nChk7 < 0 && nChk8 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 양

                                                #region 07 0 08 0
                                                else if (nChk7 == 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 0

                                                #region 07 양 08 0
                                                else if (nChk7 > 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 0

                                                #region 07 음 08 0
                                                else if (nChk7 < 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 0

                                                #region 07 0 08 양
                                                else if (nChk7 == 0 && nChk8 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 양

                                                #region 07 0 08 음
                                                else if (nChk7 == 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 음
                                            }
                                        }
                                        #endregion 06 양
                                    }
                                    #endregion 06

                                    #region 07
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")) != "")
                                    {
                                        double n07 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")));

                                        #region 07 음
                                        if (n07 < 0 && string.Equals(e.Cell.Column.Name, "7"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")) != "")
                                            {
                                                double nChk8 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")));
                                                double nChk9 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")));

                                                #region 08 음 09 음
                                                if (nChk8 < 0 && nChk9 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 음

                                                #region 08 양 09 움
                                                else if (nChk8 > 0 && nChk9 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 음

                                                #region 08 양 09 양
                                                else if (nChk8 > 0 && nChk9 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 양

                                                #region 08 음 09 양
                                                else if (nChk8 < 0 && nChk9 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 양

                                                #region 08 0 09 0
                                                else if (nChk8 == 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 0

                                                #region 08 양 09 0
                                                else if (nChk8 > 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 0

                                                #region 08 음 09 0
                                                else if (nChk8 < 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 0

                                                #region 08 0 09 양
                                                else if (nChk8 == 0 && nChk9 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 양

                                                #region 08 0 09 음
                                                else if (nChk8 == 0 && nChk9 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 음
                                            }
                                            else
                                            {
                                                double nChk8 = 0;
                                                double nChk9 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")) != "")
                                                {
                                                    nChk8 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")) != "")
                                                {
                                                    nChk9 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")));
                                                }

                                                #region 08 음 09 음
                                                if (nChk8 < 0 && nChk9 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 음

                                                #region 08 양 09 움
                                                else if (nChk8 > 0 && nChk9 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 음

                                                #region 08 양 09 양
                                                else if (nChk8 > 0 && nChk9 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 양

                                                #region 08 음 09 양
                                                else if (nChk8 < 0 && nChk9 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 양

                                                #region 08 0 09 0
                                                else if (nChk8 == 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 0

                                                #region 08 양 09 0
                                                else if (nChk8 > 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 0

                                                #region 08 음 09 0
                                                else if (nChk8 < 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 0

                                                #region 08 0 09 양
                                                else if (nChk8 == 0 && nChk9 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 양

                                                #region 08 0 09 음
                                                else if (nChk8 == 0 && nChk9 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 음
                                            }
                                        }
                                        #endregion 07 음

                                        #region 07 양
                                        else if (n07 > 0 && string.Equals(e.Cell.Column.Name, "7"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")) != "")
                                            {
                                                double nChk8 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")));
                                                double nChk9 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")));

                                                #region 08 음 09 음
                                                if (nChk8 < 0 && nChk9 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 음

                                                #region 08 양 09 움
                                                else if (nChk8 > 0 && nChk9 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 음

                                                #region 08 양 09 양
                                                else if (nChk8 > 0 && nChk9 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 양

                                                #region 08 음 09 양
                                                else if (nChk8 < 0 && nChk9 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 양

                                                #region 08 0 09 0
                                                else if (nChk8 == 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 0

                                                #region 08 양 09 0
                                                else if (nChk8 > 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 0

                                                #region 08 음 09 0
                                                else if (nChk8 < 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 0

                                                #region 08 0 09 양
                                                else if (nChk8 == 0 && nChk9 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 양

                                                #region 08 0 09 음
                                                else if (nChk8 == 0 && nChk9 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 음
                                            }
                                            else
                                            {
                                                double nChk8 = 0;
                                                double nChk9 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")) != "")
                                                {
                                                    nChk8 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")) != "")
                                                {
                                                    nChk9 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")));
                                                }

                                                #region 08 음 09 음
                                                if (nChk8 < 0 && nChk9 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 음

                                                #region 08 양 09 움
                                                else if (nChk8 > 0 && nChk9 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 음

                                                #region 08 양 09 양
                                                else if (nChk8 > 0 && nChk9 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 양

                                                #region 08 음 09 양
                                                else if (nChk8 < 0 && nChk9 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 양

                                                #region 08 0 09 0
                                                else if (nChk8 == 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 0

                                                #region 08 양 09 0
                                                else if (nChk8 > 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 0

                                                #region 08 음 09 0
                                                else if (nChk8 < 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 0

                                                #region 08 0 09 양
                                                else if (nChk8 == 0 && nChk9 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 양

                                                #region 08 0 09 음
                                                else if (nChk8 == 0 && nChk9 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 음
                                            }
                                        }
                                        #endregion 07 양
                                    }
                                    #endregion 07
                                }
                            }
                        }
                    } //dgStock
                }
            }
        }

        private void GetOpenTime(string areaid)
        {
            string time = "070000";

            if (!areaid.Equals(""))
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("AREAID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["AREAID"] = areaid;
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREAATTR_FOR_OPENTIME", "RQSTDT", "RSLTDT", dt);

                if (dtResult.Rows.Count != 0)
                {
                    time = Convert.ToString(dtResult.Rows[0]["OPEN_TIME"]);
                }

                shop_open_time = DateTime.Parse(DateTime.Now.ToShortDateString() + " " + time.Substring(0, 2) + ":" + time.Substring(2, 2) + ":" + time.Substring(4, 2));
            }
        }

        public static T FindChild<T>(DependencyObject parent, string childName)
           where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        private void end_Rotation()
        {
            if (_timer_View_Rotation != null)
            {
                _timer_View_Rotation.Stop();
                _timer_View_Rotation.Dispose();
                _timer_View_Rotation = null;
            }
        }

        private void reSet()
        {
            if (dtDiv != null && dtDiv.Length != 0)
            {
                dtDiv = null;
            }

            timers_dispose();

            //if (dgInput.Dispatcher.CheckAccess())
            //{
            //    dgInput.ItemsSource = null;
            //}
            //else
            //{
            //    dgInput.Dispatcher.BeginInvoke(new Action(() => dgInput.ItemsSource = null));
            //}

            //if (dgStock.Dispatcher.CheckAccess())
            //{
            //    dgStock.ItemsSource = null;
            //}
            //else
            //{
            //    dgStock.Dispatcher.BeginInvoke(new Action(() => dgStock.ItemsSource = null));
            //}

            if (dgInput2.Dispatcher.CheckAccess())
            {
                dgInput2.ItemsSource = null;
            }
            else
            {
                dgInput2.Dispatcher.BeginInvoke(new Action(() => dgInput2.ItemsSource = null));
            }

            if (dgStock2.Dispatcher.CheckAccess())
            {
                dgStock2.ItemsSource = null;
            }
            else
            {
                dgStock2.Dispatcher.BeginInvoke(new Action(() => dgStock2.ItemsSource = null));
            }

            realData();

            DataSearch_Start_Timer(); //data Search 타이머
        }

        private void setChart(int realViewCnt)
        {
            try
            {
                if (dgInput2.Dispatcher.CheckAccess())
                {
                    dgInput2.ItemsSource = DataTableConverter.Convert(dtDiv[realViewCnt]);
                }
                else
                {
                    dgInput2.Dispatcher.BeginInvoke(new Action(() => dgInput2.ItemsSource = DataTableConverter.Convert(dtDiv[realViewCnt])));
                }

                if (dgStock2.Dispatcher.CheckAccess())
                {
                    dgStock2.ItemsSource = DataTableConverter.Convert(dtDiv[realViewCnt]);
                }
                else
                {
                    dgStock2.Dispatcher.BeginInvoke(new Action(() => dgStock2.ItemsSource = DataTableConverter.Convert(dtDiv[realViewCnt])));
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }

        private void searchData()
        {
            try
            {
                DoEvents();

                DataSet ds = new DataSet();

                DataTable indata = new DataTable();
                indata.TableName = "INDATA";

                indata.Columns.Add("LANGID", typeof(string));
                indata.Columns.Add("SHOPID", typeof(string));
                indata.Columns.Add("AREAID", typeof(string));
                indata.Columns.Add("EQSGID", typeof(string));
                //indata.Columns.Add("YMD", typeof(string));
                //indata.Columns.Add("NOW", typeof(string));

                DataRow dr = indata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = selectedArea;
                dr["EQSGID"] = selectedEquipmentSegment;
                //dr["YMD"] = "";
                //dr["NOW"] = "";

                indata.Rows.Add(dr);

                ds.Tables.Add(indata);

                //dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_REPORT_REQ_CELL", "INDATA", "PLAN,INPUT,STOCK,INPUT2,STOCK2", ds, null);

                //if (dsResult != null)
                //{
                //    Util.GridSetData(dgInput2, dsResult.Tables["INPUT2"], FrameOperation, true);
                //    Util.GridSetData(dgStock2, dsResult.Tables["STOCK2"], FrameOperation, true);
                //}

                new ClientProxy().ExecuteService_Multi("BR_REPORT_REQ_CELL", "INDATA", "PLAN,INPUT,STOCK,INPUT2,STOCK2", (dsRslt, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            //Util.AlertByBiz("BR_REPORT_REQ_CELL", bizException.Message, bizException.ToString());
                            return;
                        }

                        Util.GridSetData(dgInput2, dsRslt.Tables["INPUT2"], FrameOperation, false);
                        Util.GridSetData(dgStock2, dsRslt.Tables["STOCK2"], FrameOperation, false);
                    }
                    catch (Exception ex)
                    {
                        ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + "\r\n" + ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        Logger.Instance.WriteLine(Logger.OPERATION_R + " : " + new System.Diagnostics.StackFrame(0, true).GetMethod().Name, ex);
                    }

                }, ds);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private void DoWait()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

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
    }
}
