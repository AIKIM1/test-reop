using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_141.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_146 : UserControl, IWorkArea
    {
        Util _Util = new Util();

        string sEqsg_ID = string.Empty;
        string sRownum_ID = string.Empty;
        string sBiz_Type = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public COM001_146()
        {
            InitializeComponent();


        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dtpFDate.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(1 - DateTime.Now.Day);
            InitCombo();
            Loaded -= UserControl_Loaded;
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboLineParent = { cboArea };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: cboLineParent);
        }


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("SHOPID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("SCAN_DTTM_ST", typeof(DateTime));
            RQSTDT.Columns.Add("SCAN_DTTM_ED", typeof(DateTime));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment).Trim() == "" ? null : Util.GetCondition(cboEquipmentSegment);
            dr["SCAN_DTTM_ST"] = dtpFDate.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
            dr["SCAN_DTTM_ED"] = dtpTDate.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";

            RQSTDT.Rows.Add(dr);

            loadingIndicator.Visibility = Visibility.Visible;
            new ClientProxy().ExecuteService("BR_RTLS_SEL_EM_LOT_MNGT_SUMMARY_NOSCANDATA", "RQSTDT", "RSLTDT", RQSTDT, (dt, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;

                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                if (dt.Rows.Count < 1)
                {
                    Util.GridSetData(dgPalletInfo, dt, FrameOperation, true);
                    Util.Alert("SFU1905");
                    return;
                }

                Util.SetTextBlockText_DataGridRowCount(tbListCount, Util.NVC(dt.Rows.Count));

                Util.GridSetData(dgPalletInfo, dt, FrameOperation);

                if (dt.Rows.Count > 0)
                {
                    string[] sColumnName = new string[] { "EQSGNAME", "TYPE" };
                    _Util.SetDataGridMergeExtensionCol(dgPalletInfo, sColumnName, DataGridMergeMode.VERTICAL);

                    dgPalletInfo.GroupBy(dgPalletInfo.Columns["TYPE"], DataGridSortDirection.None);
                    dgPalletInfo.GroupRowPosition = DataGridGroupRowPosition.AboveData;

                    //DataGridAggregate.SetAggregateFunctions(dgPalletInfo.Columns["TYPE"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("Total") } });
                    //DataGridAggregate.SetAggregateFunctions(dgPalletInfo.Columns["PROCNAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });

                    //DataGridAggregate.SetAggregateFunctions(dgPalletInfo.Columns["LINE_STOCK"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    //DataGridAggregate.SetAggregateFunctions(dgPalletInfo.Columns["PORTOUT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    //DataGridAggregate.SetAggregateFunctions(dgPalletInfo.Columns["OVERTAKE_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    //DataGridAggregate.SetAggregateFunctions(dgPalletInfo.Columns["NODATA_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    //DataGridAggregate.SetAggregateFunctions(dgPalletInfo.Columns["NOINPUT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });

                    //DataGridAggregate.SetAggregateFunctions(dgPalletInfo.Columns["EM_JUDG_RSLT_NAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    //DataGridAggregate.SetAggregateFunctions(dgPalletInfo.Columns["PERIOD_1"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    //DataGridAggregate.SetAggregateFunctions(dgPalletInfo.Columns["PERIOD_2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    //DataGridAggregate.SetAggregateFunctions(dgPalletInfo.Columns["PERIOD_3"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    //DataGridAggregate.SetAggregateFunctions(dgPalletInfo.Columns["PERIOD_4"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    //DataGridAggregate.SetAggregateFunctions(dgPalletInfo.Columns["PERIOD_5"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    //DataGridAggregate.SetAggregateFunctions(dgPalletInfo.Columns["TOTAL"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                }

            });
        }

        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            Util.gridClear(dgPalletInfo);
            //SetLocation_Cmb();
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            new LGC.GMES.MES.Common.ExcelExporter().Export(dgPalletInfo);
        }


        private void CellMaerge(object sender, DataGridMergingCellsEventArgs e)
        {
            if (dgPalletInfo.Rows.Count == 2) return;

            DataGridCellsRange currentRange = null;
            for (int i = 2; i < 8; i++)
            {
                C1.WPF.DataGrid.DataGridCell currentCell = dgPalletInfo.GetCell(5, i);
                C1.WPF.DataGrid.DataGridCell nextCell = dgPalletInfo.GetCell(7, i);
                currentRange = new DataGridCellsRange(currentCell, nextCell);
                e.Merge(currentRange);

            }

        }
    }
}
