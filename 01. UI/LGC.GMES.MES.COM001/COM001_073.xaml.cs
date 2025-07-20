/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace LGC.GMES.MES.COM001
{
    public partial class COM001_073 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        //Boolean bLoadComplete = false;
        public class ResultElement
        {
            public RadioButton radButton = null;
            public bool Visibility = true;
            public int SpaceInCharge = 1;
        }

        // 모니터링용 타이머
        private System.Windows.Threading.DispatcherTimer _timer = new System.Windows.Threading.DispatcherTimer();

        public COM001_073()
        {
            InitializeComponent();
            //this.Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void TimerSetting()
        {
            _timer.Interval = new TimeSpan(0, 0, 0, 60);
            _timer.Tick += new EventHandler(timer_Tick);
            _timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                btnRefresh_Click(null,null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            Search_Store();
            Search_Status();
            _timer.Start();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
  
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            _timer.Start();
        }

        private void dgList_Initialized(object sender, EventArgs e)
        {
            //Initialize();
            //TimerSetting();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            Initialize();
            TimerSetting();
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            rdoPanC.Tag = "C"; //양극
            rdoPanA.Tag = "A"; //음극
            
            rdoPanC.IsChecked = true; //초기화시 양극버튼을 눌러줌.
            
            Search_Store();           //
        }

        

        void SetResult(List<ResultElement> elementList, Grid grid)
        {
            int elementCol = 0;
            grid.SetValue(Grid.VerticalAlignmentProperty, VerticalAlignment.Bottom);
            int colIndex = 0;

            foreach (ResultElement re in elementList)
            {
                if (re.radButton != null) {
                    re.radButton.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    re.radButton.Margin = new Thickness(10, 0, 5, 0);
                    if (elementCol == 0)
                        re.radButton.IsChecked = true;
                    elementCol++;
                    re.radButton.SetValue(Grid.ColumnProperty, elementCol);
                    re.radButton.Checked += RadioButton_Checked;
                    Area.Children.Add(re.radButton);
                }
                colIndex += re.SpaceInCharge;
            }
        }
        #endregion

        #region Event

        /// <summary>
        /// 우상단 라디오버튼 체크시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_Checked(object sender, RoutedEventArgs e) 
        {
            RadioButton rdo = sender as RadioButton;


            if (rdo.IsChecked.Value)
            {
                Search_Status();
            }
        }

        private void _lable2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Label lblName = (Label)sender;
                        
            Label lblRack = dgList.FindName(lblName.Name.ToString().Replace("VALUE","RACK")) as Label;
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(lblRack.Content.ToString());

            //Popup창 생성
            COM001_073_RACK wndPopup = new COM001_073_RACK(lblRack.Content.ToString(), "COMM_CONV");
            wndPopup.FrameOperation = FrameOperation;
            wndPopup.Show();
        }

        private void _lable3_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Label lbName = (Label)sender;

            string sCheck = "";
            sCheck = lbName.Content.ToString();

            if (sCheck == "0")
            {
                return;
            }

            string[] sRoWCol = lbName.Tag.ToString().Split(new char[] { '-' });
            Label lblRack = dgList.FindName("RACK" + lbName.Tag) as Label;
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(lblRack.Content.ToString());
            
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            //COM001_025_RACK window = sender as COM001_025_RACK;
            //if (window.DialogResult == MessageBoxResult.OK)
            //{
            //    //GetList();
            //}
        }
            
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            COM001_073_SKDPNMAPPING wndPopup = new COM001_073_SKDPNMAPPING();
            //wndPopup.FrameOperation = FrameOperation;
            wndPopup.Show();
        }

        private void btnLotSearch_Click(object sender, RoutedEventArgs e)
        {
            //특이Lot조회
            COM001_073_UNUSUAL wndPopup = new COM001_073_UNUSUAL();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[1];
                C1WindowExtension.SetParameters(wndPopup, Parameters);
                Parameters[0] = GetWarehouseID();
                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }

            //Grid_Select("UNUSUAL", "UNUSUAL_LOT");
        }

        private void btnReInput_Click(object sender, RoutedEventArgs e)
        {
            //노칭에서 다시 재입고된 LOT_List
            COM001_073_REIN_RACK wndPopup = new COM001_073_REIN_RACK();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {

                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void btnNotInput_Click(object sender, RoutedEventArgs e)
        {
            //공정후 or 공장,Shop 이동되어 창고에 미입고된 LOT_List
            try
            {
                //Popup창 생성
                COM001_073_NOTIN_RACK wndPopup = new COM001_073_NOTIN_RACK();
                wndPopup.FrameOperation = FrameOperation;
                wndPopup.Show();

                if (wndPopup != null)
                {
                    wndPopup.Closed += new EventHandler(wndPopup_Closed);
                }
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtOutLotid.Text))
                {
                    //출고 처리할 LOT 정보가 존재하지 않습니다.
                    Util.MessageInfo("SFU3217", (Result) =>
                    {
                        return;
                    });
                    return;
                }
                #region INDATA
                DataSet inDataSet = new DataSet();
                DataTable INDATA = inDataSet.Tables.Add("INDATA"); 
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(dr);
                #endregion

                #region INLOT
                DataTable InLot = inDataSet.Tables.Add("INLOT");
                InLot.Columns.Add("LOTID", typeof(string));

                DataRow drInlot = InLot.NewRow();
                drInlot["LOTID"] = txtOutLotid.Text;
                InLot.Rows.Add(drInlot);
                #endregion       
                     
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_STOCK_OUT", "INDATA,INLOT,INRESN", null, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Information", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                       
                        return;
                    }
                    else
                    {
                        //정상처리되었습니다.
                        Util.MessageInfo("SFU1275", (Result) =>
                        {
                            SearchData();
                        });
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }

        }

        private void dgStore_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name == "GUBUN1" || e.Cell.Column.Name == "GUBUN2")
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#eaeaea"));
                    }

                }));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProcess_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name == "GUBUN1" || e.Cell.Column.Name == "GUBUN2")
                    {

                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#eaeaea"));  //<Setter Property="Background" Value="#FFF9F9F9" />
                    }

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
        } 
        private void txtModel_GotFocus(object sender, RoutedEventArgs e)
        {
            txtLotid.Text = string.Empty;
        }

        private void txtLotid_GotFocus(object sender, RoutedEventArgs e)
        {
            
            txtModel.Text = string.Empty;
        }
        #endregion

        #region Mehod

        #region 데이터 조회

        /// <summary>
        /// [창고재고] 데이터를 조회합니다.
        /// </summary>
        private void SearchStore()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;

                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_FOR_MIDWH", "INDATA", "RSLTDT", IndataTable);

                #region Customizing 창고재고
                if (dtMain.Rows.Count >= 1)
                {
                    DataTable GridData = new DataTable();
                    GridData.Columns.Add("GUBUN1", typeof(string));
                    GridData.Columns.Add("GUBUN2", typeof(string));
                    GridData.Columns.Add("CC", typeof(string));
                    //GridData.Columns.Add("CM", typeof(string));
                    GridData.Columns.Add("AC", typeof(string));
                    //GridData.Columns.Add("AM", typeof(string));

                    for (int c = 0; c < dtMain.Columns.Count; c++)
                    {
                        
                        if (c % 2 == 0)
                        {
                            string sGubun1 = "";
                            string sGubun2 = "";

                            if (c == 0)
                            { sGubun1 = ObjectDic.Instance.GetObjectName("팬케이크"); sGubun2 = ObjectDic.Instance.GetObjectName("현보관량"); }
                            if (c == 2)
                            { sGubun1 = ObjectDic.Instance.GetObjectName("팬케이크"); sGubun2 = ObjectDic.Instance.GetObjectName("적재가능수량"); }
                       
                            DataRow setData = GridData.NewRow();
                            setData["GUBUN1"] = sGubun1;
                            setData["GUBUN2"] = sGubun2;
                            setData["CC"] = dtMain.Rows[0][c].ToString();
                            setData["AC"] = dtMain.Rows[0][c + 1].ToString();
                            GridData.Rows.Add(setData);
                        }
                    }
                    dgStore.ItemsSource = DataTableConverter.Convert(GridData);
                }
                #endregion
                return;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        /// <summary>
        /// [공정재고] 데이터를 조회합니다.
        /// </summary>
        private void SearchProcess()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                //Indata["AREA"] = LoginInfo.CFG_AREA_ID;
                IndataTable.Rows.Add(Indata);

                //임시, 창고재고 데이터 조회
                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_FOR_MIDWH", "INDATA", "RSLTDT", IndataTable);
                
                #region Customizing 공정재고
                if (dtMain.Rows.Count >= 1)
                {
                    DataTable GridData = new DataTable();
                    GridData.Columns.Add("GUBUN1", typeof(string));
                    GridData.Columns.Add("GUBUN2", typeof(string));
                    GridData.Columns.Add("CC", typeof(string));
                    GridData.Columns.Add("CM", typeof(string));
                    GridData.Columns.Add("AC", typeof(string));
                    GridData.Columns.Add("AM", typeof(string));

                    for (int c = 0; c < dtMain.Columns.Count; c++)
                    {
                        if (c % 2 == 0)
                        {
                            string sGubun1 = "";
                            string sGubun2 = "";


                            sGubun1 = ObjectDic.Instance.GetObjectName("팬케이크");

                            if (c == 0)
                            {
                                sGubun2 = ObjectDic.Instance.GetObjectName("공정 대기량");
                            }
                            else if (c == 2)
                            {
                                sGubun2 = ObjectDic.Instance.GetObjectName("예약량");
                            }
                            DataRow setData = GridData.NewRow();
                            setData["GUBUN1"] = sGubun1;
                            setData["GUBUN2"] = sGubun2;
                            setData["CC"] = dtMain.Rows[0][c].ToString();
                            setData["AC"] = dtMain.Rows[0][c + 1].ToString();

                            GridData.Rows.Add(setData);
                        }
                    }
                    dgProcess.ItemsSource = DataTableConverter.Convert(GridData);
                }
                #endregion
                return;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        /// <summary>
        /// [유효기간 임박재고] 데이터를 조회합니다.
        /// </summary>
        private void SearchVLDate()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREA", typeof(string));
                

                DataRow Indata = IndataTable.NewRow();
                Indata["AREA"] = LoginInfo.CFG_AREA_ID;
                
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIDWH_STOCK_BY_ALL_VLDATE", "RQSTDT", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count >= 1)
                {
                    dgMonth.ItemsSource = DataTableConverter.Convert(dtMain);
                }
                return;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        #endregion

        private void Clear_Grid()
        {
            Util.gridClear(dgStore);
            Util.gridClear(dgProcess);
            Util.gridClear(dgMonth);
        }

        private void ClearGrid()
        {
            try
            {
                if (dgList != null)
                {
                    foreach (Label _label in dgList.Children.OfType<Label>())
                    {
                        dgList.UnregisterName(_label.Name);
                    }

                    foreach (StackPanel _stack in dgList.Children.OfType<StackPanel>())
                    {
                        dgList.UnregisterName(_stack.Name);
                    }
                }


                NameScope.SetNameScope(dgList, new NameScope());

                dgList.Children.Clear();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void Search_Store()
        {
            Clear_Grid();

            SearchStore();
            //SearchProcess();
            SearchVLDate();
        }

        private void Search_Status()
        {
            ClearGrid();
            Grid_Draw();
        }

        private string getTypeCode()
        {
            string code = string.Empty;
            foreach(Grid gd in pnlRadioButton.Children)
            {
                foreach(RadioButton rb in gd.Children)
                {
                    if (rb.IsChecked.Value)
                        code = (string)rb.Tag;
                }
            }
            return code;
        }

        private string GetWarehouseID()
        {
            /* AREAID별로 라디오버튼을 구성하여 Grid로 관리
             * 해당 Grid는 pnlRadioButton의 자식
             * Visible상태인 Grid의 라디오버튼중 체크된 것을 가져옴
             */
            string sWH_ID = string.Empty;
            
            foreach (Grid gd in pnlRadioButton.Children)
            {
                if (gd.Visibility == Visibility.Visible)
                {
                    foreach (RadioButton rb in gd.Children)
                    {
                        if (rb.IsChecked.Value)
                            sWH_ID = (string)rb.Tag;
                    }
                }
            }                      
            return sWH_ID;
        }

        private List<string> getAttribute1()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            //RQSTDT.Columns.Add("CMCODE", typeof(string)); 

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = "ko-KR";
            dr["CMCDTYPE"] = "MCS_ISS_DATE_COLR_TYPE";
            //dr["CMCODE"] = ""; //null이 되는 Input까지 입력할 경우 biz에서 데이터를 읽어들이지 못함.

            RQSTDT.Rows.Add(dr);

            DataTable rslt = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);
            
            //List Index : Sequnece, Dictionary Key : ATTRIBUTE1, Dictionary Data : ATTRIBUTE2
            var colorData = new List<string>();

            int cnt = rslt.Rows.Count;
            if (cnt >= 1)
            {
                //return할 colorData에 필요한 biz연결 데이터 입력.
                for (int i = 0; i < cnt; i++)
                {
                    colorData.Add(rslt.Rows[i]["ATTRIBUTE1"].ToString());
                }
                //UI상의 색 TextBox에 들어가는 Text
                txt1.Text = ObjectDic.Instance.GetObjectName("입고") + rslt.Rows[0]["ATTRIBUTE1"].ToString();
                txt2.Text = ObjectDic.Instance.GetObjectName("입고") + rslt.Rows[1]["ATTRIBUTE1"].ToString();
                txt3.Text = ObjectDic.Instance.GetObjectName("입고") + rslt.Rows[2]["ATTRIBUTE1"].ToString();
                txt4.Text = ObjectDic.Instance.GetObjectName("입고") + rslt.Rows[3]["ATTRIBUTE1"].ToString();
                return colorData;
            }

            else return new List<string>();

        }
        private List<string> getAttribute2()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            //RQSTDT.Columns.Add("CMCODE", typeof(string)); 

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = "ko-KR";
            dr["CMCDTYPE"] = "MCS_ISS_DATE_COLR_TYPE";
            //dr["CMCODE"] = ""; //null이 되는 Input까지 입력할 경우 biz에서 데이터를 읽어들이지 못함.

            RQSTDT.Rows.Add(dr);

            DataTable rslt = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

            //List Index : Sequnece, Data : ATTRIBUTE2
            var colorData = new List<string>();

            int cnt = rslt.Rows.Count;
            if (cnt >= 1)
            {
                //return할 colorData에 필요한 biz연결 데이터 입력.
                for (int i = 0; i < cnt; i++)
                {
                    colorData.Add(rslt.Rows[i]["ATTRIBUTE2"].ToString());
                }
                //UI상의 색 TextBox에 들어가는 Text
                //ATTRIBUTE2가 UNDER라면, 일 미만, 아니라면(OVER라는 가정 하에) 일 이상
                txt1.Text = txt1.Text + ObjectDic.Instance.GetObjectName((rslt.Rows[0]["ATTRIBUTE2"].ToString().Equals("UNDER") ? "일미만" : "일이상")); ;
                txt2.Text = txt2.Text + ObjectDic.Instance.GetObjectName((rslt.Rows[1]["ATTRIBUTE2"].ToString().Equals("UNDER") ? "일미만" : "일이상")); ;
                txt3.Text = txt3.Text + ObjectDic.Instance.GetObjectName((rslt.Rows[2]["ATTRIBUTE2"].ToString().Equals("UNDER") ? "일미만" : "일이상")); ;
                txt4.Text = txt4.Text + ObjectDic.Instance.GetObjectName((rslt.Rows[3]["ATTRIBUTE2"].ToString().Equals("UNDER") ? "일미만" : "일이상")); ;
                return colorData;
            }

            else return new List<string>();

        }
        private void Grid_Draw()
        {
            try
            {
                string sWH_ID = "A5A101";
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));
                RQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = "A5";
                dr["WH_ID"] = sWH_ID;
                dr["ELTR_TYPE_CODE"] = getTypeCode();
                RQSTDT.Rows.Add(dr);

                //Grid Cell에 그려 줄 Attribute
                var attribute1 = getAttribute1();
                var attribute2 = getAttribute2();
                new ClientProxy().ExecuteService("DA_PRD_SEL_MIDWH_DRAW", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        try
                        {
                            //헤더 셋팅 (가로)
                            for (int i = 0; i <= 100; i++)
                        {
                            ColumnDefinition gridCol1 = new ColumnDefinition();
                            //gridCol1.Width = new GridLength(100);
                            gridCol1.Width = GridLength.Auto;
                            dgList.ColumnDefinitions.Add(gridCol1);
                        }

                        for (int iCol = 0; iCol <= 100; iCol++)
                        {
                            Label _lable = new Label();
                            _lable.Content = iCol.ToString();
                            _lable.FontSize = 11;
                            _lable.HorizontalContentAlignment = HorizontalAlignment.Center;
                            _lable.VerticalContentAlignment = VerticalAlignment.Center;
                            _lable.Margin = new Thickness(0, 0, 0, 0);
                            _lable.Padding = new Thickness(0, 0, 0, 0);
                            _lable.BorderThickness = new Thickness(1, 1, 1, 1);
                            _lable.BorderBrush = new SolidColorBrush(Colors.Gray);
                            _lable.Width = 35;
                            _lable.Height = 5;
                            _lable.Tag = iCol;
                            _lable.Name = "Col" + iCol.ToString();
                            _lable.Background = new SolidColorBrush(Colors.LightGray);
                            //_lable.Visibility = Visibility.Collapsed;
                            _lable.Visibility = Visibility.Hidden;

                            Grid.SetColumn(_lable, iCol);
                            Grid.SetRow(_lable, 0);
                            dgList.Children.Add(_lable);
                            dgList.RegisterName(_lable.Name, _lable);
                        }

                        //헤더 셋팅 (세로)
                        for (int i = 0; i <= 100; i++)
                        {
                            RowDefinition gridRow1 = new RowDefinition();
                            //gridRow1.Height = new GridLength(50);
                            gridRow1.Height = GridLength.Auto;
                            dgList.RowDefinitions.Add(gridRow1);
                        }

                        for (int iRow = 0; iRow <= 100; iRow++)
                        {
                            Label _lable = new Label();
                            _lable.Content = iRow.ToString();
                            _lable.FontSize = 11;
                            _lable.HorizontalContentAlignment = HorizontalAlignment.Center;
                            _lable.VerticalContentAlignment = VerticalAlignment.Center;
                            _lable.Margin = new Thickness(0, 0, 0, 0);
                            _lable.Padding = new Thickness(0, 0, 0, 0);
                            _lable.BorderThickness = new Thickness(1, 1, 1, 1);
                            _lable.BorderBrush = new SolidColorBrush(Colors.Gray);
                            _lable.Width = 5;
                            _lable.Height = 15;
                            _lable.Tag = iRow;
                            _lable.Name = "Row" + iRow.ToString();
                            _lable.Background = new SolidColorBrush(Colors.LightGray);
                            _lable.Visibility = Visibility.Hidden;

                            Grid.SetColumn(_lable, 0);
                            Grid.SetRow(_lable, iRow);
                            dgList.Children.Add(_lable);
                            dgList.RegisterName(_lable.Name, _lable);
                        }

                        //데이타 셋팅
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            string sTag = "";
                            string sCol = "";
                            string sRow = "";

                            sTag = result.Rows[i]["X_POSITION"].ToString() + "-" + result.Rows[i]["Y_POSITION"].ToString();
                            sCol = result.Rows[i]["X_POSITION"].ToString();
                            sRow = result.Rows[i]["Y_POSITION"].ToString();

                            Label _lable = new Label();
                            _lable.Content = result.Rows[i]["RACK_ID"].ToString();
                            _lable.FontSize = 12;
                            //_lable.FontWeight = FontWeights.Bold;
                            _lable.HorizontalContentAlignment = HorizontalAlignment.Center;
                            _lable.VerticalContentAlignment = VerticalAlignment.Center;
                            _lable.Margin = new Thickness(0, 0, 0, 0);
                            _lable.Padding = new Thickness(0, 0, 0, 0);
                            _lable.BorderThickness = new Thickness(1, 1, 1, 1);
                            //_lable.BorderBrush = new SolidColorBrush(getColor(dtRslt.Rows[iCnt]["EIOSTAT"].ToString()));
                            _lable.Width = 58;
                            _lable.Height = 22;
                            _lable.Tag = result.Rows[i]["RACK_ID"].ToString();
                            _lable.Name = "RACK" + result.Rows[i]["RACK_ID"].ToString();
                            _lable.Background = new SolidColorBrush(Colors.White); //NavajoWhite, Ivory, White

                            if(!sCol.Equals(""))
                                Grid.SetColumn(_lable, int.Parse(sCol));
                            if (!sRow.Equals(""))
                                Grid.SetRow(_lable, int.Parse(sRow) + 1);
                            dgList.Children.Add(_lable);
                            dgList.RegisterName(_lable.Name, _lable);

                            Label _lable2 = new Label();
                            _lable2.Content = result.Rows[i]["RACK_CNT"].ToString();
                            _lable2.FontSize = 12;
                            //_lable2.FontWeight = FontWeights.Bold;
                            _lable2.HorizontalContentAlignment = HorizontalAlignment.Center;
                            _lable2.VerticalContentAlignment = VerticalAlignment.Center;
                            _lable2.Margin = new Thickness(0, 0, 0, 0);
                            _lable2.Padding = new Thickness(0, 0, 0, 0);
                            _lable2.BorderThickness = new Thickness(1, 1, 1, 1);
                            _lable2.Width = 58;
                            _lable2.Height = 22;
                            _lable2.Tag = result.Rows[i]["RACK_CNT"].ToString();
                            _lable2.Name = "VALUE" + result.Rows[i]["RACK_ID"].ToString();
                            _lable2.Background = new SolidColorBrush(getColor(result.Rows[i]["DIFFDATE"].ToString(), attribute1, attribute2));

                            //상세데이터 조회
                            _lable2.MouseDoubleClick += _lable2_MouseDoubleClick;
                            if (!sCol.Equals(""))
                                Grid.SetColumn(_lable2, int.Parse(sCol) + 1);
                            if (!sRow.Equals(""))
                                Grid.SetRow(_lable2, int.Parse(sRow) + 1);
                            dgList.Children.Add(_lable2);
                            dgList.RegisterName(_lable2.Name, _lable2);
                        }
                        }
                        catch (Exception ex2)
                        {
                            Util.MessageException(ex2);
                            return;
                        }
                    }
                    //요청에 의한 텍스트 추가건
                    //AddText();
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        #endregion


        private Color getColor(string sDffDate, List<string> attribute1, List<string> attribute2)
        {
            int dffDate = int.Parse(sDffDate);

            //범위를 잘못 설정했을 경우, RED
            Color cReturn = Colors.Red;


            //SEQ 4에 관한
            if (attribute2[3] == "UNDER" && dffDate < int.Parse(attribute1[3]))
                cReturn = Colors.Yellow;
            else if (attribute2[3] == "OVER" && dffDate >= int.Parse(attribute1[3]))
                cReturn = Colors.Yellow;

            //SEQ 3에 관한
            if (attribute2[2] == "UNDER" && dffDate < int.Parse(attribute1[2]))
                cReturn = Colors.LightSkyBlue;
            else if (attribute2[2] == "OVER" && dffDate >= int.Parse(attribute1[2]))
                cReturn = Colors.LightSkyBlue;

            //SEQ 2에 관한
            if (attribute2[1] == "UNDER" && dffDate < int.Parse(attribute1[1]))
                cReturn = Colors.LightGreen;
            else if (attribute2[1] == "OVER" && dffDate >= int.Parse(attribute1[1]))
                cReturn = Colors.LightGreen;

            //SEQ 1에 관한
            if (attribute2[0] == "UNDER" && dffDate < int.Parse(attribute1[0]))
                cReturn = Colors.WhiteSmoke;
            else if (attribute2[0] == "OVER" && dffDate >= int.Parse(attribute1[0]))
                cReturn = Colors.WhiteSmoke;



            //if (dffDate <= int.Parse(colorTypes[0]))
            //{
            //    cReturn = Colors.WhiteSmoke;
            //}
            //else if (dffDate <= int.Parse(colorTypes[1]))
            //{
            //    cReturn = Colors.LightGreen;
            //}
            //else if (dffDate <= int.Parse(colorTypes[2]))
            //{
            //    cReturn = Colors.LightSkyBlue; //Aqua   LightSkyBlue
            //}
            //else
            //{
            //    cReturn = Colors.Yellow;
            //}

            return cReturn;

        }
       
        private void SearchData()
        {
            DataTable RQSTDT = new DataTable("RQSTDT");
            RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
            RQSTDT.Columns.Add("LOTID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            if(!string.IsNullOrWhiteSpace(txtModel.Text))  dr["PRJT_NAME"] = txtModel.Text;
            if (!string.IsNullOrWhiteSpace(txtLotid.Text)) dr["LOTID"] = txtLotid.Text;
            RQSTDT.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_PRD_SEL_STOCK_BY_SERCH_LOT", "RQSTDT", "RSLTDT", RQSTDT, (result, exception) =>
            {
                try
                {
                    if (result == null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            SetGridColor(result.Rows[i]["RACK_ID"].ToString(), result.Rows[i]["LOTCNT"].ToString(), Colors.Bisque); // LightSkyBlue NavajoWhite
                        }
                    }
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", ex);
                }
                finally
                {
                    //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", Logger.MESSAGE_OPERATION_END);
                }

            });
        }

        private void SetGridColor(string sRackId, string sCNT, Color color)
        {
            try
            {
                Label lblRack = dgList.FindName("RACK" + sRackId) as Label;
                if (lblRack != null) lblRack.Background = new SolidColorBrush(Colors.Bisque);

                Label lblValue = dgList.FindName("VALUE" + sRackId) as Label;
                if (lblValue != null)
                {
                    lblValue.Content = lblValue.Tag + "(" + sCNT + ")" ;
                }
                //_lable2.Name = "VALUE" + "ROW" + sRow + "COL" + sCol;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

    }
}
