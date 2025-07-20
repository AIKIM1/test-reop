/*************************************************************************************
 Created Date : 2023.02.08
      Creator : Kang Dong Hee
   Decription : 충방전기 Box별 불량률
--------------------------------------------------------------------------------------
 [Change History]
  2023.02.08  NAME : Initial Created
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
    /// <summary>
    /// TSK_120.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_073 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;

        #endregion
        #region [Initialize]
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS002_073()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetWorkResetTime();
            InitCombo();
            InitControl();

            this.Loaded -= UserControl_Loaded;
        }

        private void InitCombo()
        {
            try
            {
                CommonCombo_Form_MB ComCombo = new CommonCombo_Form_MB();

                C1ComboBox[] cboLaneChild = { cboEqp };
                string[] sFilterLane = { "", "1" };
                ComCombo.SetCombo(cboLane, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "LANE", cbChild: cboLaneChild, sFilter: sFilterLane);

                string[] sFilterEqpType = { "EQPT_GR_TYPE_CODE", "1" };
                C1ComboBox[] cboEqpKindChild = { cboEqp };
                ComCombo.SetCombo(cboEqpKind, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "FORM_CMN", sFilter: sFilterEqpType, cbChild: cboEqpKindChild);

                C1ComboBox[] cboEqpParent = { cboLane, cboEqpKind };
                ComCombo.SetCombo(cboEqp, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "EQPIDBYLANE", cbParent: cboEqpParent);

                C1ComboBox[] cboLineChild = { cboModel, cboRoute };
                ComCombo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

                C1ComboBox[] cboModelParent = { cboLine };
                C1ComboBox[] cboModelChild = { cboRoute };
                ComCombo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent, cbChild: cboModelChild);

                C1ComboBox[] cboRouteParent = { cboLine, cboModel };
                ComCombo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitControl()
        {
            try
            {
                // Util 에 해당 함수 추가 필요.
                dtpFromDate.SelectedDateTime = GetJobDateFrom();
                dtpFromTime.DateTime = GetJobDateFrom();
                dtpToDate.SelectedDateTime = GetJobDateTo();
                dtpToTime.DateTime = GetJobDateTo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private DateTime GetJobDateFrom(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkReSetTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkReSetTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private DateTime GetJobDateTo(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkEndTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private void SetWorkResetTime()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREAATTR_FOR_OPENTIME", "RQSTDT", "RSLTDT", dtRqst);
            sWorkReSetTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
            sWorkEndTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
        }
        #endregion

        #region [Event]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
                    this.FrameOperation.OpenMenu("SFU010710300", true, parameters); //Tray 정보조회
                }
                else if (datagrid.CurrentColumn.Index >= datagrid.Columns["A_GRD_QTY"].Index
                         && !cell.Value.ToString().Equals("0"))
                {
                    FCS002_073_GRADE_DETAIL wndConfirm = new FCS002_073_GRADE_DETAIL();
                    wndConfirm.FrameOperation = FrameOperation;

                    if (wndConfirm != null)
                    {
                        object[] Parameters = new object[3];
                        Parameters[0] = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["PROD_LOTID"].Index).Value);
                        Parameters[1] = Util.GetCondition(cboEqp, bAllNull: true);
                        Parameters[2] = Util.NVC(datagrid.GetCell(1, datagrid.CurrentColumn.Index).Value);

                        C1WindowExtension.SetParameters(wndConfirm, Parameters);

                        grdMain.Children.Add(wndConfirm);
                        wndConfirm.BringToFront();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                if (e.Cell.Column.Name.Equals("CSTID") || e.Cell.Column.Name.Equals("LOTID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else if (e.Cell.Column.Index >= dataGrid.Columns["A_GRD_QTY"].Index && !e.Cell.Value.ToString().Equals("0"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                }
            }));
        }

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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
        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("S71", typeof(string)); //LANE_ID
                dtRqst.Columns.Add("S70", typeof(string)); //EQP_TYPE_CODE
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("START_TIME", typeof(string));
                dtRqst.Columns.Add("END_TIME", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("DELETE_YN", typeof(string));
                dtRqst.Columns.Add("HISTORY_YN", typeof(string));
                dtRqst.Columns.Add("CURRENT_YN", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S71"] = Util.GetCondition(cboLane);
                dr["S70"] = Util.GetCondition(cboEqpKind);
                dr["EQPTID"] = (Util.GetCondition(cboEqp) != "NA") ? Util.GetCondition(cboEqp) : null;

                dr["START_TIME"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpFromTime.DateTime.Value.ToString(" HH:mm:00");
                dr["END_TIME"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpToTime.DateTime.Value.ToString(" HH:mm:00");

                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                if(!string.IsNullOrEmpty(txtLotID.Text)) dr["PROD_LOTID"] = txtLotID.Text;

                if (chkHistry.IsChecked == true)
                {
                    dr["HISTORY_YN"] = "Y";
                    dr["CURRENT_YN"] = null;
                }
                else
                {
                    dr["HISTORY_YN"] = null;
                    dr["CURRENT_YN"] = "Y";
                }
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOAD_TRAY_LIST_MB", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgList, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion


    }
}
