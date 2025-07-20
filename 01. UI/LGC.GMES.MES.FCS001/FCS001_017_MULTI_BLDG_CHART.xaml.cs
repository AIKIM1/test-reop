/*************************************************************************************
 Created Date : 2024.02.27
      Creator : 서동현 책임
   Decription : 공Tray 현황(차트)
--------------------------------------------------------------------------------------
 [Change History]
  2024.02.27  서동현 : Initial Created.
**************************************************************************************/
using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Linq;
using LGC.GMES.MES.CMM001.Extensions;
using System.Collections.Generic;
using C1.WPF.DataGrid;
using C1.WPF.C1Chart;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// FCS001_017_CHART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_017_MULTI_BLDG_CHART : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        public static string[] ChartItemNames;

        public bool IsUpdated;
        private string _TrayTypeCode;
        private string _BldgCode;
        private bool IsLoading;

        private readonly Util _util = new Util();

        public FCS001_017_MULTI_BLDG_CHART()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize    
        private void Initialize()
        {
            ApplyPermissions();
            InitializeControls();
            InitializeCombo();
        }

        private void InitializeControls()
        {

        }

        private void InitializeCombo()
        {
            SetChartTypeCombo();
            
            CommonCombo_Form _fcombo = new CommonCombo_Form();
            _fcombo.SetCombo(cboTrayType, CommonCombo_Form.ComboStatus.ALL, sCase: "TRAYTYPE"); //콤보셋팅 공통함수 추가 수정 필요

            CommonCombo _combo = new CommonCombo();
            string[] sFilter1 = { "BLDG_CODE", "", LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "COMMCODEATTRS");
        }
        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.IsLoading = true;

                object[] tmps = C1WindowExtension.GetParameters(this);
                _TrayTypeCode = (tmps[1] == null) ? null : tmps[1].ToString();
                _BldgCode = (tmps[1] == null) ? null : tmps[1].ToString();

                Loaded -= C1Window_Loaded;
                Initialize();

                this.IsLoading = false;
                this.DrawChart();
                /*
                if (_dtTransferCancel != null && CommonVerify.HasTableRow(_dtTransferCancel))
                {
                    SelectFirstManualTransferCancelList();
                }
                else if (_PortId != null)
                {
                    this.btnSearch_Click(this.btnSearch, null);
                }
                */
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DrawChart();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboChartType_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                DrawChart();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboTrayType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                DrawChart();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                DrawChart();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod
        private void SetChartTypeCombo()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("CBO_NAME", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            string[] arrChartType = Enum.GetNames(typeof(ChartType));

            int iPieIdx = 0;
            for (int i = 0; i < arrChartType.Length; i++)
            {
                //차트 형식과 데이터가 안맞아서 패스
                if (arrChartType[i] == "HighLowOpenClose" ||
                    arrChartType[i] == "Candle" ||
                    arrChartType[i] == "Bubble" ||
                    arrChartType[i] == "Gantt")
                {
                    continue;
                }

                dt.Rows.Add(new object[] { arrChartType[i], arrChartType[i] });

                if (arrChartType[i] == "Pie3D")
                {
                    iPieIdx = dt.Rows.Count - 1;
                }
            }

            cboChartType.DisplayMemberPath = "CBO_NAME";
            cboChartType.SelectedValuePath = "CBO_CODE";
            //cboChartType.ItemsSource = AddStatus(dtCombo, CommonCombo.ComboStatus.NA, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
            cboChartType.ItemsSource = dt.AsDataView();

            cboChartType.SelectedIndex = iPieIdx;
        }

        private void DrawChart()
        {
            try
            {
                if (this.IsLoading) return;

                const string bizRuleName = "DA_SEL_EMPTY_TRAY_TOTAL_MULTI_BLDG_CHART";

                ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("TRAY_TYPE_CODE", typeof(string));
                inTable.Columns.Add("BLDGCODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TRAY_TYPE_CODE"] = Util.GetCondition(cboTrayType, bAllNull: true);
                dr["BLDGCODE"] = cboArea.SelectedValue.ToString();
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (CommonVerify.HasTableRow(result))
                        {
                            ChartData cData = new ChartData();

                            //string strCharNameS = "Sum";
                            //string strCharName1 = "Run Tray";
                            //string strCharName2 = "Empty Tray";

                            string strCharNameS = ObjectDic.Instance.GetObjectName("합계");
                            string strCharName1 = ObjectDic.Instance.GetObjectName("RUN_TRAY");
                            string strCharName2 = ObjectDic.Instance.GetObjectName("EMPTY_TRAY");

                            string[] names;
                            double[] valuesS;
                            double[] values1;
                            double[] values2;

                            names = new string[result.Rows.Count];
                            valuesS = new double[result.Rows.Count];
                            values1 = new double[result.Rows.Count];
                            values2 = new double[result.Rows.Count];

                            for (int i = 0; i < result.Rows.Count; i++)
                            {
                                names[i] = result.Rows[i]["CTYPE"].ToString();
                                valuesS[i] = Convert.ToDouble(result.Rows[i]["CNTS"]);
                                values1[i] = Convert.ToDouble(result.Rows[i]["CNT1"]);
                                values2[i] = Convert.ToDouble(result.Rows[i]["CNT2"]);
                            }

                            //names = new string[] { "A", "N", "S" };
                            //values = new double[] { 2, 8, 24 };

                            FCS001_017_MULTI_BLDG_CHART.ChartItemNames = names;

                            DataSeries dsS = new DataSeries() { Label = strCharNameS };
                            dsS.ValuesSource = valuesS;
                            cData.Children.Add(dsS);
                            cData.ItemNames = names;

                            DataSeries ds1 = new DataSeries() { Label = strCharName1 };
                            ds1.ValuesSource = values1;
                            cData.Children.Add(ds1);
                            cData.ItemNames = names;

                            DataSeries ds2 = new DataSeries() { Label = strCharName2 };
                            ds2.ValuesSource = values2;
                            cData.Children.Add(ds2);
                            cData.ItemNames = names;

                            #region 차트바인딩
                            ChartType ct = (ChartType)Enum.Parse(typeof(ChartType), cboChartType.SelectedValue.ToString());
                            chart.ChartType = ct;
                            string stylename = "sChartStyle";

                            if (ct == ChartType.Ribbon || ct.ToString().Contains("3D"))
                            {
                                stylename = "sChartStyle3d";
                                chart.Actions.Add(new Rotate3DAction());
                            }

                            //if (ct == ChartType.Ribbon || ct.ToString().Contains("3D"))
                            //{
                            //    stylename = "sstyle3d";
                            //    chart.Actions.Add(new Rotate3DAction());
                            //}

                            //chart.BeginUpdate();

                            //// use appropriate sample data
                            //if (ct == ChartType.HighLowOpenClose || ct == ChartType.Candle)
                            //    chart.Data = data.FinancialData;
                            //else if (ct == ChartType.Bubble)
                            //    chart.Data = data.BubbleData;
                            //else if (ct == ChartType.Gantt)
                            //    chart.Data = data.GanttData;
                            //else
                            //    chart.Data = data.DefaultData;

                            //// set style of plot elements
                            //foreach (DataSeries ds in chart.Data.Children)
                            //{
                            //    ds.SymbolStyle = FindResource(stylename) as Style;
                            //    ds.ConnectionStyle = FindResource(stylename) as Style;
                            //    ds.PointTooltipTemplate = FindResource("lbl") as DataTemplate;
                            //}

                            //chart.EndUpdate();

                            chart.BeginUpdate();
                            chart.Data = cData;

                            // set style of plot elements

                            foreach (DataSeries ds in chart.Data.Children)
                            {
                                //ds.SymbolStyle = FindResource(stylename) as Style;
                                //ds.ConnectionStyle = FindResource(stylename) as Style;
                                //ds.PointTooltipTemplate = FindResource("lbl") as DataTemplate;
                                //ds.PointTooltipTemplate = FindResource("lblChartTip") as DataTemplate;
                                //ds.PointLabelTemplate = FindResource("lblChartLabel") as DataTemplate;

                                ds.SymbolStyle = chart.Resources[stylename] as Style;
                                ds.ConnectionStyle = chart.Resources[stylename] as Style;
                                ds.PointTooltipTemplate = chart.Resources["lblChartTip"] as DataTemplate;
                                ds.PointLabelTemplate = chart.Resources["lblChartLabel"] as DataTemplate;
                            }

                            //chart.DataContext = new double[] { 1, 2, 3 };
                            //chart.Loaded += Chart_Loaded;
                            //chart.Loaded += (s, e) => ((BasePieRenderer)chart.da.Ren)

                            //DataTemplate dt = new DataTemplate();
                            //dt.

                            //ChartLegend cl = new ChartLegend();
                            //C1ChartLegend l = new C1ChartLegend();
                            //LegendItem li = new LegendItem.

                            //chart.LegendItems =

                            chart.EndUpdate();
                            #endregion
                        }
                        else
                        {
                            chart.BeginUpdate();
                            chart.Data.Children.Clear();
                            chart.EndUpdate();
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button> { btnSearch };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

    public class IndexStringConverterChartItemName : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DataPoint dp = value as DataPoint;

            if (dp != null && FCS001_017_MULTI_BLDG_CHART.ChartItemNames != null)
                return FCS001_017_MULTI_BLDG_CHART.ChartItemNames[dp.PointIndex];
            //return dp["SizeValues"].ToString() + " medals";

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IndexStringConverterChartLabel : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DataPoint dp = value as DataPoint;

            if (dp != null && FCS001_017_MULTI_BLDG_CHART.ChartItemNames != null)
                return FCS001_017_MULTI_BLDG_CHART.ChartItemNames[dp.PointIndex].Split('[')[0] + " : " + dp.Value.ToString("#,##0") + " (" + (dp.PercentageSeries * 100).ToString("#,##0.00") + " %)";
            //return MCS001_036.ChartItemNames[dp.PointIndex].Split('[')[0] + ":" + dp.Value.ToString("#,##0") + "(" + dp.PercentageSeries.ToString("#,##0.00") + " %)";
            //return dp["SizeValues"].ToString() + " medals";

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
