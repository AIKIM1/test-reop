/*************************************************************************************
 Created Date : 2018.01.22
      Creator : 장만철
   Decription : 전지 5MEGA-GMES 구축 - 전극 출하대기 창고 현황판
--------------------------------------------------------------------------------------
 [Change History]


**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_219 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public Boolean ReFlash = false;

        DataTable dtTemp;
        DataTable dtTemp_A;
        DataTable dtMainSearch;

        string WH_ID = string.Empty;
        string RACK_ID = string.Empty;
        string seleted_rack = string.Empty;
        int refresh_cnt = 1;
        bool mainSearch_flag = false;


        // 모니터링용 타이머
        private System.Windows.Threading.DispatcherTimer _timer = null;

        public enum ComboStatus
        {
            /// <summary>
            /// 콤보 바인딩 후 ALL 을 최상단에 표시
            /// </summary>
            ALL,

            /// <summary>
            /// 콤보 바인딩 후 Select 을 최상단에 표시 (필수선택 항목에 사용)
            /// </summary>
            SELECT,

            /// <summary>
            /// 바인딩 후 선택 안해도 될경우(선택 안해도 되는 콤보일때 사용)
            /// </summary>
            NA,

            /// <summary>
            /// 바인딩만 하고 끝 (바인딩후 제일 1번째 항목을 표시) 
            /// </summary>
            NONE
        }

        public BOX001_219()
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
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }

            _timer = new System.Windows.Threading.DispatcherTimer();
            refresh_cnt = Convert.ToInt32(numRefresh.Value);
            int interval = refresh_cnt * 60; //기준 : 분

            _timer.Interval = new TimeSpan(0, 0, 0, interval);
            _timer.Tick += new EventHandler(timer_Tick);
            _timer.Start();
        }
        #endregion

        #region Initialize
        private void Initialize()
        {           
            SetCombo();
        }

        private void SetImage()
        {
            try
            {
                Image img = new Image();
                Uri uriSource = null;
                int row_span = 0;

                switch (Util.GetCondition(cboWH))
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
            //창고 콤보
            getWH_COMBO(); 
        }
        

        #endregion

        #region Event

        #region Load EVENT
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            Initialize();
            TimerSetting();

            this.Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
        }
        #endregion

        #region TIMER EVENT
        void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                btnRefresh_Click(null, null);
                RACK_ID = "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void numRefresh_MouseLeave(object sender, MouseEventArgs e)
        {
            _timer.Tick -= new EventHandler(timer_Tick);
            TimerSetting();
        }
        #endregion

        #region BUTTON EVENT
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            txtProdID.Text = "";
            txtSkidID.Text = "";
            mainSearch_flag = true;
            Search_Status();
            RACK_ID = "";
        }

        private void btnSubSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {                
                mainSearch_flag = false;
                StocListSearch(dgResult);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSKIDID_Click(object sender, RoutedEventArgs e)
        {
/*            try
            {

                BOX001_219_SKID_LIST popup_SKID = new BOX001_219_SKID_LIST();
                popup_SKID.FrameOperation = this.FrameOperation;

                if (popup_SKID != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("AREAID", typeof(string));
                    dtData.Columns.Add("WH_ID", typeof(string));

                    DataRow newRow = null;
                    newRow = dtData.NewRow();
                    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    newRow["WH_ID"] = Util.GetCondition(cboWH);
                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup_SKID, Parameters);
                    //========================================================================

                    popup_SKID.Closed -= popup_SKID_Closed;
                    popup_SKID.Closed += popup_SKID_Closed;
                    popup_SKID.ShowModal();
                    popup_SKID.CenterOnScreen();
                }

            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
*/
        }

        private void btnProdID_Click(object sender, RoutedEventArgs e)
        {
/*
            try
            {

                BOX001_219_PROD_LIST popup_PROD = new BOX001_219_PROD_LIST();
                popup_PROD.FrameOperation = this.FrameOperation;

                if (popup_PROD != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("AREAID", typeof(string));
                    dtData.Columns.Add("WH_ID", typeof(string));

                    DataRow newRow = null;
                    newRow = dtData.NewRow();
                    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    newRow["WH_ID"] = Util.GetCondition(cboWH);
                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup_PROD, Parameters);
                    //========================================================================

                    popup_PROD.Closed -= popup_PROD_Closed;
                    popup_PROD.Closed += popup_PROD_Closed;
                    popup_PROD.ShowModal();
                    popup_PROD.CenterOnScreen();
                }

            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
*/
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            mainSearch_flag = false;
            Search_Status();
        }
        #endregion

        #region POPUP EVENT
        void popup_SKID_Closed(object sender, EventArgs e)
        {
/*
            try
            {

                BOX001_219_SKID_LIST popup = sender as BOX001_219_SKID_LIST;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    txtSkidID.Text = popup.SKID;
                }

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
*/
        }

        void popup_PROD_Closed(object sender, EventArgs e)
        {
/*
            try
            {

                BOX001_219_PROD_LIST popup = sender as BOX001_219_PROD_LIST;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    txtProdID.Text = popup.PRODID;
                    txtProdID.Tag = popup.PRODID;
                }

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
*/
        }
        #endregion

        #region GRID EVENT
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

                    if (e.Cell.Column.Name == "BOXID")
                    {
                        if (dtTemp.Rows[e.Cell.Row.Index]["VLD_CHECK"].ToString() == "Y" && dtTemp.Rows[e.Cell.Row.Index]["BOX_QMS_RESULT"].ToString() == "Y" && dtTemp.Rows[e.Cell.Row.Index]["WIPHOLD"].ToString() == "N")
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, dgResult.Columns["BOXID"].Index).Presenter.Background = new SolidColorBrush(Colors.DarkSeaGreen);
                        }
                        else
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, dgResult.Columns["BOXID"].Index).Presenter.Background = new SolidColorBrush(Colors.Red);
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
                            dataGrid.GetCell(e.Cell.Row.Index, dgResult.Columns["LOTID"].Index).Presenter.Background = new SolidColorBrush(Colors.DarkSeaGreen);
                        }
                        else if (dtTemp_A.Rows[e.Cell.Row.Index]["FIFO_GUBUN"].ToString() == "N")
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, dgResult.Columns["LOTID"].Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                        }

                        if (dtTemp_A.Rows[e.Cell.Row.Index]["WIPHOLD"].ToString() == "Y")
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, dgResult.Columns["LOTID"].Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                    }

                }));

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion

        //기타 EVENT
        private void _lable2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Label lblName = (Label)sender;
            Label lblRack = dgList.FindName(lblName.Name.ToString().Replace("VALUE", "RACK")) as Label;
            BOX001_219_BOXS_IN_RACK wndPopup = new BOX001_219_BOXS_IN_RACK(lblRack.Content.ToString(), Util.GetCondition(cboWH));

            wndPopup.FrameOperation = FrameOperation;
            wndPopup.Show();

        }
        #endregion

        #region Mehod
        private void getWH_COMBO()
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
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WHID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboWH.DisplayMemberPath = "CBO_NAME";
                cboWH.SelectedValuePath = "CBO_CODE";
                cboWH.SelectedIndex = 0;

                //cboWH.ItemsSource = DataTableConverter.Convert(dtResult);
                cboWH.ItemsSource = AddStatus(dtResult, ComboStatus.NONE, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable AddStatus(DataTable dt, ComboStatus cs, string sValue, string sDisplay)
        {

            DataRow dr = dt.NewRow();
            switch (cs)
            {
                case ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
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
                    //RACK_ID = click_label.Name;
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
            try
            {
                _timer.Stop();
                _timer.Tick -= new EventHandler(timer_Tick);
                
                Grid_Draw();
                ReFlash = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void Grid_Draw()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["WH_ID"] = Util.GetCondition(cboWH);
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_BOX_IN_RACK", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    ClearGrid();
                    //loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        HiddenLoadingIndicator();
                        _timer.Start();
                        _timer.Tick += new EventHandler(timer_Tick);
                        throw ex;
                    }

                    if (result.Rows.Count > 0)
                    {
                        try
                        { //헤더 셋팅 (가로) 
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
                            //SetImage();

                            int move_cnt = 0;
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
                                _lable.Tag = result.Rows[i]["RACK_ID"].ToString();
                                _lable.Name = "RACK" + result.Rows[i]["RACK_ID"].ToString();
                                _lable.Background = new SolidColorBrush(Colors.White); //NavajoWhite, Ivory, White
                                _lable.Width = 58;                              
                                _lable.Height = 22;

                                RotateTransform myRotateTransform;
                                TransformGroup myTransformGroup;

                                //Label text를 세로로
                                if (Util.GetCondition(cboWH) == "E7F192")
                                {
                                    _lable.Width = 45;
                                    _lable.Height = 40;                                 
                                    _lable.RenderTransformOrigin = new Point(0.5, 0.5);
                                    myRotateTransform = new RotateTransform();
                                    myRotateTransform.Angle = -90;
                                    myTransformGroup = new TransformGroup();
                                    myTransformGroup.Children.Add(myRotateTransform);
                                    _lable.RenderTransform = myTransformGroup;
                                    
                                    //_lable.Margin = new Thickness(move_cnt, 0, 0, 0);
                                    //move_cnt = move_cnt - 30;
                                }    

                                if (!sCol.Equals(""))
                                    Grid.SetColumn(_lable, int.Parse(sCol));
                                if (!sRow.Equals(""))
                                    Grid.SetRow(_lable, int.Parse(sRow) + 1);
                                dgList.Children.Add(_lable);
                                dgList.RegisterName(_lable.Name, _lable);

                                Label _lable2 = new Label();
                                //_lable2.Content = result.Rows[i]["RACK_CNT"].ToString() + "(" + result.Rows[i]["HOLD_CNT"].ToString() + ")";
                                _lable2.Content = result.Rows[i]["RACK_CNT"].ToString() + "(" + result.Rows[i]["SHIP_CNT"].ToString() + ")";
                                _lable2.FontSize = 12;
                                    
                                _lable2.HorizontalContentAlignment = HorizontalAlignment.Center;
                                _lable2.VerticalContentAlignment = VerticalAlignment.Center;
                                _lable2.Margin = new Thickness(0, 0, 0, 0);
                                _lable2.Padding = new Thickness(0, 0, 0, 0);
                                _lable2.BorderThickness = new Thickness(1, 1, 1, 1);
                                _lable2.Width = 40;
                                _lable2.Height = 22;                               
                                _lable2.Tag = result.Rows[i]["RACK_CNT"].ToString();
                                _lable2.Name = "VALUE" + result.Rows[i]["RACK_ID"].ToString();
                                _lable2.Background = new SolidColorBrush(getColor(  result.Rows[i]["RACK_CNT"].ToString(),
                                                                                    result.Rows[i]["STAT"].ToString()                                   
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
                            StocListSearch(dgResult); //양극
                            //StocListSearch("A", dgResult_A); //음극
                        }
                        catch (Exception ex2)
                        {
                            Util.MessageException(ex2);
                            return;
                        }
                    }
                    HiddenLoadingIndicator();
                    _timer.Start();
                    _timer.Tick += new EventHandler(timer_Tick);

                    return;
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private Color getColor(string sCNT, string STAT)
        {
            Color cReturn = Colors.WhiteSmoke;

            if (sCNT != "0")
            {
                switch(STAT)
                {
                    case "전체출고":
                        cReturn = Colors.DarkSeaGreen;
                        break;
                    case "전체불출고":
                        cReturn = Colors.Red;
                        break;
                    case "부분출고":
                        cReturn = Colors.Yellow;
                        break;
                }
            }

            return cReturn;
        }

        private void StocListSearch(C1DataGrid dg)
        {
            try
            {
                seleted_rack = "";
                RACK_ID = "";

                C1DataGrid dgGrid = dg;

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("WH_ID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("CSTID", typeof(string));               
                IndataTable.Columns.Add("RACK_ID", typeof(string));
                IndataTable.Columns.Add("LANGID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["WH_ID"] = Util.GetCondition(cboWH);
                Indata["PRODID"] = Util.GetCondition(txtProdID) == "" ? null : Util.GetCondition(txtProdID);
                Indata["CSTID"] = Util.GetCondition(txtSkidID) == "" ? null : Util.GetCondition(txtSkidID);
                Indata["RACK_ID"] = null;
                Indata["LANGID"] = LoginInfo.LANGID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_MONIT_INFO", "RQSTDT", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count > 0)
                {
                    dgGrid.ItemsSource = DataTableConverter.Convert(dtMain);
                    dtTemp = dtMain;
                    string rack_id = string.Empty;

                    if (mainSearch_flag)
                    {
                        dtMainSearch = dtMain;

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
                        //전체 Layout의 rackID 색깔 초기화
                        for (int i = 0; i < dtMainSearch.Rows.Count; i++)
                        {
                            rack_id = dtMainSearch.Rows[i]["RACK_ID"].ToString();

                            Label seleted_Label = dgList.FindName("RACK" + rack_id) as Label;

                            if (seleted_Label != null)
                            {
                                seleted_Label.Background = new SolidColorBrush(Colors.Bisque);
                            }
                        }

                        //for (int i = 0; i < dtMain.Rows.Count; i++)
                        //{
                        //    rack_id = dtMain.Rows[i]["RACK_ID"].ToString();

                        //    Label seleted_Label = dgList.FindName("RACK" + rack_id) as Label;

                        //    if (seleted_Label != null)
                        //    {
                        //        seleted_Label.Background = new SolidColorBrush(Colors.Bisque);
                        //    }
                        //}

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
        #endregion

    }
}
