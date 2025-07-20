/*************************************************************************************
 Created Date : 2022. 01. 03
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
 2022. 01. 03   SI WA3동 자재 투입 현황 모니터링
 2022. 01. 27   SI 오픈 에러 수정
 2023. 09. 18  1065185  폴란드 모듈3동 자재 투입모니터링 에러메시지 오류수정 건,화면 Close 후 타이머 적용수정
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
    public partial class MNT001_020 : Window
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

        DateTime standart_date; //Test를 위해 test 기준 date
        DateTime accept06_date; //06시 적용 Date
        string now_date_header = string.Empty;
        string yester_date_header = string.Empty;
        string tomorrow_date_header = string.Empty;
        

        public MNT001_020()
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

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            timers_dispose(); //화면 종료시 타이머 모두 죽임.
            tmrMainTimer.AutoReset = false;
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
                this.Tag = new object[] { ObjectDic.Instance.GetObjectName("자재 투입 현황") };

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

                sbExpandLeft = (Storyboard)this.Resources["ExpandLeftFrameStoryboard"];
                sbCollapseLeft = (Storyboard)this.Resources["CollapseLeftFrameStoryboard"];

                DataSet ds = LGC.GMES.MES.MNT001.MNT_Common.GetConfigXML_PACK();

                selectedShop = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["SHOP"]);
                selectedArea = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["AREA"]);
                selectedEquipmentSegment = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["EQUIPMENTSEGMENT"]);
               // selectedProcess = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["PROCESS"]);

                //selectedDisplayName = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["DISPLAYNAME"]);
                selectedDisplayTime = Convert.ToInt32(ds.Tables["MNT_CONFIG"].Rows[0]["DISPLAYTIME"]);

                if (ds.Tables["MNT_CONFIG"].Columns["DISPLAYTIMESUB"] == null)
                {
                    selectedDisplayTimeSub = 10;
                }
                else
                {
                    selectedDisplayTimeSub = Convert.ToInt32(ds.Tables["MNT_CONFIG"].Rows[0]["DISPLAYTIMESUB"]);
                }
                Init_Combo();


                numRefresh.Value = selectedDisplayTime;
               // tbTitle.Text = dtMainStartTime.ToString("yyyy-MM-dd HH:mm:ss");

                tmrMainTimer = new System.Timers.Timer(0.5 * 1000);
                tmrMainTimer.AutoReset = true;
                tmrMainTimer.Elapsed += (s, arg) =>
                {
                   // tbTitle.Dispatcher.BeginInvoke(new Action(() => tbTitle.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    TimeSpan spanMain = (DateTime.Now - dtMainStartTime);
                    if (spanMain.Minutes >= selectedDisplayTime)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() => realData()));
                        dtMainStartTime = System.DateTime.Now;
                     }
                };
                tmrMainTimer.Start();

                realData(); //data 가져와서 그리드에 바인딩 시켜준 후 화면전환 타이머 작동
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }

        private void Init_Combo()
        {
            Set_Combo_Shop(cboShop, ex01 =>
            {
                cboShop.SelectedValue = selectedShop;
                Set_Combo_Area(cboArea, ex02 =>
                {
                    cboArea.SelectedValue = selectedArea;
                    Set_Combo_EquipmentSegment(cboEquipmentSegment, ex03 =>
                    {
                        //cboEquipmentSegment.SelectedItemsToString = selectedEquipmentSegment;
                        string[] split = selectedEquipmentSegment.Split(',');
                        foreach (string str in split)
                        {
                            cboEquipmentSegment.Check(str);
                        }
                        cboShop.SelectedItemChanged -= cboShop_SelectedItemChanged;
                        cboShop.SelectedItemChanged += cboShop_SelectedItemChanged;
                        cboArea.SelectedItemChanged -= cboArea_SelectedItemChanged;
                        cboArea.SelectedItemChanged += cboArea_SelectedItemChanged;
                        cboEquipmentSegment.SelectionChanged -= cboEquipmentSegment_SelectionChanged;
                        cboEquipmentSegment.SelectionChanged += cboEquipmentSegment_SelectionChanged;
                        realData();
                    });
                });
            });
        }

        private void Set_Combo_Shop(C1ComboBox cbo, Action<Exception> action = null)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";

            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
            dtRQSTDT.Columns.Add("USERID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["SYSTEM_ID"] = LoginInfo.SYSID;
            drnewrow["USERID"] = LoginInfo.USERID;
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_AUTH_SHOP_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if (result.Rows.Count == 0)
                    {
                        selectedShop = string.Empty;
                    }
                    if (action != null)
                    {
                        action(Exception);
                    }
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + "\r\n" + ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + " : " + new System.Diagnostics.StackFrame(0, true).GetMethod().Name, ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + " : " + new System.Diagnostics.StackFrame(0, true).GetMethod().Name, Logger.MESSAGE_OPERATION_END);
                }
            });
        }

        private void Set_Combo_Area(C1ComboBox cbo, Action<Exception> action = null)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";

            dtRQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("SHOPID", typeof(string));
            dtRQSTDT.Columns.Add("USERID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["SYSTEM_ID"] = LoginInfo.SYSID;
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            drnewrow["USERID"] = LoginInfo.USERID;
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_AUTH_AREA_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if (result.Rows.Count == 0)
                    {
                        selectedArea = string.Empty;
                    }
                    cbo.SelectedIndex = 0;
                    if (action != null)
                    {
                        action(Exception);
                    }
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + "\r\n" + ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + " : " + new System.Diagnostics.StackFrame(0, true).GetMethod().Name, ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + " : " + new System.Diagnostics.StackFrame(0, true).GetMethod().Name, Logger.MESSAGE_OPERATION_END);
                }
            });
        }

        private void Set_Combo_EquipmentSegment(MultiSelectionBox cbo, Action<Exception> action = null)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("SHOPID", typeof(string));
            dtRQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            drnewrow["AREAID"] = string.IsNullOrEmpty(selectedArea) ? LoginInfo.CFG_AREA_ID : selectedArea;  //selectedArea;
            dtRQSTDT.Rows.Add(drnewrow);

            DataTable dt = new DataTable();
            dt.Columns.Add(cbo.DisplayMemberPath.ToString(), typeof(string));
            dt.Columns.Add(cbo.SelectedValuePath.ToString(), typeof(string));

            new ClientProxy().ExecuteService("DA_PRD_SEL_SHOP_AREA_EQSG_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK,  ControlsLibrary.MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if (result.Rows.Count == 0)
                    {
                        selectedEquipmentSegment = string.Empty;
                    }

                    if (action != null)
                    {
                        action(Exception);
                    }
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + "\r\n" + ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + " : " + new System.Diagnostics.StackFrame(0, true).GetMethod().Name, ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + " : " + new System.Diagnostics.StackFrame(0, true).GetMethod().Name, Logger.MESSAGE_OPERATION_END);
                }
            });
        }

        private void cboShop_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboShop.SelectedIndex > -1)
                {
                    selectedShop = Convert.ToString(cboShop.SelectedValue);
                    //selectedEquipment = string.Empty;
                    Set_Combo_Area(cboArea);
                }
                else
                {
                    selectedShop = string.Empty;
                }
            }));
        }

        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboArea.SelectedIndex > -1)
                {
                    selectedArea = Convert.ToString(cboArea.SelectedValue);
                    Set_Combo_EquipmentSegment(cboEquipmentSegment);
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
                selectedEquipmentSegment = cboEquipmentSegment.SelectedItemsToString;
            }));
        }

        private void realData()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                string area = string.IsNullOrEmpty(selectedArea) ? LoginInfo.CFG_AREA_ID : selectedArea;
                string lines = string.IsNullOrEmpty(selectedEquipmentSegment) ? LoginInfo.CFG_EQSG_ID : selectedEquipmentSegment;
         

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = area == "" ? null : area;// Util.GetCondition(cboEquipmentSegment) == "" ? null : Util.GetCondition(cboEquipmentSegment);
                dr["EQSGID"] = lines == "" ? null : lines;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                dtSearchTable = new ClientProxy().ExecuteServiceSync("BR_MTRL_REG_EQSG_MTRL_BF_QTY", "INDATA", "OUTDATA", RQSTDT);
   
                if (dtSearchTable == null || dtSearchTable.Rows.Count == 0)
                {
                    return;
                }
                else
                {
                    dgMONITORING.ItemsSource = DataTableConverter.Convert(dtSearchTable);
                    dtSearchTable = null;
                }
}
            catch (Exception ex)
            {
                throw ex;
            }
        }
      
        #endregion

        #region Event       

        private void dgMONITORING_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
        }

        private void dgMONITORING_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

                Boolean columnN = dgMONITORING.Columns[e.Cell.Column.Index].Name.Contains("_YN");
                if (columnN == true)
                {
                    if (dgMONITORING.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Value.ToString().Equals("Y"))
                    {
                        dgMONITORING.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - 1).Presenter.Background = new SolidColorBrush(Colors.Red);
                        return;
                    }
                    else
                    {
                        dgMONITORING.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - 1).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        return;
                    }
                }
              }
            ));

        }

        //ETC ==============================================================================================

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

        private void btnSetSave_Click(object sender, RoutedEventArgs e)
        {
            selectedShop = Convert.ToString(cboShop.SelectedValue);
            selectedArea = Convert.ToString(cboArea.SelectedValue);
            selectedEquipmentSegment = Convert.ToString(cboEquipmentSegment.SelectedItemsToString);
            //selectedProcess = Convert.ToString(cboProcess.SelectedValue); // Util.GetCondition(cboProcess);

            //selectedDisplayName = Convert.ToString(txtScreenName.Text);
            selectedDisplayTime = Convert.ToInt32(numRefresh.Value);
            //selectedDisplayTimeSub = Convert.ToInt32(numRefreshSub.Value);

            DataSet ds = new DataSet();

            DataTable dt = new DataTable("MNT_CONFIG");
            dt.Columns.Add("SHOP", typeof(string));
            dt.Columns.Add("AREA", typeof(string));
            dt.Columns.Add("EQUIPMENTSEGMENT", typeof(string));
            dt.Columns.Add("PROCESS", typeof(string));
            //dt.Columns.Add("DISPLAYNAME", typeof(string));
            dt.Columns.Add("DISPLAYTIME", typeof(int));
            //dt.Columns.Add("DISPLAYTIMESUB", typeof(int));

            DataRow dr = dt.NewRow();
            dr["SHOP"] = selectedShop;
            dr["AREA"] = selectedArea;
            dr["EQUIPMENTSEGMENT"] = selectedEquipmentSegment;
            //dr["PROCESS"] = Util.GetCondition(cboProcess);
            //dr["DISPLAYNAME"] = selectedDisplayName;
            dr["DISPLAYTIME"] = selectedDisplayTime;
           // dr["DISPLAYTIMESUB"] = selectedDisplayTimeSub;

            dt.Rows.Add(dr);
            ds.Tables.Add(dt);

           // GetProcess();

            LGC.GMES.MES.MNT001.MNT_Common.SetProcessConfigXML(ds);

            dtMainStartTime = System.DateTime.Now;
            realData();
            btnLeftFrame.IsChecked = false;
        }

        private void btnSetClose_Click(object sender, RoutedEventArgs e)
        {
                        btnLeftFrame.IsChecked = false;
        }
    }
}
