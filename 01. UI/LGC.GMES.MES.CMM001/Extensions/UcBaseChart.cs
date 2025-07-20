/*************************************************************************************
 Created Date : 2023.12.11
      Creator : 
   Decription : C1Chart Extension
--------------------------------------------------------------------------------------
 [Change History]
  2023.12.11  조영대 : Initial Created. 
**************************************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.C1Chart;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.CMM001.Controls
{
    public class UcBaseChart : C1Chart
    {
        #region EventHandler

        #endregion

        #region Property

        private bool isHighlightUse = true;
        [Category("GMES"), DefaultValue(true), Description("Highlight 사용 유무")]
        public bool IsHighlightUse
        {
            get { return isHighlightUse; }
            set { isHighlightUse = value; }
        }

        public IEnumerable ItemsSource
        {
            get { return this.Data.ItemsSource; }
            set 
            {
                this.Data.ItemsSource = value;

                legendControl = null;

                foreach (object child in this.Data.Children)
                {
                    if (child != null && child is C1.WPF.C1Chart.XYDataSeries)
                    {
                        C1.WPF.C1Chart.XYDataSeries series = child as C1.WPF.C1Chart.XYDataSeries;
                        series.MouseEnter -= Series_MouseEnter;
                        series.MouseEnter += Series_MouseEnter;

                        series.MouseLeave -= Series_MouseLeave;
                        series.MouseLeave += Series_MouseLeave;
                    }
                }
            }
        }

        #endregion

        #region Declaration & Constructor 
        private System.Timers.Timer highlightDelayTimer = null;
        private System.Timers.Timer highlightTermTimer = null;        
        private bool highlightOn = false;
        private C1ChartLegend legendControl = null;
        private C1.WPF.C1Chart.XYDataSeries seriesSelect = null;

        public UcBaseChart()
        {            
        }

        public void InitializeControls()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            this.Palette = ColorGeneration.Concourse;
            
            if (isHighlightUse && highlightTermTimer == null)
            {
                highlightDelayTimer = new System.Timers.Timer(300);
                highlightDelayTimer.Elapsed += HighlightDelayTimer_Elapsed;
                highlightDelayTimer.Stop();

                highlightTermTimer = new System.Timers.Timer(1500);
                highlightTermTimer.Elapsed += HighlightTermTimer_Elapsed;
                highlightTermTimer.Stop();
            }
        }
        #endregion

        #region Override

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            InitializeControls();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            try
            {
                if (isHighlightUse)
                {
                    legendControl = this.FindChild<C1ChartLegend>("");
                    if (legendControl == null) return;

                    foreach (LegendItem legendItem in this.LegendItems)
                    {
                        TextBlock legendText = this.FindChildTextBlock(legendControl, legendItem.Label);
                        legendText.MouseEnter -= LegendText_MouseEnter;
                        legendText.MouseEnter += LegendText_MouseEnter;
                        legendText.MouseLeave -= LegendText_MouseLeave;
                        legendText.MouseLeave += LegendText_MouseLeave;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event

        private void HighlightDelayTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            highlightDelayTimer.Stop();

            if (!isHighlightUse) return;

            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                if (!highlightOn) SetHighlightOn();
            }));
        }

        private void HighlightTermTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            highlightTermTimer.Stop();

            if (!isHighlightUse) return;

            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                if (highlightOn) SetHighlightOff();
            }));
        }

        private void Series_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!highlightOn)
            {
                seriesSelect = sender as C1.WPF.C1Chart.XYDataSeries;
                if (seriesSelect == null) return;

                highlightDelayTimer.Start();               
            }
            else if (highlightOn && seriesSelect != null && seriesSelect.Equals(sender))
            {
                highlightTermTimer.Stop();
            }
        }

        private void Series_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!highlightOn)
            {
                seriesSelect = null;
                highlightDelayTimer.Stop();
            }
            else if (highlightOn && seriesSelect != null && seriesSelect.Equals(sender))
            {                
                highlightTermTimer.Start();
            }
        }
        
        private void LegendText_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            if (tb == null) return;

            C1.WPF.C1Chart.LegendItem legendItem = tb.DataContext as C1.WPF.C1Chart.LegendItem;
            if (legendItem == null) return;

            if (!highlightOn)
            {
                seriesSelect = legendItem.Item as C1.WPF.C1Chart.XYDataSeries;
                if (seriesSelect == null) return;

                highlightDelayTimer.Start();
            }
            else if (highlightOn && seriesSelect != null && seriesSelect.Equals(legendItem.Item))
            {
                highlightTermTimer.Stop();
            }
            else if (highlightOn && seriesSelect != null && !seriesSelect.Equals(legendItem.Item))
            {
                highlightTermTimer.Stop();

                SetHighlightOff();

                seriesSelect = legendItem.Item as C1.WPF.C1Chart.XYDataSeries;
                if (seriesSelect == null) return;

                SetHighlightOn();
            }
        }

        private void LegendText_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!highlightOn)
            {
                seriesSelect = null;
                highlightDelayTimer.Stop();
            }
            else if (highlightOn && seriesSelect != null)
            {
                highlightTermTimer.Start();
            }
        }

        #endregion

        #region Public Method

        #endregion

        #region Method
        private TextBlock FindChildTextBlock(DependencyObject parent, string text)
        {
            TextBlock foundChild = null;

            try
            {
                if (parent == null) return null;
                if (string.IsNullOrEmpty(text)) return null;

                int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < childrenCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    TextBlock childType = child as TextBlock;
                    if (childType == null)
                    {
                        foundChild = FindChildTextBlock(child, text);

                        if (foundChild != null) break;
                    }
                    else if (childType != null && childType is TextBlock)
                    {
                        if (childType.Text == text)
                        {
                            foundChild = childType;
                            break;
                        }
                    }
                }

                return foundChild;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return foundChild;
        }

        private void SetLegendEvent(bool isOnOff)
        {
            try
            {
                legendControl = this.FindChild<C1ChartLegend>("");
                if (legendControl == null) return;

                foreach (LegendItem legendItem in this.LegendItems)
                {
                    TextBlock legendText = this.FindChildTextBlock(legendControl, legendItem.Label);
                    if (isOnOff)
                    {
                        legendText.MouseEnter += LegendText_MouseEnter;
                        legendText.MouseLeave += LegendText_MouseLeave;
                    }
                    else
                    {
                        legendText.MouseEnter -= LegendText_MouseEnter;
                        legendText.MouseLeave -= LegendText_MouseLeave;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetHighlightOn()
        {
            try
            {
                if (!isHighlightUse) return;
                if (legendControl == null) return;
                if (seriesSelect == null) return;

                SetLegendEvent(false);

                seriesSelect.ChartType = ChartType.LineSymbols;
                seriesSelect.SymbolSize = new Size(8, 8);

                if (seriesSelect.PointTooltipTemplate == null)
                {
                    seriesSelect.PointTooltipTemplate = Application.Current.Resources["UcBaseChartTooltipTemplate"] as DataTemplate;
                }
                this.UpdateLayout();

                foreach (object child in this.Data.Children)
                {
                    if (child != null && child is C1.WPF.C1Chart.XYDataSeries)
                    {
                        C1.WPF.C1Chart.XYDataSeries series = child as C1.WPF.C1Chart.XYDataSeries;

                        if (seriesSelect.Label.Equals(series.Label)) continue;

                        series.Opacity = 0.2;

                        TextBlock legendText = this.FindChildTextBlock(legendControl, series.Label);
                        if (legendText != null) legendText.Opacity = 0.2;
                    }
                }

                SetLegendEvent(true);

                highlightOn = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetHighlightOff()
        {
            try
            {
                if (!isHighlightUse) return;
                if (legendControl == null) return;
                if (seriesSelect == null) return;

                SetLegendEvent(false);

                seriesSelect.ChartType = ChartType.Line;
                legendControl = null;

                //if (seriesSelect.PointTooltipTemplate != null)
                //{
                //    seriesSelect.PointTooltipTemplate
                //}
                this.UpdateLayout();

                foreach (object child in this.Data.Children)
                {
                    if (child != null && child is C1.WPF.C1Chart.XYDataSeries)
                    {
                        C1.WPF.C1Chart.XYDataSeries series = child as C1.WPF.C1Chart.XYDataSeries;

                        if (seriesSelect.Label.Equals(series.Label)) continue;

                        series.Opacity = 1;

                        TextBlock legendText = this.FindChildTextBlock(legendControl, series.Label);
                        if (legendText != null) legendText.Opacity = 1;
                    }
                }

                SetLegendEvent(true);

                highlightOn = false;
                seriesSelect = null;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

    }
}
