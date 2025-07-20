/*************************************************************************************
 Created Date : 2019.04.18
      Creator : INS 김동일K
   Decription : CWA3동 증설 - 조립 공정 공통 - 장비완료
--------------------------------------------------------------------------------------
 [Change History]
  2019.04.18  INS 김동일K : Initial Created.

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
using C1.WPF.DataGrid;
using System.Windows.Media;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_COM_EQPTEND.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_COM_EQPTEND : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _ProcID = string.Empty;
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private string _WipStat = string.Empty;
        private string _IRREGL_PROD_LOT_TYPE_CODE = string.Empty;
        private string _PROD_LOT_OPER_MODE = string.Empty;

        private bool _CanChgQty = false;
        private string _IsSTK = string.Empty;

        private bool bSave = false;
        private System.DateTime dtNow;

        private string sCaldate = string.Empty;
        private System.DateTime dtCaldate;
        
        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;

        private Util _Util = new Util();

        private BizDataSet _Biz = new BizDataSet();

        private bool bShowMsg = false;
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

        public ASSY001_COM_EQPTEND()
        {
            InitializeComponent();
        }

        private void InitializeGrid()
        {
            DataTable dtTmp = new DataTable();

            dtTmp.Columns.Add("INPUTQTY", typeof(int));
            dtTmp.Columns.Add("GOODQTY", typeof(int));
            dtTmp.Columns.Add("DTL_DEFECT_LOT", typeof(int));
            dtTmp.Columns.Add("DTL_LOSS_LOT", typeof(int));
            dtTmp.Columns.Add("DTL_CHARGE_PROD_LOT", typeof(int));
            dtTmp.Columns.Add("DFCT_SUM", typeof(int));
            dtTmp.Columns.Add("PRE_SECTION_QTY", typeof(int));
            dtTmp.Columns.Add("AFTER_SECTION_QTY", typeof(int));
            dtTmp.Columns.Add("UNIDENTIFIED_QTY", typeof(int));
            dtTmp.Columns.Add("RE_INPUT_QTY", typeof(int));
            //dtTmp.Columns.Add("PICKUP_LOSS_QTY", typeof(int));
            dtTmp.Columns.Add("PRODUCT_LEVEL2_CODE", typeof(string));

            DataRow dtRow = dtTmp.NewRow();
            dtRow["INPUTQTY"] = 0;
            dtRow["GOODQTY"] = 0;
            dtRow["DTL_DEFECT_LOT"] = 0;
            dtRow["DTL_LOSS_LOT"] = 0;
            dtRow["DTL_CHARGE_PROD_LOT"] = 0;
            dtRow["DFCT_SUM"] = 0;
            dtRow["PRE_SECTION_QTY"] = 0;
            dtRow["AFTER_SECTION_QTY"] = 0;
            dtRow["UNIDENTIFIED_QTY"] = 0;
            dtRow["RE_INPUT_QTY"] = 0;
            //dtRow["PICKUP_LOSS_QTY"] = 0;
            dtRow["PRODUCT_LEVEL2_CODE"] = "";

            dtTmp.Rows.Add(dtRow);

            dgQty.ItemsSource = DataTableConverter.Convert(dtTmp);
        }

        private void InitializeControls()
        {
            dtNow = System.DateTime.Now;

            //if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            //{
            //    lblCstID.Visibility = Visibility.Visible;
            //    txtCstID.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    lblCstID.Visibility = Visibility.Collapsed;
            //    txtCstID.Visibility = Visibility.Collapsed;
            //}
            
            if (_ProcID.Equals(Process.LAMINATION))
            {
                dgQty.Columns["RE_INPUT_QTY"].Visibility = Visibility.Collapsed;
                dgQty.Columns["PRE_SECTION_QTY"].Visibility = Visibility.Collapsed;
                dgQty.Columns["AFTER_SECTION_QTY"].Visibility = Visibility.Collapsed;

                tabMagazine.Visibility = Visibility.Visible;
                tabBox.Visibility = Visibility.Collapsed;
                tabTray.Visibility = Visibility.Collapsed;
            }
            else if (_ProcID.Equals(Process.STACKING_FOLDING) || _ProcID.Equals(Process.CT_INSP))
            {
                dgQty.Columns["RE_INPUT_QTY"].Visibility = Visibility.Collapsed;
                dgQty.Columns["PRE_SECTION_QTY"].Visibility = Visibility.Collapsed;
                dgQty.Columns["AFTER_SECTION_QTY"].Visibility = Visibility.Collapsed;

                tabMagazine.Visibility = Visibility.Collapsed;
                tabBox.Visibility = Visibility.Visible;
                tabTray.Visibility = Visibility.Collapsed;
            }
            else if (_ProcID.Equals(Process.PACKAGING))
            {
                dgQty.Columns["RE_INPUT_QTY"].Visibility = Visibility.Visible;
                dgQty.Columns["PRE_SECTION_QTY"].Visibility = Visibility.Collapsed;
                dgQty.Columns["AFTER_SECTION_QTY"].Visibility = Visibility.Collapsed;

                tabMagazine.Visibility = Visibility.Collapsed;
                tabBox.Visibility = Visibility.Collapsed;
                tabTray.Visibility = Visibility.Visible;
            }
            else
            {
                dgQty.Columns["RE_INPUT_QTY"].Visibility = Visibility.Collapsed;
                dgQty.Columns["PRE_SECTION_QTY"].Visibility = Visibility.Collapsed;
                dgQty.Columns["AFTER_SECTION_QTY"].Visibility = Visibility.Collapsed;

                tabMagazine.Visibility = Visibility.Collapsed;
                tabBox.Visibility = Visibility.Collapsed;
                tabTray.Visibility = Visibility.Collapsed;
            }
            
            CommonCombo _combo = new CommonCombo();

            string[] sFilter;
            if (_ProcID.Equals(Process.CT_INSP))
                sFilter = new string[]{ "IRREGL_PROD_LOT_TYPE_CODE" };
            else
                sFilter = new string[] { "IRREGL_PROD_LOT_TYPE_CODE", "L" };
            _combo.SetCombo(cboAnLotType, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "COMMCODEATTRS");
        }

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 9)
            {
                _ProcID = Util.NVC(tmps[0]);
                _LineID = Util.NVC(tmps[1]);
                _EqptID = Util.NVC(tmps[2]);
                _LotID = Util.NVC(tmps[3]);
                _WipSeq = Util.NVC(tmps[4]);
                
                _LDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[5]);
                _UNLDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[6]);
                _CanChgQty = (bool)tmps[7];
                _IsSTK = Util.NVC(tmps[8]);
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
            if (bShowMsg)
                return;

            if (bSave)
                this.DialogResult = MessageBoxResult.OK;
            else
                this.DialogResult = MessageBoxResult.Cancel;
        }
        
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            double dUnidentifiedQty = 0;

            if (!CanSave(out dUnidentifiedQty))
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

        private void dgQty_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null) return;

                C1DataGrid grd = sender as C1DataGrid;

                grd.EndEdit();
                grd.EndEditRow(true);

                //CalcSumQty();

                double dGoodQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "GOODQTY")));
                double dDefectQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "DFCT_SUM")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "DFCT_SUM")));
                double dInputQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "INPUTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "INPUTQTY")));
                double dPreSectionQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "PRE_SECTION_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "PRE_SECTION_QTY")));
                double dAftSectionQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "AFTER_SECTION_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "AFTER_SECTION_QTY")));
                double dReInputQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "RE_INPUT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "RE_INPUT_QTY")));

                double dUnidentifiedQty = dInputQty - dGoodQty - dDefectQty - (dPreSectionQty + dAftSectionQty);// + dReInputQty;

                DataTableConverter.SetValue(grd.Rows[grd.TopRows.Count].DataItem, "UNIDENTIFIED_QTY", dUnidentifiedQty);
                //// 미확인 LOSS 불량 코드 관리로 인한수정                
                //int idx = _Util.GetDataGridRowIndex(dgDefect, "PRCS_ITEM_CODE", "UNIDENTIFIED_QTY");
                //if (idx >= 0)
                //{
                //    DataTableConverter.SetValue(dgDefect.Rows[idx].DataItem, "RESNQTY", dUnidentifiedQty);
                //    dgDefect.UpdateLayout();
                //}

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

        private void dgQty_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

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
                        if (_CanChgQty && 
                            (e.Cell.Column.Name.Equals("INPUTQTY") || 
                             e.Cell.Column.Name.Equals("PRE_SECTION_QTY") || 
                             e.Cell.Column.Name.Equals("AFTER_SECTION_QTY")))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFF8F"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }

                    //if (LoginInfo.LANGID.ToUpper().Equals("KO-KR"))
                    //{
                    //    if (e.Cell.Column.Name.Equals("INPUTQTY"))
                    //        ToolTipService.SetToolTip(e.Cell.Presenter, "설비보고수량");
                    //    else if (e.Cell.Column.Name.Equals("GOODQTY"))
                    //        ToolTipService.SetToolTip(e.Cell.Presenter, "생산반제품 수량 Sum");
                    //    //else if (e.Cell.Column.Name.Equals("DFCTQTY"))
                    //    //    ToolTipService.SetToolTip(e.Cell.Presenter, "불량수량 Sum");
                    //    else if (e.Cell.Column.Name.Equals("PRE_SECTION_QTY"))
                    //        ToolTipService.SetToolTip(e.Cell.Presenter, "설비보고수량");
                    //    else if (e.Cell.Column.Name.Equals("AFTER_SECTION_QTY"))
                    //        ToolTipService.SetToolTip(e.Cell.Presenter, "설비보고수량");
                    //    else if (e.Cell.Column.Name.Equals("UNIDENTIFIED_QTY"))
                    //        ToolTipService.SetToolTip(e.Cell.Presenter, "투입수 - 양품수 - 불량수 - 구간잔량");
                    //    else if (e.Cell.Column.Name.Equals("RE_INPUT_QTY"))
                    //        ToolTipService.SetToolTip(e.Cell.Presenter, "설비보고수량");
                    //    //else if (e.Cell.Column.Name.Equals("PICKUP_LOSS_QTY"))
                    //    //    ToolTipService.SetToolTip(e.Cell.Presenter, "설비보고수량");
                    //    else
                    //        ;
                    //}

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void dgQty_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

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
            if (!CanSaveDefect())
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
                //#region 미확인 Loss 음수 여부 Validation
                //if (e == null || e.Cell == null)
                //    return;

                //if (sender == null) return;

                //C1DataGrid grd = sender as C1DataGrid;

                //int idx = _Util.GetDataGridRowIndex(grd, "PRCS_ITEM_CODE", "UNIDENTIFIED_QTY");
                //if (idx >= 0)
                //{
                //    // 미확인 Loss = 투입수(타발수) - 양품수 - 불량Sum(실적제외 및 미확인LOSS 항목 제외) - 구간잔량 + 재투입수
                //    DataTable dtSrc = DataTableConverter.Convert(dgDefect.ItemsSource);
                //    double dDefectQty = dtSrc.Compute("SUM(RESNQTY)", "RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(RESNQTY)", "RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString());

                //    double dInputQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "INPUTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "INPUTQTY")));
                //    double dGoodQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")));
                //    double dPreSecQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "PRE_SECTION_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "PRE_SECTION_QTY")));
                //    double dAftSecQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "AFTER_SECTION_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "AFTER_SECTION_QTY")));
                //    double dReInputQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "RE_INPUT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "RE_INPUT_QTY")));

                //    double dUnidentifiedQty = dInputQty - dGoodQty - dDefectQty - (dPreSecQty + dAftSecQty);// + dReInputQty;
                    
                //    if (dUnidentifiedQty < 0)
                //    {
                //        // 입력오류 : 입력한 불량으로 인해 미확인LOSS가 음수가 됩니다.
                //        Util.MessageValidation("SFU6035", (action) => { bShowMsg = false; });

                //        grd.EndEdit();
                //        grd.EndEditRow(true);

                //        DataTableConverter.SetValue(grd.Rows[e.Cell.Row.Index].DataItem, "RESNQTY", 0);

                //        grd.UpdateLayout();

                //        bShowMsg = true;
                //    }
                //}
                //#endregion

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

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

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
                        //if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "RSLT_EXCL_FLAG")).Equals("Y"))
                        //{
                        //    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        //    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        //}
                        //else
                        //{
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
                        //}
                    }
                }
            }));
        }

        private void dgDefect_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

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

        private void dgQty_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            if (dgDefect?.Rows?.Count > 0)
            {
                dgDefect.EndEdit(true);

                int idx = _Util.GetDataGridRowIndex(dgDefect, "PRCS_ITEM_CODE", "UNIDENTIFIED_QTY");
                if (idx >= 0 && dgDefect.CurrentCell.Row.Index == idx)
                {
                    int iTmpRow = dgDefect.GetRowCount() - 2 > idx ? idx + 1 : idx - 1;

                    C1.WPF.DataGrid.DataGridCell dgcTmp = dgDefect.GetCell(iTmpRow, dgDefect.Columns.Count - 1);

                    if (dgcTmp != null)
                        dgDefect.CurrentCell = dgcTmp;
                }
            }
        }

        private void dgDefect_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            if (dgQty?.Rows?.Count > 0)
                dgQty.EndEdit(true);
        }
        #endregion

        #region Mehod

        #region [BizCall]
        private void GetLotInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WIP_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = _LotID;
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONFIRM_LOT_INFO_L", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dgQty.GetRowCount() > 0)
                    {
                        if (dgQty.Columns.Contains("INPUTQTY") && dtRslt.Columns.Contains("INPUTQTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "INPUTQTY", dtRslt.Rows[0]["INPUTQTY"]);

                        if (dtRslt.Columns.Contains("PRODUCT_LEVEL2_CODE") && Util.NVC(dtRslt.Rows[0]["PRODUCT_LEVEL2_CODE"]).Equals("MC"))
                        {
                            if (dgQty.Columns.Contains("GOODQTY") && dtRslt.Columns.Contains("GOODQTY"))
                                DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY", dtRslt.Rows[0]["GOODQTY"]);
                        }

                        //if (dgQty.Columns.Contains("DFCTQTY") && searchResult.Columns.Contains("DFCTQTY"))
                        //    DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "DFCTQTY", searchResult.Rows[0]["DFCTQTY"]); 
                        if (dgQty.Columns.Contains("PRE_SECTION_QTY") && dtRslt.Columns.Contains("PRE_SECTION_QTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "PRE_SECTION_QTY", dtRslt.Rows[0]["PRE_SECTION_QTY"]);
                        if (dgQty.Columns.Contains("AFTER_SECTION_QTY") && dtRslt.Columns.Contains("AFTER_SECTION_QTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "AFTER_SECTION_QTY", dtRslt.Rows[0]["AFTER_SECTION_QTY"]);
                        if (dgQty.Columns.Contains("UNIDENTIFIED_QTY") && dtRslt.Columns.Contains("UNIDENTIFIED_QTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "UNIDENTIFIED_QTY", dtRslt.Rows[0]["UNIDENTIFIED_QTY"]);
                        if (dgQty.Columns.Contains("RE_INPUT_QTY") && dtRslt.Columns.Contains("RE_INPUT_QTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "RE_INPUT_QTY", dtRslt.Rows[0]["RE_INPUT_QTY"]);
                        //if (dgQty.Columns.Contains("PICKUP_LOSS_QTY") && dtRslt.Columns.Contains("PICKUP_LOSS_QTY"))
                        //    DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "PICKUP_LOSS_QTY", dtRslt.Rows[0]["PICKUP_LOSS_QTY"]);

                        if (dgQty.Columns.Contains("PRODUCT_LEVEL2_CODE") && dtRslt.Columns.Contains("PRODUCT_LEVEL2_CODE"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "PRODUCT_LEVEL2_CODE", dtRslt.Rows[0]["PRODUCT_LEVEL2_CODE"]);
                    }

                    txtLotId.Text = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                    txtProdId.Text = Util.NVC(dtRslt.Rows[0]["PRODID"]);
                    //txtWorkOrder.Text = Util.NVC(searchResult.Rows[0]["WOID"]);
                    txtStartTime.Text = Util.NVC(dtRslt.Rows[0]["WIPDTTM_ST"]);
                    //txtEndTime.Text = Util.NVC(searchResult.Rows[0]["EQPT_END_DTTM"]);
                    //txtRemark.Text = Util.NVC(searchResult.Rows[0]["WIP_NOTE"]);

                    //txtAnLotType.Text = Util.NVC(dtRslt.Rows[0]["IRREGL_PROD_LOT_TYPE_NAME"]);
                    txtLotOperMode.Text = Util.NVC(dtRslt.Rows[0]["PROD_LOT_OPER_MODE_NAME"]);

                    _IRREGL_PROD_LOT_TYPE_CODE = Util.NVC(dtRslt.Rows[0]["IRREGL_PROD_LOT_TYPE_CODE"]);
                    cboAnLotType.SelectedValue = Util.NVC(dtRslt.Rows[0]["IRREGL_PROD_LOT_TYPE_CODE"]);
                    //_PROD_LOT_OPER_MODE = Util.NVC(dtRslt.Rows[0]["PROD_LOT_OPER_MODE"]);

                    //if (_IRREGL_PROD_LOT_TYPE_CODE.Equals("L"))
                    //{
                    //    txtAnLotType.Visibility = Visibility.Collapsed;
                        cboAnLotType.Visibility = Visibility.Visible;
                    //}
                    //else
                    //{
                    //    txtAnLotType.Visibility = Visibility.Visible;
                    //    cboAnLotType.Visibility = Visibility.Collapsed;
                    //}

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
            try
            {
                ShowLoadingIndicator();

                string bizName = string.Empty;

                if (_ProcID.Equals(Process.LAMINATION))
                    bizName = "BR_PRD_REG_EQPT_END_PROD_LOT_LM_L";
                else if (_ProcID.Equals(Process.STACKING_FOLDING))
                    bizName = "BR_PRD_REG_EQPT_END_PROD_LOT_FD_L";
                else if (_ProcID.Equals(Process.CT_INSP))
                    bizName = "BR_PRD_REG_EQPT_END_PROD_LOT_CI_L";
                else if (_ProcID.Equals(Process.PACKAGING))
                    bizName = "BR_PRD_REG_EQPT_END_PROD_LOT_CL_L";

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("LOT_MODE", typeof(string));
                inDataTable.Columns.Add("AN_LOT_TYPE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                inDataTable = indataSet.Tables.Add("IN_INPUT");
                inDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inDataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
                //inDataTable.Columns.Add("MTRLID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));
               
                inDataTable = indataSet.Tables.Add("IN_OUTPUT");              
                inDataTable.Columns.Add("INPUT_QTY", typeof(int));
                inDataTable.Columns.Add("OUTPUT_QTY", typeof(int));
                inDataTable.Columns.Add("DFCT_QTY", typeof(int));
                inDataTable.Columns.Add("BTN_QTY_PRE", typeof(int));
                inDataTable.Columns.Add("BTN_QTY_AFT", typeof(int));
                //inDataTable.Columns.Add("LOSS_PRE_STKNG", typeof(int));
                inDataTable.Columns.Add("REINPUT_QTY", typeof(int));

                inDataTable = indataSet.Tables.Add("IN_DEFECT");
                inDataTable.Columns.Add("EQPT_DFCT_CODE", typeof(string));
                inDataTable.Columns.Add("DFCT_QTY", typeof(int));
                inDataTable.Columns.Add("PORT_ID", typeof(string));
                
                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = txtLotId.Text;
                newRow["LOT_MODE"] = "L"; // 비정규  // _PROD_LOT_OPER_MODE;
                newRow["AN_LOT_TYPE"] = Util.GetCondition(cboAnLotType, bAllNull: true) == null || Util.GetCondition(cboAnLotType, bAllNull: true).Equals("") ? _IRREGL_PROD_LOT_TYPE_CODE : Util.GetCondition(cboAnLotType);
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                //DataTable inInputTable = indataSet.Tables["IN_INPUT"];

                //newRow = inInputTable.NewRow();
                //newRow["EQPT_MOUNT_PSTN_ID"] = "";
                //newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                //newRow["INPUT_LOTID"] = "";

                //if (_LDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                //    newRow["CSTID"] = "";

                //inInputTable.Rows.Add(newRow);

                newRow = null;

                if (dgQty.GetRowCount() > 0)
                {
                    DataTable inOutputTable = indataSet.Tables["IN_OUTPUT"];

                    newRow = inOutputTable.NewRow();

                    newRow["INPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "INPUTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "INPUTQTY")));
                    newRow["DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "DFCT_SUM")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "DFCT_SUM")));
                    newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")));
                    
                    if (_ProcID.Equals(Process.PACKAGING))
                    {
                        newRow["REINPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "RE_INPUT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "RE_INPUT_QTY")));
                    }
                    else
                    {
                        newRow["BTN_QTY_PRE"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "PRE_SECTION_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "PRE_SECTION_QTY")));
                        newRow["BTN_QTY_AFT"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "AFTER_SECTION_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "AFTER_SECTION_QTY")));
                    }

                    inOutputTable.Rows.Add(newRow);

                    newRow = null;
                }

                new ClientProxy().ExecuteService_Multi(bizName, "IN_EQP,IN_INPUT,IN_OUTPUT,IN_DEFECT", null, (bizResult, bizException) =>
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

                        bSave = true;

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

        private void SetDefect(bool bMsgShow = true)
        {
            try
            {
                ShowLoadingIndicator();

                dgDefect.EndEdit();

                DataSet indataSet = _Biz.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable inDEFECT_LOT = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = null;

                    newRow = inDEFECT_LOT.NewRow();
                    newRow["LOTID"] = txtLotId.Text.Trim();
                    newRow["WIPSEQ"] = _WipSeq;
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
                newRow["PROCID"] = _ProcID;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = _LotID;
                newRow["WIPSEQ"] = _WipSeq;
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

        private void GetMagzineList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = _LotID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_LM", "INDATA", "OUTDATA", inTable);
                
                Util.GridSetData(dgMagazine, dtRslt, FrameOperation, true);

                SumGoodQty();
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

        private void GetBoxList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = _LotID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_FD", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgBox, dtRslt, FrameOperation, true);

                SumGoodQty();
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

        private void GetTrayList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("TRAYID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PR_LOTID"] = _LotID;
                newRow["PROCID"] = Process.PACKAGING;
                newRow["EQSGID"] = _LineID;
                newRow["EQPTID"] = _EqptID;
                newRow["TRAYID"] = null;    // 전체 리스트 조회 처리.

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_CL", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgTray, dtRslt, FrameOperation, true);

                SumGoodQty();
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
        #endregion

        #region [Validation]
        private bool CanSave(out double dUnidentifiedQty)
        {
            dUnidentifiedQty = 0;

            bool bRet = false;

            if (bShowMsg)
                return bRet;

            //if (dgDefect.ItemsSource != null)
            //{
            //    foreach (DataRowView row in DataGridHandler.GetModifiedItems(dgDefect))
            //    {
            //        //Util.Alert("저장하지 않은 불량 정보가 있습니다.");
            //        Util.MessageValidation("SFU1878");
            //        return bRet;
            //    }
            //}

            foreach (C1.WPF.DataGrid.DataGridRow row in dgDefect.Rows)
            {
                double dRsn, dOrgRsn = 0;

                double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                if (dRsn != dOrgRsn)
                {
                    // 저장하지 않은 불량 정보가 있습니다.
                    Util.MessageValidation("SFU1878", (action) =>
                    {
                        if (tbDefect.Visibility == Visibility.Visible)
                            tbDefect.IsSelected = true;
                    });
                    return bRet;
                }
            }

            dUnidentifiedQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "UNIDENTIFIED_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "UNIDENTIFIED_QTY")));

            //if (dUnidentifiedQty < 0)
            //{
            //    // 입력오류 : 미확인LOSS가 음수로 처리할 수 없습니다.
            //    Util.MessageValidation("SFU6041");
            //    return bRet;
            //}

            bRet = true;
            return bRet;
        }

        private bool CanSaveDefect()
        {
            bool bRet = false;

            if (bShowMsg)
                return bRet;

            if (!CommonVerify.HasDataGridRow(dgDefect))
            {
                //Util.Alert("불량 항목이 없습니다.");
                Util.MessageValidation("SFU1578");
                return bRet;
            }

            if (txtLotId.Text.Trim().Length < 1)
            {
                //Util.Alert("LOT 정보가 없습니다.");
                Util.MessageValidation("SFU1195");
                return bRet;
            }

            bRet = true;
            return bRet;
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
            
            if (_ProcID.Equals(Process.LAMINATION))
            {
                GetMagzineList();
                GetDefectInfo();
            }
            else if (_ProcID.Equals(Process.STACKING_FOLDING) || _ProcID.Equals(Process.CT_INSP))
            {
                GetBoxList();
                GetDefectInfo();
            }
            else if (_ProcID.Equals(Process.PACKAGING))
            {
                GetTrayList();
                GetDefectInfo();
            }
        }

        private void ClearControls()
        {
            Util.gridClear(dgQty);
            Util.gridClear(dgDefect);
            Util.gridClear(dgMagazine);
            Util.gridClear(dgBox);
            Util.gridClear(dgTray);

            txtLotId.Text = "";
            //txtCstID.Text = "";
            txtProdId.Text = "";
            txtLotOperMode.Text = "";
            //txtAnLotType.Text = "";
            txtStartTime.Text = "";
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            listAuth.Add(btnDefectSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            if (_CanChgQty)
            {
                dgQty.Columns["INPUTQTY"].IsReadOnly = false;
                dgQty.Columns["PRE_SECTION_QTY"].IsReadOnly = false;
                dgQty.Columns["AFTER_SECTION_QTY"].IsReadOnly = false;
            }
            else
            {
                dgQty.Columns["INPUTQTY"].IsReadOnly = true;
                dgQty.Columns["PRE_SECTION_QTY"].IsReadOnly = true;
                dgQty.Columns["AFTER_SECTION_QTY"].IsReadOnly = true;
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

        private void SumGoodQty()
        {
            try
            {
                DataTable dtSrc;
                DataTable dtTgt;

                if (_ProcID.Equals(Process.LAMINATION))
                    dtSrc = DataTableConverter.Convert(dgMagazine.ItemsSource);
                else if (_ProcID.Equals(Process.STACKING_FOLDING) || _ProcID.Equals(Process.CT_INSP))
                    dtSrc = DataTableConverter.Convert(dgBox.ItemsSource);
                else if (_ProcID.Equals(Process.PACKAGING))
                    dtSrc = DataTableConverter.Convert(dgTray.ItemsSource);
                else
                    dtSrc = null;

                dtTgt = DataTableConverter.Convert(dgQty.ItemsSource);

                if (dtTgt == null || dtTgt.Rows.Count < 1) return;

                if (dtSrc != null && dtSrc.Rows.Count > 0)
                {
                    if (_ProcID.Equals(Process.PACKAGING))
                        dtTgt.Rows[0]["GOODQTY"] = dtSrc.Compute("SUM(CELLQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(CELLQTY)", string.Empty).ToString());
                    else
                    {
                        // Mono Cell 양품 수량 계산 제외. (생산Lot 정보 조회 시 수량 조회)
                        if (!Util.NVC(dtTgt.Rows[0]["PRODUCT_LEVEL2_CODE"]).Equals("MC"))
                            dtTgt.Rows[0]["GOODQTY"] = dtSrc.Compute("SUM(WIPQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(WIPQTY)", string.Empty).ToString());
                    }
                    //if (_ProcID.Equals(Process.PACKAGING))
                    //    DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY", dtSrc.Compute("SUM(CELLQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(CELLQTY)", string.Empty).ToString()));
                    //else
                    //    DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY", dtSrc.Compute("SUM(WIPQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(WIPQTY)", string.Empty).ToString()));
                }
                else
                {
                    dtTgt.Rows[0]["GOODQTY"] = 0;
                    //DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY", 0);
                }

                CalcSumQty(dtTgt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CalcSumQty(DataTable dtTmp)
        {
            try
            {
                if (dtTmp == null || dtTmp.Rows.Count < 1) return;
                
                double dGoodQty = Util.NVC(dtTmp.Rows[0]["GOODQTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["GOODQTY"]));
                double dDefectQty = Util.NVC(dtTmp.Rows[0]["DFCT_SUM"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["DFCT_SUM"]));
                double dInputQty = Util.NVC(dtTmp.Rows[0]["INPUTQTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["INPUTQTY"]));
                double dPreSecQty = Util.NVC(dtTmp.Rows[0]["PRE_SECTION_QTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["PRE_SECTION_QTY"]));
                double dAftSecQty = Util.NVC(dtTmp.Rows[0]["AFTER_SECTION_QTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["AFTER_SECTION_QTY"]));
                double dReInputQty = Util.NVC(dtTmp.Rows[0]["RE_INPUT_QTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["RE_INPUT_QTY"]));

                double dUnidentifiedQty = dInputQty - dGoodQty - dDefectQty - (dPreSecQty + dAftSecQty);// + dReInputQty;

                dtTmp.Rows[0]["UNIDENTIFIED_QTY"] = dUnidentifiedQty;
                dgQty.ItemsSource = DataTableConverter.Convert(dtTmp);

                //// 미확인 LOSS 불량 코드 관리로 인한수정
                //int idx = _Util.GetDataGridRowIndex(dgDefect, "PRCS_ITEM_CODE", "UNIDENTIFIED_QTY");
                //if (idx >= 0)
                //    DataTableConverter.SetValue(dgDefect.Rows[idx].DataItem, "RESNQTY", dUnidentifiedQty);
                
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
