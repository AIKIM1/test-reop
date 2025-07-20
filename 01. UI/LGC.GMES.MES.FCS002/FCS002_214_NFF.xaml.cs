/*************************************************************************************
 Created Date : 2022.12.07
      Creator : 
   Decription : Sorter/외관검사기 실적관리
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.07  DEVELOPER : Initial Created.
  2024.03.13 주훈       : 합계 구현 방법 변경 
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
using static LGC.GMES.MES.CMM001.Controls.UcBaseDataGrid;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_214_NFF : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;

        Util _Util = new Util();

        #endregion

        #region Initialize

        public FCS002_214_NFF()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitCombo();

                InitControl();

                // 2024.03.13 추가
                InitSpread();

                this.Loaded -= UserControl_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            C1ComboBox[] cboLineChild = { cboModel, cboRoute };
            _combo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

         // C1ComboBox[] cboLaneChild = { cboLane };
         // string[] sFilter = { "1" };   //EQPT_GR_TYPE_CODE 참고. (1:Formation)
         // _combo.SetCombo(cboLane, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "LANE_MB", sFilter: sFilter);
            string[] sFilter = { "CST", null,"M" };
            _combo.SetCombo(cboEQP, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "EQPID", sFilter: sFilter);

         // C1ComboBox[] cboEqpParent = { cboLane };
         // _combo.SetCombo(cboEqp, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "EQPIDBYLANE", cbParent: cboEqpParent);
          
            C1ComboBox[] cboModelParent = { cboLine };
            C1ComboBox[] cboModelChild = { cboRoute };
            _combo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent, cbChild: cboModelChild);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            _combo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent);

            string[] sFilter2 = { "COMBO_PROC_INFO_BY_DATE_SEARCH_CONDITION" }; //E07
            _combo.SetCombo(cboSearch, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter2);

            string[] sFilter3 = { "FORM_SEARCH_WIPSTATE" };//FIN_CD
            _combo.SetCombo(cboFinCD, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter3);

            string[] sFilter4 = { "FORM_SEARCH_ORDERBY" };  //E09
            _combo.SetCombo(cboOrder, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter4);

            //string[] sFilterEqpType = { "EQPT_GR_TYPE_CODE", "5,6,V" };
            //_combo.SetCombo(cboEol, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "FORM_CMN", sFilter: sFilterEqpType);

        }

        private void InitControl()
        {
            SetWorkResetTime();

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
                foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgList.Columns)
                {
                    switch (dgc.Name)
                    {
                        case "CSTID":
                        case "LOTID":
                        case "ROUTID":
                        case "WIPSNAME":

                        case "EQPTNAME":
                        case "WIPDTTM_ST":
                        case "WIPDTTM_ED":

                        case "V_EQPTNAME":
                        case "V_WIPDTTM_ST":
                        case "V_WIPDTTM_ED":
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

                Util _Util = new Util();
                string[] sColumnNames = new string[] { "PROD_LOTID" };
                _Util.SetDataGridMergeExtensionCol(dgList, sColumnNames, DataGridMergeMode.VERTICAL);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event

        private void dgList_LoadingRow(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            
        }

        private void dgList_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null)
                    return;

                if (Util.NVC((e.Row.DataItem as System.Data.DataRowView).Row["LOTID"]).Equals("Y") || Util.NVC((e.Row.DataItem as System.Data.DataRowView).Row["LOTID"]).Equals("N"))
                {
                    //e.Row.DataGrid.RowBackground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF7E9D5"));
                    //e.Row.DataGrid.RowForeground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }

                if (Util.NVC((e.Row.DataItem as System.Data.DataRowView).Row["WIPSTAT"]).Equals("PROC"))
                {
                    //e.Row.DataGrid.RowBackground = new SolidColorBrush(Colors.Gray);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

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

                    if (e.Cell.Column.Name.Equals("CSTID") && e.Cell.Row.Index < dataGrid.Rows.Count - dataGrid.BottomRows.Count)
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                }

                // 2024.03.13 추가
                if (e.Cell.Row.GetType() == typeof(DataGridSummaryRow))
                {
                    if (e.Cell.Row.Index == dataGrid.Rows.Count - dataGrid.BottomRows.Count)
                    {
                        e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.PapayaWhip);
                    }
                    else
                    {
                        e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.Aquamarine);

                        switch (e.Cell.Column.Name)
                        {
                            case "INPUT_SUBLOT_QTY":
                                // 비율 Row 에서 Summary 정로를 지움
                                dgList.SetSummaryRowValue(1, e.Cell.Column.Name, String.Empty);
                                break;

                            case "PROD_LOTID":
                                dgList.SetSummaryRowValue(1, e.Cell.Column.Name, ObjectDic.Instance.GetObjectName("PERCENT_VAL"));
                                break;

                            default:
                                decimal calcValue = 0;
                                decimal sumInput = 0;

                                // DataGridSummaryRow 미사용 항목
                                if (DataGridAggregate.GetAggregateFunctions(e.Cell.Column) == null)
                                    break;

                                if (dgList.GetDataTable() != null)
                                    sumInput = Util.NVC_Decimal(dgList.GetDataTable().Compute("Sum(INPUT_SUBLOT_QTY)", ""));
                                decimal sumValue = Util.NVC_Int(dgList.GetSummaryRowValue(0, e.Cell.Column.Name).Replace(",", ""));

                                if (sumInput.Equals(0) == false)
                                    calcValue = Math.Round(sumValue * 100 / sumInput, 2);
                                dgList.SetSummaryRowValue(1, e.Cell.Column.Name, calcValue.ToString("#,##0.00"));
                                break;
                        }
                        e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    }
                }
            }));
        }

        private void dgList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CSTID"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                    }
                }
            }));
        }

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                }
            }
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgList.ItemsSource == null)
                    return;

                if (sender == null)
                    return;

                if (dgList.CurrentRow != null && dgList.CurrentColumn.Name.Equals("CSTID"))
                {
                    //Tray 조회
                    object[] parameters = new object[6];
                    parameters[0] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "CSTID"));   //TRAYID
                    parameters[1] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "LOTID"));   //TRAYNO

                    //Tray 정보조회
                    this.FrameOperation.OpenMenu("SFU010710300", true, parameters);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //private void chkMORE_Checked(object sender, RoutedEventArgs e)
        //{
        //    dgList.Columns["OTHER_F11"].Visibility = Visibility.Visible;
        //    dgList.Columns["OTHER_DIS"].Visibility = Visibility.Visible;
            
        //}
        //private void chkMORE_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    dgList.Columns["OTHER_F11"].Visibility = Visibility.Collapsed;
        //    dgList.Columns["OTHER_DIS"].Visibility = Visibility.Collapsed;
        //}

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearFilterGrid(); // 2024.03.13 추가
            GetList();
        }

        private void dgList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index < dataGrid.Rows.Count - dataGrid.BottomRows.Count)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private void chkMORE_Checked(object sender, RoutedEventArgs e)
        {
            chkEOLRW.IsChecked = false;
            dgList.Columns["SORTER_F11"].Visibility = Visibility.Visible;
            dgList.Columns["SORTER_DIS"].Visibility = Visibility.Visible;
        }

        private void chkEOLRW_Checked(object sender, RoutedEventArgs e)
        {
            chkMORE.IsChecked = false;
            dgList.Columns["SORTER_F11"].Visibility = Visibility.Collapsed;
            dgList.Columns["SORTER_DIS"].Visibility = Visibility.Collapsed;
        }

        private void chkMORE_Unchecked(object sender, RoutedEventArgs e)
        {
            dgList.Columns["SORTER_F11"].Visibility = Visibility.Collapsed;
            dgList.Columns["SORTER_DIS"].Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Method

        private void ClearFilterGrid()
        {
            try
            {
                // dgList.ClearFilter(); 동기화로 변경
                if ((dgList.ItemsSource != null) && (dgList.Columns.Count > 0))
                {
                    dgList.FilterBy(dgList.Columns[0], null);
                }
            }
            finally { }
        }

        private void GetList()
        {
            try
            {
                Util.gridClear(dgList);
                btnSearch.IsEnabled = false;

                for (int i = 1; i < dgList.Columns.Count; i++)
                {
                    // 2024.03.13 추가
                    // DataGridSummaryRow 미사용 항목
                    if (DataGridAggregate.GetAggregateFunctions(dgList.Columns[i]) == null)
                        continue;

                    dgList.Columns[i].Visibility = Visibility.Visible;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("LANEID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add(Util.GetCondition(cboSearch), typeof(string));
                dtRqst.Columns.Add(Util.GetCondition(cboFinCD), typeof(string));
                dtRqst.Columns.Add(Util.GetCondition(cboOrder), typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dr["EQPT_GR_TYPE_CODE"] = "5"; // 특성 측정기
                //dr["EQPT_GR_TYPE_CODE"] = Util.GetCondition(cboEol, bAllNull: true); // 특성 측정기

                dr["EQPTID"] = Util.GetCondition(cboEQP, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
             // dr["LANEID"] = Util.GetCondition(cboLane, bAllNull: true);
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd") + dtpFromTime.DateTime.Value.ToString("HHmm00");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd") + dtpToTime.DateTime.Value.ToString("HHmm00");

                if (!string.IsNullOrEmpty(txtProdLot.Text))
                    dr["PROD_LOTID"] = Util.GetCondition(txtProdLot, bAllNull: true);

                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);

                dr[Util.GetCondition(cboSearch)] = "Y";
                dr[Util.GetCondition(cboFinCD)] = "Y";
                dr[Util.GetCondition(cboOrder)] = "Y";

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                string sBiz = string.Empty;
                DataTable dtRslt = null;

                if (chkMORE.IsChecked == true)
                {
                    // 기존
                    //dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOAD_EQP_RESULT_NEW_MB", "RQSTDT", "RSLTDT", dtRqst);

                    // 신규
                    //dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOAD_EQP_RESULT_HIST_MB", "RQSTDT", "RSLTDT", dtRqst);
                    sBiz = "DA_SEL_LOAD_EQP_RESULT_SELECT_DIS_MB";

                }
                else
                {
                    // 기존
                    //dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOAD_EQP_RESULT_MB", "RQSTDT", "RSLTDT", dtRqst);

                    // 신규
                    //dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOAD_EQP_RESULT_SELECT_MB", "RQSTDT", "RSLTDT", dtRqst);
                    sBiz = "DA_SEL_LOAD_EQP_RESULT_SELECT_MB";
                    if(chkEOLRW.IsChecked == true)
                        sBiz = "DA_SEL_LOAD_EQP_EOL_REWORK_RESULT_MB";

                    dgList.Columns["SORTER_F11"].Visibility = Visibility.Collapsed;
                    dgList.Columns["SORTER_DIS"].Visibility = Visibility.Collapsed;
                }

                dtRslt = new ClientProxy().ExecuteServiceSync(sBiz, "RQSTDT", "RSLTDT", dtRqst);

                // 2024.03.13 DataGridSummaryRow 구현으로 삭제
                //if (dtRslt.Rows.Count > 2)
                //{
                //    //행 추가(합계, 비율)
                //    CreateSumRow(dtRslt);
                //}

                // 2024.03.13 추가
                Util.GridSetData(dgList, dtRslt, this.FrameOperation, true);
                SetColVisibilityCollapsed(dtRslt);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
                btnSearch.IsEnabled = true;
            }
        }

        private void CreateSumRow(DataTable dt)
        {
            try
            {
                //DataTable preTable = DataTableConverter.Convert(dgList.ItemsSource);
                //합계 Row용
                DataTable dtSum = dt.Clone();
                DataRow drSum = dtSum.NewRow();
                drSum["LOTID"] = "";
                drSum["PROD_LOTID"] = ObjectDic.Instance.GetObjectName("합계");

                //비율 Row용
                DataTable dtRate = dt.Clone();
                DataRow drRate = dtRate.NewRow();
                drRate["LOTID"] = "";
                drRate["PROD_LOTID"] = ObjectDic.Instance.GetObjectName("비율");

                //합계
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                 
                    if (dt.Columns[i].DataType == System.Type.GetType("System.Decimal"))  // sum(qty)  sum(ISNULL(QTY,0) )
                    {
                        drSum[dt.Columns[i].ColumnName.ToString()] = Convert.ToDecimal(dt.Compute("Sum(" + dt.Columns[i].ColumnName.ToString() + ")", ""));

                        if (Convert.ToDecimal(dt.Compute("Sum(" + dt.Columns[i].ColumnName.ToString() + ")", "")) == 0)
                        {
                            dgList.Columns[dt.Columns[i].ColumnName.ToString()].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                //row 추가
                dtSum.Rows.Add(drSum);

                //비율
                for (int i = dtSum.Columns["INPUT_SUBLOT_QTY"].Ordinal; i < dtSum.Columns.Count; i++)
                {
                    if (dtSum.Columns[i].DataType == System.Type.GetType("System.Decimal"))
                    {
                        drRate[dtSum.Columns[i].ColumnName.ToString()] = string.Format("{0:0.00} ", Convert.ToDecimal(Convert.ToDecimal(dtSum.Rows[0][i].ToString()) / Convert.ToDecimal(dtSum.Rows[0]["INPUT_SUBLOT_QTY"].ToString())) * 100);
                    }
                }

                dtRate.Rows.Add(drRate);

                dt.Merge(dtSum);
                dt.Merge(dtRate);

                //dgList.ItemsSource = DataTableConverter.Convert(dt);
                Util.GridSetData(dgList, dt, this.FrameOperation, true);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetColVisibilityCollapsed(DataTable dtRslt)
        {
            try
            {
                String sColumnName = String.Empty;
                List<String> ExceptColName = new List<string>
                {
                    "PROD_LOTID", "INPUT_SUBLOT_QTY"
                  , "WIPQTY_ST",  "GOOD_SUBLOT_QTY", "BAD_QTY"  // Sorter
                  , "GRADE_1"
                  , "V_WIPQTY_ST",  "V_GOOD_QTY", "V_BAD_QTY"   // VISION
                };

                List<String> DIS_FITCAPA = new List<string>
                {
                    "SORTER_F11", "SORTER_DIS"
                };

                for (int i = 0; i < dgList.Columns.Count; i++)
                {
                    sColumnName = dgList.Columns[i].Name.ToString();

                    // DataGridSummaryRow 미사용 항목
                    if (DataGridAggregate.GetAggregateFunctions(dgList.Columns[sColumnName]) == null)
                        continue;

                    // 항시 보이는 항목
                    if (ExceptColName.Contains(sColumnName) == true)
                        continue;

                    // DIS / FITCAPA 확인
                    if (chkMORE.IsChecked == true)
                    {
                        if (DIS_FITCAPA.Contains(sColumnName) == true)
                            continue;
                    }

                    if ((dtRslt.Rows.Count > 0) && (dtRslt.Columns.Contains(sColumnName) == true))
                    {
                        if (Convert.ToDecimal(dtRslt.Compute("Sum(" + sColumnName + ")", "")) > 0)
                            continue;
                    }

                    dgList.Columns[sColumnName].Visibility = Visibility.Collapsed;
                }
            }
            finally { }
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

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime, "yyyyMMdd HHmmss", null).AddSeconds(-1);
            return dJobDate;
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
    }
}