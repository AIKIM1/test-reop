/*************************************************************************************
 Created Date : 2019.09.17
      Creator : 손우석
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2019.09.05 손우석 Initial Created. CSR ID 4087361  [요청] Cell 실시간 공급 정보 제공 |[요청번호]C20190918_87361 
  2019.09.20  손우석 CSR ID 4087361  [요청] Cell 실시간 공급 정보 제공 |[요청번호]C20190918_87361  디자인 수정 및 용어 변경
  2019.09.25  손우석 CSR ID 4087361  [요청] Cell 실시간 공급 정보 제공 |[요청번호]C20190918_87361  Grid 색상 변경
  2019.10.01  손우석 CSR ID 4087361  [요청] Cell 실시간 공급 정보 제공 |[요청번호]C20190918_87361  Grid 색상 변경
  2019.10.20  염규범 SI  PACK PJT 삭제 처리
  2020.06.26  손우석 Button 연속 클릭에 대한 오류 처리 및 로딩인디게이터 표시
  2023.03.17  김길용 재고예상 색상처리 폰트에서 Backgroud로 수정
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using DataGridCell = C1.WPF.DataGrid.DataGridCell;
using DataGridColumn = C1.WPF.DataGrid.DataGridColumn;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_047 : UserControl, IWorkArea
    {
        //2020.06.26
        public bool bClick = false;

        #region Declaration & Constructor & Init
        public PACK001_047()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Hide1.Visibility = Visibility.Hidden;
            //Hide2.Visibility = Visibility.Hidden;
            //Hide3.Visibility = Visibility.Hidden;

            if (LoginInfo.CFG_SHOP_ID.Equals("A040"))
            {
                dgStock2.Visibility = Visibility.Collapsed;
                dgOCStock2.Visibility = Visibility.Visible;

                dgInput2.Visibility = Visibility.Collapsed;
                dgOCInput2.Visibility = Visibility.Visible;
            }
            else
            {
                dgStock2.Visibility = Visibility.Visible;
                dgOCStock2.Visibility = Visibility.Collapsed;

                dgInput2.Visibility = Visibility.Visible;
                dgOCInput2.Visibility = Visibility.Collapsed;

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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //2020.06.26
                if (bClick == false)
                {
                    Action act = () =>
                    {
                        if (LoginInfo.CFG_SHOP_ID.Equals("A040"))
                        {
                            dgOCStock2.LoadedCellPresenter += dgOCStock2_LoadedCellPresenter;
                        }
                        else
                        {
                            dgStock2.LoadedCellPresenter += dgStock2_LoadedCellPresenter;
                            
                        }

                        

                        bClick = true;

                        ShowLoadingIndicator();

                        SearchData();

                        
                    };

                    if (LoginInfo.CFG_SHOP_ID.Equals("A040"))
                    {
                        dgOCStock2.LoadedCellPresenter -= dgOCStock2_LoadedCellPresenter;
                    }
                    else
                    {
                        dgStock2.LoadedCellPresenter -= dgStock2_LoadedCellPresenter;

                    }

                    btnSearch.Dispatcher.Invoke(act);
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();

                bClick = false;
                //2020.06.26
                //Util.Alert(ex.ToString());
            }
        }

        #endregion Button

        #region Grid
        private void dgInput2_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgStock2_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    int i = 0;
                    if (e.Cell.Column.Index > 8 && e.Cell.Value == "")
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                    }

                    if (string.IsNullOrEmpty(e.Cell.Column.Name.ToString()) || e.Cell.Value == "") return;

                    if (e.Cell.Row.Type == DataGridRowType.Item && int.TryParse(e.Cell.Column.Name, out i))
                    {
                      if (Util.NVC_Decimal(e.Cell.Value) > 0)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }
                        else
                        {

                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);

                            for (int j = 1; 2 >= j; j++)
                            {
                                if (string.IsNullOrEmpty(dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Column.Name.ToString())) break;

                                if (int.TryParse(dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Column.Name, out i))
                                {
                                    if (dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Value.GetDecimal() > 0)
                                    {
                                        dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                        dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        
                    }
                }));




                //2020.06.26
                //C1DataGrid dataGrid = (sender as C1DataGrid);

                //Action act = () =>
                //{
                //    if (e.Cell.Presenter == null)
                //    {
                //        return;
                //    }

                //    SetCellColor(dataGrid, e);
                //};

                //dataGrid.Dispatcher.BeginInvoke(act);

                //if (!dataGrid.Dispatcher.CheckAccess()) //main UI 에서 호출
                //{
                //    if (e.Cell.Presenter == null)
                //    {
                //        return;
                //    }

                //    SetCellColor(dataGrid, e);
                //}
                //else //UI가 아닌 다른 Thread 호출시
                //{
                //    Action act = () =>
                //    {
                //        if (e.Cell.Presenter == null)
                //        {
                //            return;
                //        }

                //        SetCellColor(dataGrid, e);
                //    };

                //    dataGrid.Dispatcher.BeginInvoke(act);
                //}
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();

                //2020.06.26
                //Util.Alert(ex.ToString());
                bClick = false;
            }
        }

        private void dgOCStock2_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    DataTable dt = DataTableConverter.Convert(dgOCStock2.ItemsSource);

                    int i = 0;

                    if (string.IsNullOrEmpty(e.Cell.Column.Name.ToString()) || string.IsNullOrEmpty(e.Cell.Value.ToString())) return;

                    if (e.Cell.Row.Type == DataGridRowType.Item && int.TryParse(e.Cell.Column.Name, out i))
                    {
                        if (Util.NVC_Decimal(e.Cell.Value) > 0)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);

                            for (int j = 1; 2 >= j; j++)
                            {
                                if (string.IsNullOrEmpty(dgOCStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Column.Name.ToString()) 
                                    || dgOCStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Value == DBNull.Value) break;

                                if (int.TryParse(dgOCStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Column.Name, out i))
                                {
                                    if (dgOCStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Value.GetDecimal() > 0)
                                    {
                                        dgOCStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                        dgOCStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }));
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                bClick = false;
            }
        }

        #endregion Grid

        #endregion Event

        #region Method

        private void SearchData()
        {
            DataSet dsResult = null;

            try
            {
                DataSet ds = new DataSet();

                DataTable indata = new DataTable();
                indata.TableName = "INDATA";

                indata.Columns.Add("LANGID", typeof(string));
                indata.Columns.Add("SHOPID", typeof(string));
                indata.Columns.Add("AREAID", typeof(string));
                indata.Columns.Add("EQSGID", typeof(string));
                //indata.Columns.Add("YMD", typeof(string));
                //indata.Columns.Add("NOW", typeof(string));

                DataRow dr = indata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = null;
                //dr["YMD"] = "";
                //dr["NOW"] = "";

                indata.Rows.Add(dr);

                ds.Tables.Add(indata);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_REPORT_REQ_CELL", "INDATA", "PLAN,INPUT,STOCK,INPUT2,STOCK2", ds, null);

                if (dsResult != null)
                {
                    //2020.06.26
                    //if ((dsResult.Tables[4].Rows.Count > 0) && (dsResult.Tables[5].Rows.Count > 0))
                    //{
                    if (LoginInfo.CFG_SHOP_ID.ToString().Equals("A040"))
                    {
                        Util.GridSetData(dgOCInput2, dsResult.Tables["INPUT2"], FrameOperation, false);
                        Util.GridSetData(dgOCStock2, dsResult.Tables["STOCK2"], FrameOperation, false);
                    }
                    else
                    {
                        Util.GridSetData(dgInput2, dsResult.Tables["INPUT2"], FrameOperation, false);
                        Util.GridSetData(dgStock2, dsResult.Tables["STOCK2"], FrameOperation, false);
                    }
                    //}
                    //else
                    //{
                        HiddenLoadingIndicator();

                        bClick = false;
                    //}
                }
            }
            catch (Exception ex)
            {
                //2020.06.26
                HiddenLoadingIndicator();

                bClick = false;

                //Util.MessageException(ex);
            }
            //2020.06.26
            //finally
            //{
            //    //HiddenLoadingIndicator();
            //}
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

            if (e.Cell.Row.DataItem != null)
            {
                if (dataGrid.Name.Equals("dgStock") || dataGrid.Name.Equals("dgStock2"))
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                if (e.Cell.Column.Name.Equals("8") || e.Cell.Column.Name.Equals("9") || e.Cell.Column.Name.Equals("10") ||
                                    e.Cell.Column.Name.Equals("11") || e.Cell.Column.Name.Equals("12") || e.Cell.Column.Name.Equals("13") ||
                                    e.Cell.Column.Name.Equals("14") || e.Cell.Column.Name.Equals("15") || e.Cell.Column.Name.Equals("16") ||
                                    e.Cell.Column.Name.Equals("17") || e.Cell.Column.Name.Equals("18") || e.Cell.Column.Name.Equals("19") ||
                                    e.Cell.Column.Name.Equals("20") || e.Cell.Column.Name.Equals("21") || e.Cell.Column.Name.Equals("22") ||
                                    e.Cell.Column.Name.Equals("23") || e.Cell.Column.Name.Equals("24") || e.Cell.Column.Name.Equals("1") ||
                                    e.Cell.Column.Name.Equals("2") || e.Cell.Column.Name.Equals("3") || e.Cell.Column.Name.Equals("4") ||
                                    e.Cell.Column.Name.Equals("5") || e.Cell.Column.Name.Equals("6") || e.Cell.Column.Name.Equals("7")
                                    )
                                {
                                    #region 08
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")) != "")
                                    {
                                        double n08 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")));

                                        #region 08 음
                                        if (n08 < 0 && string.Equals(e.Cell.Column.Name, "8"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")) != "")
                                            {
                                                double nChk9 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")));
                                                double nChk10 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")));

                                                #region 09 음 10 음
                                                if (nChk9 < 0 && nChk10 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 음 10 음

                                                #region 09 양 10 움
                                                else if (nChk9 > 0 && nChk10 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 양 10 음

                                                #region 09 양 10 양
                                                else if (nChk9 > 0 && nChk10 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 양 10 양

                                                #region 09 음 10 양
                                                else if (nChk9 < 0 && nChk10 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 음 10 양

                                                #region 09 0 10 0
                                                else if (nChk9 < 0 && nChk10 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 0 10 0
                                            }
                                            else
                                            {
                                                double nChk9 = 0;
                                                double nChk10 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")) != "")
                                                {
                                                    nChk9 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")) != "")
                                                {
                                                    nChk10 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")));
                                                }

                                                #region 07 음 08 음
                                                if (nChk9 < 0 && nChk10 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 음 10 음

                                                #region 09 양 10 움
                                                else if (nChk9 > 0 && nChk10 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 양 10 음

                                                #region 09 양 10 양
                                                else if (nChk9 > 0 && nChk10 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 양 10 양

                                                #region 09 음 10 양
                                                else if (nChk9 < 0 && nChk10 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 음 10 양

                                                #region 09 0 10 0
                                                else if (nChk9 < 0 && nChk10 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 0 10 0
                                            }
                                        }
                                        #endregion 08 음

                                        #region 08 양
                                        else if (n08 > 0 && string.Equals(e.Cell.Column.Name, "8"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")) != "")
                                            {
                                                double nChk9 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")));
                                                double nChk10 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")));

                                                #region 07 음 08 음
                                                if (nChk9 < 0 && nChk10 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 음 10 음

                                                #region 09 양 10 움
                                                else if (nChk9 > 0 && nChk10 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 양 10 음

                                                #region 09 양 10 양
                                                else if (nChk9 > 0 && nChk10 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 양 10 양

                                                #region 09 음 10 양
                                                else if (nChk9 < 0 && nChk10 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 음 10 양

                                                #region 09 0 10 0
                                                else if (nChk9 < 0 && nChk10 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 0 10 0
                                            }
                                            else
                                            {
                                                double nChk9 = 0;
                                                double nChk10 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")) != "")
                                                {
                                                    nChk9 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")) != "")
                                                {
                                                    nChk10 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")));
                                                }

                                                #region 07 음 08 음
                                                if (nChk9 < 0 && nChk10 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 음 10 음

                                                #region 09 양 10 움
                                                else if (nChk9 > 0 && nChk10 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 양 10 음

                                                #region 09 양 10 양
                                                else if (nChk9 > 0 && nChk10 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 양 10 양

                                                #region 09 음 10 양
                                                else if (nChk9 < 0 && nChk10 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 음 10 양

                                                #region 09 0 10 0
                                                else if (nChk9 < 0 && nChk10 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 09 0 10 0
                                            }
                                        }
                                        #endregion 08 양
                                    }
                                    #endregion 08

                                    #region 09
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")) != "")
                                    {
                                        double n09 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")));

                                        #region 09 음
                                        if (n09 < 0 && string.Equals(e.Cell.Column.Name, "9"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")) != "")
                                            {
                                                double nChk10 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")));
                                                double nChk11 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")));

                                                #region 10 음 11 음
                                                if (nChk10 < 0 && nChk11 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 음 11 음

                                                #region 10 양 11 움
                                                else if (nChk10 > 0 && nChk11 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 양 11 음

                                                #region 10 양 11 양
                                                else if (nChk10 > 0 && nChk11 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 양 11 양

                                                #region 10 음 11 양
                                                else if (nChk10 < 0 && nChk11 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 음 11 양

                                                #region 10 0 11 0
                                                else if (nChk10 < 0 && nChk11 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 0 11 0
                                            }
                                            else
                                            {
                                                double nChk10 = 0;
                                                double nChk11 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")) != "")
                                                {
                                                    nChk10 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")) != "")
                                                {
                                                    nChk11 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")));
                                                }

                                                #region 10 음 11 음
                                                if (nChk10 < 0 && nChk11 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 음 11 음

                                                #region 10 양 11 움
                                                else if (nChk10 > 0 && nChk11 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 양 11 음

                                                #region 10 양 11 양
                                                else if (nChk10 > 0 && nChk11 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 양 11 양

                                                #region 10 음 11 양
                                                else if (nChk10 < 0 && nChk11 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 음 11 양

                                                #region 10 0 11 0
                                                else if (nChk10 < 0 && nChk11 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 0 11 0
                                            }
                                        }
                                        #endregion 09 음

                                        #region 09 양
                                        else if (n09 > 0 && string.Equals(e.Cell.Column.Name, "9"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")) != "")
                                            {
                                                double nChk10 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")));
                                                double nChk11 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")));

                                                #region 10 음 11 음
                                                if (nChk10 < 0 && nChk11 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 음 11 음

                                                #region 10 양 11 움
                                                else if (nChk10 > 0 && nChk11 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 양 11 음

                                                #region 10 양 11 양
                                                else if (nChk10 > 0 && nChk11 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 양 11 양

                                                #region 10 음 11 양
                                                else if (nChk10 < 0 && nChk11 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 음 11 양

                                                #region 10 0 11 0
                                                else if (nChk10 < 0 && nChk11 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 0 11 0

                                            }
                                            else
                                            {
                                                double nChk10 = 0;
                                                double nChk11 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")) != "")
                                                {
                                                    nChk10 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")) != "")
                                                {
                                                    nChk11 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")));
                                                }

                                                #region 10 음 11 음
                                                if (nChk10 < 0 && nChk11 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 음 11 음

                                                #region 10 양 11 움
                                                else if (nChk10 > 0 && nChk11 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 양 11 음

                                                #region 10 양 11 양
                                                else if (nChk10 > 0 && nChk11 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 양 11 양

                                                #region 10 음 11 양
                                                else if (nChk10 < 0 && nChk11 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 음 11 양

                                                #region 10 0 11 0
                                                else if (nChk10 < 0 && nChk11 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 10 0 11 0
                                            }
                                        }
                                        #endregion 09 양                                        
                                    }
                                    #endregion 09

                                    #region 10
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")) != "")
                                    {
                                        double n10 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "10")));

                                        #region 10 음
                                        if (n10 < 0 && string.Equals(e.Cell.Column.Name, "10"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")) != "")
                                            {
                                                double nChk11 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")));
                                                double nChk12 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")));

                                                #region 11 음 12 음
                                                if (nChk11 < 0 && nChk12 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 음 12 음

                                                #region 11 양 12 움
                                                else if (nChk11 > 0 && nChk12 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 양 12 음

                                                #region 11 양 12 양
                                                else if (nChk11 > 0 && nChk12 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 양 12 양

                                                #region 11 음 12 양
                                                else if (nChk11 < 0 && nChk12 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 음 12 양

                                                #region 11 0 12 0
                                                else if (nChk11 < 0 && nChk12 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 0 12 0
                                            }
                                            else
                                            {
                                                double nChk11 = 0;
                                                double nChk12 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")) != "")
                                                {
                                                    nChk11 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")) != "")
                                                {
                                                    nChk12 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")));
                                                }

                                                #region 11 음 12 음
                                                if (nChk11 < 0 && nChk12 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 음 12 음

                                                #region 11 양 12 움
                                                else if (nChk11 > 0 && nChk12 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 양 12 음

                                                #region 11 양 12 양
                                                else if (nChk11 > 0 && nChk12 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 양 12 양

                                                #region 11 음 12 양
                                                else if (nChk11 < 0 && nChk12 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 음 12 양

                                                #region 11 0 12 0
                                                else if (nChk11 < 0 && nChk12 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 0 12 0
                                            }
                                        }
                                        #endregion 10 음

                                        #region 10 양
                                        else if (n10 > 0 && string.Equals(e.Cell.Column.Name, "10"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")) != "")
                                            {
                                                double nChk11 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")));
                                                double nChk12 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")));

                                                #region 11 음 12 음
                                                if (nChk11 < 0 && nChk12 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 음 12 음

                                                #region 11 양 12 움
                                                else if (nChk11 > 0 && nChk12 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 양 12 음

                                                #region 11 양 12 양
                                                else if (nChk11 > 0 && nChk12 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 양 12 양

                                                #region 11 음 12 양
                                                else if (nChk11 < 0 && nChk12 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 음 12 양

                                                #region 11 0 12 0
                                                else if (nChk11 < 0 && nChk12 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 0 12 0
                                            }
                                            else
                                            {
                                                double nChk11 = 0;
                                                double nChk12 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")) != "")
                                                {
                                                    nChk11 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")) != "")
                                                {
                                                    nChk12 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")));
                                                }

                                                #region 11 음 12 음
                                                if (nChk11 < 0 && nChk12 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 음 12 음

                                                #region 11 양 12 움
                                                else if (nChk11 > 0 && nChk12 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 양 12 음

                                                #region 11 양 12 양
                                                else if (nChk11 > 0 && nChk12 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 양 12 양

                                                #region 11 음 12 양
                                                else if (nChk11 < 0 && nChk12 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 음 12 양

                                                #region 11 0 12 0
                                                else if (nChk11 < 0 && nChk12 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 11 0 12 0
                                            }
                                        }
                                        #endregion 10 양                                              
                                    }
                                    #endregion 10

                                    #region 11
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")) != "")
                                    {
                                        double n11 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "11")));

                                        #region 11 음
                                        if (n11 < 0 && string.Equals(e.Cell.Column.Name, "11"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")) != "")
                                            {
                                                double nChk12 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")));
                                                double nChk13 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")));

                                                #region 12 음 13 음
                                                if (nChk12 < 0 && nChk13 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 음 13 음

                                                #region 12 양 13 움
                                                else if (nChk12 > 0 && nChk13 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 양 13 음

                                                #region 12 양 13 양
                                                else if (nChk12 > 0 && nChk13 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 양 13 양

                                                #region 12 음 13 양
                                                else if (nChk12 < 0 && nChk13 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 음 13 양

                                                #region 12 0 13 0
                                                else if (nChk12 < 0 && nChk13 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 0 13 0
                                            }
                                            else
                                            {
                                                double nChk12 = 0;
                                                double nChk13 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")) != "")
                                                {
                                                    nChk12 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")) != "")
                                                {
                                                    nChk13 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")));
                                                }

                                                #region 12 음 13 음
                                                if (nChk12 < 0 && nChk13 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 음 13 음

                                                #region 12 양 13 움
                                                else if (nChk12 > 0 && nChk13 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 양 13 음

                                                #region 12 양 13 양
                                                else if (nChk12 > 0 && nChk13 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 양 13 양

                                                #region 12 음 13 양
                                                else if (nChk12 < 0 && nChk13 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 음 13 양

                                                #region 12 0 13 0
                                                else if (nChk12 < 0 && nChk13 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 0 13 0
                                            }
                                        }
                                        #endregion 11 음

                                        #region 11 양
                                        else if (n11 > 0 && string.Equals(e.Cell.Column.Name, "11"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")) != "")
                                            {
                                                double nChk12 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")));
                                                double nChk13 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")));

                                                #region 12 음 13 음
                                                if (nChk12 < 0 && nChk13 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 음 13 음

                                                #region 12 양 13 움
                                                else if (nChk12 > 0 && nChk13 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 양 13 음

                                                #region 12 양 13 양
                                                else if (nChk12 > 0 && nChk13 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 양 13 양

                                                #region 12 음 13 양
                                                else if (nChk12 < 0 && nChk13 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 음 13 양

                                                #region 12 0 13 0
                                                else if (nChk12 < 0 && nChk13 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 0 13 0
                                            }
                                            else
                                            {
                                                double nChk12 = 0;
                                                double nChk13 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")) != "")
                                                {
                                                    nChk12 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")) != "")
                                                {
                                                    nChk13 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")));
                                                }

                                                #region 12 음 13 음
                                                if (nChk12 < 0 && nChk13 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 음 13 음

                                                #region 12 양 13 움
                                                else if (nChk12 > 0 && nChk13 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 양 13 음

                                                #region 12 양 13 양
                                                else if (nChk12 > 0 && nChk13 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 양 13 양

                                                #region 12 음 13 양
                                                else if (nChk12 < 0 && nChk13 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 음 13 양

                                                #region 12 0 13 0
                                                else if (nChk12 < 0 && nChk13 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 12 0 13 0
                                            }
                                        }
                                        #endregion 11 양
                                    }
                                    #endregion 11

                                    #region 12
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")) != "")
                                    {
                                        double n12 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "12")));

                                        #region 12 음
                                        if (n12 < 0 && string.Equals(e.Cell.Column.Name, "12"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")) != "")
                                            {
                                                double nChk13 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")));
                                                double nChk14 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")));

                                                #region 13 음 14 음
                                                if (nChk13 < 0 && nChk14 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 음 14 음

                                                #region 13 양 14 움
                                                else if (nChk13 > 0 && nChk14 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 양 14 음

                                                #region 13 양 14 양
                                                else if (nChk13 > 0 && nChk14 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 양 14 양

                                                #region 13 음 14 양
                                                else if (nChk13 < 0 && nChk14 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 음 14 양

                                                #region 13 0 14 0
                                                else if (nChk13 < 0 && nChk14 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 0 14 0
                                            }
                                            else
                                            {
                                                double nChk13 = 0;
                                                double nChk14 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")) != "")
                                                {
                                                    nChk13 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")) != "")
                                                {
                                                    nChk14 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")));
                                                }

                                                #region 13 음 14 음
                                                if (nChk13 < 0 && nChk14 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 음 14 음

                                                #region 13 양 14 움
                                                else if (nChk13 > 0 && nChk14 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 양 14 음

                                                #region 13 양 14 양
                                                else if (nChk13 > 0 && nChk14 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 양 14 양

                                                #region 13 음 14 양
                                                else if (nChk13 < 0 && nChk14 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 음 14 양

                                                #region 13 0 14 0
                                                else if (nChk13 < 0 && nChk14 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 0 14 0
                                            }
                                        }
                                        #endregion 12 음

                                        #region 12 양
                                        else if (n12 > 0 && string.Equals(e.Cell.Column.Name, "12"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")) != "")
                                            {
                                                double nChk13 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")));
                                                double nChk14 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")));

                                                #region 13 음 14 음
                                                if (nChk13 < 0 && nChk14 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 음 14 음

                                                #region 13 양 14 움
                                                else if (nChk13 > 0 && nChk14 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 양 14 음

                                                #region 13 양 14 양
                                                else if (nChk13 > 0 && nChk14 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 양 14 양

                                                #region 13 음 14 양
                                                else if (nChk13 < 0 && nChk14 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 음 14 양

                                                #region 13 0 14 0
                                                else if (nChk13 < 0 && nChk14 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 0 14 0
                                            }
                                            else
                                            {
                                                double nChk13 = 0;
                                                double nChk14 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")) != "")
                                                {
                                                    nChk13 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")) != "")
                                                {
                                                    nChk14 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")));
                                                }

                                                #region 13 음 14 음
                                                if (nChk13 < 0 && nChk14 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 음 14 음

                                                #region 13 양 14 움
                                                else if (nChk13 > 0 && nChk14 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 양 14 음

                                                #region 13 양 14 양
                                                else if (nChk13 > 0 && nChk14 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 양 14 양

                                                #region 13 음 14 양
                                                else if (nChk13 < 0 && nChk14 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 음 14 양

                                                #region 13 0 14 0
                                                else if (nChk13 < 0 && nChk14 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 13 0 14 0
                                            }
                                        }
                                        #endregion 12 양
                                    }
                                    #endregion 12

                                    #region 13
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")) != "")
                                    {
                                        double n13 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "13")));

                                        #region 13 음
                                        if (n13 < 0 && string.Equals(e.Cell.Column.Name, "13"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")) != "")
                                            {
                                                double nChk14 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")));
                                                double nChk15 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")));

                                                #region 14 음 15 음
                                                if (nChk14 < 0 && nChk15 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 음 15 음

                                                #region 14 양 15 움
                                                else if (nChk14 > 0 && nChk15 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 양 15 음

                                                #region 14 양 15 양
                                                else if (nChk14 > 0 && nChk15 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 양 15 양

                                                #region 14 음 15 양
                                                else if (nChk14 < 0 && nChk15 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 음 15 양

                                                #region 14 0 15 0
                                                else if (nChk14 < 0 && nChk15 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 0 15 0
                                            }
                                            else
                                            {
                                                double nChk14 = 0;
                                                double nChk15 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")) != "")
                                                {
                                                    nChk14 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")) != "")
                                                {
                                                    nChk15 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")));
                                                }

                                                #region 14 음 15 음
                                                if (nChk14 < 0 && nChk15 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 음 15 음

                                                #region 14 양 15 움
                                                else if (nChk14 > 0 && nChk15 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 양 15 음

                                                #region 14 양 15 양
                                                else if (nChk14 > 0 && nChk15 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 양 15 양

                                                #region 14 음 15 양
                                                else if (nChk14 < 0 && nChk15 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 음 15 양

                                                #region 14 0 15 0
                                                else if (nChk14 < 0 && nChk15 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 0 15 0
                                            }
                                        }
                                        #endregion 13 음

                                        #region 13 양
                                        else if (n13 > 0 && string.Equals(e.Cell.Column.Name, "13"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")) != "")
                                            {
                                                double nChk14 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")));
                                                double nChk15 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")));

                                                #region 14 음 15 음
                                                if (nChk14 < 0 && nChk15 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 음 15 음

                                                #region 14 양 15 움
                                                else if (nChk14 > 0 && nChk15 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 양 15 음

                                                #region 14 양 15 양
                                                else if (nChk14 > 0 && nChk15 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 양 15 양

                                                #region 14 음 15 양
                                                else if (nChk14 < 0 && nChk15 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 음 15 양

                                                #region 14 0 15 0
                                                else if (nChk14 < 0 && nChk15 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 0 15 0
                                            }
                                            else
                                            {
                                                double nChk14 = 0;
                                                double nChk15 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")) != "")
                                                {
                                                    nChk14 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")) != "")
                                                {
                                                    nChk15 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")));
                                                }

                                                #region 14 음 15 음
                                                if (nChk14 < 0 && nChk15 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 음 15 음

                                                #region 14 양 15 움
                                                else if (nChk14 > 0 && nChk15 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 양 15 음

                                                #region 14 양 15 양
                                                else if (nChk14 > 0 && nChk15 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 양 15 양

                                                #region 14 음 15 양
                                                else if (nChk14 < 0 && nChk15 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 음 15 양

                                                #region 14 0 15 0
                                                else if (nChk14 < 0 && nChk15 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 14 0 15 0
                                            }
                                        }
                                        #endregion 13 양
                                    }
                                    #endregion 13

                                    #region 14
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")) != "")
                                    {
                                        double n14 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "14")));

                                        #region 14 음
                                        if (n14 < 0 && string.Equals(e.Cell.Column.Name, "14"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")) != "")
                                            {
                                                double nChk15 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")));
                                                double nChk16 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")));

                                                #region 15 음 16 음
                                                if (nChk15 < 0 && nChk16 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 음 16 음

                                                #region 15 양 16 움
                                                else if (nChk15 > 0 && nChk16 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 양 16 음

                                                #region 15 양 16 양
                                                else if (nChk15 > 0 && nChk16 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 양 16 양

                                                #region 15 음 16 양
                                                else if (nChk15 < 0 && nChk16 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 음 16 양

                                                #region 15 0 16 0
                                                else if (nChk15 < 0 && nChk16 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 0 16 0
                                            }
                                            else
                                            {
                                                double nChk15 = 0;
                                                double nChk16 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")) != "")
                                                {
                                                    nChk15 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")) != "")
                                                {
                                                    nChk16 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")));
                                                }

                                                #region 15 음 16 음
                                                if (nChk15 < 0 && nChk16 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 음 16 음

                                                #region 15 양 16 움
                                                else if (nChk15 > 0 && nChk16 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 양 16 음

                                                #region 15 양 16 양
                                                else if (nChk15 > 0 && nChk16 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 양 16 양

                                                #region 15 음 16 양
                                                else if (nChk15 < 0 && nChk16 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 음 16 양

                                                #region 15 0 16 0
                                                else if (nChk15 < 0 && nChk16 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 0 16 0
                                            }
                                        }
                                        #endregion 14 음

                                        #region 14 양
                                        else if (n14 > 0 && string.Equals(e.Cell.Column.Name, "14"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")) != "")
                                            {
                                                double nChk15 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")));
                                                double nChk16 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")));

                                                #region 15 음 16 음
                                                if (nChk15 < 0 && nChk16 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 음 16 음

                                                #region 15 양 16 움
                                                else if (nChk15 > 0 && nChk16 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 양 16 음

                                                #region 15 양 16 양
                                                else if (nChk15 > 0 && nChk16 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 양 16 양

                                                #region 15 음 16 양
                                                else if (nChk15 < 0 && nChk16 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 음 16 양

                                                #region 15 0 16 0
                                                else if (nChk15 < 0 && nChk16 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 0 16 0
                                            }
                                            else
                                            {
                                                double nChk15 = 0;
                                                double nChk16 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")) != "")
                                                {
                                                    nChk15 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")) != "")
                                                {
                                                    nChk16 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")));
                                                }

                                                #region 15 음 16 음
                                                if (nChk15 < 0 && nChk16 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 음 16 음

                                                #region 15 양 16 움
                                                else if (nChk15 > 0 && nChk16 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 양 16 음

                                                #region 15 양 16 양
                                                else if (nChk15 > 0 && nChk16 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 양 16 양

                                                #region 15 음 16 양
                                                else if (nChk15 < 0 && nChk16 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 음 16 양

                                                #region 15 0 16 0
                                                else if (nChk15 < 0 && nChk16 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 15 0 16 0
                                            }
                                        }
                                        #endregion 14 양
                                    }
                                    #endregion 14

                                    #region 15
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")) != "")
                                    {
                                        double n15 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "15")));

                                        #region 15 음
                                        if (n15 < 0 && string.Equals(e.Cell.Column.Name, "15"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")) != "")
                                            {
                                                double nChk16 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")));
                                                double nChk17 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")));

                                                #region 16 음 17 음
                                                if (nChk17 < 0 && nChk17 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 음 17 음

                                                #region 16 양 17 움
                                                else if (nChk16 > 0 && nChk17 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 양 17 음

                                                #region 16 양 17 양
                                                else if (nChk16 > 0 && nChk17 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 양 17 양

                                                #region 16 음 17 양
                                                else if (nChk16 < 0 && nChk17 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 음 17 양

                                                #region 16 0 17 0
                                                else if (nChk16 < 0 && nChk17 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 0 17 0
                                            }
                                            else
                                            {
                                                double nChk16 = 0;
                                                double nChk17 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")) != "")
                                                {
                                                    nChk16 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")) != "")
                                                {
                                                    nChk17 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")));
                                                }

                                                #region 16 음 17 음
                                                if (nChk16 < 0 && nChk17 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 음 17 음

                                                #region 16 양 17 움
                                                else if (nChk16 > 0 && nChk17 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 양 17 음

                                                #region 16 양 17 양
                                                else if (nChk16 > 0 && nChk17 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 양 17 양

                                                #region 16 음 17 양
                                                else if (nChk16 < 0 && nChk17 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 음 17 양

                                                #region 16 0 17 0
                                                else if (nChk16 < 0 && nChk17 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 0 17 0
                                            }
                                        }
                                        #endregion 15 음

                                        #region 15 양
                                        else if (n15 > 0 && string.Equals(e.Cell.Column.Name, "15"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")) != "")
                                            {
                                                double nChk16 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")));
                                                double nChk17 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")));

                                                #region 16 음 17 음
                                                if (nChk16 < 0 && nChk17 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 음 17 음

                                                #region 16 양 17 움
                                                else if (nChk16 > 0 && nChk17 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 양 17 음

                                                #region 16 양 17 양
                                                else if (nChk16 > 0 && nChk17 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 양 17 양

                                                #region 16 음 17 양
                                                else if (nChk16 < 0 && nChk17 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 음 17 양

                                                #region 16 0 17 0
                                                else if (nChk16 < 0 && nChk17 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 0 17 0
                                            }
                                            else
                                            {
                                                double nChk16 = 0;
                                                double nChk17 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")) != "")
                                                {
                                                    nChk16 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")) != "")
                                                {
                                                    nChk17 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")));
                                                }

                                                #region 16 음 17 음
                                                if (nChk16 < 0 && nChk17 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 음 17 음

                                                #region 16 양 17 움
                                                else if (nChk16 > 0 && nChk17 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 양 17 음

                                                #region 16 양 17 양
                                                else if (nChk16 > 0 && nChk17 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 양 17 양

                                                #region 16 음 17 양
                                                else if (nChk16 < 0 && nChk17 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 음 17 양

                                                #region 16 0 17 0
                                                else if (nChk16 < 0 && nChk17 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 16 0 17 0
                                            }
                                        }
                                        #endregion 15 양
                                    }
                                    #endregion 15

                                    #region 16
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")) != "")
                                    {
                                        double n16 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "16")));

                                        #region 16 음
                                        if (n16 < 0 && string.Equals(e.Cell.Column.Name, "16"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")) != "")
                                            {
                                                double nChk17 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")));
                                                double nChk18 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")));

                                                #region 17 음 18 음
                                                if (nChk17 < 0 && nChk18 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 음 18 음

                                                #region 17 양 18 움
                                                else if (nChk17 > 0 && nChk18 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 양 18 음

                                                #region 17 양 18 양
                                                else if (nChk17 > 0 && nChk18 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 양 18 양

                                                #region 17 음 18 양
                                                else if (nChk17 < 0 && nChk18 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 음 18 양

                                                #region 17 0 18 0
                                                else if (nChk17 < 0 && nChk18 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 0 18 0
                                            }
                                            else
                                            {
                                                double nChk17 = 0;
                                                double nChk18 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")) != "")
                                                {
                                                    nChk17 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")) != "")
                                                {
                                                    nChk18 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")));
                                                }

                                                #region 17 음 18 음
                                                if (nChk17 < 0 && nChk18 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 음 18 음

                                                #region 17 양 18 움
                                                else if (nChk17 > 0 && nChk18 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 양 18 음

                                                #region 17 양 18 양
                                                else if (nChk17 > 0 && nChk18 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 양 18 양

                                                #region 17 음 18 양
                                                else if (nChk17 < 0 && nChk18 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 음 18 양

                                                #region 17 0 18 0
                                                else if (nChk17 < 0 && nChk18 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 0 18 0
                                            }
                                        }
                                        #endregion 16 음

                                        #region 16 양
                                        else if (n16 > 0 && string.Equals(e.Cell.Column.Name, "16"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")) != "")
                                            {
                                                double nChk17 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")));
                                                double nChk18 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")));

                                                #region 17 음 18 음
                                                if (nChk17 < 0 && nChk18 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 음 18 음

                                                #region 17 양 18 움
                                                else if (nChk17 > 0 && nChk18 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 양 18 음

                                                #region 17 양 18 양
                                                else if (nChk17 > 0 && nChk18 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 양 18 양

                                                #region 17 음 18 양
                                                else if (nChk17 < 0 && nChk18 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 음 18 양

                                                #region 17 0 18 0
                                                else if (nChk17 < 0 && nChk18 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 0 18 0
                                            }
                                            else
                                            {
                                                double nChk17 = 0;
                                                double nChk18 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")) != "")
                                                {
                                                    nChk17 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")) != "")
                                                {
                                                    nChk18 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")));
                                                }

                                                #region 17 음 18 음
                                                if (nChk17 < 0 && nChk18 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 음 18 음

                                                #region 17 양 18 움
                                                else if (nChk17 > 0 && nChk18 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 양 18 음

                                                #region 17 양 18 양
                                                else if (nChk17 > 0 && nChk18 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 양 18 양

                                                #region 17 음 18 양
                                                else if (nChk17 < 0 && nChk18 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 음 18 양

                                                #region 17 0 18 0
                                                else if (nChk17 < 0 && nChk18 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 17 0 18 0
                                            }
                                        }
                                        #endregion 16 양
                                    }
                                    #endregion 16

                                    #region 17
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")) != "")
                                    {
                                        double n17 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "17")));

                                        #region 17 음
                                        if (n17 < 0 && string.Equals(e.Cell.Column.Name, "17"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")) != "")
                                            {
                                                double nChk18 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")));
                                                double nChk19 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")));

                                                #region 18 음 19 음
                                                if (nChk18 < 0 && nChk19 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 음 19 음

                                                #region 18 양 19 움
                                                else if (nChk18 > 0 && nChk19 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 양 19 음

                                                #region 18 양 19 양
                                                else if (nChk18 > 0 && nChk19 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 양 19 양

                                                #region 18 음 19 양
                                                else if (nChk18 < 0 && nChk19 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 음 19 양

                                                #region 18 0 19 0
                                                else if (nChk18 < 0 && nChk19 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 0 19 0
                                            }
                                            else
                                            {
                                                double nChk18 = 0;
                                                double nChk19 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")) != "")
                                                {
                                                    nChk18 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")) != "")
                                                {
                                                    nChk19 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")));
                                                }

                                                #region 18 음 19 음
                                                if (nChk18 < 0 && nChk19 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 음 19 음

                                                #region 18 양 19 움
                                                else if (nChk18 > 0 && nChk19 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 양 19 음

                                                #region 18 양 19 양
                                                else if (nChk18 > 0 && nChk19 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 양 19 양

                                                #region 18 음 19 양
                                                else if (nChk18 < 0 && nChk19 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 음 19 양

                                                #region 18 0 19 0
                                                else if (nChk18 < 0 && nChk19 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 0 19 0
                                            }
                                        }
                                        #endregion 17 음

                                        #region 17 양
                                        else if (n17 > 0 && string.Equals(e.Cell.Column.Name, "17"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")) != "")
                                            {
                                                double nChk18 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")));
                                                double nChk19 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")));

                                                #region 18 음 19 음
                                                if (nChk18 < 0 && nChk19 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 음 19 음

                                                #region 18 양 19 움
                                                else if (nChk18 > 0 && nChk19 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 양 19 음

                                                #region 18 양 19 양
                                                else if (nChk18 > 0 && nChk19 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 양 19 양

                                                #region 18 음 19 양
                                                else if (nChk18 < 0 && nChk19 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 음 19 양

                                                #region 18 0 19 0
                                                else if (nChk18 < 0 && nChk19 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 0 19 0
                                            }
                                            else
                                            {
                                                double nChk18 = 0;
                                                double nChk19 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")) != "")
                                                {
                                                    nChk18 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")) != "")
                                                {
                                                    nChk19 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")));
                                                }

                                                #region 18 음 19 음
                                                if (nChk18 < 0 && nChk19 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 음 19 음

                                                #region 18 양 19 움
                                                else if (nChk18 > 0 && nChk19 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 양 19 음

                                                #region 18 양 19 양
                                                else if (nChk18 > 0 && nChk19 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 양 19 양

                                                #region 18 음 19 양
                                                else if (nChk18 < 0 && nChk19 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 음 19 양

                                                #region 18 0 19 0
                                                else if (nChk18 < 0 && nChk19 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 18 0 19 0
                                            }
                                        }
                                        #endregion 17 양
                                    }
                                    #endregion 17

                                    #region 18
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")) != "")
                                    {
                                        double n18 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "18")));

                                        #region 18 음
                                        if (n18 < 0 && string.Equals(e.Cell.Column.Name, "18"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")) != "")
                                            {
                                                double nChk19 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")));
                                                double nChk20 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")));

                                                #region 19 음 20 음
                                                if (nChk19 < 0 && nChk20 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 음 20 음

                                                #region 19 양 20 움
                                                else if (nChk19 > 0 && nChk20 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 양 20 음

                                                #region 19 양 20 양
                                                else if (nChk19 > 0 && nChk20 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 양 20 양

                                                #region 19 음 20 양
                                                else if (nChk19 < 0 && nChk20 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 음 20 양

                                                #region 19 0 20 0
                                                else if (nChk19 < 0 && nChk20 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 0 20 0
                                            }
                                            else
                                            {
                                                double nChk19 = 0;
                                                double nChk20 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")) != "")
                                                {
                                                    nChk19 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")) != "")
                                                {
                                                    nChk20 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")));
                                                }

                                                #region 19 음 20 음
                                                if (nChk19 < 0 && nChk20 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 음 20 음

                                                #region 19 양 20 움
                                                else if (nChk19 > 0 && nChk20 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 양 20 음

                                                #region 19 양 20 양
                                                else if (nChk19 > 0 && nChk20 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 양 20 양

                                                #region 19 음 20 양
                                                else if (nChk19 < 0 && nChk20 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 음 20 양

                                                #region 19 0 20 0
                                                else if (nChk19 < 0 && nChk20 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 0 20 0
                                            }
                                        }
                                        #endregion 18 음

                                        #region 18 양
                                        else if (n18 > 0 && string.Equals(e.Cell.Column.Name, "18"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")) != "")
                                            {
                                                double nChk19 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")));
                                                double nChk20 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")));

                                                #region 19 음 20 음
                                                if (nChk19 < 0 && nChk20 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 음 20 음

                                                #region 19 양 20 움
                                                else if (nChk19 > 0 && nChk20 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 양 20 음

                                                #region 19 양 20 양
                                                else if (nChk19 > 0 && nChk20 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 양 20 양

                                                #region 19 음 20 양
                                                else if (nChk19 < 0 && nChk20 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 음 20 양

                                                #region 19 0 20 0
                                                else if (nChk19 < 0 && nChk20 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 0 20 0
                                            }
                                            else
                                            {
                                                double nChk19 = 0;
                                                double nChk20 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")) != "")
                                                {
                                                    nChk19 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")) != "")
                                                {
                                                    nChk20 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")));
                                                }

                                                #region 19 음 20 음
                                                if (nChk19 < 0 && nChk20 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 음 20 음

                                                #region 19 양 20 움
                                                else if (nChk19 > 0 && nChk20 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 양 20 음

                                                #region 19 양 20 양
                                                else if (nChk19 > 0 && nChk20 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 양 20 양

                                                #region 19 음 20 양
                                                else if (nChk19 < 0 && nChk20 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 음 20 양

                                                #region 19 0 20 0
                                                else if (nChk19 < 0 && nChk20 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 19 0 20 0
                                            }
                                        }
                                        #endregion 18 양
                                    }
                                    #endregion 18

                                    #region 19
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")) != "")
                                    {
                                        double n19 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "19")));

                                        #region 19 음
                                        if (n19 < 0 && string.Equals(e.Cell.Column.Name, "19"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")) != "")
                                            {
                                                double nChk20 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")));
                                                double nChk21 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")));

                                                #region 20 음 21 음
                                                if (nChk20 < 0 && nChk21 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 음 21 음

                                                #region 20 양 21 움
                                                else if (nChk20 > 0 && nChk21 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 양 21 음

                                                #region 20 양 21 양
                                                else if (nChk20 > 0 && nChk21 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 양 21 양

                                                #region 20 음 21 양
                                                else if (nChk20 < 0 && nChk21 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 음 21 양

                                                #region 20 0 21 0
                                                else if (nChk20 < 0 && nChk21 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 0 21 0
                                            }
                                            else
                                            {
                                                double nChk20 = 0;
                                                double nChk21 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")) != "")
                                                {
                                                    nChk20 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")) != "")
                                                {
                                                    nChk21 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")));
                                                }

                                                #region 20 음 21 음
                                                if (nChk20 < 0 && nChk21 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 음 21 음

                                                #region 20 양 21 움
                                                else if (nChk20 > 0 && nChk21 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 양 21 음

                                                #region 20 양 21 양
                                                else if (nChk20 > 0 && nChk21 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 양 21 양

                                                #region 20 음 21 양
                                                else if (nChk20 < 0 && nChk21 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 음 21 양

                                                #region 20 0 21 0
                                                else if (nChk20 < 0 && nChk21 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 0 21 0
                                            }
                                        }
                                        #endregion 19 음

                                        #region 19 양
                                        else if (n19 > 0 && string.Equals(e.Cell.Column.Name, "19"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")) != "")
                                            {
                                                double nChk20 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")));
                                                double nChk21 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")));

                                                #region 20 음 21 음
                                                if (nChk20 < 0 && nChk21 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 음 21 음

                                                #region 20 양 21 움
                                                else if (nChk20 > 0 && nChk21 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 양 21 음

                                                #region 20 양 21 양
                                                else if (nChk20 > 0 && nChk21 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 양 21 양

                                                #region 20 음 21 양
                                                else if (nChk20 < 0 && nChk21 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 음 21 양

                                                #region 20 0 21 0
                                                else if (nChk20 < 0 && nChk21 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 0 21 0
                                            }
                                            else
                                            {
                                                double nChk20 = 0;
                                                double nChk21 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")) != "")
                                                {
                                                    nChk20 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")) != "")
                                                {
                                                    nChk21 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")));
                                                }

                                                #region 20 음 21 음
                                                if (nChk20 < 0 && nChk21 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 음 21 음

                                                #region 20 양 21 움
                                                else if (nChk20 > 0 && nChk21 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 양 21 음

                                                #region 20 양 21 양
                                                else if (nChk20 > 0 && nChk21 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 양 21 양

                                                #region 20 음 21 양
                                                else if (nChk20 < 0 && nChk21 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 음 21 양

                                                #region 20 0 21 0
                                                else if (nChk20 < 0 && nChk21 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 20 0 21 0
                                            }
                                        }
                                        #endregion 19 양
                                    }
                                    #endregion 19

                                    #region 20
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")) != "")
                                    {
                                        double n20 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "20")));

                                        #region 20 음
                                        if (n20 < 0 && string.Equals(e.Cell.Column.Name, "20"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")) != "")
                                            {
                                                double nChk21 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")));
                                                double nChk22 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")));

                                                #region 21 음 22 음
                                                if (nChk21 < 0 && nChk22 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 음 22 음

                                                #region 21 양 22 움
                                                else if (nChk21 > 0 && nChk22 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 양 22 음

                                                #region 21 양 22 양
                                                else if (nChk21 > 0 && nChk22 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 양 22 양

                                                #region 21 음 22 양
                                                else if (nChk21 < 0 && nChk22 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 음 22 양

                                                #region 21 0 22 0
                                                else if (nChk21 < 0 && nChk22 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 0 22 0
                                            }
                                            else
                                            {
                                                double nChk21 = 0;
                                                double nChk22 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")) != "")
                                                {
                                                    nChk21 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")) != "")
                                                {
                                                    nChk22 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")));
                                                }

                                                #region 21 음 22 음
                                                if (nChk21 < 0 && nChk22 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 음 22 음

                                                #region 21 양 22 움
                                                else if (nChk21 > 0 && nChk22 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 양 22 음

                                                #region 21 양 22 양
                                                else if (nChk21 > 0 && nChk22 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 양 22 양

                                                #region 21 음 22 양
                                                else if (nChk21 < 0 && nChk22 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 음 22 양

                                                #region 21 0 22 0
                                                else if (nChk21 < 0 && nChk22 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 0 22 0
                                            }
                                        }
                                        #endregion 20 음

                                        #region 20 양
                                        else if (n20 > 0 && string.Equals(e.Cell.Column.Name, "20"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")) != "")
                                            {
                                                double nChk21 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")));
                                                double nChk22 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")));

                                                #region 21 음 22 음
                                                if (nChk21 < 0 && nChk22 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 음 22 음

                                                #region 21 양 22 움
                                                else if (nChk21 > 0 && nChk22 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 양 22 음

                                                #region 21 양 22 양
                                                else if (nChk21 > 0 && nChk22 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 양 22 양

                                                #region 21 음 22 양
                                                else if (nChk21 < 0 && nChk22 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 음 22 양

                                                #region 21 0 22 0
                                                else if (nChk21 < 0 && nChk22 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 0 22 0
                                            }
                                            else
                                            {
                                                double nChk21 = 0;
                                                double nChk22 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")) != "")
                                                {
                                                    nChk21 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")) != "")
                                                {
                                                    nChk22 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")));
                                                }

                                                #region 21 음 22 음
                                                if (nChk21 < 0 && nChk22 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 음 22 음

                                                #region 21 양 22 움
                                                else if (nChk21 > 0 && nChk22 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 양 22 음

                                                #region 21 양 22 양
                                                else if (nChk21 > 0 && nChk22 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 양 22 양

                                                #region 21 음 22 양
                                                else if (nChk21 < 0 && nChk22 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 음 22 양

                                                #region 21 0 22 0
                                                else if (nChk21 < 0 && nChk22 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 21 0 22 0
                                            }
                                        }
                                        #endregion 20 양
                                    }
                                    #endregion 20

                                    #region 21
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")) != "")
                                    {
                                        double n21 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "21")));

                                        #region 21 음
                                        if (n21 < 0 && string.Equals(e.Cell.Column.Name, "21"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")) != "")
                                            {
                                                double nChk22 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")));
                                                double nChk23 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")));

                                                #region 22 음 23 음
                                                if (nChk22 < 0 && nChk23 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 음 23 음

                                                #region 22 양 23 움
                                                else if (nChk22 > 0 && nChk23 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 양 23 음

                                                #region 22 양 22 양
                                                else if (nChk22 > 0 && nChk23 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 양 23 양

                                                #region 22 음 23 양
                                                else if (nChk22 < 0 && nChk23 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 음 23 양

                                                #region 22 0 23 0
                                                else if (nChk22 < 0 && nChk23 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 0 23 0
                                            }
                                            else
                                            {
                                                double nChk22 = 0;
                                                double nChk23 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")) != "")
                                                {
                                                    nChk22 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")) != "")
                                                {
                                                    nChk23 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")));
                                                }


                                                #region 22 음 23 음
                                                if (nChk22 < 0 && nChk23 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 음 23 음

                                                #region 22 양 23 움
                                                else if (nChk22 > 0 && nChk23 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 양 23 음

                                                #region 22 양 23 양
                                                else if (nChk22 > 0 && nChk23 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 양 23 양

                                                #region 22 음 23 양
                                                else if (nChk22 < 0 && nChk23 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 음 23 양

                                                #region 22 0 23 0
                                                else if (nChk22 < 0 && nChk23 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 0 23 0
                                            }
                                        }
                                        #endregion 21 음

                                        #region 21 양
                                        else if (n21 > 0 && string.Equals(e.Cell.Column.Name, "21"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")) != "")
                                            {
                                                double nChk22 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")));
                                                double nChk23 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")));

                                                #region 22 음 23 음
                                                if (nChk22 < 0 && nChk23 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 음 23 음

                                                #region 22 양 23 움
                                                else if (nChk22 > 0 && nChk23 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 양 23 음

                                                #region 22 양 23 양
                                                else if (nChk22 > 0 && nChk23 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 양 23 양

                                                #region 22 음 23 양
                                                else if (nChk22 < 0 && nChk23 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 음 23 양

                                                #region 22 0 23 0
                                                else if (nChk22 < 0 && nChk23 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 0 23 0
                                            }
                                            else
                                            {
                                                double nChk22 = 0;
                                                double nChk23 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")) != "")
                                                {
                                                    nChk22 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")) != "")
                                                {
                                                    nChk23 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")));
                                                }


                                                #region 22 음 23 음
                                                if (nChk22 < 0 && nChk23 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 음 23 음

                                                #region 22 양 23 움
                                                else if (nChk22 > 0 && nChk23 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 양 23 음

                                                #region 22 양 23 양
                                                else if (nChk22 > 0 && nChk23 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 양 23 양

                                                #region 22 음 23 양
                                                else if (nChk22 < 0 && nChk23 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 음 23 양

                                                #region 22 0 23 0
                                                else if (nChk22 < 0 && nChk23 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 22 0 23 0
                                            }
                                        }
                                        #endregion 21 양
                                    }
                                    #endregion 21

                                    #region 22
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")) != "")
                                    {
                                        double n22 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "22")));

                                        #region 22 음
                                        if (n22 < 0 && string.Equals(e.Cell.Column.Name, "22"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")) != "")
                                            {
                                                double nChk23 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")));
                                                double nChk24 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")));

                                                #region 23 음 24 음
                                                if (nChk23 < 0 && nChk24 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 음 24 음

                                                #region 23 양 24 움
                                                else if (nChk23 > 0 && nChk24 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 양 24 음

                                                #region 23 양 24 양
                                                else if (nChk23 > 0 && nChk24 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 양 24 양

                                                #region 23 음 24 양
                                                else if (nChk23 < 0 && nChk24 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 음 24 양

                                                #region 23 0 24 0
                                                else if (nChk23 < 0 && nChk24 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 0 24 0
                                            }
                                            else
                                            {
                                                double nChk23 = 0;
                                                double nChk24 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")) != "")
                                                {
                                                    nChk23 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")) != "")
                                                {
                                                    nChk24 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")));
                                                }

                                                #region 23 음 24 음
                                                if (nChk23 < 0 && nChk24 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 음 24 음

                                                #region 23 양 24 움
                                                else if (nChk23 > 0 && nChk24 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 양 24 음

                                                #region 23 양 24 양
                                                else if (nChk23 > 0 && nChk24 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 양 24 양

                                                #region 23 음 24 양
                                                else if (nChk23 < 0 && nChk24 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 음 24 양

                                                #region 23 0 24 0
                                                else if (nChk23 < 0 && nChk24 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 0 24 0
                                            }
                                        }
                                        #endregion 22 음

                                        #region 22 양
                                        else if (n22 > 0 && string.Equals(e.Cell.Column.Name, "22"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")) != "")
                                            {
                                                double nChk23 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")));
                                                double nChk24 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")));

                                                #region 23 음 24 음
                                                if (nChk23 < 0 && nChk24 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 음 24 음

                                                #region 23 양 24 움
                                                else if (nChk23 > 0 && nChk24 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 양 24 음

                                                #region 23 양 24 양
                                                else if (nChk23 > 0 && nChk24 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 양 24 양

                                                #region 23 음 24 양
                                                else if (nChk23 < 0 && nChk24 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 음 24 양

                                                #region 23 0 24 0
                                                else if (nChk23 < 0 && nChk24 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 0 24 0
                                            }
                                            else
                                            {
                                                double nChk23 = 0;
                                                double nChk24 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")) != "")
                                                {
                                                    nChk23 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")) != "")
                                                {
                                                    nChk24 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")));
                                                }

                                                #region 23 음 24 음
                                                if (nChk23 < 0 && nChk24 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 음 24 음

                                                #region 23 양 24 움
                                                else if (nChk23 > 0 && nChk24 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 양 24 음

                                                #region 23 양 24 양
                                                else if (nChk23 > 0 && nChk24 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 양 24 양

                                                #region 23 음 24 양
                                                else if (nChk23 < 0 && nChk24 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 음 24 양

                                                #region 23 0 24 0
                                                else if (nChk23 < 0 && nChk24 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 23 0 24 0
                                            }
                                        }
                                        #endregion 22 양
                                    }
                                    #endregion 22

                                    #region 23
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")) != "")
                                    {
                                        double n23 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "23")));

                                        #region 23 음
                                        if (n23 < 0 && string.Equals(e.Cell.Column.Name, "23"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")) != "")
                                            {
                                                double nChk24 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")));
                                                double nChk1 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")));

                                                #region 24 음 1 음
                                                if (nChk24 < 0 && nChk1 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 음 1 음

                                                #region 24 양 1 움
                                                else if (nChk24 > 0 && nChk1 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 양 1 음

                                                #region 24 양 1 양
                                                else if (nChk24 > 0 && nChk1 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 양 1 양

                                                #region 24 음 1 양
                                                else if (nChk24 < 0 && nChk1 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 음 1 양

                                                #region 24 0 1 0
                                                else if (nChk24 < 0 && nChk1 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 0 1 0
                                            }
                                            else
                                            {
                                                double nChk24 = 0;
                                                double nChk1 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")) != "")
                                                {
                                                    nChk24 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")) != "")
                                                {
                                                    nChk1 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")));
                                                }

                                                #region 24 음 1 음
                                                if (nChk24 < 0 && nChk1 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 음 1 음

                                                #region 24 양 1 움
                                                else if (nChk24 > 0 && nChk1 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 양 1 음

                                                #region 24 양 1 양
                                                else if (nChk24 > 0 && nChk1 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 양 1 양

                                                #region 24 음 1 양
                                                else if (nChk24 < 0 && nChk1 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 음 1 양

                                                #region 24 0 1 0
                                                else if (nChk24 < 0 && nChk1 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 0 1 0
                                            }
                                        }
                                        #endregion 23 음

                                        #region 23 양
                                        else if (n23 > 0 && string.Equals(e.Cell.Column.Name, "23"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")) != "")
                                            {
                                                double nChk24 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")));
                                                double nChk1 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")));

                                                #region 24 음 1 음
                                                if (nChk24 < 0 && nChk1 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 음 1 음

                                                #region 24 양 1 움
                                                else if (nChk24 > 0 && nChk1 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 양 1 음

                                                #region 24 양 1 양
                                                else if (nChk24 > 0 && nChk1 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 양 1 양

                                                #region 24 음 1 양
                                                else if (nChk24 < 0 && nChk1 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 음 1 양

                                                #region 24 0 1 0
                                                else if (nChk24 < 0 && nChk1 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 0 1 0
                                            }
                                            else
                                            {
                                                double nChk24 = 0;
                                                double nChk1= 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")) != "")
                                                {
                                                    nChk24 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")) != "")
                                                {
                                                    nChk1 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")));
                                                }

                                                #region 24 음 1 음
                                                if (nChk24 < 0 && nChk1 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 음 1 음

                                                #region 24 양 1 움
                                                else if (nChk24 > 0 && nChk1 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 양 1 음

                                                #region 24 양 1 양
                                                else if (nChk24 > 0 && nChk1 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 양 1 양

                                                #region 24 음 1 양
                                                else if (nChk24 < 0 && nChk1 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 음 1 양

                                                #region 24 0 1 0
                                                else if (nChk24 < 0 && nChk1 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 24 0 1 0
                                            }
                                        }
                                        #endregion 23 양
                                    }
                                    #endregion 23

                                    #region 24
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")) != "")
                                    {
                                        double n24 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "24")));

                                        #region 24 음
                                        if (n24 < 0 && string.Equals(e.Cell.Column.Name, "24"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")) != "")
                                            {
                                                double nChk1 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")));
                                                double nChk2 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")));

                                                #region 1 음 2 음
                                                if (nChk1 < 0 && nChk2 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 음 2 음

                                                #region 1 양 2 움
                                                else if (nChk1 > 0 && nChk2 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 양 2 음

                                                #region 1 양 2 양
                                                else if (nChk1 > 0 && nChk2 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 양 2 양

                                                #region 1 음 2 양
                                                else if (nChk1 < 0 && nChk2 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 음 2 양

                                                #region 1 0 2 0
                                                else if (nChk1 < 0 && nChk2 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 0 2 0
                                            }
                                            else
                                            {
                                                double nChk1 = 0;
                                                double nChk2 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")) != "")
                                                {
                                                    nChk1 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")) != "")
                                                {
                                                    nChk2 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")));
                                                }

                                                #region 1 음 2 음
                                                if (nChk1 < 0 && nChk2 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 음 2 음

                                                #region 1 양 2 움
                                                else if (nChk1 > 0 && nChk2 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 양 2 음

                                                #region 1 양 2 양
                                                else if (nChk1 > 0 && nChk2 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 양 2 양

                                                #region 1 음 2 양
                                                else if (nChk1 < 0 && nChk2 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 음 2 양

                                                #region 1 0 2 0
                                                else if (nChk1 < 0 && nChk2 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 0 2 0
                                            }
                                        }
                                        #endregion 24 음

                                        #region 24 양
                                        else if (n24 > 0 && string.Equals(e.Cell.Column.Name, "24"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")) != "")
                                            {
                                                double nChk1 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")));
                                                double nChk2 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")));

                                                #region 1 음 2 음
                                                if (nChk1 < 0 && nChk2 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 음 2 음

                                                #region 1 양 2 움
                                                else if (nChk1 > 0 && nChk2 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 양 2 음

                                                #region 1 양 2 양
                                                else if (nChk1 > 0 && nChk2 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 양 2 양

                                                #region 1 음 2 양
                                                else if (nChk1 < 0 && nChk2 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 음 2 양

                                                #region 1 0 2 0
                                                else if (nChk1 < 0 && nChk2 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 0 2 0
                                            }
                                            else
                                            {
                                                double nChk1 = 0;
                                                double nChk2 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")) != "")
                                                {
                                                    nChk1 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")) != "")
                                                {
                                                    nChk2 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")));
                                                }

                                                #region 1 음 2 음
                                                if (nChk1 < 0 && nChk2 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 음 2 음

                                                #region 1 양 2 움
                                                else if (nChk1 > 0 && nChk2 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 양 2 음

                                                #region 1 양 2 양
                                                else if (nChk1 > 0 && nChk2 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 양 2 양

                                                #region 1 음 2 양
                                                else if (nChk1 < 0 && nChk2 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 음 2 양

                                                #region 1 0 2 0
                                                else if (nChk1 < 0 && nChk2 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 1 0 2 0
                                            }
                                        }
                                        #endregion 24 양
                                    }
                                    #endregion 24

                                    #region 01
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")) != "")
                                    {
                                        double n01 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "1")));

                                        #region 01 음
                                        if (n01 < 0 && string.Equals(e.Cell.Column.Name, "1"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")) != "")
                                            {
                                                double nChk2 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")));
                                                double nChk3 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")));

                                                #region 02 음 03 음
                                                if (nChk2 < 0 && nChk3 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 음 03 음

                                                #region 02 양 03 움
                                                else if (nChk2 > 0 && nChk3 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 양 03 음

                                                #region 02 양 03 양
                                                else if (nChk2 > 0 && nChk3 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 양 03 양

                                                #region 02 음 03 양
                                                else if (nChk2 < 0 && nChk3 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 음 03 양

                                                #region 02 0 03 0
                                                else if (nChk2 < 0 && nChk3 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 0 03 0
                                            }
                                            else
                                            {
                                                double nChk2 = 0;
                                                double nChk3 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")) != "")
                                                {
                                                    nChk2 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")) != "")
                                                {
                                                    nChk3 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")));
                                                }

                                                #region 02 음 03 음
                                                if (nChk2 < 0 && nChk3 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 음 03 음

                                                #region 02 양 03 움
                                                else if (nChk2 > 0 && nChk3 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 양 03 음

                                                #region 02 양 03 양
                                                else if (nChk2 > 0 && nChk3 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 양 03 양

                                                #region 02 음 03 양
                                                else if (nChk2 < 0 && nChk3 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 음 03 양

                                                #region 02 0 03 0
                                                else if (nChk2 < 0 && nChk3 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 0 03 0
                                            }
                                        }
                                        #endregion 01 음

                                        #region 01 양
                                        else if (n01 > 0 && string.Equals(e.Cell.Column.Name, "1"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")) != "")
                                            {
                                                double nChk2 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")));
                                                double nChk3 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")));

                                                #region 02 음 03 음
                                                if (nChk2 < 0 && nChk3 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors. Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 음 03 음

                                                #region 02 양 03 움
                                                else if (nChk2 > 0 && nChk3 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 양 03 음

                                                #region 02 양 03 양
                                                else if (nChk2 > 0 && nChk3 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 양 03 양

                                                #region 02 음 03 양
                                                else if (nChk2 < 0 && nChk3 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 음 03 양

                                                #region 02 0 03 0
                                                else if (nChk2 < 0 && nChk3 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 0 03 0
                                            }
                                            else
                                            {
                                                double nChk2 = 0;
                                                double nChk3 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")) != "")
                                                {
                                                    nChk2 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")) != "")
                                                {
                                                    nChk3 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")));
                                                }

                                                #region 02 음 03 음
                                                if (nChk2 < 0 && nChk3 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 음 03 음

                                                #region 02 양 03 움
                                                else if (nChk2 > 0 && nChk3 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 양 03 음

                                                #region 02 양 03 양
                                                else if (nChk2 > 0 && nChk3 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 양 03 양

                                                #region 02 음 03 양
                                                else if (nChk2 < 0 && nChk3 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 음 03 양

                                                #region 02 0 03 0
                                                else if (nChk2 < 0 && nChk3 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 02 0 03 0
                                            }
                                        }
                                        #endregion 01 양
                                    }
                                    #endregion 01

                                    #region 02
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")) != "")
                                    {
                                        double n02 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "2")));

                                        #region 21 음
                                        if (n02 < 0 && string.Equals(e.Cell.Column.Name, "2"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")) != "")
                                            {
                                                double nChk3 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")));
                                                double nChk4 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")));

                                                #region 03 음 04 음
                                                if (nChk3 < 0 && nChk4 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 음 04 음

                                                #region 03 양 04 움
                                                else if (nChk3 > 0 && nChk4 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 양 04 음

                                                #region 03 양 04 양
                                                else if (nChk3 > 0 && nChk4 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 양 04 양

                                                #region 03 음 04 양
                                                else if (nChk3 < 0 && nChk4 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 음 04 양

                                                #region 03 0 04 0
                                                else if (nChk3 < 0 && nChk4 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 0 04 0
                                            }
                                            else
                                            {
                                                double nChk3 = 0;
                                                double nChk4 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")) != "")
                                                {
                                                    nChk3 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")) != "")
                                                {
                                                    nChk4 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")));
                                                }

                                                #region 03 음 04 음
                                                if (nChk3 < 0 && nChk4 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 음 04 음

                                                #region 03 양 04 움
                                                else if (nChk3 > 0 && nChk4 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 양 04 음

                                                #region 03 양 04 양
                                                else if (nChk3 > 0 && nChk4 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 양 04 양

                                                #region 03 음 04 양
                                                else if (nChk3 < 0 && nChk4 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 음 04 양

                                                #region 03 0 04 0
                                                else if (nChk3 < 0 && nChk4 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 0 04 0
                                            }
                                        }
                                        #endregion 02 음

                                        #region 02 양
                                        else if (n02 > 0 && string.Equals(e.Cell.Column.Name, "2"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")) != "")
                                            {
                                                double nChk3 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")));
                                                double nChk4 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")));

                                                #region 03 음 04 음
                                                if (nChk3 < 0 && nChk4 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 음 04 음

                                                #region 03 양 04 움
                                                else if (nChk3 > 0 && nChk4 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 양 04 음

                                                #region 03 양 04 양
                                                else if (nChk3 > 0 && nChk4 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 양 04 양

                                                #region 03 음 04 양
                                                else if (nChk3 < 0 && nChk4 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 음 04 양

                                                #region 03 0 04 0
                                                else if (nChk3 < 0 && nChk4 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 0 04 0
                                            }
                                            else
                                            {
                                                double nChk3 = 0;
                                                double nChk4 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")) != "")
                                                {
                                                    nChk3 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")) != "")
                                                {
                                                    nChk4 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")));
                                                }

                                                #region 03 음 04 음
                                                if (nChk3 < 0 && nChk4 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 음 04 음

                                                #region 03 양 04 움
                                                else if (nChk3 > 0 && nChk4 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 양 04 음

                                                #region 03 양 04 양
                                                else if (nChk3 > 0 && nChk4 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 양 04 양

                                                #region 03 음 04 양
                                                else if (nChk3 < 0 && nChk4 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 음 04 양

                                                #region 03 0 04 0
                                                else if (nChk3 < 0 && nChk4 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 03 0 04 0
                                            }
                                        }
                                        #endregion 02 양
                                    }
                                    #endregion 02

                                    #region 03
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")) != "")
                                    {
                                        double n03 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "3")));

                                        #region 03 음
                                        if (n03 < 0 && string.Equals(e.Cell.Column.Name, "3"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")) != "")
                                            {
                                                double nChk4 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")));
                                                double nChk5 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")));

                                                #region 04 음 05 음
                                                if (nChk4 < 0 && nChk5 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 음 05 음

                                                #region 04 양 05 움
                                                else if (nChk4 > 0 && nChk5 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 양 05 음

                                                #region 04 양 05 양
                                                else if (nChk4 > 0 && nChk5 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 양 05 양

                                                #region 04 음 05 양
                                                else if (nChk4 < 0 && nChk5 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 음 05 양

                                                #region 04 0 05 0
                                                else if (nChk4 < 0 && nChk5 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 0 05 0
                                            }
                                            else
                                            {
                                                double nChk4 = 0;
                                                double nChk5 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")) != "")
                                                {
                                                    nChk4 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")) != "")
                                                {
                                                    nChk5 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")));
                                                }


                                                #region 04 음 05 음
                                                if (nChk4 < 0 && nChk5 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 음 05 음

                                                #region 04 양 05 움
                                                else if (nChk4 > 0 && nChk5 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 양 05 음

                                                #region 04 양 05 양
                                                else if (nChk4 > 0 && nChk5 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 양 05 양

                                                #region 04 음 05 양
                                                else if (nChk4 < 0 && nChk5 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 음 05 양

                                                #region 04 0 05 0
                                                else if (nChk4 < 0 && nChk5 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 0 05 0
                                            }
                                        }
                                        #endregion 03 음

                                        #region 03 양
                                        else if (n03 > 0 && string.Equals(e.Cell.Column.Name, "3"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")) != "")
                                            {
                                                double nChk4 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")));
                                                double nChk5 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")));

                                                #region 04 음 05 음
                                                if (nChk4 < 0 && nChk5 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 음 05 음

                                                #region 04 양 05 움
                                                else if (nChk4 > 0 && nChk5 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 양 05 음

                                                #region 04 양 05 양
                                                else if (nChk4 > 0 && nChk5 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 양 05 양

                                                #region 04 음 05 양
                                                else if (nChk4 < 0 && nChk5 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 음 05 양

                                                #region 04 0 05 0
                                                else if (nChk4 < 0 && nChk5 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 0 05 0
                                            }
                                            else
                                            {
                                                double nChk4 = 0;
                                                double nChk5 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")) != "")
                                                {
                                                    nChk4 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")) != "")
                                                {
                                                    nChk5 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")));
                                                }


                                                #region 04 음 05 음
                                                if (nChk4 < 0 && nChk5 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 음 05 음

                                                #region 04 양 05 움
                                                else if (nChk4 > 0 && nChk5 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 양 05 음

                                                #region 04 양 05 양
                                                else if (nChk4 > 0 && nChk5 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 양 05 양

                                                #region 04 음 05 양
                                                else if (nChk4 < 0 && nChk5 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 음 05 양

                                                #region 04 0 05 0
                                                else if (nChk4 < 0 && nChk5 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 04 0 05 0
                                            }
                                        }
                                        #endregion 03 양
                                    }
                                    #endregion 03

                                    #region 04
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")) != "")
                                    {
                                        double n04 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "4")));

                                        #region 04 음
                                        if (n04 < 0 && string.Equals(e.Cell.Column.Name, "4"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")) != "")
                                            {
                                                double nChk5 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")));
                                                double nChk6 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")));

                                                #region 05 음 06 음
                                                if (nChk5 < 0 && nChk6 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 음 06 음

                                                #region 05 양 06 움
                                                else if (nChk5 > 0 && nChk6 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 양 06 음

                                                #region 05 양 06 양
                                                else if (nChk5 > 0 && nChk6 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 양 06 양

                                                #region 05 음 06 양
                                                else if (nChk5 < 0 && nChk6 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 음 06 양

                                                #region 05 0 06 0
                                                else if (nChk5 < 0 && nChk6 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 0 06 0
                                            }
                                            else
                                            {
                                                double nChk5 = 0;
                                                double nChk6 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")) != "")
                                                {
                                                    nChk5 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")) != "")
                                                {
                                                    nChk6 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")));
                                                }

                                                #region 05 음 06 음
                                                if (nChk5 < 0 && nChk6 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 음 06 음

                                                #region 05 양 06 움
                                                else if (nChk5 > 0 && nChk6 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 양 06 음

                                                #region 05 양 06 양
                                                else if (nChk5 > 0 && nChk6 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 양 06 양

                                                #region 05 음 06 양
                                                else if (nChk5 < 0 && nChk6 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 음 06 양

                                                #region 05 0 06 0
                                                else if (nChk5 < 0 && nChk6 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 0 06 0
                                            }
                                        }
                                        #endregion 04 음

                                        #region 04 양
                                        else if (n04 > 0 && string.Equals(e.Cell.Column.Name, "4"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")) != "")
                                            {
                                                double nChk5 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")));
                                                double nChk6 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")));

                                                #region 05 음 06 음
                                                if (nChk5 < 0 && nChk6 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 음 06 음

                                                #region 05 양 06 움
                                                else if (nChk5 > 0 && nChk6 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 양 06 음

                                                #region 05 양 06 양
                                                else if (nChk5 > 0 && nChk6 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 양 06 양

                                                #region 05 음 06 양
                                                else if (nChk5 < 0 && nChk6 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 음 06 양

                                                #region 05 0 06 0
                                                else if (nChk5 < 0 && nChk6 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 0 06 00
                                            }
                                            else
                                            {
                                                double nChk5 = 0;
                                                double nChk6 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")) != "")
                                                {
                                                    nChk5 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")) != "")
                                                {
                                                    nChk6 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")));
                                                }

                                                #region 05 음 06 음
                                                if (nChk5 < 0 && nChk6 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 음 06 음

                                                #region 05 양 06 움
                                                else if (nChk5 > 0 && nChk6 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 양 06 음

                                                #region 05 양 06 양
                                                else if (nChk5 > 0 && nChk6 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 양 06 양

                                                #region 05 음 06 양
                                                else if (nChk5 < 0 && nChk6 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 음 06 양

                                                #region 05 0 06 0
                                                else if (nChk5 < 0 && nChk6 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 05 0 06 0
                                            }
                                        }
                                        #endregion 04 양
                                    }
                                    #endregion 04

                                    #region 05
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")) != "")
                                    {
                                        double n05 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "5")));

                                        #region 05 음
                                        if (n05 < 0 && string.Equals(e.Cell.Column.Name, "5"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")) != "")
                                            {
                                                double nChk6 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")));
                                                double nChk7 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")));

                                                #region 06 음 07 음
                                                if (nChk6 < 0 && nChk7 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 음 07 음

                                                #region 06 양 07 움
                                                else if (nChk6 > 0 && nChk7 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 양 07 음

                                                #region 06 양 07 양
                                                else if (nChk6 > 0 && nChk7 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 양 07 양

                                                #region 06 음 07 양
                                                else if (nChk6 < 0 && nChk7 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 음 07 양

                                                #region 06 0 07 0
                                                else if (nChk6 == 0 && nChk7 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 0 07 0
                                            }
                                            else
                                            {
                                                double nChk6 = 0;
                                                double nChk7 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")) != "")
                                                {
                                                    nChk6 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")) != "")
                                                {
                                                    nChk7 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")));
                                                }

                                                #region 06 음 07 음
                                                if (nChk6 < 0 && nChk7 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 음 07 음

                                                #region 06 양 07 움
                                                else if (nChk6 > 0 && nChk7 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 양 07 음

                                                #region 06 양 07 양
                                                else if (nChk6 > 0 && nChk7 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 양 07 양

                                                #region 06 음 07 양
                                                else if (nChk6 < 0 && nChk7 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 음 07 양

                                                #region 06 0 07 0
                                                else if (nChk6 == 0 && nChk7 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 0 07 0
                                            }
                                        }
                                        #endregion 05 음

                                        #region 05 양
                                        else if (n05 > 0 && string.Equals(e.Cell.Column.Name, "5"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")) != "")
                                            {
                                                double nChk6 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")));
                                                double nChk7 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")));

                                                #region 06 음 07 음
                                                if (nChk6 < 0 && nChk7 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 음 07 음

                                                #region 06 양 07 움
                                                else if (nChk6 > 0 && nChk7 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 양 07 음

                                                #region 06 양 07 양
                                                else if (nChk6 > 0 && nChk7 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 양 07 양

                                                #region 06 음 07 양
                                                else if (nChk6 < 0 && nChk7 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 음 07 양

                                                #region 06 0 07 0
                                                else if (nChk6 == 0 && nChk7 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 0 07 0
                                            }
                                            else
                                            {
                                                double nChk6 = 0;
                                                double nChk7 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")) != "")
                                                {
                                                    nChk6 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")) != "")
                                                {
                                                    nChk7 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")));
                                                }

                                                #region 06 음 07 음
                                                if (nChk6 < 0 && nChk7 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 음 07 음

                                                #region 06 양 07 움
                                                else if (nChk6 > 0 && nChk7 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 양 07 음

                                                #region 06 양 07 양
                                                else if (nChk6 > 0 && nChk7 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 양 07 양

                                                #region 06 음 07 양
                                                else if (nChk6 < 0 && nChk7 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 음 07 양

                                                #region 06 0 07 0
                                                else if (nChk6 == 0 && nChk7 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 06 0 07 0
                                            }
                                        }
                                        #endregion 05 양
                                    }
                                    #endregion 05

                                    #region 06
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")) != "")
                                    {
                                        double n06 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "6")));

                                        #region 06 음
                                        if (n06 < 0 && string.Equals(e.Cell.Column.Name, "6"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")) != "")
                                            {
                                                double nChk7 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")));
                                                double nChk8 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")));

                                                #region 07 음 08 음
                                                if (nChk7 < 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 음

                                                #region 07 양 08 움
                                                else if (nChk7 > 0 && nChk8 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 음

                                                #region 07 양 08 양
                                                else if (nChk7 > 0 && nChk8 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 양

                                                #region 07 음 08 양
                                                else if (nChk7 < 0 && nChk8 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 양

                                                #region 07 0 08 0
                                                else if (nChk7 == 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 0

                                                #region 07 양 08 0
                                                else if (nChk7 > 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 0

                                                #region 07 음 08 0
                                                else if (nChk7 < 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 0

                                                #region 07 0 08 양
                                                else if (nChk7 == 0 && nChk8 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 양

                                                #region 07 0 08 음
                                                else if (nChk7 == 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 음
                                            }
                                            else
                                            {
                                                double nChk7 = 0;
                                                double nChk8 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")) != "")
                                                {
                                                    nChk7 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")) != "")
                                                {
                                                    nChk8 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")));
                                                }


                                                #region 07 음 08 음
                                                if (nChk7 < 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 음

                                                #region 07 양 08 움
                                                else if (nChk7 > 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 음

                                                #region 07 양 08 양
                                                else if (nChk7 > 0 && nChk8 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 양

                                                #region 07 음 08 양
                                                else if (nChk7 < 0 && nChk8 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 양

                                                #region 07 0 08 0
                                                else if (nChk7 == 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 0

                                                #region 07 양 08 0
                                                else if (nChk7 > 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 0

                                                #region 07 음 08 0
                                                else if (nChk7 < 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 0

                                                #region 07 0 08 양
                                                else if (nChk7 == 0 && nChk8 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 양

                                                #region 07 0 08 음
                                                else if (nChk7 == 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 음

                                            }
                                        }
                                        #endregion 06 음

                                        #region 06 양
                                        else if (n06 > 0 && string.Equals(e.Cell.Column.Name, "6"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")) != "")
                                            {
                                                double nChk7 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")));
                                                double nChk8 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")));

                                                #region 07 음 08 음
                                                if (nChk7 < 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 음

                                                #region 07 양 08 움
                                                else if (nChk7 > 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 음

                                                #region 07 양 08 양
                                                else if (nChk7 > 0 && nChk8 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 양

                                                #region 07 음 08 양
                                                else if (nChk7 < 0 && nChk8 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 양

                                                #region 07 0 08 0
                                                else if (nChk7 == 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 0

                                                #region 07 양 08 0
                                                else if (nChk7 > 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 0

                                                #region 07 음 08 0
                                                else if (nChk7 < 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 0

                                                #region 07 0 08 양
                                                else if (nChk7 == 0 && nChk8 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 양

                                                #region 07 0 08 음
                                                else if (nChk7 == 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 음
                                            }
                                            else
                                            {
                                                double nChk7 = 0;
                                                double nChk8 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")) != "")
                                                {
                                                    nChk7 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")) != "")
                                                {
                                                    nChk8 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")));
                                                }

                                                #region 07 음 08 음
                                                if (nChk7 < 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 음

                                                #region 07 양 08 움
                                                else if (nChk7 > 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 음

                                                #region 07 양 08 양
                                                else if (nChk7 > 0 && nChk8 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 양

                                                #region 07 음 08 양
                                                else if (nChk7 < 0 && nChk8 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 양

                                                #region 07 0 08 0
                                                else if (nChk7 == 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 0

                                                #region 07 양 08 0
                                                else if (nChk7 > 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 양 08 0

                                                #region 07 음 08 0
                                                else if (nChk7 < 0 && nChk8 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 음 08 0

                                                #region 07 0 08 양
                                                else if (nChk7 == 0 && nChk8 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 양

                                                #region 07 0 08 음
                                                else if (nChk7 == 0 && nChk8 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 07 0 08 음
                                            }
                                        }
                                        #endregion 06 양
                                    }
                                    #endregion 06

                                    #region 07
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")) != "")
                                    {
                                        double n07 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "7")));

                                        #region 07 음
                                        if (n07 < 0 && string.Equals(e.Cell.Column.Name, "7"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")) != "")
                                            {
                                                double nChk8 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")));
                                                double nChk9 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")));

                                                #region 08 음 09 음
                                                if (nChk8 < 0 && nChk9 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 음

                                                #region 08 양 09 움
                                                else if (nChk8 > 0 && nChk9 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 음

                                                #region 08 양 09 양
                                                else if (nChk8 > 0 && nChk9 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 양

                                                #region 08 음 09 양
                                                else if (nChk8 < 0 && nChk9 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 양

                                                #region 08 0 09 0
                                                else if (nChk8 == 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 0

                                                #region 08 양 09 0
                                                else if (nChk8 > 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 0

                                                #region 08 음 09 0
                                                else if (nChk8 < 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 0

                                                #region 08 0 09 양
                                                else if (nChk8 == 0 && nChk9 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 양

                                                #region 08 0 09 음
                                                else if (nChk8 == 0 && nChk9 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 음
                                            }
                                            else
                                            {
                                                double nChk8 = 0;
                                                double nChk9 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")) != "")
                                                {
                                                    nChk8 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")) != "")
                                                {
                                                    nChk9 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")));
                                                }

                                                #region 08 음 09 음
                                                if (nChk8 < 0 && nChk9 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 음

                                                #region 08 양 09 움
                                                else if (nChk8 > 0 && nChk9 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 음

                                                #region 08 양 09 양
                                                else if (nChk8 > 0 && nChk9 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 양

                                                #region 08 음 09 양
                                                else if (nChk8 < 0 && nChk9 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 양

                                                #region 08 0 09 0
                                                else if (nChk8 == 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 0

                                                #region 08 양 09 0
                                                else if (nChk8 > 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 0

                                                #region 08 음 09 0
                                                else if (nChk8 < 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 0

                                                #region 08 0 09 양
                                                else if (nChk8 == 0 && nChk9 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 양

                                                #region 08 0 09 음
                                                else if (nChk8 == 0 && nChk9 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 음
                                            }
                                        }
                                        #endregion 07 음

                                        #region 07 양
                                        else if (n07 > 0 && string.Equals(e.Cell.Column.Name, "7"))
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")) != "" &&
                                                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")) != "")
                                            {
                                                double nChk8 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")));
                                                double nChk9 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")));

                                                #region 08 음 09 음
                                                if (nChk8 < 0 && nChk9 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 음

                                                #region 08 양 09 움
                                                else if (nChk8 > 0 && nChk9 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 음

                                                #region 08 양 09 양
                                                else if (nChk8 > 0 && nChk9 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 양

                                                #region 08 음 09 양
                                                else if (nChk8 < 0 && nChk9 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 양

                                                #region 08 0 09 0
                                                else if (nChk8 == 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 0

                                                #region 08 양 09 0
                                                else if (nChk8 > 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 0

                                                #region 08 음 09 0
                                                else if (nChk8 < 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 0

                                                #region 08 0 09 양
                                                else if (nChk8 == 0 && nChk9 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 양

                                                #region 08 0 09 음
                                                else if (nChk8 == 0 && nChk9 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 음
                                            }
                                            else
                                            {
                                                double nChk8 = 0;
                                                double nChk9 = 0;

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")) != "")
                                                {
                                                    nChk8 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "8")));
                                                }

                                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")) != "")
                                                {
                                                    nChk9 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "9")));
                                                }

                                                #region 08 음 09 음
                                                if (nChk8 < 0 && nChk9 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 음

                                                #region 08 양 09 움
                                                else if (nChk8 > 0 && nChk9 < 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 음

                                                #region 08 양 09 양
                                                else if (nChk8 > 0 && nChk9 > 0)
                                                {
                                                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    //e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    //e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 양

                                                #region 08 음 09 양
                                                else if (nChk8 < 0 && nChk9 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 양

                                                #region 08 0 09 0
                                                else if (nChk8 == 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 0

                                                #region 08 양 09 0
                                                else if (nChk8 > 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 양 09 0

                                                #region 08 음 09 0
                                                else if (nChk8 < 0 && nChk9 == 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 음 09 0

                                                #region 08 0 09 양
                                                else if (nChk8 == 0 && nChk9 > 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 양

                                                #region 08 0 09 음
                                                else if (nChk8 == 0 && nChk9 < 0)
                                                {
                                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                                                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                                                }
                                                #endregion 08 0 09 음
                                            }
                                        }
                                        #endregion 07 양
                                    }
                                    #endregion 07
                                }

                                //2020.06.26
                                HiddenLoadingIndicator();

                                //2020.06.26
                                bClick = false;
                            }
                        }
                    } //dgStock
                }
            }
        }

        #endregion Method

        private void btnExpandFrameLeft_Checked(object sender, RoutedEventArgs e)
        {
            //GridArea.RowDefinitions[0].Height = new GridLength(0);


            //LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            //gla.From = new GridLength(1, GridUnitType.Star);
            //gla.To = new GridLength(0, GridUnitType.Star);
            //gla.Duration = new TimeSpan(0, 0, 0, 0, 500);

            //GridArea.RowDefinitions[1].Height = new GridLength(0);
        }

        private void btnExpandFrameLeft_Unchecked(object sender, RoutedEventArgs e)
        {
            //GridArea.RowDefinitions[0].Height = new GridLength(300);
            //GridArea.RowDefinitions[1].Height = new GridLength(8);
            //GridArea.RowDefinitions[2].Height = GridLength.Auto;
            //GridArea.RowDefinitions[3].Height = new GridLength(8);
            //GridArea.RowDefinitions[4].Height = new GridLength(1, GridUnitType.Star);

            //LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            //gla.From = new GridLength(0);
            //gla.To = new GridLength(1);
            //gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
        }
    }
}