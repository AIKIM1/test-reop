/*************************************************************************************
/*************************************************************************************
 Created Date : 2023.05.24
      Creator : 김선준
   Decription : Partial ILT 해제요청 NG메시지
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.24  김선준 : Initial Created.  
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_035_REQUEST_AGINGHOLD_NG : C1Window 
    {
        #region #. Member Variable Lists...
        public IFrameOperation FrameOperation { get; internal set; }
        public DataTable dtLot;
        #endregion

        #region #. Declaration & Constructor
        public COM001_035_REQUEST_AGINGHOLD_NG()
        {
            InitializeComponent();
        }
        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null & tmps.Length != 0 && null != tmps[0])
            {
                dtLot = (DataTable)tmps[0];
                DataView dv = new DataView(dtLot);
                dv.Sort = "STATUS_NM ASC";
                 
                this.grdMain.ItemsSource = dv;
            }
        }
        #endregion

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            int iCnt = dtLot.AsEnumerable().Where(x => x.Field<string>("STATUS_NM").Contains("[OK]")).Count();
            if (iCnt == 0)
            { 
                Util.MessageValidation("FM_ME_0125");  // 등록할 대상이 존재하지 않습니다.
                return;
            }
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void grdMain_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        { 
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dataGrid.HeadersVisibility = C1.WPF.DataGrid.DataGridHeadersVisibility.Column;
            dataGrid.RowHeaderWidth = 0;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                //진행중인 색 변경
                if (e.Cell.Column.Name.Equals("STATUS_NM"))
                {
                    string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "STATUS_NM"));
                    if (sCheck.Contains("[NG]"))
                    {
                        foreach (C1.WPF.DataGrid.DataGridColumn dc in dataGrid.Columns)
                        {
                            if (dc.Visibility == Visibility.Visible)
                            {
                                if (dataGrid.GetCell(e.Cell.Row.Index, dc.Index).Presenter != null)
                                {
                                    dataGrid.GetCell(e.Cell.Row.Index, dc.Index).Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                }
                            }
                        }
                         
                    } 
                }
            }));
        }
    }
}
