/*************************************************************************************
 Created Date : 2020.12.
      Creator : 박수미
   Decription : 상대판정 Log
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.08  DEVELOPER : 박수미





 
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
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_085 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]

        private DateTime _dtFromDate = DateTime.Now.AddDays(-1);
        private DateTime _dtFromTime = DateTime.Now;
        private DateTime _dtToDate = DateTime.Now;
        private DateTime _dtToTime = DateTime.Now;

        public FCS002_085()
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
        }

        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
            string[] sFilter = { "MEASR_TYPE_CODE" };
            _combo.SetCombo(cboMeasType, CommonCombo_Form_MB.ComboStatus.ALL, sFilter: sFilter, sCase: "COMMCODE");
        }
        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("MEASR_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd ") + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd ") + dtpToTime.DateTime.Value.ToString("HH:mm:00");
                if (!string.IsNullOrEmpty(txtLotID.Text)) dr["PROD_LOTID"] = Util.NVC(txtLotID.Text);
                if (!string.IsNullOrEmpty(txtTrayNo.Text)) dr["LOTID"] = Util.NVC(txtTrayNo.Text);
                if (!string.IsNullOrEmpty(txtCelID.Text)) dr["SUBLOTID"] = Util.NVC(txtCelID.Text);
                dr["MEASR_TYPE_CODE"] = Util.GetCondition(cboMeasType, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_RJUDG_LOG_MB", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgLogList, dtRslt, this.FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Event]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        private void dgLogList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
            if (e.Cell.Presenter == null)
            {
                return;
            }

            if (e.Cell.Row.Type == DataGridRowType.Item)
            {
                if (Util.NVC(e.Cell.Column.Name).Equals("MEASR_VALUE"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Pink);
                }
                else if (Util.NVC(e.Cell.Column.Name).Equals("LOTID") || Util.NVC(e.Cell.Column.Name).Equals("SUBLOTID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }

                if (Util.NVC(e.Cell.Column.Name).Equals("RJUDG_BAS_CODE"))
                {
                    string _flag = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "RJUDG_BAS_CODE_F"));
                        if (!string.IsNullOrEmpty(_flag))
                        {
                            switch (_flag)
                            {
                                case "D":
                                    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 7).Presenter.Background = new SolidColorBrush(Colors.LightBlue);
                                    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 8).Presenter.Background = new SolidColorBrush(Colors.LightBlue);
                                    break;
                                case "L":
                                    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 8).Presenter.Background = new SolidColorBrush(Colors.LightBlue);
                                    break;
                                case "U":
                                    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 7).Presenter.Background = new SolidColorBrush(Colors.LightBlue);
                                    break;
                            }
                        }
                    }
                }
            }));
        }
        private void dgLotList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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

                if (cell.Text != datagrid.CurrentColumn.Header.ToString())
                {
                    if (cell.Column.Name.Equals("SUBLOTID"))
                    {
                        string sCellId = cell.Text;
                        FCS002_022 fcs022 = new FCS002_022();
                        fcs022.FrameOperation = FrameOperation;

                        object[] parameters = new object[2];
                        parameters[0] = Util.NVC(sCellId);
                        parameters[1] = "Y"; //_sActYN

                        this.FrameOperation.OpenMenu("SFU010710310", true, parameters);
                    }
                    if (cell.Column.Name.Equals("LOTID"))
                    {
                        FCS002_021 wndTRAY = new FCS002_021();
                        wndTRAY.FrameOperation = FrameOperation;

                        object[] Parameters = new object[10];
                        Parameters[0] = datagrid.GetCell(cell.Row.Index, 1).Text; //Tray 
                        Parameters[1] = cell.Text; //Tray No
                        this.FrameOperation.OpenMenu("SFU010710300", true, Parameters);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

    }
}
