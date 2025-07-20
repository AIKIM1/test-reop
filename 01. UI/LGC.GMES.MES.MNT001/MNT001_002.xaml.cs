/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2018.01.29  손우석 다국어 처리
  2018.02.18  김도형 CSR ID:3894540] 자동차 Pack 생산현황에 Prod. Type(W/O Type) 표기 요청 件 | [요청번호]C20190114_94540 
  2019.02.20  손우석 CSR ID:3894540] 자동차 Pack 생산현황에 Prod. Type(W/O Type) 표기 요청 件 | [요청번호]C20190114_94540
  2019.02.27  손우석 CSR ID:3894540] 자동차 Pack 생산현황에 Prod. Type(W/O Type) 표기 요청 件 | [요청번호]C20190114_94540 - Pack 상무님 요청으로 Fpnt Size 조정
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
    public partial class MNT001_002 : Window
    {
        #region Declaration & Constructor 

        System.Timers.Timer tmrMainTimer;

        private Storyboard sbExpandLeft;
        private Storyboard sbCollapseLeft;

        private static string selectedShop = string.Empty;
        private static string selectedArea = string.Empty;
        private static string selectedEquipmentSegment = string.Empty;
        private static string selectedEquipment = string.Empty;

        private static string selectedDisplayName = string.Empty;
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

        DataTable dtBindTable;
        DataTable dtSearchTable;
        private System.Timers.Timer _timer_NowDate;         //현재시간 가져오기(1초당)
        private System.Timers.Timer _timer_DataSearch;      //data 새로 가져오기(기준:분)
        private System.Timers.Timer _timer_View_Rotation;   //화면전환하기(기준:초)       

        public DataTable DefectData { get; set; }
        DataTable dtBindTable_temp;
        int dtBindTable_temp_row = 0;
        DataTable dtSortTable;

        private DataTable[] dtDiv; //로테이션할 DataTable 갯수
        int div = 0; //로테이션할 화면 갯수
        int viewRowCnt = 0; //한번에 보여줄 Row 갯수
        int bindTableRowCnt = 0; //Binding할 DataTable의 Row수
        int rotationView = 0; // 화면전환속도
        int realViewCnt = 0; //현재 보이는 화면 번호

        bool first_access = true;

        string now_date = string.Empty; // 06시 기준 날짜  - 조회시 던지는 변수
        //ex) 현재 시각이 2017-05-09 05:00:000 이면 금일 날짜는 20170508 이고
        //                                          어제 날짜는 20170507 이고
        //                                          익일 날짜는 20170508 임.

        DateTime standart_date; //Test를 위해 test 기준 date
        DateTime accept06_date; //06시 적용 Date
        //그리드 헤더에 적용할 날짜
        string now_date_header = string.Empty;
        string yester_date_header = string.Empty;
        string tomorrow_date_header = string.Empty;
        private void acceptCalDate()
        {
            DateTime dt = DateTime.Now;

            if(LoginInfo.USERID == "cnswkdakscjf")
            {               
                DateTime test = DateTime.Now.AddHours(-129);
                standart_date = test;

                int hour = test.Hour;
                int minute = test.Minute;
                if (hour < 6)
                {
                    dt = test.AddDays(-1);
                }
                else
                {
                    dt = test;
                }
            }
            else
            {
                standart_date = dt;
                int hour = dt.Hour;
                int minute = dt.Minute;
                if (hour < 6)
                {
                    dt = DateTime.Now.AddDays(-1);
                }
                else
                {
                    dt = DateTime.Now;
                }
            }

            accept06_date = dt;
            now_date = dt.ToString("yyyyMMdd");
            now_date_header = dt.ToString("MM-dd");
            yester_date_header = dt.AddDays(-1).ToString("MM-dd");
            tomorrow_date_header = dt.AddDays(+1).ToString("MM-dd");
        }

        public MNT001_002()
        {
            try
            {
                InitializeComponent();

                this.Loaded += UserControl_Loaded;
                dgMONITORING.LoadedCellPresenter += dgMONITORING_LoadedCellPresenter;
                //dgMONITORING.MergingCells -= dgMONITORING_MergingCells;
                
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }

        private void applyObejctDicToDataGrid(C1DataGrid dataGrid)
        {
            foreach (DataGridColumn column in dataGrid.Columns)
            {
                applyObejctDicToColumn(column);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            Initialize();

            InitColumnWidth();
        }

        private void InitColumnWidth()
        {
            int W = Screen.PrimaryScreen.Bounds.Width; //모니터 스크린 가로크기
            int H = Screen.PrimaryScreen.Bounds.Height; //모니터 스크린 세로크기

            double w_temp = (W - 950 -2); //전체 넓이에서 고정된 column의 넓이 뺌.
            double h_temp = (H - 70);  //전체 높이에서 고정된 Grid의 높이 뺌

            oneColumn_width = Convert.ToInt32(Math.Round(w_temp / 5)); // 반올림 : 고정되지 않은 컬럼수로 나눔
            oneRow_width = Convert.ToInt32(Math.Round(h_temp / 12)); // 반올림 : 고정되지 않은 GRID 수로 나눔

            fullSize.Width = new GridLength(W);
            dgMONITORING.Width = W;
            dgMONITORING.Height = H - 70;
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

        private void Initialize()
        {
            try
            {
                if(LoginInfo.USERID.Trim() == "cnswkdakscjf")
                {
                    tbAcceptResult.Visibility = Visibility.Visible;
                    dbAcepptResult.Visibility = Visibility.Visible;
                    tbdate1.Visibility = Visibility.Visible;
                    tbdate.Visibility = Visibility.Visible;
                }

                InitCombo();

                //2018.01.29 
                //this.Tag = new object[] { "자동차 PACK 생산현황" };
                this.Tag = new object[] { ObjectDic.Instance.GetObjectName("자동차") + " " + "PACK" + " " + ObjectDic.Instance.GetObjectName("생산현황") };

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

                //Storyboard =======================================================================================
                sbExpandLeft = (Storyboard)this.Resources["ExpandLeftFrameStoryboard"];
                sbCollapseLeft = (Storyboard)this.Resources["CollapseLeftFrameStoryboard"];

                //Clear_Display_Control();

                DataSet ds = LGC.GMES.MES.MNT001.MNT_Common.GetConfigXML_PACK();

                selectedShop = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["SHOP"]);
                selectedArea = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["AREA"]);
                selectedEquipmentSegment = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["EQUIPMENTSEGMENT"]);
                //selectedEquipment = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["EQUIPMENT"]);
                
                selectedDisplayTime = Convert.ToInt32(ds.Tables["MNT_CONFIG"].Rows[0]["DISPLAYTIME"]);                
                seletedViewRowCnt = Convert.ToInt32(ds.Tables["MNT_CONFIG"].Rows[0]["DISPLAYVIEWROWCNT"]);

                if (ds.Tables["MNT_CONFIG"].Columns["DISPLAYTIMESUB"] == null)
                {
                    selectedDisplayTimeSub = 3;
                }
                else
                {
                    selectedDisplayTimeSub = Convert.ToInt32(ds.Tables["MNT_CONFIG"].Rows[0]["DISPLAYTIMESUB"]);
                }

                numViewRowCnt.Value = seletedViewRowCnt;
                numRefresh.Value = selectedDisplayTime;
                numRefreshSub.Value = selectedDisplayTimeSub;

                cboShop.SelectedValue = selectedShop;
                cboArea.SelectedValue = selectedArea;                

                DateTime dt = standart_date;
                //tbdate.Text = dt.ToString("yyyy") + ObjectDic.Instance.GetObjectName("년") + " " +
                //                dt.ToString("MM") + ObjectDic.Instance.GetObjectName("월") + " " +
                //                dt.ToString("dd") + ObjectDic.Instance.GetObjectName("일") + " " +
                //                dt.ToString("hh") + ObjectDic.Instance.GetObjectName("시") + " " +
                //                dt.ToString("mm") + ObjectDic.Instance.GetObjectName("분") + " " +
                //                dt.ToString("ss") + ObjectDic.Instance.GetObjectName("초");

                //2018-01-29
                //tbdate.Text = dt.ToString("yyyy") + "년 " +
                //                dt.ToString("MM") + "월 " +
                //                dt.ToString("dd") + "일 " +
                //                dt.ToString("HH") + "시 " +
                //                dt.ToString("mm") + "분 " +
                //                dt.ToString("ss") + "초";
                tbdate.Text = dt.ToString("yyyy") + "- " +
                dt.ToString("MM") + "- " +
                dt.ToString("dd") + "- " +
                dt.ToString("HH") + ": " +
                dt.ToString("mm") + ": " +
                dt.ToString("ss") + ":";

                DateTime dt1 = accept06_date;
                //2018-01-29
                //dbAcepptResult.Text =   dt1.ToString("yyyy") + "년 " +
                //                        dt1.ToString("MM") + "월 " +
                //                        dt1.ToString("dd") + "일 " +
                //                        dt1.ToString("HH") + "시 " +
                //                        dt1.ToString("mm") + "분 " +
                //                        dt1.ToString("ss") + "초";
                dbAcepptResult.Text = dt1.ToString("yyyy") + "- " +
                        dt1.ToString("MM") + "- " +
                        dt1.ToString("dd") + "- " +
                        dt1.ToString("HH") + ": " +
                        dt1.ToString("mm") + ": " +
                        dt1.ToString("ss") + ":";

                acceptCalDate(); //06시로 적용된 현재 날짜

                //dtpDate.SelectedDateTime = Convert.ToDateTime("2016-11-01");

                realData(); //data 가져와서 그리드에 바인딩 시켜준 후 화면전환 타이머 작동

                NowDate_Start_Timer(); //시간 타이머

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
            //C1ComboBox[] cboShopChild = { cboArea };
            //_combo.SetCombo(cboShop, CommonCombo.ComboStatus.SELECT, cbChild: cboShopChild, sCase: "cboShopByAreaType");

            //동    
            SetArea(cboArea);
            //C1ComboBox[] cboAreaParent = { cboShop };           
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbParent: cboAreaParent);

            //라인
            SetCboLine(cboEquipmentSegment, Util.NVC(cboShop.SelectedValue), Util.NVC(cboArea.SelectedValue));
            cboEquipmentSegment.isAllUsed = false;           
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

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = Util.NVC(Util.GetCondition(cboShop)) == "" ? null : Util.NVC(Util.GetCondition(cboShop));
               
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_M_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                DataRow dr1 = dtResult.NewRow();

                dr1["CBO_NAME"] = " - ALL-";
                dr1["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(dr1, 0);

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

        private void SetCboLine(MultiSelectionBox cboMulti, string sSelectedShop, string sSelectedArea)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = sSelectedShop == "" ? null : sSelectedShop;
                dr["AREAID"] = sSelectedArea== "" ? null : sSelectedArea;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_M_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboMulti.DisplayMemberPath = "CBO_NAME";
                cboMulti.SelectedValuePath = "CBO_CODE";

                cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);

              
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
                dtBindTable_temp = null;

                end_Rotation(); //화면전환 timer 종료

                //initBindTable();

                searchVer();

                searchData();

                setRealDate();

                if (dtSearchTable == null || dtSearchTable.Rows.Count == 0)
                {
                    return;
                }

                initBindTable_REAL_SET();

                bind();

                /* header 다국어 적용 일단 제거
                                if (dgMONITORING.Dispatcher.CheckAccess())
                                {
                                    applyObejctDicToDataGrid(dgMONITORING);
                                }
                                else
                                {
                                    dgMONITORING.Dispatcher.BeginInvoke(new Action(() => applyObejctDicToDataGrid(dgMONITORING)));
                                }                               
                */

                if (dgMONITORING.Dispatcher.CheckAccess())
                {
                    applyObejctDicToDataGrid(dgMONITORING);
                }
                else
                {
                    dgMONITORING.Dispatcher.BeginInvoke(new Action(() => applyObejctDicToDataGrid(dgMONITORING)));
                }

                RotationView_Start_Timer(); //새로 data 가져온 후 화면 전환 timer 시작


            }
            catch(Exception ex)
            {
                throw ex;
            }           
        }

        private void initBindTable()
        {
            try
            {
                dtBindTable = new DataTable();

                dtBindTable.Columns.Add("LINE_ABBR_CODE", typeof(string));
                dtBindTable.Columns.Add("MODLID", typeof(string));
                dtBindTable.Columns.Add("WOTYPE", typeof(string));
                //dtBindTable.Columns.Add("PRJT_NAME", typeof(string));

                //dtBindTable.Columns.Add("M_PLAN", typeof(string));
                //dtBindTable.Columns.Add("M_PLAN_S", typeof(string));
                //dtBindTable.Columns.Add("M_RESULT_S", typeof(string));
                //dtBindTable.Columns.Add("M_ACCEPT", typeof(string));
                //dtBindTable.Columns.Add("M_ACCEPT_S", typeof(string));

                //dtBindTable.Columns.Add("W_PLAN", typeof(string));
                //dtBindTable.Columns.Add("W_PLAN_S", typeof(string));
                //dtBindTable.Columns.Add("W_RESULT_S", typeof(string));
                //dtBindTable.Columns.Add("W_ACCEPT", typeof(string));
                //dtBindTable.Columns.Add("W_ACCEPT_S", typeof(string));

                dtBindTable.Columns.Add("Y_PLAN", typeof(string));
                dtBindTable.Columns.Add("Y_RESULT", typeof(string));
                dtBindTable.Columns.Add("Y_ACCEPT", typeof(string));

                dtBindTable.Columns.Add("D_PLAN", typeof(string));
                dtBindTable.Columns.Add("D_RESULT", typeof(string));
                dtBindTable.Columns.Add("D_ACCEPT", typeof(string));

                dtBindTable.Columns.Add("T_PLAN", typeof(string));

                dtBindTable.Columns.Add("MOVE", typeof(string));

                //dtBindTable.Columns.Add("LINE_MOVE1", typeof(string));
                //dtBindTable.Columns.Add("LINE_MOVE2", typeof(string));
                //dtBindTable.Columns.Add("LINE_MOVE3", typeof(string));
                //dtBindTable.Columns.Add("LINE_MOVE4", typeof(string));

                DataRow dr;

                for (int i = 1; i <= 12; i++)
                {
                    dr = dtBindTable.Rows.Add();

                    dr["LINE_ABBR_CODE"] = i.ToString();
                    dr["MODLID"] = "";
                    dr["WOTYPE"] = "";
                    //dr["PRJT_NAME"] = "";

                    //dr["M_PLAN"] = "";
                    //dr["M_PLAN_S"] = "";
                    //dr["M_RESULT_S"] = "";
                    //dr["M_ACCEPT"] = "0.0%";
                    //dr["M_ACCEPT_S"] = "0.0%";

                    //dr["W_PLAN"] = "";
                    //dr["W_PLAN_S"] = "";
                    //dr["W_RESULT_S"] = "";
                    //dr["W_ACCEPT"] = "0.0%";
                    //dr["W_ACCEPT_S"] = "0.0%";

                    dr["Y_PLAN"] = "";
                    dr["Y_RESULT"] = "";
                    dr["Y_ACCEPT"] = "0.0%";

                    dr["D_PLAN"] = "";
                    dr["D_RESULT"] = "";
                    dr["D_ACCEPT"] = "0.0%";

                    dr["T_PLAN"] = "";

                    dr["MOVE"] = 12 % i == 1 ? "W" : i % 4 == 2 ? "R" : i % 4 == 3 ? "I" : i % 4 == 0 ? "T" : "T";

                    //dr["LINE_MOVE1"] = "";
                    //dr["LINE_MOVE2"] = "";
                    //dr["LINE_MOVE3"] = "";
                    //dr["LINE_MOVE4"] = "";
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void initBindTable_REAL_SET()
        {
            try
            {
                dtBindTable = new DataTable();

                dtBindTable.Columns.Add("LINE_ABBR_CODE", typeof(string));
                dtBindTable.Columns.Add("MODLID", typeof(string));
                dtBindTable.Columns.Add("WOTYPE", typeof(string));
                //dtBindTable.Columns.Add("PRJT_NAME", typeof(string));

                //dtBindTable.Columns.Add("M_PLAN", typeof(string));
                //dtBindTable.Columns.Add("M_PLAN_S", typeof(string));
                //dtBindTable.Columns.Add("M_RESULT_S", typeof(string));
                //dtBindTable.Columns.Add("M_ACCEPT", typeof(string));
                //dtBindTable.Columns.Add("M_ACCEPT_S", typeof(string));

                //dtBindTable.Columns.Add("W_PLAN", typeof(string));
                //dtBindTable.Columns.Add("W_PLAN_S", typeof(string));
                //dtBindTable.Columns.Add("W_RESULT_S", typeof(string));
                //dtBindTable.Columns.Add("W_ACCEPT", typeof(string));
                //dtBindTable.Columns.Add("W_ACCEPT_S", typeof(string));

                dtBindTable.Columns.Add("Y_PLAN", typeof(string));
                dtBindTable.Columns.Add("Y_RESULT", typeof(string));
                dtBindTable.Columns.Add("Y_ACCEPT", typeof(string));

                dtBindTable.Columns.Add("D_PLAN", typeof(string));
                dtBindTable.Columns.Add("D_RESULT", typeof(string));
                dtBindTable.Columns.Add("D_ACCEPT", typeof(string));

                dtBindTable.Columns.Add("T_PLAN", typeof(string));

                dtBindTable.Columns.Add("MOVE", typeof(string));

                //dtBindTable.Columns.Add("LINE_MOVE1", typeof(string));
                //dtBindTable.Columns.Add("LINE_MOVE2", typeof(string));
                //dtBindTable.Columns.Add("LINE_MOVE3", typeof(string));
                //dtBindTable.Columns.Add("LINE_MOVE4", typeof(string));

                DataRow dr;
                string temp_line = string.Empty;
                int rowCnt = dtSearchTable.Rows.Count;

                for (int i = 0; i < rowCnt; i++)
                { 
                    if(temp_line != null && temp_line !="") 
                    {
                        if(temp_line == dtSearchTable.Rows[i]["LINE_ABBR_CODE"].ToString())
                        {
                            temp_line = dtSearchTable.Rows[i]["LINE_ABBR_CODE"].ToString();
                            continue;
                        }
                    }

                    dr = dtBindTable.Rows.Add();

                    dr["LINE_ABBR_CODE"] = dtSearchTable.Rows[i]["LINE_ABBR_CODE"].ToString();
                    dr["MODLID"] = "";
                    dr["WOTYPE"] = dtSearchTable.Rows[i]["WOTYPE"].ToString();
                    //dr["PRJT_NAME"] = "";

                    //dr["M_PLAN"] = "";
                    //dr["M_PLAN_S"] = "";
                    //dr["M_RESULT_S"] = "";
                    //dr["M_ACCEPT"] = "0.0%";
                    //dr["M_ACCEPT_S"] = "0.0%";

                    //dr["W_PLAN"] = "";
                    //dr["W_PLAN_S"] = "";
                    //dr["W_RESULT_S"] = "";
                    //dr["W_ACCEPT"] = "0.0%";
                    //dr["W_ACCEPT_S"] = "0.0%";

                    dr["Y_PLAN"] = "";
                    dr["Y_RESULT"] = "";
                    dr["Y_ACCEPT"] = "0.0%";

                    dr["D_PLAN"] = "";
                    dr["D_RESULT"] = "";
                    dr["D_ACCEPT"] = "0.0%";

                    dr["T_PLAN"] = "";

                    dr["MOVE"] = rowCnt-i % 4 == 1 ? "W" : rowCnt - i % 4== 2 ? "R" : rowCnt - i % 4== 3 ? "I" : rowCnt - i % 4 == 0 ? "T" : "T";

                    //dr["LINE_MOVE1"] = "";
                    //dr["LINE_MOVE2"] = "";
                    //dr["LINE_MOVE3"] = "";
                    //dr["LINE_MOVE4"] = "";

                    temp_line =  dtSearchTable.Rows[i]["LINE_ABBR_CODE"].ToString();
                }            
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void initBindTable_temp(string line, string model, string wotype)
        {
            try
            {
                dtBindTable_temp = new DataTable();

                dtBindTable_temp.Columns.Add("LINE_ABBR_CODE", typeof(string));
                dtBindTable_temp.Columns.Add("MODLID", typeof(string));
                dtBindTable_temp.Columns.Add("WOTYPE", typeof(string));
                //dtBindTable_temp.Columns.Add("PRJT_NAME", typeof(string));

                //dtBindTable_temp.Columns.Add("M_PLAN", typeof(string));
                //dtBindTable_temp.Columns.Add("M_PLAN_S", typeof(string));
                //dtBindTable_temp.Columns.Add("M_RESULT_S", typeof(string));
                //dtBindTable_temp.Columns.Add("M_ACCEPT", typeof(string));
                //dtBindTable_temp.Columns.Add("M_ACCEPT_S", typeof(string));

                //dtBindTable_temp.Columns.Add("W_PLAN", typeof(string));
                //dtBindTable_temp.Columns.Add("W_PLAN_S", typeof(string));
                //dtBindTable_temp.Columns.Add("W_RESULT_S", typeof(string));
                //dtBindTable_temp.Columns.Add("W_ACCEPT", typeof(string));
                //dtBindTable_temp.Columns.Add("W_ACCEPT_S", typeof(string));

                dtBindTable_temp.Columns.Add("Y_PLAN", typeof(string));
                dtBindTable_temp.Columns.Add("Y_RESULT", typeof(string));
                dtBindTable_temp.Columns.Add("Y_ACCEPT", typeof(string));
                
                dtBindTable_temp.Columns.Add("D_PLAN", typeof(string));
                dtBindTable_temp.Columns.Add("D_RESULT", typeof(string));
                dtBindTable_temp.Columns.Add("D_ACCEPT", typeof(string));

                dtBindTable_temp.Columns.Add("T_PLAN", typeof(string));

                dtBindTable_temp.Columns.Add("MOVE", typeof(string));

                //dtBindTable_temp.Columns.Add("LINE_MOVE1", typeof(string));
                //dtBindTable_temp.Columns.Add("LINE_MOVE2", typeof(string));
                //dtBindTable_temp.Columns.Add("LINE_MOVE3", typeof(string));
                //dtBindTable_temp.Columns.Add("LINE_MOVE4", typeof(string));

                DataRow dr;
               
                dr = dtBindTable_temp.Rows.Add();

                dr["LINE_ABBR_CODE"] = line;
                dr["MODLID"] = model;
                dr["WOTYPE"] = wotype;
                //dr["PRJT_NAME"] = prjt_name;

                //dr["M_PLAN"] = "";
                //dr["M_PLAN_S"] = "";
                //dr["M_RESULT_S"] = "";
                //dr["M_ACCEPT"] = "0.0%";
                //dr["M_ACCEPT_S"] = "0.0%";

                //dr["W_PLAN"] = "";
                //dr["W_PLAN_S"] = "";
                //dr["W_RESULT_S"] = "";
                //dr["W_ACCEPT"] = "0.0%";
                //dr["W_ACCEPT_S"] = "0.0%";

                dr["Y_PLAN"] = "";
                dr["Y_RESULT"] = "";
                dr["Y_ACCEPT"] = "0.0%";

                dr["D_PLAN"] = "";
                dr["D_RESULT"] = "";
                dr["D_ACCEPT"] = "0.0%";

                dr["T_PLAN"] = "";

                dr["MOVE"] = "";

                //dr["LINE_MOVE1"] = "";
                //dr["LINE_MOVE2"] = "";
                //dr["LINE_MOVE3"] = "";
                //dr["LINE_MOVE4"] = "";
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void addBinTable_temp_row(string line, string model, string wotype)
        {
            try
            {
                DataRow dr;

                dr = dtBindTable_temp.Rows.Add();

                dr["LINE_ABBR_CODE"] = line;
                dr["MODLID"] = model;
                //2019.02.27
                dr["WOTYPE"] = wotype;
                //dr["PRJT_NAME"] = prjt_name;

                //dr["M_PLAN"] = "";
                //dr["M_PLAN_S"] = "";
                //dr["M_RESULT_S"] = "";
                //dr["M_ACCEPT"] = "0.0%";
                //dr["M_ACCEPT_S"] = "0.0%";

                //dr["W_PLAN"] = "";
                //dr["W_PLAN_S"] = "";
                //dr["W_RESULT_S"] = "";
                //dr["W_ACCEPT"] = "0.0%";
                //dr["W_ACCEPT_S"] = "0.0%";

                dr["Y_PLAN"] = "";
                dr["Y_RESULT"] = "";
                dr["Y_ACCEPT"] = "0.0%";

                dr["D_PLAN"] = "";
                dr["D_RESULT"] = "";
                dr["D_ACCEPT"] = "0.0%";

                dr["T_PLAN"] = "";

                dr["MOVE"] = "";

                //dr["LINE_MOVE1"] = "";
                //dr["LINE_MOVE2"] = "";
                //dr["LINE_MOVE3"] = "";
                //dr["LINE_MOVE4"] = "";
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void addBinTable_to_temp_row(DataRow drTemp)
        {
            try
            {
                DataRow dr;

                dr = dtBindTable.Rows.Add();

                dr["LINE_ABBR_CODE"] = drTemp["LINE_ABBR_CODE"].ToString();
                dr["MODLID"] = drTemp["MODLID"].ToString();
                //2019.02.27
                dr["WOTYPE"] = drTemp["WOTYPE"].ToString();
                //dr["PRJT_NAME"] = drTemp["PRJT_NAME"].ToString();

                //dr["M_PLAN"] = drTemp["M_PLAN"].ToString();
                //dr["M_PLAN_S"] = drTemp["M_PLAN_S"].ToString();
                //dr["M_RESULT_S"] = drTemp["M_RESULT_S"].ToString();
                //dr["M_ACCEPT"] = drTemp["M_ACCEPT"].ToString();
                //dr["M_ACCEPT_S"] = drTemp["M_ACCEPT_S"].ToString();

                //dr["W_PLAN"] = drTemp["W_PLAN"].ToString();
                //dr["W_PLAN_S"] = drTemp["W_PLAN_S"].ToString();
                //dr["W_RESULT_S"] = drTemp["W_RESULT_S"].ToString();
                //dr["W_ACCEPT"] = drTemp["W_ACCEPT"].ToString();
                //dr["W_ACCEPT_S"] = drTemp["W_ACCEPT_S"].ToString();

                dr["Y_PLAN"] = drTemp["Y_PLAN"].ToString();
                dr["Y_RESULT"] = drTemp["Y_RESULT"].ToString();
                dr["Y_ACCEPT"] = drTemp["Y_ACCEPT"].ToString();

                dr["D_PLAN"] = drTemp["D_PLAN"].ToString();
                dr["D_RESULT"] = drTemp["D_RESULT"].ToString();
                dr["D_ACCEPT"] = drTemp["D_ACCEPT"].ToString();

                dr["T_PLAN"] = drTemp["T_PLAN"].ToString();

                dr["MOVE"] = drTemp["MOVE"].ToString();

                //dr["LINE_MOVE1"] = drTemp["LINE_MOVE1"].ToString();
                //dr["LINE_MOVE2"] = drTemp["LINE_MOVE2"].ToString();
                //dr["LINE_MOVE3"] = drTemp["LINE_MOVE3"].ToString();
                //dr["LINE_MOVE4"] = drTemp["LINE_MOVE4"].ToString();
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void searchVer()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("DATE", typeof(string)); 

                DataRow dr = RQSTDT.NewRow();
                dr["DATE"] = now_date; // DateTime.Now.ToString("yyyyMMdd");//"20161101"; // dtpDate.SelectedDateTime.ToString("yyyyMMdd");                
               
                RQSTDT.Rows.Add(dr);

                DataTable dtVer = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOARD_SUMMARY_VER", "INDATA", "OUTDATA", RQSTDT);

                if(dtVer.Rows.Count > 0)
                {
                    if (tbVer.Dispatcher.CheckAccess())
                    {
                        tbVer.Text = "VER : " + dtVer.Rows[0]["PLAN_REV"].ToString();
                    }
                    else
                    {
                        tbVer.Dispatcher.BeginInvoke(new Action(() => tbVer.Text = "VER : " + dtVer.Rows[0]["PLAN_REV"].ToString()));
                    }

                    
                }
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void searchData()
        {
            try
            {      
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("DATE", typeof(string));                
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                string area = string.Empty;
                string lines = string.Empty;
               
                area = GetUIControl(cboArea);
                lines = GetUIControl(cboEquipmentSegment);

                DataRow dr = RQSTDT.NewRow();
                dr["DATE"] = now_date; // DateTime.Now.ToString("yyyyMMdd");//"20161101"; // dtpDate.SelectedDateTime.ToString("yyyyMMdd");                
                dr["AREAID"] = area == "" ? null : area;// Util.GetCondition(cboEquipmentSegment) == "" ? null : Util.GetCondition(cboEquipmentSegment);
                dr["EQSGID"] = lines == ""? null : lines;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                dtSearchTable = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOARD_SUMMARY", "INDATA", "OUTDATA", RQSTDT);
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
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
            dtBindTable_temp_row = 0;

            for (int i = 1; i <= dtBindTable.Rows.Count; i++)
            {
                // DataRow[] line_chk = dtSearchTable.Select("LINE_ABBR_CODE = '" + i.ToString() + "'");

                DataRow[] line_chk = dtSearchTable.Select("LINE_ABBR_CODE = '" + dtBindTable.Rows[i-1]["LINE_ABBR_CODE"].ToString() + "'");

                if (line_chk.Length != 0) //bind 기초 datatable의 라인을 조회결과 datatable에서 search 해서 있을 경우만 데이터를 bind table에 넣어줌
                {
                    for (int j = 0; j < line_chk.Length; j++)
                    {
                        if (j == 0) //기초 bind table에 넣어줌
                        {
                            dtBindTable.Rows[i - 1]["MODLID"] = line_chk[0]["MODLID"] == null ? "" : line_chk[0]["MODLID"];
                            //dtBindTable.Rows[i - 1]["MODLID"] = "AMVAGBEVM-A4-B02PW";
                            //dtBindTable.Rows[i - 1]["PRJT_NAME"] = line_chk[0]["PRJT_NAME"] == null ? "" : line_chk[0]["PRJT_NAME"];
                            dtBindTable.Rows[i - 1]["D_PLAN"] = line_chk[0]["D_PLAN"].ToString() == "" ? "-" : line_chk[0]["D_PLAN"];
                            //2019-02-20
                            dtBindTable.Rows[i - 1]["WOTYPE"] = line_chk[0]["WOTYPE"].ToString() == "" ? "-" : line_chk[0]["WOTYPE"];

                            //if(dtBindTable.Rows[i - 1]["D_PLAN"].ToString() == "0")
                            //{
                            //    dtBindTable.Rows[i - 1]["D_PLAN"] = null;
                            //    line_chk[0]["D_PLAN"] = "";
                            //}

                            if ((line_chk[0]["D_PLAN"].ToString() == "" || line_chk[0]["D_PLAN"].ToString() == "0") && line_chk[0]["D_RESULT"].ToString() == "")//계획도 없고 실적도 없을때 처리
                            {
                                dtBindTable.Rows[i - 1]["D_PLAN"] = "-";
                                dtBindTable.Rows[i - 1]["D_RESULT"] = "-";
                                dtBindTable.Rows[i - 1]["D_ACCEPT"] = "100.0%";
                            }else if(line_chk[0]["D_PLAN"].ToString() != "" && line_chk[0]["D_RESULT"].ToString() == "") //계획은 있고 실적이 없을때 처리
                            {
                                dtBindTable.Rows[i - 1]["D_RESULT"] = "0";
                                dtBindTable.Rows[i - 1]["D_ACCEPT"] = "0.0%";
                            }
                            else if(line_chk[0]["D_PLAN"].ToString() == "" && line_chk[0]["D_RESULT"].ToString() != "")
                            {
                                dtBindTable.Rows[i - 1]["D_RESULT"] = line_chk[0]["D_RESULT"];
                                dtBindTable.Rows[i - 1]["D_ACCEPT"] = "100.0%";
                            }
                            else
                            {
                                dtBindTable.Rows[i - 1]["D_RESULT"] = line_chk[0]["D_RESULT"].ToString() == "" ? "" : line_chk[0]["D_RESULT"];
                                dtBindTable.Rows[i - 1]["D_ACCEPT"] = line_chk[0]["D_ACCEPT"].ToString() == "" ? "0.0%" : line_chk[0]["D_ACCEPT"].ToString() == "" ? "0.0%" : line_chk[0]["D_ACCEPT"] + "%";
                            }

                            //dtBindTable.Rows[i - 1]["M_PLAN"] = line_chk[0]["M_PLAN"];
                            //dtBindTable.Rows[i - 1]["M_PLAN_S"] = line_chk[0]["M_PLAN_S"];
                            //dtBindTable.Rows[i - 1]["M_RESULT_S"] = line_chk[0]["M_RESULT_S"];
                            //dtBindTable.Rows[i - 1]["M_ACCEPT"] = line_chk[0]["M_ACCEPT"] + "%";
                            //dtBindTable.Rows[i - 1]["M_ACCEPT_S"] = line_chk[0]["M_ACCEPT_S"] + "%";

                            //dtBindTable.Rows[i - 1]["W_PLAN"] = line_chk[0]["W_PLAN"];
                            //dtBindTable.Rows[i - 1]["W_PLAN_S"] = line_chk[0]["W_PLAN_S"];
                            //dtBindTable.Rows[i - 1]["W_RESULT_S"] = line_chk[0]["W_RESULT_S"];
                            //dtBindTable.Rows[i - 1]["W_ACCEPT"] = line_chk[0]["W_ACCEPT"] + "%";
                            //dtBindTable.Rows[i - 1]["W_ACCEPT_S"] = line_chk[0]["W_ACCEPT_S"] + "%";

                            dtBindTable.Rows[i - 1]["Y_PLAN"] = line_chk[0]["Y_PLAN"].ToString() == "" ? "-" : line_chk[0]["Y_PLAN"];

                            //if (dtBindTable.Rows[i - 1]["Y_PLAN"].ToString() == "0")
                            //{
                            //    dtBindTable.Rows[i - 1]["Y_PLAN"] = null;
                            //    line_chk[0]["Y_PLAN"] = "";
                            //}

                            if ((line_chk[0]["Y_PLAN"].ToString() == "" || line_chk[0]["Y_PLAN"].ToString() == "0") && line_chk[0]["Y_RESULT"].ToString() == "")//계획도 없고 실적도 없을때 처리
                            {
                                dtBindTable.Rows[i - 1]["Y_PLAN"] = "-";
                                dtBindTable.Rows[i - 1]["Y_RESULT"] = "-";
                                dtBindTable.Rows[i - 1]["Y_ACCEPT"] = "100.0%";
                            }
                            else if (line_chk[0]["Y_PLAN"].ToString() != "" && line_chk[0]["Y_RESULT"].ToString() == "") //계획은 있고 실적이 없을때 처리
                            {
                                dtBindTable.Rows[i - 1]["Y_RESULT"] = "0";
                                dtBindTable.Rows[i - 1]["Y_ACCEPT"] = "0.0%";
                            }
                            else if (line_chk[0]["Y_PLAN"].ToString() == "" && line_chk[0]["Y_RESULT"].ToString() != "")
                            {
                                dtBindTable.Rows[i - 1]["Y_RESULT"] = line_chk[0]["Y_RESULT"];
                                dtBindTable.Rows[i - 1]["Y_ACCEPT"] = "100.0%";
                            }
                            else
                            {
                                dtBindTable.Rows[i - 1]["Y_RESULT"] = line_chk[0]["Y_RESULT"].ToString() == "" ? "" : line_chk[0]["Y_RESULT"];
                                dtBindTable.Rows[i - 1]["Y_ACCEPT"] = line_chk[0]["Y_ACCEPT"].ToString() == "" ? "0.0%" : line_chk[0]["Y_ACCEPT"].ToString() == "" ? "0.0%" : line_chk[0]["Y_ACCEPT"] + "%";
                            }

                            dtBindTable.Rows[i - 1]["T_PLAN"] = line_chk[0]["T_PLAN"].ToString() == "" ? "-" : line_chk[0]["T_PLAN"];

                            if(LoginInfo.USERID.Trim() == "cnswkdakscjf1")
                            {
                                dtBindTable.Rows[i - 1]["MOVE"] = "R";
                            }
                            else
                            {
                                dtBindTable.Rows[i - 1]["MOVE"] = line_chk[0]["M_STAT"].ToString() == "" ? "N/A" : line_chk[0]["M_STAT"];
                            }

                            //if (Convert.ToInt32(line_chk[0]["M_ACCEPT_S"]) >= 90)
                            //{
                            //    dtBindTable.Rows[i - 1]["LINE_MOVE1"] = " ";
                            //}
                            //else if (Convert.ToInt32(line_chk[0]["M_ACCEPT_S"]) >= 80 && Convert.ToInt32(line_chk[0]["M_ACCEPT_S"]) < 90)
                            //{
                            //    dtBindTable.Rows[i - 1]["LINE_MOVE2"] = " ";
                            //}
                            //else if (Convert.ToInt32(line_chk[0]["M_ACCEPT_S"]) >= 70 && Convert.ToInt32(line_chk[0]["M_ACCEPT_S"]) < 80)
                            //{
                            //    dtBindTable.Rows[i - 1]["LINE_MOVE3"] = " ";
                            //}
                            //else if (Convert.ToInt32(line_chk[0]["M_ACCEPT_S"]) < 70)
                            //{
                            //    dtBindTable.Rows[i - 1]["LINE_MOVE4"] = " ";
                            //}

                            //dtBindTable.Rows[i - 1]["M_ACCEPT_S"] = line_chk[0]["M_ACCEPT_S"] + "%";

                            // dtBindTable.Rows.Remove(dtBindTable.Rows[i-1]);


                            //dtBindTable.Rows.InsertAt(dr[0], i-1);
                            //DataRow row = dtBindTable.NewRow();
                            //row = dr[0];
                            //dtBindTable.Rows.Add(row);
                            // dtBindTable.AcceptChanges();
                        }
                        else if (j == 1) //search data table에 여러건이 있을 경우 2번째는 임시 bind 테이블을 만들어서  담아둠.
                        {
                            
                            if(dtBindTable_temp != null && dtBindTable_temp.Rows.Count > 0)
                            {
                                //임시 테이블에 조회된 line, 모델로 Row를 만들어둠.
                                //2019.02.27
                                //addBinTable_temp_row(line_chk[j]["LINE_ABBR_CODE"].ToString(), line_chk[j]["MODLID"].ToString());
                                addBinTable_temp_row(line_chk[j]["LINE_ABBR_CODE"].ToString(), line_chk[j]["MODLID"].ToString(), line_chk[j]["WOTYPE"].ToString());

                                row_idx = dtBindTable_temp_row;
                            }
                            else
                            {
                                //임시로 테이블 조회된 line, 모델로 테이블 만들어둠..
                                //2019.02.27
                                //initBindTable_temp(line_chk[j]["LINE_ABBR_CODE"].ToString(), line_chk[j]["MODLID"].ToString());
                                initBindTable_temp(line_chk[j]["LINE_ABBR_CODE"].ToString(), line_chk[j]["MODLID"].ToString(), line_chk[j]["WOTYPE"].ToString());
                            }

                            dtBindTable_temp.Rows[row_idx]["D_PLAN"] = line_chk[j]["D_PLAN"].ToString() == "" ? "-" : line_chk[j]["D_PLAN"];

                            //if (dtBindTable_temp.Rows[row_idx]["D_PLAN"].ToString() == "0")
                            //{
                            //    dtBindTable_temp.Rows[row_idx]["D_PLAN"] = null;
                            //    line_chk[j]["D_PLAN"] = "";
                            //}


                            //2019-02-20
                            dtBindTable.Rows[i - 1]["WOTYPE"] = line_chk[0]["WOTYPE"].ToString() == "" ? "-" : line_chk[0]["WOTYPE"];

                            if ((line_chk[j]["D_PLAN"].ToString() == "" || line_chk[j]["D_PLAN"].ToString() == "0") && line_chk[j]["D_RESULT"].ToString() == "")//계획도 없고 실적도 없을때 처리
                            {
                                dtBindTable_temp.Rows[row_idx]["D_PLAN"] = "-";
                                dtBindTable_temp.Rows[row_idx]["D_RESULT"] = "-";
                                dtBindTable_temp.Rows[row_idx]["D_ACCEPT"] = "100.0%";
                            }
                            else if (line_chk[j]["D_PLAN"].ToString() != "" && line_chk[j]["D_RESULT"].ToString() == "") //계획은 있고 실적이 없을때 처리
                            {
                                dtBindTable_temp.Rows[row_idx]["D_RESULT"] = "0";
                                dtBindTable_temp.Rows[row_idx]["D_ACCEPT"] = "0.0%";
                            }
                            else if (line_chk[j]["D_PLAN"].ToString() == "" && line_chk[j]["D_RESULT"].ToString() != "")
                            {
                                dtBindTable_temp.Rows[row_idx]["D_RESULT"] = line_chk[j]["D_RESULT"];
                                dtBindTable_temp.Rows[row_idx]["D_ACCEPT"] = "100.0%";
                            }
                            else
                            {
                                dtBindTable_temp.Rows[row_idx]["D_RESULT"] = line_chk[j]["D_RESULT"].ToString() == "" ? "" : line_chk[j]["D_RESULT"];
                                dtBindTable_temp.Rows[row_idx]["D_ACCEPT"] = line_chk[j]["D_ACCEPT"].ToString() == "" ? "0.0%" : line_chk[j]["D_ACCEPT"].ToString() == "" ? "0.0%" : line_chk[j]["D_ACCEPT"] + "%";
                            }
                            //dtBindTable_temp.Rows[row_idx]["M_PLAN"] = line_chk[j]["M_PLAN"];
                            //dtBindTable_temp.Rows[row_idx]["M_PLAN_S"] = line_chk[j]["M_PLAN_S"];
                            //dtBindTable_temp.Rows[row_idx]["M_RESULT_S"] = line_chk[j]["M_RESULT_S"];
                            //dtBindTable_temp.Rows[row_idx]["M_ACCEPT"] = line_chk[j]["M_ACCEPT"] + "%";
                            //dtBindTable_temp.Rows[row_idx]["M_ACCEPT_S"] = line_chk[j]["M_ACCEPT_S"] + "%";

                            //dtBindTable_temp.Rows[row_idx]["W_PLAN"] = line_chk[j]["W_PLAN"];
                            //dtBindTable_temp.Rows[row_idx]["W_PLAN_S"] = line_chk[j]["W_PLAN_S"];
                            //dtBindTable_temp.Rows[row_idx]["W_RESULT_S"] = line_chk[j]["W_RESULT_S"];
                            //dtBindTable_temp.Rows[row_idx]["W_ACCEPT"] = line_chk[j]["W_ACCEPT"] + "%";
                            //dtBindTable_temp.Rows[row_idx]["W_ACCEPT_S"] = line_chk[j]["W_ACCEPT_S"] + "%";

                            dtBindTable_temp.Rows[row_idx]["Y_PLAN"] = line_chk[j]["Y_PLAN"].ToString() == "" ? "-" : line_chk[j]["Y_PLAN"];

                            //if (dtBindTable_temp.Rows[row_idx]["Y_PLAN"].ToString() == "0")
                            //{
                            //    dtBindTable_temp.Rows[row_idx]["Y_PLAN"] = null;
                            //    line_chk[j]["Y_PLAN"] = "";
                            //}

                            if ((line_chk[j]["Y_PLAN"].ToString() == "" || line_chk[j]["Y_PLAN"].ToString() == "0") && line_chk[j]["Y_RESULT"].ToString() == "")//계획도 없고 실적도 없을때 처리
                            {
                                dtBindTable_temp.Rows[row_idx]["Y_PLAN"] = "-";
                                dtBindTable_temp.Rows[row_idx]["Y_RESULT"] = "-";
                                dtBindTable_temp.Rows[row_idx]["Y_ACCEPT"] = "100.0%";
                            }
                            else if (line_chk[j]["Y_PLAN"].ToString() != "" && line_chk[j]["Y_RESULT"].ToString() == "") //계획은 있고 실적이 없을때 처리
                            {
                                dtBindTable_temp.Rows[row_idx]["Y_RESULT"] = "0";
                                dtBindTable_temp.Rows[row_idx]["Y_ACCEPT"] = "0.0%";
                            }
                            else if (line_chk[j]["Y_PLAN"].ToString() == "" && line_chk[j]["Y_RESULT"].ToString() != "")
                            {
                                dtBindTable_temp.Rows[row_idx]["Y_RESULT"] = line_chk[j]["Y_RESULT"];
                                dtBindTable_temp.Rows[row_idx]["Y_ACCEPT"] = "100.0%";
                            }
                            else
                            {
                                dtBindTable_temp.Rows[row_idx]["Y_RESULT"] = line_chk[j]["Y_RESULT"].ToString() == "" ? "" : line_chk[j]["Y_RESULT"];
                                dtBindTable_temp.Rows[row_idx]["Y_ACCEPT"] = line_chk[j]["Y_ACCEPT"].ToString() == "" ? "0.0%" : line_chk[j]["Y_ACCEPT"].ToString() == "" ? "0.0%" : line_chk[j]["Y_ACCEPT"] + "%";
                            }

                            dtBindTable_temp.Rows[row_idx]["T_PLAN"] = line_chk[j]["T_PLAN"].ToString() == "" ? "-" : line_chk[j]["T_PLAN"];

                            if (LoginInfo.USERID.Trim() == "cnswkdakscjf1")
                            {
                                dtBindTable_temp.Rows[row_idx]["MOVE"] = "T";
                            }
                            else
                            {
                                dtBindTable_temp.Rows[row_idx]["MOVE"] = line_chk[j]["M_STAT"].ToString() == "" ? "N/A" : line_chk[j]["M_STAT"];
                            }

                            //if (Convert.ToInt32(line_chk[j]["M_ACCEPT_S"]) >= 90)
                            //{
                            //    dtBindTable_temp.Rows[row_idx]["LINE_MOVE1"] = " ";
                            //}
                            //else if (Convert.ToInt32(line_chk[j]["M_ACCEPT_S"]) >= 80 && Convert.ToInt32(line_chk[j]["M_ACCEPT_S"]) < 90)
                            //{
                            //    dtBindTable_temp.Rows[row_idx]["LINE_MOVE2"] = " ";
                            //}
                            //else if (Convert.ToInt32(line_chk[j]["M_ACCEPT_S"]) >= 70 && Convert.ToInt32(line_chk[j]["M_ACCEPT_S"]) < 80)
                            //{
                            //    dtBindTable_temp.Rows[row_idx]["LINE_MOVE3"] = " ";
                            //}
                            //else if (Convert.ToInt32(line_chk[j]["M_ACCEPT_S"]) < 70)
                            //{
                            //    dtBindTable_temp.Rows[row_idx]["LINE_MOVE4"] = " ";
                            //}

                            //dtBindTable_temp.Rows[row_idx]["M_ACCEPT_S"] = line_chk[j]["M_ACCEPT_S"] + "%";

                            dtBindTable_temp_row++;
                        }
                        else  //search data table에 여러건이 있을 경우 3번째부터는 위에서 만들어진 임시 bind 테이블 row를 추가함
                        {
                            //임시 테이블에 조회된 line, 모델로 Row를 만들어둠.
                            //2019.02.27
                            //addBinTable_temp_row(line_chk[j]["LINE_ABBR_CODE"].ToString(), line_chk[j]["MODLID"].ToString());
                            addBinTable_temp_row(line_chk[j]["LINE_ABBR_CODE"].ToString(), line_chk[j]["MODLID"].ToString(), line_chk[j]["WOTYPE"].ToString());

                            dtBindTable_temp.Rows[dtBindTable_temp_row]["D_PLAN"] = line_chk[j]["D_PLAN"].ToString() == "" ? "-" : line_chk[j]["D_PLAN"];

                            //if (dtBindTable_temp.Rows[dtBindTable_temp_row]["D_PLAN"].ToString() == "0")
                            //{
                            //    dtBindTable_temp.Rows[dtBindTable_temp_row]["D_PLAN"] = null;
                            //    line_chk[j]["D_PLAN"] = "";
                            //}

                            //2019-02-20
                            dtBindTable.Rows[i - 1]["WOTYPE"] = line_chk[0]["WOTYPE"].ToString() == "" ? "-" : line_chk[0]["WOTYPE"];

                            if (( line_chk[j]["D_PLAN"].ToString() == "" || line_chk[j]["D_PLAN"].ToString() == "0") && line_chk[j]["D_RESULT"].ToString() == "")//계획도 없고 실적도 없을때 처리
                            {
                                dtBindTable_temp.Rows[dtBindTable_temp_row]["D_PLAN"] = "-";
                                dtBindTable_temp.Rows[dtBindTable_temp_row]["D_RESULT"] = "-";
                                dtBindTable_temp.Rows[dtBindTable_temp_row]["D_ACCEPT"] = "100.0%";
                            }
                            else if (line_chk[j]["D_PLAN"].ToString() != "" && line_chk[j]["D_RESULT"].ToString() == "") //계획은 있고 실적이 없을때 처리
                            {
                                dtBindTable_temp.Rows[dtBindTable_temp_row]["D_RESULT"] = "0";
                                dtBindTable_temp.Rows[dtBindTable_temp_row]["D_ACCEPT"] = "0.0%";
                            }
                            else if (line_chk[j]["D_PLAN"].ToString() == "" && line_chk[j]["D_RESULT"].ToString() != "")
                            {
                                dtBindTable_temp.Rows[dtBindTable_temp_row]["D_RESULT"] = line_chk[j]["D_RESULT"];
                                dtBindTable_temp.Rows[dtBindTable_temp_row]["D_ACCEPT"] = "100.0%";
                            }
                            else
                            {
                                dtBindTable_temp.Rows[dtBindTable_temp_row]["D_RESULT"] = line_chk[j]["D_RESULT"].ToString() == "" ? "" : line_chk[j]["D_RESULT"];
                                dtBindTable_temp.Rows[dtBindTable_temp_row]["D_ACCEPT"] = line_chk[j]["D_ACCEPT"].ToString() == "" ? "0.0%" : line_chk[j]["D_ACCEPT"].ToString() == "" ? "0.0%" : line_chk[j]["D_ACCEPT"] + "%";
                            }

                             //dtBindTable_temp.Rows[dtBindTable_temp_row]["M_PLAN"] = line_chk[j]["M_PLAN"];
                            //dtBindTable_temp.Rows[dtBindTable_temp_row]["M_PLAN_S"] = line_chk[j]["M_PLAN_S"];
                            //dtBindTable_temp.Rows[dtBindTable_temp_row]["M_RESULT_S"] = line_chk[j]["M_RESULT_S"];
                            //dtBindTable_temp.Rows[dtBindTable_temp_row]["M_ACCEPT"] = line_chk[j]["M_ACCEPT"] + "%";
                            //dtBindTable_temp.Rows[dtBindTable_temp_row]["M_ACCEPT_S"] = line_chk[j]["M_ACCEPT_S"] + "%";

                            //dtBindTable_temp.Rows[dtBindTable_temp_row]["W_PLAN"] = line_chk[j]["W_PLAN"];
                            //dtBindTable_temp.Rows[dtBindTable_temp_row]["W_PLAN_S"] = line_chk[j]["W_PLAN_S"];
                            //dtBindTable_temp.Rows[dtBindTable_temp_row]["W_RESULT_S"] = line_chk[j]["W_RESULT_S"];
                            //dtBindTable_temp.Rows[dtBindTable_temp_row]["W_ACCEPT"] = line_chk[j]["W_ACCEPT"] + "%";
                            //dtBindTable_temp.Rows[dtBindTable_temp_row]["W_ACCEPT_S"] = line_chk[j]["W_ACCEPT_S"] + "%";

                            dtBindTable_temp.Rows[dtBindTable_temp_row]["Y_PLAN"] = line_chk[j]["Y_PLAN"].ToString() == "" ? "-" : line_chk[j]["Y_PLAN"];

                            //if (dtBindTable_temp.Rows[dtBindTable_temp_row]["Y_PLAN"].ToString() == "0")
                            //{
                            //    dtBindTable_temp.Rows[dtBindTable_temp_row]["Y_PLAN"] = null;
                            //    line_chk[j]["Y_PLAN"] = "";
                            //}


                            if ((line_chk[j]["Y_PLAN"].ToString() == "" || line_chk[j]["Y_PLAN"].ToString() == "" ) && line_chk[j]["Y_RESULT"].ToString() == "")//계획도 없고 실적도 없을때 처리
                            {
                                dtBindTable_temp.Rows[dtBindTable_temp_row]["Y_PLAN"] = "-";
                                dtBindTable_temp.Rows[dtBindTable_temp_row]["Y_RESULT"] = "-";
                                dtBindTable_temp.Rows[dtBindTable_temp_row]["Y_ACCEPT"] = "100.0%";
                            }
                            else if (line_chk[j]["Y_PLAN"].ToString() != "" && line_chk[j]["Y_RESULT"].ToString() == "") //계획은 있고 실적이 없을때 처리
                            {
                                dtBindTable_temp.Rows[dtBindTable_temp_row]["Y_RESULT"] = "0";
                                dtBindTable_temp.Rows[dtBindTable_temp_row]["Y_ACCEPT"] = "0.0%";
                            }
                            else if (line_chk[j]["Y_PLAN"].ToString() == "" && line_chk[j]["Y_RESULT"].ToString() != "")
                            {
                                dtBindTable_temp.Rows[dtBindTable_temp_row]["Y_RESULT"] = line_chk[j]["Y_RESULT"];
                                dtBindTable_temp.Rows[dtBindTable_temp_row]["Y_ACCEPT"] = "100.0%";
                            }
                            else
                            {
                                dtBindTable_temp.Rows[dtBindTable_temp_row]["Y_RESULT"] = line_chk[j]["Y_RESULT"].ToString() == "" ? "" : line_chk[j]["Y_RESULT"];
                                dtBindTable_temp.Rows[dtBindTable_temp_row]["Y_ACCEPT"] = line_chk[j]["Y_ACCEPT"].ToString() == "" ? "0.0%" : line_chk[j]["Y_ACCEPT"].ToString() == "" ? "0.0%" : line_chk[j]["Y_ACCEPT"] + "%";
                            }


                            dtBindTable_temp.Rows[dtBindTable_temp_row]["T_PLAN"] = line_chk[j]["T_PLAN"].ToString() == "" ? "-" : line_chk[j]["T_PLAN"];

                            if (LoginInfo.USERID.Trim() == "cnswkdakscjf1")
                            {
                                dtBindTable_temp.Rows[dtBindTable_temp_row]["MOVE"] = "W";
                            }
                            else
                            {
                                dtBindTable_temp.Rows[dtBindTable_temp_row]["MOVE"] = line_chk[j]["M_STAT"].ToString() == "" ? "N/A" : line_chk[j]["M_STAT"];
                            }

                            //if (Convert.ToInt32(line_chk[j]["M_ACCEPT_S"]) >= 90)
                            //{
                            //    dtBindTable_temp.Rows[0]["LINE_MOVE1"] = " ";
                            //}
                            //else if (Convert.ToInt32(line_chk[j]["M_ACCEPT_S"]) >= 80 && Convert.ToInt32(line_chk[j]["M_ACCEPT_S"]) < 90)
                            //{
                            //    dtBindTable_temp.Rows[0]["LINE_MOVE2"] = " ";
                            //}
                            //else if (Convert.ToInt32(line_chk[j]["M_ACCEPT_S"]) >= 70 && Convert.ToInt32(line_chk[j]["M_ACCEPT_S"]) < 80)
                            //{
                            //    dtBindTable_temp.Rows[0]["LINE_MOVE3"] = " ";
                            //}
                            //else if (Convert.ToInt32(line_chk[j]["M_ACCEPT_S"]) < 70)
                            //{
                            //    dtBindTable_temp.Rows[0]["LINE_MOVE4"] = " ";
                            //}

                            //dtBindTable_temp.Rows[0]["M_ACCEPT_S"] = line_chk[j]["M_ACCEPT_S"] + "%";

                            dtBindTable_temp_row++;
                        }
                    }//내부 for 마지막
                }
                else
                {
                    dtBindTable.Rows[i - 1]["MODLID"] = "";
                    //dtBindTable.Rows[i - 1]["PRJT_NAME"] = "";
                }
            } //외부 for 마지막

            //라인에 멀티 모델인 경우 첫번째 모델을 뺀 나머지를 임시테이블에 담아 뒀었는데 
            //그 임시 테이블의 데이터들을 grid에 바인드할 바인드 테이블에 추가해준다.
            if (dtBindTable_temp != null && dtBindTable_temp.Rows.Count > 0)
            {
                foreach(DataRow drs in dtBindTable_temp.Rows)
                {
                    addBinTable_to_temp_row(drs); //임시로 담아둠 data를 bind datatable에 넣어줌.
                }

                for(int i = 0; i < dtBindTable.Rows.Count; i++) //sort하기 위해 0을 붙여줌.
                {
                    if(dtBindTable.Rows[i]["LINE_ABBR_CODE"].ToString().Length == 2)
                    {
                        dtBindTable.Rows[i]["LINE_ABBR_CODE"] = "0" + dtBindTable.Rows[i]["LINE_ABBR_CODE"];
                    }
                }

                dtSortTable = dtBindTable.Select("", "LINE_ABBR_CODE asc").CopyToDataTable<DataRow>(); //라인으로 정렬                

                for (int i = 0; i < dtSortTable.Rows.Count; i++)
                {
                    if (dtSortTable.Rows[i]["LINE_ABBR_CODE"].ToString().Substring(0,1) == "0")
                    {
                        dtSortTable.Rows[i]["LINE_ABBR_CODE"] = dtSortTable.Rows[i]["LINE_ABBR_CODE"].ToString().Substring(1);

                        if(dtSortTable.Rows[i]["LINE_ABBR_CODE"].ToString().Substring(0,1) == "0")
                        {
                            dtSortTable.Rows[i]["LINE_ABBR_CODE"] = dtSortTable.Rows[i]["LINE_ABBR_CODE"].ToString().Substring(1);
                        }
                    }
                }

                //dtBindTable.DefaultView.Sort = "LINE_ABBR_CODE ASC";
            }
            else
            {
                dtSortTable = dtBindTable.Select("", "LINE_ABBR_CODE asc").CopyToDataTable<DataRow>(); //라인으로 정렬
            }

            //string timedata = dtCon.ToString("yyyy") + ObjectDic.Instance.GetObjectName("년") + " " +
            //                    dtCon.ToString("MM") + ObjectDic.Instance.GetObjectName("월") + " " +
            //                    dtCon.ToString("dd") + ObjectDic.Instance.GetObjectName("일") + " " +
            //                    dtCon.ToString("hh") + ObjectDic.Instance.GetObjectName("시") + " " +
            //                    dtCon.ToString("mm") + ObjectDic.Instance.GetObjectName("분") + " " +
            //                    dtCon.ToString("ss") + ObjectDic.Instance.GetObjectName("초");

            //실적시간 세팅
            setRealDate();            

            //dbTateResult.Text = dtCon.ToString("yyyy") + ObjectDic.Instance.GetObjectName("년") + " " +
            //                    dtCon.ToString("MM") + ObjectDic.Instance.GetObjectName("월") + " " +
            //                    dtCon.ToString("dd") + ObjectDic.Instance.GetObjectName("일") + " " +
            //                    dtCon.ToString("hh") + ObjectDic.Instance.GetObjectName("시") + " " +
            //                    dtCon.ToString("mm") + ObjectDic.Instance.GetObjectName("분") + " " +
            //                    dtCon.ToString("ss") + ObjectDic.Instance.GetObjectName("초");

            dtSortTable.AcceptChanges();

            if (dgMONITORING.Dispatcher.CheckAccess())
            {
                 dgMONITORING.ItemsSource = null;
            }
            else
            {
                dgMONITORING.Dispatcher.BeginInvoke(new Action(() => dgMONITORING.ItemsSource = null));
            }            

           // dgMONITORING.ItemsSource = null;

            SetGridBind(dtSortTable); //화면 VIEW Row수에 설정된 row 갯수를 가지고 lotation 화면수 결정해서 번갈아 가며 띄움.

            dtSearchTable = null;
            dtBindTable = null;
            dtBindTable_temp = null;       
        }

        private void setRealDate()
        {
            DateTime dtCon = DateTime.Now;

            //2018-01-29
            //string timedata = dtCon.ToString("yyyy") + "년 " +
            //                    dtCon.ToString("MM") + "월 " +
            //                    dtCon.ToString("dd") + "일 " +
            //                    dtCon.ToString("HH") + "시 " +
            //                    dtCon.ToString("mm") + "분 ";
            string timedata = dtCon.ToString("yyyy") + "- " +
                    dtCon.ToString("MM") + "- " +
                    dtCon.ToString("dd") + "- " +
                    dtCon.ToString("HH") + ": " +
                    dtCon.ToString("mm") + ": ";

            SetUiCondition(dbTateResult, timedata);
        }

        private void SetGridBind(DataTable dtBind)
        {
            viewRowCnt = Convert.ToInt32(GetUIControl(numViewRowCnt));
            bindTableRowCnt = dtBind.Rows.Count;
            rotationView = Convert.ToInt32(GetUIControl(numRefreshSub));

            if (viewRowCnt >= bindTableRowCnt || viewRowCnt == 0) //보고싶은 Row가 bindTable의 row갯수보다 크면 그냥 bind함.
            {  
                //dtDiv = null;

                //dtDiv = new DataTable[1]; //table 5개

                //dtDiv[0] = dtBind.Clone();

                //dtDiv[0].ImportRow(dtBind.Rows[0]);              

                //if (dgMONITORING.Dispatcher.CheckAccess())
                //{
                //    dgMONITORING.ItemsSource = DataTableConverter.Convert(dtDiv[0]);
                //}
                //else
                //{
                //    dgMONITORING.Dispatcher.BeginInvoke(new Action(() => dgMONITORING.ItemsSource = DataTableConverter.Convert(dtDiv[0])));
                //}

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

                //dgMONITORING.ItemsSource = DataTableConverter.Convert(dtDiv[0]); //첫번째 화면 

                if (dgMONITORING.Dispatcher.CheckAccess())
                {
                    dgMONITORING.ItemsSource = DataTableConverter.Convert(dtDiv[0]);
                }
                else
                {
                    dgMONITORING.Dispatcher.BeginInvoke(new Action(() => dgMONITORING.ItemsSource = DataTableConverter.Convert(dtDiv[0])));
                }

                realViewCnt = 1; //첫 화면 먼저 뿌리므로 화면번호는 1임.
            }

            string msg = realViewCnt + " / " + div;
            SetUiCondition(tbPage, msg);          
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

        private void NowDate_Start_Timer()
        {
            _timer_NowDate = new System.Timers.Timer(1000); //기준 : 초
            _timer_NowDate.AutoReset = true;
            _timer_NowDate.Elapsed += (s, arg) =>
            {
                acceptCalDate(); //06시 적용된 현재 날짜

                DateTime dt = standart_date;
                //2018-01-29
                //tbdate.Dispatcher.BeginInvoke(new Action(() => tbdate.Text =    dt.ToString("yyyy") + "년 " +
                //                                                                dt.ToString("MM") + "월 " +
                //                                                                dt.ToString("dd") + "일 " +
                //                                                                dt.ToString("HH") + "시 " +
                //                                                                dt.ToString("mm") + "분 " +
                //                                                                dt.ToString("ss") + "초"
                //                                       )
                //                             );
                tbdate.Dispatcher.BeginInvoke(new Action(() => tbdate.Text = dt.ToString("yyyy") + "- " +
                                                                dt.ToString("MM") + "- " +
                                                                dt.ToString("dd") + "- " +
                                                                dt.ToString("HH") + ": " +
                                                                dt.ToString("mm") + ": " +
                                                                dt.ToString("ss") + ":"
                                       )
                             );

                DateTime dt1 = accept06_date;
                //2018-01-29
                //dbAcepptResult.Dispatcher.BeginInvoke(new Action(() => dbAcepptResult.Text =    dt1.ToString("yyyy") + "년 " +
                //                                                                                dt1.ToString("MM") + "월 " +
                //                                                                                dt1.ToString("dd") + "일 " +
                //                                                                                dt1.ToString("HH") + "시 " +
                //                                                                                dt1.ToString("mm") + "분 " +
                //                                                                                dt1.ToString("ss") + "초"
                //                                       )
                //                             );
                dbAcepptResult.Dispatcher.BeginInvoke(new Action(() => dbAcepptResult.Text = dt1.ToString("yyyy") + "- " +
                                                                                dt1.ToString("MM") + "- " +
                                                                                dt1.ToString("dd") + "- " +
                                                                                dt1.ToString("HH") + ": " +
                                                                                dt1.ToString("mm") + ": " +
                                                                                dt1.ToString("ss") + ":"
                                       )
                             );

            };
            _timer_NowDate.Start();
        }

        //private void DataSearch_Start_Timer()
        //{
        //    this.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        // int refresh = Convert.ToInt32(numRefresh.Value) * 60000; //기준 : 분
        //        int refresh = Convert.ToInt32(Util.GetCondition_Thread(numRefresh)) * 1000; //테스트 위해 초로 바꿈

        //        _timer_DataSearch = new System.Timers.Timer(refresh);
        //        _timer_DataSearch.AutoReset = true;
        //        _timer_DataSearch.Elapsed += (s, arg) =>
        //        {
        //            try
        //            {
        //                realData();
        //            }
        //            catch(Exception ex)
        //            {
        //                Util.Alert(ex.Message);
        //            }
                    

        //            //Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
        //            //{
        //            //    try
        //            //    {
        //            //        realData();
        //            //    }
        //            //    catch(Exception ex)
        //            //    {
        //            //        Util.Alert(ex.Message);
        //            //    }     
        //            //}));

        //        };
        //        _timer_DataSearch.Start();
        //    }));
        //}

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
                    dgMONITORING.ItemsSource = DataTableConverter.Convert(dtDiv[nowviewPoint]);
                }
                else
                {

                   Action act = () => { dgMONITORING.ItemsSource = DataTableConverter.Convert(dtDiv[nowviewPoint]); };

                    dgMONITORING.Dispatcher.BeginInvoke(act);
                }

                //dgMONITORING.Dispatcher.BeginInvoke(new Action(() => dgMONITORING.ItemsSource = DataTableConverter.Convert(dtDiv[nowviewPoint])));

                realViewCnt++;

                string msg = realViewCnt + " / " + div;
                SetUiCondition(tbPage, msg);               
            }
            catch(Exception ex)
            {
                Util.Alert(ex.Message);
            }
           
        }

        #endregion

        #region Event       

        private void dgMONITORING_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            //try
            //{
            //    C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            //    dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        if(e.Column.Name.Equals("전일"))
            //        {
            //            e.Column.Name = e.Column.Name + "(" + DateTime.Now.AddDays(-1).ToString("MM-dd") + ")";
            //        }
            //    }));
            //}
            //catch(Exception ex)
            //{
            //    Util.Alert(ex.ToString());
            //}
        }

        private void dgMONITORING_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                /*
                C1DataGrid dataGrid = (sender as C1DataGrid);
                if (DataGridExtension.GetIsAlternatingRow(dataGrid) == true)
                {
                    //System.Diagnostics.Debug.WriteLine(dg.Name.ToString() + " - " + Convert.ToString(DataGridExtension.GetIsAlternatingRow(dg)));
                    if (e.Cell.Row.Index <= e.Cell.DataGrid.TopRows.Count)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Top)
                        {
                            //e.Cell.Presenter.Background = (e.Cell.Row.Index % 2) == 0 ? e.Cell.DataGrid.RowBackground : e.Cell.DataGrid.AlternatingRowBackground;
                            //e.Cell.Presenter.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                            //e.Cell.Presenter.VerticalAlignment = VerticalAlignment.Stretch;
                        }
                    }
                }
                */

                C1DataGrid dataGrid = (sender as C1DataGrid);

                if (!dataGrid.Dispatcher.CheckAccess()) //main UI 에서 호출
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    switch (e.Cell.Column.Name)
                    {
                        case "LINE_ABBR_CODE": //생산라인
                                               //e.Cell.Column.Width = e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(50);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(100);
                            break;
                        case "MODLID": //모델ID
                                       //e.Cell.Column.Width = e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(110);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            //e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(oneColumn_width);
                            //2019.02.27
                            //e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(500);
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(330);
                            break;
                        //2019.02.27
                        case "WOTYPE":
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(180);
                            break;
                        case "PRJT_NAME": //프로젝트 코드
                                          //e.Cell.Column.Width = e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(110);
                            //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                            //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            //e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(220);
                            break;
                        case "Y_PLAN":   //전일 : 계획
                        case "D_PLAN":   //금일 : 계획
                        case "T_PLAN":   //익일 : 계획  
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            //e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(150);
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(oneColumn_width);
                            break;
                        case "Y_RESULT": //전일 : 실적                        
                        case "D_RESULT": //금일 : 실적                                                                      
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(95);
                            //e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(oneColumn_width);
                            break;
                        case "D_ACCEPT": //전일 : 달성률
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(130);

                            break;
                        case "Y_ACCEPT": //금일 : 달성률
                            if (e.Cell.Value != null  && e.Cell.Value.ToString() != "")
                            {
                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(130);
                                //e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(150);
                                setCellColor(e);//실적에 맞는 색깔 지정   
                            }
                            break;
                        case "MOVE": //금일 : 가동상태
                                     //e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(200);
                            if (e.Cell.Value != null && e.Cell.Value.ToString() != "")
                            {
                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(140);
                                setCellColor(e);//실적에 맞는 색깔 지정   
                            }
                          
                            break;
                        default:
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.White);
                            dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.White);
                            dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.White);

                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(oneColumn_width);
                            break;
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

                        switch (e.Cell.Column.Name)
                        {
                            case "LINE_ABBR_CODE": //생산라인
                                                   //e.Cell.Column.Width = e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(50);
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(100);
                                //2019.02.27
                                //e.Cell.Presenter.FontSize = 50;
                                e.Cell.Presenter.FontSize = 35;
                                break;
                            case "MODLID": //모델ID
                                           //e.Cell.Column.Width = e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(110);
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(oneColumn_width);
                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(330);
                                //2019.02.27
                                //e.Cell.Presenter.FontSize = 55;
                                e.Cell.Presenter.FontSize = 35;
                                break;
                            //2019.02.27
                            case "WOTYPE": 
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(oneColumn_width);
                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(180);
                                e.Cell.Presenter.FontSize = 35;
                                break;
                            case "PRJT_NAME": //프로젝트 코드
                                              //e.Cell.Column.Width = e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(110);
                                //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                                //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                //e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(220);
                                break;
                           
                            case "Y_PLAN":   //전일 : 계획
                            case "D_PLAN":   //금일 : 계획
                            case "T_PLAN":   //익일 : 계획                            
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                //e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(150);
                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(oneColumn_width);
                                e.Cell.Presenter.FontSize = 90;                              
                                break;

                            case "Y_RESULT": //전일 : 실적
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                //e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(180);
                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(oneColumn_width-5);
                                e.Cell.Presenter.FontSize = 90;
                                //e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(oneColumn_width);
                                break;
                            case "D_ACCEPT": //전일 : 달성률
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(125);
                                e.Cell.Presenter.FontSize = 35;

                                break;

                            case "D_RESULT": //금일 : 실적   
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);

                                e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontSize = 90;

                                //e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(180);
                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(oneColumn_width-5);
                                break;

                            case "Y_ACCEPT": //금일 : 달성률
                                if (e.Cell.Value != null && e.Cell.Value.ToString() != "")
                                {
                                    e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(125);
                                    //e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(150);
                                    setCellColor(e);//실적에 맞는 색깔 지정   
                                }
                                e.Cell.Presenter.FontSize = 35;
                                break;

                            case "MOVE": //금일 : 가동상태
                                         //e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(200);
                                if (e.Cell.Value != null && e.Cell.Value.ToString() != "")
                                {
                                    e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(80);
                                    setCellColor(e);//실적에 맞는 색깔 지정   
                                }
                                //2019.02.27
                                e.Cell.Presenter.FontSize = 35;

                                break;
                            default:
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.White);
                                e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.White);
                                e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.White);
                                e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.White);
                                //dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.White);
                                //dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.White);

                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(oneColumn_width);
                                break;
                        }
                    };

                    dataGrid.Dispatcher.BeginInvoke(act);
                }

                /*
               
                                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    if (e.Cell.Presenter == null)
                                    {
                                        return;
                                    }

                                    switch (e.Cell.Column.Name)
                                    {
                                        case "LINE_ABBR_CODE": //생산라인
                                            //e.Cell.Column.Width = e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(50);
                                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(100);
                                            break;
                                        case "MODLID": //모델ID
                                            //e.Cell.Column.Width = e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(110);
                                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(oneColumn_width);
                                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(300);
                                            break;
                                        case "PRJT_NAME": //프로젝트 코드
                                            //e.Cell.Column.Width = e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(110);
                                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(220);
                                            break;
                                        //case "M_ACCEPT_S":
                                        //    setCellColor(e); //실적에 맞는 색깔 지정   
                                        //    break;
                                        //case "W_ACCEPT_S":
                                        //    setCellColor(e); //실적에 맞는 색깔 지정   
                                        //    break;
                                        //case "Y_ACCEPT":
                                        //    setCellColor(e); //실적에 맞는 색깔 지정   
                                        //    break;                      

                                        //case "Y_PLAN":
                                        case "Y_RESULT": //전일 : 실적
                                        case "T_PLAN":   //익일 : 계획
                                        case "D_PLAN":   //금일 : 계획
                                        case "D_RESULT": //금일 : 실적                        
                                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                            //e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(150);
                                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(oneColumn_width);
                                            break;
                                        case "D_ACCEPT": //전일 : 달성률
                                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(150);

                                            break;
                                        case "Y_ACCEPT": //금일 : 달성률
                                            if (e.Cell.Value.ToString() != "")
                                            {
                                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(150);
                                                //e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(150);
                                                setCellColor(e);//실적에 맞는 색깔 지정   
                                            }                          
                                            break;
                                        case "MOVE": //금일 : 가동상태
                                            //e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(200);
                                            if (e.Cell.Value.ToString() != "")
                                            {
                                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(200);
                                                setCellColor(e);//실적에 맞는 색깔 지정   
                                            }
                                            else
                                            {
                                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);                                
                                            }
                                            break;                       
                                        //case "LINE_MOVE1":
                                        //    if (e.Cell.Value.ToString() == " ")
                                        //    {
                                        //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.DeepSkyBlue);  
                                        //    }
                                        //    else
                                        //    {
                                        //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gray);
                                        //    }

                                        //    e.Cell.Column.Width = e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(40);

                                        //    break;
                                        //case "LINE_MOVE2":
                                        //    if (e.Cell.Value.ToString() == " ")
                                        //    {
                                        //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LawnGreen);

                                        //    }
                                        //    else
                                        //    {
                                        //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gray);
                                        //    }

                                        //    e.Cell.Column.Width = e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(40);
                                        //    break;
                                        //case "LINE_MOVE3":
                                        //    if (e.Cell.Value.ToString() == " ")
                                        //    {
                                        //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);

                                        //    }
                                        //    else
                                        //    {
                                        //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gray);
                                        //    }

                                        //    e.Cell.Column.Width = e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(40);
                                        //    break;
                                        //case "LINE_MOVE4":
                                        //    if (e.Cell.Value.ToString() == " ")
                                        //    {
                                        //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);                                
                                        //    }
                                        //    else
                                        //    {
                                        //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gray);
                                        //    }

                                        //    e.Cell.Column.Width = e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(40);
                                        //    break;                       
                                        default:
                                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                            e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.White);
                                            e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.White);
                                            e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.White);
                                            e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.White);
                                            dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.White);
                                            dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.White);

                                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(oneColumn_width);
                                            break;
                                    }

                                }));
*/


                /*
                if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    switch (e.Cell.Column.Name)
                    {
                        case "LINE_ABBR_CODE": //생산라인
                                               //e.Cell.Column.Width = e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(50);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(100);
                            break;
                        case "MODLID": //모델ID
                                       //e.Cell.Column.Width = e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(110);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(oneColumn_width);
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(300);
                            break;
                        case "PRJT_NAME": //프로젝트 코드
                                          //e.Cell.Column.Width = e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(110);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(220);
                            break;                      
                        case "Y_RESULT": //전일 : 실적
                        case "T_PLAN":   //익일 : 계획
                        case "D_PLAN":   //금일 : 계획
                        case "D_RESULT": //금일 : 실적                        
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            //e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(150);
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(oneColumn_width);
                            break;
                        case "D_ACCEPT": //전일 : 달성률
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(150);

                            break;
                        case "Y_ACCEPT": //금일 : 달성률
                            if (e.Cell.Value.ToString() != "")
                            {
                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(150);
                                //e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(150);
                                setCellColor(e);//실적에 맞는 색깔 지정   
                            }
                            break;
                        case "MOVE": //금일 : 가동상태
                                     //e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(200);
                            if (e.Cell.Value.ToString() != "")
                            {
                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(200);
                                setCellColor(e);//실적에 맞는 색깔 지정   
                            }
                            else
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                            }
                            break;                                       
                        default:
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.White);
                            dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.White);
                            dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.White);

                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(oneColumn_width);
                            break;
                    }
        */      

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
       
        private void applyObejctDicToColumn(DataGridColumn column)
        {
            if (column.Header is List<string>)
            {
                string[] headers = (column.Header as List<string>).ToArray();

                for(int i = 0; i < headers.Length; i++)
                {
                    headers[i] = headers[i].Replace("[#] ", "");
                }

                if(headers[0].Contains("전일") || headers[0].Contains("금일") || headers[0].Contains("익일"))
                {
                    if(headers[0].Contains("전일"))
                    {
                        //2018-01-29
                        // headers[0] = "전일" + "(" + yester_date_header + ")"; 
                        ////headers[0] = ObjectDic.Instance.GetObjectName(headers[0]) + "(" + DateTime.Now.AddDays(-1).ToString("MM/dd") + ")";
                        ////headers[0] = headers[0] + "(" + DateTime.Now.AddDays(-1).ToString("MM-dd") + ")"; //now_date_header

                        if (headers[0] == "전일")
                        {
                            headers[0] = ObjectDic.Instance.GetObjectName("전일") + "(" + yester_date_header + ")"; //now_date_header
                        }
                        else
                        {
                            headers[0] = "전일" + "(" + yester_date_header + ")";
                        }
                    }
                    else if(headers[0].Contains("금일"))
                    {
                        //2018-01-29
                        //headers[0] = "금일" + "(" + now_date_header + ")";
                        //// headers[0] = ObjectDic.Instance.GetObjectName(headers[0]) + "(" + DateTime.Now.ToString("MM/dd") + ")";
                        //// headers[0] = headers[0] + "(" + DateTime.Now.ToString("MM-dd") + ")";

                        if (headers[0] == "금일")
                        {
                            headers[0] = ObjectDic.Instance.GetObjectName("금일") + "(" + now_date_header + ")";
                        }
                        else
                        {
                            headers[0] = "금일" + "(" + now_date_header + ")";
                        }                       
                    }
                    else if(headers[0].Contains("익일"))
                    {
                        //2018-01-29
                        //headers[0] = "익일" + "(" + tomorrow_date_header + ")";
                        //    //headers[0] = ObjectDic.Instance.GetObjectName(headers[0]) + "(" + DateTime.Now.AddDays(+1).ToString("MM/dd") + ")";
                        //    headers[0] = headers[0] + "(" + DateTime.Now.AddDays(+1).ToString("MM-dd") + ")";

                        if (headers[0] == "익일")
                        {
                            headers[0] = ObjectDic.Instance.GetObjectName("익일") + "(" + tomorrow_date_header + ")";
                        }
                        else
                        {
                            headers[0] = "익일" + "(" + tomorrow_date_header + ")";
                        }
                    }
                }
                else if(headers[0] == "생산라인" || headers[0] == "생산\r\n라인")
                {
                    //2018-01-29
                    //headers[0] = "생산" + "\r\n" + "라인";
                    //headers[1] = "생산" + "\r\n" + "라인";
                    headers[0] = ObjectDic.Instance.GetObjectName("생산") + "\r\n" + ObjectDic.Instance.GetObjectName("라인");
                    headers[1] = ObjectDic.Instance.GetObjectName("생산") + "\r\n" + ObjectDic.Instance.GetObjectName("라인");
                }   

                    column.Header = new List<string>(headers);
            }
            else if (column.Header is string)
            {
                //column.Header = Common.ObjectDic.Instance.GetObjectName(column.Header.ToString());
                column.Header = column.Header.ToString();
            }
        }

        private void setCellColor(C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            string value = e.Cell.Value.ToString();
            double value_non_percent;
            int conv_value;

            if (e.Cell.Row.Index > 1)
            {
                if(e.Cell.Column.Name == "Y_ACCEPT")
                {
                    value_non_percent = value.Substring(0, value.IndexOf("%")) == "0.0" ? 0 : Convert.ToDouble(value.Substring(0, value.IndexOf("%")));
                    conv_value = Convert.ToInt32(Math.Round(value_non_percent));

                    //e.Cell.Column.Name;
                    if (conv_value < 90 && conv_value != 0)  //달성률 < 90
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                    }
                    else if (90 <= conv_value && conv_value < 100) // 90 <= 달성률 <100
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else if(conv_value >= 100) // 90 <= 달성률 <100
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }    
                    else if(conv_value == 0)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                    }         
                }
                else if(e.Cell.Column.Name == "MOVE")
                {
                    if (e.Cell.Value.ToString() =="W")  
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else if (e.Cell.Value.ToString() == "R") 
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else if (e.Cell.Value.ToString() == "U")
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gray);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else if (e.Cell.Value.ToString() == "T")
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                    }
                    else if (e.Cell.Value.ToString() == "F")
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                    }
                    else //미설정
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                    }
                }
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDate == null || dtpDate.SelectedDateTime == null)
            {
                return;
            }

            //realData();
        }

        private void tbTitle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dtpDate.Visibility == Visibility.Visible)
            {
                dtpDate.Visibility = Visibility.Hidden;
            }
            else
            {
                dtpDate.Visibility = Visibility.Visible;
            }
        }

        private void btnSetSave_Click(object sender, RoutedEventArgs e)
        {
            selectedShop = Convert.ToString(cboShop.SelectedValue);
            selectedArea = Convert.ToString(cboArea.SelectedValue);
            selectedEquipmentSegment = Convert.ToString(cboEquipmentSegment.SelectedItemsToString);
            //selectedEquipment = Convert.ToString(cboEquipment.SelectedItemsToString);

            // selectedDisplayName = Convert.ToString(txtScreenName.Text);
            seletedViewRowCnt = Convert.ToInt32(numViewRowCnt.Value);
             selectedDisplayTime = Convert.ToInt32(numRefresh.Value);
            selectedDisplayTimeSub = Convert.ToInt32(numRefreshSub.Value); //

            DataSet ds = new DataSet();

            DataTable dt = new DataTable("MNT_CONFIG");
            dt.Columns.Add("SHOP", typeof(string));
            dt.Columns.Add("AREA", typeof(string));
            dt.Columns.Add("EQUIPMENTSEGMENT", typeof(string));
            //dt.Columns.Add("EQUIPMENT", typeof(string));
            //dt.Columns.Add("DISPLAYNAME", typeof(string));
            dt.Columns.Add("DISPLAYTIME", typeof(int));
            dt.Columns.Add("DISPLAYTIMESUB", typeof(int));
            dt.Columns.Add("DISPLAYVIEWROWCNT", typeof(int));


            DataRow dr = dt.NewRow();
            dr["SHOP"] = selectedShop;
            dr["AREA"] = selectedArea;
            dr["EQUIPMENTSEGMENT"] = selectedEquipmentSegment;
            //dr["EQUIPMENT"] = selectedEquipment;
            //dr["DISPLAYNAME"] = selectedDisplayName;
            dr["DISPLAYTIME"] = selectedDisplayTime;
            dr["DISPLAYTIMESUB"] = selectedDisplayTimeSub;
            dr["DISPLAYVIEWROWCNT"] = seletedViewRowCnt;

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
            //GetDataMain("Main");
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
                    //selectedEquipment = string.Empty;
                    SetArea(cboArea);
                    //Set_Combo_Area(cboArea);
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
                    SetCboLine(cboEquipmentSegment, Util.NVC(cboShop.SelectedValue), Util.NVC(cboArea.SelectedValue));
                    cboEquipmentSegment.isAllUsed = false;
                }
                else
                {
                    selectedArea = string.Empty;
                }
            }));
        }
        
        //ETC ==============================================================================================

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

        #endregion

        #region Mehod

        //private void Init_Combo()
        //{
        //    Set_Combo_Shop(cboShop, ex01 =>
        //    {
        //        cboShop.SelectedValue = selectedShop;
        //        Set_Combo_Area(cboArea, ex02 =>
        //        {
        //            cboArea.SelectedValue = selectedArea;
        //            Set_Combo_EquipmentSegment(cboEquipmentSegment, ex03 =>
        //            {
        //                //cboEquipmentSegment.SelectedValue = selectedEquipmentSegment;
        //                Set_Combo_Equipment(cboEquipment, ex04 =>
        //                {
        //                    string[] split = selectedEquipment.Split(',');
        //                    foreach (string str in split)
        //                    {
        //                        cboEquipment.Check(str);
        //                    }

        //                    cboShop.SelectedItemChanged -= cboShop_SelectedItemChanged;
        //                    cboShop.SelectedItemChanged += cboShop_SelectedItemChanged;
        //                    cboArea.SelectedItemChanged -= cboArea_SelectedItemChanged;
        //                    cboArea.SelectedItemChanged += cboArea_SelectedItemChanged;
        //                    //cboEquipmentSegment.SelectedItemChanged -= cboEquipmentSegment_SelectedItemChanged;
        //                    //cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
        //                    cboEquipment.SelectionChanged -= cboEquipment_SelectionChanged;
        //                    cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;

        //                    realData();
        //                    //GetDataMain("Main");
        //                });
        //            });
        //        });
        //    });
        //}

       

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

            NowDate_Start_Timer(); //시간 타이머

            DataSearch_Start_Timer(); //data Search 타이머
        }
    }
}
