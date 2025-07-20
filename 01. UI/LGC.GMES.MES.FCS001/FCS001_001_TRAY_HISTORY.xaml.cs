/*************************************************************************************
 Created Date : 2020.10.14
      Creator : 
   Decription : Tray 이력
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.14  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows.Controls;
using System.Windows;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_001_TRAY_HISTORY : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _sRow;
        private string _sCol;
        private string _sStg;
        private string _sEqpID;
        private string _sType;  //E : EQPTID, R : RACK

        public string ROW
        {
            set { this._sRow = value; }
        }

        public string COL
        {
            set { this._sCol = value; }
        }
        public string STG
        {
            set { this._sStg = value; }
        }

        public string EQP
        {
            set { this._sEqpID = value; }
        }

        public string TYPE
        {
            set { this._sType = value; }
        }
        public FCS001_001_TRAY_HISTORY()
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
        private void SetText()
        {
            txtRow.Text = _sRow;
            txtCol.Text = _sCol;
            txtStg.Text = _sStg;
        }

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null && tmps.Length >= 1)
            {
                ROW = Util.NVC(tmps[0]);
                COL = Util.NVC(tmps[1]);
                STG = Util.NVC(tmps[2]);
                EQP = Util.NVC(tmps[3]);
                TYPE = Util.NVC(tmps[4]);
            }
            SetText();
            //조회함수
            GetList();
        }
        private void dgTrayHist_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
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

                if (datagrid.CurrentColumn.Name.Equals("CSTID")||datagrid.CurrentColumn.Name.Equals("LOTID"))
                {
                    FCS001_021 wndTRAY = new FCS001_021();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = ""; //Tray ID
                    Parameters[1] = DataTableConverter.GetValue(dgTrayHist.CurrentRow.DataItem, "LOTID").GetString(); //Tray No
                    this.FrameOperation.OpenMenu("SFU010710010", true, Parameters);
                    this.DialogResult = MessageBoxResult.OK;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod
        #region [조회]
        private void GetList()
        {
            try
            {
                if (_sType.Equals("R"))
                {
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("LANGID", typeof(string));
                    inDataTable.Columns.Add("RACK_ID", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["RACK_ID"] = _sEqpID;
                    inDataTable.Rows.Add(dr);
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_RACK_TRAY_LIST_R", "RQSTDT", "RSLTDT", inDataTable);
                    Util.GridSetData(dgTrayHist, dtRslt, FrameOperation, true);
                }
                else
                {
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("LANGID", typeof(string));
                    inDataTable.Columns.Add("EQPTID", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQPTID"] = _sEqpID;
                    inDataTable.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_RACK_TRAY_LIST", "RQSTDT", "RSLTDT", inDataTable);
                    Util.GridSetData(dgTrayHist, dtRslt, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        private void dgTrayHist_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

                if (Util.NVC(e.Cell.Column.Name).Equals("LOTID") ||Util.NVC(e.Cell.Column.Name).Equals("CSTID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);

            }));
        }
    }
}
