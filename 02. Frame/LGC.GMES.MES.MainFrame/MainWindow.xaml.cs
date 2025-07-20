/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2024.05.14  조범모 : E20231011-000895 GMES Main 화면 배포이력 표기
  2024.06.27  권용섭 : [E20240620-001371] 보정 dOCV 송/수신 실패시 팝업 알림
  2024.08.20  최성필 : FDS 발열셀 알림 추가
  2025.04.22  이지은 : 2024.11.01 / 강동희(kdh7609) / 고온 Aging 온도 이탈 알람 팝업 사용 여부 반영의 건
**************************************************************************************/
#region Import Library
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.MainFrame.Controls;
#endregion

namespace LGC.GMES.MES.MainFrame
{
    public partial class MainWindow : Window
    {
        #region Declaration & Constructor
        /// <summary>
        /// MainWindow.xaml에 대한 상호 작용 논리
        /// </summary>

        /// <summary>
        /// 시계 & AP Check 기능용 타이머
        /// </summary>
        private Timer _timer;
        private Timer _APtimer;
        private Timer _UpdateTimer;
        private Timer _BizLogLevelTimer;

        //활성화 : 공정 대기 시간 초과 현황 팝업 체크용 Timer - 2022-09-27
        private Timer _FormProcWaitLimitTimeOver;
        //활성화 : Aging 한계시간 초과 현황 팝업 체크용 Timer - 2020-12-28
        private Timer _FormAgingLimitTimeOver;
        //활성화 : 설비 상태 체크용 Timer - 2021-03-07
        private Timer _EqpStatusTimer;
        //활성화 : Stocker & 충방전기 화재상태 체크용 Timer - 2022-08-04
        private Timer _FcsEqpStatusTimer;
        //활성화 : FDS 발열셀 알람 체크용 Timer - 2024-08-20
        private Timer _FormFDSCELLTimer;
        //활성화 : SAS 송수신 오류 알람 체크용 Timer - 2024-08-26
        private Timer _FormSASTimer;
        //활성화 : 고온 Aging 온도 이탈 알람 체크용 Timer - 2024-11-01(2025.04.22 리빌딩 반영)
        private Timer _FormHighAgingAbnormTmprTimer;
        //활성화 : 충방전기 작업중 Tray 시간경과 체크용 Timer 2023-09-25 
        private Timer _FcsFormationTimer;
        private Timer _FormAgingOutputTimeOver; // 2023.12.17 출하 Aging 후단출고 기준시간 초과 현황 추가
        //활성화 : [E20240620-001371] 보정 dOCV 송/수신 실패시 팝업 알림 Timer >> 2024.06.27 / 권용섭(cnsyongsub)
        private Timer _FormFittedDOCVTrnfFail;
        private DateTime dtNoticeBoard = new DateTime();
        //활성화 : 
        ConfigWindow configWindow;
        EmergencyContact eContact;

        //private string updateString;

        /// <summary>
        /// 현재의 Operation 화면에 할당된 FrameOperation 객체
        /// </summary>
        private FrameOperation currentFrameOperation = null;

        /// <summary>
        /// 화면 키보드를 위한 WIN32 API 참조용
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool Wow64RevertWow64FsRedirection(IntPtr ptr);

        /// <summary>
        /// 초기 설정 실패 횟수
        /// </summary>
        private int initialConfigCancelCount = 0;

        private Dictionary<string, MegadropMenuItem> menuItemDic = new Dictionary<string, MegadropMenuItem>();
        private Dictionary<string, MenuItem> menuItemDicNormal = new Dictionary<string, MenuItem>();



        private bool isObjectDicLoaded = false;
        private bool isMessageDicLoaded = false;

        DataSet dsUpdateVersion = new DataSet();

        string myVersion = string.Empty;
        string updateVersion = string.Empty;

        bool isOpenedLineMonitoring = false;
        bool isInitAuthority = false;   // 2020-09-22 (초기권한신청자)

        /// <summary>
        /// MainWindow 생성자
        /// </summary>
        public MainWindow()
        {
            //Process[] processes = Process.GetProcesses();
            //foreach (Process p in processes)
            //{
            //    if (p.ProcessName.ToLower().Contains("werfault"))
            //    {
            //        p.Kill();
            //    }
            //}
            App.Current.MainWindow = this;
            InitializeComponent();
            Variables.getConstData();
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            Activated += MainWindow_Activated;
            Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);
            dtNoticeBoard = System.DateTime.Now;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        /// <summary>
        /// MainWindow 종료시 타이머 객체 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Close();
                _timer = null;
            }

            if (_APtimer != null)
            {
                _APtimer.Stop();
                _APtimer.Close();
                _APtimer = null;
            }

            if (_UpdateTimer != null)
            {
                _UpdateTimer.Stop();
                _UpdateTimer.Close();
                _UpdateTimer = null;
            }

            if (_BizLogLevelTimer != null)
            {
                _BizLogLevelTimer.Stop();
                _BizLogLevelTimer.Close();
                _BizLogLevelTimer = null;
            }

            //활성화 : 공정 대기 시간 초과 현황 팝업 체크용 Timer - 2022-09-27
            if (_FormProcWaitLimitTimeOver != null)
            {
                _FormProcWaitLimitTimeOver.Stop();
                _FormProcWaitLimitTimeOver.Close();
                _FormProcWaitLimitTimeOver = null;
            }

