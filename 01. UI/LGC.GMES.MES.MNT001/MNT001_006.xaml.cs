/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 공정모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.

 
**************************************************************************************/

using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.MNT001
{
    public partial class MNT001_006 : Window
    {
        #region Declaration & Constructor 

        System.Timers.Timer tmrMainTimer;

        private Storyboard sbExpandLeft;
        private Storyboard sbCollapseLeft;

        private static string selectedShop = string.Empty;
        private static string selectedArea = string.Empty;
        private static string selectedEquipmentSegment = string.Empty;
        private static string selectedProcess = string.Empty;
        private static string selectedElec = string.Empty;

        private static string selectedDisplayName = string.Empty;
        private static int selectedDisplayTime = 1;
        private static int selectedDisplayTimeSub = 10;

        private static DateTime dtMainStartTime;
        private static DateTime dtSubStartTime;

        private string ProcID = string.Empty;

        private int subTime = 10;

        DataTable dtMain = new DataTable();

        private int currentPage = 0;

       
        public MNT001_006()
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
            if (tmrMainTimer != null)
            {
                tmrMainTimer.Stop();
                tmrMainTimer.Dispose();
                tmrMainTimer = null;
            }
        }


        #endregion

        #region Initialize

        private void Initialize()
        {
            //Storyboard =======================================================================================
            sbExpandLeft = (Storyboard)this.Resources["ExpandLeftFrameStoryboard"];
            sbCollapseLeft = (Storyboard)this.Resources["CollapseLeftFrameStoryboard"];

            Clear_Display_Control();

            DataSet ds = LGC.GMES.MES.MNT001.MNT_Common.GetProcessConfigXML();

            selectedShop = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["SHOP"]);
            selectedArea = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["AREA"]);
            selectedEquipmentSegment = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["EQUIPMENTSEGMENT"]);
            selectedProcess = Convert.ToString(ds.Tables["MNT_CONFIG"].Rows[0]["PROCESS"]);

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
            numRefreshSub.Value = selectedDisplayTimeSub;

            GetProcess();
            object[] obj = this.Tag as object[];

            if (string.IsNullOrWhiteSpace(selectedDisplayName) == true)
            {
                if (obj.Length != 0)
                {
                    txtTitle.Text = "[" + ProcID + "] " +  obj[0] as string;
                }
            }
            else
            {
                txtTitle.Text = selectedDisplayName;
            }

            dtMainStartTime = System.DateTime.Now;
            dtSubStartTime = dtMainStartTime;

            txtDate.Text = dtMainStartTime.ToString("yyyy-MM-dd HH:mm:ss");

            tmrMainTimer = new System.Timers.Timer(0.5 * 1000);
            tmrMainTimer.AutoReset = true;
            tmrMainTimer.Elapsed += (s, arg) =>
            {
                txtDate.Dispatcher.BeginInvoke(new Action(() => txtDate.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                TimeSpan spanMain = (DateTime.Now - dtMainStartTime);
                if (spanMain.Minutes >= selectedDisplayTime)
                {
                    this.Dispatcher.BeginInvoke(new Action(() => GetDataMain("Main")));
                    dtMainStartTime = System.DateTime.Now;
                    dtSubStartTime = dtMainStartTime;
                }
                TimeSpan spanSub = (DateTime.Now - dtSubStartTime);
                if (spanSub.Seconds >= selectedDisplayTimeSub)
                {
                    this.Dispatcher.BeginInvoke(new Action(() => GetDataMain("Sub")));
                    dtSubStartTime = dtSubStartTime.AddSeconds(selectedDisplayTimeSub);
                }
            };
            tmrMainTimer.Start();
        }

        #endregion

        #region Event

        private void btnSetSave_Click(object sender, RoutedEventArgs e)
        {
            selectedShop = Convert.ToString(cboShop.SelectedValue);
            selectedArea = Convert.ToString(cboArea.SelectedValue);
            selectedEquipmentSegment = Convert.ToString(cboEquipmentSegment.SelectedItemsToString);
            selectedProcess = Convert.ToString(cboProcess.SelectedItemsToString); // Util.GetCondition(cboProcess);

            selectedElec = Convert.ToString(cboElec.SelectedItemsToString); // Util.GetCondition(cboProcess);

            selectedDisplayName = Convert.ToString(txtScreenName.Text);
            selectedDisplayTime = Convert.ToInt32(numRefresh.Value);
            selectedDisplayTimeSub = Convert.ToInt32(numRefreshSub.Value);

            DataSet ds = new DataSet();

            DataTable dt = new DataTable("MNT_CONFIG");
            dt.Columns.Add("SHOP", typeof(string));
            dt.Columns.Add("AREA", typeof(string));
            dt.Columns.Add("EQUIPMENTSEGMENT", typeof(string));
            dt.Columns.Add("PROCESS", typeof(string));
            dt.Columns.Add("DISPLAYNAME", typeof(string));
            dt.Columns.Add("DISPLAYTIME", typeof(int));
            dt.Columns.Add("DISPLAYTIMESUB", typeof(int));

            DataRow dr = dt.NewRow();
            dr["SHOP"] = selectedShop;
            dr["AREA"] = selectedArea;
            dr["EQUIPMENTSEGMENT"] = selectedEquipmentSegment;
            dr["PROCESS"] = selectedProcess;//Util.GetCondition(cboProcess);
            dr["DISPLAYNAME"] = selectedDisplayName;
            dr["DISPLAYTIME"] = selectedDisplayTime;
            dr["DISPLAYTIMESUB"] = selectedDisplayTimeSub;

            dt.Rows.Add(dr);
            ds.Tables.Add(dt);

            GetProcess();

            LGC.GMES.MES.MNT001.MNT_Common.SetProcessConfigXML(ds);

            object[] obj = this.Tag as object[];
            if (string.IsNullOrWhiteSpace(selectedDisplayName) == true)
            {
                if (obj.Length != 0)
                {
                    txtTitle.Text = "[" + ProcID + "] " + obj[0] as string;
                }
            }
            else
            {
                txtTitle.Text = selectedDisplayName;
            }
            dtMainStartTime = System.DateTime.Now;
            dtSubStartTime = dtMainStartTime;
            GetDataMain("Main");
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

                //if (cboEquipmentSegment.SelectedItems..ToString() > -1)
                //{
                //selectedEquipmentSegment = cboEquipmentSegment.SelectedItemsToString;
                //Set_Combo_Process(cboProcess);


                //}
                //else
                //{
                //    selectedArea = string.Empty;
                //}
            }));
        }
        private void cboEquipmentSegment_SelectionChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                selectedEquipmentSegment = cboEquipmentSegment.SelectedItemsToString;
                Set_Combo_Process(cboProcess);
            }));
        }

        private void cboEquipment_SelectedItemChanged(object sender, EventArgs e)
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

        private void GetDataMain(string Mode)
        {
            if (Mode == "Main")
            {
                //최신버전
                DataSet ds = new DataSet();
                DataTable IndataTable = ds.Tables.Add("INDATA");
                //DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("YYYYMMDD", typeof(string)); 
                IndataTable.Columns.Add("ELECCODE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["SHOPID"] = selectedShop;
                Indata["AREAID"] = selectedArea;
                Indata["EQSGID"] = selectedEquipmentSegment;
                Indata["PROCID"] = selectedProcess;
                Indata["YYYYMMDD"] = DateTime.Now.ToString("yyyyMMdd"); 
                Indata["ELECCODE"] = selectedElec;

                IndataTable.Rows.Add(Indata);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_PROCESS_MONITORING_CWA", "INDATA", "OUTDATA", (dsRslt, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.AlertByBiz("BR_PRD_PROCESS_MONITORING", bizException.Message, bizException.ToString());
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            return;
                        }

                        dtMain = dsRslt.Tables["OUTDATA"];

                        currentPage = 0;
                        this.Dispatcher.BeginInvoke(new Action(() => Set_Display_Control(Mode)));
                        loadingIndicator.Visibility = Visibility.Collapsed;

                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + "\r\n" + ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        Logger.Instance.WriteLine(Logger.OPERATION_R + " : " + new System.Diagnostics.StackFrame(0, true).GetMethod().Name, ex);
                    }

                }, ds);
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action(() => Set_Display_Control(Mode)));
            }
        }

        private void Clear_Display_Control()
        {
            lblTitleCol01.Content = ObjectDic.Instance.GetObjectName(@"라인").Replace(@"\r\n", "\r\n");
            lblTitleCol02.Content = ObjectDic.Instance.GetObjectName(@"제품").Replace(@"\r\n", "\r\n");
            lblTitleCol03.Content = ObjectDic.Instance.GetObjectName(@"계획").Replace(@"\r\n", "\r\n");
            lblTitleCol04.Content = ObjectDic.Instance.GetObjectName(@"실적").Replace(@"\r\n", "\r\n");
            lblTitleCol05.Content = ObjectDic.Instance.GetObjectName(@"진척률").Replace(@"\r\n", "\r\n");

            for (int idx = 0; idx < 10; idx++)
            {
                Label lbl01 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL01");
                lbl01.Content = string.Empty;
                lbl01.Background = new SolidColorBrush(Colors.Transparent);

                Label lbl02 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL02");
                lbl02.Content = string.Empty;
                lbl02.Background = new SolidColorBrush(Colors.Transparent);

                Label lbl03 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL03");
                lbl03.Content = string.Empty;
                lbl03.Background = new SolidColorBrush(Colors.Transparent);

                Label lbl04 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL04");
                lbl04.Content = string.Empty;
                lbl04.Background = new SolidColorBrush(Colors.Transparent);

                Label lbl05 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL05");
                lbl05.Content = string.Empty;
                lbl05.Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void Set_Display_Control(string Mode)
        {
            int totcnt = dtMain.Rows.Count;
            int allPage = Convert.ToInt32(System.Math.Truncate((totcnt - 1) / 10.0)) + 1;

            if (Mode != "Main" && allPage == 1)
            {
                return;
            }

            txtPage.Text = Convert.ToString(currentPage + 1) + " / " + Convert.ToString(allPage);

            Clear_Display_Control();

            for (int idx = 0; idx < 10; idx++)
            {
                int rowidx = idx + (currentPage * 10);

                if (rowidx >= totcnt)
                {
                    break;
                }

                Label lbl01 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL01");
                lbl01.Content = Convert.ToString(dtMain.Rows[rowidx]["EQSGNAME"]);

                Label lbl02 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL02");
                lbl02.Content = Convert.ToString(dtMain.Rows[rowidx]["PRODID"]);

                Label lbl03 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL03");
                lbl03.Content = String.Format("{0:#,##0}", dtMain.Rows[rowidx]["PLAN_QTY"]);


                Label lbl04 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL04");
                lbl04.Content = String.Format("{0:#,##0}", dtMain.Rows[rowidx]["RESULT_QTY"]);

                Label lbl05 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL05");
                lbl05.Content = String.Format("{0:#,##0.00}", dtMain.Rows[rowidx]["MEET_RATE"]);
                
            }

            currentPage++;
            if (currentPage >= allPage)
            {
                currentPage = 0;
            }

            //Set_Display_Control_Merge();
        }

        private void Set_Display_Control_Merge()
        {
            for (int idx = 0; idx < 10; idx++)
            {
                Label lbl = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL01");
                string tmp = Convert.ToString(lbl.Content);
                Grid.SetRowSpan(lbl, 1);
                lbl.Visibility = Visibility.Visible;
            }

            System.Collections.ArrayList itemList = new System.Collections.ArrayList();
            System.Collections.ArrayList cntList = new System.Collections.ArrayList();

            for (int idx01 = 0; idx01 < 10; idx01++)
            {
                Label lbl01 = FindChild<Label>(grMain, "lblRow" + (idx01 + 1).ToString("00") + "COL01");
                string tmp01 = Convert.ToString(lbl01.Content);
                itemList.Add(tmp01);
                int cnt = 0;
                for (int idx02 = 0; idx02 < 10; idx02++)
                {
                    Label lbl02 = FindChild<Label>(grMain, "lblRow" + (idx02 + 1).ToString("00") + "COL01");
                    string tmp02 = Convert.ToString(lbl02.Content);
                    if (tmp02 != string.Empty)
                    {
                        if (Convert.ToString(itemList[idx01]) == tmp02)
                        {
                            cnt++;
                        }
                    }
                }
                cntList.Add(cnt);
            }

            for (int idx01 = 0; idx01 < 10; idx01++)
            {
                Label lbl01 = FindChild<Label>(grMain, "lblRow" + (idx01 + 1).ToString("00") + "COL01");
                string tmp = Convert.ToString(lbl01.Content);
                if (Convert.ToInt32(cntList[idx01]) != 0)
                {
                    if (idx01 == 0)
                    {
                        Grid.SetRowSpan(lbl01, Convert.ToInt32(cntList[idx01]));
                    }
                    else
                    {
                        Label lbl02 = FindChild<Label>(grMain, "lblRow" + idx01.ToString("00") + "COL01");
                        if (Convert.ToString(lbl02.Content) == Convert.ToString(lbl01.Content))
                        {
                            Grid.SetRowSpan(lbl01, 1);
                        }
                        else
                        {
                            Grid.SetRowSpan(lbl01, Convert.ToInt32(cntList[idx01]));
                        }
                    }
                }
            }
        }

        //Combo ==============================================================================================

        private void Init_Combo()
        {

            string[] split = null;
            Set_Combo_Elec(cboElec, ex00 =>
            {
                split = selectedProcess.Split(',');
                foreach (string str in split)
                {
                    cboElec.Check(str);
                }

                Set_Combo_Shop(cboShop, ex01 =>
                {
                    cboShop.SelectedValue = selectedShop;
                    Set_Combo_Area(cboArea, ex02 =>
                    {
                        cboArea.SelectedValue = selectedArea;
                        Set_Combo_EquipmentSegment(cboEquipmentSegment, ex03 =>
                        {
                            //cboEquipmentSegment.SelectedItemsToString = selectedEquipmentSegment;
                            split = selectedEquipmentSegment.Split(',');
                            foreach (string str in split)
                            {
                                cboEquipmentSegment.Check(str);
                            } 

                            Set_Combo_Process(cboProcess, ex04 =>
                            { 
                                // cboProcess.SelectedValuePath = selectedProcess;
                                 
                                split = selectedProcess.Split(',');
                                foreach (string str in split)
                                {
                                    cboProcess.Check(str);
                                }
                                  
                                cboShop.SelectedItemChanged -= cboShop_SelectedItemChanged;
                                cboShop.SelectedItemChanged += cboShop_SelectedItemChanged;
                                cboArea.SelectedItemChanged -= cboArea_SelectedItemChanged;
                                cboArea.SelectedItemChanged += cboArea_SelectedItemChanged;
                                cboEquipmentSegment.SelectionChanged -= cboEquipmentSegment_SelectionChanged;
                                cboEquipmentSegment.SelectionChanged += cboEquipmentSegment_SelectionChanged;
                                cboProcess.SelectionChanged -= cboEquipment_SelectedItemChanged;
                                cboProcess.SelectionChanged += cboEquipment_SelectedItemChanged;

                                GetDataMain("Main");
                            });
                        });
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
            drnewrow["SHOPID"] = selectedShop;
            drnewrow["USERID"] = LoginInfo.USERID;
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_AUTH_AREA_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
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

        private void Set_Combo_Elec(MultiSelectionBox cbo, Action<Exception> action = null)
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
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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
            dtRQSTDT.Columns.Add("USE_FLAG", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["SHOPID"] = selectedShop;
            drnewrow["AREAID"] = selectedArea;
            dtRQSTDT.Rows.Add(drnewrow);

            DataTable dt = new DataTable();
            dt.Columns.Add(cbo.DisplayMemberPath.ToString(), typeof(string));
            dt.Columns.Add(cbo.SelectedValuePath.ToString(), typeof(string));

            new ClientProxy().ExecuteService("DA_PRD_SEL_SHOP_AREA_EQSG_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {

                    //DataRow drSelect = result.NewRow();
                    //drSelect[cbo.DisplayMemberPath.ToString()] = "-ALL-";
                    //drSelect[cbo.SelectedValuePath.ToString()] = "";
                    //result.Rows.InsertAt(drSelect, 0);

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

        private void cboEquipmentSegment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    //SetcboProcess();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void GetProcess()
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("EQSGID", typeof(string));
            dtRQSTDT.Columns.Add("PROCID", typeof(string));
            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["EQSGID"] = selectedEquipmentSegment;
            drnewrow["PROCID"] = selectedProcess; ;
            dtRQSTDT.Rows.Add(drnewrow);

            DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_MNT_ASSY", "INDATA", "RSLTDT", dtRQSTDT);

            if (dtMain.Rows.Count == 0)
                return;

            ProcID = Util.NVC(dtMain.Rows[0]["PROCNAME"]);

        }
        private void Set_Combo_Process(MultiSelectionBox cbo, Action<Exception> action = null)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            //dtRQSTDT.Columns.Add("AREAID", typeof(string));
            dtRQSTDT.Columns.Add("EQSGID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            //drnewrow["AREAID"] = selectedArea;
            drnewrow["EQSGID"] = selectedEquipmentSegment;
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_PRD_SEL_PROC_MNT_ASSY", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
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
