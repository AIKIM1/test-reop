/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2021.03.07  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.MainFrame
{
    public partial class FCS001_006_WLOTLIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private DataTable dtRslt;

        public DataTable RST_TABLE
        {
            get { return dtRslt; }
            set { dtRslt = value; }
        }

        public FCS001_006_WLOTLIST()
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
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 1)
                {
                    RST_TABLE = tmps[0] as DataTable;
                }
                GetList();
            }
            catch(Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region Mehod
        #region [조회]
        private void GetList()
        {
            try
            {
                //Util.GridSetData(dgList, RST_TABLE, this.FrameOperation, true);
                dgList.ItemsSource = DataTableConverter.Convert(RST_TABLE);
            }
            catch (Exception ex)
            {
                string msg = new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(msg, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //private void GridSetData(C1.WPF.DataGrid.C1DataGrid dataGrid, DataTable dt, IFrameOperation iFO, bool isAutoWidth = false)
        //{
        //    gridClear(dataGrid);

        //    dataGrid.ItemsSource = DataTableConverter.Convert(dt);

        //    foreach (var col in dataGrid.FilteredColumns)
        //    {
        //        foreach (var filter in col.FilterState.FilterInfo)
        //        {
        //            if (filter.FilterType == DataGridFilterType.Text)
        //                filter.Value = string.Empty;
        //        }
        //    }

        //    if (dt.Rows.Count == 0)
        //    {
        //        if (iFO != null)
        //            iFO.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "] " + MessageDic.Instance.GetMessage("SFU2816"));
        //    }
        //    else
        //    {
        //        if (iFO != null)
        //            iFO.PrintFrameMessage(dt.Rows.Count + ObjectDic.Instance.GetObjectName("건"));

        //        if (isAutoWidth && dt.Rows.Count > 0)
        //        {
        //            dataGrid.Loaded -= DataGridLoaded;

        //            double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);
        //            double sumHeight = dataGrid.ActualHeight;
        //            //double sumHeight = dataGrid.ActualHeight == 0 ? dataGrid.MaxHeight : dataGrid.ActualHeight;
        //            //double sumHeight = dataGrid.ActualHeight == 0 ? (dataGrid.Rows.Count * 25) : dataGrid.ActualHeight;

        //            foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
        //                dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

        //            dataGrid.UpdateLayout();
        //            dataGrid.Measure(new Size(sumWidth, sumHeight));

        //            foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
        //                if (dgc.ActualWidth > 0)
        //                    dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.IsReadOnly ? dgc.ActualWidth : (dgc.ActualWidth + 5), DataGridUnitType.Pixel);

        //            dataGrid.Loaded += DataGridLoaded;

        //            /*
        //            dataGrid.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
        //            dataGrid.UpdateLayout();

        //            double gridWidth = dataGrid.Parent.
        //            double sumColumnsWidth = dataGrid.Columns.Sum(x => x.ActualWidth);

        //            if (gridWidth < sumColumnsWidth)
        //            {
        //                double weight = gridWidth / sumColumnsWidth;

        //                foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
        //                    dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.ActualWidth * weight , DataGridUnitType.Pixel);
        //            }
        //            else
        //            { 
        //                dataGrid.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
        //            }
        //            */
        //        }
        //    }
        //}

        //private void DataGridLoaded(object sender, RoutedEventArgs args)
        //{
        //    C1DataGrid dataGrid = sender as C1DataGrid;

        //    double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);
        //    double sumHeight = dataGrid.ActualHeight;

        //    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
        //        dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

        //    dataGrid.UpdateLayout();
        //    dataGrid.Measure(new Size(sumWidth, sumHeight));

        //    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
        //        if (dgc.ActualWidth > 0)
        //            dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.IsReadOnly ? dgc.ActualWidth : (dgc.ActualWidth + 5), DataGridUnitType.Pixel);
        //}

        //public static void gridClear(C1.WPF.DataGrid.C1DataGrid dataGrid)
        //{
        //    if (dataGrid == null || dataGrid.ItemsSource == null) return;

        //    DataTable dtClear = DataTableConverter.Convert(dataGrid.ItemsSource);
        //    if (dtClear != null && dtClear.Rows.Count > 0)
        //    {

        //        dtClear.Rows.Clear();
        //        dataGrid.ItemsSource = DataTableConverter.Convert(dtClear);
        //        //20161015 add scpark
        //        //LoadedCellPresenter 셋팅된것들 초기화하도록
        //        dataGrid.Refresh();
        //    }
        //}


        #endregion
        #endregion

    }
}
