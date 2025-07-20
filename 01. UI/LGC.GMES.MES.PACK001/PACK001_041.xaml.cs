/*************************************************************************************
 Created Date : 2019.05.07
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2019.05.07  염규범 : Initial Created.
  2019.06.04  손우석 컬럼 추가 및 색깔 표시 컬럼 변경
**************************************************************************************/

using System.Windows.Controls;
using LGC.GMES.MES.Common;
using System.Windows;
using LGC.GMES.MES.CMM001.Class;
using System;
using C1.WPF;
using System.Data;

using System.Collections.Generic;
using C1.WPF.DataGrid;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_041 : UserControl
    {
        int iStockRate;
        private object lockObject = new object();

        #region Declaration & Constructor 
        public PACK001_041()
        {
            InitializeComponent();
            setComboBox();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        #region [ setComboBox ]
        private void setComboBox()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                #region [ 라인 기준 탭 ]
                //라인 기준 탭

                //동
                String[] sFilterTabLine = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };

                C1ComboBox[] cboLineabAreaChild = { cboLineTabLine };
                _combo.SetCombo(cboLineTabArea, CommonCombo.ComboStatus.NONE, cbChild: cboLineabAreaChild, sFilter: sFilterTabLine, sCase: "AREA_AREATYPE");

                //라인
                C1ComboBox[] cboLineParent = { cboLineTabArea };
                //C1ComboBox[] cboLineChild = { cboLineTabArea };
                _combo.SetCombo(cboLineTabLine, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent, sCase: "EQUIPMENTSEGMENT");

                #endregion

                #region [ Cell 모델 기준 탭 ]
                //Cell 모델 기준 탭

                //동
                String[] sFilterTabCell = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };

                C1ComboBox[] cboCellTabAreaChild = { cboCellTabLine };
                _combo.SetCombo(cboCellTabArea, CommonCombo.ComboStatus.NONE, cbChild: cboCellTabAreaChild, sFilter: sFilterTabLine, sCase: "AREA_AREATYPE");

                //라인
                C1ComboBox[] cboCellParent = { cboCellTabArea };
                //C1ComboBox[] cboLineChild = { cboLineTabArea };
                _combo.SetCombo(cboCellTabLine, CommonCombo.ComboStatus.ALL, cbParent: cboCellParent, sCase: "EQUIPMENTSEGMENT");

                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region [ Event ]
        private void btnLineTabSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lock (lockObject)
                {
                    getSearchLineTabData();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void btnCellTabSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lock (lockObject)
                {
                    getSearchCellTabData();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
        private void getSearchLineTabData()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PRJT_NAME", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.NVC(cboLineTabArea.SelectedValue) == "" ? null : Util.NVC(cboLineTabArea.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboLineTabLine.SelectedValue) == "" ? null : Util.NVC(cboLineTabLine.SelectedValue);
                dr["PRODID"] = txtLineTabProdID.Text;
                dr["PRJT_NAME"] = txtLineTabPjt.Text;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_STOCK_BY_LINE_V2", "RQSTDT", "RSLTDT", RQSTDT);


                if (dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgLineTabSearch, dtResult, FrameOperation, false);
                }
                else
                {
                    Util.MessageInfo("SFU2881");
                }


                //getStockRate();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void getSearchCellTabData()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PRJT_NAME", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.NVC(cboCellTabArea.SelectedValue) == "" ? null : Util.NVC(cboCellTabArea.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboCellTabLine.SelectedValue) == "" ? null : Util.NVC(cboCellTabLine.SelectedValue);
                dr["PRODID"] = txtCellTabProdID.Text;
                dr["PRJT_NAME"] = txtCellTabPjt.Text;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_STOCK_BY_PRODID_V2", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgCellTabSearch, dtResult, FrameOperation, false);
                } else
                {
                    Util.MessageInfo("SFU2881");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }


        private DataTable getStockRate()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_CELL_STOCK_SIGNAL";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count == 0) {
                    return null;
                } else
                {
                    return dtResult;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void dgLineTabSearch_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;
                C1DataGrid dg = sender as C1DataGrid;

                //DataTable Color = getStockRate();
                //DataRow RangeValue = null;

                dg?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        //2019.06.04
                        //dgLineTabSearch.GetCell(e.Cell.Row.Index, 12).Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(DataTableConverter.GetValue(e.Cell.Row.DataItem, "STOCK_SIGNAL").ToString()));
                        if (Util.NVC(e.Cell.Column.Name).Equals("STOCK_RATE"))
                        {
                            dgLineTabSearch.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(DataTableConverter.GetValue(e.Cell.Row.DataItem, "STOCK_SIGNAL").ToString()));
                        }
                        /*
                        if (Convert.ToDouble(DataTableConverter.GetValue(e.Cell.Row.DataItem, "STOCK_RATE").ToString()) > 100)
                        {
                            dgLineTabSearch.GetCell(e.Cell.Row.Index, 12).Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("Green"));
                        }
                        else if (Convert.ToDouble(DataTableConverter.GetValue(e.Cell.Row.DataItem, "STOCK_RATE").ToString()) < 0)
                        {
                            dgLineTabSearch.GetCell(e.Cell.Row.Index, 12).Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("Red"));
                        }
                        else
                        {
                            for (int i = 0; i < Color.Rows.Count; i++)
                            {
                                RangeValue = Color.Rows[i];

                                if (Convert.ToDouble(DataTableConverter.GetValue(e.Cell.Row.DataItem, "STOCK_RATE").ToString()) > Convert.ToDouble(RangeValue["ATTRIBUTE1"].ToString())
                                && Convert.ToDouble(DataTableConverter.GetValue(e.Cell.Row.DataItem, "STOCK_RATE").ToString()) < Convert.ToDouble(RangeValue["ATTRIBUTE2"].ToString()))
                                {
                                    if (RangeValue["CBO_NAME"].ToString().Equals(""))
                                    {
                                        dgLineTabSearch.GetCell(e.Cell.Row.Index, 12).Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("Red"));
                                    }
                                    else
                                    {
                                        dgLineTabSearch.GetCell(e.Cell.Row.Index, 12).Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(RangeValue["CBO_NAME"].ToString()));
                                    }
                                }
                            }
                        }
                        */
                    }
                }
                ));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgCellTabSearch_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;
                C1DataGrid dg = sender as C1DataGrid;

               // DataTable Color = getStockRate();
               // DataRow RangeValue = null;

                dg?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        //2019.06.04
                        //dgCellTabSearch.GetCell(e.Cell.Row.Index, 7).Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(DataTableConverter.GetValue(e.Cell.Row.DataItem, "STOCK_SIGNAL").ToString()));
                        if(Util.NVC(e.Cell.Column.Name).Equals("STOCK_RATE"))
                        {
                            dgCellTabSearch.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(DataTableConverter.GetValue(e.Cell.Row.DataItem, "STOCK_SIGNAL").ToString()));
                        }
                    }
                }
                ));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
    }
}
