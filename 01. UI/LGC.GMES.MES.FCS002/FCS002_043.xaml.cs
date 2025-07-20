/*************************************************************************************
 Created Date : 2023.02.13
      Creator : KANG DONG HEE
   Decription : 날짜별 공정정보
--------------------------------------------------------------------------------------
 [Change History]
  2023.02.13   DEVELOPER    : Initial Created.
  2024.01.10  주훈            조회조건에 공정 그룹 추가
  2024.01.16  주훈            현재 (Visiable) 및 양품 수량 추가
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
using static LGC.GMES.MES.CMM001.Controls.UcBaseDataGrid;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// TSK_710.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_043 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;
        #endregion

        #region [Initialize]
        public FCS002_043()
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
            InitCombo();

            // 2024.03.13 추가
            InitSpread();

            object[] parameters = this.FrameOperation.Parameters;
            if (parameters != null && parameters.Length >= 1)
            {
                DateTime nowDate = DateTime.Now;


                txtLotID.Text = Util.NVC(parameters[0]);
                cboLine.SelectedValue = Util.NVC(parameters[1]);
                cboModel.SelectedValue = Util.NVC(parameters[2]);
                cboRoute.SelectedValue = Util.NVC(parameters[3]);
                cboOper.SelectedValue = Util.NVC(parameters[4]);

                dtpFromDate.SelectedDateTime = Util.StringToDateTime(Util.NVC(parameters[5]));
                dtpFromTime.DateTime = Util.StringToDateTime(nowDate.ToString("yyyyMMdd") + " " + Util.NVC(parameters[6]), "yyyyMMdd HHmmss");
                dtpToDate.SelectedDateTime = Util.StringToDateTime(Util.NVC(parameters[7]));
                dtpToTime.DateTime = Util.StringToDateTime(nowDate.ToString("yyyyMMdd") + " " + Util.NVC(parameters[8]), "yyyyMMdd HHmmss");
                chkHistory.IsChecked = (bool)parameters[9];
                GetList();
            }
            else
            {
                InitControl();
            }
            this.Loaded -= UserControl_Loaded;

        }

        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            // 2024.01.10 공정 그룹 추가 따흔 수정
            //C1ComboBox[] cboLineChild = { cboModel };
            C1ComboBox[] cboLineChild = { cboModel, cboProcGrpCode };
            _combo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelParent = { cboLine };
            C1ComboBox[] cboModelChild = { cboRoute };
            _combo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            string[] sFilter = { "ROUT_RSLT_GR_CODE" };
            C1ComboBox[] cboRouteSetChild = { cboRoute };
            _combo.SetCombo(cboRouteSet, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter, cbChild: cboRouteSetChild);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel, cboRouteSet };
            C1ComboBox[] cboRouteChild = { cboOper };
            _combo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent, cbChild: cboRouteChild);

            // 공정 그룹
            // 2024.01.10 공정 그룹 추가
            C1ComboBox[] cboProcGrParent = { cboLine };
            C1ComboBox[] cboProcGrChild = { cboOper };
            string[] sProcGrFilter = { "PROC_GR_CODE_MB", LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cboProcGrpCode, CommonCombo_Form_MB.ComboStatus.ALL, cbChild: cboProcGrChild, cbParent: cboProcGrParent, sFilter: sProcGrFilter, sCase: "PROCGRP_BY_LINE");

            // 공정
            // 2024.01.10 공정 그룹 추가 따흔 수정
            //C1ComboBox[] cboOperParent = { cboRoute };
            C1ComboBox[] cboOperParent = { cboRoute, cboProcGrpCode };
            _combo.SetCombo(cboOper, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "ROUTE_OP", cbParent: cboOperParent);

            //string[] sFilter1 = { "COMBO_FORM_SPCL_FLAG" };
            //_combo.SetCombo(cboSpecial, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

            string[] sFilter1 = { "FORM_SPCL_FLAG_MCC" };
            _combo.SetCombo(cboSpecial, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

            string[] sFilter2 = { "COMBO_PROC_INFO_BY_DATE_SEARCH_CONDITION" }; //E07
            _combo.SetCombo(cboSearch, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter2);

            string[] sFilter3 = { "COMBO_PROC_INFO_BY_DATE_ORDER_CONDITION" }; //E08
            _combo.SetCombo(cboOrder, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter3);

            string[] sFilter4 = { "COMBO_PROC_INFO_BY_DATE_ORDER_CONDITION" }; //E08
            _combo.SetCombo(cboOrder2, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter4);

            // Lot 유형
            _combo.SetCombo(cboLotType, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LOTTYPE"); // 2021.08.19 Lot 유형 검색조건 추가

            C1ComboBox[] cboLaneChild = { cboBoxID };
            string[] sFilterLane = { "Y" };
            _combo.SetCombo(cboLane, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LANE", cbChild: cboLaneChild, sFilter: sFilterLane);

            C1ComboBox[] cboBoxidParent = { cboLane };
            _combo.SetCombo(cboBoxID, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "BOX_ID", cbParent: cboBoxidParent);
        }

        private void InitControl()
        {
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
                foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgDateOper.Columns)
                {
                    switch (dgc.Name)
                    {
                        case "LOTID":
                        case "CSTID":
                        case "LOC":
                        case "ROUTID":
                        case "SPCL_NOTE":
                        case "BF_ENDTIME":
                        case "STARTTIME":
                        case "ENDTIME":
                        case "AF_STARTTIME":
                        case "JOB_TIME":

                        case "DUMMY_FLAG":
                        case "SPCL_FLAG":
                        case "WIPSTAT":
                        case "ROUT_TYPE":
                        case "LOTDTTM_CR":
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

        #region [Method]

        private void ClearFilterGrid()
        {
            try
            {
                // dgDateOper.ClearFilter(); 동기화로 변경
                if ((dgDateOper.ItemsSource != null) && (dgDateOper.Columns.Count > 0))
                {
                    dgDateOper.FilterBy(dgDateOper.Columns[0], null);
                }
            }
            finally { }
        }

        private void GetList()
        {
            try
            {
                ClearFilterGrid();  // 2024.03.13 추가

                dgDateOper.ItemsSource = null;
                Util.gridClear(dgDateOper);
                btnSearch.IsEnabled = false;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("ROUT_RSLT_GR_CODE", typeof(string));
                dtRqst.Columns.Add("FROM_TIME", typeof(string));
                dtRqst.Columns.Add("TO_TIME", typeof(string));
                dtRqst.Columns.Add("CURRENT_YN", typeof(string));
                dtRqst.Columns.Add("HISTORY_YN", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("SPCL_FLAG", typeof(string));
                dtRqst.Columns.Add("START_TIME_YN", typeof(string));
                dtRqst.Columns.Add("END_TIME_YN", typeof(string));
                dtRqst.Columns.Add("ORDER_OPTION_YN", typeof(string));
                dtRqst.Columns.Add("ORDER_OPTION_YN2", typeof(string));
                dtRqst.Columns.Add("ORDER_OPTION_01", typeof(string));
                dtRqst.Columns.Add("ORDER_OPTION_02", typeof(string));
                dtRqst.Columns.Add(Util.GetCondition(cboSearch), typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string)); // 2021.08.19 Lot 유형 검색조건 추가
                dtRqst.Columns.Add("EQP_ID", typeof(string)); // 2021.08.19 Lot 유형 검색조건 추가
                dtRqst.Columns.Add("LANE_ID", typeof(string)); // 2021.08.19 Lot 유형 검색조건 추가

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Util.GetCondition(cboOper, sMsg: "FM_ME_0107");  //공정을 선택해주세요.
                if (string.IsNullOrEmpty(dr["PROCID"].ToString())) return;
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                dr["ROUT_RSLT_GR_CODE"] = Util.GetCondition(cboRouteSet, bAllNull: true);
                dr["FROM_TIME"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                dr["TO_TIME"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm:00");
                if (!string.IsNullOrEmpty(txtLotID.Text)) dr["PROD_LOTID"] = Util.NVC(txtLotID.Text);
                dr["SPCL_FLAG"] = Util.GetCondition(cboSpecial, bAllNull: true);

                if (!(bool)chkHistory.IsChecked)
                {
                    dr["CURRENT_YN"] = "Y";
                }
                // START_TIME_YN, END_TIME_YN 
                dr[Util.GetCondition(cboSearch)] = "Y";
                dr["LOTTYPE"] = Util.GetCondition(cboLotType, bAllNull: true); // 2021.08.19 Lot 유형 검색조건 추가

                dr["EQP_ID"] = Util.GetCondition(cboBoxID, bAllNull: true);
                dr["LANE_ID"] = Util.GetCondition(cboLane, bAllNull: true);

                dr["ORDER_OPTION_01"] = Util.GetCondition(cboOrder, bAllNull: true);
                dr["ORDER_OPTION_YN"] = "Y";
                dr["ORDER_OPTION_YN2"] = null;
                if (cboOrder2.SelectedIndex > 0)
                {
                    dr["ORDER_OPTION_02"] = Util.GetCondition(cboOrder2, bAllNull: true);
                    dr["ORDER_OPTION_YN"] = null;
                    dr["ORDER_OPTION_YN2"] = "Y";
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ROUTE_INFO_CONDITION_F_ALL_MB", "RQSTDT", "RSLTDT", dtRqst);
                                
                Util.GridSetData(dgDateOper, dtRslt, this.FrameOperation, true);

                // 2024.03.13 DataGridSummaryRow 구현으로 삭제
                ////20220802 dgDateOper의 데이터가 없으면 합계,비율(%)을 만들지 않는다.
                //if (dgDateOper.Rows.Count == 0)
                //{
                //    return;
                //}

                //DataTable dt_Per = ConvertToDataTable(dgDateOper);

                //DataRow dr_Sum = dt_Per.NewRow();
                //Decimal Total_Input = Convert.ToInt32(dt_Per.Compute("Sum(INPUT_SUBLOT_QTY)", ""));

                //dr_Sum["PROD_LOTID"] = ObjectDic.Instance.GetObjectName("합계");
                //dr_Sum["INPUT_SUBLOT_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(INPUT_SUBLOT_QTY)", ""));
                //dr_Sum["WIP_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(WIP_QTY)", ""));
                //// 2024.01.16 양품 추가
                //dr_Sum["GOOD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(GOOD_QTY)", ""));

                //dr_Sum["ERR_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(ERR_QTY)", ""));
                //dr_Sum["A_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(A_GRD_QTY)", ""));
                //dr_Sum["B_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(B_GRD_QTY)", ""));
                //dr_Sum["C_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(C_GRD_QTY)", ""));
                //dr_Sum["D_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(D_GRD_QTY)", ""));
                //dr_Sum["E_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(E_GRD_QTY)", ""));
                //dr_Sum["F_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(F_GRD_QTY)", ""));
                //dr_Sum["G_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(G_GRD_QTY)", ""));
                //dr_Sum["H_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(H_GRD_QTY)", ""));
                //dr_Sum["I_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(I_GRD_QTY)", ""));
                //dr_Sum["J_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(J_GRD_QTY)", ""));
                //dr_Sum["K_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(K_GRD_QTY)", ""));
                //dr_Sum["L_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(L_GRD_QTY)", ""));
                //dr_Sum["M_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(M_GRD_QTY)", ""));
                //dr_Sum["N_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(N_GRD_QTY)", ""));
                //dr_Sum["O_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(O_GRD_QTY)", ""));
                //dr_Sum["P_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(P_GRD_QTY)", ""));
                //dr_Sum["Q_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(Q_GRD_QTY)", ""));
                //dr_Sum["R_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(R_GRD_QTY)", ""));
                //dr_Sum["S_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(S_GRD_QTY)", ""));
                //dr_Sum["T_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(T_GRD_QTY)", ""));
                //dr_Sum["U_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(U_GRD_QTY)", ""));
                //dr_Sum["V_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V_GRD_QTY)", ""));
                //dr_Sum["W_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(W_GRD_QTY)", ""));
                //dr_Sum["Y_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(Y_GRD_QTY)", ""));
                //dr_Sum["Z_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(Z_GRD_QTY)", ""));
                //dr_Sum["V0B_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V0B_GRD_QTY)", ""));
                //dr_Sum["V0C_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V0C_GRD_QTY)", ""));
                //dr_Sum["V0D_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V0D_GRD_QTY)", ""));
                //dr_Sum["V0E_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V0E_GRD_QTY)", ""));
                //dr_Sum["V0F_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V0F_GRD_QTY)", ""));
                //dr_Sum["V0G_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V0G_GRD_QTY)", ""));
                //dr_Sum["V0H_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V0H_GRD_QTY)", ""));
                //dr_Sum["V0J_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V0J_GRD_QTY)", ""));
                //dr_Sum["V0K_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V0K_GRD_QTY)", ""));
                //dr_Sum["V0L_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V0L_GRD_QTY)", ""));
                //dr_Sum["V0O_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V0O_GRD_QTY)", ""));
                //dr_Sum["V0P_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V0P_GRD_QTY)", ""));
                //dr_Sum["V0R_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V0R_GRD_QTY)", ""));
                //dr_Sum["V0S_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V0S_GRD_QTY)", ""));
                //dr_Sum["V0T_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V0T_GRD_QTY)", ""));
                //dr_Sum["V0V_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V0V_GRD_QTY)", ""));
                //dr_Sum["V0W_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V0W_GRD_QTY)", ""));
                //dr_Sum["V0X_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V0X_GRD_QTY)", ""));
                //dr_Sum["V1B_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V1B_GRD_QTY)", ""));
                //dr_Sum["V1D_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V1D_GRD_QTY)", ""));
                //dr_Sum["V1H_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V1H_GRD_QTY)", ""));
                //dr_Sum["V1L_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V1L_GRD_QTY)", ""));
                //dr_Sum["V1R_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V1R_GRD_QTY)", ""));
                //dr_Sum["V1S_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(V1S_GRD_QTY)", ""));
                //dr_Sum["INC_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(INC_GRD_QTY)", ""));
                //dr_Sum["INB_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(INB_GRD_QTY)", ""));
                //dr_Sum["IND_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(IND_GRD_QTY)", ""));
                //dr_Sum["INR_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(INR_GRD_QTY)", ""));
                //dr_Sum["INS_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(INS_GRD_QTY)", ""));
                //dr_Sum["INO_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(INO_GRD_QTY)", ""));
                //dr_Sum["INZ_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(INZ_GRD_QTY)", ""));
                //dr_Sum["ING_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(ING_GRD_QTY)", ""));
                //dr_Sum["INM_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(INM_GRD_QTY)", ""));
                //dr_Sum["F11_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(F11_GRD_QTY)", ""));
                //dr_Sum["F00_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(F00_GRD_QTY)", ""));
                //dr_Sum["SAS_GRD_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(SAS_GRD_QTY)", ""));
                //dr_Sum["GRD_1_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(GRD_1_QTY)", ""));
                //dr_Sum["GRD_2_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(GRD_2_QTY)", ""));
                //dr_Sum["GRD_3_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(GRD_3_QTY)", ""));
                //dr_Sum["GRD_4_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(GRD_4_QTY)", ""));
                //dr_Sum["GRD_5_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(GRD_5_QTY)", ""));
                //dr_Sum["GRD_6_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(GRD_6_QTY)", ""));
                //dr_Sum["GRD_7_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(GRD_7_QTY)", ""));
                //dr_Sum["GRD_8_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(GRD_8_QTY)", ""));
                //dr_Sum["GRD_9_QTY"] = Convert.ToInt32(dt_Per.Compute("Sum(GRD_9_QTY)", ""));

                //DataRow dr_Per = dt_Per.NewRow();
                //dr_Per["PROD_LOTID"] = ObjectDic.Instance.GetObjectName("PERCENT_VAL");
                //dr_Per["INPUT_SUBLOT_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["INPUT_SUBLOT_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["WIP_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["WIP_QTY"]) * 100) / Total_Input, 2);
                //// 2024.01.16 양품 추가
                //dr_Per["GOOD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["GOOD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["ERR_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["ERR_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["A_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["A_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["B_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["B_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["C_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["C_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["D_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["D_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["E_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["E_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["F_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["F_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["G_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["G_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["H_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["H_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["I_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["I_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["J_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["J_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["K_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["K_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["L_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["L_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["M_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["M_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["N_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["N_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["O_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["O_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["P_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["P_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["Q_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["Q_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["R_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["R_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["S_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["S_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["T_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["T_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["U_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["U_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["W_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["W_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["Y_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["Y_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["Z_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["Z_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V0B_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V0B_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V0C_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V0C_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V0D_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V0D_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V0E_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V0E_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V0F_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V0F_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V0G_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V0G_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V0H_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V0H_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V0J_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V0J_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V0K_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V0K_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V0L_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V0L_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V0O_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V0O_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V0P_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V0P_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V0R_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V0R_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V0S_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V0S_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V0T_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V0T_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V0V_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V0V_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V0W_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V0W_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V0X_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V0X_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V1B_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V1B_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V1D_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V1D_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V1H_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V1H_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V1L_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V1L_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V1R_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V1R_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["V1S_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["V1S_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["INC_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["INC_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["INB_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["INB_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["IND_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["IND_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["INR_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["INR_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["INS_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["INS_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["INO_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["INO_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["INZ_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["INZ_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["ING_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["ING_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["INM_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["INM_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["F11_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["F11_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["F00_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["F00_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["SAS_GRD_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["SAS_GRD_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["GRD_1_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["GRD_1_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["GRD_2_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["GRD_2_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["GRD_3_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["GRD_3_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["GRD_4_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["GRD_4_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["GRD_5_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["GRD_5_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["GRD_6_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["GRD_6_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["GRD_7_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["GRD_7_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["GRD_8_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["GRD_8_QTY"]) * 100) / Total_Input, 2);
                //dr_Per["GRD_9_QTY"] = Math.Round((Convert.ToInt32(dr_Sum["GRD_9_QTY"]) * 100) / Total_Input, 2);

                //dt_Per.Rows.Add(dr_Sum);
                //dt_Per.Rows.Add(dr_Per);

                DataTable OperDt = DataTableConverter.Convert(dgDateOper.ItemsSource);

                List<String> ExceptColName = new List<string>
                {
                    "BF_ENDTIME", "STARTTIME", "ENDTIME", "AF_STARTTIME", "JOB_TIME"
                  , "INPUT_SUBLOT_QTY",  "WIP_QTY", "GOOD_QTY", "ERR_QTY"
                };

                for (int i = 7; i < OperDt.Columns.Count; i++)
                {
                    int sum = 0;

                    for (int j = 0; j < OperDt.Rows.Count; j++)
                    {
                        int icheck = 0;
                        bool bcheck = int.TryParse(OperDt.Rows[j][i].ToString(), out icheck);
                        if (!string.IsNullOrEmpty(OperDt.Rows[j][i].ToString()) && bcheck == true)
                        {
                            sum += Util.NVC_Int(OperDt.Rows[j][i]);
                        }
                    }

                    //if (sum == 0)
                    //    dgDateOper.Columns[OperDt.Columns[i].ColumnName].Visibility = Visibility.Collapsed; 

                    if ((ExceptColName.Contains(OperDt.Columns[i].ColumnName) == false) && (sum == 0))
                    {
                        dgDateOper.Columns[OperDt.Columns[i].ColumnName].Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        dgDateOper.Columns[OperDt.Columns[i].ColumnName].Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                btnSearch.IsEnabled = true;
            }
        }

        public static DataTable ConvertToDataTable(C1DataGrid dg)
        {
            try
            {
                DataTable dt = new DataTable();
                if (dg.ItemsSource == null)
                {
                    foreach (C1.WPF.DataGrid.DataGridColumn column in dg.Columns)
                    {
                        if (!string.IsNullOrEmpty(column.Name))
                            dt.Columns.Add(column.Name);
                    }
                    return dt;
                }
                else
                {
                    dt = ((DataView)dg.ItemsSource).Table;
                    return dt;
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
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
        #endregion

        public static Boolean IsNumeric(string pTarget)
        {
            double dNullable;
            return double.TryParse(pTarget, System.Globalization.NumberStyles.Any, null, out dNullable);
        }
        public static Boolean IsNumeric(object oTagraet)
        {
            return IsNumeric(oTagraet.ToString());
        }

        #region [Event]
        private void dgDateOper_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                if (e.Cell.Row.Index == 0)
                {
                    //return;
                }
                
                ////////////////////////////////////////////  default 색상 및 Cursor
                e.Cell.Presenter.Cursor = Cursors.Arrow;

                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                e.Cell.Presenter.FontSize = 12;
                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                ///////////////////////////////////////////////////////////////////////////////////

                //20220802 합계, 비율(%)의 배경색 변경
                if ((e.Cell.Column.Name.Equals("CSTID") || e.Cell.Column.Name.Equals("LOTID")) && e.Cell.Row.Index < dataGrid.Rows.Count - 2)
                {
                    int row = e.Cell.Row.Index;
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue); // CSTID 색상 변경

                    //DUMMY TRAY
                    string _sDummy = Util.NVC(DataTableConverter.GetValue(dgDateOper.Rows[row].DataItem, "DUMMY_FLAG"));
                    if (_sDummy.Equals("Y")) e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Green);

                    //SPECIAL TRAY
                    string _sSpecial = Util.NVC(DataTableConverter.GetValue(dgDateOper.Rows[row].DataItem, "SPCL_FLAG"));
                    if (_sSpecial.Equals("P"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else if (_sSpecial.Equals("Y"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.DarkOrange);
                    }
                }

                // 2024.03.13 삭제
                //if (e.Cell.Column.Name.Equals("PROD_LOTID"))
                //{
                //    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_LOTID")).ToString().Equals(ObjectDic.Instance.GetObjectName("합계")))
                //    {
                //        e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.PapayaWhip);
                //    }
                //    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_LOTID")).ToString().Equals(ObjectDic.Instance.GetObjectName("PERCENT_VAL")))
                //    {
                //        e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.Aquamarine);
                //    }
                //    else
                //    {
                //        e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                //    }
                //}
                //
                ////e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength((dgDateOper.ActualWidth / dgDateOper.Columns.Count) - 2);
                ////e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                //
                //// 2024.01.16 양품(GOOD_QTY) 추가
                //if (e.Cell.Column.Name.Equals("A_GRD_QTY") || e.Cell.Column.Name.Equals("B_GRD_QTY") || e.Cell.Column.Name.Equals("C_GRD_QTY") ||
                //    e.Cell.Column.Name.Equals("D_GRD_QTY") || e.Cell.Column.Name.Equals("E_GRD_QTY") || e.Cell.Column.Name.Equals("F_GRD_QTY") ||
                //    e.Cell.Column.Name.Equals("G_GRD_QTY") || e.Cell.Column.Name.Equals("H_GRD_QTY") || e.Cell.Column.Name.Equals("I_GRD_QTY") ||
                //    e.Cell.Column.Name.Equals("J_GRD_QTY") || e.Cell.Column.Name.Equals("K_GRD_QTY") || e.Cell.Column.Name.Equals("L_GRD_QTY") ||
                //    e.Cell.Column.Name.Equals("M_GRD_QTY") || e.Cell.Column.Name.Equals("N_GRD_QTY") || e.Cell.Column.Name.Equals("O_GRD_QTY") ||
                //    e.Cell.Column.Name.Equals("P_GRD_QTY") || e.Cell.Column.Name.Equals("Q_GRD_QTY") || e.Cell.Column.Name.Equals("R_GRD_QTY") ||
                //    e.Cell.Column.Name.Equals("S_GRD_QTY") || e.Cell.Column.Name.Equals("T_GRD_QTY") || e.Cell.Column.Name.Equals("U_GRD_QTY") ||
                //    e.Cell.Column.Name.Equals("Y_GRD_QTY") || e.Cell.Column.Name.Equals("V_GRD_QTY") || e.Cell.Column.Name.Equals("W_GRD_QTY") ||
                //    e.Cell.Column.Name.Equals("X_GRD_QTY") || e.Cell.Column.Name.Equals("Z_GRD_QTY") || e.Cell.Column.Name.Equals("F11_GRD_QTY") ||
                //    e.Cell.Column.Name.Equals("F00_GRD_QTY") || e.Cell.Column.Name.Equals("SAS_GRD_QTY") || e.Cell.Column.Name.Equals("GRD_1_QTY") ||
                //    e.Cell.Column.Name.Equals("GRD_2_QTY") || e.Cell.Column.Name.Equals("GRD_3_QTY") || e.Cell.Column.Name.Equals("GRD_4_QTY") ||
                //    e.Cell.Column.Name.Equals("GRD_5_QTY") || e.Cell.Column.Name.Equals("GRD_6_QTY") || e.Cell.Column.Name.Equals("GRD_7_QTY") ||
                //    e.Cell.Column.Name.Equals("GRD_8_QTY") || e.Cell.Column.Name.Equals("GRD_9_QTY") ||
                //    e.Cell.Column.Name.Equals("INPUT_SUBLOT_QTY") || e.Cell.Column.Name.Equals("WIP_QTY") || e.Cell.Column.Name.Equals("ERR_QTY") ||
                //    e.Cell.Column.Name.Equals("GOOD_QTY")
                //   )
                //{
                //    e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(45, DataGridUnitType.Auto);
                //}


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
                            case "INPUT_SUBLOT_QTY":
                                // 비율 Row 에서 Summary 정로를 지움
                                dgDateOper.SetSummaryRowValue(1, e.Cell.Column.Name, String.Empty);
                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(45, DataGridUnitType.Auto);
                                break;

                            case "PROD_LOTID":
                                dgDateOper.SetSummaryRowValue(1, e.Cell.Column.Name, ObjectDic.Instance.GetObjectName("PERCENT_VAL"));
                                break;

                            default:
                                decimal calcValue = 0;
                                decimal sumInput = 0;

                                if (String.IsNullOrEmpty(dgDateOper.GetSummaryRowValue(0, e.Cell.Column.Name)) == true)
                                    break;

                                if (dgDateOper.GetDataTable() != null)
                                {
                                    sumInput = Util.NVC_Decimal(dgDateOper.GetDataTable().Compute("Sum(INPUT_SUBLOT_QTY)", ""));
                                }

                                decimal sumValue = Util.NVC_Int(dgDateOper.GetSummaryRowValue(0, e.Cell.Column.Name).Replace(",", ""));

                                if (sumInput.Equals(0) == false)
                                {
                                    calcValue = Math.Round(sumValue * 100 / sumInput, 2);
                                }

                                dgDateOper.SetSummaryRowValue(1, e.Cell.Column.Name, calcValue.ToString("#,##0.00"));

                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(45, DataGridUnitType.Auto);
                                break;
                        }
                    }
                }
            }));
        }

        private void dgDateOper_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
                                
                //if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgDateOper.CurrentRow.DataItem, "LOTID")))) return;

                if (datagrid.CurrentColumn.Name == "CSTID" || datagrid.CurrentColumn.Name == "LOTID")
                {
                    string cstID = Util.NVC(DataTableConverter.GetValue(dgDateOper.CurrentRow.DataItem, "CSTID")); //Tray ID
                    

                    FCS002_021 wndTRAY = new FCS002_021();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgDateOper.CurrentRow.DataItem, "CSTID")); //Tray ID
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgDateOper.CurrentRow.DataItem, "LOTID")); //Tray No
                    this.FrameOperation.OpenMenu("SFU010710300", true, Parameters);
                }

                else if(string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgDateOper.CurrentRow.DataItem, "LOTID"))))
                {
                    //if (Util.NVC(DataTableConverter.GetValue(dgDateOper.CurrentRow.DataItem, "PROD_LOTID")) == ObjectDic.Instance.GetObjectName("합계"))
                    {
                        // 2024.01.16 양품(GOOD_QTY) 추가
                        if (dgDateOper.CurrentColumn.Name.Equals("PROD_LOTID") || dgDateOper.CurrentColumn.Name.Equals("LOTID") ||
                            dgDateOper.CurrentColumn.Name.Equals("CSTID") || dgDateOper.CurrentColumn.Name.Equals("LOC") ||
                            dgDateOper.CurrentColumn.Name.Equals("ROUTID") || dgDateOper.CurrentColumn.Name.Equals("SPCL_NOTE") ||
                            dgDateOper.CurrentColumn.Name.Equals("BF_ENDTIME") || dgDateOper.CurrentColumn.Name.Equals("STARTTIME") ||
                            dgDateOper.CurrentColumn.Name.Equals("ENDTIME") || dgDateOper.CurrentColumn.Name.Equals("AF_STARTTIME") ||
                            dgDateOper.CurrentColumn.Name.Equals("JOB_TIME") || 
                            dgDateOper.CurrentColumn.Name.Equals("INPUT_SUBLOT_QTY") || dgDateOper.CurrentColumn.Name.Equals("WIP_QTY") ||
                            dgDateOper.CurrentColumn.Name.Equals("QOOD_QTY") || dgDateOper.CurrentColumn.Name.Equals("ERR_QTY") ||                            
                            dgDateOper.CurrentColumn.Name.Equals("DUMMY_FLAG") || dgDateOper.CurrentColumn.Name.Equals("SPCL_FLAG")) return;
                        else if (Util.NVC(DataTableConverter.GetValue(dgDateOper.CurrentRow.DataItem, dgDateOper.CurrentColumn.Name.ToString())).Equals("0")) return;
                        else 
                        {
                            FCS002_043_DFCT_LIST DfctList = new FCS002_043_DFCT_LIST();
                            DfctList.FrameOperation = FrameOperation;

                            object[] parameters = new object[5];
                            parameters[0] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                            parameters[1] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm:00");
                            parameters[2] = Util.NVC(dgDateOper.CurrentColumn.Header.ToString()); // 등급
                            parameters[3] = Util.GetCondition(cboOper); //공정
                            if (!(bool)chkHistory.IsChecked)
                            {
                                parameters[4] = "Y";
                            }

                           
                            C1WindowExtension.SetParameters(DfctList, parameters);
                            DfctList.Closed += new EventHandler(sDfct_Closed);
                            this.Dispatcher.BeginInvoke(new Action(() => DfctList.ShowModal()));
                            DfctList.BringToFront();
                        }
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
            FCS002_043_DFCT_LIST window = sender as FCS002_043_DFCT_LIST;

            this.grdMain.Children.Remove(window);
        }
        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgDateOper_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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

        private void dgDateOper_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgDateOper_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
           C1DataGrid grid = sender as C1DataGrid;
            if (grid != null && grid.ItemsSource != null && grid.Rows.Count > 1)
            {
                var _mergeList = new List<DataGridCellsRange>();

                _mergeList.Add(new DataGridCellsRange(grid.GetCell(grid.Rows.Count - 2, 0), grid.GetCell(grid.Rows.Count - 2, 5)));
                _mergeList.Add(new DataGridCellsRange(grid.GetCell(grid.Rows.Count - 1, 0), grid.GetCell(grid.Rows.Count - 1, 5)));

                foreach (var range in _mergeList)
                {
                    e.Merge(range);
                }
            }
        }
    }
}
