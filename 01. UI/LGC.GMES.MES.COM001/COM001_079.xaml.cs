/*************************************************************************************
 Created Date : 2017.05.23
      Creator : 
   Decription : 창고 재공 조회
--------------------------------------------------------------------------------------
 [Change History]
  2017.05.23  DEVELOPER : Initial Created.


 
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
    public partial class COM001_079 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        String _PRODID = "";

        public COM001_079()
        {
            InitializeComponent();
            InitCombo();
            SetElec();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnRoute);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            if (FrameOperation.AUTHORITY.Equals("W"))
            {
            }
            else
            {
            }
            //여기까지 사용자 권한별로 버튼 숨기기
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {

            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //동
            C1ComboBox[] cboAreaChild = { cboElecWareHouse }; //cboEquipmentSegment, 
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //전극
            String[] sFilter1 = { "ELTR_TYPE_CODE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            //wip상태
            _combo.SetCombo(cboWipStat, CommonCombo.ComboStatus.ALL);

            // 창고명
            C1ComboBox[] cboWareHouseParent = { cboArea };
            C1ComboBox[] cboareHouseChild = { cboElecRack };
            _combo.SetCombo(cboElecWareHouse, CommonCombo.ComboStatus.ALL, cbChild: cboareHouseChild, cbParent: cboWareHouseParent);

            // RACK
            C1ComboBox[] cboRackParent = { cboElecWareHouse };
            _combo.SetCombo(cboElecRack, CommonCombo.ComboStatus.ALL, cbParent: cboRackParent);

            DataTable dtWipStat = DataTableConverter.Convert(cboWipStat.ItemsSource);
            
            for (int i = dtWipStat.Rows.Count-1; i >= 2; i--)
            {
                dtWipStat.Rows.Remove(dtWipStat.Rows[i]);
            }

            DataRow dr = dtWipStat.NewRow();
            dr["CBO_NAME"] = "HOLD:HOLD";
            dr["CBO_CODE"] = "HOLD";
            dtWipStat.Rows.Add(dr);

            cboWipStat.ItemsSource = dtWipStat.Copy().AsDataView();

        }

        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetStock();
        }

        private void cboWipStat_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dgSummary.GetRowCount() > 0 && dgSummary.CurrentRow != null)
            {
                GetDetailLot(Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PROCID")), 
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PRODID")),
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PROD_VER_CODE")),
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "WH_ID")),
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PRJT_NAME"))
                             );
            }
        }

        private void dgSummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgSummary.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("PRODID") || e.Cell.Column.Name.Equals("WH_NAME") || e.Cell.Column.Name.Equals("PROCNAME") || e.Cell.Column.Name.Equals("PRJT_NAME"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }

            }));
        }

        private void dgSummary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                if (dg.CurrentColumn.Name.Equals("PRODID") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                {
                    GetDetailLot(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID")), 
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROD_VER_CODE")),
                                 Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "WH_ID")),
                                 Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PRJT_NAME"))
                                 );

                    _PRODID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID"));
                }
                else if (dg.CurrentColumn.Name.Equals("WH_NAME") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                {
                    GetDetailLot(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID")),"", null,
                                 Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "WH_ID"))
                                 ,null
                                 );

                    _PRODID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID"));
                }
                else if (dg.CurrentColumn.Name.Equals("PROCNAME") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                {
                    GetDetailLot(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID")), "", null,
                                 Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "WH_ID")), null
                                 );

                    _PRODID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID"));
                }
                else if (dg.CurrentColumn.Name.Equals("PRJT_NAME") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                {
                    GetDetailLot(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID")), "", null,
                                 Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "WH_ID")), 
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRJT_NAME"))
                                 );

                    _PRODID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID"));
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

        private void GetDetailLot(string sProcId, string sProdId, string sProdVerCode, string sWhId, string sPrjtName)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("WIPSTAT", typeof(string));
                dtRqst.Columns.Add("PROD_VER_CODE", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("WH_ID", typeof(string));
                dtRqst.Columns.Add("PRDT_CLSS_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();

                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203"); //동은필수입니다.
                if (dr["AREAID"].Equals("")) return;
                dr["PROCID"] = sProcId;
                dr["PRODID"] = sProdId == "" ? null : sProdId;
                dr["WIPSTAT"] = Util.GetCondition(cboWipStat).Trim() == "" ? null : Util.GetCondition(cboWipStat);
                dr["PROD_VER_CODE"] = sProdVerCode;
                dr["PRJT_NAME"] = sPrjtName == "" ? null : sPrjtName;
                dr["WH_ID"] = sWhId;// == "" ? null : sWhId;
                dr["PRDT_CLSS_CODE"] = Util.GetCondition(cboElecType) == "" ? null : Util.GetCondition(cboElecType);

                dtRqst.Rows.Add(dr);
                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_IN_AREA_DETAIL_WH", "INDATA", "OUTDATA", dtRqst);


                Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Method
        private void GetStock()
        {
            try
            {
                string sBizName = "DA_PRD_SEL_STOCK_IN_AREA_WH";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("WH_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();

                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203");//동은필수입니다.
                if (dr["AREAID"].Equals("")) return;

                dr["EQSGID"] = "";
                dr["PROCID"] = "";
                dr["PRODID"] = Util.GetCondition(txtProdId);
                dr["PRDT_CLSS_CODE"] = Util.GetCondition(cboElecType);
                dr["MODLID"] = Util.GetCondition(txtModlId);
                dr["PRJT_NAME"] = Util.GetCondition(txtPrjtName);
                dr["WH_ID"] = Util.GetCondition(cboElecWareHouse);

                dtRqst.Rows.Add(dr);
                dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", dtRqst);
                
                Util.GridSetData(dgSummary, dtRslt, FrameOperation, true);
                Util.gridClear(dgLotList);

                if (dtRslt.Rows.Count > 0)
                {
                    string[] sColumnName = new string[] {"WH_NAME", "PROCNAME", "PRJT_NAME" };
                    _Util.SetDataGridMergeExtensionCol(dgSummary, sColumnName, DataGridMergeMode.VERTICAL);

                    dgSummary.GroupBy(dgSummary.Columns["PROCWH_NAME"], DataGridSortDirection.None);
                    dgSummary.GroupRowPosition = DataGridGroupRowPosition.AboveData;
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRJT_NAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = "" } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRODID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("Total") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRODNAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = "" } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRDT_CLSS_CODE"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = "" } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["UNIT_CODE"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = "" } });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WAIT_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WAIT_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WAIT_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROC_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROC_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROC_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["END_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["END_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["END_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVING_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVING_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVING_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUM_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUM_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
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

        private void SetElec()
        {

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new DataTable();

                dtRslt = new ClientProxy().ExecuteServiceSync("CUS_SEL_AREAATTR", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Rows[0]["S01"].ToString().Equals("E"))
                    {
                        tbElecType.Visibility = Visibility.Visible;
                        cboElecType.Visibility = Visibility.Visible;
                        dgSummary.Columns["PRDT_CLSS_CODE"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        tbElecType.Visibility = Visibility.Collapsed;
                        cboElecType.Visibility = Visibility.Collapsed;
                        dgSummary.Columns["PRDT_CLSS_CODE"].Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        #endregion

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void dgSummary_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void dgLotList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Bottom)
                        {
                            StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                            ContentPresenter presenter = panel.Children[0] as ContentPresenter;
                            if (e.Cell.Column.Index == dataGrid.Columns["LOTID"].Index)
                            {
                                if (e.Cell.Row.Index == (dataGrid.Rows.Count - 1))
                                    presenter.Content = ObjectDic.Instance.GetObjectName("합계");
                            }
                            else if (e.Cell.Column.Index == dataGrid.Columns["WIPQTY"].Index) // 측정값
                            {
                                if (e.Cell.Row.Index == (dataGrid.Rows.Count - 1))
                                    if (presenter.Content.ToString().Equals("NaN"))
                                        presenter.Content = 0;
                            }
                        }
                    }
                }));
            }
        }
    }
}
