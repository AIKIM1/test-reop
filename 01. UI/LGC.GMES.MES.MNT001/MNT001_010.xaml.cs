/*************************************************************************************
 Created Date : 2018.05.01
      Creator : 최승혁
   Decription : 재공현황 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2018.04.30  DEVELOPER : Initial Created.
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
    public partial class MNT001_010 : Window
    {
        #region Declaration & Constructor 

        System.Timers.Timer tmrMainTimer;

        private Storyboard sbExpandLeft;
        private Storyboard sbCollapseLeft;

        private static string selectedShop;
        private static string selectedArea;
        private static string selectedEquipmentSegment;
        private static string selectedProcess;
        private static string selectedElec;

        private static string selectedDisplayName;
        private static int selectedDisplayTime = 1;
        private static int selectedDisplayTimeSub = 10;
        private static int seletedViewRowCnt = 7;

        private static DateTime dtMainStartTime;
        private static DateTime dtSubStartTime;

        private int subTime = 10;

        DataTable dtMain = new DataTable();

        private int currentPage = 0;

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
        

        public MNT001_010()
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

        //private void applyObejctDicToDataGrid(C1DataGrid dataGrid)
        //{
        //    foreach (DataGridColumn column in dataGrid.Columns)
        //    {
        //        applyObejctDicToColumn(column);
        //    }
        //}

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
            double h_temp = (H - 350);  //전체 높이에서 고정된 Grid의 높이 뺌

            seletedViewRowCnt = Convert.ToInt32(numViewRowCnt.Value);
            oneRow_width = Convert.ToInt32(Math.Round(h_temp / seletedViewRowCnt)); // 반올림 : 고정되지 않은 GRID 수로 나눔

            var rowh = new C1.WPF.DataGrid.DataGridRow();
            rowh.Height = new C1.WPF.DataGrid.DataGridLength(oneRow_width);


            dgMONITORING.RowHeight = rowh.Height;

            fullSize.Width = new GridLength(W);
            dgMONITORING.Width = W;
            dgMONITORING.Height = H - 70;
        }

        private void InitSetting()
        {
            try
            {
                setTitle();

                DataTable dt = new DataTable();
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("GUBUN", typeof(string));

                DataRow dr = dt.NewRow();
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["GUBUN"] = "P";
                dt.Rows.Add(dr);
                //수정필요
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENTSEGMENT_MNT", "RQSTDT", "RSLTDT", dt);

                if (dtResult.Rows.Count == 0) //조립
                {
                    colProcess.Visibility = Visibility.Visible;
                    cboProcess.Visibility = Visibility.Visible;
                    tbProcess.Visibility = Visibility.Visible; 
                         

                }
                else//PACK
                {
                    colProcess.Visibility = Visibility.Collapsed;
                    cboProcess.Visibility = Visibility.Collapsed;
                    tbProcess.Visibility = Visibility.Collapsed; 
                }
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


        #endregion

        #region Initialize

        private void setTitle()
        {

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrWhiteSpace(selectedDisplayName) == true)
                {

                    tbTitle.Text = ObjectDic.Instance.GetObjectName("재공현황") + " ( " + DateTime.Now.Month + "/" + DateTime.Now.Day + " ) " ;
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

                DataSet ds = LGC.GMES.MES.MNT001.MNT_Common.GetConfigXML_WIPSTAT();

                selectedShop = Convert.ToString(ds.Tables["MNT_CONFIG_WIPSTAT"].Rows[0]["SHOP"]);
                selectedArea = Convert.ToString(ds.Tables["MNT_CONFIG_WIPSTAT"].Rows[0]["AREA"]);
                selectedProcess = Convert.ToString(ds.Tables["MNT_CONFIG_WIPSTAT"].Rows[0]["PROCESS"]);
                selectedDisplayTime = Convert.ToInt32(ds.Tables["MNT_CONFIG_WIPSTAT"].Rows[0]["DISPLAYTIME"]);                
                seletedViewRowCnt = Convert.ToInt32(ds.Tables["MNT_CONFIG_WIPSTAT"].Rows[0]["VIEWROWCNT"]);
               
                if (ds.Tables["MNT_CONFIG_WIPSTAT"].Columns["DISPLAYTIMESUB"] == null)
                {
                    selectedDisplayTimeSub = 3;
                }
                else
                {
                    selectedDisplayTimeSub = Convert.ToInt32(ds.Tables["MNT_CONFIG_WIPSTAT"].Rows[0]["DISPLAYTIMESUB"]);
                }

                cboShop.SelectedValue = selectedShop;
                cboArea.SelectedValue = selectedArea;

                numRefresh.Value = selectedDisplayTime;
                numRefreshSub.Value = selectedDisplayTimeSub;
                numViewRowCnt.Value = seletedViewRowCnt;


                string[] split = selectedProcess.Split(',');
                foreach (string str in split)
                {
                    cboProcess.Check(str);
                }
               

                            


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
            

            SetProcess(cboProcess);
            
            
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
               
                //DataRow dr1 = dtResult.NewRow();
               
                //dr1["CBO_NAME"] = " - ALL-";
                //dr1["CBO_CODE"] = "";
                //dtResult.Rows.InsertAt(dr1, 0);

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
                //Util.Alert(ex.Message);
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
            catch(Exception ex)
            {
                //Util.Alert(ex.Message);
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

                //if (!LoginInfo.CFG_EQSG_ID.Equals(""))
                //{
                //    cbo.SelectedValue = LoginInfo.CFG_EQSG_ID;
                //    if (cbo.SelectedIndex < 0)
                //    {
                //        cbo.SelectedIndex = 0;
                //    }
                //}
                //else
                //{
                //    cbo.SelectedIndex = 0;
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetProcess(MultiSelectionBox cboMulti)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO_MNT_WIPSTAT", "RQSTDT", "RSLTDT", RQSTDT);

                cboMulti.DisplayMemberPath = "CBO_NAME";
                cboMulti.SelectedValuePath = "CBO_CODE";

                cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);
                    
                if(first_access && selectedProcess != null)
                {
                    string[] split = selectedProcess.Split(',');
                    foreach (string str in split)
                    {
                        cboProcess.Check(str);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

           
        }

        private void SetElec(MultiSelectionBox cbo)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";

            dtRQSTDT.Columns.Add("LANGID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_PRD_SEL_ELEC_CODE", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    Util.MessageException(Exception);
                    return;
                }

                try
                {
                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if (result.Rows.Count == 0)
                    {
                        selectedElec = string.Empty;
                    }
                    //  cbo.SelectedIndex = 0;
                    
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + "\r\n" + ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + " : " + new System.Diagnostics.StackFrame(0, true).GetMethod().Name, ex);
                }
              
            });
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

                RotationView_Start_Timer();


                

            }
            catch(Exception ex)
            {
                throw ex;
            }           
        }


        private void searchData()
        { 
            try
            {
                DataSet ds = new DataSet();

                DataTable indata = new DataTable();
                indata.TableName = "INDATA";

                indata.Columns.Add("LANGID", typeof(string));
                indata.Columns.Add("AREAID", typeof(string)); 
                indata.Columns.Add("PROCID", typeof(string)); 

                DataRow dr = indata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = selectedArea; 
                dr["PROCID"] = selectedProcess; 

                indata.Rows.Add(dr);

                ds.Tables.Add(indata);

                DataSet resultSet = new ClientProxy().ExecuteServiceSync_Multi("DA_PRD_SEL_STOCK_IN_AREA_TO_MNT", "INDATA", "OUTDATA", ds);

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

            
            dtSortTable = dtSearchTable.Select("").CopyToDataTable<DataRow>();

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
                if(bindTableRowCnt % viewRowCnt ==0)
                {
                    div =(bindTableRowCnt / viewRowCnt);
                }
                else
                {
                    double temp = Math.Floor((Convert.ToDouble(bindTableRowCnt) / Convert.ToDouble(viewRowCnt)));
                    div = Convert.ToInt32(temp) + 1;
                }
            }

            if(div != 0) // ex) 화면수 1개 - dtDiv[0] / 화면수 2개 - dtDiv[0],dtDiv[1] ..... : 두번째 화면은 rotation timer 돌면 보여진다.
            {
                dtDiv = null;

                dtDiv = new DataTable[div]; //table 5개

                int start_idx = 0;
                int end_idx = viewRowCnt;
                //viewrow 4
                //searchrow 18

                for (int i = 0; i< dtDiv.Length; i++) //5번 roof
                {
                    dtDiv[i] = dtBind.Clone();
                 
                    for (int j = start_idx; j < end_idx; j++) //4
                    {
                        if(j < dtBind.Rows.Count)
                        {
                            dtDiv[i].ImportRow(dtBind.Rows[j]);
                        }
                    }

                    start_idx = end_idx;
                    end_idx = end_idx + viewRowCnt;                    
                }


                if (dgMONITORING.Dispatcher.CheckAccess())
                {
                    //dgMONITORING.ItemsSource = DataTableConverter.Convert(dtDiv[0]);
                    Util.GridSetData(dgMONITORING, dtDiv[0], FrameOperation, false);
                }
                else
                {
                   // dgMONITORING.Dispatcher.BeginInvoke(new Action(() => dgMONITORING.ItemsSource = DataTableConverter.Convert(dtDiv[0])));
                    dgMONITORING.Dispatcher.BeginInvoke(new Action(() => Util.GridSetData(dgMONITORING, dtDiv[0], FrameOperation, false)));
                }

                realViewCnt = 1; //첫 화면 먼저 뿌리므로 화면번호는 1임.
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

            _timer_View_Rotation= new System.Timers.Timer(refresh);
            _timer_View_Rotation.AutoReset = true;
            _timer_View_Rotation.Elapsed += (s, arg) =>
            {
                if(_timer_View_Rotation !=null)
                {
                    RotationViewPlay();
                }
            };
            _timer_View_Rotation.Start();
         
        }

        private void RotationViewPlay()
        {
            try
            {
                if(first_access)
                {
                    reSet();

                    first_access = false;

                    return;
                }

                if(div ==1)
                {
                    return;
                }

                int nowviewPoint = 0;

                if(realViewCnt < div)
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

                //dgMONITORING.ItemsSource = DataTableConverter.Convert(dtDiv[realViewCnt]);

                if (dgMONITORING.Dispatcher.CheckAccess())
                {
                    // dgMONITORING.ItemsSource = DataTableConverter.Convert(dtDiv[nowviewPoint]);
                    Util.GridSetData(dgMONITORING, dtDiv[nowviewPoint], FrameOperation, false);
                }
                else
                {

                  //  Action act = () => { dgMONITORING.ItemsSource = DataTableConverter.Convert(dtDiv[nowviewPoint]); };
                    Action act = () => { Util.GridSetData(dgMONITORING, dtDiv[nowviewPoint], FrameOperation, false); };
                    
                    dgMONITORING.Dispatcher.BeginInvoke(act);
                }

                //dgMONITORING.Dispatcher.BeginInvoke(new Action(() => dgMONITORING.ItemsSource = DataTableConverter.Convert(dtDiv[nowviewPoint])));

                realViewCnt++;

                string msg = realViewCnt + " / " + div;

                SetUiCondition(tbPage, msg);
                SetUiCondition(tbRefreshTime, DateTime.Now.ToString());
            }
            catch(Exception ex)
            {
                Util.Alert(ex.Message);
            }
           
        }

        #endregion

        #region Event       

       

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

                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.White);
                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.White);
                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.White);
                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.White);
                    // dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.White);
                    // dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.White);


                    if (e.Cell.Column.Name.Equals("MODLID"))
                    {
                        if (e.Cell.Row.DataItem != null)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MODLID")).Equals("TOTAL"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            }
                            
                        }
                    } 

                }
                else //UI가 아닌 다른 Thread 호출시
                {
                    Action act = () =>
                    {
                        if (e.Cell.Presenter == null)
                        {
                            return;
                        }

                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.White);



                        if (e.Cell.Row.DataItem != null && (DataTableConverter.GetValue(e.Cell.Row.DataItem, "MODLID")).Equals("TOTAL"))
                        {
                            if (!e.Cell.Column.Name.Equals("PROCNAME"))
                            {

                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            }

                        }

                        /*if (e.Cell.Column.Name.Equals("MODLID"))
                        {
                            if (e.Cell.Row.DataItem != null)
                            {
                                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MODLID")).Equals("TOTAL"))
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                }

                            }
                        }*/




                    };
                   
                    dataGrid.Dispatcher.BeginInvoke(act);
                }

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void setData()
        {
            selectedShop = Convert.ToString(cboShop.SelectedValue);
            selectedArea = Convert.ToString(cboArea.SelectedValue);
            selectedProcess = Convert.ToString(cboProcess.SelectedItemsToString);
            seletedViewRowCnt = Convert.ToInt32(numViewRowCnt.Value);
            selectedDisplayTime = Convert.ToInt32(numRefresh.Value);
            selectedDisplayTimeSub = Convert.ToInt32(numRefreshSub.Value);
        }



        private void btnSetSave_Click(object sender, RoutedEventArgs e)
        {

            selectedShop = Convert.ToString(cboShop.SelectedValue);
            selectedArea = Convert.ToString(cboArea.SelectedValue); 
            selectedProcess = Convert.ToString(cboProcess.SelectedItemsToString);  
            seletedViewRowCnt = Convert.ToInt32(numViewRowCnt.Value);
            selectedDisplayTime = Convert.ToInt32(numRefresh.Value);
            selectedDisplayTimeSub = Convert.ToInt32(numRefreshSub.Value); //

            DataSet ds = new DataSet();

            DataTable dt = new DataTable("MNT_CONFIG_WIPSTAT");
            dt.Columns.Add("SHOP", typeof(string));
            dt.Columns.Add("AREA", typeof(string));
            dt.Columns.Add("PROCESS", typeof(string));
            dt.Columns.Add("VIEWROWCNT", typeof(int));
            dt.Columns.Add("DISPLAYTIME", typeof(int));
            dt.Columns.Add("DISPLAYTIMESUB", typeof(int)); 


            DataRow dr = dt.NewRow();
            dr["SHOP"] = selectedShop;
            dr["AREA"] = selectedArea;
            dr["PROCESS"] = selectedProcess;
            dr["VIEWROWCNT"] = seletedViewRowCnt;
            dr["DISPLAYTIME"] = selectedDisplayTime;
            dr["DISPLAYTIMESUB"] = selectedDisplayTimeSub; 

            dt.Rows.Add(dr);
            ds.Tables.Add(dt);

            LGC.GMES.MES.MNT001.MNT_Common.SetConfigXML_WIPSTAT(ds);

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
            InitColumnWidth(); 
            reSet();
            btnLeftFrame.IsChecked = false;
        }

        private void btnSetClose_Click(object sender, RoutedEventArgs e)
        {
            btnLeftFrame.IsChecked = false;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            //LGC.GMES.MES.MainFrame.MonitoringWindow mnt = this.Parent as LGC.GMES.MES.MainFrame.MonitoringWindow;
            //mnt.Close();
        }

        private void btnLeftFrame_Checked(object sender, RoutedEventArgs e)
        {
            sbExpandLeft.Begin();
        }

        private void btnLeftFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            sbCollapseLeft.Begin();
        }

        //Combo ==============================================================================================

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
                    SetProcess(cboProcess);
                }
                else
                {
                    selectedArea = string.Empty;
                }
            }));
        }

        

       

        #endregion

        #region Mehod


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

        #endregion

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

        #endregion

        private void numRefresh_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
           // reSet();            
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

        private void btnSetData_Click(object sender, RoutedEventArgs e)
        {
            setData();
            InitColumnWidth();
            reSet();
        }

        private void reSet()
        {
            if(dtDiv != null && dtDiv.Length != 0 )
            {
                dtDiv = null;
            }

            timers_dispose();

            if(dgMONITORING.Dispatcher.CheckAccess())
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
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

    }
}
