/*************************************************************************************
 Created Date : 2019.05.30
      Creator : 손우석
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2019.06.11  손우석 CSR ID 4007450 [G.MES] Audi C-BEV 생산 모니터링 화면 (현황판) 수정 [요청번호]C20190531_07450
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

namespace LGC.GMES.MES.MNT001
{
    public partial class MNT001_014 : Window
    {
        #region Declaration & Constructor

        //System.Timers.Timer tmrMainTimer;

        private Storyboard sbExpandLeft;
        private Storyboard sbCollapseLeft;

        private static string selectedShop;
        private static string selectedArea;
        private static string selectedEquipmentSegment;
        //private static string selectedProcess;
        //private static string selectedElec;

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
        #endregion Declaration & Constructor

        #region Initialize
        public MNT001_014()
        {
            try
            {
                InitializeComponent();

                this.Loaded += UserControl_Loaded;
                dgMONITORING.LoadedCellPresenter += dgMONITORING_LoadedCellPresenter;
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
            dgMONITORING.Width = W;
            dgMONITORING.Height = H - 70;
        }

        private void InitSetting()
        {
            try
            {
                setTitle();

                //DataTable dt = new DataTable();
                //dt.Columns.Add("EQSGID", typeof(string));
                //dt.Columns.Add("GUBUN", typeof(string));

                //DataRow dr = dt.NewRow();
                //dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                //dr["GUBUN"] = "P";
                //dt.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENTSEGMENT_MNT", "RQSTDT", "RSLTDT", dt);

                //if (dtResult.Rows.Count == 0) //조립
                //{
                //    colProcess.Visibility = Visibility.Visible;
                //    cboProcess.Visibility = Visibility.Visible;
                //    cboElec.Visibility = Visibility.Visible;
                //    tbElec.Visibility = Visibility.Visible;
                //    tbProcess.Visibility = Visibility.Visible;
                //    colPrjtName.Visibility = Visibility.Visible;


                //}
                //else//PACK
                //{
                    colProcess.Visibility = Visibility.Collapsed;
                    //cboProcess.Visibility = Visibility.Collapsed;
                    //cboElec.Visibility = Visibility.Collapsed;
                    //tbElec.Visibility = Visibility.Collapsed;
                    //tbProcess.Visibility = Visibility.Collapsed;
                    colPrjtName.Visibility = Visibility.Collapsed;
                //}

                //dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENTSEGMENT_MNT", "RQSTDT", "RSLTDT", dt);
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
                    tbTitle.Text = ObjectDic.Instance.GetObjectName("일별 공정진척 모니터링") + " ( " + (shop_open_time >= DateTime.Now ? (DateTime.Now.AddDays(-1).Month + "/" + DateTime.Now.AddDays(-1).Day) : (DateTime.Now.Month + "/" + DateTime.Now.Day)) + " ) ";
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

                //Clear_Display_Control();

                DataSet ds = LGC.GMES.MES.MNT001.MNT_Common.GetConfigXML_PACK();

                selectedShop = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["SHOP"]);
                selectedArea = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["AREA"]);
                selectedEquipmentSegment = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["EQUIPMENTSEGMENT"]);
                //selectedProcess = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["PROCESS"]);
                //selectedElec = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["ELEC_TYPE"]);

                seletedViewRowCnt = Convert.ToInt32(ds.Tables["MNT_CONFIG"].Rows[0]["DISPLAYVIEWROWCNT"]);
                selectedDisplayTime = Convert.ToInt32(ds.Tables["MNT_CONFIG"].Rows[0]["DISPLAYTIME"]);
                selectedDisplayTimeSub = ds.Tables["MNT_CONFIG"].Columns["DISPLAYTIMESUB"] == null ? selectedDisplayTimeSub : Convert.ToInt32(ds.Tables["MNT_CONFIG"].Rows[0]["DISPLAYTIMESUB"]);
                //selectedColorDisplayFlag = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["COLOR_DISP_FLAG"]);

                cboShop.SelectedValue = selectedShop;
                cboArea.SelectedValue = selectedArea;

                //string[] splitElec = selectedElec.Split(',');
                //foreach (string str in splitElec)
                //{
                //    cboElec.Check(str);
                //}

                numViewRowCnt.Value = seletedViewRowCnt;
                numRefresh.Value = selectedDisplayTime;
                numRefreshSub.Value = selectedDisplayTimeSub;
                //cboColor.SelectedValue = selectedColorDisplayFlag;

                realData(); //data 가져와서 그리드에 바인딩 시켜준 후 화면전환 타이머 작동

                DataSearch_Start_Timer(); //data Search 타이머
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

            //SetProcess(cboProcess);

            //SetElec(cboElec);

            //SetColor(cboColor);
        }

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
            //try
            //{
            //    DataTable RQSTDT = new DataTable();
            //    RQSTDT.TableName = "RQSTDT";
            //    RQSTDT.Columns.Add("LANGID", typeof(string));
            //    RQSTDT.Columns.Add("EQSGID", typeof(string));

            //    DataRow dr = RQSTDT.NewRow();
            //    dr["LANGID"] = LoginInfo.LANGID;
            //    dr["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedItemsToString);
            //    RQSTDT.Rows.Add(dr);

            //    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO_MNT", "RQSTDT", "RSLTDT", RQSTDT);

            //    cboMulti.DisplayMemberPath = "CBO_NAME";
            //    cboMulti.SelectedValuePath = "CBO_CODE";

            //    cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private void SetElec(MultiSelectionBox cbo)
        {
            //try
            //{
            //    DataTable dtRQSTDT = new DataTable();
            //    dtRQSTDT.TableName = "RQSTDT";

            //    dtRQSTDT.Columns.Add("LANGID", typeof(string));

            //    DataRow drnewrow = dtRQSTDT.NewRow();
            //    drnewrow["LANGID"] = LoginInfo.LANGID;
            //    dtRQSTDT.Rows.Add(drnewrow);

            //    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_CODE", "RQSTDT", "RSLTDT", dtRQSTDT);

            //    cbo.DisplayMemberPath = "ELECNAME";
            //    cbo.SelectedValuePath = "ELECCODE";

            //    cbo.ItemsSource = DataTableConverter.Convert(dtResult);

            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private void SetColor(C1ComboBox cbo)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_CODE", typeof(string));
                dt.Columns.Add("CBO_NAME", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CBO_CODE"] = "Y";
                dr["CBO_NAME"] = "Y";
                dt.Rows.Add(dr);

                DataRow dr2 = dt.NewRow();
                dr2["CBO_CODE"] = "N";
                dr2["CBO_NAME"] = "N";
                dt.Rows.Add(dr2);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cbo.ItemsSource = dt.Copy().AsDataView();
                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void realData()
        {
            try
            {
                setTitle();

                end_Rotation(); //화면전환 timer 종료

                searchData();

                if (dtSearchTable == null || dtSearchTable.Rows.Count == 0)
                {
                    return;
                }

                bind();

                if (chkLock.Dispatcher.CheckAccess())
                {
                    if (!chkLock.IsChecked ?? false)
                    {
                        RotationView_Start_Timer();
                    }
                }
                else
                {
                    chkLock.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        if (!chkLock.IsChecked ?? false)
                        {
                            RotationView_Start_Timer();
                        }
                    }));
                }
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

        public static string GetMultiSelectionBox_Thread(object oCondition, string sMsg = "", bool bAllNull = false)
        {
            string sRet = "";

            switch (oCondition.GetType().Name)
            {
                case "MultiSelectionBox":
                    MultiSelectionBox cb = oCondition as MultiSelectionBox;

                    cb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        sRet = cb.SelectedItemsToString;
                    }));
                    break;
            }
            return sRet;
        }

        private void bind()
        {

            int row_idx = 0;

            dtSortTable = dtSearchTable.Select("", "EQSGNAME asc").CopyToDataTable<DataRow>();
            dtSortTable.AcceptChanges();

            if (dgMONITORING.Dispatcher.CheckAccess())
            {
                dgMONITORING.ItemsSource = null;
            }
            else
            {
                dgMONITORING.Dispatcher.BeginInvoke(new Action(() => dgMONITORING.ItemsSource = null));
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
                //viewrow 4
                //searchrow 18

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

                if (chkLock.Dispatcher.CheckAccess())
                {
                    if (chkLock.IsChecked ?? false)
                    {
                        if (realViewCnt <= 0)
                        {
                            setChart(div - 1);
                            realViewCnt = div;
                        }
                        else
                        {                                       // realViewCnt를 화면에 먼저 반영한 뒤 +1함
                            setChart(realViewCnt - 1);  // 그래서 realViewCnt-1 이 현재 보여지고 있는 페이지
                        }
                    }
                    else
                    {
                        setChart(realViewCnt);          // realViewCnt 는 다음 rotationViewPlay 시 보여질 페이지
                        realViewCnt++;
                    }
                }
                else
                {
                    chkLock.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        if (chkLock.IsChecked ?? false)
                        {
                            if (realViewCnt <= 0)
                            {
                                setChart(div - 1);
                                realViewCnt = div;
                            }
                            else
                            {
                                setChart(realViewCnt - 1);
                            }
                        }
                        else
                        {
                            setChart(realViewCnt);
                            realViewCnt++;
                        }
                    }));
                }
            }

            string msg = realViewCnt + " / " + div;
            SetUiCondition(tbPage, msg);
            SetUiCondition(tbRefreshTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        void dgMONITORING_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg.TopRows.Count > 0)
            {
                DataGridRow[] _headerColumnRows = dg.TopRows.Take(dg.TopRows.Count).ToArray();

                var nonHeadersViewportRows = dg.Viewport.Rows.Where(r => !_headerColumnRows.Contains(r)).ToArray();

                // merge column & rows headers
                foreach (var range in Merge(System.Windows.Controls.Orientation.Horizontal, _headerColumnRows, dg.Columns.ToArray(), true))
                {
                    e.Merge(range);
                }
            }

            dataCellMerge(dg, e);

            dgMONITORING.MergingCells -= dgMONITORING_MergingCells;
        }

        private void dataCellMerge(C1DataGrid dataGrid, DataGridMergingCellsEventArgs e)
        {
            int leftCol = -1;
            DataGridMergeMode mode = DataGridMergeMode.NONE;
            foreach (DataGridColumn column in dataGrid.Columns)
            {
                if (leftCol == -1)
                {
                    if (!DataGridMergeExtension.GetMergeMode(column).Equals(DataGridMergeMode.NONE))
                    {
                        leftCol = column.Index;
                        mode = DataGridMergeExtension.GetMergeMode(column);
                    }
                }
                else if (mode.Equals(DataGridMergeExtension.GetMergeMode(column)) && mode != DataGridMergeMode.VERTICAL)
                {
                    continue;
                }
                else
                {
                    if (mode != DataGridMergeMode.NONE)
                    {
                        System.Windows.Controls.Orientation orientation = (mode == DataGridMergeMode.HORIZONTAL) || (mode == DataGridMergeMode.HORIZONTALHIERARCHI) ? System.Windows.Controls.Orientation.Horizontal : System.Windows.Controls.Orientation.Vertical;
                        bool hierarchi = (mode == DataGridMergeMode.HORIZONTALHIERARCHI) || (mode == DataGridMergeMode.VERTICALHIERARCHI) ? true : false;
                        DataGridRow[] rows = null;
                        if (dataGrid.TopRows.Count > 0)
                        {
                            DataGridRow[] _headerColumnRows = dataGrid.TopRows.Take(dataGrid.TopRows.Count).ToArray();
                            rows = dataGrid.Viewport.Rows.Where(r => !_headerColumnRows.Contains(r)).ToArray();
                        }
                        else
                        {
                            rows = dataGrid.Viewport.Rows.ToArray();
                        }
                        DataGridColumn[] columns = dataGrid.Columns.Where(col => col.Index >= leftCol && col.Index < column.Index).ToArray();
                        List<DataGridCellsRange> rangeList = Merge(orientation, rows, columns, hierarchi);
                        foreach (DataGridCellsRange range in rangeList)
                        {
                            e.Merge(range);
                        }
                    }

                    if (!DataGridMergeExtension.GetMergeMode(column).Equals(DataGridMergeMode.NONE))
                    {
                        leftCol = column.Index;
                        mode = DataGridMergeExtension.GetMergeMode(column);
                    }
                    else
                    {
                        leftCol = -1;
                        mode = DataGridMergeMode.NONE;
                    }
                }
            }

            if (leftCol != -1 && mode != DataGridMergeMode.NONE)
            {
                System.Windows.Controls.Orientation orientation = (mode == DataGridMergeMode.HORIZONTAL) || (mode == DataGridMergeMode.HORIZONTALHIERARCHI) ? System.Windows.Controls.Orientation.Horizontal : System.Windows.Controls.Orientation.Vertical;
                bool hierarchi = (mode == DataGridMergeMode.HORIZONTALHIERARCHI) || (mode == DataGridMergeMode.VERTICALHIERARCHI) ? true : false;
                DataGridRow[] rows = null;
                if (dataGrid.TopRows.Count > 0)
                {
                    DataGridRow[] _headerColumnRows = dataGrid.TopRows.Take(dataGrid.TopRows.Count).ToArray();
                    rows = dataGrid.Viewport.Rows.Where(r => !_headerColumnRows.Contains(r)).ToArray();
                }
                else
                {
                    rows = dataGrid.Viewport.Rows.ToArray();
                }
                DataGridColumn[] columns = dataGrid.Columns.Where(col => col.Index >= leftCol).ToArray();
                List<DataGridCellsRange> rangeList = Merge(orientation, rows, columns, hierarchi);
                foreach (DataGridCellsRange range in rangeList)
                {
                    e.Merge(range);
                }
            }
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

        #endregion

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

        private void dgMONITORING_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

        #region Check
        private void chkLock_Checked(object sender, RoutedEventArgs e)
        {
            end_Rotation();
        }

        private void chkLock_Unchecked(object sender, RoutedEventArgs e)
        {
            RotationView_Start_Timer();
        }
        #endregion Check

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

            realData();

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

        #endregion Button

        #region Combo
        private void cboShop_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboShop.SelectedIndex > -1)
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
                if (cboArea.SelectedIndex > -1)
                {
                    SetLine(cboEquipmentSegment);
                    GetOpenTime(Convert.ToString(cboArea.SelectedValue));

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
                    selectedArea = string.Empty;
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

            if (dgMONITORING.GetCell(e.Cell.Row.Index, 1).Value.Equals("F") || dgMONITORING.GetCell(e.Cell.Row.Index, 1).Value.Equals("S"))
            {
                if (e.Cell.Column.Index != 0 && e.Cell.Column.Index != dataGrid.Columns.Count - 1)
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                }
            }

            if (e.Cell.Column.Name.Equals("MEET_RATE") || e.Cell.Column.Name.Equals("MEET_RATE_YES"))
            {
                if (e.Cell.Row.DataItem != null)
                {
                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)).Equals(""))
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)).Equals("-"))
                        {
                            if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)).Substring(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)).Length - 1, 1).Equals("%"))
                            {
                                DataTableConverter.SetValue(e.Cell.Row.DataItem, e.Cell.Column.Name, (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)) + "%"));
                            }
                        }
                    }
                }
            }

            if (e.Cell.Column.Name.Equals("MEET_RATE_BYTIME") || e.Cell.Column.Name.Equals("MEET_RATE_YES"))
            {

                if (e.Cell.Row.DataItem != null)
                {
                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)).Equals(""))
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)).Equals("-"))
                        {
                            if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)).Substring(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)).Length - 1, 1).Equals("%"))
                            {
                                DataTableConverter.SetValue(e.Cell.Row.DataItem, e.Cell.Column.Name, (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)) + "%"));
                            }

                            //if (cboColor.SelectedValue == null || cboColor.SelectedValue.ToString().Equals("N"))
                            //{
                            //    return;
                            //}

                            double meet_rate = Double.Parse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)).Substring(0, Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)).Length - 2));

                            if (meet_rate >= 0 && meet_rate < 90)
                            {
                                System.Drawing.Color color = GridBackColor.Red;
                                e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));

                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);

                            }
                            else if (meet_rate >= 90 && meet_rate < 100)
                            {
                                System.Drawing.Color color = GridBackColor.Yellow;
                                e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            }
                            else
                            {
                                System.Drawing.Color color = GridBackColor.Green;
                                e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));

                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            }
                        }
                    }

                }
            }

            if (e.Cell.Column.Name.Equals("PLAN_QTY") || e.Cell.Column.Name.Equals("RESULT_QTY"))
            {
                if (e.Cell.Row.DataItem != null)
                {
                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESULT_QTY")).Equals(""))
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESULT_QTY")).Equals("-"))
                        {
                            double result_qty = Double.Parse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESULT_QTY")));
                            double plan_qty = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PLAN_QTY")).Equals("-") ? 0 : Double.Parse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PLAN_QTY")));

                            if (result_qty <= 0)
                            {
                                DataTableConverter.SetValue(e.Cell.Row.DataItem, "RESULT_QTY", "-");
                            }

                            if (plan_qty <= 0)
                            {
                                DataTableConverter.SetValue(e.Cell.Row.DataItem, "PLAN_QTY", "-");
                            }
                        }
                    }
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

            if (dgMONITORING.Dispatcher.CheckAccess())
            {
                dgMONITORING.ItemsSource = null;
            }
            else
            {
                dgMONITORING.Dispatcher.BeginInvoke(new Action(() => dgMONITORING.ItemsSource = null));
            }

            realData();

            DataSearch_Start_Timer(); //data Search 타이머
        }

        public static class GridBackColor
        {
            public static readonly System.Drawing.Color Green = System.Drawing.Color.FromArgb(0, 255, 0);
            public static readonly System.Drawing.Color Yellow = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color Red = System.Drawing.Color.FromArgb(255, 0, 0);

            public static readonly System.Drawing.Color R = System.Drawing.Color.FromArgb(0, 255, 0);
            public static readonly System.Drawing.Color W = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color T = System.Drawing.Color.FromArgb(255, 0, 0);
            public static readonly System.Drawing.Color F = System.Drawing.Color.FromArgb(0, 0, 0);
            public static readonly System.Drawing.Color N = System.Drawing.Color.FromArgb(255, 255, 255);
            public static readonly System.Drawing.Color U = System.Drawing.Color.FromArgb(128, 128, 128);

            public static readonly System.Drawing.Color I = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color P = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color O = System.Drawing.Color.FromArgb(0, 0, 0);
        }

        private void setChart(int realViewCnt)
        {
            try
            {
                if (dgMONITORING.Dispatcher.CheckAccess())
                {
                    dgMONITORING.ItemsSource = DataTableConverter.Convert(dtDiv[realViewCnt]);
                }
                else
                {
                    dgMONITORING.Dispatcher.BeginInvoke(new Action(() => dgMONITORING.ItemsSource = DataTableConverter.Convert(dtDiv[realViewCnt])));
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
                DataSet ds = new DataSet();

                DataTable indata = new DataTable();
                indata.TableName = "INDATA";

                indata.Columns.Add("YYYYMMDD", typeof(string));
                indata.Columns.Add("AREAID", typeof(string));
                indata.Columns.Add("EQSGID", typeof(string));
                indata.Columns.Add("LANGID", typeof(string));
                
                DataRow dr = indata.NewRow();
                dr["YYYYMMDD"] = DateTime.Now >= shop_open_time ? DateTime.Now.ToString("yyyyMMdd") : DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                dr["AREAID"] = selectedArea;
                dr["EQSGID"] = selectedEquipmentSegment;
                dr["LANGID"] = LoginInfo.LANGID;

                indata.Rows.Add(dr);

                ds.Tables.Add(indata);

                DataSet resultSet = new ClientProxy().ExecuteServiceSync_Multi("DA_PRD_SEL_CUST_BOARD_SUMMARY", "INDATA", "OUTDATA", ds);

                if (resultSet != null)
                {
                    dtSearchTable = resultSet.Tables["OUTDATA"];

                }
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
    }
}
