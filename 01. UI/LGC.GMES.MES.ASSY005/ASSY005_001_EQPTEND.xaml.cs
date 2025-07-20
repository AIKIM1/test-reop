/*************************************************************************************
 Created Date : 2020.10.29
      Creator : 신광희
   Decription : CNB2동 증설 - Notching 공정 - 장비완료(ASSY004_001_EQPTEND Copy 후 작성)
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.29  신광희 : Initial Created.
  2021.10.18  김지은    SI     노칭 공정 종료 시 무지부/권취방향설정 추가
  2023.01.05  유재홍    SI     설비완공시 잔량, 생산랏 HOLD 사유 입력 추가
  2024.01.11  남재현 :  MES 리빌딩 Notching UI 장비 완료 시, 잔량 배출 / 소진 완료 항목 제거. 
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
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.ASSY005
{
    /// <summary>
    /// ASSY005_001_EQPTEND.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY005_001_EQPTEND : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _ProcID = string.Empty;
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private string _WipStat = string.Empty;
        private string _MountPstsID = string.Empty;

        private string _IRREGL_PROD_LOT_TYPE_CODE = string.Empty;
        private string _PROD_LOT_OPER_MODE = string.Empty;

        private bool _CanChgQty = false;
        private bool _isSideRollDirctnUse = false;

        private bool bShowMsg = false;

        private bool bSave = false;
        private System.DateTime dtNow;

        private string sCaldate = string.Empty;
        private System.DateTime dtCaldate;

        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;

        private Util _Util = new Util();

        private BizDataSet _Biz = new BizDataSet();

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

        public ASSY005_001_EQPTEND()
        {
            InitializeComponent();
        }

        private void InitializeGrid()
        {
            DataTable dtTmp = new DataTable();

            dtTmp.Columns.Add("M_RMN_QTY", typeof(int));
            dtTmp.Columns.Add("M_WIPQTY", typeof(int));
            dtTmp.Columns.Add("EQPT_INPUT_QTY", typeof(int));
            dtTmp.Columns.Add("GOODQTY", typeof(int));
            dtTmp.Columns.Add("M_CURR_PROC_LOSS_QTY", typeof(int));  //자공정LOSS
            dtTmp.Columns.Add("M_FIX_LOSS_QTY", typeof(int));
            dtTmp.Columns.Add("UNIDENTIFIED_QTY", typeof(int));
            dtTmp.Columns.Add("M_PRE_PROC_LOSS_QTY", typeof(int));
            dtTmp.Columns.Add("DTL_DEFECT_LOT", typeof(int));
            dtTmp.Columns.Add("DTL_LOSS_LOT", typeof(int));
            dtTmp.Columns.Add("DTL_CHARGE_PROD_LOT", typeof(int));
            dtTmp.Columns.Add("DFCT_SUM", typeof(int));
            dtTmp.Columns.Add("M_LOTID", typeof(string));
            dtTmp.Columns.Add("M_CSTID", typeof(string));

            DataRow dtRow = dtTmp.NewRow();
            dtRow["M_RMN_QTY"] = 0;
            dtRow["M_WIPQTY"] = 0;
            dtRow["EQPT_INPUT_QTY"] = 0;
            dtRow["GOODQTY"] = 0;
            dtRow["M_CURR_PROC_LOSS_QTY"] = 0;   //자공정LOSS
            dtRow["M_FIX_LOSS_QTY"] = 0;
            dtRow["UNIDENTIFIED_QTY"] = 0;
            dtRow["M_PRE_PROC_LOSS_QTY"] = 0;
            dtRow["DTL_DEFECT_LOT"] = 0;
            dtRow["DTL_LOSS_LOT"] = 0;
            dtRow["DTL_CHARGE_PROD_LOT"] = 0;
            dtRow["DFCT_SUM"] = 0;
            dtRow["M_LOTID"] = "";
            dtRow["M_CSTID"] = "";

            dtTmp.Rows.Add(dtRow);

            dgQty.ItemsSource = DataTableConverter.Convert(dtTmp);
        }

        private void InitializeControls()
        {
            dtNow = System.DateTime.Now;

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

            DataTable dt = new DataTable();
            dt.Columns.Add("CODE");
            dt.Columns.Add("NAME");

            DataRow dr = dt.NewRow();
            dr["CODE"] = "N";
            dr["NAME"] = "N";
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["CODE"] = "H";
            dr["NAME"] = "Y"; // Hold 배출
            dt.Rows.Add(dr);

            cboHold.DisplayMemberPath = "NAME";
            cboHold.SelectedValuePath = "CODE";
            cboHold.ItemsSource = dt.Copy().AsDataView();

            cboHold.SelectedIndex = 0;

            cboEndType.DisplayMemberPath = "NAME";
            cboEndType.SelectedValuePath = "CODE";
            cboEndType.ItemsSource = dt.Copy().AsDataView();

            cboEndType.SelectedIndex = 0;
            cboEndType.IsEnabled = true;

            
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { "IRREGL_PROD_LOT_TYPE_CODE", "L" };
            _combo.SetCombo(cboAnLotType, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "COMMCODEATTR");

            //2023 01 04 유재홍 hold 사유코드 Combobox
            

            string[] arrColumn = { "LANGID", "AREAID", "COM_TYPE_CODE","ATTR2", "ATTR3" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "UNMOUNT_EQPT_MES_HOLD_CODE", _ProcID,"IN" };
            cboInHoldCode.SetDataComboItem("DA_BAS_SEL_AREA_COM_CODE_CBO_ATTR_LIKE", arrColumn, arrCondition);
          

            cboInHoldCode.SelectedIndex = 0;

            string[] arrColumn2 = { "LANGID", "AREAID", "COM_TYPE_CODE", "ATTR2", "ATTR3" };
            string[] arrCondition2 = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "UNMOUNT_EQPT_MES_HOLD_CODE", _ProcID, "OUT" };
            cboOutHoldCode.SetDataComboItem("DA_BAS_SEL_AREA_COM_CODE_CBO_ATTR_LIKE", arrColumn2, arrCondition2);

            cboOutHoldCode.SelectedIndex = 0;




            //실적확정에서 처리하기로 하여, 장비완료에서는 안보이도록 한다, 2024-11-26, 김선영, START 
            // 무지부/권취 두 방향 모두 사용하는 AREA에선 코터 완공 시 무지부/권취 방향 저장할 수 있도록 함
            //if (string.Equals(_ProcID, Process.NOTCHING) && _Util.IsCommonCodeUse("NON_COATED_WINDING_DIRCTN_USE_AREA", LoginInfo.CFG_AREA_ID))
            //{
            //    _isSideRollDirctnUse = true;
            //    tbSSWD.Visibility = Visibility.Visible;       
            //    dgSSWD.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    _isSideRollDirctnUse = false;
            //    tbSSWD.Visibility = Visibility.Collapsed;
            //    dgSSWD.Visibility = Visibility.Collapsed;
            //}
            
            _isSideRollDirctnUse = false;
            tbSSWD.Visibility = Visibility.Collapsed;
            dgSSWD.Visibility = Visibility.Collapsed;
            //실적확정에서 처리하기로 하여, 장비완료에서는 안보이도록 한다, 2024-11-26, 김선영, END 
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
                _MountPstsID = Util.NVC(tmps[7]);

                _CanChgQty = (bool)tmps[8];
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

                //double dRmnQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "M_RMN_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "M_RMN_QTY")));
                double dInputQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "M_WIPQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "M_WIPQTY")));
                double dPunchQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "EQPT_INPUT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "EQPT_INPUT_QTY")));
                double dGoodQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "GOODQTY")));
                double dFixLossQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "M_FIX_LOSS_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "M_FIX_LOSS_QTY")));
                double dDefectQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "DFCT_SUM")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "DFCT_SUM")));
                //자공정 LOSS
                double dCurrProdLossQty = Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "M_CURR_PROC_LOSS_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(grd.Rows[grd.TopRows.Count].DataItem, "M_CURR_PROC_LOSS_QTY")));

                double dUnidentifiedQty = dPunchQty - dGoodQty - dDefectQty;
                double dLengLEQty = dInputQty - dFixLossQty - dPunchQty - dCurrProdLossQty; //자공정LOSS 추가
                double dRmnQty = dInputQty - dPunchQty;

                DataTableConverter.SetValue(grd.Rows[grd.TopRows.Count].DataItem, "UNIDENTIFIED_QTY", dUnidentifiedQty);
                //// 미확인 LOSS 불량 코드 관리로 인한수정
                //int idx = _Util.GetDataGridRowIndex(dgDefect, "PRCS_ITEM_CODE", "UNIDENTIFIED_QTY");
                //if (idx >= 0)
                //{                    
                //    DataTableConverter.SetValue(dgDefect.Rows[idx].DataItem, "RESNQTY", dUnidentifiedQty);
                //    dgDefect.UpdateLayout();
                //}


                if ((bool)rdoCpl.IsChecked)
                {
                    DataTableConverter.SetValue(grd.Rows[grd.TopRows.Count].DataItem, "M_PRE_PROC_LOSS_QTY", dLengLEQty);
                    DataTableConverter.SetValue(grd.Rows[grd.TopRows.Count].DataItem, "M_RMN_QTY", 0);
                }
                else
                {
                    DataTableConverter.SetValue(grd.Rows[grd.TopRows.Count].DataItem, "M_PRE_PROC_LOSS_QTY", 0);
                    DataTableConverter.SetValue(grd.Rows[grd.TopRows.Count].DataItem, "M_RMN_QTY", dRmnQty);
                }

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
                            (e.Cell.Column.Name.Equals("EQPT_INPUT_QTY") || e.Cell.Column.Name.Equals("GOODQTY")))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFF8F"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                        else
                        {
                            if (_CanChgQty && e.Cell.Column.Name.Equals("M_FIX_LOSS_QTY") && (bool)rdoCpl.IsChecked)
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
                    }

                    //if (LoginInfo.LANGID.ToUpper().Equals("KO-KR"))
                    //{
                    //    if (e.Cell.Column.Name.Equals("M_RMN_QTY"))
                    //        ToolTipService.SetToolTip(e.Cell.Presenter, "소진완료 : 0, 잔량배출 : 투입량 - 타발수");
                    //    else if (e.Cell.Column.Name.Equals("M_WIPQTY"))
                    //        ToolTipService.SetToolTip(e.Cell.Presenter, "재공수");
                    //    else if (e.Cell.Column.Name.Equals("EQPT_INPUT_QTY"))
                    //        ToolTipService.SetToolTip(e.Cell.Presenter, "설비 타발 수");
                    //    else if (e.Cell.Column.Name.Equals("GOODQTY"))
                    //        ToolTipService.SetToolTip(e.Cell.Presenter, "양품수");
                    //    else if (e.Cell.Column.Name.Equals("M_FIX_LOSS_QTY"))
                    //        ToolTipService.SetToolTip(e.Cell.Presenter, "소진완료 : 설정값, 잔량배출 : 0");
                    //    else if (e.Cell.Column.Name.Equals("UNIDENTIFIED_QTY"))
                    //        ToolTipService.SetToolTip(e.Cell.Presenter, "타발수 - 배출수 - 불량Sum");
                    //    else if (e.Cell.Column.Name.Equals("M_PRE_PROC_LOSS_QTY"))
                    //        ToolTipService.SetToolTip(e.Cell.Presenter, "소진완료 : 투입량 - 타발수 - 고정LOSS, 잔량배출 : 0");
                    //    //else if (e.Cell.Column.Name.Equals("DFCTQTY"))
                    //    //    ToolTipService.SetToolTip(e.Cell.Presenter, "불량 Sum(실적제외 항목 및 고정LOSS 항목 제외)");
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
                #region 미확인 Loss 음수 여부 Validation
                if (e == null || e.Cell == null)
                    return;

                if (sender == null) return;

                C1DataGrid grd = sender as C1DataGrid;

                int idx = _Util.GetDataGridRowIndex(grd, "PRCS_ITEM_CODE", "UNIDENTIFIED_QTY");
                if (idx >= 0)
                {
                    // 미확인 Loss = 투입수(타발수) - 양품수 - 불량Sum(실적제외 및 미확인LOSS 항목 제외) - 구간잔량
                    DataTable dtSrc = DataTableConverter.Convert(dgDefect.ItemsSource);
                    double dDefectQty = dtSrc.Compute("SUM(RESNQTY)", "RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(RESNQTY)", "RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'").ToString());

                    double dPunchQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "EQPT_INPUT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "EQPT_INPUT_QTY")));
                    double dGoodQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")));

                    double dUnidentifiedQty = dPunchQty - dGoodQty - dDefectQty;

                    if (dUnidentifiedQty < 0)
                    {
                        // 입력오류 : 입력한 불량으로 인해 미확인LOSS가 음수가 됩니다.
                        Util.MessageValidation("SFU6035", (action) => { bShowMsg = false; });

                        grd.EndEdit();
                        grd.EndEditRow(true);

                        DataTableConverter.SetValue(grd.Rows[e.Cell.Row.Index].DataItem, "RESNQTY", 0);

                        grd.UpdateLayout();

                        bShowMsg = true;
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

        private void rdoCpl_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgQty == null || dgQty.Columns == null || !dgQty.Columns.Contains("M_FIX_LOSS_QTY")) return;

                if (cboHold == null) return;

                cboHold.IsEnabled = false;

                

                dgQty.EndEdit();

                if (_CanChgQty)
                {
                    dgQty.Columns["M_FIX_LOSS_QTY"].IsReadOnly = false;
                    if (dgQty.GetCell(dgQty.TopRows.Count, dgQty.Columns["M_FIX_LOSS_QTY"].Index).Presenter != null)
                    {
                        dgQty.GetCell(dgQty.TopRows.Count, dgQty.Columns["M_FIX_LOSS_QTY"].Index).Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFF8F"));
                        dgQty.GetCell(dgQty.TopRows.Count, dgQty.Columns["M_FIX_LOSS_QTY"].Index).Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }

                DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_RMN_QTY", 0);
                SumDefectQty();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoRmn_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgQty == null || dgQty.Columns == null || !dgQty.Columns.Contains("M_FIX_LOSS_QTY")) return;

                if (cboHold == null) return;

                cboHold.IsEnabled = true;
                

                dgQty.EndEdit();

                if (_CanChgQty)
                {
                    dgQty.Columns["M_FIX_LOSS_QTY"].IsReadOnly = true;
                    if (dgQty.GetCell(dgQty.TopRows.Count, dgQty.Columns["M_FIX_LOSS_QTY"].Index).Presenter != null)
                    {
                        dgQty.GetCell(dgQty.TopRows.Count, dgQty.Columns["M_FIX_LOSS_QTY"].Index).Presenter.Background = null;
                        dgQty.GetCell(dgQty.TopRows.Count, dgQty.Columns["M_FIX_LOSS_QTY"].Index).Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }

                DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_FIX_LOSS_QTY", 0);
                DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_PRE_PROC_LOSS_QTY", 0);

                SumDefectQty();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSSWD_Click(object sender, RoutedEventArgs e)
        {
            CMM_HALF_SLITTING_ROLL_DIRCTN popupSelSideRollDirctn = new CMM_HALF_SLITTING_ROLL_DIRCTN();
            popupSelSideRollDirctn.FrameOperation = FrameOperation;

            if (popupSelSideRollDirctn != null)
            {
                popupSelSideRollDirctn.Closed += (s, arg) =>
                {
                    if (popupSelSideRollDirctn.DialogResult == MessageBoxResult.OK)
                    {
                        txtSSWD.Text = popupSelSideRollDirctn.SSWDNAME;
                        txtSSWD.Tag = popupSelSideRollDirctn.SSWDCODE;
                        popupSelSideRollDirctn = null;
                    }
                    else
                    {
                        txtSSWD.Text = null;
                        txtSSWD.Tag = null;
                        popupSelSideRollDirctn = null;
                        return;
                    }
                };

                this.Dispatcher.BeginInvoke(new Action(() => popupSelSideRollDirctn.ShowModal()));
                popupSelSideRollDirctn.CenterOnScreen();
                popupSelSideRollDirctn.BringToFront();
            }
        }
        #endregion

        #region Mehod

        #region [BizCall]
        private void GetLotInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = _LotID;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONFIRM_LOT_INFO_NT_L", "INDATA", "OUTDATA", inTable);

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

                        if (dgQty.Columns.Contains("EQPT_INPUT_QTY") && dtRslt.Columns.Contains("EQPT_INPUT_QTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "EQPT_INPUT_QTY", dtRslt.Rows[0]["EQPT_INPUT_QTY"]);
                        //if (dgQty.Columns.Contains("DFCTQTY") && dtRslt.Columns.Contains("DFCTQTY"))
                        //    DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "DFCTQTY", dtRslt.Rows[0]["DFCTQTY"]);
                        //if (dgQty.Columns.Contains("UNIDENTIFIED_QTY") && dtRslt.Columns.Contains("UNIDENTIFIED_QTY"))
                        //    DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "UNIDENTIFIED_QTY", dtRslt.Rows[0]["UNIDENTIFIED_QTY"]);
                        if (dgQty.Columns.Contains("GOODQTY") && dtRslt.Columns.Contains("GOODQTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY", dtRslt.Rows[0]["GOODQTY"]);

                        if (dgQty.Columns.Contains("M_LOTID") && dtRslt.Columns.Contains("M_LOTID"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_LOTID", dtRslt.Rows[0]["M_LOTID"]);
                        if (dgQty.Columns.Contains("M_CSTID") && dtRslt.Columns.Contains("M_CSTID"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_CSTID", dtRslt.Rows[0]["M_CSTID"]);
                    }

                    txtLotId.Text = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                    txtCstID.Text = Util.NVC(dtRslt.Rows[0]["CSTID"]);
                    txtProdId.Text = Util.NVC(dtRslt.Rows[0]["PRODID"]);
                    //txtWorkOrder.Text = Util.NVC(searchResult.Rows[0]["WOID"]);
                    txtStartTime.Text = Util.NVC(dtRslt.Rows[0]["WIPDTTM_ST"]);
                    //txtEndTime.Text = Util.NVC(searchResult.Rows[0]["EQPT_END_DTTM"]);
                    //txtRemark.Text = Util.NVC(searchResult.Rows[0]["WIP_NOTE"]);

                    //txtLotType.Text = Util.NVC(dtRslt.Rows[0]["IRREGL_PROD_LOT_TYPE_NAME"]);
                    cboAnLotType.SelectedValue = Util.NVC(dtRslt.Rows[0]["IRREGL_PROD_LOT_TYPE_CODE"]);
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
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("WORK_TYPE", typeof(string));
                inDataTable.Columns.Add("AN_LOT_TYPE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                inDataTable = indataSet.Tables.Add("IN_INPUT");
                inDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inDataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));
                inDataTable.Columns.Add("UNMOUNT_TYPE", typeof(string));
                inDataTable.Columns.Add("REMAIN_TYPE", typeof(string));
                inDataTable.Columns.Add("LOSS_CON", typeof(int));
                inDataTable.Columns.Add("LOSS_PRE", typeof(int));
                inDataTable.Columns.Add("REMAIN_QTY", typeof(int));
                inDataTable.Columns.Add("HOLD_CODE", typeof(string));

                inDataTable = indataSet.Tables.Add("IN_OUTPUT");
                inDataTable.Columns.Add("OUT_LOTID", typeof(string));
                inDataTable.Columns.Add("EQPT_END_PSTN_ID", typeof(string));
                inDataTable.Columns.Add("OUT_CSTID", typeof(string));
                inDataTable.Columns.Add("INPUT_QTY", typeof(int));
                inDataTable.Columns.Add("DFCT_QTY", typeof(int));
                inDataTable.Columns.Add("OUTPUT_QTY", typeof(int));
                if (_isSideRollDirctnUse)
                {
                    inDataTable.Columns.Add("HALF_SLIT_SIDE", typeof(string));
                    inDataTable.Columns.Add("EM_SECTION_ROLL_DIRCTN", typeof(string));
                }
                inDataTable.Columns.Add("END_TYPE", typeof(string));
                inDataTable.Columns.Add("HOLD_CODE", typeof(string));

                inDataTable = indataSet.Tables.Add("IN_DEFECT");
                inDataTable.Columns.Add("EQPT_DFCT_CODE", typeof(string));
                inDataTable.Columns.Add("DFCT_QTY", typeof(int));
                inDataTable.Columns.Add("PORT_ID", typeof(string));

                inDataTable = indataSet.Tables.Add("IN_CELL");
                inDataTable.Columns.Add("TABID_S", typeof(int));
                inDataTable.Columns.Add("TABID_E", typeof(int));

                inDataTable = indataSet.Tables.Add("IN_UBM");
                inDataTable.Columns.Add("UBMID", typeof(string));
                inDataTable.Columns.Add("ACCU_USE_COUNT", typeof(Int32));


                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["WORK_TYPE"] = "P";
                newRow["AN_LOT_TYPE"] = Util.GetCondition(cboAnLotType, bAllNull: true) == null || Util.GetCondition(cboAnLotType, bAllNull: true).Equals("") ? _IRREGL_PROD_LOT_TYPE_CODE : Util.GetCondition(cboAnLotType);
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                if (dgQty.GetRowCount() > 0)
                {
                    string sMountPstnID = GetMountPstnID(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_LOTID")));

                    DataTable inInputTable = indataSet.Tables["IN_INPUT"];

                    newRow = inInputTable.NewRow();

                    if (!sMountPstnID.Equals(""))
                    {
                        newRow["EQPT_MOUNT_PSTN_ID"] = sMountPstnID;
                    }
                    newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_LOTID"));

                    if (_LDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                        newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_CSTID"));

                    newRow["UNMOUNT_TYPE"] = (bool)rdoCpl.IsChecked ? "C" : "R";

                    if ((bool)rdoRmn.IsChecked)
                        newRow["REMAIN_TYPE"] = cboHold.SelectedValue.ToString();

                    newRow["LOSS_CON"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_FIX_LOSS_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_FIX_LOSS_QTY")));
                    newRow["LOSS_PRE"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_PRE_PROC_LOSS_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_PRE_PROC_LOSS_QTY")));
                    newRow["REMAIN_QTY"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_RMN_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "M_RMN_QTY")));
                    if (cboInHoldCode.IsEnabled)
                    {
                        newRow["HOLD_CODE"] = cboInHoldCode.SelectedValue.ToString();
                    }

                    inInputTable.Rows.Add(newRow);

                    newRow = null;
                    DataTable inOutputTable = indataSet.Tables["IN_OUTPUT"];

                    newRow = inOutputTable.NewRow();
                    newRow["OUT_LOTID"] = txtLotId.Text;
                    newRow["EQPT_END_PSTN_ID"] = null;

                    if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                        newRow["OUT_CSTID"] = txtCstID.Text;

                    newRow["INPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "EQPT_INPUT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "EQPT_INPUT_QTY")));
                    newRow["DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "DFCT_SUM")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "DFCT_SUM")));
                    newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")));
                    if (_isSideRollDirctnUse)
                    {
                        newRow["HALF_SLIT_SIDE"] = txtSSWD.Tag.ToString().Substring(0, 1);
                        newRow["EM_SECTION_ROLL_DIRCTN"] = txtSSWD.Tag.ToString().Substring(1, 1);
                    }
                    newRow["END_TYPE"] = cboEndType.SelectedValue.ToString();

                    if (cboOutHoldCode.IsEnabled)
                    {
                        newRow["HOLD_CODE"] = cboOutHoldCode.SelectedValue.ToString();
                    }
                    inOutputTable.Rows.Add(newRow);

                    newRow = null;
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_EQPT_END_LOT_NT_L", "IN_EQP,IN_INPUT,IN_OUTPUT,IN_DEFECT,IN_CELL,IN_UBM", "OUT_LOT", (bizResult, bizException) =>
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

        private string GetMountPstnID(string sInputLot)
        {
            try
            {
                string sRet = "";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _EqptID;
                newRow["INPUT_LOTID"] = sInputLot;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL_INPUT_LOTID_CNT", "INDATA", "OUTDATA", inTable);

                if (dtRslt?.Rows?.Count > 0)
                {
                    sRet = Util.NVC(dtRslt.Rows[0]["EQPT_MOUNT_PSTN_ID"]);
                }

                return sRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
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
            //        //if (Util.NVC(row["PRCS_ITEM_CODE"]).Equals("UNIDENTIFIED_QTY")) continue;

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
                    Util.MessageValidation("SFU1878");
                    return bRet;
                }
            }

            double dInputQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "EQPT_INPUT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "EQPT_INPUT_QTY")));
            double dGoodQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")));
            dUnidentifiedQty = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "UNIDENTIFIED_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "UNIDENTIFIED_QTY")));

            //if (dInputQty < 0)
            //{
            //    // 입력오류 : 투입수량이 음수로 처리할 수 없습니다.
            //    Util.MessageValidation("SFU6045");
            //    return bRet;
            //}

            //if (dGoodQty < 0)
            //{
            //    // 입력오류 : 양품(배출)수량이 음수로 처리할 수 없습니다.
            //    Util.MessageValidation("SFU6046");
            //    return bRet;
            //}

            //if (dUnidentifiedQty < 0)
            //{
            //    // 입력오류 : 미확인LOSS가 음수로 처리할 수 없습니다.
            //    Util.MessageValidation("SFU6041");
            //    return bRet;
            //}

            if (_isSideRollDirctnUse)
            {
                if (string.IsNullOrEmpty(txtSSWD.Text))
                {
                    Util.MessageValidation("SFU6030");  // 무지부 방향을 선택하세요.
                    return bRet;
                }
            }

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

            GetDefectInfo();
        }

        private void ClearControls()
        {
            Util.gridClear(dgQty);
            Util.gridClear(dgDefect);

            txtLotId.Text = "";
            txtCstID.Text = "";
            txtProdId.Text = "";
            //txtLotType.Text = "";
            txtStartTime.Text = "";
            txtSSWD.Text = "";
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            listAuth.Add(btnDefectSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            if (_CanChgQty)
            {
                dgQty.Columns["M_FIX_LOSS_QTY"].IsReadOnly = false;
                dgQty.Columns["EQPT_INPUT_QTY"].IsReadOnly = false;
                dgQty.Columns["GOODQTY"].IsReadOnly = false;
            }
            else
            {
                dgQty.Columns["M_FIX_LOSS_QTY"].IsReadOnly = true;
                dgQty.Columns["EQPT_INPUT_QTY"].IsReadOnly = true;
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
                double dPunchQty = Util.NVC(dtTmp.Rows[0]["EQPT_INPUT_QTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["EQPT_INPUT_QTY"]));
                double dGoodQty = Util.NVC(dtTmp.Rows[0]["GOODQTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["GOODQTY"]));
                double dFixLossQty = Util.NVC(dtTmp.Rows[0]["M_FIX_LOSS_QTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["M_FIX_LOSS_QTY"]));
                double dDefectQty = Util.NVC(dtTmp.Rows[0]["DFCT_SUM"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["DFCT_SUM"]));
                //자공정 Loss
                double dCurrProdLossQty = Util.NVC(dtTmp.Rows[0]["M_CURR_PROC_LOSS_QTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["M_CURR_PROC_LOSS_QTY"]));


                double dUnidentifiedQty = dPunchQty - dGoodQty - dDefectQty;
                double dLengLEQty = dInputQty - dFixLossQty - dPunchQty - dCurrProdLossQty; // 자공정 LOSS 추가
                double dRmnQty = dInputQty - dPunchQty;

                dtTmp.Rows[0]["UNIDENTIFIED_QTY"] = dUnidentifiedQty;
                if ((bool)rdoCpl.IsChecked)
                {
                    dtTmp.Rows[0]["M_PRE_PROC_LOSS_QTY"] = dLengLEQty;
                    dtTmp.Rows[0]["M_RMN_QTY"] = 0;
                }
                else
                {
                    dtTmp.Rows[0]["M_PRE_PROC_LOSS_QTY"] = 0;
                    dtTmp.Rows[0]["M_RMN_QTY"] = dRmnQty;
                }

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

        private void ChangeHoldIN(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboHold.IsEnabled == true && cboHold.Text == "Y" && cboInHoldCode.Items.Count !=0)
            {
                cboInHoldCode.IsEnabled = true;
            }
            else
            {
                cboInHoldCode.IsEnabled = false;
            }
                
        }

        private void ChangeHoldOut(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEndType.IsEnabled == true && cboEndType.Text == "Y" &&cboOutHoldCode.Items.Count != 0)
            {
                cboOutHoldCode.IsEnabled = true;
            }
            else
            {
                cboOutHoldCode.IsEnabled = false;
            }
            
        }

        private void cboHoldEnabled(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(cboInHoldCode == null)
            {
                return;
            }
            if (cboHold.IsEnabled == true && cboHold.Text == "Y" && cboInHoldCode.Items.Count != 0)
            {
                    cboInHoldCode.IsEnabled = true;
            }
            else
            {
                    cboInHoldCode.IsEnabled = false;
            }
        }

        private void EndTypeEnabled(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (cboOutHoldCode == null)
            {
                return;
            }
            if (cboEndType.IsEnabled == true && cboEndType.Text == "Y" && cboOutHoldCode.Items.Count != 0)
            {

                    cboOutHoldCode.IsEnabled = true;

            }
            else
            {
                    cboOutHoldCode.IsEnabled = false;
            }
        }
    }
}
