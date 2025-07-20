/*************************************************************************************
 Created Date : 2022.12.08
      Creator : KIM TAEKYUN
   Decription : Aging 점유율 Report
--------------------------------------------------------------------------------------
 [Change History]
------------------------------------------------------------------------------
     Date     |   NAME   |                  DESCRIPTION
------------------------------------------------------------------------------
  2022.12.08  DEVELOPER : Initial Created.
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
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.C1Chart;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_218 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]

        private DateTime _dtFromDate = DateTime.Now.AddDays(-1);
        private DateTime _dtFromTime = DateTime.Now;
        private DateTime _dtToDate = DateTime.Now;
        private DateTime _dtToTime = DateTime.Now;

        private string[] sDateArray;
        private string[] sDataArray;

        public FCS002_218()
        {
            InitializeComponent();
        }
        #endregion

        #region [Initialize]

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            MakeDateArray();
        }

        private void InitCombo()
        {
            
        }
        #endregion

        #region [Event]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //MakeDateArray();

            chrTrend.Data.Children.Clear();

            GetList();  //상단 Aging 점유율 현황
            GetListTrend(); //하단 Aging 점유율 Trend
            GetListChart(); //Chart
        }

        private void dgList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private void dgListTrend_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;

                dtRqst.Rows.Add(dr);
                ShowLoadingIndicator();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_AGING_RACK_REPORT_MB", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    Util.GridSetData(dgList, dtRslt, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetListTrend()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;

                dtRqst.Rows.Add(dr);
                ShowLoadingIndicator();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_AGING_RACK_BEFORE_REPORT_MB", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    Util.GridSetData(dgListTrend, dtRslt, FrameOperation, true);
                }

                DataTable dtTest = DataTableConverter.Convert(dgListTrend.ItemsSource);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetListChart()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;

                dtRqst.Rows.Add(dr);
                ShowLoadingIndicator();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_AGING_RACK_BEFORE_REPORT_CHART_MB", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {

                    sDataArray = new string[dtRslt.Columns.Count-1];
                    int idx = 0;
                    for (int i = 1; i < dtRslt.Columns.Count; i++)
                    {
                        if (idx == 0)
                        {
                            sDataArray[idx] = dtRslt.Columns[i].ToString();
                            idx++;
                        }

                        else if (sDataArray[idx-1] != dtRslt.Columns[i].ToString())
                        {
                            sDataArray[idx] = dtRslt.Columns[i].ToString();
                            idx++;
                        }
                    }
                    
                    SetSeries();
                    chrTrend.Data.ItemsSource = DataTableConverter.Convert(dtRslt);
                    ModifyGraph();
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void SetSeries()
        {

            for (int i = 0; i < sDataArray.Length; i++)
            {
                C1.WPF.C1Chart.XYDataSeries x = new XYDataSeries();
                x.Label = sDataArray[i];
                x.RenderMode = RenderMode.Default;
                x.ValueBinding = new Binding(sDataArray[i]);

                chrTrend.Data.Children.Add(x);
            }
            
        }

        private void ModifyGraph()
        {
            chrTrend.View.AxisX.IsTime = true;
            chrTrend.View.AxisX.AnnoFormat = "yyyyMMdd";
            chrTrend.View.AxisX.AnnoAngle = -90;
            chrTrend.View.AxisY.AnnoFormat = "#0.##";
        }

        private void MakeDateArray()
        {
            string sDay = string.Empty;
            DateTime dToDay = DateTime.Today;

            sDateArray = new string[10];
            int j = 0;

            for (int i = -1; i > -11; i--)
            {
                //sDay = dToDay.AddDays(i).ToString("yyyy'/'MM'/'dd");
                sDay = dToDay.AddDays(i).ToString("yyyyMMdd");
                sDateArray[j] = sDay;

                j++;
            }

            //그리드 컬럼 추가
            for (int i = 0; i < sDateArray.Length; i++)
            {
                //dgListTrend.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                //{
                //    Name = sDateArray[i],
                //    Header = sDateArray[i],
                //    Binding = new Binding()
                //    {
                //        Path = new PropertyPath(sDateArray[i]),
                //        Mode = BindingMode.TwoWay
                //    },
                //    TextWrapping = TextWrapping.NoWrap,
                //    IsReadOnly = true,
                //    Format = "#,##0"
                //});

                dgListTrend.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = sDateArray[i],
                    Header = "[*]" + sDateArray[i],
                    Binding = new Binding() { Path = new PropertyPath(sDateArray[i]), Mode = BindingMode.TwoWay },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.NoWrap,
                });
            }
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
