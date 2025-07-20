/*************************************************************************************
 Created Date : 2021.01.11
      Creator : 
   Decription : 특별관리이력
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.11  DEVELOPER : Initial Created.
  2023.05.04  최도훈 : 인도네시아 조회시 오류나는 현상 수정
  2024.02.26  조영대 : 조회 조건 복원
  2024.04.16  최동훈 : E20240202-001601 탭추가하여 기존화면 현황으로 변경 및 이력탭 신규 개발
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_036 : UserControl, IWorkArea
    {
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS001_036()
        {
            InitializeComponent();
            InitCombo();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnHistSearch_Click(object sender, RoutedEventArgs e)
        {
            GetHistList();
        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelChild = { cboRoute };
            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            _combo.SetCombo(cboRoute, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent);

            C1ComboBox[] cboHistLineChild = { cboHistModel };
            _combo.SetCombo(cboHistLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboHistLineChild);

            C1ComboBox[] cboHistModelChild = { cboHistRoute };
            C1ComboBox[] cboHistModelParent = { cboHistLine };
            _combo.SetCombo(cboHistModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboHistModelChild, cbParent: cboHistModelParent);

            C1ComboBox[] cboHistRouteParent = { cboHistLine, cboHistModel };
            _combo.SetCombo(cboHistRoute, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboHistRouteParent);
        }

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("CURR_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                if (!string.IsNullOrEmpty(txtTrayId.Text)) dr["CSTID"] = txtTrayId.Text;
                if (!string.IsNullOrEmpty(txtLotId.Text)) dr["PROD_LOTID"] = txtLotId.Text;
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd000000"); 
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd235959"); 

                if (chkCurrTray.IsChecked == true)
                {
                    dr["CURR_FLAG"] = "Y";
                }

                dtRqst.Rows.Add(dr);

                // 백그라운드 실행 실행 후 dgSpecialHist_ExecuteDataCompleted 이벤트 처리
                dgSpecialHist.ExecuteService("DA_SEL_TRAY_SPECIAL", "RQSTDT", "RSLTDT", dtRqst);                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSpecialHist_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {

        }

        private void dgSpecialHist_DoubleClick(object sender, MouseButtonEventArgs e)
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

                if (cell.Column.Name.Equals("CSTID") || cell.Column.Name.Equals("LOTID"))
                {
                    //Tray 정보조회 화면 연계
                    object[] parameters = new object[10];
                    parameters[0] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[datagrid.CurrentRow.Index].DataItem, "CSTID"));
                    parameters[1] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[datagrid.CurrentRow.Index].DataItem, "LOTID"));
                    this.FrameOperation.OpenMenu("SFU010710010", true, parameters); //Tray 정보조회
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSpecialHist_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("CSTID") || e.Cell.Column.Name.Equals("LOTID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                }
            }));
        }

        private void GetHistList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("CURR_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                if (!string.IsNullOrEmpty(txtHistTrayId.Text)) dr["CSTID"] = txtHistTrayId.Text;
                if (!string.IsNullOrEmpty(txtHistProdLotId.Text)) dr["PROD_LOTID"] = txtHistProdLotId.Text;
                if (!string.IsNullOrEmpty(txtHistTrayLotId.Text)) dr["LOTID"] = txtHistTrayLotId.Text;
                dr["EQSGID"] = Util.GetCondition(cboHistLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboHistModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboHistRoute, bAllNull: true);
                dr["FROM_DATE"] = dtpHistFromDate.SelectedDateTime.ToString("yyyyMMdd000000");
                dr["TO_DATE"] = dtpHistToDate.SelectedDateTime.ToString("yyyyMMdd235959");

                if (chkHistCurrTray.IsChecked == true)
                {
                    dr["CURR_FLAG"] = "Y";
                }

                dtRqst.Rows.Add(dr);

                // 백그라운드 실행 실행 후 dgSpecialHist_ExecuteDataCompleted 이벤트 처리
                dgHistSpcl.ExecuteService("DA_SEL_TB_SFC_LOT_SPCL_GR_HIST_INFO", "RQSTDT", "RSLTDT", dtRqst);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgHistSpcl_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {

        }

        private void dgHistSpcl_DoubleClick(object sender, MouseButtonEventArgs e)
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

                if (cell.Column.Name.Equals("CSTID") || cell.Column.Name.Equals("LOTID"))
                {
                    //Tray 정보조회 화면 연계
                    object[] parameters = new object[10];
                    parameters[0] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[datagrid.CurrentRow.Index].DataItem, "CSTID"));
                    parameters[1] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[datagrid.CurrentRow.Index].DataItem, "LOTID"));
                    this.FrameOperation.OpenMenu("SFU010710010", true, parameters); //Tray 정보조회
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgHistSpcl_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("CSTID") || e.Cell.Column.Name.Equals("LOTID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                }
            }));
        }
    }
}
