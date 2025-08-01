﻿/*************************************************************************************
 Created Date : 2023.05.02
      Creator : 윤지해
   Decription : 노칭대기창고 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.02  윤지해   E20230202-000262    최초 생성. 라미대기창고 모니터링과 동일하게 노칭대기창고 모니터링 추가





 
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
    public partial class COM001_378 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public Boolean ReFlash = false;

        DataTable dtTemp;
        DataTable dtTemp_A;

        string WH_ID = string.Empty;
        string RACK_ID = string.Empty;
        string seleted_rack = string.Empty;
        int refresh_cnt = 1;

        // 모니터링용 타이머
        private System.Windows.Threading.DispatcherTimer _timer = null;

        public COM001_378()
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
            Search_Status();
            RACK_ID = "";
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

        private void btnLotID_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_207_LOTLIST popup_LOT = new COM001_207_LOTLIST();
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
                Util.MessageException(ex);
            }
        }

        private void btnPrj_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_207_PJTLIST popup_PRJ = new COM001_207_PJTLIST();
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
                Util.MessageException(ex);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Search_Status();
        }
        #endregion

        #region POPUP EVENT
        void popup_LOT_Closed(object sender, EventArgs e)
        {
            try
            {
                COM001_207_LOTLIST popup = sender as COM001_207_LOTLIST;
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

        void popup_PRJ_Closed(object sender, EventArgs e)
        {
            try
            {
                COM001_207_PJTLIST popup = sender as COM001_207_PJTLIST;
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
                            dataGrid.GetCell(e.Cell.Row.Index, dgResult.Columns["LOTID"].Index).Presenter.Background = new SolidColorBrush(Colors.DarkSeaGreen);
                        }
                        else if (dtTemp.Rows[e.Cell.Row.Index]["FIFO_GUBUN"].ToString() == "N")
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, dgResult.Columns["LOTID"].Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                        }

                        if (dtTemp.Rows[e.Cell.Row.Index]["WIPHOLD"].ToString() == "Y")
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
            COM001_378_LOTS_IN_RACK wndPopup = new COM001_378_LOTS_IN_RACK(lblRack.Content.ToString(), Util.GetCondition(cboArea), Util.GetCondition(cboFloor));

            wndPopup.FrameOperation = FrameOperation;
            wndPopup.Show();
        }
        #endregion

        #region Mehod
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

        private void ClearGrid()
        {
            try
            {
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
                dr["AREAID"] = Util.GetCondition(cboArea);
                dr["WH_ID"] = Util.GetCondition(cboFloor);
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("BR_PRD_GET_WH_PANCAKE_LIST_WITH_FIFO", "INDATA", "OUTDATA", RQSTDT, (result, ex) =>
                {
                    ClearGrid();
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
                            StocListSearch("C", dgResult); //양극
                            StocListSearch("A", dgResult_A); //음극
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

        private Color getColor(string sCnt, string sMax, string fifo_gubun, string hold_cnt)
        {
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

        private void StocListSearch(string elec_type, C1DataGrid dg)
        {
            try
            {
                C1DataGrid dgGrid = dg;

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("WIPHOLD", typeof(string));
                IndataTable.Columns.Add("WH_ID", typeof(string));
                IndataTable.Columns.Add("PJT", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("ELEC_TYPE", typeof(string));
                IndataTable.Columns.Add("RACK_ID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = Util.GetCondition(cboArea);
                if ((bool)chkHold.IsChecked) Indata["WIPHOLD"] = "N";

                Indata["WH_ID"] = Util.GetCondition(cboFloor);
                Indata["PJT"] = txtPrj.Text == "" ? null : txtPrj.Tag.ToString();
                Indata["LOTID"] = Util.GetCondition(txtLotID) == "" ? null : Util.GetCondition(txtLotID);
                Indata["ELEC_TYPE"] = elec_type == "" ? null : elec_type;
                Indata["RACK_ID"] = null;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_NT_STANDBY_PANCAKE_LIST", "INDATA", "OUTDATA", IndataTable);

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
        #endregion

    }
}
