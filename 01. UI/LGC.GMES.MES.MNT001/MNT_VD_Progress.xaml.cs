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
    public partial class MNT_VD_Progress : Window
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

        private static string mesFlag = "RMS"; // default rms 시간

        private static DateTime dtMainStartTime;
        private static DateTime dtSubStartTime;

        private int subTime = 10;

        DataTable dtMain = new DataTable();

        private int currentPage = 0;

        public MNT_VD_Progress()
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
            numRefreshSub.Value = selectedDisplayTimeSub;

            object[] obj = this.Tag as object[];

            if (string.IsNullOrWhiteSpace(selectedDisplayName) == true)
            {
                if (obj.Length != 0)
                {
                    txtTitle.Text = obj[0] as string;
                }
            }
            else
            {
                txtTitle.Text = selectedDisplayName;
            }


            dtMainStartTime = System.DateTime.Now;
            dtSubStartTime = dtMainStartTime;

            txtDate.Text = Convert.ToString(dtMainStartTime);

            tmrMainTimer = new System.Timers.Timer(0.5 * 1000);
            tmrMainTimer.AutoReset = true;
            tmrMainTimer.Elapsed += (s, arg) =>
            {
                txtDate.Dispatcher.BeginInvoke(new Action(() => txtDate.Text = DateTime.Now.ToString()));
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
            selectedEquipmentSegment = Convert.ToString(cboEquipmentSegment.SelectedValue);
            selectedEquipment = Convert.ToString(cboEquipment.SelectedItemsToString);

            selectedDisplayName = Convert.ToString(txtScreenName.Text);
            selectedDisplayTime = Convert.ToInt32(numRefresh.Value);
            selectedDisplayTimeSub = Convert.ToInt32(numRefreshSub.Value);

            mesFlag = rdoRms.IsChecked == true ? "RMS" : "MES"; //default rms시간

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
                    txtTitle.Text = obj[0] as string;
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
                if (cboEquipmentSegment.SelectedIndex > -1)
                {
                    selectedEquipmentSegment = Convert.ToString(cboEquipmentSegment.SelectedValue);
                    Set_Combo_Equipment(cboEquipment);
                
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

        private void GetDataMain(string Mode)
        {
            if (Mode == "Main")
            {
                DataSet ds = new DataSet();
                DataTable IndataTable = ds.Tables.Add("INDATA");
                //DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("MES", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["SHOPID"] = selectedShop;
                Indata["AREAID"] = selectedArea;
                Indata["EQSGID"] = selectedEquipmentSegment;
                Indata["EQPTID"] = selectedEquipment;
                Indata["MES"] = mesFlag;
                IndataTable.Rows.Add(Indata);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_VD_MONITORING", "INDATA", "OUTDATA", (dsRslt, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.AlertByBiz("BR_PRD_REG_VD_MONITORING", bizException.Message, bizException.ToString());
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

        private Label State_BackGround (Label lbl, DataRow dr)
        {
            switch (Convert.ToString(dr["WIPSTAT"]))
            {
                case "END":
                    lbl.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF99FF66"));
                    break;
                case "PROC":
                    lbl.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFCF600"));
                    break;
                case "DELAY":
                    lbl.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFF0000"));
                    break;
                case "RESERVE":
                    lbl.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF2F2F2"));
                    break;
                default:
                    lbl.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFD1D1D1"));
                    break;
            }

            return lbl;
        }

        private void Clear_Display_Control()
        {
            lblTitleCol01.Content = ObjectDic.Instance.GetObjectName(@"설비(호기)").Replace(@"\r\n", "\r\n");
            lblTitleCol02.Content = ObjectDic.Instance.GetObjectName(@"반제품").Replace(@"\r\n", "\r\n");
            lblTitleCol03.Content = ObjectDic.Instance.GetObjectName(@"투입\r\nLOT수").Replace(@"\r\n", "\r\n");
            lblTitleCol04.Content = ObjectDic.Instance.GetObjectName(@"투입시간").Replace(@"\r\n", "\r\n");
            lblTitleCol05.Content = ObjectDic.Instance.GetObjectName(@"예상종료\r\n" + "(" + mesFlag + ")").Replace(@"\r\n", "\r\n");
            lblTitleCol06.Content = ObjectDic.Instance.GetObjectName(@"잔여시간\r\n(HH:MI)").Replace(@"\r\n", "\r\n");
            lblTitleCol07.Content = ObjectDic.Instance.GetObjectName(@"생산\r\n상태").Replace(@"\r\n", "\r\n");
            lblTitleCol08.Content = ObjectDic.Instance.GetObjectName(@"검사\r\n대상").Replace(@"\r\n", "\r\n");
            lblTitleCol09.Content = ObjectDic.Instance.GetObjectName(@"프로젝트명").Replace(@"\r\n", "\r\n");
            lblTitleCol10.Content = ObjectDic.Instance.GetObjectName(@"종료시점\r\n(Door Open)").Replace(@"\r\n", "\r\n");
            lblTitleCol11.Content = ObjectDic.Instance.GetObjectName(@"검사대기시간(분)").Replace(@"\r\n", "\r\n");
            lblTitleCol12.Content = ObjectDic.Instance.GetObjectName(@"QA타입").Replace(@"\r\n", "\r\n");

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

                Label lbl06 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL06");
                lbl06.Content = string.Empty;
                lbl06.Background = new SolidColorBrush(Colors.Transparent);

                Label lbl07 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL07");
                lbl07.Visibility = Visibility.Collapsed;
                Border br07 = FindChild<Border>(grMain, "bdRow" + (idx + 1).ToString("00") + "COL07");
                br07.Background = new SolidColorBrush(Colors.Transparent);

                Label lbl08 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL08");
                lbl08.Content = string.Empty;
                lbl08.Background = new SolidColorBrush(Colors.Transparent);

                Label lbl09 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL09");
                lbl09.Content = string.Empty;
                lbl09.Background = new SolidColorBrush(Colors.Transparent);

                Label lbl10 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL10");
                lbl10.Content = string.Empty;
                lbl10.Background = new SolidColorBrush(Colors.Transparent);

                Label lbl11 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL11");
                lbl11.Content = string.Empty;
                lbl11.Background = new SolidColorBrush(Colors.Transparent);

                Label lbl12 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL12");
                lbl12.Content = string.Empty;
                lbl12.Background = new SolidColorBrush(Colors.Transparent);

                Label lbl13 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL13");
                lbl13.Content = string.Empty;
                lbl13.Background = new SolidColorBrush(Colors.Transparent);
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
                lbl01.Content = Convert.ToString(dtMain.Rows[rowidx]["EQPTNAME"]);
                lbl01 = State_BackGround(lbl01, dtMain.Rows[rowidx]);

                Label lbl02 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL02");
                lbl02.Content = Convert.ToString(dtMain.Rows[rowidx]["PRODID"]) ;
                lbl02 = State_BackGround(lbl02, dtMain.Rows[rowidx]);

                Label lbl03 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL03");
                lbl03.Content = Convert.ToString(dtMain.Rows[rowidx]["PRJT_NAME"]);
                lbl03 = State_BackGround(lbl03, dtMain.Rows[rowidx]);

                Label lbl04 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL04");
                lbl04.Content = Convert.ToString(dtMain.Rows[rowidx]["LOTQTY"]);
                lbl04 = State_BackGround(lbl04, dtMain.Rows[rowidx]);

                //=======================================================================================================
                DateTime dtStartTime = new DateTime();
                Label lbl05 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL05");
                if (DateTime.TryParse(Convert.ToString(dtMain.Rows[rowidx]["STARTTIME"]), out dtStartTime))
                {
                    lbl05.Content = dtStartTime.ToString("HH:mm");
                }
                else
                {
                    lbl05.Content = string.Empty;
                }
                lbl05 = State_BackGround(lbl05, dtMain.Rows[rowidx]);


                Label lbl06 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL06");
                DateTime dtEndTime = new DateTime();
                if (DateTime.TryParse(Convert.ToString(dtMain.Rows[rowidx]["ENDTIME"]), out dtEndTime))
                {
                    lbl06.Content = dtEndTime.ToString("HH:mm");
                }
                else
                {
                    lbl06.Content = string.Empty;
                }
                lbl06 = State_BackGround(lbl06, dtMain.Rows[rowidx]);
                //=======================================================================================================

                if (Convert.ToString(dtMain.Rows[idx]["WIPSTAT"]) == "DELAY")
                {
                    Label lbl07 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL07");
                    lbl07 = State_BackGround(lbl07, dtMain.Rows[rowidx]);
                    lbl07.Visibility = Visibility.Visible;
                    Border br07 = FindChild<Border>(grMain, "bdRow" + (idx + 1).ToString("00") + "COL07");
                    br07.Background = lbl07.Background;
                }
                else
                {
                    Label lbl07 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL07");
                    lbl07.Visibility = Visibility.Collapsed;
                    Border br07 = FindChild<Border>(grMain, "bdRow" + (idx + 1).ToString("00") + "COL07");
                    br07.Background = lbl06.Background;
                }

                Label lbl08 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL08");

                if (Convert.ToString(dtMain.Rows[idx]["WIPSTAT"]) == "DELAY")
                {
                    if (Convert.ToString(dtMain.Rows[rowidx]["TERMTIME"]) == string.Empty)
                    {
                        lbl08.Content = string.Empty;
                    }
                    else
                    {
                        Int32 tmp = System.Math.Abs(Convert.ToInt32(dtMain.Rows[rowidx]["TERMTIME"]));
                        lbl08.Content = Convert.ToString(tmp);
                    }
                }
                else
                {
                    if (Convert.ToString(dtMain.Rows[rowidx]["TERMTIME"]) == string.Empty)
                    {
                        lbl08.Content = string.Empty;
                    }
                    else
                    {
                        Int32 tmp = System.Math.Abs(Convert.ToInt32(dtMain.Rows[rowidx]["TERMTIME"]));
                        lbl08.Content = Convert.ToString(tmp);
                    }
                }

                lbl08 = State_BackGround(lbl08, dtMain.Rows[rowidx]);

                Label lbl09 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL09");
                lbl09.Content = Convert.ToString(dtMain.Rows[rowidx]["WIPSTATNAME"]);
                lbl09 = State_BackGround(lbl09, dtMain.Rows[rowidx]);

                Label lbl10 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL10");
                lbl10.Content = Convert.ToString(dtMain.Rows[rowidx]["QATRGT"]);
                //lbl10.Content = Convert.ToString(dtMain.Rows[rowidx]["QASTATNAME"]);
                lbl10 = State_BackGround(lbl10, dtMain.Rows[rowidx]);


                Label lbl11 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL11");
                lbl11.Content = Convert.ToString(dtMain.Rows[rowidx]["EQPT_END_DTTM"]);
                lbl11 = State_BackGround(lbl11, dtMain.Rows[rowidx]);


                Label lbl12 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL12");
                lbl12.Content = Convert.ToString(dtMain.Rows[rowidx]["REMAIN_TIME"]);
                lbl12 = State_BackGround(lbl12, dtMain.Rows[rowidx]);

                Label lbl13 = FindChild<Label>(grMain, "lblRow" + (idx + 1).ToString("00") + "COL13");
                lbl13.Content = Convert.ToString(dtMain.Rows[rowidx]["VD_QA_MANL_CHG_NAME"]);
                lbl13 = State_BackGround(lbl13, dtMain.Rows[rowidx]);
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
            Set_Combo_Shop(cboShop, ex01 =>
            {
                cboShop.SelectedValue = selectedShop;
                Set_Combo_Area(cboArea, ex02 =>
                {
                    cboArea.SelectedValue = selectedArea;
                    Set_Combo_EquipmentSegment(cboEquipmentSegment, ex03 =>
                    {
                        cboEquipmentSegment.SelectedValue = selectedEquipmentSegment;
                        Set_Combo_Equipment(cboEquipment, ex04 =>
                        {
                            string[] split = selectedEquipment.Split(',');
                            foreach (string str in split)
                            {
                                cboEquipment.Check(str);
                            }

                            cboShop.SelectedItemChanged -= cboShop_SelectedItemChanged;
                            cboShop.SelectedItemChanged += cboShop_SelectedItemChanged;
                            cboArea.SelectedItemChanged -= cboArea_SelectedItemChanged;
                            cboArea.SelectedItemChanged += cboArea_SelectedItemChanged;
                            cboEquipmentSegment.SelectedItemChanged -= cboEquipmentSegment_SelectedItemChanged;
                            cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
                            cboEquipment.SelectionChanged -= cboEquipment_SelectionChanged;
                            cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;

                            GetDataMain("Main");
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
            dtRQSTDT.Columns.Add("USE_FLAG", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["SYSTEM_ID"] = LoginInfo.SYSID;
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["SHOPID"] = selectedShop;
            drnewrow["USERID"] = LoginInfo.USERID;
            drnewrow["USE_FLAG"] = "Y";
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_PRD_SEL_AREA_FOR_VD_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
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

        private void Set_Combo_EquipmentSegment(C1ComboBox cbo, Action<Exception> action = null)
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
            drnewrow["USE_FLAG"] = "Y";
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_PRD_SEL_EQSG_FOR_VD_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
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

        private void Set_Combo_EquipmentFllor(MultiSelectionBox cbo, Action<Exception> action = null)
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
