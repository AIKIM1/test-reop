using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ELEC_SlBatch.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_RECREATE_SCRAP_LOT : C1Window, IWorkArea
    {
        private string _EqsgID = string.Empty;
        private string _EqptID = string.Empty;
        private string _ProcID = string.Empty;
        private string _LotID = string.Empty;
        private decimal _ActQty2 = 0;
        DataTable ReCreatTable = new DataTable();

        Util _Util = new Util();
        DataTable dtiUse = new DataTable();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ELEC_RECREATE_SCRAP_LOT()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null)
                {
                    _EqsgID = Util.NVC(tmps[0]);
                    _EqptID = Util.NVC(tmps[1]);
                    _ProcID = Util.NVC(tmps[2]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            searCh_List();
        }

        private void searCh_List()
        {
            DataTable scrapTable = new DataTable();
            scrapTable.Columns.Add("LANGID", typeof(string));
            scrapTable.Columns.Add("EQSGID", typeof(string));
            scrapTable.Columns.Add("PROCID", typeof(string));
            scrapTable.Columns.Add("EQPTID", typeof(string));
            scrapTable.Columns.Add("FROMDATE", typeof(string));
            scrapTable.Columns.Add("TODATE", typeof(string));

            DataRow indata = scrapTable.NewRow();

            indata["LANGID"] = LoginInfo.LANGID;
            indata["EQSGID"] = _EqsgID;
            indata["PROCID"] = _ProcID;
            indata["EQPTID"] = _EqptID;
            indata["FROMDATE"] = Util.GetCondition(dtpDateFrom);
            indata["TODATE"] = Util.GetCondition(dtpDateTo);

            scrapTable.Rows.Add(indata);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_LIST_END_TERM", "RQSTDT", "RSLTDT", scrapTable);
            dgLotList.CurrentCellChanged -= dgLotList_CurrentCellChanged;
            Util.GridSetData(dgLotList, result, FrameOperation, true);
            dgLotList.CurrentCellChanged += dgLotList_CurrentCellChanged;

        }

        private void reCreateLotChk(DataSet dataset)
        {
            new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CANCEL_TERM_LOT_ELEC_CHK", "INDATA,INLOT", null, dataset);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable chkTable = ((DataView)dgLotList.ItemsSource).ToTable();
                DataSet dataset = new DataSet();
                DataTable RQSTDT = dataset.Tables.Add("INDATA");

                RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                RQSTDT.Columns.Add("IFMODE", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow indata = RQSTDT.NewRow();

                indata["SRCTYPE"] = "UI";
                indata["IFMODE"] = "OFF";
                indata["EQPTID"] = _EqptID;
                indata["PROCID"] = _ProcID;
                indata["USERID"] = LoginInfo.USERID;

                dataset.Tables["INDATA"].Rows.Add(indata);

                DataTable RQSTDT2 = dataset.Tables.Add("INLOT");
                RQSTDT2.Columns.Add("LOTID", typeof(string));
                RQSTDT2.Columns.Add("ACTQTY", typeof(string));
                RQSTDT2.Columns.Add("ACTQTY2", typeof(string));


                for (int i = 0; i < dgLotList.Rows.Count; i++)
                {
                    if (DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "CHK").IsTrue())
                    {
                        if (!String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LANE_QTY"))))
                        {
                            _ActQty2 = decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LANE_QTY"))) * decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "WIPQTY")));
                            DataRow indata2 = RQSTDT2.NewRow();
                            indata2["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID"));
                            indata2["ACTQTY"] = decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "WIPQTY")));
                            indata2["ACTQTY2"] = _ActQty2;

                            dataset.Tables["INLOT"].Rows.Add(indata2);
                        }
                        else
                        {
                            Util.MessageInfo("SFU1351");
                            return;

                        }
                    }
                }

                reCreateLotChk(dataset);

                 new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CANCEL_TERM_LOT_ELEC", "INDATA,INLOT", null, dataset);

                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                searCh_List();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void Check_Lot(int rowIndex) {
            if (DataTableConverter.GetValue(dgLotList.Rows[rowIndex].DataItem, "CHK").ToString().Equals("0"))
            {
                dgLotList.SelectedIndex = rowIndex;

                C1.WPF.DataGrid.DataGridCell gridCell = dgLotList.GetCell(rowIndex, dgLotList.Columns["WIPQTY"].Index) as C1.WPF.DataGrid.DataGridCell;
                if (gridCell != null && gridCell.Presenter != null && gridCell.Presenter.Content != null)
                {
                    gridCell.Column.IsReadOnly = true;
                }
            }
            else
            {
                C1.WPF.DataGrid.DataGridCell gridCell = dgLotList.GetCell(rowIndex, dgLotList.Columns["WIPQTY"].Index) as C1.WPF.DataGrid.DataGridCell;
                if (gridCell != null && gridCell.Presenter != null && gridCell.Presenter.Content != null)
                {
                    gridCell.Column.IsReadOnly = false;
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (dgLotList.ItemsSource == null || dgLotList.Rows.Count == 0)
                return;

            if (dgLotList.CurrentRow.DataItem == null)
                return;
            int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;

            DataTable dt = ((DataView)dgLotList.ItemsSource).Table;

            if (DataTableConverter.GetValue(dgLotList.Rows[rowIndex].DataItem, "CHK").ToString().Equals("1"))
            {
                dgLotList.SelectedIndex = rowIndex;

                Check_Lot(rowIndex);
            }
        }

        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            if (dgLotList.ItemsSource == null || dgLotList.Rows.Count == 0)
                return;

            if (dgLotList.CurrentRow.DataItem == null)
                return;
            int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;

            DataTable dt = ((DataView)dgLotList.ItemsSource).Table;

            if (DataTableConverter.GetValue(dgLotList.Rows[rowIndex].DataItem, "CHK").ToString().Equals("0"))
            {
                dgLotList.SelectedIndex = rowIndex;

                Check_Lot(rowIndex);
            }
        }

        private void dgLotList_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {

            try { 
                if(e.Cell != null && e.Cell.Row != null) { 
                    int rowIndex = e.Cell.Row.Index;
                    Check_Lot(rowIndex);                    
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}

