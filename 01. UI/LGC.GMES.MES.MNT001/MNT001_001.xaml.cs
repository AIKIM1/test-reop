/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.MNT001
{
    public partial class MNT001_001 : Window
    {
        #region Declaration & Constructor 
        
        private MNT001_001_CMI1 window_CMI_1 = new MNT001_001_CMI1();

        private Storyboard sbExpandLeft;
        private Storyboard sbCollapseLeft;

        private static string selectedShop = string.Empty;
        private static string selectedArea = string.Empty;
        private static string selectedEquipmentSegment = string.Empty;
        private static string selectedEquipment = string.Empty;

        private static string selectedDisplayName = string.Empty;
        private static int selectedDisplayTime = 1;
        private static int selectedDisplayTimeSub = 10;

        private static DateTime dtMainStartTime;
        private static DateTime dtSubStartTime;
        
        private Timer _timer;
        private Timer _timer2;

        public MNT001_001()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            Initialize();

            
        }


        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
            if (_timer2 != null)
            {
                _timer2.Stop();
                _timer2.Dispose();
                _timer2 = null;
            }
        }


        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        private void Initialize()
        {
            //Storyboard =======================================================================================
            sbExpandLeft = (Storyboard)this.Resources["ExpandLeftFrameStoryboard"];
            sbCollapseLeft = (Storyboard)this.Resources["CollapseLeftFrameStoryboard"];

            //Clear_Display_Control();

            DataSet ds = LGC.GMES.MES.MNT001.MNT_Common.GetConfigXML();

            selectedShop = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["SHOP"]);
            selectedArea = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["AREA"]);
            selectedEquipmentSegment = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["EQUIPMENTSEGMENT"]);
            selectedEquipment = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["EQUIPMENT"]);

            selectedDisplayName = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["DISPLAYNAME"]);
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

            txtScreenName.Text = selectedDisplayName;
            numRefresh.Value = selectedDisplayTime;

            DateTime dt = DateTime.Now;
            tbdate.Text = dt.ToString("yyyy.MM.dd hh:mm:ss");

            //testDATA();

            realData();

            beginTimer();

            beginTimer1();

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
        }
        
        

        private void beginTimer()
        {
            _timer = new Timer(1000);
            _timer.AutoReset = true;
            _timer.Elapsed += (s, arg) =>
            {
                tbdate.Dispatcher.BeginInvoke(new Action(() => tbdate.Text = DateTime.Now.ToString("yyyy.MM.dd hh:mm:ss")
                                                                                                                                    //ObjectDic.Instance.GetObjectName(DateTime.Now.ToString("yyyy" + ObjectDic.Instance.GetObjectName("년") + 
                                                                                                                                    //                                 " MM" + ObjectDic.Instance.GetObjectName("월") + 
                                                                                                                                    //                                 " dd" + ObjectDic.Instance.GetObjectName("일") + 
                                                                                                                                    //                                 " hh" + ObjectDic.Instance.GetObjectName("시") + 
                                                                                                                                    //                                 " mm" + ObjectDic.Instance.GetObjectName("분") + 
                                                                                                                                    //                                 " ss" + ObjectDic.Instance.GetObjectName("초")
                                                                                                              //                      )
                                                                                                              //)
                                                        )
                                                );
            };
            _timer.Start();
        }

        private void beginTimer1()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if(_timer2 != null)
                {
                    _timer2.Stop();
                    _timer2 = null;
                }

                _timer2 = new Timer(selectedDisplayTime * 1000);
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

        #region Event
        

        
        private void btnSetSave_Click(object sender, RoutedEventArgs e)
        {
            selectedShop = Convert.ToString(cboShop.SelectedValue);
            selectedArea = Convert.ToString(cboArea.SelectedValue);
            selectedEquipmentSegment = Convert.ToString(cboEquipmentSegment.SelectedValue);
            selectedEquipment = "";

            selectedDisplayName = Convert.ToString(txtScreenName.Text);
            selectedDisplayTime = Convert.ToInt32(numRefresh.Value);

            DataSet ds = new DataSet();

            DataTable dt = new DataTable("MNT_CONFIG");
            dt.Columns.Add("SHOP", typeof(string));
            dt.Columns.Add("AREA", typeof(string));
            dt.Columns.Add("EQUIPMENTSEGMENT", typeof(string));
            dt.Columns.Add("EQUIPMENT", typeof(string));
            dt.Columns.Add("DISPLAYNAME", typeof(string));
            dt.Columns.Add("DISPLAYTIME", typeof(int));
            dt.Columns.Add("DISPLAYTIMESUB", typeof(int));

            DataRow dr = dt.NewRow();
            dr["SHOP"] = selectedShop;
            dr["AREA"] = selectedArea;
            dr["EQUIPMENTSEGMENT"] = selectedEquipmentSegment;
            dr["EQUIPMENT"] = selectedEquipment;
            dr["DISPLAYNAME"] = selectedDisplayName;
            dr["DISPLAYTIME"] = selectedDisplayTime;
            dr["DISPLAYTIMESUB"] = selectedDisplayTimeSub;

            dt.Rows.Add(dr);
            ds.Tables.Add(dt);

            LGC.GMES.MES.MNT001.MNT_Common.SetConfigXML(ds);

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
            beginTimer1();
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

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboEquipmentSegment.SelectedIndex > -1)
                {
                    selectedEquipmentSegment = Convert.ToString(cboEquipmentSegment.SelectedValue);
                    //Set_Combo_Equipment(cboEquipment);
                }
                else
                {
                    selectedArea = string.Empty;
                }

                
            }));
        }

        private void cboEquipment_SelectionChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                System.Diagnostics.Debug.WriteLine("");
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
                        cboEquipmentSegment.SelectedValue = selectedEquipmentSegment;
                        cboShop.SelectedItemChanged -= cboShop_SelectedItemChanged;
                        cboShop.SelectedItemChanged += cboShop_SelectedItemChanged;
                        cboArea.SelectedItemChanged -= cboArea_SelectedItemChanged;
                        cboArea.SelectedItemChanged += cboArea_SelectedItemChanged;
                        cboEquipmentSegment.SelectedItemChanged -= cboEquipmentSegment_SelectedItemChanged;
                        cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;

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
            dtRQSTDT.Columns.Add("USE_FLAG", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["SYSTEM_ID"] = LoginInfo.SYSID;
            drnewrow["USERID"] = LoginInfo.USERID;
            drnewrow["USE_FLAG"] = "Y";
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_PRD_SEL_SHOP_FOR_VD_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if (result.Rows.Count == 0)
                    {
                        selectedShop = string.Empty;
                    }
                    else
                    {
                        cbo.SelectedValue = LoginInfo.CFG_SHOP_ID;
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
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("SHOPID", typeof(string));
            dtRQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["SHOPID"] = Convert.ToString(cboShop.SelectedValue);
            drnewrow["AREA_TYPE_CODE"] = Area_Type.PACK;
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_AREA_BY_AREATYPE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if (result.Rows.Count == 0)
                    {
                        selectedArea = string.Empty;
                    }
                    else
                    {
                        cbo.SelectedValue = LoginInfo.CFG_AREA_ID;
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

        private void Set_Combo_EquipmentSegment(C1ComboBox cbo, Action<Exception> action = null)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["AREAID"] = Convert.ToString(cboArea.SelectedValue);
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if (result.Rows.Count == 0)
                    {
                        selectedEquipmentSegment = string.Empty;
                    }
                    else
                    {
                        cbo.SelectedValue = LoginInfo.CFG_EQSG_ID;
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

        private void Set_Combo_Equipment(MultiSelectionBox cbo, Action<Exception> action = null)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("SHOPID", typeof(string));
            dtRQSTDT.Columns.Add("AREAID", typeof(string));
            dtRQSTDT.Columns.Add("EQSGID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["SHOPID"] = selectedShop;
            drnewrow["AREAID"] = selectedArea;
            drnewrow["EQSGID"] = selectedEquipmentSegment;
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_PRD_SEL_EQPT_BY_EQSG_FOR_VD_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if (result.Rows.Count ==0)
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
        
        private void setMonitoringInfo(string sEqsgid)
        {
            try
            {
                gdLineMonitoring.Children.Clear();
                switch (sEqsgid)
                {
                    case "P5Q01":
                        window_CMI_1.MNT001_001 = this;
                        gdLineMonitoring.Children.Add(window_CMI_1);
                        break;
                    default:
                        //window_CMI_1.MNT001_001 = this;
                        //gdLineMonitoring.Children.Add(window_CMI_1);
                        break;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        //BR_PRD_SEL_MONITORING_CMI
        private void realData()
        {

            DataSet ds = new DataSet();
            DataTable IndataTable = ds.Tables.Add("INDATA");
            //DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("SRCTYPE", typeof(string));
            IndataTable.Columns.Add("SHOPID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("EQSGID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["SRCTYPE"] = LoginInfo.LANGID;
            Indata["SHOPID"] = Util.NVC(cboShop.SelectedValue);
            Indata["AREAID"] = Util.NVC(cboArea.SelectedValue);
            Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
            IndataTable.Rows.Add(Indata);

            //loadingIndicator.Visibility = Visibility.Visible;

            new ClientProxy().ExecuteService_Multi("BR_PRD_SEL_MONITORING_CMI", "INDATA", "CALLMGR_RSLTDT,QTY_RSLTDT,RTY_RSLTDT,YIELD_RSLTDT", (dsRslt, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.AlertByBiz("BR_PRD_SEL_MONITORING_CMI", bizException.Message, bizException.ToString());
                        //loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }
                    setMonitoringInfo(Util.NVC(cboEquipmentSegment.SelectedValue));
                    
                    switch (Util.NVC(cboEquipmentSegment.SelectedValue))
                    {
                        case "P5Q01":
                            window_CMI_1.setLineMapInfo(dsRslt);
                            window_CMI_1.setQty(dsRslt);
                            break;
                        default:
                            //window_CMI_1.setLineMapInfo(dsRslt);
                            //window_CMI_1.setQty(dsRslt);
                            break;
                    }

                    //loadingIndicator.Visibility = Visibility.Collapsed;

                }
                catch (Exception ex)
                {
                    //loadingIndicator.Visibility = Visibility.Collapsed;
                    ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + "\r\n" + ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + " : " + new System.Diagnostics.StackFrame(0, true).GetMethod().Name, ex);
                }

            }, ds);
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
