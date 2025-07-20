/*************************************************************************************
 Created Date : 2021.04.26
      Creator : 신광희
   Decription : 자동차 전극 Roll Map - C1 Chart BackGround 컬러 적용 클래스
--------------------------------------------------------------------------------------
 [Change History]
  2021.04.26  신광희 : Initial Created.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C1.WPF.C1Chart;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001.Class
{
    public class AlarmZone : XYDataSeries
    {
        #region "constructor"

        public AlarmZone()
        {
            //this.LayoutUpdated += new EventHandler(AlarmZone_LayoutUpdated);
            this.ChartType = C1.WPF.C1Chart.ChartType.PolygonFilled;
            this.XValues = new DoubleCollection();
            this.Values = new DoubleCollection();
            //UpdateLegend();
            Update();
        }

        #endregion

        #region "members"

        private C1Chart _chart;
        public C1Chart Chart
        {
            get { return _chart; }
            set { _chart = value; }
        }

        private double? _lowerExtent = null;
        public double? LowerExtent
        {
            get { return _lowerExtent; }
            set
            {
                _lowerExtent = value;
                Update();
            }
        }

        private double? _upperExtent = null;
        public double? UpperExtent
        {
            get { return _upperExtent; }
            set
            {
                _upperExtent = value;
                Update();
            }
        }

        private double? _near = null;
        public double? Near
        {
            get { return _near; }
            set
            {
                _near = value;
                Update();
            }
        }

        private double? _far = null;
        public double? Far
        {
            get { return _far; }
            set
            {
                _far = value;
                Update();
            }
        }

        private bool _showInLegend = false;
        public bool ShowInLegend
        {
            get { return _showInLegend; }
            set
            {
                _showInLegend = value;
                UpdateLegend();
            }
        }

        #endregion

        #region "implementation"

        public void Update()
        {
            if (_near != null && _far != null)
            {
                this.XValues.Clear();
                this.XValues.Add((double)_near);
                this.XValues.Add((double)_far);
                this.XValues.Add((double)_far);
                this.XValues.Add((double)_near);
            }


            if (_lowerExtent != null && _upperExtent != null)
            {
                this.Values.Clear();
                this.Values.Add((double)_lowerExtent);
                this.Values.Add((double)_lowerExtent);
                this.Values.Add((double)_upperExtent);
                this.Values.Add((double)_upperExtent);
            }
        }

        public void UpdateLegend()
        {
            if (_showInLegend)
            {
                this.Display = SeriesDisplay.SkipNaN;
            }
            else
            {
                this.Display = SeriesDisplay.HideLegend;
            }
        }

        private void chart_LayoutUpdated(object sender, EventArgs e)
        {
            if (Chart?.View != null)
            {
                // if extent is null, set to axis bounds
                if (_near == null)
                {
                    _near = Chart.View.AxisX.ActualMin;
                    Update();
                }
                if (_far == null)
                {
                    _far = Chart.View.AxisX.ActualMax;
                    Update();
                }
                if (_upperExtent == null)
                {
                    _upperExtent = Chart.View.AxisY.ActualMax;
                    Update();
                }
                if (_lowerExtent == null)
                {
                    _lowerExtent = Chart.View.AxisY.ActualMin;
                    Update();
                }

            }
        }

        // obtains the parent chart control so we can later get axis bounds
        void AlarmZone_LayoutUpdated(object sender, EventArgs e)
        {
            if (this.Parent != null && this.Chart == null)
            {
                Canvas c = this.Parent as Canvas;
                Canvas cv = c?.Parent as Canvas;
                Border b = cv?.Parent as Border;
                Grid g = b?.Parent as Grid;
                C1Chart chart = g?.Parent as C1Chart;
                if (chart != null)
                {
                    this.Chart = chart;
                    this.Chart.LayoutUpdated += chart_LayoutUpdated;
                }

            }
        }

        #endregion

    }
}
