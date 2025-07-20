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

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_001_TRAY_HISTORY : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _sRow;
        private string _sCol;
        private string _sStg;
        private string _sEqpID;
        private string _sEqpname;
        private string _sType;  // R : RACK, 1 : 충방전기, L : CPF, 8 : 전용 OCV, I : IR OCV

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
        public string EQPTNAME
        {
            set { this._sEqpname = value; }
        }
        public FCS002_001_TRAY_HISTORY()
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
                ROW = Util.NVC(tmps[0]);
                COL = Util.NVC(tmps[1]);
                STG = Util.NVC(tmps[2]);
                EQP = Util.NVC(tmps[3]);
                TYPE = Util.NVC(tmps[4]);
                EQPTNAME = Util.NVC(tmps[5]);
            }
            SetText();
            SetTextBox();
            InitControl();
            //조회함수
            GetList();
        }
        private void SetTextBox()
        {
            switch (_sType)
            {
                // 기본값은 열연단 정보            
                case "8":
                case "I":
                    txtCol.Visibility = Visibility.Collapsed;
                    labelCol.Visibility = Visibility.Collapsed;
                    txtRow.Visibility = Visibility.Collapsed;
                    labelRow.Visibility = Visibility.Collapsed;
                    txtStg.Visibility = Visibility.Collapsed;
                    labelStg.Visibility = Visibility.Collapsed;
                    break;
                default:
                    txtEqp.Visibility = Visibility.Collapsed;
                    labelEqp.Visibility = Visibility.Collapsed;
                    break;

            }   
        }

        private void SetText()
        {
            txtEqp.Text = _sEqpname;
            txtRow.Text = _sRow;
            txtCol.Text = _sCol;
            txtStg.Text = _sStg;
        }

        private void InitControl()
        {
            dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-30);
            dtpToDate.SelectedDateTime = DateTime.Now.AddDays(1);
        }

        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
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
                    FCS002_021 wndTRAY = new FCS002_021();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = ""; //Tray ID
                    Parameters[1] = DataTableConverter.GetValue(dgTrayHist.CurrentRow.DataItem, "LOTID").GetString(); //Tray No
                    this.FrameOperation.OpenMenu("SFU010710300", true, Parameters);
                    this.DialogResult = MessageBoxResult.OK;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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

                if (Util.NVC(e.Cell.Column.Name).Equals("LOTID") || Util.NVC(e.Cell.Column.Name).Equals("CSTID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);

            }));
        }

        #endregion

        #region Mehod
        private void GetList()
        {
            try
            {


                TimeSpan dt = dtpToDate.SelectedDateTime - dtpFromDate.SelectedDateTime;

                int dtDays = dt.Days;

                if(dtDays > 31)
                {

                    Util.MessageValidation("SFU4466");  //조회기간은 30일을 초과할수 없습니다.
                    return;
                }


                if (_sType.Equals("R"))
                {
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("LANGID", typeof(string));
                    inDataTable.Columns.Add("RACK_ID", typeof(string));
                    inDataTable.Columns.Add("FROM_DATE", typeof(string));
                    inDataTable.Columns.Add("TO_DATE", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["RACK_ID"] = _sEqpID;
                    dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd");
                    dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd");

                    inDataTable.Rows.Add(dr);
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_RACK_TRAY_LIST_R_MB", "RQSTDT", "RSLTDT", inDataTable);
                    Util.GridSetData(dgTrayHist, dtRslt, FrameOperation, true);
                }
                else
                {
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("LANGID", typeof(string));
                    inDataTable.Columns.Add("EQPTID", typeof(string));
                    inDataTable.Columns.Add("FROM_DATE", typeof(string));
                    inDataTable.Columns.Add("TO_DATE", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQPTID"] = _sEqpID;
                    dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd");
                    dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd");

                    inDataTable.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_RACK_TRAY_LIST_MB", "RQSTDT", "RSLTDT", inDataTable);
                    Util.GridSetData(dgTrayHist, dtRslt, FrameOperation);
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
