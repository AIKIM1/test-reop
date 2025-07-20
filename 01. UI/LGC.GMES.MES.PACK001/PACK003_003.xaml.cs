/*************************************************************************************
 Created Date : 2020.09.09
      Creator : 김길용
   Decription : 동간이동 자동물류 Cell 재고 예상(Demestic)
--------------------------------------------------------------------------------------
 [Change History]
 2020.09.09  김길용 : Initial Created.
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

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_003 : UserControl, IWorkArea
    {

        public bool bClick = false;
        string sMtrlid = string.Empty;
        string sMtrlname = string.Empty;
        string sPlanqty = string.Empty;
        string chkRemovetg_Flag = "N";
        bool bReturn = true;
        int Rint = 0;
        #region Declaration & Constructor & Init
        public PACK003_003()
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
        //Cell 재고예상(Domestic) Search
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (bClick == false)
                {
                    Action act = () =>
                    {
                        bClick = true;
                        SearchData();
                    };
                    btnSearch.Dispatcher.Invoke(act);
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                bClick = false;
            }
        }
        //Cell 현황조회 Search
        private void btnCell_ISI_Search_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (bClick == false)
                {
                    Action act = () =>
                    {
                        bClick = true;


                        SearchSumData();
                    };
                    btnSearch.Dispatcher.Invoke(act);
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();

                bClick = false;
            }
        }
        //투입실적 - 계획총량 더블클릭 시 POPUP을 위한 처리
        private void dgInput2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgInput2.GetCellFromPoint(pnt);
            if (cell != null)
            {
                if (cell.Row.Index > 2)
                {
                    string sMtrlid = Util.NVC(DataTableConverter.GetValue(dgInput2.Rows[cell.Row.Index].DataItem, "MTRLID"));
                    string sMtrlname = Util.NVC(DataTableConverter.GetValue(dgInput2.Rows[cell.Row.Index].DataItem, "MTRL_PRJT_NAME"));
                    string sPlanqty = Util.NVC(DataTableConverter.GetValue(dgInput2.Rows[cell.Row.Index].DataItem, "PLANQTY"));
                    if (cell.Column.Name == "PLANQTY")
                    {
                        popUpOpenPalletInfo(sMtrlid, sMtrlname, sPlanqty);
                    }
                }
            }
        }

        private void btnExcel_CELL_ISI_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgCell_ISI);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion Button

        #region Grid
        //색상처리 - 계획총량
        private void dgInput2_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dataGrid = (sender as C1DataGrid);

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    // 색상 구분
                    if (e.Cell.Column.Name.ToString().Equals("PLANQTY"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch
            {
                HiddenLoadingIndicator();

                bClick = false;
            }
        }
        //Cell 현황조회 - 계획대비보유 현황이 음수일 경우 색상처리
        private void dgCell_ISI_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dataGrid = (sender as C1DataGrid);

                Action act = () =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name.Equals("DIFF_QTY_TOT"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DIFF_QTY_TOT")) != "")
                        {
                            string data = (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DIFF_QTY_TOT"))).Replace(",", "");
                            int sdata = int.Parse(data);
                            //int sDiff_Qty = int.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DIFF_QTY_TOT")));
                            if (sdata < 0)
                            {
                                //음수표현일 경우
                                dgCell_ISI.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                                dgCell_ISI.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                dgCell_ISI.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.FontWeight = FontWeights.Bold;
                                return;
                            }

                        }
                    }
                };

                dataGrid.Dispatcher.BeginInvoke(act);
            }
            catch
            {
                HiddenLoadingIndicator();

                bClick = false;
            }
        }
        //재고예상 - 시간별 색상처리 ( 음수예상 - 빨간색 , 음수예상 시각 - 2H - 노란색 , 양수예상 - 초록색 )
        private void dgStock2_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dataGrid = (sender as C1DataGrid);

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    // 색상 구분
                    if (e.Cell.Column.Index <= 14)
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    //인덱스 13 이후 ==> 08~07를 나타냄
                    else
                    {
                        if (e.Cell.Column.Name == "8")
                        {
                            bReturn = true;
                        }

                        if ((Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "HH_SUM")).Length == 0))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                            return;
                        }
                        int Rint = int.Parse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "HH_SUM")));
                        int sName = int.Parse(e.Cell.Column.Name);

                        if (sName == Rint - 1)
                        {
                            if (Rint != 8)
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                if (Rint != 9)
                                {
                                    dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - 1).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - 1).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                    return;
                                }
                                return;
                            }
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            return;

                        }
                        if (Rint == 1 && sName == 1)
                        {
                            bReturn = false;

                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);

                            dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - 1).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - 1).Presenter.Background = new SolidColorBrush(Colors.Yellow);

                            dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - 2).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - 2).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            return;
                        }
                        if (bReturn == true && sName != Rint)
                        {
                            //bReturn = false;
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                            return;
                        }
                        if (sName == Rint)
                        {
                            bReturn = false;

                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            return;
                        }
                        if (bReturn == false)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            return;
                        }
                    }
                }));

                return;
                
                //Action act = () =>
                //{
                //    if (e.Cell.Presenter == null)
                //    {
                //        return;
                //    }
                //    if (e.Cell.Row.Type == DataGridRowType.Item)
                //    {
                //        if (e.Cell.Column.Index < 14)
                //        {
                //            dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                //            //dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new SolidColorBrush(Colors.Green);
                //            return;
                //        }
                //        //인덱스 13 이후 ==> 08~07를 나타냄
                //        if (e.Cell.Column.Index > 14)
                //        {
                //            if (e.Cell.Column.Name == "8")
                //            {
                //                bReturn = true;
                //            }

                //            if ((Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "HH_SUM")).Length == 0))
                //            {
                //                dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.White);
                //                dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new SolidColorBrush(Colors.Green);
                //                return;
                //            }
                //            int Rint = int.Parse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "HH_SUM")));
                //            int sName = int.Parse(e.Cell.Column.Name);

                //            if (sName == Rint - 1)
                //            {
                //                if (Rint != 8)
                //                {
                //                    dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                //                    dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                //                    if (Rint != 9)
                //                    {
                //                        dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - 1).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                //                        dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - 1).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                //                        return;
                //                    }
                //                    return;
                //                }

                //                dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.White);
                //                dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                //                return;

                //            }
                //            if (Rint == 1 && sName == 1)
                //            {
                //                bReturn = false;

                //                dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.White);
                //                dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new SolidColorBrush(Colors.Red);

                //                dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - 1).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                //                dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - 1).Presenter.Background = new SolidColorBrush(Colors.Yellow);

                //                dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - 2).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                //                dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - 2).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                //                return;
                //            }
                //            if (bReturn == true && sName != Rint)
                //            {
                //                //bReturn = false;
                //                dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.White);
                //                dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new SolidColorBrush(Colors.Green);
                //                return;
                //            }
                //            if (sName == Rint)
                //            {
                //                bReturn = false;

                //                dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.White);
                //                dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                //                return;
                //            }
                //            if (bReturn == false)
                //            {
                //                dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.White);
                //                dgStock2.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                //                return;
                //            }
                //        }
                //    }

                //};

                //dataGrid.Dispatcher.BeginInvoke(act);


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

        //Cell 재고예상 조회
        private void SearchData()
        {
            DataSet dsResult = null;
            ShowLoadingIndicator();
            //loadingIndicator.Visibility = Visibility.Visible;
            try
            {

                if (chkRemovetg.IsChecked == true)
                    chkRemovetg_Flag = "Y";

                DataSet ds = new DataSet();

                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("SHOPID", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("EQSGID", typeof(string));
                dtINDATA.Columns.Add("YMD", typeof(DateTime));
                dtINDATA.Columns.Add("NOW", typeof(DateTime));
                dtINDATA.Columns.Add("LOGIS_FLAG", typeof(string));
                dtINDATA.Columns.Add("MANUAL_LOGIS_FLAG", typeof(string));
                dtINDATA.Columns.Add("NOW_WO_STAND_YN", typeof(string));

                DataRow dr = dtINDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = null;
                dr["YMD"] = DBNull.Value;
                dr["NOW"] = DBNull.Value; ;
                dr["LOGIS_FLAG"] = chkRemovetg_Flag.ToString();
                dr["MANUAL_LOGIS_FLAG"] = null;
                dr["NOW_WO_STAND_YN"] = null;
                dtINDATA.Rows.Add(dr);

                ds.Tables.Add(dtINDATA);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_RPT_CELL_DATA_LOGIS", dtINDATA.TableName, "INPUT2,STOCK2", ds, null);

                if (dsResult != null)
                {
                    if ((dsResult.Tables["INPUT2"].Rows.Count > 0) || (dsResult.Tables["STOCK2"].Rows.Count > 0))
                    {
                        Util.GridSetData(dgInput2, dsResult.Tables["INPUT2"], FrameOperation, false);
                        Util.GridSetData(dgStock2, dsResult.Tables["STOCK2"], FrameOperation, false);
                        HiddenLoadingIndicator();
                        //loadingIndicator.Visibility = Visibility.Collapsed;
                        bClick = false;
                        chkRemovetg_Flag = "N";
                    }
                    else
                    {
                        HiddenLoadingIndicator();
                        //loadingIndicator.Visibility = Visibility.Collapsed;
                        bClick = false;
                        chkRemovetg_Flag = "N";
                    }
                }
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
                HiddenLoadingIndicator();
                //loadingIndicator.Visibility = Visibility.Collapsed;
                bClick = false;
                chkRemovetg_Flag = "N";

            }

        }
        //Cell 재고현황 조회
        private void SearchSumData()
        {
            ShowLoadingIndicator();
            DataSet dsResult = null;
            //loadingIndicator.Visibility = Visibility.Visible;
            try
            {
                if (chkRemovetgTwo.IsChecked == true)
                    chkRemovetg_Flag = "Y";

                DataSet ds = new DataSet();

                DataTable indata = new DataTable();
                indata.TableName = "INDATA";
                indata.Columns.Add("LANGID", typeof(string));
                indata.Columns.Add("SHOPID", typeof(string));
                indata.Columns.Add("AREAID", typeof(string));
                indata.Columns.Add("LOGIS_FLAG", typeof(string));

                DataRow dr = indata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LOGIS_FLAG"] = chkRemovetg_Flag.ToString();

                indata.Rows.Add(dr);

                ds.Tables.Add(indata);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_RPT_CELL_DATA_LOGIS_SUM", "INDATA", "OUTDATA", ds, null);

                if (dsResult != null)
                {
                    if ((dsResult.Tables["OUTDATA"].Rows.Count > 0))
                    {
                        Util.GridSetData(dgCell_ISI, dsResult.Tables["OUTDATA"], FrameOperation, false);
                        this.tbCell_ISI_Count.Text = "[" + dsResult.Tables["OUTDATA"].Rows.Count.ToString() + " 건]";
                        bClick = false;
                        HiddenLoadingIndicator();
                        //loadingIndicator.Visibility = Visibility.Collapsed;
                        chkRemovetg_Flag = "N";
                    }
                    else
                    {
                        HiddenLoadingIndicator();
                        //loadingIndicator.Visibility = Visibility.Collapsed;
                        this.tbCell_ISI_Count.Text = "[0 건]";
                        chkRemovetg_Flag = "N";
                        bClick = false;
                    }
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                //loadingIndicator.Visibility = Visibility.Collapsed;
                chkRemovetg_Flag = "N";
                bClick = false;
            }
        }
        //투입실적 계획총량 상세 팝업처리
        private void popUpOpenPalletInfo(string sMtrlid, string sMtrlname, string sPlanqty)
        {
            try
            {
                PACK003_003_POPUP popup = new PACK003_003_POPUP();
                popup.FrameOperation = this.FrameOperation;
                if (chkRemovetg.IsChecked == true)
                    chkRemovetg_Flag = "Y";

                if (popup != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = sMtrlid;
                    Parameters[1] = sMtrlname;
                    Parameters[2] = sPlanqty;
                    Parameters[3] = chkRemovetg_Flag;
                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
                chkRemovetg_Flag = "N";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Method

    }
}