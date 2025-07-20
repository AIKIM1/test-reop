/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_029_monitoring : Window, IWorkArea
    {
        #region Declaration & Constructor 
        DataTable dtBindTable;
        DataTable dtSearchTable;
        private Timer _timer;
        private Timer _timer2;
        public DataTable DefectData { get; set; }

        private Storyboard sbExpandLeft;
        private Storyboard sbCollapseLeft;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_029_monitoring()
        {
            InitializeComponent();

            this.Loaded += PACK001_029_monitoring_Loaded;
            dgMain.LoadedCellPresenter += dgMain_LoadedCellPresenter;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            try
            {
                sbExpandLeft = (Storyboard)this.Resources["ExpandLeftFrameStoryboard"];
                sbCollapseLeft = (Storyboard)this.Resources["CollapseLeftFrameStoryboard"];

                DateTime dt = DateTime.Now;
                tbdate.Text = ObjectDic.Instance.GetObjectName(dt.ToString("yyyy년 MM월 dd일 hh시 mm분 ss초"));

                //testDATA();

                realData();

                beginTimer();

                beginTimer1();
            }
            catch (Exception ex)
            {

                Util.AlertInfo(ex.Message);
            }     
        }
        #endregion

        #region Event
        private void PACK001_029_monitoring_Loaded(object sender, RoutedEventArgs e)
        {            
            try
            {
                this.Loaded -= PACK001_029_monitoring_Loaded;

                Initialize();

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void dgMain_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

                    switch (e.Cell.Column.Name)
                    {
                        case "LINE_MOVE1":
                            if (e.Cell.Value.ToString() == ".")
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.DeepSkyBlue);
                            }
                            break;
                        case "LINE_MOVE2":
                            if (e.Cell.Value.ToString() == ".")
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.LawnGreen);
                            }
                            break;
                        case "LINE_MOVE3":
                            if (e.Cell.Value.ToString() == ".")
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);
                            }
                            break;
                        case "LINE_MOVE4":
                            if (e.Cell.Value.ToString() == ".")
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            }
                            break;
                    }

                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDate == null || dtpDate.SelectedDateTime == null)
            {
                return;
            }

            realData();
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
        #endregion

        #region Method
        private void realData()
        {
            initBindTable();

            searchData();

            bind();
        }

        private void searchData()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["DATE"] = dtpDate.SelectedDateTime.ToString("yyyyMMdd");

                RQSTDT.Rows.Add(dr);

                dtSearchTable = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOARD_SUMMARY", "INDATA", "OUTDATA", RQSTDT);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void reSet_lineMoves()
        {
            DataTable dt = DataTableConverter.Convert(dgMain.ItemsSource);

            for (int i = 0; i < dgMain.GetRowCount(); i++)
            {
                dt.Rows[i]["LINE_MOVE1"] = "";
                dt.Rows[i]["LINE_MOVE2"] = "";
                dt.Rows[i]["LINE_MOVE3"] = "";
                dt.Rows[i]["LINE_MOVE4"] = "";
            }

            dgMain.ItemsSource = DataTableConverter.Convert(dt);

        }

        private void testDATA()
        {
            try
            {
                DataTable dt = new DataTable();

                dt.Columns.Add("LINE", typeof(string));
                dt.Columns.Add("D_PLAN", typeof(string));
                dt.Columns.Add("D_RESULT", typeof(string));
                dt.Columns.Add("D_ACCEPT", typeof(string));
                dt.Columns.Add("M_PLAN", typeof(string));
                dt.Columns.Add("M_PLAN_S", typeof(string));
                dt.Columns.Add("M_RESULT_S", typeof(string));
                dt.Columns.Add("M_ACCEPT", typeof(string));
                dt.Columns.Add("M_ACCEPT_S", typeof(string));
                dt.Columns.Add("LINE_MOVE1", typeof(string));
                dt.Columns.Add("LINE_MOVE2", typeof(string));
                dt.Columns.Add("LINE_MOVE3", typeof(string));
                dt.Columns.Add("LINE_MOVE4", typeof(string));

                List<object[]> menulist = new List<object[]>();

                menulist.Add(new object[] { "1", "", "", "0%", "", "", "", "0%", "0%", ".", "", "", "" });
                menulist.Add(new object[] { "2", "440", "350", "80%", "2,160", "2,160", "2,100", "97%", "97%", "", ".", "", "" });
                menulist.Add(new object[] { "3", "16", "15", "94%", "320", "300", "298", "93%", "99%", "", "", ".", "" });
                menulist.Add(new object[] { "4", "1,600", "1,500", "94%", "38,000", "33,600", "32,000", "84%", "95%", "", "", "", "." });
                menulist.Add(new object[] { "5", "", "", "0%", "", "", "", "0%", "0%", "", "", "", "" });
                menulist.Add(new object[] { "6", "", "", "0%", "", "", "", "0%", "0%", "", "", "", "" });
                menulist.Add(new object[] { "7", "", "", "0%", "", "", "", "0%", "0%", "", "", "", "" });
                menulist.Add(new object[] { "8", "", "", "0%", "", "", "", "0%", "0%", "", "", "", "" });
                menulist.Add(new object[] { "9", "", "", "0%", "", "", "", "0%", "0%", "", "", "", "" });
                menulist.Add(new object[] { "10", "", "", "0%", "", "", "", "0%", "0%", "", "", "", "" });
                menulist.Add(new object[] { "11", "", "", "0%", "", "", "", "0%", "0%", "", "", "", "" });
                menulist.Add(new object[] { "12", "", "", "0%", "", "", "", "0%", "0%", "", "", "", "" });

                DataRow dr;

                foreach (object[] item in menulist)
                {
                    dr = dt.NewRow();
                    dr.ItemArray = item;
                    dt.Rows.Add(dr);
                }

                Util.GridSetData(dgMain, dt, FrameOperation);

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void initBindTable()
        {
            try
            {
                dtBindTable = new DataTable();

                dtBindTable.Columns.Add("LINE", typeof(string));
                dtBindTable.Columns.Add("D_PLAN", typeof(string));
                dtBindTable.Columns.Add("D_RESULT", typeof(string));
                dtBindTable.Columns.Add("D_ACCEPT", typeof(string));
                dtBindTable.Columns.Add("M_PLAN", typeof(string));
                dtBindTable.Columns.Add("M_PLAN_S", typeof(string));
                dtBindTable.Columns.Add("M_RESULT_S", typeof(string));
                dtBindTable.Columns.Add("M_ACCEPT", typeof(string));
                dtBindTable.Columns.Add("M_ACCEPT_S", typeof(string));
                dtBindTable.Columns.Add("LINE_MOVE1", typeof(string));
                dtBindTable.Columns.Add("LINE_MOVE2", typeof(string));
                dtBindTable.Columns.Add("LINE_MOVE3", typeof(string));
                dtBindTable.Columns.Add("LINE_MOVE4", typeof(string));

                DataRow dr;

                for (int i = 1; i <= 12; i++)
                {
                    dr = dtBindTable.Rows.Add();

                    dr["LINE"] = i.ToString();
                    dr["D_PLAN"] = "";
                    dr["D_RESULT"] = "";
                    dr["D_ACCEPT"] = "0%";
                    dr["M_PLAN"] = "";
                    dr["M_PLAN_S"] = "";
                    dr["M_RESULT_S"] = "";
                    dr["M_ACCEPT"] = "0%";
                    dr["M_ACCEPT_S"] = "0%";
                    dr["LINE_MOVE1"] = "";
                    dr["LINE_MOVE2"] = "";
                    dr["LINE_MOVE3"] = "";
                    dr["LINE_MOVE4"] = "";

                    //dtBindTable.Rows.Add(dr);
                }

                //Util.GridSetData(dgMain, dtBindTable, FrameOperation);

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void bind()
        {
            for (int i = 1; i <= dtBindTable.Rows.Count; i++)
            {
                DataRow[] dr = dtSearchTable.Select("LINE = '" + i.ToString() + "'");

                if (dr.Length > 0)
                {
                    dtBindTable.Rows[i - 1]["D_PLAN"] = dr[0]["D_PLAN"] == null ? "" : dr[0]["D_PLAN"];
                    dtBindTable.Rows[i - 1]["D_RESULT"] = dr[0]["D_RESULT"] == null ? "" : dr[0]["D_RESULT"];
                    dtBindTable.Rows[i - 1]["D_ACCEPT"] = dr[0]["D_ACCEPT"] == null ? "0%" : dr[0]["D_ACCEPT"].ToString() == "" ? "0%" : dr[0]["D_ACCEPT"] + "%";
                    dtBindTable.Rows[i - 1]["M_PLAN"] = dr[0]["M_PLAN"];
                    dtBindTable.Rows[i - 1]["M_PLAN_S"] = dr[0]["M_PLAN_S"];
                    dtBindTable.Rows[i - 1]["M_RESULT_S"] = dr[0]["M_RESULT_S"];
                    dtBindTable.Rows[i - 1]["M_ACCEPT"] = dr[0]["M_ACCEPT"] + "%";


                    if (Convert.ToInt32(dr[0]["M_ACCEPT_S"]) >= 90)
                    {
                        dtBindTable.Rows[i - 1]["LINE_MOVE1"] = ".";
                    }
                    else if (Convert.ToInt32(dr[0]["M_ACCEPT_S"]) >= 80 && Convert.ToInt32(dr[0]["M_ACCEPT_S"]) < 90)
                    {
                        dtBindTable.Rows[i - 1]["LINE_MOVE2"] = ".";
                    }
                    else if (Convert.ToInt32(dr[0]["M_ACCEPT_S"]) >= 70 && Convert.ToInt32(dr[0]["M_ACCEPT_S"]) < 80)
                    {
                        dtBindTable.Rows[i - 1]["LINE_MOVE3"] = ".";
                    }
                    else if (Convert.ToInt32(dr[0]["M_ACCEPT_S"]) < 70)
                    {
                        dtBindTable.Rows[i - 1]["LINE_MOVE4"] = ".";
                    }

                    dtBindTable.Rows[i - 1]["M_ACCEPT_S"] = dr[0]["M_ACCEPT_S"] + "%";

                    // dtBindTable.Rows.Remove(dtBindTable.Rows[i-1]);


                    //dtBindTable.Rows.InsertAt(dr[0], i-1);
                    //DataRow row = dtBindTable.NewRow();
                    //row = dr[0];
                    //dtBindTable.Rows.Add(row);
                    // dtBindTable.AcceptChanges();
                }
                else
                {
                    dtBindTable.Rows[i - 1]["LINE_MOVE4"] = ".";
                }



                //string line_no;
                //for (int k = 0; k <dtSearchTable.Rows.Count; k++)
                //{
                //    line_no = dtSearchTable.Rows[k]["LINE_NO"].ToString();

                //    if(i.ToString() == line_no)
                //    {
                //        dtBindTable.Rows.Remove(dtBindTable.Rows[i]);
                //        dtBindTable.Rows.InsertAt(dtSearchTable.Rows[Convert.ToInt32(line_no)], i);
                //    }
                //}
            }

            dtBindTable.AcceptChanges();

            dgMain.ItemsSource = DataTableConverter.Convert(dtBindTable);
        }

        private void beginTimer()
        {
            _timer = new Timer(1000);
            _timer.AutoReset = true;
            _timer.Elapsed += (s, arg) =>
            {
                tbdate.Dispatcher.BeginInvoke(new Action(() => tbdate.Text = ObjectDic.Instance.GetObjectName(DateTime.Now.ToString("yyyy년 MM월 dd일 hh시 mm분 ss초"))));
            };
            _timer.Start();
        }

        private void beginTimer1()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                _timer2 = new Timer(10000);
                _timer2.AutoReset = true;
                _timer2.Elapsed += (s, arg) =>
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        realData();
                    }));

                };
                _timer2.Start();
            }));
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
    }
}
