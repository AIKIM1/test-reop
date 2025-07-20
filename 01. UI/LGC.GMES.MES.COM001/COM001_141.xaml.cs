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


namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_141.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_141 : UserControl, IWorkArea
    {
        string sArea_ID = string.Empty;
        string sLocation_ID = string.Empty;
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public COM001_141()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();

            InitOpenSearch();
        }

        private void InitOpenSearch()
        {

            if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            {
                Array ary = FrameOperation.Parameters;
                string sArea_ID = ary.GetValue(1).ToString();
                string sLocationid = ary.GetValue(0).ToString();
                var cnt = cboArea.ItemsSource.Cast<DataRowView>().Where(x => x["CBO_CODE"].ToString() == sArea_ID).Count();
                if (cnt < 1)
                {
                    cboArea.SelectedIndex = 0;
                }
                else
                {
                    cboArea.SelectedValue = sArea_ID;
                }

                cnt = cboLocation.ItemsSource.Cast<DataRowView>().Where(x => x["LOCATION_ID"].ToString() == sLocationid).Count();
                if (cnt < 1)
                {
                    cboLocation.SelectedIndex = 0;
                }
                else
                {
                    cboLocation.SelectedValue = sLocationid;
                }       
                btnSearch_Click(null, null);
            }
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter_cboArea = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, sFilter: sFilter_cboArea);
        }

        private void SetLocation_Cmb()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string)); 
                //RQSTDT.Columns.Add("LOCATION_ID", typeof(string));
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.ToString() == "" ? null : cboArea.SelectedValue.ToString();
                //dr["LOCATION_ID"] = cboLocation.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_RTLS_SEL_LOCATION_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                DataRow ro = dtResult.NewRow();
                ro["LOCATION_NAME"] = "-ALL-";
                ro["LOCATION_ID"] = "";
                dtResult.Rows.InsertAt(ro, 0);
                cboLocation.ItemsSource = DataTableConverter.Convert(dtResult);
                cboLocation.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgPalletInfo);
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("LOCATION_ID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOCATION_ID"] = cboLocation.SelectedValue.ToString() == "" ? null : cboLocation.SelectedValue.ToString();
            dr["AREAID"] = cboArea.SelectedValue.ToString() == "" ? null : cboArea.SelectedValue.ToString();  


            RQSTDT.Rows.Add(dr);

            loadingIndicator.Visibility = Visibility.Visible;
            new ClientProxy().ExecuteService("DA_RTLS_SEL_LOCATION_LOT_INFO_DETAIL", "RQSTDT", "RSLTDT", RQSTDT, (dt, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;

                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                if (dt.Rows.Count < 1)
                {
                    Util.Alert("SFU1905");
                    return;
                }

                Util.SetTextBlockText_DataGridRowCount(tbListCount, Util.NVC(dt.Rows.Count));

                var mesHold_Cnt = dt.AsEnumerable().Where(X => X["WIPHOLD"].ToString() == "Y").Count();
                var qmsHold_Cnt = dt.AsEnumerable().Where(X => X["QMS_HOLD"].ToString() == "Y").Count();
                var oqcNG_Cnt = dt.AsEnumerable().Where(X => X["OQC_RESULT"].ToString() == "F").Count();
                var Carrier_Cnt = dt.AsEnumerable().Select(X => X["CARRIER_ID"].ToString()).Distinct().Count();
                var Abnormal_Cnt = dt.AsEnumerable().Where(X => X["DELAY_YN"].ToString() == "Y").Count();

                DataRow ro = dt.NewRow();
                ro["LOTID"] = dt.Rows.Count.ToString();
                ro["WIPHOLD"] = mesHold_Cnt.ToString();
                ro["QMS_HOLD"] = qmsHold_Cnt.ToString();
                ro["OQC_RESULT"] = oqcNG_Cnt.ToString();
                ro["CARRIER_ID"] = Carrier_Cnt.ToString();
                ro["DELAY_YN"] = Abnormal_Cnt.ToString();

                dt.Rows.InsertAt(ro,0);

                Util.GridSetData(dgPalletInfo, dt, FrameOperation);
                
                //this.dgPalletInfo.FrozenTopRowsCount = 0;

            });
        }

        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            Util.gridClear(dgPalletInfo);
            SetLocation_Cmb();
        }
       

        private void dgPalletInfo_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dgPalletInfo.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if(e.Cell.Row.Index == 2)
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                }
                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DELAY_YN")) == "Y")
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGoldenrodYellow);
                    e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.LightGoldenrodYellow);
                }

            }));
        }
    }
}
