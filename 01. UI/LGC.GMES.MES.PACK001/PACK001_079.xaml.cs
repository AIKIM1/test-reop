/*************************************************************************************
 Created Date : 2021.03.22
      Creator : 김민석
   Decription : CELL 공급 프로젝트 조립 화면
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_079 : UserControl, IWorkArea
    {
        //2020.06.26
        public bool bClick = false;
        string sCellAreaId = string.Empty;

        #region Declaration & Constructor & Init

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        CommonCombo _combo = new CommonCombo();

        #region Initialize 
        public PACK001_079()
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


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;

            Initialize();

        }

        private void Initialize()
        {
            try
            {
                InitCombo();

                // 반제품/제품전환으로 인한 
                if (LoginInfo.CFG_AREA_ID.Contains("P"))
                {
                    this.dgCellPlan.Columns["ASSY_MTRLID"].Visibility = Visibility.Collapsed;
                    this.dgCellPlan.Columns["PACK_MTRLID"].Visibility = Visibility.Visible;
                    this.dgCellStock.Columns["ASSY_MTRLID"].Visibility = Visibility.Collapsed;
                    this.dgCellStock.Columns["PACK_MTRLID"].Visibility = Visibility.Visible;
                }
                else
                {
                    this.dgCellPlan.Columns["ASSY_MTRLID"].Visibility = Visibility.Visible;
                    this.dgCellPlan.Columns["PACK_MTRLID"].Visibility = Visibility.Collapsed;
                    this.dgCellStock.Columns["ASSY_MTRLID"].Visibility = Visibility.Visible;
                    this.dgCellStock.Columns["PACK_MTRLID"].Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }

        #endregion 

        private void InitCombo()
        {
            try
            {
                String[] sFiltercboAreaRslt = { LoginInfo.CFG_SHOP_ID, Area_Type.ASSY };
                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sFilter: sFiltercboAreaRslt, sCase: "cboAreaByAreaType");
            }
            catch (Exception ex)
            {

            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion Declaration & Constructor & Init

        #region Event

        #region Button
        #region 조회 버튼 클릭
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (bClick == false)
                {
                    ShowLoadingIndicator();
                    HideTestMode();
                    sCellAreaId = cboArea.SelectedValue.ToString();
                    int iToday = rdoToday.IsChecked == true ? 1 : 0;
                    Refresh();
                    SearchData(sCellAreaId, iToday);
                    HiddenLoadingIndicator();
                }
            }
            catch (Exception ex)
            {
                bClick = false;
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 팩 동 더블클릭 - 요청현황 조회 이벤트
        private void dgCellPlan_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (bClick == false)
                {
                    Point pnt = e.GetPosition(null);
                    C1.WPF.DataGrid.DataGridCell cell = dgCellPlan.GetCellFromPoint(pnt);

                    if (cell != null)
                    {
                        if (cell.Column.Name == "PACK_AREANAME" && !(Util.NVC(DataTableConverter.GetValue(dgCellPlan.Rows[cell.Row.Index].DataItem, "PACK_AREAID")) == null))
                        {
                            ShowTestMode();
                            string sPackBldg = Util.NVC(DataTableConverter.GetValue(dgCellPlan.Rows[cell.Row.Index].DataItem, "PACK_AREAID"));
                            string sCellPrjt = Util.NVC(DataTableConverter.GetValue(dgCellPlan.Rows[cell.Row.Index].DataItem, "CELL_PRJT"));

                            string sProdId = Util.NVC(DataTableConverter.GetValue(dgCellPlan.Rows[cell.Row.Index].DataItem, "MTRLID"));

                            int iToday = rdoToday.IsChecked == true ? 1 : 0;
                            SearchData2(sPackBldg, sCellPrjt, sProdId, iToday);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                bClick = false;
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 공급 버튼 클릭 이벤트
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PACK001_079_REQUESTCONF popup = new PACK001_079_REQUESTCONF();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index - 3;
                    DataTable dtReqCond = new DataTable();
                    dtReqCond = DataTableConverter.Convert(dgCellPlan.ItemsSource);

                    if (dtReqCond.Rows[index]["REQ_YN"].ToString().Equals(0))
                    {
                        Util.MessageInfo("SFU4940");//요청내용이 없습니다.
                        return;
                    }

                    object[] Parameters = new object[4];
                    Parameters[0] = dtReqCond.Rows[index]["PACK_AREAID"].ToString();
                    Parameters[1] = dtReqCond.Rows[index]["CELL_PRJT"].ToString();
                    Parameters[2] = dtReqCond.Rows[index]["CELL_AREAID"].ToString();
                    Parameters[3] = dtReqCond.Rows[index]["MTRLID"].ToString();

                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                    //popup.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 거절 버튼 클릭 이벤트
        private void btnReject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PACK001_079_REJECT popup = new PACK001_079_REJECT();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = null;
                    C1WindowExtension.SetParameters(popup, Parameters);

                    //popup.Closed -= popup_Closed;
                    //popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                    //popup.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion Button

        #region Grid
        private void dgPlan_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (!(e.Cell.Row.Index > 2)) return;

                    if (e.Cell.Column.Name != null && e.Cell.Column.Name.Contains("CELL_STAT") && e.Cell.Value != null)
                    {
                        char sp = '/';
                        string strTempValue = string.Empty;
                        strTempValue = e.Cell.Value.ToString().Substring(e.Cell.Value.ToString().IndexOf("(") + 1, e.Cell.Value.ToString().IndexOf(")") - (e.Cell.Value.ToString().IndexOf("(") + 1));
                        string[] spstring = strTempValue.Split(sp);
                        spstring[1].ToString();

                        if (spstring[0].ToString().Equals("0") && spstring[1].ToString().Equals("0"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                            dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.Black);
                            dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.Black);
                        }
                        else if (spstring[1].ToString().Equals(spstring[0].ToString()))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                            dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.Black);
                            dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.Black);
                        }
                        else
                        {

                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                            dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.Black);
                            dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.Black);
                        }
                    }
                    if (e.Cell.Column.Name == "PACK_AREANAME")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCellStock_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {

                if (sender == null)
                    return;

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null || e.Cell.Column.Name == null && e.Cell.Value == null)
                    {
                        return;
                    }

                    int i = 0;

                    //if (e.Cell.Column.Name.Contains("LINE") && e.Cell.Value.ToString() == "TOTAL")
                    //{
                    //    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                    //    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    //    e.Cell.Row.Presenter.FontWeight = FontWeights.UltraBold;
                    //    return;

                    //}

                    if (e.Cell.Column.Name.Contains("AREAID") || e.Cell.Column.Name.Contains("AREANAME") || e.Cell.Column.Name.Contains("MTRLID") || e.Cell.Column.Name.Contains("CELL_PRJT_NAME") || e.Cell.Column.Name.Contains("AVAQTY") || e.Cell.Column.Name.Contains("INPUT_QTY"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;

                        if (e.Cell.Row.Index == 3 && (e.Cell.Column.Name.Contains("AVAQTY") || e.Cell.Column.Name.Contains("INPUT_QTY")))
                        {
                            e.Cell.Presenter.FontWeight = FontWeights.UltraBold;
                        }

                        return;
                    }

                    // RTS Plan 배경색 제외
                    if (dgCellStock.GetCell(e.Cell.Row.Index, 7).Value.ToString().Equals("PLAN"))
                    {
                        return;
                    }

                    if (e.Cell.Row.Type == DataGridRowType.Item && int.TryParse(e.Cell.Column.Name.Substring(1, 1), out i))
                    {

                        if (Util.NVC_Decimal(e.Cell.Value) > 0)
                        {

                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                            dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.Black);
                            dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.Black);

                            if (e.Cell.Row.Index == 3)
                            {
                                e.Cell.Presenter.FontWeight = FontWeights.UltraBold;
                            }

                            return;
                        }
                        else if (Util.NVC_Decimal(e.Cell.Value) == 0)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                            dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.Black);
                            dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.Black);

                            if (e.Cell.Row.Index == 3)
                            {
                                e.Cell.Presenter.FontWeight = FontWeights.UltraBold;
                            }

                            return;
                        }
                        else
                        {

                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                            dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.Black);
                            dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.Black);

                            for (int j = 1; 3 >= j; j++)
                            {
                                if (int.TryParse(dgCellStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Column.Name.Substring(1, 1), out i) && !dgCellStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Column.Name.Contains("CELL"))
                                {
                                    if (dgCellStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Value.GetDecimal() > 0)
                                    {
                                        if (dgCellStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter == null) return;

                                        dgCellStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                        dgCellStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                        dgCellStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                        dgCellStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                        dgCellStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                        dgCellStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                    }
                                }
                                else if (Util.NVC_Decimal(e.Cell.Value) < 0)
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                    dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.Black);
                                    dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.Black);
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                    dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.Black);
                                    dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.Black);
                                }

                                if (e.Cell.Row.Index == 3)
                                {
                                    e.Cell.Presenter.FontWeight = FontWeights.UltraBold;
                                }
                            }
                            return;

                        }
                    }
                }));


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion Grid

        #endregion Event

        #region Method

        #region 응답현황 조회
        private void SearchData(string cellAreaId, int iToday)
        {
            try
            {
                
                #region 임시 데이터
                //DataTable inTable = new DataTable();
                //DataTable dt_Return = new DataTable();

                //inTable.Columns.Add("AREAID", typeof(string));
                //inTable.Columns.Add("MTRLID", typeof(string));
                //inTable.Columns.Add("CELL_PRJT", typeof(string));
                //inTable.Columns.Add("CELL_AVAIL_QTY", typeof(Int64));
                //inTable.Columns.Add("CELL_QA_QTY", typeof(Int64));
                //inTable.Columns.Add("CELL_HOLD_QTY", typeof(Int64));
                //inTable.Columns.Add("PACK_BLDG", typeof(string));
                //inTable.Columns.Add("INTRANSIT_QTY", typeof(Int64));
                //inTable.Columns.Add("PACK_AVAIL_QTY", typeof(Int64));
                //inTable.Columns.Add("INPUT_QTY", typeof(Int64));
                //inTable.Columns.Add("DAILY_REQD_TOTAL", typeof(Int64));
                //inTable.Columns.Add("DAILTY_REP_TOTAL", typeof(Int64));
                //inTable.Columns.Add("CELL_STAT_REQ", typeof(string));
                //inTable.Columns.Add("CELL_STAT_SHP", typeof(string));

                //inTable.Rows.Add("C1", "ACEN1078I-A1-C04", mboCellPjt.SelectedItemsToString.Split(':')[0].ToString(), 56000, 350, 800, "P7", 3600, 17900, 10000, 6120, 2000, "CONFIRM(7/7)", "SHIP(7/1)");
                //inTable.Rows.Add("C1", "ACEN1078I-A1-C04", mboCellPjt.SelectedItemsToString.Split(':')[0].ToString(), 56000, 350, 800, "P8", 3600, 17900, 10000, 6120, 2000, "CONFIRM(5/0)", "SHIP(0/0)");

                //inTable.Rows.Add("C2", "ACEN1078I-A1-C03", mboCellPjt.SelectedItemsToString.Contains(':') ? mboCellPjt.SelectedItemsToString.Split(':')[1].ToString() : mboCellPjt.SelectedItemsToString.Split(',')[0].ToString(), 76000, 470, 950, "P6", 4200, 22300, 15000, 9920, 3120, "CONFIRM(10/10)", "SHIP(10/0)");
                //inTable.Rows.Add("C2", "ACEN1078I-A1-C03", mboCellPjt.SelectedItemsToString.Contains(':') ? mboCellPjt.SelectedItemsToString.Split(':')[1].ToString() : mboCellPjt.SelectedItemsToString.Split(',')[0].ToString(), 76000, 470, 950, "P7", 4200, 22300, 15000, 9920, 3120, "CONFIRM(10/10)", "SHIP(10/10)");
                //inTable.Rows.Add("C2", "ACEN1078I-A1-C03", mboCellPjt.SelectedItemsToString.Contains(':') ? mboCellPjt.SelectedItemsToString.Split(':')[1].ToString() : mboCellPjt.SelectedItemsToString.Split(',')[0].ToString(), 76000, 470, 950, "PA", 4200, 22300, 15000, 9920, 3120, "CONFIRM(10/10)", "SHIP(10/2)");

                ////string[] grCode = new string[1] { "CELL_REQ_STAT" };

                ////string[] colName = inTable.Columns.Cast<DataColumn>()
                ////                                                    .Where(x => !(x.ColumnName.Contains("CONFIRM")) && !(x.ColumnName.Contains("CANCEL")))
                ////                                                    .Select(x => x.ColumnName)
                ////                                                    .ToArray();

                ////dt_Return = Util.GetGroupBySum(inTable, colName, grCode, false);


                //Util.GridSetData(dgCellPlan, inTable, FrameOperation);

                //HiddenLoadingIndicator();
                #endregion
                if (mboCellPjt.SelectedItems.Count == 0 || cboArea.SelectedItem == null)
                {
                    bClick = false;
                    HiddenLoadingIndicator();
                    Util.Alert("SFU1651");
                    return;
                }


                char sp = ':';
                object[] arrCellPjt = mboCellPjt.SelectedItems.ToArray();
                string[] strCellTemp = null;
                strCellTemp = arrCellPjt.Cast<string>().ToArray();
                string[] strCellPrj = new string[1];
                string[] strCellProd = new string[1];
                for (int i = 0; i < strCellTemp.Count(); i++)
                {
                    strCellPrj[0] = strCellPrj[0] + ',' + strCellTemp[i].Split(sp)[0];
                    strCellProd[0] = strCellProd[0] + ',' + strCellTemp[i].Split(sp)[1];
                }

                DataSet ds = new DataSet();

                DataTable indata = new DataTable();
                indata.TableName = "INDATA";

                indata.Columns.Add("MTRLID", typeof(string));
                indata.Columns.Add("CELL_AREAID", typeof(string));
                indata.Columns.Add("LANGID", typeof(string));
                indata.Columns.Add("TODAY", typeof(string));

                DataRow dr = indata.NewRow();
                dr["MTRLID"] = strCellProd[0];
                dr["CELL_AREAID"] = cellAreaId;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TODAY"] = iToday;


                indata.Rows.Add(dr);

                ds.Tables.Add(indata);

                new ClientProxy().ExecuteService_Multi("BR_PRD_SEL_ASSY_SPLY_STATUS", "INDATA", "RSLTDT", (dsRslt, bizException) =>
                {
                    try
                    {
                        bClick = true;

                        ShowLoadingIndicator();
                        dgCellPlan.LoadedCellPresenter += dgPlan_LoadedCellPresenter;

                        if (bizException != null)
                        {
                            bClick = false;
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.GridSetData(dgCellPlan, dsRslt.Tables["RSLTDT"], FrameOperation, false);
                    }
                    catch (Exception ex)
                    {
                        bClick = false;
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        bClick = false;
                        HiddenLoadingIndicator();
                        dgCellPlan.LoadedCellPresenter -= dgPlan_LoadedCellPresenter;
                    }

                }, ds);

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                bClick = false;
                HiddenLoadingIndicator();
                Refresh();
                Util.MessageException(ex);
            }

        }
        #endregion

        #region Pack 동별 생산현황 조회
        private void SearchData2(string sPackBldg, string sCellPrjt, string sProdId, int today)
        {
            try
            {
                #region  임시 데이터
                //DataTable inTable2 = new DataTable();
                //inTable2.Columns.Add("AREAID", typeof(string));
                //inTable2.Columns.Add("CELL_PRJT", typeof(string));
                //inTable2.Columns.Add("CELLID", typeof(string));
                //inTable2.Columns.Add("PACK_PRJT", typeof(string));
                //inTable2.Columns.Add("CELL_AVA_QTY", typeof(Int64));
                //inTable2.Columns.Add("DAILTY_INPUT_TOTAL", typeof(Int64));
                //inTable2.Columns.Add("LINE", typeof(string));
                //inTable2.Columns.Add("7", typeof(string));
                //inTable2.Columns.Add("8", typeof(string));
                //inTable2.Columns.Add("9", typeof(string));
                //inTable2.Columns.Add("10", typeof(string));
                //inTable2.Columns.Add("11", typeof(string));
                //inTable2.Columns.Add("12", typeof(string));
                //inTable2.Columns.Add("13", typeof(string));
                //inTable2.Columns.Add("14", typeof(string));
                //inTable2.Columns.Add("15", typeof(string));
                //inTable2.Columns.Add("16", typeof(string));
                //inTable2.Columns.Add("17", typeof(string));
                //inTable2.Columns.Add("18", typeof(string));
                //inTable2.Columns.Add("19", typeof(string));
                //inTable2.Columns.Add("20", typeof(string));
                //inTable2.Columns.Add("21", typeof(string));
                //inTable2.Columns.Add("22", typeof(string));
                //inTable2.Columns.Add("23", typeof(string));
                //inTable2.Columns.Add("24", typeof(string));
                //inTable2.Columns.Add("1", typeof(string));
                //inTable2.Columns.Add("2", typeof(string));
                //inTable2.Columns.Add("3", typeof(string));
                //inTable2.Columns.Add("4", typeof(string));
                //inTable2.Columns.Add("5", typeof(string));
                //inTable2.Columns.Add("6", typeof(string));

                //if (sPackBldg == "P6")
                //{
                //    inTable2.Rows.Add("P6", "E78-C04", "ACEN1078I-A1-C04", "MEB2", 17900, 24000, "TOTAL", "16,000", "14,000", "12,000", "10,000", "8,000", "6,000"
                //                        , "4,000", "2,000", "1,000", "500", "0", "-2,000", "-4,000", "-6,000", "-8,000", "-10,000", "-12,000", "-14,000"
                //                        , "-16,000", "-18,000", "-20,000", "-22,000", "-24,000", "-26,000");
                //    inTable2.Rows.Add("P6", "E78-C04", "ACEN1078I-A1-C04", "MEB2", 10000, 10000, "M#3", "9,583", "9,167", "8,750", "8,333", "7,917", "7,500"
                //                        , "7,083", "6,667", "6,250", "5,833", "5,417", "4,583", "4,167", "4,750", "3,333", "2,917", "2,500", "2,083"
                //                        , "1,667", "1,250", "833", "417", "0", "-417");
                //    inTable2.Rows.Add("P6", "E78-C04", "ACEN1078I-A1-C04", "MEB2", 7000, 10000, "M#4", "24,000", "21,000", "18,000", "15,000", "12,000", "9,000"
                //                       , "6,000", "3,000", "0", "-3,000", "-6,000", "-9,000", "-12,000", "-15,000", "-18,000", "-21,000", "-24,000", "-27,000"
                //                       , "-30,000", "-33,000", "-36,000", "-39,000", "-42,000", "-45,000");
                //    inTable2.Rows.Add("P6", "E78-C04", "ACEN1078I-A1-C04", "MEB2", 900, 4000, "M#5", "16,000", "14,000", "12,000", "10,000", "8,000", "6,000"
                //                       , "4,000", "2,000", "1,000", "500", "0", "-2,000", "-4,000", "-6,000", "-8,000", "-10,000", "-12,000", "-14,000"
                //                       , "-16,000", "-18,000", "-20,000", "-22,000", "-24,000", "-26,000");
                //}
                //else if (sPackBldg == "P7")
                //{
                //    inTable2.Rows.Add("P7", "E78-C03", "ACEN1078I-A1-C03", "MEB2", 17900, 24000, "TOTAL", "30,000", "28,000", "26,000", "24,000", "22,000", "20,000"
                //                       , "18,000", "16,000", "14,000", "12,000", "10,000", "8,000", "6,000", "4,000", "2,000", "0", "-2,000", "-4,000"
                //                       , "-6,000", "-8,000", "-10,000", "-12,000", "-14,000", "-16,000");
                //    inTable2.Rows.Add("P7", "E78-C03", "ACEN1078I-A1-C03", "MEB2", 17900, 24000, "M#1", "30,000", "28,000", "26,000", "24,000", "22,000", "20,000"
                //                       , "18,000", "16,000", "14,000", "12,000", "10,000", "8,000", "6,000", "4,000", "2,000", "0", "-2,000", "-4,000"
                //                       , "-6,000", "-8,000", "-10,000", "-12,000", "-14,000", "-16,000");
                //    inTable2.Rows.Add("P7", "E78-C03", "ACEN1078I-A1-C03", "MEB2", 17900, 24000, "M#2", "16,000", "14,000", "12,000", "10,000", "8,000", "6,000"
                //                       , "4,000", "2,000", "1,000", "500", "0", "-2,000", "-4,000", "-6,000", "-8,000", "-10,000", "-12,000", "-14,000"
                //                       , "-16,000", "-18,000", "-20,000", "-22,000", "-24,000", "-26,000");
                //}
                //else
                //{
                //    inTable2.Rows.Add("PA", "E78-C03", "ACEN1078I-A1-C03", "EV2020", 17900, 24000, "TOTAL", "16,000", "14,000", "12,000", "10,000", "8,000", "6,000"
                //                       , "4,000", "2,000", "1,000", "500", "0", "-2,000", "-4,000", "-6,000", "-8,000", "-10,000", "-12,000", "-14,000"
                //                       , "-16,000", "-18,000", "-20,000", "-22,000", "-24,000", "-26,000");
                //    inTable2.Rows.Add("PA", "E78-C03", "ACEN1078I-A1-C03", "EV2020", 17900, 24000, "M#1", "16,000", "14,000", "12,000", "10,000", "8,000", "6,000"
                //                       , "4,000", "2,000", "1,000", "500", "0", "-2,000", "-4,000", "-6,000", "-8,000", "-10,000", "-12,000", "-14,000"
                //                       , "-16,000", "-18,000", "-20,000", "-22,000", "-24,000", "-26,000");
                //}

                //dgCellStock.LoadedCellPresenter += dgStock_LoadedCellPresenter;
                //Util.GridSetData(dgCellStock, inTable2, FrameOperation);
                //dgCellStock.LoadedCellPresenter -= dgStock_LoadedCellPresenter;

                //HiddenLoadingIndicator();
                #endregion
                ShowLoadingIndicator();

                DataSet ds = new DataSet();

                DataTable indata = new DataTable();
                indata.TableName = "INDATA";

                indata.Columns.Add("PRODID", typeof(string));
                indata.Columns.Add("CELL_PRJT", typeof(string));
                indata.Columns.Add("AREAID", typeof(string));
                indata.Columns.Add("TODAY", typeof(string));
                indata.Columns.Add("LANGID", typeof(string));

                DataRow dr = indata.NewRow();
                dr["PRODID"] = sProdId;
                dr["CELL_PRJT"] = sCellPrjt;
                dr["AREAID"] = sPackBldg;
                dr["TODAY"] = today;
                dr["LANGID"] = LoginInfo.LANGID;

                indata.Rows.Add(dr);

                ds.Tables.Add(indata);

                new ClientProxy().ExecuteService_Multi("BR_PRD_SEL_PACK_PRDT_STATUS", "INDATA", "REQ_CELL", (dsRslt, bizException) =>
                {
                    try
                    {
                        bClick = true;

                        ShowLoadingIndicator();
                        dgCellStock.LoadedCellPresenter += dgCellStock_LoadedCellPresenter;

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.GridSetData(dgCellStock, dsRslt.Tables["REQ_CELL"], FrameOperation, false);
                    }
                    catch (Exception ex)
                    {
                        bClick = false;
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        bClick = false;
                        HiddenLoadingIndicator();
                        dgCellStock.LoadedCellPresenter -= dgPlan_LoadedCellPresenter;
                    }




                }, ds);

                HiddenLoadingIndicator();

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                bClick = false;
                Refresh2();
                Util.MessageException(ex);
            }

        }
        #endregion

        #region Refresh
        private void Refresh()
        {
            try
            {
                //그리드 clear
                bClick = false;
                Util.gridClear(dgCellPlan);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Refresh2()
        {
            try
            {
                //그리드 clear
                bClick = false;
                Util.gridClear(dgCellStock);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Refresh

        #region 생산현황 닫기
        private void dgCellStock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            bClick = false;
            HideTestMode();
        }
        #endregion


        #region 팝업 닫기
        void popup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_079_REQUESTCONF popup = sender as PACK001_079_REQUESTCONF;
                if (popup.DialogResult == MessageBoxResult.Cancel)
                {
                    Refresh();
                    int iToday = rdoToday.IsChecked == true ? 1 : 0;
                    SearchData(sCellAreaId, iToday);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion Method


        #region [ Animation ]
        private void showTestAnimationCompleted(object sender, EventArgs e)
        {
            //ColorAnimationInredRectangle();
        }

        private void ShowTestMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (GridArea.RowDefinitions[2].Height.Value > 0 && GridArea.RowDefinitions[3].Height.Value > 0) return;

                GridArea.RowDefinitions[2].Height = new GridLength(34, GridUnitType.Pixel);
                GridArea.RowDefinitions[3].Height = new GridLength(1, GridUnitType.Star);
                //LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                //gla.From = new GridLength(0, GridUnitType.Star);
                //gla.To = new GridLength(1, GridUnitType.Star);
                //gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                //gla.Completed += showTestAnimationCompleted;
                //cellSupply.RowDefinitions[4].BeginAnimation(RowDefinition.HeightProperty, gla);

                //ColorAnimationInredRectangle();
            }));

            //bTestMode = true;
        }

        private void HideTestMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (!bTestMode) return;
                if (GridArea.RowDefinitions[1].Height.Value <= 0) return;

                GridArea.RowDefinitions[2].Height = new GridLength(0);
                GridArea.RowDefinitions[3].Height = new GridLength(0);
                //LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                //gla.From = new GridLength(1, GridUnitType.Star);
                //gla.To = new GridLength(0, GridUnitType.Star);
                //gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                ////  gla.Completed += HideTestAnimationCompleted;
                //cellSupply.RowDefinitions[4].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));
        }


        #endregion

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string sSelectedValue = cboArea.SelectedValue.ToString();
                string[] sSelectedList = sSelectedValue.Split(',');
                char sp = ':';

                DataTable INDATA = new DataTable();
                INDATA.Columns.Add("SHOPID", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("CMCDTYPE", typeof(string));
                INDATA.Columns.Add("COND", typeof(Int32));

                DataRow dr = INDATA.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "CSLY_CELL_PRJT";
                dr["COND"] = 1;

                INDATA.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_CHK_CMMCD_CBO", "INDATA", "OUTDATA", INDATA);

                mboCellPjt.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                    {
                        for (int j = 0; j < sSelectedList.Length; j++)
                        {
                            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                            {
                                mboCellPjt.Check(i);
                                break;
                            }
                        }
                    }
                    else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_EQSG_ID)
                    {
                        mboCellPjt.Check(i);
                        break;
                    }
                }
            }

            catch (Exception ex)
            {

            }
        }


    }
}