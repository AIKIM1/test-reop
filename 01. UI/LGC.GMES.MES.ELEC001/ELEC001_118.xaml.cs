using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Navigation;
using System.Data;
using LGC.GMES.MES.CMM001.Extensions;





namespace LGC.GMES.MES.ELEC001
{
    /// <summary>
    /// ELEC001_118.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ELEC001_118 : UserControl
    {
        public ELEC001_118()
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
            cboAreaSelect();
            SetcboEquipment();
            InitCombo();
            dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
        }
        //화면내 combo 셋팅
        private void InitCombo()
        {

            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //동
            //C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild);

            //라인
            //C1ComboBox[] cboLineChild = { cboElecType };
            //C1ComboBox[] cboLineParent = { cboArea };
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent);

            String[] sFilter1 = { "ELTR_TYPE_CODE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

        }

        private void cboAreaSelect()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["USERID"] = LoginInfo.USERID;
                dr["SYSTEM_ID"] = LoginInfo.SYSID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AUTH_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboArea.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex) { }

        }


        private void cboArea_selectionChanged(object sender, EventArgs e)
        {
            SetcboEquipment();
        }

        private void SetcboEquipment()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedItemsToString;
                dr["EQSGID"] = cboEquipmentSegment.SelectedItemsToString;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipmentSegment.ItemsSource = DataTableConverter.Convert(dtResult);

            }
            catch (Exception ex)
            {

            }
        }


        private void cboEquipmentSegment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetcboEquipment();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;


                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }

            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            {                
                Util.MessageValidation("SFU2042", "31");
                return;
            }

            if (String.IsNullOrWhiteSpace(cboArea.SelectedItemsToString))
            {
                Util.MessageValidation("SFU1499");
                return;
            }

            if (String.IsNullOrWhiteSpace(cboEquipmentSegment.SelectedItemsToString))
            {
                Util.MessageValidation("SFU1223");
                return;
            }

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("SHOPID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("EQSGID", typeof(string));
            IndataTable.Columns.Add("FROMDATE", typeof(string));
            IndataTable.Columns.Add("TODATE", typeof(string));
            IndataTable.Columns.Add("PRODID", typeof(string));
            IndataTable.Columns.Add("PRJT_NAME", typeof(string));
            IndataTable.Columns.Add("ELEC_TYPE", typeof(string));

            DataRow dr = IndataTable.NewRow();

            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["AREAID"] = cboArea.SelectedItemsToString;
            dr["EQSGID"] = cboEquipmentSegment.SelectedItemsToString;
            dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
            dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd");
            dr["PRODID"] = string.IsNullOrWhiteSpace(txtProdId.Text.Trim()) ? null : txtProdId.Text.Trim();
            dr["PRJT_NAME"] = string.IsNullOrWhiteSpace(txtPrjtName.Text.Trim()) ? null : txtPrjtName.Text.Trim();
            if (cboElecType.SelectedValue.Equals("ALL") || !String.IsNullOrEmpty(cboElecType.SelectedValue.ToString()))
            {
                dr["ELEC_TYPE"] = cboElecType.SelectedValue;
            }

            IndataTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_SUMMARY_BY_ASSY_PLAN", "RQSTDT", "RSLTDT", IndataTable);

            Util.GridSetData(dgMoPan, dtResult, FrameOperation, true);

            DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
            DataGridAggregatesCollection daq = new DataGridAggregatesCollection();
            DataGridAggregatesCollection dat = new DataGridAggregatesCollection();
            DataGridAggregateSum dagsum = new DataGridAggregateSum();
            DataGridAggregateCount dgcount = new DataGridAggregateCount();
            DataGridAggregateDistinct dgdiscount = new DataGridAggregateDistinct();
            dagsum.ResultTemplate = dgMoPan.Resources["ResultTemplate"] as DataTemplate;
            dac.Add(dagsum);
            daq.Add(dgcount);
            dat.Add(dgdiscount);

            DataGridAggregate.SetAggregateFunctions(dgMoPan.Columns["WIPQTY"], dac);
            DataGridAggregate.SetAggregateFunctions(dgMoPan.Columns["ROLLQTY"], dac);
            DataGridAggregate.SetAggregateFunctions(dgMoPan.Columns["PLANQTY_M"], dac);
            DataGridAggregate.SetAggregateFunctions(dgMoPan.Columns["TOTQTY_M"], dac);
            DataGridAggregate.SetAggregateFunctions(dgMoPan.Columns["MOVEQTY_M"], dac);
            DataGridAggregate.SetAggregateFunctions(dgMoPan.Columns["RETURNQTY_M"], dac);
            DataGridAggregate.SetAggregateFunctions(dgMoPan.Columns["GAPQTY_M"], dac);
            DataGridAggregate.SetAggregateFunctions(dgMoPan.Columns["PLANQTY_R"], dac);
            DataGridAggregate.SetAggregateFunctions(dgMoPan.Columns["TOTQTY_R"], dac);
            DataGridAggregate.SetAggregateFunctions(dgMoPan.Columns["MOVEQTY_R"], dac);
            DataGridAggregate.SetAggregateFunctions(dgMoPan.Columns["RETURNQTY_R"], dac);
            DataGridAggregate.SetAggregateFunctions(dgMoPan.Columns["GAPQTY_R"], dac);
            DataGridAggregate.SetAggregateFunctions(dgMoPan.Columns["PLANQTY_C"], dac);
            DataGridAggregate.SetAggregateFunctions(dgMoPan.Columns["TOTQTY_C"], dac);
            DataGridAggregate.SetAggregateFunctions(dgMoPan.Columns["MOVEQTY_C"], dac);
            DataGridAggregate.SetAggregateFunctions(dgMoPan.Columns["RETURNQTY_C"], dac);
            DataGridAggregate.SetAggregateFunctions(dgMoPan.Columns["GAPQTY_C"], dac);



        }

    }
}
