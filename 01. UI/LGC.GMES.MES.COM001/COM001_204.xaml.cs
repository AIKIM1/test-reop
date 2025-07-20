/*************************************************************************************
 Created Date : 2017.11.09
      Creator : 장만철c
   Decription : 라미대기창고 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2017.11.09  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
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
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.IO;
using System.Reflection;



namespace LGC.GMES.MES.COM001
{
    public partial class COM001_204 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        public Boolean ReFlash = false;


        public class ResultElement
        {
            public RadioButton radButton = null;
            public bool Visibility = true;
            public int SpaceInCharge = 1;
        }

        public C1ComboBox DATE_COMBO { get; set; }

        DataTable dtTemp;
        DataTable dtTemp_A;

        public int DATECharge = 30;
        string WH_ID = string.Empty;
        string RACK_ID = string.Empty;
        string seleted_rack = string.Empty;
        int refresh_cnt = 1;
        string img_path = "/LGC.GMES.MES.CMM001; component/Images/btn_i_print.png";

        //ImageSource="/LGC.GMES.MES.ControlsLibrary;component/Images/LGC/icon_input_search.png" Stretch="Uniform"

        // 모니터링용 타이머
        private System.Windows.Threading.DispatcherTimer _timer = new System.Windows.Threading.DispatcherTimer();

        public COM001_204()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void TimerSetting()
        {
            _timer.Stop();
            refresh_cnt = Convert.ToInt32(numRefresh.Value);
            int interval = refresh_cnt * 60; //기준 : 분

            _timer.Interval = new TimeSpan(0, 0, 0, interval);
            _timer.Tick += new EventHandler(timer_Tick);
            _timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                btnRefresh_Click(null, null);

                //StocListSearch("C", dgResult); //양극
                //StocListSearch("A", dgResult_A); //음극

                RACK_ID = "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();

            Search_Status();
            _timer.Start();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            _timer.Start();

            Initialize();
        }

        private void dgList_Initialized(object sender, EventArgs e)
        {

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
            //Set_RadioButton_GR();   
            SetCombo();
        }

        private void SetImage()
        {
            try
            {
                Image img = new Image();
                Uri uriSource = null;
                int row_span = 0;

                switch (Util.GetCondition(cboFloor))
                {
                    case "A1A201": // 1동 2층 라미 대기전극 적재대 
                        uriSource = new Uri(@"/LGC.GMES.MES.CMM001;component/Images/1-2_lami_stanby_vd.png", UriKind.Relative);
                        row_span = 32;
                        break;
                    case "A1A301": // 1동 3층 라미 대기전극 적재대
                        uriSource = new Uri(@"/LGC.GMES.MES.CMM001;component/Images/1-2_lami_stanby_vd.png", UriKind.Relative);
                        row_span = 31;
                        break;
                    case "A2A201": // 2동 2층 자동차 10,13,14호 라미 대기전극 적재대
                        uriSource = new Uri(@"/LGC.GMES.MES.CMM001;component/Images/1-2_lami_stanby_vd.png", UriKind.Relative);
                        row_span = 32;
                        //row_span = 52;
                        break;
                    case "A2A301": // 2동 3층 라미 대기전극 적재대
                        uriSource = new Uri(@"/LGC.GMES.MES.CMM001;component/Images/1-2_lami_stanby_vd.png", UriKind.Relative);
                        row_span = 32;
                        break;
                    case "S2A201": // 2동 2층 ESS 1호 라미 대기전극 적재대
                        uriSource = new Uri(@"/LGC.GMES.MES.CMM001;component/Images/1-2_lami_stanby_vd.png", UriKind.Relative);
                        row_span = 27;
                        break;
                    default:
                        break;
                }

                img.Source = new BitmapImage(uriSource);
                img.VerticalAlignment = VerticalAlignment.Stretch;
                img.Stretch = Stretch.Fill;
                img.Name = "VD_IMG";

                Grid.SetColumn(img, 0);
                Grid.SetRow(img, 0);
                Grid.SetRowSpan(img, row_span);
                dgList.Children.Add(img);
                dgList.RegisterName(img.Name, img);



            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }

        private void SetCombo()
        {
            CommonCombo _combo = new CommonCombo();
            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;


            //동           
            C1ComboBox[] cboAreaChild = { cboFloor };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //층
            C1ComboBox[] cboFloorParend = { cboSHOPID, cboArea };
            _combo.SetCombo(cboFloor, CommonCombo.ComboStatus.NONE, cbParent: cboFloorParend);
        }


        public static List<ResultElement> RadioButtonList(DataTable dt)
        {
            List<ResultElement> lst = new List<ResultElement>();
            DataTable dt2 = dt as DataTable;
            for (int row = 0; row < dt2.Rows.Count; row++)
            {
                lst.Add(new ResultElement { radButton = new RadioButton() { GroupName = "RadioButton_" + LoginInfo.CFG_AREA_ID, Name = dt2.Rows[row][2].ToString(), Tag = dt2.Rows[row][3].ToString(), Content = dt2.Rows[row][1].ToString() + " ", VerticalAlignment = VerticalAlignment.Center } });
            }
            return lst;
        }

        private void Set_RadioButton_GR()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_WH_MONITORING_RADIO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        List<ResultElement> elemList;
                        elemList = RadioButtonList(result);
                        if (elemList.Count > 0)
                        {
                            //SetResult(elemList, Area);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        void SetResult(List<ResultElement> elementList, Grid grid)
        {
            int elementCol = 0;
            grid.SetValue(Grid.VerticalAlignmentProperty, VerticalAlignment.Bottom);
            int colIndex = 0;

            foreach (ResultElement re in elementList)
            {
                if (re.radButton != null)
                {
                    re.radButton.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    re.radButton.Margin = new Thickness(10, 0, 5, 0);
                    if (elementCol == 0)
                        re.radButton.IsChecked = true;
                    elementCol++;
                    re.radButton.SetValue(Grid.ColumnProperty, elementCol);
                    re.radButton.Checked += RadioButton_Checked;
                    //Area.Children.Add(re.radButton);
                }
                colIndex += re.SpaceInCharge;
            }
            Search_Status();
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

            Label lblRack = dgList.FindName(lblName.Name.ToString().Replace("VALUE", "RACK")) as Label;


            COM001_204_LOTS_IN_RACK wndPopup = new COM001_204_LOTS_IN_RACK(lblRack.Content.ToString(), Util.GetCondition(cboArea), Util.GetCondition(cboFloor));
            //COM001_025_SRS_RACK wndPopup = new COM001_025_SRS_RACK(lblRack.Content.ToString(), "COMM_CONV");

            wndPopup.FrameOperation = FrameOperation;
            wndPopup.Show();

        }

        private void setLayoutLabelColors(object sender, C1DataGrid dg)
        {
            Label click_label = (Label)sender;
            SolidColorBrush sb = click_label.Background as SolidColorBrush;

            if (seleted_rack == DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "RACK_ID").ToString())
            {
                return;
            }

            seleted_rack = DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "RACK_ID").ToString();

            if (sb.ToString() == "#FFFFE4C4")
            {
                click_label.BorderThickness = new Thickness(2, 2, 2, 2);
                click_label.Background = new SolidColorBrush(Colors.Chocolate);

                if (RACK_ID.Length > 0) //이전에 선택한 rack이 있으면
                {
                    Label lbl = dgList.FindName(RACK_ID) as Label;

                    lbl.BorderThickness = new Thickness(1, 1, 1, 1);
                    lbl.Background = new SolidColorBrush(Colors.Bisque);
                    RACK_ID = click_label.Name;
                }

                RACK_ID = click_label.Name;
            }
            else
            {
                click_label.BorderThickness = new Thickness(1, 1, 1, 1);
                click_label.Background = new SolidColorBrush(Colors.Bisque);
                RACK_ID = "";
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search_Status();
            RACK_ID = "";
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
        }

        #endregion

        #region Mehod

        #region 데이터 조회   

        /// <summary>
        /// [유효기간 임박재고] 데이터를 조회합니다.
        /// </summary>

        #endregion

        private void Clear_Grid()
        {
            //Util.gridClear(dgStore);
            //Util.gridClear(dgProcess);
            //Util.gridClear(dgMonth);
        }

        private void ClearGrid()
        {
            try
            {
                //if (dgList != null)
                //{
                //    foreach(Image _img in dgList.Children.OfType<Image>())
                //    {
                //        dgList.UnregisterName(_img.Name);
                //    }

                //    foreach (Label _label in dgList.Children.OfType<Label>())
                //    {
                //        if(_label.Name != "Col0" && _label.Name != "Col1")
                //        {
                //            dgList.UnregisterName(_label.Name);
                //        }

                //    }

                //    foreach (StackPanel _stack in dgList.Children.OfType<StackPanel>())
                //    {
                //        dgList.UnregisterName(_stack.Name);
                //    }
                //}

                NameScope.SetNameScope(dgList, new NameScope());

                dgList.Children.Clear();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Search_Status()
        {
            ClearGrid();
            Grid_Draw();
            ReFlash = true;
        }

        private void Grid_Draw()
        {
            try
            {
                //string sWH_ID = ""; // GetWarehouseID();
                //if (string.IsNullOrWhiteSpace(sWH_ID)) return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = Util.GetCondition(cboArea);
                dr["WH_ID"] = Util.GetCondition(cboFloor);
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_GET_LAMI_HW_PANCAKE_WITH_FIFO", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
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

                            //img 세팅
                            SetImage();

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
                                _lable.HorizontalContentAlignment = HorizontalAlignment.Center;
                                _lable.VerticalContentAlignment = VerticalAlignment.Center;
                                _lable.Margin = new Thickness(0, 0, 0, 0);
                                _lable.Padding = new Thickness(0, 0, 0, 0);
                                _lable.BorderThickness = new Thickness(1, 1, 1, 1);
                                _lable.Width = 58;
                                _lable.Height = 22;
                                _lable.Tag = result.Rows[i]["RACK_ID"].ToString();
                                _lable.Name = "RACK" + result.Rows[i]["RACK_ID"].ToString();
                                _lable.Background = new SolidColorBrush(Colors.White); //NavajoWhite, Ivory, White

                                if (!sCol.Equals(""))
                                    Grid.SetColumn(_lable, int.Parse(sCol));
                                if (!sRow.Equals(""))
                                    Grid.SetRow(_lable, int.Parse(sRow) + 1);
                                dgList.Children.Add(_lable);
                                dgList.RegisterName(_lable.Name, _lable);

                                Label _lable2 = new Label();
                                _lable2.Content = result.Rows[i]["RACK_CNT"].ToString() + "(" + result.Rows[i]["HOLD_CNT"].ToString() + ")";
                                _lable2.FontSize = 12;
                                _lable2.HorizontalContentAlignment = HorizontalAlignment.Center;
                                _lable2.VerticalContentAlignment = VerticalAlignment.Center;
                                _lable2.Margin = new Thickness(0, 0, 0, 0);
                                _lable2.Padding = new Thickness(0, 0, 0, 0);
                                _lable2.BorderThickness = new Thickness(1, 1, 1, 1);
                                _lable2.Width = 58;
                                _lable2.Height = 22;
                                _lable2.Tag = result.Rows[i]["RACK_CNT"].ToString();
                                _lable2.Name = "VALUE" + result.Rows[i]["RACK_ID"].ToString();
                                _lable2.Background = new SolidColorBrush(getColor(result.Rows[i]["RACK_CNT"].ToString(),
                                                                                    result.Rows[i]["MAX_QTY"].ToString(),
                                                                                    result.Rows[i]["FIFO_GUBUN"].ToString(),
                                                                                    result.Rows[i]["HOLD_CNT"].ToString()
                                                                                  )
                                                                        );

                                //상세데이터 조회
                                _lable2.MouseDoubleClick += _lable2_MouseDoubleClick;

                                string qty_location = result.Rows[i]["QTY_POSITION"].ToString();

                                if (!sCol.Equals(""))
                                {
                                    if (qty_location == "D") //수량이 밑에 위치
                                    {
                                        Grid.SetColumn(_lable2, int.Parse(sCol));
                                    }
                                    else if (qty_location == "U")//수량이 위에 위치
                                    {
                                        Grid.SetColumn(_lable2, int.Parse(sCol));
                                    }
                                    else //수량이 오른쪽 옆에 위치
                                    {
                                        Grid.SetColumn(_lable2, int.Parse(sCol) + 1);
                                    }
                                }
                                if (!sRow.Equals(""))
                                {
                                    if (qty_location == "D") //수량이 밑에 위치
                                    {
                                        Grid.SetRow(_lable2, int.Parse(sRow) + 2);
                                    }
                                    else if (qty_location == "U") //수량이 위에 위치
                                    {
                                        Grid.SetRow(_lable2, int.Parse(sRow));
                                    }
                                    else  // 수량이 오른쪽 옆에 위치
                                    {
                                        Grid.SetRow(_lable2, int.Parse(sRow) + 1);
                                    }
                                }

                                dgList.Children.Add(_lable2);
                                dgList.RegisterName(_lable2.Name, _lable2);
                            }
                            /* 한 화면의 경계선 표현 
                                                        for(int irow = 0; irow < 34; irow++)
                                                        {
                                                            int icol = 29; 
                                                            Border bd = new Border();
                                                            bd.BorderBrush = new SolidColorBrush(Colors.Red);
                                                            bd.BorderThickness = new Thickness(0, 0, 2, 0);                                

                                                            Grid.SetColumn(bd, icol);
                                                            Grid.SetRow(bd, irow);
                                                            dgList.Children.Add(bd);                               
                                                        }

                                                        for(int icol = 0; icol < 30; icol++)
                                                        {
                                                            int irow = 33;
                                                            Border bd = new Border();
                                                            bd.BorderBrush = new SolidColorBrush(Colors.Red);
                                                            bd.BorderThickness = new Thickness(0, 0, 0, 2);                             

                                                            Grid.SetColumn(bd, icol);
                                                            Grid.SetRow(bd, irow);
                                                            dgList.Children.Add(bd);                               
                                                        }
                            */
                            StocListSearch("C", dgResult); //양극
                            StocListSearch("A", dgResult_A); //음극
                        }
                        catch (Exception ex2)
                        {
                            Util.MessageException(ex2);
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        #endregion

        private void MakeText(string sText, int iRow, int iCol)
        {
            string sRow = "";
            string sCol = "";

            sRow = iRow.ToString();
            sCol = iCol.ToString();

            Label _lable = new Label();
            _lable.Content = sText;
            _lable.FontSize = 12;
            _lable.HorizontalContentAlignment = HorizontalAlignment.Center;
            _lable.VerticalContentAlignment = VerticalAlignment.Center;
            _lable.Margin = new Thickness(0, 0, 0, 0);
            _lable.Padding = new Thickness(0, 0, 0, 0);
            _lable.BorderThickness = new Thickness(1, 1, 1, 1);
            _lable.Width = 58;
            _lable.Height = 22;
            _lable.Tag = sRow + "-" + sCol;
            _lable.Name = "TEXT" + "ROW" + sRow + "COL" + sCol;
            _lable.Background = new SolidColorBrush(Colors.White);
            _lable.Foreground = new SolidColorBrush(Colors.Blue);
            _lable.FontWeight = FontWeights.Bold;

            Grid.SetColumn(_lable, iCol);
            Grid.SetRow(_lable, iRow);
            dgList.Children.Add(_lable);
            dgList.RegisterName(_lable.Name, _lable);

        }

        private Color getColor(string sCnt, string sMax, string fifo_gubun, string hold_cnt)
        {
            /* 최대 적재수량과 현재 수량을 가지고 퍼센트 비율 반영 (2017-11-28일 이후로 안씀)
                        double Yield = 0;
                        double iCnt = 0;
                        double iMax = 0;

                        Color cReturn = Colors.White;
                        iCnt = double.Parse(sCnt);
                        iMax = double.Parse(sMax);

                        //Rack 에 실물이 차지하고 있는 퍼센트 비율 계산 처리
                        Yield = Math.Round((iCnt / iMax) * 100, 2);

                        if (Yield >= 100)
                        {
                            cReturn = Colors.Red;
                        }
                        else
                        {
                            cReturn = Colors.DarkSeaGreen;  
                        }
                        //else if (Yield < 100 && Yield > 66)
                        //{
                        //    cReturn = Colors.Yellow;
                        //}
                        //else if (Yield < 66 && Yield > 33)
                        //{
                        //    cReturn = Colors.LightSkyBlue; //Aqua   LightSkyBlue
                        //}
                        //else if (Yield < 33 && Yield > 0)
                        //{
                        //    cReturn = Colors.LightGreen;  //Lime  LawnGreen
                        //}
                        //else if (Yield <= 0)
                        //{
                        //    cReturn = Colors.WhiteSmoke;
                        //}
            */

            Color cReturn = Colors.WhiteSmoke;

            switch (fifo_gubun)
            {
                case "Y": //모두 선입선출 가능
                    if (hold_cnt != "0")
                    {
                        cReturn = Colors.Yellow;
                    }
                    else
                    {
                        cReturn = Colors.DarkSeaGreen;
                    }

                    break;
                case "N": //모두 선입선출 불가능
                    cReturn = Colors.Red;
                    break;
                case "M": //선입선출 가능 + 불가능 혼재
                    cReturn = Colors.Yellow;
                    break;
                default:
                    break;
            }

            return cReturn;

        }

        private void SearchData()
        {
            DataTable RQSTDT = new DataTable("RQSTDT");
            RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
            RQSTDT.Columns.Add("LOTID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            //if (!string.IsNullOrWhiteSpace(txtModel.Text)) dr["PRJT_NAME"] = txtModel.Text;
            //if (!string.IsNullOrWhiteSpace(txtLotid.Text)) dr["LOTID"] = txtLotid.Text;
            RQSTDT.Rows.Add(dr);
            new ClientProxy().ExecuteService(GetDrawBiz2RuleName(), "RQSTDT", "RSLTDT", RQSTDT, (result, exception) =>
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
                        ReFlash = false;
                    }
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                }
                finally
                {
                }

            });
        }

        string GetDrawBiz2RuleName()
        {

            string sBizRuleName = string.Empty;

            sBizRuleName = "DA_PRD_SEL_STOCK_BY_SERCH_LOT";

            sBizRuleName = "DA_PRD_SEL_STOCK_BY_SERCH_LOT_SRS";

            return sBizRuleName;
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
                    lblValue.Content = lblValue.Tag + "(" + sCNT + ")";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnPrj_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_204_PJTLIST popup_PRJ = new COM001_204_PJTLIST();
                popup_PRJ.FrameOperation = this.FrameOperation;

                if (popup_PRJ != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("AREAID", typeof(string));
                    dtData.Columns.Add("WH_ID", typeof(string));

                    DataRow newRow = null;
                    newRow = dtData.NewRow();
                    newRow["AREAID"] = Util.GetCondition(cboArea);
                    newRow["WH_ID"] = Util.GetCondition(cboFloor);
                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup_PRJ, Parameters);
                    //========================================================================

                    popup_PRJ.Closed -= popup_PRJ_Closed;
                    popup_PRJ.Closed += popup_PRJ_Closed;
                    popup_PRJ.ShowModal();
                    popup_PRJ.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        void popup_PRJ_Closed(object sender, EventArgs e)
        {
            try
            {
                COM001_204_PJTLIST popup = sender as COM001_204_PJTLIST;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    txtPrj.Text = popup.PRJ;
                    txtPrj.Tag = popup.PRJ;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnLotID_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_204_LOTLIST popup_LOT = new COM001_204_LOTLIST();
                popup_LOT.FrameOperation = this.FrameOperation;

                if (popup_LOT != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("AREAID", typeof(string));
                    dtData.Columns.Add("WH_ID", typeof(string));

                    DataRow newRow = null;
                    newRow = dtData.NewRow();
                    newRow["AREAID"] = Util.GetCondition(cboArea);
                    newRow["WH_ID"] = Util.GetCondition(cboFloor);
                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup_LOT, Parameters);
                    //========================================================================

                    popup_LOT.Closed -= popup_LOT_Closed;
                    popup_LOT.Closed += popup_LOT_Closed;
                    popup_LOT.ShowModal();
                    popup_LOT.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        void popup_LOT_Closed(object sender, EventArgs e)
        {
            try
            {
                COM001_204_LOTLIST popup = sender as COM001_204_LOTLIST;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    txtLotID.Text = popup.LOTID;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnSubSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StocListSearch("C", dgResult); //양극
                StocListSearch("A", dgResult_A); //음극
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 재고현황 리스트
        /// </summary>
        private void StocListSearch(string elec_type, C1DataGrid dg)
        {
            try
            {
                C1DataGrid dgGrid = dg;

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("WIPHOLD", typeof(string));
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("WH_ID", typeof(string));
                IndataTable.Columns.Add("PJT", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("ELEC_TYPE", typeof(string));
                IndataTable.Columns.Add("RACK_ID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = Util.GetCondition(cboArea);
                if ((bool)chkHold.IsChecked) Indata["WIPHOLD"] = "N";

                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["WH_ID"] = Util.GetCondition(cboFloor);
                Indata["PJT"] = txtPrj.Text == "" ? null : txtPrj.Tag.ToString();
                Indata["LOTID"] = Util.GetCondition(txtLotID) == "" ? null : Util.GetCondition(txtLotID);
                Indata["ELEC_TYPE"] = elec_type == "" ? null : elec_type;
                Indata["RACK_ID"] = null;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_LAMI_STANDBY_PANCAKE_LIST", "RQSTDT", "RSLTDT", IndataTable);

                //txtPrj.Text = "";
                //txtPrj.Tag = "";
                //txtLotID.Text = "";

                if (dtMain.Rows.Count > 0)
                {
                    dgGrid.ItemsSource = DataTableConverter.Convert(dtMain);

                    if (elec_type == "C")
                    {
                        dtTemp = dtMain;
                    }
                    else if (elec_type == "A")
                    {
                        dtTemp_A = dtMain;
                    }


                    string rack_id = string.Empty;
                    for (int i = 0; i < dtMain.Rows.Count; i++)
                    {
                        rack_id = dtMain.Rows[i]["RACK_ID"].ToString();

                        Label seleted_Label = dgList.FindName("RACK" + rack_id) as Label;

                        if (seleted_Label != null)
                        {
                            seleted_Label.Background = new SolidColorBrush(Colors.Bisque);
                        }
                    }
                }
                else
                {
                    dgGrid.ItemsSource = null;
                }

                return;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dgResult_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgResult.Rows.Count == 0 || dgResult == null)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgResult.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = cell.Row.Index;
                int _col = cell.Column.Index;
                string value = cell.Value.ToString();

                string seleted_RACKID = DataTableConverter.GetValue(dgResult.Rows[currentRow].DataItem, "RACK_ID").ToString();

                Label seleted_Label = dgList.FindName("RACK" + seleted_RACKID) as Label;


                if (seleted_Label == null)
                {
                    return;
                }

                setLayoutLabelColors(seleted_Label, dgResult);

            }
            catch (Exception ex)
            {
                //Util.Alert(ex.Message);
                Util.MessageException(ex);
            }
        }
        private void dgResult_A_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgResult_A.Rows.Count == 0 || dgResult_A == null)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgResult_A.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = cell.Row.Index;
                int _col = cell.Column.Index;
                string value = cell.Value.ToString();

                string seleted_RACKID = DataTableConverter.GetValue(dgResult_A.Rows[currentRow].DataItem, "RACK_ID").ToString();

                Label seleted_Label = dgList.FindName("RACK" + seleted_RACKID) as Label;


                if (seleted_Label == null)
                {
                    return;
                }

                setLayoutLabelColors(seleted_Label, dgResult_A);

            }
            catch (Exception ex)
            {
                //Util.Alert(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void dgResult_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                    if (e.Cell.Column.Name == "LOTID")
                    {
                        if (dtTemp.Rows[e.Cell.Row.Index]["FIFO_GUBUN"].ToString() == "Y")
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, 3).Presenter.Background = new SolidColorBrush(Colors.DarkSeaGreen);
                        }
                        else if (dtTemp.Rows[e.Cell.Row.Index]["FIFO_GUBUN"].ToString() == "N")
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, 3).Presenter.Background = new SolidColorBrush(Colors.Red);
                        }

                        if (dtTemp.Rows[e.Cell.Row.Index]["WIPHOLD"].ToString() == "Y")
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, 3).Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                    }

                }));

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgResult_A_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                    if (e.Cell.Column.Name == "LOTID")
                    {
                        if (dtTemp_A.Rows[e.Cell.Row.Index]["FIFO_GUBUN"].ToString() == "Y")
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, 3).Presenter.Background = new SolidColorBrush(Colors.DarkSeaGreen);
                        }
                        else if (dtTemp_A.Rows[e.Cell.Row.Index]["FIFO_GUBUN"].ToString() == "N")
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, 3).Presenter.Background = new SolidColorBrush(Colors.Red);
                        }

                        if (dtTemp_A.Rows[e.Cell.Row.Index]["WIPHOLD"].ToString() == "Y")
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, 3).Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                    }

                }));

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void numRefresh_MouseLeave(object sender, MouseEventArgs e)
        {
            _timer.Stop();
            TimerSetting();
        }
    }
}
