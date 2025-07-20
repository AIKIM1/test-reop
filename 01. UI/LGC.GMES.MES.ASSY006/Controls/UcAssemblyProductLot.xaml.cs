/*************************************************************************************
 Created Date : 2021.12.08
      Creator : 신광희
   Decription : 소형 조립 공정진척(NFF) - 생산 LOT List UserControl
--------------------------------------------------------------------------------------
 [Change History]
 2021.12.08  신광희 : Initial Created.
 2022.04.18  김태균 : DA_PRD_SEL_INPUT_MATERIAL_AS -> DA_PRD_SEL_INPUT_MATERIAL_AS_DRB로 변경
 2023.02.02  배현우 : 투입량수정시 PROD_LOTID 매핑 오류 수정
 2023.05.23  김대현 : 조회결과가 0일때 조회버튼 비활성화 되는 현상 수정
 2023.06.13  김광규 : 투입량 관련 헤더 다국어 처리
 2023.06.21  배현우 : Assembly 투입 이력 조회시 Winding LOTID 및 투입LOTID 컬럼추가
 2023.10.25  김용군 : 오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
**************************************************************************************/
using System;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using System.Data;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using C1.WPF;
using LGC.GMES.MES.ControlsLibrary;
namespace LGC.GMES.MES.ASSY006.Controls
{
    /// <summary>
    /// UcAssemblyProductLot.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcAssemblyProductLot : UserControl
    {
        #region Declaration & Constructor

        public IFrameOperation FrameOperation { get; set; }

        public UserControl UcParentControl;

        public string EquipmentSegmentCode { get; set; }
        public string ProcessCode { get; set; }
        public string ProcessName { get; set; }
        public string EquipmentCode { get; set; }

        public bool IsProductLotChoiceRadioButtonEnable = true;

        public bool IsSearchResult;

        public C1DataGrid DgProductLot { get; set; }


        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();
        private string _selectedLotId = string.Empty;

        //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
        private string _clickLotID = string.Empty;

        DataTable _dwipColorLegnd;

        public UcAssemblyProductLot()
        {
            InitializeComponent();
            InitializeControls();

            //SetControl();
            //SetButtons();

            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            SetComboBox();
        }

        #endregion


        public void InitializeControls()
        {
            //Util.gridClear(DgProductLot);

        }

        private void SetControl()
        {
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            //DgProductLot = dgProductLot;
            //DgProductLot = ProcessCode == Process.NOTCHING ? dgProductLot : dgProductLotCommon;
            DgProductLot = ProcessCode == Process.ZTZ ? dgProductLotZtz : dgProductLot;
        }

        private void SetButtons()
        {

        }

        private void SetComboBox()
        {
            //SetWipColorLegendCombo();
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            SetWipColorLegendCombo();
        }

        public void SetControlVisibility()
        {
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            if (string.Equals(ProcessCode, Process.ZTZ))
            {
                dgProductLotZtz.Visibility = Visibility.Visible;
                dgProductLot.Visibility = Visibility.Collapsed;
                TabInputHistory.Visibility = Visibility.Collapsed;
                chkWait.Visibility = Visibility.Visible;
                cboColor.Visibility = Visibility.Visible;
                dgProductLotZtz.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
            }
            //if (string.Equals(ProcessCode, Process.WINDING))
            else if (string.Equals(ProcessCode, Process.WINDING))
            {
                //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
                dgProductLotZtz.Visibility = Visibility.Collapsed;
                dgProductLot.Visibility = Visibility.Visible;
                TabInputHistory.Visibility = Visibility.Visible;
                chkWait.Visibility = Visibility.Collapsed;
                cboColor.Visibility = Visibility.Collapsed;

                //EQPT_END_QTY
                dgProductLot.Columns["EQPT_END_QTY_M_EA"].Header = new string[] { ObjectDic.Instance.GetObjectName("투입량"), ObjectDic.Instance.GetObjectName("설비투입수량(M/EA)") }.ToList();
                dgProductLot.Columns["EQPT_END_QTY_M_EA"].Visibility = Visibility.Visible;

                dgProductLot.Columns["EQPT_END_QTY"].Header = new string[] { ObjectDic.Instance.GetObjectName("투입량"), ObjectDic.Instance.GetObjectName("설비투입수량(M/EA)") }.ToList();
                dgProductLot.Columns["EQPT_END_QTY"].Visibility = Visibility.Collapsed;

                dgProductLot.Columns["GOODQTY"].Visibility = Visibility.Visible;
                dgProductLot.Columns["CELL_CNT"].Visibility = Visibility.Visible;
                dgProductLot.Columns["JR_CNT"].Visibility = Visibility.Visible;
                dgProductLot.Columns["TRAY_CNT"].Visibility = Visibility.Visible;
                dgProductLot.Columns["WOTYPEDESC"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["PRODID"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["PRODNAME"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["CELL_CNT"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["INPUT_QTY"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["REINPUT_QTY"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["OUT_QTY"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["BOX_QTY"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["OUTPUTQTY"].Visibility = Visibility.Collapsed;
                dgInputHistoryDetail.Columns["WN_LOTID"].Visibility = Visibility.Collapsed;
                dgInputHistoryDetail.Columns["LOTID"].Visibility = Visibility.Collapsed;
                dgInputHistoryDetail.Columns["INPUT_LOTID"].Header = ObjectDic.Instance.GetObjectName("투입LOT");
            }
            else if (string.Equals(ProcessCode, Process.ASSEMBLY))
            {
                //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
                dgProductLotZtz.Visibility = Visibility.Collapsed;
                dgProductLot.Visibility = Visibility.Visible;
                TabInputHistory.Visibility = Visibility.Visible;
                chkWait.Visibility = Visibility.Collapsed;
                cboColor.Visibility = Visibility.Collapsed;

                dgProductLot.Columns["EQPT_END_QTY"].Header = new string[] { ObjectDic.Instance.GetObjectName("투입량"), ObjectDic.Instance.GetObjectName("설비생산수량") }.ToList();

                dgProductLot.Columns["INPUT_QTY"].Visibility = Visibility.Visible;
                dgProductLot.Columns["REINPUT_QTY"].Visibility = Visibility.Visible;
                dgProductLot.Columns["OUT_QTY"].Visibility = Visibility.Visible;
                dgProductLot.Columns["EQPT_END_QTY"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["EQPT_END_QTY_M_EA"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["GOODQTY"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["TRAY_CNT"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["JR_CNT"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["CELL_CNT"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["GOODQTY"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["BOX_QTY"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["OUTPUTQTY"].Visibility = Visibility.Collapsed;
                dgInputHistoryDetail.Columns["WN_LOTID"].Visibility = Visibility.Visible;
                dgInputHistoryDetail.Columns["LOTID"].Visibility = Visibility.Visible;
                dgInputHistoryDetail.Columns["INPUT_LOTID"].Header = ObjectDic.Instance.GetObjectName("TRAYID");
            }
            else
            {
                //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
                dgProductLotZtz.Visibility = Visibility.Collapsed;
                dgProductLot.Visibility = Visibility.Visible;
                TabInputHistory.Visibility = Visibility.Visible;
                chkWait.Visibility = Visibility.Collapsed;
                cboColor.Visibility = Visibility.Collapsed;

                dgProductLot.Columns["INPUT_QTY"].Visibility = Visibility.Visible;      //설비투입수  
                dgProductLot.Columns["GOODQTY"].Visibility = Visibility.Visible;        //설비양품수
                dgProductLot.Columns["BOX_QTY"].Visibility = Visibility.Visible;        //BOX수량
                dgProductLot.Columns["EQPT_END_QTY"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["EQPT_END_QTY_M_EA"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["TRAY_CNT"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["JR_CNT"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["CELL_CNT"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["REINPUT_QTY"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["INPUT_QTY"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["OUTPUTQTY"].Visibility = Visibility.Visible;
                dgInputHistoryDetail.Columns["WN_LOTID"].Visibility = Visibility.Collapsed;
                dgInputHistoryDetail.Columns["LOTID"].Visibility = Visibility.Collapsed;
                dgInputHistoryDetail.Columns["INPUT_LOTID"].Header = ObjectDic.Instance.GetObjectName("투입LOT");
            }
        }


        #region Event
        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (IsProductLotChoiceRadioButtonEnable == false) return;
            try
            {
                if (sender == null) return;

                RadioButton rb = sender as RadioButton;
                if (rb != null && rb.DataContext == null) return;

                SetUserControlProductLotSelect(rb);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProductLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;
            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {

                    string wipstat = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT"));
                    string wipHold = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD"));

                    e.Cell.Presenter.FontSize = 12;

                    if (e.Cell.Column.Name == "TOGGLEKEY")
                    {
                        if (!string.IsNullOrEmpty(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TOGGLEKEY").GetString()))
                        {
                            e.Cell.Presenter.FontSize = 16;
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            e.Cell.Presenter.Cursor = Cursors.Hand;
                        }
                    }
                    else if (e.Cell.Column.Name == "WIPSTAT_IMAGES" || e.Cell.Column.Name == "WIPSTAT_IMAGES_OUT")
                    {
                        e.Cell.Presenter.FontSize = 16;

                        e.Cell.Presenter.Foreground = wipstat == "WAIT" ? new SolidColorBrush(Colors.Yellow) : new SolidColorBrush(Colors.Green);

                        if (wipHold == "Y")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        }

                        e.Cell.Presenter.Cursor = Cursors.Arrow;
                    }

                    else if (e.Cell.Column.Name == "LOTID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                    else if (e.Cell.Column.Name == "EQPTID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        if (!string.IsNullOrEmpty(txtSelectEquipment.Text) && string.IsNullOrEmpty(txtSelectLot.Text))
                        {
                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTID").GetString() == txtSelectEquipment.Text)
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            }
                        }
                    }
                    else if (e.Cell.Column.Name == "PRJT_NAME")
                    {
                        //e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                        if (string.IsNullOrEmpty(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT_IMAGES_OUT").GetString()))
                        {
                            e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                        }
                        else
                        {
                            e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Right;
                        }
                    }
                    else if (e.Cell.Column.Name == "WIPSNAME")
                    {
                        if (string.IsNullOrEmpty(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT_IMAGES_OUT").GetString()))
                        {
                            e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Center;
                        }
                        else
                        {
                            e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Left;
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontSize = 12;
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                        e.Cell.Presenter.Cursor = Cursors.Arrow;

                        if (e.Cell.Column.Name == "IRREGL_PROD_LOT_TYPE_NAME")
                        {
                            e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Center;
                        }
                    }
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTTYPE")).Equals("X"))
                    {
                        if (e.Cell.Column.Name == "TOGGLEKEY" || e.Cell.Column.Name == "CHK" || e.Cell.Column.Name == "WIPSTAT_IMAGES") return;

                        if (e.Cell.Column.Name == "EQPTID")
                        {
                            if (!string.IsNullOrEmpty(txtSelectEquipment.Text) && string.IsNullOrEmpty(txtSelectLot.Text))
                            {
                                if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTID").GetString() == txtSelectEquipment.Text) return;
                            }
                        }

                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF2424"));
                    }

                }
            }));

        }

        private void dgProductLot_LoadingRow(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg?.ItemsSource == null) return;

            DataRowView drv = e.Row.DataItem as DataRowView;
            if (drv == null) return;

            int rowIndex = e.Row.Index;
            var row = dg.Rows[rowIndex];

            if (string.IsNullOrEmpty(drv["LOTID"].GetString()))
            {
                if (row != null)
                {
                    row.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void dgProductLot_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgProductLot.GetCellFromPoint(pnt);

                if (cell == null) return;

                int rowIdx = cell.Row.Index;
                DataRowView drv = dgProductLot.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null || string.IsNullOrEmpty(drv["TOGGLEKEY"].GetString())) return;

                string displayMode = drv["TOGGLEKEY"].GetString();
                string lotId = drv["LOTID"].GetString();
                string parentLotId = drv["PR_LOTID"].GetString();

                if (cell.Column.Name == "TOGGLEKEY" && !string.IsNullOrEmpty(displayMode))
                {
                    foreach (DataGridRow row in dg.Rows)
                    {
                        if (displayMode == "+")
                        {
                            if (DataTableConverter.GetValue(row.DataItem, "PR_LOTID").GetString() == parentLotId && DataTableConverter.GetValue(row.DataItem, "PR_LOTID").GetString() != DataTableConverter.GetValue(row.DataItem, "LOTID").GetString())
                            {
                                row.Visibility = Visibility.Visible;
                                dg.Columns["WIPSNAME"].Width = C1.WPF.DataGrid.DataGridLength.Auto;
                            }
                        }
                        else
                        {
                            if (DataTableConverter.GetValue(row.DataItem, "PR_LOTID").GetString() == parentLotId && DataTableConverter.GetValue(row.DataItem, "PR_LOTID").GetString() != DataTableConverter.GetValue(row.DataItem, "LOTID").GetString())
                            {
                                row.Visibility = Visibility.Collapsed;
                            }
                        }
                    }

                    DataTableConverter.SetValue(drv, "TOGGLEKEY", displayMode.Equals("+") ? "-" : "+");
                    dg.EndEdit();
                    dg.EndEditRow(true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProductLot_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg?.ItemsSource == null) return;

            try
            {
                for (int i = 0; i < dg.Rows.Count; i++)
                {
                    if (!string.IsNullOrEmpty(DataTableConverter.GetValue(dg.Rows[i].DataItem, "LOTID").GetString()) && DataTableConverter.GetValue(dg.Rows[i].DataItem, "OUT_LOTSEQ").GetString() == "99999")
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["LOTID"].Index), dg.GetCell(i, dg.Columns["OUT_LOTSEQ"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["PRJT_NAME"].Index), dg.GetCell(i, dg.Columns["UNIT_CODE"].Index)));
                    }

                    if (!string.IsNullOrEmpty(DataTableConverter.GetValue(dg.Rows[i].DataItem, "WIPSTAT_IMAGES_OUT").GetString()))
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["TOGGLEKEY"].Index), dg.GetCell(i, dg.Columns["LOTID"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["WIPSNAME"].Index), dg.GetCell(i, dg.Columns.Count - 1)));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void dgProductLot_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        private void rdChoice_Checked(object sender, RoutedEventArgs e)
        {
           
        }

        private void tcProductLot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tabItem = ((C1TabItem)((ItemsControl)sender).Items.CurrentItem).Name.GetString();

            if (string.Equals(tabItem, "TabInputHistory"))
            {
                if (!string.IsNullOrEmpty(txtSelectEquipment.Text) && !string.IsNullOrEmpty(txtSelectLot.Text))
                {

                    C1DataGrid dg = dgInputHistory;
                    int idx = -1;

                    if (!CommonVerify.HasDataGridRow(dg) || !dg.Rows.Any(x => x.Visibility == Visibility.Visible && x.Type == DataGridRowType.Item)) return;

                    var dr = (from DataGridRow rows in dg.Rows
                              where rows.DataItem != null
                                    && rows.Visibility == Visibility.Visible
                                    && rows.Type == DataGridRowType.Item
                                    && DataTableConverter.GetValue(rows.DataItem, "EQPTID").GetString() == txtSelectEquipment.Text
                                    && DataTableConverter.GetValue(rows.DataItem, "LOTID").GetString() == txtSelectLot.Text
                              select rows).FirstOrDefault();

                    if (dr != null)
                        idx = dr.Index;

                    if (idx > -1)
                    {
                        DataTableConverter.SetValue(dgInputHistory.Rows[idx].DataItem, "CHK", true);
                        dgInputHistory.SelectedIndex = idx;
                        GetInputHistoryDetail(dgInputHistory.Rows[idx].DataItem);
                    }
                }
            }
        }

        private void dgInputHistoryChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if (rb.IsChecked != null && ((bool)rb.IsChecked && (((DataRowView)rb.DataContext).Row["CHK"].ToString().Equals("0") || ((DataRowView)rb.DataContext).Row["CHK"].ToString().ToUpper().Equals("FALSE"))))
            {
                int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                }

                //row 색 바꾸기
                dgInputHistory.SelectedIndex = idx;
                GetInputHistoryDetail(dgInputHistory.Rows[idx].DataItem);
            }
        }

        private void Splitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            GridSplitter splitter = (GridSplitter)sender;

            try
            {
                C1DataGrid dataGrid = dgInputHistory;
                double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);

                if (gridInputHistory.ColumnDefinitions[0].Width.Value > sumWidth)
                {
                    gridInputHistory.ColumnDefinitions[0].Width = new GridLength(sumWidth + splitter.ActualWidth);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnHistoryDetailSearch_Click(object sender, RoutedEventArgs e)
        {
            int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgInputHistory, "CHK");

            if (rowIndex < 0) return;
            GetInputHistoryDetail(dgInputHistory.Rows[rowIndex].DataItem);
        }

        private void dgInputHistory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg == null || dg.CurrentCell == null) return;
            if (dg.CurrentCell.Row.IsMouseOver == false) return;
            if (dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns["CHK"].Index).Presenter == null) return;
            RadioButton rb = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as RadioButton;

            if (rb?.DataContext == null) return;
            SetProductLotHistorySelect(rb);
        }

        #endregion

        #region Mehod

        private void SetProductLotHistorySelect(RadioButton rb)
        {
            int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

            for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
            {
                DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
            }
            dgInputHistory.SelectedIndex = idx;
            GetInputHistoryDetail(dgInputHistory.Rows[idx].DataItem);
        }

        private void SetApplyPermissions()
        {
            // 추가작성 필요~~~~~~~~~
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        public void SelectProductList(string processLotId = null, string sFromMode = null)
        {
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            //GetProductList(processLotId, sFromMode);
            //GetInputHistory();

            if (string.Equals(ProcessCode, Process.ZTZ))
            {
                GetProductList(processLotId, sFromMode);
                //GetProductListZtz(null);
            }
            else
            {
                GetProductList(processLotId, sFromMode);
                GetInputHistory();
            }            
        }


        private void SetWipColorLegendCombo()
        {
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            cboColor.Items.Clear();
            C1ComboBoxItem cbItemTiTle = new C1ComboBoxItem { Content = ObjectDic.Instance.GetObjectName("범례") };
            cboColor.Items.Add(cbItemTiTle);

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["CMCDTYPE"] = "WIP_COLOR_LEGEND";
            newRow["PROCID"] = ProcessCode;

            inTable.Rows.Add(newRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_COLOR_LEGEND_CBO", "RQSTDT", "RSLTDT", inTable);

            foreach (DataRow row in dtResult.Rows)
            {
                if (row["COLOR_BACK"].ToString().IsNullOrEmpty() || row["COLOR_FORE"].ToString().IsNullOrEmpty())
                {
                    continue;
                }

                C1ComboBoxItem cbItem = new C1ComboBoxItem
                {
                    Content = row["NAME"].ToString(),
                    Background = new BrushConverter().ConvertFromString(row["COLOR_BACK"].ToString()) as SolidColorBrush,
                    Foreground = new BrushConverter().ConvertFromString(row["COLOR_FORE"].ToString()) as SolidColorBrush
                };
                cboColor.Items.Add(cbItem);
            }
            cboColor.SelectedIndex = 0;

            _dwipColorLegnd = dtResult;
        }

        private void GetProductList(string processLotId, string sFromMode)
        {
            string bizRuleName;

            if (string.Equals(ProcessCode, Process.WINDING))
            {
                bizRuleName = "DA_PRD_SEL_PRODUCTLOT_WN_DRB";
            }
            else if (string.Equals(ProcessCode, Process.ASSEMBLY))
            {
                bizRuleName = "DA_PRD_SEL_PRODUCTLOT_AS_DRB";
            }
            else if (string.Equals(ProcessCode, Process.ZZS))
            {
                bizRuleName = "DA_PRD_SEL_PRODUCTLOT_ZZS_DRB";
            }
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            else if (string.Equals(ProcessCode, Process.ZTZ))
            {
                bizRuleName = "DA_PRD_SEL_PRODUCTLOT_ZTZ_DRB";
            }
            else
            {
                bizRuleName = "DA_PRD_SEL_PRODUCTLOT_WS_DRB";
            }

            string previousLot = string.Empty;
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            if (string.Equals(ProcessCode, Process.ZTZ))
            {
                if (CommonVerify.HasDataGridRow(dgProductLotZtz))
                {
                    int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgProductLotZtz, "CHK");
                    if (rowIndex >= 0)
                    {
                        previousLot = Util.NVC(DataTableConverter.GetValue(dgProductLotZtz.Rows[rowIndex].DataItem, "LOTID"));
                        if (string.IsNullOrEmpty(previousLot))
                        {
                            previousLot = Util.NVC(DataTableConverter.GetValue(dgProductLotZtz.Rows[rowIndex].DataItem, "PR_LOTID"));
                        }
                    }
                }
            }
            else
            {
                if (CommonVerify.HasDataGridRow(dgProductLot))
                {
                    int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    if (rowIndex >= 0)
                    {
                        previousLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "LOTID"));
                        if (string.IsNullOrEmpty(previousLot))
                        {
                            previousLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "PR_LOTID"));
                        }
                    }
                }
            }

            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = ProcessCode;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["EQPTID"] = EquipmentCode;

                if (sFromMode.Equals("PILOT"))
                    newRow["LOTTYPE"] = 'X';

                inTable.Rows.Add(newRow);
                //2023.05.22 김대현
                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        DataTable dtcompare = bizResult.Copy();

                        foreach (DataRow row in bizResult.Rows)
                        {
                            if (row["LOTID"].GetString() == string.Empty) continue;

                            var query = (from t in dtcompare.AsEnumerable()
                                         where t.Field<string>("PR_LOTID") != t.Field<string>("LOTID")
                                               && t.Field<string>("PR_LOTID") == row["LOTID"].GetString()
                                         select t).ToList();

                            if (!query.Any())
                                row["TOGGLEKEY"] = string.Empty;
                        }
                        bizResult.AcceptChanges();

                        //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
                        //Util.GridSetData(dgProductLot, bizResult, null);
                        if (string.Equals(ProcessCode, Process.ZTZ))
                        {
                            Util.GridSetData(dgProductLotZtz, bizResult, null, true);
                        }
                        else
                        {
                            Util.GridSetData(dgProductLot, bizResult, null);
                        }

                        IsSearchResult = true;

                        if (!string.IsNullOrEmpty(processLotId))
                        {
                            SetGridSelectRow(processLotId);
                        }
                        else
                        {
                            if (bizResult.Rows.Count < 1)
                            {
                                HiddenLoadingIndicator();
                                return;
                            }
                        }

                        HiddenLoadingIndicator();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
                #region Old
                //DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                //if (CommonVerify.HasTableRow(dtResult))
                //{
                //    DataTable dtcompare = dtResult.Copy();
                //    foreach (DataRow row in dtResult.Rows)
                //    {
                //        if (row["LOTID"].GetString() == string.Empty) continue;

                //        var query = (from t in dtcompare.AsEnumerable()
                //                     where t.Field<string>("PR_LOTID") != t.Field<string>("LOTID")
                //                           && t.Field<string>("PR_LOTID") == row["LOTID"].GetString()
                //                     select t).ToList();

                //        if (!query.Any())
                //            row["TOGGLEKEY"] = string.Empty;
                //    }
                //    dtResult.AcceptChanges();

                //    Util.GridSetData(dgProductLot, dtResult, null, true);                

                //    IsSearchResult = true;

                //    if (!string.IsNullOrEmpty(processLotId))
                //    {
                //        SetGridSelectRow(processLotId);
                //    }
                //    else
                //    {
                //        if (dtResult.Rows.Count < 1 || string.IsNullOrEmpty(previousLot))
                //        {
                //            return;
                //        }
                //    }
                //}
                //else
                //{
                //    Util.gridClear(dgProductLot);
                //}
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetInputHistory()
        {
            Util.gridClear(dgInputHistory);
            Util.gridClear(dgInputHistoryDetail);

            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_WIP_PRODLOT_DRB";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["PROCID"] = ProcessCode;
                newRow["EQPTID"] = EquipmentCode;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgInputHistory, bizResult, null, true);

                        int rowIndex = _util.GetDataGridFirstRowIndexByColumnValue(dgInputHistory, "WIPSTAT", "PROC");
                        if (rowIndex > -1)
                        {
                            DataTableConverter.SetValue(dgInputHistory.Rows[rowIndex].DataItem, "CHK", true);
                            dgInputHistory.SelectedIndex = rowIndex;
                            GetInputHistoryDetail(dgInputHistory.Rows[rowIndex].DataItem);
                        }
                        else
                        {
                            if (CommonVerify.HasTableRow(bizResult))
                            {
                                rowIndex = 0;
                                DataTableConverter.SetValue(dgInputHistory.Rows[rowIndex].DataItem, "CHK", true);
                                dgInputHistory.SelectedIndex = rowIndex;
                                GetInputHistoryDetail(dgInputHistory.Rows[rowIndex].DataItem);
                            }
                        }
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
                Util.MessageException(ex);
            }
        }

        private void GetInputHistoryDetail(object obj)
        {
            DataRowView rowview = obj as DataRowView;
            if (rowview == null) return;

            try
            {
                ShowLoadingIndicator();

                string bizRuleName;
                if (string.Equals(ProcessCode, Process.ASSEMBLY))
                {
                    //bizRuleName = "DA_PRD_SEL_INPUT_MATERIAL_AS";
                    bizRuleName = "DA_PRD_SEL_INPUT_MATERIAL_AS_DRB";
                }
                else if (string.Equals(ProcessCode, Process.WASHING))
                {
                    bizRuleName = "DA_PRD_SEL_INPUT_MTRL_HIST_WS";
                }
                else
                {
                    bizRuleName = "DA_PRD_SEL_INPUT_MTRL_HIST";
                }

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("PROD_WIPSEQ", typeof(int));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inTable.Columns.Add("MTRLTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = rowview["EQPTID"];
                newRow["PROD_LOTID"] = rowview["LOTID"];
                newRow["PROD_WIPSEQ"] = rowview["WIPSEQ"];
                newRow["INPUT_LOTID"] = null;
                newRow["EQPT_MOUNT_PSTN_ID"] = null;
                //if (string.Equals(ProcessCode, Process.ASSEMBLY))
                //    newRow["MTRLTYPE"] = "MTRL";

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.GridSetData(dgInputHistoryDetail, bizResult, null, true);
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
                Util.MessageException(ex);
            }
        }


        private void SetGridSelectRow(string processLotId)
        {
            //////////////////////////////////////////////////// 라디오 버튼 체크
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            //if (!string.IsNullOrEmpty(processLotId))
            //{
            //    int idx = -1;
            //    for (int row = 0; row < dgProductLot.Rows.Count; row++)
            //    {
            //        if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[row].DataItem, "LOTID")) == processLotId)
            //        {
            //            idx = row;
            //            break;
            //        }
            //    }

            //    if (idx < 0)
            //    {
            //        txtSelectLot.Text = string.Empty;
            //        return;
            //    }

            //    DataTableConverter.SetValue(dgProductLot.Rows[idx].DataItem, "CHK", true);
            //}
            if (string.Equals(ProcessCode, Process.ZTZ))
            {
                if (!string.IsNullOrEmpty(processLotId))
                {
                    int idx = -1;
                    for (int row = 0; row < dgProductLotZtz.Rows.Count; row++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgProductLotZtz.Rows[row].DataItem, "LOTID")) == processLotId)
                        {
                            idx = row;
                            break;
                        }
                    }

                    if (idx < 0)
                    {
                        txtSelectLot.Text = string.Empty;
                        return;
                    }

                    DataTableConverter.SetValue(dgProductLotZtz.Rows[idx].DataItem, "CHK", true);
                }

            }
            else
            {
                if (!string.IsNullOrEmpty(processLotId))
                {
                    int idx = -1;
                    for (int row = 0; row < dgProductLot.Rows.Count; row++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[row].DataItem, "LOTID")) == processLotId)
                        {
                            idx = row;
                            break;
                        }
                    }

                    if (idx < 0)
                    {
                        txtSelectLot.Text = string.Empty;
                        return;
                    }

                    DataTableConverter.SetValue(dgProductLot.Rows[idx].DataItem, "CHK", true);
                }
            }
        }

        protected virtual void SetUserControlProductLotSelect(RadioButton rb)
        {
            if (UcParentControl == null) return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("SetProductLotSelect");

                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    object[] parameterArrys = new object[parameters.Length];
                    parameterArrys[0] = rb;

                    methodInfo.Invoke(UcParentControl, parameterArrys);
                }
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

        private void dgInputHistoryDetail_OnCurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            DataGridCurrentCellChanged(sender, e);
        }

        private void DataGridCurrentCellChanged(object sender, DataGridCellEventArgs e)
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
            
        }

        private void btnInHalfProductInPutQty_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInHalfProductInPutQty()) return;

            DataRow selectedProductRow = GetSelectProductRow();
            if (selectedProductRow == null) return;
            
            int idx = _util.GetDataGridCheckFirstRowIndex(dgInputHistoryDetail, "CHK");
            CMM_ASSY_MODIFY_INPUT_LOT_QTY popModifyQty = new CMM_ASSY_MODIFY_INPUT_LOT_QTY { FrameOperation = FrameOperation };
            object[] parameters = new object[5];
            parameters[0] = EquipmentCode;
            parameters[1] = DataTableConverter.GetValue(dgInputHistory.Rows[dgInputHistory.SelectedIndex].DataItem, "LOTID").GetString();
            //parameters[1] = selectedProductRow["LOTID"].GetString();
            if(string.Equals(ProcessCode,Process.ASSEMBLY))
                parameters[2] = DataTableConverter.GetValue(dgInputHistoryDetail.Rows[idx].DataItem, "LOTID").GetString();
            else
                parameters[2] = DataTableConverter.GetValue(dgInputHistoryDetail.Rows[idx].DataItem, "INPUT_LOTID").GetString();
            parameters[3] = DataTableConverter.GetValue(dgInputHistoryDetail.Rows[idx].DataItem, "INPUT_SEQNO").GetString();
            parameters[4] = DataTableConverter.GetValue(dgInputHistoryDetail.Rows[idx].DataItem, "INPUT_QTY").GetString();
            C1WindowExtension.SetParameters(popModifyQty, parameters);
            popModifyQty.Closed += popModifyQty_Closed;

            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    grid.Children.Add(popModifyQty);
                    popModifyQty.BringToFront();
                    break;
                }
            }
        }

        private void popModifyQty_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_MODIFY_INPUT_LOT_QTY pop = sender as CMM_ASSY_MODIFY_INPUT_LOT_QTY;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgInputHistory, "CHK");
                if (rowIndex > -1)
                {
                    DataTableConverter.SetValue(dgInputHistory.Rows[rowIndex].DataItem, "CHK", true);
                    dgInputHistory.SelectedIndex = rowIndex;
                    GetInputHistoryDetail(dgInputHistory.Rows[rowIndex].DataItem);
                }
                
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

        private bool ValidationInHalfProductInPutQty()
        {
            int iRow = _util.GetDataGridCheckFirstRowIndex(dgInputHistoryDetail, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgInputHistoryDetail.Rows[iRow].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private DataRow GetSelectProductRow()
        {
            DataRow row = null;
            try
            {
                row = _util.GetDataGridFirstRowBycheck(dgInputHistory, "CHK");
                return row;
            }
            catch (Exception)
            {
                return row;
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
                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    object[] parameterArrys = new object[parameters.Length];

                    for (int i = 0; i < parameterArrys.Length; i++)
                    {
                        parameterArrys[i] = null;
                    }

                    methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidationInHalfProductInPutCancel()
        {
            int iRow = _util.GetDataGridCheckFirstRowIndex(dgInputHistoryDetail, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgInputHistoryDetail.Rows[iRow].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private void InputHalfProductCancel()
        {
            try
            {
                ShowLoadingIndicator();
                string bizRuleName = string.Equals(ProcessCode, Process.WASHING) ? "BR_PRD_REG_CANCEL_TERMINATE_LOT_WS" : "BR_PRD_REG_CANCEL_TERMINATE_LOT";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_CANCEL_BASKET_CL();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inMtrlTable = indataSet.Tables["INLOT"];

                for (int i = 0; i < dgInputHistoryDetail.Rows.Count - dgInputHistoryDetail.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgInputHistoryDetail, "CHK", i)) continue;

                    newRow = inMtrlTable.NewRow();
                    newRow["INPUT_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgInputHistoryDetail.Rows[i].DataItem, "INPUT_SEQNO")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInputHistoryDetail.Rows[i].DataItem, "INPUT_SEQNO")));
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputHistoryDetail.Rows[i].DataItem, "INPUT_LOTID"));
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgInputHistoryDetail.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInputHistoryDetail.Rows[i].DataItem, "INPUT_QTY")));

                    inMtrlTable.Rows.Add(newRow);
                }
                //string xml = indataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //GetHalfProductList();
                        //GetProductLot();
                        int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgInputHistory, "CHK");
                        if (rowIndex > -1)
                        {
                            DataTableConverter.SetValue(dgInputHistory.Rows[rowIndex].DataItem, "CHK", true);
                            dgInputHistory.SelectedIndex = rowIndex;
                            GetInputHistoryDetail(dgInputHistory.Rows[rowIndex].DataItem);
                        }
                    
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
        }

        private void InputHalfProductCancelAssembly()
        {
            try
            {
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;
                ShowLoadingIndicator();
                //const string bizRuleName = "BR_PRD_REG_CANCEL_INPUT_LOT_AS";
                string bizRuleName = "BR_PRD_REG_CANCEL_INPUT_LOT_AS";

                DataSet inDataSet = _bizDataSet.GetBR_PRD_DEL_INPUT_LOT_AS();
                DataTable inDataTable = inDataSet.Tables["INDATA"];
                DataRow row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = EquipmentCode;
                row["USERID"] = LoginInfo.USERID;
                row["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();
                inDataTable.Rows.Add(row);

                DataTable inInputTable = inDataSet.Tables["INLOT"];
                for (int i = 0; i < dgInputHistoryDetail.GetRowCount(); i++)
                {
                    if (_util.GetDataGridCheckValue(dgInputHistoryDetail, "CHK", i))
                    {
                        row = inInputTable.NewRow();
                        row["INPUT_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgInputHistoryDetail.Rows[i].DataItem, "INPUT_SEQNO"));
                        row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputHistoryDetail.Rows[i].DataItem, "LOTID"));
                        row["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgInputHistoryDetail.Rows[i].DataItem, "INPUT_QTY")).GetDecimal();
                        row["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputHistoryDetail.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                        inInputTable.Rows.Add(row);
                    }
                }
                //string xmlText = inDataSet.GetXml();
                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    //GetHalfProductList();
                    //GetProductLot();
                    int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgInputHistory, "CHK");
                    if (rowIndex > -1)
                    {
                        DataTableConverter.SetValue(dgInputHistory.Rows[rowIndex].DataItem, "CHK", true);
                        dgInputHistory.SelectedIndex = rowIndex;
                        GetInputHistoryDetail(dgInputHistory.Rows[rowIndex].DataItem);
                    }

                    Util.MessageInfo("SFU1275");

                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnInHalfProductInPutCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInHalfProductInPutCancel()) return;

            Util.MessageConfirm("SFU1988", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (string.Equals(ProcessCode, Process.ASSEMBLY))
                    {
                        InputHalfProductCancelAssembly();
                    }
                    else
                    {
                        InputHalfProductCancel();
                    }
                }
            });
        }

