/*************************************************************************************
 Created Date : 2020.10.16
      Creator : 신광희
   Decription : 조립 공정진척(CNB 2동) - 생산 LOT List UserControl
--------------------------------------------------------------------------------------
 [Change History]
 2021.08.12  김지은 : 시생산샘플 추가
 2021.10.15  김지은 : 노칭 대기 무지부방향 추가
 2023.03.20  강성묵 : 슬리팅 레인 번호 추가
 2023.06.29  주동석 : NND Unwinder 무지부/권취 방향 저장
 2025.03.05  류경흠 : UI Design 부분 CP Version 컬럼 추가(CatchUp)
 2025.05.20  천진수 : ESHG 증설 조립공정진척 DNC공정추가 
 2025.06.15  권준서 : 동별 공통코드로 NND QA대상 랏 구분 컬럼 추가
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
using LGC.GMES.MES.Common;
using C1.WPF;

namespace LGC.GMES.MES.ASSY005.Controls
{
    /// <summary>
    /// UcAssemblyProductLot.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcAssemblyProductLot : UserControl
    {
        #region Declaration

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public UserControl UcParentControl;

        public string EquipmentSegmentCode { get; set; }
        public string ProcessCode { get; set; }
        public string ProcessName { get; set; }
        public string EquipmentCode { get; set; }
        public string LdrLotIdentBasCode { get; set; }
        public string UnldrLotIdentBasCode { get; set; }

        public bool IsProductLotChoiceRadioButtonEnable = true;

        public bool IsSearchResult;

        public C1DataGrid DgProductLot { get; set; }


        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();
        private string _selectedLotId = string.Empty;

        DataTable _dwipColorLegnd;

        public UcAssemblyProductLot()
        {
            InitializeComponent();
            InitializeControls();

            //SetControl();
            SetButtons();
        }

        #endregion

        #region Initialize

        public void InitializeControls()
        {
            Util.gridClear(DgProductLot);

        }

        private void SetControl()
        {
            DgProductLot = ProcessCode == Process.NOTCHING ? dgProductLot : dgProductLotCommon;
        }

        private void SetButtons()
        {
        }

        private void SetComboBox()
        {
            SetWipColorLegendCombo();
        }

        private void CanShowQaTarget()
        {
            try
            {
                string sBizRuleName = "DA_BAS_SEL_AREA_COM_CODE_USE";

                DataTable dtInTable = new DataTable("INDATA");
                dtInTable.Columns.Add("LANGID", typeof(string));
                dtInTable.Columns.Add("AREAID", typeof(string));
                dtInTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtInTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = dtInTable.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "NND_QA_TARGET_MARK";
                dr["USE_FLAG"] = "Y";
                dtInTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizRuleName, "INDATA", "OUTDATA", dtInTable);

                QA_Target.Visibility = dtResult.Rows.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetControlVisibility()
        {
            SetControl();
            SetComboBox();

            tbWorkHalfSlittingSide.Visibility = Visibility.Collapsed;
            txtWorkHalfSlittingSide.Visibility = Visibility.Collapsed;

            // 강성묵 : 20230404 레인번호 추가
            slittingLaneNo.Visibility = Visibility.Collapsed;
            tbSlittingLaneNo.Visibility = Visibility.Collapsed;
            txtSlittingLaneNo.Visibility = Visibility.Collapsed;
            //txtSlittingLaneNo.Text = "";

            dgProductLotCommon.Columns["RE_INPUT_QTY"].Visibility = Visibility.Collapsed;
            dgProductLotCommon.Columns["FCS_HOLD_FLAG_NAME"].Visibility = Visibility.Collapsed;
            dgProductLotCommon.Columns["GOODQTY"].Visibility = Visibility.Collapsed;
            dgProductLotCommon.Columns["WIPQTY"].Visibility = Visibility.Visible;

            dgInputHistoryDetail.Columns["CUT_QTY"].Visibility = Visibility.Visible;
            dgInputHistoryDetail.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Visible;
            dgInputHistoryDetail.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
            dgInputHistoryDetail.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Visible;

            dgInputHistoryDetail.TopRows[0].Visibility = Visibility.Visible;
            dgInputHistoryDetail.Columns["CURR_PROC_LOSS_QTY"].Header = new List<string>(new string[] { ObjectDic.Instance.GetObjectName("LOSS"), ObjectDic.Instance.GetObjectName("자공정") });

            if (ProcessCode == Process.NOTCHING)
            {
                tbWorkHalfSlittingSide.Visibility = Visibility.Visible;
                txtWorkHalfSlittingSide.Visibility = Visibility.Visible;
                dgProductLot.Visibility = Visibility.Visible;
                dgProductLotCommon.Visibility = Visibility.Collapsed;
                TabInputHistory.Visibility = Visibility.Collapsed;
                CanShowQaTarget();

                if (UnldrLotIdentBasCode == "RF_ID")
                    dgProductLot.Columns["CSTID"].Visibility = Visibility.Visible;
                else
                    dgProductLot.Columns["CSTID"].Visibility = Visibility.Collapsed;

                if (LdrLotIdentBasCode == "RF_ID")
                    dgProductLot.Columns["PR_CSTID"].Visibility = Visibility.Visible;
                else
                    dgProductLot.Columns["PR_CSTID"].Visibility = Visibility.Collapsed;

                if (chkWait != null && chkWait.IsChecked == true)
                    dgProductLot.Columns["SLIT_SIDE_WINDING_DIRCTN"].Visibility = Visibility.Visible;
                else
                    dgProductLot.Columns["SLIT_SIDE_WINDING_DIRCTN"].Visibility = Visibility.Collapsed;
            }
            else
            {
                dgProductLot.Visibility = Visibility.Collapsed;
                dgProductLotCommon.Visibility = Visibility.Visible;
                TabInputHistory.Visibility = Visibility.Visible;
                QA_Target.Visibility = Visibility.Collapsed;
                TabProductLot.IsSelected = true;

                if (ProcessCode == Process.LAMINATION || ProcessCode == Process.AZS_ECUTTER || ProcessCode == Process.DNC)  // 250428 ESHG DNC공정추가 
                {
                    dgInputHistoryDetail.Columns["CUT_QTY"].Visibility = Visibility.Collapsed;
                    dgInputHistoryDetail.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                }
                else if (ProcessCode.Equals(Process.STACKING_FOLDING) || ProcessCode == Process.AZS_STACKING)
                {
                    dgInputHistoryDetail.Columns["CUT_QTY"].Visibility = Visibility.Collapsed;
                    dgInputHistoryDetail.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Collapsed;
                    dgInputHistoryDetail.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;

                    if (dgInputHistoryDetail.TopRows.Count > 1)
                    {
                        dgInputHistoryDetail.TopRows[0].Visibility = Visibility.Collapsed;
                        dgInputHistoryDetail.Columns["CURR_PROC_LOSS_QTY"].Header = new List<string>(new string[] { ObjectDic.Instance.GetObjectName("LOSS"), ObjectDic.Instance.GetObjectName("자공정LOSS") });
                    }
                }

                else if (ProcessCode == Process.PACKAGING)
                {
                    dgInputHistoryDetail.Columns["CUT_QTY"].Visibility = Visibility.Collapsed;
                    dgInputHistoryDetail.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Collapsed;
                    dgInputHistoryDetail.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;

                    if (dgInputHistoryDetail.TopRows.Count > 1)
                    {
                        dgInputHistoryDetail.TopRows[0].Visibility = Visibility.Collapsed;
                    }

                    dgProductLotCommon.Columns["RE_INPUT_QTY"].Visibility = Visibility.Visible;
                    dgProductLotCommon.Columns["FCS_HOLD_FLAG_NAME"].Visibility = Visibility.Visible;
                }
            }

            // 강성묵 : 20230404 레인번호 추가
            try
            {
                string sBizRuleName = "DA_BAS_SEL_AREA_COM_CODE_USE";

                DataTable dtInTable = new DataTable("INDATA");
                dtInTable.Columns.Add("LANGID", typeof(string));
                dtInTable.Columns.Add("AREAID", typeof(string));
                dtInTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtInTable.Columns.Add("COM_CODE", typeof(string));
                dtInTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = dtInTable.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "LOGIS_COND_BY_PROC";
                dr["COM_CODE"] = ProcessCode;
                dr["USE_FLAG"] = "Y";
                dtInTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizRuleName, "INDATA", "OUTDATA", dtInTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    //if (Util.NVC(dtResult.Rows[0]["ATTR1"]) == "Y")
                    //{
                    //    dgProductLot.Columns["SLIT_SIDE_WINDING_DIRCTN"].Visibility = Visibility.Visible;
                    //    tbWorkHalfSlittingSide.Visibility = Visibility.Visible;
                    //    txtWorkHalfSlittingSide.Visibility = Visibility.Visible;
                    //}
                    //else
                    //{
                    //    dgProductLot.Columns["SLIT_SIDE_WINDING_DIRCTN"].Visibility = Visibility.Collapsed;
                    //    tbWorkHalfSlittingSide.Visibility = Visibility.Collapsed;
                    //    txtWorkHalfSlittingSide.Visibility = Visibility.Collapsed;
                    //}

                    if (Util.NVC(dtResult.Rows[0]["ATTR4"]) == "Y")
                    {
                        if (ProcessCode == Process.NOTCHING)
                        {
                            dgProductLot.Columns["SLIT_SIDE_WINDING_DIRCTN"].Visibility = Visibility.Collapsed;
                            tbWorkHalfSlittingSide.Visibility = Visibility.Collapsed;
                            txtWorkHalfSlittingSide.Visibility = Visibility.Collapsed;

                            if (chkWait != null && chkWait.IsChecked == true)
                            {
                                slittingLaneNo.Visibility = Visibility.Visible;
                            }
                        }

                        tbSlittingLaneNo.Visibility = Visibility.Visible;
                        txtSlittingLaneNo.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        slittingLaneNo.Visibility = Visibility.Collapsed;
                        tbSlittingLaneNo.Visibility = Visibility.Collapsed;
                        txtSlittingLaneNo.Visibility = Visibility.Collapsed;
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
                    string qaHold = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "QMS_LOT_INSP_JUDG_HOLD_FLAG"));

                    if (e.Cell.Column.Name == "WIPSTAT_IMAGES")
                    {
                        e.Cell.Presenter.FontSize = 16;

                        e.Cell.Presenter.Foreground = wipstat == "WAIT" ? new SolidColorBrush(Colors.Yellow) : new SolidColorBrush(Colors.Green);

                        if (wipHold == "Y")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        }

                        //if (qaHold == "Y")
                        //{
                        //    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Orange);
                        //    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        //}

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
                        if (QA_Target.Visibility == Visibility.Visible)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                        }

                        if (e.Cell.Column.Name == "CHK" || e.Cell.Column.Name == "WIPSTAT_IMAGES") return;

                        if (e.Cell.Column.Name == "EQPTID")
                        {
                            if (!string.IsNullOrEmpty(txtSelectEquipment.Text) && string.IsNullOrEmpty(txtSelectLot.Text))
                            {
                                if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTID").GetString() == txtSelectEquipment.Text) return;
                            }
                        }
                    }

                    if (e.Cell.Column.Name == "QA_INSP_TRGT_FLAG" && DataTableConverter.GetValue(e.Cell.Row.DataItem, "QA_INSP_TRGT_FLAG").GetString().Equals("Y") && QA_Target.Visibility == Visibility.Visible)
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
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
                    //2021.08.12 : 시생산 샘플 설정 추가
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
                    {
                        idx = dr.Index;
                    }

                    if (idx > -1)
                    {
                        DataTableConverter.SetValue(dgInputHistory.Rows[idx].DataItem, "CHK", true);
                        dgInputHistory.SelectedIndex = idx;
                        GetInputHistoryDetail(dgInputHistory.Rows[idx].DataItem);
                    }
                }
            }

            /*
            string tabItem = ((C1TabItem)((ItemsControl)sender).Items.CurrentItem).Name.GetString();

            if (string.Equals(tabItem, "TabInputHistory"))
            {
                // Validation 체크 로직
                // 1. 투입이력 조회
                // 2. 설비, 생산 Lot 표시 및 진행중인 생산Lot 맨 위로 선택 되게 표시 및 이력 표시
                // 3. 생산 Lot 체크시 해당 이력 표시
            }
            */
        }

        private void dgProductLotCommon_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    string qaHold = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "QMS_LOT_INSP_JUDG_HOLD_FLAG"));

                    ////////////////////////////////////////////  default 
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

                        //if (qaHold == "Y")
                        //{
                        //    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Orange);
                        //    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        //}

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

                    //2021.08.12 : 시생산 샘플 설정 추가
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTTYPE")).Equals("L"))
                    {
                        if (e.Cell.Column.Name == "TOGGLEKEY" || e.Cell.Column.Name == "CHK" || e.Cell.Column.Name == "WIPSTAT_IMAGES") return;

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

        private void dgProductLotCommon_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgProductLotCommon_MergingCells(object sender, DataGridMergingCellsEventArgs e)
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

        private void dgProductLotCommon_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg == null) return;

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgProductLotCommon.GetCellFromPoint(pnt);

            if (cell == null) return;

            int rowIdx = cell.Row.Index;
            DataRowView drv = DgProductLot.Rows[rowIdx].DataItem as DataRowView;
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

        private void dgProductLotCommon_LoadingRow(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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

        private void chkWait_Checked(object sender, RoutedEventArgs e)
        {
            SetControlVisibility();
            GetProductListNotching(null);
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
            if (dg == null || dg.Rows.Count == 0) return;
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

        #region [외부 호출]
        public void SetApplyPermissions()
        {
            // 추가작성 필요~~~~~~~~~
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        public void SelectProductList(string processLotId = null)
        {
            //SetControl();
            /*
            if (ProcessCode == Process.NOTCHING)
                GetProductListNotching();
            else if (ProcessCode == Process.LAMINATION)
                GetProductListLamination();
            else if (ProcessCode == Process.STACKING_FOLDING)
                GetProductListStackingFolding();
            else if (ProcessCode == Process.PACKAGING)
                GetProductListPackaging();
            */

            if (ProcessCode == Process.NOTCHING)
            {
                GetProductListNotching(processLotId);
            }
            else
            {
                GetProductListCommon(processLotId);
                GetInputHistory();
            }

        }

        #endregion

        #region [BizCall]
        private void SetWipColorLegendCombo()
        {
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

        private void GetProductListNotching(string processLotId)
        {
            string previousLot = string.Empty;

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
                //newRow["WIPSTAT"] = "WAIT,PROC,EQPT_END";
                newRow["WIPSTAT"] = wipState;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_NT_L_DRB", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgProductLot, bizResult, null, true);
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

                            /*
                            int idx = _util.GetDataGridRowIndex(dgProductLot, "LOTID", previousLot);
                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgProductLot.Rows[idx].DataItem, "CHK", true);
                                dgProductLot.SelectedIndex = idx;
                            }
                            else
                            {
                                idx = _util.GetDataGridRowIndex(dgProductLot, "PR_LOTID", previousLot);
                                if (idx >= 0)
                                {
                                    DataTableConverter.SetValue(dgProductLot.Rows[idx].DataItem, "CHK", true);
                                    dgProductLot.SelectedIndex = idx;
                                }
                            }
                            */
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

        private void GetProductListCommon(string processLotId)
        {
            string previousLot = string.Empty;
            if (CommonVerify.HasDataGridRow(dgProductLotCommon))
            {
                int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgProductLotCommon, "CHK");
                if (rowIndex >= 0) previousLot = Util.NVC(DataTableConverter.GetValue(dgProductLotCommon.Rows[rowIndex].DataItem, "LOTID"));
            }

            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = ProcessCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["EQSGID"] = EquipmentSegmentCode;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_LM_L_DRB", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
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

                        Util.GridSetData(dgProductLotCommon, bizResult, null);
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
                            /*
                            int idx = _util.GetDataGridRowIndex(dgProductLotCommon, "LOTID", previousLot);
                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgProductLotCommon.Rows[idx].DataItem, "CHK", true);
                                dgProductLotCommon.SelectedIndex = idx;
                            }
                            */
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
                const string bizRuleName = "DA_PRD_SEL_INPUT_MTRL_HIST_L";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("PROD_WIPSEQ", typeof(int));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = rowview["EQPTID"];
                newRow["PROD_LOTID"] = rowview["LOTID"];
                newRow["PROD_WIPSEQ"] = rowview["WIPSEQ"]; //PROD_WIPSEQ.Equals("") ? 1 : Convert.ToDecimal(PROD_WIPSEQ);
                newRow["INPUT_LOTID"] = null;
                newRow["EQPT_MOUNT_PSTN_ID"] = null;
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


        #endregion

        #region [Func]

        private void SetGridSelectRow(string processLotId)
        {
            //////////////////////////////////////////////////// 라디오 버튼 체크
            if (!string.IsNullOrEmpty(processLotId))
            {
                int idx = -1;
                for (int row = 0; row < DgProductLot.Rows.Count; row++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[row].DataItem, "LOTID")) == processLotId)
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

                // DgProductLot.GetCell(idx, DgProductLot.Columns["CHK"].Index).Presenter null 인 경우가 발생?
                DataTableConverter.SetValue(DgProductLot.Rows[idx].DataItem, "CHK", true);

                //RadioButton rb = DgProductLot.GetCell(idx, DgProductLot.Columns["CHK"].Index).Presenter?.Content as RadioButton;
                //if (rb?.DataContext == null) return;
                //for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                //{
                //    DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                //}
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

        #endregion;


        #endregion
    }
}
