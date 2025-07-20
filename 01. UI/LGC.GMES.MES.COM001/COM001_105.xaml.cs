/*************************************************************************************
 Created Date : 2017.08.31
      Creator : J.S HONG
   Decription : 비용처리 현황 조회 < 특이작업
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.31  J.S HONG : Initial Created.


 
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
using System.Linq;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_105 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public COM001_105()
        {
            InitializeComponent();
            InitCombo();
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

        }

        //화면내 combo 셋팅
        private void InitCombo()
        {

            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            //C1ComboBox[] cboLineChild = { cboElecType };
            C1ComboBox[] cboLineParent = { cboArea };
            C1ComboBox[] cboLineChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboLineChild, cbParent: cboLineParent);

            C1ComboBox[] cboProcessParent = { cboArea, cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent, sCase: "PROCESSWITHAREA");

            if (cboEquipmentSegment.Items.Count > 0) cboEquipmentSegment.SelectedIndex = 0;
            if (cboProcess.Items.Count > 0) cboProcess.SelectedIndex = 0;

            // 비용창고
            String[] sFilterShop = { LoginInfo.CFG_SHOP_ID, null };
            _combo.SetCombo(cboSloc, CommonCombo.ComboStatus.ALL, sFilter: sFilterShop, sCase: "SLOC_BY_COST");

            // 유형
            String[] sFilterReason = { "", "PRDT_REQ_TYPE_CODE" };
            _combo.SetCombo(cboPrdtReqType, CommonCombo.ComboStatus.ALL, sFilter: sFilterReason, sCase: "COMMCODES");
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
                GetDetailLot(Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "EQSGID")), 
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PROCID")), 
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PRODID")),
                             Util.NVC(Convert.ToDateTime(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "CALDATE")).ToString("yyyy-MM-dd")));
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
                if (e.Cell.Column.Name.Equals("PRODID") || e.Cell.Column.Name.Equals("PROCNAME"))
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
                    GetDetailLot(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "EQSGID")), 
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID")), 
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID")),
                                 Util.NVC(Convert.ToDateTime(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "CALDATE")).ToString("yyyy-MM-dd")));
                }
                else if (dg.CurrentColumn.Name.Equals("PROCNAME") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                {
                    GetDetailLot(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "EQSGID")), 
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID")), null, null);
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

        private void dgLotList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgLotList.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                
                //if (e.Cell.Column.Name.Equals("LOTID") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD")).Equals("Y"))
                //{
                //    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                //}

            }));
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            new LGC.GMES.MES.Common.ExcelExporter().Export(dgLotList);
        }

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void dgSummary_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }
        #endregion

        #region Method
        private void GetStock()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("COST_PRCS_SLOC_ID", typeof(string));
                dtRqst.Columns.Add("PRDT_REQ_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("LOT", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203");//동은필수입니다.
                if (dr["AREAID"].Equals("")) return;
                dr["EQSGID"] = Util.ConvertEmptyToNull((string)cboEquipmentSegment.SelectedValue);
                dr["PROCID"] = Util.ConvertEmptyToNull((string)cboProcess.SelectedValue);
                dr["COST_PRCS_SLOC_ID"] = Util.ConvertEmptyToNull((string)cboSloc.SelectedValue);
                dr["PRDT_REQ_TYPE_CODE"] = Util.ConvertEmptyToNull((string)cboPrdtReqType.SelectedValue);
                dr["PRODID"] = Util.ConvertEmptyToNull(txtProdId.Text.Trim());
                dr["MODLID"] = Util.ConvertEmptyToNull(txtModlId.Text.Trim());
                dr["PRJT_NAME"] = Util.ConvertEmptyToNull(txtPrjtName.Text.Trim());
                dr["LOT"] = Util.ConvertEmptyToNull(txtLotid.Text.Trim());
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_COST_PRDT_REQ_HIST_LOT_LIST", "INDATA", "OUTDATA", dtRqst);
                
                Util.GridSetData(dgSummary, dtRslt, FrameOperation, true);
                Util.gridClear(dgLotList);

                if (dtRslt.Rows.Count > 0)
                {
                    string[] sColumnName = new string[] { "EQSGNAME", "PROCNAME"};
                    _Util.SetDataGridMergeExtensionCol(dgSummary, sColumnName, DataGridMergeMode.VERTICAL);

                    dgSummary.GroupBy(new DataGridColumnValue<DataGridSortDirection>(dgSummary.Columns["EQSGNAME"], DataGridSortDirection.None), new DataGridColumnValue<DataGridSortDirection>(dgSummary.Columns["PROCNAME"], DataGridSortDirection.None));
                    dgSummary.GroupRowPosition = DataGridGroupRowPosition.AboveData;

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROCNAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("Total") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MODLID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRJT_NAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRODID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRODNAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["UNIT_CODE"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["RESNQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["RESNQTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
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

        private void GetDetailLot(string sEqsgId, string sProcId, string sProdId, string sCALDATE)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("CALDATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sEqsgId;
                dr["PRODID"] = sProdId;
                dr["PROCID"] = sProcId;
                dr["CALDATE"] = sCALDATE;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_COST_PRDT_REQ_HIST_LOT_DETAIL", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        
    }
}