        //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응_Start
        private void dgProductLotZtz_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    string EqptID = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTID"));
                    string Lot = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTID"));
                    string Wipstat = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT"));
                    string WipHold = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD"));
                    string QmsHold = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "QMS_LOT_INSP_JUDG_HOLD_FLAG"));


                    dg.Columns["LOTID_LARGE"].Visibility = Visibility.Collapsed;
                    dg.Columns["CUT"].Visibility = Visibility.Collapsed;
                    dg.Columns["OUT_CSTID"].Visibility = Visibility.Collapsed;
                    dg.Columns["COATER_PRJT_NAME"].Visibility = Visibility.Collapsed;

                    dg.Columns["COATERVER"].Visibility = Visibility.Collapsed;
                    dg.Columns["CSTID"].Visibility = Visibility.Collapsed;
                    dg.Columns["INPUT_BACK_QTY"].Visibility = Visibility.Collapsed;
                    dg.Columns["INPUT_TOP_QTY"].Visibility = Visibility.Collapsed;
                    dg.Columns["WIPQTY"].Visibility = Visibility.Collapsed;
                    dg.Columns["ROLLPRESS_SEQNO"].Visibility = Visibility.Collapsed;
                    dg.Columns["LANE_QTY"].Visibility = Visibility.Collapsed;


                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Column.Name == "WIPSTAT_IMAGES")
                    {
                        e.Cell.Presenter.FontSize = 16;

                        e.Cell.Presenter.Foreground = Wipstat == "WAIT" ? new SolidColorBrush(Colors.Yellow) : new SolidColorBrush(Colors.Green);

                        if (WipHold == "Y")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        }

                        e.Cell.Presenter.Cursor = Cursors.Arrow;
                    }

                    else if (e.Cell.Column.Name == "LOTID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                    else if (e.Cell.Column.Name == "PR_LOTID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Cursor = Cursors.Arrow;
                    }
                    else if (e.Cell.Column.Name == "EQPTID")
                    {
                        if (!string.IsNullOrEmpty(txtSelectEquipment.Text) && string.IsNullOrEmpty(txtSelectLot.Text))
                        {
                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTID").GetString() == txtSelectEquipment.Text)
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            }
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontSize = 12;
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                        e.Cell.Presenter.Cursor = Cursors.Arrow;
                    }

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "QA_INSP_TRGT_FLAG")).Equals("Y"))
                    {
                        if (e.Cell.Column.Name == "CHK" || e.Cell.Column.Name == "WIPSTAT_IMAGES") return;

                        if (e.Cell.Column.Name == "EQPTID")
                        {
                            if (!string.IsNullOrEmpty(txtSelectEquipment.Text) && string.IsNullOrEmpty(txtSelectLot.Text))
                            {
                                if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTID").GetString() == txtSelectEquipment.Text) return;
                            }
                        }

                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                    }
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTTYPE")).Equals("X"))
                    {
                        if (e.Cell.Column.Name == "CHK" || e.Cell.Column.Name == "WIPSTAT_IMAGES") return;

                        if (e.Cell.Column.Name == "EQPTID")
                        {
                            if (!string.IsNullOrEmpty(txtSelectEquipment.Text) && string.IsNullOrEmpty(txtSelectLot.Text))
                            {
                                if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTID").GetString() == txtSelectEquipment.Text) return;
                            }
                        }

                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF2424"));
                    }

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTTYPE")).Equals("L"))
                    {
                        if (e.Cell.Column.Name == "CHK" || e.Cell.Column.Name == "WIPSTAT_IMAGES") return;

                        if (e.Cell.Column.Name == "EQPTID")
                        {
                            if (!string.IsNullOrEmpty(txtSelectEquipment.Text) && string.IsNullOrEmpty(txtSelectLot.Text))
                            {
                                if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTID").GetString() == txtSelectEquipment.Text) return;
                            }
                        }

                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF9808"));
                    }
                }
            }));

        }

        private void dgProductLotZtz_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                }));
            }
        }

        private void dgProductLotZtz_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgProductLotZtz.GetCellFromPoint(pnt);

            if (cell == null) return;

            // 선택한 셀의 Row 위치
            int rowIdx = cell.Row.Index;
            DataRowView dv = dgProductLotZtz.Rows[rowIdx].DataItem as DataRowView;

            if (dv == null) return;

            if (string.IsNullOrWhiteSpace(dv["LOTID"].ToString()))
                _clickLotID = dv["PR_LOTID"].ToString();
            else
                _clickLotID = dv["LOTID"].ToString();
        }

        private void dgProductLotZtz_LoadedRowDetailsPresenter(object sender, C1.WPF.DataGrid.DataGridRowDetailsEventArgs e)
        {
            
        }

        private void dgChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                SetUserControlProductLotSelect(rb);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkWait_Checked(object sender, RoutedEventArgs e)
        {
            SetControlVisibility();
            GetProductListZtz(null);
        }

        private void GetProductListZtz(string processLotId)
        {
            string previousLot = string.Empty;

            if (CommonVerify.HasDataGridRow(dgProductLotZtz))
            {
                int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgProductLotZtz, "CHK");
                if (rowIndex >= 0)
                {
                    previousLot = Util.NVC(DataTableConverter.GetValue(dgProductLotZtz.Rows[rowIndex].DataItem, "LOTID"));
                    if (string.IsNullOrEmpty(previousLot))
                    {
                        previousLot = Util.NVC(DataTableConverter.GetValue(dgProductLotZtz.Rows[rowIndex].DataItem, "PR_LOTID"));
                    }
                }
            }

            string wipState = chkWait != null && chkWait.IsChecked == true ? "WAIT,PROC,EQPT_END" : "PROC,EQPT_END";

            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = ProcessCode;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["WIPSTAT"] = wipState;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_ZTZ_L_DRB", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgProductLotZtz, bizResult, null, true);
                        IsSearchResult = true;

                        if (!string.IsNullOrEmpty(processLotId))
                        {
                            SetGridSelectRow(processLotId);
                        }
                        else
                        {
                            if (bizResult.Rows.Count < 1 || string.IsNullOrEmpty(previousLot))
                            {
                                HiddenLoadingIndicator();
                                return;
                            }

                        }
                        HiddenLoadingIndicator();
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

        private void dgProductLotZtzChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (IsProductLotChoiceRadioButtonEnable == false) return;
            try
            {
                if (sender == null) return;

                RadioButton rb = sender as RadioButton;
                if (rb != null && rb.DataContext == null) return;

                SetUserControlProductLotSelect(rb);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응_End

        #endregion
    }
}
