using System;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.Data;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcFormProductionPalette.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcPolymerFormProductionResultDSF
    {
        #region Declaration

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public UserControl UcParentControl;

        public C1DataGrid DgProductionInbox { get; set; }
        public C1DataGrid DgProductionDefectExternal { get; set; }
        public C1DataGrid DgProductionDefectEquipment { get; set; }

        public string ProdLotId { get; set; }
        public string ProcessCode { get; set; }
        public string EquipmentCode { get; set; }
        public string WipSeq { get; set; }





        public UcPolymerFormProductionResultDSF()
        {
            InitializeComponent();
            SetControl();
            SetButtons();
            SetCombo();
        }

        #endregion

        #region Initialize

        private void SetControl()
        {
            DgProductionInbox = dgProductionInbox;
            DgProductionDefectExternal = dgDefectExternal;
            DgProductionDefectEquipment = dgDefectEquipment;
        }
        private void SetButtons()
        {
        }

        private void SetCombo()
        {
            // 제품별일 경우 생산 Lot 선택시 변경 필요
            CommonCombo combo = new CommonCombo();
            string[] sFilterCapa = { LoginInfo.CFG_AREA_ID, "CAPA_GRD_CODE", "G" };
            combo.SetCombo(cboCapaType, CommonCombo.ComboStatus.NONE, sCase: "FORM_GRADE_TYPE_CODE", sFilter: sFilterCapa);
        }

        public void InitializeControls()
        {
            SetDataGridCheckHeaderInitialize(dgProductionInbox);
            SetDataGridCheckHeaderInitialize(dgDefectExternal);
            SetDataGridCheckHeaderInitialize(dgDefectEquipment);

            Util.gridClear(dgProductionInbox);
            txtInboxType.Text = string.Empty;
            Util.gridClear(dgDefectExternal);
            Util.gridClear(dgDefectEquipment);
        }

        #endregion

        #region Event

        #region ### [완성 Inbox] ###

        #region 대차변경
        private void btnCartChange_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Inbox 생성
        private void btnInboxCreate_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Inbox 삭제
        private void btnInboxDelete_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Inbox 저장
        private void btnInboxSave_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region 양품 태그 발행
        private void btnTagPrint_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Grid Event
        private void dgProductionInbox_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //if (sender == null)
            //    return;

            //C1DataGrid dg = sender as C1DataGrid;

            //dg?.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (e.Cell.Presenter == null)
            //    {
            //        return;
            //    }

            //    //Grid Data Binding 이용한 Background 색 변경
            //    if (e.Cell.Row.Type == DataGridRowType.Item)
            //    {
            //        var convertFromString = ColorConverter.ConvertFromString("#E8F7C8");

            //        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD")).Equals("Y"))
            //        {
            //            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
            //        }
            //        else
            //        {
            //            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
            //        }

            //        if (string.Equals(ProcessCode, Process.CircularCharacteristic))
            //        {
            //            // 원형 특성인 경우 출고에 따른 색상 표시
            //            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblReady.Tag))
            //            {
            //                e.Cell.Presenter.Background = lblReady.Background;
            //            }
            //            else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblShipping.Tag))
            //            {
            //                e.Cell.Presenter.Background = lblShipping.Background;
            //            }
            //            else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblShipped.Tag))
            //            {
            //                e.Cell.Presenter.Background = lblShipped.Background;
            //            }
            //            else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblCancel.Tag))
            //            {
            //                e.Cell.Presenter.Background = lblCancel.Background;
            //            }
            //            else
            //            {
            //                e.Cell.Presenter.Background = null;
            //            }

            //        }
            //        else
            //        {
            //            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("Y"))
            //            {
            //                if (convertFromString != null)
            //                    e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
            //            }
            //            else
            //            {
            //                e.Cell.Presenter.Background = null;
            //            }
            //        }

            //    }
            //}));
        }

        private void dgProductionInbox_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //if (sender == null)
            //    return;

            //C1DataGrid dg = sender as C1DataGrid;

            //dg?.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (e.Cell.Presenter != null)
            //    {
            //        if (e.Cell.Row.Type == DataGridRowType.Item)
            //        {
            //            e.Cell.Presenter.Background = null;
            //        }
            //    }
            //}));
        }

        private void chkDefectExternalHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgDefectExternal);
        }

        private void chkDefectExternalHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgDefectExternal);
        }

        private void chkDefectEquipmentHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgDefectEquipment);
        }

        private void chkDefectEquipmentHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgDefectEquipment);
        }

        private void dgDefectEquipment_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgDefectEquipment_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgDefectEquipment_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {

        }

        private void dgDefectExternal_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
            {
                CheckBox cbo = e.Cell.Presenter.Content as CheckBox;
                if (cbo != null)
                {
                    cbo.Visibility = DataTableConverter.GetValue(e.Cell.Row.DataItem, "ACTID").GetString() == "DEFECT_LOT" ? Visibility.Visible : Visibility.Collapsed;
                    cbo.IsChecked = string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN").GetString(), "N");
                }
            }

            if (e.Cell.Row.Type == DataGridRowType.Item && string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN").GetString(), "Y"))
            {
                //var convertFromString = ColorConverter.ConvertFromString("#F8DAC0");
                //if (convertFromString != null)
                //    e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
            }
        }

        private void dgDefectExternal_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgDefectExternal_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
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

        private void chkAllInbox_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgProductionInbox);
        }

        private void chkAllInbox_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgProductionInbox);
        }

        #endregion

        #endregion

        #region ### [불량(외관)] ###
        #region 대차변경
        private void btnDefectExternalPrint_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Inbox 생성
        private void btnDefectExternalSave_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region 불량 Cell 등록
        private void btnCellSave_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Grid Event
        #endregion

        #endregion

        #region ### [불량(설비)] ###
        #region 대차변경
        private void btnDefectPrintEquipment_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Inbox 생성
        private void btnDefectSaveEquipment_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region 불량 Cell 등록
        #endregion

        #region Grid Event
        #endregion

        #endregion

        #endregion

        #region Mehod

        public void SetButtonVisibility()
        {

        }

        public void SetControlHeader()
        {

        }

        public void SelectResultList()
        {
            SelectInboxList();
            SelectDefectDefectExternal();
            SelectDefectEquipment();
        }

        private void SelectInboxList()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_INBOX_PC";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("DELETE_YN", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = ProdLotId;
                newRow["WIPSEQ"] = WipSeq;
                newRow["DELETE_YN"] = "N";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                //dgProductionInbox.CurrentCellChanged -= dgProductionInbox_CurrentCellChanged;

                Util.GridSetData(dgProductionInbox, dtResult, FrameOperation, true);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dgProductionInbox.CurrentCell = dgProductionInbox.GetCell(0, 1);

                //dgProductionInbox.CurrentCellChanged += dgProductionInbox_CurrentCellChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectDefectDefectExternal()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectDefectEquipment()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public DataRow GetSelectPalletLotRow()
        {
            DataRow row;

            try
            {
                DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

                if (dr == null || dr.Length < 1)
                    row = null;
                else
                    row = dr[0];

                return row;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        
        public void SelectPalletList()
        {
            //try
            //{
            //    chkAll.IsChecked = false;

            //    DataTable inTable = new DataTable();
            //    inTable.Columns.Add("LANGID", typeof(string));
            //    inTable.Columns.Add("PR_LOTID", typeof(string));
            //    inTable.Columns.Add("WIPSEQ", typeof(string));

            //    DataRow newRow = inTable.NewRow();
            //    newRow["LANGID"] = LoginInfo.LANGID;
            //    newRow["PR_LOTID"] = ProdLotId;
            //    newRow["WIPSEQ"] = WipSeq;
            //    inTable.Rows.Add(newRow);

            //    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_FO", "INDATA", "OUTDATA", inTable);

            //    //dgProductionPallet.CurrentCellChanged -= dgProductionPallet_CurrentCellChanged;

            //    Util.GridSetData(dgProductionPallet, dtResult, FrameOperation, true);

            //    if (dtResult != null && dtResult.Rows.Count > 0)
            //        dgProductionPallet.CurrentCell = dgProductionPallet.GetCell(0, 1);

            //    //dgProductionPallet.CurrentCellChanged += dgProductionPallet_CurrentCellChanged;
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        public void SelectPalletSummary()
        {
            //try
            //{
            //    DataTable inTable = new DataTable();
            //    inTable.Columns.Add("LANGID", typeof(string));
            //    inTable.Columns.Add("PR_LOTID", typeof(string));
            //    inTable.Columns.Add("WIPSEQ", typeof(string));

            //    DataRow newRow = inTable.NewRow();
            //    newRow["LANGID"] = LoginInfo.LANGID;
            //    newRow["PR_LOTID"] = ProdLotId;
            //    newRow["WIPSEQ"] = WipSeq;
            //    inTable.Rows.Add(newRow);

            //    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_SUM_FO", "INDATA", "OUTDATA", inTable);

            //    Util.GridSetData(dgPalletSummary, dtResult, null);

            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private bool ValidationCartChange()
        {
            if (string.IsNullOrWhiteSpace(ProdLotId))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            if (_util.GetDataGridFirstRowBycheck(DgProductionInbox, "CHK") == null)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationInBoxInput()
        {
            if (string.IsNullOrWhiteSpace(ProdLotId))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtInboxType.Text))
            {
                // 설비의 Inbox 유형을 설정 하세요.
                Util.MessageValidation("설비의 Inbox 유형을 설정 하세요.");
                return false;
            }

            return true;
        }

        private void SetDataGridCheckHeaderInitialize(C1DataGrid dg)
        {
            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dg.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;
            if (allCheck?.IsChecked == true)
            {
                switch (dg.Name)
                {
                    case "dgDefectExternal":
                        allCheck.Unchecked -= chkDefectExternalHeaderAll_Unchecked;
                        allCheck.IsChecked = false;
                        allCheck.Unchecked += chkDefectExternalHeaderAll_Unchecked;
                        break;
                    case "dgDefectEquipment":
                        allCheck.Unchecked -= chkDefectEquipmentHeaderAll_Unchecked;
                        allCheck.IsChecked = false;
                        allCheck.Unchecked += chkDefectEquipmentHeaderAll_Unchecked;
                        break;
                    default: // dgProductionInbox
                        allCheck.Unchecked -= chkDefectExternalHeaderAll_Unchecked;
                        allCheck.IsChecked = false;
                        allCheck.Unchecked += chkDefectExternalHeaderAll_Unchecked;
                        break;
                }
            }
        }



        #endregion




    }
}
