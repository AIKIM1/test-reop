using System;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcFormProdLot.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcFormProdLot
    {
        #region Declaration & Constructor 
        public UserControl UcParentControl;
        public C1DataGrid DgProductLot { get; set; }

        public string ProcessCode { get; set; }

        public UcFormProdLot()
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
                    if (string.IsNullOrWhiteSpace(e.Cell.Value.ToString()) || e.Cell.Value.ToString() == "0")
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

        #endregion


        #region Mehod

        public void SetDataGridColumnVisibility()
        {
            dgProductLot.Columns["GRADER_DEFECT_QTY"].Visibility = Visibility.Collapsed;
            dgProductLot.Columns["CHARACTERISTIC_DEFECT_QTY"].Visibility = Visibility.Collapsed;

            if (string.Equals(ProcessCode, Process.CircularGrader) || 
                string.Equals(ProcessCode, Process.CircularCharacteristic) || 
                string.Equals(ProcessCode, Process.CircularCharacteristicGrader) ||
                string.Equals(ProcessCode, Process.CircularReTubing) ||
                string.Equals(ProcessCode, Process.CircularVoltage))
            {
                dgProductLot.Columns["WND_GR_CODE"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["WND_EQPTID"].Visibility = Visibility.Collapsed;
            }

            if (!string.Equals(ProcessCode, Process.CircularVoltage))
            {
                dgProductLot.Columns["REWORKCNT"].Visibility = Visibility.Collapsed;
            }

            if (LoginInfo.CFG_SHOP_ID.Equals("A010"))
            {
                dgProductLot.Columns["GRADER_DEFECT_QTY"].Visibility = Visibility.Visible;
                dgProductLot.Columns["CHARACTERISTIC_DEFECT_QTY"].Visibility = Visibility.Visible;
            }
        }

        public DataRow GetSelectProductLotRow()
        {
            DataRow row;

            try
            {
                DataRow[] dr = DataTableConverter.Convert(dgProductLot.ItemsSource).Select("CHK = 1");

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
