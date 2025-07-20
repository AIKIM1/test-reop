/*************************************************************************************
 Created Date : 2020.11.24
      Creator : 
   Decription : 수동 판정
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.24  DEVELOPER : Initial Created.
  2021.04.02     PSM    : Lot별 예측 dOCV 수동판정 추가
  2022.08.24    김진섭  : C20220816-000503 -W등급 기준정보 등록 & 삭제시 발생하는 오류 수정 및 수정기능 추가
  2022.12.14    조영대  : UI Event Log 수정(USER_IP, PC_NAME, MENUID)
  2023.01.08    이정미  : 수동판정 Tab 조회 INDATA 수정
  2023.01.31    조영대  : Validation 추가
  2024.01.31    조영대  : 판정저장 - 멀티 실행에서 순차실행으로 변경
  2024.02.07    조영대  : 판정저장 - 진행률 표시기 및 REMARK 동적 표시 처리 (PROGRAM_BY_FUNC_USE_FLAG : FCS001_030_DELAY_FLAG,FCS001_030_RUN_SEQUENCE 제거)
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_030 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private readonly Util _util = new Util();
        bool bProdLotLikeUseUseFlag = false; // 2023.10.29 PROD LOT ID LIKE 조회 동별 코드 추가
        int judgeTotalCount = 0;

        public FCS001_030()
        {
            InitializeComponent();
            InitCombo();

            bProdLotLikeUseUseFlag = _util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_034_PRODLOTLIKE_USEFLAG"); // 2023.10.29 PROD LOT ID LIKE 조회 동별 코드 추가
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelChild = { cboRoute };
            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            C1ComboBox[] cboRouteChild = { cboJudgOp, cboOp };
            _combo.SetCombo(cboRoute, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE", cbChild: cboRouteChild, cbParent: cboRouteParent);

            C1ComboBox[] cboOperParent = { cboRoute };
            _combo.SetCombo(cboOp, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE_OP", cbParent: cboOperParent);

            _combo.SetCombo(cboJudgOp, CommonCombo_Form.ComboStatus.NONE, sCase: "JUDGE_OP", cbParent: cboOperParent);

            string[] sFilter1 = { "COMBO_WIPSTATE" };
            _combo.SetCombo(cboState, CommonCombo_Form.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

            string[] sFilter2 = { "FLAG_YN" };

            _combo.SetCombo(cboAbnorm, CommonCombo_Form.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);
            _combo.SetCombo(cboISS, CommonCombo_Form.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);

            //Lot별 예측 dOCV 수동판정
            _combo.SetCombo(cboModel2, CommonCombo_Form.ComboStatus.ALL, sCase: "MODEL");
        }
        #endregion

        #region Method

        #region [수동판정 tab]
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("WIPSTAT", typeof(string));
                dtRqst.Columns.Add("ABNORM_FLAG", typeof(string));
                dtRqst.Columns.Add("ISS_RSV_FLAG", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Util.GetCondition(cboOp, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                dr["WIPSTAT"] = Util.GetCondition(cboState, bAllNull: true);
                dr["ABNORM_FLAG"] = Util.GetCondition(cboAbnorm, bAllNull: true);
                dr["ISS_RSV_FLAG"] = Util.GetCondition(cboISS, bAllNull: true);
                if (dr["ISS_RSV_FLAG"].Equals("N")) dr["ISS_RSV_FLAG"] = null;
                if (!string.IsNullOrEmpty(txtTrayID.Text)) dr["CSTID"] = Util.GetCondition(txtTrayID, bAllNull: true);
                dtRqst.Rows.Add(dr);

                // 2023.10.29 PROD LOT ID LIKE 조회 동별 코드 추가
                if (bProdLotLikeUseUseFlag)
                {
                    if (!string.IsNullOrEmpty(txtLotID.Text))
                    {
                        if (txtLotID.Text.Length < 8)
                        {
                            Util.Alert("SFU4075");
                            return;
                        }
                        else
                        {
                            dtRqst.Columns.Add("PROD_LOTID_LIKE", typeof(string));
                            if (!string.IsNullOrEmpty(txtLotID.Text)) dr["PROD_LOTID_LIKE"] = Util.GetCondition(txtLotID, bAllNull: true) + "%";

                        }
                    }
                }
                else
                {
                    dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                    if (!string.IsNullOrEmpty(txtLotID.Text)) dr["PROD_LOTID"] = Util.GetCondition(txtLotID, bAllNull: true);
                }

                btnSearch.IsEnabled = false;

                // 백그라운드 실행, 비즈 실행 후 dgMaintJudg_ExecuteDataModify, dgMaintJudg_ExecuteDataCompleted 순서대로 실행
                dgMaintJudg.ExecuteService("DA_SEL_TRAY_LIST_JDA_FOR_JUDGE", "RQSTDT", "RSLTDT", dtRqst);
                
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgMaintJudg_ExecuteDataModify(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            DataTable dtResult = e.ResultData as DataTable;
            dtResult.Columns.Add("CHK", typeof(bool));
            dtResult.Columns.Add("REMARK", typeof(string));
        }

        private void dgMaintJudg_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            btnSearch.IsEnabled = true;
        }

        private string PrevJudgOpFittedCheck(string sTrayNo, string sRouteId, string sOp, int iFitted)
        {
            string sRtn = string.Empty;

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("JUDG_PROCID", typeof(string));
                dtRqst.Columns.Add("FITTED_MODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LOTID"] = sTrayNo;
                dr["ROUTID"] = sRouteId;
                dr["JUDG_PROCID"] = sOp;
                dr["FITTED_MODE"] = iFitted.ToString();
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_JUDG_FITTED_OP", "RQSTDT", "RSLTDT", dtRqst);
                sRtn = dtRslt.Rows[0]["RETVAL"].ToString();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            return sRtn;
        }

        #endregion

        #region [Lot별 예측 dOCV 수동판정 tab]
        private void GetListWJudg()
        {
            xProgressW.Value = 0;
            xTextBlockW.Text = "0/0";

            Util.gridClear(dgTrayList); //Lot List 초기화

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("PROD_LOTID", typeof(string));
            dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
            dtRqst.Columns.Add("W_JUDG_TYPE_CODE", typeof(string));
            dtRqst.Columns.Add("PREDCT_DAY", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            if (!string.IsNullOrEmpty(txtLotID2.Text)) dr["PROD_LOTID"] = txtLotID2.Text;
            dr["MDLLOT_ID"] = Util.GetCondition(cboModel2, bAllNull: true);
            dr["W_JUDG_TYPE_CODE"] = "G";
            dtRqst.Rows.Add(dr);

            string sBiz = "DA_SEL_TM_LOW_VOLTAGE_JUDG_LOT";
            //  if ((bool)chkLLOT.IsChecked) sBiz = "DA_SEL_TM_LOW_VOLTAGE_JUDG_LOT_ASSY";

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBiz, "RQSTDT", "RSLTDT", dtRqst);
            dtRslt.Columns.Add("CHK", typeof(bool));
            dtRslt.Columns.Add("FLAG", typeof(string));
            dtRslt.Columns.Add("NEWFLAG", typeof(string));
            for (int i = 0; i < dtRslt.Rows.Count; i++)
            {
                dtRslt.Rows[i]["FLAG"] = "N";
                dtRslt.Rows[i]["NEWFLAG"] = "O";
            }
            Util.GridSetData(dgPredDocv, dtRslt, this.FrameOperation, true);
        }

        private async void JudgLotW(int i)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                const string bizRuleName = "BR_SET_W_MANUAL_LOT";

                Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("SRCTYPE", typeof(string));
                    dtRqst.Columns.Add("IFMODE", typeof(string));
                    dtRqst.Columns.Add("LOTID", typeof(string));           //TRAY_NO
                    dtRqst.Columns.Add("MANUAL_YN", typeof(string));
                    dtRqst.Columns.Add("USERID", typeof(string));          //MDF_ID

                    DataRow dr = dtRqst.NewRow();
                    dr["SRCTYPE"] = "UI";
                    dr["IFMODE"] = "OFF";
                    dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                    dr["MANUAL_YN"] = (bool)rdoManual.IsChecked ? "Y" : "N";
                    dr["USERID"] = LoginInfo.USERID;
                    dtRqst.Rows.Add(dr);

                    new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                    {
                        if (bizException != null)
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "REMARK", "FAIL : " + bizException.Message);
                            UpdateProgressBarW();
                            return;
                        }
                        if (bizResult.Rows[0]["RETVAL"].ToString().Equals("1"))
                        {
                            DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "REMARK", "SUCCESS");
                        }
                        else
                        {
                            DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "REMARK", "FAIL");
                        }
                        UpdateProgressBarW();
                    });

                }),
                    System.Windows.Threading.DispatcherPriority.Input
                );
            });
            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void UpdateProgressBarW()
        {
            Application.Current.Dispatcher.Invoke(
                new Action(delegate
                {
                    xProgressW.Value += 1;
                    xTextBlockW.Text = xProgressW.Value + "/" + dgTrayList.Rows.Count;
                }),
                System.Windows.Threading.DispatcherPriority.Input
            );
        }
        #endregion

        #endregion

        #region Event
        

        #region [수동판정 tab]

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnJudge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!dgMaintJudg.IsCheckedRow("CHK"))
                {
                    Util.Alert("SFU1645"); //선택된 작업대상이 없습니다.
                    return;
                }

                //Util.MessageConfirm("FM_ME_0177", async (result) => //수동판정을 등록하시겠습니까?
                //{
                //    if (result != MessageBoxResult.OK) return;

                //int iFitted = 0;

                //if ((bool)chkDCIR.IsChecked)
                //    iFitted = iFitted + 1;

                //if ((bool)chkCapa.IsChecked)
                //    iFitted = iFitted + 2;


                //string sOp = Util.GetCondition(cboJudgOp);

                //if (string.IsNullOrEmpty(sOp))
                //{
                //    Util.Alert("FM_ME_0248");  //판정공정을 선택해주세요.
                //    return;
                //}

                //if (result != MessageBoxResult.OK) return;

                //List<DataRow> chkRows = dgMaintJudg.GetCheckedDataRow("CHK");

                //object[] argument = new object[3] { chkRows, iFitted, sOp };

                //xProgress.Percent = 0;
                //judgeTotalCount = _util.GetDataGridRowCountByCheck(dgMaintJudg, "CHK", true);
                //xProgress.ProgressText = "0 / " + judgeTotalCount.Nvc();
                //xProgress.Visibility = Visibility.Visible;

                //xProgress.RunWorker(argument);
                //});

                int iFitted = 0;

                if ((bool)chkDCIR.IsChecked)
                    iFitted = iFitted + 1;

                if ((bool)chkCapa.IsChecked)
                    iFitted = iFitted + 2;


                string sOp = Util.GetCondition(cboJudgOp);

                if (string.IsNullOrEmpty(sOp))
                {
                    Util.Alert("FM_ME_0248");  //판정공정을 선택해주세요.
                    return;
                }

                //if (result != MessageBoxResult.OK) return;

                List<DataRow> chkRows = dgMaintJudg.GetCheckedDataRow("CHK");

                object[] argument = new object[4] { chkRows, iFitted, sOp, chkCell.IsChecked == true ? "Y" : "N" };

                xProgress.Percent = 0;
                judgeTotalCount = _util.GetDataGridRowCountByCheck(dgMaintJudg, "CHK", true);
                xProgress.ProgressText = "0 / " + judgeTotalCount.Nvc();
                xProgress.Visibility = Visibility.Visible;

                Util.MessageConfirm("FM_ME_0177", (result) => //수동판정을 등록하시겠습니까?
                {
                    if (result != MessageBoxResult.OK) return;

                    btnSearch.IsEnabled = false;
                    btnJudge.IsEnabled = false;

                    xProgress.RunWorker(argument);
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private object xProgress_WorkProcess(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                object[] arguments = e.Arguments as object[];

                List<DataRow> checkedRows = arguments[0] as List<DataRow>;

                int iFitted = arguments[1].NvcInt();

                string sOp = arguments[2].Nvc();

                string isCheckCell = arguments[3].Nvc();

                int workCount = 0;

                foreach (DataRow checkRow in checkedRows)
                {
                    workCount++;

                    if (iFitted > 0)
                    {
                        switch (PrevJudgOpFittedCheck(checkRow["LOTID"].Nvc(), checkRow["ROUTID"].Nvc(), sOp, iFitted))
                        {
                            case "0":
                                break;
                            case "1":
                                //[Tray ID : {0}] 이전 판정에 Fitted 공정이 포함되어 있습니다.\r\n이전 판정 공정을 확인하세요.
                                return MessageDic.Instance.GetMessage("FM_ME_0005", new string[] { checkRow["CSTID"].Nvc() });
                            case "2":
                                //[Tray ID : {0}] 현재 이후 판정 공정 조건에 Fitted 공정이 포함되어 있지 않습니다. 판정 공정을 확인하세요.
                                return MessageDic.Instance.GetMessage("FM_ME_0007", new string[] { checkRow["CSTID"].Nvc() });
                            default:
                                //[Tray ID : {0}] 정상 처리되지 않았습니다. 판정 공정을 확인하세요.
                                return MessageDic.Instance.GetMessage("FM_ME_0006", new string[] { checkRow["CSTID"].Nvc() });
                        }
                    }

                    string updateResultText = string.Empty;

                    try
                    {
                        const string bizRuleName = "BR_SET_CELL_JUDGMENT_MANUAL";

                        DataTable dtRqst = new DataTable();
                        dtRqst.Columns.Add("SRCTYPE", typeof(string));
                        dtRqst.Columns.Add("IFMODE", typeof(string));
                        dtRqst.Columns.Add("LOTID", typeof(string));           //TRAY_NO
                        dtRqst.Columns.Add("JUDG_PROCID", typeof(string));     //JUDG_OP_ID
                        dtRqst.Columns.Add("USERID", typeof(string));          //MDF_ID
                        dtRqst.Columns.Add("FITTED_MODE", typeof(string));     //FITTED_MODE
                        dtRqst.Columns.Add("NOT_INIT_GRADE_FLAG", typeof(string)); //NOT_INIT_GRADE
                        dtRqst.Columns.Add("MENUID", typeof(string));
                        dtRqst.Columns.Add("USER_PC_IP", typeof(string));
                        dtRqst.Columns.Add("PC_NAME", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["SRCTYPE"] = "UI";
                        dr["IFMODE"] = "OFF";
                        dr["LOTID"] = checkRow["LOTID"].Nvc();
                        dr["JUDG_PROCID"] = sOp;
                        dr["USERID"] = LoginInfo.USERID;
                        dr["FITTED_MODE"] = iFitted;
                        dr["NOT_INIT_GRADE_FLAG"] = isCheckCell;
                        dr["MENUID"] = LoginInfo.CFG_MENUID;
                        dr["USER_PC_IP"] = LoginInfo.USER_IP;
                        dr["PC_NAME"] = LoginInfo.PC_NAME;
                        dtRqst.Rows.Add(dr);

                        DataTable bizResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dtRqst);
                        if (bizResult != null && bizResult.Rows.Count > 0 && bizResult.Rows[0]["RETVAL"].ToString().Equals("0"))
                        {
                            updateResultText = "SUCCESS";
                        }
                        else
                        {
                            updateResultText = "FAIL";
                        }
                    }
                    catch (Exception ex)
                    {
                        updateResultText = "FAIL : " + ex.Message;
                    }

                    object[] progressArgument = new object[2] { workCount.Nvc() + " / " + judgeTotalCount.Nvc(), updateResultText };

                    e.Worker.ReportProgress(Convert.ToInt16((double)workCount / (double)judgeTotalCount * 100), progressArgument);

                    checkRow["REMARK"] = updateResultText;
                }
                return "SUCCESS";
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private void xProgress_WorkProcessChanged(object sender, int percent, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                object[] progressArguments = e.Arguments as object[];

                string progressText = progressArguments[0].Nvc();
                string updateText = progressArguments[1].Nvc();

                xProgress.Percent = percent;
                xProgress.ProgressText = progressText;
                xProgress.InvalidateVisual();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void xProgress_WorkProcessCompleted(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                object[] arguments = e.Arguments as object[];

                if (e.Result != null && e.Result is string)
                {
                    if (e.Result.Nvc().Equals("SUCCESS"))
                    {
                        Util.AlertInfo("SFU1889");
                    }
                    else
                    {
                        Util.AlertInfo("[*]" + e.Result.Nvc());
                    }                    
                }
                else if (e.Result != null && e.Result is Exception)
                {
                    Util.MessageException(e.Result as Exception);
                }
                else
                {
                    string msg = MessageDic.Instance.GetMessage("FM_ME_0202");
                    Util.AlertInfo(msg);  //작업중 오류가 발생하였습니다.
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
                btnJudge.IsEnabled = true;
            }
        }

        private void dgMaintJudg_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgMaintJudg.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }
        #endregion

        #region [Lot별 예측 dOCV 수동판정 tab]

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                DataTable dt = new DataTable();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);
                        DataRow dr2 = dt.NewRow();
                        dt.Rows.Add(dr2);
                        // dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        //  dg.EndEdit();
                    }
                }
                else
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dt.Rows.Add(dr);
                        // dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        // dg.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DataGridRowRemove(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                if (dg.GetRowCount() > 0)
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    dt.Rows[dt.Rows.Count - 1].Delete();
                    dt.AcceptChanges();
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgMaintJudg);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgMaintJudg);
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            GetListWJudg();
        }

        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowAdd(dgPredDocv, 1);
            DataTableConverter.SetValue(dgPredDocv.Rows[dgPredDocv.Rows.Count - 1].DataItem, "FLAG", "Y");
            DataTableConverter.SetValue(dgPredDocv.Rows[dgPredDocv.Rows.Count - 1].DataItem, "NEWFLAG", "N");
        }

        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            //string flag = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[dgPredDocv.Rows.Count - 1].DataItem, "FLAG"));
            //if (flag.Equals("Y")) DataGridRowRemove(dgPredDocv);

            string flag = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[dgPredDocv.Rows.Count - 1].DataItem, "NEWFLAG"));
            if (flag.Equals("N")) DataGridRowRemove(dgPredDocv);
        }

        private void btnLotW_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("FM_ME_0211", (result) =>  //재판정하시겠습니까?
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }

                    xProgressW.Maximum = dgTrayList.Rows.Count;
                    xProgressW.Minimum = 0;
                    xTextBlockW.Text = "0/0";
                    for (int i = 0; i < dgTrayList.Rows.Count; i++)
                    {
                        JudgLotW(i);
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("FM_ME_0214", (result) => //저장하시겠습니까?
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                    dtRqst.Columns.Add("PREDCT_DAY", typeof(string));
                    dtRqst.Columns.Add("L_THRES", typeof(double));
                    dtRqst.Columns.Add("U_THRES", typeof(double));
                    dtRqst.Columns.Add("W_JUDG_TYPE_CODE", typeof(string));
                    dtRqst.Columns.Add("PASS_YN", typeof(string));
                    dtRqst.Columns.Add("USERID", typeof(string));

                    for (int i = 0; i < dgPredDocv.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "FLAG")).Equals("Y"))  //수정 된 Row만
                        {
                            string lot = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "PROD_LOTID"));
                            string predct = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "PREDCT_DAY"));
                            string lthres = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "L_THRES"));
                            string uthres = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "U_THRES"));

                            if (string.IsNullOrEmpty(lot) || string.IsNullOrEmpty(predct)) //PROD_LOTID, PREDCT_DAY는 필수조건
                            {
                                continue;
                            }
                            DataRow dr = dtRqst.NewRow();
                            dr["PROD_LOTID"] = lot;
                            dr["PREDCT_DAY"] = predct;
                            if (!string.IsNullOrEmpty(lthres)) dr["L_THRES"] = Convert.ToDouble(lthres);
                            if (!string.IsNullOrEmpty(uthres)) dr["U_THRES"] = Convert.ToDouble(uthres);
                            dr["W_JUDG_TYPE_CODE"] = "G";
                            dr["PASS_YN"] = "N";
                            dr["USERID"] = LoginInfo.USERID;

                            
                            dtRqst.Rows.Add(dr);

                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_LOW_VOLTAGE_JUDG_LOT", "RQSTDT", "RSLTDT", dtRqst);
                            if (int.Parse(dtRslt.Rows[0]["RETVAL"].ToString()) > 0)
                                Util.Alert("FM_ME_0215");  //저장하였습니다.
                            else
                                Util.Alert("FM_ME_0213");  //저장실패하였습니다.

                            GetListWJudg();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgPredDocv_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            if (e.Column.Name.Equals("PROD_LOTID"))
            {
                //if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "FLAG")).Equals("N"))
                if(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "NEWFLAG")).Equals("O"))
                {
                    e.Cancel = true;
                }
            }
            dataGrid.Rows[e.Row.Index].Presenter.Background = new SolidColorBrush(Colors.LightYellow);

            //C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
            int row = e.Row.Index;
            DataTableConverter.SetValue(dataGrid.Rows[row].DataItem, "FLAG", "Y");
        }

        private void dgPredDocv_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!dgPredDocv.IsCheckedRow("CHK"))
                {
                    Util.Alert("SFU1645"); //선택된 작업대상이 없습니다.
                    return;
                }

                Util.MessageConfirm("FM_ME_0167", (result) => //선택한 데이터를 삭제하시겠습니까?
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                    dtRqst.Columns.Add("PREDCT_DAY", typeof(string));
                    dtRqst.Columns.Add("L_THRES", typeof(string));
                    dtRqst.Columns.Add("U_THRES", typeof(string));
                    dtRqst.Columns.Add("USERID", typeof(string));
                    dtRqst.Columns.Add("W_JUDG_TYPE_CODE", typeof(string));
                    dtRqst.Columns.Add("PASS_YN", typeof(string));

                    for (int i = 0; i < dgPredDocv.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                        {
                            DataRow dr = dtRqst.NewRow();
                            dr["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "PROD_LOTID"));
                            dr["PREDCT_DAY"] = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "PREDCT_DAY"));

                            string lthres = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "L_THRES"));
                            string uthres = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "U_THRES"));

                            if (!string.IsNullOrEmpty(lthres)) dr["L_THRES"] = lthres;
                            if (!string.IsNullOrEmpty(uthres)) dr["U_THRES"] = uthres;

                            dr["USERID"] = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[i].DataItem, "UPDUSER"));
                            dr["W_JUDG_TYPE_CODE"] = "G";
                            dr["PASS_YN"] = "N";

                            // 김진섭
                            dtRqst.Rows.Add(dr);
                        }
                    }
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_LOW_VOLTAGE_JUDG_DEL_LOT", "RQSTDT", "RSLTDT", dtRqst);
                    Util.Alert("FM_ME_0154"); //삭제완료하였습니다.
                    GetListWJudg();
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgPredDocv_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgPredDocv.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (!cell.Column.Name.Equals("CHK"))
                    {
                        return;
                    }

                    if (Util.NVC(cell.Value).ToUpper().Equals("TRUE")) RemoveTrayList(cell.Row.Index);
                    else SearchTrayList(cell.Row.Index);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);

            }
        }

        private void SearchTrayList(int rowIdx)
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("PROD_LOTID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[rowIdx].DataItem, "PROD_LOTID"));
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_TRAY_LIST_W_REJUDGE", "RQSTDT", "RSLTDT", dtRqst);

            dtRslt.Columns.Add("REMARK", typeof(string));
            DataTable temp = DataTableConverter.Convert(dgTrayList.ItemsSource);

            temp.Merge(dtRslt);

            if (temp.Rows.Count == 0)
            {
                Util.Alert("FM_ME_0210");  //재판정대상이 없습니다.
                return;
            }

            Util.GridSetData(dgTrayList, temp, this.FrameOperation);

        }

        private void RemoveTrayList(int rowIdx)
        {
            string pkgLot = Util.NVC(DataTableConverter.GetValue(dgPredDocv.Rows[rowIdx].DataItem, "PROD_LOTID"));

            DataTable temp = DataTableConverter.Convert(dgTrayList.ItemsSource);

            for (int i = 0; i < temp.Rows.Count; i++)
            {
                if (temp.Rows[i]["PROD_LOTID"].Equals(pkgLot))
                {
                    temp.Rows.RemoveAt(i);
                    i--;
                }
            }
            if (temp.Rows.Count == 0) Util.gridClear(dgTrayList);
            else Util.GridSetData(dgTrayList, temp, this.FrameOperation);
        }

        private void dgTrayList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgTrayList.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void dgPredDocv_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgPredDocv.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void dgPredDocv_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            if (e.Column.Name.Equals("PROD_LOTID"))
            {
                string prodlotid = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[e.Row.Index].DataItem, "PROD_LOTID"));

                if (string.IsNullOrEmpty(prodlotid)) return;

                for (int i = 0; i < datagrid.Rows.Count; i++)
                {
                    if (i == e.Row.Index) continue;

                    if (prodlotid.Equals(Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "PROD_LOTID"))))
                    {
                        DataTableConverter.SetValue(dgPredDocv.Rows[e.Row.Index].DataItem, "PROD_LOTID", "");

                        Util.Alert("SFU6005");

                        return;
                    }
                }
            }
        }
        #endregion

        #endregion
    }
}
