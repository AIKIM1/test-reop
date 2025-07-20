/*************************************************************************************
 Created Date : 2019.10.28
      Creator : 정문교
   Decription : FOL,STK Rework 실적확정
--------------------------------------------------------------------------------------
 [Change History]
  2019.10.28  정문교 : Initial Created.   폴란드3동 & 빈강용 조립 공정에서 사용
                                          ASSY004_COM_CONFIRM Copy ASSY004_060_CONFIRM
  2019.11.27  정문교 : 공급량을 작업자가 입력하는 것에서 양품, 불량~물청 수량 등록 시 실시간으로 공급량 업데이트로 변경
  2024.12.11  박성진   E20240923-000237   요청자 미선택하고 실적확정 시 에러발생 조치 (MES 2.0 Catch-Up)
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

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_060_CONFIRM.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_060_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _ProcID = string.Empty;
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private string _WipStat = string.Empty;

        private string _ShftName = string.Empty;
        private string _ShftID = string.Empty;
        private string _WorkerName = string.Empty;
        private string _WorkerID = string.Empty;
        private string _IRREGL_PROD_LOT_TYPE_CODE = string.Empty;
        private string _PROD_LOT_OPER_MODE = string.Empty;

        private bool _CanChgQty = false;
        private string _IsSTK = string.Empty;
        private int _EqptInputQty;

        private bool bShowMsg = false;

        private bool bSave = false;

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

        public ASSY004_060_CONFIRM()
        {
            InitializeComponent();
        }

        private void InitializeUserControls()
        {
            Util.gridClear(dgQty);
            Util.gridClear(dgDefect);
            Util.gridClear(dgBox);

            //txtShift.Text = _ShftName;
            //txtShift.Tag = _ShftID;
            //txtWorker.Text = _WorkerName;
            //txtWorker.Tag = _WorkerID;

            //txtUserName.Text = string.Empty;
            //txtUserName.Tag = string.Empty;
            //txtReqNote.Text = string.Empty;
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
            dtTmp.Rows.Add(dtRow);

            dgQty.ItemsSource = DataTableConverter.Convert(dtTmp);
        }

        private void InitCombo()
        {
        }

        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _ProcID = Util.NVC(tmps[0]);
            _LineID = Util.NVC(tmps[1]);
            _EqptID = Util.NVC(tmps[2]);
            _LotID = Util.NVC(tmps[3]);
            _WipSeq = Util.NVC(tmps[4]);

            _LDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[5]);
            _UNLDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[6]);

            _ShftName = Util.NVC(tmps[7]);
            _ShftID = Util.NVC(tmps[8]);
            _WorkerName = Util.NVC(tmps[9]);
            _WorkerID = Util.NVC(tmps[10]);

            _CanChgQty = (bool)tmps[11];
            _IsSTK = Util.NVC(tmps[12]);
            _EqptInputQty = Util.NVC_Int(tmps[13]);

            //if (_CanChgQty)
            //{
            //    dgQty.Columns["INPUTQTY"].IsReadOnly = false;
            //    dgQty.Columns["PRE_SECTION_QTY"].IsReadOnly = false;
            //    dgQty.Columns["AFTER_SECTION_QTY"].IsReadOnly = false;
            //}
            //else
            //{
            //    dgQty.Columns["INPUTQTY"].IsReadOnly = true;
            //    dgQty.Columns["PRE_SECTION_QTY"].IsReadOnly = true;
            //    dgQty.Columns["AFTER_SECTION_QTY"].IsReadOnly = true;
            //}
        }

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            InitializeUserControls();
            InitializeGrid();
            InitCombo();
            SetControl();

            GetAllData();
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
            
            string messageCode = dUnidentifiedQty < 0 ? "SFU3746" : "SFU2039";
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

                double dUnidentifiedQty = dInputQty - dGoodQty - dDefectQty - (dPreSectionQty + dAftSectionQty) + dReInputQty;

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

                //    double dUnidentifiedQty = dInputQty - dGoodQty - dDefectQty - (dPreSecQty + dAftSecQty) + dReInputQty;

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

        private void btnWorker_Click(object sender, RoutedEventArgs e)
        {
            CMM001.Popup.CMM_SHIFT_USER2 wndPopup = new CMM001.Popup.CMM_SHIFT_USER2();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(_LineID);
                Parameters[3] = _ProcID;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(_EqptID);
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));                
            }
        }

        private void txtUserName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                popupUser();
            }
        }

        private void btnReqUser_Click(object sender, RoutedEventArgs e)
        {
            popupUser();
        }
        #endregion

        #region Mehod

        #region [BizCall]

        private void GetWrokCalander()
        {
            try
            {
                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));


                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = _LineID;
                Indata["PROCID"] = _ProcID;

                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORK_CALENDAR_WRKR_INFO", "RQSTDT", "RSLTDT", IndataTable);

                if (result.Rows.Count > 0)
                {
                    txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                    txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                    txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                    txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                }
                else
                {
                    txtWorker.Text = string.Empty;
                    txtWorker.Tag = string.Empty;
                    txtShift.Text = string.Empty;
                    txtShift.Tag = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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
                        //if (dgQty.Columns.Contains("INPUTQTY") && dtRslt.Columns.Contains("INPUTQTY"))
                        //    DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "INPUTQTY", dtRslt.Rows[0]["INPUTQTY"]);
                        if (dgQty.Columns.Contains("INPUTQTY") && dtRslt.Columns.Contains("INPUTQTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "INPUTQTY", _EqptInputQty.ToString());

                        if (dgQty.Columns.Contains("PRE_SECTION_QTY") && dtRslt.Columns.Contains("PRE_SECTION_QTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "PRE_SECTION_QTY", dtRslt.Rows[0]["PRE_SECTION_QTY"]);
                        if (dgQty.Columns.Contains("AFTER_SECTION_QTY") && dtRslt.Columns.Contains("AFTER_SECTION_QTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "AFTER_SECTION_QTY", dtRslt.Rows[0]["AFTER_SECTION_QTY"]);
                        if (dgQty.Columns.Contains("UNIDENTIFIED_QTY") && dtRslt.Columns.Contains("UNIDENTIFIED_QTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "UNIDENTIFIED_QTY", dtRslt.Rows[0]["UNIDENTIFIED_QTY"]);
                        if (dgQty.Columns.Contains("RE_INPUT_QTY") && dtRslt.Columns.Contains("RE_INPUT_QTY"))
                            DataTableConverter.SetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "RE_INPUT_QTY", dtRslt.Rows[0]["RE_INPUT_QTY"]);
                    }

                    txtLotId.Text = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                    txtProdId.Text = Util.NVC(dtRslt.Rows[0]["PRODID"]);
                    txtStartTime.Text = Util.NVC(dtRslt.Rows[0]["WIPDTTM_ST"]);
                    txtEndTime.Text = Util.NVC(dtRslt.Rows[0]["EQPT_END_DTTM"]);
                    txtRemark.Text = Util.NVC(dtRslt.Rows[0]["WIP_NOTE"]);

                    txtAnLotType.Text = Util.NVC(dtRslt.Rows[0]["IRREGL_PROD_LOT_TYPE_NAME"]);
                    txtLotOperMode.Text = Util.NVC(dtRslt.Rows[0]["PROD_LOT_OPER_MODE_NAME"]);

                    _IRREGL_PROD_LOT_TYPE_CODE = Util.NVC(dtRslt.Rows[0]["IRREGL_PROD_LOT_TYPE_CODE"]);
                    _PROD_LOT_OPER_MODE = Util.NVC(dtRslt.Rows[0]["PROD_LOT_OPER_MODE"]);
                    
                    // Caldate Lot의 Caldate로...
                    if (Util.NVC(dtRslt.Rows[0]["CALDATE_LOT"]).Trim().Equals(""))
                    {
                        sCaldate = Util.NVC(dtRslt.Rows[0]["NOW_CALDATE_YMD"]);
                        dtCaldate = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["NOW_CALDATE"]));
                    }
                    else
                    {
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
                ///////////////////////////////////////////////////////////////////// DataSet
                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("PROD_LOTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));

                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInput.Columns.Add("INPUT_LOTID", typeof(string));

                DataTable inOutput = indataSet.Tables.Add("IN_OUTPUT");
                inOutput.Columns.Add("INPUT_QTY", typeof(decimal));
                inOutput.Columns.Add("DFCT_QTY", typeof(decimal));
                inOutput.Columns.Add("OUTPUT_QTY", typeof(decimal));
                inOutput.Columns.Add("BTN_QTY_PRE", typeof(decimal));
                inOutput.Columns.Add("BTN_QTY_AFT", typeof(decimal));
                inOutput.Columns.Add("REINPUT_QTY", typeof(decimal));
                inOutput.Columns.Add("WIPNOTE", typeof(string));
                inOutput.Columns.Add("CRRT_NOTE", typeof(string));
                inOutput.Columns.Add("REQ_USERID", typeof(string));

                DataTable inProcWork = indataSet.Tables.Add("IN_PROC_WRKR");
                inProcWork.Columns.Add("SHIFT", typeof(string));
                inProcWork.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                inProcWork.Columns.Add("WRK_USERID", typeof(string));
                inProcWork.Columns.Add("WRK_USER_NAME", typeof(string));

                ///////////////////////////////////////////////////////////////////// 바인딩
                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = txtLotId.Text;                
                newRow["USERID"] = LoginInfo.USERID;
                inData.Rows.Add(newRow);

                if (dgQty.GetRowCount() > 0)
                {                    
                    newRow = inOutput.NewRow();
                    newRow["INPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "INPUTQTY")).Equals("") ? 0 : decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "INPUTQTY")));
                    newRow["DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "DFCT_SUM")).Equals("") ? 0 : decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "DFCT_SUM")));
                    newRow["OUTPUT_QTY"] = 0;// Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "GOODQTY")));
                    newRow["BTN_QTY_PRE"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "PRE_SECTION_QTY")).Equals("") ? 0 : decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "PRE_SECTION_QTY")));
                    newRow["BTN_QTY_AFT"] = Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "AFTER_SECTION_QTY")).Equals("") ? 0 : decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgQty.Rows[dgQty.TopRows.Count].DataItem, "AFTER_SECTION_QTY")));
                    newRow["WIPNOTE"] = txtRemark.Text;
                    newRow["CRRT_NOTE"] = txtReqNote.Text;
                    newRow["REQ_USERID"] = txtUserName.Tag;

                    inOutput.Rows.Add(newRow);
                }

                newRow = inProcWork.NewRow();
                newRow["SHIFT"] = txtShift.Tag;
                //newRow["WIPDTTM_ED"] = null;
                newRow["WRK_USERID"] = txtWorker.Tag;
                newRow["WRK_USER_NAME"] = txtWorker.Text;
                inProcWork.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_FD_L", "INDATA,IN_INPUT,IN_OUTPUT,IN_PROC_WRKR", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HideLoadingIndicator();

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
                        HideLoadingIndicator();
                        Util.MessageException(ex);
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
                DataTable inData= indataSet.Tables["INDATA"];

                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;
                inData.Rows.Add(newRow);

                DataTable inResn = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = inResn.NewRow();
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

                    inResn.Rows.Add(newRow);
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

        #endregion

        #region [Validation]
        private bool CanSave(out double dUnidentifiedQty)
        {
            dUnidentifiedQty = 0;

            bool bRet = false;

            if (bShowMsg)
                return bRet;

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

            if (dUnidentifiedQty != 0)
            {
                // 양품수량과 불량수량의 합이 투입수량과 맞지 않습니다.
                Util.MessageValidation("SFU1723");
                return bRet;
            }

            if (_CanChgQty)
            {
                if (string.IsNullOrWhiteSpace(txtWorker.Text) || string.IsNullOrWhiteSpace(txtWorker.Tag.ToString()))
                {
                    // 작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843", (action) => { txtWorker.Focus(); });
                    return bRet;
                }

                if (string.IsNullOrWhiteSpace(txtUserName.Text) || string.IsNullOrWhiteSpace(Util.NVC(txtUserName.Tag)))
                {
                    // 요청자를 입력 하세요.
                    Util.MessageValidation("SFU3451", (action) => { txtUserName.Focus(); });
                    return bRet;
                }

                if (string.IsNullOrWhiteSpace(txtReqNote.Text))
                {
                    // 사유를 입력하세요.
                    Util.MessageValidation("SFU1594", (action) => { txtReqNote.Focus(); });
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
                // 불량 항목이 없습니다.
                Util.MessageValidation("SFU1578");
                return bRet;
            }

            if (txtLotId.Text.Trim().Length < 1)
            {
                // LOT 정보가 없습니다.
                Util.MessageValidation("SFU1195");
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        #endregion

        #region [Function]
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
            InitializeUserControls();
            InitializeGrid();

            GetWrokCalander();
            GetLotInfo();
            GetBoxList();
            GetDefectInfo();
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            listAuth.Add(btnDefectSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

                dtSrc = DataTableConverter.Convert(dgBox.ItemsSource);
                dtTgt = DataTableConverter.Convert(dgQty.ItemsSource);

                if (dtTgt == null || dtTgt.Rows.Count < 1) return;

                if (dtSrc != null && dtSrc.Rows.Count > 0)
                {
                    dtTgt.Rows[0]["GOODQTY"] = dtSrc.Compute("SUM(WIPQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(WIPQTY)", string.Empty).ToString());
                }
                else
                {
                    dtTgt.Rows[0]["GOODQTY"] = 0;
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
                //double dInputQty = Util.NVC(dtTmp.Rows[0]["INPUTQTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["INPUTQTY"]));
                double dInputQty = 0;
                double dPreSecQty = Util.NVC(dtTmp.Rows[0]["PRE_SECTION_QTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["PRE_SECTION_QTY"]));
                double dAftSecQty = Util.NVC(dtTmp.Rows[0]["AFTER_SECTION_QTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["AFTER_SECTION_QTY"]));
                double dReInputQty = Util.NVC(dtTmp.Rows[0]["RE_INPUT_QTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTmp.Rows[0]["RE_INPUT_QTY"]));

                dInputQty = dGoodQty + dDefectQty;
                double dUnidentifiedQty = dInputQty - dGoodQty - dDefectQty - (dPreSecQty + dAftSecQty) + dReInputQty;

                dtTmp.Rows[0]["INPUTQTY"] = dInputQty;
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

        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_SHIFT_USER2 wndPopup = sender as CMM001.Popup.CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker.Tag = Util.NVC(wndPopup.USERID);
            }
        }

        private void popupUser()
        {
            CMM001.Popup.CMM_PERSON popUser = new CMM001.Popup.CMM_PERSON();
            popUser.FrameOperation = FrameOperation;

            object[] Parameters = new object[1];
            Parameters[0] = txtUserName.Text;
            C1WindowExtension.SetParameters(popUser, Parameters);

            popUser.Closed += new EventHandler(popUser_Closed);

            this.Dispatcher.BeginInvoke(new Action(() => popUser.ShowModal()));
        }

        private void popUser_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_PERSON popup = sender as CMM001.Popup.CMM_PERSON;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = popup.USERNAME;
                txtUserName.Tag = popup.USERID;

                txtReqNote.Focus();
            }
        }
        #endregion

        #endregion
                
    }
}
