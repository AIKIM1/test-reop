/*************************************************************************************
 Created Date : 2020.12.01
      Creator : 김대근
   Decription : 실시간 수율 현황(진행 중 LOT 기준)
--------------------------------------------------------------------------------------
 [Change History]
 --------------------------------------------------------------------------------------
     날 짜     이 름      CSR 번호                      내용
--------------------------------------------------------------------------------------
  2020.12.22  김대근 : Initial Created.  
  2021.02.06  오화백 : 설비불량 정보 추가
  2022.03.30  김광오    C20220122-000042    GMES 작업중 lot불량현황 창 기준 변경 요청
  2022.05.18  안유수    C20220516-000656    작업 중 Lot 불량 현황 화면에서 사용자가 화면 크기 조절을 할 수 있는 구분선 추가
  2022.12.20  윤지해    C20221001-000004    [ESWA PI] FMCS 시스템 상 양품률 (OK rate) 현황 모니터링 기능 추가
  2023.02.01  윤지해    C20221001-000004    [ESWA PI] FMCS 시스템 상 양품률 (OK rate) 현황 모니터링 기능 추가(DA -> BR 변경)
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Linq;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;
using System.Windows.Input;
using System.Threading;
using LGC.GMES.MES.CMM001.UserControls;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_347 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private decimal _yieldYellowReferenceValue = 0;
        private decimal _yieldRedReferenceValue = 0;

        public COM001_347()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }
        #endregion

        #region Initialize
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetMultiCboEqsg();
            TimerSetting();
            _isLoaded = true;
            Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // 2022.12.20 C20221001-000004 [ESWA PI] FMCS 시스템 상 양품률 (OK rate) 현황 모니터링 기능 추가_사용하지 않음
            //SetYieldColorReferenceValue(); 
            SetContents();
        }

        private void dgProc_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (e == null || e.Cell == null || e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.Name.Equals("YEILD"))
                {
                    int colorDataRow = e.Cell.Row.Index;
                    int colorDataCol = e.Cell.DataGrid.Columns["CLR_TYPE"].Index;
                    C1.WPF.DataGrid.DataGridCell colorDataCell = e.Cell.DataGrid[colorDataRow, colorDataCol];
                    string colorData = Util.NVC(colorDataCell.Value);
                    SolidColorBrush solidColorBrush = null;

                    #region 2022.12.20 C20221001-000004 [ESWA PI] FMCS 시스템 상 양품률 (OK rate) 현황 모니터링 기능 추가_양품률 색상 기준값 수정
                    if (sender == null)
                        return;

                    C1DataGrid dataGrid = sender as C1DataGrid;

                    if (e.Cell.Row.DataItem == null) return;

                    switch (colorData)
                    {
                        case "R":
                            solidColorBrush = new SolidColorBrush(Colors.Red);
                            break;
                        case "W":
                            solidColorBrush = new SolidColorBrush(Colors.White);
                            break;
                        default:
                            solidColorBrush = new SolidColorBrush(Colors.White);
                            break;
                    }

                    #endregion

                    #region C20220122-000042    GMES 작업중 lot불량현황 창 기준 변경 요청 commented by kimgwango on 2022.03.30
                    //switch (colorData)
                    //{
                    //    case "R":
                    //        solidColorBrush = new SolidColorBrush(Colors.Red);
                    //        break;
                    //    case "Y":
                    //        solidColorBrush = new SolidColorBrush(Colors.Yellow);
                    //        break;
                    //    case "W":
                    //        solidColorBrush = new SolidColorBrush(Colors.White);
                    //        break;
                    //    default:
                    //        solidColorBrush = null;
                    //        break;
                    //}
                    #endregion

                    #region 2022.12.20 C20221001-000004 [ESWA PI] FMCS 시스템 상 양품률 (OK rate) 현황 모니터링 기능 추가_주석처리
                    //if (sender == null)
                    //    return;

                    //C1DataGrid dataGrid = sender as C1DataGrid;

                    //if (e.Cell.Row.DataItem == null) return;

                    //DataRowView dvr = (DataRowView)e.Cell.Row.DataItem;
                    //DataRow dr = dvr.Row;

                    //decimal inputQty = (decimal)Util.StringToDouble(dr["INPUTQTY"].ToString());
                    //decimal dfctQty = (decimal)Util.StringToDouble(dr["DFCTQTY"].ToString());

                    //if (inputQty == 0)
                    //{
                    //    solidColorBrush = new SolidColorBrush(Colors.White);
                    //}
                    //else if (_yieldRedReferenceValue > 0 && ((inputQty - dfctQty) / inputQty) * 100 < _yieldRedReferenceValue)
                    //{
                    //    solidColorBrush = new SolidColorBrush(Colors.Red);
                    //}
                    //else if (_yieldYellowReferenceValue > 0 && ((inputQty - dfctQty) / inputQty) * 100 < _yieldYellowReferenceValue)
                    //{
                    //    solidColorBrush = new SolidColorBrush(Colors.Yellow);
                    //}
                    //else
                    //{
                    //    solidColorBrush = new SolidColorBrush(Colors.White);
                    //}
                    #endregion

                    if (solidColorBrush != null)
                    {
                        C1DataGrid dg = sender as C1DataGrid;
                        if (dg == null)
                        {
                            return;
                        }

                        dg.Dispatcher.BeginInvoke(new Action(() => {
                            try
                            {
                                if (e == null || e.Cell == null || e.Cell.Presenter == null)
                                {
                                    return;
                                }
                                e.Cell.Presenter.Background = solidColorBrush;
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }));
                    }
                    /*
                     * 1. CELL 선택
                     * 2. 해당 CELL의 ROW 선택
                     * 3. ROW에서 CLR_TYPE 선택
                     * 4. ROW에서 YEILD 선택
                     * 4-1. YEILD를 CLR_TYPE에 따라 배경색 변경함
                     */
                }
                else if (e.Cell.Column.Name.Equals("DFCTQTY") || e.Cell.Column.Name.Equals("EQPT_DFCT_QTY"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("blue"));
                    e.Cell.Presenter.Cursor = Cursors.Hand;
                }



            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProc_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //try
            //{
            //    if (e == null || e.Cell == null || e.Cell.Presenter == null)
            //    {
            //        return;
            //    }

            //    if (!e.Cell.Column.Name.Equals("YEILD"))
            //    {
            //        return;
            //    }

            //    C1DataGrid dg = sender as C1DataGrid;
            //    if(dg == null)
            //    {
            //        return;
            //    }

            //    dg.Dispatcher.BeginInvoke(new Action(() => {
            //        try
            //        {
            //            if (e == null || e.Cell == null || e.Cell.Presenter == null)
            //            {
            //                return;
            //            }
            //            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
            //        }
            //        catch(Exception ex)
            //        {
            //            throw ex;
            //        }
            //    }));
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
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

        //private void dgProc_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    //try
        //    //{
        //    //    C1DataGrid dg = sender as C1DataGrid;
        //    //    if (dg == null)
        //    //    {
        //    //        return;
        //    //    }

        //    //    C1.WPF.DataGrid.DataGridCell cell = dg.CurrentCell;
        //    //    if (cell == null)
        //    //    {
        //    //        return;
        //    //    }

        //    //    string colName = cell.Column.Name;
        //    //    if (!colName.Equals("DFCTQTY"))
        //    //    {
        //    //        return;
        //    //    }

        //    //    int rowIdx = cell.Row.Index;
        //    //    int colIdx = cell.DataGrid.Columns["LOTID"].Index;
        //    //    string lotid = Util.NVC(dg[rowIdx, colIdx].Value);

        //    //    COM001_347_DEFECT wndDefect = new COM001_347_DEFECT();
        //    //    wndDefect.FrameOperation = FrameOperation;

        //    //    if (wndDefect != null)
        //    //    {
        //    //        object[] Parameters = new object[1];
        //    //        Parameters[0] = lotid;
        //    //        C1WindowExtension.SetParameters(wndDefect, Parameters);

        //    //        this.Dispatcher.BeginInvoke(new Action(() => {
        //    //            wndDefect.ShowModal();
        //    //        }));
        //    //    }
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    Util.MessageException(ex);
        //    //}
        //}

        private void dgProc_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                C1DataGrid dg = (sender as C1DataGrid);

                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null)
                    return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                //C1.WPF.DataGrid.DataGridCell cell = dg.CurrentCell;
                if (cell == null)
                {
                    return;
                }

                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                string colName = cell.Column.Name;
                if (colName.Equals("DFCTQTY"))
                {


                    COM001_347_DEFECT wndDefect = new COM001_347_DEFECT();
                    wndDefect.FrameOperation = FrameOperation;

                    if (wndDefect != null)
                    {
                        object[] Parameters = new object[1];
                        Parameters[0] = DataTableConverter.GetValue(drv, "LOTID").GetString();
                        C1WindowExtension.SetParameters(wndDefect, Parameters);

                        this.Dispatcher.BeginInvoke(new Action(() => {
                            wndDefect.ShowModal();
                        }));
                    }
                }
                else if (colName.Equals("EQPT_DFCT_QTY"))
                {


                    COM001_347_EQPT_DEFECT wndEqptDefect = new COM001_347_EQPT_DEFECT();
                    wndEqptDefect.FrameOperation = FrameOperation;

                    if (wndEqptDefect != null)
                    {
                        object[] parameters = new object[3];
                        parameters[0] = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                        parameters[1] = DataTableConverter.GetValue(drv, "LOTID").GetString();
                        parameters[2] = DataTableConverter.GetValue(drv, "WIPSEQ").GetString();
                        C1WindowExtension.SetParameters(wndEqptDefect, parameters);

                        this.Dispatcher.BeginInvoke(new Action(() => {
                            wndEqptDefect.ShowModal();
                        }));
                    }
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
                        Util.MessageValidation("SFU8310");
                        return;
                    }
                    _monitorTimer.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer.Start();

                    if (_isSelectedAutoTime)
                    {
                        if (cboTimer != null)
                            Util.MessageInfo("SFU8311", Convert.ToString(Convert.ToInt32(cboTimer.SelectedValue) / 60));
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
            if (sender == null)
                return;

            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;

                    // 2022.12.20 C20221001-000004 [ESWA PI] FMCS 시스템 상 양품률 (OK rate) 현황 모니터링 기능 추가_사용하지 않음
                    //SetYieldColorReferenceValue();
                    SetContents();
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
        #endregion

        #region Method
        private void SetMultiCboEqsg()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EXCEPT_GROUP", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EXCEPT_GROUP"] = "VD";
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT, (bizResult, exception) => {

                    if (exception != null)
                    {
                        throw exception;
                    }

                    try
                    {
                        multiCboEqsg.ItemsSource = DataTableConverter.Convert(bizResult);
                        multiCboEqsg.CheckAll();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void SetContents()
        {
            try
            {
                /*
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQGRID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQGRID"] = "LAM";
                dr["EQSGID"] = string.Join(",", multiCboEqsg.SelectedItems);
                RQSTDT.Rows.Add(dr);

                DataTable dtLam = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CURR_LOT_YEILD_BY_EQPT", "RQSTDT", "RSLTDT", RQSTDT);

                #region C20220122-000042    GMES 작업중 lot불량현황 창 기준 변경 요청 commented by kimgwango on 2022.03.30
                //RQSTDT.Rows[0]["EQGRID"] = "STK,FOL";
                //DataTable dtStkFol = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CURR_LOT_YEILD_BY_EQPT", "RQSTDT", "RSLTDT", RQSTDT);
                #endregion

                RQSTDT.Rows[0]["EQGRID"] = "PKG";
                DataTable dtPkg = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CURR_LOT_YEILD_BY_EQPT", "RQSTDT", "RSLTDT", RQSTDT);

                #region C20220122-000042    GMES 작업중 lot불량현황 창 기준 변경 요청 added by kimgwango on 2022.03.30
                RQSTDT.Rows[0]["EQGRID"] = "STK";
                DataTable dtStk = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CURR_LOT_YEILD_BY_EQPT", "RQSTDT", "RSLTDT", RQSTDT);

                RQSTDT.Rows[0]["EQGRID"] = "FOL";
                DataTable dtFol = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CURR_LOT_YEILD_BY_EQPT", "RQSTDT", "RSLTDT", RQSTDT);
                #endregion
                */

                #region 2023.02.01 C20221001-000004 [ESWA PI] FMCS 시스템 상 양품률 (OK rate) 현황 모니터링 기능 추가_DA -> BR 변경
                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("EQGRID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["EQGRID"] = "LAM";
                newRow["EQSGID"] = string.Join(",", multiCboEqsg.SelectedItems);
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                inDataTable.Rows.Add(newRow);

                string bizName = "BR_PRD_GET_CURR_LOT_YIELD";

                // LAM DataGrid
                DataTable dtLam = new ClientProxy().ExecuteServiceSync(bizName, "INDATA", "OUTDATA", inDataTable);

                // PKG DataGrid
                inDataTable.Rows[0]["EQGRID"] = "PKG";
                DataTable dtPkg = new ClientProxy().ExecuteServiceSync(bizName, "INDATA", "OUTDATA", inDataTable);

                // STK DataGrid
                inDataTable.Rows[0]["EQGRID"] = "STK";
                DataTable dtStk = new ClientProxy().ExecuteServiceSync(bizName, "INDATA", "OUTDATA", inDataTable);

                // FOL DataGrid
                inDataTable.Rows[0]["EQGRID"] = "FOL";
                DataTable dtFol = new ClientProxy().ExecuteServiceSync(bizName, "INDATA", "OUTDATA", inDataTable);
                #endregion

                if (dtLam.Rows.Count > 0)
                {
                    for (int i = 0; i < dtLam.Rows.Count; i++)
                    {
                        if (string.IsNullOrEmpty(dtLam.Rows[i]["EQPT_DFCT_QTY"].ToString()))
                        {
                            dtLam.Rows[i]["EQPT_DFCT_QTY"] = 0;
                        }
                    }
                }
                Util.GridSetData(dgLam, dtLam, this.FrameOperation);
                Util.GridSetData(dgLamBottom, GetBottomRowData(dtLam), this.FrameOperation);

                #region C20220122-000042    GMES 작업중 lot불량현황 창 기준 변경 요청 commented by kimgwango on 2022.03.30
                //if (dtStkFol.Rows.Count > 0)
                //{
                //    for (int i = 0; i < dtStkFol.Rows.Count; i++)
                //    {
                //        if (string.IsNullOrEmpty(dtStkFol.Rows[i]["EQPT_DFCT_QTY"].ToString()))
                //        {
                //            dtStkFol.Rows[i]["EQPT_DFCT_QTY"] = 0;
                //        }
                //    }
                //}
                //Util.GridSetData(dgStkFol, dtStkFol, this.FrameOperation);
                //Util.GridSetData(dgStkFolBottom, GetBottomRowData(dtStkFol), this.FrameOperation);
                #endregion

                #region C20220122-000042    GMES 작업중 lot불량현황 창 기준 변경 요청 added by kimgwango on 2022.03.30
                if (dtStk.Rows.Count > 0)
                {
                    for (int i = 0; i < dtStk.Rows.Count; i++)
                    {
                        if (string.IsNullOrEmpty(dtStk.Rows[i]["EQPT_DFCT_QTY"].ToString()))
                        {
                            dtStk.Rows[i]["EQPT_DFCT_QTY"] = 0;
                        }
                    }

                    //StkRowDef.Height = new GridLength(1, GridUnitType.Star);
                }
                //else
                //{
                //    StkRowDef.Height = new GridLength(0, GridUnitType.Star);
                //}
                Util.GridSetData(dgStk, dtStk, this.FrameOperation);
                Util.GridSetData(dgStkBottom, GetBottomRowData(dtStk), this.FrameOperation);

                if (dtFol.Rows.Count > 0)
                {
                    for (int i = 0; i < dtFol.Rows.Count; i++)
                    {
                        if (string.IsNullOrEmpty(dtFol.Rows[i]["EQPT_DFCT_QTY"].ToString()))
                        {
                            dtFol.Rows[i]["EQPT_DFCT_QTY"] = 0;
                        }
                    }

                    //FolRowDef.Height = new GridLength(1, GridUnitType.Star);
                }
                //else
                //{
                //    FolRowDef.Height = new GridLength(0, GridUnitType.Star);
                //}
                Util.GridSetData(dgFol, dtFol, this.FrameOperation);
                Util.GridSetData(dgFolBottom, GetBottomRowData(dtFol), this.FrameOperation);
                #endregion

                //if(dtStk.Rows.Count == 0 && dtFol.Rows.Count == 0)
                //{
                //    StkRowDef.Height = new GridLength(1, GridUnitType.Star);
                //    FolRowDef.Height = new GridLength(1, GridUnitType.Star);
                //}

                if (dtPkg.Rows.Count > 0)
                {
                    for (int i = 0; i < dtPkg.Rows.Count; i++)
                    {
                        if (string.IsNullOrEmpty(dtPkg.Rows[i]["EQPT_DFCT_QTY"].ToString()))
                        {
                            dtPkg.Rows[i]["EQPT_DFCT_QTY"] = 0;
                        }
                    }
                }
                Util.GridSetData(dgPkg, dtPkg, this.FrameOperation);
                Util.GridSetData(dgPkgBottom, GetBottomRowData(dtPkg), this.FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TimerSetting()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "INTERVAL_MIN" };
            combo.SetCombo(cboTimer, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboTimer != null && cboTimer.Items.Count > 0)
                cboTimer.SelectedIndex = 1;

            if (_monitorTimer != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.ToString()))
                    second = int.Parse(cboTimer.SelectedValue.ToString());

                _monitorTimer.Tick += _dispatcherTimer_Tick;
                _monitorTimer.Interval = new TimeSpan(0, 0, second);

                _monitorTimer.Start();

            }
        }

        private DataTable GetBottomRowData(DataTable dtOrigin)
        {
            DataTable result = new DataTable();
            result.Columns.Add("TOTAL", typeof(string));
            result.Columns.Add("INPUTQTY", typeof(decimal));
            result.Columns.Add("WIPQTY", typeof(decimal));
            result.Columns.Add("DFCTQTY", typeof(decimal));
            result.Columns.Add("EQPT_DFCT_QTY", typeof(decimal));
            result.Columns.Add("YEILD", typeof(decimal));

            decimal sumInputQty = 0;
            decimal sumWipQty = 0;
            decimal sumDeftQty = 0;
            decimal sumEqptDeftQty = 0;

            decimal avgYield = 0;

            foreach (DataRowView drv in dtOrigin.DefaultView)
            {
                sumInputQty += Util.NVC_Decimal(drv["INPUTQTY"]);
                sumWipQty += Util.NVC_Decimal(drv["WIPQTY"]);
                sumDeftQty += Util.NVC_Decimal(drv["DFCTQTY"]);
                sumEqptDeftQty += Util.NVC_Decimal(drv["EQPT_DFCT_QTY"]);
            }
            if (sumInputQty == 0)
            {
                avgYield = 0;
            }
            else
            {
                avgYield = ((sumInputQty - sumDeftQty) * 100) / sumInputQty;
            }
            DataRow dr = result.NewRow();
            dr["TOTAL"] = "TOTAL";
            dr["INPUTQTY"] = sumInputQty;
            dr["WIPQTY"] = sumWipQty;
            dr["DFCTQTY"] = sumDeftQty;
            dr["EQPT_DFCT_QTY"] = sumEqptDeftQty;
            dr["YEILD"] = avgYield;
            result.Rows.Add(dr);

            return result;
        }
        #endregion

        private void dgProcBottom_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (e == null || e.Cell == null || e.Cell.Presenter == null)
                {
                    return;
                }

                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null)
                {
                    return;
                }

                dg.Dispatcher.BeginInvoke(new Action(() => {
                    try
                    {
                        if (e == null || e.Cell == null || e.Cell.Presenter == null)
                        {
                            return;
                        }
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Beige);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [ Util ] - DA
        private void SetYieldColorReferenceValue()
        {
            _yieldYellowReferenceValue = 0;
            _yieldRedReferenceValue = 0;

            DataTable inTable = new DataTable();
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
            inTable.Columns.Add("COM_CODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "REAL_TIME_YIELD_MONITORING_INFO";
            dr["COM_CODE"] = "YIELD_COLOR_REFERENCE_VALUE";

            inTable.Rows.Add(dr);

            string bizName = "DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA";

            try
            {
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizName, "INDATA", "OUTDATA", inTable);

                if (dtRslt?.Rows?.Count > 0)
                {
                    decimal decAttr1 = 0;
                    string strAttr1 = dtRslt.Rows[0]["ATTR1"]?.ToString();

                    if (decimal.TryParse(strAttr1, out decAttr1))
                    {
                        _yieldYellowReferenceValue = decAttr1;
                    }

                    decimal decAttr2 = 0;
                    string strAttr2 = dtRslt.Rows[0]["ATTR2"]?.ToString();

                    if (decimal.TryParse(strAttr2, out decAttr2))
                    {
                        _yieldRedReferenceValue = decAttr2;
                    }
                }
            }
            catch (Exception ex)
            {
                // Util.MessageException(ex);
            }
        }
        #endregion
    }
}
