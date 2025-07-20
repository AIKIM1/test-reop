/*************************************************************************************
 Created Date : 2020.12.
      Creator : 
   Decription : Box별 공정정보
--------------------------------------------------------------------------------------
 [Change History]
  2020.  DEVELOPER : Initial Created.





 
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
using System.Collections;

namespace LGC.GMES.MES.FCS002
{

    public partial class FCS002_044 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        #endregion
        #region [Initialize]
        public FCS002_044()
        {
            InitializeComponent();
            InitCombo();
            InitControl();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }
        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
            C1ComboBox[] cboLaneChild = { cboRow, cboCol, cboStg };
            string[] sFilterLane = { "", "1" };
            _combo.SetCombo(cboLane, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "LANE", cbChild: cboLaneChild, sFilter: sFilterLane);

            object[] oParent = { "1", cboLane };
            _combo.SetComboObjParent(cboRow, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "ROW", objParent: oParent);
            _combo.SetComboObjParent(cboCol, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "COL", objParent: oParent);
            _combo.SetComboObjParent(cboStg, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "STG", objParent: oParent);

            string[] sFilter1 = { "COMBO_FORM_SPCL_FLAG" };
            _combo.SetCombo(cboSpecial, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

            string[] sFilter2 = { "COMBO_PROC_INFO_BY_DATE_SEARCH_CONDITION" }; //E07
            _combo.SetCombo(cboSearch, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter2);

            string[] sFilter3 = { "COMBO_PROC_INFO_BY_DATE_ORDER_CONDITION" }; //E08
            _combo.SetCombo(cboOrder, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter3);

            string[] sFilter4 = {"1",null, null, null };
            _combo.SetCombo(cboOper, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "PROC_BY_GR", sFilter:sFilter4);
        }

        private void InitControl()
        {
            dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-1);
            dtpToDate.SelectedDateTime = DateTime.Now;
        }
        #endregion
        #region [Method]
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("S71", typeof(string));
                dtRqst.Columns.Add("FROM_TIME", typeof(string));
                dtRqst.Columns.Add("TO_TIME", typeof(string));
                dtRqst.Columns.Add("CURRENT_YN", typeof(string));
                dtRqst.Columns.Add("HISTORY_YN", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("SPCL_FLAG", typeof(string));
                dtRqst.Columns.Add("S66", typeof(string)); //EQP_ROW_LOC
                dtRqst.Columns.Add("S67", typeof(string)); //EQP_COL_LOC
                dtRqst.Columns.Add("S68", typeof(string)); //EQP_STG_LOC
                dtRqst.Columns.Add(Util.GetCondition(cboSearch), typeof(string));
                dtRqst.Columns.Add("ORDER_" + Util.GetCondition(cboOrder), typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROCID"] = Util.GetCondition(cboOper, bAllNull: true);
                dr["S71"] = Util.GetCondition(cboLane);
                dr["S66"] = Util.GetCondition(cboRow, bAllNull: true);
                dr["S67"] = Util.GetCondition(cboCol, bAllNull: true);
                dr["S68"] = Util.GetCondition(cboStg, bAllNull: true);
                dr["FROM_TIME"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                dr["TO_TIME"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm:00");
                dr["CURRENT_YN"] = (bool)chkHistory.IsChecked ? null : "Y";
                dr["HISTORY_YN"] = (bool)chkHistory.IsChecked ? "Y" : null;
                if (!string.IsNullOrEmpty(txtLotID.Text)) dr["PROD_LOTID"] = Util.NVC(txtLotID.Text);
                dr["SPCL_FLAG"] = Util.GetCondition(cboSpecial, bAllNull: true);
                dr[Util.GetCondition(cboSearch)] = "Y";
                dr["ORDER_" + Util.GetCondition(cboOrder)] = "Y";

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_INFO_CONDITION_ALL_MB", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgBoxOper, dtRslt, this.FrameOperation,true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Event]
        private void dgDateOper_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                if (e.Cell.Column.Name.Equals("CSTID"))
                {
                    int row = e.Cell.Row.Index;
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue); // CSTID 색상 변경

                    //DUMMY TRAY
                    string _sDummy = Util.NVC(DataTableConverter.GetValue(dgBoxOper.Rows[row].DataItem, "DUMMY_FLAG"));
                    if (_sDummy.Equals("Y")) e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Green);

                    //SPECIAL TRAY
                    string _sSpecial = Util.NVC(DataTableConverter.GetValue(dgBoxOper.Rows[row].DataItem, "SPCL_FLAG"));
                    if (_sSpecial.Equals("P"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else if (_sSpecial.Equals("Y"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.DarkOrange);
                    }
                }
         
            }));
        }

        private void dgDateOper_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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

                if (cell.Text == datagrid.CurrentColumn.Header.ToString()) return;


                if (datagrid.CurrentColumn.Name == "CSTID")
                {
                    FCS002_021 wndTRAY = new FCS002_021();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgBoxOper.CurrentRow.DataItem, "CSTID")); //Tray ID
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgBoxOper.CurrentRow.DataItem, "LOTID")); //Tray No
                    this.FrameOperation.OpenMenu("SFU010710300", true, Parameters);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        // ROW HEADER에 NUMBER
        private void dgBoxOper_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();
            tb.Text = (e.Row.Index + 1 - dgBoxOper.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void dgBoxOper_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }
    }
}
