/*************************************************************************************
 Created Date : 2020.12.22
      Creator : 박수미
   Decription : 출하 예정일 조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.22  NAME : Initial Created
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
    /// <summary>
    /// TSK_710.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_041 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        #endregion

        #region [Initialize]
        public FCS001_041()
        {
            InitializeComponent();
            //Combo Setting
            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            //공정경로 별 조회
            C1ComboBox[] cboLineChild = { cboModel };
            ComCombo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelChild = { cboRoute };
            C1ComboBox[] cboModelParent = { cboLine };
            ComCombo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            ComCombo.SetCombo(cboRoute, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent);

        }
        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                Util.gridClear(dgShipPlanDate);
                for (int i = 1; i < dgShipPlanDate.Columns.Count; i++)
                {
                    dgShipPlanDate.Columns[i].Visibility = Visibility.Visible;
                }
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                if (!string.IsNullOrEmpty(txtLotId.Text)) dr["PROD_LOTID"] = Util.NVC(txtLotId.Text);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SHIPMENT_DATE", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgShipPlanDate, dtRslt, this.FrameOperation, true);

                //0인 컬럼 숨기기 Start
                if (dgShipPlanDate.Rows.Count > 1)
                {
                    for (int i = 1; i < dgShipPlanDate.Columns.Count; i++)
                    {
                        if (dgShipPlanDate[dgShipPlanDate.Rows.Count - 1, i].Text == "0")
                        {
                            dgShipPlanDate.Columns[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
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

        private void dgShipPlanDate_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
                if (cell.Value.Equals("0")) return;
                if (cell.Row.Index == datagrid.Rows.Count - 1) return;

                FCS001_042 FCS001_042 = new FCS001_042();
                FCS001_042.FrameOperation = FrameOperation;

                object[] Parameters = new object[10];
                Parameters[0] = Util.NVC(txtLotId.Text);//LOT_ID
                Parameters[1] = Util.GetCondition(cboLine); //LINE_ID
                Parameters[2] = Util.GetCondition(cboModel); //MODEL_ID
                Parameters[3] = Util.GetCondition(cboRoute); // ROUTE_ID
                Parameters[4] = Util.NVC(cell.Column.Tag);
                Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgShipPlanDate.CurrentRow.DataItem, "SHIPING_DATE")); //SHIPING_DATE

                this.FrameOperation.OpenMenuFORM("FCS001_042", "FCS001_042", "LGC.GMES.MES.FCS001", ObjectDic.Instance.GetObjectName("출하 예정 Tray List"), true, Parameters);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgShipPlanDate_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(e.Cell.Column.Name.Substring(0, 1)).Equals("A") &&
                        !Util.NVC(e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Text).Equals("0"))
                    {
                        e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                }
            }));
        }

        private void dgShipPlanDate_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {

            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();
            if (e.Row.Index != dgShipPlanDate.Rows.Count - 1) //마지막 RowHeader 표시 x
            {
                tb.Text = (e.Row.Index + 1 - dgShipPlanDate.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        #endregion
    }
}
