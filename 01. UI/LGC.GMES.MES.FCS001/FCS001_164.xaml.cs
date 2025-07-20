/*************************************************************************************
 Created Date : 2023.09.25
      Creator : 
   Decription : 비정상 트레이 조회
--------------------------------------------------------------------------------------
 [Change History]
  2023.09.25  DEVELOPER : Initial Created.
  2024.01.05  김용식         : 로스시간 조회 조건 추가, Aging 후단출고 현황팝업 추가
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
    public partial class FCS001_164 : UserControl, IWorkArea
    {
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS001_164()
        {
            InitializeComponent();
            InitCombo();
            InitControl();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelChild = { cboRoute };
            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            string[] sFilter = { "ROUT_TYPE_GR_CODE" };
            C1ComboBox[] cboRouteSetChild = { cboRoute };
            _combo.SetCombo(cboRouteDG, CommonCombo_Form.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter, cbChild: cboRouteSetChild);

            C1ComboBox nCbo = new C1ComboBox();
            C1ComboBox[] cboRouteParent = { cboLine, cboModel, nCbo, nCbo, cboRouteDG };
            _combo.SetCombo(cboRoute, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent);
        }

        private void InitControl()
        {
            dtpFromDate.SelectedDateTime = DateTime.Now.AddMonths(-2);
            dtpToDate.SelectedDateTime = DateTime.Now;
            txtLossMI.Text = "60";
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
                dtRqst.Columns.Add("ROUT_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("CST_ID", typeof(string));
                dtRqst.Columns.Add("LOSS_MI", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUT_TYPE_CODE"] = Util.GetCondition(cboRouteDG, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                if (!string.IsNullOrEmpty(txtLotId.Text)) dr["PROD_LOTID"] = txtLotId.Text;
                if (!string.IsNullOrEmpty(txtCstId.Text)) dr["CST_ID"] = txtCstId.Text;
                if (!string.IsNullOrEmpty(txtLossMI.Text)) dr["LOSS_MI"] = txtLossMI.Text; // 2024.01.05 로스시간 추가

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_ABNORMAL", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgAbnormalTrayList, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgAbnormalTrayList_DoubleClick(object sender, MouseButtonEventArgs e)
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

        private void dgAbnormalTrayList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void txtLossMI_KeyDown(object sender, KeyEventArgs e)
        {
            if (!Util.isNumber(txtLossMI.Text))
            {
                txtLossMI.Text = "0";
                return;
            }
        }

        private void btnPop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FCS001_164_AGING_OUTPUT_TIME_OVER popAgingOutputPresent = new FCS001_164_AGING_OUTPUT_TIME_OVER();
                popAgingOutputPresent.FrameOperation = FrameOperation;

                if (popAgingOutputPresent != null)
                {
                    this.Dispatcher.BeginInvoke(new Action(() => popAgingOutputPresent.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
