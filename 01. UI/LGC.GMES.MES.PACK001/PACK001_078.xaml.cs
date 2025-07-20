/*************************************************************************************
 Created Date : 2021.03.21
      Creator : 김민석
   Decription : CELL 공급 프로젝트 PACK화면
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

//#define TEST

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
using System.Windows.Media.Animation;


namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_078 : UserControl, IWorkArea
    {
        public bool bClick = false;

        #region Declaration & Constructor & Init
        bool _popup = true;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        CommonCombo _combo = new CommonCombo();

        #region Initialize 
        public PACK001_078()
        {
            try
            {
                InitializeComponent();
                InitCombo();

                //this.Loaded += UserControl_Loaded;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            bClick = false;
            this.Loaded -= UserControl_Loaded;
        }


        #endregion 

        private void InitCombo()
        {
            try
            {
                String[] sFiltercboAreaRslt = { Area_Type.PACK };
                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sFilter: sFiltercboAreaRslt, sCase: "AREA_PACK");
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
                    Refresh();
                    HideTestMode();
                    int iToday = rdoToday.IsChecked == true ? 1 : 0;
                    SearchData(iToday);
                }
            }
            catch (Exception ex)
            {
                bClick = false;
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 공급유형
        private void btnSupplyType_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //PACK001_078_CELLREQUEST popup = new PACK001_078_CELLREQUEST();
                //popup.FrameOperation = this.FrameOperation;

                //if (popup != null)
                //{
                //    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index - 3;
                //    DataTable dtReqCond = new DataTable();
                //    dtReqCond = DataTableConverter.Convert(dgPlan.ItemsSource);

                //    object[] Parameters = new object[3];
                //    Parameters[0] = cboArea.SelectedValue;
                //    Parameters[1] = dtReqCond.Rows[index]["MTRLID"].ToString();
                //    Parameters[2] = rdoToday.IsChecked == true ? "1" : "0";
                //    C1WindowExtension.SetParameters(popup, Parameters);

                //    popup.Closed -= popup_Closed;
                //    popup.Closed += popup_Closed;

                //    popup.ShowModal();
                //    popup.CenterOnScreen();

                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 요청 버튼 클릭
        private void btnRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index - 3;
                DataTable dtReqCond = new DataTable();
                dtReqCond = DataTableConverter.Convert(dgPlan.ItemsSource);

                // 수동공급의 경우만 Cell요청 팝업 호출 확인
                if (Util.NVC(dtReqCond.Rows[index]["AUTO_TRF_REQ_FLAG"].ToString()) == "Y")
                {
                    return;
                }

                PACK001_078_CELLREQUEST popup = new PACK001_078_CELLREQUEST();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = cboArea.SelectedValue;
                    Parameters[1] = dtReqCond.Rows[index]["MTRLID"].ToString();
                    Parameters[2] = rdoToday.IsChecked == true ? "1" : "0";
                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;

                    popup.ShowModal();
                    popup.CenterOnScreen();
                   
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region PACK 생산현황 숨기기
        private void dgStock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            bClick = false;
            HideTestMode();
        }
        #endregion

        #region PACK 생산현황 조회 이벤트
        private void dgPlan_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (bClick == false)
                {
                    Point pnt = e.GetPosition(null);
                    C1.WPF.DataGrid.DataGridCell cell = dgPlan.GetCellFromPoint(pnt);

                    if (cell != null)
                    {
                        if (cell.Column.Name == "CELL_PRJT" && !(Util.NVC(DataTableConverter.GetValue(dgPlan.Rows[cell.Row.Index].DataItem, "CELL_PRJT")) == null))
                        {
                            ShowTestMode();
                            string sCellPrjt = Util.NVC(DataTableConverter.GetValue(dgPlan.Rows[cell.Row.Index].DataItem, "CELL_PRJT"));
                            string sPackPjt = Util.NVC(DataTableConverter.GetValue(dgPlan.Rows[cell.Row.Index].DataItem, "PACK_PRJT"));
                            string sProdId = Util.NVC(DataTableConverter.GetValue(dgPlan.Rows[cell.Row.Index].DataItem, "MTRLID"));
                            int iToday = rdoToday.IsChecked == true ? 1 : 0;
                            SearchData2(sProdId, sCellPrjt, sPackPjt, iToday);
                        }

                    }
                }

                loadingIndicator.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                bClick = false;
                HiddenLoadingIndicator();
                Util.MessageException(ex);

            }
        }
        #endregion

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_popup)
                    return;

                Button btn = sender as Button;
                string _tagname = string.Empty;

                int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)
                               ((System.Windows.Controls.Grid)
                               ((System.Windows.Controls.Grid)
                               ((System.Windows.Controls.StackPanel)
                               ((sender as Button).Parent)).Parent).Parent).Parent).Row.Index;
                int colIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)
                               ((System.Windows.Controls.Grid)
                               ((System.Windows.Controls.Grid)
                               ((System.Windows.Controls.StackPanel)
                               ((sender as Button).Parent)).Parent).Parent).Parent).Column.Index;

                string sTagName = btn.Tag.ToString();
                
                PACK001_078_AUTO_COND condpopup = new PACK001_078_AUTO_COND();
                condpopup.FrameOperation = this.FrameOperation;

                if (condpopup != null)
                {
                    DataTable dtReqCond = new DataTable();
                    dtReqCond = DataTableConverter.Convert(dgPlan.ItemsSource);

                    object[] Parameters = new object[3];
                    Parameters[0] = rdoToday.IsChecked == true ? 1 : 0;
                    Parameters[1] = Util.NVC(cboArea.SelectedValue.ToString());
                    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgPlan.Rows[rowIndex].DataItem, "MTRLID"));
                    C1WindowExtension.SetParameters(condpopup, Parameters);

                    condpopup.Closed -= condpopup_Closed;
                    condpopup.Closed += condpopup_Closed;

                    condpopup.ShowModal();
                    condpopup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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

                    if (e.Cell.Column.Name != null && e.Cell.Column.Name == "PACK_AVAIL_QTY")
                    {
                        string _StockColor = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "STOCK_COLOR"));
                        string _StockFColor = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "STOCK_FCOLOR"));

                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(_StockColor));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(_StockFColor));
                    }

                    if (e.Cell.Column.Name != null && (e.Cell.Column.Name.Contains("CELL_I") || (e.Cell.Column.Name.Contains("IWMS_RCV"))))
                    {
                        string _ValueToReq = e.Cell.Value.ToString();
                        if (!_ValueToReq.Equals("0"))
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
                    if (e.Cell.Column.Name == "CELL_PRJT")
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

        private void dgStock_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                    if (e.Cell.Column.Name.Contains("AREAID") || e.Cell.Column.Name.Contains("AREANAME") || e.Cell.Column.Name.Contains("MTRLID") || e.Cell.Column.Name.Contains("CELL_PRJT_NAME") || e.Cell.Column.Name.Contains("AVAQTY") || e.Cell.Column.Name.Contains("INPUT_QTY") || e.Cell.Column.Name.Contains("PACK_PRJT_NAME"))
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
                    if (dgStock.GetCell(e.Cell.Row.Index, 7).Value.ToString().Equals("PLAN"))
                    {
                        return;
                    }

                    if (e.Cell.Row.Type == DataGridRowType.Item && int.TryParse(e.Cell.Column.Name.Substring(1,1), out i) )
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
                        else if(Util.NVC_Decimal(e.Cell.Value) == 0)
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
                                if (int.TryParse(dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Column.Name.Substring(1, 1), out i) && !dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Column.Name.Contains("CELL"))
                                {
                                    if (dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Value.GetDecimal() > 0)
                                    {
                                        if (dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter == null) return;

                                        dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                        dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                        dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                        dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                        dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                        dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
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
                                  //  e.Cell.Presenter.FontWeight = FontWeights.UltraBold;
                                }
                            }
                        }
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

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion Grid

        #region COMBO BOX
        #region COMBO BOX SETTING
        private void SetCboPrj()
        {
            try
            {
                string sSelectedValue = cboArea.SelectedValue.ToString();
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sSelectedValue;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["SYSTEM_ID"] = LoginInfo.SYSID;
                dr["USERID"] = LoginInfo.USERID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJMODEL_AUTH_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboPackPjt.DisplayMemberPath = "CBO_NAME";
                cboPackPjt.SelectedValuePath = "CBO_CODE";

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                    {
                        for (int j = 0; j < sSelectedList.Length; j++)
                        {
                            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                            {
                                cboPackPjt.Check(i);
                                break;
                            }
                        }
                    }
                    else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_EQSG_ID)
                    {
                        cboPackPjt.Check(i);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string sSelectedValue = cboArea.SelectedValue.ToString();
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PACK_PRJ", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("COND", typeof(Int32));
                RQSTDT.Columns.Add("TODAY", typeof(Int32));  


                DataRow dr = RQSTDT.NewRow();
                dr["SYSTEM_ID"] = LoginInfo.SYSID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["COND"] = 4;
                dr["TODAY"] = rdoToday.IsChecked == true ? 0 : 1;

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_SEL_PRJMODEL_CBO", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, exception) => {

                    if (exception != null)
                    {
                        throw exception;
                    }

                    try
                    {
                        cboPackPjt.ItemsSource = DataTableConverter.Convert(dtResult);
                        cboPackPjt.CheckAll();
                    }
                    catch (Exception ex)
                    {
                        //throw ex;
                    }
                });
            }
            catch (Exception ex)
            {
                bClick = false;
            }
        }

        private void cboPackPjt_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                string sSelectedValue = cboArea.SelectedValue.ToString();
                string[] sSelectedList = sSelectedValue.Split(',');

                //선택한 거 없을 경우 validation 걸어야함
                char sp = ':';
                object[] arrPackPjt = cboPackPjt.SelectedItems.ToArray();
                string[] strTempValue = null;
                strTempValue = arrPackPjt.Cast<string>().ToArray();
                string[] strPrj = new string[1];
                string[] strProd = new string[1];
                string[] strCellPrj = new string[1];
                string[] strCellId = new string[1];
                for (int i = 0; i < strTempValue.Count(); i++)
                {
                    strPrj[0] = strPrj[0] + ',' + strTempValue[i].Split(sp)[0];
                    strProd[0] = strProd[0] + ',' + strTempValue[i].Split(sp)[1];
                }
                // string[] spstring = strTempValue.Split(sp);

                DataTable INDATA = new DataTable();
                INDATA.TableName = "RQSTDT";
                INDATA.TableName = "RQSTDT";
                INDATA.Columns.Add("SYSTEM_ID", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("CMCDTYPE", typeof(string));
                INDATA.Columns.Add("SHOPID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("PACK_PRJ", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("COND", typeof(Int32));
                INDATA.Columns.Add("TODAY", typeof(Int16));

                DataRow dr = INDATA.NewRow();
                dr["SYSTEM_ID"] = LoginInfo.SYSID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "CSLY_CELL_PRJT";
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["PACK_PRJ"] = strPrj[0];
                dr["COND"] = 1;
                dr["TODAY"] = rdoToday.IsChecked == true ? 0 : 1;
                INDATA.Rows.Add(dr);

                string _bizRule = "DA_BAS_SEL_CELLMODEL_BY_PLAN_CBO";   // BR_PRD_SEL_CHK_CMMCD_CBO

                new ClientProxy().ExecuteService(_bizRule, "RQSTDT", "RSLTDT", INDATA, (dtResult, exception) =>
                {
                    if (exception != null)
                    {
                        throw exception;
                    }

                    try
                    {
                        cboCellPjt.ItemsSource = DataTableConverter.Convert(dtResult);
                        cboCellPjt.CheckAll();
                    }
                    catch (Exception ex)
                    {
                        //throw ex;
                    }
                });
            }

            catch (Exception ex)
            {
                bClick = false;
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion Event

        #region Method

        #region 요청현황 조회
        private void SearchData(int today)
        {
            try
            {
                
                if (cboCellPjt.SelectedItems.Count < 1 || cboPackPjt.SelectedItems.Count < 1)
                {
                    bClick = false;
                    Util.Alert("SFU1651");//선택된 항목이 없습니다.
                    return;
                }

                #region TEST 데이터
                //임시 데이터
                //DataTable inTable = new DataTable();
                //DataTable dt_Return = new DataTable();

                //inTable.Columns.Add("AREAID", typeof(string));
                //inTable.Columns.Add("CELLID", typeof(string));
                //inTable.Columns.Add("CELL_PRJT", typeof(string));
                //inTable.Columns.Add("PACK_PRJT", typeof(string));
                //inTable.Columns.Add("PACK_LINE", typeof(string));
                //inTable.Columns.Add("CELL_AVA_QTY", typeof(Int64));
                //inTable.Columns.Add("CELL_QA_QTY", typeof(Int64));
                //inTable.Columns.Add("CELL_HOLD_QTY", typeof(Int64));
                //inTable.Columns.Add("INTRANSITQTY", typeof(Int64));
                //inTable.Columns.Add("FP_DAILY_CONF_QTY", typeof(Int64));
                //inTable.Columns.Add("PACK_AVA_QTY", typeof(Int64));
                //inTable.Columns.Add("DAILTY_INPUT_TOTAL", typeof(Int64));
                //inTable.Columns.Add("SUGGEST", typeof(Int64));
                //inTable.Columns.Add("CELL_I_REQ", typeof(string));
                //inTable.Columns.Add("CELL_I_RCV", typeof(string));
                //inTable.Columns.Add("CELL_II_REQ", typeof(string));
                //inTable.Columns.Add("CELL_II_RCV", typeof(string));
                //inTable.Columns.Add("CELL_III_REQ", typeof(string));
                //inTable.Columns.Add("CELL_III_RCV", typeof(string));


                ////inTable.Rows.Add(cboArea.SelectedValue.ToString(), "ACEN1078I-A1-C04", "E78-C04", "MEB2", "M#3", 56000, 350, 800, 3600, 10000, 17900, 24000, 6000, "CONFIRM(24/24)", "RECEIVE(24/10)", "CONFIRM(10/10)", "RECEIVE(10/2)", "CONFIRM(0/0)", "RECEIVE(0/0)");
                ////inTable.Rows.Add(cboArea.SelectedValue.ToString(), "ACEN1078I-A1-C04", "E78-C04", "MEB2", "M#4", 56000, 350, 800, 3600, 10000, 17900, 24000, 6000, "CONFIRM(24/24)", "RECEIVE(24/10)", "CONFIRM(10/10)", "RECEIVE(10/2)", "CONFIRM(0/0)", "RECEIVE(0/0)");
                ////inTable.Rows.Add(cboArea.SelectedValue.ToString(), "ACEN1078I-A1-C04", "E78-C04", "MEB2", "M#5", 56000, 350, 800, 3600, 10000, 17900, 24000, 6000, "CONFIRM(24/24)", "RECEIVE(24/10)", "CONFIRM(10/10)", "RECEIVE(10/2)", "CONFIRM(0/0)", "RECEIVE(0/0)");

                ////inTable.Rows.Add(cboArea.SelectedValue.ToString(), "ACEN1078I-A1-C03", "E78-C03", "MEB2", "M#1", 76000, 470, 950, 4200, 22300, 15000, 35000, 23000, "CONFIRM(15/10)", "RECEIVE(7/2)", "CONFIRM(15/15)", "RECEIVE(15/2)", "CONFIRM(0/0)", "RECEIVE(0/0)");
                ////inTable.Rows.Add(cboArea.SelectedValue.ToString(), "ACEN1078I-A1-C03", "E78-C03", "MEB2", "M#2", 76000, 470, 950, 4200, 22300, 15000, 35000, 23000, "CONFIRM(15/10)", "RECEIVE(7/2)", "CONFIRM(15/15)", "RECEIVE(15/2)", "CONFIRM(0/0)", "RECEIVE(0/0)");


                //inTable.Rows.Add(cboArea.SelectedValue.ToString(), "ACEN1078I-A1-C04", cboCellPjt.SelectedItemsToString.Split(',')[0].ToString(), cboPackPjt.SelectedItemsToString.Split(',')[0].ToString(), "M#3", 56000, 350, 800, 3600, 10000, 17900, 24000, 6000,"CONFIRM(24/24)", "RECEIVE(24/10)", "CONFIRM(10/10)", "RECEIVE(10/2)", "CONFIRM(0/0)", "RECEIVE(0/0)");
                //inTable.Rows.Add(cboArea.SelectedValue.ToString(), "ACEN1078I-A1-C04", cboCellPjt.SelectedItemsToString.Split(',')[0].ToString(), cboPackPjt.SelectedItemsToString.Split(',')[0].ToString(), "M#4", 56000, 350, 800, 3600, 10000, 17900, 24000, 6000,"CONFIRM(24/24)", "RECEIVE(24/10)", "CONFIRM(10/10)", "RECEIVE(10/2)", "CONFIRM(0/0)", "RECEIVE(0/0)");
                //inTable.Rows.Add(cboArea.SelectedValue.ToString(), "ACEN1078I-A1-C04", cboCellPjt.SelectedItemsToString.Split(',')[0].ToString(), cboPackPjt.SelectedItemsToString.Split(',')[0].ToString(), "M#5", 56000, 350, 800, 3600, 10000, 17900, 24000, 6000,"CONFIRM(24/24)", "RECEIVE(24/10)", "CONFIRM(10/10)", "RECEIVE(10/2)", "CONFIRM(0/0)", "RECEIVE(0/0)");

                ////inTable.Rows.Add(cboArea.SelectedValue.ToString(), "ACEN1078I-A1-C03", cboCellPjt.SelectedItemsToString.Contains(',') ? cboCellPjt.SelectedItemsToString.Split(',')[1].ToString() : cboCellPjt.SelectedItemsToString.Split(',')[0].ToString(), cboPackPjt.SelectedItemsToString.Contains(',') ? cboPackPjt.SelectedItemsToString.Split(',')[1].ToString() : cboPackPjt.SelectedItemsToString.Split(',')[0].ToString(), "M#1", 76000, 470, 950, 4200, 22300, 15000, 35000, 23000, "CONFIRM(15/10)", "RECEIVE(7/2)", "CONFIRM(15/15)", "RECEIVE(15/2)", "CONFIRM(0/0)", "RECEIVE(0/0)");
                ////inTable.Rows.Add(cboArea.SelectedValue.ToString(), "ACEN1078I-A1-C03", cboCellPjt.SelectedItemsToString.Contains(',') ? cboCellPjt.SelectedItemsToString.Split(',')[1].ToString() : cboCellPjt.SelectedItemsToString.Split(',')[0].ToString(), cboPackPjt.SelectedItemsToString.Contains(',') ? cboPackPjt.SelectedItemsToString.Split(',')[1].ToString() : cboPackPjt.SelectedItemsToString.Split(',')[0].ToString(), "M#2", 76000, 470, 950, 4200, 22300, 15000, 35000, 23000, "CONFIRM(15/10)", "RECEIVE(7/2)", "CONFIRM(15/15)", "RECEIVE(15/2)", "CONFIRM(0/0)", "RECEIVE(0/0)");
                                                                                                                                                                                                                       
                //string[] grCode = new string[3] { "CELL_I", "CELL_II", "CELL_III" };

                ////string[] colName = inTable.Columns.Cast<DataColumn>()
                ////                                                    .Where(x => !(x.ColumnName.Contains("CELL_I")) && !(x.ColumnName.Contains("CELL_II")) && !(x.ColumnName.Contains("CELL_III")))
                ////                                                    .Select(x => x.ColumnName)
                ////                                                    .ToArray();

                ////dt_Return = Util.GetGroupBySum(inTable, colName, grCode, false);

                #endregion
                //선택한 거 없을 경우 validation 걸어야함
                char sp = ':';
                object[] arrPackPjt = cboPackPjt.SelectedItems.ToArray();
                object[] arrCellPjt = cboCellPjt.SelectedItems.ToArray();
                string[] strPackTemp = null;
                string[] strCellTemp = null;
                strPackTemp = arrPackPjt.Cast<string>().ToArray();
                strCellTemp = arrCellPjt.Cast<string>().ToArray();
                string[] strPackPrj = new string[1];
                string[] strPackProd = new string[1];
                string[] strCellPrj = new string[1];
                string[] strCellProd = new string[1];
                for (int i = 0; i < strPackTemp.Count(); i++)
                {
                    strPackPrj[0] = strPackPrj[0] + ',' + strPackTemp[i].Split(sp)[0];
                    strPackProd[0] = strPackProd[0] + ',' + strPackTemp[i].Split(sp)[1];
                }
                for (int i = 0; i < strCellTemp.Count(); i++)
                {
                    strCellPrj[0] = strCellPrj[0] + ',' + strCellTemp[i].Split(sp)[0];
                    strCellProd[0] = strCellProd[0] + ',' + strCellTemp[i].Split(sp)[1];
                }

                DataSet ds = new DataSet();

                DataTable indata = new DataTable();
                indata.TableName = "INDATA";

                indata.Columns.Add("MTRLID", typeof(string));
                indata.Columns.Add("AREAID", typeof(string));
                indata.Columns.Add("LANGID", typeof(string));
                indata.Columns.Add("SHOPID", typeof(string));
                indata.Columns.Add("TODAY", typeof(string));

                DataRow dr = indata.NewRow();
                dr["MTRLID"] = strCellProd[0];
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["TODAY"] = today;

                indata.Rows.Add(dr);

                ds.Tables.Add(indata);

                new ClientProxy().ExecuteService_Multi("BR_PRD_SEL_PACK_REQ_STATUS", "INDATA", "RSLTDT", (dsRslt, bizException) =>
                {
                    try
                    {
                        bClick = true;
                        ShowLoadingIndicator();
                        dgPlan.LoadedCellPresenter += dgPlan_LoadedCellPresenter;

                        if (bizException != null)
                        {
                            bClick = false;
                            HiddenLoadingIndicator();
                            Refresh2();
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.GridSetData(dgPlan, dsRslt.Tables["RSLTDT"], FrameOperation, false);
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
                        dgPlan.LoadedCellPresenter -= dgPlan_LoadedCellPresenter;
                    }

                    //if (dsRslt.Tables["RSLTDT"].Rows.Count > 0)
                    //{

                    //    string[] strColName = dsRslt.Tables["RSLTDT"].Columns.Cast<DataColumn>()
                    //                                                            .Select(x => x.ColumnName)
                    //                                                            .ToArray();

                    //    for (int i = 0; strColName.Length > i; i++)
                    //    {
                    //        int j = 0;

                    //        if (int.TryParse(strColName[i], out j) && string.IsNullOrEmpty(dsRslt.Tables["RSLTDT"].Rows[0][i].ToString()))
                    //        {
                    //            if (int.TryParse(dgPlan.Columns[i - 1].Name, out j))
                    //            {
                    //                dgPlan.Columns[i - 1].Visibility = Visibility.Collapsed;
                    //            }
                    //        }

                    //    }
                    //}

                }, ds);

                //dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_PACK_REQ_STATUS", "INDATA", "RSLTDT", ds, null);

                //if(dsRslt != null)
                //{
                //    dgPlan.LoadedCellPresenter += dgPlan_LoadedCellPresenter;
                //    Util.GridSetData(dgPlan, dsRslt.Tables["RSLTDT"], FrameOperation, false);
                //    bClick = false;
                //    dgPlan.LoadedCellPresenter -= dgPlan_LoadedCellPresenter;
                //    dsRslt.Tables["RSLTDT"].DefaultView.ToTable(true, new string[] {"PACK_PJT"});
                //}
            }
            catch (Exception ex)
            {
                bClick = false;
                HiddenLoadingIndicator();
                Refresh2();
                Util.MessageException(ex);
            }
            finally
            {
                bClick = false;
                Refresh2();
                HiddenLoadingIndicator();
            }

        }
#endregion

        #region PACK 생산 현황 조회
        private void SearchData2(string prodId, string cellPrjt, string packPjt, int today)
        {
            try
            {
                //ShowLoadingIndicator();
                #region TEST 데이터
                //임시 데이터
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



                //if (cellPrjt == "E78-C04")
                //{
                //    inTable2.Rows.Add("P6", "E78-C04", "ACEN1078I-A1-C04", packPjt, 17900, 24000, "TOTAL", "16,000", "14,000", "12,000", "10,000", "8,000", "6,000"
                //                        , "4,000", "2,000", "1,000", "500", "0", "-2,000", "-4,000", "-6,000", "-8,000", "-10,000", "-12,000", "-14,000"
                //                        , "-16,000", "-18,000", "-20,000", "-22,000", "-24,000", "-26,000");
                //    inTable2.Rows.Add("P6", "E78-C04", "ACEN1078I-A1-C04", packPjt, 10000, 10000, "M#3",  "9,583", "9,167", "8,750", "8,333", "7,917", "7,500"
                //                        , "7,083", "6,667", "6,250", "5,833", "5,417", "4,583", "4,167", "4,750", "3,333", "2,917", "2,500", "2,083"
                //                        , "1,667", "1,250", "833", "417", "0", "-417");
                //    inTable2.Rows.Add("P6", "E78-C04", "ACEN1078I-A1-C04", packPjt, 7000, 10000, "M#4", "24,000", "21,000", "18,000", "15,000", "12,000", "9,000"
                //                       , "6,000", "3,000", "0", "-3,000", "-6,000", "-9,000", "-12,000", "-15,000", "-18,000", "-21,000", "-24,000", "-27,000"
                //                       , "-30,000", "-33,000", "-36,000", "-39,000", "-42,000", "-45,000");
                //    inTable2.Rows.Add("P6", "E78-C04", "ACEN1078I-A1-C04", packPjt, 900, 4000, "M#5",  "16,000", "14,000", "12,000", "10,000", "8,000", "6,000"
                //                       , "4,000", "2,000", "1,000", "500", "0", "-2,000", "-4,000", "-6,000", "-8,000", "-10,000", "-12,000", "-14,000"
                //                       , "-16,000", "-18,000", "-20,000", "-22,000", "-24,000", "-26,000");
                //}
                //else
                //{

                //    inTable2.Rows.Add("P6", "E78-C03", "ACEN1078I-A1-C03", packPjt, 17900, 24000, "Total", "460,000", "42,000", "38,000", "34,000", "30,000", "26,000"
                //                       , "22,000", "18,000", "15,000", "12,500", "10,000", "6,000", "4,000", "0", "-2,000", "-10,000", "-14,000", "-18,000"
                //                       , "-22,000", "-26,000", "-30,000", "-34,000", "-38,000", "-42,000");
                //    inTable2.Rows.Add("P6", "E78-C03", "ACEN1078I-A1-C03", packPjt, 17900, 24000, "M#1",  "30,000", "28,000", "26,000", "24,000", "22,000", "20,000"
                //                       , "18,000", "16,000", "14,000", "12,000", "10,000", "8,000", "6,000", "4,000", "2,000", "0", "-2,000", "-4,000"
                //                       , "-6,000", "-8,000", "-10,000", "-12,000", "-14,000", "-16,000");
                //    inTable2.Rows.Add("P6", "E78-C03", "ACEN1078I-A1-C03", packPjt, 17900, 24000, "M#2", "16,000", "14,000", "12,000", "10,000", "8,000", "6,000"
                //                       , "4,000", "2,000", "1,000", "500", "0", "-2,000", "-4,000", "-6,000", "-8,000", "-10,000", "-12,000", "-14,000"
                //                       , "-16,000", "-18,000", "-20,000", "-22,000", "-24,000", "-26,000");
                //}
                #endregion

                DataSet ds = new DataSet();

                DataTable indata = new DataTable();
                indata.TableName = "INDATA";

                indata.Columns.Add("PRODID", typeof(string));
                indata.Columns.Add("CELL_PRJT", typeof(string));
                indata.Columns.Add("PACK_PRJT", typeof(string));
                indata.Columns.Add("AREAID", typeof(string)); 
                indata.Columns.Add("TODAY", typeof(string));
                indata.Columns.Add("LANGID", typeof(string));


                DataRow dr = indata.NewRow();
                dr["PRODID"] = prodId;
                dr["CELL_PRJT"] = cellPrjt;
                dr["PACK_PRJT"] = packPjt;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["TODAY"] = today;
                dr["LANGID"] = LoginInfo.LANGID;

                indata.Rows.Add(dr);

                ds.Tables.Add(indata);

                //dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_PACK_PRDT_STATUS", "INDATA", "REQ_CELL", ds, null);

                //if (dsRslt != null)
                //{
                //    dgStock.LoadedCellPresenter += dgStock_LoadedCellPresenter;
                //    Util.GridSetData(dgStock, dsRslt.Tables["REQ_CELL"], FrameOperation, false);
                //    dgStock.LoadedCellPresenter -= dgStock_LoadedCellPresenter;
                //    bClick = false;
                //}

                new ClientProxy().ExecuteService_Multi("BR_PRD_SEL_PACK_PRDT_STATUS", "INDATA", "REQ_CELL", (dsRslt, bizException) =>
                {
                    try
                    {
                        bClick = true;

                        ShowLoadingIndicator();
                        dgStock.LoadedCellPresenter += dgStock_LoadedCellPresenter;

                        if (bizException != null)
                        {
                            bClick = false;
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.GridSetData(dgStock, dsRslt.Tables["REQ_CELL"], FrameOperation, false);
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
                        dgStock.LoadedCellPresenter -= dgPlan_LoadedCellPresenter;
                    }


                }, ds);

            }
            catch (Exception ex)
            {
                bClick = false;
                HiddenLoadingIndicator();
                Refresh();
                Util.MessageException(ex);
            }
            finally
            {
                bClick = false;
                HiddenLoadingIndicator();
            }

        }
        #endregion

        #region Refresh
        private void Refresh()
        {
            try
            {
                //그리드 clear
                Util.gridClear(dgStock);

            }
            catch (Exception ex)
            {
                bClick = false;
                throw ex;
            }
        }
        #endregion Refresh

        #region Refresh2
        private void Refresh2()
        {
            try
            {
                //그리드 clear
                Util.gridClear(dgPlan);

            }
            catch (Exception ex)
            {
                bClick = false;
                throw ex;
            }
        }
        #endregion Refresh

        #region 팝업 닫기
        void popup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_078_CELLREQUEST popup = sender as PACK001_078_CELLREQUEST;
                if (popup.DialogResult == MessageBoxResult.Cancel)
                {
                    Refresh2();
                    int iToday = rdoToday.IsChecked == true ? 1 : 0;
                    bClick = false;
                    SearchData(iToday);
                }
            }
            catch (Exception ex)
            {
                bClick = false;
                Util.MessageException(ex);
            }
        }
        void condpopup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_078_AUTO_COND condpopup = sender as PACK001_078_AUTO_COND;
                if (condpopup.DialogResult == MessageBoxResult.Cancel)
                {
                    Refresh2();
                    int iToday = rdoToday.IsChecked == true ? 1 : 0;
                    bClick = false;
                    SearchData(iToday);
                }
            }
            catch (Exception ex)
            {
                bClick = false;
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
    }
}