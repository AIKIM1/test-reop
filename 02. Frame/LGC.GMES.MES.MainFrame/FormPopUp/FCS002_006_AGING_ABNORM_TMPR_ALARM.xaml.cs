/*************************************************************************************
 Created Date : 2024.11.07
      Creator : 강동희
   Decription : 고온 Aging 온도 이탈 알람
--------------------------------------------------------------------------------------
 [Change History]
  2024.11.01  DEVELOPER :  Initial Created.

  
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Reflection;

namespace LGC.GMES.MES.MainFrame
{
    public partial class FCS002_006_AGING_ABNORM_TMPR_ALARM : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private DataTable _dt = new DataTable();

        public FCS002_006_AGING_ABNORM_TMPR_ALARM()
        {
            InitializeComponent();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _dt = tmps[0] as DataTable;
            }

            dgAbnormTmprList.ItemsSource = DataTableConverter.Convert(_dt);
        }
        #endregion

        #region Mehod
        public DataTable GetDataTableFromObjects(object[] objects)
        {
            if (objects != null && objects.Length > 0)
            {
                Type t = objects[0].GetType();
                DataTable dt = new DataTable(t.Name);

                foreach (PropertyInfo pi in t.GetProperties())
                {
                    dt.Columns.Add(new DataColumn(pi.Name));
                }

                foreach (var o in objects)
                {
                    DataRow dr = dt.NewRow();
                    foreach (DataColumn dc in dt.Columns)
                    {
                        dr[dc.ColumnName] = o.GetType().GetProperty(dc.ColumnName).GetValue(o, null);
                    }
                    dt.Rows.Add(dr);
                }

                return dt;
            }
            return null;
        }
        #endregion

        #region Event
        private void dgAbnormTmprList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null)
                {
                    return;
                }

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Row.Index == 0)
                    {
                        return;
                    }
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        ////////////////////////////////////////////  default 색상 및 Cursor
                        e.Cell.Presenter.Cursor = Cursors.Arrow;

                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontSize = 12;
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                        ///////////////////////////////////////////////////////////////////////////////////

                        e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    }
                }));
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgAbnormTmprList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index + 1 - dataGrid.TopRows.Count > 0)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        #endregion
    }
}
