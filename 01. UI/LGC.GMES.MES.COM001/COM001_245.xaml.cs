/*************************************************************************************
 Created Date : 2018.07.23
      Creator : 
   Decription : 전극창고-물류 재고 Summary SNAP SHOT
--------------------------------------------------------------------------------------
 [Change History]
 
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_245 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_245()
        {
            InitializeComponent();
            InitCombo();
        }

        #endregion Declaration & Constructor 


        #region Initialize

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();            

            /// 동 ///            
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT);
            
            /// 공정 ///
            String[] sFilter1 = { "STOCK_SUMMARY_GUBUN" };            
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            /// 극성 ///
            String[] sFilter2 = { "ELTR_TYPE_CODE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");
            
            if (cboProcess.Items.Count > 0) cboProcess.SelectedIndex = 0;            
        }

        #endregion Initialize


        #region Event
        private void dgSummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgSummary.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("PRODID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }

            }));
        }
        private void dgLotList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgLotList.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                // LotID 컬럼 색상 변경
                if (e.Cell.Column.Name.Equals("LOTID") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD")).Equals("Y"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                }
            }));
        }

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgSummary); 
            Util.gridClear(dgLotList);

            GetStockSummary();            
        }

        private void chkCollapsed_Checked(object sender, RoutedEventArgs e)
        {            
            dgSummary.Columns["AREANAME"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["PROCNAME"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["QMS_NG_QTY2"].Visibility  = Visibility.Collapsed;
            dgSummary.Columns["WAIT_LOT_QTY2"].Visibility = Visibility.Collapsed; ;            
            dgSummary.Columns["HOLD_LOT_QTY2"].Visibility = Visibility.Collapsed; ;                        
            dgSummary.Columns["GNRL_MOVING_OUT_QTY2"].Visibility = Visibility.Collapsed; ;        
            dgSummary.Columns["CNVR_MOVING_OUT_QTY2"].Visibility = Visibility.Collapsed; ;            
            dgSummary.Columns["GNRL_MOVING_IN_QTY2"].Visibility = Visibility.Collapsed; ;            
            dgSummary.Columns["CNVR_MOVING_IN_QTY2"].Visibility = Visibility.Collapsed; ;            
            dgSummary.Columns["SUM_QTY2"].Visibility = Visibility.Collapsed; ;                 
        }
        private void chkCollapsed_Unchecked(object sender, RoutedEventArgs e)
        {
            dgSummary.Columns["AREANAME"].Visibility = Visibility.Visible;
            dgSummary.Columns["PROCNAME"].Visibility = Visibility.Visible;
            dgSummary.Columns["QMS_NG_QTY2"].Visibility = Visibility.Visible;
            dgSummary.Columns["WAIT_LOT_QTY2"].Visibility = Visibility.Visible; ;
            dgSummary.Columns["HOLD_LOT_QTY2"].Visibility = Visibility.Visible; ;
            dgSummary.Columns["GNRL_MOVING_OUT_QTY2"].Visibility = Visibility.Visible; ;
            dgSummary.Columns["CNVR_MOVING_OUT_QTY2"].Visibility = Visibility.Visible; ;
            dgSummary.Columns["GNRL_MOVING_IN_QTY2"].Visibility = Visibility.Visible; ;
            dgSummary.Columns["CNVR_MOVING_IN_QTY2"].Visibility = Visibility.Visible; ;
            dgSummary.Columns["SUM_QTY2"].Visibility = Visibility.Visible; ;
        }

        private void dgSummary_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }
        private void dgSummary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                if (dg.CurrentColumn.Name.Equals("PRODID") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "STCK_SUM_TYPE")).Equals(""))
                        return;

                    GetDetailLot(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "AREAID")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "STCK_SUM_TYPE")));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion Event


        #region Method

        private void GetStockSummary()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SUM_DATE", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("STCK_SUM_TYPE", typeof(string));
                dtRqst.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("QMS_NG", typeof(string));
                                
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUM_DATE"] = Util.GetCondition(dtpDate);
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203"); //동은필수입니다.
                if (dr["AREAID"].Equals("")) return;
                dr["STCK_SUM_TYPE"] = Util.GetCondition(cboProcess, bAllNull: true);
                dr["PRDT_CLSS_CODE"] = Util.GetCondition(cboElecType, bAllNull: true);
                dr["PRJT_NAME"] = Util.GetCondition(txtPrjtName, bAllNull: true);
                dr["QMS_NG"] = chkQMSFlag.IsChecked == true ? "Y" : null;
                dtRqst.Rows.Add(dr);
            
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_SUMMARY_SNAP", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgSummary, dtRslt, FrameOperation, true);
                
                if (dtRslt.Rows.Count > 0)
                {
                    string[] sColumnName = new string[] { "STCK_SUM_TYPE_NAME" };
                    _Util.SetDataGridMergeExtensionCol(dgSummary, sColumnName, DataGridMergeMode.VERTICAL);

                    dgSummary.GroupBy(dgSummary.Columns["STCK_SUM_TYPE_NAME"], DataGridSortDirection.None);
                    dgSummary.GroupRowPosition = DataGridGroupRowPosition.AboveData;

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["AREANAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("Total") } });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["QMS_NG_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["QMS_NG_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });                    
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WAIT_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });                    
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WAIT_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["GNRL_MOVING_OUT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });                    
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["GNRL_MOVING_OUT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CNVR_MOVING_OUT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });                    
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CNVR_MOVING_OUT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["GNRL_MOVING_IN_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });                    
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["GNRL_MOVING_IN_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CNVR_MOVING_IN_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });                    
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CNVR_MOVING_IN_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });                    

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUM_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });                    
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUM_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void GetDetailLot(string sAreaID, string sProdId, string sProcId)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SUM_DATE", typeof(string));
                dtRqst.Columns.Add("STCK_SUM_TYPE", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();
                dr["SUM_DATE"] = Util.GetCondition(dtpDate); 
                dr["AREAID"] = sAreaID;
                dr["PRODID"] = sProdId;
                dr["STCK_SUM_TYPE"] = sProcId;

                dtRqst.Rows.Add(dr);

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_SUMMARY_LOT_SNAP", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion Method
    }
}
