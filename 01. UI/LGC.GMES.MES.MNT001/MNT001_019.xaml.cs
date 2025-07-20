/*************************************************************************************
 Created Date : 2021.01.12
      Creator : 안인효
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.12  DEVELOPER : Initial Created.
  2021.02.15  Grid ColumnName 수정 및 모니터링 Set에 따른 Grid컬럼 width 조정
  2021.02.16  조건에 그리드 폰트사이즈 추가
  2021.02.22  타이머 message 처리
  2021.03.18  저장기능 추가
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
    public partial class MNT001_019 : Window
    {
        #region Declaration
        Util _Util = new Util();
        private Storyboard sbExpandLeft;
        private Storyboard sbCollapseLeft;



       
        private static string selectedEquipmentSegment;
        private static string selectedEquipment_1;
        private static string selectedEquipment_2;
        private static string selectedEquipment_3;
        private static int selectedDisplayTime = 1;

        int oneColumn_width = 0;
        int oneRow_width = 0;
        int iColumnCount = 0;
        int iGgO1ColCount = 0;
        int iGgI1ColCount = 0;
        int iGgO2ColCount = 0;
        int iGgI2ColCount = 0;
        int iGgO3ColCount = 0;
        int iGgI3ColCount = 0;

        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isChnageAutoTime = false;

        private string _bizRuleIp;
        private string _bizRuleProtocol;
        private string _bizRulePort;
        private string _bizRuleServiceMode;
        private string _bizRuleServiceIndex;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion


        #region Constructor

        public MNT001_019()
        {
            try
            {
                InitializeComponent();

                this.Loaded += UserControl_Loaded;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }

        #endregion


        #region Initialize

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            GetBizActorServerInfo();
        }

        #endregion


        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;

            Initialize();

          

            TimerSetting();
        }


        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_monitorTimer != null)
                {
                    _monitorTimer.Stop();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void btnLeftFrame_Checked(object sender, RoutedEventArgs e)
        {
            sbExpandLeft.Begin();
        }


        private void btnLeftFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            sbCollapseLeft.Begin();
        }


        /// <summary>
        /// 설계그룹 변경에 따른 설비 재바인딩
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboEquipmentSegment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboEquipmentSegment.SelectedIndex > -1)
                {
                    selectedEquipmentSegment = Convert.ToString(cboEquipmentSegment.SelectedValue);

                    SetEqptID(cboEquipment_1);
                    SetEqptID(cboEquipment_2);
                    SetEqptID(cboEquipment_3);
                }
                else
                {
                    selectedEquipmentSegment = string.Empty;
                }
            }));
        }


        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            selectedEquipmentSegment = Convert.ToString(cboEquipmentSegment.SelectedValue);
            selectedEquipment_1 = Convert.ToString(cboEquipment_1.SelectedValue);
            selectedEquipment_2 = Convert.ToString(cboEquipment_2.SelectedValue);
            selectedEquipment_3 = Convert.ToString(cboEquipment_3.SelectedValue);
            selectedDisplayTime = Convert.ToInt32(numRefresh.Value);

            if (string.IsNullOrWhiteSpace(selectedEquipmentSegment)
                || (string.IsNullOrWhiteSpace(selectedEquipment_1) && string.IsNullOrWhiteSpace(selectedEquipment_2) && string.IsNullOrWhiteSpace(selectedEquipment_3)))
            {
                Util.MessageValidation("SFU4979");  // 필수 입력
                return;
            }

            #region 파일 저장 추가  -- 오화백
            DataSet ds = new DataSet();

            DataTable dt = new DataTable("MNT_SETTING");
            dt.Columns.Add("EQUIPMENTSEGMENT", typeof(string));
            dt.Columns.Add("EQUIPMENT_1", typeof(string));
            dt.Columns.Add("EQUIPMENT_2", typeof(string));
            dt.Columns.Add("EQUIPMENT_3", typeof(string));
            dt.Columns.Add("DISPLAYTIME", typeof(int));

            DataRow dr = dt.NewRow();
            dr["EQUIPMENTSEGMENT"] = selectedEquipmentSegment;
            dr["EQUIPMENT_1"] = selectedEquipment_1;
            dr["EQUIPMENT_2"] = selectedEquipment_2;
            dr["EQUIPMENT_3"] = selectedEquipment_3;
            dr["DISPLAYTIME"] = selectedDisplayTime;

            dt.Rows.Add(dr);
            ds.Tables.Add(dt);

            LGC.GMES.MES.MNT001.MNT_Common.SetConfigXML_LDR_UDR(ds);

            #endregion


            try
            {
                if (_monitorTimer != null)
                {
                    _monitorTimer.Stop();

                    int second = 0;

                    if (Convert.ToInt32(numRefresh.Value) != 0)
                    {
                        second = (Convert.ToInt32(numRefresh.Value)) * 60;
                        _isSelectedAutoTime = true;
                    }
                    else
                    {
                        _isSelectedAutoTime = false;
                    }

                    if (second == 0 && !_isSelectedAutoTime)
                    {
                        if (_isChnageAutoTime)  // 주기를 변경했을때 ( 0 -> X.. -> 0 도 해당)
                        {
                            Util.MessageValidation("SFU8310");  // 자동조회가 사용하지 않도록 변경 되었습니다.
                            _isChnageAutoTime = false;
                        }
                        
                        btnSetCondition_Click(null, null);
                        return;
                    }

                    btnSetCondition_Click(null, null);

                    _monitorTimer.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer.Start();

                    // 메세지 처리 하지 않도록 요청 - 황보길 위원님
                    //if (_isSelectedAutoTime)
                    //{
                    //    if (Convert.ToInt32(numRefresh.Value) != 0 && _isChnageAutoTime)
                    //    {
                    //        _isChnageAutoTime = false;
                    //        Util.MessageInfo("SFU8311", Convert.ToString((Convert.ToInt32(numRefresh.Value) * 60) / 60));
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void btnSetCondition_Click(object sender, RoutedEventArgs e)
        {
            iColumnCount = 0;

            selectedEquipmentSegment = Convert.ToString(cboEquipmentSegment.SelectedValue);
            selectedEquipment_1 = Convert.ToString(cboEquipment_1.SelectedValue);
            selectedEquipment_2 = Convert.ToString(cboEquipment_2.SelectedValue);
            selectedEquipment_3 = Convert.ToString(cboEquipment_3.SelectedValue);

            if (selectedEquipmentSegment == ""
                || (selectedEquipment_1 == "" && selectedEquipment_2 == "" && selectedEquipment_3 == ""))
            {
                Util.MessageValidation("SFU4979");  // 필수 입력
                return;
            }

            if (!string.IsNullOrWhiteSpace(selectedEquipment_1))
                ++iColumnCount;

            if (!string.IsNullOrWhiteSpace(selectedEquipment_2))
                ++iColumnCount;

            if (!string.IsNullOrWhiteSpace(selectedEquipment_3))
                ++iColumnCount;

            //InitColumnWidth(iColumnCount);

            // 선택 단별수에 따른 Font Size 설정
            switch (iColumnCount)
            {
                case 1:
                    numFontSize.Value = 36;
                    break;
                case 2:
                    numFontSize.Value = 22;
                    break;
                case 3:
                    numFontSize.Value = 20;
                    break;
            }

            #region Column visible 처리

            colOutStatus_1_2.Visibility = Visibility.Visible;
            colOutMode_1_2.Visibility = Visibility.Visible;
            colOutStatus_1_3.Visibility = Visibility.Visible;
            colOutMode_1_3.Visibility = Visibility.Visible;
            colOutStatus_1_4.Visibility = Visibility.Visible;
            colOutMode_1_4.Visibility = Visibility.Visible;

            colOutStatus_2_2.Visibility = Visibility.Visible;
            colOutMode_2_2.Visibility = Visibility.Visible;
            colOutStatus_2_3.Visibility = Visibility.Visible;
            colOutMode_2_3.Visibility = Visibility.Visible;
            colOutStatus_2_4.Visibility = Visibility.Visible;
            colOutMode_2_4.Visibility = Visibility.Visible;

            colOutStatus_3_2.Visibility = Visibility.Visible;
            colOutMode_3_2.Visibility = Visibility.Visible;
            colOutStatus_3_3.Visibility = Visibility.Visible;
            colOutMode_3_3.Visibility = Visibility.Visible;
            colOutStatus_3_4.Visibility = Visibility.Visible;
            colOutMode_3_4.Visibility = Visibility.Visible;



            colInStatus_1_2.Visibility = Visibility.Visible;
            colInMode_1_2.Visibility = Visibility.Visible;
            colInStatus_1_3.Visibility = Visibility.Visible;
            colInMode_1_3.Visibility = Visibility.Visible;
            colInStatus_1_4.Visibility = Visibility.Visible;
            colInMode_1_4.Visibility = Visibility.Visible;

            colInStatus_2_2.Visibility = Visibility.Visible;
            colInMode_2_2.Visibility = Visibility.Visible;
            colInStatus_2_3.Visibility = Visibility.Visible;
            colInMode_2_3.Visibility = Visibility.Visible;
            colInStatus_2_4.Visibility = Visibility.Visible;
            colInMode_2_4.Visibility = Visibility.Visible;

            colInStatus_3_2.Visibility = Visibility.Visible;
            colInMode_3_2.Visibility = Visibility.Visible;
            colInStatus_3_3.Visibility = Visibility.Visible;
            colInMode_3_3.Visibility = Visibility.Visible;
            colInStatus_3_4.Visibility = Visibility.Visible;
            colInMode_3_4.Visibility = Visibility.Visible;

            #endregion


            #region 단 정리 (1단, 2단, 3단)

            gdMain.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            gdMain.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
            gdMain.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);

            if (selectedEquipment_1 == "")
            {
                gdMain.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Star);
            }

            if (selectedEquipment_2 == "")
            {
                gdMain.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Star);
            }

            if (selectedEquipment_3 == "")
            {
                gdMain.ColumnDefinitions[2].Width = new GridLength(0, GridUnitType.Star);
            }

            #endregion


            if (cboEquipmentSegment.SelectedValue.ToString().Equals("LAM"))
            {
                gd_1.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Star);
                gd_2.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Star);
                gd_3.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Star);
            }
            else
            {
                gd_1.RowDefinitions[0].Height = new GridLength(0.26, GridUnitType.Star);
                gd_2.RowDefinitions[0].Height = new GridLength(0.26, GridUnitType.Star);
                gd_3.RowDefinitions[0].Height = new GridLength(0.26, GridUnitType.Star);
            }

            searchData();

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


        private void dgLoader_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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


        private void numRefresh_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            _isChnageAutoTime = true;
        }


        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;

                    btnSetCondition_Click(null, null);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));
        }


        /// <summary>
        /// // 최초 로드시점에 그리드 컬럼 헤더에 # 나오는 것을 해결하기 위해 타이머 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 0) return;

                    btnSetCondition_Click(null, null);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    dpcTmr.Stop();
                    btnExecute_Click(null, null);
                }
            }));
        }

        #endregion


        #region Mehod

        private void GetBizActorServerInfo()
        {

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("KEYGROUPID", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["KEYGROUPID"] = "MCS_AP_CONFIG";
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MCS_CONFIG_INFO", "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                foreach (DataRow newRow in dtResult.Rows)
                {
                    if (newRow["KEYID"].ToString() == "BizActorIP")
                        _bizRuleIp = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorPort")
                        _bizRulePort = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorProtocol")
                        _bizRuleProtocol = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorServiceIndex")
                        _bizRuleServiceIndex = newRow["KEYVALUE"].ToString();
                    else
                        _bizRuleServiceMode = newRow["KEYVALUE"].ToString();
                }
            }

            //_bizRuleIp = "10.32.169.226";
            //_bizRulePort = "7865";
            //_bizRuleProtocol = "TCP";
            //_bizRuleServiceIndex = "0";
            //_bizRuleServiceMode = "SERVICE";

        }


        private void Initialize()
        {
            try
            {
                SetEqsg(cboEquipmentSegment);

                InitColumnWidth(0, 3);

                //Storyboard =======================================================================================
                sbExpandLeft = (Storyboard)this.Resources["ExpandLeftFrameStoryboard"];
                sbCollapseLeft = (Storyboard)this.Resources["CollapseLeftFrameStoryboard"];


                #region 파일 저장 추가  -- 오화백
                DataSet ds = LGC.GMES.MES.MNT001.MNT_Common.GetConfigXML_LDR_UDR();

                selectedEquipmentSegment = Convert.ToString(ds.Tables["MNT_SETTING"].Rows[0]["EQUIPMENTSEGMENT"]);
                selectedEquipment_1 = Convert.ToString(ds.Tables["MNT_SETTING"].Rows[0]["EQUIPMENT_1"]);
                selectedEquipment_2 = Convert.ToString(ds.Tables["MNT_SETTING"].Rows[0]["EQUIPMENT_2"]);
                selectedEquipment_3 = Convert.ToString(ds.Tables["MNT_SETTING"].Rows[0]["EQUIPMENT_3"]);
                selectedDisplayTime = Convert.ToInt32(ds.Tables["MNT_SETTING"].Rows[0]["DISPLAYTIME"]);

                SetEqptID(cboEquipment_1);
                SetEqptID(cboEquipment_2);
                SetEqptID(cboEquipment_3);
             
                cboEquipmentSegment.SelectedValue = selectedEquipmentSegment;
                cboEquipment_1.SelectedValue = selectedEquipment_1;
                cboEquipment_2.SelectedValue = selectedEquipment_2;
                cboEquipment_3.SelectedValue = selectedEquipment_3;
                numRefresh.Value = selectedDisplayTime;
                //if(selectedEquipmentSegment != string.Empty)
                //{
                //    btnExecute_Click(null, null);
                //}

                // 최초 로드시점에 그리드 컬럼 헤더에 # 나오는 것을 해결하기 위해 타이머 처리
                DispatcherTimer timer = new DispatcherTimer();
                timer.Tick += Timer_Tick;
                timer.Interval = TimeSpan.FromSeconds(0.1);
                timer.Start();
                #endregion

            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }


        private void InitColumnWidth(int iCheck, int iColumn)
        {
            int W = Screen.PrimaryScreen.Bounds.Width; //모니터 스크린 가로크기
            int H = Screen.PrimaryScreen.Bounds.Height; //모니터 스크린 세로크기

            double w_temp = (W - 950 - 2); //전체 넓이에서 고정된 column의 넓이 뺌.
            double h_temp = (H - 70);  //전체 높이에서 고정된 Grid의 높이 뺌

            oneColumn_width = Convert.ToInt32(Math.Round(w_temp / 5)); // 반올림 : 고정되지 않은 컬럼수로 나눔
            oneRow_width = Convert.ToInt32(Math.Round(h_temp / 12)); // 반올림 : 고정되지 않은 GRID 수로 나눔

            fullSize.Width = new GridLength(W);


            dgLoader_1.Width = W / iColumn;

            foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgLoader_1.Columns)
            {
                switch (dgc.Name)
                {
                    case "EQPTID":
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength((dgLoader_1.Width / (dgLoader_1.Columns.Count + 2)) * 1.4);
                        break;
                    case "PRJT_NAME":
                    case "VER":
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength((dgLoader_1.Width / (dgLoader_1.Columns.Count + 2)) * 2);
                        break;
                    case "STOCK_CNT":
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgLoader_1.Width / (dgLoader_1.Columns.Count + 2));
                        break;
                    default:
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength((dgLoader_1.Width / (dgLoader_1.Columns.Count + 2)) * 0.6);
                        break;
                }
            }

            if (iCheck == 0 || iCheck == 1)
            {
                dgUnLoaderOut_1.Width = W / iColumn;
                if (iGgO1ColCount == 0)
                    iGgO1ColCount = dgUnLoaderOut_1.Columns.Count;
                //++iGgO1ColCount;    // Port width를 두배로 하기 위해  -- > 요청으로 막음 (by 오화백G, 2021-02-24) 
                for (int i = 0; i < dgUnLoaderOut_1.Columns.Count; i++)
                {
                    //if (i != 0)
                    //{
                    dgUnLoaderOut_1.Columns[i].Width = new C1.WPF.DataGrid.DataGridLength(dgUnLoaderOut_1.Width / iGgO1ColCount);
                    //}
                    //else
                    //{
                    //    dgUnLoaderOut_1.Columns[i].Width = new C1.WPF.DataGrid.DataGridLength((dgUnLoaderOut_1.Width / iGgO1ColCount) * 2);
                    //}
                }
            }

            if (iCheck == 0 || iCheck == 2)
            {
                dgUnLoaderIn_1.Width = W / iColumn;
                if (iGgI1ColCount == 0)
                    iGgI1ColCount = dgUnLoaderIn_1.Columns.Count;
                ++iGgI1ColCount;    // 1) Port width를 두배로 하기 위해  -- > 요청으로 막음 (by 오화백G, 2021-02-24)
                                    // 2) STOKER  width를 두배로 하기 위해 재사용

                for (int i = 0; i < dgUnLoaderIn_1.Columns.Count; i++)
                {
                    //if (i != 0)
                    if (i != dgUnLoaderIn_1.Columns.Count - 1)
                    {
                        dgUnLoaderIn_1.Columns[i].Width = new C1.WPF.DataGrid.DataGridLength(dgUnLoaderIn_1.Width / iGgI1ColCount);
                    }
                    else
                    {
                        dgUnLoaderIn_1.Columns[i].Width = new C1.WPF.DataGrid.DataGridLength((dgUnLoaderIn_1.Width / iGgI1ColCount) * 2);
                    }
                }
            }


            dgLoader_2.Width = W / iColumn;

            foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgLoader_2.Columns)
            {
                switch (dgc.Name)
                {
                    case "EQPTID":
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength((dgLoader_2.Width / (dgLoader_2.Columns.Count + 2)) * 1.4);
                        break;
                    case "PRJT_NAME":
                    case "VER":
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength((dgLoader_2.Width / (dgLoader_2.Columns.Count + 2)) * 2);
                        break;
                    case "STOCK_CNT":
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgLoader_2.Width / (dgLoader_2.Columns.Count + 2));
                        break;
                    default:
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength((dgLoader_2.Width / (dgLoader_2.Columns.Count + 2)) * 0.6);
                        break;
                }
            }

            if (iCheck == 0 || iCheck == 3)
            {
                dgUnLoaderOut_2.Width = W / iColumn;
                if (iGgO2ColCount == 0)
                    iGgO2ColCount = dgUnLoaderOut_2.Columns.Count;
                //++iGgO2ColCount;    // Port width를 두배로 하기 위해  -- > 요청으로 막음 (by 오화백G, 2021-02-24)
                for (int i = 0; i < dgUnLoaderOut_2.Columns.Count; i++)
                {
                    //if (i != 0)
                    //{
                    dgUnLoaderOut_2.Columns[i].Width = new C1.WPF.DataGrid.DataGridLength(dgUnLoaderOut_2.Width / iGgO2ColCount);
                    //}
                    //else
                    //{
                    //    dgUnLoaderOut_2.Columns[i].Width = new C1.WPF.DataGrid.DataGridLength((dgUnLoaderOut_2.Width / iGgO2ColCount) * 2);
                    //}
                }
            }

            if (iCheck == 0 || iCheck == 4)
            {
                dgUnLoaderIn_2.Width = W / iColumn;
                if (iGgI2ColCount == 0)
                    iGgI2ColCount = dgUnLoaderIn_2.Columns.Count;
                ++iGgI2ColCount;    // Port width를 두배로 하기 위해  -- > 요청으로 막음 (by 오화백G, 2021-02-24)
                                    // 2) STOKER  width를 두배로 하기 위해 재사용
                for (int i = 0; i < dgUnLoaderIn_2.Columns.Count; i++)
                {
                    //if (i != 0)
                    if (i != dgUnLoaderIn_2.Columns.Count - 1)
                    {
                        dgUnLoaderIn_2.Columns[i].Width = new C1.WPF.DataGrid.DataGridLength(dgUnLoaderIn_2.Width / iGgI2ColCount);
                    }
                    else
                    {
                        dgUnLoaderIn_2.Columns[i].Width = new C1.WPF.DataGrid.DataGridLength((dgUnLoaderIn_2.Width / iGgI2ColCount) * 2);
                    }
                }
            }
                

            dgLoader_3.Width = W / iColumn;
            
            foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgLoader_3.Columns)
            {
                switch (dgc.Name)
                {
                    case "EQPTID":
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength((dgLoader_3.Width / (dgLoader_3.Columns.Count + 2)) * 1.4);
                        break;
                    case "PRJT_NAME":
                    case "VER":
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength((dgLoader_3.Width / (dgLoader_3.Columns.Count + 2)) * 2);
                        break;
                    case "STOCK_CNT":
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgLoader_3.Width / (dgLoader_3.Columns.Count + 2));
                        break;
                    default:
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength((dgLoader_3.Width / (dgLoader_3.Columns.Count + 2)) * 0.6);
                        break;
                }
            }

            if (iCheck == 0 || iCheck == 5)
            {
                dgUnLoaderOut_3.Width = W / iColumn;
                if (iGgO3ColCount == 0)
                    iGgO3ColCount = dgUnLoaderOut_3.Columns.Count;
                //++iGgO3ColCount;    // Port width를 두배로 하기 위해  -- > 요청으로 막음 (by 오화백G, 2021-02-24)
                for (int i = 0; i < dgUnLoaderOut_3.Columns.Count; i++)
                {
                    //if (i != 0)
                    //{
                    dgUnLoaderOut_3.Columns[i].Width = new C1.WPF.DataGrid.DataGridLength(dgUnLoaderOut_3.Width / iGgO3ColCount);
                    //}
                    //else
                    //{
                    //    dgUnLoaderOut_3.Columns[i].Width = new C1.WPF.DataGrid.DataGridLength((dgUnLoaderOut_3.Width / iGgO3ColCount) * 2);
                    //}
                }
            }

            if (iCheck == 0 || iCheck == 6)
            {
                dgUnLoaderIn_3.Width = W / iColumn;
                if (iGgI3ColCount == 0)
                    iGgI3ColCount = dgUnLoaderIn_3.Columns.Count;
                ++iGgI3ColCount;    // Port width를 두배로 하기 위해  -- > 요청으로 막음 (by 오화백G, 2021-02-24)
                                    // 2) STOKER  width를 두배로 하기 위해 재사용
                for (int i = 0; i < dgUnLoaderIn_3.Columns.Count; i++)
                {
                    //if (i != 0)
                    if (i != dgUnLoaderIn_3.Columns.Count - 1)
                    {
                        dgUnLoaderIn_3.Columns[i].Width = new C1.WPF.DataGrid.DataGridLength(dgUnLoaderIn_3.Width / iGgI3ColCount);
                    }
                    else
                    {
                        dgUnLoaderIn_3.Columns[i].Width = new C1.WPF.DataGrid.DataGridLength((dgUnLoaderIn_3.Width / iGgI3ColCount) * 2);
                    }
                }
            }
        }


        private void TimerSetting()
        {
            if (_monitorTimer != null)
            {
                int second = 0;

                second = (Convert.ToInt32(numRefresh.Value)) * 60;

                _monitorTimer.Tick += _dispatcherTimer_Tick;
                _monitorTimer.Interval = new TimeSpan(0, 0, second);
            }
        }


        private void SetEqsg(C1ComboBox cbo)
        {
            try
            {
                DataTable inDataTable = new DataTable();

                DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync("DA_SEL_MMD_EQPT_GROUP", "RQSTDT", "RSLTDT", inDataTable);

                cbo.DisplayMemberPath = "EQPT_GROUP_NM";
                cbo.SelectedValuePath = "EQPT_GROUP";

                DataRow dr1 = dtResult.NewRow();
                dr1["EQPT_GROUP_NM"] = " - SELECT-";
                dr1["EQPT_GROUP"] = "";
                dtResult.Rows.InsertAt(dr1, 0);

                cbo.ItemsSource = dtResult.Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void SetEqptID(C1ComboBox cbo)
        {

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPT_GROUP", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPT_GROUP"] = selectedEquipmentSegment;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync("DA_SEL_MMD_EQPT_ID_NAME", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "EQPT_NAME";
                cbo.SelectedValuePath = "EQPTID";

                DataRow dr1 = dtResult.NewRow();
                dr1["EQPT_NAME"] = " - SELECT-";
                dr1["EQPTID"] = "";
                dtResult.Rows.InsertAt(dr1, 0);

                cbo.ItemsSource = DataTableConverter.Convert(dtResult);


                //cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void searchData()
        {
            try
            {
                if(selectedEquipment_1 != "")
                {
                    #region set - 1

                    DataSet ds = new DataSet();

                    DataTable indata = new DataTable();
                    indata.TableName = "IN_INFORM";
                    indata.Columns.Add("EQPT_GROUP", typeof(string));
                    indata.Columns.Add("EQPTID", typeof(string));

                    DataRow dr = indata.NewRow();
                    dr["EQPT_GROUP"] = selectedEquipmentSegment;
                    dr["EQPTID"] = selectedEquipment_1;
                    indata.Rows.Add(dr);

                    ds.Tables.Add(indata);

                    DataSet resultSet = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync_Multi("BR_GET_UDR_LDR_MNTR_INFO", "IN_INFORM", "OUT_UDR_IN_EQPT_INFO,OUT_UDR_IN_STATUS,OUT_UDR_IN_STK_INFO,OUT_UDR_OUT_EQPT_INFO,OUT_UDR_OUT_STATUS,OUT_UDR_OUT_STK_INFO,OUT_LDR_CONN_STK_INFO", ds);

                    if (resultSet != null)
                    {
                        if (selectedEquipmentSegment.Equals("NTC"))
                        {
                            tbTitle_1.Text = resultSet.Tables["OUT_UDR_OUT_EQPT_INFO"].Rows[0]["EQPT_NAME"].ToString();
                            Util.GridSetData(dgLoader_1, resultSet.Tables["OUT_LDR_CONN_STK_INFO"], FrameOperation, false);
                        }

                        tbTitUnLoaderOut_1.Text = resultSet.Tables["OUT_UDR_IN_EQPT_INFO"].Rows[0]["EQPT_NAME"].ToString();


                        #region Output

                        string strLocID = string.Empty;

                        DataTable dtOUT_UDR_OUT_STATUS = new DataTable();

                        dtOUT_UDR_OUT_STATUS.Columns.Add("SRC_LOCID", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("JOB", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("SKIDID", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("SRC_STATUS", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("SRC_MODE", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_STATUS_11", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_MODE_11", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_STATUS_12", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_MODE_12", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_STATUS_13", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_MODE_13", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_STATUS_14", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_MODE_14", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("CURR_STOCK_RATIO", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("AVAIL_QTY", typeof(string));

                        DataRow drNew = null;
                        int iStep = 0;

                        foreach (DataRow row in resultSet.Tables["OUT_UDR_OUT_STATUS"].Rows)
                        {
                            if (row["SRC_LOCID"].Equals(strLocID))
                            {
                                switch (iStep)
                                {
                                    case 1:
                                        drNew["DST_STATUS_12"] = row["DST_STATUS"].ToString();
                                        drNew["DST_MODE_12"] = row["DST_MODE"].ToString();

                                        dgUnLoaderOut_1.Columns["DST_STATUS_12"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "S" };
                                        dgUnLoaderOut_1.Columns["DST_MODE_12"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "M" };

                                        ++iStep;
                                        break;
                                    case 2:
                                        drNew["DST_STATUS_13"] = row["DST_STATUS"].ToString();
                                        drNew["DST_MODE_13"] = row["DST_MODE"].ToString();

                                        dgUnLoaderOut_1.Columns["DST_STATUS_13"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "S" };
                                        dgUnLoaderOut_1.Columns["DST_MODE_13"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "M" };

                                        ++iStep;
                                        break;
                                    case 3:
                                        drNew["DST_STATUS_14"] = row["DST_STATUS"].ToString();
                                        drNew["DST_MODE_14"] = row["DST_MODE"].ToString();

                                        dgUnLoaderOut_1.Columns["DST_STATUS_14"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "S" };
                                        dgUnLoaderOut_1.Columns["DST_MODE_14"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "M" };
                                        ++iStep; // display를 위해 사용
                                        break;
                                }

                                continue;
                            }
                            else
                            {
                                if (drNew != null)
                                {
                                    dtOUT_UDR_OUT_STATUS.Rows.Add(drNew);
                                    iStep = 0;
                                }

                                strLocID = row["SRC_LOCID"].ToString();

                                drNew = dtOUT_UDR_OUT_STATUS.NewRow();
                                drNew["SRC_LOCID"] = row["SRC_LOCID"].ToString().Substring(8, 4);
                                drNew["JOB"] = row["JOB"].ToString();
                                drNew["SKIDID"] = row["SKIDID"].ToString();
                                drNew["SRC_STATUS"] = row["SRC_STATUS"].ToString().Trim();
                                drNew["SRC_MODE"] = row["SRC_MODE"].ToString();

                                drNew["DST_STATUS_11"] = row["DST_STATUS"].ToString();
                                drNew["DST_MODE_11"] = row["DST_MODE"].ToString();

                                txtUnloderOut_1.Text  = row["SRC_LOCID"].ToString().Substring(0, 8);

                                //List<string> args;
                                //args = new List<string>();
                                //args.Add("To(" + row["DST_LOCID"].ToString().Substring(0, 8) + ")");
                                //args.Add(row["DST_LOCID"].ToString().Substring(8, 4));
                                //args.Add("S");
                                //dgUnLoaderOut_1.Columns["DST_STATUS_11"].Header = args;
                                //dgUnLoaderOut_1.Columns["DST_STATUS_11"].Width = new C1.WPF.DataGrid.DataGridLength(100);

                                //args = new List<string>();
                                //args.Add("To(" + row["DST_LOCID"].ToString().Substring(0, 8) + ")");
                                //args.Add(row["DST_LOCID"].ToString().Substring(8, 4));
                                //args.Add("M");
                                //dgUnLoaderOut_1.Columns["DST_MODE_11"].Header = args;

                                  //< c1:DataGridTextColumn Header = "[From,NND,Skid]"                Binding = "{Binding SKIDID}"            HorizontalAlignment = "Left"      HeaderStyle = "{StaticResource CenterGridHeaderStyle_13}" x: Name = "colOutSkid_1_1" />

                                  // < c1:DataGridTextColumn Header = "[From,NND,S]"                   Binding = "{Binding SRC_STATUS}"        HorizontalAlignment = "Center"    HeaderStyle = "{StaticResource CenterGridHeaderStyle_13}" x: Name = "colOutSrcSatus_1_2" />

                                  //< c1:DataGridTextColumn Header = "[From,NND,M]"                   Binding = "{Binding SRC_MODE}"          HorizontalAlignment = "Center"    HeaderStyle = "{StaticResource CenterGridHeaderStyle_13}" x: Name = "colOutSrcMode_1_2" />
                                if (selectedEquipmentSegment == "LAM")
                                {
                                    dgUnLoaderOut_1.Columns["SKIDID"].Header = new List<string>() {"From", "LAM", "Skid" };
                                    dgUnLoaderOut_1.Columns["SRC_STATUS"].Header = new List<string>() { "From", "LAM", "S" };
                                    dgUnLoaderOut_1.Columns["SRC_MODE"].Header = new List<string>() { "From", "LAM", "M" };
                                }
                                else
                                {
                                    dgUnLoaderOut_1.Columns["SKIDID"].Header = new List<string>() { "From", "NND", "Skid" };
                                    dgUnLoaderOut_1.Columns["SRC_STATUS"].Header = new List<string>() { "From", "NND", "S" };
                                    dgUnLoaderOut_1.Columns["SRC_MODE"].Header = new List<string>() { "From", "NND", "M" };
                                }



                                dgUnLoaderOut_1.Columns["DST_STATUS_11"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "S" };
                                dgUnLoaderOut_1.Columns["DST_MODE_11"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "M" };

                                if (resultSet.Tables["OUT_UDR_OUT_STK_INFO"].Rows.Count != 0)
                                {
                                    drNew["CURR_STOCK_RATIO"] = resultSet.Tables["OUT_UDR_OUT_STK_INFO"].Rows[0]["CURR_STOCK_RATIO"];
                                    drNew["AVAIL_QTY"] = resultSet.Tables["OUT_UDR_OUT_STK_INFO"].Rows[0]["AVAIL_QTY"];

                                    dgUnLoaderOut_1.Columns["CURR_STOCK_RATIO"].Header = new List<string>() { "STK (" + resultSet.Tables["OUT_UDR_OUT_STK_INFO"].Rows[0]["EQPTID"].ToString().Substring(5,3) +")", "STK (" + resultSet.Tables["OUT_UDR_OUT_STK_INFO"].Rows[0]["EQPTID"].ToString().Substring(5, 3) + ")", "RATE" };
                                    dgUnLoaderOut_1.Columns["AVAIL_QTY"].Header = new List<string>() { "STK (" + resultSet.Tables["OUT_UDR_OUT_STK_INFO"].Rows[0]["EQPTID"].ToString().Substring(5, 3) + ")", "STK (" + resultSet.Tables["OUT_UDR_OUT_STK_INFO"].Rows[0]["EQPTID"].ToString().Substring(5, 3) + ")", "QTY" };
                                }
                                else
                                {
                                    dgUnLoaderOut_1.Columns["CURR_STOCK_RATIO"].Header = new List<string>() { "STK", "STK", "RATE" };
                                    dgUnLoaderOut_1.Columns["AVAIL_QTY"].Header = new List<string>() { "STK", "STK", "QTY" };
                                }

                                ++iStep;
                            }
                        }

                        if (drNew != null)
                        {
                            //dgUnLoaderOut_1.Refresh();
                            dtOUT_UDR_OUT_STATUS.Rows.Add(drNew);
                        }

                        if (dtOUT_UDR_OUT_STATUS.Rows.Count > 0)
                        {
                            Util.GridSetData(dgUnLoaderOut_1, dtOUT_UDR_OUT_STATUS, FrameOperation,false);

                            iGgO1ColCount = dgUnLoaderOut_1.Columns.Count;
                            switch (iStep)
                            {
                                case 1:
                                    colOutStatus_1_2.Visibility = Visibility.Collapsed;
                                    colOutMode_1_2.Visibility = Visibility.Collapsed;
                                    colOutStatus_1_3.Visibility = Visibility.Collapsed;
                                    colOutMode_1_3.Visibility = Visibility.Collapsed;
                                    colOutStatus_1_4.Visibility = Visibility.Collapsed;
                                    colOutMode_1_4.Visibility = Visibility.Collapsed;
                                    iGgO1ColCount = dgUnLoaderOut_1.Columns.Count - 6;
                                    break;
                                case 2:
                                    colOutStatus_1_3.Visibility = Visibility.Collapsed;
                                    colOutMode_1_3.Visibility = Visibility.Collapsed;
                                    colOutStatus_1_4.Visibility = Visibility.Collapsed;
                                    colOutMode_1_4.Visibility = Visibility.Collapsed;
                                    iGgO1ColCount = dgUnLoaderOut_1.Columns.Count - 4;
                                    break;
                                case 3:
                                    colOutStatus_1_4.Visibility = Visibility.Collapsed;
                                    colOutMode_1_4.Visibility = Visibility.Collapsed;
                                    iGgO1ColCount = dgUnLoaderOut_1.Columns.Count - 2;
                                    break;
                            }

                            InitColumnWidth(1, iColumnCount);

                        }
                        #endregion


                        #region Input

                        strLocID = string.Empty;

                        DataTable dtOUT_UDR_IN_STATUS = new DataTable();

                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_LOCID", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("JOB", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("SKIDID", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("SRC_STATUS", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("SRC_MODE", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_STATUS_21", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_MODE_21", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_STATUS_22", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_MODE_22", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_STATUS_23", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_MODE_23", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_STATUS_24", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_MODE_24", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("E_SKID_CNT", typeof(string));

                        drNew = null;
                        iStep = 0;

                        foreach (DataRow row in resultSet.Tables["OUT_UDR_IN_STATUS"].Rows)
                        {
                            if (row["DST_LOCID"].Equals(strLocID))
                            {
                                switch (iStep)
                                {
                                    case 1:
                                        drNew["DST_STATUS_22"] = row["SRC_STATUS"].ToString();
                                        drNew["DST_MODE_22"] = row["SRC_MODE"].ToString();

                                        dgUnLoaderIn_1.Columns["DST_STATUS_22"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "S" };
                                        dgUnLoaderIn_1.Columns["DST_MODE_22"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "M" };
                                        ++iStep;
                                        break;
                                    case 2:
                                        drNew["DST_STATUS_23"] = row["SRC_STATUS"].ToString();
                                        drNew["DST_MODE_23"] = row["SRC_MODE"].ToString();

                                        dgUnLoaderIn_1.Columns["DST_STATUS_23"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "S" };
                                        dgUnLoaderIn_1.Columns["DST_MODE_23"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "M" };
                                        ++iStep;
                                        break;
                                    case 3:
                                        drNew["DST_STATUS_24"] = row["SRC_STATUS"].ToString();
                                        drNew["DST_MODE_24"] = row["SRC_MODE"].ToString();

                                        dgUnLoaderIn_1.Columns["DST_STATUS_24"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "S" };
                                        dgUnLoaderIn_1.Columns["DST_MODE_24"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "M" };
                                        ++iStep; // display를 위해 사용
                                        break;
                                }

                                continue;
                            }
                            else
                            {
                                if (drNew != null)
                                {
                                    dtOUT_UDR_IN_STATUS.Rows.Add(drNew);
                                    iStep = 0;
                                }

                                strLocID = row["DST_LOCID"].ToString();

                                drNew = dtOUT_UDR_IN_STATUS.NewRow();
                                drNew["DST_LOCID"] = row["DST_LOCID"].ToString().Substring(8, 4);
                                drNew["JOB"] = row["JOB"].ToString();
                                drNew["SKIDID"] = row["SKIDID"].ToString();
                                drNew["SRC_STATUS"] = row["DST_STATUS"].ToString().Trim();
                                drNew["SRC_MODE"] = row["DST_MODE"].ToString();

                                drNew["DST_STATUS_21"] = row["SRC_STATUS"].ToString();
                                drNew["DST_MODE_21"] = row["SRC_MODE"].ToString();

                                txtUnloderIn_1.Text = row["DST_LOCID"].ToString().Substring(0, 8);


                                if (selectedEquipmentSegment == "LAM")
                                {
                                    dgUnLoaderIn_1.Columns["SKIDID"].Header = new List<string>() { "To", "LAM", "Skid" };
                                    dgUnLoaderIn_1.Columns["SRC_STATUS"].Header = new List<string>() { "To", "LAM", "S" };
                                    dgUnLoaderIn_1.Columns["SRC_MODE"].Header = new List<string>() { "To", "LAM", "M" };
                                }
                                else
                                {
                                    dgUnLoaderIn_1.Columns["SKIDID"].Header = new List<string>() { "To", "NND", "Skid" };
                                    dgUnLoaderIn_1.Columns["SRC_STATUS"].Header = new List<string>() { "To", "NND", "S" };
                                    dgUnLoaderIn_1.Columns["SRC_MODE"].Header = new List<string>() { "To", "NND", "M" };
                                }
                                string[] sColumnName = new string[] { "E_SKID_CNT" };
                                if (selectedEquipmentSegment == "NTC")
                                {
                                    _Util.SetDataGridMergeExtensionCol(dgUnLoaderIn_1, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                                }
                                else
                                {
                                    _Util.SetDataGridMergeExtensionCol(dgUnLoaderIn_1, sColumnName, DataGridMergeMode.NONE);
                                }
                                    
                              

                                dgUnLoaderIn_1.Columns["DST_STATUS_21"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "S" };
                                dgUnLoaderIn_1.Columns["DST_MODE_21"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "M" };

                             


                                if (resultSet.Tables["OUT_UDR_IN_STK_INFO"].Rows.Count != 0)
                                {
                                    drNew["E_SKID_CNT"] = resultSet.Tables["OUT_UDR_IN_STK_INFO"].Rows[0]["E_SKID_CNT"];

                                    dgUnLoaderIn_1.Columns["E_SKID_CNT"].Header = new List<string>() { "STK(" + resultSet.Tables["OUT_UDR_IN_STK_INFO"].Rows[0]["EQPTID"].ToString().Substring(5,3) +")", "STK(" + resultSet.Tables["OUT_UDR_IN_STK_INFO"].Rows[0]["EQPTID"].ToString().Substring(5, 3) + ")", "QTY" };
                                }
                                else
                                {
                                    dgUnLoaderIn_1.Columns["E_SKID_CNT"].Header = new List<string>() { "STK", "STK", "" };
                                }

                                ++iStep;
                            }
                        }

                        if (drNew != null)
                        {
                            dtOUT_UDR_IN_STATUS.Rows.Add(drNew);
                        }

                        if (dtOUT_UDR_IN_STATUS.Rows.Count > 0)
                        {
                            Util.GridSetData(dgUnLoaderIn_1, dtOUT_UDR_IN_STATUS, FrameOperation, false);

                            iGgI1ColCount = dgUnLoaderIn_1.Columns.Count;
                            switch (iStep)
                            {
                                case 1:
                                    colInStatus_1_2.Visibility = Visibility.Collapsed;
                                    colInMode_1_2.Visibility = Visibility.Collapsed;
                                    colInStatus_1_3.Visibility = Visibility.Collapsed;
                                    colInMode_1_3.Visibility = Visibility.Collapsed;
                                    colInStatus_1_4.Visibility = Visibility.Collapsed;
                                    colInMode_1_4.Visibility = Visibility.Collapsed;
                                    iGgI1ColCount = dgUnLoaderIn_1.Columns.Count - 6;
                                    break;
                                case 2:
                                    colInStatus_1_3.Visibility = Visibility.Collapsed;
                                    colInMode_1_3.Visibility = Visibility.Collapsed;
                                    colInStatus_1_4.Visibility = Visibility.Collapsed;
                                    colInMode_1_4.Visibility = Visibility.Collapsed;
                                    iGgI1ColCount = dgUnLoaderIn_1.Columns.Count - 4;
                                    break;
                                case 3:
                                    colInStatus_1_4.Visibility = Visibility.Collapsed;
                                    colInMode_1_4.Visibility = Visibility.Collapsed;
                                    iGgI1ColCount = dgUnLoaderIn_1.Columns.Count - 2;
                                    break;
                            }

                            InitColumnWidth(2, iColumnCount);
                        }
                        #endregion
                    }
                    #endregion
                }

                if (selectedEquipment_2 != "")
                {
                    #region set - 2

                    DataSet ds = new DataSet();

                    DataTable indata = new DataTable();
                    indata.TableName = "IN_INFORM";
                    indata.Columns.Add("EQPT_GROUP", typeof(string));
                    indata.Columns.Add("EQPTID", typeof(string));

                    DataRow dr = indata.NewRow();
                    dr["EQPT_GROUP"] = selectedEquipmentSegment;
                    dr["EQPTID"] = selectedEquipment_2;
                    indata.Rows.Add(dr);

                    ds.Tables.Add(indata);

                    DataSet resultSet = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync_Multi("BR_GET_UDR_LDR_MNTR_INFO", "IN_INFORM", "OUT_UDR_IN_EQPT_INFO,OUT_UDR_IN_STATUS,OUT_UDR_IN_STK_INFO,OUT_UDR_OUT_EQPT_INFO,OUT_UDR_OUT_STATUS,OUT_UDR_OUT_STK_INFO,OUT_LDR_CONN_STK_INFO", ds);

                    if (resultSet != null)
                    {
                        if (selectedEquipmentSegment.Equals("NTC"))
                        {
                            tbTitle_2.Text = resultSet.Tables["OUT_UDR_OUT_EQPT_INFO"].Rows[0]["EQPT_NAME"].ToString();
                            Util.GridSetData(dgLoader_2, resultSet.Tables["OUT_LDR_CONN_STK_INFO"], FrameOperation, false);
                        }

                        tbTitUnLoaderOut_2.Text = resultSet.Tables["OUT_UDR_IN_EQPT_INFO"].Rows[0]["EQPT_NAME"].ToString();


                        #region Output

                        string strLocID = string.Empty;

                        DataTable dtOUT_UDR_OUT_STATUS = new DataTable();

                        dtOUT_UDR_OUT_STATUS.Columns.Add("SRC_LOCID", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("JOB", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("SKIDID", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("SRC_STATUS", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("SRC_MODE", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_STATUS_11", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_MODE_11", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_STATUS_12", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_MODE_12", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_STATUS_13", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_MODE_13", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_STATUS_14", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_MODE_14", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("CURR_STOCK_RATIO", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("AVAIL_QTY", typeof(string));

                        DataRow drNew = null;
                        int iStep = 0;

                        foreach (DataRow row in resultSet.Tables["OUT_UDR_OUT_STATUS"].Rows)
                        {
                            if (row["SRC_LOCID"].Equals(strLocID))
                            {
                                switch (iStep)
                                {
                                    case 1:
                                        drNew["DST_STATUS_12"] = row["DST_STATUS"].ToString();
                                        drNew["DST_MODE_12"] = row["DST_MODE"].ToString();

                                        dgUnLoaderOut_2.Columns["DST_STATUS_12"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "S" };
                                        dgUnLoaderOut_2.Columns["DST_MODE_12"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "M" };
                                        ++iStep;
                                        break;
                                    case 2:
                                        drNew["DST_STATUS_13"] = row["DST_STATUS"].ToString();
                                        drNew["DST_MODE_13"] = row["DST_MODE"].ToString();

                                        dgUnLoaderOut_2.Columns["DST_STATUS_13"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "S" };
                                        dgUnLoaderOut_2.Columns["DST_MODE_13"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "M" };
                                        ++iStep;
                                        break;
                                    case 3:
                                        drNew["DST_STATUS_14"] = row["DST_STATUS"].ToString();
                                        drNew["DST_MODE_14"] = row["DST_MODE"].ToString();

                                        dgUnLoaderOut_2.Columns["DST_STATUS_14"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "S" };
                                        dgUnLoaderOut_2.Columns["DST_MODE_14"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "M" };
                                        ++iStep; // display를 위해 사용
                                        break;
                                }

                                continue;
                            }
                            else
                            {
                                if (drNew != null)
                                {
                                    dtOUT_UDR_OUT_STATUS.Rows.Add(drNew);
                                    iStep = 0;
                                }

                                strLocID = row["SRC_LOCID"].ToString();

                                drNew = dtOUT_UDR_OUT_STATUS.NewRow();
                                drNew["SRC_LOCID"] = row["SRC_LOCID"].ToString().Substring(8, 4);
                                drNew["JOB"] = row["JOB"].ToString();
                                drNew["SKIDID"] = row["SKIDID"].ToString();
                                drNew["SRC_STATUS"] = row["SRC_STATUS"].ToString().Trim();
                                drNew["SRC_MODE"] = row["SRC_MODE"].ToString();

                                drNew["DST_STATUS_11"] = row["DST_STATUS"].ToString();
                                drNew["DST_MODE_11"] = row["DST_MODE"].ToString();

                                txtUnloderOut_2.Text = row["SRC_LOCID"].ToString().Substring(0, 8);

                                dgUnLoaderOut_2.Columns["DST_STATUS_11"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "S" };
                                dgUnLoaderOut_2.Columns["DST_MODE_11"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "M" };


                                if (selectedEquipmentSegment == "LAM")
                                {
                                    dgUnLoaderOut_2.Columns["SKIDID"].Header = new List<string>() { "From", "LAM", "Skid" };
                                    dgUnLoaderOut_2.Columns["SRC_STATUS"].Header = new List<string>() { "From", "LAM", "S" };
                                    dgUnLoaderOut_2.Columns["SRC_MODE"].Header = new List<string>() { "From", "LAM", "M" };
                                }
                                else
                                {
                                    dgUnLoaderOut_2.Columns["SKIDID"].Header = new List<string>() { "From", "NND", "Skid" };
                                    dgUnLoaderOut_2.Columns["SRC_STATUS"].Header = new List<string>() { "From", "NND", "S" };
                                    dgUnLoaderOut_2.Columns["SRC_MODE"].Header = new List<string>() { "From", "NND", "M" };
                                }



                                if (resultSet.Tables["OUT_UDR_OUT_STK_INFO"].Rows.Count != 0)
                                {
                                    drNew["CURR_STOCK_RATIO"] = resultSet.Tables["OUT_UDR_OUT_STK_INFO"].Rows[0]["CURR_STOCK_RATIO"];
                                    drNew["AVAIL_QTY"] = resultSet.Tables["OUT_UDR_OUT_STK_INFO"].Rows[0]["AVAIL_QTY"];


                                    dgUnLoaderOut_2.Columns["CURR_STOCK_RATIO"].Header = new List<string>() { "STK (" + resultSet.Tables["OUT_UDR_OUT_STK_INFO"].Rows[0]["EQPTID"].ToString().Substring(5, 3) + ")", "STK (" + resultSet.Tables["OUT_UDR_OUT_STK_INFO"].Rows[0]["EQPTID"].ToString().Substring(5, 3) + ")", "RATE" };
                                    dgUnLoaderOut_2.Columns["AVAIL_QTY"].Header = new List<string>() { "STK (" + resultSet.Tables["OUT_UDR_OUT_STK_INFO"].Rows[0]["EQPTID"].ToString().Substring(5, 3) + ")", "STK (" + resultSet.Tables["OUT_UDR_OUT_STK_INFO"].Rows[0]["EQPTID"].ToString().Substring(5, 3) + ")", "QTY" };
                                }
                                else
                                {
                                    dgUnLoaderOut_2.Columns["CURR_STOCK_RATIO"].Header = new List<string>() { "STK", "STK", "RATE" };
                                    dgUnLoaderOut_2.Columns["AVAIL_QTY"].Header = new List<string>() { "STK", "STK", "QTY" };
                                }

                                ++iStep;
                            }
                        }

                        if (drNew != null)
                        {
                            dtOUT_UDR_OUT_STATUS.Rows.Add(drNew);
                        }

                        if (dtOUT_UDR_OUT_STATUS.Rows.Count > 0)
                        {
                            Util.GridSetData(dgUnLoaderOut_2, dtOUT_UDR_OUT_STATUS, FrameOperation, false);

                            iGgO2ColCount = dgUnLoaderOut_2.Columns.Count;
                            switch (iStep)
                            {
                                case 1:
                                    colOutStatus_2_2.Visibility = Visibility.Collapsed;
                                    colOutMode_2_2.Visibility = Visibility.Collapsed;
                                    colOutStatus_2_3.Visibility = Visibility.Collapsed;
                                    colOutMode_2_3.Visibility = Visibility.Collapsed;
                                    colOutStatus_2_4.Visibility = Visibility.Collapsed;
                                    colOutMode_2_4.Visibility = Visibility.Collapsed;
                                    iGgO2ColCount = dgUnLoaderOut_2.Columns.Count - 6;
                                    break;
                                case 2:
                                    colOutStatus_2_3.Visibility = Visibility.Collapsed;
                                    colOutMode_2_3.Visibility = Visibility.Collapsed;
                                    colOutStatus_2_4.Visibility = Visibility.Collapsed;
                                    colOutMode_2_4.Visibility = Visibility.Collapsed;
                                    iGgO2ColCount = dgUnLoaderOut_2.Columns.Count - 4;
                                    break;
                                case 3:
                                    colOutStatus_2_4.Visibility = Visibility.Collapsed;
                                    colOutMode_2_4.Visibility = Visibility.Collapsed;
                                    iGgO2ColCount = dgUnLoaderOut_2.Columns.Count - 2;
                                    break;
                            }

                            InitColumnWidth(3, iColumnCount);
                        }
                        #endregion


                        #region Input

                        strLocID = string.Empty;

                        DataTable dtOUT_UDR_IN_STATUS = new DataTable();

                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_LOCID", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("JOB", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("SKIDID", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("SRC_STATUS", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("SRC_MODE", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_STATUS_21", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_MODE_21", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_STATUS_22", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_MODE_22", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_STATUS_23", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_MODE_23", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_STATUS_24", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_MODE_24", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("E_SKID_CNT", typeof(string));

                        drNew = null;
                        iStep = 0;

                        foreach (DataRow row in resultSet.Tables["OUT_UDR_IN_STATUS"].Rows)
                        {
                            if (row["DST_LOCID"].Equals(strLocID))
                            {
                                switch (iStep)
                                {
                                    case 1:
                                        drNew["DST_STATUS_22"] = row["SRC_STATUS"].ToString();
                                        drNew["DST_MODE_22"] = row["SRC_MODE"].ToString();

                                        dgUnLoaderIn_2.Columns["DST_STATUS_22"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "S" };
                                        dgUnLoaderIn_2.Columns["DST_MODE_22"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "M" };
                                        ++iStep;
                                        break;
                                    case 2:
                                        drNew["DST_STATUS_23"] = row["SRC_STATUS"].ToString();
                                        drNew["DST_MODE_23"] = row["SRC_MODE"].ToString();

                                        dgUnLoaderIn_2.Columns["DST_STATUS_23"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "S" };
                                        dgUnLoaderIn_2.Columns["DST_MODE_23"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "M" };
                                        ++iStep;
                                        break;
                                    case 3:
                                        drNew["DST_STATUS_24"] = row["SRC_STATUS"].ToString();
                                        drNew["DST_MODE_24"] = row["SRC_MODE"].ToString();

                                        dgUnLoaderIn_2.Columns["DST_STATUS_24"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "S" };
                                        dgUnLoaderIn_2.Columns["DST_MODE_24"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "M" };
                                        ++iStep; // display를 위해 사용
                                        break;
                                }

                                continue;
                            }
                            else
                            {
                                if (drNew != null)
                                {
                                    dtOUT_UDR_IN_STATUS.Rows.Add(drNew);
                                    iStep = 0;
                                }

                                strLocID = row["DST_LOCID"].ToString();

                                drNew = dtOUT_UDR_IN_STATUS.NewRow();
                                drNew["DST_LOCID"] = row["DST_LOCID"].ToString().Substring(8, 4);
                                drNew["JOB"] = row["JOB"].ToString();
                                drNew["SKIDID"] = row["SKIDID"].ToString();
                                drNew["SRC_STATUS"] = row["DST_STATUS"].ToString().Trim();
                                drNew["SRC_MODE"] = row["DST_MODE"].ToString();

                                drNew["DST_STATUS_21"] = row["SRC_STATUS"].ToString();
                                drNew["DST_MODE_21"] = row["SRC_MODE"].ToString();

                                txtUnloderIn_2.Text = row["DST_LOCID"].ToString().Substring(0, 8);


                                if (selectedEquipmentSegment == "LAM")
                                {
                                    dgUnLoaderIn_2.Columns["SKIDID"].Header = new List<string>() { "To", "LAM", "Skid" };
                                    dgUnLoaderIn_2.Columns["SRC_STATUS"].Header = new List<string>() { "To", "LAM", "S" };
                                    dgUnLoaderIn_2.Columns["SRC_MODE"].Header = new List<string>() { "To", "LAM", "M" };
                                }
                                else
                                {
                                    dgUnLoaderIn_2.Columns["SKIDID"].Header = new List<string>() { "To", "NND", "Skid" };
                                    dgUnLoaderIn_2.Columns["SRC_STATUS"].Header = new List<string>() { "To", "NND", "S" };
                                    dgUnLoaderIn_2.Columns["SRC_MODE"].Header = new List<string>() { "To", "NND", "M" };
                                }

                                string[] sColumnName = new string[] { "E_SKID_CNT" };
                                if (selectedEquipmentSegment == "NTC")
                                {
                                    _Util.SetDataGridMergeExtensionCol(dgUnLoaderIn_2, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                                }
                                else
                                {
                                    _Util.SetDataGridMergeExtensionCol(dgUnLoaderIn_2, sColumnName, DataGridMergeMode.NONE);
                                }

                                dgUnLoaderIn_2.Columns["DST_STATUS_21"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "S" };
                                dgUnLoaderIn_2.Columns["DST_MODE_21"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "M" };

                                if (resultSet.Tables["OUT_UDR_IN_STK_INFO"].Rows.Count != 0)
                                {
                                    drNew["E_SKID_CNT"] = resultSet.Tables["OUT_UDR_IN_STK_INFO"].Rows[0]["E_SKID_CNT"];

                                    dgUnLoaderIn_2.Columns["E_SKID_CNT"].Header = new List<string>() { "STK(" + resultSet.Tables["OUT_UDR_IN_STK_INFO"].Rows[0]["EQPTID"].ToString().Substring(5,3)+")", "STK(" + resultSet.Tables["OUT_UDR_IN_STK_INFO"].Rows[0]["EQPTID"].ToString().Substring(5, 3) + ")", "QTY" };
                                }
                                else
                                {
                                    dgUnLoaderIn_2.Columns["E_SKID_CNT"].Header = new List<string>() { "STK", "STK", "" };
                                }

                                ++iStep;
                            }
                        }

                        if (drNew != null)
                        {
                            dtOUT_UDR_IN_STATUS.Rows.Add(drNew);
                        }

                        if (dtOUT_UDR_IN_STATUS.Rows.Count > 0)
                        {
                            Util.GridSetData(dgUnLoaderIn_2, dtOUT_UDR_IN_STATUS, FrameOperation, false);

                            iGgI2ColCount = dgUnLoaderIn_2.Columns.Count;
                            switch (iStep)
                            {
                                case 1:
                                    colInStatus_2_2.Visibility = Visibility.Collapsed;
                                    colInMode_2_2.Visibility = Visibility.Collapsed;
                                    colInStatus_2_3.Visibility = Visibility.Collapsed;
                                    colInMode_2_3.Visibility = Visibility.Collapsed;
                                    colInStatus_2_4.Visibility = Visibility.Collapsed;
                                    colInMode_2_4.Visibility = Visibility.Collapsed;
                                    iGgI2ColCount = dgUnLoaderIn_2.Columns.Count - 6;
                                    break;
                                case 2:
                                    colInStatus_2_3.Visibility = Visibility.Collapsed;
                                    colInMode_2_3.Visibility = Visibility.Collapsed;
                                    colInStatus_2_4.Visibility = Visibility.Collapsed;
                                    colInMode_2_4.Visibility = Visibility.Collapsed;
                                    iGgI2ColCount = dgUnLoaderIn_2.Columns.Count - 4;
                                    break;
                                case 3:
                                    colInStatus_2_4.Visibility = Visibility.Collapsed;
                                    colInMode_2_4.Visibility = Visibility.Collapsed;
                                    iGgI2ColCount = dgUnLoaderIn_2.Columns.Count - 2;
                                    break;
                            }

                            InitColumnWidth(4, iColumnCount);
                        }
                        #endregion
                    }
                    #endregion
                }

                if (selectedEquipment_3 != "")
                {
                    #region set - 3

                    DataSet ds = new DataSet();

                    DataTable indata = new DataTable();
                    indata.TableName = "IN_INFORM";
                    indata.Columns.Add("EQPT_GROUP", typeof(string));
                    indata.Columns.Add("EQPTID", typeof(string));

                    DataRow dr = indata.NewRow();
                    dr["EQPT_GROUP"] = selectedEquipmentSegment;
                    dr["EQPTID"] = selectedEquipment_3;
                    indata.Rows.Add(dr);

                    ds.Tables.Add(indata);

                    DataSet resultSet = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync_Multi("BR_GET_UDR_LDR_MNTR_INFO", "IN_INFORM", "OUT_UDR_IN_EQPT_INFO,OUT_UDR_IN_STATUS,OUT_UDR_IN_STK_INFO,OUT_UDR_OUT_EQPT_INFO,OUT_UDR_OUT_STATUS,OUT_UDR_OUT_STK_INFO,OUT_LDR_CONN_STK_INFO", ds);

                    if (resultSet != null)
                    {
                        if (selectedEquipmentSegment.Equals("NTC"))
                        {
                            tbTitle_3.Text = resultSet.Tables["OUT_UDR_OUT_EQPT_INFO"].Rows[0]["EQPT_NAME"].ToString();
                            Util.GridSetData(dgLoader_3, resultSet.Tables["OUT_LDR_CONN_STK_INFO"], FrameOperation, false);
                        }

                        tbTitUnLoaderOut_3.Text = resultSet.Tables["OUT_UDR_IN_EQPT_INFO"].Rows[0]["EQPT_NAME"].ToString();


                        #region Output

                        string strLocID = string.Empty;

                        DataTable dtOUT_UDR_OUT_STATUS = new DataTable();

                        dtOUT_UDR_OUT_STATUS.Columns.Add("SRC_LOCID", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("JOB", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("SKIDID", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("SRC_STATUS", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("SRC_MODE", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_STATUS_11", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_MODE_11", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_STATUS_12", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_MODE_12", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_STATUS_13", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_MODE_13", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_STATUS_14", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("DST_MODE_14", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("CURR_STOCK_RATIO", typeof(string));
                        dtOUT_UDR_OUT_STATUS.Columns.Add("AVAIL_QTY", typeof(string));

                        DataRow drNew = null;
                        int iStep = 0;

                        foreach (DataRow row in resultSet.Tables["OUT_UDR_OUT_STATUS"].Rows)
                        {
                            if (row["SRC_LOCID"].Equals(strLocID))
                            {
                                switch (iStep)
                                {
                                    case 1:
                                        drNew["DST_STATUS_12"] = row["DST_STATUS"].ToString();
                                        drNew["DST_MODE_12"] = row["DST_MODE"].ToString();

                                        dgUnLoaderOut_3.Columns["DST_STATUS_12"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "S" };
                                        dgUnLoaderOut_3.Columns["DST_MODE_12"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "M" };
                                        ++iStep;
                                        break;
                                    case 2:
                                        drNew["DST_STATUS_13"] = row["DST_STATUS"].ToString();
                                        drNew["DST_MODE_13"] = row["DST_MODE"].ToString();

                                        dgUnLoaderOut_3.Columns["DST_STATUS_13"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "S" };
                                        dgUnLoaderOut_3.Columns["DST_MODE_13"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "M" };
                                        ++iStep;
                                        break;
                                    case 3:
                                        drNew["DST_STATUS_14"] = row["DST_STATUS"].ToString();
                                        drNew["DST_MODE_14"] = row["DST_MODE"].ToString();

                                        dgUnLoaderOut_3.Columns["DST_STATUS_14"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "S" };
                                        dgUnLoaderOut_3.Columns["DST_MODE_14"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "M" };
                                        ++iStep; // display를 위해 사용
                                        break;
                                }

                                continue;
                            }
                            else
                            {
                                if (drNew != null)
                                {
                                    dtOUT_UDR_OUT_STATUS.Rows.Add(drNew);
                                    iStep = 0;
                                }

                                strLocID = row["SRC_LOCID"].ToString();

                                drNew = dtOUT_UDR_OUT_STATUS.NewRow();
                                drNew["SRC_LOCID"] = row["SRC_LOCID"].ToString().Substring(8, 4);
                                drNew["JOB"] = row["JOB"].ToString();
                                drNew["SKIDID"] = row["SKIDID"].ToString();
                                drNew["SRC_STATUS"] = row["SRC_STATUS"].ToString().Trim();
                                drNew["SRC_MODE"] = row["SRC_MODE"].ToString();

                                drNew["DST_STATUS_11"] = row["DST_STATUS"].ToString();
                                drNew["DST_MODE_11"] = row["DST_MODE"].ToString();

                                txtUnloderOut_3.Text = row["SRC_LOCID"].ToString().Substring(0, 8);

                                dgUnLoaderOut_3.Columns["DST_STATUS_11"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "S" };
                                dgUnLoaderOut_3.Columns["DST_MODE_11"].Header = new List<string>() { "To", row["DST_LOCID"].ToString().Substring(8, 4), "M" };

                                if (selectedEquipmentSegment == "LAM")
                                {
                                    dgUnLoaderOut_3.Columns["SKIDID"].Header = new List<string>() { "From", "LAM", "Skid" };
                                    dgUnLoaderOut_3.Columns["SRC_STATUS"].Header = new List<string>() { "From", "LAM", "S" };
                                    dgUnLoaderOut_3.Columns["SRC_MODE"].Header = new List<string>() { "From", "LAM", "M" };
                                }
                                else
                                {
                                    dgUnLoaderOut_3.Columns["SKIDID"].Header = new List<string>() { "From", "NND", "Skid" };
                                    dgUnLoaderOut_3.Columns["SRC_STATUS"].Header = new List<string>() { "From", "NND", "S" };
                                    dgUnLoaderOut_3.Columns["SRC_MODE"].Header = new List<string>() { "From", "NND", "M" };
                                }


                                if (resultSet.Tables["OUT_UDR_OUT_STK_INFO"].Rows.Count != 0)
                                {
                                    drNew["CURR_STOCK_RATIO"] = resultSet.Tables["OUT_UDR_OUT_STK_INFO"].Rows[0]["CURR_STOCK_RATIO"];
                                    drNew["AVAIL_QTY"] = resultSet.Tables["OUT_UDR_OUT_STK_INFO"].Rows[0]["AVAIL_QTY"];

                                    dgUnLoaderOut_3.Columns["CURR_STOCK_RATIO"].Header = new List<string>() { "STK (" + resultSet.Tables["OUT_UDR_OUT_STK_INFO"].Rows[0]["EQPTID"].ToString().Substring(5, 3) + ")", "STK (" + resultSet.Tables["OUT_UDR_OUT_STK_INFO"].Rows[0]["EQPTID"].ToString().Substring(5, 3) + ")", "RATE" };
                                    dgUnLoaderOut_3.Columns["AVAIL_QTY"].Header = new List<string>() { "STK (" + resultSet.Tables["OUT_UDR_OUT_STK_INFO"].Rows[0]["EQPTID"].ToString().Substring(5, 3) + ")", "STK (" + resultSet.Tables["OUT_UDR_OUT_STK_INFO"].Rows[0]["EQPTID"].ToString().Substring(5, 3) + ")", "QTY" };
                                }
                                else
                                {
                                    dgUnLoaderOut_3.Columns["CURR_STOCK_RATIO"].Header = new List<string>() { "STK", "STK", "RATE" };
                                    dgUnLoaderOut_3.Columns["AVAIL_QTY"].Header = new List<string>() { "STK", "STK", "QTY" };
                                }

                                ++iStep;
                            }
                        }

                        if (drNew != null)
                        {
                            dtOUT_UDR_OUT_STATUS.Rows.Add(drNew);
                        }

                        if (dtOUT_UDR_OUT_STATUS.Rows.Count > 0)
                        {
                            Util.GridSetData(dgUnLoaderOut_3, dtOUT_UDR_OUT_STATUS, FrameOperation, false);

                            iGgO3ColCount = dgUnLoaderOut_3.Columns.Count;
                            switch (iStep)
                            {
                                case 1:
                                    colOutStatus_3_2.Visibility = Visibility.Collapsed;
                                    colOutMode_3_2.Visibility = Visibility.Collapsed;
                                    colOutStatus_3_3.Visibility = Visibility.Collapsed;
                                    colOutMode_3_3.Visibility = Visibility.Collapsed;
                                    colOutStatus_3_4.Visibility = Visibility.Collapsed;
                                    colOutMode_3_4.Visibility = Visibility.Collapsed;
                                    iGgO3ColCount = dgUnLoaderOut_3.Columns.Count - 6;
                                    break;
                                case 2:
                                    colOutStatus_3_3.Visibility = Visibility.Collapsed;
                                    colOutMode_3_3.Visibility = Visibility.Collapsed;
                                    colOutStatus_3_4.Visibility = Visibility.Collapsed;
                                    colOutMode_3_4.Visibility = Visibility.Collapsed;
                                    iGgO3ColCount = dgUnLoaderOut_3.Columns.Count - 4;
                                    break;
                                case 3:
                                    colOutStatus_3_4.Visibility = Visibility.Collapsed;
                                    colOutMode_3_4.Visibility = Visibility.Collapsed;
                                    iGgO3ColCount = dgUnLoaderOut_3.Columns.Count - 2;
                                    break;
                            }

                            InitColumnWidth(5, iColumnCount);
                        }
                        #endregion


                        #region Input

                        strLocID = string.Empty;

                        DataTable dtOUT_UDR_IN_STATUS = new DataTable();

                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_LOCID", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("JOB", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("SKIDID", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("SRC_STATUS", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("SRC_MODE", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_STATUS_21", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_MODE_21", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_STATUS_22", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_MODE_22", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_STATUS_23", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_MODE_23", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_STATUS_24", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("DST_MODE_24", typeof(string));
                        dtOUT_UDR_IN_STATUS.Columns.Add("E_SKID_CNT", typeof(string));

                        drNew = null;
                        iStep = 0;

                        foreach (DataRow row in resultSet.Tables["OUT_UDR_IN_STATUS"].Rows)
                        {
                            if (row["DST_LOCID"].Equals(strLocID))
                            {
                                switch (iStep)
                                {
                                    case 1:
                                        drNew["DST_STATUS_22"] = row["SRC_STATUS"].ToString();
                                        drNew["DST_MODE_22"] = row["SRC_MODE"].ToString();

                                        dgUnLoaderIn_3.Columns["DST_STATUS_22"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "S" };
                                        dgUnLoaderIn_3.Columns["DST_MODE_22"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "M" };
                                        ++iStep;
                                        break;
                                    case 2:
                                        drNew["DST_STATUS_23"] = row["SRC_STATUS"].ToString();
                                        drNew["DST_MODE_23"] = row["SRC_MODE"].ToString();

                                        dgUnLoaderIn_3.Columns["DST_STATUS_23"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "S" };
                                        dgUnLoaderIn_3.Columns["DST_MODE_23"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "M" };
                                        ++iStep;
                                        break;
                                    case 3:
                                        drNew["DST_STATUS_24"] = row["SRC_STATUS"].ToString();
                                        drNew["DST_MODE_24"] = row["SRC_MODE"].ToString();

                                        dgUnLoaderIn_3.Columns["DST_STATUS_24"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "S" };
                                        dgUnLoaderIn_3.Columns["DST_MODE_24"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "M" };
                                        ++iStep; // display를 위해 사용
                                        break;
                                }

                                continue;
                            }
                            else
                            {
                                if (drNew != null)
                                {
                                    dtOUT_UDR_IN_STATUS.Rows.Add(drNew);
                                    iStep = 0;
                                }

                                strLocID = row["DST_LOCID"].ToString();

                                drNew = dtOUT_UDR_IN_STATUS.NewRow();
                                drNew["DST_LOCID"] = row["DST_LOCID"].ToString().Substring(8, 4);
                                drNew["JOB"] = row["JOB"].ToString();
                                drNew["SKIDID"] = row["SKIDID"].ToString();
                                drNew["SRC_STATUS"] = row["DST_STATUS"].ToString().Trim();
                                drNew["SRC_MODE"] = row["DST_MODE"].ToString();

                                drNew["DST_STATUS_21"] = row["SRC_STATUS"].ToString();
                                drNew["DST_MODE_21"] = row["SRC_MODE"].ToString();

                                txtUnloderIn_3.Text = row["DST_LOCID"].ToString().Substring(0, 8);

                                if (selectedEquipmentSegment == "LAM")
                                {
                                    dgUnLoaderIn_3.Columns["SKIDID"].Header = new List<string>() { "To", "LAM", "Skid" };
                                    dgUnLoaderIn_3.Columns["SRC_STATUS"].Header = new List<string>() { "To", "LAM", "S" };
                                    dgUnLoaderIn_3.Columns["SRC_MODE"].Header = new List<string>() { "To", "LAM", "M" };
                                }
                                else
                                {
                                    dgUnLoaderIn_3.Columns["SKIDID"].Header = new List<string>() { "To", "NND", "Skid" };
                                    dgUnLoaderIn_3.Columns["SRC_STATUS"].Header = new List<string>() { "To", "NND", "S" };
                                    dgUnLoaderIn_3.Columns["SRC_MODE"].Header = new List<string>() { "To", "NND", "M" };
                                }

                                string[] sColumnName = new string[] { "E_SKID_CNT" };
                                if (selectedEquipmentSegment == "NTC")
                                {
                                    _Util.SetDataGridMergeExtensionCol(dgUnLoaderIn_3, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                                }
                                else
                                {
                                    _Util.SetDataGridMergeExtensionCol(dgUnLoaderIn_3, sColumnName, DataGridMergeMode.NONE);
                                }

                                dgUnLoaderIn_3.Columns["DST_STATUS_21"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "S" };
                                dgUnLoaderIn_3.Columns["DST_MODE_21"].Header = new List<string>() { "From", row["SRC_LOCID"].ToString().Substring(8, 4), "M" };

                                if (resultSet.Tables["OUT_UDR_IN_STK_INFO"].Rows.Count != 0)
                                {
                                    drNew["E_SKID_CNT"] = resultSet.Tables["OUT_UDR_IN_STK_INFO"].Rows[0]["E_SKID_CNT"];

                                    dgUnLoaderIn_3.Columns["E_SKID_CNT"].Header = new List<string>() { "STK("+ resultSet.Tables["OUT_UDR_IN_STK_INFO"].Rows[0]["EQPTID"].ToString().Substring(5,3)+")", "STK(" + resultSet.Tables["OUT_UDR_IN_STK_INFO"].Rows[0]["EQPTID"].ToString().Substring(5, 3) + ")", "QTY" };
                                }
                                else
                                {
                                    dgUnLoaderIn_3.Columns["E_SKID_CNT"].Header = new List<string>() { "STK", "STK", "" };
                                }

                                ++iStep;
                            }
                        }

                        if (drNew != null)
                        {
                            dtOUT_UDR_IN_STATUS.Rows.Add(drNew);
                        }

                        if (dtOUT_UDR_IN_STATUS.Rows.Count > 0)
                        {
                            Util.GridSetData(dgUnLoaderIn_3, dtOUT_UDR_IN_STATUS, FrameOperation, false);

                            iGgI3ColCount = dgUnLoaderIn_3.Columns.Count;
                            switch (iStep)
                            {
                                case 1:
                                    colInStatus_3_2.Visibility = Visibility.Collapsed;
                                    colInMode_3_2.Visibility = Visibility.Collapsed;
                                    colInStatus_3_3.Visibility = Visibility.Collapsed;
                                    colInMode_3_3.Visibility = Visibility.Collapsed;
                                    colInStatus_3_4.Visibility = Visibility.Collapsed;
                                    colInMode_3_4.Visibility = Visibility.Collapsed;
                                    iGgI3ColCount = dgUnLoaderIn_3.Columns.Count - 6;
                                    break;
                                case 2:
                                    colInStatus_3_3.Visibility = Visibility.Collapsed;
                                    colInMode_3_3.Visibility = Visibility.Collapsed;
                                    colInStatus_3_4.Visibility = Visibility.Collapsed;
                                    colInMode_3_4.Visibility = Visibility.Collapsed;
                                    iGgI3ColCount = dgUnLoaderIn_3.Columns.Count - 4;
                                    break;
                                case 3:
                                    colInStatus_3_4.Visibility = Visibility.Collapsed;
                                    colInMode_3_4.Visibility = Visibility.Collapsed;
                                    iGgI3ColCount = dgUnLoaderIn_3.Columns.Count - 2;
                                    break;
                            }

                            InitColumnWidth(6, iColumnCount);
                        }
                        #endregion
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

            if (e.Cell.Column.Name != null)
            {
                if (e.Cell.Column.Name.Equals("EQPTID")
                    || e.Cell.Column.Name.Equals("WOID")
                    || e.Cell.Column.Name.Equals("PRJT_NAME")
                    || e.Cell.Column.Name.Equals("VER")
                    || e.Cell.Column.Name.Equals("LINK_COT")
                    || e.Cell.Column.Name.Equals("STOCK_CNT")
                    || e.Cell.Column.Name.Equals("SRC_LOCID")
                    || e.Cell.Column.Name.Equals("SKIDID")
                    || e.Cell.Column.Name.Equals("AVAIL_QTY")
                    || e.Cell.Column.Name.Equals("DST_LOCID"))
                {
                    if (e.Cell.Row.DataItem != null)
                    {
                        e.Cell.Presenter.FontSize = numFontSize.Value;
                    }
                }

                else if (e.Cell.Column.Name.Equals("JOB"))
                {
                    if (e.Cell.Row.DataItem != null)
                    {
                        if ((Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)) == "OK"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                        }
                        else if ((Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)) == "NG"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        }

                        e.Cell.Presenter.FontSize = numFontSize.Value;
                    }
                }

                else if ((e.Cell.Column.Name.Equals("SRC_STATUS")) || (e.Cell.Column.Name.Equals("SRC_MODE"))
                    || (e.Cell.Column.Name.Equals("DST_STATUS_11"))
                    || (e.Cell.Column.Name.Equals("DST_STATUS_12"))
                    || (e.Cell.Column.Name.Equals("DST_STATUS_13"))
                    || (e.Cell.Column.Name.Equals("DST_STATUS_14"))
                    || (e.Cell.Column.Name.Equals("DST_MODE_11"))
                    || (e.Cell.Column.Name.Equals("DST_MODE_12"))
                    || (e.Cell.Column.Name.Equals("DST_MODE_13"))
                    || (e.Cell.Column.Name.Equals("DST_MODE_14"))
                    || (e.Cell.Column.Name.Equals("DST_STATUS_21"))
                    || (e.Cell.Column.Name.Equals("DST_STATUS_22"))
                    || (e.Cell.Column.Name.Equals("DST_STATUS_23"))
                    || (e.Cell.Column.Name.Equals("DST_STATUS_24"))
                    || (e.Cell.Column.Name.Equals("DST_MODE_21"))
                    || (e.Cell.Column.Name.Equals("DST_MODE_22"))
                    || (e.Cell.Column.Name.Equals("DST_MODE_23"))
                    || (e.Cell.Column.Name.Equals("DST_MODE_24"))
                    )
                {
                    if (e.Cell.Row.DataItem != null)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)) == "O")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                        }
                        else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)) != "")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontSize = numFontSize.Value;
                        }
                    }
                }

                else if (e.Cell.Column.Name.Equals("CURR_STOCK_RATIO"))
                {
                    if (e.Cell.Row.DataItem != null)
                    {
                        double dRate = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)) == "" ? 0 : Double.Parse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)).Replace("%", ""));


                        if (dRate >= 90)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                        else if (dRate >= 80)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);
                        }

                        if (dRate != 0)
                        {
                            DataTableConverter.SetValue(e.Cell.Row.DataItem, e.Cell.Column.Name, dRate + "%");
                            e.Cell.Presenter.FontSize = numFontSize.Value;
                        }
                    }
                }

                else if (e.Cell.Column.Name.Equals("E_SKID_CNT"))
                {
                    if (e.Cell.Row.DataItem != null)
                    {
                        int iCount = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)) == "" ? 0 : Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name));


                        if (iCount == 0)
                        {
                            DataTableConverter.SetValue(e.Cell.Row.DataItem, e.Cell.Column.Name, 0);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                        else if (iCount <= 5)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }

                        e.Cell.Presenter.FontSize = numFontSize.Value;
                    }
                }
            }
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


        #region Setting

        void dgLoader_MergingCells(object sender, DataGridMergingCellsEventArgs e)
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

            dgLoader_1.MergingCells -= dgLoader_MergingCells;
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

        #endregion

        private void btnSetSave_Click(object sender, RoutedEventArgs e)
        {
           
            selectedEquipmentSegment = Convert.ToString(cboEquipmentSegment.SelectedValue);
            selectedEquipment_1 = Convert.ToString(cboEquipment_1.SelectedValue);
            selectedEquipment_2 = Convert.ToString(cboEquipment_2.SelectedValue);
            selectedEquipment_3 = Convert.ToString(cboEquipment_3.SelectedValue);
            selectedDisplayTime = Convert.ToInt32(numRefresh.Value);
            DataSet ds = new DataSet();

            DataTable dt = new DataTable("MNT_SETTING");
            dt.Columns.Add("EQUIPMENTSEGMENT", typeof(string));
            dt.Columns.Add("EQUIPMENT_1", typeof(string));
            dt.Columns.Add("EQUIPMENT_2", typeof(string));
            dt.Columns.Add("EQUIPMENT_3", typeof(string));
            dt.Columns.Add("DISPLAYTIME", typeof(int));
          
            DataRow dr = dt.NewRow();
            dr["EQUIPMENTSEGMENT"] = selectedEquipmentSegment;
            dr["EQUIPMENT_1"] = selectedEquipment_1;
            dr["EQUIPMENT_2"] = selectedEquipment_2;
            dr["EQUIPMENT_3"] = selectedEquipment_3;
            dr["DISPLAYTIME"] = selectedDisplayTime;
         
            dt.Rows.Add(dr);
            ds.Tables.Add(dt);

            LGC.GMES.MES.MNT001.MNT_Common.SetConfigXML_LDR_UDR(ds);
            btnExecute_Click(null, null);


            btnLeftFrame.IsChecked = false;
        }



    }
}
