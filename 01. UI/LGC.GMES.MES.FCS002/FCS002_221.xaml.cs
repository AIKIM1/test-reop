/*************************************************************************************
 Created Date : 2023.06.01
      Creator : Kim Taekyun
   Decription : LOT별 일일 실적
--------------------------------------------------------------------------------------
 [Change History]
  2023.06.01  NAME            Initial Created
  2024.01.17  주훈            양품 수량  및 1등급 추가, 조회 조건에 공정 그룹 추가
  2024.01.23  주훈            조회 조건에 공정 그룹, 작업 공정, 조회 기준 삭제
                              BizRule 변경
  2024.01.24  주훈             X 등급 제외
  2024.03.13  주훈            합계 구현 방법 변경
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using static LGC.GMES.MES.CMM001.Controls.UcBaseDataGrid;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_221 : UserControl,IWorkArea
    {
        #region [Declaration & Constructor]
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;

        System.ComponentModel.BackgroundWorker bgWorker = null;
        #endregion

        #region [Initialize]
        public FCS002_221()
        {
            InitializeComponent();

            bgWorker = new System.ComponentModel.BackgroundWorker();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.ProgressChanged += BgWorker_ProgressChanged;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetWorkResetTime();
            //Combo Setting            
            InitCombo();
            //Control Setting
            InitControl();

            // 2024.03.13 추가
            InitSpread();

            this.Loaded -= UserControl_Loaded;
        }

        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form_MB ComCombo = new CommonCombo_Form_MB();

            // 202\4.01.23 조회 조건에 공정 그룹, 작업 공정, 조회 기준 삭제
            //C1ComboBox[] cboLineChild = { cboModel, cboProcGrpCode };
            C1ComboBox[] cboLineChild = { cboModel };
            ComCombo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelChild = { cboRoute };
            C1ComboBox[] cboModelParent = { cboLine };
            ComCombo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            // 202\4.01.23 조회 조건에 공정 그룹, 작업 공정, 조회 기준 삭제
            //C1ComboBox[] cboRouteChild = { cboOper };
            //ComCombo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent, cbChild: cboRouteChild);
            ComCombo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent);

            // 202\4.01.23 조회 조건에 공정 그룹, 작업 공정, 조회 기준 삭제
            //// 202\4.01.17 공정 그룹 추가
            //// 공정 그룹
            //C1ComboBox[] cboProcGrParent = { cboLine };
            //C1ComboBox[] cboProcGrChild = { cboOper };
            //string[] sProcGrFilter = { "PROC_GR_CODE_MB", LoginInfo.CFG_AREA_ID };
            //ComCombo.SetCombo(cboProcGrpCode, CommonCombo_Form_MB.ComboStatus.ALL, cbChild: cboProcGrChild, cbParent: cboProcGrParent, sFilter: sProcGrFilter, sCase: "PROCGRP_BY_LINE");

            // 202\4.01.23 조회 조건에 공정 그룹, 작업 공정, 조회 기준 삭제
            //// 202\4.01.17 공정 그룹 추가에 따른 수정
            ////C1ComboBox[] cboOperParent = { cboRoute };
            ////ComCombo.SetCombo(cboOper, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "ROUTE_OP", cbParent: cboOperParent);
            //C1ComboBox[] cboOperParent = { cboRoute, cboProcGrpCode };
            //ComCombo.SetCombo(cboOper, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE_OP", cbParent: cboOperParent);        

            // 202\4.01.23 조회 조건에 공정 그룹, 작업 공정, 조회 기준 삭제
            //string[] sFilter = { "FORM_SEARCH_ORDERBY", "ORDER_START_TIME,ORDER_END_TIME" }; //E07
            //ComCombo.SetCombo(cboOrder, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter);
        }

        private void InitControl()
        {
            // Util 에 해당 함수 추가 필요.
            dtpFromDate.SelectedDateTime = GetJobDateFrom();
            dtpFromTime.DateTime = GetJobDateFrom();
            dtpToDate.SelectedDateTime = GetJobDateTo();
            dtpToTime.DateTime = GetJobDateTo();
        }
    
        private void InitSpread()
        {
            try
            {
                // 2024.03.13 DataGridSummaryRow로 합계 정보 구현
                foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgOperResult.Columns)
                {
                    switch (dgc.Name)
                    {
                        case "ROUTID":
                            //  Summary Row에 정보를 표시하지 않음
                            break;

                        case "PROD_LOTID":
                            DataGridAggregate.SetAggregateFunctions(dgc,
                                new DataGridAggregatesCollection { new DataGridAggregateText("합계") { ResultTemplate = grdMain.Resources["ResultTemplateSum"] as DataTemplate } });

                            // dgOperResult_LoadedCellPresenter에서 Summary Row에 따라 비율(%)로 표시함
                            break;

                        default:
                            // 합계 정보
                            DataGridAggregate.SetAggregateFunctions(dgc,
                                new DataGridAggregatesCollection { new DataGridAggregateSum { ResultTemplate = grdMain.Resources["ResultTemplate"] as DataTemplate } });

                            // dgOperResult_LoadedCellPresenter에서 Summary Row에 따라 비율 정보로 표시함
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Event]

        private void dgOperResult_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            // 2024.03.13 추가
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                // 2024.01.17 합계와 비율 색 불리
                //if ((e.Cell.Row.Index == dg.Rows.Count - 1) || (e.Cell.Row.Index == dg.Rows.Count - 2))
                //{
                //    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF7E9D5"));
                //}

                // 2024.03.13 삭제
                //if (e.Cell.Row.Index == dg.Rows.Count - 2)
                //    e.Cell.Presenter.Background = new SolidColorBrush(Colors.PapayaWhip);
                //if (e.Cell.Row.Index == dg.Rows.Count - 1)
                //    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Aquamarine);

                // 2024.03.13 추가
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////
                }

                // 2024.03.13 추가
                if (e.Cell.Row.GetType() == typeof(DataGridSummaryRow))
                {
                    if (e.Cell.Row.Index == dataGrid.Rows.Count - 2)
                    {
                        e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.PapayaWhip);
                    }
                    else
                    {
                        e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.Aquamarine);

                        switch (e.Cell.Column.Name)
                        {
                            case "INPUT_QTY":
                                // 비율 Row 에서 Summary 정로를 지움
                                dgOperResult.SetSummaryRowValue(1, e.Cell.Column.Name, String.Empty);
                                break;

                            case "PROD_LOTID":
                                dgOperResult.SetSummaryRowValue(1, e.Cell.Column.Name, ObjectDic.Instance.GetObjectName("PERCENT_VAL"));
                                break;

                            default:
                                decimal calcValue = 0;
                                decimal sumInput = 0;

                                if (String.IsNullOrEmpty(dgOperResult.GetSummaryRowValue(0, e.Cell.Column.Name)) == true)
                                    break;

                                if (dgOperResult.GetDataTable() != null)
                                    sumInput = Util.NVC_Decimal(dgOperResult.GetDataTable().Compute("Sum(INPUT_QTY)", ""));
                                decimal sumValue = Util.NVC_Int(dgOperResult.GetSummaryRowValue(0, e.Cell.Column.Name).Replace(",", ""));

                                if (sumInput.Equals(0) == false)
                                    calcValue = Math.Round(sumValue * 100 / sumInput, 2);
                                dgOperResult.SetSummaryRowValue(1, e.Cell.Column.Name, calcValue.ToString("#,##0.00"));
                                break;
                        }
                    }
                }
            }));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ColumnClear();

            dgOperResult.ItemsSource = null;

            btnSearch.IsEnabled = false;

            object[] argument = new object[8];
            argument[0] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd") + dtpFromTime.DateTime.Value.ToString("HHmm00");
            argument[1] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd") + dtpToTime.DateTime.Value.ToString("HHmm00");
            argument[2] = Util.GetCondition(cboLine, bAllNull: true);
            argument[3] = Util.GetCondition(cboModel, bAllNull: true);
            argument[4] = Util.GetCondition(cboRoute, bAllNull: true);

            // 202\4.01.23 조회 조건에 공정 그룹, 작업 공정, 조회 기준 삭제 및 BizRle 변경
            //argument[5] = Util.GetCondition(cboOper, bAllNull: true);
            //argument[6] = Util.GetCondition(cboOrder, bAllNull: true);
            argument[7] = Util.GetCondition(chkHist, bAllNull: true);

            xProgress.Percent = 0;
            xProgress.ProgressText = string.Empty;
            xProgress.Visibility = Visibility.Visible;

            if (!bgWorker.IsBusy)
            {
                bgWorker.RunWorkerAsync(argument);
            }
        }

        private void BgWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            e.Result = GetList(e.Argument);
        }

        private void BgWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            xProgress.Percent = e.ProgressPercentage;
            xProgress.ProgressText = e.UserState == null ? "" : e.UserState.ToString();
        }

        private void BgWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null && e.Result is Exception)
                {
                    Util.MessageException((Exception)e.Result);
                }
                else if (e.Result != null && e.Result is DataTable)
                {
                    DataTable dt = (DataTable)e.Result;

                    if (dt.Rows.Count > 0)
                    {
                        // 2024.03.13 DataGridSummaryRow 구현으로 삭제
                        //SetBottomRow(dt);

                        Util.GridSetData(dgOperResult, dt, FrameOperation, true);

                        //수량 0인 Column Visible 조정
                        DataTable OperDt = DataTableConverter.Convert(dgOperResult.ItemsSource);
                        String ColumnName;

                        for (int i = 3; i < OperDt.Columns.Count; i++)
                        {
                            // 2024.01.17 Collapsed 로직 수정
                            ColumnName = OperDt.Columns[i].ColumnName;
                            if (dgOperResult.Columns.Contains(ColumnName) == false)
                                continue;

                            int sum = 0;
                            for (int j = 0; j < OperDt.Rows.Count; j++)
                            {
                                sum += Util.NVC_Int(OperDt.Rows[j][i]);
                            }

                            if (sum == 0)
                            {
                                // 2024.01.17 Collapsed 로직 수정
                                //dgOperResult.Columns[OperDt.Columns[i].ColumnName].Visibility = Visibility.Collapsed;
                                dgOperResult.Columns[ColumnName].Visibility = Visibility.Collapsed;
                            }
                        }
                    } 
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            xProgress.Visibility = Visibility.Collapsed;
            xProgress.Percent = 0;

            btnSearch.IsEnabled = true;
        }

        #endregion

        #region [Method]

        private void ColumnClear()
        {
            DataTable OperDt = DataTableConverter.Convert(dgOperResult.ItemsSource);
            String ColumnName;

            ClearFilterGrid(); // 2024.03.13 추가

            for (int i = 3; i < OperDt.Columns.Count; i++)
            {
                // 2024.01.17 Visible 로직 수정
                //dgOperResult.Columns[OperDt.Columns[i].ColumnName].Visibility = Visibility.Visible;

                ColumnName = OperDt.Columns[i].ColumnName;
                if (dgOperResult.Columns.Contains(ColumnName) == false)
                    continue;

                dgOperResult.Columns[ColumnName].Visibility = Visibility.Visible;
            }
        }

        private void ClearFilterGrid()
        {
            try
            {
                // dgOperResult.ClearFilter(); 동기화로 변경
                if ((dgOperResult.ItemsSource != null) && (dgOperResult.Columns.Count > 0))
                {
                    dgOperResult.FilterBy(dgOperResult.Columns[0], null);
                }
            }
            finally { }
        }

        private object GetList(object arg)
        {
            try
            {
                object[] argument = (object[])arg;

                string bizName = string.Empty;
                string FROM_DATE = argument[0].ToString();
                string TO_DATE = argument[1].ToString();
                string EQSGID = argument[2] == null ? null : argument[2].ToString();
                string MDLLOT_ID = argument[3] == null ? null : argument[3].ToString();
                string ROUTID = argument[4] == null ? null : argument[4].ToString();

                // 202\4.01.23 조회 조건에 공정 그룹, 작업 공정, 조회 기준 삭제 및 BizRle 변경
                //string PROCID = argument[5] == null ? null : argument[5].ToString();
                //string ORDER = argument[6] == null ? null : argument[6].ToString();
                string HIST = argument[7] == null ? null : argument[7].ToString();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));

                // 202\4.01.23 조회 조건에 공정 그룹, 작업 공정, 조회 기준 삭제 및 BizRle 변경
                //dtRqst.Columns.Add("PROCID", typeof(string));
                //dtRqst.Columns.Add(ORDER, typeof(string));
                dtRqst.Columns.Add("HISTORY_YN", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["FROM_DATE"] = FROM_DATE;
                dr["TO_DATE"] = TO_DATE;
                dr["EQSGID"] = EQSGID;
                dr["MDLLOT_ID"] = MDLLOT_ID;
                dr["ROUTID"] = ROUTID;

                // 202\4.01.23 조회 조건에 공정 그룹, 작업 공정, 조회 기준 삭제 및 BizRle 변경
                //dr["PROCID"] = PROCID;
                //dr[ORDER] = "Y";
                if (HIST.Equals("N"))
                    dr["HISTORY_YN"] = "Y";

                dtRqst.Rows.Add(dr);

                // 202\4.01.23 조회 조건에 공정 그룹, 작업 공정, 조회 기준 삭제 및 BizRle 변경
                //if (HIST.Equals("Y"))
                //    bizName = "DA_SEL_LOAD_LOT_RESULT_VISION_HIST_MB";
                //else
                //    bizName = "DA_SEL_LOAD_LOT_RESULT_VISION_MB";
                bizName = "DA_SEL_LOAD_LOT_RESULT_VISION_MB_NFF";

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizName, "RQSTDT", "RSLTDT", dtRqst);

                return dtRslt;
            }
            catch (Exception ex)
            {
                return ex;
            }

            return null;
        }

        // 공통함수로 뺄지 확인 필요 START
        private DateTime GetJobDateFrom(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkReSetTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkReSetTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private DateTime GetJobDateTo(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkEndTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private void SetWorkResetTime()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREAATTR_FOR_OPENTIME", "RQSTDT", "RSLTDT", dtRqst);
            sWorkReSetTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
            sWorkEndTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
        }

        private void SetBottomRow(DataTable dt)
        {
            decimal dINPUT_SUM = 0;
            DataRow dr1 = dt.NewRow();
            dr1["PROD_LOTID"] = ObjectDic.Instance.GetObjectName("합계");

            DataRow dr2 = dt.NewRow();
            dr2["PROD_LOTID"] = ObjectDic.Instance.GetObjectName("비율");

            int colIdx = dt.Columns.IndexOf("INPUT_QTY");
            for (int i = colIdx; i < dt.Columns.Count; i++)
            {
                int sum = 0;
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    sum += Util.NVC_Int(dt.Rows[j][i]);
                }
                dr1[i] = sum;

                if (i == colIdx)
                {
                    dINPUT_SUM = sum;
                }
                else
                {
                    if (dINPUT_SUM == 0)
                        dr2[i] = 0;
                    else
                        dr2[i] = (sum / dINPUT_SUM) * 100;
                }
            }

            dt.Rows.Add(dr1);
            dt.Rows.Add(dr2);
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

        private void dgOperResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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

                if (cell.Text == datagrid.CurrentColumn.Header.ToString()) return;

                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgOperResult.CurrentRow.DataItem, "ROUTID")))) return;

                if (dgOperResult.CurrentColumn.Name.Equals("B_GRD_QTY") ||
                    dgOperResult.CurrentColumn.Name.Equals("C_GRD_QTY") || dgOperResult.CurrentColumn.Name.Equals("D_GRD_QTY") ||
                    dgOperResult.CurrentColumn.Name.Equals("E_GRD_QTY") || dgOperResult.CurrentColumn.Name.Equals("F_GRD_QTY") ||
                    dgOperResult.CurrentColumn.Name.Equals("G_GRD_QTY") || dgOperResult.CurrentColumn.Name.Equals("H_GRD_QTY") ||
                    dgOperResult.CurrentColumn.Name.Equals("I_GRD_QTY") || dgOperResult.CurrentColumn.Name.Equals("J_GRD_QTY") ||
                    dgOperResult.CurrentColumn.Name.Equals("K_GRD_QTY") || dgOperResult.CurrentColumn.Name.Equals("L_GRD_QTY") ||
                    dgOperResult.CurrentColumn.Name.Equals("M_GRD_QTY") || dgOperResult.CurrentColumn.Name.Equals("N_GRD_QTY") ||
                    dgOperResult.CurrentColumn.Name.Equals("O_GRD_QTY") || dgOperResult.CurrentColumn.Name.Equals("P_GRD_QTY") ||
                    dgOperResult.CurrentColumn.Name.Equals("Q_GRD_QTY") || dgOperResult.CurrentColumn.Name.Equals("R_GRD_QTY") ||
                    dgOperResult.CurrentColumn.Name.Equals("S_GRD_QTY") || dgOperResult.CurrentColumn.Name.Equals("T_GRD_QTY") ||
                    dgOperResult.CurrentColumn.Name.Equals("U_GRD_QTY") || dgOperResult.CurrentColumn.Name.Equals("V_GRD_QTY") ||
                    dgOperResult.CurrentColumn.Name.Equals("W_GRD_QTY") || dgOperResult.CurrentColumn.Name.Equals("Y_GRD_QTY") )
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgOperResult.CurrentRow.DataItem, dgOperResult.CurrentColumn.Name.ToString())).Equals("0")) return;

                    else
                    {
                        FCS002_221_DFCT DfctList = new FCS002_221_DFCT();
                        DfctList.FrameOperation = FrameOperation;

                        string SubLot_GRD = Util.NVC(dgOperResult.CurrentColumn.Name).Substring(0, 1);

                        object[] parameters = new object[6];
                        parameters[0] = LoginInfo.LANGID;
                        parameters[1] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd") + dtpFromTime.DateTime.Value.ToString("HHmm00");
                        parameters[2] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd") + dtpToTime.DateTime.Value.ToString("HHmm00");
                        parameters[3] = Util.NVC(DataTableConverter.GetValue(dgOperResult.CurrentRow.DataItem, "ROUTID")); //공정
                        parameters[4] = Util.NVC(DataTableConverter.GetValue(dgOperResult.CurrentRow.DataItem, "PROD_LOTID")); //조립lot
                        parameters[5] = SubLot_GRD; //등급



                        C1WindowExtension.SetParameters(DfctList, parameters);
                        DfctList.Closed += new EventHandler(sDfct_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => DfctList.ShowModal()));
                        DfctList.BringToFront();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void sDfct_Closed(object sender, EventArgs e)
        {
            FCS002_221_DFCT window = sender as FCS002_221_DFCT;

            this.grdMain.Children.Remove(window);
        }
    }
}
