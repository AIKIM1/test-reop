/*************************************************************************************
 Created Date : 2020.12.22
      Creator : 박수미
   Decription : 출하 예정 Tray List
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.22  NAME : Initial Created
  2022.02.23  KDH : AREA 조건 추가
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
    public partial class FCS001_042 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]


        #endregion

        #region [Initialize]
        public FCS001_042()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] parameters = this.FrameOperation.Parameters;
                if (parameters != null && parameters.Length >= 1)
                {
                    txtLotID.Text = Util.NVC(parameters[0]);
                    txtLine.Text = Util.NVC(parameters[1]);
                    txtModel.Text = Util.NVC(parameters[2]);
                    txtRout.Text = Util.NVC(parameters[3]);
                    txtRoutOp.Text = Util.NVC(parameters[4]);
                    txtDate.Text = Util.NVC(parameters[5]);
                }
                GetList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("SHIPPING_DATE", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string)); //2022.02.23_AREA 조건 추가

                DataRow dr = dtRqst.NewRow();
                if (!string.IsNullOrEmpty(txtLine.Text)) dr["EQSGID"] = Util.NVC(txtLine.Text);
                if (!string.IsNullOrEmpty(txtModel.Text)) dr["MDLLOT_ID"] = Util.NVC(txtModel.Text);
                if (!string.IsNullOrEmpty(txtRout.Text)) dr["ROUTID"] = Util.NVC(txtRout.Text);
                if (!string.IsNullOrEmpty(txtRoutOp.Text)) dr["PROCID"] = Util.NVC(txtRoutOp.Text);
                if (!string.IsNullOrEmpty(txtLotID.Text)) dr["PROD_LOTID"] = Util.NVC(txtLotID.Text);
                dr["SHIPPING_DATE"] = Util.NVC(txtDate.Text).Replace("-", "");
                dr["AREAID"] = LoginInfo.CFG_AREA_ID; //2022.02.23_AREA 조건 추가
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SHIPING_PLAN_TRAY_LIST", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgTrayList, dtRslt, this.FrameOperation);
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

        private void dgTrayList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
                if (cell.Column.Name.Equals("CSTID"))
                {
                    int Row = cell.Row.Index;

                    object[] Parameters = new object[10];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[Row].DataItem, "CSTID")); //TrayID
                    Parameters[1] = "";  //Tray NO
                    //FIN_CD 넘기기
                    Parameters[2] = null;   //FinCD       
                    Parameters[3] = "true"; //FinCheck
                    Parameters[4] = null; //EQPID

                    this.FrameOperation.OpenMenu("SFU010710010", true, Parameters);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgTrayList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgTrayList.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;
            }
            ));
            if (e.Cell.Column.Name.Equals("CSTID"))
            {
                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
            }

        }

        private void dgTrayList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgTrayList.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }
        #endregion

    }
}
