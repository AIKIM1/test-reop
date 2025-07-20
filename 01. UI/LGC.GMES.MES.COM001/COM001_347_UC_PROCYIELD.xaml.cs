/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : Lot별 불량현황 (동적구성)
--------------------------------------------------------------------------------------
 [Change History]
 --------------------------------------------------------------------------------------
     날 짜     이 름      CSR 번호                      내용
--------------------------------------------------------------------------------------
                     : Initial Created.  
  2024.08.23  김동일 : [E20240717-000992] - 조회 시 TEST Lot 유형 제외 조건 추가
**************************************************************************************/

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
using System.Windows.Navigation;
using System.Windows.Shapes;


using System.Data;

using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.ControlsLibrary;


namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_347_UcLotList.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 
    public partial class COM001_347_UC_PROCYIELD : UserControl
    {
        #region Declaration & Constructor
        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;

        private string proc2char = null;
        private string eqgrid = null;
        private string procid = null;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private decimal _yieldYellowReferenceValue = 0;
        private decimal _yieldRedReferenceValue = 0;

        public COM001_347_UC_PROCYIELD()
        {
            InitializeComponent();
        }

        public COM001_347_UC_PROCYIELD(string procid)
        {
            InitializeComponent();

            this.proc2char = procid;
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

            _isLoaded = true;
            Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
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

        #endregion

        public void SetTitle(string title) {
            txtProcName.Text = title;
        }

        public void SetEqgrid(string eqgrid)
        {
            this.eqgrid = eqgrid;
        }
        #region Method


        public void SetContents(MultiSelectionBox cboEqsg, string sTestLotExclFlag)
        {
            try
            {

                #region 2023.02.01 C20221001-000004 [ESWA PI] FMCS 시스템 상 양품률 (OK rate) 현황 모니터링 기능 추가_DA -> BR 변경

                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("EQGRID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("TEST_TYPE_EXCL", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["EQGRID"] = eqgrid;
                newRow["EQSGID"] = string.Join(",", cboEqsg.SelectedItems);
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = procid;

                if (sTestLotExclFlag.Equals("Y"))
                    newRow["TEST_TYPE_EXCL"] = sTestLotExclFlag;

                inDataTable.Rows.Add(newRow);

                string bizName = "BR_PRD_GET_CURR_LOT_YIELD";
                                
                DataTable dt = new ClientProxy().ExecuteServiceSync(bizName, "INDATA", "OUTDATA", inDataTable);
                
                #endregion

                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (string.IsNullOrEmpty(dt.Rows[i]["EQPT_DFCT_QTY"].ToString()))
                        {
                            dt.Rows[i]["EQPT_DFCT_QTY"] = 0;
                        }
                    }
                }
                Util.GridSetData(dgDefectList, dt, this.FrameOperation);
                Util.GridSetData(dgSummary, GetBottomRowData(dt), this.FrameOperation);

 
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

        public void SetProcId(string procid) {
            this.procid = procid;
        }
        #endregion
    }
}
