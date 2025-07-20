using System;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcPolymerFormProdLot.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcPolymerFormProdLot
    {
        #region Declaration & Constructor 
        public UserControl UcParentControl;
        public C1DataGrid DgProductLot { get; set; }

        public string ProcessCode { get; set; }

        public UcPolymerFormProdLot()
        {
            InitializeComponent();
            SetControl();
        }

        #endregion


        #region Initialize
        private void SetControl()
        {
            DgProductLot = dgProductLot;
        }
        #endregion


        #region Event
        private void dgProductLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Column.Name.Equals("DIFF_QTY"))
                {
                    if (e.Cell.Value == null || string.IsNullOrWhiteSpace(e.Cell.Value.ToString()) || e.Cell.Value.ToString() == "0")
                        return;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                }
            }));
        }

        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
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

                        //Control Clear
                        ClearDetailControls();

                        //상세 정보 조회
                        ProdListClickedProcess(idx);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion


        #region Mehod

        public void SetDataGridColumnVisibility()
        {
            if (string.Equals(ProcessCode, Process.PolymerOffLineCharacteristic) ||
                string.Equals(ProcessCode, Process.PolymerFinalExternalDSF) ||
                string.Equals(ProcessCode, Process.PolymerFinalExternal))
            {
                // 특성 최종외관 WIPSNAME
                dgProductLot.Columns["CTNR_ID"].Visibility = Visibility.Visible;
                dgProductLot.Columns["WIPSNAME"].Visibility = Visibility.Visible;
                dgProductLot.Columns["GOOD_QTY"].Visibility = Visibility.Visible;
                dgProductLot.Columns["DIFF_QTY"].Visibility = Visibility.Visible;
            }
            else if(string.Equals(ProcessCode, Process.CELL_BOXING) || string.Equals(ProcessCode,Process.CELL_BOXING_RETURN))
            {
                dgProductLot.Columns["FORM_WRK_TYPE_NAME"].Visibility = Visibility.Collapsed;
            }

            // 작업Mode
            if (string.Equals(ProcessCode, Process.PolymerDSF))
            {
                dgProductLot.Columns["CTNR_TYPE_NAME"].Visibility = Visibility.Visible;
            }
            else
            {
                dgProductLot.Columns["CTNR_TYPE_NAME"].Visibility = Visibility.Collapsed;
            }

            //GMES-R0438 조립LOT의 PKG설비명 추가 
            if (string.Equals(ProcessCode, Process.CELL_BOXING) || // B1000 CELL 포장
                string.Equals(ProcessCode, Process.CELL_BOXING_RETURN) || // B9000 물류 반품
                string.Equals(ProcessCode, Process.CELL_BOXING_RETURN_RMA) || //B9100 RMA 반품
                string.Equals(ProcessCode, Process.PolymerFairQuality) || // F7500 DSF 양품화
                string.Equals(ProcessCode, "F7700") || // F7700 TCO 양품화
                string.Equals(ProcessCode, "F7800"))  // F7800 특성/외관 양품화
            {
                dgProductLot.Columns["PKG_EQPTNAME"].Visibility = Visibility.Visible;
            }
            else
            {
                dgProductLot.Columns["PKG_EQPTNAME"].Visibility = Visibility.Collapsed;
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
                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    object[] parameterArrys = new object[parameters.Length];
                    parameterArrys[0] = idx;

                    methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected virtual void ClearDetailControls()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("ClearDetailControls");

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

        #endregion

    }
}
