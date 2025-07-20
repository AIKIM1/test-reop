/*************************************************************************************
 Created Date : 2017.03.02
      Creator : 정문교C
   Decription : Washing 초소형 공정 착공
--------------------------------------------------------------------------------------
 [Change History]
   2017.03.02   INS 정문교C : Initial Created.
   2017.07.05   INS 이대영D : BizRule 변경에 의한 로직 및 UI 재정의
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.ASSY002
{
    public partial class ASSY002_006_RUNSTART : C1Window, IWorkArea
    {
        #region Declaration

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private DataTable selectDt = null;
        private string procID = string.Empty;
        private string lotID = string.Empty;
        private string lineID = string.Empty;
        private string eqptID = string.Empty;
        private string eqptName = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public ASSY002_006_RUNSTART()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void ASSY002_006_RUNSTART_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            procID = tmps[0] as string;
            lineID = tmps[1] as string;
            eqptID = tmps[2] as string;
            eqptName = tmps[3] as string;
            lotID = tmps[4] as string;

            // SET COMMON
            txtEquipment.Text = eqptName;

            // SET WORKORDER
            DataRow workOrder = tmps[5] as DataRow;   // WORKORDER

            // SET WASHING RUN LIST
            this.GetWashingRunList();
        }
        #endregion

        #region [DataGrid] - gdInputItem_CommittedEdit, txtLotId_PreviewKeyDown, btnSearch_Click
        private void dgInputProduct_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            if (selectDt == null || selectDt.Rows.Count < 0)
                return;

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;

                int rowIndex = 0;
                int colIndex = 0;

                if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null && dgInputProduct.SelectedIndex >= 0)
                {
                    rowIndex = e.Cell.Row.Index;
                    colIndex = e.Cell.Column.Index;

                    int selectedIdx = dgInputProduct.SelectedIndex;

                    if (selectedIdx < 0 || selectDt.Rows.Count < 0 || selectDt == null)
                        return;

                    for (int row = 0; row < selectDt.Rows.Count; row++)
                    {
                        if (string.Equals(selectDt.Rows[row]["CHK"].ToString(), "1"))
                        {
                            if (string.Equals(selectDt.Rows[selectedIdx]["CHK"].ToString(), "0"))
                            {
                                dgInputProduct.SelectedIndex = row;
                            }
                        }
                    }
                }
            }));
        }
        #endregion

        #region [작업시작]
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateStartRun())
                return;
            
            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    // LOT 착공
                    StartRunProcess();
                }
            });
        }

        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #endregion

        #region User Method

        #region [BizCall]
        private string GetProcessFPInfo(ref string sReturnValue)
        {
            sReturnValue = "";

            try
            {
                string sBizName = "DA_PRD_SEL_PROCESS_FP_INFO";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("PROCID", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["PROCID"] = procID;

                inDataTable.Rows.Add(searchCondition);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "OUTDATA", inDataTable);

                if (dtRslt == null || dtRslt.Rows.Count < 1)
                {
                    return sReturnValue;
                }

                // Reference 공정인 경우는 REF 공정 정보 설정.
                //if (!Util.NVC(dtRslt.Rows[0]["ERPRPTIUSE"]).Equals("Y") && Util.NVC(dtRslt.Rows[0]["PLAN_MNGT_TYPE_CODE"]).Equals("REF") && !Util.NVC(dtRslt.Rows[0]["FP_REF_PROCID"]).Equals(""))
                //{
                //    sReturnValue = Util.NVC(dtRslt.Rows[0]["FP_REF_PROCID"]);
                //}
                if (Util.NVC(dtRslt.Rows[0]["PLAN_MNGT_TYPE_CODE"]).Equals("REF") && !Util.NVC(dtRslt.Rows[0]["FP_REF_PROCID"]).Equals(""))
                {
                    sReturnValue = Util.NVC(dtRslt.Rows[0]["FP_REF_PROCID"]);
                }

                return sReturnValue;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return sReturnValue;
            }
        }
        private void GetWashingRunList()
        {
            // SELECT INPUT PRODUCT
            try
            {
                Util.gridClear(dgInputProduct);

                const string sBizName = "DA_PRD_SEL_LOT_LIST_WS";

                //string sFPrefProcid = string.Empty;
                //GetProcessFPInfo(ref sFPrefProcid);
                
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));

                DataRow inDataRow = inDataTable.NewRow();
                inDataRow["LANGID"] = LoginInfo.LANGID;
                inDataRow["PROCID"] = procID;
                inDataRow["EQSGID"] = Util.NVC(lineID);

                inDataTable.Rows.Add(inDataRow);

                DataTable dt = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", inDataTable);

                if (dt.Rows.Count == 0)
                {
                    //Util.AlertInfo("조회 가능한 투입 반제품 정보가 없습니다.");
                    Util.MessageValidation("SFU1903");
                    return;
                }

                //Util.GridSetData(dgInputProduct, dt, FrameOperation, true);
                dgInputProduct.ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void StartRunProcess()
        {
            try
            {
                ShowLoadingIndicator();

                string sBizName = "BR_PRD_REG_START_PROD_LOT_WSS"; // 초소형

                // DATA SET
                DataSet inDataSet = new DataSet();
                DataTable inEQP = inDataSet.Tables.Add("IN_EQP");
                inEQP.Columns.Add("SRCTYPE", typeof(string));
                inEQP.Columns.Add("IFMODE", typeof(string));
                inEQP.Columns.Add("EQPTID", typeof(string));
                inEQP.Columns.Add("USERID", typeof(string));
                inEQP.Columns.Add("INPUT_LOTID", typeof(string));
                inEQP.Columns.Add("EQPT_LOTID", typeof(string));

                string inputLotId = _Util.GetDataGridFirstRowBycheck(dgInputProduct, "CHK").Field<string>("LOTID").GetString();
                DataRow newRow = inEQP.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(eqptID);
                newRow["USERID"] = LoginInfo.USERID;
                if (selectDt != null)
                   // newRow["INPUT_LOTID"] = Util.NVC(selectDt.Rows[0]["LOTID"]);
                newRow["INPUT_LOTID"] = inputLotId;
                newRow["EQPT_LOTID"] = string.Empty;
                inEQP.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(sBizName, "IN_EQP", "OUT_EQP", (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region[[Validation]
        private bool ValidateStartRun()
        {
            // CHECK INPUT HALFPRODUCT
            if (dgInputProduct.Rows.Count == 0)
            {
                //Util.Alert("투입 반제품이 존재하지 않습니다.");
                Util.MessageValidation("SFU1952");
                return false;
            }

            if (_Util.GetDataGridCheckCnt(dgInputProduct, "CHK") == 0)
            {
                //Util.Alert("선택된 투입 반제품이 존재하지 않습니다.");
                Util.MessageValidation("SFU1650");
                return false;
            }

            int idx = _Util.GetDataGridFirstRowIndexByCheck(dgInputProduct, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 투입 반제품이 존재하지 않습니다.");
                Util.MessageValidation("SFU1650");
                return false;
            }

            if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[idx].DataItem, "LOTID"))))
            {
                //Util.Alert("투입 반제품 정보에 LOT ID가 없습니다.");
                Util.MessageValidation("SFU1946");
                return false;
            }

            if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[idx].DataItem, "PRODID"))))
            {
                //Util.Alert("투입 반제품 정보에 제품ID가 없습니다.");
                Util.MessageValidation("SFU1946");
                return false;
            }

            //for (int i = 0; i < dgInputProduct.Rows.Count; i++)
            //{
            //    if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "LOTID"))))
            //    {
            //        //Util.Alert("투입 반제품 정보에 LOT ID가 없습니다.");
            //        Util.MessageValidation("SFU1946");
            //        dgInputProduct.SelectedIndex = i;
            //        return false;
            //    }

            //    if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "PRODID"))))
            //    {
            //        //Util.Alert("투입 반제품 정보에 제품ID가 없습니다.");
            //        Util.MessageValidation("SFU1946");
            //        dgInputProduct.SelectedIndex = i;
            //        return false;
            //    }
            //}
            return true;
        }
        #endregion

        #region [Func]
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

        #endregion

        private void rdoChk_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;

                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                        DataRow dtRow = (rb.DataContext as DataRowView).Row;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }
                        //row 색 바꾸기
                        dgInputProduct.SelectedIndex = idx;

                        selectDt = dtRow.Table;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally { }
        }

        private void dgInputProduct_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (selectDt == null || selectDt.Rows.Count < 0)
                return;

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;

                int selectedIdx = dgInputProduct.SelectedIndex;

                if (selectedIdx < 0 || selectDt.Rows.Count < 0 || selectDt == null)
                    return;

                for (int row = 0; row < selectDt.Rows.Count; row++)
                {
                    if (string.Equals(selectDt.Rows[row]["CHK"].ToString(), "1"))
                    {
                        if (string.Equals(selectDt.Rows[selectedIdx]["CHK"].ToString(), "0"))
                        {
                            dgInputProduct.SelectedIndex = row;
                        }
                    }
                }
            }));
        }
    }
}
