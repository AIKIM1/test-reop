/*************************************************************************************
 Created Date : 2018.11.30
      Creator : CNS 고현영
   Decription : 믹서이물관리 트렌드
--------------------------------------------------------------------------------------
 [Change History]
  2018.11.30  고현영 : Initial Created.
**************************************************************************************/

using C1.WPF.C1Chart;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using C1.WPF;
using C1.WPF.C1Chart.Extended;
using System.Linq;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Data;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_255_IMPURITY_TREND : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        decimal hopperUSL = 0;
        decimal hopperUCL = 0;
        DataTable hopperDt = new DataTable();
        DataTable hopperGrp = new DataTable();
        decimal slurryUSL = 0;
        decimal slurryUCL = 0;
        DataTable slurryDt = new DataTable();
        DataTable slurryGrp = new DataTable();

        public COM001_255_IMPURITY_TREND()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length == 6)
            {
                hopperUSL = Util.NVC_Decimal(tmps[0]);
                hopperUCL = Util.NVC_Decimal(tmps[1]);
                hopperDt = tmps[2] == null ? new DataTable() : (DataTable)tmps[2];
                slurryUSL = Util.NVC_Decimal(tmps[3]);
                slurryUCL = Util.NVC_Decimal(tmps[4]);
                slurryDt = tmps[5] == null ? new DataTable() : (DataTable)tmps[5];
            }

            List<Button> listAuth = new List<Button> { };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            SetHopperData();
            SetSlurryData();

            ModifyHopperGraph();
            ModifySlurryGraph();

            if (hopperGrp.Rows.Count <= 0 || slurryGrp.Rows.Count <= 0)
                return;

            DrawHopperUSL();
            DrawHopperUCL();
            DrawSlurryUSL();
            DrawSlurryUCL();
        }

        private void ModifySlurryGraph()
        {
            chrSlurry.View.AxisX.IsTime = true;
            chrSlurry.View.AxisX.AnnoFormat = "yyyy-MM-dd";
            //chrSlurry.View.AxisX.Title = ObjectDic.Instance.GetObjectName("등록일자");
            
            chrSlurry.View.AxisX.FontWeight = FontWeights.Bold;
            chrSlurry.View.AxisX.FontSize = 12;
            chrSlurry.View.AxisX.AnnoPosition = AnnoPosition.Near;
            chrSlurry.View.AxisX.Foreground = new SolidColorBrush(Colors.Black);
            chrSlurry.View.AxisX.MajorGridStroke = new SolidColorBrush(Colors.Gray);
            chrSlurry.View.AxisX.MinorTickThickness = 0;
            chrSlurry.View.AxisX.MinorGridStroke = new SolidColorBrush(Colors.Transparent);

            //chrSlurry.View.AxisY.Title = ObjectDic.Instance.GetObjectName("PPM");
            chrSlurry.View.AxisY.FontWeight = FontWeights.Bold;
            chrSlurry.View.AxisY.FontSize = 12;
            chrSlurry.View.AxisY.Max = 1;
            chrSlurry.View.AxisY.Min = 0.00000001;
            chrSlurry.View.AxisY.LogBase = 10;
            chrSlurry.View.AxisY.AnnoFormat = "#0.#######";
            chrSlurry.View.AxisY.Foreground = new SolidColorBrush(Colors.Black);
            chrSlurry.View.AxisY.MinorTickThickness = 0;
            chrSlurry.View.AxisY.MinorGridStroke = new SolidColorBrush(Colors.Transparent);
        }

        private void ModifyHopperGraph()
        {
            chrHopper.View.AxisX.IsTime = true;
            chrHopper.View.AxisX.AnnoFormat = "yyyy-MM-dd";
            //chrHopper.View.AxisX.Title = ObjectDic.Instance.GetObjectName("등록일자");
            chrHopper.View.AxisX.FontWeight = FontWeights.Bold;
            chrHopper.View.AxisX.FontSize = 12;
            chrHopper.View.AxisX.AnnoPosition = AnnoPosition.Near;
            chrHopper.View.AxisX.Foreground = new SolidColorBrush(Colors.Black);
            chrHopper.View.AxisX.MajorGridStroke = new SolidColorBrush(Colors.Gray);
            chrHopper.View.AxisX.MinorTickThickness = 0;
            chrHopper.View.AxisX.MinorGridStroke = new SolidColorBrush(Colors.Transparent);

            //chrHopper.View.AxisY.Title = ObjectDic.Instance.GetObjectName("PPM");
            chrHopper.View.AxisY.FontWeight = FontWeights.Bold;
            chrHopper.View.AxisY.FontSize = 12;
            chrHopper.View.AxisY.Max = 1;
            chrHopper.View.AxisY.Min = 0.00000001;
            chrHopper.View.AxisY.LogBase = 10;
            chrHopper.View.AxisY.AnnoFormat = "#0.#######";
            chrHopper.View.AxisY.Foreground = new SolidColorBrush(Colors.Black);
            chrHopper.View.AxisY.MinorTickThickness = 0;
            chrHopper.View.AxisY.MinorGridStroke = new SolidColorBrush(Colors.Transparent);
        }

        public void SetHopperData()
        {
            DataTable hopperGrouped = hopperDt.AsEnumerable().GroupBy(g => new
            {
                CLCT_DTTM = g.Field<DateTime>("CLCT_DTTM"),
                TOTAL_PPM_QTY = g.Field<decimal>("TOTAL_PPM_QTY")
            }).Select(s => new
            {
                CLCT_DTTM = s.Key.CLCT_DTTM,
                TOTAL_PPM_QTY = s.Key.TOTAL_PPM_QTY
            }).ToList().ToDataTable();

            hopperGrp = hopperGrouped;

            chrHopper.Data.ItemsSource = DataTableConverter.Convert(hopperGrp);
        }

        public void SetSlurryData()
        {
            DataTable slurryGrouped = slurryDt.AsEnumerable().GroupBy(g => new
            {
                CLCT_DTTM = g.Field<DateTime>("CLCT_DTTM"),
                TOTAL_PPM_QTY = g.Field<decimal>("TOTAL_PPM_QTY")
            }).Select(s => new
            {
                CLCT_DTTM = s.Key.CLCT_DTTM,
                TOTAL_PPM_QTY = s.Key.TOTAL_PPM_QTY
            }).ToList().ToDataTable();

            slurryGrp = slurryGrouped;

            chrSlurry.Data.ItemsSource = DataTableConverter.Convert(slurryGrp);
        }

        public void DrawHopperUCL()
        {

            DateTime[] x = new DateTime[2];
            decimal[] y = new decimal[2];

            DateTime minDate = Convert.ToDateTime(hopperGrp.AsEnumerable().OrderBy(o => o.Field<DateTime>("CLCT_DTTM")).First()["CLCT_DTTM"]);
            DateTime maxDate = Convert.ToDateTime(hopperGrp.AsEnumerable().OrderBy(o => o.Field<DateTime>("CLCT_DTTM")).Last()["CLCT_DTTM"]);

            x[0] = minDate.AddDays(-1);
            y[0] = hopperUCL;
            x[1] = maxDate.AddDays(1);
            y[1] = hopperUCL;

            TrendLine tlUCL = new TrendLine()
            {
                XValuesSource = x,
                ValuesSource = y,
                ConnectionFill = new SolidColorBrush(Colors.Blue),
                ConnectionStrokeThickness = 2,
                Label = String.Format("UCL({0})", hopperUCL.ToString("#.0000000"))
            };

            chrHopper.Data.Children.Add(tlUCL);
        }

        public void DrawHopperUSL()
        {
            DateTime[] x = new DateTime[2];
            decimal[] y = new decimal[2];

            DateTime minDate = Convert.ToDateTime(hopperGrp.AsEnumerable().OrderBy(o => o.Field<DateTime>("CLCT_DTTM")).First()["CLCT_DTTM"]);
            DateTime maxDate = Convert.ToDateTime(hopperGrp.AsEnumerable().OrderBy(o => o.Field<DateTime>("CLCT_DTTM")).Last()["CLCT_DTTM"]);

            x[0] = minDate.AddDays(-1);
            y[0] = hopperUSL;
            x[1] = maxDate.AddDays(1);
            y[1] = hopperUSL;

            TrendLine tlUSL = new TrendLine()
            {
                XValuesSource = x,
                ValuesSource = y,
                ConnectionFill = new SolidColorBrush(Colors.Red),
                ConnectionStrokeThickness = 2,
                Label = String.Format("USL({0})", hopperUSL.ToString("#.0000000"))
            };

            chrHopper.Data.Children.Add(tlUSL);
        }

        public void DrawSlurryUCL()
        {

            DateTime[] x = new DateTime[2];
            decimal[] y = new decimal[2];

            DateTime minDate = Convert.ToDateTime(slurryGrp.AsEnumerable().OrderBy(o => o.Field<DateTime>("CLCT_DTTM")).First()["CLCT_DTTM"]);
            DateTime maxDate = Convert.ToDateTime(slurryGrp.AsEnumerable().OrderBy(o => o.Field<DateTime>("CLCT_DTTM")).Last()["CLCT_DTTM"]);

            x[0] = minDate.AddDays(-1);
            y[0] = slurryUCL;
            x[1] = maxDate.AddDays(1);
            y[1] = slurryUCL;

            TrendLine tlUCL = new TrendLine()
            {
                XValuesSource = x,
                ValuesSource = y,
                ConnectionFill = new SolidColorBrush(Colors.Blue),
                ConnectionStrokeThickness = 2,
                Label = String.Format("UCL({0})", slurryUCL.ToString("#.0000000"))
            };

            chrSlurry.Data.Children.Add(tlUCL);

           
        }

        public void DrawSlurryUSL()
        {
            DateTime[] x = new DateTime[2];
            decimal[] y = new decimal[2];

            DateTime minDate = Convert.ToDateTime(slurryGrp.AsEnumerable().OrderBy(o => o.Field<DateTime>("CLCT_DTTM")).First()["CLCT_DTTM"]);
            DateTime maxDate = Convert.ToDateTime(slurryGrp.AsEnumerable().OrderBy(o => o.Field<DateTime>("CLCT_DTTM")).Last()["CLCT_DTTM"]);

            x[0] = minDate.AddDays(-1);
            y[0] = slurryUSL;
            x[1] = maxDate.AddDays(1);
            y[1] = slurryUSL;

            TrendLine tlUSL = new TrendLine()
            {
                XValuesSource = x,
                ValuesSource = y,
                ConnectionFill = new SolidColorBrush(Colors.Red),
                ConnectionStrokeThickness = 2,
                Label = String.Format("USL({0})", slurryUSL.ToString("#.0000000")),
                
            };

            chrSlurry.Data.Children.Add(tlUSL);
        }

        #endregion
    }
}
