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

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_028 : UserControl, IWorkArea
    {
        private string LANE_ID = string.Empty;
        private string EQPT_GR_TYPE_CODE = string.Empty;

        DataTable dtPopup = new DataTable();

        public FCS001_028()
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
          //  InitControl();
          //  SetEvent();
        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            //Tray Type combo
            //_combo.SetCombo(cboTrayType, CommonCombo_Form.ComboStatus.ALL, sCase: "TRAYTYPE");            

            string[] sFilterEqpType = { "EQPT_GR_TYPE_CODE", "3,4,7" };
            C1ComboBox[] cboEqpKindChild = { cboRow, cboCol, cboStg };
            _combo.SetCombo(cboEqp, CommonCombo_Form.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilterEqpType);


            //_combo.SetCombo(cboRow, CommonCombo_Form.ComboStatus.NONE, sCase: "ROW");

            object[] oParent = { cboEqp };
            _combo.SetComboObjParent(cboRow, CommonCombo_Form.ComboStatus.ALL, sCase: "ROW", objParent: oParent);
            _combo.SetComboObjParent(cboCol, CommonCombo_Form.ComboStatus.ALL, sCase: "COL", objParent: oParent);
            _combo.SetComboObjParent(cboStg, CommonCombo_Form.ComboStatus.ALL, sCase: "STG", objParent: oParent);
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
                dtRqst.Columns.Add("EQP_ROW_LOC", typeof(string));
                dtRqst.Columns.Add("EQP_COL_LOC", typeof(string));
                dtRqst.Columns.Add("EQP_STG_LOC", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["FROM_DATE"] = Util.GetCondition(dtpFromDate, "yyyy-MM-dd 00:00:00");
                dr["TO_DATE"] = Util.GetCondition(dtpToDate, "yyyy-MM-dd 23:59:59");
                dr["EQP_KIND_CD"] = Util.GetCondition(cboEqp, bAllNull: true);
                dr["EQP_ROW_LOC"] = Util.GetCondition(cboRow, bAllNull: true);
                dr["EQP_COL_LOC"] = Util.GetCondition(cboCol, bAllNull: true);
                dr["EQP_STG_LOC"] = Util.GetCondition(cboStg, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TROUBLE_FIRE_HIST", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgTrayLoc, dtRslt, FrameOperation);
               // dtPopup = dtRslt.DefaultView.ToTable(false, "EQP_ID", "TROUBLE_OCCUR_TIME2","EQPT_ALARM_CODE");
                DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_SEL_TROUBLE_FIRE_HIST_CNT", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgHist, dtRslt1, FrameOperation);
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
             /*   Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgTrayLoc.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "EQP_NAME")
                    {
                        if (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgTrayLoc.Rows[cell.Row.Index].DataItem, cell.Column.Name))))
                        {
                            FCS001_028_ACTSAVE wndPopup = new FCS001_028_ACTSAVE();
                            wndPopup.FrameOperation = FrameOperation;
                            if (wndPopup != null)
                            {
                                int rowidx = cell.Row.Index;
                                object[] Parameters = new object[3];
                                Parameters[0] = dtPopup.Rows[rowidx]["EQP_ID"].ToString(); //EQP_ID
                                Parameters[1] = dtPopup.Rows[rowidx]["TROUBLE_OCCUR_TIME2"].ToString();  //OCCUR_TIME
                                Parameters[2] = dtPopup.Rows[rowidx]["EQPT_ALARM_CODE"].ToString();
                                C1WindowExtension.SetParameters(wndPopup, Parameters);

                                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                                // 팝업 화면 숨겨지는 문제 수정.
                                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                            }
                        }
                    }
                }*/
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            FCS001_028_ACTSAVE window = sender as FCS001_028_ACTSAVE;

            if (window.DialogResult == MessageBoxResult.Yes)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }
        private void cboEqpSelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //CommonCombo_Form _combo = new CommonCombo_Form();
            //if (cboEqp.SelectedIndex > -1)
            //{
            //    object[] objParent = { EQPT_GR_TYPE_CODE, LANE_ID, "Y", cboEqp };
            //    _combo.SetComboObjParent(cboRow, CommonCombo_Form.ComboStatus.NONE, sCase: "ROW", objParent: objParent);
            //}
        }

        private void dgTrayLoc_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            /*this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("EQP_NAME"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                }
            }));*/
        }
    }
}
