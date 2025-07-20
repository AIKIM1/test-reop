/*************************************************************************************
 Created Date : 2017.06.20
      Creator : 신광희C
   Decription : 원각 초소형 공정진척 투입영역 UserControl(참조 UC_IN_OUTPUT.xaml)
--------------------------------------------------------------------------------------
 [Change History]
  
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using System.Globalization;
using System.Reflection;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcAssyInput.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcAssyInput
    {
        #region Declaration & Constructor

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public C1DataGrid DgDefectDetail { get; set; }

        public C1DataGrid DgDefect { get; set; }

        public Button ButtonSaveWipHistory { get; set; }

        public RichTextBox TextRemark { get; set; }

        public TextBox TextDifferenceQty { get; set; }

        public UserControl UcParentControl;

        public C1TabControl TabControl { get; set; }

        public string EquipmentSegment { get; set; }

        public string EquipmentId { get; set; }

        public string ProcessCode { get; set; }

        public string ProdLotId { get; set; }

        public string ProdWorkInProcessSequence { get; set; }

        public string ProdWorkOrderId { get; set; }

        public string ProdWorkOrderDetailId { get; set; }

        public string ProdLotState { get; set; }
        //추가
        public TextBox TxtProdVerCode { get; set; }
        //추가
        public Button ButtonVersion { get; set; }

        public bool IsSmallType;

        public bool IsReWork;

        private string _maxPeviewProcessEndDay = string.Empty;
        private DateTime _dtMinValid;
        private DateTime _dtCaldate;

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();



        private struct PRV_VALUES
        {
            public string sPrvOutTray;
            public string sPrvCurrIn;
            public string sPrvOutBox;

            public PRV_VALUES(string sTray, string sIn, string sBox)
            {
                this.sPrvOutTray = sTray;
                this.sPrvCurrIn = sIn;
                this.sPrvOutBox = sBox;
            }
        }

        private PRV_VALUES _PRV_VLAUES = new PRV_VALUES("", "", "");

        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>


        public UcAssyInput()
        {
            InitializeComponent();
            SetControl();
        }

        private void SetControl()
        {
            DgDefect = dgDefect;
            DgDefectDetail = dgDefectDetail;
            ButtonSaveWipHistory = btnSaveWipHistory;
            TextRemark = txtRemark;
            TextDifferenceQty = txtDifferenceQty;
            TabControl = AssyInputTab;
            TxtProdVerCode = txtProdVerCode;
            ButtonVersion = btnVersion;
            dgDefect.Columns["CLSS_NAME1"].Visibility = Visibility.Collapsed;
        }

        public void InitializeControls()
        {
            try
            {
                CommonCombo combo = new CommonCombo();

                // 자재 투입위치 코드
                String[] sFilter1 = { EquipmentId, "PROD" };
                String[] sFilter2 = { EquipmentId, null }; // 자재,제품 전체
                String[] sFilter3 = { EquipmentId, "MTRL" }; // 자재,제품 전체
                //MTRL

                combo.SetCombo(cboPancakeMountPstnID, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                combo.SetCombo(cboHistMountPstsID, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                combo.SetCombo(cboWaitHalfProduct, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                combo.SetCombo(cboInHalfMountPstnID, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                combo.SetCombo(cboInputMaterialMountPstsID, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetControlVisibility()
        {
            try
            {
                ApplyPermissions();

                tabDefectDetail.Visibility = Visibility.Collapsed;
                tabDefect.Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["DEFECTQTY"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["ALPHAQTY_P"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["ALPHAQTY_M"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["EQPTQTY"].Visibility = Visibility.Collapsed;

                if (ProcessCode.Equals(Process.WINDING))
                {
                    tbWaitHalfProduct.Visibility = Visibility.Collapsed;
                    tbInputMaterial.Visibility = Visibility.Collapsed;
                    tbInHalfProduct.Visibility = Visibility.Collapsed;
                    grdMagTypeCntInfo.Visibility = Visibility.Collapsed;
                }
                else if (ProcessCode.Equals(Process.ASSEMBLY))
                {
                    tbPancake.Visibility = Visibility.Collapsed;
                    tbHist.Visibility = Visibility.Collapsed;
                    grdMagTypeCntInfo.Visibility = Visibility.Collapsed;

                    if (IsSmallType)
                    {
                        dgWaitHalfProduct.Columns["TRAYID"].Visibility = Visibility.Visible;
                        dgWaitHalfProduct.Columns["LOTID"].Visibility = Visibility.Collapsed;
                        tbWaitLotId.Text = ObjectDic.Instance.GetObjectName("TRAYID");
                    }

                    else
                    {
                        dgWaitHalfProduct.Columns["TRAYID"].Visibility = Visibility.Collapsed;
                        dgWaitHalfProduct.Columns["LOTID"].Visibility = Visibility.Visible;
                        tbWaitLotId.Text = ObjectDic.Instance.GetObjectName("LOT ID");
                    }
                }
                else if (ProcessCode.Equals(Process.WASHING))
                {
                    tbCurrIn.Header = ObjectDic.Instance.GetObjectName("투입처리");
                    tbPancake.Visibility = Visibility.Collapsed;
                    tbHist.Visibility = Visibility.Collapsed;
                    grdMagTypeCntInfo.Visibility = Visibility.Collapsed;
                    dgDefectDetail.Columns["GOODQTY"].IsReadOnly = true;
                    if (IsReWork)
                    {
                        InputTitle.Text = ObjectDic.Instance.GetObjectName("생산");
                        tabDefectDetail.Visibility = Visibility.Visible;
                        tabDefect.Visibility = Visibility.Visible;
                        dgWaitHalfProduct.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;
                        dgWaitHalfProduct.Columns["TRAYID"].Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }   
        }

        #endregion

        #region Event



        private void C1TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tabItem = (((C1.WPF.C1TabItem)((ItemsControl)sender).Items.CurrentItem)).Name.GetString();
            if (tabItem == "tbInHalfProduct")
            {
                GetHalfProductList();
            }

            //    if (tabItem == "TabWashingResult")
            //    {
            //        if (!string.IsNullOrEmpty(ProdLotId))
            //            GetWashingResult();
            //    }
            //    else if (tabItem == "TabOut")
            //    {
            //        grdLegend.Visibility = Visibility.Visible;
            //    }

        }

        private void TbDefectDetail_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CalculateDefectQty();
        }

        private void dgDefectDetail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Index == dataGrid.Columns["ALPHAQTY_P"].Index)
                    {
                        var convertFromString = ColorConverter.ConvertFromString("#F8DAC0");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else if (e.Cell.Column.Index == dataGrid.Columns["ALPHAQTY_M"].Index)
                    {
                        var convertFromString = ColorConverter.ConvertFromString("#F8DAC0");
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

        private void dgDefectDetail_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgDefectDetail_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e?.Cell?.Column != null)
            {
                if (string.Equals(ProcessCode, Process.WINDING))
                {
                    if (e.Cell.Column.Name.Equals("GOODQTY"))
                    {
                        double goodqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "GOODQTY").GetDouble();
                        double defectqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_DEFECT").GetDouble();
                        double lossqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_LOSS").GetDouble();
                        double chargeprdqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_CHARGEPRD").GetDouble();
                        double eqptqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTQTY").GetDouble();
                        double outputqty = goodqty + defectqty + lossqty + chargeprdqty;

                        DataTableConverter.SetValue(e.Cell.Row.DataItem, "OUTPUTQTY", outputqty);


                    }
                }
                else
                {
                    double goodqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "GOODQTY").GetDouble();
                    double defectqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_DEFECT").GetDouble();
                    double lossqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_LOSS").GetDouble();
                    double chargeprdqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_CHARGEPRD").GetDouble();
                    double outputqty = goodqty + defectqty + lossqty + chargeprdqty;

                    DataTableConverter.SetValue(e.Cell.Row.DataItem, "OUTPUTQTY", outputqty);
                }
            }
        }

        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveDefect()) return;
            SaveDefect();
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("불량정보를 저장 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            //Util.MessageConfirm("SFU1587", result =>
            //{
            //    if (result == MessageBoxResult.OK)
            //    {
            //        SaveDefect();
            //    }
            //});
        }

        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            //try
            //{
            //    IsChangeDefect = true;
            //    //CalculateDefectQty();
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private void dgDefect_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Column != null)
                {
                    if (e.Column.Name == "RESNQTY")
                    {
                        DataRowView drv = e.Row.DataItem as DataRowView;
                        if (drv != null)
                        {
                            e.Cancel = drv["DFCT_QTY_CHG_BLOCK_FLAG"].GetString() == "Y";
                        }
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
            C1DataGrid dg = sender as C1DataGrid;
            if (dg == null) return;

            try
            {
                if (e.Cell?.Presenter == null) return;
                if (e.Cell.Row.Type == DataGridRowType.Bottom)
                {
                    StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                    if (panel != null)
                    {
                        ContentPresenter presenter = panel.Children[0] as ContentPresenter;
                        if (e.Cell.Column.Index == dg.Columns["RESNQTY"].Index)
                        {
                            if (e.Cell.Row.Index == dg.Rows.Count - 1)
                            {
                                if (presenter != null)
                                {
                                    presenter.Content = GetSumDefectQty().GetInt();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
        }

        private void btnCurrInCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCurrInCancel())
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입취소 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1988", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable ininDataTable = _bizDataSet.GetUC_BR_PRD_REG_CURR_INPUT();

                        for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
                        {
                            if (!_util.GetDataGridCheckValue(dgCurrIn, "CHK", i)) continue;

                            if (!Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID")).Equals(""))
                            {
                                DataRow newRow = ininDataTable.NewRow();
                                newRow["WIPNOTE"] = "";
                                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID"));
                                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                                newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_QTY")));

                                ininDataTable.Rows.Add(newRow);
                            }
                        }

                        InputCancel(ininDataTable);
                        //OnCurrInCancel(ininDataTable);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCurrInReplace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCurrInReplace())
                    return;


                if (string.Equals(ProcessCode, Process.WINDING))
                {
                    PopupReplaceWinding();
                }
                else if (string.Equals(ProcessCode, Process.ASSEMBLY))
                {
                    if (IsSmallType)
                    {
                        PopupReplaceAssemblySmallType();
                    }
                    else
                    {
                        PopupReplaceAssembly();
                    }
                }
                else
                {
                    PopupReplaceWashingRework();
                    //if (IsReWork)
                    //{
                    //    PopupReplaceWashingRework();
                    //}
                    //else
                    //{
                    //    PopuoReplaceWashing();
                    //}
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void popPanReplace_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_PAN_REPLACE pop = sender as CMM_ASSY_PAN_REPLACE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    grid.Children.Remove(pop);
                    break;
                }
            }
        }

        private void popWindingReplace_Closed(object sender, EventArgs e)
        {
            CMM_WINDING_PAN_REPLACE pop = sender as CMM_WINDING_PAN_REPLACE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                //GetProductLot();
                GetCurrInList();
            }
            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    grid.Children.Remove(pop);
                    break;
                }
                    
            }
        }

        private void popAssemblyReplace_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_WG_PAN_REPLACE pop = sender as CMM_ASSY_WG_PAN_REPLACE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                //GetProductLot();
                GetCurrInList();
            }
            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    grid.Children.Remove(pop);
                    break;
                }
                    
            }
        }

        private void popAssemblySmallTypeReplace_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CSH_PAN_REPLACE pop = sender as CMM_ASSY_CSH_PAN_REPLACE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                //GetProductLot();
                GetCurrInList();
            }
            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    grid.Children.Remove(pop);
                    break;
                }
                    
            }
        }

        private void btnCurrInComplete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCurrInComplete())
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입완료 처리 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1972", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (string.Equals(ProcessCode, Process.WINDING))
                        {
                            DataTable inDataTable = _bizDataSet.GetUC_BR_PRD_REG_CURR_INPUT();

                            for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
                            {
                                if (!_util.GetDataGridCheckValue(dgCurrIn, "CHK", i)) continue;

                                if (!Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID")).Equals(""))
                                {
                                    DataRow newRow = inDataTable.NewRow();
                                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID"));
                                    newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                                    newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                                    newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "MTRLID"));
                                    newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_QTY")));

                                    inDataTable.Rows.Add(newRow);
                                }
                            }

                            InputComplete(inDataTable);
                        }
                        else if (string.Equals(ProcessCode, Process.ASSEMBLY))
                        {
                            DataTable inDataTable = _bizDataSet.GetBR_PRD_REG_EQPT_END_INPUT_LOT_AS();
                            for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
                            {
                                if (!_util.GetDataGridCheckValue(dgCurrIn, "CHK", i)) continue;

                                if (!string.IsNullOrEmpty(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID").GetString()))
                                {
                                    DataRow dr = inDataTable.NewRow();
                                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                    dr["IFMODE"] = IFMODE.IFMODE_OFF;
                                    dr["EQPTID"] = EquipmentId;
                                    dr["USERID"] = LoginInfo.USERID;
                                    dr["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                                    dr["EQPT_MOUNT_PSTN_STATE"] = "A";
                                    dr["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID"));
                                    dr["PROD_LOTID"] = ProdLotId;
                                    dr["EQPT_LOTID"] = string.Empty;
                                    inDataTable.Rows.Add(dr);
                                }
                            }
                            InputCompleteAssembly(inDataTable);
                        }
                        else if (string.Equals(ProcessCode, Process.WASHING))
                        {
                            DataTable inDataTable = _bizDataSet.GetBR_PRD_REG_END_INPUT_LOT_WS();
                            for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
                            {
                                if (!_util.GetDataGridCheckValue(dgCurrIn, "CHK", i)) continue;

                                if (!string.IsNullOrEmpty(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID").GetString()))
                                {
                                    DataRow dr = inDataTable.NewRow();
                                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                    dr["IFMODE"] = IFMODE.IFMODE_OFF;
                                    dr["EQPTID"] = EquipmentId;
                                    dr["USERID"] = LoginInfo.USERID;
                                    dr["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                                    dr["EQPT_MOUNT_PSTN_STATE"] = "A";
                                    dr["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID"));
                                    dr["PROD_LOTID"] = ProdLotId;
                                    dr["EQPT_LOTID"] = string.Empty;
                                    inDataTable.Rows.Add(dr);
                                }
                            }
                            InputCompleteWashing(inDataTable);
                        }

                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCurrInLotID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    // 권한 없으면 Skip.
                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                        return;

                    if (!CanCurrAutoInputLot())
                        return;

                    string positionId = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "EQPT_MOUNT_PSTN_ID"));
                    string positionName = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "EQPT_MOUNT_PSTN_NAME"));
                    string outLotId = txtCurrInLotID.Text.Trim();

                    object[] parameters = new object[2];
                    parameters[0] = positionName;
                    parameters[1] = txtCurrInLotID.Text.Trim();

                    Util.MessageConfirm("SFU1291", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            if (string.Equals(ProcessCode, Process.WINDING))
                            {
                                InputAutoLot(txtCurrInLotID.Text.Trim(), positionId, string.Empty, string.Empty);
                            }
                            else if(string.Equals(ProcessCode, Process.ASSEMBLY))
                            {
                                if (IsSmallType)
                                {
                                    InputAutoLotSmallAssembly(txtCurrInLotID.Text.Trim(), positionId);
                                }
                                else
                                {
                                    InputAutoLotAssembly(txtCurrInLotID.Text.Trim(), positionId);
                                }
                            }
                            else if (string.Equals(ProcessCode, Process.WASHING))
                            {
                                InputAutoLotWashing(txtCurrInLotID.Text.Trim(), positionId);
                            }

                            txtCurrInLotID.Text = string.Empty;
                        }
                    }, parameters);

                    //if (ProcessCode.Equals(Process.PACKAGING) && Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD"))
                    //{
                    //    InputAutoLot(txtCurrInLotID.Text.Trim(), sInPos, string.Empty, string.Empty);
                    //    txtCurrInLotID.Text = string.Empty;
                    //}
                    //else
                    //{
                    //    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("{0} 위치에 {1} 을 투입 하시겠습니까?", sInPosName, txtCurrInLotID.Text.Trim()), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>

                    //}
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCurrIn_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    C1DataGrid dg = sender as C1DataGrid;
                    if (e.Cell != null &&
                        e.Cell.Presenter != null &&
                        e.Cell.Presenter.Content != null)
                    {
                        CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                        if (chk != null)
                        {
                            switch (Convert.ToString(e.Cell.Column.Name))
                            {
                                case "CHK":
                                    if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                       dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                       (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                       (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                       !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                        chk.IsChecked = true;

                                        _PRV_VLAUES.sPrvCurrIn = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_MOUNT_PSTN_ID"));

                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (e.Cell.Row.Index != idx)
                                            {
                                                if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                    dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                                {
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                                }
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                            }
                                        }
                                    }
                                    else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                             dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                             (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                             (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                             (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                        chk.IsChecked = false;

                                        _PRV_VLAUES.sPrvCurrIn = "";
                                    }
                                    break;
                            }

                            if (dg.CurrentCell != null)
                                dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                            else if (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null)
                                dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtWaitPancakeLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetWaitPancake();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnWaitPancakeSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetWaitPancake();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnWaitPancakeInPut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanWaitPanCakeInput())
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1248", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        PancakeInput();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWaitPancake_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                // 대기 1개만 선택.
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    C1DataGrid dg = sender as C1DataGrid;
                    if (e.Cell != null &&
                        e.Cell.Presenter != null &&
                        e.Cell.Presenter.Content != null)
                    {
                        CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                        if (chk != null)
                        {
                            switch (Convert.ToString(e.Cell.Column.Name))
                            {
                                case "CHK":
                                    if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                       dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                       (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                       (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                       !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                        chk.IsChecked = true;

                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (e.Cell.Row.Index != idx)
                                            {
                                                if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                    dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                                {
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                                }
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                            }
                                        }
                                    }
                                    else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                             dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                             (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                             (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                             (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                        chk.IsChecked = false;
                                    }
                                    break;
                            }

                            if (dg.CurrentCell != null)
                                dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                            else if (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null)
                                dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboPancakeMountPstnID_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetWaitPancake();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnWaitHalfProductSearch_Click(object sender, RoutedEventArgs e)
        {
            GetWaitHalfProductList();
        }

        private void txtTrayId_KeyDown(object sender, KeyEventArgs e)
        {
            GetWaitHalfProductList();
        }

        private void btnInputWaitHalfProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationInputWaitHalfProduct()) return;

                Util.MessageConfirm("SFU1248", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        InputWaitHalfProduct();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void cboWaitHalfProduct_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            GetWaitHalfProductList();
        }

        private void btnHistSearch_Click(object sender, RoutedEventArgs e)
        {
            GetInputHistory();
        }

        private void txtHistLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetInputHistory();
            }
        }

        private void cboHistMountPstsID_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetInputHistory();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnInBoxInputCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanInBoxInputCancel(dgInputHist))
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입취소 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1988", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        BoxInputCancel();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnInputMaterialtSearch_Click(object sender, RoutedEventArgs e)
        {
            GetMaterialList();
        }

        private void btnInputMaterialCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInputMaterialCancel()) return;

            Util.MessageConfirm("SFU1988", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    InputMaterialCancel();
                }
            });

        }

        private void cboInputMaterialMountPstsID_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            GetMaterialList();
        }

        private void txtInputMaterialLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetMaterialList();
            }
        }

        private void dgWaitPancake_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(_maxPeviewProcessEndDay).Equals(""))
                    {
                        e.Cell.Presenter.Background = null;
                    }
                    else
                    {
                        int iDay = 0;
                        int.TryParse(_maxPeviewProcessEndDay, out iDay);

                        if (iDay > 0)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")).Equals("") || txtWaitPancakeLot.Text.Trim().Length > 0)
                            {
                                e.Cell.Presenter.Background = null;
                            }
                            else
                            {
                                DateTime dtValid;
                                DateTime.TryParse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")), out dtValid);

                                if (_dtMinValid.AddDays(iDay) >= dtValid)
                                {
                                    var convertFromString = ColorConverter.ConvertFromString("#E8F7C8");
                                    if (convertFromString != null)
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = null;
                                }
                            }
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                        }
                    }
                }
            }));
        }

        private void dgWaitPancake_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
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

        private void btnInHalfProductSearch_Click(object sender, RoutedEventArgs e)
        {
            GetHalfProductList();
        }

        private void btnInHalfProductInPutCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInHalfProductInPutCancel()) return;

            Util.MessageConfirm("SFU1988", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    InputHalfProductCancel();
                }
            });

        }

        private void cboInHalfMountPstnID_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            GetHalfProductList();
        }

        private void DgWaitHalfProduct_OnCurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            DataGridCurrentCellChanged(sender,e);
        }

        private void dgInputMaterial_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            CheckInDataGridCheckBox(dg, e);
        }

        private void dgInHalfProduct_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            CheckInDataGridCheckBox(dg, e);
        }

        private void DgInHalfProduct_OnCurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            DataGridCurrentCellChanged(sender,e);
        }

        #endregion

        #region Mehod

        public void ChangeEquipment(string equipmentCode, string equipmentSegmentCode)
        {
            try
            {
                EquipmentSegment = equipmentSegmentCode;
                EquipmentId = equipmentCode;

                ProdLotId = string.Empty;
                ProdWorkInProcessSequence = string.Empty;
                ProdWorkOrderId = string.Empty;
                ProdWorkOrderDetailId = string.Empty;
                ProdLotState = string.Empty;

                InitializeControls();

                ClearAll();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SearchAll()
        {
            try
            {
                if (tbCurrIn.Visibility == Visibility.Visible)
                {
                    GetCurrInList();
                }
                if (tbPancake.Visibility == Visibility.Visible)
                {
                    GetWaitPancake();
                }
                if (tbWaitHalfProduct.Visibility == Visibility.Visible)
                {
                    GetWaitHalfProductList();
                }
                if (tbInputMaterial.Visibility == Visibility.Visible)
                {
                    GetMaterialList();
                }
                if (tbHist.Visibility == Visibility.Visible)
                {
                    GetInputHistory();
                }
                if (tbInHalfProduct.Visibility == Visibility.Visible)
                {
                    GetHalfProductList();
                }

                AssyInputTab.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected virtual void GetProductLot()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("GetProductLot");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }

                methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected virtual void OnCurrInCancel(DataTable inMtrl)
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("OnCurrInCancel");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    if (i == 0) parameterArrys[i] = inMtrl;
                    else parameterArrys[i] = null;
                }

                methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InputCancel(DataTable dt)
        {
            try
            {
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;

                string bizRuleName = string.Equals(ProcessCode, Process.WINDING) ? "BR_PRD_REG_CANCEL_INPUT_IN_LOT_WN" : "BR_PRD_REG_CANCEL_INPUT_IN_LOT";

                ShowParentLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_CANCEL_LM();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentId;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();

                inTable.Rows.Add(newRow);

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                foreach (DataRow sourcerow in dt.Rows)
                {
                    DataRow destRow = inInputTable.NewRow();
                    foreach (DataColumn colname in inInputTable.Columns)
                    {
                        if (sourcerow[colname.ColumnName] != null)
                            destRow[colname.ColumnName] = sourcerow[colname.ColumnName];
                    }
                    inInputTable.Rows.Add(destRow);
                }

                string xmlText = indataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    HiddenParentLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    //GetProductLot();
                    GetCurrInList();
                    Util.MessageInfo("SFU1275");

                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InputComplete(DataTable dt)
        {
            try
            {
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;

                const string bizRuleName = "BR_PRD_REG_END_INPUT_IN_LOT_WN";

                ShowParentLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_COMPLETE_LM();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentId;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();

                inTable.Rows.Add(newRow);

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                foreach (DataRow sourcerow in dt.Rows)
                {
                    DataRow destRow = inInputTable.NewRow();
                    foreach (DataColumn colname in inInputTable.Columns)
                    {
                        if (sourcerow[colname.ColumnName] != null)
                            destRow[colname.ColumnName] = sourcerow[colname.ColumnName];
                    }
                    inInputTable.Rows.Add(destRow);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    HiddenParentLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    //GetProductLot();
                    GetCurrInList();
                    Util.MessageInfo("SFU1275");
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InputCompleteAssembly(DataTable dt)
        {
            const string bizRuleName = "BR_PRD_REG_EQPT_END_INPUT_LOT_AS";
            ShowParentLoadingIndicator();

            DataSet ds = new DataSet();
            ds.Tables.Add(dt);

            try
            {
                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP", null, (bizResult, bizException) =>
                {
                    HiddenParentLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    //GetProductLot();
                    GetCurrInList();
                    Util.MessageInfo("SFU1275");
                }, ds, FrameOperation.MENUID);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void InputCompleteWashing(DataTable dt)
        {
            const string bizRuleName = "BR_PRD_REG_END_INPUT_LOT_WS";
            ShowParentLoadingIndicator();

            DataSet ds = new DataSet();
            ds.Tables.Add(dt);

            try
            {
                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP", null, (bizResult, bizException) =>
                {
                    HiddenParentLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    //GetProductLot();
                    GetCurrInList();
                    Util.MessageInfo("SFU1275");
                }, ds, FrameOperation.MENUID);
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void InputAutoLot(string inputLot, string positionId, string inMaterialId, string inQty)
        {
            try
            {
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;

                const string bizRuleName = "BR_PRD_REG_START_INPUT_IN_LOT_WN";

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_MTRL_INPUT_WN();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentId;
                newRow["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                newRow = inInputTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = positionId;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = inputLot;

                inInputTable.Rows.Add(newRow);

                string xmlText = indataSet.GetXml();

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,IN_INPUT", null, indataSet);
                //GetProductLot();
                GetCurrInList();

                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InputAutoLotWinding(string inputLot, string positionId)
        {
            try
            {
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;

                const string bizRuleName = "BR_PRD_REG_START_INPUT_IN_LOT_WN";

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_MTRL_INPUT_WN();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentId;
                newRow["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                newRow = inInputTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = positionId;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = inputLot;

                inInputTable.Rows.Add(newRow);

                string xmlText = indataSet.GetXml();

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,IN_INPUT", null, indataSet);

                GetProductLot();

                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InputAutoLotSmallAssembly(string inputLot, string positionId)
        {
            try
            {
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;

                const string bizRuleName = "BR_PRD_REG_START_INPUT_LOT_ASS";

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_MTRL_INPUT_AS();

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentId;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();
                newRow["EQPT_LOTID"] = null;
                newRow["EQPT_MOUNT_PSTN_ID"] = positionId;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["CSTID"] = inputLot;
                newRow["OUT_LOTID"] = string.Empty;
                inTable.Rows.Add(newRow);


                string xmlText = indataSet.GetXml();

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,IN_INPUT", null, indataSet);
                //GetProductLot();
                GetCurrInList();

                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }            
        }

        private void InputAutoLotAssembly(string inputLot, string positionId)
        {
            try
            {
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;

                const string bizRuleName = "BR_PRD_REG_INPUT_LOT_AS";

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_LOT_AS();

                DataTable inDataTable = indataSet.Tables["IN_EQP"];
                DataRow row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = EquipmentId;
                row["USERID"] = LoginInfo.USERID;
                row["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();
                row["EQPT_LOTID"] = string.Empty;
                inDataTable.Rows.Add(row);

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                row = inInputTable.NewRow();
                row["EQPT_MOUNT_PSTN_ID"] = positionId;
                row["EQPT_MOUNT_PSTN_STATE"] = "A";
                row["PRODID"] = string.Empty;
                row["WINDING_RUNCARD_ID"] = inputLot;
                inInputTable.Rows.Add(row);

                string xmlText = indataSet.GetXml();
                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_EQP,IN_INPUT", "OUT_EQP", indataSet);
                //GetProductLot();
                GetCurrInList();

                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InputAutoLotWashing(string inputLot, string positionId)
        {
            try
            {
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;

                const string bizRuleName = "BR_PRD_REG_INPUT_LOT_WS";

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_LOT_WS();

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentId;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();
                newRow["EQPT_LOTID"] = string.Empty;
                inTable.Rows.Add(newRow);

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                newRow = inInputTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = positionId;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = inputLot;

                inInputTable.Rows.Add(newRow);
                string xmlText = indataSet.GetXml();

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,IN_INPUT", null, indataSet);
                //GetProductLot();
                GetCurrInList();

                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InputHalfProductCancel()
        {
            try
            {
                ShowParentLoadingIndicator();
                //const string bizRuleName = "BR_PRD_REG_CANCEL_TERMINATE_LOT";
                string bizRuleName = string.Equals(ProcessCode, Process.WASHING) ? "BR_PRD_REG_CANCEL_TERMINATE_LOT_WS" : "BR_PRD_REG_CANCEL_TERMINATE_LOT";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_CANCEL_BASKET_CL();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inMtrlTable = indataSet.Tables["INLOT"];

                for (int i = 0; i < dgInHalfProduct.Rows.Count - dgInHalfProduct.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgInHalfProduct, "CHK", i)) continue;

                    newRow = inMtrlTable.NewRow();
                    newRow["INPUT_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_SEQNO")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_SEQNO")));
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_LOTID"));
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_QTY")));

                    inMtrlTable.Rows.Add(newRow);
                }
                string xml = indataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetHalfProductList();
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                }, indataSet);

                /*
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;

                ShowParentLoadingIndicator();
                const string bizRuleName = "BR_PRD_DEL_INPUT_LOT_AS";

                DataSet inDataSet = _bizDataSet.GetBR_PRD_DEL_INPUT_LOT_AS();

                DataTable inDataTable = inDataSet.Tables["IN_EQP"];
                DataRow row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = EquipmentId;
                row["USERID"] = LoginInfo.USERID;
                row["PROD_LOTID"] = ProdLotId;
                inDataTable.Rows.Add(row);

                DataTable inInputTable = inDataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgInHalfProduct.GetRowCount(); i++)
                {
                    // 삭제 대상은 체크 되어 있고, INPUTSEQ_NO가 0이 아닌것만 삭제
                    if (_util.GetDataGridCheckValue(dgInHalfProduct, "CHK", i) == true && !string.Equals(Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_SEQNO")), "0"))
                    {
                        row = inInputTable.NewRow();
                        row["INPUT_SEQNO"] = Util.NVC_Int(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_SEQNO"));
                        row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_LOTID"));
                        row["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));

                        inInputTable.Rows.Add(row);
                    }
                }

                string xmlText = inDataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_INPUT", null, (bizResult, bizException) =>
                {
                    HiddenParentLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    //GetProductLot();
                    GetHalfProductList();
                    Util.MessageInfo("SFU1275");

                }, inDataSet);
                */

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
        }

        private void InputMaterialCancel()
        {
            try
            {
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;
                ShowParentLoadingIndicator();

                string bizRuleName;

                if (string.Equals(ProcessCode, Process.WASHING))
                {
                    bizRuleName = "BR_PRD_REG_CANCEL_INPUT_LOT_WS";
                }
                else
                {
                    bizRuleName = IsSmallType ? "BR_PRD_REG_CANCEL_INPUT_LOT_ASS" : "BR_PRD_REG_CANCEL_INPUT_LOT_AS";
                }

                DataSet inDataSet = _bizDataSet.GetBR_PRD_DEL_INPUT_LOT_AS();

                DataTable inDataTable = inDataSet.Tables["INDATA"];
                DataRow row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = EquipmentId;
                row["USERID"] = LoginInfo.USERID;
                row["PROD_LOTID"] = ProdLotId;
                inDataTable.Rows.Add(row);

                DataTable inInputTable = inDataSet.Tables["INLOT"];

                for (int i = 0; i < dgInputMaterial.GetRowCount(); i++)
                {
                    // 삭제 대상은 체크 되어 있고, INPUTSEQ_NO가 0이 아닌것만 삭제
                    if (_util.GetDataGridCheckValue(dgInputMaterial, "CHK", i) == true && !string.Equals(Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "INPUT_SEQNO")), "0"))
                    {
                        row = inInputTable.NewRow();
                        row["INPUT_SEQNO"] = Util.NVC_Int(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "INPUT_SEQNO"));
                        row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "INPUT_LOTID"));
                        row["WIPQTY"] = 0;
                        row["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));

                        inInputTable.Rows.Add(row);
                    }
                }

                string xmlText = inDataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    HiddenParentLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    GetMaterialList();
                    Util.MessageInfo("SFU1275");

                }, inDataSet);


                /*
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;
                ShowParentLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_CANCEL_INPUT_IN_LOT_AS";

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_CANCEL_LM();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentId;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();
                inTable.Rows.Add(newRow);

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                for (int i = 0; i < dgInputMaterial.GetRowCount(); i++)
                {
                    if (!_util.GetDataGridCheckValue(dgInputMaterial, "CHK", i)) continue;
                    DataRow dr = inInputTable.NewRow();

                    dr["EQPT_MOUNT_PSTN_ID"] = DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID").GetString();
                    dr["INPUT_LOTID"] = DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "INPUT_LOTID").GetString();
                    dr["WIPNOTE"] = null;
                    dr["INPUT_SEQNO"] = DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "INPUT_SEQNO");
                    inInputTable.Rows.Add(dr);
                }

                string xmlText = indataSet.GetXml();
                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    HiddenParentLoadingIndicator();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }
                    //GetProductLot();
                    GetHalfProductList();
                    Util.MessageInfo("SFU1275");

                }, indataSet);
                */
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
        }

        private void InputWaitHalfProduct()
        {
            try
            {
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;

                ShowParentLoadingIndicator();
                // 추후 분리작업이 필요할 수 있음

                int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgWaitHalfProduct, "CHK");

                DataSet inDataSet = new DataSet();
                string bizRuleName = string.Empty;
                string outData = null;

                if (string.Equals(ProcessCode, Process.ASSEMBLY))
                {
                    if (IsSmallType)
                    {
                        bizRuleName = "BR_PRD_REG_START_INPUT_LOT_ASS";

                        DataTable inDataTable = inDataSet.Tables.Add("IN_EQP");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                        inDataTable.Columns.Add("EQPT_LOTID", typeof(string));
                        inDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        inDataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        inDataTable.Columns.Add("CSTID", typeof(string));
                        inDataTable.Columns.Add("OUT_LOTID", typeof(string));

                        DataRow dr = inDataTable.NewRow();
                        dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        dr["IFMODE"] = IFMODE.IFMODE_OFF;
                        dr["EQPTID"] = EquipmentId;
                        dr["USERID"] = LoginInfo.USERID;
                        dr["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();
                        dr["EQPT_LOTID"] = null;
                        dr["EQPT_MOUNT_PSTN_ID"] = cboWaitHalfProduct.SelectedValue;
                        dr["EQPT_MOUNT_PSTN_STATE"] = "A";
                        dr["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitHalfProduct.Rows[rowIndex].DataItem, "TRAYID"));
                        dr["OUT_LOTID"] = null;
                        inDataTable.Rows.Add(dr);

                        outData = null;
                    }
                    else
                    {
                        bizRuleName = "BR_PRD_REG_INPUT_LOT_AS";

                        DataTable inDataTable = inDataSet.Tables.Add("IN_EQP");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                        inDataTable.Columns.Add("EQPT_LOTID", typeof(string));

                        DataRow dr = inDataTable.NewRow();
                        dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        dr["IFMODE"] = IFMODE.IFMODE_OFF;
                        dr["EQPTID"] = EquipmentId;
                        dr["USERID"] = LoginInfo.USERID;
                        dr["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();
                        dr["EQPT_LOTID"] = null;
                        inDataTable.Rows.Add(dr);

                        DataTable ininput = inDataSet.Tables.Add("IN_INPUT");
                        ininput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        ininput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        ininput.Columns.Add("PRODID", typeof(string));
                        ininput.Columns.Add("WINDING_RUNCARD_ID", typeof(string));
                        ininput.Columns.Add("INPUT_QTY", typeof(decimal));

                        DataRow newRow = ininput.NewRow();
                        newRow["EQPT_MOUNT_PSTN_ID"] = cboWaitHalfProduct.SelectedValue;
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgWaitHalfProduct.Rows[rowIndex].DataItem, "PRODID"));
                        newRow["WINDING_RUNCARD_ID"] = Util.NVC(DataTableConverter.GetValue(dgWaitHalfProduct.Rows[rowIndex].DataItem, "LOTID"));
                        newRow["INPUT_QTY"] = Convert.ToDecimal(DataTableConverter.GetValue(dgWaitHalfProduct.Rows[rowIndex].DataItem, "WIPQTY2"));

                        ininput.Rows.Add(newRow);
                        outData = "OUT_EQP";
                    }
                }
                else if (string.Equals(ProcessCode, Process.WASHING))
                {
                    bizRuleName = "BR_PRD_REG_INPUT_LOT_WS";

                    DataTable inDataTable = inDataSet.Tables.Add("IN_EQP");
                    inDataTable.Columns.Add("SRCTYPE", typeof(string));
                    inDataTable.Columns.Add("IFMODE", typeof(string));
                    inDataTable.Columns.Add("EQPTID", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));
                    inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                    inDataTable.Columns.Add("EQPT_LOTID", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["IFMODE"] = IFMODE.IFMODE_OFF;
                    dr["EQPTID"] = EquipmentId;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();
                    dr["EQPT_LOTID"] = null;
                    inDataTable.Rows.Add(dr);

                    DataTable ininput = inDataSet.Tables.Add("IN_INPUT");
                    ininput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                    ininput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                    //ininput.Columns.Add("PRODID", typeof(string));
                    ininput.Columns.Add("INPUT_LOTID", typeof(string));
                    ininput.Columns.Add("INPUT_QTY", typeof(decimal));

                    DataRow newRow = ininput.NewRow();
                    newRow["EQPT_MOUNT_PSTN_ID"] = cboWaitHalfProduct.SelectedValue;
                    newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                    //newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgWaitHalfProduct.Rows[rowIndex].DataItem, "PRODID"));
                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitHalfProduct.Rows[rowIndex].DataItem, "LOTID"));
                    newRow["INPUT_QTY"] = Convert.ToDecimal(DataTableConverter.GetValue(dgWaitHalfProduct.Rows[rowIndex].DataItem, "WIPQTY2"));

                    ininput.Rows.Add(newRow);
                    outData = null;
                }


                string xml = inDataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", outData, (bizResult, bizException) =>
                {
                    HiddenParentLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    //GetProductLot();
                    GetWaitHalfProductList();
                    GetCurrInList();
                    Util.MessageInfo("SFU1275");

                }, inDataSet);

            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void ApplyPermissions()
        {
            try
            {
                List<Button> listAuth = new List<Button>
                {
                    btnCurrInCancel,
                    btnCurrInComplete,
                    btnWaitPancakeInPut,
                    btnInputMaterialCancel,
                    btnInBoxInputCancel
                };


                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ClearAll()
        {
            try
            {
                //투입현황
                if (dgCurrIn != null) Util.gridClear(dgCurrIn);
                if (tbATypeCnt != null) tbATypeCnt.Text = "0";
                if (tbCTypeCnt != null) tbCTypeCnt.Text = "0";
                if (txtCurrInLotID != null) txtCurrInLotID.Text = string.Empty;

                //대기Pancake
                if (dgWaitPancake != null) Util.gridClear(dgWaitPancake);
                if (txtWaitPancakeLot != null) txtWaitPancakeLot.Text = string.Empty;

                //대기 반제품
                if (dgWaitHalfProduct != null) Util.gridClear(dgWaitHalfProduct);
                if (txtTrayId != null) txtTrayId.Text = string.Empty;

                // 투입자재
                if (dgInputMaterial != null) Util.gridClear(dgInputMaterial);

                //투입 반제품
                if (dgInHalfProduct != null) Util.gridClear(dgInHalfProduct);

                //투입이력
                if (dgInputHist != null) Util.gridClear(dgInputHist);

                if(dgDefect != null) Util.gridClear(dgDefect);

                if (dgDefectDetail != null) Util.gridClear(dgDefectDetail);

                txtWorkOrder.Text = string.Empty;
                txtLotId.Text = string.Empty;
                txtStartTime.Text = string.Empty;
                txtEndTime.Text = string.Empty;
                txtWorkMinute.Text = string.Empty;
                txtRemark.Document.Blocks.Clear();
                txtProdId.Text = string.Empty;
                txtProdVerCode.Text = string.Empty;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void ClearDataGrid()
        {
            try
            {
                //투입현황
                if (dgCurrIn != null) Util.gridClear(dgCurrIn);
                if (txtCurrInLotID != null) txtCurrInLotID.Text = string.Empty;

                //Pancake투입
                if (dgWaitPancake != null) Util.gridClear(dgWaitPancake);
                if (txtWaitPancakeLot != null) txtWaitPancakeLot.Text = string.Empty;

                //대기매거진
                if (dgWaitHalfProduct != null) Util.gridClear(dgWaitHalfProduct);
                if (txtTrayId != null) txtTrayId.Text = string.Empty;

                //바구니투입
                if (dgInputMaterial != null) Util.gridClear(dgInputMaterial);

                //투입 반제품
                if (dgInHalfProduct != null) Util.gridClear(dgInHalfProduct);

                //투입이력
                if (dgInputHist != null) Util.gridClear(dgInputHist);

                if (dgDefect != null) Util.gridClear(dgDefect);

                if (dgDefectDetail != null) Util.gridClear(dgDefectDetail);

                txtDifferenceQty.Text = string.Empty;
                txtWorkOrder.Text = string.Empty;
                txtLotId.Text = string.Empty;
                txtStartTime.Text = string.Empty;
                txtEndTime.Text = string.Empty;
                txtWorkMinute.Text = string.Empty;
                txtRemark.Document.Blocks.Clear();
                txtProdId.Text = string.Empty;
                txtProdVerCode.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void GetCurrInList()
        {
            try
            {
                // 메인 LOT이 없는 경우 disable 처리..
                if (string.IsNullOrEmpty(ProdLotId))
                {
                    btnCurrInCancel.IsEnabled = false;
                    btnCurrInComplete.IsEnabled = false;
                }
                else
                {
                    btnCurrInCancel.IsEnabled = true;
                    btnCurrInComplete.IsEnabled = true;
                }

                string bizRuleName;
                if (ProcessCode.Equals(Process.WINDING))
                {
                    bizRuleName = "DA_PRD_SEL_CURR_IN_LOT_LIST_WNS";
                }
                else
                {
                    bizRuleName = "DA_PRD_SEL_CURR_IN_LOT_LIST";
                }
                //const string bizRuleName = "DA_PRD_SEL_CURR_IN_LOT_LIST_WNS";

                ShowParentLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_CURR_IN_LOT_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EquipmentId;

                inTable.Rows.Add(newRow);
                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgCurrIn.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgCurrIn, searchResult, FrameOperation);

                        if (!_PRV_VLAUES.sPrvCurrIn.Equals(""))
                        {
                            int idx = _util.GetDataGridRowIndex(dgCurrIn, "EQPT_MOUNT_PSTN_ID", _PRV_VLAUES.sPrvCurrIn);

                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgCurrIn.Rows[idx].DataItem, "CHK", true);

                                //row 색 바꾸기
                                dgCurrIn.SelectedIndex = idx;

                                dgCurrIn.ScrollIntoView(idx, dgCurrIn.Columns["CHK"].Index);
                            }
                        }

                        // WINDING 의 경우 컬럼 다르게 보이도록 수정.
                        if (ProcessCode.Equals(Process.WINDING))
                        {
                            dgCurrIn.Columns["MOUNT_MTRL_TYPE_NAME"].Visibility = Visibility.Collapsed;
                            dgCurrIn.Columns["MTRLNAME"].Visibility = Visibility.Collapsed;
                            dgCurrIn.Columns["INPUT_MTRL_CLSS_NAME"].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgCurrIn.Columns["MOUNT_MTRL_TYPE_NAME"].Visibility = Visibility.Visible;
                            dgCurrIn.Columns["MTRLNAME"].Visibility = Visibility.Visible;
                            dgCurrIn.Columns["INPUT_MTRL_CLSS_NAME"].Visibility = Visibility.Collapsed;
                        }

                        if (dgCurrIn.CurrentCell != null)
                            dgCurrIn.CurrentCell = dgCurrIn.GetCell(dgCurrIn.CurrentCell.Row.Index, dgCurrIn.Columns.Count - 1);
                        else if (dgCurrIn.Rows.Count > 0 && dgCurrIn.GetCell(dgCurrIn.Rows.Count, dgCurrIn.Columns.Count - 1) != null)
                            dgCurrIn.CurrentCell = dgCurrIn.GetCell(dgCurrIn.Rows.Count, dgCurrIn.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetWaitHalfProductList()
        {
            try
            {
                //if (!string.Equals(ProcessCode, Process.ASSEMBLY)) return;
                Util.gridClear(dgInHalfProduct);

                string bizRuleName;

                if (string.Equals(ProcessCode, Process.ASSEMBLY))
                {
                    bizRuleName = IsSmallType ? "DA_PRD_SEL_WAIT_HALFPROD_ASS" : "DA_PRD_SEL_WAIT_HALFPROD_AS";
                }
                else
                {
                    bizRuleName = "DA_PRD_SEL_WAIT_HALFPROD_WS";
                }

                DataTable indataTable = new DataTable();
                indataTable.Columns.Add("LANGID", typeof(string));
                indataTable.Columns.Add("EQSGID", typeof(string));
                indataTable.Columns.Add("PROCID", typeof(string));
                indataTable.Columns.Add("INPUT_LOTID", typeof(string));

                if (!string.Equals(ProcessCode, Process.ASSEMBLY))
                {
                    indataTable.Columns.Add("WOID", typeof(string));
                    indataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                }


                DataRow dr = indataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = EquipmentSegment;
                dr["PROCID"] = ProcessCode;
                dr["INPUT_LOTID"] = string.IsNullOrEmpty(txtTrayId.Text.Trim()) ? null : txtTrayId.Text.Trim();
                if (!string.Equals(ProcessCode, Process.ASSEMBLY))
                {
                    dr["WOID"] = ProdWorkOrderId;
                    dr["EQPT_MOUNT_PSTN_ID"] = cboWaitHalfProduct.SelectedValue;
                }

                indataTable.Rows.Add(dr);

                DataSet ds = new DataSet();
                ds.Tables.Add(indataTable);
                string xml = ds.GetXml();

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", indataTable);
                dgWaitHalfProduct.ItemsSource = DataTableConverter.Convert(dt);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetHalfProductList()
        {
            try
            {
                Util.gridClear(dgInHalfProduct);

                string bizRuleName = string.Equals(ProcessCode, Process.ASSEMBLY) ? "DA_PRD_SEL_INPUT_HALFPROD_AS" : "DA_PRD_SEL_INPUT_HALFPROD_WS";
                const string materialType = "PROD";

                DataTable indataTable = new DataTable();
                indataTable.Columns.Add("LANGID", typeof(string));
                indataTable.Columns.Add("LOTID", typeof(string));
                indataTable.Columns.Add("MTRLTYPE", typeof(string));
                indataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                indataTable.Columns.Add("EQPTID", typeof(string));
                indataTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow dr = indataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtInHalfProductLot.Text.Trim();
                dr["MTRLTYPE"] = materialType;
                dr["EQPT_MOUNT_PSTN_ID"] = string.IsNullOrEmpty(cboInHalfMountPstnID.SelectedValue.GetString()) ? null : cboInHalfMountPstnID.SelectedValue.GetString();
                dr["EQPTID"] = EquipmentId;
                dr["PROD_LOTID"] = ProdLotId;

                indataTable.Rows.Add(dr);
                DataSet ds = new DataSet();
                ds.Tables.Add(indataTable);
                string xml = ds.GetXml();

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", indataTable);
                dgInHalfProduct.ItemsSource = DataTableConverter.Convert(dt);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetMaterialList()
        {
            try
            {
                Util.gridClear(dgInputMaterial);

                string bizRuleName = string.Equals(ProcessCode, Process.ASSEMBLY) ? "DA_PRD_SEL_INPUT_MATERIAL_AS" : "DA_PRD_SEL_INPUT_MATERIAL_WS";
                const string materialType = "MTRL";

                DataTable indataTable = new DataTable();
                indataTable.Columns.Add("LANGID", typeof(string));
                indataTable.Columns.Add("INPUT_LOTID", typeof(string));
                indataTable.Columns.Add("MTRLTYPE", typeof(string));
                indataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                indataTable.Columns.Add("EQPTID", typeof(string));
                indataTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow dr = indataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["INPUT_LOTID"] = txtInputMaterialLotID.Text.Trim();
                dr["MTRLTYPE"] = materialType;
                dr["EQPT_MOUNT_PSTN_ID"] = string.IsNullOrEmpty(cboInputMaterialMountPstsID.SelectedValue.GetString()) ? null : cboInputMaterialMountPstsID.SelectedValue.GetString();
                dr["EQPTID"] = EquipmentId;
                dr["PROD_LOTID"] = ProdLotId;
                

                indataTable.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", indataTable);
                dgInputMaterial.ItemsSource = DataTableConverter.Convert(dt);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetInputHistory()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_INPUT_MTRL_HIST";

                ShowParentLoadingIndicator();

                DataTable inTable = _bizDataSet.GetUC_DA_PRD_SEL_INPUT_HIST();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EquipmentId;
                newRow["PROD_LOTID"] = ProdLotId;
                newRow["PROD_WIPSEQ"] = ProdWorkInProcessSequence.Equals("") ? 1 : Convert.ToDecimal(ProdWorkInProcessSequence);
                newRow["INPUT_LOTID"] = txtHistLotID.Text.Trim().Equals("") ? null : txtHistLotID.Text;
                newRow["EQPT_MOUNT_PSTN_ID"] = cboHistMountPstsID.SelectedValue.ToString().Equals("") ? null : cboHistMountPstsID.SelectedValue.ToString();

                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgInputHist.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgInputHist, searchResult, FrameOperation);

                        if (dgInputHist.CurrentCell != null)
                            dgInputHist.CurrentCell = dgInputHist.GetCell(dgInputHist.CurrentCell.Row.Index, dgInputHist.Columns.Count - 1);
                        else if (dgInputHist.Rows.Count > 0 && dgInputHist.GetCell(dgInputHist.Rows.Count, dgInputHist.Columns.Count - 1) != null)
                            dgInputHist.CurrentCell = dgInputHist.GetCell(dgInputHist.Rows.Count, dgInputHist.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        public void GetProcMtrlInputRule()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_PROC_MTRL_INPUT_RULE";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = ProcessCode;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt) && dtRslt.Columns.Contains("MAX_PRE_PROC_END_DAY"))
                {
                    _maxPeviewProcessEndDay = Util.NVC(dtRslt.Rows[0]["MAX_PRE_PROC_END_DAY"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void GetWaitPancake()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_WAIT_LOT_LIST_WN_BY_LV3_CODE";
                string sInMtrlClssCode = GetInputMtrlClssCode();
                ShowParentLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_READY_LOT_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = EquipmentSegment;
                newRow["EQPTID"] = EquipmentId;
                newRow["PROCID"] = ProcessCode;
                newRow["WOID"] = ProdWorkOrderId;
                newRow["IN_LOTID"] = txtWaitPancakeLot.Text;
                //newRow["PRDT_CLSS_CODE"] = null;  -- 설비 조건으로 PROD_CLASS_CODE 조회 함..
                newRow["INPUT_MTRL_CLSS_CODE"] = sInMtrlClssCode;
                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        // FIFO 기준 Date
                        if (CommonVerify.HasTableRow(searchResult) && searchResult.Columns.Contains("VALID_DATE_YMDHMS"))
                        {
                            DateTime.TryParse(Util.NVC(searchResult.Rows[0]["VALID_DATE_YMDHMS"]), out _dtMinValid);
                        }

                        //dgWaitPancake.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgWaitPancake, searchResult, FrameOperation);

                        //lblSelWaitPancakeCnt.Text = (dgWaitPancake.Rows.Count - dgWaitPancake.BottomRows.Count).ToString();

                        if (dgWaitPancake.CurrentCell != null)
                            dgWaitPancake.CurrentCell = dgWaitPancake.GetCell(dgWaitPancake.CurrentCell.Row.Index, dgWaitPancake.Columns.Count - 1);
                        else if (dgWaitPancake.Rows.Count > 0 && dgWaitPancake.GetCell(dgWaitPancake.Rows.Count, dgWaitPancake.Columns.Count - 1) != null)
                            dgWaitPancake.CurrentCell = dgWaitPancake.GetCell(dgWaitPancake.Rows.Count, dgWaitPancake.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private string GetInputMtrlClssCode()
        {
            try
            {
                if (cboPancakeMountPstnID?.SelectedValue == null)
                {
                    return string.Empty;
                }

                const string bizRuleName = "DA_PRD_SEL_INPUT_PRDT_CLSS_CODE_BY_MOUNT_PSTN_ID";
                string sInputMtrlClssCode = string.Empty;

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_INPUT_MTRL_CLSS_CODE();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = EquipmentId;
                newRow["EQPT_MOUNT_PSTN_ID"] = cboPancakeMountPstnID.SelectedValue.ToString();

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sInputMtrlClssCode = Util.NVC(dtResult.Rows[0]["INPUT_MTRL_CLSS_CODE"]);
                    txtWaitPancakeInputClssCode.Text = Util.NVC(dtResult.Rows[0]["INPUT_MTRL_CLSS_CODE"]);
                }
                return sInputMtrlClssCode;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }

        public void GetWaitBox()
        {
            try
            {
                ShowParentLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_WAIT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = ProcessCode != Process.SSC_FOLDED_BICELL ? Process.PACKAGING : Process.SSC_FOLDED_BICELL;
                newRow["EQSGID"] = EquipmentSegment;
                newRow["EQPTID"] = EquipmentId;
                newRow["WO_DETL_ID"] = ProdWorkOrderDetailId;

                inTable.Rows.Add(newRow);

                const string bizRuleName = "DA_PRD_SEL_WAIT_LOT_LIST_CL";

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        // FIFO 기준 Date
                        if (searchResult != null && searchResult.Rows.Count > 0 && searchResult.Columns.Contains("VALID_DATE_YMDHMS"))
                        {
                            DateTime.TryParse(Util.NVC(searchResult.Rows[0]["VALID_DATE_YMDHMS"]), out _dtMinValid);
                        }


                        Util.GridSetData(dgInputMaterial, searchResult, FrameOperation);

                        if (dgInputMaterial.CurrentCell != null)
                            dgInputMaterial.CurrentCell = dgInputMaterial.GetCell(dgInputMaterial.CurrentCell.Row.Index, dgInputMaterial.Columns.Count - 1);
                        else if (dgInputMaterial.Rows.Count > 0 && dgInputMaterial.GetCell(dgInputMaterial.Rows.Count, dgInputMaterial.Columns.Count - 1) != null)
                            dgInputMaterial.CurrentCell = dgInputMaterial.GetCell(dgInputMaterial.Rows.Count, dgInputMaterial.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void BasketInput(bool bAuto, int iRow)
        {
            try
            {

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_BASKET_CL();

                DataTable inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentId;
                newRow["PROD_LOTID"] = ProdLotId;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                if (bAuto)
                {
                    if (iRow < 0)
                        return;

                    DataTable inMtrlTable = indataSet.Tables["IN_INPUT"];
                    newRow = inMtrlTable.NewRow();
                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[iRow].DataItem, "LOTID"));
                    newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[iRow].DataItem, "PRODID"));
                    newRow["EQPT_MOUNT_PSTN_ID"] = cboInputMaterialMountPstsID.SelectedValue?.ToString() ?? "";
                    newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                    newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[iRow].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[iRow].DataItem, "WIPQTY")));

                    inMtrlTable.Rows.Add(newRow);
                }
                else
                {
                    DataTable inMtrlTable = indataSet.Tables["IN_INPUT"];

                    for (int i = 0; i < dgInputMaterial.Rows.Count - dgInputMaterial.BottomRows.Count; i++)
                    {
                        if (!_util.GetDataGridCheckValue(dgInputMaterial, "CHK", i)) continue;
                        newRow = inMtrlTable.NewRow();
                        newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "LOTID"));
                        newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "PRODID"));
                        newRow["EQPT_MOUNT_PSTN_ID"] = cboInputMaterialMountPstsID.SelectedValue?.ToString() ?? "";
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "WIPQTY")));

                        inMtrlTable.Rows.Add(newRow);
                    }
                }

                ShowParentLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_INPUT_LOT_CL", "IN_EQP,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetProductLot();
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void BoxInputCancel()
        {

            try
            {
                //const string bizRuleName = "BR_PRD_REG_CANCEL_TERMINATE_LOT";
                string bizRuleName = string.Equals(ProcessCode, Process.WASHING) ? "BR_PRD_REG_CANCEL_TERMINATE_LOT_WS" : "BR_PRD_REG_CANCEL_TERMINATE_LOT";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_CANCEL_BASKET_CL();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inMtrlTable = indataSet.Tables["INLOT"];

                for (int i = 0; i < dgInputHist.Rows.Count - dgInputHist.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgInputHist, "CHK", i)) continue;
                    newRow = null;
                    newRow = inMtrlTable.NewRow();
                    newRow["INPUT_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[i].DataItem, "INPUT_SEQNO")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[i].DataItem, "INPUT_SEQNO")));
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[i].DataItem, "INPUT_LOTID"));
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[i].DataItem, "INPUT_QTY")));

                    inMtrlTable.Rows.Add(newRow);
                }
                //string xml = indataSet.GetXml();
                ShowParentLoadingIndicator();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetInputHistory();
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                }, indataSet);

            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }

        }

        private string GetNowRunProdLot()
        {
            try
            {
                ShowParentLoadingIndicator();

                string sNowLot = "";
                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_NOW_PROD_INFO();

                DataRow newRow = inTable.NewRow();

                newRow["PROCID"] = ProcessCode;
                newRow["EQPTID"] = EquipmentId;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_NOW_PROD_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    sNowLot = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                }

                return sNowLot;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HiddenParentLoadingIndicator();
            }
        }

        private void PancakeInput()
        {
            try
            {
                ShowParentLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_START_INPUT_IN_LOT_WN";

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_MTRL_INPUT_WN();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentId;
                newRow["PROD_LOTID"] = ProdLotId;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                for (int i = 0; i < dgWaitPancake.Rows.Count - dgWaitPancake.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgWaitPancake, "CHK", i)) continue;

                    newRow = inInputTable.NewRow();
                    newRow["EQPT_MOUNT_PSTN_ID"] = cboPancakeMountPstnID.SelectedValue.ToString();
                    newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitPancake.Rows[i].DataItem, "LOTID"));

                    inInputTable.Rows.Add(newRow);
                }

                string xml = indataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //GetProductLot(GetSelectWorkOrderInfo());
                        GetProductLot();

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }

        }

        private bool CanCurrInCancel()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!ProcessCode.Equals(Process.PACKAGING))
            {
                if (Util.NVC(ProdLotId).Equals(""))
                {
                    //Util.Alert("선택된 실적정보가 없습니다.");
                    Util.MessageValidation("SFU1640");
                    return false;
                }
            }

            if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private bool CanCurrInReplace()
        {
            int iRow = _util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private bool ValidationInHalfProductInPutCancel()
        {
            int iRow = _util.GetDataGridCheckFirstRowIndex(dgInHalfProduct, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[iRow].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private bool ValidationInputMaterialCancel()
        {
            int iRow = _util.GetDataGridCheckFirstRowIndex(dgInputMaterial, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[iRow].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private bool ValidationInputWaitHalfProduct()
        {
            if (cboWaitHalfProduct.SelectedValue == null || cboWaitHalfProduct.SelectedValue.GetString() == "SELECT" || string.IsNullOrEmpty(cboWaitHalfProduct.SelectedValue.GetString()))
            {
                //투입 위치를 선택하세요.
                Util.MessageValidation("SFU1957");
                return false;
            }

            if (_util.GetDataGridCheckCnt(dgWaitHalfProduct, "CHK") < 1)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationSaveDefect()
        {
            if (!CommonVerify.HasDataGridRow(dgDefect))
            {
                //Util.Alert("불량 항목이 없습니다.");
                Util.MessageValidation("SFU1578");
                return false;
            }

            if (ProdLotId.Length < 1)
            {
                Util.MessageValidation("SFU1195");
                return false;
            }

            return true;
        }

        private bool CanCurrInComplete()
        {
            int iRow = _util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private bool CanCurrAutoInputLot()
        {
            if (txtCurrInLotID.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU1379");
                return false;
            }

            if (ProdLotId.Equals(""))
            {
                //Util.Alert("선택한 작업대상 LOT이 없어 투입할 수 없습니다.");
                Util.MessageValidation("SFU1664");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK") < 0)
            {
                //Util.Alert("투입 위치를 선택하세요.");
                Util.MessageValidation("SFU1957");
                return false;
            }

            if (ProcessCode.Equals(Process.LAMINATION) || ProcessCode.Equals(Process.STACKING_FOLDING))
            {
                for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID")).Equals(txtCurrInLotID.Text.Trim()))
                    {
                        Util.MessageValidation("SFU3278", Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_NAME"))); // %1 에 이미 투입되었습니다.
                        return false;
                    }
                }
            }
            else
            {
                for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID")).Equals(txtCurrInLotID.Text.Trim()))
                        {
                            Util.MessageValidation("SFU3278", Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_NAME")));    // %1 에 이미 투입되었습니다.
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public bool CanPkgConfirm(string sProdLot)
        {
            for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "WIPSTAT")).Equals("PROC") &&
                        Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "PROD_LOTID")).Equals(sProdLot))
                    {
                        //Util.Alert("[{0}] 위치에 투입완료되지 않은 바구니[{1}]가 존재 합니다.", Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_NAME")), Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID")));
                        object[] parameters = new object[2];
                        parameters[0] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_NAME"));
                        parameters[1] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID"));

                        Util.MessageValidation("SFU1282", parameters);
                        return false;
                    }
                }
            }

            return true;
        }

        private bool CanWaitPanCakeInput()
        {
            if (Util.NVC(ProdLotId).Equals(""))
            {
                //Util.Alert("선택한 작업대상 LOT이 없습니다.");
                Util.MessageValidation("SFU1663");
                return false;
            }

            if (cboPancakeMountPstnID.SelectedValue == null || cboPancakeMountPstnID.SelectedValue.Equals("") || cboPancakeMountPstnID.SelectedValue.Equals("SELECT"))
            {
                //Util.Alert("투입 위치를 선택 하세요.");
                Util.MessageValidation("SFU1957");
                return false;
            }

            if (_util.GetDataGridCheckCnt(dgWaitPancake, "CHK") < 1)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            
            int rowIndex = _util.GetDataGridRowIndex(dgCurrIn, "EQPT_MOUNT_PSTN_ID", cboPancakeMountPstnID.SelectedValue.ToString());
            if (rowIndex >= 0)
            {
                string classCode = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[rowIndex].DataItem, "PRDT_CLSS_CODE"));

                if (classCode != "C")
                {
                    string sInPancake = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[rowIndex].DataItem, "INPUT_LOTID"));
                    string sInState = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[rowIndex].DataItem, "WIPSTAT"));
                    string sMtgrid = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[rowIndex].DataItem, "MOUNT_MTRL_TYPE_CODE"));

                    if (sMtgrid.Equals("PROD") && sInState.Equals("PROC"))//if (!sInPancake.Trim().Equals(""))
                    {
                        //Util.Alert("{0} 에 진행중인 LOT이 존재 합니다.", Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "EQPT_MOUNT_PSTN_NAME")));
                        Util.MessageValidation("SFU1290", Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[rowIndex].DataItem, "EQPT_MOUNT_PSTN_NAME")));
                        return false;
                    }
                }
            }

            //if (iRow >= 0)
            //{
            //    string sInPancake = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "INPUT_LOTID"));
            //    string sInState = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "WIPSTAT"));
            //    string sMtgrid = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "MOUNT_MTRL_TYPE_CODE"));

            //    if (sMtgrid.Equals("PROD") && sInState.Equals("PROC"))//if (!sInPancake.Trim().Equals(""))
            //    {
            //        //Util.Alert("{0} 에 진행중인 LOT이 존재 합니다.", Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "EQPT_MOUNT_PSTN_NAME")));
            //        Util.MessageValidation("SFU1290", Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "EQPT_MOUNT_PSTN_NAME")));
            //        return false;
            //    }
            //}
            
            
            return true;
        }

        public bool CanStkConfirm(string sProdLot)
        {
            for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "PROD_LOTID")).Equals(sProdLot) &&
                        Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "PRODUCT_LEVEL2_CODE")).Equals("MC"))   // Mono Cell 만 체크..
                    {
                        object[] parameters = new object[1];
                        parameters[0] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_NAME"));
                        //parameters[1] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID"));

                        Util.MessageValidation("SFU1290", parameters);
                        return false;
                    }
                }
            }

            return true;
        }

        private bool CanWaitBoxInPut()
        {
            if (Util.NVC(ProdLotId).Equals(""))
            {
                //Util.Alert("선택한 작업대상 LOT이 없습니다.");
                Util.MessageValidation("SFU1663");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgInputMaterial, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            // 패키지 바구니 투입 시 복수개의 PROD가 존재하는 경우 처리 되도록 변경.
            if (cboInputMaterialMountPstsID == null || cboInputMaterialMountPstsID.SelectedValue == null || cboInputMaterialMountPstsID.SelectedIndex < 0)
            {
                //Util.Alert("투입 위치를 선택하세요.");
                Util.MessageValidation("SFU1957");
                return false;
            }

            int iRow = _util.GetDataGridRowIndex(dgCurrIn, "EQPT_MOUNT_PSTN_ID", cboInputMaterialMountPstsID.SelectedValue.ToString());
            if (iRow < 0)
            {
                //Util.Alert("자재 투입 위치에 존재하지 않는 투입 위치 입니다.");
                Util.MessageValidation("SFU1819");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "WIPSTAT")).Equals("PROC"))
            {
                //Util.Alert("[{0}] 위치에 RUN 상태의 바구니가 존재 합니다.\n완료 처리 후 투입 하십시오.", cboBoxMountPstsID.Text);
                Util.MessageValidation("SFU1281", cboInputMaterialMountPstsID.Text);
                return false;
            }

            return true;
        }

        private void PopupReplaceWinding()
        {
            CMM_WINDING_PAN_REPLACE popPanReplace = new CMM_WINDING_PAN_REPLACE { FrameOperation = FrameOperation };

            int idx = _util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");

            object[] parameters = new object[8];
            parameters[0] = EquipmentSegment;
            parameters[1] = EquipmentId;
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOTID"));
            parameters[3] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "WIPSEQ"));
            parameters[4] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_QTY"));
            parameters[5] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID"));
            parameters[6] = ProcessCode;
            parameters[7] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOT_TYPE_CODE"));
            C1WindowExtension.SetParameters(popPanReplace, parameters);

            popPanReplace.Closed += new EventHandler(popWindingReplace_Closed);

            // 팝업 화면 숨겨지는 문제 수정.
            //this.Dispatcher.BeginInvoke(new Action(() => wndReplace.ShowModal()));
            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    grid.Children.Add(popPanReplace);
                    popPanReplace.BringToFront();
                    break;
                }
            }
        }

        private void PopupReplaceAssemblySmallType()
        {
            CMM_ASSY_CSH_PAN_REPLACE popPanReplace = new CMM_ASSY_CSH_PAN_REPLACE { FrameOperation = FrameOperation };

            int idx = _util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");

            object[] parameters = new object[8];
            parameters[0] = EquipmentSegment;
            parameters[1] = EquipmentId;
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOTID"));
            parameters[3] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "WIPSEQ"));
            parameters[4] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_QTY"));
            parameters[5] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID"));
            parameters[6] = ProcessCode;
            parameters[7] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOT_TYPE_CODE"));
            C1WindowExtension.SetParameters(popPanReplace, parameters);

            popPanReplace.Closed += new EventHandler(popAssemblySmallTypeReplace_Closed);

            // 팝업 화면 숨겨지는 문제 수정.
            //this.Dispatcher.BeginInvoke(new Action(() => wndReplace.ShowModal()));
            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    grid.Children.Add(popPanReplace);
                    popPanReplace.BringToFront();
                    break;
                }
            }
        }

        private void PopupReplaceAssembly()
        {
            CMM_ASSY_WG_PAN_REPLACE popPanReplace = new CMM_ASSY_WG_PAN_REPLACE { FrameOperation = FrameOperation };

            int idx = _util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");

            object[] parameters = new object[8];
            parameters[0] = EquipmentSegment;
            parameters[1] = EquipmentId;
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOTID"));
            parameters[3] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "WIPSEQ"));
            parameters[4] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_QTY"));
            parameters[5] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID"));
            parameters[6] = ProcessCode;
            parameters[7] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOT_TYPE_CODE"));
            C1WindowExtension.SetParameters(popPanReplace, parameters);

            popPanReplace.Closed += new EventHandler(popAssemblyReplace_Closed);

            // 팝업 화면 숨겨지는 문제 수정.
            //this.Dispatcher.BeginInvoke(new Action(() => wndReplace.ShowModal()));
            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    grid.Children.Add(popPanReplace);
                    popPanReplace.BringToFront();
                    break;
                }
            }
        }

        private void PopupReplaceWashingRework()
        {
            CMM_ASSY_WSH_REWORK_PAN_REPLACE popReplace = new CMM_ASSY_WSH_REWORK_PAN_REPLACE { FrameOperation = FrameOperation };

            int idx = _util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");

            object[] parameters = new object[6];
            parameters[0] = EquipmentId;
            parameters[1] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOTID"));
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID"));
            parameters[3] = ProcessCode;
            parameters[4] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOT_TYPE_CODE"));
            parameters[5] = IsReWork;
            C1WindowExtension.SetParameters(popReplace, parameters);

            popReplace.Closed += new EventHandler(popWashingReworkReplace_Closed);

            // 팝업 화면 숨겨지는 문제 수정.
            //this.Dispatcher.BeginInvoke(new Action(() => wndReplace.ShowModal()));
            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    grid.Children.Add(popReplace);
                    popReplace.BringToFront();
                    break;
                }
            }
        }

        private void popWashingReworkReplace_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_WSH_REWORK_PAN_REPLACE pop = sender as CMM_ASSY_WSH_REWORK_PAN_REPLACE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                //GetProductLot();
                GetCurrInList();
            }
            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    grid.Children.Remove(pop);
                    break;
                }
            }
        }


        private void PopuoReplaceWashing()
        {
            CMM_ASSY_WSH_PAN_REPLACE popReplace = new CMM_ASSY_WSH_PAN_REPLACE { FrameOperation = FrameOperation };

            int idx = _util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");

            object[] parameters = new object[5];
            parameters[0] = EquipmentId;
            parameters[1] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOTID"));
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID"));
            parameters[3] = ProcessCode;
            parameters[4] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOT_TYPE_CODE"));
            C1WindowExtension.SetParameters(popReplace, parameters);

            popReplace.Closed += new EventHandler(popWashingReplace_Closed);

            // 팝업 화면 숨겨지는 문제 수정.
            //this.Dispatcher.BeginInvoke(new Action(() => wndReplace.ShowModal()));
            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    grid.Children.Add(popReplace);
                    popReplace.BringToFront();
                    break;
                }
            }
        }

        private void popWashingReplace_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_WSH_PAN_REPLACE pop = sender as CMM_ASSY_WSH_PAN_REPLACE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                //GetProductLot();
                GetCurrInList();
            }
            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    grid.Children.Remove(pop);
                    break;
                }
                    
            }
        }

        private void PopuoReplaceWashingSmallType()
        {
            
        }

        private bool CanInBoxInputCancel(C1.WPF.DataGrid.C1DataGrid dg)
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dg, "CHK") < 0 || !CommonVerify.HasDataGridRow(dg))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private void ShowParentLoadingIndicator()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("ShowLoadingIndicator");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }

                methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void HiddenParentLoadingIndicator()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("HiddenLoadingIndicator");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }

                methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataRow GetSelectProductRow()
        {
            if (UcParentControl == null)
                return null;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("GetSelectProductRow");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }

                return (DataRow)methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void CheckInDataGridCheckBox(C1DataGrid dg, DataGridBeganEditEventArgs e)
        {
            if (dg == null) return;

            if (e?.Row != null)
            {
                if (e.Row.Index < 0 || e.Column.Name != "CHK") return;

                int rowIndex = e.Row.Index;

                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (i == rowIndex && !Convert.ToBoolean(DataTableConverter.GetValue(e.Row.DataItem, "CHK")))
                    {
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", true);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);
                    }
                }
            }
        }

        private void DataGridCurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;

                CheckBox chk = e.Cell?.Presenter?.Content as CheckBox;

                if (chk != null)
                {
                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHK":
                            if (dg != null)
                            {
                                var checkBox = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                if (checkBox != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                         dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                         checkBox.IsChecked.HasValue &&
                                                         !(bool)checkBox.IsChecked))
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                    chk.IsChecked = true;

                                    for (int i = 0; i < dg.Rows.Count; i++)
                                    {
                                        if (i != e.Cell.Row.Index)
                                        {
                                            DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                            if (dg.GetCell(i, e.Cell.Column.Index).Presenter != null)
                                            {
                                                chk = dg.GetCell(i, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                if (chk != null) chk.IsChecked = false;
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    var box = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                    if (box != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                        dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                        box.IsChecked.HasValue &&
                                                        (bool)box.IsChecked))
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                        chk.IsChecked = false;
                                    }
                                }
                            }
                            break;
                    }
                    if (dg?.CurrentCell != null)
                        dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                    else if (dg != null && (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null))
                        dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
                }
            }));
        }

        private void SaveDefect(bool bMsgShow = true)
        {
            try
            {
                dgDefect.EndEdit();

                const string bizRuleName = "BR_QCA_REG_WIPREASONCOLLECT_MOBILE";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentId;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable inDefectLot = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = null;

                    newRow = inDefectLot.NewRow();
                    newRow["LOTID"] = ProdLotId;
                    newRow["WIPSEQ"] = ProdWorkInProcessSequence;
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

                    inDefectLot.Rows.Add(newRow);
                }
                string xml = indataSet.GetXml();
                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INRESN", null, indataSet);

                //if (bMsgShow)
                //    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                //IsChangeDefect = false;
                GetProductLot();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SaveDefectBeforeConfirm()
        {
            try
            {
                dgDefect.EndEdit();

                const string bizRuleName = "BR_QCA_REG_WIPREASONCOLLECT_MOBILE";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentId;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable inDefectLot = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = null;

                    newRow = inDefectLot.NewRow();
                    newRow["LOTID"] = ProdLotId;
                    newRow["WIPSEQ"] = ProdWorkInProcessSequence;
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

                    inDefectLot.Rows.Add(newRow);
                }
                string xml = indataSet.GetXml();
                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INRESN", null, indataSet);

                //IsChangeDefect = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetResultDetailControl(object selectedItem)
        {
            DataRowView rowview = selectedItem as DataRowView;
            if (rowview == null) return;

            txtWorkOrder.Text = rowview["WOID"].GetString();
            txtLotId.Text = rowview["LOTID"].GetString();
            txtProdId.Text = rowview["PRODID"].GetString();
            txtStartTime.Text = rowview["WIPDTTM_ST"].GetString();
            txtEndTime.Text = rowview["WIPDTTM_ED"].GetString();
            txtProdVerCode.Text = rowview["PROD_VER_CODE"].GetString();
            //txtWorkMinute

            DateTime dTmpEnd;
            DateTime dTmpStart;

            if (!string.IsNullOrEmpty(rowview["WIPDTTM_ST"].GetString()) && string.IsNullOrEmpty(rowview["WIPDTTM_ED"].GetString()))
            {
                if (DateTime.TryParse(txtStartTime.Text, out dTmpStart))
                {
                    txtWorkMinute.Text = Math.Truncate(DateTime.Now.Subtract(dTmpStart).TotalMinutes).ToString(CultureInfo.InvariantCulture);
                }
            }

            else if (!string.IsNullOrEmpty(rowview["WIPDTTM_ST"].GetString()) && !string.IsNullOrEmpty(rowview["WIPDTTM_ED"].GetString()))
            {
                if (DateTime.TryParse(txtEndTime.Text, out dTmpEnd) && DateTime.TryParse(txtStartTime.Text, out dTmpStart))
                    txtWorkMinute.Text = Math.Truncate(dTmpEnd.Subtract(dTmpStart).TotalMinutes).ToString(CultureInfo.InvariantCulture);
            }

            txtRemark.AppendText(rowview["REMARK"].GetString());
        }

        protected virtual void GetDefectInfo()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("GetDefectInfo");
                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    object[] parameterArrys = new object[parameters.Length];
                    parameterArrys[0] = ProdLotId;
                    parameterArrys[1] = ProdWorkInProcessSequence;

                    methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected virtual void CalculateDefectQty()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("CalculateDefectQty");
                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    object[] parameterArrys = new object[parameters.Length];

                    methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private Decimal GetSumDefectQty()
        {
            if (!CommonVerify.HasDataGridRow(dgDefect))
                return 0;

            decimal defectqty = 0;
            decimal lossqty = 0;
            decimal chargeprdqty = 0;

            DataTable dt = ((DataView)dgDefect.ItemsSource).Table;
            defectqty = dt.AsEnumerable().Where(w => w.Field<string>("ACTID").Equals("DEFECT_LOT") && !w.Field<string>("RSLT_EXCL_FLAG").Equals("Y")).Sum(s => s.Field<Decimal>("RESNQTY"));
            lossqty = dt.AsEnumerable().Where(w => w.Field<string>("ACTID").Equals("LOSS_LOT")).Sum(s => s.Field<Decimal>("RESNQTY"));
            chargeprdqty = dt.AsEnumerable().Where(w => w.Field<string>("ACTID").Equals("CHARGE_PROD_LOT")).Sum(s => s.Field<Decimal>("RESNQTY"));

            return defectqty + lossqty + chargeprdqty;
        }

        #endregion



        
    }
}
