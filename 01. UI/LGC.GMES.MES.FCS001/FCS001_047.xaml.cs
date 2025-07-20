/*************************************************************************************
 Created Date : 2020.12.15
      Creator : 박수미
   Decription : 가압단락검사 측정값 조회
   --------------------------------------------------------------------------------------
 [Change History]
  2020.12.15  DEVELOPER : Initial Created.
  2022.02.21  KDH    : AREA 조건 추가
  2022.10.21  이정미 : 측정값 조회Tab, 측정값 이력조회 Tab 설비판정결과 컬럼 배경 색상 추가 
  2022.11.30  조영대 : Paging 처리 추가.
  2023.01.09  형준우 : ESWA#1 HPCD 설비레벨이 C 로 되어있어, 해당 사항 대응으로 동별공통코드를 활용하여 Machine 설비를 가져오도록 수정
  2023.05.09  최도훈 : 인도네시아 조회시 오류나는 현상 수정
  2023.10.16  조영대 : Summary 탭 추가
  2023.11.05  이의철 : EQPT_JUDG_RSLT_CODE 0 to 1 양품변경 구분
  2023.11.08  이의철 : EQPT_JUDG_RSLT_CODE 0 to 1 양품변경 구분 - 측정값 조회에 적용
**************************************************************************************/

