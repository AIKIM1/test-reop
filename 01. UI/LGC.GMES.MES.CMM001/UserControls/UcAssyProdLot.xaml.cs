/*************************************************************************************
 Created Date : 2017.05.17
      Creator : 신 광희
   Decription : 조립 Product Lot UserControl
--------------------------------------------------------------------------------------
 [Change History]
 2017.05.26  신 광희 : 원각 및 초소형 작업영역 UserControl 데이터 그리드 수정 및 메소드 추가
 2021.09.03  심찬보S : 오창 소형조립UI 버전추가 및 활성화(PROD_VER_CODE)
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcAssyProdLot.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcAssyProdLot
    {
        #region Declaration & Constructor 
        public UserControl UcParentControl;
        public C1DataGrid DgProductLot { get; set; }

        public C1DataGrid DgLotInfo { get; set; }

        public string ProcessCode { get; set; }

        public string ProdVerCode { get; set; }


        public bool IsSmalltype;

        public UcAssyProdLot()
        {
            InitializeComponent();
            SetControl();
        }

        private readonly Util _util = new Util();
        #endregion


        #region Initialize
        private void SetControl()
        {
            DgProductLot = dgProductLot;
        }
        #endregion


        #region Event
        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //(grdResult.Children[0] as UcAssyResult).dtpCaldate.SelectedDataTimeChanged -= OndtpCaldate_SelectedDataTimeChanged;
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }
                        //row 색 바꾸기
                        dgProductLot.SelectedIndex = idx;

                        ClearResultCollectControls();
                        //상세 정보 조회
                        ProdListClickedProcess(idx);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                
            }
        }

        private void dgProductLot_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgProductLot.GetCellFromPoint(pnt);

            if (cell != null && cell.Column.Name != "CHK")
            {
                if (_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0) return;

                int rowIndex = _util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK");

                if (cell.Row.Index != rowIndex)
                    e.Handled = true;
            }
        }

        private void dgProductLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null || !CommonVerify.HasDataGridRow(dgProductLot) || !(string.Equals(ProcessCode, Process.WINDING) || string.Equals(ProcessCode, Process.WINDING_POUCH)))
                    return;

                C1DataGrid dataGrid = sender as C1DataGrid;

                DataTable dt = ((DataView)dgProductLot.ItemsSource).Table;
                var query = (from t in dt.AsEnumerable()
                             where t.Field<string>("WIPSTAT") == "EQPT_END"
                             orderby t.Field<string>("WIPDTTM_ED") descending
                             select new
                             {
                                 LotId = t.Field<string>("LOTID")
                             } ).FirstOrDefault();

                string selectedLotId = string.Empty;
                if (query != null)
                    selectedLotId = query.LotId;

                dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    //Grid Data Binding 이용한 Background 색 변경
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (!string.IsNullOrEmpty(selectedLotId))
                        {
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT")).Equals("EQPT_END") && DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTID").GetString() == selectedLotId)
                            {
                                var convertFromString = ColorConverter.ConvertFromString("#47C83E");
                                if (convertFromString != null)
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color) convertFromString);
                            }
                            else
                            {
                                e.Cell.Presenter.Background = null;
                            }
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProductLot_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            if (sender == null || !CommonVerify.HasDataGridRow(dgProductLot))
                return;

            if (dgProductLot.SelectedItem != null)
            {
                DataTableConverter.SetValue(dgProductLot.SelectedItem, "CHK", true);
                int idx = dgProductLot.SelectedIndex;

                for (int i = 0; i < dgProductLot.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", idx == i);
                }

                ClearResultCollectControls();
                //상세 정보 조회
                ProdListClickedProcess(idx);

                if (string.Equals(ProcessCode, Process.WINDING) || string.Equals(ProcessCode, Process.WINDING_POUCH))
                {
                    dgProductLot.Selection.Clear();
                }

                /*
                foreach (C1.WPF.DataGrid.DataGridCell cell in dgProductLot.Selection.SelectedCells)
                {
                    if (cell.Presenter != null)
                    {
                        var convertFromString = ColorConverter.ConvertFromString("#47C83E");
                        if (cell.Presenter.Background == convertFromString)
                            cell.Presenter.Background = null;
                        //continue;
                        else
                            cell.Presenter.Background = null;
                    }
                }
                */
            }


        }
        #endregion


        #region Mehod

        public void SetDataGridColumnVisibility()
        {
            if (string.Equals(ProcessCode, Process.WINDING) || string.Equals(ProcessCode, Process.WINDING_POUCH))
            {
                //EQPT_END_QTY
                dgProductLot.Columns["EQPT_END_QTY"].Header = new string[] {"설비투입수량","설비투입수량"}.ToList<string>();
                dgProductLot.Columns["CELL_CNT"].Visibility = Visibility.Visible;
                dgProductLot.Columns["WOTYPEDESC"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["PRODID"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["PRODNAME"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["CELL_CNT"].Visibility = Visibility.Collapsed;

                if (!IsSmalltype)
                {
                    dgProductLot.Columns["TRAY_CNT"].Visibility = Visibility.Collapsed;
                    dgProductLot.Columns["JR_CNT"].Visibility = Visibility.Collapsed;
                }

            }
            else if (string.Equals(ProcessCode, Process.ASSEMBLY))
            {
                dgProductLot.Columns["EQPT_END_QTY"].Header = new string[] { "설비생산수량", "설비생산수량" }.ToList<string>();
                dgProductLot.Columns["TRAY_CNT"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["JR_CNT"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["CELL_CNT"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["GOODQTY"].Visibility = Visibility.Collapsed;
            }
            else
            {
                //dgProductLot.Columns["EQPT_END_QTY"].Header = new string[] {"생산 수량","생산 수량"}.ToList<string>();
                dgProductLot.Columns["EQPT_END_QTY"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["TRAY_CNT"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["JR_CNT"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["CELL_CNT"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["GOODQTY"].Visibility = Visibility.Collapsed;
            }
        }

        protected virtual void ProdListClickedProcess(int idx)
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("ProdListClickedProcess");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                parameterArrys[0] = idx;

                methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected virtual void ClearResultCollectControls()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("ClearResultCollectControls");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }




        #endregion

        
    }
}
