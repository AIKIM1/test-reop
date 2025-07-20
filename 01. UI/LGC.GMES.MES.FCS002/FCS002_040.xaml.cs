/*************************************************************************************
 Created Date : 2020.11.23
      Creator : Kang Dong Hee
   Decription : 공정별 실적
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.23   NAME : Initial Created
  2021.04.01    KDH : 링크 기능을 Head -> Cell 로 전환 및 Bold 제거
  2021.04.05    KDH : 컬럼명 변경(Lot ID -> PKG Lot ID) 및 검색조건에 공정 그룹 추가
  2021.04.09    KDH : Line별 공정그룹 Setting으로 수정.
  2021.08.19    KDH : Lot 유형 검색조건 추가
  2021.11.30    KDH : 조회 시 에러 발생 수정 조치
  2022.08.17 이정미 : 조회 시 합계 오류 수정 
  2023.12.27 주훈   : 조회 BizRule 변경
  2024.01.16 주훈   : 수량/양품 추가
  2024.03.13 주훈   : 합계 구현 방법 변경
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
    public partial class FCS002_040 : UserControl,IWorkArea
    {
        #region [Declaration & Constructor]
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;

        #endregion

        #region [Initialize]

        public FCS002_040()
        {
            InitializeComponent();
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
          
            // 동
            ComCombo.SetCombo(cboArea, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "ALLAREA");


            //C1ComboBox[] cboLineChild = { cboModel }; //2021.04.09 Line별 공정그룹 Setting으로 수정 START
            C1ComboBox[] cboLineChild = { cboModel, cboProcGrpCode }; //2021.04.09 Line별 공정그룹 Setting으로 수정 START
            ComCombo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelParent = { cboLine };
            C1ComboBox[] cboModelChild = { cboRoute };
            ComCombo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            C1ComboBox[] cboRouteChild = { cboOper };
            ComCombo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent, cbChild: cboRouteChild);

            // 공정 그룹
            C1ComboBox[] cboProcGrParent = { cboLine }; //2021.04.09 Line별 공정그룹 Setting으로 수정 START
            C1ComboBox[] cboProcGrChild = { cboOper }; //2021.04.05 공정그룹 추가
            string[] sFilter = { "PROC_GR_CODE_MB", LoginInfo.CFG_AREA_ID };
            ComCombo.SetCombo(cboProcGrpCode, CommonCombo_Form_MB.ComboStatus.ALL, cbChild: cboProcGrChild, cbParent: cboProcGrParent, sFilter: sFilter, sCase: "PROCGRP_BY_LINE");

            //2021.04.09 Line별 공정그룹 Setting으로 수정 END
            C1ComboBox[] cboOperParent = { cboRoute, cboProcGrpCode };
            ComCombo.SetCombo(cboOper, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "ROUTE_OP", cbParent: cboOperParent); //2021.04.05 공정그룹 추가

            // 공정
            //C1ComboBox[] cboProcParent = { cboArea, cboLine, cboProcGrpCode };
            //ComCombo.SetCombo(cboOper, CommonCombo_Form_MB.ComboStatus.SELECT, cbParent: cboProcParent, sCase: "PROC_BY_PROCGRP");

            // Lot 유형
            ComCombo.SetCombo(cboLotType, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LOTTYPE"); // 2021.08.19 Lot 유형 검색조건 추가
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
                    switch(dgc.Name)
                    {
                        case "DATES":
                            //  Summary Row에 정보를 표시하지 않음
                            break;

                        case "LOT_ID":
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

        #region [Method]

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

        private void GetList()
        {
            try
            {
                Util.gridClear(dgOperResult);
                btnSearch.IsEnabled = false;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string)); // 2021.08.19 Lot 유형 검색조건 추가

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                // 2023.12.27 수정
                //dr["PROCID"] = Util.GetCondition(cboOper, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboOper, sMsg: "FM_ME_0107");  //공정을 선택해주세요.
                if (string.IsNullOrEmpty(dr["PROCID"].ToString())) return;

                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd") + dtpFromTime.DateTime.Value.ToString("HHmm00");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd") + dtpToTime.DateTime.Value.ToString("HHmm00");
                dr["LOTTYPE"] = Util.GetCondition(cboLotType, bAllNull: true); // 2021.08.19 Lot 유형 검색조건 추가

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                // 2023.12.27 
                // 조회 BizRule 변경
                // 판정 정보 변경(TB_SFC_SUBLOT_HIST_COMP → TB_SFC_LOT_JUDG_RSLT_SUM)
                //String sBizName = "DA_SEL_LOAD_TRAY_DETAILS_MB";
                String sBizName = "DA_SEL_LOAD_TRAY_DETAILS_LOTJUDG_MB";
                new ClientProxy().ExecuteService(sBizName, "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            // 2023.12.27 수정
                            // 조회 후 Display가 느려 수정 (Util.GridSetData 자주 사용시 늘려짐)
                            //Util.GridSetData(dgOperResult, result, FrameOperation, true);

                            //DataTable dt = DataTableConverter.Convert(dgOperResult.ItemsSource);
                            //AddRowSumQty(dgOperResult, dt);

                            //DataTable dt1 = DataTableConverter.Convert(dgOperResult.ItemsSource);
                            //AddRowPercent(dgOperResult, dt1);

                            DataTable dt = result.Copy();

                            // 2024.03.13 DataGridSummaryRow 구현으로 삭제
                            //AddRowSumQty2(dt);
                            //AddRowPercent2(dt);

                            Util.GridSetData(dgOperResult, dt, FrameOperation, true);

                            Util _Util = new Util();
                            // 2024.03.13 LOT_ID Merge 제외
                            //string[] sColumnName = new string[] { "DATES", "LOT_ID" };
                            string[] sColumnName = new string[] { "DATES" };
                            _Util.SetDataGridMergeExtensionCol(dgOperResult, sColumnName, DataGridMergeMode.VERTICAL);
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
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                btnSearch.IsEnabled = true;
            }
        }

        private void AddRowSumQty(C1.WPF.DataGrid.C1DataGrid datagrid, DataTable dt)
        {
            DataTable preTable = DataTableConverter.Convert(datagrid.ItemsSource);

            if (preTable.Columns.Count == 0)
            {
                preTable = new DataTable();
                foreach (C1.WPF.DataGrid.DataGridColumn col in datagrid.Columns)
                {
                    preTable.Columns.Add(Convert.ToString(col.Name));
                }
            }

            int iInputCol = dgOperResult.Columns["INPUT_QTY"].Index;
            int iSumFromCol = dgOperResult.Columns["TRAYCNT"].Index;
            int iSumToCol = dgOperResult.Columns["GRADE_Z"].Index;
            int iSumInputQty = 0;
            int iTotalSumQty = 0;
            int iSumQty = 0;
            string ColName = string.Empty;

            DataRow row = preTable.NewRow();
            row["DATES"] = string.Empty;
            row["LOT_ID"] = ObjectDic.Instance.GetObjectName("합계");

            for (int iCol = iSumFromCol; iCol <= iSumToCol; iCol++)
            {
                ColName = dgOperResult.Columns[iCol].Name;

                for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                {
                    iSumQty = Convert.ToInt32(Util.NVC(Convert.ToString(dt.Rows[iRow][ColName])));
                    iTotalSumQty = iTotalSumQty + iSumQty;
                }

                row[ColName] = Convert.ToString(iTotalSumQty);

                if (ColName.Equals("INPUT_QTY"))
                {
                    iSumInputQty = iTotalSumQty;
                }

                iTotalSumQty = 0;
                iSumQty = 0;
            }

            preTable.Rows.Add(row);

            Util.GridSetData(datagrid, preTable, FrameOperation, true);
        }

        private void AddRowPercent(C1.WPF.DataGrid.C1DataGrid datagrid, DataTable dt)
        {
            DataTable preTable = DataTableConverter.Convert(datagrid.ItemsSource);

            if (preTable.Columns.Count == 0)
            {
                preTable = new DataTable();
                foreach (C1.WPF.DataGrid.DataGridColumn col in datagrid.Columns)
                {
                    preTable.Columns.Add(Convert.ToString(col.Name));
                }
            }

            int iInputCol = dgOperResult.Columns["INPUT_QTY"].Index;
            int iPerFromCol = dgOperResult.Columns["CURR_QTY"].Index;
            int iPerToCol = dgOperResult.Columns["GRADE_Z"].Index;

            double iSumInputQty = 0;
            double iSumQty = 0;
            double ColValue = 0;

            int iMaxRow = 0;
            string ColName = string.Empty;

            iMaxRow = dt.Rows.Count - 1;
            //iSumInputQty = Convert.ToInt16(Util.NVC(Convert.ToString(dt.Rows[iMaxRow]["INPUT_QTY"]))); //2021.11.30 조회 시 에러 발생 수정 조치(변수 Type에 맞춰 Convert 변경)
            iSumInputQty = Convert.ToDouble(Util.NVC(Convert.ToString(dt.Rows[iMaxRow]["INPUT_QTY"]))); //2021.11.30 조회 시 에러 발생 수정 조치(변수 Type에 맞춰 Convert 변경)


            DataRow row = preTable.NewRow();
            row["DATES"] = string.Empty;
            row["LOT_ID"] = ObjectDic.Instance.GetObjectName("PERCENT_VAL");

            for (int iCol = iPerFromCol; iCol <= iPerToCol; iCol++)
            {
                ColName = dgOperResult.Columns[iCol].Name;
                //iSumQty = Convert.ToInt16(Util.NVC(Convert.ToString(dt.Rows[iMaxRow][ColName]))); //2021.11.30 조회 시 에러 발생 수정 조치(변수 Type에 맞춰 Convert 변경)
                iSumQty = Convert.ToDouble(Util.NVC(Convert.ToString(dt.Rows[iMaxRow][ColName]))); //2021.11.30 조회 시 에러 발생 수정 조치(변수 Type에 맞춰 Convert 변경)

                ColValue = (iSumQty / iSumInputQty) * 100;
                row[ColName] = ColValue.ToString("#,#.00");
            }

            preTable.Rows.Add(row);

            Util.GridSetData(datagrid, preTable, FrameOperation, true);
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
            dJobDate = dJobDate.AddSeconds(-1);
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
        // 공통함수로 뺄지 확인 필요 END

        private void SetProcessGroupCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMBO_PROCESS_GROUP_BY_LINE_MB";
            string[] arrColumn = { "LANGID", "AREAID", "EQSGID", "COM_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, cboLine.SelectedValue == null ? null : cboLine.SelectedValue.ToString(), "PROC_GR_CODE_MB" };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo_Form.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo_Form.ComboStatus.NONE, selectedValueText, displayMemberText, null);
        }

        /// <summary>
        /// Add Row SumQty
        /// </summary>
        private void AddRowSumQty2(DataTable dt)
        {
            int iRow = 0;
            int iCol = 0;
            int iStaCol = 0;

            string ColName = string.Empty;
            List<String> ExceptColName = new List<string>
            {
                "GRADE_2", "GRADE_3", "GRADE_4", "GRADE_5"
              , "GRADE_6", "GRADE_7", "GRADE_8", "GRADE_9"
              , "GRADE_F00", "GRADE_F11", "GRADE_SAS"
            };

            int iMaxRow = 0;
            DataRow dr = dt.NewRow();

            dr["DATES"] = string.Empty;
            dr["LOT_ID"] = ObjectDic.Instance.GetObjectName("합계");

            iMaxRow = dt.Rows.Count;
            iStaCol = dt.Columns.IndexOf("TRAYCNT");
            for (iRow = 0; iRow < iMaxRow; iRow++)
            {
                for (iCol = iStaCol; iCol < dt.Columns.Count; iCol++)
                {
                    ColName = dt.Columns[iCol].ColumnName;
                    if (ExceptColName.Contains(ColName) == true)
                        continue;

                    dr[iCol] = Util.NVC_Int(dr[iCol]) + Util.NVC_Int(dt.Rows[iRow][iCol]);
                }
            }
            dt.Rows.Add(dr);
        }

        /// <summary>
        /// Add Row Percent
        /// </summary>
        private void AddRowPercent2(DataTable dt)
        {
            int iSumRow = 0;
            int iCol = 0;
            int iStaCol = 0;

            double iSumInputQty = 0;
            string ColName = string.Empty;
            List<String> ExceptColName = new List<string>
            {
                "INPUT_QTY"
              , "GRADE_2", "GRADE_3", "GRADE_4", "GRADE_5"
              , "GRADE_6", "GRADE_7", "GRADE_8", "GRADE_9"
              , "GRADE_F00", "GRADE_F11", "GRADE_SAS"
            };

            double iSumQty = 0;
            double ColValue = 0;

            DataRow dr = dt.NewRow();

            dr["DATES"] = string.Empty;
            dr["LOT_ID"] = ObjectDic.Instance.GetObjectName("PERCENT_VAL");

            iSumRow = dt.Rows.Count - 1;
            iStaCol = dt.Columns.IndexOf("TRAYCNT");
            iSumInputQty = Convert.ToDouble(Util.NVC(Convert.ToString(dt.Rows[iSumRow]["INPUT_QTY"])));

            for (iCol = iStaCol + 1; iCol < dt.Columns.Count; iCol++)
            {
                ColName = dt.Columns[iCol].ColumnName;
                if (ExceptColName.Contains(ColName) == true)
                    continue;

                iSumQty = Convert.ToDouble(Util.NVC(Convert.ToString(dt.Rows[iSumRow][iCol])));
                ColValue = (iSumQty / iSumInputQty) * 100;
                dr[iCol] = ColValue.ToString("#,#.00");
            }

            dt.Rows.Add(dr);
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

        #region [Event]

        private void chkNormalDefaultYN_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                CommonCombo_Form_MB ComCombo = new CommonCombo_Form_MB();

                C1ComboBox[] cboRouteParent = { cboLine, cboModel };
                C1ComboBox[] cboRouteChild = { cboOper };
                ComCombo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent, cbChild: cboRouteChild);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void chkNormalDefaultYN_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CommonCombo_Form_MB ComCombo = new CommonCombo_Form_MB();

                C1ComboBox[] cboRouteParent = { cboLine, cboModel };
                C1ComboBox[] cboRouteChild = { cboOper };
                string[] sFilter = { null, null, null, "D,G" };
                ComCombo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent, cbChild: cboRouteChild, sFilter: sFilter);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            ClearFilterGrid(); // 2024.03.13 추가
            GetList();
        }

        private void dgOperResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgOperResult.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (!cell.Column.Name.Equals("LOT_ID"))
                    {
                        return;
                    }

                    //화면 ID 확인 후 수정
                    loadingIndicator.Visibility = Visibility.Visible;

                    object[] parameters = new object[11];
                    parameters[0] = Util.NVC(DataTableConverter.GetValue(dgOperResult.Rows[cell.Row.Index].DataItem, "LOT_ID")).ToString(); // LOT_ID
                    parameters[1] = Util.GetCondition(cboLine);                                                                             //LINE_ID
                    parameters[2] = Util.GetCondition(cboModel);                                                                            //MODEL_ID
                    parameters[3] = Util.GetCondition(cboRoute);                                                                            //ROUTE_ID
                    parameters[4] = Util.GetCondition(cboOper);                                                                             //OPER_ID

                    parameters[5] = dtpFromDate.SelectedDateTime;                                                                           //FROM_DATE
                    parameters[6] = dtpFromTime.DateTime.Value.ToString("HHmm00");                                                          //FROM_TIME
                    parameters[7] = dtpToDate.SelectedDateTime;                                                                             //TO_DATE
                    parameters[8] = dtpToTime.DateTime.Value.ToString("HHmm00");                                                            //TO_TIME
                    parameters[9] = true;                                                                                                   //CHK_HIS
                    parameters[10] = "Y";                                                                                                   //ACTYN

                    this.FrameOperation.OpenMenu("SFU010715260", true, parameters); //날짜별 공정정보 (FCS002_043 / PMM_210)
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }

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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //string Lot = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOT_ID"));

                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    //if (e.Cell.Column.Name.ToString() == "LOT_ID")
                    //{
                    //    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    //    e.Cell.Presenter.Cursor = Cursors.Hand;

                    //    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOT_ID")).ToString().Equals(ObjectDic.Instance.GetObjectName("합계")))
                    //    {
                    //        e.Cell.Presenter.Cursor = Cursors.Arrow;
                    //        e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.PapayaWhip);
                    //    }
                    //    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOT_ID")).ToString().Equals(ObjectDic.Instance.GetObjectName("PERCENT_VAL")))
                    //    {
                    //        e.Cell.Presenter.Cursor = Cursors.Arrow;
                    //        e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.Aquamarine);
                    //    }
                    //    else
                    //    {
                    //        e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    //    }
                    //}

                    // 2024.03.13 추가
                    if ((e.Cell.Column.Name.Equals("LOT_ID")) && e.Cell.Row.Index < dataGrid.Rows.Count - 2)
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
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
                            case "TRAYCNT":
                            case "INPUT_QTY":
                                // 비율 Row 에서 Summary 정로를 지움
                                dgOperResult.SetSummaryRowValue(1, e.Cell.Column.Name, String.Empty);
                                break;

                            case "LOT_ID":
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

        private void dgOperResult_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgOperResult_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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

        #endregion
    }
}