            //활성화 : Aging 한계시간 초과 현황 팝업 체크용 Timer - 2020-12-28
            if (_FormAgingLimitTimeOver != null)
            {
                _FormAgingLimitTimeOver.Stop();
                _FormAgingLimitTimeOver.Close();
                _FormAgingLimitTimeOver = null;
            }
            //활성화 : 설비 Trouble 팝업 체크용 Timer 2021-03-07
            if (_EqpStatusTimer != null)
            {
                _EqpStatusTimer.Stop();
                _EqpStatusTimer.Close();
                _EqpStatusTimer = null;
            }
            //활성화 : Stocker & 충방전기 화재상태 체크용 Timer - 2022-08-04
            if (_FcsEqpStatusTimer != null)
            {
                _FcsEqpStatusTimer.Stop();
                _FcsEqpStatusTimer.Close();
                _FcsEqpStatusTimer = null;
            }
            //활성화 : Stocker & 충방전기 화재상태 체크용 Timer - 2022-08-04
            if (_FcsFormationTimer != null)
            {
                _FcsFormationTimer.Stop();
                _FcsFormationTimer.Close();
                _FcsFormationTimer = null;
            }
            //활성화 : Aging 출하시간 초과 현황 팝업 체크용 Timer  / 2023.12.17 출하 Aging 후단출고 기준시간 초과 현황 추가
            if (_FormAgingOutputTimeOver != null)
            {
                _FormAgingOutputTimeOver.Stop();
                _FormAgingOutputTimeOver.Close();
                _FormAgingOutputTimeOver = null;
            }
            //활성화 : [E20240620-001371] 보정 dOCV 송/수신 실패시 팝업 알림 Timer >> 2024.06.27 / 권용섭(cnsyongsub)
            if (_FormFittedDOCVTrnfFail != null)
            {
                _FormFittedDOCVTrnfFail.Stop();
                _FormFittedDOCVTrnfFail.Close();
                _FormFittedDOCVTrnfFail = null;
            }
            //활성화 : FDS 발열셀 알람 팝업 Timer >> 2024.08.20 / 최성필(cso59463)
            if (_FormFDSCELLTimer != null)
            {
                _FormFDSCELLTimer.Stop();
                _FormFDSCELLTimer.Close();
                _FormFDSCELLTimer = null;
            }
            //활성화 : SAS 송수신 오류 알람 팝업 Timer >> 2024.08.26 / 최성필(cso59463)
            if (_FormSASTimer != null)
            {
                _FormSASTimer.Stop();
                _FormSASTimer.Close();
                _FormSASTimer = null;
            }
            //활성화 : 고온 Aging 온도 이탈 알람 체크용 Timer - 2024-11-01 / 강동희(kdh7609) - 2025.04.22 리빌딩 소형 반영
            if (_FormHighAgingAbnormTmprTimer != null)
            {
                _FormHighAgingAbnormTmprTimer.Stop();
                _FormHighAgingAbnormTmprTimer.Close();
                _FormHighAgingAbnormTmprTimer = null;
            }

        }

        /// <summary>
        /// MainWindow Loaded 이벤트 핸들러
        /// 타이머 셋팅
        /// 초기 설정 필요성 검사 및 초기 설정 시작
        /// 공정 정보 로드 시작
        /// 메뉴 생성 시작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LogIn();
        }
        #endregion

        #region Initialize
        #endregion

        #region Event
        private void grMenu_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                Visibility beLeftVisibility = btnLeftNext.Visibility;
                Visibility beRightVisibility = btnRightNext.Visibility;

                double totWidth = 0;

                foreach (MenuItem item in munMainMenu.Items)
                    totWidth += item.ActualWidth;

                if (totWidth == 0)
                    return;

                totWidth += 55;
                //string tmp = "Actual Width : " + Convert.ToString(grMenu.ActualWidth) + "\r\n";
                //tmp += "Total Width : " + Convert.ToString(totWidth) + "\r\n";
                //System.Diagnostics.Debug.WriteLine(tmp);

                if (grMenu.ActualWidth < totWidth)
                {
                    btnLeftNext.Visibility = Visibility.Visible;
                    btnRightNext.Visibility = Visibility.Visible;
                }
                else
                {
                    btnLeftNext.Visibility = Visibility.Collapsed;
                    btnRightNext.Visibility = Visibility.Collapsed;
                    munMainMenu.Margin = new Thickness(0, 0, 0, 0);
                }

                if (totWidth == 0)
                {
                    btnLeftNext.Visibility = beLeftVisibility;
                    btnRightNext.Visibility = beRightVisibility;
                }
            }
            catch (Exception ex)
            {
                string msg = new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(msg, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void munMainMenu_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                double totWidth = 0;

                foreach (MenuItem item in munMainMenu.Items)
                    totWidth += item.ActualWidth;

                if (totWidth == 0)
                    return;

                totWidth += 55;
                //string tmp = "Actual Width : " + Convert.ToString(grMenu.ActualWidth) + "\r\n";
                //tmp += "Total Width : " + Convert.ToString(totWidth) + "\r\n";
                //System.Diagnostics.Debug.WriteLine(tmp);

                if (grMenu.ActualWidth < totWidth)
                {
                    btnLeftNext.Visibility = Visibility.Visible;
                    btnRightNext.Visibility = Visibility.Visible;
                }
                else
                {
                    btnLeftNext.Visibility = Visibility.Collapsed;
                    btnRightNext.Visibility = Visibility.Collapsed;
                    munMainMenu.Margin = new Thickness(0, 0, 0, 0);
                }
            }
            catch (Exception ex)
            {
                string msg = new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(msg, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnLeftNext_Click(object sender, RoutedEventArgs e)
        {
            Thickness newthick = new Thickness();
            Thickness oldthick = munMainMenu.Margin;
            MenuItem itemoffset = munMainMenu.Items[0] as MenuItem;
            newthick.Left = oldthick.Left + itemoffset.ActualWidth;

            if (newthick.Left > 0)
            {
                newthick.Left = 0;
                munMainMenu.Margin = newthick;
            }
            else
            {
                munMainMenu.Margin = newthick;
            }
        }

        private void btnRightNext_Click(object sender, RoutedEventArgs e)
        {
            Thickness newthick = new Thickness();
            Thickness oldthick = munMainMenu.Margin;
            MenuItem itemoffset = munMainMenu.Items[munMainMenu.Items.Count - 1] as MenuItem;

            newthick.Left = oldthick.Left - itemoffset.ActualWidth;
            munMainMenu.Margin = newthick;
            double totWidth = 0;

            foreach (MenuItem item in munMainMenu.Items)
                totWidth += item.ActualWidth;

            if (newthick.Left < 0)
            {
                if (totWidth + 200 < Math.Abs(newthick.Left) + grMenu.ActualWidth)
                    munMainMenu.Margin = oldthick;
                else
                    munMainMenu.Margin = newthick;
            }
            else
            {
                munMainMenu.Margin = newthick;
            }
        }

        private void ddFavorite_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((icFavorite.ItemsSource as ObservableCollection<object>).Count == 0)
                ddFavorite.IsDropDownOpen = false;
            else
                ddFavorite.IsDropDownOpen = true;
        }

        private void TextBlock_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            OpenMenu((sender as FrameworkElement).DataContext);
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataTable delBookmarkIndataTable = new DataTable();
            delBookmarkIndataTable.Columns.Add("USERID", typeof(string));
            delBookmarkIndataTable.Columns.Add("MENUID", typeof(string));
            DataRow delBookmarkIndata = delBookmarkIndataTable.NewRow();
            delBookmarkIndata["USERID"] = LoginInfo.USERID;
            delBookmarkIndata["MENUID"] = DataTableConverter.GetValue((sender as FrameworkElement).DataContext, "MENUID");
            delBookmarkIndataTable.Rows.Add(delBookmarkIndata);

            new ClientProxy().ExecuteService("COR_DEL_BOOKMARK", "INDATA", null, delBookmarkIndataTable, (delBookmarkResult, delBookmarkException) => { });

            (icFavorite.ItemsSource as ObservableCollection<object>).Remove((sender as FrameworkElement).DataContext);
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                object systemLinkInfo = (sender as FrameworkElement).DataContext;
                string url = DataTableConverter.GetValue(systemLinkInfo, "SYS_URL_ADDR").ToString();

                if (!string.IsNullOrEmpty(url))
                    System.Diagnostics.Process.Start(url);
            }
            finally
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ddFavorite.IsDropDownOpen = false;
                }));
            }
        }

        /// <summary>
        /// 초기 설정 화면 Closed 이벤트 핸들러
        /// 초기 설정을 취소하면 초기 설정 화면으로 돌아감
        /// 초기 설정을 3번 취소하면 강제로 로그아웃
        /// 초기 설정에 성공하면 설정 정보 반영 시작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void configWindow_Closed(object sender, EventArgs e)
        {
            ConfigWindow configWindow = sender as ConfigWindow;

            if (configWindow.DialogResult != MessageBoxResult.OK)
            {
                if (initialConfigCancelCount > 3)
                {
                    Logger.Instance.WriteLine("[FRAME] Initial Configuration", "Fail to initialize", LogCategory.FRAME);
                    tblLogout_MouseLeftButtonDown(null, null);
                    return;
                }

                Logger.Instance.WriteLine("[FRAME] Initial Configuration", "Fail", LogCategory.FRAME);
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("초기설정 취소 불가 합니다"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);

                initialConfigCancelCount++;
                configWindow = new ConfigWindow();
                configWindow.Closed -= new EventHandler(configWindow_Closed);
                configWindow.Closed += new EventHandler(configWindow_Closed);
                Dispatcher.BeginInvoke(new Action(() => configWindow.ShowModal()));
                Logger.Instance.WriteLine("[FRAME] Initial Configuration", "Retry", LogCategory.FRAME);
            }
            else
            {
                Logger.Instance.WriteLine("[FRAME] Initial Configuration", Logger.MESSAGE_OPERATION_END, LogCategory.FRAME);
                configChanged();
            }
        }

        /// <summary>
        /// 화면 키보트 버튼 Click 이벤트 핸들러
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnKeyBoard_Click(object sender, RoutedEventArgs e)
        {
            Logger.Instance.WriteLine("[FRAME] Screen Keyboard", Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);

            try
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    IntPtr ptr = new IntPtr();
                    bool isWow64FsRedirectionDisabled = Wow64DisableWow64FsRedirection(ref ptr);

                    System.Diagnostics.Process ps = new System.Diagnostics.Process();
                    ps.StartInfo.FileName = "osk.exe";
                    ps.Start();

                    if (isWow64FsRedirectionDisabled)
                        Wow64RevertWow64FsRedirection(ptr);
                }
                else
                {
                    System.Diagnostics.Process ps = new System.Diagnostics.Process();
                    ps.StartInfo.FileName = "osk.exe";
                    ps.Start();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage("오류"), ex.Message, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

            Logger.Instance.WriteLine("[FRAME] Screen Keyboard", Logger.MESSAGE_OPERATION_END, LogCategory.FRAME);
        }

        /// <summary>
        /// Logout 버튼 Click 이벤트 핸들러
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tblLogout_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Logger.Instance.WriteLine("[FRAME] MainWindow", "Log out", LogCategory.FRAME);
            Visibility = System.Windows.Visibility.Collapsed;
            Close();
        }

        /// <summary>
        /// 메인 메뉴 Click 이벤트 핸들러
        /// 선택한 메뉴에 해당하는 팝업 화면을 생성한다
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnMainMenu_MenuItemClick(object sender, MegadropMenuItemEventArg e)
        {
            MegadropMenuItem item = sender as MegadropMenuItem;

            if (item != null && item.Items.Count == 0)
            {
                object menuData = item.DataContext;

                if (menuData != null && "3".Equals(DataTableConverter.GetValue(menuData, "MENULEVEL")))
                {
                    OpenMenu(menuData);
                    return;
                }
            }
        }

        private void childMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (item != null && item.Items.Count == 0)
            {
                object menuData = item.DataContext;

                if (menuData != null && "3".Equals(DataTableConverter.GetValue(menuData, "MENULEVEL")))
                {
                    OpenMenu(menuData);
                    return;
                }
            }
        }

        private void tcMainContentTabControl_TabItemClosing(object sender, CancelSourceEventArgs e)
        {
            if (e.Source is C1TabItem)
            {
                C1TabItem tabItem = e.Source as C1TabItem;

                if (tabItem.Content is MainTabItemLayout)
                {
                    MainTabItemLayout mainTabLayout = tabItem.Content as MainTabItemLayout;

                    if (mainTabLayout.ContentArea is IWorkArea)
                    {
                        IWorkArea workArea = mainTabLayout.ContentArea as IWorkArea;

                        if (workArea.FrameOperation.IsCurrupted)
                        {
                            e.Cancel = true;

                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("COM0004"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    tcMainContentTabControl.TabItemClosing -= tcMainContentTabControl_TabItemClosing;

                                    tcMainContentTabControl.Items.Remove(tabItem);

                                    tcMainContentTabControl.TabItemClosing += tcMainContentTabControl_TabItemClosing;
                                }
                            });
                        }
                    }

                    if (mainTabLayout.ContentArea is IDisposable)
                        (mainTabLayout.ContentArea as IDisposable).Dispose();
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// COM Port 로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void spPortCon_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Instance.WriteLine("[FRAME] COM Port connection", Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);

                foreach (UIElement element in spPortCon.Children)
                    if (element is IDisposable)
                        (element as IDisposable).Dispose();

                spPortCon.Children.Clear();

                if (LoginInfo.CFG_SERIAL_PRINT == null)
                    return;

                if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 0)
                {
                    IEnumerable<DataRow> activeSerialPortList = (from DataRow r in LoginInfo.CFG_SERIAL_PRINT.Rows
                                                                 where r[CustomConfig.CONFIGTABLE_SERIALPRINTER_ISACTIVE].Equals(true)
                                                                     && r[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE].Equals("S")
                                                                 select r).Distinct();

                    foreach (DataRow serialPort in activeSerialPortList)
                    {
                        ScannerControl scanner = new ScannerControl() { Margin = new Thickness(4, 0, 4, 0) };
                        spPortCon.Children.Add(scanner);

                        scanner.Dispatcher.BeginInvoke(new Action(() => scanner.Open(serialPort[CustomConfig.CONFIGTABLE_SERIALPORT_PORTNAME].ToString(),
                                                                          (int)serialPort[CustomConfig.CONFIGTABLE_SERIALPORT_BAUDRATE],
                                                                          (Parity)Enum.Parse(typeof(Parity), serialPort[CustomConfig.CONFIGTABLE_SERIALPORT_PARITYBIT].ToString(), false),
                                                                          (int)serialPort[CustomConfig.CONFIGTABLE_SERIALPORT_DATABIT],
                                                                          (System.IO.Ports.StopBits)Enum.Parse(typeof(System.IO.Ports.StopBits), serialPort[CustomConfig.CONFIGTABLE_SERIALPORT_STOPBIT].ToString(), false),
                                                                          (s, arg) =>
                                                                          {
                                                                              Logger.Instance.WriteLine("[FRAME] Read scanner", s + " - " + arg, LogCategory.FRAME);
                                                                              currentFrameOperation.ReadScanData(s, arg);
                                                                          })));

                        scanner.Dispatcher.BeginInvoke(new Action(() => scanner.StatusCheck()));
                        // scanner.Visibility = Visibility.Visible;
                    }

                    if (spPortCon.Children.Count > 0)
                    {
                        (spPortCon.Children[spPortCon.Children.Count - 1] as FrameworkElement).Margin = new Thickness(4, 0, 0, 0);

                    }
                }

                //if (CustomConfig.Instance.ConfigSet.Tables.Contains(CustomConfig.CONFIGTABLE_SERIALPORT)
                //    && CustomConfig.Instance.ConfigSet.Tables[CustomConfig.CONFIGTABLE_SERIALPORT].Rows.Count > 0)
                //{
                //    foreach (DataRow serialPort in CustomConfig.Instance.ConfigSet.Tables[CustomConfig.CONFIGTABLE_SERIALPORT].Rows)
                //    {
                //        ScannerControl scanner = new ScannerControl() { Margin = new Thickness(4, 0, 4, 0) };
                //        spPortCon.Children.Add(scanner);

                //        scanner.Dispatcher.BeginInvoke(new Action(() => scanner.Open(serialPort[CustomConfig.CONFIGTABLE_SERIALPORT_PORTNAME].ToString(),
                //                                                          (int)serialPort[CustomConfig.CONFIGTABLE_SERIALPORT_BAUDRATE],
                //                                                          (Parity)Enum.Parse(typeof(Parity), serialPort[CustomConfig.CONFIGTABLE_SERIALPORT_PARITYBIT].ToString(), false),
                //                                                          (int)serialPort[CustomConfig.CONFIGTABLE_SERIALPORT_DATABIT],
                //                                                          (System.IO.Ports.StopBits)Enum.Parse(typeof(System.IO.Ports.StopBits), serialPort[CustomConfig.CONFIGTABLE_SERIALPORT_STOPBIT].ToString(), false),
                //                                                          (s, arg) =>
                //                                                          {
                //                                                              Logger.Instance.WriteLine("[FRAME] Read scanner", s + " - " + arg, LogCategory.FRAME);
                //                                                              currentFrameOperation.ReadScanData(s, arg);
                //                                                          })));

                //        scanner.Dispatcher.BeginInvoke(new Action(() => scanner.StatusCheck()));
                //    }

                //    if (spPortCon.Children.Count > 0)
                //        (spPortCon.Children[spPortCon.Children.Count - 1] as FrameworkElement).Margin = new Thickness(4, 0, 0, 0);
                //}

                Logger.Instance.WriteLine("[FRAME] COM Port connection", Logger.MESSAGE_OPERATION_END, LogCategory.FRAME);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Printer 로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void spPrinter_Loaded(object sender, RoutedEventArgs e)
        {
            Logger.Instance.WriteLine("[FRAME] Printer connection", Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);

            foreach (UIElement element in spPrinter.Children)
                if (element is IDisposable)
                    (element as IDisposable).Dispose();

            spPrinter.Children.Clear();
            PrinterManager.Instance.Clear();

            if (LoginInfo.CFG_SERIAL_PRINT == null)
                return;

            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 0)
            {
                IEnumerable<DataRow> activePrinterList = (from DataRow r in LoginInfo.CFG_SERIAL_PRINT.Rows
                                                          where r[CustomConfig.CONFIGTABLE_SERIALPRINTER_ISACTIVE].Equals(true)
                                                          select r).Distinct();

                foreach (DataRow activePrinter in activePrinterList)
                {
                    if (activePrinter[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE].ToString() == "S")
                        continue;

                    IEnumerable<DataRow> standbyPrinterList = (from DataRow r in LoginInfo.CFG_SERIAL_PRINT.Rows
                                                               where r[CustomConfig.CONFIGTABLE_SERIALPRINTER_PARENTPRINTERKEY].Equals(activePrinter[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERKEY])
                                                               select r);

                    PrinterControl printer = new PrinterControl(activePrinter[CustomConfig.CONFIGTABLE_SERIALPRINTER_LABELTYPE].ToString(), (int)activePrinter[CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES]) { Margin = new Thickness(4, 0, 4, 0) };
                    spPrinter.Children.Add(printer);

                    printer.Open(activePrinter[CustomConfig.CONFIGTABLE_SERIALPRINTER_PORTNAME].ToString(),
                                (int)activePrinter[CustomConfig.CONFIGTABLE_SERIALPRINTER_BAUDRATE],
                                (Parity)Enum.Parse(typeof(Parity), activePrinter[CustomConfig.CONFIGTABLE_SERIALPRINTER_PARITYBIT].ToString(), false),
                                (int)activePrinter[CustomConfig.CONFIGTABLE_SERIALPRINTER_DATABIT],
                                (System.IO.Ports.StopBits)Enum.Parse(typeof(System.IO.Ports.StopBits), activePrinter[CustomConfig.CONFIGTABLE_SERIALPRINTER_STOPBIT].ToString(), false),
                                (int)activePrinter[CustomConfig.CONFIGTABLE_SERIALPRINTER_X],
                                (int)activePrinter[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y],
                                activePrinter.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_CONNECTIONLESS) ? (bool)activePrinter[CustomConfig.CONFIGTABLE_SERIALPRINTER_CONNECTIONLESS] : false);

                    foreach (DataRow standbyPrinter in standbyPrinterList)
                    {
                        printer.Open(standbyPrinter[CustomConfig.CONFIGTABLE_SERIALPRINTER_PORTNAME].ToString(),
                                    (int)standbyPrinter[CustomConfig.CONFIGTABLE_SERIALPRINTER_BAUDRATE],
                                    (Parity)Enum.Parse(typeof(Parity), standbyPrinter[CustomConfig.CONFIGTABLE_SERIALPRINTER_PARITYBIT].ToString(), false),
                                    (int)standbyPrinter[CustomConfig.CONFIGTABLE_SERIALPRINTER_DATABIT],
                                    (System.IO.Ports.StopBits)Enum.Parse(typeof(System.IO.Ports.StopBits), standbyPrinter[CustomConfig.CONFIGTABLE_SERIALPRINTER_STOPBIT].ToString(), false),
                                    (int)standbyPrinter[CustomConfig.CONFIGTABLE_SERIALPRINTER_X],
                                    (int)standbyPrinter[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y],
                                    standbyPrinter.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_CONNECTIONLESS) ? (bool)standbyPrinter[CustomConfig.CONFIGTABLE_SERIALPRINTER_CONNECTIONLESS] : false);
                    }

                    System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex(@"COM\d");
                    System.Text.RegularExpressions.Match m = rgx.Match(activePrinter[CustomConfig.CONFIGTABLE_SERIALPRINTER_PORTNAME].ToString());

                    if (m.Success)
                        printer.Visibility = Visibility.Visible;
                    else
                        printer.Visibility = Visibility.Collapsed;
                }

                if (spPrinter.Children.Count > 0)
                    (spPrinter.Children[spPrinter.Children.Count - 1] as FrameworkElement).Margin = new Thickness(4, 0, 0, 0);

                Dispatcher.BeginInvoke(new Action(() => PrinterManager.Instance.StartPrinterChecking()));
            }

            Logger.Instance.WriteLine("[FRAME] Printer connection", Logger.MESSAGE_OPERATION_END, LogCategory.FRAME);
        }

        /// <summary>
        /// 윈도우 헤더 영역 Mouse Left Button 이벤트 핸들러
        /// 보통/최대화/윈도우 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (mnMainMenu.megaDropBox.IsOpen)
                mnMainMenu.megaDropBox.IsOpen = false;

            if (e.ClickCount == 2)
            {
                if (WindowState == System.Windows.WindowState.Normal)
                    WindowState = System.Windows.WindowState.Maximized;
                else
                    WindowState = System.Windows.WindowState.Normal;
            }
            else
            {
                try
                {
                    DragMove();
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage("오류"), ex.Message, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            msgPopup.IsOpen = true;
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            msgPopup.IsOpen = false;
        }

        private void Image_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            WindowState = System.Windows.WindowState.Minimized;
        }

        private void Image_MouseLeftButtonDown_2(object sender, MouseButtonEventArgs e)
        {
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("COM0005"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            //{
            //    if (result == MessageBoxResult.OK)
            //    {
            Close();
            //    }
            //}
            //);
        }

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
                X = x;
                Y = y;
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
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr mWindowHandle = (new WindowInteropHelper(this)).Handle;
            HwndSource.FromHwnd(mWindowHandle).AddHook(new HwndSourceHook(WindowProc));

            WindowState = System.Windows.WindowState.Maximized;
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

            if (!GetMonitorInfo(lPrimaryScreen, lPrimaryScreenInfo))
                return;

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

        private void tblAllMenu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (mnMainMenu.megaDropBox.IsOpen)
            {
                mnMainMenu.megaDropBox.IsOpen = false;
            }
            else
            {
                MegadropMenuItem item = mnMainMenu.Items[0] as MegadropMenuItem;
                mnMainMenu.SelectedMenuItem = item;
                mnMainMenu.megaDropBox.IsOpen = true;
            }
        }

        private void btnAllMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mnMainMenu.megaDropBox.IsOpen)
                {
                    mnMainMenu.megaDropBox.IsOpen = false;
                }
                else
                {
                    MegadropMenuItem item = mnMainMenu.Items[0] as MegadropMenuItem;
                    mnMainMenu.SelectedMenuItem = item;
                    mnMainMenu.megaDropBox.IsOpen = true;
                }
            }
            catch { }
        }

        private void mnuItemClose_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (item != null)
            {
                ContextMenu ctm = (ContextMenu)item.Parent;
                C1TabItem tabitem = ctm.PlacementTarget as C1TabItem;
                tcMainContentTabControl.Items.Remove(tabitem);
            }
        }

        private void mnuItemAllClose_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (item != null)
                tcMainContentTabControl.Items.Clear();
        }

        private void mnuItemExceptAllClose_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (item != null)
            {
                ContextMenu ctm = (ContextMenu)item.Parent;
                C1TabItem seltabItem = ctm.PlacementTarget as C1TabItem;

                List<C1TabItem> lst = new List<C1TabItem>();

                foreach (C1TabItem tabitem in tcMainContentTabControl.Items)
                    lst.Add(tabitem);

                for (int idx = 0; idx < lst.Count; idx++)
                    if (lst[idx] != seltabItem)
                        tcMainContentTabControl.Items.Remove(lst[idx]);
            }
        }

        private void tblHelp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //SetMessage(System.DateTime.Now.ToString() +  " >>>>>>> Help", false);

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show("aaaaaaaaaaaaaaaaaaaa", null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);

            //C1Window win = new C1Window();
            //win.Show();

            ManualViewer mv = new ManualViewer();
            mv.ShowModal();

            //try
            //{
            //    StringBuilder sb = new StringBuilder();

            //    C1TabItem currentTabItem = tcMainContentTabControl.Items[tcMainContentTabControl.SelectedIndex] as C1TabItem;
            //    object menuData = currentTabItem.DataContext;
            //    string MENUID = DataTableConverter.GetValue(menuData, "MENUID").ToString();

            //    MenuItem menuItem = menuItemDicNormal[MENUID];
            //    string lv3MenuName = DataTableConverter.GetValue(menuData, "MENUNAME").ToString();

            //    MenuItem itemDepth2 = menuItem.Parent as MenuItem;
            //    string lv2MenuName = DataTableConverter.GetValue(itemDepth2.DataContext, "MENUNAME").ToString();

            //    MenuItem itemDepth3 = itemDepth2.Parent as MenuItem;
            //    string lv1MenuName = DataTableConverter.GetValue(itemDepth3.DataContext, "MENUNAME").ToString();

            //    sb.Append(lv1MenuName);
            //    sb.Append("," + lv2MenuName);
            //    sb.Append("," + lv3MenuName);

            //    ManualViewer mv = new ManualViewer(sb.ToString(), DataTableConverter.GetValue(menuData, "MAN_PAGE_NAME").ToString());
            //    mv.ShowModal();

            //    //StringBuilder sb = new StringBuilder();

            //    //C1TabItem currentTabItem = tcMainContentTabControl.Items[tcMainContentTabControl.SelectedIndex] as C1TabItem;
            //    //object menuData = currentTabItem.DataContext;
            //    //string MENUID = DataTableConverter.GetValue(menuData, "MENUID").ToString();

            //    //MegadropMenuItem menuItem = menuItemDic[MENUID];
            //    //string lv3MenuName = DataTableConverter.GetValue(menuData, "MENUNAME").ToString();

            //    //MegadropMenuItem itemDepth2 = menuItem.Parent as MegadropMenuItem;
            //    //string lv2MenuName = DataTableConverter.GetValue(itemDepth2.DataContext, "MENUNAME").ToString();

            //    //MegadropMenuItem itemDepth3 = itemDepth2.Parent as MegadropMenuItem;
            //    //string lv1MenuName = DataTableConverter.GetValue(itemDepth3.DataContext, "MENUNAME").ToString();

            //    //sb.Append(lv1MenuName);
            //    //sb.Append("," + lv2MenuName);
            //    //sb.Append("," + lv3MenuName);

            //    //ManualViewer mv = new ManualViewer(sb.ToString(), DataTableConverter.GetValue(menuData, "MAN_PAGE_NAME").ToString());
            //    //mv.ShowModal();
            //}
            //catch (Exception ex)
            //{
            //    ManualViewer mv = new ManualViewer();
            //    mv.ShowModal();
            //}
        }

        /// <summary>
        /// Configuration 버튼 Click 이벤트 핸들러
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tblSetting_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (configWindow != null)
                return;

            configWindow = new ConfigWindow();

            configWindow.Closed += (s, arg) =>
            {
                if (configWindow.DialogResult == MessageBoxResult.OK)
                {
                    configChanged();
                    Debug_Mode_Display();
                }

                configWindow = null;
            };

            Dispatcher.BeginInvoke(new Action(() => configWindow.ShowModal()));
        }

        private void grdUpdate_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("");
                Storyboard sb = grdUpdate.Resources["UpdateEnabledAnimation"] as Storyboard;
                ClockState aClockState = sb.GetCurrentState();

                if (aClockState == ClockState.Active)
                {
                    //FRA0002 : 계속하면 편집된 내용은 저장되지 않습니다. 계속하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FRA0002"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                            Close();
                    });
                }
            }
            catch { }
        }

        private void imgPrint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CustomConfig.Instance.Reload(ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_NAME"]);
            spPrinter_Loaded(null, null);
        }

        private void tcMainContentTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                dynamic contentArea = ((tcMainContentTabControl.SelectedItem as C1TabItem).Content as MainTabItemLayout).ContentArea;
                FrameOperation frameOperation = contentArea.FrameOperation;
                currentFrameOperation = frameOperation;

                if (myVersion != updateVersion)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //FRA0001 : 새로운 버전이 있습니다. 종료하시겠습니까?
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FRA0001"), null, "Update", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                                Close();
                        });
                    }));
                }
            }
            catch (Exception ex)
            { }
        }

        private void imgNoticeBoard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CheckPopup("BUTTON");
        }
        #endregion

        #region Mehod
        private void addBookmarkMenu(DataRow row)
        {
            try
            {
                (icFavorite.ItemsSource as ObservableCollection<object>).Add(menuItemDic[row["MENUID"].ToString()].DataContext);
            }
            catch (Exception ex)
            {
            }
        }

        private void CreateMenu()
        {
            if (isInitAuthority == true || string.IsNullOrEmpty(LoginInfo.CFG_AREA_ID)) // 2020-09-22
                return;

            DataTable dtIndata = new DataTable();
            dtIndata.Columns.Add("USERID", typeof(string));
            dtIndata.Columns.Add("PROGRAMTYPE", typeof(string));
            dtIndata.Columns.Add("MENUIUSE", typeof(string));
            dtIndata.Columns.Add("LANGID", typeof(string));
            dtIndata.Columns.Add("MENULEVEL", typeof(string));
            dtIndata.Columns.Add("SYSTEM_ID", typeof(string));
            dtIndata.Columns.Add("AREAID", typeof(string));

            DataRow menuIndata = dtIndata.NewRow();
            menuIndata["USERID"] = LoginInfo.USERID;
            menuIndata["PROGRAMTYPE"] = LGC.GMES.MES.Common.Common.APP_System;
            menuIndata["MENUIUSE"] = "Y";
            menuIndata["LANGID"] = LoginInfo.LANGID;
            menuIndata["MENULEVEL"] = null;
            menuIndata["SYSTEM_ID"] = LoginInfo.SYSID;
            menuIndata["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtIndata.Rows.Add(menuIndata);
            Logger.Instance.WriteLine("[FRAME] Create Menu", Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);

            new ClientProxy().ExecuteService("DA_BAS_SEL_MENU_WITH_BOOKMARK", "INDATA", "OUTDATA", dtIndata, (dtOutdata, menuException) =>
            {
                if (dtOutdata.Rows.Count == 0)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show("No execute authority. Contact administrator." /*실행 권한이 없습니다. 담당자에게 문의하세요.*/, null, "Information", MessageBoxButton.OK, MessageBoxIcon.Warning, (msgresult) =>
                    {
                        Close();
                    });
                }

                if (menuException != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(menuException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    DataRow[] drChk = dtOutdata.Select("MENULEVEL = '2'");

                    foreach (DataRow dr in drChk)
                    {
                        int iCnt = Convert.ToInt16(dtOutdata.Compute("Count(MENUID)", "MENUID_PR='" + dr["MENUID"] + "'"));

                        if (iCnt == 0)
                            dtOutdata.Rows.Remove(dr);
                    }

                    CreateNormalMenu(dtOutdata);
                    CreateMegadropMenu(dtOutdata);
                    if(!string.IsNullOrWhiteSpace(LoginInfo.CTX_MENUID))
                    {
                        object[] parameters = new object[2];
                        parameters[0] = string.Empty;
                        parameters[1] = string.Empty;

                        OpenMenu(GetMENUIDfromFORMID(LoginInfo.CTX_MENUID), false, parameters);
                    }
                    else{
                        OpenDefaultMenu();
                    }

                    grMenu_SizeChanged(grMenu, null);
                    Logger.Instance.WriteLine("[FRAME] Create Menu", Logger.MESSAGE_OPERATION_END, LogCategory.FRAME);

                    // [E20231011-000895] GMES Main 화면 배포이력 표기
                    if (CheckOnGMESMainStartUp()) OpenGMESMain();
                }
                catch (Exception ex)
                {
                    Logger.Instance.WriteLine("[FRAME] Create Menu", ex, LogCategory.FRAME);
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            });
        }

        private void CreateNormalMenu(DataTable menuResult)
        {
            tcMainContentTabControl.Items.Clear();
            munMainMenu.Items.Clear();
            menuItemDicNormal.Clear();

            IEnumerable menuList = DataTableConverter.Convert(menuResult);
            string MENUID_PR = "";
            ItemsControl parentMenu = munMainMenu;
            CreateChildNormalMenu(menuList, MENUID_PR, parentMenu);
        }

        /// <summary>
        /// 메뉴 생성
        /// </summary>
        private void CreateMegadropMenu(DataTable menuResult)
        {
            tcMainContentTabControl.Items.Clear();
            menuItemDic.Clear();
            mnMainMenu.Items.Clear();
            IEnumerable menuList = DataTableConverter.Convert(menuResult);
            string MENUID_PR = "";
            ItemsControl parentMenu = mnMainMenu;
            CreateChildMegadropMenu(menuList, MENUID_PR, parentMenu);
            CreateBookmark();
        }

        /// <summary>
        /// 메뉴 생성시 사용하는 재귀함수
        /// 전체 메뉴 정보 리스트에서 일치하는 부모 메뉴 ID를 가지는 메뉴 정보를 이용하여
        /// 메뉴 아이템을 생성하고 부모 메뉴 컨트롤에 추가한다
        /// </summary>
        /// <param name="menuList">전체 메뉴 정보 리스트</param>
        /// <param name="MENUID_PR">부모 메뉴 ID</param>
        /// <param name="parentMenu">부모 메뉴 컨트롤</param>

        private void CreateChildNormalMenu(IEnumerable menuList, string MENUID_PR, ItemsControl parentMenu)
        {
            foreach (object menu in menuList)
            {
                switch (DataTableConverter.GetValue(menu, "MENULEVEL").ToString())
                {
                    case "2":
                        MenuItem homeMenuItem = new MenuItem();
                        homeMenuItem.Header = DataTableConverter.GetValue(menu, "MENUNAME").ToString();
                        homeMenuItem.Name = DataTableConverter.GetValue(menu, "MENUID").ToString();
                        munMainMenu.Items.Add(homeMenuItem);
                        break;

                    case "3":
                        if (DataTableConverter.GetValue(menu, "MENULEVEL").ToString() == "3")
                        {
                            string parentsmenu = DataTableConverter.GetValue(menu, "MENUID_PR").ToString();

                            foreach (MenuItem parentsitem in munMainMenu.Items)
                            {
                                if (parentsitem.Name == parentsmenu)
                                {
                                    MenuItem childMenuItem = new MenuItem();
                                    childMenuItem.Header = DataTableConverter.GetValue(menu, "MENUNAME").ToString();
                                    childMenuItem.Name = DataTableConverter.GetValue(menu, "MENUID").ToString();
                                    childMenuItem.Click += childMenuItem_Click;
                                    childMenuItem.DataContext = menu;
                                    parentsitem.Items.Add(childMenuItem);
                                    menuItemDicNormal.Add(DataTableConverter.GetValue(menu, "MENUID").ToString(), childMenuItem);
                                    break;
                                }
                            }
                        }
                        break;
                }
            }
        }

        private void CreateChildMegadropMenu(IEnumerable menuList, string MENUID_PR, ItemsControl parentMenu)
        {
            IEnumerable<object> childMenuList = (from object menu in menuList where (DataTableConverter.GetValue(menu, "MENUID_PR") ?? string.Empty).Equals(MENUID_PR) orderby DataTableConverter.GetValue(menu, "MENUSEQ") select menu);

            foreach (object menu in childMenuList)
            {
                MegadropMenuItem menuItem = new MegadropMenuItem();
                menuItem.Header = DataTableConverter.GetValue(menu, "MENUNAME").ToString();
                menuItem.DataContext = menu;
                menuItemDic.Add(DataTableConverter.GetValue(menu, "MENUID").ToString(), menuItem);
                Image seperator = null;

                switch (DataTableConverter.GetValue(menu, "MENULEVEL").ToString())
                {
                    case "1":
                        menuItem.Style = (Style)mnMainMenu.Resources["FirstMenuItem"];
                        seperator = new Image();
                        break;

                    case "2":
                        menuItem.Style = (Style)mnMainMenu.Resources["SecondMenuItem"];
                        break;

                    default:
                        menuItem.Style = (Style)mnMainMenu.Resources["ThirdMenuItem"];
                        break;
                }

                parentMenu.Items.Add(menuItem);

                if (seperator != null)
                    parentMenu.Items.Add(seperator);

                CreateChildMegadropMenu(menuList, DataTableConverter.GetValue(menu, "MENUID").ToString(), menuItem);
            }
        }

        private void CreateBookmark()
        {
            icFavorite.ItemsSource = new ObservableCollection<object>();

            DataTable bookmarkIndataTable = new DataTable();
            bookmarkIndataTable.Columns.Add("LANGID", typeof(string));
            bookmarkIndataTable.Columns.Add("USERID", typeof(string));
            DataRow bookmarkIndata = bookmarkIndataTable.NewRow();
            bookmarkIndata["LANGID"] = LoginInfo.LANGID;
            bookmarkIndata["USERID"] = LoginInfo.USERID;
            bookmarkIndataTable.Rows.Add(bookmarkIndata);

            Logger.Instance.WriteLine("[FRAME] Create Bookmark", Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);

            new ClientProxy().ExecuteService("COR_SEL_BOOKMARK_BY_USERID", "INDATA", "OUTDATA", bookmarkIndataTable, (bookmarkResult, bookmarkException) =>
            {
                try
                {
                    if (bookmarkException != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(bookmarkException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    foreach (DataRow row in bookmarkResult.Rows)
                        addBookmarkMenu(row);

                    Logger.Instance.WriteLine("[FRAME] Create Bookmark", Logger.MESSAGE_OPERATION_END);
                }
                catch (Exception ex)
                {
                    Logger.Instance.WriteLine("[FRAME] Create Bookmark", ex, LogCategory.FRAME);
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            });
        }

        //활성화 팝업메뉴(?) Open용 - 메뉴 등록하지 않아도, Link된 화면 열림
        public void OpenMenuFORM(string sMenuID, string sFormID, string sNameSpace, string sMenuName, bool reopen = false, params object[] param)
        {
            Logger.Instance.WriteLine("[FRAME] Main Menu Open", Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);

            try
            {
                string MENUID = sMenuID;
                string MAINFORMPATH = sNameSpace;
                string MAINFORMNAME = sFormID;
                string MENUNAME = sMenuName;
                //object menuData = menuItemDicNormal[sMenuID].DataContext;

                for (int inx = 0; inx < tcMainContentTabControl.Items.Count; inx++)
                {
                    C1TabItem tabItem = tcMainContentTabControl.Items[inx] as C1TabItem;

                    //if (sMenuID.Equals(DataTableConverter.GetValue(tabItem.DataContext, "MENUID")))
                    if (MENUNAME.Equals(tabItem.Header))
                    {
                        if (reopen)
                        {
                            tcMainContentTabControl.Items.RemoveAt(inx);
                            inx--;
                            break;
                        }
                        else
                        {
                            tcMainContentTabControl.SelectedIndex = inx;
                            mnMainMenu.CloseMenu();
                            return;
                        }
                    }
                }

                Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
                Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
                object obj = Activator.CreateInstance(targetType);

                MainTabItemLayout tabItemLayout = new MainTabItemLayout() { Style = Application.Current.Resources.MergedDictionaries[Application.Current.Resources.MergedDictionaries.Count - 2]["MainTabItemBaseLayout"] as Style };

                tabItemLayout.BookmarkStatusChanged += (layoutSender, layoutArg) =>
                {
                    CreateBookmark();
                };

                if (obj is IWorkArea)
                {
                    List<PrinterControl> printerList = new List<PrinterControl>();
                    List<ScannerControl> scannerList = new List<ScannerControl>();

                    foreach (PrinterControl printer in spPrinter.Children)
                        printerList.Add(printer);

                    foreach (ScannerControl scanner in spPortCon.Children)
                        scannerList.Add(scanner);

                    //(obj as IWorkArea).FrameOperation = new FrameOperation(this, tabItemLayout, printerList) { MENUID = MENUID, AUTHORITY = "W", Parameters = param, IsCurrupted = false };
                    (obj as IWorkArea).FrameOperation = currentFrameOperation = new FrameOperation(this, tabItemLayout, printerList, scannerList) { MENUID = MENUID, AUTHORITY = "W", Parameters = param, IsCurrupted = false };
                }

                tabItemLayout.ContentArea = obj;
                tabItemLayout.Title = MENUNAME;

                tabItemLayout.TitleDepth2 = ConfigurationManager.AppSettings["APP_SERVER"];
                tabItemLayout.TitleDepth2Visibility = Visibility.Collapsed;

                //string parentMenuID = DataTableConverter.GetValue(menuData, "MENUID_PR").ToString();

                //if (menuItemDicNormal.ContainsKey(parentMenuID))
                //{
                //    MenuItem itemDepth2 = menuItemDicNormal[parentMenuID];
                //    tabItemLayout.TitleDepth2 = DataTableConverter.GetValue(itemDepth2.DataContext, "MENUNAME").ToString();
                //    parentMenuID = DataTableConverter.GetValue(itemDepth2.DataContext, "MENUID_PR").ToString();

                //    if (menuItemDicNormal.ContainsKey(parentMenuID))
                //    {
                //        MenuItem itemDepth3 = menuItemDicNormal[parentMenuID];
                //        tabItemLayout.TitleDepth3 = DataTableConverter.GetValue(itemDepth3.DataContext, "MENUNAME").ToString();
                //    }
                //}

                C1TabItem newTabItem = new C1TabItem() { Header = MENUNAME, Content = tabItemLayout, DataContext = null, Style = tcMainContentTabControl.Resources["MainContentTabItemStyle"] as Style };

                newTabItem.PreviewMouseLeftButtonDown += new MouseButtonEventHandler((s, e) => item_PreviewMouseLeftButtonDown(s, e, MENUID));
                newTabItem.PreviewMouseRightButtonDown += item_PreviewMouseRightButtonDown;

                tcMainContentTabControl.Items.Add(newTabItem);
                tcMainContentTabControl.SelectedIndex = tcMainContentTabControl.Items.Count - 1;

                if (Common.Common.APP_MODE == "DEBUG")
                    tabItemLayout.TitleToolTip = MAINFORMPATH + " >> " + MAINFORMNAME;
                else
                    tabItemLayout.TitleToolTip = null;


                DataTable menuHitIndata = new DataTable();
                menuHitIndata.Columns.Add("SYSTEM_ID", typeof(string));
                menuHitIndata.Columns.Add("MENUID", typeof(string));
                menuHitIndata.Columns.Add("USERID", typeof(string));
                menuHitIndata.Columns.Add("SHOPID", typeof(string));
                menuHitIndata.Columns.Add("AREAID", typeof(string));
                menuHitIndata.Columns.Add("EQSGID", typeof(string));
                menuHitIndata.Columns.Add("PROCID", typeof(string));
                menuHitIndata.Columns.Add("EQPTID", typeof(string));
                menuHitIndata.Columns.Add("VER_NO", typeof(string));

                DataRow menuHitRow = menuHitIndata.NewRow();
                menuHitRow["SYSTEM_ID"] = Common.Common.APP_System;
                menuHitRow["MENUID"] = MENUID;
                menuHitRow["USERID"] = LoginInfo.USERID;
                menuHitRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                menuHitRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                menuHitRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                menuHitRow["PROCID"] = LoginInfo.CFG_PROC_ID;
                menuHitRow["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                menuHitRow["VER_NO"] = tblVersion.Text;
                menuHitIndata.Rows.Add(menuHitRow);

                new ClientProxy().ExecuteService("COR_INS_MENU_USE_COUNT", "INDATA", null, menuHitIndata, (menuHitReuslt, menuHitException) => { });
                LoginInfo.CFG_MENUID = MENUID;

            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLine("[FRAME] Main menu open", ex);
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

            mnMainMenu.CloseMenu();

            Logger.Instance.WriteLine("[FRAME] Main Menu Open", Logger.MESSAGE_OPERATION_END, LogCategory.FRAME);
            return;
        }

        public void OpenMenu(string menuid, bool reopen = false, params object[] param)
        {
            try
            {
                OpenMenu(menuItemDicNormal[menuid].DataContext, reopen, param);
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage("오류")
                    , string.Format("{0} : MENUID ({1})", ex.Message, menuid)
                    , "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void OpenMenu(object menuData, bool reopen = false, params object[] param)
        {
            Logger.Instance.WriteLine("[FRAME] Main Menu Open", Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);

            try
            {
                string MENUID = DataTableConverter.GetValue(menuData, "MENUID").ToString();

                for (int inx = 0; inx < tcMainContentTabControl.Items.Count; inx++)
                {
                    C1TabItem tabItem = tcMainContentTabControl.Items[inx] as C1TabItem;

                    if (MENUID.Equals(DataTableConverter.GetValue(tabItem.DataContext, "MENUID")))
                    {
                        if (reopen)
                        {
                            tcMainContentTabControl.Items.RemoveAt(inx);
                            inx--;
                            break;
                        }
                        else
                        {
                            tcMainContentTabControl.SelectedIndex = inx;
                            mnMainMenu.CloseMenu();
                            return;
                        }
                    }
                }

                string MAINFORMPATH = DataTableConverter.GetValue(menuData, "NAMESPACE").ToString();
                string MAINFORMNAME = DataTableConverter.GetValue(menuData, "FORMID").ToString();
                
                Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
                Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
                object obj = Activator.CreateInstance(targetType);

                if (MAINFORMPATH.Contains("LGC.GMES.MES.MNT001"))
                {
                    Window win = obj as Window;

                    if (win != null)
                    {
                        // POPUP에서도 TAB OPEN 가능하게 변경 [2018-05-14] => 총괄님 협의 후 사용안함 폐기
                        //MainTabItemLayout tabItemLayout = new MainTabItemLayout() { Style = Application.Current.Resources.MergedDictionaries[Application.Current.Resources.MergedDictionaries.Count - 2]["MainTabItemBaseLayout"] as Style };

                        //List<PrinterControl> printerList = new List<PrinterControl>();
                        //List<ScannerControl> scannerList = new List<ScannerControl>();

                        //foreach (PrinterControl printer in spPrinter.Children)
                        //    printerList.Add(printer);

                        //foreach (ScannerControl scanner in spPortCon.Children)
                        //    scannerList.Add(scanner);

                        //currentFrameOperation = new FrameOperation(this, tabItemLayout, printerList, scannerList) { MENUID = MENUID, AUTHORITY = DataTableConverter.GetValue(menuData, "ACCESS_FLAG").ToString(), Parameters = param, IsCurrupted = false };

                        isOpenedLineMonitoring = true;

                        win.WindowState = WindowState.Maximized;
                        win.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        win.Owner = this;
                        //win.Tag = new object[] { DataTableConverter.GetValue(menuData, "MENUNAME").ToString(), currentFrameOperation };
                        win.Tag = new object[] { DataTableConverter.GetValue(menuData, "MENUNAME").ToString() };
                        win.ShowInTaskbar = false;
                        win.ShowDialog();
                    }
                }
                else
                {
                    MainTabItemLayout tabItemLayout = new MainTabItemLayout() { Style = Application.Current.Resources.MergedDictionaries[Application.Current.Resources.MergedDictionaries.Count - 2]["MainTabItemBaseLayout"] as Style };

                    tabItemLayout.BookmarkStatusChanged += (layoutSender, layoutArg) =>
                    {
                        CreateBookmark();
                    };

                    if (obj is IWorkArea)
                    {
                        List<PrinterControl> printerList = new List<PrinterControl>();
                        List<ScannerControl> scannerList = new List<ScannerControl>();

                        foreach (PrinterControl printer in spPrinter.Children)
                            printerList.Add(printer);

                        foreach (ScannerControl scanner in spPortCon.Children)
                            scannerList.Add(scanner);

                        //(obj as IWorkArea).FrameOperation = new FrameOperation(this, tabItemLayout, printerList) { MENUID = MENUID, AUTHORITY = "W", Parameters = param, IsCurrupted = false };
                        (obj as IWorkArea).FrameOperation = currentFrameOperation = new FrameOperation(this, tabItemLayout, printerList, scannerList) { MENUID = MENUID, AUTHORITY = DataTableConverter.GetValue(menuData, "ACCESS_FLAG").ToString(), Parameters = param, IsCurrupted = false };
                    }

                    tabItemLayout.ContentArea = obj;
                    tabItemLayout.Title = DataTableConverter.GetValue(menuData, "MENUNAME").ToString();

                    tabItemLayout.TitleDepth2 = ConfigurationManager.AppSettings["APP_SERVER"];
                    tabItemLayout.TitleDepth2Visibility = Visibility.Collapsed;

                    string parentMenuID = DataTableConverter.GetValue(menuData, "MENUID_PR").ToString();

                    if (menuItemDicNormal.ContainsKey(parentMenuID))
                    {
                        MenuItem itemDepth2 = menuItemDicNormal[parentMenuID];
                        tabItemLayout.TitleDepth2 = DataTableConverter.GetValue(itemDepth2.DataContext, "MENUNAME").ToString();
                        parentMenuID = DataTableConverter.GetValue(itemDepth2.DataContext, "MENUID_PR").ToString();

                        if (menuItemDicNormal.ContainsKey(parentMenuID))
                        {
                            MenuItem itemDepth3 = menuItemDicNormal[parentMenuID];
                            tabItemLayout.TitleDepth3 = DataTableConverter.GetValue(itemDepth3.DataContext, "MENUNAME").ToString();
                        }
                    }

                    C1TabItem newTabItem = new C1TabItem() { Header = DataTableConverter.GetValue(menuData, "MENUNAME"), Content = tabItemLayout, DataContext = menuData, Style = tcMainContentTabControl.Resources["MainContentTabItemStyle"] as Style };

                    newTabItem.PreviewMouseLeftButtonDown += new MouseButtonEventHandler((s, e) => item_PreviewMouseLeftButtonDown(s, e, MENUID));
                    newTabItem.PreviewMouseRightButtonDown += item_PreviewMouseRightButtonDown;

                    tcMainContentTabControl.Items.Add(newTabItem);
                    tcMainContentTabControl.SelectedIndex = tcMainContentTabControl.Items.Count - 1;

                    if (Common.Common.APP_MODE == "DEBUG")
                        tabItemLayout.TitleToolTip = MAINFORMPATH + " >> " + MAINFORMNAME;
                    else
                        tabItemLayout.TitleToolTip = null;
                }

                DataTable menuHitIndata = new DataTable();
                menuHitIndata.Columns.Add("SYSTEM_ID", typeof(string));
                menuHitIndata.Columns.Add("MENUID", typeof(string));
                menuHitIndata.Columns.Add("USERID", typeof(string));
                menuHitIndata.Columns.Add("SHOPID", typeof(string));
                menuHitIndata.Columns.Add("AREAID", typeof(string));
                menuHitIndata.Columns.Add("EQSGID", typeof(string));
                menuHitIndata.Columns.Add("PROCID", typeof(string));
                menuHitIndata.Columns.Add("EQPTID", typeof(string));
                menuHitIndata.Columns.Add("VER_NO", typeof(string));

                DataRow menuHitRow = menuHitIndata.NewRow();
                menuHitRow["SYSTEM_ID"] = Common.Common.APP_System;
                menuHitRow["MENUID"] = MENUID;
                menuHitRow["USERID"] = LoginInfo.USERID;
                menuHitRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                menuHitRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                menuHitRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                menuHitRow["PROCID"] = LoginInfo.CFG_PROC_ID;
                menuHitRow["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                menuHitRow["VER_NO"] = tblVersion.Text;
                menuHitIndata.Rows.Add(menuHitRow);

                new ClientProxy().ExecuteService("COR_INS_MENU_USE_COUNT", "INDATA", null, menuHitIndata, (menuHitReuslt, menuHitException) => { });
                LoginInfo.CFG_MENUID = MENUID;
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLine("[FRAME] Main menu open", ex);
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

            mnMainMenu.CloseMenu();

            Logger.Instance.WriteLine("[FRAME] Main Menu Open", Logger.MESSAGE_OPERATION_END, LogCategory.FRAME);
            return;
        }

        void item_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e, string menuId)
        {
            // 2023.11.22. 여러 탭이 열려있는 상황에서, UI 작업 이력이 제일 왼쪽 탭으로 남는 오류 수정(TB_SFC_UI_EVENT_HIST)
            if (!LoginInfo.CFG_MENUID.Equals(menuId))
            {
                LoginInfo.CFG_MENUID = menuId;
            }
        }

        void item_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            C1TabItem tabitem = e.Source as C1TabItem;

            if (tabitem != null)
            {
                tabitem.IsSelected = true;
                ContextMenu menu = new ContextMenu();

                MenuItem mnuItemClose = new MenuItem();
                mnuItemClose.Header = ObjectDic.Instance.GetObjectName("닫기");
                mnuItemClose.Click += mnuItemClose_Click;
                menu.Items.Add(mnuItemClose);

                MenuItem mnuItemAllClose = new MenuItem();
                mnuItemAllClose.Header = ObjectDic.Instance.GetObjectName("모두 닫기");
                mnuItemAllClose.Click += mnuItemAllClose_Click;
                menu.Items.Add(mnuItemAllClose);

                MenuItem mnuItemExceptAllClose = new MenuItem();
                mnuItemExceptAllClose.Header = ObjectDic.Instance.GetObjectName("이 창을 제외하고 모두 닫기");
                mnuItemExceptAllClose.Click += mnuItemExceptAllClose_Click;
                menu.Items.Add(mnuItemExceptAllClose);

                C1TabItem item = (C1TabItem)sender;
                item.ContextMenu = menu;
            }
            else
            {
                C1TabItem item = (C1TabItem)sender;
                item.ContextMenu = null;
            }
        }

        private void MnuClose_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void OpenDumyMenu(object menuData, bool reopen = false, params object[] param)
        {
            //개발 초기 메뉴 데이터 없이 구동 하도록 개발함.
            Logger.Instance.WriteLine("[FRAME] Main menu open", Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);

            try
            {
                string FORMID = DataTableConverter.GetValue(menuData, "FORMID").ToString();

                for (int inx = 0; inx < tcMainContentTabControl.Items.Count; inx++)
                {
                    C1TabItem tabItem = tcMainContentTabControl.Items[inx] as C1TabItem;

                    if (FORMID.Equals(DataTableConverter.GetValue(tabItem.DataContext, "FORMID")))
                    {
                        if (reopen)
                        {
                            tcMainContentTabControl.Items.RemoveAt(inx);
                            inx--;
                            break;
                        }
                        else
                        {
                            tcMainContentTabControl.SelectedIndex = inx;
                            mnMainMenu.CloseMenu();
                            return;
                        }
                    }
                }

                string MAINFORMPATH = DataTableConverter.GetValue(menuData, "NAMESPACE").ToString();
                string MAINFORMNAME = DataTableConverter.GetValue(menuData, "FORMID").ToString();

                Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
                Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
                object obj = Activator.CreateInstance(targetType);

                MainTabItemLayout tabItemLayout = new MainTabItemLayout() { Style = Application.Current.Resources.MergedDictionaries[Application.Current.Resources.MergedDictionaries.Count - 2]["MainTabItemBaseLayout"] as Style };

                tabItemLayout.BookmarkStatusChanged += (layoutSender, layoutArg) =>
                {
                    CreateBookmark();
                };

                if (obj is IWorkArea)
                {
                    List<PrinterControl> printerList = new List<PrinterControl>();
                    List<ScannerControl> scannerList = new List<ScannerControl>();

                    foreach (PrinterControl printer in spPrinter.Children)
                        printerList.Add(printer);

                    foreach (ScannerControl scanner in spPortCon.Children)
                        scannerList.Add(scanner);

                    //(obj as IWorkArea).FrameOperation = new FrameOperation(this, tabItemLayout, printerList) { MENUID = "Dumy", AUTHORITY = "W", Parameters = param, IsCurrupted = false };
                    (obj as IWorkArea).FrameOperation = currentFrameOperation = new FrameOperation(this, tabItemLayout, printerList, scannerList) { MENUID = "Dumy", AUTHORITY = DataTableConverter.GetValue(menuData, "ACCESS_FLAG").ToString(), Parameters = param, IsCurrupted = false };
                }

                tabItemLayout.ContentArea = obj;
                tabItemLayout.Title = DataTableConverter.GetValue(menuData, "MENUNAME").ToString();
                string parentMenuID = string.Empty; //DataTableConverter.GetValue(menuData, "MENUID_PR").ToString();

                if (menuItemDicNormal.ContainsKey(parentMenuID))
                {
                    MenuItem itemDepth2 = menuItemDicNormal[parentMenuID];
                    tabItemLayout.TitleDepth2 = DataTableConverter.GetValue(itemDepth2.DataContext, "MENUNAME").ToString();
                    parentMenuID = DataTableConverter.GetValue(itemDepth2.DataContext, "MENUID_PR").ToString();

                    if (menuItemDicNormal.ContainsKey(parentMenuID))
                    {
                        MenuItem itemDepth3 = menuItemDicNormal[parentMenuID];
                        tabItemLayout.TitleDepth3 = DataTableConverter.GetValue(itemDepth3.DataContext, "MENUNAME").ToString();
                    }
                }

                C1TabItem newTabItem = new C1TabItem() { Header = DataTableConverter.GetValue(menuData, "MENUNAME"), Content = tabItemLayout, DataContext = menuData, Style = tcMainContentTabControl.Resources["MainContentTabItemStyle"] as Style };
                tcMainContentTabControl.Items.Add(newTabItem);
                tcMainContentTabControl.SelectedIndex = tcMainContentTabControl.Items.Count - 1;

                if (LGC.GMES.MES.Common.Common.APP_MODE == "DEBUG")
                    tabItemLayout.TitleToolTip = MAINFORMPATH + " >> " + MAINFORMNAME;
                else
                    tabItemLayout.TitleToolTip = null;

                //DataTable menuHitIndata = new DataTable();
                //menuHitIndata.Columns.Add("SYSTEM_ID", typeof(string));
                //menuHitIndata.Columns.Add("MENUID", typeof(string));
                //menuHitIndata.Columns.Add("USERID", typeof(string));
                //DataRow menuHitRow = menuHitIndata.NewRow();
                //menuHitRow["SYSTEM_ID"] = LoginInfo.CFG_SHOP_ID;
                //menuHitRow["MENUID"] = "Dumy";
                //menuHitRow["USERID"] = LoginInfo.USERID;
                //menuHitIndata.Rows.Add(menuHitRow);
                //new ClientProxy().ExecuteService("COR_INS_MENU_USE_COUNT", "INDATA", null, menuHitIndata, (menuHitReuslt, menuHitException) => { });
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLine("[FRAME] Main menu open", ex, LogCategory.FRAME);
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

            mnMainMenu.CloseMenu();

            Logger.Instance.WriteLine("[FRAME] Main menu open", Logger.MESSAGE_OPERATION_END, LogCategory.FRAME);
        }

        public void OpenDumyMenu(string sFormID, string sFormName, string sMenuSpace, bool reopen = false, params object[] param)
        {
            Logger.Instance.WriteLine("[FRAME] Main menu open", Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);

            try
            {
                for (int inx = 0; inx < tcMainContentTabControl.Items.Count; inx++)
                {
                    C1TabItem tabItem = tcMainContentTabControl.Items[inx] as C1TabItem;

                    if (sFormID.Equals(DataTableConverter.GetValue(tabItem.DataContext, "FORMID")))
                    {
                        if (reopen)
                        {
                            tcMainContentTabControl.Items.RemoveAt(inx);
                            inx--;
                            break;
                        }
                        else
                        {
                            tcMainContentTabControl.SelectedIndex = inx;
                            mnMainMenu.CloseMenu();
                            return;
                        }
                    }
                }

                Assembly asm = Assembly.LoadFrom("ClientBin\\" + sMenuSpace + ".dll");
                Type targetType = asm.GetType(sMenuSpace + "." + sFormID);
                object obj = Activator.CreateInstance(targetType);

                MainTabItemLayout tabItemLayout = new MainTabItemLayout() { Style = Application.Current.Resources.MergedDictionaries[Application.Current.Resources.MergedDictionaries.Count - 2]["MainTabItemBaseLayout"] as Style };

                tabItemLayout.BookmarkStatusChanged += (layoutSender, layoutArg) =>
                {
                    CreateBookmark();
                };

                if (obj is IWorkArea)
                {
                    List<PrinterControl> printerList = new List<PrinterControl>();
                    List<ScannerControl> scannerList = new List<ScannerControl>();

                    foreach (PrinterControl printer in spPrinter.Children)
                        printerList.Add(printer);

                    foreach (ScannerControl scanner in spPortCon.Children)
                        scannerList.Add(scanner);

                    (obj as IWorkArea).FrameOperation = currentFrameOperation = new FrameOperation(this, tabItemLayout, printerList, scannerList) { MENUID = "Dummy", AUTHORITY = "Y", Parameters = param, IsCurrupted = false };
                }

                tabItemLayout.ContentArea = obj;
                tabItemLayout.Title = sFormName;
                string parentMenuID = string.Empty;

                if (menuItemDicNormal.ContainsKey(parentMenuID))
                {
                    MenuItem itemDepth2 = menuItemDicNormal[parentMenuID];
                    tabItemLayout.TitleDepth2 = DataTableConverter.GetValue(itemDepth2.DataContext, "MENUNAME").ToString();
                    parentMenuID = DataTableConverter.GetValue(itemDepth2.DataContext, "MENUID_PR").ToString();

                    if (menuItemDicNormal.ContainsKey(parentMenuID))
                    {
                        MenuItem itemDepth3 = menuItemDicNormal[parentMenuID];
                        tabItemLayout.TitleDepth3 = DataTableConverter.GetValue(itemDepth3.DataContext, "MENUNAME").ToString();
                    }
                }

                C1TabItem newTabItem = new C1TabItem() { Header = sFormName, Content = tabItemLayout, DataContext = null, Style = tcMainContentTabControl.Resources["MainContentTabItemStyle"] as Style };
                tcMainContentTabControl.Items.Add(newTabItem);
                tcMainContentTabControl.SelectedIndex = tcMainContentTabControl.Items.Count - 1;
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLine("[FRAME] Main menu open", ex, LogCategory.FRAME);
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

            Logger.Instance.WriteLine("[FRAME] Main menu open", Logger.MESSAGE_OPERATION_END, LogCategory.FRAME);
        }


        /// <summary>
        /// 설정 정보 반영
        /// 공정 작업 화면 영역 초기화
        /// 공정 정보 Reload
        /// COM Port Reload
        /// Printer Reload
        /// </summary>
        private void configChanged()
        {
            bool isObjectDicLoaded = false;
            bool isMessageDicLoaded = false;

            Logger.Instance.WriteLine("[FRAME] Applying changed configuration", Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);
            Logger.Instance.WriteLine("[FRAME] Login", Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);
            DataTable loginIndataTable = new DataTable();
            loginIndataTable.Columns.Add("USERID", typeof(string));
            loginIndataTable.Columns.Add("USERPSWD", typeof(string));
            loginIndataTable.Columns.Add("LANGID", typeof(string));
            DataRow loginIndata = loginIndataTable.NewRow();
            loginIndata["USERID"] = LoginInfo.USERID;
            loginIndata["LANGID"] = LoginInfo.LANGID;
            loginIndataTable.Rows.Add(loginIndata);

            new ClientProxy().ExecuteService("DA_BAS_SEL_PERSON_TBL", "INDATA", "OUTDATA", loginIndataTable, (loginOutdata, loginException) =>
            {
                if (loginException != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(loginException.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataRow userInfo = loginOutdata.Rows[0];
                LoginInfo.USGRID = userInfo["USGRID"] != null ? userInfo["USGRID"].ToString() : string.Empty;
                LoginInfo.USERID = userInfo["USERID"] != null ? userInfo["USERID"].ToString() : string.Empty;
                LoginInfo.USERNAME = userInfo["USERNAME"] != null ? userInfo["USERNAME"].ToString() : string.Empty;
                LoginInfo.AUTHID = userInfo["AUTHID"] != null ? userInfo["AUTHID"].ToString() : string.Empty;
                LoginInfo.USERMAIL = userInfo["USERMAIL"] != null ? userInfo["USERMAIL"].ToString() : string.Empty;
                tblUserName.Text = LoginInfo.USERNAME;

                DataTable dtUserType = new ClientProxy().ExecuteServiceSync("CUS_SEL_PERSONTYPE_USERID", "INDATA", "OUTDATA", loginIndataTable);

                if (dtUserType.Rows.Count > 0)
                {
                    LoginInfo.LOGGEDBYSSO = dtUserType.Rows[0]["USERTYPE"].ToString().Equals("G") ? true : false;
                    LoginInfo.USERTYPE = dtUserType.Rows[0]["USERTYPE"].ToString();
                }
                else
                {
                    LoginInfo.LOGGEDBYSSO = false;
                }

                Logger.Instance.WriteLine("[FRAME] Login", Logger.MESSAGE_OPERATION_END, LogCategory.FRAME);

                //============================================================================================================================
                Logger.Instance.WriteLine("[FRAME] Object Dic", Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);
                DataTable objectDicIndataTable = new DataTable();
                objectDicIndataTable.Columns.Add("LANGID", typeof(string));
                objectDicIndataTable.Columns.Add("OBJECTIUSE", typeof(string));
                DataRow objectDicIndata = objectDicIndataTable.NewRow();
                objectDicIndata["LANGID"] = LoginInfo.LANGID;
                objectDicIndata["OBJECTIUSE"] = "Y";
                objectDicIndataTable.Rows.Add(objectDicIndata);

                new ClientProxy().ExecuteService("CUS_SEL_OBJECTDIC_BAS", "INDATA", "OUTDATA", objectDicIndataTable, (objectDicResult, objectDicException) =>
                {
                    if (objectDicException != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(objectDicException.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    ObjectDic.Instance.SetObjectDicData(objectDicResult);
                    isObjectDicLoaded = true;

                    // MAINFRAME 별도 다국어 처리 [2018-01-04]
                    tblCharger.Text = ObjectDic.Instance.GetObjectName("비상연락망");

                    Logger.Instance.WriteLine("[FRAME] Object Dic", Logger.MESSAGE_OPERATION_END);

                    tcMainContentTabControl.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        tcMainContentTabControl.Items.Clear();
                    }));

                    //spPortCon_Loaded(null, null);
                    spPrinter_Loaded(null, null);
                    CreateMenu();
                    Debug_Mode_Display();
                    Logger.Instance.WriteLine("[FRAME] Applying changed configuration", Logger.MESSAGE_OPERATION_END, LogCategory.FRAME);

                    //2024.06.27 / 권용섭(cnsyongsub) / [E20240620-001371] 보정 dOCV 전송실패시 팝업 알림 여부
                    if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
                    {
                        SetFormCheckTimerRestart();
                    }
                }, null, false, true);
                //============================================================================================================================

                Logger.Instance.WriteLine("[FRAME] Message Dic", Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);
                DataTable messageDicIndataTable = new DataTable();
                messageDicIndataTable.Columns.Add("LANGID", typeof(string));
                messageDicIndataTable.Columns.Add("MSGIUSE", typeof(string));
                DataRow messageDicIndata = messageDicIndataTable.NewRow();
                messageDicIndata["LANGID"] = LoginInfo.LANGID;
                messageDicIndata["MSGIUSE"] = "Y";
                messageDicIndataTable.Rows.Add(messageDicIndata);

                new ClientProxy().ExecuteService("CUS_SEL_MESSAGE_BAS", "INDATA", "OUTDATA", messageDicIndataTable, (messageDicResult, messageDicException) =>
                {
                    if (messageDicException != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(messageDicException.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    MessageDic.Instance.SetMessageDicData(messageDicResult);
                    isMessageDicLoaded = true;
                    GetLineInfo();
                    grMenu_SizeChanged(grMenu, null);

                    Logger.Instance.WriteLine("[FRAME] Message Dic", Logger.MESSAGE_OPERATION_END, LogCategory.FRAME);
                }, null, false, true);
            });
        }

        private void OpenDefaultMenu()
        {
            Logger.Instance.WriteLine("[FRAME] Default Menu Open", Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);

            string defaultMenuID = LoginInfo.CFG_INI_MENUID;

            if (menuItemDic.ContainsKey(defaultMenuID))
            {
                MegadropMenuItem item = menuItemDic[defaultMenuID];

                if (item != null)
                    OpenMenu(item.DataContext);
            }
            tcMainContentTabControl.SelectionChanged -= tcMainContentTabControl_SelectionChanged;
            tcMainContentTabControl.SelectionChanged += tcMainContentTabControl_SelectionChanged;

            Logger.Instance.WriteLine("[FRAME] Default Menu Open", Logger.MESSAGE_OPERATION_END, LogCategory.FRAME);
        }

        /// <summary>
        /// 메세지 영역에 지정된 메세지를 표시한다
        /// </summary>
        /// <param name="msg"></param>
        public void SetMessage(string msg, bool isUrgent = false)
        {
            mainMessage.Text = msg;
            //mainMessage.Text = "[" + DateTime.Now.ToString("G") + "] " + msg;
            mainMessage.IsUrgent = isUrgent;

            Grid grid = new Grid() { Margin = new Thickness(0, 11, 0, 0) };
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            FlowMessage flowMessage = new FlowMessage() { Text = msg, IsUrgent = isUrgent, VerticalAlignment = VerticalAlignment.Center };
            //FlowMessage flowMessage = new FlowMessage() { Text = "[" + DateTime.Now.ToString("G") + "]" + msg, IsUrgent = isUrgent, VerticalAlignment = VerticalAlignment.Center };
            grid.Children.Add(flowMessage);

            Image deleteMessageImg = new Image() { Margin = new Thickness(11, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, Width = 11, Height = 11, Source = new BitmapImage(new Uri("pack://application:,,,/Images/LGC/MDI_msg_list_erase.png", UriKind.Absolute)) };
            Grid.SetColumn(deleteMessageImg, 1);
            grid.Children.Add(deleteMessageImg);

            deleteMessageImg.MouseLeftButtonDown += (sender, arg) =>
            {
                Image self = sender as Image;
                Grid parent = self.Parent as Grid;
                msgStackPanel.Children.Remove(parent);

                if (msgStackPanel.Children.Count == 0)
                {
                    mainMessage.Text = string.Empty;
                }
                else
                {
                    Grid lastMessageGrid = msgStackPanel.Children[msgStackPanel.Children.Count - 1] as Grid;
                    FlowMessage lastMessage = lastMessageGrid.Children[0] as FlowMessage;
                    mainMessage.Text = lastMessage.Text;
                    mainMessage.IsUrgent = lastMessage.IsUrgent;
                }
            };

            msgStackPanel.Children.Add(grid);

            if (msgStackPanel.Children.Count > 8)
                msgStackPanel.Children.RemoveAt(0);
        }

        public void ClearMessage()
        {
            msgStackPanel.Children.Clear();

            mainMessage.Text = string.Empty;
            mainMessage.IsUrgent = false;
        }

        private void beginBizLogTimer()
        {
            //Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    _BizLogLevelTimer = new Timer(5 * 1000);
            //    _BizLogLevelTimer.AutoReset = true;
            //    _BizLogLevelTimer.Elapsed += (s, arg) =>
            //    {
            //        DataTable bizLogLevelIndataTable = new DataTable();
            //        bizLogLevelIndataTable.Columns.Add("KEYID", typeof(string));
            //        DataRow bizLogLevelIndata = bizLogLevelIndataTable.NewRow();
            //        bizLogLevelIndata["KEYID"] = "BIZLOGLEVEL";
            //        bizLogLevelIndataTable.Rows.Add(bizLogLevelIndata);

            //        Dispatcher.BeginInvoke(new Action(() =>
            //        {
            //            new ClientProxy().ExecuteService("COR_SEL_CONFIG_TBL", "INDATA", "OUTDATA", bizLogLevelIndataTable, (result, ex) =>
            //            {
            //                if (ex != null)
            //                {
            //                    Dispatcher.BeginInvoke(new Action(() => Common.Common.bizloglevel = "-1"));
            //                }
            //                else
            //                {
            //                    if (result.Rows.Count == 0 || !"Y".Equals(result.Rows[0]["KEYIUSE"]))
            //                    {
            //                        Dispatcher.BeginInvoke(new Action(() => Common.Common.bizloglevel = "-1"));
            //                    }
            //                    else
            //                    {
            //                        Dispatcher.BeginInvoke(new Action(() => Common.Common.bizloglevel = result.Rows[0]["KEYVALUE"].ToString()));
            //                    }
            //                }
            //            }
            //            );
            //        }));
            //    };
            //    _BizLogLevelTimer.Start();
            //}
            //));
        }

        private void beginCheckUpdate()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                CheckUpdate();
                _UpdateTimer = new Timer(60 * 1000);
                _UpdateTimer.AutoReset = true;

                _UpdateTimer.Elapsed += (checkSender, checkArg) =>
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    CheckUpdate();

                    if (!isOpenedLineMonitoring)
                        CheckNoticeBoard();
                };

                _UpdateTimer.Start();
            }));

            #region MyRegion
            //Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    WebClient client = new WebClient();
            //    client.OpenReadCompleted += (s, arg) =>
            //    {
            //        if (arg.Error == null)
            //        {
            //            StreamReader sr = new StreamReader(arg.Result);
            //            updateString = sr.ReadToEnd();
            //            sr.Close();

            //            _UpdateTimer = new Timer(60 * 1000);
            //            _UpdateTimer.AutoReset = true;

            //            _UpdateTimer.Elapsed += (checkSender, checkArg) =>
            //            {
            //                WebClient updateCheckClient = new WebClient();

            //                updateCheckClient.OpenReadCompleted += (updateSender, updateArg) =>
            //                {
            //                    if (updateArg.Error == null)
            //                    {
            //                        StreamReader updateReader = new StreamReader(updateArg.Result);
            //                        string checkString = updateReader.ReadToEnd();

            //                        if (!updateString.Equals(checkString))
            //                            (grdUpdate.Resources["UpdateEnabledAnimation"] as Storyboard).Begin();

            //                        updateReader.Close();
            //                    }
            //                };

            //                Dispatcher.BeginInvoke(new Action(() =>
            //                {
            //                    updateCheckClient.OpenReadAsync(new Uri(Common.Common.DeploymentUrl + "CheckUpdate.ashx"));
            //                }));
            //            };

            //            _UpdateTimer.Start();
            //        }
            //    };

            //    client.OpenReadAsync(new Uri(Common.Common.DeploymentUrl + "CheckUpdate.ashx"));
            //})); 
            #endregion
        }

        //활성화 : 공정 대기 시간 초과 현황 팝업 체크용 Timer - 2022-09-27
        private void formProcWaitLimitTimeOverCheck()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _FormProcWaitLimitTimeOver = new Timer(60 * 30 * 1000);
                _FormProcWaitLimitTimeOver.AutoReset = true;

                _FormProcWaitLimitTimeOver.Elapsed += (checkSenders, checkArg) =>
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    _FormProcWaitLimitTimeOver.Stop();
                    //2023.01.17 : 공정 대기시간 초과 팝업 관련 Setting Flag 추가 - leeyj
                    if(LoginInfo.CFG_FORM_PROC_WAIT_LIMIT_TIME_OVER) ChkFormProcWaitLimit();
                };

                _FormProcWaitLimitTimeOver.Start();
            }));
        }

        //활성화 : 공정 대기 시간 초과 현황 팝업 체크용 Timer - 2022-09-27
        private void ChkFormProcWaitLimit()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dt.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_SEL_PROCESS_WAIT_LIMIT_TRAY_LIST", "RQSTDT", "RSLTDT", dt, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        //POPUP
                        FCS001_005_PROCWAIT_LIMIT_TIME_OVER wndRunStart = new FCS001_005_PROCWAIT_LIMIT_TIME_OVER();
                        wndRunStart.FrameOperation = FrameOperation;

                        if (wndRunStart != null)
                        {
                            object[] Parameters = new object[1];
                            Parameters[0] = result;

                            C1WindowExtension.SetParameters(wndRunStart, Parameters);

                            wndRunStart.Closed += new EventHandler(wndProcWait_Closed);
                            wndRunStart.ShowModal();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //활성화 : 공정 대기 시간 초과 현황 팝업 체크용 Timer - 2022-09-27
        private void wndProcWait_Closed(object sender, EventArgs e)
        {
            _FormProcWaitLimitTimeOver.Start();
        }


        private void formAgingLimitTimeOverCheck()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _FormAgingLimitTimeOver = new Timer(60 * 30 * 1000);
                _FormAgingLimitTimeOver.AutoReset = true;

                _FormAgingLimitTimeOver.Elapsed += (checkSenders, checkArg) =>
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    _FormAgingLimitTimeOver.Stop();
                    //2023.01.17 : Aging 시간 초과 팝업 관련 Setting Flag 추가 - leeyj
                    if (LoginInfo.CFG_FORM_AGING_LIMIT_TIME_OVER) ChkFormAgingLimit();
                };

                _FormAgingLimitTimeOver.Start();
            }));
        }

        private void ChkFormAgingLimit()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("LANGID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dt.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_SEL_AGING_LIMIT_TRAY_CNT", "RQSTDT", "RSLTDT", dt, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        //POPUP
                        FCS001_014_AGING_LIMIT_TIME_OVER wndRunStart = new FCS001_014_AGING_LIMIT_TIME_OVER();
                        wndRunStart.FrameOperation = FrameOperation;

                        if (wndRunStart != null)
                        {
                            object[] Parameters = new object[1];
                            Parameters[0] = result;

                            C1WindowExtension.SetParameters(wndRunStart, Parameters);

                            wndRunStart.Closed += new EventHandler(wndRunStart_Closed);
                            wndRunStart.ShowModal();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void wndRunStart_Closed(object sender, EventArgs e)
        {
            _FormAgingLimitTimeOver.Start();
        }

        #region
        private void formAgingOutputTimeOverCheck()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _FormAgingOutputTimeOver = new Timer(60 * 1000); // 1분
                _FormAgingOutputTimeOver.AutoReset = true;

                _FormAgingOutputTimeOver.Elapsed += (checkSenders, checkArg) =>
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    _FormAgingOutputTimeOver.Stop();
                    if (LoginInfo.CFG_FORM_AGING_OUTPUT_TIME_OVER) ChkFormAgingOutput();
                };

                _FormAgingOutputTimeOver.Start();
            }));
        }

        private void ChkFormAgingOutput()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("MAINPOPYN", typeof(string)); // 2024.01.05 Main Popup Flag Add

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["MAINPOPYN"] = "Y"; // 2024.01.05 Main Popup Flag Add
                dt.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_SEL_AGING_OUTPUT_TIME_OVER_TRAY_CNT", "RQSTDT", "RSLTDT", dt, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        //POPUP
                        FCS001_164_AGING_OUTPUT_TIME_OVER wndAgingOutputStart = new FCS001_164_AGING_OUTPUT_TIME_OVER();
                        wndAgingOutputStart.FrameOperation = FrameOperation;

                        if (wndAgingOutputStart != null)
                        {
                            object[] Parameters = new object[1];
                            Parameters[0] = result;

                            C1WindowExtension.SetParameters(wndAgingOutputStart, Parameters);

                            wndAgingOutputStart.Closed += new EventHandler(wndAgingOutputStart_Closed);
                            wndAgingOutputStart.ShowModal();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void wndAgingOutputStart_Closed(object sender, EventArgs e)
        {
            _FormAgingOutputTimeOver.Start();
        }
        #endregion

        #region 2024.06.27 / 권용섭(cnsyongsub) / [E20240620-001371] 보정 dOCV 송/수신 실패시 팝업 알림
        #region 동별 공통코드 보정 dOCV 송/수신 실패 사용여부
        /// <summary>
        /// 동별 공통코드 보정 dOCV 송/수신 실패 사용여부
        /// </summary>
        private void GetFormFittedDOCVTrnfFailPopup()
        {
            try
            {
                #region Defined DataTable and DataRow Variable
                /// <summary> DataTable </summary>
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("COM_CODE", typeof(string));
                dtRQSTDT.Columns.Add("USE_FLAG", typeof(string));

                /// <summary> DataRow </summary>
                DataRow drIssue = dtRQSTDT.NewRow();
                drIssue["LANGID"] = LoginInfo.LANGID;
                drIssue["AREAID"] = LoginInfo.CFG_AREA_ID;
                drIssue["COM_TYPE_CODE"] = "FORM_SITE_BASE_INFO";
                drIssue["COM_CODE"] = "FORM_FITTED_DOCV_TRNF_FAIL_POPUP";
                drIssue["USE_FLAG"] = "Y";
                dtRQSTDT.Rows.Add(drIssue);
                #endregion

                #region Excute Data Access
                new ClientProxy().ExecuteService("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", dtRQSTDT, (dtRSLTDT, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (dtRSLTDT.Rows.Count > 0)
                    {
                        foreach (DataRow drRSLTDT in dtRSLTDT.Rows)
                        {
                            /// 보정 DOCV 전송실패 팝업 사용여부 + 조회 날짜 범위
                            if (!string.IsNullOrEmpty(drRSLTDT["ATTR1"].ToString()) && !drRSLTDT["ATTR1"].Equals("NaN") && !drRSLTDT["ATTR1"].Equals(System.DBNull.Value)
                                 &&
                                 !string.IsNullOrEmpty(drRSLTDT["ATTR2"].ToString()) && !drRSLTDT["ATTR2"].Equals("NaN") && !drRSLTDT["ATTR2"].Equals(System.DBNull.Value))
                            {
                                if (drRSLTDT["ATTR1"].ToString().Trim().Equals("Y"))
                                {
                                    ChkFormFittedDOCVTrnfFail(drRSLTDT["ATTR2"].ToString().Trim());
                                }
                            }
                        }
                    }
                });
                #endregion
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 보정 dOCV 송/수신 실패 타이머 처리
        /// <summary>
        /// 보정 dOCV 송/수신 실패 타이머 처리
        /// </summary>
        private void GetFormFittedDOCVTrnfFailCheck()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _FormFittedDOCVTrnfFail = new Timer(60 * 10 * 1000);    // 10 Minute
                //_FormFittedDOCVTrnfFail = new Timer(10 * 1 * 1000);     // 테스트 할때 10초 셋팅후 진행
                _FormFittedDOCVTrnfFail.AutoReset = true;
                _FormFittedDOCVTrnfFail.Elapsed += (checkSenders, checkArg) =>
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    _FormFittedDOCVTrnfFail.Stop();
                    if ((bool)LoginInfo.CFG_FORM_FITTED_DOCV_TRNF_FAIL == true)
                    {
                        GetFormFittedDOCVTrnfFailPopup();
                    }
                };
                _FormFittedDOCVTrnfFail.Start();
            }));
        }
        #endregion

        #region 보정 dOCV 송/수신 실패 BizActor 호출
        /// <summary>
        /// 보정 dOCV 송/수신 실패 BizActor 호출
        /// </summary>
        private void ChkFormFittedDOCVTrnfFail(string strDayRange = "-15")
        {
            try
            {
                #region Defined DataTable and DataRow Variable
                /// <summary> DataTable </summary>
                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("DAY_RNG", typeof(Int32));

                /// <summary> DataRow </summary>
                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["DAY_RNG"] = Convert.ToInt32(strDayRange);
                dt.Rows.Add(dr);
                #endregion

                #region Excute Data Access
                new ClientProxy().ExecuteService("DA_SEL_LOT_FITTED_DOCV_TRNF_FAIL_LIST", "RQSTDT", "RSLTDT", dt, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        FCS001_999_FITTED_DOCV_TRNF_FAIL wndRunFittedDOCVTrnfFail = new FCS001_999_FITTED_DOCV_TRNF_FAIL();
                        wndRunFittedDOCVTrnfFail.FrameOperation = FrameOperation;
                        if (wndRunFittedDOCVTrnfFail != null)
                        {
                            object[] Parameters = new object[1];
                            Parameters[0] = result;

                            C1WindowExtension.SetParameters(wndRunFittedDOCVTrnfFail, Parameters);

                            wndRunFittedDOCVTrnfFail.Closed += new EventHandler(wndFormFittedDOCVTrnfFail_Closed);
                            wndRunFittedDOCVTrnfFail.ShowModal();
                        }
                    }
                });
                #endregion
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 팝업창 종료시 보정 dOCV 송/수신 실패 타이머 시작
        /// <summary>
        /// 팝업창 종료시 보정 dOCV 송/수신 실패 타이머 시작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wndFormFittedDOCVTrnfFail_Closed(object sender, EventArgs e)
        {
            _FormFittedDOCVTrnfFail.Start();
        }
        #endregion

        #region Restart timer for Formation popup check
        /// <summary>
        /// Restart timer for Formation popup check
        /// 기존에는 환경설정 진행후 재로그인 해야 하기때문에, 재로그인 없이 타이머 컨트롤 진행하기 위해 새롭게 추가됨
        /// </summary>
        public void SetFormCheckTimerRestart()
        {
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                Logger.Instance.WriteLine("[FRAME] SetFormCheckTimerRestart : Restart timer for Formation >> ", String.Format("{0}, CFG_FORM_FITTED_DOCV_TRNF_FAIL={1}", Logger.MESSAGE_OPERATION_START, LoginInfo.CFG_FORM_FITTED_DOCV_TRNF_FAIL), LogCategory.FRAME);
                GetFormFittedDOCVTrnfFailCheck();   //2024.06.27 / 권용섭(cnsyongsub) / [E20240620-001371] 보정 dOCV 전송실패시 팝업 알림 여부
                Logger.Instance.WriteLine("[FRAME] SetFormCheckTimerRestart : Restart timer for Formation >> ", String.Format("{0}, CFG_FORM_FITTED_DOCV_TRNF_FAIL={1}", Logger.MESSAGE_OPERATION_END, LoginInfo.CFG_FORM_FITTED_DOCV_TRNF_FAIL), LogCategory.FRAME);
            }
        }
        #endregion
        #endregion 2024.06.27 / 권용섭(cnsyongsub) / [E20240620-001371] 보정 dOCV 송/수신 실패시 팝업 알림

        private void EqpStatusCheck()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _EqpStatusTimer = new Timer(60 * 30 * 1000); //30분 주기
                _EqpStatusTimer.AutoReset = true;

                _EqpStatusTimer.Elapsed += (checkSenders, checkArg) =>
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    _EqpStatusTimer.Stop();
                    if(LoginInfo.CFG_EQP_STATUS) ChkEqpTrouble();
                    if(LoginInfo.CFG_W_LOT) GetWGradeRJudge();
                };
               _EqpStatusTimer.Start();
            }));
        }

        private void ChkEqpTrouble()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_SEL_EQP_TROUBLE_SEARCH", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        //POPUP
                        FCS001_027_TROUBLE wndTrouble = new FCS001_027_TROUBLE();
                        wndTrouble.FrameOperation = FrameOperation;

                        if (wndTrouble != null)
                        {

                            object[] Parameters = new object[1];
                            Parameters[0] = result;

                            C1WindowExtension.SetParameters(wndTrouble, Parameters);

                            wndTrouble.Closed += new EventHandler(wndTrouble_Closed);
                            wndTrouble.ShowModal();

                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void wndTrouble_Closed(object sender, EventArgs e)
        {
            _EqpStatusTimer.Start();
        }

        private void GetWGradeRJudge()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_SEL_WGRADE_RJUDGE_CHECK", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        //POPUP
                        FCS001_006_WLOTLIST wndWLot = new FCS001_006_WLOTLIST();
                        wndWLot.FrameOperation = FrameOperation;

                        if (wndWLot != null)
                        {
                            object[] Parameters = new object[1];
                            Parameters[0] = result;
                            C1WindowExtension.SetParameters(wndWLot, Parameters);
                            wndWLot.Closed += new EventHandler(wndWLot_Closed);
                            wndWLot.ShowModal();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void wndWLot_Closed(object sender, EventArgs e)
        {
          //  _EqpStatusTimer.Start();
        }

        //활성화 : Stocker & 충방전기 화재상태 체크 2022-08-04
        private void FcsEqpStatusCheck()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _FcsEqpStatusTimer = new Timer(60 * 1000); //1분 주기
                _FcsEqpStatusTimer.AutoReset = true;

                _FcsEqpStatusTimer.Elapsed += (checkSenders, checkArg) =>
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    _FcsEqpStatusTimer.Stop();
                    ChkFcsFireAlarm();
                };
                _FcsEqpStatusTimer.Start();
            }));
        }

        //활성화 : Stocker & 충방전기 화재상태 체크 2022-08-04
        private void ChkFcsFireAlarm()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dt.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_SEL_EQP_FIRE_SEARCH", "RQSTDT", "RSLTDT", dt, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        // 알람 팝업이 안뜨는 현상이 있어 백업 차원에서 메인화면 상단 깜빡임 추가 : 2023.06.15 - 조영대
                        ColorAnimation fireAni = new ColorAnimation();
                        fireAni.From = Color.FromRgb(71, 71, 81);
                        fireAni.To = Colors.Red;
                        fireAni.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                        fireAni.AutoReverse = true;
                        fireAni.RepeatBehavior = RepeatBehavior.Forever;
                        TopGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, fireAni);

                        //POPUP
                        FCS001_137_FCS_EQP_CHK wndFireAlarm = new FCS001_137_FCS_EQP_CHK();
                        wndFireAlarm.FrameOperation = FrameOperation;

                        if (wndFireAlarm != null)
                        {
                            object[] Parameters = new object[1];
                            Parameters[0] = result;

                            C1WindowExtension.SetParameters(wndFireAlarm, Parameters);

                            wndFireAlarm.Closed += new EventHandler(wndFireAlarm_Closed);
                            wndFireAlarm.ShowModal();                            
                        }
                    }
                    else
                    {
                        _FcsEqpStatusTimer.Start();
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //활성화 : Stocker & 충방전기 화재상태 체크 2022-08-04
        private void wndFireAlarm_Closed(object sender, EventArgs e)
        {
            TopGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);

            _FcsEqpStatusTimer.Start();
        }
        private void FDSCELLCheck()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _FormFDSCELLTimer = new Timer(60 * 1000); //1분 주기
                _FormFDSCELLTimer.AutoReset = true;

                _FormFDSCELLTimer.Elapsed += (checkSenders, checkArg) =>
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    _FormFDSCELLTimer.Stop();
                    if (LoginInfo.CFG_FORM_FDS_ALARM) ChkFDSCellAlarm();
                };
                _FormFDSCELLTimer.Start();
            }));

        }

        //활성화 : 충방 발열셀 체크 2022-08-04
        private void ChkFDSCellAlarm()
        {
            try
            {

                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dt.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_SEL_FDS_DFCT_MB", "RQSTDT", "RSLTDT", dt, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (result.Rows.Count > 0)
                    {
                        ColorAnimation fdsAni = new ColorAnimation();
                        fdsAni.From = Color.FromRgb(71, 71, 81);
                        fdsAni.To = Colors.Red;
                        fdsAni.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                        fdsAni.AutoReverse = true;
                        fdsAni.RepeatBehavior = RepeatBehavior.Forever;
                        TopGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, fdsAni);

                        //POPUP
                        FCS002_001_FDS_DFCT wndFDSAlarm = new FCS002_001_FDS_DFCT();
                        wndFDSAlarm.FrameOperation = FrameOperation;

                        if (wndFDSAlarm != null)
                        {
                            object[] Parameters = new object[1];
                            Parameters[0] = result;

                            C1WindowExtension.SetParameters(wndFDSAlarm, Parameters);

                            wndFDSAlarm.Closed += new EventHandler(wndFDSAlarm_Closed);
                            wndFDSAlarm.ShowModal();
                        }
                    }
                    else
                    {
                        _FormFDSCELLTimer.Start();
                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //활성화 : 충방전 발열셀 체크 2022-08-04
        private void wndFDSAlarm_Closed(object sender, EventArgs e)
        {
            TopGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);

            _FormFDSCELLTimer.Start();
        }
        private void SASAlarmCheck()
        {

            Dispatcher.BeginInvoke(new Action(() =>
            {
                _FormSASTimer = new Timer(60 * 1000); //1분 주기
                _FormSASTimer.AutoReset = true;

                _FormSASTimer.Elapsed += (checkSenders, checkArg) =>
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    _FormSASTimer.Stop();
                    if (LoginInfo.CFG_FORM_SAS_ALARM)  ChkSASAlarm();
                };
                _FormSASTimer.Start();
            }));

        }

        //활성화 : 충방 발열셀 체크 2022-08-04
        private void ChkSASAlarm()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("COM_TYPE_CODE", typeof(string));
                dt.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = dt.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORM_SITE_BASE_INFO";
                dr["COM_CODE"] = "SAS_ML_FITTED_CAPA";
                dt.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_SEL_SAS_ALARM_MB", "RQSTDT", "RSLTDT", dt, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        //POPUP
                        FCS002_005_SAS_ALARM wndRunStart = new FCS002_005_SAS_ALARM();
                        wndRunStart.FrameOperation = FrameOperation;

                        if (wndRunStart != null)
                        {
                            object[] Parameters = new object[1];
                            Parameters[0] = result;

                            C1WindowExtension.SetParameters(wndRunStart, Parameters);

                            wndRunStart.Closed += new EventHandler(wndSASAlarm_Closed);
                            wndRunStart.ShowModal();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //활성화 : 충방전 발열셀 체크 2022-08-04
        private void wndSASAlarm_Closed(object sender, EventArgs e)
        {
            TopGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);

            _FormSASTimer.Start();
        }

        //활성화 : 고온 Aging 온도 이탈 알람 체크용 Timer - 2024-11-01
        private void HighAgingAbnormTmprCheck()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _FormHighAgingAbnormTmprTimer = new Timer(60 * 1000); //1분 주기
                _FormHighAgingAbnormTmprTimer.AutoReset = true;

                _FormHighAgingAbnormTmprTimer.Elapsed += (checkSenders, checkArg) =>
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    _FormHighAgingAbnormTmprTimer.Stop();
                    if (LoginInfo.CFG_FORM_HIGH_AGING_ABNORM_TMPR_ALARM) ChkHighAgingAbnormTmprAlarm();
                };
                _FormHighAgingAbnormTmprTimer.Start();
            }));

        }

        //활성화 : 고온 Aging 온도 이탈 알람 체크용 Timer - 2024-11-01 > 2025.04.22 소형 리빌딩 적용
        private void ChkHighAgingAbnormTmprAlarm()
        {
            try
            {

                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("S70", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S70"] = "4";
                dt.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_SEL_HIGH_AGING_ABNORM_TMPR_ALARM_POPUP_MB", "RQSTDT", "RSLTDT", dt, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (result.Rows.Count > 0)
                    {
                        ColorAnimation fdsAni = new ColorAnimation();
                        fdsAni.From = Color.FromRgb(71, 71, 81);
                        fdsAni.To = Colors.Red;
                        fdsAni.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                        fdsAni.AutoReverse = true;
                        fdsAni.RepeatBehavior = RepeatBehavior.Forever;
                        TopGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, fdsAni);

                        //POPUP
                        FCS002_006_AGING_ABNORM_TMPR_ALARM wndHighAgingAbnormTmprAlarm = new FCS002_006_AGING_ABNORM_TMPR_ALARM();
                        wndHighAgingAbnormTmprAlarm.FrameOperation = FrameOperation;

                        if (wndHighAgingAbnormTmprAlarm != null)
                        {
                            object[] Parameters = new object[1];
                            Parameters[0] = result;

                            C1WindowExtension.SetParameters(wndHighAgingAbnormTmprAlarm, Parameters);

                            wndHighAgingAbnormTmprAlarm.Closed += new EventHandler(wndHighAgingAbnormTmprAlarm_Closed);
                            wndHighAgingAbnormTmprAlarm.ShowModal();
                        }
                    }
                    else
                    {
                        _FormHighAgingAbnormTmprTimer.Start();
                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //활성화 : 고온 Aging 온도 이탈 알람 체크용 Timer - 2024-11-01
        private void wndHighAgingAbnormTmprAlarm_Closed(object sender, EventArgs e)
        {
            TopGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);

            _FormHighAgingAbnormTmprTimer.Start();
        }


        //활성화 : 충방전기 작업중 Tray 시간경과 체크 2023-09-25
        private void FcsFormationTimeCheck()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {

               _FcsFormationTimer = new Timer(60 * 1000 * 30); //30분 주기
               // _FcsFormationTimer = new Timer( 1000*60 ); //1초 주기
                _FcsFormationTimer.AutoReset = true;

                _FcsFormationTimer.Elapsed += (checkSenders, checkArg) =>
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    _FcsFormationTimer.Stop();
                    ChkFcsFormationAlarm();
                };
                _FcsFormationTimer.Start();
            }));
        }

        //활성화 : 충방전기 작업중 Tray 시간경과 체크 2023-09-25
        private void ChkFcsFormationAlarm()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "PROGRAM_BY_FUNC_USE_FLAG";
                dr["COM_CODE"] = "FCS001_001_TIME_OVER";
                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                    foreach (DataRow row in dtResult.Rows)
                    if (string.Equals("Y", row["ATTR1"])) //20220502_오류수정
                    {

                    DataTable dt = new DataTable();
                    dt.TableName = "RQSTDT";
                    dt.Columns.Add("LANGID", typeof(string));
                    dt.Columns.Add("EQPT_ALARM_CODE", typeof(string));
                    dt.Columns.Add("WRKR_CHK_FLAG", typeof(string));
                    dt.Columns.Add("INSUSER", typeof(string));
                    dt.Columns.Add("UPDUSER", typeof(string));
   
          

                    DataRow dr1 = dt.NewRow();
                    dr1["LANGID"] = LoginInfo.LANGID;
                    dr1["EQPT_ALARM_CODE"] = "0001TO";
                    dr1["WRKR_CHK_FLAG"] = "N";
                    dr1["INSUSER"] = "SYSTEM";
                    dr1["UPDUSER"] = "SYSTEM";
                    dt.Rows.Add(dr1);

                    new ClientProxy().ExecuteService("BR_GET_TROUBLE_TIMEOVER", "RQSTDT", "RSLTDT", dt, (result, Exception) =>
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                        // 알람 팝업이 안뜨는 현상이 있어 백업 차원에서 메인화면 상단 깜빡임 추가 : 2023.06.15 - 조영대
                        ColorAnimation fireAni = new ColorAnimation();
                            fireAni.From = Color.FromRgb(71, 71, 81);
                            fireAni.To = Colors.Red;
                            fireAni.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                            fireAni.AutoReverse = true;
                            fireAni.RepeatBehavior = RepeatBehavior.Forever;
                            TopGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, fireAni);

                        //POPUP
                        FCS001_001_FORMATION_TIME_OVER wndFireAlarm = new FCS001_001_FORMATION_TIME_OVER();
                            wndFireAlarm.FrameOperation = FrameOperation;

                            if (wndFireAlarm != null)
                            {
                                object[] Parameters = new object[1];
                                Parameters[0] = result;

                                C1WindowExtension.SetParameters(wndFireAlarm, Parameters);

                                wndFireAlarm.Closed += new EventHandler(wndFormationAlarm_Closed);
                                wndFireAlarm.ShowModal();
                            }
                        }
                        else
                        {
                            _FcsFormationTimer.Start();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //활성화 : 충방전기 작업중 Tray 시간경과 체크 2023-09-25
        private void wndFormationAlarm_Closed(object sender, EventArgs e)
        {
            TopGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);

            _FcsFormationTimer.Start();
        }

        private void CheckNoticeBoard()
        {
            /*
            DateTime dtNowTime = System.DateTime.Now;
            System.TimeSpan timeDiff = dtNowTime - dtNoticeBoard;
            double diffTotalHours = timeDiff.TotalHours;
            //double diffTotalMiniute = timeDiff.TotalMinutes;

            if (diffTotalHours > 6)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    dtNoticeBoard = System.DateTime.Now;
                    CheckPopup("LOAD");
                }));
            }
            */
        }

        private void OpenNotice()
        {
            bool flag = false;
           

            foreach (C1Window popup in FindVisualChildren<C1Window>(this))
            {
                if (Convert.ToString(popup.GetType().Name) == "COM001_388")
                {
                    flag = true;
                    
                    break;
                }

            }

            if (!flag)
            {
                string MAINFORMPATH = "LGC.GMES.MES.COM001";
                string MAINFORMNAME = "COM001_388";
                Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
                Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
                object obj = Activator.CreateInstance(targetType);

                C1Window w = obj as C1Window;
                //string Parameter = mode;
                //C1WindowExtension.SetParameter(w, Parameter);
                w.Show();
            }
          
        }

       

        private void CheckPopup(string mode)
        {
            bool flag = false;

            foreach (C1Window popup in FindVisualChildren<C1Window>(this))
            {
                if (Convert.ToString(popup.GetType().Name) == "COM001_047")
                {
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                string MAINFORMPATH = "LGC.GMES.MES.COM001";
                string MAINFORMNAME = "COM001_047";
                Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
                Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
                object obj = Activator.CreateInstance(targetType);

                C1Window w = obj as C1Window;
                string Parameter = mode;
                C1WindowExtension.SetParameter(w, Parameter);
                w.Show();
            }
        }

        private void CheckUpdate()
        {
            string assemblyname = string.Empty;

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
            {
                return;
                //assemblyname = Convert.ToString(Assembly.GetEntryAssembly().GetName().Name);
            }
            else
            {
                assemblyname = "MainFrame";
            }

            string fullname = AppDomain.CurrentDomain.BaseDirectory.ToString().Substring(0, AppDomain.CurrentDomain.BaseDirectory.ToString().IndexOf(assemblyname) + (assemblyname).Length);
            fullname += "\\" + Convert.ToString(ConfigurationManager.AppSettings["UPDATEVERSIONFILE"]); // "\\LGC.GMES.UpdateVersion.config";
            FileInfo file = new FileInfo(fullname);

            if (file.Exists)
            {
                dsUpdateVersion = new DataSet();
                dsUpdateVersion.ReadXml(fullname, XmlReadMode.ReadSchema);
                AssemblyInformationalVersionAttribute[] attr = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false) as AssemblyInformationalVersionAttribute[];
                myVersion = attr[0].InformationalVersion;
                updateVersion = Convert.ToString(dsUpdateVersion.Tables["UPDATEVERSION"].Rows[0]["UPDATEVERSION"].ToString());

                if (myVersion != updateVersion && !CheckOnTheRun()) //특정 공정 화면에서는 업데이트 알림을 띄우지 않는다. (공정 BCR과 엔터키 충돌)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        bool flag = false;

                        foreach (C1Window popup in FindVisualChildren<C1Window>(this))
                            if (Convert.ToString(popup.Header) == "Update")
                                flag = true;

                        if (!flag && !isOpenedLineMonitoring)
                        {
                            (grdUpdate.Resources["UpdateEnabledAnimation"] as Storyboard).Begin();
                            //tcMainContentTabControl.IsEnabled = false;

                            if (isObjectDicLoaded)
                            {
                                //FRA0001 : 새로운 버전이 있습니다. 종료하시겠습니까?
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FRA0001"), null, "Update", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                        Close();
                                });
                            }
                        }
                    }));
                }
            }
            else
            {
                string strmessage = "File Not Found" + " : " + Convert.ToString(ConfigurationManager.AppSettings["UPDATEVERSIONFILE"]);
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(strmessage, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 특정 메뉴가 실행 중인지 체크한다
        /// </summary>
        /// <returns>실행 중이면 true, 실행 중이지 않으면 false</returns>
        bool CheckOnTheRun()
        {
            DataTable dtAvoidUpdate = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MENU_AVOID_UPDATE_ALERT", null, "OUTDATA", null);

            foreach (DataRow dr in dtAvoidUpdate.Rows)
            {
                foreach (C1TabItem tabitem in tcMainContentTabControl.Items)
                {
                    string menuId = DataTableConverter.GetValue(tabitem.DataContext, "MENUID").ToString();

                    if (menuId.Equals(dr["MENUID"] as string))
                        return true;
                }
            }

            return false;
        }

        private void beginAPCheck()
        {
            //_APtimer = new Timer(5000);
            //_APtimer.AutoReset = true;
            //_APtimer.Elapsed += (s, arg) =>
            //{
            //    using (TcpClient client = new TcpClient())
            //    {
            //        try
            //        {
            //            client.Connect(Common.Common.BizActorIP, int.Parse(Common.Common.BizActorPort));

            //            if (client.Connected)
            //            {
            //                Dispatcher.BeginInvoke(new Action(() =>
            //                {
            //                    imgBizCon.Visibility = Visibility.Visible;
            //                    imgBizDisCon.Visibility = Visibility.Collapsed;
            //                }));
            //            }
            //            else
            //            {
            //                Dispatcher.BeginInvoke(new Action(() =>
            //                {
            //                    Logger.Instance.WriteLine("[FRAME] BizActor connection checker", "Disconnected", LogCategory.FRAME);
            //                    imgBizCon.Visibility = Visibility.Collapsed;
            //                    imgBizDisCon.Visibility = Visibility.Visible;
            //                }));
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            Dispatcher.BeginInvoke(new Action(() =>
            //            {
            //                Logger.Instance.WriteLine("[FRAME] BizActor connection checker", ex, LogCategory.FRAME);
            //                imgBizCon.Visibility = Visibility.Collapsed;
            //                imgBizDisCon.Visibility = Visibility.Visible;
            //            }));
            //        }
            //    }
            //};
            //_APtimer.Start();
        }

        private void beginTimer()
        {
            _timer = new Timer(1000);
            _timer.AutoReset = true;

            _timer.Elapsed += (s, arg) =>
            {
                tblTime.Dispatcher.BeginInvoke(new Action(() => tblTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            };

            _timer.Start();
        }

        public void Get_TB_SOM_USER_SCRN_SET_MST(Action<Exception> ACTION_COMPLETED = null)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("USERID", typeof(string));
            dtRQSTDT.Columns.Add("SYSTEM_TYPE_CODE", typeof(string));
            //dtRQSTDT.Columns.Add("SHOPID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["USERID"] = LoginInfo.USERID;
            drnewrow["SYSTEM_TYPE_CODE"] = LoginInfo.SYSID + "_" + Common.Common.APP_System;
            //drnewrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SOM_USER_SCRN_SET_MST", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (result.Rows.Count > 0)
                {
                    LoginInfo.CFG_SHOP_ID = Convert.ToString(result.Rows[0]["SHOPID"]);
                    LoginInfo.CFG_AREA_ID = Convert.ToString(result.Rows[0]["AREAID"]);
                    LoginInfo.CFG_EQSG_ID = Convert.ToString(result.Rows[0]["EQSGID"]);
                    LoginInfo.CFG_PROC_ID = Convert.ToString(result.Rows[0]["PROCID"]);
                    LoginInfo.CFG_EQPT_ID = Convert.ToString(result.Rows[0]["EQPTID"]);

                    LoginInfo.CFG_SHOP_NAME = Convert.ToString(result.Rows[0]["SHOPNAME"]);
                    LoginInfo.CFG_AREA_NAME = Convert.ToString(result.Rows[0]["AREANAME"]);
                    LoginInfo.CFG_EQSG_NAME = Convert.ToString(result.Rows[0]["EQSGNAME"]);
                    LoginInfo.CFG_PROC_NAME = Convert.ToString(result.Rows[0]["PROCNAME"]);
                    LoginInfo.CFG_EQPT_NAME = Convert.ToString(result.Rows[0]["EQPTNAME"]);

                    if (Convert.ToString(result.Rows[0]["LANGID"]) == string.Empty)
                        LoginInfo.LANGID = "ko-KR";
                    else
                        LoginInfo.LANGID = Convert.ToString(result.Rows[0]["LANGID"]);

                    LoginInfo.CFG_INI_MENUID = Convert.ToString(result.Rows[0]["INI_MENUID"]);

                    tblLine.Text = LoginInfo.CFG_SHOP_NAME + " >> " + LoginInfo.CFG_AREA_NAME + " >> " + LoginInfo.CFG_EQSG_NAME;
                    tblLine.ToolTip = Convert.ToString(tblLine.Text);
                }
                else
                {
                    LoginInfo.CFG_SHOP_ID = string.Empty;
                    LoginInfo.CFG_AREA_ID = string.Empty;
                    LoginInfo.CFG_EQSG_ID = string.Empty;
                    LoginInfo.CFG_PROC_ID = string.Empty;
                    LoginInfo.CFG_EQPT_ID = string.Empty;
                    //LoginInfo.LANGID = "ko-KR";
                    LoginInfo.LANGID = string.IsNullOrEmpty(LoginInfo.LANGID) ? "ko-KR" : LoginInfo.LANGID;
                    LoginInfo.CFG_INI_MENUID = string.Empty;
                }

                //2020.11.13 - 활성화와 기존 조립동 구분
                Get_SYSTEM_TYPE_CODE();

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(LoginInfo.LANGID);
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(LoginInfo.LANGID);

                //DA_BAS_SEL_TB_SOM_USER_SCRN_SET_MST
                //DA_BAS_SEL_TB_MMD_USER_SHOP_AREA
                Get_TB_MMD_USER_SHOP_AREA((exception, dtrtn) =>
                {
                    IList<string> mmdauth = new List<string>();

                    foreach (DataRow dr in dtrtn.Rows)
                        mmdauth.Add(Convert.ToString(dr["AREAID"]));

                    if (mmdauth.Contains(LoginInfo.CFG_AREA_ID) == false)
                    {
                        LoginInfo.CFG_AREA_ID = string.Empty;
                        LoginInfo.CFG_EQSG_ID = string.Empty;
                        LoginInfo.CFG_PROC_ID = string.Empty;
                        LoginInfo.CFG_EQPT_ID = string.Empty;
                        LoginInfo.CFG_INI_MENUID = string.Empty;
                    }

                    if (ACTION_COMPLETED != null)
                    {
                        Exception actionException = null;
                        ACTION_COMPLETED(actionException);
                    }
                });
            });
        }

        private void Get_SYSTEM_TYPE_CODE()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("SYSTEM_ID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["SYSTEM_ID"] = LoginInfo.SYSID;
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SYSTEM_CHK", "RQSTDT", "RSLTDT", dt);

                if (dtResult.Rows.Count > 0)
                {
                    LoginInfo.CFG_SYSTEM_TYPE_CODE = dtResult.Rows[0]["SYSTEM_TYPE_CODE"].ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage("오류"), ex.Message, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void Get_TB_MMD_USER_SHOP_AREA(Action<Exception, DataTable> ACTION_COMPLETED = null)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("USERID", typeof(string));
            dtRQSTDT.Columns.Add("SHOPID", typeof(string));
            dtRQSTDT.Columns.Add("USE_FLAG", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["USERID"] = LoginInfo.USERID;
            drnewrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            drnewrow["USE_FLAG"] = "Y";
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_TB_MMD_USER_SHOP_AREA", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (ACTION_COMPLETED != null)
                {
                    Exception actionException = null;
                    ACTION_COMPLETED(actionException, result);
                }
            });
        }

        private void Debug_Mode_Display()
        {
            //DebugWindow debugWindow = new DebugWindow();
            //debugWindow.ShowModal();

            if (Common.Common.APP_MODE == "DEBUG")
            {
                string tmp = string.Format("SYS:{0} S:{1} A:{2} EQ:{3} P:{4} E:{5}", LoginInfo.SYSID
                                                                                   , LoginInfo.CFG_SHOP_ID
                                                                                   , LoginInfo.CFG_AREA_ID
                                                                                   , LoginInfo.CFG_EQSG_ID
                                                                                   , LoginInfo.CFG_PROC_ID
                                                                                   , LoginInfo.CFG_EQPT_ID);

                txtbConfig.Text = Convert.ToString(Common.LoginInfo.PARAM) + " " + tmp;
                txtbConfig.Visibility = Visibility.Visible;
            }
            else
            {
                txtbConfig.Visibility = Visibility.Collapsed;
            }
        }

        private DataTable createEmptySerialPrinterTable()
        {
            DataTable emptySerialPrinterTable = new DataTable();
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERKEY, typeof(string));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_ISACTIVE, typeof(bool));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_PARENTPRINTERKEY, typeof(string));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_LABELTYPE, typeof(string));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_PORTNAME, typeof(string));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_BAUDRATE, typeof(int));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_PARITYBIT, typeof(string));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_DATABIT, typeof(int));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_STOPBIT, typeof(string));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_X, typeof(int));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_Y, typeof(int));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES, typeof(int));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_CONNECTIONLESS, typeof(bool));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT, typeof(string));
            return emptySerialPrinterTable;
        }

        private void LogIn()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["APP_SERVER"]))
                {
                    BrushConverter bc = new BrushConverter();
                    Brush b = bc.ConvertFrom("#D80546") as Brush; //LG컬러(#D80546)
                    TopGrid.Background = b;
                }

                //bool isObjectDicLoaded = false;
                //bool isMessageDicLoaded = false;

                spHelp.Visibility = Visibility.Visible;
                Loaded -= new RoutedEventHandler(MainWindow_Loaded);

                System.Reflection.AssemblyInformationalVersionAttribute[] attr = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false) as AssemblyInformationalVersionAttribute[];
                tblVersion.Text = "Ver. " + attr[0].InformationalVersion;
                //tblVersion.Text = "Ver. " + Assembly.GetExecutingAssembly().GetName().Version.ToString();

                tblTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                Logger.Instance.WriteLine("[FRAME] Login", Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);
                beginTimer();
                //beginAPCheck();
                beginCheckUpdate();
                //beginBizLogTimer();

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Get_TB_SOM_USER_SCRN_SET_MST((exception) =>
                    {
                        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(LoginInfo.LANGID);
                        System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(LoginInfo.LANGID);

                        Debug_Mode_Display();

                        new ClientProxy().ExecuteService("COR_SEL_LANGUAGE", null, "RSLTDT", null, (result, ex) =>
                        {
                            if (ex != null)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            LoginInfo.SUPPORTEDLANGLIST = (from DataRow row in result.Rows select row["LANGID"]).Cast<string>().ToArray();
                            LoginInfo.SUPPORTEDLANGINFOLIST = DataTableConverter.Convert(result).Cast<object>().ToArray();

                            DataTable loginIndataTable = new DataTable();
                            loginIndataTable.Columns.Add("USERID", typeof(string));
                            loginIndataTable.Columns.Add("LANGID", typeof(string));
                            DataRow loginIndata = loginIndataTable.NewRow();
                            loginIndata["USERID"] = LoginInfo.USERID;
                            loginIndata["LANGID"] = LoginInfo.LANGID;
                            loginIndataTable.Rows.Add(loginIndata);

                            new ClientProxy().ExecuteService("DA_BAS_SEL_PERSON_TBL", "INDATA", "OUTDATA", loginIndataTable, (loginOutdata, loginException) =>
                            {
                                if (loginException != null)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(loginException.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                                /**
                                 * 보안진단팀 권고에 따라 본인 계정이 아닌 타 계정으로 로그인 할 수 있는 취약점이 발견되어 Windows로그인 계정과 불일치할 경우 로그인 불가하도록 추가함
                                 * - Added by Jaeyoung Ko (2023.03.16)
                                 * CSR ID : E20230316-000229
                                 * */
                                System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                                // 실행환경이 Visual Studio가 아니거나 로그인 사용자 ID가 OS 로그인 ID와 다를경우 
                                if (!System.Diagnostics.Process.GetCurrentProcess().ProcessName.ToLower().Contains("vshost") && !identity.Name.Contains(LoginInfo.USERID))
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show("Unusual approach.", null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (msgresult) =>
                                    {
                                        Close();
                                    });
                                }

                                if (loginOutdata.Rows.Count == 0)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show("User ID does not exist.", null, "Information", MessageBoxButton.OK, MessageBoxIcon.Warning, (msgresult) =>
                                    {
                                        Close();
                                    });
                                }
                                else
                                {
                                    DataRow userInfo = loginOutdata.Rows[0];
                                    LoginInfo.USGRID = userInfo["USGRID"] != null ? userInfo["USGRID"].ToString() : string.Empty;
                                    LoginInfo.USERID = userInfo["USERID"] != null ? userInfo["USERID"].ToString() : string.Empty;
                                    LoginInfo.USERNAME = userInfo["USERNAME"] != null ? userInfo["USERNAME"].ToString() : string.Empty;
                                    LoginInfo.AUTHID = userInfo["AUTHID"] != null ? userInfo["AUTHID"].ToString() : string.Empty;
                                    LoginInfo.USERMAIL = userInfo["USERMAIL"] != null ? userInfo["USERMAIL"].ToString() : string.Empty;
                                    tblUserName.Text = LoginInfo.USERNAME;

                                    // System IDEA 적용(GPortal ID의 경우만 System Idea 호출)
                                    LoginInfo.USERTYPE = userInfo["USERTYPE"] != null ? userInfo["USERTYPE"].ToString() : string.Empty;
                                    if (string.Equals(LoginInfo.USERTYPE, "G"))
                                        //2020.12.07
                                        //tblSystemIdea.Visibility = Visibility.Visible;
                                        tblSystemIdea.Visibility = Visibility.Collapsed;
                                    else
                                        tblSystemIdea.Visibility = Visibility.Collapsed;

                                    // 하도급법 대응 (하도급 작업자 유형이 null일 때만 GMES 로고 조회가 보이게 변경)
                                    LoginInfo.PNTR_WRKR_TYPE = userInfo["PNTR_WRKR_TYPE"] != null ? userInfo["PNTR_WRKR_TYPE"].ToString() : string.Empty;
                                    if (!string.IsNullOrEmpty(LoginInfo.PNTR_WRKR_TYPE))
                                    {
                                        imgLogoGMES.Visibility = Visibility.Collapsed;
                                    }

                                    DataTable dtUserType = new ClientProxy().ExecuteServiceSync("CUS_SEL_PERSONTYPE_USERID", "INDATA", "OUTDATA", loginIndataTable);

                                    if (dtUserType.Rows.Count > 0)
                                    {
                                        LoginInfo.LOGGEDBYSSO = dtUserType.Rows[0]["USERTYPE"].ToString().Equals("G") ? true : false;
                                        LoginInfo.USERTYPE = dtUserType.Rows[0]["USERTYPE"].ToString();
                                    }
                                    else
                                    {
                                        LoginInfo.LOGGEDBYSSO = false;
                                    }

                                    // PC명, IP 정보 저장 2022.12.14
                                    LoginInfo.PC_NAME = System.Net.Dns.GetHostName();
                                    System.Net.IPHostEntry host = System.Net.Dns.GetHostEntry(LoginInfo.PC_NAME);
                                    if (host != null)
                                    {
                                        foreach (System.Net.IPAddress ip in host.AddressList)
                                        {
                                            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                            {
                                                LoginInfo.USER_IP = ip?.ToString();
                                                break;
                                            }
                                        }
                                    }

                                    Logger.Instance.WriteLine("[FRAME] Login", Logger.MESSAGE_OPERATION_END, LogCategory.FRAME);

                                    //권한유무 체크 (2020 - 09 - 22)
                                    //COM001_388에 동일 로직 존재, 추후 택일하여 정리해야함 (23-12-11)
                                    Logger.Instance.WriteLine("[FRAME] Initial Authority", Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);

                                    DataTable loginAuthTable = new DataTable();
                                    loginAuthTable.Columns.Add("USERID", typeof(string));
                                    DataRow authIndata = loginAuthTable.NewRow();
                                    authIndata["USERID"] = LoginInfo.USERID;
                                    loginAuthTable.Rows.Add(authIndata);

                                    DataTable dtAuthority = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_USER_AREA_AUTH_CONN", "INDATA", "OUTDATA", loginAuthTable);

                                    isInitAuthority = false;
                                    if (dtAuthority == null || dtAuthority.Rows.Count == 0)
                                        isInitAuthority = true;

                                    if (isInitAuthority == true)
                                    {
                                        DataTable dtComCode = new ClientProxy().ExecuteServiceSync("CUS_SEL_LANGID", null, "OUTDATA", null);

                                        if (dtComCode != null && dtComCode.Rows.Count > 0 && !string.IsNullOrEmpty(Convert.ToString(dtComCode.Rows[0]["LANGID"])))
                                            LoginInfo.LANGID = Convert.ToString(dtComCode.Rows[0]["LANGID"]);
                                    }

                                    Logger.Instance.WriteLine("[FRAME] Initial Authority", Logger.MESSAGE_OPERATION_END, LogCategory.FRAME);

                                    //CreateMenu();

                                    Logger.Instance.WriteLine("[FRAME] Object Dic", Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);
                                    DataTable objectDicIndataTable = new DataTable();
                                    objectDicIndataTable.Columns.Add("LANGID", typeof(string));
                                    objectDicIndataTable.Columns.Add("OBJECTIUSE", typeof(string));
                                    DataRow objectDicIndata = objectDicIndataTable.NewRow();
                                    objectDicIndata["LANGID"] = LoginInfo.LANGID;
                                    objectDicIndata["OBJECTIUSE"] = "Y";
                                    objectDicIndataTable.Rows.Add(objectDicIndata);

                                    new ClientProxy().ExecuteService("CUS_SEL_OBJECTDIC_BAS", "INDATA", "OUTDATA", objectDicIndataTable, (objectDicResult, objectDicException) =>
                                    {
                                        if (objectDicException != null)
                                        {
                                            ControlsLibrary.MessageBox.Show(objectDicException.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                            return;
                                        }

                                        ObjectDic.Instance.SetObjectDicData(objectDicResult);
                                        isObjectDicLoaded = true;
                                        GetLineInfo();

                                        // MAINFRAME 별도 다국어 처리 [2018-01-04]
                                        tblCharger.Text = ObjectDic.Instance.GetObjectName("비상연락망");

                                        // LOGIN시 공지BOARD 띄우지 않도록 변경, Setting값 추가 연동으로 변경 [2018-01-19]
                                        if (LoginInfo.CFG_ETC != null && LoginInfo.CFG_ETC.Rows.Count > 0 && LoginInfo.CFG_ETC.Columns["NOTICE"] != null)
                                            if (Convert.ToBoolean(LoginInfo.CFG_ETC.Rows[0][CustomConfig.CONFIGTABLE_ETC_NOTICEUSE]) == true)
                                                CheckPopup("LOAD");

                                        Logger.Instance.WriteLine("[FRAME] Object Dic", Logger.MESSAGE_OPERATION_END);

                                        //다국어 정상적용을 위하여 ObjectDic 세팅 후 호출로 변경[2020.03.20 김대근]
                                        CreateMenu();

                                        //// 초기유저 권한 신청 (2020-09-22)
                                        //if (isInitAuthority == true)
                                        //{
                                        //    OpenDumyMenu("COM001_USER_AUTH", "Request for Permission", "LGC.GMES.MES.COM001");
                                        //}

                                        OpenNotice();

                                        //// [E20231011-000895] GMES Main 화면 배포이력 표기
                                        //if (CheckOnGMESMainStartUp()) OpenGMESMainStartUp();

                                    }, null, false, true);

                                    Logger.Instance.WriteLine("[FRAME] Message Dic", Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);
                                    DataTable messageDicIndataTable = new DataTable();
                                    messageDicIndataTable.Columns.Add("LANGID", typeof(string));
                                    messageDicIndataTable.Columns.Add("MSGIUSE", typeof(string));
                                    DataRow messageDicIndata = messageDicIndataTable.NewRow();
                                    messageDicIndata["LANGID"] = LoginInfo.LANGID;
                                    messageDicIndata["MSGIUSE"] = "Y";
                                    messageDicIndataTable.Rows.Add(messageDicIndata);

                                    new ClientProxy().ExecuteService("CUS_SEL_MESSAGE_BAS", "INDATA", "OUTDATA", messageDicIndataTable, (messageDicResult, messageDicException) =>
                                    {
                                        if (messageDicException != null)
                                        {
                                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(messageDicException.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                            return;
                                        }

                                        MessageDic.Instance.SetMessageDicData(messageDicResult);
                                        isMessageDicLoaded = true;
                                        Logger.Instance.WriteLine("[FRAME] Message Dic", Logger.MESSAGE_OPERATION_END, LogCategory.FRAME);

                                        // LOGIN 접속자 체크 [2018-06-12]
                                        DataTable PersonLongTermIndataTable = new DataTable();
                                        PersonLongTermIndataTable.Columns.Add("USERID", typeof(string));
                                        DataRow PersonLongTermIndata = PersonLongTermIndataTable.NewRow();
                                        PersonLongTermIndata["USERID"] = LoginInfo.USERID;
                                        PersonLongTermIndataTable.Rows.Add(PersonLongTermIndata);

                                        Logger.Instance.WriteLine("[FRAME] Connect User Valid Check", Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);

                                        new ClientProxy().ExecuteService("DA_BAS_SEL_PERSON_LONG_TERM_CHK", "INDATA", "OUTDATA", PersonLongTermIndataTable, (PersonLongTermResult, PersonLongTermException) =>
                                        {
                                            bool isConnLongTerm = true;
                                            if (PersonLongTermException != null)
                                            {
                                                isConnLongTerm = false;
                                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(PersonLongTermException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                                return;
                                            }

                                            if (PersonLongTermResult == null || PersonLongTermResult.Rows.Count == 0)
                                            {
                                                isConnLongTerm = false;
                                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FRA0007"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (msgresult) =>
                                                {
                                                    Close();
                                                });
                                            }

                                            if (PersonLongTermResult != null && string.Equals(PersonLongTermResult.Rows[0]["LONG_TERM_NON_CONN_FLAG"], "Y"))
                                            {
                                                isConnLongTerm = false;
                                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FRA0008"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (msgresult) =>
                                                {
                                                    Close();
                                                });
                                            }

                                            Logger.Instance.WriteLine("[FRAME] Connect User Valid Pass", Logger.MESSAGE_OPERATION_END);

                                            // 접속 이력 추가
                                            if (isConnLongTerm == true)
                                            {
                                                Logger.Instance.WriteLine("[FRAME] Connect Time Update", Logger.MESSAGE_OPERATION_START);

                                                DataTable PersonConnIndataTable = new DataTable();
                                                PersonConnIndataTable.Columns.Add("USERID", typeof(string));
                                                DataRow PersonConnIndata = PersonConnIndataTable.NewRow();
                                                PersonConnIndata["USERID"] = LoginInfo.USERID;
                                                PersonConnIndataTable.Rows.Add(PersonConnIndata);

                                                new ClientProxy().ExecuteServiceSync("DA_BAS_UPD_PERSON_CONN_DTTM", "INDATA", null, PersonConnIndataTable);

                                                Logger.Instance.WriteLine("[FRAME] Connect Time Update Success", Logger.MESSAGE_OPERATION_END);
                                            }
                                        });
                                    }, null, false, true);

                                    if (isInitAuthority == true) // 2020-09-22
                                    {
                                        btnAllMenu.Visibility = Visibility.Collapsed;
                                        tblSetting.Visibility = Visibility.Collapsed;
                                        grdNotice.Visibility = Visibility.Collapsed;
                                        grdFavorite.Visibility = Visibility.Collapsed;
                                        grdPrint.Visibility = Visibility.Collapsed;
                                        return;
                                    }

                                    if (LoginInfo.CFG_AREA_ID == string.Empty)
                                    {
                                        ConfigWindow configWindow = new ConfigWindow();
                                        configWindow.Closed += new EventHandler(configWindow_Closed);
                                        configWindow.ShowModal();
                                    }
                                    else
                                    {
                                        //활성화는 라인,공정 선택하지 않음. - 2020.11.13 
                                        DataTable dt = new DataTable();
                                        dt.TableName = "RQSTDT";
                                        dt.Columns.Add("SYSTEM_ID", typeof(string));

                                        DataRow dr = dt.NewRow();
                                        dr["SYSTEM_ID"] = LoginInfo.SYSID;
                                        dt.Rows.Add(dr);

                                        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SYSTEM_CHK", "RQSTDT", "RSLTDT", dt);

                                        if (dtResult.Rows.Count > 0)
                                        {
                                            LoginInfo.CFG_SYSTEM_TYPE_CODE = dtResult.Rows[0]["SYSTEM_TYPE_CODE"].ToString();

                                            //활성화는 라인,공정 선택하지 않음. - 2020.11.13 
                                            if (dtResult.Rows[0]["SYSTEM_TYPE_CODE"].ToString().Equals("F"))
                                            {
                                                if (Check_Mandatory_String(new List<string> { LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID }) == false)
                                                {
                                                    ConfigWindow configWindow = new ConfigWindow();
                                                    configWindow.Closed += new EventHandler(configWindow_Closed);
                                                    configWindow.ShowModal();
                                                }
                                            }
                                            else
                                            {
                                                if (Check_Mandatory_String(new List<string> { LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, LoginInfo.CFG_EQSG_ID, LoginInfo.CFG_PROC_ID }) == false)
                                                {
                                                    ConfigWindow configWindow = new ConfigWindow();
                                                    configWindow.Closed += new EventHandler(configWindow_Closed);
                                                    configWindow.ShowModal();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (Check_Mandatory_String(new List<string> { LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, LoginInfo.CFG_EQSG_ID, LoginInfo.CFG_PROC_ID }) == false)
                                            {
                                                ConfigWindow configWindow = new ConfigWindow();
                                                configWindow.Closed += new EventHandler(configWindow_Closed);
                                                configWindow.ShowModal();
                                            }
                                        }


                                        //if (Check_Mandatory_String(new List<string> { LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, LoginInfo.CFG_EQSG_ID, LoginInfo.CFG_PROC_ID }) == false)
                                        //{
                                        //    ConfigWindow configWindow = new ConfigWindow();
                                        //    configWindow.Closed += new EventHandler(configWindow_Closed);
                                        //    configWindow.ShowModal();
                                        //}

                                        //if (LoginInfo.CFG_AREA_ID.Substring(0, 1) == "P")
                                        //{
                                        //    //Pack 공장은 설비 등록이 필수 아님
                                        //    if (Check_Mandatory_String(new List<string> { LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, LoginInfo.CFG_EQSG_ID, LoginInfo.CFG_PROC_ID }) == false)
                                        //    {
                                        //        ConfigWindow configWindow = new ConfigWindow();
                                        //        configWindow.Closed += new EventHandler(configWindow_Closed);
                                        //        configWindow.ShowModal();
                                        //    }
                                        //}
                                        //else
                                        //{

                                        //    if (Check_Mandatory_String(new List<string> { LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, LoginInfo.CFG_EQSG_ID, LoginInfo.CFG_PROC_ID, LoginInfo.CFG_EQPT_ID }) == false)
                                        //    {
                                        //        ConfigWindow configWindow = new ConfigWindow();
                                        //        configWindow.Closed += new EventHandler(configWindow_Closed);
                                        //        configWindow.ShowModal();
                                        //    }
                                        //}
                                    }
                                }
                            }, null, true);
                        });
                    });
                }));

                Closed += (closeSender, closeArg) =>
                {
                    foreach (UIElement element in spPrinter.Children)
                        if (element is IDisposable)
                            (element as IDisposable).Dispose();

                    PrinterManager.Instance.Clear();
                };

                CustomConfig.Instance.Reload(ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_NAME"]);

                tcMainContentTabControl.TabItemClosing += tcMainContentTabControl_TabItemClosing;
                Logger.Instance.WriteLine("[FRAME] MainWindow Loaded", Logger.MESSAGE_OPERATION_END, LogCategory.FRAME);

                //활성화 : Aging 한계시간 초과 현황 팝업 체크용 Timer - 2020-12-28
                Get_SYSTEM_TYPE_CODE();
                if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
                {
                    formProcWaitLimitTimeOverCheck(); //활성화 : 공정 대기 시간 초과 현황 팝업 체크용 Timer - 2022-09-27
                    formAgingLimitTimeOverCheck(); // 활성화 : Aging 제한 시간 초과 현황 체크용
                    EqpStatusCheck(); //활성화 : 설비 상태 체크용  2021-03-07
                    FcsEqpStatusCheck(); //활성화 : Stocker & 충방전기 화재상태 체크 2022-08-04
                    FDSCELLCheck(); //활성화 : 충방전기 FDS 발열셀 체크 2024-08-20
                    SASAlarmCheck(); //활성화 : SAS 송수신 오류 체크 2024-08-26
                    FcsFormationTimeCheck(); //활성화 : 충방전기 작업중 Tray 시간경과 체크 2023-09-25
                    formAgingOutputTimeOverCheck();// 2023.12.17 출하 Aging 후단출고 기준시간 초과 현황 추가
                    GetFormFittedDOCVTrnfFailCheck();   //2024.06.27 / 권용섭(cnsyongsub) / [E20240620-001371] 보정 dOCV 전송실패시 팝업 알림 여부
                    HighAgingAbnormTmprCheck(); //활성화 : 고온 Aging 온도 이탈 알람 체크용 Timer - 2024-11-01
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage("오류"), ex.Message, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetLineInfo()
        {
            Get_TB_SOM_USER_SCRN_SET_MST();
            tblLine.Text = LoginInfo.CFG_SHOP_NAME + " >> " + LoginInfo.CFG_AREA_NAME + " >> " + LoginInfo.CFG_EQSG_NAME;
            tblLine.ToolTip = Convert.ToString(tblLine.Text);
        }

        public bool Check_Mandatory_String(List<string> contList)
        {
            foreach (var item in contList)
            {
                if (item == string.Empty)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("필수입력항목을 모두 입력하십시오."), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            return true;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                    if (child != null && child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }
        #endregion
        private void tblSystemIdea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            bool flag = false;

            foreach (C1Window popup in FindVisualChildren<C1Window>(this))
            {
                if (Convert.ToString(popup.GetType().Name) == "COM001_WEB")
                {
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                string MAINFORMPATH = "LGC.GMES.MES.COM001";
                string MAINFORMNAME = "COM001_WEB";
                Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
                Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
                object obj = Activator.CreateInstance(targetType);

                C1Window w = obj as C1Window;
                C1WindowExtension.SetParameter(w, null);
            }
        }

        private void tblCharger_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (eContact != null)
                return;

            eContact = new EmergencyContact();

            eContact.Closed += (s, arg) =>
            {
                eContact = null;
            };

            Dispatcher.BeginInvoke(new Action(() => eContact.ShowModal()));
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            if (configWindow != null)
                configWindow.BringToFront();

            if (eContact != null)
                eContact.BringToFront();
        }

        #region [E20231011-000895] GMES Main 화면 배포이력 표기
        string _MenuID_GMESMain = string.Empty;
        bool CheckOnGMESMainStartUp()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("FORMID", typeof(string));
            RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["FORMID"] = "COM001_MAIN";
            dr["SYSTEM_ID"] = LoginInfo.SYSID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MENU_BY_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                _MenuID_GMESMain = dtResult.Rows[0]["MENUID"].ToString();
                return true;
            }

            return false;
        }

        private void OpenGMESMainStartUp()
        {
            //bool flag = CheckOnGMESMainStartUp();

            //if (flag)
            //{
                string MAINFORMPATH = "LGC.GMES.MES.COM001";
                string MAINFORMNAME = "COM001_MAIN_StartUp";
                Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
                Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
                object obj = Activator.CreateInstance(targetType);

                C1Window w = obj as C1Window;
                //string Parameter = mode;
                //C1WindowExtension.SetParameter(w, Parameter);
                w.Show();
            //}
        }

        private void OpenGMESMain()
        {
            if (string.IsNullOrEmpty(_MenuID_GMESMain)) CheckOnGMESMainStartUp();

            if (string.IsNullOrEmpty(_MenuID_GMESMain) == false)
            {
                object[] parameters = new object[2];
                parameters[0] = string.Empty;
                parameters[1] = string.Empty;
                OpenMenu(_MenuID_GMESMain, true, parameters);
            }
        }

        #endregion  [E20231011-000895] GMES Main 화면 배포이력 표기

        #region [NFF 자재출하 물류 RTD전환 구축] FORMID로 MENUID 조회
        string GetMENUIDfromFORMID(string formid)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("FORMID", typeof(string));
            RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["FORMID"] = formid;
            dr["SYSTEM_ID"] = LoginInfo.SYSID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;

            RQSTDT.Rows.Add(dr);
            
            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MENU_BY_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                return dtResult.Rows[0]["MENUID"].ToString();
            }

            return null;
        }
        #endregion
    }
}