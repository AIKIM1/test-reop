#define SAMPLE_DEV
//#undef SAMPLE_DEV

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ELEC_SAMPLING_OQC_RP.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_SAMPLING_OQC_RP : C1Window, IWorkArea
    {
        #region Initialize
        public IFrameOperation FrameOperation { get; set; }

        public CMM_ELEC_SAMPLING_OQC_RP()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            SetActSamplingData();
        }

        private void btnApply_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        private void dgLotInfo_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (string.Equals(e.Cell.Column.Name, "JUDG_NAME") || string.Equals(e.Cell.Column.Name, "SHIP_FLAG"))
                            {
                                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "JUDG_FLAG"))))
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                                else if (string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "JUDG_FLAG"), "F"))
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.OrangeRed);
                                else if (string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "JUDG_FLAG"), "Y"))
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.SkyBlue);
                            }
                        }
                    }
                }));
            }
        }
        #endregion
        #region User Method
        private void SetActSamplingData()
        {
            try
            {
                ShowLoadingIndicator();
                string BizRule = string.Empty;
#if SAMPLE_DEV
                BizRule = "DA_PRD_SEL_LOT_SAMPLE_QA";
#else
                BizRule = "DA_PRD_SEL_LOT_SAMPLE_CNA_QA";
#endif

                DataTable IndataTable = new DataTable("INDATA");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("JUDGFLAG", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                Indata["PROCID"] = Process.ROLL_PRESSING;
                Indata["JUDGFLAG"] = "Y";
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService(BizRule, "INDATA", "RSLTDT", IndataTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                            throw searchException;

                        Util.GridSetData(dgLotInfo, result, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex) { Util.MessageException(ex); }
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
#endregion
    }
}
