/*************************************************************************************
 Created Date : 2018.03.06
      Creator : 
   Decription : 파우치 활성화 대차 이력 조회 
--------------------------------------------------------------------------------------
 [Change History]
  2018.03.06  DEVELOPER : Initial Created.

**************************************************************************************/

using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using C1.WPF.DataGrid;
using C1.WPF;
using System.Linq;
namespace LGC.GMES.MES.COM001
{
    public partial class COM001_220 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();

        private DataTable _inboxList;

        public COM001_220()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        private void UserControl_Initialized(object sender, EventArgs e)
        {
            Initialize();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            {
                object[] tmps = this.FrameOperation.Parameters;

                txtCtnr_Search.Text = tmps[0] as string;
             

                CartSelect();
            }

        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            SetDisplayRight();
            txtCtnr_Search.Focus();
        }

        private void InitializeGrid()
        {
            Util.gridClear(dgLotHistory);
        }

        private void InitializeUserControl()
        {
            txtCtnrID.Text = string.Empty;
            txtPrjtName.Text = string.Empty;
            txtProdID.Text = string.Empty;
            txtMKTType.Text = string.Empty;
            txtWipQltyType.Text = string.Empty;
            txtStatus.Text = string.Empty;
            txtFormWrkType.Text = string.Empty;
            txtWipPrcsType.Text = string.Empty;
            txtProcess.Text = string.Empty;
            txtEquipmentSegment.Text = string.Empty;
            txtEquipment.Text = string.Empty;
            txtCartDeleteNote.Text = string.Empty;
            txtCartSheetPrint.Text = string.Empty;
            txtInsertUser.Text = string.Empty;
            txtInsertDate.Text = string.Empty;
            txtUpdateUser.Text = string.Empty;
            txtUpdateDate.Text = string.Empty;
        }

        #endregion

