/*************************************************************************************
 Created Date : 2020.12.08
      Creator : 박준규
   Decription : Aging 화재발생 이력조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.08  DEVELOPER : 박준규





 
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
    public partial class FCS002_028 : UserControl
    {
        private string LANE_ID = string.Empty;
        private string EQPT_GR_TYPE_CODE = string.Empty;

        DataTable dtPopup = new DataTable();

        public FCS002_028()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
          InitCombo();

            dtpFromDate.SelectedDateTime = System.DateTime.Now;
            dtpToDate.SelectedDateTime = System.DateTime.Now.AddDays(1);

            this.Loaded -= UserControl_Loaded;
        }

        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            string[] sFilter = { "AGING_LANE_TYPE_CODE" };
            _combo.SetCombo(cboEqpLane, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "AREA_COMMON_CODE", sFilter: sFilter);

            string[] sFilterEqpType = { "EQPT_GR_TYPE_CODE", "3,4" };
            C1ComboBox[] cboEqpKindChild = { cboSCLine };
            _combo.SetCombo(cboEqpKind, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilterEqpType , cbChild: cboEqpKindChild);
            
            object[] oParent = { cboEqpKind, cboEqpLane };
            C1ComboBox[] cboEqpKindChild3 = { cboCol };
            _combo.SetComboObjParent(cboSCLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "SCLINE", objParent: oParent , cbChild:cboEqpKindChild3); //20210331 S/C 호기 필수 값으로 변경

            object[] oParent3 = { cboEqpKind, cboEqpLane,cboSCLine };
            C1ComboBox[] cboEqpKindChild1 = {  cboCol };
            _combo.SetComboObjParent(cboRow, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "AGING_ROW", objParent: oParent3, cbChild: cboEqpKindChild1);
            object[] oParent1 = { cboEqpKind, cboEqpLane, cboSCLine,cboRow };
            C1ComboBox[] cboEqpKindChild2 = { cboStg };
            _combo.SetComboObjParent(cboCol, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "AGING_COL", objParent: oParent1, cbChild: cboEqpKindChild2);
            object[] oParent2 = { cboEqpKind, cboEqpLane, cboSCLine, cboRow,cboCol };
            _combo.SetComboObjParent(cboStg, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "AGING_STG", objParent: oParent2);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQP_KIND_CD", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("EQP_ROW_LOC", typeof(string));
                dtRqst.Columns.Add("EQP_COL_LOC", typeof(string));
                dtRqst.Columns.Add("EQP_STG_LOC", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["FROM_DATE"] = Util.GetCondition(dtpFromDate, "yyyy-MM-dd 00:00:00");
                dr["TO_DATE"] = Util.GetCondition(dtpToDate, "yyyy-MM-dd 23:59:59");
                dr["EQP_KIND_CD"] = Util.GetCondition(cboEqpKind, bAllNull: true);
                dr["EQPTID"] = Util.GetCondition(cboSCLine, bAllNull: true);
                dr["LANE_ID"] = Util.GetCondition(cboEqpLane, bAllNull: true);
                dr["EQP_ROW_LOC"] = Util.GetCondition(cboRow, bAllNull: true);
                dr["EQP_COL_LOC"] = Util.GetCondition(cboCol, bAllNull: true);
                dr["EQP_STG_LOC"] = Util.GetCondition(cboStg, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TROUBLE_FIRE_HIST_MB", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgTrayLoc, dtRslt, FrameOperation,true);

                DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_SEL_TROUBLE_FIRE_HIST_CNT_MB", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgHist, dtRslt1, FrameOperation,true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgTrayLocDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgTrayLoc.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    //if (cell.Column.Name == "EQP_NAME")
                    //{
                        if (Util.NVC(DataTableConverter.GetValue(dgTrayLoc.Rows[cell.Row.Index].DataItem, "CNTT")).Equals("조치"))
                        {
                        FCS002_028_ACTSAVE wndPopup = new FCS002_028_ACTSAVE();
                        wndPopup.FrameOperation = FrameOperation;
                        if (wndPopup != null)
                        {

                            int rowidx = cell.Row.Index;

                            
                            object[] Parameters = new object[4];
                            Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgTrayLoc.Rows[cell.Row.Index].DataItem, "RACK_ID")); //EQP_ID
                            Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgTrayLoc.Rows[cell.Row.Index].DataItem, "TROUBLE_OCCUR_TIME")); // dtPopup.Rows[rowidx]["TROUBLE_OCCUR_TIME2"].ToString();  //OCCUR_TIME
                            Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgTrayLoc.Rows[cell.Row.Index].DataItem, "EQP_NAME")); 
                            Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgTrayLoc.Rows[cell.Row.Index].DataItem, "TRANSACTION_SERIAL_NO")); 
                            C1WindowExtension.SetParameters(wndPopup, Parameters);

                            wndPopup.Closed += new EventHandler(wndPopup_Closed);

                            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        }
                    }
                    //}
                 }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            FCS002_028_ACTSAVE window = sender as FCS002_028_ACTSAVE;

            if (window.DialogResult == MessageBoxResult.Yes)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        private void dgTrayLoc_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private void dgHist_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private void cboEqpKind_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }
    }
}
