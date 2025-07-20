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
    public partial class UcFormProductionPalette
    {
        #region Declaration

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public UserControl UcParentControl;

        public C1DataGrid DgProductionPallet { get; set; }
        public C1DataGrid DgProductionShipPallet { get; set; }

        public Button ButtonPalletHold { get; set; }
        public Button ButtonInboxCreate { get; set; }
        public Button ButtonGoodPalletCreate { get; set; }
        public Button ButtonDefectPalletCreate { get; set; }
        public Button ButtonPalletEdit { get; set; }
        public Button ButtonPalletDelete { get; set; }
        public Button ButtonTagPrint { get; set; }
        public Button ButtonChangeProdLot { get; set; }

        public string ProdLotId { get; set; }

        public string ProcessCode { get; set; }

        public string EquipmentCode { get; set; }
        public string WipSeq { get; set; }
        public bool IsInboxCreate { get; set; }

        private readonly BizDataSet _bizDataSet = new BizDataSet();

        private readonly Util _util = new Util();

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        CheckBox chkAllShipPallet = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        public UcFormProductionPalette()
        {
            InitializeComponent();
            SetControl();
            SetButtons();
        }

        #endregion

        #region Initialize

        private void SetControl()
        {
            DgProductionPallet = dgProductionPallet;
            DgProductionShipPallet = dgProductionShipPallet;
        }
        private void SetButtons()
        {
            ButtonPalletHold = btnPalletHold;
            ButtonInboxCreate = btnInboxCreate;
            ButtonGoodPalletCreate = btnGoodPalletCreate;
            ButtonDefectPalletCreate = btnDefectPalletCreate;
            ButtonPalletEdit = btnPalletEdit;
            ButtonPalletDelete = btnPalletDelete;
            ButtonTagPrint = btnTagPrint;

            ButtonChangeProdLot = btnChangeProdLot;
        }

        public void InitializeControls()
        {
            chkAll.IsChecked = false;
            chkAllShipPallet.IsChecked = false;

            Util.gridClear(dgProductionPallet);
            Util.gridClear(dgPalletSummary);
            Util.gridClear(dgProductionShipPallet);

            if (LoginInfo.CFG_SHOP_ID.Equals("A010") && ProcessCode.Equals(Process.CircularCharacteristicGrader))
            {
                tiShipPallet.Visibility = Visibility.Visible;
            }
            else
            {
                tiShipPallet.Visibility = Visibility.Collapsed;
            }

        }

        #endregion

        #region Event

        private void dgProductionPallet_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            //try
            //{
            //    this.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        C1DataGrid dg = sender as C1DataGrid;
            //        CheckBox chk = e.Cell?.Presenter?.Content as CheckBox;
            //        if (chk != null)
            //        {
            //            switch (Convert.ToString(e.Cell.Column.Name))
            //            {
            //                case "CHK":
            //                    if (dg != null)
            //                    {
            //                        var checkBox = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
            //                        if (checkBox != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
            //                                                 dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
            //                                                 checkBox.IsChecked.HasValue &&
            //                                                 !(bool)checkBox.IsChecked))
            //                        {
            //                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
            //                            chk.IsChecked = true;

            //                            for (int idx = 0; idx < dg.Rows.Count; idx++)
            //                            {
            //                                if (e.Cell.Row.Index != idx)
            //                                {
            //                                    if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
            //                                        dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
            //                                        (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
            //                                    {
            //                                        var box = dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox;
            //                                        if (box != null)
            //                                            box.IsChecked = false;
            //                                    }
            //                                    DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
            //                                }
            //                            }
            //                        }
            //                        else
            //                        {
            //                            var o = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
            //                            if (o != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
            //                                              dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
            //                                              o.IsChecked.HasValue &&
            //                                              (bool)o.IsChecked))
            //                            {
            //                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
            //                                chk.IsChecked = false;
            //                            }
            //                        }
            //                    }
            //                    break;
            //            }

            //            if (dg?.CurrentCell != null)
            //                dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
            //            else if (dg != null && (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null))
            //                dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
            //        }

            //    }));
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private void dgProductionPallet_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    var convertFromString = ColorConverter.ConvertFromString("#E8F7C8");

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD")).Equals("Y"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }

                    if (string.Equals(ProcessCode, Process.CircularCharacteristic))
                    {
                        // 원형 특성인 경우 출고에 따른 색상 표시
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblReady.Tag))
                        {
                            e.Cell.Presenter.Background = lblReady.Background;
                        }
                        else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblShipping.Tag))
                        {
                            e.Cell.Presenter.Background = lblShipping.Background;
                        }
                        else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblShipped.Tag))
                        {
                            e.Cell.Presenter.Background = lblShipped.Background;
                        }
                        else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblCancel.Tag))
                        {
                            e.Cell.Presenter.Background = lblCancel.Background;
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                        }

                    }
                    else
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("Y"))
                        {
                            if (convertFromString != null)
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                        }
                    }

                }
            }));
        }

        private void dgProductionPallet_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
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

        private void dgProductionPallet_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));

        }

        private void dgProductionPallet_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgProductionPallet.GetCellFromPoint(pnt);

            if (cell != null)
            {
                DataTable dt = new DataTable();
                dt = DataTableConverter.Convert(dgProductionPallet.ItemsSource);

                //if (!cell.Column.Name.Equals("CHK"))
                //{
                if (dt.Rows[cell.Row.Index]["CHK"].Equals(1))
                    dt.Rows[cell.Row.Index]["CHK"] = 0;
                else
                    dt.Rows[cell.Row.Index]["CHK"] = 1;
                //}

                dt.AcceptChanges();
                dgProductionPallet.ItemsSource = dt.AsDataView();

                DataRow[] drUnchk = dt.Select("CHK = 0");

                if (drUnchk.Length == 0)
                {
                    chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                    chkAll.IsChecked = true;
                    chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                }
                else
                {
                    chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                    chkAll.IsChecked = false;
                    chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                }

            }

        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgProductionPallet.ItemsSource == null) return;

            DataTable dt = ((DataView)dgProductionPallet.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgProductionPallet.ItemsSource == null) return;

            DataTable dt = ((DataView)dgProductionPallet.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();
        }

        #endregion

        #region Mehod

        public void SetButtonVisibility()
        {
            // Inbox 등록 버튼
            if (!IsInboxCreate.Equals(true))
            {
                btnInboxCreate.Visibility = Visibility.Collapsed;
            }

            // 전압등급
            if (string.Equals(ProcessCode, Process.CircularGrader))
            {
                dgProductionPallet.Columns["VLTG_GRD_CODE"].Visibility = Visibility.Collapsed;
                dgPalletSummary.Columns["VLTG_GRD_CODE"].Visibility = Visibility.Collapsed;
            }

            // 출하처
            // [C20181121_50523] 오창 && 특성/Grader 인 경우도 보여야 함
            if (!string.Equals(ProcessCode, Process.CircularCharacteristic) 
                && !string.Equals(ProcessCode, Process.SmallPacking) 
                && !(string.Equals(ProcessCode, Process.CircularCharacteristicGrader) && LoginInfo.CFG_SHOP_ID.Equals("A010")))
            {
                dgProductionPallet.Columns["SHIPTO_NAME"].Visibility = Visibility.Collapsed;
                dgProductionPallet.Columns["SHIPTO_NOTE"].Visibility = Visibility.Collapsed;
                dgProductionPallet.Columns["RCV_ISS_STAT_NAME"].Visibility = Visibility.Collapsed;
                this.grdColor.Visibility = Visibility.Collapsed;
            }

            if (string.Equals(ProcessCode, Process.SmallPacking))
            {
                dgProductionPallet.Columns["SHIPTO_NAME"].Visibility = Visibility.Collapsed;
                dgProductionPallet.Columns["SHIPTO_NOTE"].Visibility = Visibility.Collapsed;
            }

            // 저항등급
            if (string.Equals(ProcessCode, Process.CircularCharacteristicGrader) || string.Equals(ProcessCode, Process.CircularReTubing))
            {
                dgProductionPallet.Columns["RSST_GRD_CODE"].Visibility = Visibility.Visible;
                dgPalletSummary.Columns["RSST_GRD_CODE"].Visibility = Visibility.Visible;
            }
            else
            {
                dgProductionPallet.Columns["RSST_GRD_CODE"].Visibility = Visibility.Collapsed;
                dgPalletSummary.Columns["RSST_GRD_CODE"].Visibility = Visibility.Collapsed;
            }

        }

        public void SetControlHeader()
        {
            if (string.Equals(ProcessCode, Process.SmallOcv) || string.Equals(ProcessCode, Process.SmallLeak) || string.Equals(ProcessCode, Process.SmallDoubleTab))
            {
                // 초소형 OCV 검사, 초소형 누액검사, 초소형 더블탭
                tiPallet.Header = ObjectDic.Instance.GetObjectName("완성 대차");
                btnGoodPalletCreate.Content = ObjectDic.Instance.GetObjectName("대차 생성");
                btnPalletEdit.Content = ObjectDic.Instance.GetObjectName("대차 수정");
                btnPalletDelete.Content = ObjectDic.Instance.GetObjectName("대차 삭제");

                dgProductionPallet.Columns["PALLETE_ID"].Header = ObjectDic.Instance.GetObjectName("대차 ID");
                dgPalletSummary.Columns["PALLET_QTY"].Header = ObjectDic.Instance.GetObjectName("대차 수량");
            }
            else
            {
                tiPallet.Header = ObjectDic.Instance.GetObjectName("완성 Pallet");
                btnGoodPalletCreate.Content = ObjectDic.Instance.GetObjectName("Pallet 생성");
                btnPalletEdit.Content = ObjectDic.Instance.GetObjectName("Pallet 수정");
                btnPalletDelete.Content = ObjectDic.Instance.GetObjectName("Pallet 삭제");

                dgProductionPallet.Columns["PALLETE_ID"].Header = ObjectDic.Instance.GetObjectName("Pallet ID");
                dgPalletSummary.Columns["PALLET_QTY"].Header = ObjectDic.Instance.GetObjectName("Pallet 수량");
            }

        }


        public DataRow GetSelectPalletLotRow()
        {
            DataRow row;

            try
            {
                DataRow[] dr = DataTableConverter.Convert(dgProductionPallet.ItemsSource).Select("CHK = 1");

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
            try
            {
                chkAll.IsChecked = false;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = ProdLotId;
                newRow["WIPSEQ"] = WipSeq;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_FO", "INDATA", "OUTDATA", inTable);

                //dgProductionPallet.CurrentCellChanged -= dgProductionPallet_CurrentCellChanged;

                Util.GridSetData(dgProductionPallet, dtResult, FrameOperation, true);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dgProductionPallet.CurrentCell = dgProductionPallet.GetCell(0, 1);

                //dgProductionPallet.CurrentCellChanged += dgProductionPallet_CurrentCellChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        public void SelectPalletSummary()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = ProdLotId;
                newRow["WIPSEQ"] = WipSeq;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_SUM_FO", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgPalletSummary, dtResult, null);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #endregion

        public void SelectShipPalletList()
        {
            try
            {
                chkAllShipPallet.IsChecked = false;

                Util.gridClear(dgProductionShipPallet);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));
                
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PR_LOTID"] = ProdLotId;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_AUTOBOXING_LIST", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgProductionShipPallet, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void dgProductionShipPallet_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgProductionShipPallet_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == C1.WPF.DataGrid.DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXSTAT_LIST")).Equals(lblShipCreated.Tag))
                    {
                        e.Cell.Presenter.Background = lblShipCreated.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXSTAT_LIST")).Equals(lblShipPacked.Tag))
                    {
                        e.Cell.Presenter.Background = lblShipPacked.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXSTAT_LIST")).Equals(lblShipShipping.Tag))
                    {
                        e.Cell.Presenter.Background = lblShipShipping.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXSTAT_LIST")).Equals(lblShipShipped.Tag))
                    {
                        e.Cell.Presenter.Background = lblShipShipped.Background;
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgProductionShipPallet_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgProductionShipPallet_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgProductionShipPallet.GetCellFromPoint(pnt);

            if (cell != null)
            {
                DataTable dt = new DataTable();
                dt = DataTableConverter.Convert(dgProductionShipPallet.ItemsSource);

                if (dt.Rows[cell.Row.Index]["CHK"].Equals(1) || dt.Rows[cell.Row.Index]["CHK"].ToString().Equals("True") || dt.Rows[cell.Row.Index]["CHK"].ToString().Equals("true"))
                {
                    dt.Rows[cell.Row.Index]["CHK"] = 0;
                }
                else
                {
                    dt.Rows[cell.Row.Index]["CHK"] = 1;
                }

                dt.AcceptChanges();
                dgProductionShipPallet.ItemsSource = dt.AsDataView();

                DataRow[] drUnchk = dt.Select("CHK = 0");

                if (drUnchk.Length == 0)
                {
                    chkAllShipPallet.Checked -= new RoutedEventHandler(chkAllShipPallet_Checked);
                    chkAllShipPallet.IsChecked = true;
                    chkAllShipPallet.Checked += new RoutedEventHandler(chkAllShipPallet_Checked);
                }
                else
                {
                    chkAllShipPallet.Unchecked -= new RoutedEventHandler(chkAllShipPallet_Unchecked);
                    chkAllShipPallet.IsChecked = false;
                    chkAllShipPallet.Unchecked += new RoutedEventHandler(chkAllShipPallet_Unchecked);
                }

            }
        }

        private void dgProductionShipPallet_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAllShipPallet;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAllShipPallet.Checked -= new RoutedEventHandler(chkAllShipPallet_Checked);
                        chkAllShipPallet.Unchecked -= new RoutedEventHandler(chkAllShipPallet_Unchecked);
                        chkAllShipPallet.Checked += new RoutedEventHandler(chkAllShipPallet_Checked);
                        chkAllShipPallet.Unchecked += new RoutedEventHandler(chkAllShipPallet_Unchecked);
                    }
                }
            }));
        }

        private void chkAllShipPallet_Checked(object sender, RoutedEventArgs e)
        {
            if (dgProductionShipPallet.ItemsSource == null) return;

            DataTable dt = ((DataView)dgProductionShipPallet.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();
        }
        private void chkAllShipPallet_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgProductionShipPallet.ItemsSource == null) return;

            DataTable dt = ((DataView)dgProductionShipPallet.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();
        }


    }
}
