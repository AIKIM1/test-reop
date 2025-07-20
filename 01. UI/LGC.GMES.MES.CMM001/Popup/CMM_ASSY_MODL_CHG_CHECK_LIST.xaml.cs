using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Printing;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_ASSY_MODL_CHG_CHECK_LIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _Proc = string.Empty;
        private string _Line = string.Empty;
        private string _Eqpt = string.Empty;
        private string _LotId = string.Empty;
        private string _WipSeq = string.Empty;
        private string _WO_DETL_ID = string.Empty;
        private bool bLoad = false;
        private readonly Util _util = new Util();
        #endregion

        #region Initialize        
        public CMM_ASSY_MODL_CHG_CHECK_LIST()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 6)
            {
                _Line = Util.NVC(tmps[0]);
                _Eqpt = Util.NVC(tmps[1]);
                _Proc = Util.NVC(tmps[2]);
                _LotId = Util.NVC(tmps[3]);
                _WipSeq = Util.NVC(tmps[4]);
                _WO_DETL_ID = Util.NVC(tmps[5]);
            }

            CheckModelChangeFirstLot();

            GetBasicInfo(dg: dgQualityInfo);

            bLoad = true;
        }

        private void CheckModelChangeFirstLot()
        {
            try
            {
                DataTable dtRqstAdd = new DataTable();

                dtRqstAdd.Columns.Add("LOTID", typeof(string));
                dtRqstAdd.Columns.Add("EQPTID", typeof(string));

                DataRow drAdd = dtRqstAdd.NewRow();
                drAdd["LOTID"] = _LotId;
                drAdd["EQPTID"] = _Eqpt;

                dtRqstAdd.Rows.Add(drAdd);

                new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_MODL_CHG_WN_LOT", "INDATA", null, dtRqstAdd);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.DialogResult = MessageBoxResult.Cancel;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnQualitySave_Click(object sender, RoutedEventArgs e)
        {
            if (dgQualityInfo == null || dgQualityInfo.Rows.Count < 1) return;

            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveQuality(dgQualityInfo);
                }
            });
        }

        private void CLCTVAL_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                int rIdx = 0;
                int cIdx = 0;
                C1.WPF.DataGrid.C1DataGrid grid;

                if (sender.GetType().Name == "C1NumericBox")
                {
                    if (InputMethod.Current != null)
                        InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                    C1NumericBox n = sender as C1NumericBox;
                    StackPanel panel = n.Parent as StackPanel;
                    C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    rIdx = p.Cell.Row.Index;
                    cIdx = p.Cell.Column.Index;
                    grid = p.DataGrid;
                }

                else if (sender.GetType().Name == "ComboBox")
                {
                    ComboBox n = sender as ComboBox;
                    StackPanel panel = n.Parent as StackPanel;
                    C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    rIdx = p.Cell.Row.Index;
                    cIdx = p.Cell.Column.Index;
                    grid = p.DataGrid;
                }
                else
                    return;


                if (grid.GetRowCount() > ++rIdx)
                {
                    if (grid.GetRowCount() - 1 != rIdx)
                    {
                        grid.ScrollIntoView(rIdx + 1, cIdx);
                    }

                    C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;
                    if (p == null) return;
                    StackPanel panel = p.Content as StackPanel;

                    for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                    {
                        if (panel.Children[cnt].Visibility == Visibility.Visible)
                            panel.Children[cnt].Focus();
                    }
                }
                else if (CommonVerify.HasDataGridRow(grid) && grid.GetRowCount() == rIdx)
                {
                    return;
                    //btnQualitySave.Focus();
                }
            }
        }

        private void dgQualityInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.ToUpper().IndexOf("CLCTVAL01", StringComparison.Ordinal) >= 0)
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgQualityInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void CLCTVAL_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Down)
                {
                    int rIdx = 0;
                    int cIdx = 0;
                    C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        if (InputMethod.Current != null)
                            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                        C1NumericBox n = sender as C1NumericBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }

                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox n = sender as ComboBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else
                        return;

                    if (grid.GetRowCount() > ++rIdx)
                    {
                        if (grid.GetRowCount() - 1 != rIdx)
                        {
                            grid.ScrollIntoView(rIdx + 1, cIdx);
                        }

                        DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;
                        if (p == null) return;
                        StackPanel panel = p.Content as StackPanel;

                        for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                        {
                            if (panel.Children[cnt].Visibility == Visibility.Visible)
                                panel.Children[cnt].Focus();
                        }
                    }

                    e.Handled = true;
                }
                else if (e.Key == Key.Up)
                {
                    int rIdx = 0;
                    int cIdx = 0;
                    C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        if (InputMethod.Current != null)
                            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                        C1NumericBox n = sender as C1NumericBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }

                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox n = sender as ComboBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else
                        return;

                    if (grid.GetRowCount() > --rIdx)
                    {
                        if (rIdx < 0)
                        {
                            e.Handled = true;
                            return;
                        }

                        //if (grid.GetRowCount() - 1 != rIdx)
                        if (rIdx > 0)
                        {
                            grid.ScrollIntoView(rIdx - 1, cIdx);
                        }

                        DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;
                        if (p == null) return;
                        StackPanel panel = p.Content as StackPanel;

                        for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                        {
                            if (panel.Children[cnt].Visibility == Visibility.Visible)
                                panel.Children[cnt].Focus();
                        }
                    }

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CLCTVAL_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender.GetType().Name == "C1NumericBox")
                {
                    if (InputMethod.Current != null)
                        InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                    C1NumericBox n = sender as C1NumericBox;
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF");
                    if (convertFromString != null)
                        n.Background = new SolidColorBrush((Color)convertFromString);
                }
                else if (sender.GetType().Name == "ComboBox")
                {
                    ComboBox n = sender as ComboBox;
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF");
                    if (convertFromString != null)
                        n.Background = new SolidColorBrush((Color)convertFromString);
                }
                else
                    return;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CLCTVAL_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView drv = ((FrameworkElement)sender).DataContext as DataRowView;
                if (drv == null) return;

                if (sender.GetType().Name == "C1NumericBox")
                {
                    C1NumericBox n = sender as C1NumericBox;
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0");
                    if (convertFromString != null)
                        if (n != null) n.Background = new SolidColorBrush((Color)convertFromString);

                }
                else if (sender.GetType().Name == "ComboBox")
                {
                    ComboBox n = sender as ComboBox;
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0");
                    if (convertFromString != null)
                        n.Background = new SolidColorBrush((Color)convertFromString);
                }

                else
                    return;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod
        #region Biz
        private void GetBasicInfo(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqstAdd = new DataTable();
                dtRqstAdd.Columns.Add("LANGID", typeof(string));
                dtRqstAdd.Columns.Add("AREAID", typeof(string));
                dtRqstAdd.Columns.Add("PROCID", typeof(string));
                dtRqstAdd.Columns.Add("LOTID", typeof(string));
                dtRqstAdd.Columns.Add("WIPSEQ", typeof(string));
                dtRqstAdd.Columns.Add("EQPTID", typeof(string));
                dtRqstAdd.Columns.Add("CLCT_BAS_CODE", typeof(string));
                dtRqstAdd.Columns.Add("CLCT_ITVL", typeof(string));

                DataRow drAdd = dtRqstAdd.NewRow();
                drAdd["LANGID"] = LoginInfo.LANGID;
                drAdd["AREAID"] = LoginInfo.CFG_AREA_ID;
                drAdd["PROCID"] = _Proc;
                drAdd["LOTID"] = _LotId;
                drAdd["WIPSEQ"] = _WipSeq;
                drAdd["EQPTID"] = _Eqpt;
                drAdd["CLCT_BAS_CODE"] = "Modl_Chg";

                dtRqstAdd.Rows.Add(drAdd);

                DataTable dtRsltAdd = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_CLCTITEM_MODL_CHG", "INDATA", "OUTDATA", dtRqstAdd);

                Util.gridClear(dg);

                Util.GridSetData(dgQualityInfo, dtRsltAdd, FrameOperation, true);

                _util.SetDataGridMergeExtensionCol(dgQualityInfo, new string[] { "CLSS_NAME1", "CLSS_NAME2", "CLSS_NAME3" }, DataGridMergeMode.VERTICALHIERARCHI);

                string sNote = "";

                if (dtRqstAdd.Rows.Count > 0)
                    sNote = dtRsltAdd.Rows[0]["NOTE"].ToString();

                AddNoteGrid(dgdNote, sNote);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void AddNoteGrid(C1DataGrid dgd, string sNote)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("TITLE", typeof(string));
            dt.Columns.Add("NOTE", typeof(string));

            DataRow dr = dt.NewRow();

            string title = "비고";
            dr["TITLE"] = ObjectDic.Instance.GetObjectName(title);
            dr["NOTE"] = sNote;
            dt.Rows.Add(dr);

            Util.GridSetData(dgd, dt, FrameOperation, false);
        }

        private DateTime GetDBDateTime()
        {
            try
            {
                DateTime tmpDttm = DateTime.Now;
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_GETDATE", null, "OUTDATA", null);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    tmpDttm = (DateTime)dtRslt.Rows[0]["DATE"];
                }

                return tmpDttm;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                return DateTime.Now;
            }
        }

        private void SaveQuality(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SRCTYPE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(Int16));
                dtRqst.Columns.Add("CLCTSEQ", typeof(Int16));
                dtRqst.Columns.Add("CLCTITEM", typeof(string));
                dtRqst.Columns.Add("CLCTVAL01", typeof(string));
                dtRqst.Columns.Add("CLCTMAX", typeof(string));
                dtRqst.Columns.Add("CLCTMIN", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("CLCTSEQ_ORG", typeof(Int16));
                dtRqst.Columns.Add("NOTE", typeof(string));

                dg.EndEdit();

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows) // DataGridHandler.GetAddedItems(dg))
                {
                    DataRowView drvTmp = (row.DataItem as DataRowView);
                    DataRow dr = dtRqst.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["LOTID"] = _LotId;
                    dr["WIPSEQ"] = _WipSeq;
                    dr["CLCTSEQ"] = 1;// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");
                    dr["CLCTITEM"] = drvTmp["CLCTITEM"];// DataTableConverter.GetValue(row.DataItem, "CLCTITEM");
                    dr["CLCTVAL01"] = (!drvTmp["CLCTVAL01"].Equals(DBNull.Value) && !drvTmp["CLCTVAL01"].Equals("-")) ? drvTmp["CLCTVAL01"].ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "") : "";// DataTableConverter.GetValue(row.DataItem, "CLCTVAL01");

                    if (Util.NVC(dr["CLCTVAL01"]).Length < 1)
                    {
                        object[] parameters = new object[1];
                        parameters[0] = Util.NVC(drvTmp["CLSS_NAME1"]) + (Util.NVC(drvTmp["CLSS_NAME2"]).Length > 0 ? " - " + Util.NVC(drvTmp["CLSS_NAME2"]) : string.Empty);
                        Util.MessageInfo("SFU8346", parameters); // 형교환 체크시트 항목[%1]이 입력되지 않았습니다.

                        return;
                    }
                    else
                    {
                        if(Util.NVC(dr["CLCTVAL01"]).ToString() == "NG")
                        {
                           object[] parameters = new object[1];
                            parameters[0] = Util.NVC(drvTmp["CLSS_NAME1"]) + (Util.NVC(drvTmp["CLSS_NAME2"]).Length > 0 ? " - " + Util.NVC(drvTmp["CLSS_NAME2"]) : string.Empty);
                            Util.MessageInfo("SFU8349", parameters); // [%1]의 값이 NG입니다.

                            return;
                        }

                        if (!string.IsNullOrEmpty(Util.NVC(drvTmp["USL"]).ToString()))
                        {
                            if(Int32.Parse(dr["CLCTVAL01"].ToString()) > Int32.Parse(Util.NVC(drvTmp["USL"]).ToString()))
                            {
                                object[] parameters = new object[1];
                                parameters[0] = Util.NVC(drvTmp["CLSS_NAME1"]) + (Util.NVC(drvTmp["CLSS_NAME2"]).Length > 0 ? " - " + Util.NVC(drvTmp["CLSS_NAME2"]) : string.Empty);
                                Util.MessageInfo("SFU8350", parameters); // [%1] 항목이 SPEC 값을 벗어났습니다.

                                return;
                            }
                        }

                        if (!string.IsNullOrEmpty(Util.NVC(drvTmp["LSL"]).ToString()))
                        {
                            if (Int32.Parse(dr["CLCTVAL01"].ToString()) < Int32.Parse(Util.NVC(drvTmp["LSL"]).ToString()))
                            {
                                object[] parameters = new object[1];
                                parameters[0] = Util.NVC(drvTmp["CLSS_NAME1"]) + (Util.NVC(drvTmp["CLSS_NAME2"]).Length > 0 ? " - " + Util.NVC(drvTmp["CLSS_NAME2"]) : string.Empty);
                                Util.MessageInfo("SFU8350", parameters);  // [%1] 항목이 SPEC 값을 벗어났습니다.

                                return;
                            }
                        }


                    }
                    //dr["CLCTMAX"] = drvTmp["USL"];// DataTableConverter.GetValue(row.DataItem, "USL");
                    //dr["CLCTMIN"] = drvTmp["LSL"];// DataTableConverter.GetValue(row.DataItem, "LSL");
                    dr["EQPTID"] = _Eqpt;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["NOTE"] = DataTableConverter.GetValue(dgdNote.Rows[0].DataItem, "NOTE").GetString();
                    dtRqst.Rows.Add(dr);
                }

                if (dtRqst.Rows.Count > 0)
                {
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, dtRqst);

                    Util.MessageInfo("SFU1270");      //저장되었습니다.
                }
                else
                {
                    Util.MessageInfo("SFU1566");      //변경된데이타가없습니다.
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }


        #endregion

        #region Func

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


        #endregion

        #endregion

        private void cbVal_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox cbo = sender as ComboBox;
            if (cbo.IsVisible)
                cbo.SelectedIndex = 0;
        }

        private void dgQualityInfo_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            dgQualityInfo.Focus();
        }
        public static FixedDocument GetFixedDocument(FrameworkElement toPrint, PrintDialog printDialog, Thickness margin)
        {
            System.Printing.PrintCapabilities capabilities = printDialog.PrintQueue.GetPrintCapabilities(printDialog.PrintTicket);
            Size pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
            Size visibleSize = new Size(capabilities.PageImageableArea.ExtentWidth - margin.Left - margin.Right, capabilities.PageImageableArea.ExtentHeight - margin.Top - margin.Bottom);
            FixedDocument fixedDoc = new FixedDocument();
            //If the toPrint visual is not displayed on screen we neeed to measure and arrange it   
            //toPrint.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            //toPrint.Arrange(new Rect(new Point(0, 0), toPrint.DesiredSize));
            toPrint.Measure(new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight));
            toPrint.Arrange(new Rect(new Size(toPrint.ActualWidth, toPrint.ActualHeight)));

            //   
            Size size = toPrint.DesiredSize;
            //Will assume for simplicity the control fits horizontally on the page   
            double yOffset = 0;
            while (yOffset < size.Height)
            {
                VisualBrush vb = new VisualBrush(toPrint);
                vb.Stretch = Stretch.None;
                vb.AlignmentX = AlignmentX.Left;
                vb.AlignmentY = AlignmentY.Top;
                vb.ViewboxUnits = BrushMappingMode.Absolute;
                vb.TileMode = TileMode.None;
                vb.Viewbox = new Rect(0, yOffset, visibleSize.Width, visibleSize.Height);

                PageContent pageContent = new PageContent();
                FixedPage page = new FixedPage();
                ((System.Windows.Markup.IAddChild)pageContent).AddChild(page);
                fixedDoc.Pages.Add(pageContent);
                page.Width = pageSize.Width;
                page.Height = pageSize.Height;
                Canvas canvas = new Canvas();
                FixedPage.SetLeft(canvas, capabilities.PageImageableArea.OriginWidth);
                FixedPage.SetTop(canvas, capabilities.PageImageableArea.OriginHeight);
                canvas.Width = visibleSize.Width;
                canvas.Height = visibleSize.Height;
                canvas.Background = vb;
                canvas.Margin = margin;

                page.Children.Add(canvas);
                yOffset += visibleSize.Height;
            }
            return fixedDoc;
        }


        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {

            //C1DataGrid tmp = this.dgQualityInfo + this.dgdNote;

            //tmp.Print("GMES Model Change Check List", ScaleMode.SinglePage, new Thickness(5,5, 5, 5), 1);
        }
    }
}
