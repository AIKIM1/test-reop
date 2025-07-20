/*************************************************************************************
 Created Date : 2020.10.08
      Creator : 신 광희
   Decription : CWA3동 증설 - V/D공정진척(Redry / Rewinding) - 장비완료(ASSY004_001_EQPTEND 소스 카피 후 작성)
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.08  신 광희 : Initial Created.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Linq;
using C1.WPF.DataGrid;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_008_EQPTEND.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_008_EQPTEND : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _processCode = string.Empty;
        private string _equipmentSegmentCode = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _lotId = string.Empty;
        private string _wipSeq = string.Empty;
        private string _wipState = string.Empty;
        private string _equipmentMountPositionId = string.Empty;
        private string _workType = string.Empty;

        private string _IRREGL_PROD_LOT_TYPE_CODE = string.Empty;
        private string _PROD_LOT_OPER_MODE = string.Empty;

        private bool _productQtyChangePermission = false;
        private bool _isShowMessage = false;
        private bool _isSaved = false;
        private DateTime dtNow;
        private string sCaldate = string.Empty;
        private DateTime dtCaldate;

        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;

        private readonly Util _util = new Util();

        private BizDataSet _bizDataSet = new BizDataSet();

        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY004_008_EQPTEND()
        {
            InitializeComponent();
        }

        private void InitializeGrid()
        {
            DataTable dtTmp = new DataTable();

            dtTmp.Columns.Add("M_RMN_QTY", typeof(int));
            dtTmp.Columns.Add("M_WIPQTY", typeof(int));
            dtTmp.Columns.Add("WIPQTY", typeof(int));
            dtTmp.Columns.Add("GOODQTY", typeof(int));
            dtTmp.Columns.Add("M_CURR_PROC_LOSS_QTY", typeof(int));  //자공정LOSS
            dtTmp.Columns.Add("M_FIX_LOSS_QTY", typeof(int));
            dtTmp.Columns.Add("UNIDENTIFIED_QTY", typeof(int));
            dtTmp.Columns.Add("M_PRE_PROC_LOSS_QTY", typeof(int));
            dtTmp.Columns.Add("DTL_DEFECT_LOT", typeof(int));
            dtTmp.Columns.Add("DTL_LOSS_LOT", typeof(int));
            dtTmp.Columns.Add("DTL_CHARGE_PROD_LOT", typeof(int));
            dtTmp.Columns.Add("DFCT_SUM", typeof(int));
            dtTmp.Columns.Add("NOTCH_REWND_TAB_COUNT_QTY", typeof(int));
            dtTmp.Columns.Add("FIX_LOSS_QTY", typeof(int));
            dtTmp.Columns.Add("EQPT_INPUT_QTY", typeof(int));
            dtTmp.Columns.Add("EQPT_END_QTY", typeof(int));

            dtTmp.Columns.Add("M_LOTID", typeof(string));
            dtTmp.Columns.Add("M_CSTID", typeof(string));

            DataRow dtRow = dtTmp.NewRow();
            dtRow["M_RMN_QTY"] = 0;
            dtRow["M_WIPQTY"] = 0;
            dtRow["WIPQTY"] = 0;
            dtRow["GOODQTY"] = 0;
            dtRow["M_CURR_PROC_LOSS_QTY"] = 0;   //자공정LOSS
            dtRow["M_FIX_LOSS_QTY"] = 0;
            dtRow["UNIDENTIFIED_QTY"] = 0;
            dtRow["M_PRE_PROC_LOSS_QTY"] = 0;
            dtRow["DTL_DEFECT_LOT"] = 0;
            dtRow["DTL_LOSS_LOT"] = 0;
            dtRow["DTL_CHARGE_PROD_LOT"] = 0;
            dtRow["DFCT_SUM"] = 0;
            dtRow["NOTCH_REWND_TAB_COUNT_QTY"] = 0;
            dtRow["M_LOTID"] = "";
            dtRow["M_CSTID"] = "";

            dtRow["FIX_LOSS_QTY"] = 0;
            dtRow["EQPT_INPUT_QTY"] = 0;
            dtRow["EQPT_END_QTY"] = 0;

            dtTmp.Rows.Add(dtRow);

            dgQty.ItemsSource = DataTableConverter.Convert(dtTmp);
        }

        private void InitializeControls()
        {
            dtNow = DateTime.Now;

            if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                lblCstID.Visibility = Visibility.Visible;
                txtCstID.Visibility = Visibility.Visible;
            }
            else
            {
                lblCstID.Visibility = Visibility.Collapsed;
                txtCstID.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 9)
            {
                _processCode = Util.NVC(tmps[0]);
                _equipmentSegmentCode = Util.NVC(tmps[1]);
                _equipmentCode = Util.NVC(tmps[2]);
                _lotId = Util.NVC(tmps[3]);
                _wipSeq = Util.NVC(tmps[4]);
                _LDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[5]);
                _UNLDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[6]);
                _equipmentMountPositionId = Util.NVC(tmps[7]);
                _productQtyChangePermission = (bool)tmps[8];
                _workType = Util.NVC(tmps[9]);
            }

            ApplyPermissions();
            InitializeControls();
            GetAllData();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            InitializeGrid();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (_isShowMessage)
                return;

            if (_isSaved)
                this.DialogResult = MessageBoxResult.OK;
            else
                this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            double dUnidentifiedQty = 0;
            if (!ValidationSave(out dUnidentifiedQty))
                return;

            string messageCode = dUnidentifiedQty < 0 ? "SFU3746" : "SFU1865";

            Util.MessageConfirm(messageCode, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save();
                }
            });
        }

        private void dgQty_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null) return;

                C1DataGrid grd = sender as C1DataGrid;

                grd.EndEdit();
                grd.EndEditRow(true);

                //CalcSumQty();

                //double dRmnQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "M_RMN_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "M_RMN_QTY")));
                double dInputQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "M_WIPQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "M_WIPQTY")));
                double dPunchQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "WIPQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "WIPQTY")));
                double dGoodQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "GOODQTY")));
                double dFixLossQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "M_FIX_LOSS_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "M_FIX_LOSS_QTY")));
                double dDefectQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "DFCT_SUM")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "DFCT_SUM")));
                //자공정 LOSS
                double dCurrProdLossQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "M_CURR_PROC_LOSS_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "M_CURR_PROC_LOSS_QTY")));

                double dUnidentifiedQty = dPunchQty - dGoodQty - dDefectQty;
                double dLengLEQty = dInputQty - dFixLossQty - dPunchQty - dCurrProdLossQty; //자공정LOSS 추가
                double dRmnQty = dInputQty - dPunchQty;

                DataTableConverter.SetValue(grd.Rows[grd.TopRows.Count].DataItem, "UNIDENTIFIED_QTY", dUnidentifiedQty);
                grd.UpdateLayout();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                {
                    this.Focus();
                    return;
                }

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (sCaldate.Equals(""))
                {
                    this.Focus();
                    return;
                }

                if ((Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) - 1 > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))) ||
                    (Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) + 1 < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))))
                {
                    dtPik.Text = dtCaldate.ToLongDateString();
                    dtPik.SelectedDateTime = dtCaldate;

                    Util.MessageValidation("SFU1669");  // 선택할 수 없습니다.
                    //e.Handled = false;
                    return;
                }

                this.Focus();
            }));
        }

        private void dgQty_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    //Grid Data Binding 이용한 Background 색 변경
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (_productQtyChangePermission && e.Cell.Column.Name.Equals("GOODQTY"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFF8F"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                        //if (e.Cell.Column.Name.Equals("NOTCH_REWND_TAB_COUNT_QTY"))
                        //{
                        //    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F6F6F6"));
                        //}

                        if (e.Cell.Column.Name.Equals("FIX_LOSS_QTY") || e.Cell.Column.Name.Equals("EQPT_INPUT_QTY") || e.Cell.Column.Name.Equals("EQPT_END_QTY"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F6F6F6"));
                        }

                    }


                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void dgQty_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgQty_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid grd = sender as C1DataGrid;

            if ((bool)e.NewValue == false)
                grd.EndEdit();
        }

        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveDefect())
                return;

            //불량정보를 저장 하시겠습니까?
            Util.MessageConfirm("SFU1587", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetDefect();
                }
            });
        }

        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                #region 미확인 Loss 음수 여부 Validation
                if (e == null || e.Cell == null)
                    return;

                if (sender == null) return;

                C1DataGrid grd = sender as C1DataGrid;

                int idx = _util.GetDataGridRowIndex(grd, "PRCS_ITEM_CODE", "UNIDENTIFIED_QTY");
                if (idx >= 0)
                {
                    // 미확인 Loss = 투입수(타발수) - 양품수 - 불량Sum(실적제외 및 미확인LOSS 항목 제외) - 구간잔량
                    DataTable dtSrc = DataTableConverter.Convert(dgDefect.ItemsSource);
                    double dDefectQty = dtSrc.Compute("SUM(RESNQTY)", "RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(RESNQTY)", "RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString());

                    double dPunchQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "WIPQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "WIPQTY")));
                    double dGoodQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")));
                    
                    double dUnidentifiedQty = dPunchQty - dGoodQty - dDefectQty;

                    if (dUnidentifiedQty < 0)
                    {
                        // 입력오류 : 입력한 불량으로 인해 미확인LOSS가 음수가 됩니다.
                        Util.MessageValidation("SFU6035", (action) => { _isShowMessage = false; });

                        grd.EndEdit();
                        grd.EndEditRow(true);

                        DataTableConverter.SetValue(grd.Rows[e.Cell.Row.Index].DataItem, "RESNQTY", 0);
                        
                        grd.UpdateLayout();
                        _isShowMessage = true;
                    }
                }
                #endregion

                SumDefectQty();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDefect_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                if (e.Column.Name.Equals("RESNQTY"))
                {
                    string sFlag = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                    if (sFlag == "Y")
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (!Util.NVC(e.Cell.Column.Name).Equals("ACTNAME"))
                    {
                        string sFlag = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                        if (sFlag == "Y")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D4D4D4"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;// new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }
                }
            }));
        }

        private void dgDefect_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgDefect_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid grd = sender as C1DataGrid;

            if ((bool)e.NewValue == false)
                grd.EndEdit();
        }


        private void dgDefect_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            if (dgQty?.Rows?.Count > 0)
                dgQty.EndEdit(true);
        }

        private void dgQty_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            try
            {
                if (dgDefect?.Rows?.Count > 0)
                {
                    dgDefect.EndEdit(true);

                    int idx = _util.GetDataGridRowIndex(dgDefect, "PRCS_ITEM_CODE", "UNIDENTIFIED_QTY");
                    if (idx >= 0 && dgDefect.CurrentCell.Row.Index == idx)
                    {
                        int iTmpRow = dgDefect.GetRowCount() - 2 > idx ? idx + 1 : idx - 1;

                        C1.WPF.DataGrid.DataGridCell dgcTmp = dgDefect.GetCell(iTmpRow, dgDefect.Columns.Count - 1);

                        if (dgcTmp != null)
                            dgDefect.CurrentCell = dgcTmp;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod

        #region [BizCall]
        private void GetLotInfo()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_CONFIRM_LOT_INFO_VD_L";

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = _lotId;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                inTable.Rows.Add(newRow);

                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONFIRM_LOT_INFO_NT_L", "INDATA", "OUTDATA", inTable);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dgQty.GetRowCount() > 0)
                    {
                        if (dgQty.Columns.Contains("M_WIPQTY") && dtRslt.Columns.Contains("M_WIPQTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_WIPQTY", dtRslt.Rows[0]["M_WIPQTY"]);
                        if (dgQty.Columns.Contains("M_PRE_PROC_LOSS_QTY") && dtRslt.Columns.Contains("M_PRE_PROC_LOSS_QTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_PRE_PROC_LOSS_QTY", dtRslt.Rows[0]["M_PRE_PROC_LOSS_QTY"]);

                         // 자공정 LOSS 추가
                        if (dgQty.Columns.Contains("M_CURR_PROC_LOSS_QTY") && dtRslt.Columns.Contains("M_CURR_PROC_LOSS_QTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_CURR_PROC_LOSS_QTY", dtRslt.Rows[0]["M_CURR_PROC_LOSS_QTY"]);


                        if (dgQty.Columns.Contains("M_FIX_LOSS_QTY") && dtRslt.Columns.Contains("M_FIX_LOSS_QTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_FIX_LOSS_QTY", dtRslt.Rows[0]["M_FIX_LOSS_QTY"]);
                        if (dgQty.Columns.Contains("M_RMN_QTY") && dtRslt.Columns.Contains("M_RMN_QTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_RMN_QTY", dtRslt.Rows[0]["M_RMN_QTY"]);

                        if (dgQty.Columns.Contains("WIPQTY") && dtRslt.Columns.Contains("WIPQTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "WIPQTY", dtRslt.Rows[0]["WIPQTY"]);

                        if (dgQty.Columns.Contains("GOODQTY") && dtRslt.Columns.Contains("GOODQTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY", dtRslt.Rows[0]["GOODQTY"]);

                        if (dgQty.Columns.Contains("NOTCH_REWND_TAB_COUNT_QTY") && dtRslt.Columns.Contains("NOTCH_REWND_TAB_COUNT_QTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "NOTCH_REWND_TAB_COUNT_QTY", dtRslt.Rows[0]["NOTCH_REWND_TAB_COUNT_QTY"]);

                        if (dgQty.Columns.Contains("FIX_LOSS_QTY") && dtRslt.Columns.Contains("FIX_LOSS_QTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "FIX_LOSS_QTY", dtRslt.Rows[0]["FIX_LOSS_QTY"]);

                        if (dgQty.Columns.Contains("EQPT_INPUT_QTY") && dtRslt.Columns.Contains("EQPT_INPUT_QTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "EQPT_INPUT_QTY", dtRslt.Rows[0]["EQPT_INPUT_QTY"]);

                        if (dgQty.Columns.Contains("EQPT_END_QTY") && dtRslt.Columns.Contains("EQPT_END_QTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "EQPT_END_QTY", dtRslt.Rows[0]["EQPT_END_QTY"]);

                        if (dgQty.Columns.Contains("M_LOTID") && dtRslt.Columns.Contains("M_LOTID"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_LOTID", dtRslt.Rows[0]["M_LOTID"]);
                        if (dgQty.Columns.Contains("M_CSTID") && dtRslt.Columns.Contains("M_CSTID"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_CSTID", dtRslt.Rows[0]["M_CSTID"]);
                    }

                    txtLotId.Text = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                    txtCstID.Text = Util.NVC(dtRslt.Rows[0]["CSTID"]);
                    txtProdId.Text = Util.NVC(dtRslt.Rows[0]["PRODID"]);
                    txtProjectName.Text = Util.NVC(dtRslt.Rows[0]["PRJT_NAME"]);
                    txtStartTime.Text = Util.NVC(dtRslt.Rows[0]["WIPDTTM_ST"]);
                    _IRREGL_PROD_LOT_TYPE_CODE = Util.NVC(dtRslt.Rows[0]["IRREGL_PROD_LOT_TYPE_CODE"]);

                    // Caldate Lot의 Caldate로...
                    if (Util.NVC(dtRslt.Rows[0]["CALDATE_LOT"]).Trim().Equals(""))
                    {
                        dtpCaldate.Text = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["NOW_CALDATE"])).ToLongDateString();
                        dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["NOW_CALDATE"]));

                        sCaldate = Util.NVC(dtRslt.Rows[0]["NOW_CALDATE_YMD"]);
                        dtCaldate = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["NOW_CALDATE"]));
                    }
                    else
                    {
                        dtpCaldate.Text = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["CALDATE_LOT"])).ToLongDateString();
                        dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["CALDATE_LOT"]));

                        sCaldate = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["CALDATE_LOT"])).ToString("yyyyMMdd");
                        dtCaldate = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["CALDATE_LOT"]));
                    }
                }
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void Save()
        {
            const string bizRuleName = "BR_PRD_REG_EQPT_END_LOT_VD_R2R_L";

            try
            {
                ShowLoadingIndicator();
                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("WORK_TYPE", typeof(string));

                inTable = indataSet.Tables.Add("IN_INPUT");
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));

                inTable = indataSet.Tables.Add("IN_OUTPUT");
                inTable.Columns.Add("OUT_CSTID", typeof(string));
                inTable.Columns.Add("OUT_LOTID", typeof(string));
                inTable.Columns.Add("INPUT_QTY", typeof(decimal));
                inTable.Columns.Add("OUTPUT_QTY", typeof(decimal));
                inTable.Columns.Add("RWND_TAB_COUNT_QTY", typeof(decimal));

                inTable = indataSet.Tables.Add("IN_DEFFECT");
                inTable.Columns.Add("EQPT_DFCT_CODE", typeof(string));
                inTable.Columns.Add("DFCT_QTY", typeof(decimal));

                DataTable dtDataTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = dtDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WORK_TYPE"] = _workType;
                dtDataTable.Rows.Add(newRow);
                newRow = null;

                if (dgQty.GetRowCount() > 0)
                {
                    string equipmentMountPositionId = GetEquipmentMountPositionId(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_LOTID"))) ;

                    DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                    newRow = inInputTable.NewRow();

                    if (!string.IsNullOrEmpty(equipmentMountPositionId))
                    {
                        newRow["EQPT_MOUNT_PSTN_ID"] = equipmentMountPositionId;
                    }
                    newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                    newRow["INPUT_LOTID"] = _lotId;

                    if (_LDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                    {
                        newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_CSTID"));
                    }
                    inInputTable.Rows.Add(newRow);
                    newRow = null;

                    DataTable inOutputTable = indataSet.Tables["IN_OUTPUT"];
                    newRow = inOutputTable.NewRow();
                    newRow["OUT_LOTID"] = txtLotId.Text;

                    if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                    {
                        newRow["OUT_CSTID"] = txtCstID.Text;
                    }

                    newRow["INPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "WIPQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "WIPQTY")));
                    //newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")));
                    newRow["OUTPUT_QTY"] = GetOutPutQty(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "WIPQTY")), Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "DFCT_SUM")));
                    newRow["RWND_TAB_COUNT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "NOTCH_REWND_TAB_COUNT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "NOTCH_REWND_TAB_COUNT_QTY")));
                    inOutputTable.Rows.Add(newRow);
                    newRow = null;

                    if (CommonVerify.HasDataGridRow(dgDefect))
                    {
                        DataTable inDefectTable = indataSet.Tables["IN_DEFFECT"];
                        newRow = inDefectTable.NewRow();

                        /*
                        DataTable dt = ((DataView)dgDefect.ItemsSource).Table;
                        //var query = (from t in dt.AsEnumerable()
                        //    where t.Field<string>("ACTID") == "DEFECT_LOT"
                        //          && t.Field<string>("RESNNAME") == "NG Tag"
                        //          && t.Field<string>("RESNCODE") == "MAA49A74007"
                        //    select new
                        //    {
                        //        ResnQty = t.Field<decimal>("RESNQTY")
                        //    }).FirstOrDefault();

                        var query = (from t in dt.AsEnumerable()
                            where t.Field<string>("RESNCODE") == "MAA49A74007"
                            select new
                            {
                                EquipmentDefactCode = t.Field<string>("RESNCODE"),
                                DefectQty = t.Field<decimal>("RESNQTY")
                            }).FirstOrDefault();

                        if (query != null)
                        {
                            newRow["EQPT_DFCT_CODE"] = query.EquipmentDefactCode;
                            newRow["DFCT_QTY"] = query.DefectQty;
                            inDefectTable.Rows.Add(newRow);
                        }
                        */
                    }
                }

                //string xml = indataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_INPUT,IN_OUTPUT,IN_DEFECT", "OUT_LOT", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        btnSave.IsEnabled = false;

                        Util.MessageInfo("SFU1275");
                        _isSaved = true;
                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }

        }

        private decimal GetOutPutQty(string wipQty, string defectqty)
        {
            
            decimal a = string.IsNullOrEmpty(wipQty) ? 0 : wipQty.GetDecimal();
            decimal b = string.IsNullOrEmpty(defectqty) ? 0 : defectqty.GetDecimal();

            return a - b;
        }

        private void SetDefect(bool bMsgShow = true)
        {
            try
            {
                ShowLoadingIndicator();

                dgDefect.EndEdit();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable inDEFECT_LOT = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = null;

                    newRow = inDEFECT_LOT.NewRow();
                    newRow["LOTID"] = txtLotId.Text.Trim();
                    newRow["WIPSEQ"] = _wipSeq;
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNCODE"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")));
                    newRow["RESNCODE_CAUSE"] = "";
                    newRow["PROCID_CAUSE"] = "";
                    newRow["RESNNOTE"] = "";
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = "";
                    }

                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;

                    inDEFECT_LOT.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, indataSet);

                if (bMsgShow)
                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                GetDefectInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void GetDefectInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _processCode;
                newRow["EQPTID"] = _equipmentCode;
                newRow["LOTID"] = _lotId;
                newRow["WIPSEQ"] = _wipSeq;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPRESONCOLLECT_L", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgDefect, searchResult, FrameOperation, false);

                SumDefectQty();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private string GetEquipmentMountPositionId(string sInputLot)
        {   
            try
            {
                string returnValue = string.Empty;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _equipmentCode;
                newRow["INPUT_LOTID"] = sInputLot;
               
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL_INPUT_LOTID_CNT", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    returnValue = Util.NVC(dtRslt.Rows[0]["EQPT_MOUNT_PSTN_ID"]);
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }
        #endregion

        #region [Validation]
        private bool ValidationSave(out double dUnidentifiedQty)
        {
            dUnidentifiedQty = 0;

            if (_isShowMessage) return false;

            foreach (C1.WPF.DataGrid.DataGridRow row in dgDefect.Rows)
            {
                double dRsn, dOrgRsn = 0;

                double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                if (dRsn != dOrgRsn)
                {
                    // 저장하지 않은 불량 정보가 있습니다.
                    Util.MessageValidation("SFU1878");
                    return false;
                }
            }

            double dInputQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "WIPQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "WIPQTY")));
            double dGoodQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")));
            dUnidentifiedQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "UNIDENTIFIED_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "UNIDENTIFIED_QTY")));

            return true;
        }

        private bool ValidationSaveDefect()
        {
            if (_isShowMessage) return false;

            if (!CommonVerify.HasDataGridRow(dgDefect))
            {
                //Util.Alert("불량 항목이 없습니다.");
                Util.MessageValidation("SFU1578");
                return false;
            }

            if (txtLotId.Text.Trim().Length < 1)
            {
                //Util.Alert("LOT 정보가 없습니다.");
                Util.MessageValidation("SFU1195");
                return false;
            }

            return true;
        }
        #endregion

        #region [Func]
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void GetAllData()
        {
            ClearControls();

            InitializeGrid();
            GetLotInfo();

            GetDefectInfo();
        }

        private void ClearControls()
        {
            Util.gridClear(dgQty);
            Util.gridClear(dgDefect);

            txtLotId.Text = string.Empty;
            txtCstID.Text = string.Empty;
            txtProdId.Text = string.Empty;
            //txtLotType.Text = "";
            txtStartTime.Text = string.Empty;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            listAuth.Add(btnDefectSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            if (_productQtyChangePermission)
            {
                dgQty.Columns["M_FIX_LOSS_QTY"].IsReadOnly = false;
                dgQty.Columns["GOODQTY"].IsReadOnly = false;
            }
            else
            {
                dgQty.Columns["M_FIX_LOSS_QTY"].IsReadOnly = true;
                dgQty.Columns["GOODQTY"].IsReadOnly = true;
            }
        }


        private void CalcSumQty(DataTable dtTmp)
        {
            try
            {
                if (dtTmp == null || dtTmp.Rows.Count < 1) return;

                //double dRmnQty = Util.NVC(dtTmp.Rows[0]["M_RMN_QTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["M_RMN_QTY"]));
                double dInputQty = Util.NVC(dtTmp.Rows[0]["M_WIPQTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["M_WIPQTY"]));
                double dPunchQty = Util.NVC(dtTmp.Rows[0]["WIPQTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["WIPQTY"]));
                double dGoodQty = Util.NVC(dtTmp.Rows[0]["GOODQTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["GOODQTY"]));
                double dFixLossQty = Util.NVC(dtTmp.Rows[0]["M_FIX_LOSS_QTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["M_FIX_LOSS_QTY"]));
                double dDefectQty = Util.NVC(dtTmp.Rows[0]["DFCT_SUM"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["DFCT_SUM"]));
                //자공정 Loss
                double dCurrProdLossQty = Util.NVC(dtTmp.Rows[0]["M_CURR_PROC_LOSS_QTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["M_CURR_PROC_LOSS_QTY"]));


                double dUnidentifiedQty = dPunchQty - dGoodQty - dDefectQty;
                double dLengLEQty = dInputQty - dFixLossQty - dPunchQty - dCurrProdLossQty; // 자공정 LOSS 추가
                double dRmnQty = dInputQty - dPunchQty;

                dtTmp.Rows[0]["UNIDENTIFIED_QTY"] = dUnidentifiedQty;

                dgQty.ItemsSource = DataTableConverter.Convert(dtTmp);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SumDefectQty()
        {
            try
            {
                DataTable dtSrc = DataTableConverter.Convert(dgDefect.ItemsSource);
                DataTable dtTgt = DataTableConverter.Convert(dgQty.ItemsSource);

                if (dtTgt == null || dtTgt.Rows.Count < 1) return;

                if (dtSrc != null && dtSrc.Rows.Count > 0)
                {
                    dtTgt.Rows[0]["DFCT_SUM"] = dtSrc.Compute("SUM(RESNQTY)", "RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(RESNQTY)", "RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString());
                    dtTgt.Rows[0]["DTL_DEFECT_LOT"] = dtSrc.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString());
                    dtTgt.Rows[0]["DTL_LOSS_LOT"] = dtSrc.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT' AND RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT' AND RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString());
                    dtTgt.Rows[0]["DTL_CHARGE_PROD_LOT"] = dtSrc.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT' AND RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT' AND RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString());
                }
                else
                {
                    dtTgt.Rows[0]["DFCT_SUM"] = 0;
                    dtTgt.Rows[0]["DTL_DEFECT_LOT"] = 0;
                    dtTgt.Rows[0]["DTL_LOSS_LOT"] = 0;
                    dtTgt.Rows[0]["DTL_CHARGE_PROD_LOT"] = 0;
                }

                CalcSumQty(dtTgt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        
    }
}
