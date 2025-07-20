/*************************************************************************************
 Created Date : 2024.05.08
      Creator : 김동일
   Decription : AZS-PKG 순환 CNV 재공관리
--------------------------------------------------------------------------------------
 [Change History]
  2024.05.08  김동일      : Initial Created.    [E20240205-000271]
  2024.08.29  안유수      E20240719-001569 dgConveyorDetail Grid에 경과시간 컬럼 추가, Tray 수량관리 Grid위에 MAX Capa값 표기하도록 화면 수정
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_093.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_093 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private readonly Util _util = new Util();
        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public MCS001_093()
        {
            InitializeComponent();
        }

        private DataTable MakeEmptyTrayTable()
        {
            DataTable _dtTable = new DataTable();
            _dtTable.Columns.Add("EMPTY_NORMAL", typeof(decimal));
            _dtTable.Columns.Add("EMPTY_MAX_TRF_QTY", typeof(decimal));
            _dtTable.Columns.Add("EMPTY_INPUT_RATIO", typeof(decimal));

            return _dtTable;
        }

        #endregion

        #region Event

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> { btnSearch };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            //InitializeControl();
            TimerSetting();
            Loaded -= UserControl_Loaded;
            _isLoaded = true;
            //Util.GridAllColumnWidthAuto(ref dgConveyor);

            //SelectTrayMaxEmptyTrfQty();
            InitializeConverGridControl();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();
            //SelectTrayMaxEmptyTrfQty();
            SelectConveyorSummary();
        }

        private void cboTimer_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!_isLoaded) return;

            try
            {
                if (_monitorTimer != null)
                {
                    _monitorTimer.Stop();

                    int second = 0;
                    if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.GetString()))
                    {
                        second = int.Parse(cboTimer.SelectedValue.ToString());
                        _isSelectedAutoTime = true;
                    }
                    else
                    {
                        _isSelectedAutoTime = false;
                    }

                    if (second == 0 && !_isSelectedAutoTime)
                    {
                        Util.MessageValidation("SFU8170");
                        return;
                    }
                    _monitorTimer.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer.Start();

                    if (_isSelectedAutoTime)
                    {
                        //자동조회  %1초로 변경 되었습니다.
                        if (cboTimer != null)
                            Util.MessageInfo("SFU5127", cboTimer.SelectedValue.GetString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void dgConveyor_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PJT")), ObjectDic.Instance.GetObjectName("TOTAL")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        if (string.Equals(e.Cell.Column.Name, "NORMAL") || string.Equals(e.Cell.Column.Name, "MESHOLD") || string.Equals(e.Cell.Column.Name, "QMSHOLD") ||
                            string.Equals(e.Cell.Column.Name, "EMPTY_NORMAL"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else if (string.Equals(e.Cell.Column.Name, "MAX_TRF_QTY") || string.Equals(e.Cell.Column.Name, "EMPTY_MAX_TRF_QTY"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }

                        double iRatio = 0;
                        if (string.Equals(e.Cell.Column.Name, "INPUT_RATIO") && double.TryParse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "INPUT_RATIO")), out iRatio))
                        {
                            if (iRatio >= 90)
                            {
                                var convertString = System.Windows.Media.ColorConverter.ConvertFromString("Orange");
                                if (convertString != null)
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)convertString);
                            }
                            else
                            {
                                var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                                if (convertFromString != null)
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                            }
                        }
                        else
                        {
                            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                            if (convertFromString != null)
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                        }
                    }
                }
            }));
        }

        private void dgConveyor_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgConveyor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);
                if (cell == null) return;

                // 선택한 셀의 위치
                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                Util.gridClear(dgTrayMaxCnt);

                if (cell.Column.Name.Equals("EMPTY_NORMAL"))
                    SelectConveyorDetailEmpty();
                else if (cell.Column.Name.Equals("NORMAL") ||
                         cell.Column.Name.Equals("MESHOLD") ||
                         cell.Column.Name.Equals("QMSHOLD"))
                    SelectConveyorDetail(Util.NVC(DataTableConverter.GetValue(drv, "PJT")), cell.Column.Name.Equals("MESHOLD") ? "Y" : "N", cell.Column.Name.Equals("QMSHOLD") ? "Y" : "N");
                else if ((cell.Column.Name.Equals("MAX_TRF_QTY") ||
                         cell.Column.Name.Equals("EMPTY_MAX_TRF_QTY"))
                         && !Util.NVC(DataTableConverter.GetValue(drv, "PJT")).Equals(ObjectDic.Instance.GetObjectName("TOTAL")))
                    SelectTrayMaxCnt(cell.Column.Name.Equals("MAX_TRF_QTY") ? Util.NVC(DataTableConverter.GetValue(drv, "PJT")) : "EMPTY");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgConveyor_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg?.ItemsSource == null) return;

            try
            {
                if (dg.GetRowCount() > 0)
                {
                    int idx = 0;

                    for (int i = dg.TopRows.Count; i < dg.Rows.Count; i++)
                    {
                        if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "PJT").GetString() == ObjectDic.Instance.GetObjectName("TOTAL"))
                        {
                            idx = i;
                        }
                    }

                    if (idx > dg.TopRows.Count)
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(dg.TopRows.Count, dg.Columns["EMPTY_NORMAL"].Index), dg.GetCell(idx - 1, dg.Columns["EMPTY_NORMAL"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(dg.TopRows.Count, dg.Columns["EMPTY_MAX_TRF_QTY"].Index), dg.GetCell(idx - 1, dg.Columns["EMPTY_MAX_TRF_QTY"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(dg.TopRows.Count, dg.Columns["EMPTY_INPUT_RATIO"].Index), dg.GetCell(idx - 1, dg.Columns["EMPTY_INPUT_RATIO"].Index)));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null) return;

            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;

                    btnSearch_Click(btnSearch, null);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));
        }

        private void btnSaveTrayCnt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgTrayMaxCnt.GetRowCount() < 1) return;

                const string bizRuleName = "BR_MHS_REG_MAX_PRJ_INPUT_QTY";
                DataTable inDataTable = new DataTable("IN_DATA");
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PRJT_NAME", typeof(string));
                inDataTable.Columns.Add("MAX_TRF_QTY", typeof(Int32));
                inDataTable.Columns.Add("UPDUSER", typeof(string));

                DataRow dr = inDataTable.NewRow();
                string sPjt = Util.NVC(DataTableConverter.GetValue(dgTrayMaxCnt.Rows[0].DataItem, "PRJT_NAME"));
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PRJT_NAME"] = sPjt;
                dr["MAX_TRF_QTY"] = Util.NVC_Int(DataTableConverter.GetValue(dgTrayMaxCnt.Rows[0].DataItem, "MAX_TRF_QTY"));
                dr["UPDUSER"] = LoginInfo.USERID;

                inDataTable.Rows.Add(dr);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService(bizRuleName, "IN_DATA", "OUT_DATA", inDataTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult?.Rows?.Count > 0)
                        {
                            Util.MessageValidation(Util.NVC(bizResult.Rows[0]["RESULT_CODE"]));
                            SelectTrayMaxCnt(sPjt);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상처리되었습니다.
                        SelectTrayMaxCnt(sPjt);
                        btnSearch_Click(btnSearch, null);
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

        #region Method

        private void SelectConveyorSummary()
        {
            const string bizRuleName = "DA_MHS_SEL_AZS_PKG_CNV_SUMMARY";
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    if (bizResult.Rows.Count > 0)
                    {
                        // 합계
                        var querySum = bizResult.AsEnumerable().GroupBy(x => new
                        { }).Select(g => new
                        {
                            Code = "Z_SUM_ROW",
                            PjtName = ObjectDic.Instance.GetObjectName("TOTAL"),
                            UTNormal = g.Sum(x => x.Field<Int32?>("NORMAL")),
                            UTHoldCount = g.Sum(x => x.Field<Int32?>("MESHOLD")),
                            UTQMSHoldCount = g.Sum(x => x.Field<Int32?>("QMSHOLD")),
                            UTMaxTrfQty = g.Sum(x => x.Field<Int32?>("MAX_TRF_QTY")),
                            //UTInputRatio = g.Sum(x => x.Field<decimal?>("INPUT_RATIO")),
                            Count = g.Count()
                        }).ToList();

                        if (CommonVerify.HasTableRow(bizResult))
                        {
                            if (querySum != null && querySum.Any())
                            {
                                foreach (var item in querySum)
                                {
                                    double dbTmp1 = 0;
                                    double dbTmp2 = 0;

                                    double.TryParse((item.UTNormal + item.UTHoldCount + item.UTQMSHoldCount).ToString(), out dbTmp1);
                                    double.TryParse((item.UTMaxTrfQty).ToString(), out dbTmp2);

                                    DataRow newRow = bizResult.NewRow();
                                    newRow["PJT"] = item.PjtName;
                                    newRow["NORMAL"] = item.UTNormal;
                                    newRow["MESHOLD"] = item.UTHoldCount;
                                    newRow["QMSHOLD"] = item.UTQMSHoldCount;
                                    newRow["MAX_TRF_QTY"] = item.UTMaxTrfQty;
                                    newRow["INPUT_RATIO"] = dbTmp1 / (dbTmp2 != 0 ? dbTmp2 : 1) * 100;

                                    newRow["EMPTY_NORMAL"] = bizResult.Rows[0]["EMPTY_NORMAL"];
                                    newRow["EMPTY_MAX_TRF_QTY"] = bizResult.Rows[0]["EMPTY_MAX_TRF_QTY"];
                                    newRow["EMPTY_INPUT_RATIO"] = bizResult.Rows[0]["EMPTY_INPUT_RATIO"];

                                    bizResult.Rows.Add(newRow);
                                }
                            }
                        }
                    }

                    Util.GridSetData(dgConveyor, bizResult, FrameOperation, false);

                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectConveyorDetailEmpty()
        {
            const string bizRuleName = "DA_MHS_SEL_AZS_PKG_CNV_EMPTY_TRAY_LIST";
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    var dtBinding = bizResult.Copy();
                    dtBinding.Columns.Add(new DataColumn() { ColumnName = "SEQ", DataType = typeof(int) });
                    int rowIndex = 1;
                    foreach (DataRow row in dtBinding.Rows)
                    {
                        row["SEQ"] = rowIndex;
                        rowIndex++;
                    }
                    dtBinding.AcceptChanges();

                    Util.GridSetData(dgConveyorDetail, dtBinding, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectConveyorDetail(string sPJT, string sMesHoldFlag, string sQmsHoldFlag)
        {
            const string bizRuleName = "DA_MHS_SEL_AZS_PKG_CNV_WIP_LIST";
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("FINL_HOLD_FLAG", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("WIPHOLD", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                if (!sPJT.Equals(ObjectDic.Instance.GetObjectName("TOTAL")))
                    dr["PRJT_NAME"] = sPJT;

                if (sQmsHoldFlag.Equals("Y"))
                    dr["FINL_HOLD_FLAG"] = sQmsHoldFlag;
                if (sMesHoldFlag.Equals("Y"))
                    dr["WIPHOLD"] = sMesHoldFlag;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    var dtBinding = bizResult.Copy();
                    dtBinding.Columns.Add(new DataColumn() { ColumnName = "SEQ", DataType = typeof(int) });
                    int rowIndex = 1;
                    foreach (DataRow row in dtBinding.Rows)
                    {
                        row["SEQ"] = rowIndex;
                        rowIndex++;
                    }
                    dtBinding.AcceptChanges();

                    Util.GridSetData(dgConveyorDetail, dtBinding, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectTrayMaxCnt(string sPJT)
        {
            const string bizRuleName = "DA_MHS_SEL_MAX_PRJT_INPUT_QTY";
            try
            {
                if (grdTrayMaxCntMngt.Visibility != Visibility.Visible) return;
                ShowLoadingIndicator();
                tbMaxCapa.Text = "";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRJT_NAME"] = sPJT;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (dtRslt?.Rows?.Count < 1)
                {
                    if (dtRslt?.Columns?.Contains("PRJT_NAME") == false) dtRslt.Columns.Add("PRJT_NAME", typeof(string));
                    if (dtRslt?.Columns?.Contains("MAX_TRF_QTY") == false) dtRslt.Columns.Add("MAX_TRF_QTY", typeof(Int32));

                    DataRow drTmp = dtRslt.NewRow();
                    drTmp["PRJT_NAME"] = sPJT;
                    drTmp["MAX_TRF_QTY"] = 0;

                    dtRslt.Rows.Add(drTmp);
                }

                Util.GridSetData(dgTrayMaxCnt, dtRslt, FrameOperation, false);

                tbMaxCapa.Text = "MAX CaPa : Using(" + Util.NVC(dtRslt.Rows[0]["MAX_USING"]) +") / Empty(" + Util.NVC(dtRslt.Rows[0]["MAX_EMPTY"]) + ")";

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        //private void SelectTrayMaxEmptyTrfQty()
        //{
        //    ShowLoadingIndicator();

        //    const string bizRuleName = "DA_BAS_SEL_COMMONCODE_TBL";
        //    try
        //    {
        //        DataTable inTable = new DataTable("RQSTDT");
        //        inTable.Columns.Add("LANGID", typeof(string));
        //        inTable.Columns.Add("CMCDTYPE", typeof(string));
        //        inTable.Columns.Add("CMCODE", typeof(string));

        //        DataRow dr = inTable.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["CMCDTYPE"] = "MHS_AZS_PKG_CNV_PRJ_MAX_CST_TRF_QTY";
        //        dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

        //        inTable.Rows.Add(dr);

        //        DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

        //        if (dtRslt?.Rows?.Count > 0)
        //        {
        //            tbCnvMaxQty.Text = Util.NVC(dtRslt.Rows[0]["ATTRIBUTE1"]);
        //        }

        //        HiddenLoadingIndicator();
        //    }
        //    catch (Exception ex)
        //    {
        //        HiddenLoadingIndicator();
        //        Util.MessageException(ex);
        //    }
        //}

        private void InitializeControl()
        {
            if (LoginInfo.LANGID == "ko-KR")
            {
                ContentsRow.ColumnDefinitions[0].Width = new GridLength(2.4, GridUnitType.Star);
                ContentsRow.ColumnDefinitions[1].Width = new GridLength(8, GridUnitType.Pixel);
                ContentsRow.ColumnDefinitions[2].Width = new GridLength(7.6, GridUnitType.Star);
            }
            else if (LoginInfo.LANGID == "zh-CN")
            {
                ContentsRow.ColumnDefinitions[0].Width = new GridLength(2.4, GridUnitType.Star);
                ContentsRow.ColumnDefinitions[1].Width = new GridLength(8, GridUnitType.Pixel);
                ContentsRow.ColumnDefinitions[2].Width = new GridLength(7.6, GridUnitType.Star);
            }
            else if (LoginInfo.LANGID == "en-US")
            {
                ContentsRow.ColumnDefinitions[0].Width = new GridLength(2.8, GridUnitType.Star);
                ContentsRow.ColumnDefinitions[1].Width = new GridLength(8, GridUnitType.Pixel);
                ContentsRow.ColumnDefinitions[2].Width = new GridLength(7.2, GridUnitType.Star);
            }
            else if (LoginInfo.LANGID == "uk-UA")
            {
                ContentsRow.ColumnDefinitions[0].Width = new GridLength(2.8, GridUnitType.Star);
                ContentsRow.ColumnDefinitions[1].Width = new GridLength(8, GridUnitType.Pixel);
                ContentsRow.ColumnDefinitions[2].Width = new GridLength(7.2, GridUnitType.Star);
            }
            else if (LoginInfo.LANGID == "pl-PL")
            {
                ContentsRow.ColumnDefinitions[0].Width = new GridLength(4.25, GridUnitType.Star);
                ContentsRow.ColumnDefinitions[1].Width = new GridLength(8, GridUnitType.Pixel);
                ContentsRow.ColumnDefinitions[2].Width = new GridLength(5.75, GridUnitType.Star);
            }
            else if (LoginInfo.LANGID == "ru-RU")
            {
                ContentsRow.ColumnDefinitions[0].Width = new GridLength(3.85, GridUnitType.Star);
                ContentsRow.ColumnDefinitions[1].Width = new GridLength(8, GridUnitType.Pixel);
                ContentsRow.ColumnDefinitions[2].Width = new GridLength(6.15, GridUnitType.Star);
            }
        }

        private void InitializeConverGridControl()
        {
            try
            {
                if (dgConveyor?.Columns?.Count < 1) return;

                if (LoginInfo.LANGID == "id-ID")
                {
                    if (dgConveyor?.Columns?.Contains("MESHOLD") == true) dgConveyor.Columns["MESHOLD"].Width = new C1.WPF.DataGrid.DataGridLength(90, DataGridUnitType.Pixel);
                    if (dgConveyor?.Columns?.Contains("QMSHOLD") == true) dgConveyor.Columns["QMSHOLD"].Width = new C1.WPF.DataGrid.DataGridLength(90, DataGridUnitType.Pixel);
                    if (dgConveyor?.Columns?.Contains("INPUT_RATIO") == true) dgConveyor.Columns["INPUT_RATIO"].Width = new C1.WPF.DataGrid.DataGridLength(110, DataGridUnitType.Pixel);
                    if (dgConveyor?.Columns?.Contains("EMPTY_INPUT_RATIO") == true) dgConveyor.Columns["EMPTY_INPUT_RATIO"].Width = new C1.WPF.DataGrid.DataGridLength(110, DataGridUnitType.Pixel);
                }
                else
                {
                    if (dgConveyor?.Columns?.Contains("MESHOLD") == true) dgConveyor.Columns["MESHOLD"].Width = new C1.WPF.DataGrid.DataGridLength(70, DataGridUnitType.Pixel);
                    if (dgConveyor?.Columns?.Contains("QMSHOLD") == true) dgConveyor.Columns["QMSHOLD"].Width = new C1.WPF.DataGrid.DataGridLength(70, DataGridUnitType.Pixel);
                    if (dgConveyor?.Columns?.Contains("INPUT_RATIO") == true) dgConveyor.Columns["INPUT_RATIO"].Width = new C1.WPF.DataGrid.DataGridLength(80, DataGridUnitType.Pixel);
                    if (dgConveyor?.Columns?.Contains("EMPTY_INPUT_RATIO") == true) dgConveyor.Columns["EMPTY_INPUT_RATIO"].Width = new C1.WPF.DataGrid.DataGridLength(80, DataGridUnitType.Pixel);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void ClearControl()
        {
            Util.gridClear(dgConveyor);
            Util.gridClear(dgConveyorDetail);
            Util.gridClear(dgTrayMaxCnt);
        }

        private void TimerSetting()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "SECOND_INTERVAL" };
            combo.SetCombo(cboTimer, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboTimer != null && cboTimer.Items.Count > 0)
                cboTimer.SelectedIndex = 0;

            if (_monitorTimer != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.ToString()))
                    second = int.Parse(cboTimer.SelectedValue.ToString());

                _monitorTimer.Tick += _dispatcherTimer_Tick;
                _monitorTimer.Interval = new TimeSpan(0, 0, second);
            }
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

        #endregion

    }
}