using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using Microsoft.Win32;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Excel;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// FCS001_047.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_047 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        bool bEqpMachineUseFlag = false;
        Util _Util = new Util();

        bool bFCS001_047_EQPT_JUDG_RSLT_CODE_0_TO_1 = false; //EQPT_JUDG_RSLT_CODE 0 to 1 양품변경 구분

        public FCS001_047()
        {
            InitializeComponent();

            this.Loaded += UserControl_Loaded;
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        /// <summary>
        /// 화면 내 ComboBox Setting
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            bEqpMachineUseFlag = _Util.IsAreaCommonCodeUse("EQP_LEVEL_TYPE", "MACHINE");

            //EQPT_JUDG_RSLT_CODE 0 to 1 양품변경 구분            
            bFCS001_047_EQPT_JUDG_RSLT_CODE_0_TO_1 = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_047_EQPT_JUDG_RSLT_CODE_0_TO_1");

            if (bEqpMachineUseFlag)
            {
                string[] sFilterM = { "U", null, "C" };
                _combo.SetCombo(cboEqp, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPID", sFilter: sFilterM);

                string[] sFilterHistM = { "U", null, "C" };
                _combo.SetCombo(cboEqpHist, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPID", sFilter: sFilterHistM);

                string[] sFilterSummaryM = { "U", null, "C" };
                _combo.SetCombo(cboEqpSummary, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPID", sFilter: sFilterHistM);
            }
            else
            {
                string[] sFilter = { "U", null, "M" };
                _combo.SetCombo(cboEqp, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPID", sFilter: sFilter);

                string[] sFilterHist = { "U", null, "M" };
                _combo.SetCombo(cboEqpHist, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPID", sFilter: sFilterHist);

                string[] sFilterSummary = { "U", null, "M" };
                _combo.SetCombo(cboEqpSummary, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPID", sFilter: sFilterHist);
            }
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitCombo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            pageNavi.Clear();

            GetList(1);
        }

        private void BtnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            pageNaviHist.Clear();

            GetHistList(1);
        }

        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //link 색변경
                if (e.Cell.Row.Type == C1.WPF.DataGrid.DataGridRowType.Item)
                {
                    DataRowView dr = (DataRowView)e.Cell.Row.DataItem;

                    string sJUDG = Util.NVC(dr.Row["EQPT_JUDG_RSLT_CODE"]);

                    if (e.Cell.Column.Name.Equals("LOTID") || e.Cell.Column.Name.Equals("CSTID") || e.Cell.Column.Name.Equals("SUBLOTID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }

                    //NA 특화 : 0 -> 1 로 판정결과 코드 사용
                    //EQPT_JUDG_RSLT_CODE 0 to 1 양품변경 구분
                    if (bFCS001_047_EQPT_JUDG_RSLT_CODE_0_TO_1.Equals(true))
                    {

                        if (!sJUDG.Equals("1"))
                        {

                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Row.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Row.Presenter.FontWeight = FontWeights.Normal;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }
                    else
                    {
                        if (!sJUDG.Equals("0"))
                        {

                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Row.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Row.Presenter.FontWeight = FontWeights.Normal;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }
                }
            }));
        }

        private void dgList_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

        private void dgMeasList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //link 색변경
                if (e.Cell.Row.Type == C1.WPF.DataGrid.DataGridRowType.Item)
                {
                    DataRowView dr = (DataRowView)e.Cell.Row.DataItem;

                    string sJUDG = Util.NVC(dr.Row["EQPT_JUDG_RSLT_CODE"]);

                    if (e.Cell.Column.Name.Equals("LOTID") || e.Cell.Column.Name.Equals("CSTID") || e.Cell.Column.Name.Equals("SUBLOTID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }

                    //NA 특화 : 0 -> 1 로 판정결과 코드 사용
                    //EQPT_JUDG_RSLT_CODE 0 to 1 양품변경 구분
                    if (bFCS001_047_EQPT_JUDG_RSLT_CODE_0_TO_1.Equals(true))
                    {
                        if (!sJUDG.Equals("1"))
                        {

                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Row.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Row.Presenter.FontWeight = FontWeights.Normal;
                        }

                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }
                    else
                    {
                        if (!sJUDG.Equals("0"))
                        {

                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Row.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Row.Presenter.FontWeight = FontWeights.Normal;
                        }

                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }

                    
                }
            }));
        }

        private void dgMeasList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
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
        }

        private void dgMeasList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell == null || datagrid.CurrentRow == null)
                {
                    return;
                }

                if (cell.Text != datagrid.CurrentColumn.Header.ToString())
                {
                    //TRAY_LOT_ID or CSTID 더블클릭 시 Tray 정보조회 화면으로 이동
                    if (cell.Column.Name.Equals("LOTID") || cell.Column.Name.Equals("CSTID"))
                    {
                        FCS001_021 wndTRAY = new FCS001_021();
                        wndTRAY.FrameOperation = FrameOperation;

                        object[] Parameters = new object[10];
                        Parameters[0] = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "CSTID")); //Tray ID
                        Parameters[1] = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "LOTID")); //Tray No
                        this.FrameOperation.OpenMenu("SFU010710010", true, Parameters);
                    }
                    if (cell.Column.Name.Equals("SUBLOTID"))
                    {
                        string sCellId = cell.Text;
                        FCS001_022 fcs022 = new FCS001_022();
                        fcs022.FrameOperation = FrameOperation;

                        object[] parameters = new object[2];
                        parameters[0] = Util.NVC(sCellId);
                        parameters[1] = "Y"; //_sActYN

                        this.FrameOperation.OpenMenuFORM("SFU010710020", "FCS001_022", "LGC.GMES.MES.FCS001", ObjectDic.Instance.GetObjectName("Cell 정보조회"), true, parameters);
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void pageNavi_PageChanged(object sender, int pageNumber)
        {
            GetList(pageNumber);
        }

        private void dgMeasList_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            try
            {
                if (e.ResultData is DataTable)
                {
                    DataTable dtResult = e.ResultData as DataTable;

                    if (dtResult.Rows.Count > 0 && dtResult.Columns.Contains("ROW_CNT"))
                    {
                        pageNavi.RowCountPerPage = Convert.ToInt32(dtResult.Rows[0]["PAGE_CNT"]);
                        pageNavi.MaxRowCount = Convert.ToInt32(dtResult.Rows[0]["ROW_CNT"]);
                    }
                    else
                    {
                        pageNavi.MaxRowCount = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                btnSearch.IsEnabled = true;
                btnExcel.IsEnabled = true;
                pageNavi.IsEnabled = true;
            }
        }

        private void pageNaviHist_PageChanged(object sender, int pageNumber)
        {
            GetHistList(pageNumber);
        }

        private void dgMeasHistList_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            try
            {
                if (e.ResultData is DataTable)
                {
                    DataTable dtResult = e.ResultData as DataTable;

                    if (dtResult.Rows.Count > 0 && dtResult.Columns.Contains("ROW_CNT"))
                    {
                        pageNaviHist.RowCountPerPage = Convert.ToInt32(dtResult.Rows[0]["PAGE_CNT"]);
                        pageNaviHist.MaxRowCount = Convert.ToInt32(dtResult.Rows[0]["ROW_CNT"]);
                    }
                    else
                    {
                        pageNaviHist.MaxRowCount = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                btnSearchHist.IsEnabled = true;
                btnExcelHist.IsEnabled = true;
                pageNaviHist.IsEnabled = true;
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable inDataTable = GetInData(1);
            if (inDataTable == null) return;

            string excelFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

            SaveFileDialog saveDialog = new SaveFileDialog();
            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                saveDialog.InitialDirectory = @"\\Client\C$\Users";
            }
            saveDialog.Filter = "Excel Files (.xlsx)|*.xlsx";
            saveDialog.FileName = excelFileName;

            if (saveDialog.ShowDialog() == true)
            {
                excelFileName = saveDialog.FileName;
            }
            else
            {
                return;
            }

            object[] arguments = new object[2];
            arguments[0] = excelFileName;
            arguments[1] = inDataTable;

            btnExcel.IsEnabled = false;
            progExcel.Clear();
            progExcel.ProgressText = MessageDic.Instance.GetMessage("10057");
            progExcel.Visibility = Visibility.Visible;

            progExcel.RunWorker(arguments);
        }
                
        private void progExcel_ClickProgress(object sender)
        {
            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    progExcel.CancelWorkProcess();
                }
            });
        }

        private object progExcel_WorkProcess(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            object[] args = (object[])e.Arguments;
            string excelFileName = args[0].ToString();
            DataTable inDataTable = args[1] as DataTable;

            int pageNo = 1;

            DataTable dtResult = null;

            while (!e.Worker.CancellationPending)
            {
                DataRow drInData = inDataTable.Rows[0];
                drInData["PAGE_NO"] = pageNo;

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CURR_MEAS_PAGING", "INDATA", "OUTDATA", inDataTable);
                if (dtRslt == null || dtRslt.Rows.Count == 0) break;

                if (dtResult == null)
                {
                    dtResult = dtRslt.Copy();
                }
                else
                {
                    dtResult.Merge(dtRslt, true, MissingSchemaAction.Ignore);
                }

                int rowCountPerPage = Convert.ToInt32(dtRslt.Rows[0]["PAGE_CNT"]);
                int maxRowCount = Convert.ToInt32(dtRslt.Rows[0]["ROW_CNT"]);

                long lessTime = stopWatch.ElapsedMilliseconds / pageNo * (maxRowCount / rowCountPerPage + 1) - stopWatch.ElapsedMilliseconds;
                long lessMinute = lessTime / 1000 / 60;
                long lessSecond = (lessTime / 1000) - (lessMinute * 60);

                e.Worker.ReportProgress((pageNo * rowCountPerPage * 100) / maxRowCount,
                    (pageNo * rowCountPerPage).ToString("#,##0") + " / " + maxRowCount.ToString("#,##0") + "   [ " +
                    lessMinute.ToString("00") + ":" + lessSecond.ToString("00") + " ]");

                pageNo++;

                if (pageNo > (maxRowCount / rowCountPerPage) + 1) break;
            }

            if (e.Worker.CancellationPending)
            {
                return null;
            }
            else
            {
                e.Worker.ReportProgress(100, MessageDic.Instance.GetMessage("SFU5207"));
                System.Threading.Thread.Sleep(1000);

                object[] returnArg = new object[2];
                returnArg[0] = excelFileName;
                returnArg[1] = dtResult;

                return returnArg;
            }
        }

        private void progExcel_WorkProcessChanged(object sender, int percent, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            progExcel.Percent = percent;
            progExcel.ProgressText = Util.NVC(e.Arguments);
        }

        private void progExcel_WorkProcessCompleted(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        object[] args = (object[])e.Result;
                        string excelFileName = args[0].ToString();
                        DataTable dtResult = args[1] as DataTable;

                        dgMeasListExcel.ItemsSource = dtResult.DefaultView;
                        dgMeasListExcel.ExcelExport(excelFileName);
                        dgMeasListExcel.ItemsSource = null;

                        // 전송 완료 되었습니다.
                        Util.MessageValidation("SFU1880");

                    }));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                btnExcel.IsEnabled = true;
                progExcel.Visibility = Visibility.Collapsed;
            }
        }

        private void btnExcelHist_Click(object sender, RoutedEventArgs e)
        {
            DataTable inDataTable = GetInDataHist(1);
            if (inDataTable == null) return;

            string excelFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

            SaveFileDialog saveDialog = new SaveFileDialog();
            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                saveDialog.InitialDirectory = @"\\Client\C$\Users";
            }
            saveDialog.Filter = "Excel Files (.xlsx)|*.xlsx";
            saveDialog.FileName = excelFileName;

            if (saveDialog.ShowDialog() == true)
            {
                excelFileName = saveDialog.FileName;
            }
            else
            {
                return;
            }

            object[] arguments = new object[2];
            arguments[0] = excelFileName;
            arguments[1] = inDataTable;

            btnExcelHist.IsEnabled = false;
            progExcelHist.Clear();
            progExcelHist.ProgressText = MessageDic.Instance.GetMessage("10057");
            progExcelHist.Visibility = Visibility.Visible;

            progExcelHist.RunWorker(arguments);
        }
         
        private void progExcelHist_ClickProgress(object sender)
        {
            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    progExcelHist.CancelWorkProcess();
                }
            });
        }

        private object progExcelHist_WorkProcess(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            object[] args = (object[])e.Arguments;
            string excelFileName = args[0].ToString();
            DataTable inDataTable = args[1] as DataTable;

            int pageNo = 1;

            DataTable dtResult = null;

            while (!e.Worker.CancellationPending)
            {
                DataRow drInData = inDataTable.Rows[0];
                drInData["PAGE_NO"] = pageNo;

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CURR_MEAS_HIST_PAGING", "INDATA", "OUTDATA", inDataTable);
                if (dtRslt == null || dtRslt.Rows.Count == 0) break;

                if (dtResult == null)
                {
                    dtResult = dtRslt.Copy();
                }
                else
                {
                    dtResult.Merge(dtRslt, true, MissingSchemaAction.Ignore);
                }

                int rowCountPerPage = Convert.ToInt32(dtRslt.Rows[0]["PAGE_CNT"]);
                int maxRowCount = Convert.ToInt32(dtRslt.Rows[0]["ROW_CNT"]);

                long lessTime = stopWatch.ElapsedMilliseconds / pageNo * (maxRowCount / rowCountPerPage + 1) - stopWatch.ElapsedMilliseconds;
                long lessMinute = lessTime / 1000 / 60;
                long lessSecond = (lessTime / 1000) - (lessMinute * 60);

                e.Worker.ReportProgress((pageNo * rowCountPerPage * 100) / maxRowCount,
                    (pageNo * rowCountPerPage).ToString("#,##0") + " / " + maxRowCount.ToString("#,##0") + "   [ " +
                    lessMinute.ToString("00") + ":" + lessSecond.ToString("00") + " ]");

                pageNo++;

                if (pageNo > (maxRowCount / rowCountPerPage) + 1) break;
            }

            if (e.Worker.CancellationPending)
            {
                return null;
            }
            else
            {
                e.Worker.ReportProgress(100, MessageDic.Instance.GetMessage("SFU5207"));
                System.Threading.Thread.Sleep(1000);

                object[] returnArg = new object[2];
                returnArg[0] = excelFileName;
                returnArg[1] = dtResult;

                return returnArg;
            }
        }

        private void progExcelHist_WorkProcessChanged(object sender, int percent, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            progExcelHist.Percent = percent;
            progExcelHist.ProgressText = Util.NVC(e.Arguments);
        }

        private void progExcelHist_WorkProcessCompleted(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        object[] args = (object[])e.Result;
                        string excelFileName = args[0].ToString();
                        DataTable dtResult = args[1] as DataTable;

                        dgMeasHistListExcel.ItemsSource = dtResult.DefaultView;
                        dgMeasHistListExcel.ExcelExport(excelFileName);
                        dgMeasHistListExcel.ItemsSource = null;

                        // 전송 완료 되었습니다.
                        Util.MessageValidation("SFU1880");

                    }));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                btnExcelHist.IsEnabled = true;
                progExcelHist.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Method

        private DataTable GetInData(int pageNo)
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string)); //2022.02.21_AREA 조건 추가
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));
                inDataTable.Columns.Add("NO_CSTID", typeof(string));
                inDataTable.Columns.Add("SUBLOTID", typeof(string));
                inDataTable.Columns.Add("CSTSLOT", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(DateTime));
                inDataTable.Columns.Add("TO_DATE", typeof(DateTime));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("FINL_JUDG_CODE", typeof(string));
                inDataTable.Columns.Add("PAGE_NO", typeof(Int16));
                inDataTable.Columns.Add("PAGE_UNIT", typeof(Int32));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID; //2022.02.21_AREA 조건 추가

                if (string.IsNullOrEmpty(txtLotId.Text)) newRow["PROD_LOTID"] = null;
                else newRow["PROD_LOTID"] = txtLotId.Text;

                if (string.IsNullOrEmpty(txtTrayNo.Text))
                    newRow["LOTID"] = DBNull.Value;
                else
                    newRow["LOTID"] = Util.GetCondition(txtTrayNo, bAllNull: true);

                if (string.IsNullOrEmpty(txtTrayId.Text))
                {
                    newRow["CSTID"] = DBNull.Value;
                    newRow["NO_CSTID"] = "NO";
                }
                else
                {
                    newRow["CSTID"] = Util.GetCondition(txtTrayId, bAllNull: true);
                    newRow["NO_CSTID"] = null;
                }

                if (string.IsNullOrEmpty(txtCellID.Text))
                    newRow["SUBLOTID"] = DBNull.Value;
                else
                    newRow["SUBLOTID"] = Util.GetCondition(txtCellID, bAllNull: true);

                if (string.IsNullOrEmpty(txtCellNo.Text))
                    newRow["CSTSLOT"] = DBNull.Value;
                else
                    newRow["CSTSLOT"] = Util.GetCondition(txtCellNo, bAllNull: true);

                if (string.IsNullOrEmpty(txtGrade.Text))
                    newRow["FINL_JUDG_CODE"] = DBNull.Value;
                else
                    newRow["FINL_JUDG_CODE"] = Util.GetCondition(txtGrade, bAllNull: true);

                newRow["EQPTID"] = Util.GetCondition(cboEqp, bAllNull: true);

                if (chkNoDate.IsChecked == true)
                {
                    newRow["FROM_DATE"] = DBNull.Value;
                    newRow["TO_DATE"] = DBNull.Value;

                    if (string.IsNullOrEmpty(txtTrayId.Text) && string.IsNullOrEmpty(txtTrayNo.Text) && string.IsNullOrEmpty(txtCellID.Text) && string.IsNullOrEmpty(txtLotId.Text))
                    {
                        Util.Alert("FM_ME_0331"); //조회기간 미 지정시 Tray ID, Tray No, Cell ID, Lot ID 중 하나는 선택해야합니다.
                        return null;
                    }
                }
                else
                {
                    newRow["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                    newRow["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm:00");
                }
                newRow["PAGE_NO"] = pageNo;

                if (rdo1000.IsChecked.Equals(true)) newRow["PAGE_UNIT"] = 1000;
                if (rdo2000.IsChecked.Equals(true)) newRow["PAGE_UNIT"] = 2000;
                if (rdo5000.IsChecked.Equals(true)) newRow["PAGE_UNIT"] = 5000;
                if (rdo10000.IsChecked.Equals(true)) newRow["PAGE_UNIT"] = 10000;
                if (rdo20000.IsChecked.Equals(true)) newRow["PAGE_UNIT"] = 20000;

                inDataTable.Rows.Add(newRow);

                return inDataTable;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return null;
        }

        private void GetList(int pageNo)
        {
            try
            {
                DataTable inDataTable = GetInData(pageNo);
                if (inDataTable == null) return;

                btnSearch.IsEnabled = false;
                btnExcel.IsEnabled = false;
                pageNavi.IsEnabled = false;

                // 멀티 Thread 실행 작업완료 후 그리드 자동 바인딩, 이후 dgMeasList_QueryDataCompleted 이벤트로 이동.
                dgMeasList.ExecuteService("DA_SEL_CELL_CURR_MEAS_PAGING", "INDATA", "OUTDATA", inDataTable, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable GetInDataHist(int pageNo)
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string)); //2022.02.21_AREA 조건 추가
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));
                inDataTable.Columns.Add("NO_CSTID", typeof(string));
                inDataTable.Columns.Add("SUBLOTID", typeof(string));
                inDataTable.Columns.Add("CSTSLOT", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(DateTime));
                inDataTable.Columns.Add("TO_DATE", typeof(DateTime));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("FINL_JUDG_CODE", typeof(string));
                inDataTable.Columns.Add("PAGE_NO", typeof(Int16));
                inDataTable.Columns.Add("PAGE_UNIT", typeof(Int32));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID; //2022.02.21_AREA 조건 추가

                if (string.IsNullOrEmpty(txtLotIdHist.Text))
                    newRow["PROD_LOTID"] = null;
                else
                    newRow["PROD_LOTID"] = txtLotIdHist.Text;

                if (string.IsNullOrEmpty(txtTrayNoHist.Text))
                    newRow["LOTID"] = DBNull.Value;
                else
                    newRow["LOTID"] = Util.GetCondition(txtTrayNoHist, bAllNull: true);

                if (string.IsNullOrEmpty(txtTrayIdHist.Text))
                {
                    newRow["CSTID"] = DBNull.Value;
                    newRow["NO_CSTID"] = "NO";
                }
                else
                {
                    newRow["CSTID"] = Util.GetCondition(txtTrayIdHist, bAllNull: true);
                    newRow["NO_CSTID"] = null;
                }

                if (string.IsNullOrEmpty(txtCellIDHist.Text))
                    newRow["SUBLOTID"] = DBNull.Value;
                else
                    newRow["SUBLOTID"] = Util.GetCondition(txtCellIDHist, bAllNull: true);

                if (string.IsNullOrEmpty(txtCellNoHist.Text))
                    newRow["CSTSLOT"] = DBNull.Value;
                else
                    newRow["CSTSLOT"] = Util.GetCondition(txtCellNoHist, bAllNull: true);

                if (string.IsNullOrEmpty(txtGradeHist.Text))
                    newRow["FINL_JUDG_CODE"] = DBNull.Value;
                else
                    newRow["FINL_JUDG_CODE"] = Util.GetCondition(txtGradeHist, bAllNull: true);

                newRow["EQPTID"] = Util.GetCondition(cboEqpHist, bAllNull: true);

                if (chkNoDateHist.IsChecked == true)
                {
                    newRow["FROM_DATE"] = DBNull.Value;
                    newRow["TO_DATE"] = DBNull.Value;

                    if (string.IsNullOrEmpty(txtTrayIdHist.Text) && string.IsNullOrEmpty(txtTrayNoHist.Text) && string.IsNullOrEmpty(txtCellIDHist.Text) && string.IsNullOrEmpty(txtLotIdHist.Text))
                    {
                        Util.Alert("FM_ME_0331"); //조회기간 미 지정시 Tray ID, Tray No, Cell ID, Lot ID 중 하나는 선택해야합니다.
                        return null;
                    }
                }
                else
                {
                    newRow["FROM_DATE"] = dtpFromDateHist.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTimeHist.DateTime.Value.ToString("HH:mm:00");
                    newRow["TO_DATE"] = dtpToDateHist.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTimeHist.DateTime.Value.ToString("HH:mm:00");
                }
                newRow["PAGE_NO"] = pageNo;

                if (rdoh1000.IsChecked.Equals(true)) newRow["PAGE_UNIT"] = 1000;
                if (rdoh2000.IsChecked.Equals(true)) newRow["PAGE_UNIT"] = 2000;
                if (rdoh5000.IsChecked.Equals(true)) newRow["PAGE_UNIT"] = 5000;
                if (rdoh10000.IsChecked.Equals(true)) newRow["PAGE_UNIT"] = 10000;
                if (rdoh20000.IsChecked.Equals(true)) newRow["PAGE_UNIT"] = 20000;

                inDataTable.Rows.Add(newRow);

                return inDataTable;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return null;
        }

        private void GetHistList(int pageNo)
        {
            try
            {
                DataTable inDataTable = GetInDataHist(pageNo);
                if (inDataTable == null) return;

                btnSearchHist.IsEnabled = false;
                btnExcelHist.IsEnabled = false;
                pageNaviHist.IsEnabled = false;

                dgMeasHistList.ExecuteService("DA_SEL_CELL_CURR_MEAS_HIST_PAGING", "INDATA", "OUTDATA", inDataTable, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetSummaryList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("SUBLOTID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));
                inDataTable.Columns.Add("CSTSLOT", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(DateTime));
                inDataTable.Columns.Add("TO_DATE", typeof(DateTime));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("FINL_JUDG_CODE", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROD_LOTID"] = txtLotIdSummary.GetBindValue();
                newRow["LOTID"] = txtTrayNoSummary.GetBindValue();
                newRow["CSTID"] = txtTrayIdSummary.GetBindValue();
                newRow["SUBLOTID"] = txtCellIDSummary.GetBindValue();
                newRow["CSTSLOT"] = txtCellNoSummary.GetBindValue();
                newRow["FINL_JUDG_CODE"] = txtGradeSummary.GetBindValue();
                newRow["EQPTID"] = cboEqpSummary.GetBindValue();

                if (chkNoDateSummary.IsChecked == true)
                {
                    newRow["FROM_DATE"] = DBNull.Value;
                    newRow["TO_DATE"] = DBNull.Value;

                    if (string.IsNullOrEmpty(txtTrayIdSummary.Text) && string.IsNullOrEmpty(txtTrayNoSummary.Text) && string.IsNullOrEmpty(txtCellIDSummary.Text) && string.IsNullOrEmpty(txtLotIdSummary.Text))
                    {                        
                        Util.Alert("FM_ME_0331"); //조회기간 미 지정시 Tray ID, Tray No, Cell ID, Lot ID 중 하나는 선택해야합니다.
                        return;
                    }
                }
                else
                {
                    newRow["FROM_DATE"] = dtpSummaryDate.SelectedFromDateTime.ToString("yyyy-MM-dd HH:mm:00");
                    newRow["TO_DATE"] = dtpSummaryDate.SelectedToDateTime.ToString("yyyy-MM-dd HH:mm:00");
                }

                inDataTable.Rows.Add(newRow);

                btnSearchSummary.IsEnabled = false;

                // 멀티 Thread 실행 작업완료 후 그리드 자동 바인딩, 이후 dgMeasSummaryList_QueryDataCompleted 이벤트로 이동.
                dgMeasSummaryList.ExecuteService("DA_SEL_CELL_CURR_MEAS_SUMMARY", "INDATA", "OUTDATA", inDataTable, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkNoDate_Checked(object sender, RoutedEventArgs e)
        {
            dtpFromDate.IsEnabled = false;
            dtpToDate.IsEnabled = false;
            dtpFromTime.IsEnabled = false;
            dtpToTime.IsEnabled = false;
        }

        private void chkNoDate_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpFromDate.IsEnabled = true;
            dtpToDate.IsEnabled = true;
            dtpFromTime.IsEnabled = true;
            dtpToTime.IsEnabled = true;
        }

        private void chkNoDateHist_Checked(object sender, RoutedEventArgs e)
        {
            dtpFromDateHist.IsEnabled = false;
            dtpToDateHist.IsEnabled = false;
            dtpFromTimeHist.IsEnabled = false;
            dtpToTimeHist.IsEnabled = false;
        }

        private void chkNoDateHist_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpFromDateHist.IsEnabled = true;
            dtpToDateHist.IsEnabled = true;
            dtpFromTimeHist.IsEnabled = true;
            dtpToTimeHist.IsEnabled = true;
        }

        private void dgMeasList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();
            if (e.Row.Index > 1)
            {
                tb.Text = (e.Row.Index + 1 - dgMeasList.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private void dgMeasHistList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();
            if (e.Row.Index > 1)
            {
                tb.Text = (e.Row.Index + 1 - dgMeasHistList.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        #endregion

        private void chkNoDateSummary_CheckedChanged(object sender, CMM001.Controls.UcBaseCheckBox.CheckedChangedEventArgs e)
        {
            dtpSummaryDate.IsEnabled = !e.NewValue;
        }

        private void btnSearchSummary_Click(object sender, RoutedEventArgs e)
        {
            if ((dtpSummaryDate.SelectedToDateTime.Date - dtpSummaryDate.SelectedFromDateTime.Date).Days >= 7)
            {
                Util.Alert("FM_ME_0231"); //조회기간은 7일을 초과할 수 없습니다.
                return;
            }

            GetSummaryList();
        }

        private void btnExcelSummary_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgMeasSummaryList_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            btnSearchSummary.IsEnabled = true;
        }
    }
}
