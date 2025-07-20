/*************************************************************************************
 Created Date : 2019.07.24
      Creator : 정문교
   Decription : 원자재관리 - 월 설비,자재코드별 수불 조회
--------------------------------------------------------------------------------------
 [Change History]


 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_012 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MTRL001_012()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void InitializeControls()
        {
        }

        private void InitializeGrid()
        {
            if (((System.Windows.FrameworkElement)tabStock.SelectedItem).Name.Equals("ctbEquipment"))
            {
                Util.gridClear(dgList);
                Util.gridClear(dgMlot);

                if (!(bool)chkMlot.IsChecked)
                    dgList.AlternatingRowBackground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF9F9F9"));
                else
                    dgList.AlternatingRowBackground = null;
            }
            else
            {
                Util.gridClear(dgListM);
                Util.gridClear(dgMlotM);

                if (!(bool)chkMlotM.IsChecked)
                    dgListM.AlternatingRowBackground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF9F9F9"));
                else
                    dgListM.AlternatingRowBackground = null;
            }
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            /////////////////////////////////////////// 설비
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChild, cbParent: cboProcessParent);

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            /////////////////////////////////////////// 자재
            //동
            C1ComboBox[] cboAreaMChild = { cboEquipmentSegmentM };
            _combo.SetCombo(cboAreaM, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaMChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentMParent = { cboAreaM };
            C1ComboBox[] cboEquipmentSegmentMChild = { cboProcessM, cboEquipmentM };
            _combo.SetCombo(cboEquipmentSegmentM, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentMChild, cbParent: cboEquipmentSegmentMParent, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessMParent = { cboEquipmentSegmentM };
            C1ComboBox[] cboProcessMChild = { cboEquipmentM };
            _combo.SetCombo(cboProcessM, CommonCombo.ComboStatus.ALL, cbChild: cboProcessMChild, cbParent: cboProcessMParent, sCase: "PROCESS");

            //설비
            C1ComboBox[] cboEquipmentMParent = { cboEquipmentSegmentM, cboProcessM };
            _combo.SetCombo(cboEquipmentM, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentMParent, sCase: "EQUIPMENT");
        }

        private void SetControl(bool isVisibility = false)
        {
            GridColumnVisibility(false);
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeControls();
            InitializeGrid();
            InitCombo();
            SetControl();

            this.Loaded -= UserControl_Loaded;
        }

        /// <summary>
        /// 자재 Lot Visibility
        /// </summary>
        private void chkMlot_Checked(object sender, RoutedEventArgs e)
        {
            //GridColumnVisibility(true);
        }
        private void chkMlot_Unchecked(object sender, RoutedEventArgs e)
        {
            //GridColumnVisibility(false);
        }

        #region 설비

        private void dgList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgList.GetCellFromPoint(pnt);

            if (cell != null)
            {
                SearchMLOT(dgList, cell.Row.Index, dtpMonth.SelectedDateTime.ToString("yyyyMM"));
            }
        }

        /// <summary>
        /// 조회
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            SearchProcess();
        }

        /// <summary>
        /// 엑셀
        /// </summary>
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationExcel())
                    return;

                new ExcelExporter().Export(dgList);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 자재
        /// <summary>
        /// Grid Event
        /// </summary>
        private void dgListM_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgListM.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (!string.Equals(Util.NVC(DataTableConverter.GetValue(cell.Row.DataItem, "SORT_NO")), "9"))
                    SearchMLOT(dgListM, cell.Row.Index, dtpMonthM.SelectedDateTime.ToString("yyyyMM"));
            }
        }

        private void dgListM_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SORT_NO")), "9"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        //e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            }));
        }

        /// <summary>
        /// 조회
        /// </summary>
        private void btnSearchM_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearchM())
                return;

            SearchProcessM();
        }

        /// <summary>
        /// 엑셀
        /// </summary>
        private void btnExcelM_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationExcelM())
                    return;

                new ExcelExporter().Export(dgListM);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region Mehod

        #region [BizCall]
        /// <summary>
        /// 설비별 조회
        /// </summary>
        private void SearchProcess()
        {
            try
            {
                // Clear  
                InitializeGrid();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("YM", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("GUBUN", typeof(string));
                inTable.Columns.Add("MLOT_YN", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["YM"] = dtpMonth.SelectedDateTime.ToString("yyyyMM");
                newRow["AREAID"] = cboArea.SelectedValue.ToString();
                newRow["EQSGID"] = string.IsNullOrWhiteSpace(cboEquipmentSegment.SelectedValue.ToString()) ? null : cboEquipmentSegment.SelectedValue.ToString();
                newRow["PROCID"] = string.IsNullOrWhiteSpace(cboProcess.SelectedValue.ToString()) ? null : cboProcess.SelectedValue.ToString();
                newRow["EQPTID"] = string.IsNullOrWhiteSpace(cboEquipment.SelectedValue.ToString()) ? null : cboEquipment.SelectedValue.ToString();
                newRow["GUBUN"] = "EQP";
                newRow["MLOT_YN"] = (bool)chkMlot.IsChecked ? "Y" : "N";
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_MTR_SEL_EQPT_MATERIAL_STOCK", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgList, bizResult, FrameOperation, true);
                        GridColumnVisibility((bool)chkMlot.IsChecked);
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

        /// <summary>
        /// 자재별 조회
        /// </summary>
        private void SearchProcessM()
        {
            try
            {
                // Clear  
                InitializeGrid();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("YM", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("GUBUN", typeof(string));
                inTable.Columns.Add("MLOT_YN", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["YM"] = dtpMonthM.SelectedDateTime.ToString("yyyyMM");
                newRow["AREAID"] = cboAreaM.SelectedValue.ToString();
                newRow["EQSGID"] = string.IsNullOrWhiteSpace(cboEquipmentSegmentM.SelectedValue.ToString()) ? null : cboEquipmentSegmentM.SelectedValue.ToString();
                newRow["PROCID"] = string.IsNullOrWhiteSpace(cboProcessM.SelectedValue.ToString()) ? null : cboProcessM.SelectedValue.ToString();
                newRow["EQPTID"] = string.IsNullOrWhiteSpace(cboEquipmentM.SelectedValue.ToString()) ? null : cboEquipmentM.SelectedValue.ToString();
                newRow["GUBUN"] = "MAT";
                newRow["MLOT_YN"] = (bool)chkMlotM.IsChecked ? "Y" : "N";
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_MTR_SEL_EQPT_MATERIAL_STOCK", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgListM, bizResult, FrameOperation, true);
                        GridColumnVisibility((bool)chkMlotM.IsChecked);
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

        /// <summary>
        /// 지재 LOT 조회
        /// </summary>
        private void SearchMLOT(C1DataGrid dg, int row, String YM)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("YM", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("MTRLID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["YM"] = YM;
                newRow["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[row].DataItem, "EQPTID").ToString());
                newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[row].DataItem, "MTRLID").ToString());
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_MTR_SEL_EQPT_MATERIAL_STOCK_DETL", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (((System.Windows.FrameworkElement)tabStock.SelectedItem).Name.Equals("ctbEquipment"))
                        {
                            Util.GridSetData(dgMlot, bizResult, null);
                        }
                        else
                        {
                            Util.GridSetData(dgMlotM, bizResult, null);
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Function

        #region [Validation]
        private bool ValidationSearch()
        {
            if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("SELECT"))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU1499");
                return false;
            }

            return true;
        }

        private bool ValidationSearchM()
        {
            if (cboAreaM.SelectedValue == null || cboAreaM.SelectedValue.ToString().Equals("SELECT"))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU1499");
                return false;
            }

            return true;
        }

        private bool ValidationExcel()
        {
            if (dgList.Rows.Count - dgList.TopRows.Count - dgList.BottomRows.Count == 0)
            {
                // 조회된 Data가 없습니다.
                Util.MessageValidation("SFU1905");
                return false;
            }

            return true;
        }

        private bool ValidationExcelM()
        {
            if (dgListM.Rows.Count - dgListM.TopRows.Count - dgListM.BottomRows.Count == 0)
            {
                // 조회된 Data가 없습니다.
                Util.MessageValidation("SFU1905");
                return false;
            }

            return true;
        }

        #endregion

        #region [팝업]
        #endregion

        #region [Func]

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

        private void GridColumnVisibility(bool isVisibility = false)
        {
            C1DataGrid dg;

            if (((System.Windows.FrameworkElement)tabStock.SelectedItem).Name.Equals("ctbEquipment"))
            {
                dg = dgList;
            }
            else
            {
                dg = dgListM;
            }

            if (isVisibility)
            {
                dg.Columns["MLOTID"].Visibility = Visibility.Visible;
            }
            else
            {
                dg.Columns["MLOTID"].Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #endregion

        #endregion

    }
}
