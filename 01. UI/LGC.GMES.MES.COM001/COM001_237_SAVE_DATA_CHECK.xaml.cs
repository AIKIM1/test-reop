/*************************************************************************************
 Created Date : 2024.03.11
      Creator : 안유수
   Decription : LOT 정보 변경 데이터 확인 팝업 창
--------------------------------------------------------------------------------------
 [Change History]
  2024.03.11  안유수 : Initial Created.


**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{

    public partial class COM001_237_SAVE_DATA_CHECK : C1Window, IWorkArea
    {
        #region Initialize
        DataTable dtbefore;
        DataTable dtafter;

        public IFrameOperation FrameOperation { get; set; }

        public COM001_237_SAVE_DATA_CHECK()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 2)
            {
                dtbefore = tmps[0] as DataTable; 
                dtafter = tmps[1] as DataTable;
            }

            dgbefore.ItemsSource = DataTableConverter.Convert(dtbefore);
            dgafter.ItemsSource = DataTableConverter.Convert(dtafter);
        }

        #endregion

        #region Event

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dgafter_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                foreach (C1.WPF.DataGrid.DataGridRow dc in dataGrid.Rows)
                {
                    for (int i = 0; i < dgbefore.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[dc.Index].DataItem, "LOTID")) == Util.NVC(DataTableConverter.GetValue(dgbefore.Rows[i].DataItem, "LOTID")))
                        {
                            for (int j = 0; j < dgbefore.Columns.Count; j++)
                            {
                                if (dataGrid.GetCell(dc.Index, j).Value.ToString() != dgbefore.GetCell(i, j).Value.ToString() && dataGrid.Columns[j].Name != "LOTID")
                                {
                                    if (dataGrid.GetCell(dc.Index, j).Presenter != null)
                                        dataGrid.GetCell(dc.Index, j).Presenter.Background = new SolidColorBrush(Colors.Orange);
                                }
                            }
                        }
                    }
                }
            }));
        }

        #endregion

    }
}
