/*************************************************************************************
 Created Date : 2020.10.12
      Creator : Kang Dong Hee
   Decription : 활성화 공정진척 - Cell 상세정보
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.12  NAME : Initial Created
**************************************************************************************/
using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Windows.Media;
using System.Windows.Input;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Popup;
using System.Collections.Generic;
using System.Globalization;
using LGC.GMES.MES.CMM001;
using System.Linq;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.FCS002.Controls
{
    /// <summary>
    /// UcFCSDefectDetail.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcFCSDefectDetail : UserControl
    {
        #region Declaration

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public Button ButtonProduction { get; set; }

        public DataTable DtEquipment { get; set; }
        public DataRowView DvProductLot { get; set; }
        public string EquipmentSegmentCode { get; set; }
        public string ProcessCode { get; set; }
        public string ProcessName { get; set; }
        public string EquipmentCode { get; set; }
        public string EquipmentName { get; set; }
        public string LdrLotIdentBasCode { get; set; }
        public string UnldrLotIdentBasCode { get; set; }

        public bool bProductionUpdate { get; set; }

        public C1DataGrid DgDefectDetail { get; set; }

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        bool _isResnCountUse = false;

        SolidColorBrush greenBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFDAFFF6"));

        public UcFCSDefectDetail()
        {
            InitializeComponent();

            InitializeControls();
            //SetControl();
            SetButtons();
            //SetControlVisibility();
            SetPrivateVariable();
        }

        #endregion

        #region Initialize

        public void InitializeControls()
        {
            this.RegisterName("greenBrush", greenBrush);
        }

        private void SetButtons()
        {

        }

        private void SetControl()
        {
            recEquipment.Fill = greenBrush;
            txtEquipment.Text = "[" + EquipmentCode + "] " + EquipmentName;

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(opacityAnimation, "greenBrush");
            Storyboard.SetTargetProperty(
                opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(opacityAnimation);
            storyboard.Begin(this);
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetControlClear()
        {
            txtGroupLotID.Text = string.Empty;
            txtLotType.Text = string.Empty;
            txtWorkDate.Text = string.Empty;

            Util.gridClear(dgDefectDetail);
        }

        private void SetControlVisibility()
        {
            dgDefectDetail.Visibility = Visibility.Visible;
            txtGroupLotID.Visibility = Visibility.Visible;
            txtLotType.Visibility = Visibility.Visible;
            txtWorkDate.Visibility = Visibility.Visible;
        }

        private void SetPrivateVariable()
        {
            _isResnCountUse = IsAreaCommonCodeUse("RESN_COUNT_USE_YN", ProcessCode);
        }

        #endregion

        #region Event

        #region =============================공통

        /// <summary>
        /// 버전 팝업 
        /// </summary>
        private void btnVersion_Click(object sender, RoutedEventArgs e)
        {
            PopupVersion();
        }

        private void btnProductionUpdate_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region = NG Group Lot List
        private void dgNGGroupLotList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                if (string.Equals(e.Cell.Column.Name, "COUNTQTY") && !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));

                                if ((string.Equals(e.Cell.Column.Name, "COUNTQTY") || string.Equals(e.Cell.Column.Name, "DFCT_TAG_QTY") || string.Equals(e.Cell.Column.Name, "RESN_TOT_CHK") || string.Equals(e.Cell.Column.Name, "RESNQTY")) &&
                                        (string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"), "Y") || string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TOP_LOSS_BAS_DFCT_APPLY_FLAG"), "Y")))
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));

                                if (string.Equals(e.Cell.Column.Name, "RESN_TOT_CHK") && string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ACTID"), "CHARGE_PROD_LOT"))
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));
                            }
                        }
                    }
                }));
            }

        }

        private void dgNGGroupLotList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            e.Cell.Presenter.Background = null;
                        }
                    }
                }));
            }

        }

        private void dgNGGroupLotList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                if (string.Equals(e.Column.Name, "COUNTQTY") &&
                    !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
                    e.Cancel = true;

                if ((string.Equals(e.Column.Name, "COUNTQTY") || string.Equals(e.Column.Name, "DFCT_TAG_QTY") || string.Equals(e.Column.Name, "RESN_TOT_CHK") || string.Equals(e.Column.Name, "RESNQTY")) &&
                        (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"), "Y") || string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "TOP_LOSS_BAS_DFCT_APPLY_FLAG"), "Y")))
                    e.Cancel = true;

                if (string.Equals(e.Column.Name, "RESN_TOT_CHK") && string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "ACTID"), "CHARGE_PROD_LOT"))
                    e.Cancel = true;

            }

        }

        private void dgNGGroupLotList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                //// Top Loss 기본 수량 변경 로직 적용 (단순에 차감, 증가분 만큼만 움직이도록 변경)
                //if (string.Equals(dataGrid.Tag, "DEFECT_TOP") && !string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WEB_BREAK_FLAG"), "Y"))
                //{
                //    decimal dTopLossQty = Util.NVC_Decimal(DvProductLot["INPUT_TOP_QTY"]) - Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[_util.GetDataGridRowIndex(dataGrid, "WEB_BREAK_FLAG", "Y")].DataItem, "FRST_AUTO_RSLT_RESNQTY"));
                //    decimal dRemainQty = SumDefectQtyCoating(dataGrid, e.Cell.Row.Index);

                //    if (Util.NVC_Decimal(e.Cell.Value) > (dTopLossQty - dRemainQty))
                //    {
                //        DataTableConverter.SetValue(e.Cell.Row.DataItem, "RESNQTY", (dTopLossQty - dRemainQty));

                //        if (_util.GetDataGridRowIndex(dataGrid, "TOP_LOSS_BAS_DFCT_APPLY_FLAG", "Y") >= 0)
                //            DataTableConverter.SetValue(dataGrid.Rows[_util.GetDataGridRowIndex(dataGrid, "TOP_LOSS_BAS_DFCT_APPLY_FLAG", "Y")].DataItem, "RESNQTY", 0);
                //    }
                //    else
                //    {
                //        if (_util.GetDataGridRowIndex(dataGrid, "TOP_LOSS_BAS_DFCT_APPLY_FLAG", "Y") >= 0)
                //            DataTableConverter.SetValue(dataGrid.Rows[_util.GetDataGridRowIndex(dataGrid, "TOP_LOSS_BAS_DFCT_APPLY_FLAG", "Y")].DataItem, "RESNQTY", (dTopLossQty - dRemainQty) - (Util.NVC_Decimal(e.Cell.Value)));
                //    }
                //}

                //if (e.Cell.Column.Index == dataGrid.Columns["RESNQTY"].Index)
                //{
                //    if (Util.NVC_Decimal(e.Cell.Value) == 0 &&
                //        Convert.ToBoolean(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESN_TOT_CHK")) == false)
                //    {
                //        SumDefectQtyCoating();
                //        //dgProductResult_C.Refresh(false);
                //        return;
                //    }

                //    for (int i = 0; i < dataGrid.Rows.Count; i++)
                //    {
                //        if (Convert.ToBoolean(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESN_TOT_CHK")) == true)
                //        {
                //            DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESN_TOT_CHK", false);

                //            if (e.Cell.Row.Index != i)
                //                DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", 0);
                //        }
                //    }

                //    if (Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNQTY")) ==
                //        Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult_C.Rows[dgProductResult_C.TopRows.Count].DataItem, "INPUT_BACK_QTY")))
                //        DataTableConverter.SetValue(e.Cell.Row.DataItem, "RESN_TOT_CHK", true);

                //    _isChangeWipReason = true;

                //    SumDefectQtyCoating();
                //    dgProductResult_C.Refresh(false);
                //}
            }
        }

        private void dgNGGroupLotList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null && dataGrid.CurrentCell != null && dataGrid.CurrentCell.Presenter != null)
            {
                if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
                {
                    dataGrid.EndEdit(true);
                }
                else if (e.Key == Key.Delete)
                {
                    if (dataGrid.CurrentCell.Column.IsReadOnly == false)
                    {
                        DataTableConverter.SetValue(dataGrid.CurrentCell.Row.DataItem, dataGrid.CurrentCell.Column.Name, 0);
                        dataGrid.BeginEdit(dataGrid.CurrentCell);
                        dataGrid.EndEdit(true);

                        //DataTableConverter.SetValue(dataGrid.CurrentCell.Row.DataItem, dataGrid.CurrentCell.Column.Name, DBNull.Value);

                        if (dataGrid.CurrentCell != null && dataGrid.CurrentCell.Presenter != null)
                        {
                            dataGrid.CurrentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                            dataGrid.CurrentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            dataGrid.CurrentCell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }
                }
                else if (!char.IsControl((char)e.Key) && !char.IsDigit((char)e.Key))
                {
                    if (dataGrid.CurrentCell.Column.IsReadOnly == false && dataGrid.CurrentCell.IsEditable == false)
                        dataGrid.BeginEdit(dataGrid.CurrentCell.Row.Index, dataGrid.CurrentCell.Column.Index);
                }
            }
        }

        private void dgNGGroupLotList_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                {
                    C1DataGrid dataGrid = sender as C1DataGrid;

                    C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                    CheckBox cb = cell.Presenter.Content as CheckBox;

                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;

                    if (dataGrid != null)
                    {
                        if (cb.IsChecked == true)
                        {
                            Util.MessageConfirm("SFU5128", (vResult) =>         // %1에 전체 수량을 등록 하시겠습니까?
                            {
                                if (vResult == MessageBoxResult.OK)
                                {
                                    // BACK단선은 전체 체크 시 설비에서 올라온 단선BASE수량만 변경하도록 변경 (실수로 체크 시 TOP/BACK수량이 투입량에 반영되어 크게 왜곡 발생) [2019-12-04]
                                    // 코터 공정 단선 조정 시 투입량 변경으로 전체 불량 등록 시 단선 수 차감하고 등록하도록 수정 [2019-01-13]
                                    decimal dWebBreakQty = Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[_util.GetDataGridRowIndex(dataGrid, "WEB_BREAK_FLAG", "Y")].DataItem, "FRST_AUTO_RSLT_RESNQTY"));

                                    //if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[idx].DataItem, "WEB_BREAK_FLAG"), "Y"))
                                    //{
                                    //    DataTableConverter.SetValue(dataGrid.Rows[idx].DataItem, "RESNQTY",
                                    //                    DataTableConverter.GetValue(dataGrid.Rows[idx].DataItem, "FRST_AUTO_RSLT_RESNQTY"));
                                    //}
                                    //else
                                    //{
                                    //    DataTableConverter.SetValue(dataGrid.Rows[idx].DataItem, "RESNQTY",
                                    //                    Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult_C.Rows[dgProductResult_C.TopRows.Count].DataItem, "INPUT_BACK_QTY")) - dWebBreakQty);
                                    //}

                                    for (int i = 0; i < dataGrid.Rows.Count; i++)
                                    {
                                        if (i != idx)
                                        {
                                            DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESN_TOT_CHK", false);
                                            DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", 0);
                                        }
                                    }
                                    SumDefectQty();
                                    //dgProductResult_C.Refresh(false);
                                }
                                else
                                {
                                    cb.IsChecked = false;
                                    DataTableConverter.SetValue(dataGrid.Rows[e.Row.Index].DataItem, "RESN_TOT_CHK", false);
                                }

                            }, new object[] { DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "RESNNAME") });
                        }
                        else
                        {
                            DataTableConverter.SetValue(dataGrid.Rows[idx].DataItem, "RESNQTY", 0);
                            SumDefectQty();
                            //dgProductResult_C.Refresh(false);
                        }
                    }
                }
            }));
        }
        #endregion

        #endregion

        #region Mehod

        #region [외부호출]
        public void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        public void SelectProductionResult()
        {
            SetControl();
            SetControlClear();
            SetControlVisibility();

            // 불량항목별 Cell ID 상세정보
            //SelectDefect(dgDefectDetail);

        }

        #endregion

        #region [BizCall]
        #region =============================공통
        private bool IsAreaCommonCodeUse(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["COM_TYPE_CODE"] = sCodeType;
                newRow["COM_CODE"] = sCodeName;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return false;
        }

        /// <summary>
        /// 불량항목별 Cell ID 상세정보
        /// </summary>
        private void SelectDefect(C1DataGrid dg, string Resnposition = null, string sCode = null)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));
                inTable.Columns.Add("RESNPOSITION", typeof(string));          // TOP/BACK
                inTable.Columns.Add("CODE", typeof(string));                  // MIX 세정 Option

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = Util.NVC(DvProductLot["LOTID"]);

                if (!string.IsNullOrWhiteSpace(sCode))
                {
                    newRow["CODE"] = sCode;
                }
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_ACTIVITYREASON_ELEC", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dg, bizResult, null, true);

                        SetCauseTitle(dg);
                        SumDefectQty();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
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

        private void SetResultInfo()
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(Int32));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = DvProductLot["LOTID"];
                newRow["WIPSEQ"] = DvProductLot["WIPSEQ"];
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_INFO_CT", "INDATA", "RSLTDT", inTable);

                // 설비수량
                if (dtResult != null && dtResult.Rows.Count > 1)
                {
                    DataRow[] dr = dtResult.Select("ORDER_NO = 1");
                    Util.GridSetData(dgNGGroupLotList, dr.CopyToDataTable(), null);
                }
                else
                {
                    Util.GridSetData(dgNGGroupLotList, dtResult, null);
                }
                // 실적수량
                Util.GridSetData(dgNGGroupLotList, dtResult, null);

                _util.SetDataGridMergeExtensionCol(dgNGGroupLotList, new string[] { "LOTID", "OUT_CSTID", "PR_LOTID", "CSTID" }, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        #endregion

        #endregion

        #region [Func]
        #region =============================공통
        private void SetUnitFormatted()
        {
            if (string.IsNullOrWhiteSpace(DvProductLot["UNIT_CODE"].ToString())) return;

            string sFormatted = GetUnitFormatted(DvProductLot["UNIT_CODE"].ToString());
        }
        private string GetUnitFormatted(string sUnit)
        {
            string sFormatted = "0";
            if (!string.IsNullOrWhiteSpace(sUnit))
            {
                switch (sUnit)
                {
                    case "KG":
                        sFormatted = "F3";
                        break;

                    case "M":
                        sFormatted = "F2";
                        break;

                    case "EA":
                        sFormatted = "F1";
                        break;
                    default:
                        sFormatted = "F0";
                        break;
                }
            }
            return sFormatted;
        }

        private void SetCauseTitle(C1DataGrid dg)
        {
            int causeqty = 0;

            if (dg.ItemsSource != null)
            {
                //DataTable dt = (dg.ItemsSource as DataView).Table;
                //for (int i = dg.TopRows.Count; i < dt.Rows.Count + dgWipReason.TopRows.Count; i++)
                //{
                //    string resnname = DataTableConverter.GetValue(dg.Rows[i].DataItem, "RESNNAME").ToString();
                //    if (resnname.IndexOf("*") == 1)
                //        causeqty++;
                //}
                //if (causeqty > 0)
                //{
                //    if (ProcessCode == Process.MIXING)
                //    {
                //        lblTop.Margin = new Thickness(300, 0, 8, 0);
                //        lblTop.Visibility = Visibility.Collapsed;
                //    }

                //    if (dg.Name.ToString() != "dgWipReasonBack_C")
                //    {
                //        if (lblTop_C.Visibility == Visibility.Visible)
                //        {
                //            lblTop_C.Text = ObjectDic.Instance.GetObjectName("Top(*는 타공정 귀속)");
                //        }
                //        else
                //        {
                //            lblTop_C.Visibility = Visibility.Visible;
                //            lblTop_C.Text = ObjectDic.Instance.GetObjectName("(*는 타공정 귀속)");
                //        }
                //    }
                //    else
                //    {
                //        lblBack_C.Text = ObjectDic.Instance.GetObjectName("Back(*는 타공정 귀속)");
                //    }
                //}
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

        private void SetNGGroupLotListResult()
        {
            if (DvProductLot["WIPSTAT"].ToString() == "WAIT") return;

            SetResultInfo();

            // UNIT별로 FORMAT
            SetUnitFormatted();
        }

        private decimal SumDefectQty(C1DataGrid dataGrid, int iRow)
        {
            decimal dSumQty = 0;

            if (dataGrid.Rows.Count > 0)
                for (int i = 0; i < dataGrid.GetRowCount(); i++)
                    if (iRow != i)
                        if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY"))))
                            if (!string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "TOP_LOSS_BAS_DFCT_APPLY_FLAG"), "Y"))
                                if (!string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "WEB_BREAK_FLAG"), "Y"))
                                    dSumQty += (Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY")));

            return dSumQty;
        }

        private void SumDefectQty()
        {
            if (!string.Equals(DvProductLot["WIPSTAT"], Wip_State.END))
                return;

            decimal dTopInputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgNGGroupLotList.Rows[dgNGGroupLotList.TopRows.Count].DataItem, "INPUT_TOP_QTY"));
            decimal dBackInputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgNGGroupLotList.Rows[dgNGGroupLotList.TopRows.Count].DataItem, "INPUT_BACK_QTY"));
            decimal dBackWebBreakQty = GetDiffWebBreakQty(dgNGGroupLotList, "DEFECT_LOT", "BACK");

            for (int i = 0; i < dgNGGroupLotList.GetRowCount(); i++)
            {
                //if (!string.Equals(DataTableConverter.GetValue(dgProductResult_C.Rows[i + dgProductResult_C.TopRows.Count].DataItem, "CHG_BLOCK_FLAG"), "Y"))
                //{
                //    DataTableConverter.SetValue(dgProductResult_C.Rows[i + dgProductResult_C.TopRows.Count].DataItem, "DTL_TOP_DEFECT", dTopDefectQty);
                //    DataTableConverter.SetValue(dgProductResult_C.Rows[i + dgProductResult_C.TopRows.Count].DataItem, "DTL_TOP_LOSS", dTopLossQty);
                //    DataTableConverter.SetValue(dgProductResult_C.Rows[i + dgProductResult_C.TopRows.Count].DataItem, "DTL_TOP_CHARGEPRD", dTopChargeProdQty);
                //    DataTableConverter.SetValue(dgProductResult_C.Rows[i + dgProductResult_C.TopRows.Count].DataItem, "DTL_TOP_DEFECT_SUM", dTopTotalQty);
                //    DataTableConverter.SetValue(dgProductResult_C.Rows[i + dgProductResult_C.TopRows.Count].DataItem, "DTL_BACK_DEFECT", dBackDefectQty);
                //    DataTableConverter.SetValue(dgProductResult_C.Rows[i + dgProductResult_C.TopRows.Count].DataItem, "DTL_BACK_LOSS", dBackLossQty);
                //    DataTableConverter.SetValue(dgProductResult_C.Rows[i + dgProductResult_C.TopRows.Count].DataItem, "DTL_BACK_CHARGEPRD", dBackChargeProdQty);
                //    DataTableConverter.SetValue(dgProductResult_C.Rows[i + dgProductResult_C.TopRows.Count].DataItem, "DTL_BACK_DEFECT_SUM", dBackTotalQty);
                //    DataTableConverter.SetValue(dgProductResult_C.Rows[i + dgProductResult_C.TopRows.Count].DataItem, "GOODQTY", ((dBackInputQty - dBackWebBreakQty) - dBackTotalQty));
                //    DataTableConverter.SetValue(dgProductResult_C.Rows[i + dgProductResult_C.TopRows.Count].DataItem, "GOODQTY2", (((dBackInputQty - dBackWebBreakQty) - dBackTotalQty) * dLaneQty));
                //    DataTableConverter.SetValue(dgProductResult_C.Rows[i + dgProductResult_C.TopRows.Count].DataItem, "INPUT_TOP_QTY", dTopTotalQty);
                //    DataTableConverter.SetValue(dgProductResult_C.Rows[i + dgProductResult_C.TopRows.Count].DataItem, "INPUT_BACK_QTY", dBackInputQty - dBackWebBreakQty);
                //}
                //else
                //{
                //    DataTableConverter.SetValue(dgProductResult_C.Rows[i + dgProductResult_C.TopRows.Count].DataItem, "GOODQTY2",
                //        Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult_C.Rows[i + dgProductResult_C.TopRows.Count].DataItem, "GOODQTY")) * dLaneQty);
                //}
            }
        }

        private decimal GetSumDefectQty(C1DataGrid dataGrid, string sActId, string sResnPosition)
        {
            // 요청으로 0.5반영 로직 제거 (실적 전송 시만 TOP은 0.5로 변경)
            decimal dSumQty = 0;

            if (dataGrid.Rows.Count > 0)
                for (int i = 0; i < dataGrid.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY"))))
                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                            if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "ACTID"), sActId))
                                dSumQty += (Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY")));
            //* (string.Equals(sResnPosition, "TOP") ? 0.5M : 1M));
            return dSumQty;
        }

        private decimal GetDiffWebBreakQty(C1DataGrid dataGrid, string sActId, string sResnPosition)
        {
            // 요청으로 0.5반영 로직 제거 (실적 전송 시만 TOP은 0.5로 변경)
            decimal dSumQty = 0;

            if (dataGrid.Rows.Count > 0)
                for (int i = 0; i < dataGrid.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY"))))
                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                            if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "WEB_BREAK_FLAG"), "Y"))
                                if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "ACTID"), sActId))
                                    dSumQty += (Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "FRST_AUTO_RSLT_RESNQTY")) -
                                        Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY")));
            //* (string.Equals(sResnPosition, "TOP") ? 0.5M : 1M));
            return dSumQty;
        }

        #endregion;

        #region[[Validation]

        #region =============================공통

        #endregion

        #endregion

        #region [팝업]
        #region =============================공통
        private void PopupVersion()
        {
            CMM_ELECRECIPE popupVersion = new CMM_ELECRECIPE { FrameOperation = FrameOperation };

            if (popupVersion != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = Util.NVC(DvProductLot["PRODID"]);
                Parameters[1] = ProcessCode;
                Parameters[2] = LoginInfo.CFG_AREA_ID;
                Parameters[3] = EquipmentCode;
                Parameters[4] = Util.NVC(DvProductLot["LOTID"]); 
                Parameters[5] = "Y";    // 전극 버전 확정 여부
                C1WindowExtension.SetParameters(popupVersion, Parameters);

                popupVersion.Closed += new EventHandler(PopupVersion_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupVersion.ShowModal()));
            }

        }

        private void PopupVersion_Closed(object sender, EventArgs e)
        {
            CMM_ELECRECIPE popup = sender as CMM_ELECRECIPE;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                //if (Util.NVC_Decimal(txtLaneQty.Value) != Util.NVC_Decimal(popup._ReturnLaneQty))
                //{
                //    txtVersion.Text = popup._ReturnRecipeNo;
                //    txtLaneQty.Value = Convert.ToDouble(popup._ReturnLaneQty);
                //    txtCurLaneQty.Value = getCurrLaneQty(Util.NVC(DvProductLot["LOTID"]));

                //    if (ProcessCode == Process.MIXING)
                //    {
                //        SumDefectQtyMixing();
                //    }
                //    else if (ProcessCode == Process.COATING)
                //    {
                //        if (dgProductResult_C.GetRowCount() > 0)
                //            for (int i = 0; i < dgProductResult_C.GetRowCount(); i++)
                //                DataTableConverter.SetValue(dgProductResult_C.Rows[i + dgProductResult_C.TopRows.Count].DataItem, "LANE_QTY", txtLaneQty.Value);

                //        SumDefectQtyCoating();
                //        dgProductResult_C.Refresh(false);
                //    }
                //}
                //else
                //{
                //    txtVersion.Text = popup._ReturnRecipeNo;
                //}
            }
        }

        #endregion

        #endregion

        #endregion

    }
}
