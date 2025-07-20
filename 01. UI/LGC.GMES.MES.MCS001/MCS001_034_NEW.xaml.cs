/*************************************************************************************
 Created Date : 2020.11.29
      Creator : 오화백
   Decription : Carrier 현황 조회(신규)
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.29  오화백 과장 : Initial Created. (기존 화면을 새로 신규로 메뉴를 만듬 (2020.10.01 수정 버전임    

**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_034.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_034_NEW : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private readonly Util _util = new Util();
        private string _selectedCarrierTypeCode;
        private string _selectedCarrierStateCode;
        private string _selectedCarrierProductCode;
        private string _selectedAreaCode;

        public MCS001_034_NEW()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> { btnSearch, btnSave };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeCombo();

            this.rdoDefect.IsChecked = true;
            this.rdoAbnormal.Visibility = Visibility.Collapsed;

            Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void cboCarrierType_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            // Carrier Prod 콤보박스
            SetCarrierProdCombo(cboCarrierProd, cboCarrierType);

            // Carrier Summary Grid 설정
            SetGridSummaryColumnChange();

            // Carrier 상세 정보 BOBBIN_ID 칼럼 명칭 설정
            SetGridDetailColumnText();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();

            if (TabItemCarrierInfo.IsSelected)
            {
                if (cboCarrierType.SelectedValue.ToString() == "PT")
                {
                    rdoAbnormal.Visibility = Visibility.Visible;
                }
                else
                {
                    rdoDefect.IsChecked = true;
                    rdoAbnormal.Visibility = Visibility.Collapsed;
                }

                SetDataGridCheckHeaderInitialize(dgCarrierDetail);

                if (!string.IsNullOrEmpty(txtCarrierId.Text))
                {
                    SelectCarrierDetailList(true, false);
                }
                else if (!string.IsNullOrEmpty(txtLotId.Text))
                {
                    SelectCarrierDetailList(false, true);
                }
                else
                {
                    if (!ValidationSearch()) return;

                    SelecCarrierSummary();
                }
            }
            else if (TabItemSkidHistory.IsSelected)
            {
                SelectCarrierStateList(dgSkidHistory);
            }
            else if (TabItemCarrierChange.IsSelected)
            {
                SetDataGridCheckHeaderInitialize(dgCarrierChange);
                SelectCarrierStateList(dgCarrierChange);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveCarrierState()) return;
            SaveCarrierState();
        }

        private void dgCarrierSummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //if (Convert.ToString(e.Cell.Column.Name) == "CNT")
                    //{
                    //    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CNT").GetInt() > 0)
                    //    {
                    //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    //        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    //    }
                    //    else
                    //    {
                    //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    //    }
                    //}
                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CSTPROD_NAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }

        private void dgCarrierSummary_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgCarrierSummary_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null) return;
                C1DataGrid dataGrid = sender as C1DataGrid;

                if (dataGrid == null || dataGrid.CurrentCell == null || dataGrid.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgCarrierSummary.GetCellFromPoint(pnt);

                if (cell == null) return;

                // 선택한 셀의 위치
                int rowIdx = cell.Row.Index;
                DataRowView drv = dataGrid.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                if (cell.Column.Name == "CSTPROD_NAME" || cell.Column.Name == "T_CNT")
                {
                    _selectedCarrierTypeCode = DataTableConverter.GetValue(drv, "CSTTYPE").GetString();
                    _selectedCarrierProductCode = null;
                    _selectedCarrierStateCode = null;

                    if (DataTableConverter.GetValue(drv, "CSTPROD_NAME").GetString() != ObjectDic.Instance.GetObjectName("합계"))
                    {
                        _selectedCarrierProductCode = DataTableConverter.GetValue(drv, "CSTPROD").GetString();
                    }
                }
                else
                {
                    if (cell.Column.Name == "U_CNT")
                    {
                        _selectedCarrierStateCode = "U";
                    }
                    else if (cell.Column.Name == "E_CNT")
                    {
                        _selectedCarrierStateCode = "E";
                    }
                    else if (cell.Column.Name == "ET_CNT")
                    {
                        _selectedCarrierStateCode = "T";
                    }
                    else if (cell.Column.Name == "D_CNT")
                    {
                        _selectedCarrierStateCode = "D";
                    }
                    else if (cell.Column.Name == "ABNORM_CNT")
                    {
                        _selectedCarrierStateCode = "AB";
                    }
                    else if (cell.Column.Name == "UN_CNT")
                    {
                        _selectedCarrierStateCode = "UN";
                    }
                    else
                        _selectedCarrierStateCode = null;

                    _selectedCarrierTypeCode = DataTableConverter.GetValue(drv, "CSTTYPE").GetString();
                    _selectedCarrierProductCode = string.IsNullOrEmpty(DataTableConverter.GetValue(drv, "CSTPROD").GetString()) ? null : DataTableConverter.GetValue(drv, "CSTPROD").GetString();
                }

                SelectCarrierDetailList(false, false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCarrierDetail_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {

        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = TabItemCarrierChange.IsSelected ? dgCarrierChange : dgCarrierDetail;

            int idx = _util.GetDataGridFirstRowIndexByCheck(dg, "CHK");

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (idx > -1)
                {
                    string selectedequipmentCode = DataTableConverter.GetValue(dg.Rows[idx].DataItem, "EQPT_CUR").GetString();
                    if (DataTableConverter.GetValue(row.DataItem, "EQPT_CUR").GetString() == selectedequipmentCode)
                    {
                        DataTableConverter.SetValue(row.DataItem, "CHK", 1);
                    }
                    else
                    {
                        DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                    }
                }
                else
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 1);
                }
            }
            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            //Util.DataGridCheckAllUnChecked(dgCarrierDetail);
            C1DataGrid dg = TabItemCarrierChange.IsSelected ? dgCarrierChange : dgCarrierDetail;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", 0);
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void txtCarrierId_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(txtCarrierId.Text))
            {
                cboAreaGroup.IsEnabled = true;
                cboCarrierType.IsEnabled = true;
                txtLotId.IsEnabled = true;
            }
            else
            {
                cboAreaGroup.IsEnabled = false;
                cboCarrierType.IsEnabled = false;
                txtLotId.IsEnabled = false;
            }
        }

        private void cboCarrierState_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //상태가 불량인 경우 불량사유 콤보박스, 비고 텍스트박스 활성화
            if (cboCarrierState.SelectedValue?.GetString() == "Y")
            {
                cboDefectReason.IsEnabled = true;
                cboDefectReason.SelectedIndex = 0;
                txtNote.IsReadOnly = false;
            }
            else
            {
                cboDefectReason.IsEnabled = false;
                cboDefectReason.SelectedIndex = -1;
                txtNote.IsReadOnly = true;
                txtNote.Text = string.Empty;
            }
        }

        private void btnSaveCarrierChange_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveCarrierChange()) return;
            SaveCarrierChange();
        }

        private void cboAreaGroup_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetAreaCombo(cboArea);
        }

        private void btnAbnormalSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveAbnormalState()) return;
            SaveAbnormal();
        }

        private void btnScrapSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveScrap()) return;
            SaveScrap();
        }

        private void cboAbnormalState_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //상태가 비정상인 경우 비정상사유 콤보박스 활성화
            if (cboAbnormalState.SelectedValue?.GetString() == "Y")
            {
                cboAbnormalReason.IsEnabled = true;
                cboAbnormalReason.SelectedIndex = 0;
            }
            else
            {
                cboAbnormalReason.IsEnabled = false;
                cboAbnormalReason.SelectedIndex = -1;
            }
        }

        private void rdoSaveType_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton == null) return;

            BottomDefectArea.Visibility = Visibility.Collapsed;
            BottomAbnormalArea.Visibility = Visibility.Collapsed;
            BottomScrapArea.Visibility = Visibility.Collapsed;

            switch (radioButton.Name)
            {
                case "rdoDefect":
                    BottomDefectArea.Visibility = Visibility.Visible;
                    break;
                case "rdoAbnormal":
                    BottomAbnormalArea.Visibility = Visibility.Visible;
                    break;
                case "rdoScrap":
                    BottomScrapArea.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Method
        private void SelecCarrierSummary()
        {
            string bizRuleName = string.Empty;

            if (cboArea.SelectedValue.ToString() == "ED")
            {
               bizRuleName = "BR_MCS_GET_ELTR_CARRIER_SUMMARY_INFO";
            }
            else
            {
                bizRuleName = "BR_MCS_GET_CARRIER_SUMMARY_INFO";
            }
          
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTTYPE", typeof(string));
                inTable.Columns.Add("CSTPROD", typeof(string));
                inTable.Columns.Add("CSTOWNER", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTTYPE"] = cboCarrierType.SelectedValue;
                dr["CSTPROD"] = cboCarrierProd.SelectedValue;
                dr["CSTOWNER"] = cboArea.SelectedValue;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    if(cboArea.SelectedValue.ToString() == "ED")
                    {
                        var query = bizResult.AsEnumerable().GroupBy(x => new
                        { }).Select(g => new
                        {
                            CarrierTypeCode = cboCarrierType.SelectedValue,
                            CarrierProductCode = string.Empty,
                            CarrierProductName = ObjectDic.Instance.GetObjectName("합계"),
                            CarrierTotalCount = g.Sum(x => x.Field<Int32>("T_CNT")),
                            CarrierUseTotalCount = g.Sum(x => x.Field<Int32>("U_T_CNT")),
                            CarrierUseElectCount = g.Sum(x => x.Field<Int32>("ELTR_U_CNT")),
                            CarrierUseCellCount = g.Sum(x => x.Field<Int32>("CELL_U_CNT")),
                            CarrierEmptyTotalCount = g.Sum(x => x.Field<Int32>("E_T_CNT")),
                            CarrierEmptyElectCount = g.Sum(x => x.Field<Int32>("ELTR_E_CNT")),
                            CarrierEmptyCellCount = g.Sum(x => x.Field<Int32>("CELL_E_CNT")),
                            CarrierDefectCount = g.Sum(x => x.Field<Int32>("D_CNT"))
                        }).FirstOrDefault();

                        if (query != null)
                        {
                            DataRow newRow = bizResult.NewRow();
                            newRow["CSTTYPE"] = query.CarrierTypeCode;
                            newRow["CSTPROD"] = query.CarrierProductCode;
                            newRow["CSTPROD_NAME"] = query.CarrierProductName;
                            newRow["T_CNT"] = query.CarrierTotalCount;
                            newRow["U_T_CNT"] = query.CarrierUseTotalCount;
                            newRow["ELTR_U_CNT"] = query.CarrierUseElectCount;
                            newRow["CELL_U_CNT"] = query.CarrierUseCellCount;
                            newRow["E_T_CNT"] = query.CarrierEmptyTotalCount;
                            newRow["ELTR_E_CNT"] = query.CarrierEmptyElectCount;
                            newRow["CELL_E_CNT"] = query.CarrierEmptyCellCount;
                            newRow["D_CNT"] = query.CarrierDefectCount;

                            bizResult.Rows.Add(newRow);
                        }
                        Util.GridSetData(dgCarrierSummary_Elec, bizResult, null, true);
                    }
                    else
                    {
                        var query = bizResult.AsEnumerable().GroupBy(x => new
                        { }).Select(g => new
                        {
                            CarrierTypeCode = cboCarrierType.SelectedValue,
                            CarrierProductCode = string.Empty,
                            CarrierProductName = ObjectDic.Instance.GetObjectName("합계"),
                            CarrierTotalCount = g.Sum(x => x.Field<Int32>("T_CNT")),
                            CarrierUseCount = g.Sum(x => x.Field<Int32>("U_CNT")),
                            CarrierEmptyCount = g.Sum(x => x.Field<Int32>("E_CNT")),
                            CarrierEmptyTrayCount = g.Sum(x => x.Field<Int32>("ET_CNT")),
                            CarrierDefectCount = g.Sum(x => x.Field<Int32>("D_CNT")),
                            CarrierAbnormalCount = g.Sum(x => x.Field<Int32>("ABNORM_CNT")),
                            //CarrierNonUseCount = g.Sum(x => x.Field<Int32>("UN_CNT"))
                        }).FirstOrDefault();

                        if (query != null)
                        {
                            DataRow newRow = bizResult.NewRow();
                            newRow["CSTTYPE"] = query.CarrierTypeCode;
                            newRow["CSTPROD"] = query.CarrierProductCode;
                            newRow["CSTPROD_NAME"] = query.CarrierProductName;
                            newRow["T_CNT"] = query.CarrierTotalCount;
                            newRow["U_CNT"] = query.CarrierUseCount;
                            newRow["E_CNT"] = query.CarrierEmptyCount;
                            newRow["ET_CNT"] = query.CarrierEmptyTrayCount;
                            newRow["D_CNT"] = query.CarrierDefectCount;
                            newRow["ABNORM_CNT"] = query.CarrierAbnormalCount;
                            //newRow["UN_CNT"] = query.CarrierNonUseCount;
                            bizResult.Rows.Add(newRow);
                        }
                        Util.GridSetData(dgCarrierSummary, bizResult, null, true);
                    }
                  
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectCarrierDetailList(bool isCarrierIdOnly, bool isLotOnly)
        {
            SetDataGridCheckHeaderInitialize(dgCarrierDetail);
            //const string bizRuleName = "DA_MCS_SEL_CARRIER_AREA_SUMMARY_LIST";
            //const string bizRuleName = "BR_MCS_GET_BOBBIN_INFO";

            const string bizRuleName = "BR_MCS_GET_CARRIER_DETAIL_INFO";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTTYPE", typeof(string));
                inTable.Columns.Add("CSTPROD", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("CSTSTAT", typeof(string));
                inTable.Columns.Add("CSTOWNER", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["CSTOWNER"] = cboArea.SelectedValue;

                if (isCarrierIdOnly)
                {
                    dr["CSTID"] = !string.IsNullOrEmpty(txtCarrierId.Text) ? txtCarrierId.Text : null;
                }
                else if (isLotOnly)
                {
                    dr["LOTID"] = !string.IsNullOrEmpty(txtLotId.Text) ? txtLotId.Text : null;
                }
                else
                {
                    dr["CSTTYPE"] = _selectedCarrierTypeCode;
                    dr["CSTPROD"] = _selectedCarrierProductCode;
                    dr["CSTSTAT"] = _selectedCarrierStateCode;
                    dr["CSTID"] = !string.IsNullOrEmpty(txtCarrierId.Text) ? txtCarrierId.Text : null;
                }
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgCarrierDetail, bizResult, null, true);

                    if (isCarrierIdOnly || isLotOnly)
                    {
                        if (dgCarrierDetail.Rows.Count > 2)
                        {
                            string strCstType = DataTableConverter.GetValue(dgCarrierDetail.Rows[2].DataItem, "CSTTYPE").GetString();
                            this.cboCarrierType.SelectedValue = strCstType;

                            if (cboCarrierType.SelectedValue.ToString() == "PT")
                            {
                                rdoAbnormal.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                rdoDefect.IsChecked = true;
                                rdoAbnormal.Visibility = Visibility.Collapsed;
                            }
                        }
                        else
                        {
                            this.cboCarrierType.SelectedValue = "SELECT";
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectCarrierStateList(C1DataGrid dg)
        {
            const string bizRuleName = "DA_MCS_SEL_CARRIER_AREA_SUMMARY_LIST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("CSTTYPE", typeof(string));
                inTable.Columns.Add("CSTPROD", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("CSTOWNER", typeof(string));
                inTable.Columns.Add("CST_DFCT_FLAG", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["AREAID"] = cboAreaGroup.SelectedValue;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["CSTTYPE"] = string.Equals(dg.Name, "dgCarrierChange") ? cboCarrierType.SelectedValue : "SD";
                dr["CSTID"] = !string.IsNullOrEmpty(txtCarrierId.Text) ? txtCarrierId.Text : null;
                dr["CST_DFCT_FLAG"] = string.Equals(dg.Name, "dgCarrierChange") ? "Y" : null;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dg, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveCarrierState()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MCS_REG_CARRIER_DEFECT";

                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("CST_DFCT_FLAG", typeof(string));
                inTable.Columns.Add("CST_DFCT_RESNCODE", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["CST_DFCT_FLAG"] = cboCarrierState.SelectedValue;

                if (cboCarrierState.SelectedValue?.GetString() == "Y")
                {
                    newRow["CST_DFCT_RESNCODE"] = cboDefectReason.SelectedValue;
                    newRow["NOTE"] = !string.IsNullOrEmpty(txtNote.Text) ? txtNote.Text : null;
                }

                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inCarrierTable = ds.Tables.Add("INCST");
                inCarrierTable.Columns.Add("CSTID", typeof(string));

                //foreach (C1.WPF.DataGrid.DataGridRow row in dgCarrierDetail.Rows)
                //{
                //    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                //    {
                //        DataRow dr = inCarrierTable.NewRow();
                //        dr["CSTID"] = DataTableConverter.GetValue(row.DataItem, "CSTID").GetString();
                //        inCarrierTable.Rows.Add(dr);
                //    }
                //}

                DataRow[] drSelect = DataTableConverter.Convert(dgCarrierDetail.ItemsSource).Select("CHK = 1");
                foreach (DataRow row in drSelect)
                {
                    DataRow dr = inCarrierTable.NewRow();
                    dr["CSTID"] = row["BOBBIN_ID"];
                    inCarrierTable.Rows.Add(dr);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INCST", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상처리되었습니다.
                        SelectCarrierDetailList(false, false);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveCarrierChange()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MCS_REG_CARRIER_DEFECT";

                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("CST_DFCT_FLAG", typeof(string));
                inTable.Columns.Add("CST_DFCT_RESNCODE", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["CST_DFCT_FLAG"] = "N";
                newRow["NOTE"] = !string.IsNullOrEmpty(txtCarrierChangeNote.Text) ? txtCarrierChangeNote.Text : null;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inCarrierTable = ds.Tables.Add("INCST");
                inCarrierTable.Columns.Add("CSTID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgCarrierChange.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow dr = inCarrierTable.NewRow();
                        dr["CSTID"] = DataTableConverter.GetValue(row.DataItem, "CSTID").GetString();
                        inCarrierTable.Rows.Add(dr);
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INCST", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상처리되었습니다.
                                                        //SelectCarrierDetailList(false);
                        SelectCarrierStateList(dgCarrierChange);
                        //btnSearch_Click(btnSearch, null);

                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitializeCombo()
        {
            // 동그룹 콤보박스
            SetAreaGroupCombo(cboAreaGroup);

            // 동 콤보박스
            SetAreaCombo(cboArea);

            // Carrier Type 콤보박스
            SetCarrierTypeCombo(cboCarrierType);

            // Carrier Prod 콤보박스
            SetCarrierProdCombo(cboCarrierProd, cboCarrierType);

            // Carrier 상태 콤보박스
            SetCarrierStateCombo(cboCarrierState);

            // 불량 사유 
            SetDefectReasonCombo(cboDefectReason);

            // 비정상 상태 콤보박스
            SetAbnomalStateCombo(cboAbnormalState);

            // 비정상 사유 코드 콤보박스
            SetAbnomalReasonCombo(cboAbnormalReason);

            // 폐기 상태 콤보박스
            SetScrapStateCombo(cboScrapState);

            cboCarrierType.SelectedItemChanged += cboCarrierType_SelectedItemChanged;
        }

        private void ClearControl()
        {
            _selectedCarrierTypeCode = string.Empty;
            _selectedCarrierProductCode = string.Empty;
            _selectedCarrierStateCode = string.Empty;

            Util.gridClear(dgCarrierSummary);
            Util.gridClear(dgCarrierDetail);
            Util.gridClear(dgSkidHistory);
            Util.gridClear(dgCarrierChange);
        }

        private bool ValidationSaveCarrierState()
        {
            C1DataGrid dg = dgCarrierDetail;

            if (!CommonVerify.HasDataGridRow(dg))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dg, "CHK", true) < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (cboCarrierState.SelectedIndex < 0 || cboCarrierState.SelectedValue == null)
            {
                Util.MessageValidation("MCS0005", ObjectDic.Instance.GetObjectName("상태"));
                return false;
            }

            return true;
        }

        private bool ValidationSaveCarrierChange()
        {
            C1DataGrid dg = dgCarrierChange;

            if (!CommonVerify.HasDataGridRow(dg))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dg, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            return true;
        }

        private static void SetAreaGroupCombo(C1ComboBox cbo)
        {
            //const string bizRuleName = "DA_BAS_SEL_AUTH_AREA_CBO";
            //string[] arrColumn = { "LANGID", "SHOPID", "SYSTEM_ID", "USERID" };
            //string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.SYSID, LoginInfo.USERID };
            const string bizRuleName = "DA_MCS_SEL_LOGIS_AREA_GR_CODE_CBO";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);
        }

        private void SetAreaCombo(C1ComboBox cbo)
        {
            string areaGroup = string.IsNullOrEmpty(cboAreaGroup.SelectedValue.GetString()) ? null : cboAreaGroup.SelectedValue.GetString();

            const string bizRuleName = "DA_BAS_SEL_COMMONCODE_BY_ATTR1";
            string[] arrColumn = { "LANGID", "CMCDTYPE", "ATTRIBUTE1" };
            string[] arrCondition = { LoginInfo.LANGID, "LOGIS_TRF_SECTION_CODE", areaGroup };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            //2020 11 23 오화백  ALL -> SELECT 변경
            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);
        }

        private static void SetCarrierTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MES_SEL_CARRIER_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "CSTTYPE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);
        }

        private static void SetCarrierProdCombo(C1ComboBox cbo, C1ComboBox cboParent)
        {
            if (cboParent.SelectedValue == null) return;

            const string bizRuleName = "DA_MCS_SEL_CARRIER_PROD_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE", "ATTRIBUTE1" };
            string[] arrCondition = { LoginInfo.LANGID, "CSTPROD", cboParent.SelectedValue.ToString() };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private static void SetCarrierStateCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "CST_DFCT_FLAG" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private static void SetDefectReasonCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_ACTIVITIREASON_CBO";
            string[] arrColumn = { "LANGID", "ACTID" };
            string[] arrCondition = { LoginInfo.LANGID, "DEFECT_CST" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private static void SetAbnomalStateCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "CST_ABNORM_FLAG" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private static void SetAbnomalReasonCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "ABNORM_TRF_RSN_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private static void SetScrapStateCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "CST_SCRAP_FLAG" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetDataGridCheckHeaderInitialize(C1DataGrid dg)
        {
            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dg.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;
            if (allCheck?.IsChecked == true)
            {
                allCheck.Unchecked -= chkHeaderAll_Unchecked;
                allCheck.IsChecked = false;
                allCheck.Unchecked += chkHeaderAll_Unchecked;
            }
        }

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null && (loadingIndicator != null || loadingIndicator.Visibility != Visibility.Visible))
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null && loadingIndicator.Visibility == Visibility.Visible)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private bool ValidationSearch()
        {
            if (TabItemCarrierInfo.IsSelected)
            {

                if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString() == "SELECT")
                {
                    //동을 선택하세요.
                    Util.MessageValidation("SFU4238");
                    return false;
                }


                if (cboCarrierType.SelectedValue == null || cboCarrierType.SelectedValue.ToString() == "SELECT")
                {
                    // Carrier Type을 선택하세요.
                    Util.MessageValidation("SFU4523");
                    return false;
                }
            }

            return true;
        }

        private void SetGridSummaryColumnChange()
        {
            if (cboCarrierType.SelectedValue == null || cboCarrierType.SelectedValue.ToString() == "SELECT") return;

            dgCarrierSummary.Columns["ET_CNT"].Visibility = Visibility.Collapsed;
            dgCarrierSummary.Columns["ABNORM_CNT"].Visibility = Visibility.Collapsed;

            switch (cboCarrierType.SelectedValue.ToString())
            {
                case "BB":
                case "EB":
                case "SD":
                case "BK":
                case "MG":
                    break;
                case "PT":
                    dgCarrierSummary.Columns["ET_CNT"].Visibility = Visibility.Visible;
                    dgCarrierSummary.Columns["ABNORM_CNT"].Visibility = Visibility.Visible;
                    break;
            }
        }

        private void SetGridDetailColumnText()
        {
            if (cboCarrierType.SelectedValue == null || cboCarrierType.SelectedValue.ToString() == "SELECT") return;

            dgCarrierDetail.Columns["CARRIERID"].Visibility = Visibility.Visible;

            switch (cboCarrierType.SelectedValue.ToString())
            {
                case "BB":
                case "EB":
                case "SD":
                    dgCarrierDetail.Columns["BOBBIN_ID"].Header = ObjectDic.Instance.GetObjectName("보빈 ID");
                    break;
                case "BK":
                    dgCarrierDetail.Columns["BOBBIN_ID"].Header = ObjectDic.Instance.GetObjectName("BOX");
                    break;
                case "MG":
                    dgCarrierDetail.Columns["BOBBIN_ID"].Header = ObjectDic.Instance.GetObjectName("MAGAZINEID");
                    break;
                case "PT":
                    dgCarrierDetail.Columns["BOBBIN_ID"].Header = ObjectDic.Instance.GetObjectName("Carrier ID");
                    dgCarrierDetail.Columns["CARRIERID"].Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private bool ValidationSaveAbnormalState()
        {
            C1DataGrid dg = dgCarrierDetail;

            if (!CommonVerify.HasDataGridRow(dg))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dg, "CHK", true) < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (cboAbnormalState.SelectedIndex < 0 || cboAbnormalState.SelectedValue == null)
            {
                Util.MessageValidation("MCS0005", ObjectDic.Instance.GetObjectName("상태"));
                return false;
            }

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                {
                    //DataRow newRow = inTable.NewRow();
                    //newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "CSTID").GetString();
                    //newRow["SRC_EQPTID"] = DataTableConverter.GetValue(row.DataItem, "EQPTID").GetString();
                    //newRow["SRC_LOCID"] = DataTableConverter.GetValue(row.DataItem, "RACK_ID").GetString();
                    //newRow["DST_EQPTID"] = strOutPortTagList[0];
                    //newRow["DST_LOCID"] = strOutPortTagList[1];
                    //newRow["USER"] = LoginInfo.USERID;
                    //newRow["DTTM"] = dtSystem;
                    //newRow["PRODID"] = IsProdIdNull ? null : DataTableConverter.GetValue(row.DataItem, "PRODID").GetString();
                    //newRow["CARRIER_STRUCT"] = IsCstStatNull ? null : DataTableConverter.GetValue(row.DataItem, "CSTSTAT").GetString();
                    //newRow["MDL_TP"] = IsTrayTypeNull ? null : DataTableConverter.GetValue(row.DataItem, "CSTPROD").GetString();
                    //inTable.Rows.Add(newRow);
                }
            }

            return true;
        }

        private void SaveAbnormal()
        {
            try
            {
                ShowLoadingIndicator();

                if (cboAbnormalState.SelectedValue.ToString() == "N")
                {
                    //정상일경우
                    const string bizRuleName = "BR_MCS_REG_CARRIER_ABNORM_RELEASE_LIST";

                    DataTable inTable = new DataTable("INDATA");
                    inTable.Columns.Add("SRCTYPE", typeof(string));
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("CSTID", typeof(string));
                    inTable.Columns.Add("USERID", typeof(string));

                    DataRow[] drSelect = DataTableConverter.Convert(dgCarrierDetail.ItemsSource).Select("CHK = 1");
                    foreach (DataRow row in drSelect)
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["EQPTID"] = null;
                        newRow["CSTID"] = row["BOBBIN_ID"];
                        newRow["USERID"] = LoginInfo.USERID;
                        inTable.Rows.Add(newRow);
                    }

                    new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                    {
                        try
                        {
                            HiddenLoadingIndicator();

                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            Util.MessageInfo("SFU1275");    //정상처리되었습니다.
                            SelectCarrierDetailList(false, false);
                        }
                        catch (Exception ex)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(ex);
                        }
                    });
                }
                else
                {
                    //비정상일 경우
                    const string bizRuleName = "BR_MCS_REG_CARRIER_ABNORM_LIST";

                    DataTable inTable = new DataTable("INDATA");
                    inTable.Columns.Add("SRCTYPE", typeof(string));
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("CSTID", typeof(string));
                    inTable.Columns.Add("ABNORM_TRF_RSN_CODE", typeof(string));
                    inTable.Columns.Add("USERID", typeof(string));

                    DataRow[] drSelect = DataTableConverter.Convert(dgCarrierDetail.ItemsSource).Select("CHK = 1");
                    foreach (DataRow row in drSelect)
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["EQPTID"] = null;
                        newRow["CSTID"] = row["BOBBIN_ID"];
                        newRow["ABNORM_TRF_RSN_CODE"] = (cboAbnormalState.SelectedValue.ToString() == "N") ? "" : cboAbnormalReason.SelectedValue.ToString();
                        newRow["USERID"] = LoginInfo.USERID;
                        inTable.Rows.Add(newRow);
                    }

                    new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                    {
                        try
                        {
                            HiddenLoadingIndicator();

                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            Util.MessageInfo("SFU1275");    //정상처리되었습니다.
                            SelectCarrierDetailList(false, false);
                        }
                        catch (Exception ex)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(ex);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidationSaveScrap()
        {
            C1DataGrid dg = dgCarrierDetail;

            if (!CommonVerify.HasDataGridRow(dg))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dg, "CHK", true) < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (cboScrapState.SelectedIndex < 0 || cboScrapState.SelectedValue == null)
            {
                Util.MessageValidation("MCS0005", ObjectDic.Instance.GetObjectName("상태"));
                return false;
            }

            return true;
        }

        private void SaveScrap()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MCS_REG_CARRIER_SCRAP_LIST";

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("CST_MNGT_STAT_CODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow[] drSelect = DataTableConverter.Convert(dgCarrierDetail.ItemsSource).Select("CHK = 1");
                foreach (DataRow row in drSelect)
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["EQPTID"] = null;
                    newRow["CSTID"] = row["BOBBIN_ID"];
                    newRow["CST_MNGT_STAT_CODE"] = (cboScrapState.SelectedValue.ToString() == "N") ? "I" : "S";
                    newRow["USERID"] = LoginInfo.USERID;
                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상처리되었습니다.
                        SelectCarrierDetailList(false, false);
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

        private void txtLotId_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(txtLotId.Text))
            {
                cboAreaGroup.IsEnabled = true;
                cboCarrierType.IsEnabled = true;
                txtCarrierId.IsEnabled = true;
            }
            else
            {
                cboAreaGroup.IsEnabled = false;
                cboCarrierType.IsEnabled = false;
                txtCarrierId.IsEnabled = false;
            }
        }



        /// <summary>
        /// 2020 11 23 오화백 동정보 선택시 스프레드 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();

            if (cboArea.SelectedValue.ToString() == "ED")
            {
                dgCarrierSummary.Visibility = Visibility.Collapsed;
                dgCarrierSummary_Elec.Visibility = Visibility.Visible;
            }
            else
            {
                dgCarrierSummary.Visibility = Visibility.Visible;
                dgCarrierSummary_Elec.Visibility = Visibility.Collapsed;
            }
        }

        private void dgCarrierSummary_Elec_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CSTPROD_NAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }

        private void dgCarrierSummary_Elec_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgCarrierSummary_Elec_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null) return;
                C1DataGrid dataGrid = sender as C1DataGrid;

                if (dataGrid == null || dataGrid.CurrentCell == null || dataGrid.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgCarrierSummary_Elec.GetCellFromPoint(pnt);

                if (cell == null) return;

                // 선택한 셀의 위치
                int rowIdx = cell.Row.Index;
                DataRowView drv = dataGrid.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                if (cell.Column.Name == "CSTPROD_NAME" || cell.Column.Name == "T_CNT")
                {
                    _selectedCarrierTypeCode = DataTableConverter.GetValue(drv, "CSTTYPE").GetString();
                    _selectedCarrierProductCode = null;
                    _selectedCarrierStateCode = null;

                    if (DataTableConverter.GetValue(drv, "CSTPROD_NAME").GetString() != ObjectDic.Instance.GetObjectName("합계"))
                    {
                        _selectedCarrierProductCode = DataTableConverter.GetValue(drv, "CSTPROD").GetString();
                    }
                }
                else
                {
                    if (cell.Column.Name == "U_T_CNT")
                    {
                        _selectedCarrierStateCode = "U";
                    }
                    else if (cell.Column.Name == "ELTR_U_CNT")
                    {
                        _selectedCarrierStateCode = "UE";
                    }
                    else if (cell.Column.Name == "CELL_U_CNT")
                    {
                        _selectedCarrierStateCode = "UC";
                    }
                    else if (cell.Column.Name == "E_T_CNT")
                    {
                        _selectedCarrierStateCode = "E";
                    }
                    else if (cell.Column.Name == "ELTR_E_CNT")
                    {
                        _selectedCarrierStateCode = "EE";
                    }
                    else if (cell.Column.Name == "CELL_E_CNT")
                    {
                        _selectedCarrierStateCode = "EC";
                    }
                    else if (cell.Column.Name == "D_CNT")
                    {
                        _selectedCarrierStateCode = "D";
                    }
                    else
                        _selectedCarrierStateCode = null;

                    _selectedCarrierTypeCode = DataTableConverter.GetValue(drv, "CSTTYPE").GetString();
                    _selectedCarrierProductCode = string.IsNullOrEmpty(DataTableConverter.GetValue(drv, "CSTPROD").GetString()) ? null : DataTableConverter.GetValue(drv, "CSTPROD").GetString();
                }

                SelectCarrierDetailList(false, false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
