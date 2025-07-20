/*************************************************************************************
 Created Date : 2023.02.23
      Creator : 홍석원
   Decription : 작업 중 Lot 불량 현황 조회 화면
--------------------------------------------------------------------------------------
 [Change History]
  2023.02.23  홍석원 : Initial Created.
  2023.03.13  조영대 : 생산실적 레포트와 최대한 같게 하기 위하여 비즈룰에서 직행과 재작업을 따로 만들어 머지함.
  2023.03.30  조영대 : 라인 ALL 추가, LOT 유형 조건 추가
  2024.06.20  Meilia : E20240613-001554 LINE Multiselectionbox, add Combobox Shift(GDC)
  2024.07.03  Meilia : E20240613-001554 Improvement in the number and list of defect cells(GDC)
  2024.08.08  김상영 : E20240613-001554 DateTime Picker Toggle 기능 수정
**************************************************************************************/

using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF.DataGrid;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Controls;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// FCS001_152.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_152 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private DateTime fromDate = DateTime.Now;
        private DateTime toDate = DateTime.Now;

        private string eqsgId = string.Empty;
        private string lotType = string.Empty;
        private string shiftID = string.Empty; //2024.06.20 Meilia add shift ID

        private DataTable dtEqsgid = null;
        private DataSet dsResultData = null;

        public FCS001_152()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitControls();

            Loaded -= UserControl_Loaded;
        }
        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        //2024.06.20 Meilia comment old line segment due to changes
        //private void cboLine_SelectedIndexChanged(object sender, C1.WPF.PropertyChangedEventArgs<int> e)
        //{
        //    dgCharge1st.ClearRows();
        //    dgCharge1stSum.ClearRows();
        //    dgCharge2nd.ClearRows();
        //    dgCharge2ndSum.ClearRows();
        //    dgLowVolt.ClearRows();
        //    dgLowVoltSum.ClearRows();
        //    dgDegas.ClearRows();
        //    dgDegasSum.ClearRows();
        //    dgEol.ClearRows();
        //    dgEolSum.ClearRows();
        //}

        private void chkDirect_CheckedChanged(object sender, UcBaseCheckBox.CheckedChangedEventArgs e)
        {
            if (!e.NewValue)
            {
                if (chkRework != null && chkRework.IsChecked.Equals(false))
                {
                    chkDirect.IsChecked = true;
                    return;
                }
            }

            DisplayDataGrid();
        }

        private void chkRework_CheckedChanged(object sender, UcBaseCheckBox.CheckedChangedEventArgs e)
        {
            if (!e.NewValue)
            {
                if (chkDirect != null && chkDirect.IsChecked.Equals(false))
                {
                    chkRework.IsChecked = true;
                    return;
                }
            }

            DisplayDataGrid();
        }

        private void btnCharge1st_Click(object sender, RoutedEventArgs e)
        {
            if (btnCharge1st.Content.Equals("↗"))
            {
                btnCharge1st.Content = "↙";

                rdCharge1st.Height = new GridLength(1, GridUnitType.Star);
                rdCharge2nd.Height = new GridLength(0);
                rdLowVoltage.Height = new GridLength(0);
            }
            else
            {
                btnCharge1st.Content = "↗";

                rdCharge1st.Height = new GridLength(1, GridUnitType.Star);
                rdCharge2nd.Height = new GridLength(1, GridUnitType.Star);
                rdLowVoltage.Height = new GridLength(1, GridUnitType.Star);
            }
        }

        private void btnCharge2nd_Click(object sender, RoutedEventArgs e)
        {
            if (btnCharge2nd.Content.Equals("↗"))
            {
                btnCharge2nd.Content = "↙";

                rdCharge1st.Height = new GridLength(0);
                rdCharge2nd.Height = new GridLength(1, GridUnitType.Star);
                rdLowVoltage.Height = new GridLength(0);
            }
            else
            {
                btnCharge2nd.Content = "↗";

                rdCharge1st.Height = new GridLength(1, GridUnitType.Star);
                rdCharge2nd.Height = new GridLength(1, GridUnitType.Star);
                rdLowVoltage.Height = new GridLength(1, GridUnitType.Star);
            }
        }

        private void btnLowVolt_Click(object sender, RoutedEventArgs e)
        {
            if (btnLowVolt.Content.Equals("↗"))
            {
                btnLowVolt.Content = "↙";

                rdCharge1st.Height = new GridLength(0);
                rdCharge2nd.Height = new GridLength(0);
                rdLowVoltage.Height = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                btnLowVolt.Content = "↗";

                rdCharge1st.Height = new GridLength(1, GridUnitType.Star);
                rdCharge2nd.Height = new GridLength(1, GridUnitType.Star);
                rdLowVoltage.Height = new GridLength(1, GridUnitType.Star);
            }
        }

        private void cboLotType_SelectionChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cboLotType.GetStringValue())) cboLotType.CheckAll();
        }

        private void dgCommon_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null) return;

                C1DataGrid dg = sender as C1DataGrid;

                dg?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null) return;

                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (e.Cell.Column.Name.ToString() == "LOSS")
                        {
                            e.Cell.Presenter.Foreground = System.Windows.Media.Brushes.Blue;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = System.Windows.Media.Brushes.Black;
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCommon_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null) return;

                System.Windows.Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null) return;
                if (cell.Column.Name == "LOSS")
                {
                    Int32 cellValue = 0;
                    if (!Int32.TryParse(Util.NVC(cell.Value), out cellValue)) return;
                    if (cellValue <= 0) return;

                    string eqptId = Util.NVC(dg.GetValue(cell.Row.Index, "EQPTID"));
                    string workType = Util.NVC(dg.GetValue(cell.Row.Index, "WORK_TYPE_CODE"));

                    FCS001_152_DEFECT defectPopup = new FCS001_152_DEFECT();
                    defectPopup.FrameOperation = FrameOperation;

                    if (defectPopup != null)
                    {
                        object[] Parameters = new object[9]; // 2024.07.03 Meilia Add shiftID
                        Parameters[0] = Util.NVC(dg.Tag);
                        Parameters[1] = fromDate.ToString("yyyyMMdd");
                        Parameters[2] = toDate.ToString("yyyyMMdd");
                        Parameters[3] = eqsgId.Equals(string.Empty) ? null : eqsgId;
                        Parameters[4] = lotType.Equals(string.Empty) ? null : lotType;

                        if (dg.Name.ToUpper().Contains("SUM"))
                        {
                            Parameters[5] = null;
                            if (chkDirect.IsChecked.Equals(true) && chkRework.IsChecked.Equals(true))
                            {
                                Parameters[6] = true;
                                Parameters[7] = true;
                            }
                            else if (chkDirect.IsChecked.Equals(true))
                            {
                                Parameters[6] = true;
                                Parameters[7] = false;

                            }
                            else if (chkRework.IsChecked.Equals(true))
                            {
                                Parameters[6] = false;
                                Parameters[7] = true;
                            }
                        }
                        else
                        {
                            Parameters[5] = eqptId;
                            if (workType.Equals("0"))
                            {
                                Parameters[6] = true;
                                Parameters[7] = false;
                            }
                            else
                            {
                                Parameters[6] = false;
                                Parameters[7] = true;
                            }
                        }

                        //2024.07.03 Meilia add ShiftID
                        Parameters[8] = shiftID.Equals(string.Empty) ? null : shiftID;

                        C1WindowExtension.SetParameters(defectPopup, Parameters);

                        this.Dispatcher.BeginInvoke(new Action(() => { defectPopup.ShowModal(); }));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private object xProgress_WorkProcess(object sender, UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                if (dsResultData != null)
                {
                    dsResultData.Clear();
                    dsResultData.Tables.Clear();
                    dsResultData.AcceptChanges();
                    dsResultData = null;
                }

                string LANGID = LoginInfo.LANGID;

                DateTime dFromDate = fromDate;
                DateTime dToDate = toDate;

                string AREAID = LoginInfo.CFG_AREA_ID;
                string EQSGID = eqsgId;
                string LOTTYPE = lotType;
                string SHFT_ID = shiftID; //2024.06.20 Meilia add SHIFTID

                TimeSpan tsDateDiff = dToDate - dFromDate;

                int totalCount = (tsDateDiff.Days + 1);

                DataSet indataSet = new DataSet();
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));
                dtRqst.Columns.Add("SHFT_ID", typeof(string)); //2024.06.20 Meilia add SHIFTID

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LANGID;
                dr["FROM_DATE"] = string.Empty;
                dr["TO_DATE"] = string.Empty;
                dr["AREAID"] = AREAID;
                if (!string.IsNullOrEmpty(EQSGID)) dr["EQSGID"] = EQSGID;
                if (!string.IsNullOrEmpty(LOTTYPE)) dr["LOTTYPE"] = LOTTYPE;
                if (!string.IsNullOrEmpty(SHFT_ID)) dr["SHFT_ID"] = SHFT_ID; //2024.06.20 Meilia add SHIFTID

                dtRqst.Rows.Add(dr);
                indataSet.Tables.Add(dtRqst);

                //string returnTableList = "FORMATION1_CHARGE,FORMATION1_CHARGE_REWORK,DEGAS,DEGAS_REWORK,FORMATION2_CHARGE,FORMATION2_CHARGE_REWORK,GRADE,GRADE_REWORK,QUALITY,QUALITY_REWORK,GRADE2,GRADE2_REWORK";
                string returnTableList = "BFDGS_FRST,BFDGS_RWK,DGS_FRST,DGS_RWK,AFDGS_FRST,AFDGS_RWK,DOCV_FRST,DOCV_RWK,EOL_FRST,EOL_RWK";
                int runCount = 0;

                ClientProxy client = new ClientProxy();

                for (int i = 0; i <= tsDateDiff.Days; i++)
                {
                    dtRqst.Rows[0]["FROM_DATE"] = dFromDate.AddDays(i).ToString("yyyy-MM-dd");
                    dtRqst.Rows[0]["TO_DATE"] = dFromDate.AddDays(i).ToString("yyyy-MM-dd");

                    runCount++;

                    e.Worker.ReportProgress(runCount * 100 / totalCount, "[" + dFromDate.AddDays(i).ToString("yyyy-MM-dd") + "] - " + ObjectDic.Instance.GetObjectName("WORKING") + ".....");

                    DataSet bizResult = client.ExecuteServiceSync_Multi("BR_SEL_PROD_PERF_REPORT_BY_DAY_EQPT", "INDATA", returnTableList, indataSet);
                    if (dsResultData == null)
                    {
                        dsResultData = bizResult.Clone();
                    }

                    MergeTables(dsResultData.Tables["BFDGS_FRST"], bizResult.Tables["BFDGS_FRST"]);
                    MergeTables(dsResultData.Tables["BFDGS_RWK"], bizResult.Tables["BFDGS_RWK"]);

                    MergeTables(dsResultData.Tables["AFDGS_FRST"], bizResult.Tables["AFDGS_FRST"]);
                    MergeTables(dsResultData.Tables["AFDGS_RWK"], bizResult.Tables["AFDGS_RWK"]);

                    MergeTables(dsResultData.Tables["DOCV_FRST"], bizResult.Tables["DOCV_FRST"]);
                    MergeTables(dsResultData.Tables["DOCV_RWK"], bizResult.Tables["DOCV_RWK"]);

                    MergeTables(dsResultData.Tables["DGS_FRST"], bizResult.Tables["DGS_FRST"]);
                    MergeTables(dsResultData.Tables["DGS_RWK"], bizResult.Tables["DGS_RWK"]);

                    MergeTables(dsResultData.Tables["EOL_FRST"], bizResult.Tables["EOL_FRST"]);
                    MergeTables(dsResultData.Tables["EOL_RWK"], bizResult.Tables["EOL_RWK"]);

                }

                return dsResultData;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private void xProgress_WorkProcessChanged(object sender, int percent, UcProgress.WorkProcessEventArgs e)
        {
            xProgress.Percent = percent;
            xProgress.ProgressText = Util.NVC(e.Arguments);
        }

        private void xProgress_WorkProcessCompleted(object sender, UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                if (e.Result is DataSet)
                {
                    DisplayDataGrid();
                }
                else if (e.Result is Exception)
                {
                    Util.MessageException((Exception)e.Result);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                xProgress.Visibility = Visibility.Collapsed;

                btnSearch.IsEnabled = true;
                gdCondition.IsEnabled = true;

                HiddenLoadingIndicator();
            }
        }

        #endregion

        #region Method

        private void InitControls()
        {
            InitCombo();
            dtpSearchDate.SelectedFromDateTime = DateTime.Now;
        }

        private void InitCombo()
        {
            try
            {
                CommonCombo_Form _combo = new CommonCombo_Form();
                //Meilia 2024.06.20 Init multibox line, shift ID
                //_combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE");
                SetLineCombo(cboLine);
                _combo.SetCombo(cboShift, CommonCombo_Form.ComboStatus.ALL, sCase: "FORM_SHIFT");

                SetLotTypeCombo(cboLotType);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        //Meilia 2024.06.20 muntiselectionbox LINE
        private void SetLineCombo(MultiSelectionBox mcb)
        {
            try
            {
                DataTable dtRqstA = new DataTable();
                dtRqstA.TableName = "RQSTDT";
                dtRqstA.Columns.Add("LANGID", typeof(string));
                dtRqstA.Columns.Add("AREAID", typeof(string));

                DataRow drA = dtRqstA.NewRow();
                drA["LANGID"] = LoginInfo.LANGID;
                drA["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqstA.Rows.Add(drA);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE", "RQSTDT", "RSLTDT", dtRqstA);

                if (dtResult.Rows.Count != 0)
                {
                    mcb.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.Check(-1);
                    }
                    else
                    {
                        mcb.isAllUsed = true;
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.CheckAll();
                    }
                }
                else
                {
                    mcb.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        private void SetLotTypeCombo(MultiSelectionBox mcb)
        {
            try
            {
                DataTable dtRqstA = new DataTable();
                dtRqstA.TableName = "RQSTDT";
                dtRqstA.Columns.Add("LANGID", typeof(string));

                DataRow drA = dtRqstA.NewRow();
                drA["LANGID"] = LoginInfo.LANGID;
                dtRqstA.Rows.Add(drA);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTTYPE_CBO", "RQSTDT", "RSLTDT", dtRqstA);

                if (dtResult.Rows.Count != 0)
                {
                    mcb.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.Check(-1);
                    }
                    else
                    {
                        mcb.isAllUsed = true;
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.CheckAll();
                    }
                }
                else
                {
                    mcb.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetList()
        {
            try
            {

                fromDate = dtpSearchDate.SelectedFromDateTime;
                toDate = dtpSearchDate.SelectedToDateTime;
                eqsgId = cboLine.GetStringValue();
                lotType = cboLotType.GetStringValue();
                shiftID = cboShift.GetStringValue(); //2024.06.20 Meilia add SHIFTID



                dgCharge1st.ClearRows();
                dgCharge1stSum.ClearRows();
                dgCharge2nd.ClearRows();
                dgCharge2ndSum.ClearRows();
                dgLowVolt.ClearRows();
                dgLowVoltSum.ClearRows();
                dgDegas.ClearRows();
                dgDegasSum.ClearRows();
                dgEol.ClearRows();
                dgEolSum.ClearRows();

                if (dtpSearchDate.IsFromTo)
                {
                    xProgress.Clear();
                    xProgress.ProgressText = MessageDic.Instance.GetMessage("10057");
                    xProgress.Visibility = Visibility.Visible;
                }

                btnSearch.IsEnabled = false;
                gdCondition.IsEnabled = false;

                ShowLoadingIndicator();

                xProgress.RunWorker();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void DisplayDataGrid()
        {
            if (dsResultData == null || dsResultData.Tables.Count == 0) return;

            SetDataGrid(dgCharge1st, dgCharge1stSum, "BFDGS_FRST", "BFDGS_RWK");
            SetDataGrid(dgCharge2nd, dgCharge2ndSum, "AFDGS_FRST", "AFDGS_RWK");
            SetDataGrid(dgLowVolt, dgLowVoltSum, "DOCV_FRST", "DOCV_RWK");
            SetDataGrid(dgDegas, dgDegasSum, "DGS_FRST", "DGS_RWK");
            SetDataGrid(dgEol, dgEolSum, "EOL_FRST", "EOL_RWK");
        }

        private DataTable MergeTables(DataTable dtTable1, DataTable dtTable2)
        {
            DataTable dtResult = dtTable1;
            if (!dtResult.Columns.Contains("YIELD")) dtResult.Columns.Add("YIELD", typeof(decimal));

            // EQPTID Null 일때 처리
            dtResult.AsEnumerable().Where(x => x.Field<string>("EQPTID") == null).ToList<DataRow>().ForEach(row => row["EQPTID"] = "NO EQP");

            foreach (DataRow drMerge in dtTable2.Rows)
            {
                DataRow findRow = dtResult.AsEnumerable()
                    .Where(x => x.Field<string>("EQPTID").Equals(drMerge["EQPTID"]) && x.Field<string>("WORK_TYPE_CODE").Equals(drMerge["WORK_TYPE_CODE"]))
                    .FirstOrDefault();
                if (findRow == null)
                {
                    object a = drMerge["INPUT"].GetType();

                    DataRow newRow = dtResult.NewRow();
                    newRow["EQPTID"] = drMerge["EQPTID"];
                    newRow["EQPTNAME"] = drMerge["EQPTNAME"];
                    newRow["WORK_TYPE_CODE"] = drMerge["WORK_TYPE_CODE"];
                    newRow["WORK_TYPE_NAME"] = drMerge["WORK_TYPE_NAME"];
                    newRow["INPUT"] = drMerge["INPUT"];
                    newRow["GOOD"] = drMerge["GOOD"];
                    newRow["REWORK"] = drMerge["REWORK"];
                    newRow["LOSS"] = drMerge["LOSS"];

                    double good_cnt = (Int64)drMerge["GOOD"];
                    double input_cnt = (Int64)drMerge["INPUT"];
                    double ratio = Math.Round(good_cnt / input_cnt, 2);
                    newRow["YIELD"] = (Int64)drMerge["INPUT"] <= 0 ? 0 : (ratio) * 100;
                    dtResult.Rows.Add(newRow);
                }
                else
                {
                    object a = findRow["INPUT"].GetType().FullName;

                    double good_cnt = (Int64)findRow["GOOD"];
                    double input_cnt = (Int64)findRow["INPUT"];
                    double ratio;
                    if (input_cnt == 0)
                    {
                        ratio = 0;
                    }
                    else
                    {
                         ratio = Math.Round(good_cnt / input_cnt, 2);
                    }
                    findRow["INPUT"] = (Int64)findRow["INPUT"] + (Int64)drMerge["INPUT"];
                    findRow["GOOD"] = (Int64)findRow["GOOD"] + (Int64)drMerge["GOOD"];
                    findRow["LOSS"] = (Int64)findRow["LOSS"] + (Int64)drMerge["LOSS"];
                    findRow["YIELD"] = (Int64)findRow["INPUT"] <= 0 ? 0 : (ratio) * 100;
                }
            }

            dtResult.AcceptChanges();

            return dtResult;
        }

        private void SetDataGrid(C1DataGrid dg, C1DataGrid dgSum, string direct, string rework)
        {
            DataTable dtBind = null;
            DataTable dt = dsResultData.Tables[direct].Copy();
            if (chkDirect.IsChecked.Equals(true) && chkRework.IsChecked.Equals(true))
            {
                dtBind = MergeTables(dt, dsResultData.Tables[rework]);
            }
            else if (chkDirect.IsChecked.Equals(true))
            {
                dtBind = dsResultData.Tables[direct];
            }
            else if (chkRework.IsChecked.Equals(true))
            {
                dtBind = dsResultData.Tables[rework];
            }

            dg.SetItemsSource(dtBind, FrameOperation);
            dgSum.SetItemsSource(SumTable(dtBind), FrameOperation);

        }

        private DataTable SumTable(DataTable dt)
        {
            DataTable dtSum = dt.Clone();
            dtSum.TableName = dtSum.TableName + "_SUM";
            dtSum.Columns.Add("DFCT_SUM", typeof(decimal));

            DataRow sumRow = dtSum.NewRow();
            if (chkDirect.IsChecked.Equals(true) && chkRework.IsChecked.Equals(true))
            {
                // 총 생산량 = 직행 투입
                sumRow["INPUT"] = dt.AsEnumerable().Where(s => s.Field<string>("WORK_TYPE_CODE").Equals("0")).Sum(s => s.Field<Int64>("INPUT"));
                // 총 양품량 = 직행 투입 + 재작업 투입 - 직행 불량 - 재작업 불량
                sumRow["GOOD"] = dt.AsEnumerable().Sum(s => s.Field<Int64>("GOOD"));
                // 총 불량 = 총 생산량 - 총 양품량
                sumRow["DFCT_SUM"] = (Int64)sumRow["INPUT"] - (Int64)sumRow["GOOD"];
            }
            else
            {
                sumRow["INPUT"] = dt.AsEnumerable().Sum(s => s.Field<Int64>("INPUT"));
                sumRow["GOOD"] = dt.AsEnumerable().Sum(s => s.Field<Int64>("GOOD"));
                // 총 불량 = 불량 수량
                sumRow["DFCT_SUM"] = dt.AsEnumerable().Sum(s => s.Field<Int64>("LOSS"));
            }

            // 불량합계
            sumRow["LOSS"] = dt.AsEnumerable().Sum(s => s.Field<Int64>("LOSS"));

            // 불량률
            if (sumRow["INPUT"].Equals(decimal.Zero))
            {
                sumRow["YIELD"] = 0;
            }
            else if(Util.NVC_Decimal(sumRow["INPUT"]) == 0)
            {
                sumRow["YIELD"] = 0;
            }
            else
            {
                double good_int = (Int64)sumRow["GOOD"];
                double input_cnt = (Int64)sumRow["INPUT"];
                double ratio = Math.Round(good_int / input_cnt, 2);
                sumRow["YIELD"] = (ratio) * 100;
            }
            dtSum.Rows.Add(sumRow);

            dtSum.AcceptChanges();

            return dtSum;
        }

        private DataTable SumTable(DataTable dtDir, DataTable dtRework)
        {
            DataTable dtSum = dtDir.Clone();
            dtSum.TableName = dtSum.TableName + "_SUM";
            dtSum.Columns.Add("WORK_TYPE", typeof(string));
            dtSum.Columns.Add("WORK_CODE", typeof(string));
            dtSum.Columns.Add("DFCT_SUM", typeof(decimal));

            DataRow sumDir = dtSum.NewRow();
            sumDir["WORK_TYPE"] = ObjectDic.Instance.GetObjectName("직행 실적");
            sumDir["WORK_CODE"] = "DIRECT";
            sumDir["INPUT"] = dtDir.AsEnumerable().Sum(s => s.Field<decimal>("INPUT"));
            sumDir["GOOD"] = dtDir.AsEnumerable().Sum(s => s.Field<decimal>("GOOD"));
            sumDir["LOSS"] = dtDir.AsEnumerable().Sum(s => s.Field<decimal>("LOSS"));
            if (sumDir["INPUT"].Equals(decimal.Zero))
            {
                sumDir["YIELD"] = 0;
            }
            else
            {
                sumDir["YIELD"] = ((decimal)sumDir["GOOD"] / (decimal)sumDir["INPUT"]) * 100;
            }
            sumDir["DFCT_SUM"] = 0;

            DataRow sumRework = dtSum.NewRow();
            sumRework["WORK_TYPE"] = ObjectDic.Instance.GetObjectName("재작업 실적");
            sumRework["WORK_CODE"] = "REWORK";
            sumRework["INPUT"] = dtRework.AsEnumerable().Sum(s => s.Field<decimal>("INPUT"));
            sumRework["GOOD"] = dtRework.AsEnumerable().Sum(s => s.Field<decimal>("GOOD"));
            sumRework["LOSS"] = dtRework.AsEnumerable().Sum(s => s.Field<decimal>("LOSS"));
            if (sumRework["INPUT"].Equals(decimal.Zero))
            {
                sumRework["YIELD"] = 0;
            }
            else
            {
                sumRework["YIELD"] = ((decimal)sumRework["GOOD"] / (decimal)sumRework["INPUT"]) * 100;
            }

            if (chkDirect.IsChecked.Equals(true) && chkRework.IsChecked.Equals(true))
            {
                dtSum.Rows.Add(sumDir);
                dtSum.Rows.Add(sumRework);
            }
            else if (chkDirect.IsChecked.Equals(true))
            {
                dtSum.Rows.Add(sumDir);
            }
            else if (chkRework.IsChecked.Equals(true))
            {
                dtSum.Rows.Add(sumRework);
            }

            dtSum.Rows.Add(sumDir);

            dtSum.AcceptChanges();

            return dtSum;
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

        #endregion
        private void cboLine_SelectionChanged(object sender, EventArgs e)
        {
            if (cboLine.SelectedItems.Count == 0)
            {
                cboLine.CheckAll();
            }
        }
    }
}
