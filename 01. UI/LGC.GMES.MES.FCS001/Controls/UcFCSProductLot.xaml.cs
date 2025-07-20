/*************************************************************************************
 Created Date : 2020.10.12
      Creator : Kang Dong Hee
   Decription : 활성화 공정진척 - 생산 Lot List (Good)
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.12  NAME : Initial Created
**************************************************************************************/
using System;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using System.Data;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using System.Collections.Generic;
using System.Windows.Input;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using C1.WPF;
using System.Reflection;

namespace LGC.GMES.MES.FCS001.Controls
{
    /// <summary>
    /// UcFCSProductLot.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcFCSProductLot : UserControl
    {
        #region Declaration

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public UserControl UcParentControl;

        public DataTable DtEquipment { get; set; }

        public string EquipmentSegmentCode { get; set; }
        public string ProcessCode { get; set; }
        public string ProcessName { get; set; }
        public string EquipmentCode { get; set; }

        public C1DataGrid DgProductLot { get; set; }

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        private string _clickLotID = string.Empty;
        DataTable _dProductLot;


        public UcFCSProductLot()
        {
            InitializeComponent();

            InitializeControls();
            SetControl();
            SetButtons();
            //SetControlVisibility();
        }

        #endregion

        #region Initialize

        public void InitializeControls()
        {
            Util.gridClear(dgProductLot);
        }

        private void SetControl()
        {
            DgProductLot = dgProductLot;
        }
        private void SetButtons()
        {
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    string WipstatImages = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT_IMAGES"));
                    string Lot = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTID"));
                    string Wipstat = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT"));

                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Column.Name.ToString() == "WIPSTAT_IMAGES")
                    {
                        e.Cell.Presenter.FontSize = 16;

                        if (Wipstat == "WAIT")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Yellow);
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Green);
                        }

                        e.Cell.Presenter.Cursor = Cursors.Arrow;
                    }
                    else if (e.Cell.Column.Name.ToString() == "LOTID")
                    {
                        //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        //e.Cell.Presenter.Cursor = Cursors.Hand;
                    }

                    if (Wipstat == "EQPT_END" || Wipstat == "END")
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFBEF0F8"));
                    }
                    else
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    }
                }
            }));

        }

        private void dgProductLot_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgProductLot_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgProductLot.GetCellFromPoint(pnt);

            if (cell == null) return;

            // 선택한 셀의 Row 위치
            int rowIdx = cell.Row.Index;
            DataRowView dv = DgProductLot.Rows[rowIdx].DataItem as DataRowView;

            if (dv == null) return;

            _clickLotID = dv["LOTID"].ToString();
        }

        #endregion

        #region Mehod

        #region [외부 호출]
        public void SetApplyPermissions()
        {
            // 추가작성 필요~~~~~~~~~
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        public void SelectProductList(string LotID = null)
        {
            GetProductList(LotID);
        }

        #endregion

        #region [BizCall]

        private void GetProductList(string LotID = null)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTLIST", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["PROCID"] = ProcessCode;
                newRow["EQPTLIST"] = EquipmentCode;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_FCS_G_DRB", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        _dProductLot = bizResult.Copy();
                        Util.GridSetData(dgProductLot, bizResult, null, true);
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

        #region [Func]
        protected virtual void SetUserControlProductLotSelect(RadioButton rb)
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("SetProductLotSelect");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                parameterArrys[0] = rb;

                methodInfo.Invoke(UcParentControl, parameterArrys);
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
