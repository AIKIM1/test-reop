/*************************************************************************************
 Created Date : 2020.10.29
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.29  DEVELOPER : Initial Created.





 
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Data;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// TSK_112.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_021_SEL_TRAY : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        private string sTrayId = string.Empty;

        public FCS001_021_SEL_TRAY()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Initialize]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null && tmps.Length >= 1)
            {
                sTrayId = Util.NVC(tmps[0]);
            }
            else
            {
                sTrayId = "";
            }
            GetList();
        }
        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = sTrayId;
                inDataTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_OP_STATUS", "INDATA", "OUTDATA", inDataTable);
                dgTrayList.ItemsSource = DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Event]

        private void dgTrayList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell == null || datagrid.CurrentRow == null)
                {
                    return;
                }

                if (cell.Text != datagrid.CurrentColumn.Header.ToString() && cell.Column.Name.Equals("LOTID"))
                {
                    object[] Parameters = new object[10];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgTrayList.CurrentRow.DataItem, "CSTID"));  //TrayID
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgTrayList.CurrentRow.DataItem, "LOTID"));  //Tray NO
                    //FIN_CD 넘기기
                    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgTrayList.CurrentRow.DataItem, "WIPSTAT"));   //FinCD       
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.CurrentRow.DataItem, "ATCALIB_TYPE_CODE")).Equals("P"))
                    {
                        Parameters[3] = "true"; //FinCheck
                        Parameters[4] = null; //EQPID
                    }
                    else
                    {
                        Parameters[3] = "false"; //FinCheck
                    }
                    Parameters[5] = "Y"; //PROCESS_HIST_YN
                    this.FrameOperation.OpenMenu("SFU010710010", true, Parameters);
                    this.DialogResult = MessageBoxResult.OK;
                }

               /* if (datagrid.CurrentColumn.Name.Equals("CSTID"))
                {
                    FCS001_021 wndTRAY = new FCS001_021();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = ""; //Tray ID
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgTrayList.CurrentRow.DataItem, "LOTID")); //Tray No
                    this.FrameOperation.OpenMenu("SFU010710010", true, Parameters);
                    this.DialogResult = MessageBoxResult.OK;
                }*/
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgTrayList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (Util.NVC(e.Cell.Column.Name) == "LOTID")
                {
                    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }

            }));
        }
        #endregion

    }
}