        #region Event
        //조회
        private void txtCtnr_Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CartSelect();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            CartSelect();
        }

        private void btnLeftWidth_Click(object sender, RoutedEventArgs e)
        {
            SetDisplayLeft();
        }
        private void btnRightWidth_Click(object sender, RoutedEventArgs e)
        {
            SetDisplayRight();
        }

        private void dgAssyLot_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgAssyLot.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (!cell.Column.Name.Equals("CHK"))
                {
                    return;
                }

                DataTable dt = new DataTable();
                dt = DataTableConverter.Convert(dgAssyLot.ItemsSource);

                if (dt.Rows[cell.Row.Index]["CHK"].Equals(1))
                {
                    dt.Rows[cell.Row.Index]["CHK"] = 0;
                }
                else
                {
                    dt.Rows[cell.Row.Index]["CHK"] = 1;
                }

                dt.AcceptChanges();
                Util.GridSetData(dgAssyLot, dt, null, true);

                // 완성 Inbox 조회
                GetGridProductInboxList(dt);
            }

        }

        private void dgProductionInbox_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8");

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TAKEOVER_YN")).Equals("Y"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("Y"))
                    {
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgProductionInbox_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgCartHistory_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgCartHistory.GetCellFromPoint(pnt);

            if (cell != null)
            {
                SelectCartLotHistory(cell.Row.Index);
            }

        }

        #endregion

        #region Mehod
        /// <summary>
        /// Cart
        /// </summary>
        private void SetGridCartList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("CTNR_STAT_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CTNR_ID"] = txtCtnr_Search.Text;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_CTNR_PC", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgCart, dtResult, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 조립LOT List
        /// </summary>
        private void SetGridAssyLotList()
        {
            try
            {
                string bizRuleName = string.Empty;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                // Data Row
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "CTNR_ID"));
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ASSYLOT_CART_LOAD", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dtResult.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
                }
                Util.GridSetData(dgAssyLot, dtResult, null, true);

                SetDataGridColumnVisibility();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 조립LOT 완성 Inbox List
        /// </summary>
        private void SetGridProductInboxList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("ASSY_LOTID", typeof(string));
                inTable.Columns.Add("LOTSTAT", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "CTNR_ID"));
                inTable.Rows.Add(newRow);

                _inboxList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INBOX_CART_LOAD", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgProductionInbox, _inboxList, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SelectCartInfo()
        {
            try
            {
                InitializeUserControl();

                ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CTNR_ID"] = txtCtnr_Search.Text.Trim();

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_POLYMER_CTNR_INFO", "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (result != null && result.Rows.Count > 0)
                        {
                            // 대차정보
                           
                            txtPrjtName.Text = Util.NVC(result.Rows[0]["PRJT_NAME"]);
                            txtProdID.Text = Util.NVC(result.Rows[0]["PRODID"]);
                            txtMKTType.Text = Util.NVC(result.Rows[0]["MKT_TYPE_NAME"]);
                            txtWipQltyType.Text = Util.NVC(result.Rows[0]["WIP_QLTY_TYPE_NAME"]);
                            txtStatus.Text = Util.NVC(result.Rows[0]["CTNR_STAT_NAME"]);
                            txtFormWrkType.Text = Util.NVC(result.Rows[0]["FORM_WRK_TYPE_NAME"]);
                            txtWipPrcsType.Text = Util.NVC(result.Rows[0]["WIP_PRCS_TYPE_NAME"]);
                            txtProcess.Text = Util.NVC(result.Rows[0]["CURR_PROCNAME"]);
                            txtEquipmentSegment.Text = Util.NVC(result.Rows[0]["CURR_EQSGNAME"]);
                            txtEquipment.Text = Util.NVC(result.Rows[0]["CURR_EQPTNAME"]);
                            txtCartDeleteNote.Text = Util.NVC(result.Rows[0]["CART_DEL_RSN_CODE"]);
                            txtCartSheetPrint.Text = Util.NVC(result.Rows[0]["CART_SHEET_PRT_FLAG"]);
                            txtInsertUser.Text = Util.NVC(result.Rows[0]["INSUSER_NAME"]);
                            txtInsertDate.Text = Util.NVC(result.Rows[0]["INSDTTM"]);
                            txtUpdateUser.Text = Util.NVC(result.Rows[0]["UPDUSER_NAME"]);
                            txtUpdateDate.Text = Util.NVC(result.Rows[0]["UPDDTTM"]);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectCartHistory()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_PRD_SEL_POLYMER_CTNR_HIST";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CTNR_ID"] = txtCtnr_Search.Text.Trim();

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgCartHistory, result, FrameOperation, true);
                        txtCtnrID.Text = txtCtnr_Search.Text.Trim();
                        txtCtnr_Search.Text = string.Empty;
                        txtCtnr_Search.Focus();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectCartLotHistory(int row)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));
                inTable.Columns.Add("ACTDTTM", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgCartHistory.Rows[row].DataItem, "CTNR_ID"));
                newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgCartHistory.Rows[row].DataItem, "ACTID"));
                newRow["ACTDTTM"] = Util.NVC(DataTableConverter.GetValue(dgCartHistory.Rows[row].DataItem, "ACTDTTM_KEY"));

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_POLYMER_CTNR_LOT_HIST", "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgLotHistory, result, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Func
        private bool ValidationSearch()
        {
            if (string.IsNullOrWhiteSpace(txtCtnr_Search.Text))
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            return true;
        }

        private void CartSelect()
        {
            if (!ValidationSearch())
                return;

            InitializeGrid();

            SetGridCartList();
           
            if (dgCart != null && dgCart.Rows.Count > 0)
            {
                SetGridAssyLotList();
                SetGridProductInboxList();
            }

            SelectCartInfo();
            SelectCartHistory();
        }

        private void SetDisplayLeft()
        {
            if (btnLeftWidth.Tag == null || btnLeftWidth.Tag.Equals("Visible"))
            {
                gdCartInfo.Visibility = Visibility.Collapsed;
                btnLeftWidth.Tag = "Collapsed";
                btnLeftWidth.Content = "▶";
                btnLeftWidth.Background = new SolidColorBrush(Colors.Red);

                gsLeft.Visibility = Visibility.Collapsed;
            }
            else
            {
                gdCartInfo.Visibility = Visibility.Visible;
                btnLeftWidth.Tag = "Visible";
                btnLeftWidth.Content = "◀";
                btnLeftWidth.Background = new SolidColorBrush(Colors.Black);

                gsLeft.Visibility = Visibility.Visible;
            }

            SetGridWidth();
        }

        private void SetDisplayRight()
        {
            if (btnRightWidth.Tag == null || btnRightWidth.Tag.Equals("Visible"))
            {
                gdCartLotHistory.Visibility = Visibility.Collapsed;
                btnRightWidth.Tag = "Collapsed";
                btnRightWidth.Content = "▶";
                btnRightWidth.Background = new SolidColorBrush(Colors.Red);

                gsRight.Visibility = Visibility.Collapsed;
            }
            else
            {
                gdCartLotHistory.Visibility = Visibility.Visible;
                btnRightWidth.Tag = "Visible";
                btnRightWidth.Content = "◀";
                btnRightWidth.Background = new SolidColorBrush(Colors.Black);

                gsRight.Visibility = Visibility.Visible;
            }

            SetGridWidth();
        }

        private void SetGridWidth()
        {
            if (gdCartInfo.Visibility == Visibility.Collapsed && gdCartLotHistory.Visibility == Visibility.Collapsed)
            {
                grdCart.ColumnDefinitions[0].Width = new GridLength(0);
                grdCart.ColumnDefinitions[1].Width = new GridLength(0);
                grdCart.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                grdCart.ColumnDefinitions[3].Width = new GridLength(0);
                grdCart.ColumnDefinitions[4].Width = new GridLength(0);
            }
            else if (gdCartInfo.Visibility == Visibility.Collapsed)
            {
                grdCart.ColumnDefinitions[0].Width = new GridLength(0);
                grdCart.ColumnDefinitions[1].Width = new GridLength(0);
                grdCart.ColumnDefinitions[2].Width = new GridLength(8, GridUnitType.Star);
                grdCart.ColumnDefinitions[3].Width = new GridLength(5);
                grdCart.ColumnDefinitions[4].Width = new GridLength(2, GridUnitType.Star);
            }
            else if (gdCartLotHistory.Visibility == Visibility.Collapsed)
            {
                grdCart.ColumnDefinitions[0].Width = new GridLength(2, GridUnitType.Star);
                grdCart.ColumnDefinitions[1].Width = new GridLength(5);
                grdCart.ColumnDefinitions[2].Width = new GridLength(8, GridUnitType.Star);
                grdCart.ColumnDefinitions[3].Width = new GridLength(0);
                grdCart.ColumnDefinitions[4].Width = new GridLength(0);
            }
            else
            {
                grdCart.ColumnDefinitions[0].Width = new GridLength(2, GridUnitType.Star);
                grdCart.ColumnDefinitions[1].Width = new GridLength(5);
                grdCart.ColumnDefinitions[2].Width = new GridLength(6, GridUnitType.Star);
                grdCart.ColumnDefinitions[3].Width = new GridLength(5);
                grdCart.ColumnDefinitions[4].Width = new GridLength(2, GridUnitType.Star);
            }
        }

        private void SetDataGridColumnVisibility()
        {
            if (dgCart.Rows.Count > 0)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "WIP_QLTY_TYPE_CODE")).Equals("G"))
                {
                    dgProductionInbox.Columns["LOTID"].Header = ObjectDic.Instance.GetObjectName("InBox ID");

                    // 양품
                    dgAssyLot.Columns["INBOX_QTY"].Visibility = Visibility.Visible;
                    dgAssyLot.Columns["DEFECT_GROUP_LOTQTY"].Visibility = Visibility.Collapsed;

                    dgProductionInbox.Columns["VISL_INSP_USERNAME"].Visibility = Visibility.Visible;
                    dgProductionInbox.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Visible;
                    dgProductionInbox.Columns["PRINT_YN"].Visibility = Visibility.Visible;
                    dgProductionInbox.Columns["RESNGRNAME"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgProductionInbox.Columns["LOTID"].Header = ObjectDic.Instance.GetObjectName("불량그룹LOT");

                    // 불량
                    dgAssyLot.Columns["INBOX_QTY"].Visibility = Visibility.Collapsed;
                    dgAssyLot.Columns["DEFECT_GROUP_LOTQTY"].Visibility = Visibility.Visible;

                    dgProductionInbox.Columns["VISL_INSP_USERNAME"].Visibility = Visibility.Collapsed;
                    dgProductionInbox.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Collapsed;
                    dgProductionInbox.Columns["PRINT_YN"].Visibility = Visibility.Collapsed;
                    dgProductionInbox.Columns["RESNGRNAME"].Visibility = Visibility.Visible;
                }
            }
        }

        private void GetGridProductInboxList(DataTable dt)
        {
            DataTable dtinbox = _inboxList.Copy();
            DataRow[] drDel = DataTableConverter.Convert(dgAssyLot.ItemsSource).Select("CHK = 0");

            foreach (DataRow rowdel in drDel)
            {
                dtinbox.Select("ASSY_LOTID = '" + Util.NVC(rowdel["ASSY_LOTID"]) + "'").ToList<DataRow>().ForEach(row => row.Delete());
            }
            dtinbox.AcceptChanges();

            Util.GridSetData(dgProductionInbox, dtinbox, FrameOperation, true);
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

